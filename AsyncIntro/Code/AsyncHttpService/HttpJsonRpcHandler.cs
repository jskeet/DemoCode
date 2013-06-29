// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsyncHttpService
{
    /// <summary>
    /// Prototype (and never going beyond that) HTTP RPC server using JSON in the HTTP body to construct
    /// method parameters. This isn't meant to be even slightly production-ready, so please don't use it in the real world.
    /// </summary>
    internal sealed class HttpJsonRpcHandler
    {
        private readonly int port;
        private readonly string prefix;
        private readonly ConcurrentDictionary<string, object> targets = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<int, Task> outstandingRequests = new ConcurrentDictionary<int, Task>();

        internal HttpJsonRpcHandler(int port, string prefix)
        {
            this.port = port;
            this.prefix = prefix;
        }

        internal async Task Start()
        {
            string fullPrefix = "/" + prefix + "/";

            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add("http://localhost:" + port + "/" + prefix + "/");
                listener.Start();

                int requestId = 0;
                while (true)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    HttpListenerRequest request = context.Request;

                    if (!request.Url.AbsolutePath.StartsWith(fullPrefix))
                    {
                        Console.WriteLine("Unexpected request for " + request.Url);
                        await WriteErrorResponse(context, HttpStatusCode.NotFound, "Unknown url");
                        continue;
                    }

                    requestId++;

                    string relativePath = request.Url.AbsolutePath.Substring(fullPrefix.Length);
                    if (relativePath == "quit")
                    {
                        using (HttpListenerResponse response = context.Response)
                        {
                            response.StatusCode = (int) HttpStatusCode.OK;
                            response.ContentType = "text/plain";
                            await WriteResponseBody(response, "Shutting down");
                        }
                        break;
                    }

                    string[] parts = relativePath.Split('/');

                    if (parts.Length != 2)
                    {
                        Console.WriteLine("Expected target/method relative path; was " + relativePath);
                        await WriteErrorResponse(context, HttpStatusCode.BadRequest, "Bad path");
                        continue;
                    }

                    object target;
                    if (!targets.TryGetValue(parts[0], out target))
                    {
                        Console.WriteLine("Unknown target for " + request.Url);
                        await WriteErrorResponse(context, HttpStatusCode.NotFound, "Unknown target");
                        continue;
                    }

                    // TODO: Make this cleaner. Possibly use a linked list?
                    Task task = HandleRequestAsync(context, target, parts[1], requestId);
                    outstandingRequests[requestId] = task;
                    int thisRequestId = requestId;
#pragma warning disable 4014
                    task.ContinueWith(t => outstandingRequests.TryRemove(thisRequestId, out task));
#pragma warning restore
                }
                Console.WriteLine("Shutting down... waiting for {0} outstanding requests", outstandingRequests.Count);
                await Task.WhenAll(outstandingRequests.Values);
                listener.Stop();
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context, object target, string method, int requestId)
        {
            Console.WriteLine("{0:HH:mm:ss.ffff}: Handling request {1} for {2}",
                              DateTime.Now,
                              requestId,
                              context.Request.Url.AbsolutePath);
            string requestText;
            using (var reader = new StreamReader(context.Request.InputStream))
            {
                requestText = await reader.ReadToEndAsync();
            }
            Console.WriteLine(requestText);

            JObject json = JObject.Parse(requestText);

            var methodInfo = target.GetType().GetMethod(method);
            if (methodInfo == null)
            {
                await WriteErrorResponse(context, HttpStatusCode.NotFound, "Unknown method");
                return;
            }
            
            var parameters = methodInfo.GetParameters();
            var arguments = parameters.Select(parameter => ConvertToken(json[parameter.Name], parameter.ParameterType))
                                      .ToArray();

            // Use dynamic typing to convert to Task<T> for the right T...
            dynamic task= methodInfo.Invoke(target, arguments);
            string jsonResponse = await ConvertToJson(task);
            using (HttpListenerResponse response = context.Response)
            {
                response.StatusCode = (int) HttpStatusCode.OK;
                response.ContentType = "application/json";
                await WriteResponseBody(response, jsonResponse);
                Console.WriteLine("{0:HH:mm:ss.ffff}: Completed request {1} for {2}",
                                  DateTime.Now,
                                  requestId,
                                  context.Request.Url.AbsolutePath);
            }
        }

        // It's annoying that LINQ to JSON doesn't have this, but...
        private static readonly MethodInfo ToObjectMethod = typeof(JToken).GetMethod("ToObject", BindingFlags.Instance | BindingFlags.Public, null, new Type[0], new ParameterModifier[0]);
        private static object ConvertToken(JToken token, Type targetType)
        {
            return ToObjectMethod.MakeGenericMethod(targetType).Invoke(token, null);
        }

        private async Task<string> ConvertToJson<T>(Task<T> task)
        {
            T result = await task;
            return JsonConvert.SerializeObject(result);
        }

        private async Task WriteErrorResponse(HttpListenerContext context, HttpStatusCode status, string text)
        {
            using (var response = context.Response)
            {
                response.StatusCode = (int)status;
                response.ContentType = "text/plain";
                await WriteResponseBody(response, text);
            }
        }

        private async Task WriteResponseBody(HttpListenerResponse response, string text)
        {
            response.ContentEncoding = Encoding.UTF8;
            byte[] data = Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = data.Length;
            using (Stream output = response.OutputStream)
            {
                await output.WriteAsync(data, 0, data.Length);
            }
        }

        internal void AddTarget(string path, object target)
        {
            targets[path] = target;
        }
    }
}
