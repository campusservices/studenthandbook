using chatgptbot.Entities;
using chatgptbot.Services.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace chatgptbot.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        String _apiKey;
        string _assistantId;
        public OpenAIService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
           _apiKey = _configuration.GetValue<String>("OPEN_AI_KEY");
            _assistantId = _configuration.GetValue<String>("Assistant_Id");
            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
        }
        public async Task StreamAssistantResponseAsync(string assistantId, string threadId, string userMessage, Func<string, Task> onMessageReceived)
        {
            await AddUserMessageAsync(threadId, userMessage);

            await RunAssistantStreamAsync(assistantId, threadId, onMessageReceived);
        }

        private async Task AddUserMessageAsync(string threadId, string userMessage)
        {
            var payload = new
            {
                role = "user",
                content = userMessage
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"https://api.openai.com/v1/threads/{threadId}/messages", content);
            response.EnsureSuccessStatusCode();
        }
        private async Task RunAssistantStreamAsync(string assistantId, string threadId, Func<string, Task> onMessageReceived)
        {
            var runPayload = new
            {
                assistant_id = assistantId == null ? _assistantId : assistantId,
                stream = true
            };
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.openai.com/v1/threads/{threadId}/runs")
            {
                Content = new StringContent(JsonSerializer.Serialize(runPayload), Encoding.UTF8, "application/json")
            };
            
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

                var jsonLine = line.Substring(5);
                if (jsonLine == "[DONE]") break;

                try
                {
                    var doc = JsonDocument.Parse(jsonLine);
                    var objType = doc.RootElement.GetProperty("object").GetString();

                    if (objType == "thread.message.delta")
                    {
                        if (doc.RootElement.TryGetProperty("delta", out var delta) &&
                            delta.TryGetProperty("content", out var contentArray))
                        {
                            foreach (var content in contentArray.EnumerateArray())
                            {
                                if (content.TryGetProperty("type", out var typeProp) &&
                                    typeProp.GetString() == "text" &&
                                    content.TryGetProperty("text", out var textProp) &&
                                    textProp.TryGetProperty("value", out var valueProp))
                                {
                                    var value = valueProp.GetString();
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        await onMessageReceived(value);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Parse error: {ex.Message}");
                }
            }
        }
    }
}
