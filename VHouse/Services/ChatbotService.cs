using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using VHouse.Interfaces;

public class ChatbotService : IChatbotService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<ChatbotService> logger;
    private readonly IConfiguration configuration;

    public ChatbotService(HttpClient httpClient, ILogger<ChatbotService> logger, IConfiguration configuration)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        this.configuration = configuration;
    }

    public async Task<List<int>> ExtractProductIdsAsync(string catalogJson, string customerInput)
    {
        try
        {
            // Define the OpenAI API URL
            string apiUrl = "https://api.openai.com/v1/completions";

            string prompt = $@"
            You are a highly accurate system that extracts product IDs from customer requests.
            The customer speaks Spanish, so the input will be in Spanish. However, your output must always be in the form of a valid JSON array of product IDs.

            Rules:
            1. Your output must ONLY be a valid JSON array, with no additional text, formatting, or explanations.
            2. If no products are found matching the customer input, return the placeholder ID [-1].
            3. Always ensure the JSON array is valid and well-formed.

            Product Catalog (JSON):
            {catalogJson}

            Customer Input (in Spanish):
            {customerInput}

            Output:
            Only return a JSON array of product IDs. Example output: [101, 104]
            ";


            // Create the request payload
            var requestPayload = new
            {
                model = "gpt-3.5-turbo-instruct",
                prompt = prompt,
                temperature = 0.7,
                max_tokens = 500,
                top_p = 1.0,
                frequency_penalty = 0.0,
                presence_penalty = 0.0
            };

            // Serialize the payload to JSON
            string payloadJson = JsonSerializer.Serialize(requestPayload);

            // Set the authorization header with the OpenAI API key
            string apiKey = configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("OpenAI API key is not configured. Please set the OPENAI_API_KEY environment variable or configure OpenAI:ApiKey in appsettings.");
            }
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // Create the HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
            };

            // Send the request
            var response = await httpClient.SendAsync(request);

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Parse the response content
                string responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize the response JSON
                var responseObject = JsonSerializer.Deserialize<ResponseObject>(responseContent);

                // Extract and return the product IDs from the response
                string responseText = responseObject.choices[0].text.Trim();
                var productIds = JsonSerializer.Deserialize<List<int>>(responseText);

                return productIds;
            }
            else
            {
                // Log and handle HTTP errors
                logger.LogError($"HTTP Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                throw new HttpRequestException($"HTTP Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            // Log and rethrow exceptions
            logger.LogError(ex, "Error extracting product IDs");
            throw;
        }
    }

    // Response structure for OpenAI API
    private class ResponseObject
    {
        public List<Choice> choices { get; set; }
    }

    private class Choice
    {
        public string text { get; set; }
    }
}
