using chatgptbot.Entities;
using chatgptbot.Services.Interface;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using OpenAI;
using OpenAI.Assistants;
using System.IO;
using Microsoft.AspNetCore.Http;
using chatgptbot.Exceptions;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using chatgptbot.dto;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace chatgptbot.Services
{
    public class ChatGPTService : IChatGPTService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        String _apiKey;
        public ChatGPTService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _apiKey = _configuration.GetValue<String>("OPEN_AI_KEY");
            _httpClient.DefaultRequestHeaders.Clear();
           
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
        }
        //create assistant to upload pdf document

        ///.1
        public async Task<string> CreateThreadAsync()
        {
            string url = "https://api.openai.com/v1/threads";
            
            var content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            string responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine("CreateThreadAsync Response: " + responseString); // Debugging

            using JsonDocument doc = JsonDocument.Parse(responseString);
            string threadId = doc.RootElement.GetProperty("id").GetString();

            Console.WriteLine("Thread ID Created: " + threadId);
            return threadId;

        }
        public async Task<string> CreateAssistantForFileAsync()
        {
            var payload = new
            {
                name = "File Processing Assistant",
                instructions = "You can analyze and retrieve information from uploaded files.",
                model = "gpt-4-turbo",
                tools = new object[]
                {
                    new { type = "file_search" } // ✅ Correct format (no extra properties)
                }
            };

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/assistants",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            );

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(responseJson).RootElement.GetProperty("id").GetString();
        }
        //3.
        public async Task<string> RunAssistantAsync(string threadId, string assistantId)
        {

            string url = $"https://api.openai.com/v1/threads/{threadId}/runs";

            var requestBody = new
            {
                assistant_id = assistantId,
                model = "gpt-4-turbo", // ✅ Force gpt-4-turbo here
            };

   
            string jsonBody = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            string responseString = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseString);
            string runId = doc.RootElement.GetProperty("id").GetString();
            return runId;
        }
        //4.
        public async Task<bool> IsRunCompletedAsync(string threadId, string runId)
        {
           
            while (true)
            {
                var response = await _httpClient.GetAsync(
                    $"https://api.openai.com/v1/threads/{threadId}/runs/{runId}");

                

                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(responseContent);
                

                //dynamic jsonResponse = JsonSerializer.Deserialize<dynamic>(responseContent);
                string status = doc.RootElement.GetProperty("status").ToString();

                Console.WriteLine($"Run Status: {status}");

                if (status == "completed") return true;
                if (status == "failed" || status == "cancelled") return false;

                await Task.Delay(2000); // Wait 2 seconds before checking again
            }
        }

        public async Task<String> GetThreadMessagesAsync(string threadId)
        {
            var lastMessage = "";
            var response = await _httpClient.GetAsync($"https://api.openai.com/v1/threads/{threadId}/messages");
            var responseContent = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseContent);
            var messages = doc.RootElement.GetProperty("data");

            if (messages.GetArrayLength() > 0)
            {
                lastMessage = messages[0].GetProperty("content")[0].GetProperty("text").GetProperty("value").GetString();
                Console.WriteLine($"Thread Messages: {responseContent}");
                return lastMessage;
            }
            return CleanResponse(lastMessage);
        }

        private string CleanResponse(string input)
        {
            return Regex.Replace(input, @"【.*?†source】", "");
        }
        /// <summary>
        /// Add a message to an existing thread
        /// </summary>
        /// 2.
        public async Task AddMessageToThreadAsync(string threadId, string userMessage)
        {
            string url = $"https://api.openai.com/v1/threads/{threadId}/messages";
            
            var messageContent = new
            {
                role = "user",
                content = "Can you summarize the content of the file I uploaded?"
            };

            // Build the request body, including the file_id in the 'file_ids' parameter
            var requestBody = new
            {
                role = "user",
                content = "Can you summarize the content of the file I uploaded?",
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();

        }
        /// <summary>
        /// Run the assistant on the thread
        /// </summary>
        ///.3
       
        public async Task<string> GetLatestMessageFromThread(string threadId)
        {
            string url = $"https://api.openai.com/v1/threads/{threadId}/messages";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            string responseString = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseString);
            var messages = doc.RootElement.GetProperty("data");

            if (messages.GetArrayLength() > 0)
            {
                var lastMessage = messages[0].GetProperty("content")[0].GetProperty("text").GetProperty("value").GetString();
                return lastMessage;
            }

            return "No response received.";
        }

       
        private async Task<string> SendRequestAsync(string url, string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }
        
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            using var fileStream = new FileStream(filePath, FileMode.Open);
            using var content = new MultipartFormDataContent
            {
                { new StreamContent(fileStream), "file", Path.GetFileName(filePath) },
                { new StringContent("assistants"), "purpose" } // Required for Assistants API
            };

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/files", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(responseJson).RootElement.GetProperty("id").GetString();
        }

        public async Task<string> CreateThreadWithFileAsync(string fileId)
        {
            var threadResponse = await _httpClient.PostAsync(
                "https://api.openai.com/v1/threads",
                new StringContent("{}", Encoding.UTF8, "application/json")
            );
            var threadJson = await threadResponse.Content.ReadAsStringAsync();
            var threadId = JsonDocument.Parse(threadJson).RootElement.GetProperty("id").GetString();

            var messagePayload = new
            {
                /*
                role = "user",
                content = userMessage,
                file_ids = new[] { fileId }*/
                messages = new[]
                {
                  new { role = "user", content = "Analyze this document.", file_ids = new[] { fileId } }
                }
            };

            await _httpClient.PostAsync(
                $"https://api.openai.com/v1/threads/{threadId}/messages",
                new StringContent(JsonSerializer.Serialize(messagePayload), Encoding.UTF8, "application/json")
            );

            return threadId;
        }

        public async Task<string> GetAssistantIdAsync()
        {
            var url = "https://api.openai.com/v1/assistants";
            var response = await _httpClient.GetAsync(url);
            return GetFirstAssistantId(await response.Content.ReadAsStringAsync());
        }

        private String GetFirstAssistantId(String responseString)
        {
            

            var jsonDoc = JsonDocument.Parse(responseString);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("data", out var assistants) && assistants.GetArrayLength() > 0)
            {
                var firstAssistant = assistants[0];
                if (firstAssistant.TryGetProperty("id", out var assistantId))
                {
                    return assistantId.GetString();
                }
            }

            return "No assistant found.";
        }

        public async Task<List<FilePropertiesDto>> ListFilesAsync()
        {
            List<FilePropertiesDto> list = new List<FilePropertiesDto>();
            string apiUrl = "https://api.openai.com/v1/files";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Set up the authorization header
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                    // Send GET request to list files
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Parse the JSON string into a JsonDocument (or JsonObject)
                        JsonDocument doc = JsonDocument.Parse(responseBody);

                        // Accessing values from the JsonDocument using EnumerateArray
                        foreach (JsonElement file in doc.RootElement.GetProperty("data").EnumerateArray())
                        {
                            FilePropertiesDto dto = JsonSerializer.Deserialize<FilePropertiesDto>(file);
                            list.Add(dto);

                            Console.WriteLine("Files List: " + responseBody);


                        }
                        
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.ReasonPhrase);
                    }

                    return list;

                }
            }
            catch (Exception e)
            {
                throw new FileException(String.Format("File list unsuccessfully returned {0}", e.Message));
            }

        }

        public async Task UpdateAssistantAsync(string assistantId)
        {
            string[] file_Ids = new[] { "file-12dPYnrrfBmqVHqDZmAgbz",
                                     "file-PAD6Vys3KTtzyKcRdTnFEa",
                                       "file-L4g6MyjV3LcDdua3u6LrMq",
                                        "file-WjPj9n4YKgJqrcFcY1vt8d",
                                         "file-XHw92mzCtqeutiHBSBtXQb",
                                          "file-XXn67gBgKNqXt5Rnj9nL6o",
                                           "file-HHFiXgcDwPyAKoZNkyCyMs",
                                             "file-AqrKUETA74AMaHyU6aZ4MG",
                                              "file-5RFCZuzKuLTGFnFdz4eZTq",
                                               "file-GMUSVGdFe2dEpWcocE3zjT",
                                                "file-6oktFJMydk2djDXFN4BLhe"};

            //new[] { fileId }
            var requestBody = JsonSerializer.Serialize(new
            {
                file_ids = file_Ids  // Attach the uploaded file
            });

            var response = await _httpClient.PostAsync(
                $"https://api.openai.com/v1/assistants/{assistantId}",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();
           
            Console.WriteLine($"Updated Assistant: {responseContent}");
        }

        private async Task ListAssistantFilesAsync(string assistantId)
        {
           
            var response = await _httpClient.GetAsync(
                $"https://api.openai.com/v1/assistants/{assistantId}/files");

            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Assistant Files: {responseContent}");
        }
        public async Task CheckAssistantFilesAsync(string assistantId)
        {
            
            var response = await _httpClient.GetAsync($"https://api.openai.com/v1/assistants/{assistantId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Assistant Details: {responseContent}");
        }


        public async Task AddMessageToAssistantAsync(String assistantId)
        {
            // Create the message content (querying the assistant based on the file)
            var messageContent = new
            {
                role = "user",
                content = "Can you summarize the content of the file I uploaded?"
            };

            // Build the request body, including the file_id in the 'file_ids' parameter
            var requestBody = new
            {
                messages = new[] { messageContent },
                file_ids = new[] { "file-12dPYnrrfBmqVHqDZmAgbz" }  // Specify the uploaded file ID here
            };

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"https://api.openai.com/v1/assistants/{assistantId}/messages", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {responseContent}");
            
        }

        public async Task<string> CreateAssistantAsync(string[] fileIds)
        {
            
            var requestBody = JsonSerializer.Serialize(new
            {
                name = "My Assistant",
                instructions = "You are a helpful assistant that can analyze documents.",
                model = "gpt-4-turbo",
                file_ids = fileIds // Attach multiple files
            });

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/assistants",
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic jsonResponse = JsonSerializer.Deserialize<dynamic>(responseContent);
            return jsonResponse.id; // Returns assistant_id
        }

        public async Task AddMessageWithFileSearchAsync(string threadId, String msg)
        {

            // The file is attached with the file_search tool
            var attachments = new List<object>();
           
            string[] fileIds = new[] { "file-12dPYnrrfBmqVHqDZmAgbz",
                                     "file-PAD6Vys3KTtzyKcRdTnFEa",
                                       "file-L4g6MyjV3LcDdua3u6LrMq",
                                        "file-WjPj9n4YKgJqrcFcY1vt8d",
                                         "file-XHw92mzCtqeutiHBSBtXQb",
                                          "file-XXn67gBgKNqXt5Rnj9nL6o",
                                           "file-HHFiXgcDwPyAKoZNkyCyMs",
                                             "file-AqrKUETA74AMaHyU6aZ4MG"};

            foreach (var fileId in fileIds)
            {
                attachments.Add(new
                {
                    file_id = fileId,  // Add file ID
                    tools = new[]
                    {
                        new
                        {
                            type = "file_search"  // Use the file_search tool for each file
                        }
                    }
                });
            }

            // Build the message body
            var requestBody = new
            {
                role = "user",
                content = msg,
                attachments = attachments.ToArray()  // Attach all files with the file_search tool
            };

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);

            // Send the request to add the message
            var response = await _httpClient.PostAsync(
                $"https://api.openai.com/v1/threads/{threadId}/messages",
                new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Message added with multiple file searches: {responseContent}");
        }

        public async Task<string> getDiscussion(string threadId)
        {
            //_httpClient.GetAsync($"https://api.openai.com/v1/threads/{threadId}/messages
            var response = await _httpClient.GetAsync($"https://api.openai.com/v1/threads/{threadId}/messages");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve messages: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            var messages = json["data"]
                .Reverse() // newest messages are first; reverse to show in order
                .Select(msg => $"{msg["role"].ToString().ToUpper()}:\n{msg["content"]?[0]?["text"]?["value"]?.ToString()}\n")
                .ToList();
           
            string cleaned = Regex.Replace("【4:0†source】【4:1†source】", "【\\d+:\\d+†source】", string.Empty);
            return string.Join("\n", messages);
            
        }

        public Task<string> sendMail(string body, string toEmail)
        {

            var smtpClient = new SmtpClient("smtp.cavehill.uwi.edu")
            {
                Port = 25,
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("webteam@cavehill.uwi.edu"),
                Subject = "Cave Hill ChatBot Discussion",
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            try
            {
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully!");
                return Task.FromResult("Email sent successfully!");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
            }

            return Task.FromResult("Email sent successfully!");
        }
}

}
