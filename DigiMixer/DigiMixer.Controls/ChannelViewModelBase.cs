using JonSkeet.WpfUtil;
using DigiMixer;
using DigiMixer.Core;
using Microsoft.Extensions.Logging;
using NodaTime;
using System.ComponentModel;

namespace DigiMixer.Controls;

public abstract class ChannelViewModelBase<T> : ViewModelBase, IChannelViewModelBase where T : ChannelBase
{
    private T channel;
    [RelatedProperties(nameof(DisplayName), nameof(Output), nameof(StereoOutput), nameof(IsStereo), nameof(Muted))]
    private T Channel
    {
        get => channel;
        set
        {
            var oldChannel = channel;
            if (SetProperty(ref channel, value))
            {
                Notifications.MaybeUnsubscribe(oldChannel, HandleChannelPropertyChanged);
                Notifications.MaybeSubscribe(channel, HandleChannelPropertyChanged);
            }
        }
    }

    public ChannelId ChannelId { get; }
    public string Id { get; }

    public ChannelAppearance Appearance { get; }

    public string DisplayName { get; }
    public string ShortName { get; }

    public bool IsStereo => Channel?.IsStereo ?? false;
    public MeterLevel Output => Channel?.MeterLevel ?? default;
    public MeterLevel StereoOutput => Channel?.StereoMeterLevel ?? default;

    private MeterLevel peakOutput;
    public MeterLevel PeakOutput
    {
        get => peakOutput;
        private set => SetProperty(ref peakOutput, value);
    }

    private MeterLevel stereoPeakOutput;
    public MeterLevel StereoPeakOutput
    {
        get => stereoPeakOutput;
        private set => SetProperty(ref stereoPeakOutput, value);
    }

    public bool Muted
    {
        get => Channel?.Muted ?? false;
        set => Channel?.SetMuted(value);
    }

    public IReadOnlyList<FaderViewModel> Faders { get; protected set; } = new List<FaderViewModel>();

    private readonly PeakBuffer peakBuffer;
    private readonly PeakBuffer stereoPeakBuffer;
    private readonly double? feedbackMutingThreshold;
    private readonly Duration? feedbackMutingDuration;

    /// <summary>
    /// When we last started detecting feedback.
    /// </summary>
    private Instant? feedbackStart;
    /// <summary>
    /// Number of samples we've seen above the feedback threshold.
    /// </summary>
    private int feedbackCount;

    public ChannelViewModelBase(
        ChannelId channelId, ChannelMapping mapping,
        double? feedbackMutingThreshold = null, Duration? feedbackMutingDuration = null)
    {
        ChannelId = channelId;
        Id = mapping.Id;
        Appearance = ChannelAppearance.ForMapping(mapping);
        DisplayName = mapping.EffectiveDisplayName ?? channelId.ToString();
        ShortName = mapping.EffectiveShortName ?? DisplayName;
        // TODO: Make this configurable
        peakBuffer = new PeakBuffer(TimeSpan.FromSeconds(1).Ticks);
        stereoPeakBuffer = new PeakBuffer(TimeSpan.FromSeconds(1).Ticks);
        this.feedbackMutingThreshold = feedbackMutingThreshold;
        this.feedbackMutingDuration = feedbackMutingDuration;
    }

    internal void UpdatePeakOutputs(ILogger logger)
    {
        PeakOutput = peakBuffer.UpdatePeak(Output);
        StereoPeakOutput = stereoPeakBuffer.UpdatePeak(StereoOutput);
        MaybeHandleFeedback(logger);
    }

    private void MaybeHandleFeedback(ILogger logger)
    {
        if (feedbackMutingThreshold is null || feedbackMutingDuration is null)
        {
            return;
        }
        if (Output.Value < feedbackMutingThreshold.Value && StereoOutput.Value < feedbackMutingThreshold.Value)
        {
            feedbackStart = null;
            feedbackCount = 0;
            return;
        }
        // (We can inject a clock if we ever want to test this...)
        var now = SystemClock.Instance.GetCurrentInstant();
        feedbackCount++;
        if (feedbackStart is null)
        {
            feedbackStart = now;
        }
        else
        {
            var duration = now - feedbackStart.Value;
            if (duration >= feedbackMutingDuration.Value)
            {
                logger.LogWarning("Muting channel {channel} ({description}) due to feedback ({count} samples observed)", ChannelId, DisplayName, feedbackCount);
                Muted = true;
            }
        }
    }

    public void UpdateFromMixer(Mixer mixer)
    {
        Channel = GetChannels(mixer).FirstOrDefault(ch => ch.LeftOrMonoChannelId == ChannelId);
        foreach (var fader in Faders)
        {
            fader.PopulateFromMixer(mixer);
        }
    }

    protected abstract IEnumerable<T> GetChannels(Mixer mixer);

    protected void UpdateChannel(IEnumerable<T> channels) =>
        Channel = channels.FirstOrDefault(ch => ch.LeftOrMonoChannelId == ChannelId);

    private void HandleChannelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ChannelBase.Name):
                RaisePropertyChanged(nameof(DisplayName));
                break;
            case nameof(ChannelBase.Muted):
                RaisePropertyChanged(nameof(Muted));
                break;
            case nameof(ChannelBase.MeterLevel):
                RaisePropertyChanged(nameof(Output));
                PeakOutput = peakBuffer.UpdatePeak(Output);
                break;
            case nameof(ChannelBase.StereoMeterLevel):
                RaisePropertyChanged(nameof(StereoOutput));
                StereoPeakOutput = stereoPeakBuffer.UpdatePeak(StereoOutput);
                break;
        }
    }

    private class PeakBuffer
    {
        private Queue<OutputRecord> queue;
        private MeterLevel currentPeak;
        private readonly long bufferPeriodTicks;

        internal PeakBuffer(long bufferPeriodTicks)
        {
            this.bufferPeriodTicks = bufferPeriodTicks;
            queue = new Queue<OutputRecord>();
            currentPeak = MeterLevel.MinValue;
        }

        /// <summary>
        /// Adds a value to the buffer, trimming the buffer and returning the current peak.
        /// </summary>
        internal MeterLevel UpdatePeak(MeterLevel value)
        {
            long now = DateTime.UtcNow.Ticks;

            // First trim, remembering whether or not we've trimmed away our current peak.
            bool trimmedPeak = false;
            while (queue.TryPeek(out var oldest) && oldest.ExpiryTicks < now)
            {
                queue.Dequeue();
                trimmedPeak |= oldest.Value == currentPeak;
            }

            // If the new value is greater than the "old peak", that's definitely
            // the new peak.
            if (value > currentPeak)
            {
                currentPeak = value;
            }

            // Otherwise, if (and only if) we've removed the "old peak", we need
            // to recalculate.
            else if (trimmedPeak)
            {
                currentPeak = queue.Count == 0 ? MeterLevel.MinValue : queue.Max(entry => entry.Value);
            }

            // Now we've worked out the new peak, enqueue the new value with a suitable expiry.
            queue.Enqueue(new OutputRecord(now + bufferPeriodTicks, value));

            return currentPeak;
        }

        record struct OutputRecord(long ExpiryTicks, MeterLevel Value);
    }
}
