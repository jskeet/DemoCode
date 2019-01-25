using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MentorSearchModels
{
    [FirestoreData]
    public class Mentor
    {
        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string Type { get; set; }

        [FirestoreProperty]
        public string Region { get; set; }

        [JsonProperty("Topics")]
        public string CommaSeparatedTopics { get; set; }

        [FirestoreProperty, JsonIgnore]
        public List<string> Topics { get; set; }

        [FirestoreProperty]
        public string Location { get; set; }

        [FirestoreProperty]
        public string Twitter { get; set; }

        // Note: there are lots of alternative approaches to this munging. We could definitely have an input model and an output model, for example.
        public void Normalize()
        {
            // Split topics so it's a collection
            Topics = CommaSeparatedTopics?.Split(",").Select(t => t.Trim(',', '.', ' ').ToLowerInvariant()).ToList();
            // Just get the first part of each location
            Location = Location?.Split(',')[0].ToLowerInvariant();
        }
    }
}
