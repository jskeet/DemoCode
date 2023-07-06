// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json.Linq;

namespace DigiMixer.Ssc;

/// <summary>
/// A property within an <see cref="SscMessage"/>.
/// </summary>
/// <param name="Address">The address of the property.</param>
/// <param name="Value">The value of the property, which may be null.</param>
public record SscProperty(string Address, object? Value)
{
    /// <summary>
    /// Constructs a property from the given errors, primarily for testing.
    /// </summary>
    /// <param name="errors">The errors to convert into a property.</param>
    /// <returns>An error property containing the given errors.</returns>
    public static SscProperty FromErrors(params SscError[] errors) =>
        FromErrors((IEnumerable<SscError>) errors);

    /// <summary>
    /// Constructs a property from the given errors, primarily for testing.
    /// </summary>
    /// <param name="errors">The errors to convert into a property.</param>
    /// <returns>An error property containing the given errors.</returns>
    public static SscProperty FromErrors(IEnumerable<SscError> errors)
    {
        JObject obj = new JObject();
        foreach (var error in errors)
        {
            var segments = error.Address.Split('/');
            JObject current = obj;
            for (int i = 1; i < segments.Length - 1; i++)
            {
                string segment = segments[i];
                if (current.ContainsKey(segment))
                {
                    // The way we're constructing this, the indexer will never return null.
                    current = (JObject) current[segment]!;
                }
                else
                {
                    var nestedObj = new JObject();
                    current[segment] = nestedObj;
                    current = nestedObj;
                }
            }
            current[segments.Last()] = error.ToArray();
        }
        return new SscProperty(SscAddresses.Osc.Error, new JArray(obj));
    }
}
