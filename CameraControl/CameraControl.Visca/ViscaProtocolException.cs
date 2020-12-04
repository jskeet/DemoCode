// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace CameraControl.Visca
{
    /// <summary>
    /// Exception to indicate that the camera stream has violated the VISCA protocol.
    /// </summary>
    public class ViscaProtocolException : Exception
    {
        public ViscaProtocolException()
        {
        }

        public ViscaProtocolException(string message) : base(message)
        {
        }

        public ViscaProtocolException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
