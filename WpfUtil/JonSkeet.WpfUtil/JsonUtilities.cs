// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using System.IO;

namespace JonSkeet.WpfUtil;

/// <summary>
/// Centralized JSON conversion code to avoid inconsistent settings etc.
/// This class's presence is far from ideal - it's more "this is used in a lot of
/// apps Jon writes" rather than "this logically belongs with other WPF utilities"
/// but it's pragmatic.
/// </summary>
public static class JsonUtilities
{
    private static readonly JsonSerializerSettings serializationSettings = new JsonSerializerSettings
    {
        DefaultValueHandling = DefaultValueHandling.Ignore,
        Converters = { new StringEnumConverter() }
    }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    private static readonly JsonSerializerSettings deserializationSettings = new JsonSerializerSettings
    {
        DateParseHandling = DateParseHandling.None
    }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    public static T LoadJson<T>(string file) => Parse<T>(File.ReadAllText(file));

    public static async Task<T> LoadJsonAsync<T>(string file) => Parse<T>(await File.ReadAllTextAsync(file));

    public static string SaveJson(string file, object model)
    {
        string json = ToJson(model);
        File.WriteAllText(file, json);
        return json;
    }

    public static async Task<string> SaveJsonAsync(string file, object model)
    {
        string json = ToJson(model);
        await File.WriteAllTextAsync(file, json);
        return json;
    }

    public static string ToJsonOrNull(object model) => model is null ? null : ToJson(model, Formatting.Indented);

    public static string ToJson(object model) => ToJson(model, Formatting.Indented);

    public static string ToJson(object model, Formatting formatting) => JsonConvert.SerializeObject(model, formatting, serializationSettings);

    public static T Parse<T>(string json) => JsonConvert.DeserializeObject<T>(json, deserializationSettings);

    public static T Clone<T>(T model) => Parse<T>(ToJson(model));

    /// <summary>
    /// Parse the given file, indicating whether it is valid with a strict interpretation (no unknown members)
    /// or whether it required more lenient settings.
    /// </summary>
    public static bool TryParseFileStrict<T>(string file, ILogger logger, out T model, out JsonException parseFailure)
    {
        var json = File.ReadAllText(file);
        return TryParseJsonStrict(json, logger, out model, out parseFailure);
    }

    public static bool TryParseJsonStrict<T>(string json, ILogger logger, out T model, out JsonException parseFailure)
    {
        var settings = new JsonSerializerSettings(deserializationSettings)
        {
            MissingMemberHandling = MissingMemberHandling.Error
        };
        try
        {
            model = JsonConvert.DeserializeObject<T>(json, settings);
            parseFailure = null;
            return true;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failure when performing strict parsing");
            model = Parse<T>(json);
            parseFailure = ex;
            return false;
        }
    }

    /// <summary>
    /// Checks whether the two given pieces of JSON are equivalent,
    /// by parsing them both as JObjects and reserializing.
    /// </summary>
    public static bool CheckEquivalent(string json1, string json2)
    {
        var reserialized1 = ToJson(JObject.Parse(json1));
        var reserialized2 = ToJson(JObject.Parse(json2));
        return reserialized1 == reserialized2;
    }
}
