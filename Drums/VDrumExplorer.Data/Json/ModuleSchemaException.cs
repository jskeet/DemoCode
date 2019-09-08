// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Data.Json
{
    public class ModuleSchemaException : Exception
    {
        public ModuleSchemaException(string message)
            : base(message)
        {
        }
    }
}
