// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Layout;

namespace VDrumExplorer.Wpf
{
    internal static class TextConversions
    {
        /// <summary>
        /// Used to check that integer inputs (e.g. for the kit number) comprise entirely ASCII digits.
        /// </summary>
        internal static void CheckDigits(object sender, TextCompositionEventArgs e) =>
            e.Handled = !e.Text.All(c => c >= '0' && c <= '9');

        internal static bool TryGetKitRoot(string text, ModuleSchema schema, ILogger logger, out VisualTreeNode kitRoot)
        {
            if (!TryParseInt32(text, out var kitNumber) ||
                !schema.KitRoots.TryGetValue(kitNumber, out kitRoot))
            {
                logger?.LogError($"Invalid kit number: {text}");
                kitRoot = null;
                return false;
            }
            return true;
        }

        internal static bool TryParseInt32(string text, out int result) =>
            int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out result);

        internal static bool TryParseDecimal(string text, out decimal result) =>
            decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

        internal static string Format(int value) =>
            value.ToString(CultureInfo.InvariantCulture);

        internal static string Format(decimal value) =>
            value.ToString(CultureInfo.InvariantCulture);

        internal static string Format(double value) =>
            value.ToString(CultureInfo.InvariantCulture);
    }
}
