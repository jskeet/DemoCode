using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VDrumExplorer.Models.Json
{
    public sealed class JsonLoader
    {
        private const string ResourcePrefix = "$resource:";
        private readonly Func<string, Stream> resourceLoader;

        private JsonLoader(Func<string, Stream> resourceLoader) =>
            this.resourceLoader = resourceLoader;

        public static JsonLoader FromAssemblyResources(Assembly assembly, string resourceBase) =>
            new JsonLoader(resourceName =>
            {
                // Allow / and \ in resource names to work as on a file system.
                resourceName = resourceName.Replace('\\', '.').Replace('/', '.');
                string fullResourceName = $"{resourceBase}.{resourceName}";
                var stream = assembly.GetManifestResourceStream(fullResourceName);
                if (stream == null)
                {
                    throw new FileNotFoundException(fullResourceName);
                }
                return stream;
            });

        public static JsonLoader FromDirectory(string path) =>
            new JsonLoader(resourceName => File.OpenRead(Path.Combine(path, resourceName)));

        public JObject LoadResource(string resourceName)
        {
            JToken token = LoadResourceToken(resourceName, new Stack<string>());
            if (token.Type != JTokenType.Object)
            {
                throw new InvalidOperationException($"Resource {resourceName} is not an object");
            }
            return (JObject) token;
        }

        private JToken LoadResourceToken(string resourceName, Stack<string> loadedResources)
        {
            if (loadedResources.Contains(resourceName))
            {
                throw new InvalidOperationException($"Resource {resourceName} is included recursively");
            }
            loadedResources.Push(resourceName);
            string text = LoadResourceText(resourceName);
            JToken parsed = JToken.Parse(text);
            var result = Visit(parsed);
            loadedResources.Pop();
            return result;

            JToken Visit(JToken token)
            {
                switch (token)
                {
                    case JProperty property:
                        property.Value = Visit(property.Value);
                        return property;
                    case JArray array:
                        // TODO: Should we have a way of "including one array within another"?
                        for (int i = 0; i < array.Count; i++)
                        {
                            array[i] = Visit(array[i]);
                        }
                        return array;
                    case JValue value when value.Type == JTokenType.String:
                        string valueText = (string) value.Value;
                        if (!valueText.StartsWith(ResourcePrefix))
                        {
                            return value;
                        }
                        string newResourceName = valueText.Substring(ResourcePrefix.Length).Trim();
                        return LoadResourceToken(newResourceName, loadedResources);
                    case JObject obj:
                        foreach (var property in obj.Properties())
                        {
                            Visit(property);
                        }
                        return obj;
                    default:
                        return token;
                }
            }
        }

        private string LoadResourceText(string resourceName)
        {
            using (var stream = resourceLoader(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
