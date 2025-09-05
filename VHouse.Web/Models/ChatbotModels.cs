// Creado por Bernard Uriza Orozco
using VHouse.Domain.Enums;
using VHouse.Application.DTOs;

namespace VHouse.Web.Models;

public class QuickSuggestion
{
    public string Icon { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class UniversalChatMessage
{
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Icon => IsUser ? "👤" : "🤖";
    public string CssClass => IsUser ? "user-message" : "ai-message";
}

public class ChatbotConfiguration
{
    public string Title { get; set; } = "Asistente IA";
    public string Subtitle { get; set; } = "VHouse";
    public string HeaderIcon { get; set; } = "🤖";
    public string WelcomeMessage { get; set; } = "¡Hola! ¿En qué puedo ayudarte?";
    public string PlaceholderText { get; set; } = "Escribe tu mensaje...";
    public List<QuickSuggestion> QuickSuggestions { get; set; } = new();
    public BusinessConversationType ConversationType { get; set; } = BusinessConversationType.General;
    public int? CustomerId { get; set; }
    public Func<string> ContextBuilder { get; set; } = () => string.Empty;
}

public static class ChatbotConfigurations
{
    public static ChatbotConfiguration ForPOS(List<object> products, Dictionary<int, int> selectedProducts, string clientName)
    {
        return new ChatbotConfiguration
        {
            Title = "Asistente IA Vegano",
            Subtitle = $"Para {clientName}",
            HeaderIcon = "🌱",
            WelcomeMessage = "¡Hola! Soy tu asistente IA especializado en productos veganos. Puedo ayudarte con recomendaciones y gestión de pedidos.",
            PlaceholderText = "Pregúntame sobre productos veganos, recomendaciones, pedidos...",
            QuickSuggestions = new List<QuickSuggestion>
            {
                new() { Icon = "📋", Label = "Ver catálogo", Message = "¿Qué productos tienes disponibles?" },
                new() { Icon = "🏪", Label = "Para mi negocio", Message = "Necesito productos para mi tienda vegana" },
                new() { Icon = "💰", Label = "Mejores precios", Message = "¿Cuáles productos tienen mejor precio?" }
            },
            ConversationType = BusinessConversationType.ProductInquiry
        };
    }
    
    public static ChatbotConfiguration ForReports(List<object> orders)
    {
        return new ChatbotConfiguration
        {
            Title = "Analista IA de Pedidos",
            Subtitle = "Reportes con datos reales",
            HeaderIcon = "📊",
            WelcomeMessage = "¡Hola! Soy tu analista IA especializado en pedidos. Puedo crear reportes personalizados con tus datos reales.",
            PlaceholderText = "Pregúntame sobre ventas, clientes, productos, tendencias...",
            QuickSuggestions = new List<QuickSuggestion>
            {
                new() { Icon = "🍩", Label = "Ventas Mona la Dona", Message = "¿Cuáles son las ventas de Mona la Dona este mes?" },
                new() { Icon = "📊", Label = "Comparar clientes", Message = "Compara las ventas entre Sano Market y La Papelería" },
                new() { Icon = "⭐", Label = "Top productos", Message = "¿Cuáles son los productos más vendidos?" },
                new() { Icon = "📈", Label = "Tendencias diarias", Message = "Análisis de tendencias de ventas por día" }
            },
            ConversationType = BusinessConversationType.General
        };
    }
    
    public static ChatbotConfiguration ForClientPortal(string clientName, int clientId)
    {
        return new ChatbotConfiguration
        {
            Title = "Asistente Comercial",
            Subtitle = $"Para {clientName}",
            HeaderIcon = "💼",
            WelcomeMessage = $"¡Hola {clientName}! Soy tu asistente comercial. Puedo ayudarte con pedidos, productos y consultas comerciales.",
            PlaceholderText = "Pregúntame sobre productos, precios, pedidos...",
            QuickSuggestions = new List<QuickSuggestion>
            {
                new() { Icon = "🛒", Label = "Hacer pedido", Message = "Quiero hacer un pedido personalizado" },
                new() { Icon = "💳", Label = "Ver precios", Message = "¿Cuáles son mis precios especiales?" },
                new() { Icon = "📦", Label = "Estado pedidos", Message = "¿Cuál es el estado de mis pedidos?" },
                new() { Icon = "🎯", Label = "Recomendaciones", Message = "¿Qué productos me recomiendas para mi negocio?" }
            },
            ConversationType = BusinessConversationType.OrderInquiry,
            CustomerId = clientId
        };
    }
}