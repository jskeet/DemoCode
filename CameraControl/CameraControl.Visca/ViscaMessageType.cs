// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace CameraControl.Visca;

internal enum ViscaMessageType
{
    Command = 0x01_00,
    Inquiry = 0x01_10,
    Reply = 0x01_11,
    DeviceSetting = 0x01_20,
    Control = 0x02_00,
    ControlReply = 0x02_01
}
