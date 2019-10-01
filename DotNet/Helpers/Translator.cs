using FunctionApp.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp.Helpers
{
    public static class Translator
    {
        static public async Task<string> TranslateTextRequest(string subscriptionKey, string endpoint, string route, string inputText)
        {
            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                // Set the method to Post
                request.Method = HttpMethod.Post;

                // Construct the URI and add headers
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Send the request and get response
                var response = await client.SendAsync(request).ConfigureAwait(false);

                // Read response as a string
                string result = await response.Content.ReadAsStringAsync();

                // Deserialize the result
                var deserializedResult = JsonConvert.DeserializeObject<TranslationResult[]>(result);

                // Use teh first result. Advanced logic could be applied here to determine whihc result to use
                var firstResultText = deserializedResult[0].Translations[0].Text;

                return firstResultText;
            }
        }
    }
}
