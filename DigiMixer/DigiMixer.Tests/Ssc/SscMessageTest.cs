// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Ssc;
using Newtonsoft.Json.Linq;

namespace DigiMixer.Tests.Ssc;

public class SscMessageTest
{
    [Test]
    public void ToJson_SimpleAddresses()
    {
        var message = new SscMessage("/a", "/b", "/c");
        var json = "{'a': null, 'b': null, 'c': null}";
        AssertJson(json, message.ToJson());
    }

    [Test]
    public void ToJson_NestedAddress()
    {
        var message = new SscMessage("/a/b/c");
        var json = """{"a": { "b": {"c": null } } }""";
        AssertJson(json, message.ToJson());
    }

    [Test]
    public void ToJson_OverlappingAddresses()
    {
        var message = new SscMessage("/a/b1", "/a/b2");
        var json = "{'a': { 'b1': null, 'b2': null } }";
        AssertJson(json, message.ToJson());
    }

    [Test]
    public void ToJson_Values()
    {
        var message = new SscMessage(new SscProperty("/a/b1", 1.5), new SscProperty("/a/b2", "test"));
        var json = """{"a": { "b1": 1.5, "b2": "test" } }""";
        AssertJson(json, message.ToJson());
    }

    [Test]
    public void ParseJson()
    {
        var message = SscMessage.FromJson("""{"a": { "b1": 1.5, "b2": "test" } }""");
        var expected = new[]
        {
            new SscProperty("/a/b1", 1.5),
            new SscProperty("/a/b2", "test")
        };
        Assert.That(message.Properties, Is.EqualTo(expected).AsCollection);
    }

    [Test]
    public void WithId_NullId()
    {
        var original = new SscMessage(new SscProperty("/a/b/c", 10), new SscProperty(SscAddresses.Osc.Xid, "id"));
        Assert.That(original.Id, Is.EqualTo("id"));
        var noId = original.WithId(null);
        Assert.That(noId.Id, Is.Null);
        Assert.That(noId.Properties.Count, Is.EqualTo(1));
    }

    [Test]
    public void InvalidAddresses()
    {
        var badProperty = new SscProperty("no/slash/at/start", null);
        Assert.Throws<ArgumentException>(() => new SscMessage(badProperty));

        badProperty = new SscProperty("/slash/at/end/", null);
        Assert.Throws<ArgumentException>(() => new SscMessage(badProperty));
    }

    [Test]
    public void WithId_NonNullId()
    {
        var original = new SscMessage(new SscProperty("/a/b/c", 10), new SscProperty(SscAddresses.Osc.Xid, "id1"));
        Assert.That(original.Id, Is.EqualTo("id1"));
        var withId = original.WithId("id2");
        var expected = new[]
        {
            new SscProperty("/a/b/c", 10),
            new SscProperty(SscAddresses.Osc.Xid, "id2")
        };
        Assert.That(withId.Properties, Is.EquivalentTo(expected));
    }

    [Test]
    public void ParseJson_ObservedErrors()
    {
        // Actual response from a request...
        var message = SscMessage.FromJson("""
        {
          "osc": {
            "error":[{"abc":[404,{"desc":"address not found"}], "mates":{ "tx1":{ "battery":{ "xyz":[404,{ "desc":"address not found"}],"type":[424,{ "desc":"failed dependency"}]} } } }]
           },
          "device":{ "name":"EWDXEM2"}
        }
        """);

        // The error address still shows up as a property; Errors is just a convenience.
        Assert.That(message.Properties, Has.Member(new SscProperty("/device/name", "EWDXEM2")));
        Assert.That(message.GetProperty(SscAddresses.Osc.Error), Is.Not.Null);

        var expected = new[]
        {
            new SscError("/abc", 404, "address not found"),
            new SscError("/mates/tx1/battery/xyz", 404, "address not found"),
            new SscError("/mates/tx1/battery/type", 424, "failed dependency")
        };
        Assert.That(message.Errors, Is.EqualTo(expected).AsCollection);
    }

    [Test]
    public void ParseJson_MultipleTopLevelErrors()
    {
        // We haven't seen an example with the "error" array having multiple elements, but 
        var message = SscMessage.FromJson("""
        {
          "osc": {
            "error":[{"abc":[404,{"desc":"address not found"}]}, {"x":{ "y":[404,{ "desc":"address not found"}]} }]
           }
        }
        """);

        var expected = new[]
        {
            new SscError("/abc", 404, "address not found"),
            new SscError("/x/y", 404, "address not found")
        };
        Assert.That(message.Errors, Is.EqualTo(expected).AsCollection);
    }

    [Test]
    public void ParseJson_PartialErrors()
    {
        // We've never seen this, but it could happen... assert reasonable behavior
        var message = SscMessage.FromJson("""
        {
          "osc": {
            "error":[{"e1":[404]}, {"e2":[404, {"name":"not a description"}]}, {"e3":[404, "not an object"]}]
           }
        }
        """);

        var expected = new[]
        {
            new SscError("/e1", 404, null),
            new SscError("/e2", 404, null),
            new SscError("/e3", 404, null)
        };
        Assert.That(message.Errors, Is.EqualTo(expected).AsCollection);
    }

    [Test]
    public void ParseJson_InvalidErrors()
    {
        // We've never seen this, but it could happen... these errors are so malformed
        // that we ignorem.
        var message = SscMessage.FromJson("""
        {
          "osc": {
            "error":[{"e1":["not a code"]}, {"e2":"not an array"}, {"e3":[]}]
           }
        }
        """);

        Assert.That(message.Errors, Is.Empty);
    }

    [Test]
    public void ParseJson_NonArrayError()
    {
        // We've never seen this, but it could happen... these errors are so malformed
        // that we ignorem.
        var message = SscMessage.FromJson("""
        {
          "osc": {
            "error":"not an array"
           }
        }
        """);

        Assert.That(message.Errors, Is.Empty);
    }

    private static void AssertJson(string expectedJson, string actualJson)
    {
        JObject expectedObj = JObject.Parse(expectedJson);
        JObject actualObj = JObject.Parse(actualJson);

        Assert.That(actualObj.ToString(), Is.EqualTo(expectedObj.ToString()));
    }
}