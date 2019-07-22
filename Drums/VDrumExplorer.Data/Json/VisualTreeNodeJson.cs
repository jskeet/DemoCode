using System;
using System.Collections.Generic;
using System.Text;

namespace VDrumExplorer.Data.Json
{
    internal sealed class VisualTreeNodeJson
    {
        public List<VisualTreeNodeJson> Children { get; set; } = new List<VisualTreeNodeJson>();
        public List<VisualTreeDetailJson> Details { get; set; } = new List<VisualTreeDetailJson>();
        public string Description { get; set; }
        public string Path { get; set; }

        public List<string> FormatPaths { get; set; }
        public string Repeat { get; set; }
        public string Format { get; set; }
        public string Index { get; set; }

    }
}
