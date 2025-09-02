using Microsoft.Extensions.Logging;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace VHouse.Application.Services;

/// <summary>
/// Servicio de contexto de chat que detecta automáticamente la página actual
/// y proporciona información contextual para el chatbot AI
/// </summary>
public class ChatContextService : IChatContextService
{
    private readonly ILogger<ChatContextService> _logger;

    public ChatContextService(ILogger<ChatContextService> logger)
    {
        _logger = logger;
    }

    public async Task<ChatContext> GetContextAsync(string currentUrl, object? contextData = null)
    {
        _logger.LogInformation("Detectando contexto para URL: {Url}", currentUrl);
        
        var context = new ChatContext();
        
        // Detectar tipo de página basado en URL
        context.PageType = DetectPageType(currentUrl);
        context.PageTitle = GetPageTitle(context.PageType);
        context.UserRole = DetermineUserRole(currentUrl);
        context.AvailableActions = GetAvailableActions(context.PageType);
        context.PrimaryFocus = GetPrimaryFocus(context.PageType);
        
        // Agregar datos específicos del contexto si están disponibles
        if (contextData != null)
        {
            context.Data["rawContextData"] = contextData;
        }
        
        _logger.LogInformation("Contexto detectado: Página={PageType}, Usuario={UserRole}", 
            context.PageType, context.UserRole);
            
        return await Task.FromResult(context);
    }

    public async Task<ChatContext> GetClientPortalContextAsync(ClientTenant clientTenant, List<ClientProduct> products)
    {
        _logger.LogInformation("Generando contexto para portal de cliente: {ClientName}", clientTenant.BusinessName);
        
        var context = new ChatContext
        {
            PageType = PageTypes.ClientPortal,
            PageTitle = $"Portal B2B - {clientTenant.BusinessName}",
            UserRole = "ClientUser",
            ClientTenant = clientTenant,
            PrimaryFocus = "Consulta de productos y pedidos veganos"
        };
        
        // Extraer productos disponibles
        context.AvailableProducts = products.Select(cp => cp.Product).ToList();
        
        // Agregar información específica del cliente
        context.Data["clientName"] = clientTenant.BusinessName;
        context.Data["clientType"] = clientTenant.TenantCode;
        context.Data["productCount"] = products.Count;
        context.Data["clientSpecialty"] = GetClientSpecialty(clientTenant.TenantCode);
        
        // Acciones disponibles para clientes
        context.AvailableActions = new List<string>
        {
            "Ver catálogo de productos",
            "Consultar precios personalizados",
            "Hacer pedido",
            "Consultar histórico de pedidos",
            "Obtener recomendaciones",
            "Información nutricional",
            "Contactar a Bernard"
        };
        
        // Instrucciones especiales basadas en el cliente
        context.SpecialInstructions = GetClientSpecialInstructions(clientTenant.TenantCode);
        
        return await Task.FromResult(context);
    }

    public async Task<ChatContext> GetOrdersContextAsync(List<Order> orders, List<Product> products)
    {
        _logger.LogInformation("Generando contexto para órdenes: {OrderCount} órdenes", orders.Count);
        
        var context = new ChatContext
        {
            PageType = PageTypes.Orders,
            PageTitle = "Gestión de Pedidos",
            UserRole = "Admin",
            RecentOrders = orders,
            AvailableProducts = products,
            PrimaryFocus = "Análisis de pedidos y gestión de inventario"
        };
        
        // Calcular estadísticas de órdenes
        var totalValue = orders.Sum(o => o.TotalAmount);
        var averageOrder = orders.Any() ? totalValue / orders.Count : 0;
        
        context.Data["totalOrderValue"] = totalValue;
        context.Data["averageOrderValue"] = averageOrder;
        context.Data["orderCount"] = orders.Count;
        context.Data["topProducts"] = GetTopProducts(orders);
        
        context.AvailableActions = new List<string>
        {
            "Analizar tendencias de pedidos",
            "Generar reporte de ventas",
            "Optimizar inventario",
            "Predecir demanda",
            "Contactar clientes",
            "Procesar pedidos pendientes"
        };
        
        return await Task.FromResult(context);
    }

    public async Task<ChatContext> GetAdminContextAsync()
    {
        _logger.LogInformation("Generando contexto administrativo");
        
        var context = new ChatContext
        {
            PageType = PageTypes.AdminDashboard,
            PageTitle = "Panel Administrativo VHouse",
            UserRole = "Admin",
            PrimaryFocus = "Gestión integral del negocio vegano"
        };
        
        context.AvailableActions = new List<string>
        {
            "Gestionar productos",
            "Análisis de ventas",
            "Gestión de clientes",
            "Optimización de inventario",
            "Reportes financieros",
            "Marketing vegano",
            "Análisis predictivo"
        };
        
        context.SpecialInstructions = "Enfócate en insights de negocio y optimización operativa para maximizar el impacto vegano";
        
        return await Task.FromResult(context);
    }

    public string GenerateSystemMessage(ChatContext context)
    {
        var baseMessage = $@"Eres VeganAI, el asistente especializado de VHouse - Distribución Vegana Profesional de Bernard Uriza Orozco.

CONTEXTO ACTUAL:
- Página: {context.PageTitle}
- Usuario: {context.UserRole}
- Enfoque: {context.PrimaryFocus}
";

        switch (context.PageType)
        {
            case PageTypes.ClientPortal:
                return baseMessage + GenerateClientPortalSystemMessage(context);
                
            case PageTypes.Orders:
                return baseMessage + GenerateOrdersSystemMessage(context);
                
            case PageTypes.AdminDashboard:
                return baseMessage + GenerateAdminSystemMessage(context);
                
            default:
                return baseMessage + @"
INSTRUCCIONES GENERALES:
- Responde SIEMPRE en español con emojis veganos
- Usa HTML para formato (br, strong, em)
- Mantén un tono profesional pero amigable
- Enfócate en la misión vegana de VHouse
- Termina con una pregunta para continuar la conversación

POR LOS ANIMALES. CADA PEDIDO SALVA VIDAS. 🌱";
        }
    }

    public List<string> GetContextualSuggestions(ChatContext context)
    {
        return context.PageType switch
        {
            PageTypes.ClientPortal => GetClientPortalSuggestions(context),
            PageTypes.Orders => GetOrdersSuggestions(context),
            PageTypes.AdminDashboard => GetAdminSuggestions(context),
            _ => GetDefaultSuggestions()
        };
    }

    // Métodos privados de apoyo
    private string DetectPageType(string url)
    {
        var lowerUrl = url.ToLowerInvariant();
        
        if (Regex.IsMatch(lowerUrl, @"/client/[^/]+$"))
            return PageTypes.ClientPortal;
        if (lowerUrl.Contains("/orders"))
            return PageTypes.Orders;
        if (lowerUrl.Contains("/admin") || lowerUrl.Contains("/dashboard"))
            return PageTypes.AdminDashboard;
        if (lowerUrl.Contains("/products"))
            return PageTypes.Products;
        if (lowerUrl.Contains("/pos"))
            return PageTypes.POS;
        if (lowerUrl.Contains("/reports"))
            return PageTypes.Reports;
            
        return PageTypes.Unknown;
    }

    private string GetPageTitle(string pageType)
    {
        return pageType switch
        {
            PageTypes.ClientPortal => "Portal B2B VHouse",
            PageTypes.AdminDashboard => "Panel Administrativo",
            PageTypes.Orders => "Gestión de Pedidos",
            PageTypes.Products => "Catálogo de Productos",
            PageTypes.POS => "Punto de Venta",
            PageTypes.Reports => "Reportes y Analytics",
            _ => "VHouse - Distribución Vegana"
        };
    }

    private string DetermineUserRole(string url)
    {
        if (url.Contains("/client/"))
            return "ClientUser";
        if (url.Contains("/admin") || url.Contains("/dashboard") || url.Contains("/orders"))
            return "Admin";
        return "Guest";
    }

    private List<string> GetAvailableActions(string pageType)
    {
        return pageType switch
        {
            PageTypes.ClientPortal => new List<string>
            {
                "Ver productos disponibles",
                "Hacer pedido",
                "Consultar histórico",
                "Obtener recomendaciones"
            },
            PageTypes.Orders => new List<string>
            {
                "Procesar pedidos",
                "Analizar tendencias",
                "Optimizar inventario",
                "Generar reportes"
            },
            PageTypes.AdminDashboard => new List<string>
            {
                "Gestionar productos",
                "Análisis de ventas",
                "Reportes financieros",
                "Optimización operativa"
            },
            _ => new List<string>
            {
                "Consultar productos",
                "Información general",
                "Contactar soporte"
            }
        };
    }

    private string GetPrimaryFocus(string pageType)
    {
        return pageType switch
        {
            PageTypes.ClientPortal => "Facilitación de pedidos B2B veganos",
            PageTypes.Orders => "Gestión eficiente de pedidos",
            PageTypes.AdminDashboard => "Optimización del negocio vegano",
            _ => "Asistencia general VHouse"
        };
    }

    private string GetClientSpecialty(string tenantCode)
    {
        return tenantCode switch
        {
            "mona-la-dona" => "Especializada en donas veganas artesanales",
            "sano-market" => "Mercado consciente con enfoque en salud",
            "papeleria-fenix" => "Papelería con sección vegana",
            _ => "Negocio especializado en productos veganos"
        };
    }

    private string GetClientSpecialInstructions(string tenantCode)
    {
        return tenantCode switch
        {
            "mona-la-dona" => "Enfócate en ingredientes para repostería vegana, especialmente para donas. Menciona texturas, sabores y técnicas.",
            "sano-market" => "Prioriza aspectos nutricionales, beneficios de salud y certificaciones orgánicas.",
            "papeleria-fenix" => "Combina información de productos veganos con necesidades de una papelería comunitaria.",
            _ => "Personaliza recomendaciones según el perfil del negocio."
        };
    }

    private List<object> GetTopProducts(List<Order> orders)
    {
        // Lógica para obtener productos más vendidos
        return new List<object>(); // Implementar según necesidades
    }

    private string GenerateClientPortalSystemMessage(ChatContext context)
    {
        var clientName = context.Data.GetValueOrDefault("clientName", "nuestro cliente").ToString();
        var specialty = context.Data.GetValueOrDefault("clientSpecialty", "").ToString();
        var productCount = context.Data.GetValueOrDefault("productCount", 0);

        return $@"
CLIENTE ACTUAL: {clientName} ({specialty})
PRODUCTOS DISPONIBLES: {productCount}

PRODUCTOS EN CONTEXTO:
{string.Join("\n", context.AvailableProducts.Take(10).Select(p => 
    $"• {p.Emoji} {p.ProductName} - ${p.PricePublic:F2} - {p.Description}"))}

INSTRUCCIONES ESPECÍFICAS:
- {context.SpecialInstructions}
- Incluye precios, stock y cantidades mínimas específicas
- Da ejemplos prácticos de uso para {clientName}
- Enfócate en cómo los productos ayudan a su negocio
- Si preguntan sobre pedidos, guíalos paso a paso

¿Cómo puedo ayudar a {clientName} hoy? 🌱";
    }

    private string GenerateOrdersSystemMessage(ChatContext context)
    {
        var orderCount = context.Data.GetValueOrDefault("orderCount", 0);
        var totalValue = context.Data.GetValueOrDefault("totalOrderValue", 0);

        return $@"
GESTIÓN DE PEDIDOS VEGANOS:
- Órdenes activas: {orderCount}
- Valor total: ${totalValue:F2}

CAPACIDADES DISPONIBLES:
- Análisis de tendencias de pedidos
- Optimización de inventario
- Predicción de demanda
- Identificación de productos top
- Recomendaciones de stock

ENFOQUE: Maximizar eficiencia operativa para Bernard mientras servimos mejor a nuestros clientes veganos.

¿Qué análisis necesitas para optimizar las operaciones? 📊";
    }

    private string GenerateAdminSystemMessage(ChatContext context)
    {
        return @"
PANEL ADMINISTRATIVO VHOUSE:
Acceso completo a todas las funcionalidades del sistema.

CAPACIDADES AVANZADAS:
- Analytics predictivos con IA
- Optimización de inventario inteligente
- Análisis de rentabilidad por cliente
- Insights de mercado vegano
- Automatización de procesos

MISIÓN: Ayudar a Bernard a escalar su impacto vegano mientras mantiene la calidad y personalización que caracteriza a VHouse.

¿Qué aspecto del negocio quieres optimizar hoy? 🚀";
    }

    private List<string> GetClientPortalSuggestions(ChatContext context)
    {
        var clientType = context.Data.GetValueOrDefault("clientType", "").ToString();
        
        return clientType switch
        {
            "mona-la-dona" => new List<string>
            {
                "¿Qué ingredientes necesito para donas veganas?",
                "Receta de glaseado vegano",
                "Cantidades para 100 donas",
                "Conservantes naturales"
            },
            "sano-market" => new List<string>
            {
                "Productos con certificación orgánica",
                "Información nutricional detallada",
                "Beneficios de salud",
                "Productos sin gluten"
            },
            _ => new List<string>
            {
                "Ver catálogo completo",
                "Productos más vendidos",
                "Hacer pedido",
                "Contactar a Bernard"
            }
        };
    }

    private List<string> GetOrdersSuggestions(ChatContext context)
    {
        return new List<string>
        {
            "Análisis de pedidos del mes",
            "Productos con baja rotación",
            "Clientes más activos",
            "Predicción de demanda",
            "Optimizar inventario"
        };
    }

    private List<string> GetAdminSuggestions(ChatContext context)
    {
        return new List<string>
        {
            "Reporte financiero mensual",
            "Análisis de rentabilidad",
            "Estrategia de crecimiento",
            "Optimización operativa",
            "Insights de mercado vegano"
        };
    }

    private List<string> GetDefaultSuggestions()
    {
        return new List<string>
        {
            "Información sobre VHouse",
            "Productos veganos disponibles",
            "Cómo hacer un pedido",
            "Contactar soporte"
        };
    }
}