using System;
using System.Collections.Generic;
using System.Text;

namespace VDrumExplorer.Models.Fields
{
    /// <summary>
    /// A parsed value from a field.
    /// </summary>
    public sealed class FieldValue
    {
        private readonly string text;

        public FieldValue(string text)
        {
            this.text = text;
        }

        public override string ToString() => text;
    }
}
