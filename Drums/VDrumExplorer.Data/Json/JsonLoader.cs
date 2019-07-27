using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VDrumExplorer.Data.Json
{
    /// <summary>
    /// A class to load JSON files which can include other files.
    /// </summary>
    public sealed class JsonLoader
    {
        private const string ResourcePrefix = "$resource:";
        private readonly Func<string, Stream> resourceLoader;

        private JsonLoader(Func<string, Stream> resourceLoader) =>
            this.resourceLoader = resourceLoader;

        /// <summary>
        /// Creates a JsonLoader which loads resources from an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to load resources from.</param>
        /// <param name="resourceBase">The base of the resources names. May be empty, for resources
        /// at the "root" of the assembly. Otherwise, a dot is appended to this name before appending
        /// the individual resource names.</param>
        /// <returns>The JsonLoader loading resources from an assembly.</returns>
        public static JsonLoader FromAssemblyResources(Assembly assembly, string resourceBase)
        {
            Preconditions.CheckNotNull(assembly, nameof(assembly));
            Preconditions.CheckNotNull(resourceBase, nameof(resourceBase));
            if (resourceBase != "")
            {
                resourceBase += ".";
            }
            Preconditions.CheckNotNull(assembly, nameof(assembly));
            Preconditions.CheckNotNull(resourceBase, nameof(resourceBase));
            return new JsonLoader(resourceName =>
            {
                // Allow / and \ in resource names to work as on a file system.
                resourceName = resourceName.Replace('\\', '.').Replace('/', '.');
                string fullResourceName = resourceBase + resourceName;
                var stream = assembly.GetManifestResourceStream(fullResourceName);
                if (stream == null)
                {
                    throw new FileNotFoundException(fullResourceName);
                }
                return stream;
            });
        }

        /// <summary>
        /// Creates a JsonLoader which loads resources from the file system.
        /// </summary>
        /// <param name="path">The path to the directory from which the JSON files should be loaded.</param>
        /// <returns>A JsonLoader which loads from the given directory.</returns>
        public static JsonLoader FromDirectory(string path)
        {
            Preconditions.CheckNotNull(path, nameof(path));
            return new JsonLoader(resourceName => File.OpenRead(Path.Combine(path, resourceName)));
        }

        /// <summary>
        /// Loads the given resource name as a JObject.
        /// </summary>
        /// <param name="resourceName">The name of the resource to load.</param>
        /// <returns>The parsed JObject.</returns>
        public JObject LoadResource(string resourceName)
        {
            Preconditions.CheckNotNull(resourceName, nameof(resourceName));
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
