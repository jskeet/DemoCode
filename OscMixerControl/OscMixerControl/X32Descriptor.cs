// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace OscMixerControl;

/// <summary>
/// The <see cref="IMixerDescriptor"/> for X32 mixers.
/// </summary>
public class X32Descriptor : IMixerDescriptor
{
    public static X32Descriptor Instance { get; } = new X32Descriptor();

    private X32Descriptor()
    {
    }

    public string InputChannelLevelsMeter => "/meters/1";
    public string OutputChannelLevelsMeter => "/meters/2";
    public bool ReflectsChanges => false;

    public short GetMeterValue(byte[] blob, int index)
    {
        // TODO: Make this a lot better
        float linear = BitConverter.ToSingle(blob, index * 4 + 4);
        double db = linear switch
        {
            float f when f >= 0.8f => (f - 1) * 12.5,
            float f when f >= 0.55f => (f - 0.8) * 10 - 2.5,
            float f when f >= 0.30f => (f - 0.55) * 20 - 5,
            float f when f >= 0.10f => (f - 0.3) * 50 - 10,
            float f when f >= 0.03f => (f - 0.1) * 142.9 - 20,
            float f when f >= 0.01f => (f - 0.03) * 500 - 30,
            _ => short.MinValue / 256d
        };
        return (short) (db * 256);
    }

    /// <summary>
    /// Creates a main input channel representation for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <param name="index">The index of the channel, in the range 1-18.</param>
    /// <param name="stereo">True for stereo inputs (expecting the index to be the lower one); false for mono</param>
    /// <returns>The new channel.</returns>
    public Channel CreateInputChannel(Mixer mixer, int index, bool stereo = false)
    {
        var prefix = $"/ch/{index:00}";
        return new Channel(mixer,
            $"{prefix}/config/name",
            $"{prefix}/mix/fader",
            InputChannelLevelsMeter,
            meterIndex: index - 1,
            meterIndex2: stereo ? index : default(int?),
            $"{prefix}/mix/on");
    }

    /// <summary>
    /// Creates a bus input channel representation for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <param name="busIndex">The index of the bus, in the range 1-6.</param>
    /// <param name="channelIndex">The index of the channel, in the range 1-18.</param>
    /// <param name="stereo">True for stereo inputs (expecting the index to be the lower one); false for mono</param>
    /// <returns>The new channel.</returns>
    public Channel CreateBusInputChannel(Mixer mixer, int busIndex, int channelIndex, bool stereo = false)
    {
        var prefix = $"/ch/{channelIndex:00}";
        return new Channel(mixer,
            // Channel has one name overall
            $"{prefix}/config/name",
            $"{prefix}/mix/{busIndex:00}/level",
            // This is the raw input, so doesn't depend on bus.
            InputChannelLevelsMeter,
            meterIndex: channelIndex - 1,
            meterIndex2: stereo ? channelIndex : default(int?),
            // This is the main mute; that's still
            // generally what's wanted.
            $"{prefix}/mix/on");
    }

    /// <summary>
    /// Creates a stereo aux input channel the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <returns>The new channel.</returns>
    public Channel CreateAuxInputChannel(Mixer mixer)
    {
        var prefix = $"/rtn/aux";
        return new Channel(mixer,
            $"{prefix}/config/name",
            $"{prefix}/mix/fader",
            InputChannelLevelsMeter,
            meterIndex: 16, // Aux is effectively channels 17 and 18 for meters
            meterIndex2: 17,
            $"{prefix}/mix/on");
    }

    /// <summary>
    /// Creates a stereo aux input channel the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <param name="busIndex">The index of the bus, in the range 1-6.</param>
    /// <returns>The new channel.</returns>
    public Channel CreateBusAuxInputChannel(Mixer mixer, int busIndex)
    {
        // FIXME - no aux
        var prefix = $"/rtn/aux";
        return new Channel(mixer,
            $"{prefix}/config/name",
            $"{prefix}/mix/{busIndex:00}/level",
            InputChannelLevelsMeter,
            meterIndex: 16, // Aux is effectively channels 17 and 18 for meters
            meterIndex2: 17,
            $"{prefix}/mix/on");
    }

    /// <summary>
    /// Creates a representation of an aux output bus
    /// for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer the output channel belongs to.</param>
    /// <param name="index">The index of the bus, in the range 1-6.</param>
    /// <returns>The output channel.</returns>
    public Channel CreateAuxOutputChannel(Mixer mixer, int index, bool stereo = false)
    {
        var prefix = $"/bus/{index:00}";
        return new Channel(mixer,
            $"{prefix}/config/name",
            $"{prefix}/mix/fader",
            OutputChannelLevelsMeter,
            meterIndex: index - 1,
            meterIndex2: stereo ? index : default(int?),
            $"{prefix}/mix/on");
    }

    /// <summary>
    /// Creates a representation of the main (stereo) output channel
    /// for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer the output channel belongs to.</param>
    /// <returns>The output channel.</returns>
    public Channel CreateMainOutputChannel(Mixer mixer) =>
        new Channel(mixer,
            "/main/st/config/name",
            "/main/st/mix/fader",
            OutputChannelLevelsMeter,
            meterIndex: 22,
            meterIndex2: 23,
            "/main/st/mix/on");
}
