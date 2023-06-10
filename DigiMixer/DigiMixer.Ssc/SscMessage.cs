// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DigiMixer.Ssc;

/// <summary>
/// A list-based representation of an SSC message, consisting of a collection of properties.
/// </summary>
public sealed class SscMessage
{
    private static readonly JsonSerializerSettings deserializationSettings = new JsonSerializerSettings
    {
        DateParseHandling = DateParseHandling.None
    };

    /// <summary>
    /// The ID of the message, as represented by the <see cref="SscAddresses.Xid"/> property.
    /// If the property does not exist or does not have a string value, this is null.
    /// </summary>
    public string? Id { get; }
    public IReadOnlyList<SscProperty> Properties { get; }
    public IReadOnlyList<SscError> Errors { get; }

    /// <summary>
    /// Creates a message with the given properties.
    /// </summary>
    public SscMessage(params SscProperty[] properties) : this(properties, true, nameof(properties))
    {
    }

    /// <summary>
    /// Creates a message with the given properties.
    /// </summary>
    public SscMessage(IEnumerable<SscProperty> properties) : this(properties, true, nameof(properties))
    {
    }

    /// <summary>
    /// Creates a message with the given addresses, where the value for each address is null.
    /// </summary>
    public SscMessage(params string[] addresses) : this((IEnumerable<string>) addresses)
    {
    }

    /// <summary>
    /// Creates a message with the given addresses, where the value for each address is null.
    /// </summary>
    public SscMessage(IEnumerable<string> addresses)
        : this(addresses.Select(addr => new SscProperty(addr, null)), true, nameof(addresses))
    {
    }

    /// <summary>
    /// Returns the property with the given address, or null if there is no such property.
    /// </summary>
    /// <param name="address">The address to find within the list of properties.</param>
    /// <returns>The property with the given address, or null if there is no such property.</returns>
    public SscProperty? GetProperty(string address) => Properties.Where(p => p.Address == address).FirstOrDefault();

    /// <summary>
    /// Returns a new instance with the same properties as this message, but with the given ID.
    /// This message is not affected.
    /// </summary>
    /// <param name="id">The ID to expose in the new message. If this is null, the new message
    /// will not have a property for <see cref="SscAddresses.Xid"/>.</param>
    /// <returns>A new message with the given properties.</returns>
    public SscMessage WithId(string? id)
    {
        // TODO: Possibly optimize by returning "this" if the ID is the existing one;
        // this is slightly fiddly as if the parameter is null but there's a non-string-valued property,
        // we can't use the optimization even though Id is still correct.

        var propertiesWithoutId = Properties.Where(p => p.Address != SscAddresses.Xid);
        var newProperties = id is null
            ? propertiesWithoutId
            : propertiesWithoutId.Append(new SscProperty(SscAddresses.Xid, id));
        return new SscMessage(newProperties, clone: true, addressValidationParameterName: null);
    }

    private SscMessage(IEnumerable<SscProperty> properties, bool clone, string? addressValidationParameterName)
    {
        Properties = clone ? properties.ToList().AsReadOnly() : (IReadOnlyList<SscProperty>) properties;
        Errors = DeriveErrors().ToList().AsReadOnly();
        Id = Properties.FirstOrDefault(prop => prop.Address == SscAddresses.Xid)?.Value as string;

        if (addressValidationParameterName is not null)
        {
            foreach (var property in Properties)
            {
                if (!property.Address.StartsWith('/'))
                {
                    throw new ArgumentException("SSC address must start with /", addressValidationParameterName);
                }
                if (property.Address.EndsWith('/'))
                {
                    throw new ArgumentException("SSC address must not end with /", addressValidationParameterName);
                }
            }
        }
    }

    public string ToJson()
    {
        JObject obj = new JObject();
        foreach (var property in Properties)
        {
            var segments = property.Address.Split('/');
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
            // Assume an appropriate conversion.
            current[segments.Last()] = property.Value is JToken token ? token : new JValue(property.Value);
        }
        return obj.ToString(Formatting.None);
    }

    public static SscMessage FromJson(string json)
    {
        var properties = new List<SscProperty>();
        JObject obj = JsonConvert.DeserializeObject<JObject>(json, deserializationSettings)
            ?? throw new ArgumentException("JSON must contain an object", nameof(json));
        PopulateProperties(obj, "");
        return new SscMessage(properties.AsReadOnly(), clone: false, null);

        void PopulateProperties(JObject node, string address)
        {
            foreach (var prop in node.Properties())
            {
                string name = $"{address}/{prop.Name}";
                if (prop.Value is JObject child)
                {
                    PopulateProperties(child, name);
                }
                else
                {
                    var value = prop.Value is JValue primitive ? primitive.Value : prop.Value;
                    properties.Add(new SscProperty(name, value));
                }
            }
        }
    }

    /// <summary>
    /// Derives errors from the existing properties.
    /// - Errors are stored in a property with an address of /osc/error
    /// - The value of the property must be an array
    /// - Each element of the array must be an object (or we ignore it)
    /// - Each object element of the array has the same structure as an SSC message:
    ///   - Nested objects create an address structure
    ///   - The actual error details are in child arrays:
    ///     - The first element is numeric, and is the error code
    ///     - The second element is an object with a "desc" property which is the description
    /// </summary>
    private IEnumerable<SscError> DeriveErrors()
    {
        var errorsProperty = Properties.FirstOrDefault(p => p.Address == SscAddresses.Error);
        if (errorsProperty is null || errorsProperty.Value is not JArray array)
        {
            yield break;
        }
        foreach (var obj in array.Children<JObject>())
        {
            foreach (var error in GetErrorsFromArrayElement(obj))
            {
                yield return error;
            }
        }

        IEnumerable<SscError> GetErrorsFromArrayElement(JObject obj)
        {
            var errors = new List<SscError>();
            PopulateErrors(obj, "");
            return errors;
            void PopulateErrors(JObject node, string address)
            {
                foreach (var prop in node.Properties())
                {
                    string childAddress = $"{address}/{prop.Name}";
                    if (prop.Value is JObject child)
                    {
                        PopulateErrors(child, childAddress);
                    }
                    else if (prop.Value is JArray childArray)
                    {
                        var error = SscError.FromAddressAndArray(childAddress, childArray);
                        if (error is not null)
                        {
                            errors.Add(error);
                        }
                    }
                }
            }
        }
    }
}