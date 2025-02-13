using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenAI;


namespace chatgptbot.Util
{
    public class ChatGptHelper
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string ChatGptApiUrl = "https://api.openai.com/v1/chat/completions";
        String _apiKey;
        public ChatGptHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _apiKey = _configuration.GetValue<String>("OPEN_AI_KEY");
            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
        }

        public  async Task<string> SendTextToChatGpt(string text)
        {

            var lastMessage = "";

            var body = new
            {
                model = "gpt-4-turbo",
                messages = new[]
                {
                new { role = "system", content = "You are an AI that processes PDF content." },
                new { role = "user", content = text }
            },
                max_tokens = 500
            };

            string jsonBody = JsonSerializer.Serialize(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ChatGptApiUrl, content);
            string responseString = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseString);
            var messages = doc.RootElement.GetProperty("data");

            if (messages.GetArrayLength() > 0)
            {
                lastMessage = messages[0].GetProperty("content")[0].GetProperty("text").GetProperty("value").GetString();
                
            }
            return lastMessage;
        }

    }
}
