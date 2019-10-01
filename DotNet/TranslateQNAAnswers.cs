using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FunctionApp.Models;
using System.Collections.Generic;
using FunctionApp.Helpers;

namespace FunctionApp
{
    public static class TranslateQNAAnswers
    {
        [FunctionName("TranslateQNAAnswers")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("TranslateQNAAnswers function processed a request.");

            //Get translateToLanguageCode from query or return BadRequest if not present
            var translateToLanguageCode = req.Query["translateToLanguageCode"];
            if (string.IsNullOrEmpty(translateToLanguageCode)) return new BadRequestObjectResult("Please pass a translateToLanguageCode on the query string.");

            //Get POST'd json payload or return BadRequest if not present
            string requestBodyJson = new StreamReader(req.Body).ReadToEnd();
            if (string.IsNullOrEmpty(requestBodyJson)) return new BadRequestObjectResult("Please pass the QnA response json to be translated in the request body as Application/Json.");

            //Deserialise the QnA response json to a QnaResponse object (object class generated via http://json2csharp.com)
            var qnaResponse = JsonConvert.DeserializeObject<QnaResponse>(requestBodyJson);

            //Get keys from environment variables. For local, use a file called local.settings.json based on local.settings.sample.json. For deployed code in Azure, use the Function Application settings, again based on local.settings.sample.json
            var translatorKey = Environment.GetEnvironmentVariable("TRANSLATOR_TEXT_SUBSCRIPTION_KEY");
            var translatorEndpoint = Environment.GetEnvironmentVariable("TRANSLATOR_TEXT_ENDPOINT");

            //Translate each answer using the Translator text API. Add the translated answer to a new translatedAnswers collection
            var translatedAnswers = new List<Answer>();
            foreach (var originalAnswer in qnaResponse.answers)
            {
                //Initially duplicate the entire answer object so that other properties are retained (questions, metadata, score etc)
                var translatedAnswer = originalAnswer;

                //Translate the answer to the translateToLanguageCode language and overwrite the answer property on translatedAnswer
                string route = $"/translate?api-version=3.0&to={translateToLanguageCode}";
                translatedAnswer.answer = await Translator.TranslateTextRequest(translatorKey, translatorEndpoint, route, originalAnswer.answer);

                //Add to translatedAnswers collection
                translatedAnswers.Add(translatedAnswer);
            }

            //Replace initial answers collection with translatedAnswers
            qnaResponse.answers = translatedAnswers;

            //Respond with 200/OK and translatedAnswers
            return new OkObjectResult(qnaResponse);

        }
    }
}
