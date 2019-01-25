using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using MentorSearchModels;

namespace SearchTest
{
    /// <summary>
    /// Just a simple program to allow us to test searching quickly without deploying to Dialogflow etc.
    /// This smells of code duplication, of course - but for prototyping, it's a quick and easy approach.
    /// If this were more than a prototype, I'd write a separate local web application allowing a Dialogflow
    /// request to be crafted then sent to the webhook.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Please specify project ID, name, city and topic");
                return;
            }

            FirestoreDb db = FirestoreDb.Create(args[0]);

            string name = EmptyToNull(args[1]);
            string city = EmptyToNull(args[2]);
            string topic = EmptyToNull(args[3]);

            Query query = db.Collection("mentors");
            if (name != null)
            {
                query = query.WhereGreaterThanOrEqualTo("Name", name).WhereLessThan("Name", name + "~");
            }
            if (city != null)
            {
                query = query.WhereEqualTo("Location", city.ToLowerInvariant());
            }
            if (topic != null)
            {
                query = query.WhereArrayContains("Topics", topic.ToLowerInvariant());
            }
            var results = await query.GetSnapshotAsync().ConfigureAwait(false);
            var mentors = results.Select(doc => doc.ConvertTo<Mentor>()).OrderBy(m => m.Name).ToList();

            Console.WriteLine($"{mentors.Count} results");
            foreach (var mentor in mentors)
            {
                Console.WriteLine(mentor.Name);
            }
        }

        // It can be annoying to try to get empty command line arguments; treat "-" as a missing parameter.
        static string EmptyToNull(string input) => input == "-" || input == "" ? null : input;
    }
}
