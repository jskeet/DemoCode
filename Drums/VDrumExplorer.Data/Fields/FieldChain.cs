// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// Represents a chain of fields, creating from a slash-separated path of names, e.g.
    /// Kit[5]/KitUnits[0]/Volume. All but the final element of the path must resolve to a container;
    /// the final element must resolve to a path of the type argument for the chain.
    /// </summary>
    public sealed class FieldChain<T> where T : IField
    {
        private static readonly LinkedList<Container> EmptyContainerList = new LinkedList<Container>();

        private readonly LinkedList<Container> intermediateContainers;
        public T FinalField { get; }

        internal FieldChain(LinkedList<Container> intermediateContainers, T finalField) =>
            (this.intermediateContainers, FinalField) = (intermediateContainers, finalField);

        internal static FieldChain<T> Create(Container parent, string path)
        {
            string[] pathElements = path.Split('/');
            var containers = pathElements.Length == 1 ? EmptyContainerList : new LinkedList<Container>();
            for (int i = 0; i < pathElements.Length - 1; i++)
            {
                //IField fieldByName = parent.Fields.Single(f => f.Name == pathElements[i]);
                IField fieldByName = parent.GetField(pathElements[i]);
                Container container = (Container) fieldByName;
                containers.AddLast(container);
                parent = container;
            }

            var finalField = (T) parent.GetField(pathElements[pathElements.Length - 1]);
            return new FieldChain<T>(containers, finalField);
        }

        /// <summary>
        /// Returns a <see cref="FixedContainer"/> representing the context in which
        /// <see cref="FinalField"/> should be interpreted, given a context from the parent of this chain.
        /// </summary>
        public FixedContainer GetFinalContext(FixedContainer context)
        {
            foreach (var intermediate in intermediateContainers)
            {
                context = context.ToChildContext(intermediate);
            }
            return context;
        }

        public static FieldChain<T> EmptyChain(T field) =>
            new FieldChain<T>(EmptyContainerList, field);
    }
}
