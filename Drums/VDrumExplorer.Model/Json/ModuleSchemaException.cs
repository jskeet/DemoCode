// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Model.Json
{
    /// <summary>
    /// Indicates an error in the JSON representation of a schema.
    /// </summary>
    public class ModuleSchemaException : Exception
    {
        public ModuleSchemaException(string message) : base(message)
        {
        }
    }
}
