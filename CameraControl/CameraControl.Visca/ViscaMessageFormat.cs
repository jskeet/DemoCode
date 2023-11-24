// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace CameraControl.Visca;

public enum ViscaMessageFormat
{
    /// <summary>
    /// The message payload is delivered directly, with no header.
    /// </summary>
    Raw = 0,
    /// <summary>
    /// Each message starts with a header containing the type of the message,
    /// the payload length, and a sequence number, before the payload.
    /// </summary>
    Encapsulated = 1
}
