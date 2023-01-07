// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDrumExplorer.Model;

namespace VDrumExplorer.Proto
{
    internal partial class ModuleIdentifier
    {
        internal Model.Midi.ModuleIdentifier ToModel() =>
            new Model.Midi.ModuleIdentifier(Name, ModelId, FamilyCode, FamilyNumberCode, SoftwareRevision);
        
        internal static ModuleIdentifier FromModel(Model.Midi.ModuleIdentifier id) =>
            new ModuleIdentifier
            {
                Name = id.Name,
                ModelId = id.ModelId,
                FamilyCode = id.FamilyCode,
                FamilyNumberCode = id.FamilyNumberCode,
                SoftwareRevision = id.SoftwareRevision
            };

        /// <summary>
        /// Returns the schema associated with this module identifier, inferring the software revision if it's not present.
        /// </summary>
        /// <param name="schemaValidator">A validator to use when inferring the software revision if necessary.</param>
        /// <param name="logger">Logger to alert the user to any inference required.</param>
        /// <returns>The schema associated with this identifier.</returns>
        /// <exception cref="InvalidDataException">No schema matches the identifier.</exception>
        internal ModuleSchema GetOrInferSchema(Func<ModuleSchema, bool> schemaValidator, ILogger logger)
        {
            var modelIdentifier = HasSoftwareRevision ? ToModel() : InferModelWithRevision(schemaValidator, logger);
            if (!ModuleSchema.KnownSchemas.TryGetValue(modelIdentifier, out var schema))
            {
                throw new InvalidDataException($"No known schema matches identifier {modelIdentifier}");
            }
            return schema.Value;
        }

        private Model.Midi.ModuleIdentifier InferModelWithRevision(Func<ModuleSchema, bool> validator, ILogger logger)
        {
            logger.LogInformation("File has no software revision in the module identifier. Attempting to infer an appropriate revision.");
            var thisModel = ToModel();
            var candidates = ModuleSchema.KnownSchemas
                .Where(pair => thisModel.WithSoftwareRevision(pair.Key.SoftwareRevision).Equals(pair.Key))
                .Select(pair => pair.Value.Value)
                .Where(validator)
                .ToList();
            switch (candidates.Count)
            {
                case 0:
                    // An exception will be thrown in the calling code
                    return thisModel.WithSoftwareRevision(-1);
                case 1:
                    logger.LogInformation("Exactly one software revision matches. Assuming this is the correct revision.");
                    return candidates[0].Identifier;
                default:
                    logger.LogInformation("Multiple software revisions match; picking the one with the latest software revision.");
                    return candidates.Select(schema => schema.Identifier).OrderByDescending(id => id.SoftwareRevision).First();
            }
        }
    }
}
