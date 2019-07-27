using System;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// Path to a field in the module data.
    /// </summary>
    public sealed class FieldPath : IEquatable<FieldPath?>
    {
        private readonly string text;

        public FieldPath(string text)
        {
            this.text = text;
        }

        public override string ToString() => text;

        public static FieldPath operator +(FieldPath parent, string field) => new FieldPath(CombinePaths(parent.text, field));

        public FieldPath WithIndex(int index) => new FieldPath($"{text}[{index}]");

        private static string CombinePaths(string currentPath, string suffix)
        {
            if (suffix == ".")
            {
                return currentPath;
            }
            if (currentPath == "")
            {
                return suffix;
            }
            return $"{currentPath}/{suffix}";
        }

        internal static FieldPath Root() => new FieldPath("");

        public override bool Equals(object? obj) => Equals(obj as FieldPath);
        public bool Equals(FieldPath? other) => other != null && other.text == text;

        public override int GetHashCode() => text.GetHashCode();
    }
}
