// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model;

namespace VDrumExplorer.ViewModel
{
    /// <summary>
    /// Convenient extension methods on model classes.
    /// </summary>
    internal static class ModelExtensions
    {
        public static int ValidateKitNumber(this ModuleSchema? schema, int value) =>
            value >= 1 && value <= schema?.KitRoots.Count
            ? value
            : throw new ArgumentOutOfRangeException("Invalid kit number");

        public static int ValidateUserSampleNumber(this ModuleSchema? schema, int value) =>
            value >= 1 && value <= schema?.UserSampleInstruments.Count
            ? value
            : throw new ArgumentOutOfRangeException("Invalid sample number");
    }
}
