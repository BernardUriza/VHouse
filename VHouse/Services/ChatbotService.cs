using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class ChatbotService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<ChatbotService> logger;

    public ChatbotService(HttpClient httpClient, ILogger<ChatbotService> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    public async Task<string> ConsultChatbotAsync(string additionalText)
    {
        try
        {
            string jsonText = await httpClient.GetStringAsync("chatbotRequest.json");

            if (string.IsNullOrEmpty(jsonText))
            {
                logger.LogWarning("JSON file is empty or missing.");
                return "Error: JSON file is empty or missing.";
            }

            using (JsonDocument doc = JsonDocument.Parse(jsonText))
            {
                var root = doc.RootElement;

                var updatedPrompt = root.GetProperty("prompt").GetString() + " " + additionalText;

                var updatedJsonObject = new
                {
                    model = root.GetProperty("model").GetString(),
                    prompt = updatedPrompt,
                    temperature = root.GetProperty("temperature").GetDouble(),
                    max_tokens = root.GetProperty("max_tokens").GetInt32(),
                    top_p = root.GetProperty("top_p").GetDouble(),
                    frequency_penalty = root.GetProperty("frequency_penalty").GetDouble(),
                    presence_penalty = root.GetProperty("presence_penalty").GetDouble()
                };

                string updatedJsonText = JsonSerializer.Serialize(updatedJsonObject);

                string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                if (string.IsNullOrEmpty(apiKey))
                {
                    logger.LogError("API key is missing.");
                    return "Error: API key is not set.";
                }

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/completions")
                {
                    Content = new StringContent(updatedJsonText, Encoding.UTF8, "application/json")
                };

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonSerializer.Deserialize<ResponseObject>(responseContent);
                    return responseObject.choices[0].text;
                }
                else
                {
                    logger.LogError($"HTTP Error: {response.StatusCode}");
                    return $"HTTP Error: {response.StatusCode}";
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consulting the chatbot");
            return "Error consulting the chatbot: " + ex.Message;
        }
    }

    public class ResponseObject
    {
        public List<Choice> choices { get; set; }
    }

    public class Choice
    {
        public string text { get; set; }
    }
}
