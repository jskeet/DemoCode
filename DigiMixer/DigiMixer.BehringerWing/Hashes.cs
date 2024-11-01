using DigiMixer.Core;
using System.Collections.Immutable;

namespace DigiMixer.BehringerWing;

/// <summary>
/// The node hashes associated with an input channel.
/// </summary>
internal partial record InputChannelHashes(ChannelId Id, uint Name, uint Fader, uint Mute, uint StereoMode, ImmutableList<uint> OutputLevels);

/// <summary>
/// The node hashes associated with an output channel.
/// </summary>
internal partial record OutputChannelHashes(ChannelId Id, uint Name, uint Fader, uint Mute);
