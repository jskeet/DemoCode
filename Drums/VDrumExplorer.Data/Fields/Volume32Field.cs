﻿using System;
using static System.FormattableString;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// A volume value between -60DB and +6DB, or "-inf".
    /// </summary>
    public class Volume32Field : FieldBase, IPrimitiveField
    {
        public Volume32Field(FieldPath path, ModuleAddress address, string description)
            : base(path, address, 4, description)
        {
        }

        public string GetText(ModuleData data)
        {
            int rawValue = GetRawValue(data);
            string text = rawValue == -601 ? "-INF"
                : rawValue >= -600 && rawValue <= 60 ? Invariant($"{rawValue / 10m}dB")
                : throw new InvalidOperationException($"Invalid volume value: {rawValue}");
            return text;
        }
    }
}
