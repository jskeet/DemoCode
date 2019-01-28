﻿using Google.Cloud.Dialogflow.V2;
using Google.Cloud.Firestore;
using Google.Protobuf;
using MentorSearchModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Google.Protobuf.WellKnownTypes.Value;

namespace DialogflowDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MentorSearchController : ControllerBase
    {
        // A Protobuf JSON parser configured to ignore unknown fields. This makes
        // the action robust against new fields being introduced by Dialogflow.
        private static readonly JsonParser jsonParser =
            new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        private readonly FirestoreDb firestore;

        public MentorSearchController(FirestoreDb firestore)
        {
            this.firestore = firestore;
        }

        [HttpGet, HttpPost]
        public async Task<ContentResult> FulfilRequest([FromBody] JObject rawRequest)
        {
            var queryResult = (JObject) rawRequest["queryResult"];
            queryResult.Remove("fulfillmentMessages");

            if (queryResult.TryGetValue("sentimentAnalysisResult", out var sentimentToken) && sentimentToken is JObject sentimentObject &&
                sentimentObject.TryGetValue("queryTextSentiment", out var querySentimentToken) && querySentimentToken is JObject querySentimentObject &&
                querySentimentObject.TryGetValue("score", out var scoreToken) &&
                querySentimentObject.TryGetValue("magnitude", out var magnitudeToken))
            {
                double score = (double) scoreToken;
                double magnitude = (double) magnitudeToken;
                if (score < 0 && magnitude > 0.5)
                {
                    var transfer = CreateResponse("Transferring you to a friendly human");
                    transfer.FollowupEventInput = new EventInput { Name = "telephone-event" };
                    return Content(transfer.ToString(), "application/json");
                }
            }

            // Parse the body of the request using the Protobuf JSON parser,
            // *not* Json.NET.
            WebhookRequest request;
            using (var reader = new StringReader(rawRequest.ToString()))
            {
                request = jsonParser.Parse<WebhookRequest>(reader);
            }

            // TODO: Authenticate the request


            WebhookResponse response = await GetResponseAsync(request);
            // Ask Protobuf to format the JSON to return.
            // Again, we don't want to use Json.NET - it doesn't know how to handle Struct
            // values etc.
            string responseJson = response.ToString();
            return Content(responseJson, "application/json");
        }

        private async Task<WebhookResponse> GetResponseAsync(WebhookRequest request)
        {
            
            switch (request.QueryResult.Intent.DisplayName)
            {
                case "Default Welcome Intent":
                    return GetWelcomeResponse(request);
                case "Default Fallback Intent":
                    return GetFallbackResponse(request);
                case "Mentor Search":
                    return await GetMentorSearchResponse(request);
                case "Transfer Call Escalation to Human":
                    return GetTransferCallResponse(request);
                case "Goodbye":
                    return GetGoodbyeResponse(request);
                default:
                    return new WebhookResponse { FulfillmentText = "Sorry, I didn't understand that intent." };
            }
        }

        private static WebhookResponse GetWelcomeResponse(WebhookRequest request) =>
            CreateResponse("Greetings! What mentor are you looking for? You can search by name, city or topic.");

        private static WebhookResponse GetFallbackResponse(WebhookRequest request) =>
            CreateResponse("I didn't understand. I'm sorry, can you try again?");

        private async Task<WebhookResponse> GetMentorSearchResponse(WebhookRequest request)
        {
            var name = GetStringParameter(request, "given-name");
            var topic = GetStringParameter(request, "topic");
            var city = GetStringParameter(request, "geo-city");

            Console.WriteLine($"Search: name={name}; topic={topic}; city={city}");

            if (name == null && topic == null && city == null)
            {
                return CreateResponse("I'm sorry, I didn't understand your request. Please specify a name, topic or city.");
            }

            Query query = firestore.Collection("mentors");
            if (name != null)
            {
                query = query.WhereGreaterThanOrEqualTo("Name", name).WhereLessThan("Name", name + "~");
            }
            if (topic != null)
            {
                query = query.WhereArrayContains("Topics", topic.ToLowerInvariant());
            }
            if (city != null)
            {
                query = query.WhereEqualTo("Location", city.ToLowerInvariant());
            }

            var results = await query.GetSnapshotAsync().ConfigureAwait(false);
            // Ordering here rather than in the query reduces the number of indexes we need.
            var mentors = results.Select(doc => doc.ConvertTo<Mentor>()).OrderBy(m => m.Name).ToList();

            string mentorNames = string.Join(", ", mentors.Select(m => m.Name));
            return CreateResponse(
                $"I found {mentors.Count} {(mentors.Count == 1 ? "mentor" : "mentors")}: {mentorNames}");
        }

        private static WebhookResponse GetTransferCallResponse(WebhookRequest request) =>
            CreateResponse("Transfering you now!");

        private static WebhookResponse GetGoodbyeResponse(WebhookRequest request) =>
            CreateResponse("Goodbye");

        private static WebhookResponse CreateResponse(string message) =>
            new WebhookResponse { FulfillmentText = message, };

        private static string GetStringParameter(WebhookRequest request, string parameterName) =>
            request.QueryResult.Parameters.Fields.TryGetValue(parameterName, out var value) &&
            value.KindCase == KindOneofCase.StringValue && value.StringValue != ""
            ? value.StringValue : null;
    }
}
