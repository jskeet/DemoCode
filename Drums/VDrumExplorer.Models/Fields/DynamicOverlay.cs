﻿using System.Collections.Generic;

namespace VDrumExplorer.Models.Fields
{
    public sealed class DynamicOverlay : FieldBase, IContainerField
    {
        private readonly int switchAddress;
        private readonly string switchTransform;
        private readonly IReadOnlyList<Container> containers;

        public DynamicOverlay(string description, string path, int address, int size,
            int switchAddress, string switchTransform, IReadOnlyList<Container> containers)
            : base(description, path, address, size) =>
            (this.switchAddress, this.switchTransform, this.containers) = (switchAddress, switchTransform, containers);

        public IEnumerable<FieldBase> GetFields(ModuleData data)
        {
            throw new System.NotImplementedException();
        }
    }
}
