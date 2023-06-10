// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace DigiMixer.Ssc;

/// <summary>
/// Well-known SSC addresses.
/// </summary>
public static class SscAddresses
{
    /// <summary>
    /// The address of the xid property, /osc/xid. This can be
    /// used to correlate requests with responses.
    /// </summary>
    public const string Xid = "/osc/xid";

    /// <summary>
    /// The address of the error property, /osc/error.
    /// </summary>
    /// <remarks>
    /// The value of this property is an array of objects (typically only one).
    /// Each object represents a tree of errors, in the same form as
    /// a normal message. Each address with an error has a value of an array,
    /// with an integer element (the error code) followed by an object element
    /// with a "desc" property for the description of the error.
    /// </remarks>
    public const string Error = "/osc/error";
}
