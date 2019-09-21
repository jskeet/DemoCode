// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Data.Layout;

namespace VDrumExplorer.Data.Json
{
    using static Validation;

    internal sealed class VisualTreeNodeJson
    {
        public List<VisualTreeNodeJson> Children { get; set; } = new List<VisualTreeNodeJson>();
        public List<VisualTreeDetailJson> Details { get; set; } = new List<VisualTreeDetailJson>();
        public string? Description { get; set; }
        public string? Path { get; set; }
        public string? MidiNotePath { get; set; }

        public List<string>? FormatPaths { get; set; }
        public string? Repeat { get; set; }
        public string? Format { get; set; }
        public string? KitOnlyFormat { get; set; }
        public string? Index { get; set; }
        public string? KitIndex { get; set; }

        internal IEnumerable<VisualTreeNode> ConvertVisualNodes(VisualTreeNode? parent, VisualTreeConversionContext parentContext)
        {
            int? repeat = parentContext.GetRepeat(Repeat);
            var relativePath = ValidateNotNull(Path, nameof(Path));
            if (repeat == null)
            {
                var context = parentContext.WithContextFromPath(relativePath);
                yield return ToVisualTreeNode(context);
            }
            else
            {
                var index = ValidateNotNull(Index, nameof(Index));
                for (int i = 1; i <= repeat; i++)
                {
                    var context = parentContext.WithIndex(index, i).WithContextFromPath(relativePath);
                    yield return ToVisualTreeNode(context);
                }
            }

            VisualTreeNode ToVisualTreeNode(VisualTreeConversionContext context)
            {
                FormattableDescription description = BuildDescription(context);
                FormattableDescription? kitOnlyDescription = BuildKitOnlyDescription(context);

                Func<VisualTreeNode?, IReadOnlyList<VisualTreeNode>> childrenProvider = newNode => Children.SelectMany(child => child.ConvertVisualNodes(newNode, context)).ToList().AsReadOnly();
                var details = Details.Select(detail => detail.ToVisualTreeDetail(context)).ToList().AsReadOnly();
                var midiNoteField = MidiNotePath is null ? null : context.GetMidiNoteField(MidiNotePath);
                var kitIndex = KitIndex == null ? (int?) null : context.GetIndex(KitIndex);
                return new VisualTreeNode(parent, context.ContainerContext, childrenProvider, details, description, kitOnlyDescription, midiNoteField, kitIndex);
            }

            FormattableDescription BuildDescription(VisualTreeConversionContext context)
            {
                if (Description != null)
                {
                    ValidateNull(Format, nameof(Format), nameof(Description));
                    ValidateNull(FormatPaths, nameof(FormatPaths), nameof(Description));
                    return new FormattableDescription(Description, null);
                }
                else
                {
                    var format = ValidateNotNull(Format, nameof(Format));
                    var formatPaths = ValidateNotNull(FormatPaths, nameof(FormatPaths));
                    return context.BuildDescription(format, formatPaths);
                }
            }

            FormattableDescription? BuildKitOnlyDescription(VisualTreeConversionContext context)
            {
                if (KitOnlyFormat is null)
                {
                    return null;
                }
                else
                {
                    var formatPaths = ValidateNotNull(FormatPaths, nameof(FormatPaths));
                    return context.BuildDescription(KitOnlyFormat, formatPaths);
                }
            }
        }
    }
}
