// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace OscMixerControl;

/// <summary>
/// Abstracts different OSC addresses for differnet models of mixer. This is
/// a very incomplete abstraction, but allows for X32 and XAir mixers to be handled
/// with the same code.
/// </summary>
public interface IMixerDescriptor
{
    /// <summary>
    /// Returns true if the mixer sends changes made by the client back to itself
    /// (e.g. for XAir mixers) or false if the mixer only sends changes made by
    /// other clients (e.g. for X32 mixers).
    /// </summary>
    bool ReflectsChanges { get; }

    /// <summary>
    /// Returns the name of the meter used for input channel monitoring.
    /// </summary>
    string InputChannelLevelsMeter { get; }

    /// <summary>
    /// Returns the name of the meter used for output channel monitoring.
    /// </summary>
    string OutputChannelLevelsMeter { get; }

    /// <summary>
    /// Returns the value for the given index, as an integer value, which is
    /// 256 * the dB value (for compatibility with earlier code).
    /// </summary>
    short GetMeterValue(byte[] blob, int index);

    /// <summary>
    /// Creates a main input channel representation for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <param name="index">The index of the channel, in the range 1-18.</param>
    /// <param name="stereo">True for stereo inputs (expecting the index to be the lower one); false for mono</param>
    /// <returns>The new channel.</returns>
    Channel CreateInputChannel(Mixer mixer, int index, bool stereo = false);

    /// <summary>
    /// Creates a bus input channel representation for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <param name="busIndex">The index of the bus, in the range 1-6.</param>
    /// <param name="channelIndex">The index of the channel, in the range 1-18.</param>
    /// <param name="stereo">True for stereo inputs (expecting the index to be the lower one); false for mono</param>
    /// <returns>The new channel.</returns>
    Channel CreateBusInputChannel(Mixer mixer, int busIndex, int channelIndex, bool stereo = false);

    /// <summary>
    /// Creates a stereo aux input channel the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <returns>The new channel.</returns>
    Channel CreateAuxInputChannel(Mixer mixer);

    /// <summary>
    /// Creates a stereo aux input channel the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer this channel belongs to.</param>
    /// <param name="busIndex">The index of the bus, in the range 1-6.</param>
    /// <returns>The new channel.</returns>
    Channel CreateBusAuxInputChannel(Mixer mixer, int busIndex);

    /// <summary>
    /// Creates a representation of an aux output bus
    /// for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer the output channel belongs to.</param>
    /// <param name="index">The index of the bus, in the range 1-6.</param>
    /// <returns>The output channel.</returns>
    Channel CreateAuxOutputChannel(Mixer mixer, int index, bool stereo = false);

    /// <summary>
    /// Creates a representation of the main (stereo) output channel
    /// for the given mixer.
    /// </summary>
    /// <param name="mixer">The mixer the output channel belongs to.</param>
    /// <returns>The output channel.</returns>
    Channel CreateMainOutputChannel(Mixer mixer);
}
