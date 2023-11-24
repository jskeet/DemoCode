// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace CameraControl.Visca;

/// <summary>
/// Represents a failure successfully reported by a VISCA endpoint.
/// </summary>
public class ViscaResponseException : Exception
{
    public ViscaResponseException()
    {
    }

    public ViscaResponseException(string message) : base(message)
    {
    }

    public ViscaResponseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
