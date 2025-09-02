using VHouse.Domain.Entities;

namespace VHouse.Domain.Interfaces;

/// <summary>
/// Servicio para detectar automáticamente el contexto de la página actual
/// y proporcionar información contextual al chatbot
/// </summary>
public interface IChatContextService
{
    /// <summary>
    /// Detecta el contexto actual basado en la URL y datos disponibles
    /// </summary>
    Task<ChatContext> GetContextAsync(string currentUrl, object? contextData = null);
    
    /// <summary>
    /// Obtiene el contexto específico para el portal de clientes B2B
    /// </summary>
    Task<ChatContext> GetClientPortalContextAsync(ClientTenant clientTenant, List<ClientProduct> products);
    
    /// <summary>
    /// Obtiene el contexto específico para la pantalla de órdenes
    /// </summary>
    Task<ChatContext> GetOrdersContextAsync(List<Order> orders, List<Product> products);
    
    /// <summary>
    /// Obtiene el contexto específico para la pantalla de administración
    /// </summary>
    Task<ChatContext> GetAdminContextAsync();
    
    /// <summary>
    /// Genera un mensaje del sistema personalizado basado en el contexto
    /// </summary>
    string GenerateSystemMessage(ChatContext context);
    
    /// <summary>
    /// Obtiene sugerencias contextuales para el chatbot
    /// </summary>
    List<string> GetContextualSuggestions(ChatContext context);
}

/// <summary>
/// Representa el contexto actual de la aplicación para el chatbot
/// </summary>
public class ChatContext
{
    public string PageType { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public string UserRole { get; set; } = "Guest";
    public Dictionary<string, object> Data { get; set; } = new();
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    public List<string> AvailableActions { get; set; } = new();
    public string PrimaryFocus { get; set; } = string.Empty;
    
    // Contexto específico para páginas
    public ClientTenant? ClientTenant { get; set; }
    public List<Product> AvailableProducts { get; set; } = new();
    public List<Order> RecentOrders { get; set; } = new();
    public decimal? TotalSpent { get; set; }
    public string? SpecialInstructions { get; set; }
}

/// <summary>
/// Tipos de páginas que el sistema puede detectar
/// </summary>
public static class PageTypes
{
    public const string ClientPortal = "client-portal";
    public const string AdminDashboard = "admin-dashboard";
    public const string Orders = "orders";
    public const string Products = "products";
    public const string POS = "pos";
    public const string Reports = "reports";
    public const string Unknown = "unknown";
}