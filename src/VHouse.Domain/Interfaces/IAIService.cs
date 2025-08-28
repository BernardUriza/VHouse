using VHouse.Domain.ValueObjects;

namespace VHouse.Domain.Interfaces;

/// <summary>
/// Servicio de AI que da prioridad a Claude y fallback a OpenAI
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Genera una respuesta de texto usando Claude (preferido) con fallback a OpenAI
    /// </summary>
    Task<AIResponse> GenerateTextAsync(AIRequest request);
    
    /// <summary>
    /// Extrae IDs de productos de input del usuario usando AI
    /// </summary>
    Task<List<int>> ExtractProductIdsAsync(string catalogJson, string customerInput);
    
    /// <summary>
    /// Genera imagen usando DALL-E (OpenAI) o Claude (cuando esté disponible)
    /// </summary>
    Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request);
    
    /// <summary>
    /// Analiza texto para determinar intención del usuario
    /// </summary>
    Task<AIResponse> AnalyzeIntentAsync(string userInput, string context = "");
    
    /// <summary>
    /// Genera descripción de producto vegano usando AI
    /// </summary>
    Task<AIResponse> GenerateProductDescriptionAsync(string productName, bool isVegan = true);
    
    /// <summary>
    /// Verifica el estado de los servicios AI disponibles
    /// </summary>
    Task<Dictionary<string, bool>> CheckAIServicesHealthAsync();
    
    /// <summary>
    /// Obtiene el estado de salud detallado de los servicios AI
    /// </summary>
    Task<AIHealthStatus> GetHealthStatusAsync();
    
    /// <summary>
    /// Predice la demanda futura de un producto usando análisis de datos históricos
    /// </summary>
    Task<DemandForecast> PredictDemandAsync(int productId, int days, List<object> historicalData);
    
    /// <summary>
    /// Analiza patrones de inventario y sugiere optimizaciones
    /// </summary>
    Task<InventoryOptimization> OptimizeInventoryAsync(List<object> inventoryData, List<object> salesData);
    
    /// <summary>
    /// Genera insights de negocio basados en datos de ventas y patrones
    /// </summary>
    Task<BusinessInsights> GenerateBusinessInsightsAsync(object businessData);
    
    /// <summary>
    /// Procesa pedidos con cantidades y fechas específicas (Fase 1)
    /// </summary>
    Task<EnhancedOrderResult> ProcessEnhancedOrderAsync(string catalogJson, string customerInput);
    
    /// <summary>
    /// Valida disponibilidad inteligente de productos considerando stock y estado
    /// </summary>
    Task<ProductAvailabilityValidation> ValidateProductAvailabilityAsync(List<object> orderItems, object dbContext);
    
    /// <summary>
    /// Genera sugerencias de productos alternativos cuando hay falta de stock
    /// </summary>
    Task<AlternativeProductSuggestions> GenerateAlternativeProductsAsync(List<int> unavailableProductIds, string availableProductsJson);
}