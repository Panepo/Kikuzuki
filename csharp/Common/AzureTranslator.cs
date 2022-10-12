using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kikuzuki
{
    class AzureTranslator
    {
        public class TransResult
        {
            public List<TransRes> Translations { get; set; }
        }

        public class TransRes
        {
            public string Text { get; set; }
            public string To { get; set; }
        }

        public static async Task<string> Translate(string input)
        {
            string route = "/translate?api-version=3.0&from=en&to=zh-tw";
            object[] body = new object[] { new { Text = input } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                try
                {
                    // Build the request.
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(TranslatorConfig.TranslatorEndpoint + route);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", TranslatorConfig.TranslatorKey);
                    // location required if you're using a multi-service or regional (not global) resource.
                    request.Headers.Add("Ocp-Apim-Subscription-Region", TranslatorConfig.TranslatorRegion);

                    // Send the request and get response.
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    // Read response as a string.
                    string responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<List<TransResult>>(responseString);

                    return result[0].Translations[0].Text;
                }
                catch (HttpRequestException e)
                {
                    throw new HttpRequestException(@"HTTP request exception.", e);
                }
            }
        }
    }
}