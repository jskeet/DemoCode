// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Globalization;
using System.Linq;
using System.Windows.Input;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Layout;

namespace VDrumExplorer.Wpf
{
    internal static class KitInputValidation
    {
        /// <summary>
        /// Used to check that the input for the kit number is entirely ASCII digits.
        /// </summary>
        internal static void CheckDigits(object sender, TextCompositionEventArgs e) =>
            e.Handled = !e.Text.All(c => c >= '0' && c <= '9');

        internal static bool TryGetKitRoot(string text, ModuleSchema schema, ILogger logger, out VisualTreeNode kitRoot)
        {
            if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var kitNumber) ||
                !schema.KitRoots.TryGetValue(kitNumber, out kitRoot))
            {
                logger.Log($"Invalid kit number: {text}");
                kitRoot = null;
                return false;
            }
            return true;
        }
    }
}
