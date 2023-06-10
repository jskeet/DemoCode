// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json.Linq;

namespace DigiMixer.Ssc;

/// <summary>
/// An error, represented within a property with address <see cref="SscAddresses.Error"/>.
/// </summary>
/// <param name="Address">The address of the property which triggered the error.</param>
/// <param name="Code">The error code.</param>
/// <param name="Description">The description of the error, if any.</param>
public record SscError(string Address, long Code, string? Description)
{
    /// <summary>
    /// Converts the given address and array into an <see cref="SscError"/>, if
    /// the array is valid for an error.
    /// </summary>
    /// <param name="address">The address of the error.</param>
    /// <param name="array">The array containing the code and description.</param>
    /// <returns>An error from the given parameters, or null if the array does not represent error details.</returns>
    internal static SscError? FromAddressAndArray(string address, JArray array)
    {
        if (array.Count == 0)
        {
            return null;
        }
        var codeToken = array[0];
        if (codeToken is not JValue { Type: JTokenType.Integer } codeValue)
        {
            return null;
        }
        long code = codeValue.Value<long>();
        string? description = null;
        if (array.Count > 1 && array[1] is JObject obj)
        {
            if (obj.TryGetValue("desc", StringComparison.Ordinal, out var descToken) &&
                descToken is JValue { Type: JTokenType.String } descValue)
            {
                description = descValue.Value<string>();
            }
        }
        return new SscError(address, code, description);
    }

    /// <summary>
    /// Converts the code and description of the error to an array. This is the value
    /// of the address in the overall /osc/error value.
    /// </summary>
    /// <returns>An array with the code and description (if any).</returns>
    internal JArray ToArray()
    {
        var array = new JArray(Code);
        if (Description is string desc)
        {
            array.Add(new JObject { ["desc"] = desc });
        }
        return array;
    }
}
