using chatgptbot.Entities;
using chatgptbot.Services.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace chatgptbot.Services
{
    public class ChatGPTService : IChatGPTService
    {
        private readonly IConfiguration _configuration;

        public ChatGPTService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task<string> ExtractTextFromPDF(string filePath)
        {
            using (var document = PdfDocument.Open(filePath))
            {
                var text = new StringBuilder();

                foreach (var page in document.GetPages())
                {
                    text.AppendLine(page.Text);
                }

                return Task.FromResult(text.ToString());
            }

        }

        public async Task<string> SendMessage(UserMessage message)
        {

            var apiKey = _configuration.GetValue<String>("OPEN_AI_KEY"); 
            var requestBody = new
            {
                prompt = message.Text,
                max_tokens = 150
            };

            var requestContent = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri("https://api.openai.com/v1/completions"),
                Headers =
                   {
                       { "Authorization", $"Bearer {apiKey}" }
                   },
                Content = requestContent
            };
            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                var jsonResponse = JObject.Parse(responseString);
                var reply = jsonResponse["choices"][0]["text"].ToString().Trim();
                return await Task.FromResult(reply);
            }
           
    }

    
    public async Task<string> SendToChatGPT(string content)
        {
            var apiKey = _configuration.GetValue<String>("OPEN_AI_KEY");

            var apiEndpoint = "https://api.openai.com/v1/completions";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-4.0",
                    prompt = content,
                    max_tokens = 150
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiEndpoint, stringContent);
                var responseString = await response.Content.ReadAsStringAsync();

                return responseString;

            }
        }
    }
}
