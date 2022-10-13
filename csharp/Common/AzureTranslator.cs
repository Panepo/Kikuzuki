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
        public class LangData
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }

        public static List<LangData> LangDatas = new List<LangData>()
        {
            new LangData
            {
                Name = "English",
                Code = "en"
            },
            new LangData
            {
                Name = "Chinese Traditional",
                Code = "zh-Hant"
            },
            new LangData
            {
                Name = "French",
                Code = "fr"
            },
            new LangData
            {
                Name = "German",
                Code = "de"
            },
        };

        private static string CheckLang(string name)
        {
            foreach (LangData lang in LangDatas)
            {
                if (lang.Name == name)
                {
                    return lang.Code;
                }
            }

            return "en";
        }
        
        public class TransResult
        {
            public List<TransRes> Translations { get; set; }
        }

        public class TransRes
        {
            public string Text { get; set; }
            public string To { get; set; }
        }

        public static async Task<string> Translate(string input, string from = "English", string to = "Chinese Traditional")
        {
            string Tfrom = CheckLang(from);
            string Tto = CheckLang(to);

            string route = "/translate?api-version=3.0&from=" + Tfrom + "&to=" + Tto;
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