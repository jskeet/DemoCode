using Google.Cloud.Firestore;
using MentorSearchModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PopulateFirestore
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Please specify input file, project ID and collection name");
                return;
            }

            string json = File.ReadAllText(args[0]);
            FileRoot root = JsonConvert.DeserializeObject<FileRoot>(json);
            foreach (var mentor in root.Mentors)
            {
                mentor.Normalize();
            }

            FirestoreDb db = FirestoreDb.Create(args[1]);
            var collection = db.Collection(args[2]);

            await DeleteExistingDocuments(collection);
            await CreateDocuments(collection, root.Mentors);            
        }

        static async Task DeleteExistingDocuments(CollectionReference collection)
        {
            // If the collection existed before, let's delete everything in it.
            var docs = await collection.ListDocumentsAsync().ToList();
            if (docs.Count == 0)
            {
                return;
            }

            var batch = collection.Database.StartBatch();
            foreach (var doc in docs)
            {
                batch.Delete(doc);
            }            
            var results = await batch.CommitAsync();
            Console.WriteLine($"Deleted {results.Count} documents");
        }

        static async Task CreateDocuments(CollectionReference collection, List<Mentor> mentors)
        {
            var batch = collection.Database.StartBatch();
            foreach (var mentor in mentors)
            {
                batch.Create(collection.Document(), mentor);
            }
            var results = await batch.CommitAsync();
            Console.WriteLine($"Created {results.Count} documents");
        }
    }

    class FileRoot
    {
        [JsonProperty("techWomenCollection")]
        public List<Mentor> Mentors { get; set; }
    }
}
