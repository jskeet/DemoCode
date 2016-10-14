namespace NotInPreview3
{
    public class NoPublicConstructors
    {
        private NoPublicConstructors()
        {
        }

        public static NoPublicConstructors Create() => new NoPublicConstructors();
    }

    public static class Extensions
    {
        // Making up syntax here...
        public static NoPublicConstructors NoPublicConstructors.new()
        {
            return NoPublicConstructors.Create();
        }

        // And here
        public static bool HasEvenLength(this string x) { get { return x.Length % 2 == 0; } }

        // And here!
        public static Math.SinDeg(double deg) => Math.Sin(deg * Math.Pi / 180));
    }

    class ExtensionEverything
    {
        static void Main()
        {
            var x = new NoPublicConstructors();
            bool even = "foo".HasEvenLength;
            double one = Math.SinDeg(90);
        }
    }
}
