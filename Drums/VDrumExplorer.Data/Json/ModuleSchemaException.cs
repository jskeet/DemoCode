using System;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Json
{
    public class ModuleSchemaException : Exception
    {
        public ModuleSchemaException(string message)
            : base(message)
        {
        }

        public ModuleSchemaException(FieldPath path, string message)
            : base($"Schema error at {path}: {message}")
        {
        }
    }
}
