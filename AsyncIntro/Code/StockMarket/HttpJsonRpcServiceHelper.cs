// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StockMarket
{
    internal static class HttpJsonRpcServiceHelper
    {
        internal static async Task<T> ExecuteRequestAsync<T>(string url, object request)
        {
            string body = JsonConvert.SerializeObject(request);
            WebClient client = new WebClient();
            byte[] response = await client.UploadDataTaskAsync(url, Encoding.UTF8.GetBytes(body));
            string responseJson = Encoding.UTF8.GetString(response);
            return JsonConvert.DeserializeObject<T>(responseJson);
        }
    }
}
