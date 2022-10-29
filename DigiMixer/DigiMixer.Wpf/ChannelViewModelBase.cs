using DigiMixer.Core;
using DigiMixer.Wpf.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DigiMixer.Wpf;

public class ChannelViewModelBase<T> : ViewModelBase<T> where T : ChannelBase
{
    public ChannelViewModelBase(T model, string id, string displayName) : base(model)
    {
        Id = id;
        this.displayName = displayName;
        // TODO: Make this configurable
        peakBuffer = new PeakBuffer(TimeSpan.FromSeconds(1).Ticks);
        stereoPeakBuffer = new PeakBuffer(TimeSpan.FromSeconds(1).Ticks);
    }

    public string Id { get; }

    private string displayName;
    public string DisplayName => displayName ?? Name ?? Model.FallbackName;

    public bool Visible => Name is not null;

    public bool IsStereo => Model.IsStereo;
    public string Name => Model.Name;
    public MeterLevel Output => Model.MeterLevel;
    public MeterLevel StereoOutput => Model.StereoMeterLevel;

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

    internal void UpdatePeakOutputs()
    {
        PeakOutput = peakBuffer.UpdatePeak(Output);
        StereoPeakOutput = stereoPeakBuffer.UpdatePeak(StereoOutput);
    }

    private readonly PeakBuffer peakBuffer;
    private readonly PeakBuffer stereoPeakBuffer;

    public bool Muted
    {
        get => Model.Muted;
        set => Model.SetMuted(value).Ignore();
    }

    protected override void OnPropertyModelChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ChannelBase.Name):
                RaisePropertyChanged(nameof(Name));
                RaisePropertyChanged(nameof(DisplayName));
                RaisePropertyChanged(nameof(Visible));
                break;
            case nameof(ChannelBase.Muted):
                RaisePropertyChanged(nameof(Muted));
                break;
            case nameof(ChannelBase.MeterLevel):
                RaisePropertyChanged(nameof(Output));
                break;
            case nameof(ChannelBase.StereoMeterLevel):
                RaisePropertyChanged(nameof(StereoOutput));
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
