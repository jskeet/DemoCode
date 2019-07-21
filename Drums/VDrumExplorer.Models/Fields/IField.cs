using System;
using System.Collections.Generic;
using System.Text;

namespace VDrumExplorer.Models.Fields
{
    public interface IField
    {
        string Description { get; }
        string Path { get; }
        ModuleAddress Address { get; }
        int Size { get; }
    }
}
