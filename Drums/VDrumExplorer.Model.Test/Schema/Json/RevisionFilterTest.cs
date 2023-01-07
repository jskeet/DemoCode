// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDrumExplorer.Model.Schema.Json;

namespace VDrumExplorer.Model.Test.Schema.Json;

public class RevisionFilterTest
{
    private static readonly JArray allTests = JArray.Parse(LoadTestJson());

    private static IEnumerable<string> AllTestNames => allTests.Cast<JObject>().Select(obj => (string) obj["name"]);

    [TestCaseSource(nameof(AllTestNames))]
    public void TestVisit(string testName)
    {
        var test = allTests.Cast<JObject>().Single(obj => (string) obj["name"] == testName);
        var source = (JObject) test["source"];
        var expectedResults = (JObject) test["expectedResults"];
        foreach (var prop in expectedResults.Properties())
        {
            int revision = HexInt32.Parse(prop.Name).Value;
            JObject clone = (JObject) source.DeepClone();
            RevisionFilter.VisitObject(clone, revision);
            string actualText = clone.ToString();
            string expectedText = prop.Value.ToString();
            Assert.AreEqual(expectedText, actualText, $"Revision {revision}");
        }
    }

    private static string LoadTestJson()
    {
        using var stream = typeof(RevisionFilterTest).Assembly.GetManifestResourceStream(typeof(RevisionFilterTest).Namespace + ".RevisionFilterTests.json");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
