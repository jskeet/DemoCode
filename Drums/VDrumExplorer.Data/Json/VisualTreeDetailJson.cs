// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Layout;

namespace VDrumExplorer.Data.Json
{
    using static Validation;

    internal sealed class VisualTreeDetailJson
    {
        public string? Description { get; set; }
        public string? Path { get; set; }
        
        public List<string>? FormatPaths { get; set; }
        public string? Repeat { get; set; }
        public string? Format { get; set; }
        public string? Index { get; set; }

        internal VisualTreeDetail ToVisualTreeDetail(VisualTreeConversionContext context)
        {
            var description = ValidateNotNull(Description, nameof(Description));
            
            int? repeat = context.GetRepeat(Repeat);
            if (repeat == null)
            {
                string relativePath = ValidateNotNull(Path, nameof(Path));
                ValidateNull(Format, nameof(Format), nameof(Repeat));
                ValidateNull(FormatPaths, nameof(FormatPaths), nameof(Repeat));

                return new VisualTreeDetail(description, context.GetContext(relativePath));
            }
            else
            {                
                var format = ValidateNotNull(Format, nameof(Format));
                var formatPaths = ValidateNotNull(FormatPaths, nameof(FormatPaths));
                var index = ValidateNotNull(Index, nameof(Index));
                ValidateNull(Path, nameof(Path), nameof(Repeat));
                
                List<FormattableDescription> detailDescriptions = new List<FormattableDescription>();
                for (int i = 1; i <= repeat; i++)
                {
                    var formatElement = context.WithIndex(index, i).BuildDescription(format, formatPaths);
                    detailDescriptions.Add(formatElement);
                }
                return new VisualTreeDetail(description, detailDescriptions);
            }
        }

    }
}
