using VHouse.Domain.Entities;

namespace VHouse.Domain.Interfaces;

/// <summary>
/// Servicio de chatbot inteligente que combina contexto con AI para respuestas personalizadas
/// </summary>
public interface IIntelligentChatbotService
{
    /// <summary>
    /// Procesa un mensaje de usuario con contexto automático y genera respuesta inteligente
    /// </summary>
    Task<ChatbotResponse> ProcessMessageAsync(ChatbotRequest request);
    
    /// <summary>
    /// Inicializa una nueva sesión de chat con contexto específico
    /// </summary>
    Task<ChatSession> InitializeChatSessionAsync(string currentUrl, object? contextData = null);
    
    /// <summary>
    /// Actualiza el contexto de una sesión existente
    /// </summary>
    Task<ChatSession> UpdateSessionContextAsync(string sessionId, string currentUrl, object? contextData = null);
    
    /// <summary>
    /// Obtiene sugerencias contextuales basadas en el estado actual
    /// </summary>
    List<string> GetContextualSuggestions(string sessionId, string? lastMessage = null);
    
    /// <summary>
    /// Procesa comandos especiales del chatbot (ej: /productos, /pedido)
    /// </summary>
    ChatbotResponse ProcessSpecialCommand(string command, ChatSession session);
    
    /// <summary>
    /// Analiza la intención del usuario y determina acciones recomendadas
    /// </summary>
    Task<UserIntentAnalysis> AnalyzeUserIntentAsync(string message, ChatContext context);
}

/// <summary>
/// Solicitud de chatbot con contexto
/// </summary>
public class ChatbotRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string CurrentUrl { get; set; } = string.Empty;
    public object? ContextData { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Respuesta del chatbot con información contextual
/// </summary>
public class ChatbotResponse
{
    public string Content { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public List<string> Suggestions { get; set; } = new();
    public List<ChatAction> RecommendedActions { get; set; } = new();
    public ChatContext UsedContext { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Sesión de chat con contexto persistente
/// </summary>
public class ChatSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public ChatContext Context { get; set; } = new();
    public List<ChatMessage> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> SessionData { get; set; } = new();
}

/// <summary>
/// Mensaje individual en la sesión de chat
/// </summary>
public class ChatMessage
{
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Análisis de intención del usuario
/// </summary>
public class UserIntentAnalysis
{
    public string PrimaryIntent { get; set; } = string.Empty;
    public List<string> SecondaryIntents { get; set; } = new();
    public List<string> ExtractedEntities { get; set; } = new();
    public double Confidence { get; set; }
    public List<ChatAction> SuggestedActions { get; set; } = new();
}

/// <summary>
/// Acción recomendada por el chatbot
/// </summary>
public class ChatAction
{
    public string ActionType { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// Tipos de intenciones comunes
/// </summary>
public static class IntentTypes
{
    public const string ProductInquiry = "product-inquiry";
    public const string OrderRequest = "order-request";
    public const string PriceInquiry = "price-inquiry";
    public const string StockCheck = "stock-check";
    public const string NutritionalInfo = "nutritional-info";
    public const string BusinessSupport = "business-support";
    public const string OrderTracking = "order-tracking";
    public const string GeneralHelp = "general-help";
}

/// <summary>
/// Tipos de acciones disponibles
/// </summary>
public static class ActionTypes
{
    public const string ShowProducts = "show-products";
    public const string CreateOrder = "create-order";
    public const string ShowPrices = "show-prices";
    public const string CheckStock = "check-stock";
    public const string ShowNutrition = "show-nutrition";
    public const string ContactSupport = "contact-support";
    public const string ViewOrders = "view-orders";
}