using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data
{
    /// <summary>
    /// A module consists of a <see cref="ModuleData"/> and
    /// the <see cref="ModuleSchema"/> that the data abides by.
    /// </summary>
    public sealed class Module
    {
        public ModuleSchema Schema { get; }
        public ModuleData Data { get; }

        public Module(ModuleSchema schema, ModuleData data) =>
            (Schema, Data) = (schema, data);

        /// <summary>
        /// Loads module data, autodetecting the schema using the <see cref="SchemaRegistry"/>.
        /// </summary>
        public static Module FromStream(Stream stream)
        {
            Header header;
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                header = Header.Load(reader);
                foreach (var schema in SchemaRegistry.GetSchemas())
                {
                    if (header.Equals(Header.FromSchema(schema)))
                    {
                        var data = ModuleData.Load(reader);
                        return new Module(schema, data);
                    }
                }
            }
            throw new InvalidOperationException($"No built-in schemas match the file's header ({header})");
        }

        /// <summary>
        /// Validates that every field in the schema has a valid value.
        /// This will fail if only partial data has been loaded.
        /// </summary>
        public void Validate()
        {
            List<Exception> exceptions = new List<Exception>();
            foreach (var field in Schema.Root.DescendantsAndSelf(this).OfType<IPrimitiveField>())
            {
                try
                {
                    field.GetText(this);
                }
                catch (Exception e)
                {
                    exceptions.Add(new InvalidOperationException($"Field {field.Path} failed validation: {e.Message}"));
                }
            }
            if (exceptions.Count != 0)
            {
                throw new AggregateException("Validation failed", exceptions.ToArray());
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                var header = Header.FromSchema(Schema);
                header.Save(writer);
                Data.Save(writer);
            }
        }
        
        private sealed class Header : IEquatable<Header?>
        {
            public const int CurrentFormatVersion = 1;

            public int FormatVersion { get; }
            public int ModelId { get; }
            public int FamilyCode { get; }
            public int FamilyNumberCode { get; }
            public string Name { get; }

            public Header(int formatVersion, int modelId, int familyCode, int familyNumberCode, string name) =>
                (FormatVersion, ModelId, FamilyCode, FamilyNumberCode, Name) = (formatVersion, modelId, familyCode, familyNumberCode, name);

            public void Save(BinaryWriter writer)
            {
                writer.Write(FormatVersion);
                writer.Write(ModelId);
                writer.Write(FamilyCode);
                writer.Write(FamilyNumberCode);
                writer.Write(Name);
            }

            public static Header Load(BinaryReader reader)
            {
                var version = reader.ReadInt32();
                if (version != CurrentFormatVersion)
                {
                    throw new InvalidOperationException($"Unknown file format version. Expected {CurrentFormatVersion}; was {version}");
                }
                var modelId = reader.ReadInt32();
                var familyCode = reader.ReadInt32();
                var familyNumberCode = reader.ReadInt32();
                var name = reader.ReadString();
                return new Header(version, modelId, familyCode, familyNumberCode, name);
            }

            public override bool Equals(object? obj) => Equals(obj as Header);

            public bool Equals(Header? other) =>
                other != null &&
                FormatVersion == other.FormatVersion &&
                Name == other.Name &&
                ModelId == other.ModelId &&
                FamilyCode == other.FamilyCode &&
                FamilyNumberCode == other.FamilyNumberCode;

            public override int GetHashCode() => HashCode.Combine(FormatVersion, ModelId, FamilyCode, FamilyNumberCode, Name.GetHashCode());

            // TODO: Work out how to handle compatibility.
            public static Header FromSchema(ModuleSchema schema) =>
                new Header(CurrentFormatVersion, schema.ModelId, schema.FamilyCode, schema.FamilyNumberCode, schema.Name);

            public override string ToString() => $"Name: {Name}; Midi ID: {ModelId}; Family code: {FamilyCode}; Family number code: {FamilyNumberCode}";
        }

    }
}
