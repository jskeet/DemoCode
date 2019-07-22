using System.Collections.Generic;

namespace VDrumExplorer.Data.Json
{
    internal sealed class VisualTreeDetailJson
    {
        public string Description { get; set; }
        public string Path { get; set; }
        
        public List<string> FormatPaths { get; set; }
        public string Repeat { get; set; }
        public string Format { get; set; }
        public string Index { get; set; }
    }
}
