using Microsoft.Extensions.Logging;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;
using VHouse.Domain.ValueObjects;
using VHouse.Domain.Enums;
using System.Collections.Concurrent;
using System.Text.Json;

namespace VHouse.Application.Services;

/// <summary>
/// Servicio de chatbot inteligente que combina detección de contexto con AI
/// para proporcionar respuestas personalizadas y acciones contextuales
/// </summary>
public class IntelligentChatbotService : IIntelligentChatbotService
{
    private readonly IChatContextService _contextService;
    private readonly IAIService _aiService;
    private readonly ILogger<IntelligentChatbotService> _logger;
    
    // Cache de sesiones activas en memoria
    private static readonly ConcurrentDictionary<string, ChatSession> _activeSessions = new();

    public IntelligentChatbotService(
        IChatContextService contextService,
        IAIService aiService,
        ILogger<IntelligentChatbotService> logger)
    {
        _contextService = contextService;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<ChatbotResponse> ProcessMessageAsync(ChatbotRequest request)
    {
        try
        {
            _logger.LogInformation("Procesando mensaje para sesión {SessionId}: {Message}", 
                request.SessionId, request.UserMessage);

            // Obtener o crear sesión
            var session = await GetOrCreateSessionAsync(request.SessionId, request.CurrentUrl, request.ContextData);
            
            // Actualizar contexto si es necesario
            if (request.CurrentUrl != session.SessionData.GetValueOrDefault("lastUrl", "").ToString())
            {
                session = await UpdateSessionContextAsync(session.SessionId, request.CurrentUrl, request.ContextData);
            }

            // Analizar intención del usuario
            var intentAnalysis = await AnalyzeUserIntentAsync(request.UserMessage, session.Context);
            
            // Procesar comandos especiales si existen
            if (IsSpecialCommand(request.UserMessage))
            {
                return await ProcessSpecialCommandAsync(request.UserMessage, session);
            }

            // Generar respuesta usando AI con contexto
            var aiResponse = await GenerateContextualAIResponseAsync(request.UserMessage, session);
            
            // Agregar mensaje del usuario a la sesión
            session.Messages.Add(new ChatMessage
            {
                Content = request.UserMessage,
                IsUser = true,
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["intent"] = intentAnalysis.PrimaryIntent,
                    ["confidence"] = intentAnalysis.Confidence
                }
            });

            // Agregar respuesta del bot a la sesión
            session.Messages.Add(new ChatMessage
            {
                Content = aiResponse.Content,
                IsUser = false,
                Timestamp = DateTime.UtcNow
            });

            // Actualizar actividad de la sesión
            session.LastActivity = DateTime.UtcNow;

            // Generar sugerencias contextuales
            var suggestions = await GetContextualSuggestionsAsync(session.SessionId, request.UserMessage);

            var response = new ChatbotResponse
            {
                Content = aiResponse.Content,
                IsSuccessful = aiResponse.IsSuccessful,
                Suggestions = suggestions,
                RecommendedActions = intentAnalysis.SuggestedActions,
                UsedContext = session.Context,
                Metadata = new Dictionary<string, object>
                {
                    ["sessionId"] = session.SessionId,
                    ["messageCount"] = session.Messages.Count,
                    ["intent"] = intentAnalysis.PrimaryIntent,
                    ["usedProvider"] = aiResponse.UsedProvider.ToString()
                }
            };

            if (!aiResponse.IsSuccessful)
            {
                response.ErrorMessage = aiResponse.ErrorMessage;
            }

            _logger.LogInformation("Respuesta generada exitosamente para sesión {SessionId}", request.SessionId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando mensaje para sesión {SessionId}", request.SessionId);
            
            return new ChatbotResponse
            {
                Content = "🌱 Disculpa, tengo un problema técnico momentáneo. ¿Puedo ayudarte con algo más específico sobre nuestros productos veganos?",
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                Suggestions = _contextService.GetContextualSuggestions(new ChatContext())
            };
        }
    }

    public async Task<ChatSession> InitializeChatSessionAsync(string currentUrl, object? contextData = null)
    {
        var sessionId = Guid.NewGuid().ToString();
        _logger.LogInformation("Inicializando nueva sesión de chat: {SessionId} para URL: {Url}", sessionId, currentUrl);

        var context = await _contextService.GetContextAsync(currentUrl, contextData);
        
        var session = new ChatSession
        {
            SessionId = sessionId,
            Context = context,
            CreatedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow,
            SessionData = new Dictionary<string, object>
            {
                ["lastUrl"] = currentUrl,
                ["initialContext"] = context.PageType
            }
        };

        // Mensaje de bienvenida contextual
        var welcomeMessage = GenerateWelcomeMessage(context);
        session.Messages.Add(new ChatMessage
        {
            Content = welcomeMessage,
            IsUser = false,
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { ["type"] = "welcome" }
        });

        _activeSessions.TryAdd(sessionId, session);
        
        return session;
    }

    public async Task<ChatSession> UpdateSessionContextAsync(string sessionId, string currentUrl, object? contextData = null)
    {
        _logger.LogInformation("Actualizando contexto para sesión {SessionId} con URL: {Url}", sessionId, currentUrl);

        if (_activeSessions.TryGetValue(sessionId, out var session))
        {
            var newContext = await _contextService.GetContextAsync(currentUrl, contextData);
            session.Context = newContext;
            session.LastActivity = DateTime.UtcNow;
            session.SessionData["lastUrl"] = currentUrl;
            session.SessionData["contextUpdatedAt"] = DateTime.UtcNow;

            // Agregar mensaje informativo sobre cambio de contexto
            if (newContext.PageType != session.SessionData.GetValueOrDefault("initialContext", "").ToString())
            {
                var contextMessage = $"🔄 He detectado que cambiaste a: <strong>{newContext.PageTitle}</strong><br/>Mi enfoque ahora es: <em>{newContext.PrimaryFocus}</em>";
                session.Messages.Add(new ChatMessage
                {
                    Content = contextMessage,
                    IsUser = false,
                    Timestamp = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object> { ["type"] = "context-change" }
                });
            }
        }
        else
        {
            // Si no existe la sesión, crear una nueva
            session = await InitializeChatSessionAsync(currentUrl, contextData);
        }

        return session;
    }

    public List<string> GetContextualSuggestions(string sessionId, string? lastMessage = null)
    {
        if (_activeSessions.TryGetValue(sessionId, out var session))
        {
            var baseSuggestions = _contextService.GetContextualSuggestions(session.Context);
            
            // Personalizar sugerencias basado en el último mensaje
            if (!string.IsNullOrEmpty(lastMessage))
            {
                var customSuggestions = GenerateCustomSuggestions(lastMessage, session.Context);
                return customSuggestions.Any() ? customSuggestions : baseSuggestions;
            }
            
            return baseSuggestions;
        }

        return _contextService.GetContextualSuggestions(new ChatContext());
    }

    public ChatbotResponse ProcessSpecialCommand(string command, ChatSession session)
    {
        _logger.LogInformation("Procesando comando especial: {Command} para sesión {SessionId}", command, session.SessionId);

        var lowerCommand = command.ToLowerInvariant().Trim();
        
        return lowerCommand switch
        {
            "/productos" or "/catalog" => ProcessProductsCommand(session),
            "/pedido" or "/order" => ProcessOrderCommand(session),
            "/precios" or "/prices" => ProcessPricesCommand(session),
            "/help" or "/ayuda" => ProcessHelpCommand(session),
            "/stats" or "/estadisticas" => ProcessStatsCommand(session),
            "/clear" or "/limpiar" => ProcessClearCommand(session),
            _ => new ChatbotResponse
            {
                Content = $"🤔 No reconozco el comando <strong>{command}</strong><br/><br/>Comandos disponibles:<br/>• /productos - Ver catálogo<br/>• /pedido - Crear pedido<br/>• /precios - Consultar precios<br/>• /help - Mostrar ayuda<br/>• /clear - Limpiar chat",
                IsSuccessful = true,
                Suggestions = _contextService.GetContextualSuggestions(session.Context)
            }
        };
    }

    public async Task<UserIntentAnalysis> AnalyzeUserIntentAsync(string message, ChatContext context)
    {
        try
        {
            var analysisPrompt = $@"Analiza la siguiente consulta de usuario y determina su intención principal.

MENSAJE DEL USUARIO: {message}
CONTEXTO: {context.PageType} - {context.PrimaryFocus}
PRODUCTOS DISPONIBLES: {context.AvailableProducts.Count}

Identifica:
1. Intención principal (product-inquiry, order-request, price-inquiry, stock-check, etc.)
2. Entidades mencionadas (productos, cantidades, fechas)
3. Nivel de confianza (0.0 a 1.0)

Responde en formato: INTENT|ENTITIES|CONFIDENCE
Ejemplo: product-inquiry|leche de avena, 500ml|0.85";

            var aiRequest = new AIRequest
            {
                Prompt = analysisPrompt,
                SystemMessage = "Eres un analizador de intenciones para un sistema de distribución vegana.",
                PreferredProvider = AIProvider.Claude,
                MaxTokens = 150,
                Temperature = 0.3
            };

            var response = await _aiService.GenerateTextAsync(aiRequest);
            
            if (response.IsSuccessful)
            {
                return ParseIntentAnalysisResponse(response.Content, message);
            }
            else
            {
                return CreateFallbackIntentAnalysis(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analizando intención del usuario, usando fallback");
            return CreateFallbackIntentAnalysis(message);
        }
    }

    // Métodos privados de apoyo
    private async Task<ChatSession> GetOrCreateSessionAsync(string sessionId, string currentUrl, object? contextData)
    {
        if (_activeSessions.TryGetValue(sessionId, out var existingSession))
        {
            return existingSession;
        }

        return await InitializeChatSessionAsync(currentUrl, contextData);
    }

    private bool IsSpecialCommand(string message)
    {
        return message.TrimStart().StartsWith('/');
    }

    private async Task<AIResponse> GenerateContextualAIResponseAsync(string userMessage, ChatSession session)
    {
        var systemMessage = _contextService.GenerateSystemMessage(session.Context);
        
        // Enriquecer el mensaje del sistema con datos reales si están disponibles
        if (session.Context.AdditionalData.ContainsKey("totalOrders"))
        {
            var totalOrders = session.Context.AdditionalData["totalOrders"];
            var totalRevenue = session.Context.AdditionalData.ContainsKey("totalRevenue") ? 
                session.Context.AdditionalData["totalRevenue"] : 0;
                
            systemMessage += $"\n\nDATOS REALES DEL SISTEMA:\n";
            systemMessage += $"- Total de pedidos activos: {totalOrders}\n";
            systemMessage += $"- Ingresos totales: ${totalRevenue:F2}\n";
            
            if (session.Context.AdditionalData.ContainsKey("lastOrder") && 
                session.Context.AdditionalData["lastOrder"] != null)
            {
                systemMessage += "- Hay pedidos recientes en el sistema\n";
            }
            
            systemMessage += "\nUSA ESTOS DATOS REALES EN TUS RESPUESTAS. Ayuda a Bernard con análisis de su negocio vegano.";
        }
        
        // Incluir historial reciente de la conversación para mejor contexto
        var conversationHistory = GetRecentConversationHistory(session);
        var enhancedPrompt = $"{conversationHistory}\n\nUsuario: {userMessage}";

        var aiRequest = new AIRequest
        {
            Prompt = enhancedPrompt,
            SystemMessage = systemMessage,
            PreferredProvider = AIProvider.Claude,
            MaxTokens = 600,
            Temperature = 0.7
        };

        return await _aiService.GenerateTextAsync(aiRequest);
    }

    private string GetRecentConversationHistory(ChatSession session)
    {
        var recentMessages = session.Messages
            .Where(m => !m.Metadata.ContainsKey("type") || m.Metadata["type"].ToString() != "welcome")
            .TakeLast(6) // Últimos 3 intercambios
            .ToList();

        if (!recentMessages.Any())
            return "";

        var history = "HISTORIAL RECIENTE:\n";
        foreach (var msg in recentMessages)
        {
            var speaker = msg.IsUser ? "Usuario" : "Asistente";
            history += $"{speaker}: {msg.Content}\n";
        }
        
        return history;
    }

    private string GenerateWelcomeMessage(ChatContext context)
    {
        return context.PageType switch
        {
            PageTypes.ClientPortal when context.ClientTenant != null => 
                $"¡Hola {context.ClientTenant.BusinessName}! 👋<br/><br/>🌱 Soy tu asistente especializado en productos veganos. Tengo acceso a tu catálogo personalizado con <strong>{context.AvailableProducts.Count} productos</strong> disponibles.<br/><br/>Puedo ayudarte con:<br/>• Información detallada de productos<br/>• Recomendaciones para tu negocio<br/>• Crear pedidos personalizados<br/>• Consultar precios y disponibilidad<br/><br/>¿En qué puedo ayudarte hoy? 💚",
                
            PageTypes.Orders => 
                "📊 ¡Hola Bernard! Estoy aquí para ayudarte con la gestión de pedidos.<br/><br/>Puedo analizar tendencias, optimizar inventario, predecir demanda y generar insights que maximicen el impacto vegano de VHouse.<br/><br/>¿Qué análisis necesitas? 🚀",
                
            PageTypes.AdminDashboard => 
                "🎯 ¡Bienvenido al panel administrativo, Bernard!<br/><br/>Tengo acceso completo a las capacidades de análisis y optimización. Puedo ayudarte a escalar VHouse mientras mantienes la calidad y misión vegana.<br/><br/>¿Qué aspecto del negocio optimizamos hoy? 💡",
                
            _ => 
                "🌱 ¡Hola! Soy el asistente vegano de VHouse.<br/><br/>Estoy aquí para ayudarte con información sobre productos, pedidos y todo lo relacionado con nuestra misión de distribución vegana.<br/><br/>¿Cómo puedo ayudarte? 💚"
        };
    }

    private List<string> GenerateCustomSuggestions(string lastMessage, ChatContext context)
    {
        var lowerMessage = lastMessage.ToLowerInvariant();
        
        if (lowerMessage.Contains("dona"))
        {
            return new List<string>
            {
                "Ingredientes para 100 donas",
                "Receta de glaseado vegano",
                "Conservantes naturales",
                "Cálculo de costos"
            };
        }
        
        if (lowerMessage.Contains("leche"))
        {
            return new List<string>
            {
                "Comparar leches vegetales",
                "Usos en repostería",
                "Información nutricional",
                "Precios por volumen"
            };
        }
        
        if (lowerMessage.Contains("precio"))
        {
            return new List<string>
            {
                "Descuentos por volumen",
                "Comparar productos",
                "Costos de envío",
                "Formas de pago"
            };
        }

        return new List<string>();
    }

    private UserIntentAnalysis ParseIntentAnalysisResponse(string response, string originalMessage)
    {
        try
        {
            var parts = response.Split('|');
            if (parts.Length >= 3)
            {
                var intent = parts[0].Trim();
                var entities = parts[1].Split(',').Select(e => e.Trim()).ToList();
                var confidence = double.TryParse(parts[2].Trim(), out var conf) ? conf : 0.5;

                return new UserIntentAnalysis
                {
                    PrimaryIntent = intent,
                    ExtractedEntities = entities,
                    Confidence = confidence,
                    SuggestedActions = GenerateActionsForIntent(intent, entities)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parseando análisis de intención");
        }

        return CreateFallbackIntentAnalysis(originalMessage);
    }

    private UserIntentAnalysis CreateFallbackIntentAnalysis(string message)
    {
        var lowerMessage = message.ToLowerInvariant();
        
        string intent = IntentTypes.GeneralHelp;
        if (lowerMessage.Contains("producto") || lowerMessage.Contains("ingrediente"))
            intent = IntentTypes.ProductInquiry;
        else if (lowerMessage.Contains("pedido") || lowerMessage.Contains("comprar"))
            intent = IntentTypes.OrderRequest;
        else if (lowerMessage.Contains("precio") || lowerMessage.Contains("costo"))
            intent = IntentTypes.PriceInquiry;

        return new UserIntentAnalysis
        {
            PrimaryIntent = intent,
            ExtractedEntities = new List<string>(),
            Confidence = 0.6,
            SuggestedActions = GenerateActionsForIntent(intent, new List<string>())
        };
    }

    private List<ChatAction> GenerateActionsForIntent(string intent, List<string> entities)
    {
        return intent switch
        {
            IntentTypes.ProductInquiry => new List<ChatAction>
            {
                new ChatAction { ActionType = ActionTypes.ShowProducts, Label = "Ver Productos", Icon = "🛒" },
                new ChatAction { ActionType = ActionTypes.CheckStock, Label = "Verificar Stock", Icon = "📦" }
            },
            IntentTypes.OrderRequest => new List<ChatAction>
            {
                new ChatAction { ActionType = ActionTypes.CreateOrder, Label = "Crear Pedido", Icon = "🛍️" },
                new ChatAction { ActionType = ActionTypes.ShowPrices, Label = "Ver Precios", Icon = "💰" }
            },
            IntentTypes.PriceInquiry => new List<ChatAction>
            {
                new ChatAction { ActionType = ActionTypes.ShowPrices, Label = "Lista de Precios", Icon = "💲" }
            },
            _ => new List<ChatAction>
            {
                new ChatAction { ActionType = ActionTypes.ShowProducts, Label = "Ver Catálogo", Icon = "📋" }
            }
        };
    }

    // Comandos especiales
    private ChatbotResponse ProcessProductsCommand(ChatSession session)
    {
        var productList = string.Join("<br/>", 
            session.Context.AvailableProducts.Take(10).Select(p => 
                $"• {p.Emoji} <strong>{p.ProductName}</strong> - ${p.PricePublic:F2}<br/>  <em>{p.Description}</em>"));

        var content = $"🛒 <strong>Catálogo de Productos Disponibles</strong><br/><br/>{productList}";
        
        if (session.Context.AvailableProducts.Count > 10)
        {
            content += $"<br/><br/>... y {session.Context.AvailableProducts.Count - 10} productos más.";
        }

        return new ChatbotResponse
        {
            Content = content,
            IsSuccessful = true,
            Suggestions = new List<string> { "Ver más detalles", "Consultar precios", "Hacer pedido" }
        };
    }

    private ChatbotResponse ProcessOrderCommand(ChatSession session)
    {
        return new ChatbotResponse
        {
            Content = "🛍️ <strong>Crear Nuevo Pedido</strong><br/><br/>Para ayudarte con tu pedido, dime:<br/>• ¿Qué productos necesitas?<br/>• ¿Qué cantidades?<br/>• ¿Para qué fecha?<br/><br/>Por ejemplo: <em>\"Necesito 5kg de harina de avena y 10 litros de leche de almendra para el viernes\"</em>",
            IsSuccessful = true,
            Suggestions = new List<string> { "Ver productos disponibles", "Consultar precios", "Pedido recurrente" }
        };
    }

    private ChatbotResponse ProcessPricesCommand(ChatSession session)
    {
        var priceList = string.Join("<br/>", 
            session.Context.AvailableProducts.Take(8).Select(p => 
                $"• <strong>{p.ProductName}</strong>: ${p.PricePublic:F2}"));

        return new ChatbotResponse
        {
            Content = $"💰 <strong>Lista de Precios</strong><br/><br/>{priceList}<br/><br/><em>Precios especiales disponibles para pedidos grandes. ¡Pregúntame por descuentos!</em>",
            IsSuccessful = true,
            Suggestions = new List<string> { "Descuentos por volumen", "Comparar productos", "Crear pedido" }
        };
    }

    private ChatbotResponse ProcessHelpCommand(ChatSession session)
    {
        var availableCommands = @"🆘 <strong>Ayuda - VeganAI Assistant</strong><br/><br/>
<strong>Comandos disponibles:</strong><br/>
• <code>/productos</code> - Ver catálogo completo<br/>
• <code>/pedido</code> - Crear un nuevo pedido<br/>
• <code>/precios</code> - Consultar lista de precios<br/>
• <code>/stats</code> - Ver estadísticas<br/>
• <code>/clear</code> - Limpiar conversación<br/><br/>
<strong>También puedes preguntarme:</strong><br/>
• Información sobre productos específicos<br/>
• Recomendaciones para tu negocio<br/>
• Consejos de uso y recetas<br/>
• Disponibilidad y stock<br/><br/>
¡Solo escribe tu pregunta naturalmente! 💬";

        return new ChatbotResponse
        {
            Content = availableCommands,
            IsSuccessful = true,
            Suggestions = _contextService.GetContextualSuggestions(session.Context)
        };
    }

    private ChatbotResponse ProcessStatsCommand(ChatSession session)
    {
        // Obtener estadísticas reales si están disponibles en el contexto
        var ordersStats = "";
        if (session.Context.AdditionalData.ContainsKey("totalOrders"))
        {
            var totalOrders = session.Context.AdditionalData["totalOrders"];
            var totalRevenue = session.Context.AdditionalData.ContainsKey("totalRevenue") ? 
                session.Context.AdditionalData["totalRevenue"] : 0;
            var lastOrder = session.Context.AdditionalData.ContainsKey("lastOrder") ? 
                session.Context.AdditionalData["lastOrder"] : null;
                
            ordersStats = $@"<br/><strong>📦 Estadísticas de tu Negocio:</strong><br/>
• Total de pedidos: {totalOrders}<br/>
• Ingresos totales: ${totalRevenue:F2}<br/>
• Último pedido: {(lastOrder != null ? "Reciente" : "Sin pedidos recientes")}<br/>";
        }
        
        var stats = $@"📊 <strong>Dashboard de Bernard - VHouse</strong><br/><br/>
• Mensajes en sesión: {session.Messages.Count}<br/>
• Sesión iniciada: {session.CreatedAt:HH:mm}<br/>
• Contexto actual: {session.Context.PageTitle}<br/>
• Productos disponibles: {session.Context.AvailableProducts.Count}
{ordersStats}<br/>
🌱 <em>Tu negocio vegano creciendo día a día!</em>";

        return new ChatbotResponse
        {
            Content = stats,
            IsSuccessful = true,
            Suggestions = new List<string> { "Analizar ventas", "Ver clientes", "Revisar inventario" }
        };
    }

    private ChatbotResponse ProcessClearCommand(ChatSession session)
    {
        // Mantener solo mensaje de bienvenida
        var welcomeMessage = session.Messages.FirstOrDefault(m => 
            m.Metadata.ContainsKey("type") && m.Metadata["type"].ToString() == "welcome");
        
        session.Messages.Clear();
        if (welcomeMessage != null)
        {
            session.Messages.Add(welcomeMessage);
        }

        return new ChatbotResponse
        {
            Content = "🧹 <strong>Conversación limpiada</strong><br/><br/>He borrado nuestro historial de chat. ¡Empezamos de nuevo!<br/><br/>¿En qué puedo ayudarte hoy? 🌱",
            IsSuccessful = true,
            Suggestions = _contextService.GetContextualSuggestions(session.Context)
        };
    }
}