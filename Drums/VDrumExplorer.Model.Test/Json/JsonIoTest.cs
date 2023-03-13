// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.Linq;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Json;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Test.Json;

public class JsonIoTest
{
    [Test]
    public void RoundtripModule()
    {
        var module = TestData.LoadTD27();
        string json = module.ToJson();
        var newModule = (Module) JsonIo.ReadModel(json, NullLogger.Instance);
        var newJson = newModule.ToJson();
        Assert.AreEqual(json, newJson);

        AssertDataEqual(module.Data, newModule.Data);        
    }

    [Test]
    public void RoundtripKit()
    {
        var kit = TestData.LoadTD27().ExportKit(24);
        string json = kit.ToJson();
        var newKit = (Kit) JsonIo.ReadModel(json, NullLogger.Instance);
        var newJson = newKit.ToJson();
        Assert.AreEqual(json, newJson);
        AssertDataEqual(kit.Data, newKit.Data);
    }

    private static void AssertDataEqual(ModuleData expectedData, ModuleData actualData)
    {
        var originalSegments = expectedData.CreateSnapshot().Segments.ToList();
        var newSegments = actualData.CreateSnapshot().Segments.ToList();
        Assert.AreEqual(originalSegments.Count, newSegments.Count);

        for (int i = 0; i < originalSegments.Count; i++)
        {
            var originalSegment = originalSegments[i];
            var newSegment = newSegments[i];
            Assert.AreEqual(originalSegment.Address, newSegment.Address, $"Address of segment {i}");
            Assert.AreEqual(originalSegment.Size, newSegment.Size, $"Size of segment {i}");
            Assert.AreEqual(originalSegment.CopyData(), newSegment.CopyData(), $"Data in segment starting at {originalSegment.Address}");
        }
    }
}
