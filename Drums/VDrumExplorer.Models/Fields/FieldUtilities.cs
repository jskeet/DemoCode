using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;

namespace VDrumExplorer.Models.Fields
{
    internal static class FieldUtilities
    {
        internal static int ParseHex(string text)
        {
            if (text == null)
            {
                throw new InvalidOperationException("No value specified");
            }
            if (!text.StartsWith("0x"))
            {
                throw new InvalidOperationException($"Expected hex value; was {text}");
            }
            return int.Parse(text.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        }

        internal static JObject LoadJson(string resource)
        {
            var type = typeof(FieldUtilities);
            using (var stream = type.Assembly.GetManifestResourceStream(resource))
            {
                using (var reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    return JObject.Parse(json);
                }
            }
        }
    }
}
