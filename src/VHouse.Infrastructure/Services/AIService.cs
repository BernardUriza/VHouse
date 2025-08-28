using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VHouse.Domain.Enums;
using VHouse.Domain.Interfaces;
using VHouse.Domain.ValueObjects;

namespace VHouse.Infrastructure.Services;

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;
    private readonly IConfiguration _configuration;

    // Configuration keys
    private const string CLAUDE_API_KEY = "Claude:ApiKey";
    private const string CLAUDE_BASE_URL = "https://api.anthropic.com/v1/messages";
    private const string OPENAI_API_KEY = "OpenAI:ApiKey";
    private const string OPENAI_BASE_URL = "https://api.openai.com/v1/chat/completions";
    private const string DALLE_BASE_URL = "https://api.openai.com/v1/images/generations";

    public AIService(HttpClient httpClient, ILogger<AIService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<AIResponse> GenerateTextAsync(AIRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Primero intenta Claude (prioridad)
        if (request.PreferredProvider == AIProvider.Claude || request.PreferredProvider == AIProvider.Fallback)
        {
            var claudeResponse = await TryClaudeAsync(request);
            if (claudeResponse.IsSuccessful)
            {
                stopwatch.Stop();
                return claudeResponse with { ResponseTime = stopwatch.Elapsed };
            }
            
            _logger.LogWarning("Claude failed: {Error}. Falling back to OpenAI...", claudeResponse.ErrorMessage);
        }

        // Fallback a OpenAI
        var openAIResponse = await TryOpenAIAsync(request);
        stopwatch.Stop();
        return openAIResponse with { ResponseTime = stopwatch.Elapsed };
    }

    public async Task<List<int>> ExtractProductIdsAsync(string catalogJson, string customerInput)
    {
        var systemMessage = """
            Eres un sistema que extrae IDs de productos de requests de clientes.
            El cliente habla español. Tu salida debe ser SOLO un array JSON válido de IDs de productos.
            Si no encuentras productos que coincidan, devuelve [-1].
            """;

        var prompt = $"""
            Catálogo de productos (JSON):
            {catalogJson}

            Input del cliente (español):
            {customerInput}

            Extrae solo los IDs de productos mencionados. Respuesta en formato JSON array:
            """;

        var request = new AIRequest
        {
            Prompt = prompt,
            SystemMessage = systemMessage,
            PreferredProvider = AIProvider.Claude,
            MaxTokens = 200,
            Temperature = 0.3
        };

        var response = await GenerateTextAsync(request);

        if (!response.IsSuccessful)
        {
            _logger.LogError("Failed to extract product IDs: {Error}", response.ErrorMessage);
            return new List<int> { -1 };
        }

        try
        {
            // Parse JSON response
            var productIds = JsonSerializer.Deserialize<List<int>>(response.Content);
            return productIds ?? new List<int> { -1 };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI response as JSON: {Content}", response.Content);
            return new List<int> { -1 };
        }
    }

    public async Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // DALL-E es el estándar para generación de imágenes
        var response = await TryDallEAsync(request);
        stopwatch.Stop();
        
        return response with { ResponseTime = stopwatch.Elapsed };
    }

    public async Task<AIResponse> AnalyzeIntentAsync(string userInput, string context = "")
    {
        var systemMessage = """
            Eres un asistente de un punto de venta vegano que analiza la intención del usuario.
            Responde en español. Identifica si el usuario quiere:
            - Comprar productos específicos
            - Ver información de productos
            - Hacer una pregunta general
            - Procesar una orden
            """;

        var prompt = $"""
            Contexto: {context}
            Input del usuario: {userInput}
            
            Analiza la intención del usuario y proporciona una respuesta útil.
            """;

        var request = new AIRequest
        {
            Prompt = prompt,
            SystemMessage = systemMessage,
            PreferredProvider = AIProvider.Claude,
            MaxTokens = 500,
            Temperature = 0.7
        };

        return await GenerateTextAsync(request);
    }

    public async Task<AIResponse> GenerateProductDescriptionAsync(string productName, bool isVegan = true)
    {
        var systemMessage = $"""
            Genera descripciones atractivas para productos {(isVegan ? "veganos" : "")} de un punto de venta.
            Responde en español. Sé creativo pero preciso.
            """;

        var prompt = $"""
            Producto: {productName}
            Tipo: {(isVegan ? "Vegano" : "Regular")}
            
            Genera una descripción atractiva y apetitosa para este producto.
            """;

        var request = new AIRequest
        {
            Prompt = prompt,
            SystemMessage = systemMessage,
            PreferredProvider = AIProvider.Claude,
            MaxTokens = 300,
            Temperature = 0.8
        };

        return await GenerateTextAsync(request);
    }

    public async Task<Dictionary<string, bool>> CheckAIServicesHealthAsync()
    {
        var healthStatus = new Dictionary<string, bool>();

        // Check Claude
        var claudeKey = GetClaudeApiKey();
        healthStatus["Claude"] = !string.IsNullOrEmpty(claudeKey);

        // Check OpenAI
        var openAIKey = GetOpenAIApiKey();
        healthStatus["OpenAI"] = !string.IsNullOrEmpty(openAIKey);

        return healthStatus;
    }

    public async Task<AIHealthStatus> GetHealthStatusAsync()
    {
        var serviceStatus = await CheckAIServicesHealthAsync();
        
        string recommendedProvider = "Claude";
        bool fallbackAvailable = false;

        if (serviceStatus.GetValueOrDefault("Claude", false))
        {
            recommendedProvider = "Claude";
            fallbackAvailable = serviceStatus.GetValueOrDefault("OpenAI", false);
        }
        else if (serviceStatus.GetValueOrDefault("OpenAI", false))
        {
            recommendedProvider = "OpenAI";
            fallbackAvailable = false;
        }
        else
        {
            recommendedProvider = "None";
            fallbackAvailable = false;
        }

        return new AIHealthStatus
        {
            ServiceStatus = serviceStatus,
            RecommendedProvider = recommendedProvider,
            FallbackAvailable = fallbackAvailable
        };
    }

    #region Private Methods

    private async Task<AIResponse> TryClaudeAsync(AIRequest request)
    {
        var apiKey = GetClaudeApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            return new AIResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Claude API key not configured"
            };
        }

        try
        {
            var payload = new
            {
                model = GetClaudeModelName(request.PreferredModel),
                max_tokens = request.MaxTokens,
                temperature = request.Temperature,
                system = request.SystemMessage,
                messages = new[]
                {
                    new { role = "user", content = request.Prompt }
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var httpResponse = await _httpClient.PostAsync(CLAUDE_BASE_URL, content);

            if (httpResponse.IsSuccessStatusCode)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                var claudeResponse = JsonSerializer.Deserialize<ClaudeAPIResponse>(responseContent);
                
                return new AIResponse
                {
                    Content = claudeResponse?.Content?.FirstOrDefault()?.Text ?? "",
                    UsedProvider = AIProvider.Claude,
                    UsedModel = request.PreferredModel,
                    IsSuccessful = true,
                    TokensUsed = claudeResponse?.Usage?.OutputTokens ?? 0
                };
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                return new AIResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Claude API error: {httpResponse.StatusCode} - {errorContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude API");
            return new AIResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"Claude API exception: {ex.Message}"
            };
        }
    }

    private async Task<AIResponse> TryOpenAIAsync(AIRequest request)
    {
        var apiKey = GetOpenAIApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            return new AIResponse
            {
                IsSuccessful = false,
                ErrorMessage = "OpenAI API key not configured"
            };
        }

        try
        {
            var messages = new List<object>();
            if (!string.IsNullOrEmpty(request.SystemMessage))
            {
                messages.Add(new { role = "system", content = request.SystemMessage });
            }
            messages.Add(new { role = "user", content = request.Prompt });

            var payload = new
            {
                model = GetOpenAIModelName(request.PreferredModel),
                messages = messages,
                temperature = request.Temperature,
                max_tokens = request.MaxTokens
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var httpResponse = await _httpClient.PostAsync(OPENAI_BASE_URL, content);

            if (httpResponse.IsSuccessStatusCode)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIAPIResponse>(responseContent);
                
                return new AIResponse
                {
                    Content = openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "",
                    UsedProvider = AIProvider.OpenAI,
                    UsedModel = request.PreferredModel,
                    IsSuccessful = true,
                    TokensUsed = openAIResponse?.Usage?.TotalTokens ?? 0
                };
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                return new AIResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = $"OpenAI API error: {httpResponse.StatusCode} - {errorContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            return new AIResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"OpenAI API exception: {ex.Message}"
            };
        }
    }

    private async Task<ImageGenerationResponse> TryDallEAsync(ImageGenerationRequest request)
    {
        var apiKey = GetOpenAIApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            return new ImageGenerationResponse
            {
                IsSuccessful = false,
                ErrorMessage = "OpenAI API key not configured for DALL-E"
            };
        }

        try
        {
            var payload = new
            {
                model = request.PreferredModel == AIModel.DallE2 ? "dall-e-2" : "dall-e-3",
                prompt = request.Prompt,
                n = 1,
                size = request.Size,
                quality = request.Quality,
                style = request.Style,
                response_format = "url"
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var httpResponse = await _httpClient.PostAsync(DALLE_BASE_URL, content);

            if (httpResponse.IsSuccessStatusCode)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                var dalleResponse = JsonSerializer.Deserialize<DallEAPIResponse>(responseContent);
                var imageData = dalleResponse?.Data?.FirstOrDefault();
                
                return new ImageGenerationResponse
                {
                    ImageUrl = imageData?.Url ?? "",
                    RevisedPrompt = imageData?.RevisedPrompt ?? request.Prompt,
                    UsedProvider = AIProvider.OpenAI,
                    UsedModel = request.PreferredModel,
                    IsSuccessful = true
                };
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                return new ImageGenerationResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = $"DALL-E API error: {httpResponse.StatusCode} - {errorContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DALL-E API");
            return new ImageGenerationResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"DALL-E API exception: {ex.Message}"
            };
        }
    }

    private string GetClaudeApiKey()
    {
        return _configuration[CLAUDE_API_KEY] ?? 
               Environment.GetEnvironmentVariable("CLAUDE_API_KEY") ?? 
               string.Empty;
    }

    private string GetOpenAIApiKey()
    {
        return _configuration[OPENAI_API_KEY] ?? 
               Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? 
               string.Empty;
    }

    private string GetClaudeModelName(AIModel model)
    {
        return model switch
        {
            AIModel.Claude3Opus => "claude-3-opus-20240229",
            AIModel.Claude3Sonnet => "claude-3-5-sonnet-20241022",
            AIModel.Claude3Haiku => "claude-3-haiku-20240307",
            _ => "claude-3-5-sonnet-20241022" // Default
        };
    }

    private string GetOpenAIModelName(AIModel model)
    {
        return model switch
        {
            AIModel.GPT4 => "gpt-4",
            AIModel.GPT4Turbo => "gpt-4-turbo-preview",
            AIModel.GPT35Turbo => "gpt-3.5-turbo",
            _ => "gpt-4" // Default
        };
    }

    #endregion

    #region API Response Models

    private record ClaudeAPIResponse(
        List<ClaudeContent> Content,
        ClaudeUsage Usage
    );

    private record ClaudeContent(string Text, string Type);
    private record ClaudeUsage(int InputTokens, int OutputTokens);

    private record OpenAIAPIResponse(
        List<OpenAIChoice> Choices,
        OpenAIUsage Usage
    );

    private record OpenAIChoice(OpenAIMessage Message);
    private record OpenAIMessage(string Content);
    private record OpenAIUsage(int TotalTokens);

    private record DallEAPIResponse(List<DallEImageData> Data);
    private record DallEImageData(string Url, string RevisedPrompt);

    #endregion

    public async Task<DemandForecast> PredictDemandAsync(int productId, int days, List<object> historicalData)
    {
        var systemMessage = """
            Eres un experto en predicción de demanda para productos veganos.
            Analiza los datos históricos y predice la demanda futura.
            Considera tendencias, patrones estacionales y factores específicos de productos veganos.
            """;

        var prompt = $"""
            Producto ID: {productId}
            Días a predecir: {days}
            
            Datos históricos:
            {JsonSerializer.Serialize(historicalData, new JsonSerializerOptions { WriteIndented = true })}
            
            Analiza los datos y proporciona:
            1. Predicción de demanda diaria para los próximos {days} días
            2. Análisis de tendencias
            3. Factores influyentes
            4. Recomendaciones de stock
            5. Nivel de confianza (0-1)
            
            Responde en JSON con este formato exacto:
            {
                "predictions": [
                    {"date": "2024-MM-DD", "quantity": 100, "confidence": 0.85, "factors": ["trend", "season"]}
                ],
                "trendAnalysis": "análisis detallado",
                "confidenceScore": 0.85,
                "recommendations": ["recomendación 1", "recomendación 2"]
            }
            """;

        var request = new AIRequest
        {
            Prompt = prompt,
            SystemMessage = systemMessage,
            PreferredProvider = AIProvider.Claude,
            MaxTokens = 1500,
            Temperature = 0.3
        };

        var response = await GenerateTextAsync(request);

        if (!response.IsSuccessful)
        {
            _logger.LogError("Failed to predict demand: {Error}", response.ErrorMessage);
            return new DemandForecast
            {
                ProductId = productId,
                DaysPredicted = days,
                ConfidenceScore = 0,
                TrendAnalysis = "Error al generar predicción",
                Recommendations = new List<string> { "Revisar datos históricos" }
            };
        }

        try
        {
            var jsonResponse = JsonSerializer.Deserialize<JsonDocument>(response.Content);
            var root = jsonResponse?.RootElement;

            var predictions = new List<DemandPrediction>();
            
            if (root?.TryGetProperty("predictions", out var predictionsElement) == true && 
                predictionsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var pred in predictionsElement.EnumerateArray())
                {
                    var dateStr = pred.GetProperty("date").GetString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                    var quantity = pred.GetProperty("quantity").GetDouble();
                    var confidence = pred.TryGetProperty("confidence", out var confProp) ? confProp.GetDouble() : 0.8;
                    
                    var factors = new List<string>();
                    if (pred.TryGetProperty("factors", out var factorsArray) && 
                        factorsArray.ValueKind == JsonValueKind.Array)
                    {
                        factors = factorsArray.EnumerateArray()
                            .Select(f => f.GetString() ?? "")
                            .Where(f => !string.IsNullOrEmpty(f))
                            .ToList();
                    }

                    predictions.Add(new DemandPrediction
                    {
                        Date = DateTime.Parse(dateStr),
                        PredictedQuantity = quantity,
                        LowerBound = quantity * 0.8,
                        UpperBound = quantity * 1.2,
                        Confidence = confidence,
                        InfluencingFactors = factors
                    });
                }
            }

            var trendAnalysis = root?.TryGetProperty("trendAnalysis", out var trendProp) == true 
                ? trendProp.GetString() ?? "Sin análisis disponible" 
                : "Sin análisis disponible";

            var confidenceScore = root?.TryGetProperty("confidenceScore", out var confScoreProp) == true 
                ? confScoreProp.GetDouble() 
                : 0.7;

            var recommendations = new List<string>();
            if (root?.TryGetProperty("recommendations", out var recArray) == true && 
                recArray.ValueKind == JsonValueKind.Array)
            {
                recommendations = recArray.EnumerateArray()
                    .Select(r => r.GetString() ?? "")
                    .Where(r => !string.IsNullOrEmpty(r))
                    .ToList();
            }

            return new DemandForecast
            {
                ProductId = productId,
                ProductName = $"Product {productId}",
                DaysPredicted = days,
                Predictions = predictions,
                ConfidenceScore = confidenceScore,
                TrendAnalysis = trendAnalysis,
                Recommendations = recommendations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse demand forecast response: {Content}", response.Content);
            return new DemandForecast
            {
                ProductId = productId,
                DaysPredicted = days,
                ConfidenceScore = 0.5,
                TrendAnalysis = "Error al procesar predicción",
                Recommendations = new List<string> { "Verificar calidad de datos" }
            };
        }
    }

    public async Task<InventoryOptimization> OptimizeInventoryAsync(List<object> inventoryData, List<object> salesData)
    {
        var systemMessage = """
            Eres un experto en optimización de inventarios para productos veganos.
            Analiza el inventario actual y los datos de ventas para optimizar stock.
            Considera rotación de productos, caducidad, demanda estacional y márgenes.
            """;

        var prompt = $"""
            Datos de inventario actual:
            {JsonSerializer.Serialize(inventoryData, new JsonSerializerOptions { WriteIndented = true })}
            
            Datos de ventas:
            {JsonSerializer.Serialize(salesData, new JsonSerializerOptions { WriteIndented = true })}
            
            Analiza y proporciona:
            1. Productos que necesitan restock urgente
            2. Productos con exceso de inventario
            3. Recomendaciones de cantidades óptimas
            4. Factores de riesgo identificados
            5. Score de optimización general (0-1)
            
            Responde en JSON con este formato exacto:
            {
                "recommendations": [
                    {
                        "productId": 1,
                        "action": "restock",
                        "quantity": 100,
                        "reason": "Stock bajo, alta demanda",
                        "priority": "high",
                        "impact": 0.8
                    }
                ],
                "optimizationScore": 0.75,
                "summary": "Análisis general del inventario",
                "riskFactors": ["factor 1", "factor 2"]
            }
            """;

        var request = new AIRequest
        {
            Prompt = prompt,
            SystemMessage = systemMessage,
            PreferredProvider = AIProvider.Claude,
            MaxTokens = 1500,
            Temperature = 0.3
        };

        var response = await GenerateTextAsync(request);

        if (!response.IsSuccessful)
        {
            _logger.LogError("Failed to optimize inventory: {Error}", response.ErrorMessage);
            return new InventoryOptimization
            {
                OptimizationScore = 0,
                Summary = "Error al generar optimización",
                RiskFactors = new List<string> { "Error en análisis de datos" }
            };
        }

        try
        {
            var jsonResponse = JsonSerializer.Deserialize<JsonDocument>(response.Content);
            var root = jsonResponse?.RootElement;

            var recommendations = new List<InventoryRecommendation>();
            
            if (root?.TryGetProperty("recommendations", out var recElement) == true && 
                recElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var rec in recElement.EnumerateArray())
                {
                    var productId = rec.TryGetProperty("productId", out var idProp) ? idProp.GetInt32() : 0;
                    var action = rec.TryGetProperty("action", out var actionProp) ? actionProp.GetString() ?? "" : "";
                    var quantity = rec.TryGetProperty("quantity", out var qtyProp) ? qtyProp.GetInt32() : 0;
                    var reason = rec.TryGetProperty("reason", out var reasonProp) ? reasonProp.GetString() ?? "" : "";
                    var priority = rec.TryGetProperty("priority", out var priProp) ? priProp.GetString() ?? "medium" : "medium";
                    var impact = rec.TryGetProperty("impact", out var impactProp) ? impactProp.GetDouble() : 0.5;

                    recommendations.Add(new InventoryRecommendation
                    {
                        ProductId = productId,
                        ProductName = $"Product {productId}",
                        Action = action,
                        RecommendedQuantity = quantity,
                        Reason = reason,
                        Priority = priority,
                        EstimatedImpact = impact
                    });
                }
            }

            var optimizationScore = root?.TryGetProperty("optimizationScore", out var scoreProp) == true 
                ? scoreProp.GetDouble() 
                : 0.5;

            var summary = root?.TryGetProperty("summary", out var sumProp) == true 
                ? sumProp.GetString() ?? "Análisis completado" 
                : "Análisis completado";

            var riskFactors = new List<string>();
            if (root?.TryGetProperty("riskFactors", out var riskArray) == true && 
                riskArray.ValueKind == JsonValueKind.Array)
            {
                riskFactors = riskArray.EnumerateArray()
                    .Select(r => r.GetString() ?? "")
                    .Where(r => !string.IsNullOrEmpty(r))
                    .ToList();
            }

            return new InventoryOptimization
            {
                Recommendations = recommendations,
                OptimizationScore = optimizationScore,
                Summary = summary,
                RiskFactors = riskFactors,
                Metrics = new Dictionary<string, object>
                {
                    ["totalRecommendations"] = recommendations.Count,
                    ["highPriorityItems"] = recommendations.Count(r => r.Priority == "high"),
                    ["restockItems"] = recommendations.Count(r => r.Action.Contains("restock"))
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse inventory optimization response: {Content}", response.Content);
            return new InventoryOptimization
            {
                OptimizationScore = 0.3,
                Summary = "Error al procesar optimización",
                RiskFactors = new List<string> { "Error en análisis de datos" }
            };
        }
    }

    public async Task<BusinessInsights> GenerateBusinessInsightsAsync(object businessData)
    {
        var systemMessage = """
            Eres un consultor experto en negocios veganos y análisis de datos.
            Analiza los datos de negocio y genera insights accionables.
            Enfócate en oportunidades de crecimiento, optimizaciones y tendencias del mercado vegano.
            """;

        var prompt = $"""
            Datos del negocio:
            {JsonSerializer.Serialize(businessData, new JsonSerializerOptions { WriteIndented = true })}
            
            Genera un análisis completo incluyendo:
            1. Insights clave más importantes
            2. Acciones recomendadas específicas
            3. Tendencias identificadas
            4. Oportunidades de crecimiento
            5. Resumen ejecutivo
            6. Score de análisis (0-1)
            
            Responde en JSON con este formato exacto:
            {
                "keyInsights": ["insight 1", "insight 2", "insight 3"],
                "recommendedActions": ["acción 1", "acción 2"],
                "summary": "Resumen ejecutivo del análisis",
                "analysisScore": 0.85,
                "trends": ["tendencia 1", "tendencia 2"],
                "opportunities": ["oportunidad 1", "oportunidad 2"],
                "metrics": {
                    "growthPotential": 0.8,
                    "riskLevel": 0.2
                }
            }
            """;

        var request = new AIRequest
        {
            Prompt = prompt,
            SystemMessage = systemMessage,
            PreferredProvider = AIProvider.Claude,
            MaxTokens = 2000,
            Temperature = 0.4
        };

        var response = await GenerateTextAsync(request);

        if (!response.IsSuccessful)
        {
            _logger.LogError("Failed to generate business insights: {Error}", response.ErrorMessage);
            return new BusinessInsights
            {
                AnalysisScore = 0,
                Summary = "Error al generar insights",
                KeyInsights = new List<string> { "Error en análisis de datos" }
            };
        }

        try
        {
            var jsonResponse = JsonSerializer.Deserialize<JsonDocument>(response.Content);
            var root = jsonResponse?.RootElement;

            var keyInsights = new List<string>();
            if (root?.TryGetProperty("keyInsights", out var insightsArray) == true && 
                insightsArray.ValueKind == JsonValueKind.Array)
            {
                keyInsights = insightsArray.EnumerateArray()
                    .Select(i => i.GetString() ?? "")
                    .Where(i => !string.IsNullOrEmpty(i))
                    .ToList();
            }

            var recommendedActions = new List<string>();
            if (root?.TryGetProperty("recommendedActions", out var actionsArray) == true && 
                actionsArray.ValueKind == JsonValueKind.Array)
            {
                recommendedActions = actionsArray.EnumerateArray()
                    .Select(a => a.GetString() ?? "")
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();
            }

            var trends = new List<string>();
            if (root?.TryGetProperty("trends", out var trendsArray) == true && 
                trendsArray.ValueKind == JsonValueKind.Array)
            {
                trends = trendsArray.EnumerateArray()
                    .Select(t => t.GetString() ?? "")
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            }

            var opportunities = new List<string>();
            if (root?.TryGetProperty("opportunities", out var oppsArray) == true && 
                oppsArray.ValueKind == JsonValueKind.Array)
            {
                opportunities = oppsArray.EnumerateArray()
                    .Select(o => o.GetString() ?? "")
                    .Where(o => !string.IsNullOrEmpty(o))
                    .ToList();
            }

            var summary = root?.TryGetProperty("summary", out var sumProp) == true 
                ? sumProp.GetString() ?? "Análisis completado" 
                : "Análisis completado";

            var analysisScore = root?.TryGetProperty("analysisScore", out var scoreProp) == true 
                ? scoreProp.GetDouble() 
                : 0.7;

            var metrics = new Dictionary<string, object>();
            if (root?.TryGetProperty("metrics", out var metricsObj) == true && 
                metricsObj.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in metricsObj.EnumerateObject())
                {
                    metrics[property.Name] = property.Value.ValueKind switch
                    {
                        JsonValueKind.Number => property.Value.GetDouble(),
                        JsonValueKind.String => property.Value.GetString() ?? "",
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => property.Value.GetRawText()
                    };
                }
            }

            return new BusinessInsights
            {
                KeyInsights = keyInsights,
                RecommendedActions = recommendedActions,
                Summary = summary,
                AnalysisScore = analysisScore,
                Metrics = metrics,
                Trends = trends,
                Opportunities = opportunities
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse business insights response: {Content}", response.Content);
            return new BusinessInsights
            {
                AnalysisScore = 0.4,
                Summary = "Error al procesar insights",
                KeyInsights = new List<string> { "Error en análisis de datos" }
            };
        }
    }
}