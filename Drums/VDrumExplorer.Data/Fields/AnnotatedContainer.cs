// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Fields
{
    public sealed class AnnotatedContainer
    {
        public string Path { get; }
        /// <summary>
        /// The underlying container.
        /// </summary>
        public Container Container => Context.Container;

        /// <summary>
        /// The context of the container, including the address.
        /// </summary>
        public FixedContainer Context { get; }

        public AnnotatedContainer(string path, FixedContainer context) =>
            (Path, Context) = (path, context);

        public AnnotatedContainer AnnotateChildContainer(Container container) =>
            new AnnotatedContainer($"{Path}/{container.Name}", Context.ToChildContext(container));

        public AnnotatedField AnnotateChildField(IField field) =>
            new AnnotatedField($"{Path}/{field.Name}", field, Context.Address + field.Offset);
    }
}
