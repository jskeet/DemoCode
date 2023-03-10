// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Json;

/// <summary>
/// I/O methods for JSON representations of V-Drum data.
/// 
/// Format for module files:
/// 
/// {
///   "identifier": { "name": "...", "modelId": ..., "familyCode": ..., "familyNumberCode": ..., "softwareRevision": ... },
///   "moduleData": { ...}
/// }
/// 
/// Format for kit files:
/// 
/// {
///   "identifier": { ... },
///   "defaultKitNumber": ...,
///   "kitData": { ... }
/// }
/// </summary>
public static class JsonIo
{
    private const string IdentifierProperty = "identifier";
    private const string ModuleDataProperty = "moduleData";
    private const string KitDataProperty = "kitData";
    private const string DefaultKitNumberProperty = "defaultKitNumber";

    /// <summary>
    /// Creates a model from the given data. This is a convenience method over <see cref="ReadModel(TextReader, ILogger)"/>.
    /// </summary>
    /// <param name="reader">The stream to read data from.</param>
    /// <param name="logger">A logger to report non-fatal validation issues.</param>
    /// <returns>The model data. The type depends on the data in the stream.</returns>
    public static object ReadModel(string json, ILogger logger)
    {
        using var reader = new StringReader(json);
        return ReadModel(reader, logger);
    }

    /// <summary>
    /// Creates a model from the data in a text stream.
    /// </summary>
    /// <param name="reader">The stream to read data from.</param>
    /// <param name="logger">A logger to report non-fatal validation issues.</param>
    /// <returns>The model data. The type depends on the data in the stream.</returns>
    public static object ReadModel(TextReader reader, ILogger logger)
    {
        // TODO: Avoid reading it all in one go :)
        JObject obj = JObject.Parse(reader.ReadToEnd());
        var schemaId = GetModuleIdentifier(obj);
        var schema = ModuleSchema.KnownSchemas[schemaId].Value;
        var kitData = obj[KitDataProperty];
        var moduleData = obj[ModuleDataProperty];
        var defaultKitNumber = obj[DefaultKitNumberProperty];

        if (kitData is not null && moduleData is not null)
        {
            throw new InvalidDataException($"Either '{ModuleDataProperty}' and {KitDataProperty}' must be present, but not both.");
        }

        if (kitData is JObject kitObj)
        {
            if (defaultKitNumber is not JToken { Type: JTokenType.Integer })
            {
                throw new InvalidDataException($"Missing or invalid'{DefaultKitNumberProperty}'.");
            }
            return ReadKitData(kitObj, schema, (int) defaultKitNumber, logger);
        }
        else if (moduleData is JObject moduleObj)
        {
            return ReadModuleData(moduleObj, schema, logger);
        }
        else
        {
            throw new InvalidDataException($"Either '{ModuleDataProperty}' and {KitDataProperty}' must be present as a JSON object.");
        }
    }

    private static ModuleIdentifier GetModuleIdentifier(JObject parent)
    {
        var jsonId = parent[IdentifierProperty];
        if (jsonId is not JObject)
        {
            throw new InvalidDataException($"'{IdentifierProperty}' property is missing or invalid.");
        }
        return jsonId.ToObject<ModuleIdentifier>()!;
    }

    /// <summary>
    /// Loads a model from the data in a file. This is a convenience method to call
    /// <see cref="ReadModel(TextReader, ILogger)"/> using a file.
    /// </summary>
    /// <param name="file">The file to load.</param>
    /// <param name="logger">The logger to write validation results to.</param>
    /// <returns>The model data. The type depends on the data in the stream.</returns>
    public static object LoadModel(string file, ILogger logger)
    {
        using var reader = File.OpenText(file);
        return ReadModel(reader, logger);
    }

    // Note: these methods are currently called by the convenience methods in ModelExtensions.
    // We could make them public if we wanted.
    internal static void Write(TextWriter writer, Module module)
    {
        var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName(IdentifierProperty);
        new JsonSerializer().Serialize(jsonWriter, module.Schema.Identifier);
        jsonWriter.WritePropertyName(ModuleDataProperty);
        WriteNode(jsonWriter, module.Schema.PhysicalRoot, module.Data);
        jsonWriter.WriteEndObject();
        jsonWriter.Flush();
    }

    internal static void Write(TextWriter writer, Kit kit)
    {
        var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName(IdentifierProperty);
        new JsonSerializer().Serialize(jsonWriter, kit.Schema.Identifier);
        jsonWriter.WritePropertyName(DefaultKitNumberProperty);
        jsonWriter.WriteValue(kit.DefaultKitNumber);
        jsonWriter.WritePropertyName(KitDataProperty);
        WriteNode(jsonWriter, kit.KitRoot.Container, kit.Data);
        jsonWriter.WriteEndObject();
        jsonWriter.Flush();
    }

    internal static void WriteNode(JsonWriter jsonWriter, IContainer container, ModuleData data)
    {
        jsonWriter.WriteStartObject();

        switch (container)
        {
            case FieldContainer fields:
                foreach (var (name, value) in data.GetDataFieldFormattedValues(fields))
                {
                    jsonWriter.WritePropertyName(name);
                    jsonWriter.WriteValue(value);
                }
                break;
            case ContainerContainer containers:
                foreach (var child in containers.Containers)
                {
                    jsonWriter.WritePropertyName(child.Name);
                    WriteNode(jsonWriter, child, data);
                }
                break;
            default:
                throw new Exception($"Unknown IContainer type: {container.GetType()}");
        }

        jsonWriter.WriteEndObject();
    }

    private static Kit ReadKitData(JObject obj, ModuleSchema schema, int defaultKitNumber, ILogger logger)
    {
        var data = ModuleData.FromLogicalRootNode(schema.Kit1Root);
        MergeData(obj, data, schema.Kit1Root.Container, logger);
        return new Kit(data, defaultKitNumber);
    }

    private static Module ReadModuleData(JObject obj, ModuleSchema schema, ILogger logger)
    {
        var data = ModuleData.FromLogicalRootNode(schema.LogicalRoot);
        MergeData(obj, data, schema.LogicalRoot.Container, logger);
        return new Module(data);
    }

    private static void MergeData(JObject obj, ModuleData data, IContainer container, ILogger logger)
    {
        switch (container)
        {
            case FieldContainer fc:
                MergeFields(fc);
                break;
            case ContainerContainer cc:
                MergeContainers(cc);
                break;
            default:
                throw new InvalidOperationException($"Unexpected container type: {container.GetType()}");
        }

        void MergeFields(FieldContainer fc)
        {
            var pairs = new List<(string, string)>();
            foreach (var property in obj.Properties())
            {
                if (property.Value is not JToken { Type: JTokenType.String })
                {
                    logger.LogWarning("Invalid property type for property '{property.Name}'");
                    continue;
                }
                pairs.Add((property.Name, (string) property.Value!));
            }
            data.MergeTextValues(fc, pairs, logger);
        }

        void MergeContainers(ContainerContainer cc)
        {
            foreach (var property in obj.Properties())
            {
                if (property.Value is not JObject propertyObj)
                {
                    logger.LogWarning("Invalid JSON value for property '{property}' in container '{container}'", property.Name, cc.Path);
                    continue;
                }
                var container = cc.GetContainerOrNull(property.Name);
                if (container is null)
                {
                    logger.LogWarning("Ignoring property '{property}' in container '{container}': no such child", property.Name, cc.Path);
                    continue;
                }
                MergeData(propertyObj, data, container, logger);
            }
        }
    }
}
