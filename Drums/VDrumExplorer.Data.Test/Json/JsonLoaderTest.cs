using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;
using VDrumExplorer.Data.Json;

namespace VDrumExplorer.Data.Test.Json
{
    public class JsonLoaderTest
    {
        [Test]
        public void Simple()
        {
            var obj = LoadResourceForTest();
            Assert.AreEqual(new JObject { ["value"] = "test" }, obj);
        }

        [Test]
        public void PropertyResourceValue()
        {
            var obj = LoadResourceForTest();
            Assert.AreEqual(new JObject { ["value"] = new JObject { ["nested"] = "nestedValue" } }, obj);
        }

        [Test]
        public void MultipleNonCyclic()
        {
            var obj = LoadResourceForTest();
            Assert.AreEqual(new JObject
            {
                ["value1"] = "MultipleNonCyclic1",
                ["value2"] = "MultipleNonCyclic1",
                ["value3"] = new JObject { ["value3a"] = "MultipleNonCyclic2b" }
            }, obj);
        }

        [Test]
        public void ArrayValues()
        {
            var obj = LoadResourceForTest();
            Assert.AreEqual(new JObject
            {
                ["array"] = new JArray
                {
                    "value1",
                    "value2",
                    "value3"
                }
            }, obj);
        }

        [Test]
        [TestCase("CyclicResources")]
        [TestCase("SelfInclusion")]
        public void InvalidJson(string name)
        {
            Assert.Throws<InvalidOperationException>(() => LoadResourceForTest(name));
        }

        private JObject LoadResourceForTest([CallerMemberName] string resource = null)
        {
            var loader = JsonLoader.FromAssemblyResources(typeof(JsonLoaderTest).Assembly, typeof(JsonLoaderTest).Namespace);
            return loader.LoadResource($"{resource}.json");
        }
    }
}
