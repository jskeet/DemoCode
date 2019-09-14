// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDrumExplorer.Data.Proto;

namespace VDrumExplorer.Data.Test
{
    public class ModuleSchemaTest
    {
        private static IEnumerable<string> SchemaNames =>
            SchemaRegistry.KnownSchemas.Keys.Select(id => id.Name);

        [Test]
        [TestCaseSource(nameof(SchemaNames))]
        public void AllLoadableContainersHaveUniqueAddresses(string name)
        {
            var schema = SchemaRegistry.KnownSchemas.Single(pair => pair.Key.Name == name).Value.Value;
            var sharedContainers = schema.Root.AnnotateDescendantsAndSelf()
                .Where(c => c.Container.Loadable)
                .GroupBy(c => c.Context.Address)
                .Where(g => g.Count() > 1)
                .Select(g => $"Containers with address {g.Key}: {string.Join(", ", g.Select(c => c.Path))}")
                .ToList();
            Assert.IsEmpty(sharedContainers);
        }

        [Test]
        [TestCaseSource(nameof(SchemaNames))]
        public void SampleDataIsValid(string name)
        {
            var module = TryLoadModule(name);
            Assert.NotNull(module);
            module.Validate();
        }

        [Test]
        [TestCaseSource(nameof(SchemaNames))]
        public void SampleDataMatchesLoadableContainers(string name)
        {
            var module = TryLoadModule(name);
            var loadedSegmentStarts = module.Data.GetSegments()
                .Select(s => s.Start)
                .ToList();
            var loadableSegmentStarts = module.Schema.Root.AnnotateDescendantsAndSelf()
                .Where(c => c.Container.Loadable)
                .Select(c => c.Context.Address)
                .OrderBy(address => address)
                .ToList();
            Assert.AreEqual(loadedSegmentStarts, loadableSegmentStarts);
        }

        private static Module TryLoadModule(string name)
        {
            var ns = typeof(ModuleSchemaTest).Namespace;
            string filename = name.ToLowerInvariant().Replace("-", "") + ".vdrum";
            string resourceName = $"{ns}.{filename}";
            using (var stream = typeof(ModuleSchemaTest).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }
                return (Module) ProtoIo.ReadStream(stream);
            }
        }
    }
}
