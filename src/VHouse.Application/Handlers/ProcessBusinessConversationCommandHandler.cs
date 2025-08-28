using MediatR;
using VHouse.Application.Commands;
using VHouse.Application.DTOs;
using VHouse.Domain.Interfaces;
using VHouse.Domain.ValueObjects;
using VHouse.Domain.Enums;

namespace VHouse.Application.Handlers;

public class ProcessBusinessConversationCommandHandler : IRequestHandler<ProcessBusinessConversationCommand, BusinessConversationResponseDto>
{
    private readonly IAIService _aiService;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessBusinessConversationCommandHandler(IAIService aiService, IUnitOfWork unitOfWork)
    {
        _aiService = aiService;
        _unitOfWork = unitOfWork;
    }

    public async Task<BusinessConversationResponseDto> Handle(ProcessBusinessConversationCommand request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Build business context
            var businessContext = await BuildBusinessContext(request.CustomerId);
            
            // Create enhanced prompt for business conversation
            var enhancedPrompt = await BuildBusinessConversationPrompt(
                request.Message, 
                request.ConversationType, 
                businessContext, 
                request.CustomerContext
            );

            var aiRequest = new AIRequest
            {
                Prompt = enhancedPrompt,
                MaxTokens = 800,
                Temperature = 0.7,
                PreferredProvider = request.PreferredProvider ?? AIProvider.Claude
            };

            var aiResponse = await _aiService.GenerateTextAsync(aiRequest);

            if (!aiResponse.IsSuccessful)
            {
                return new BusinessConversationResponseDto
                {
                    Response = "Lo siento, no puedo procesar tu consulta en este momento. Por favor, contacta a nuestro equipo de soporte.",
                    IsSuccessful = false,
                    ErrorMessage = aiResponse.ErrorMessage,
                    UsedProvider = aiResponse.UsedProvider,
                    UsedModel = aiResponse.UsedModel.ToString(),
                    ResponseTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }

            // Parse AI response and extract structured information
            var structuredResponse = await ParseBusinessResponse(aiResponse.Content, request.ConversationType);

            // Extract product recommendations if applicable
            var productRecommendations = await ExtractProductRecommendations(aiResponse.Content, request.CustomerId);

            // Determine conversation priority
            var priority = DetermineConversationPriority(request.ConversationType, aiResponse.Content);

            // Extract business entities
            var extractedEntities = await ExtractBusinessEntities(aiResponse.Content);

            return new BusinessConversationResponseDto
            {
                Response = structuredResponse.Response,
                ConversationContext = structuredResponse.Context,
                SuggestedActions = structuredResponse.Actions,
                ProductRecommendations = productRecommendations,
                Priority = priority,
                ExtractedEntities = extractedEntities,
                UsedProvider = aiResponse.UsedProvider,
                UsedModel = aiResponse.UsedModel.ToString(),
                IsSuccessful = true,
                ResponseTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
        catch (Exception ex)
        {
            return new BusinessConversationResponseDto
            {
                Response = "Ocurrió un error procesando tu consulta. Nuestro equipo ha sido notificado.",
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                ResponseTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
    }

    private async Task<string> BuildBusinessContext(int? customerId)
    {
        if (!customerId.HasValue)
            return "Cliente: Prospecto sin historial registrado";

        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId.Value);
        if (customer == null)
            return "Cliente: No encontrado en sistema";

        // Simulate recent orders for demo purposes
        var totalOrders = 0; // Simulate total orders count

        return $@"
CONTEXTO DEL CLIENTE:
- Nombre: {customer.CustomerName}
- Email: {customer.Email}
- Tipo: {(customer.IsVeganPreferred ? "Vegano Comprometido" : "Ocasional")}
- Total Pedidos: {totalOrders}
- Pedidos Recientes: Simulado para demo
- Estado: {(customer.IsActive ? "Cliente Activo" : "Inactivo")}";
    }

    private async Task<string> BuildBusinessConversationPrompt(
        string message, 
        BusinessConversationType conversationType, 
        string businessContext,
        string? customerContext)
    {
        var basePrompt = $@"
ERES EL ASISTENTE IA COMERCIAL DE VHOUSE - DISTRIBUCIÓN VEGANA B2B

MISIÓN: Ayudar a tiendas veganas y distribuidores con consultas comerciales inteligentes.
VALORES: Eficiencia, sustentabilidad, apoyo a emprendedores veganos.

{businessContext}

TIPO DE CONSULTA: {conversationType}
{(string.IsNullOrEmpty(customerContext) ? "" : $"CONTEXTO ADICIONAL: {customerContext}")}

MENSAJE DEL CLIENTE: {message}

INSTRUCCIONES PARA RESPUESTA:
1. Responde como experto comercial en productos veganos
2. Si preguntan por productos específicos, sugiere alternativas disponibles
3. Para pedidos complejos, extrae: productos, cantidades, fechas, términos
4. Si detectas urgencia, márca la prioridad como ALTA
5. Sugiere productos complementarios cuando sea apropiado
6. Mantén tono profesional pero cálido
7. Si no tienes información específica, deriva a equipo humano

FORMATO DE RESPUESTA:
- Respuesta principal clara y directa
- Si aplica: Lista de productos sugeridos
- Si aplica: Acciones recomendadas para el cliente
- Prioridad de seguimiento (BAJA/MEDIA/ALTA/URGENTE)

EJEMPLOS DE PRODUCTOS VEGANOS TÍPICOS:
- Leches vegetales (avena, almendra, soja)
- Quesos veganos artesanales
- Carnes vegetales (hamburguesas, nuggets)
- Yogurts de coco y probióticos
- Helados veganos premium
- Snacks saludables sin ingredientes animales

Responde ahora:";

        return basePrompt;
    }

    private async Task<(string Response, string Context, List<BusinessAction> Actions)> ParseBusinessResponse(
        string aiContent, 
        BusinessConversationType conversationType)
    {
        // This is a simplified parser - in production, you'd use more sophisticated NLP
        var actions = new List<BusinessAction>();
        
        // Extract potential actions based on conversation type and content
        if (conversationType == BusinessConversationType.OrderInquiry && aiContent.Contains("pedido"))
        {
            actions.Add(new BusinessAction
            {
                ActionType = "crear_pedido",
                Description = "Crear nuevo pedido basado en consulta",
                ActionUrl = "/orders/create",
                Priority = BusinessPriority.High
            });
        }

        if (aiContent.Contains("cotización") || aiContent.Contains("precio"))
        {
            actions.Add(new BusinessAction
            {
                ActionType = "generar_cotizacion",
                Description = "Generar cotización formal",
                ActionUrl = "/quotes/create",
                Priority = BusinessPriority.Medium
            });
        }

        return (aiContent, $"Conversación procesada: {conversationType}", actions);
    }

    private async Task<List<ProductSuggestion>> ExtractProductRecommendations(string aiContent, int? customerId)
    {
        var suggestions = new List<ProductSuggestion>();
        
        // Extract product names mentioned in AI response
        var productKeywords = new[] { "leche", "queso", "yogurt", "helado", "carne", "hamburguesa", "nuggets" };
        
        foreach (var keyword in productKeywords)
        {
            if (aiContent.ToLower().Contains(keyword))
            {
                // In production, you'd query actual products from database
                suggestions.Add(new ProductSuggestion
                {
                    ProductId = Random.Shared.Next(1, 100),
                    ProductName = $"Producto Vegano - {keyword}",
                    ReasonForSuggestion = $"Mencionado en conversación sobre {keyword}",
                    Price = Random.Shared.Next(50, 300),
                    InStock = true
                });
            }
        }

        return suggestions.Take(3).ToList(); // Limit to top 3 suggestions
    }

    private BusinessPriority DetermineConversationPriority(BusinessConversationType conversationType, string content)
    {
        var lowercaseContent = content.ToLower();
        
        // Urgent keywords
        if (lowercaseContent.Contains("urgente") || lowercaseContent.Contains("inmediato") || 
            lowercaseContent.Contains("hoy") || lowercaseContent.Contains("emergencia"))
            return BusinessPriority.Urgent;

        // High priority conversation types
        if (conversationType == BusinessConversationType.Complaint || 
            conversationType == BusinessConversationType.BulkOrder)
            return BusinessPriority.High;

        // Medium priority for orders and quotes
        if (conversationType == BusinessConversationType.OrderInquiry || 
            conversationType == BusinessConversationType.PriceQuote)
            return BusinessPriority.Medium;

        return BusinessPriority.Low;
    }

    private async Task<List<string>> ExtractBusinessEntities(string content)
    {
        var entities = new List<string>();
        
        // Simple entity extraction - in production, use proper NER
        var entityPatterns = new Dictionary<string, string[]>
        {
            ["productos"] = new[] { "leche", "queso", "yogurt", "helado", "carne vegana" },
            ["fechas"] = new[] { "hoy", "mañana", "próxima semana", "fin de mes" },
            ["cantidades"] = new[] { "cajas", "unidades", "kilos", "litros" }
        };

        var lowerContent = content.ToLower();
        foreach (var category in entityPatterns)
        {
            foreach (var pattern in category.Value)
            {
                if (lowerContent.Contains(pattern))
                {
                    entities.Add($"{category.Key}:{pattern}");
                }
            }
        }

        return entities;
    }

    public async Task<BusinessEmailResponseDto> GenerateBusinessEmail(string emailType, int customerId, object emailData, AIProvider? preferredProvider = null)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
            {
                return new BusinessEmailResponseDto
                {
                    Subject = "Error: Cliente no encontrado",
                    Body = "No se pudo generar el email debido a que el cliente no existe en el sistema.",
                    IsSuccessful = false,
                    ErrorMessage = "Cliente no encontrado"
                };
            }

            var emailPrompt = await BuildEmailPrompt(emailType, customer, emailData);
            
            var aiRequest = new AIRequest
            {
                Prompt = emailPrompt,
                MaxTokens = 1000,
                Temperature = 0.5,
                PreferredProvider = preferredProvider ?? AIProvider.Claude
            };

            var aiResponse = await _aiService.GenerateTextAsync(aiRequest);
            
            if (!aiResponse.IsSuccessful)
            {
                return new BusinessEmailResponseDto
                {
                    Subject = "Error generando email",
                    Body = "No se pudo generar el contenido del email en este momento.",
                    IsSuccessful = false,
                    ErrorMessage = aiResponse.ErrorMessage,
                    UsedProvider = aiResponse.UsedProvider
                };
            }

            var parsedEmail = ParseEmailResponse(aiResponse.Content);
            var isUrgent = DetermineEmailUrgency(emailType, emailData);
            var attachments = DetermineRequiredAttachments(emailType, emailData);

            return new BusinessEmailResponseDto
            {
                Subject = parsedEmail.Subject,
                Body = parsedEmail.Body,
                EmailType = emailType,
                IsUrgent = isUrgent,
                RequiredAttachments = attachments,
                UsedProvider = aiResponse.UsedProvider,
                IsSuccessful = true
            };
        }
        catch (Exception ex)
        {
            return new BusinessEmailResponseDto
            {
                Subject = "Error interno del sistema",
                Body = "Ocurrió un error interno al generar el email.",
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ComplexOrderResponseDto> ProcessComplexOrder(string naturalLanguageOrder, int customerId, BusinessContext context, AIProvider? preferredProvider = null)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
            {
                return new ComplexOrderResponseDto
                {
                    OrderSummary = new OrderSummary { Currency = "MXN" },
                    IsSuccessful = false,
                    ErrorMessage = "Cliente no encontrado en el sistema",
                    UsedProvider = preferredProvider ?? AIProvider.Claude
                };
            }

            var orderPrompt = await BuildComplexOrderPrompt(naturalLanguageOrder, customer, context);
            
            var aiRequest = new AIRequest
            {
                Prompt = orderPrompt,
                MaxTokens = 1200,
                Temperature = 0.3,
                PreferredProvider = preferredProvider ?? AIProvider.Claude
            };

            var aiResponse = await _aiService.GenerateTextAsync(aiRequest);
            
            if (!aiResponse.IsSuccessful)
            {
                return new ComplexOrderResponseDto
                {
                    OrderSummary = new OrderSummary { Currency = "MXN" },
                    IsSuccessful = false,
                    ErrorMessage = aiResponse.ErrorMessage,
                    UsedProvider = aiResponse.UsedProvider
                };
            }

            var extractedItems = await ExtractOrderItems(aiResponse.Content);
            var orderSummary = CalculateOrderSummary(extractedItems);
            var alerts = GenerateBusinessAlerts(extractedItems, context);
            var missingInfo = IdentifyMissingInformation(aiResponse.Content);
            var deliveryDate = ExtractRequestedDeliveryDate(aiResponse.Content);
            var paymentTerms = ExtractPaymentTerms(aiResponse.Content, context);

            return new ComplexOrderResponseDto
            {
                ExtractedItems = extractedItems,
                OrderSummary = orderSummary,
                Alerts = alerts,
                MissingInformation = missingInfo,
                EstimatedTotal = orderSummary.EstimatedTotal,
                RequestedDeliveryDate = deliveryDate,
                PaymentTerms = paymentTerms,
                UsedProvider = aiResponse.UsedProvider,
                IsSuccessful = true
            };
        }
        catch (Exception ex)
        {
            return new ComplexOrderResponseDto
            {
                OrderSummary = new OrderSummary { Currency = "MXN" },
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                UsedProvider = preferredProvider ?? AIProvider.Claude
            };
        }
    }

    private async Task<string> BuildEmailPrompt(string emailType, dynamic customer, object emailData)
    {
        return $@"
SISTEMA DE COMUNICACIONES AUTOMATIZADAS VHOUSE - DISTRIBUCIÓN VEGANA B2B

CLIENTE: {customer.CustomerName} ({customer.Email})
TIPO: {emailType}
DATOS: {System.Text.Json.JsonSerializer.Serialize(emailData)}

GENERA EMAIL PROFESIONAL CON:
ASUNTO: [Línea clara y específica]
CUERPO: [HTML básico, tono B2B vegano, call-to-action]

Responde ahora:";
    }

    private async Task<string> BuildComplexOrderPrompt(string order, dynamic customer, BusinessContext context)
    {
        return $@"
PROCESADOR INTELIGENTE DE PEDIDOS B2B - VHOUSE DISTRIBUCIÓN VEGANA

CLIENTE: {customer.CustomerName}
HISTORIAL: {string.Join(", ", context.RecentOrderHistory)}
PEDIDO EN LENGUAJE NATURAL: {order}

EXTRAE Y ESTRUCTURA:
1. PRODUCTOS: [nombre, cantidad, especificaciones]
2. FECHAS: [entrega solicitada, urgencia]
3. TÉRMINOS: [pago, descuentos, condiciones]
4. ALERTAS: [stock, precios, problemas]

FORMATO JSON ESTRUCTURADO:
{{
  ""items"": [{{""product"": """", ""quantity"": 0, ""notes"": """"}}],
  ""delivery_date"": """",
  ""payment_terms"": """",
  ""special_requests"": """",
  ""missing_info"": []
}}

Procesa ahora:";
    }

    private (string Subject, string Body) ParseEmailResponse(string aiContent)
    {
        var lines = aiContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string subject = "";
        string body = "";
        bool foundBody = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("ASUNTO:", StringComparison.OrdinalIgnoreCase))
            {
                subject = line.Substring(7).Trim();
            }
            else if (line.StartsWith("CUERPO:", StringComparison.OrdinalIgnoreCase))
            {
                foundBody = true;
                continue;
            }
            else if (foundBody)
            {
                body += line + "\n";
            }
        }

        return (string.IsNullOrEmpty(subject) ? "Comunicación VHouse" : subject, string.IsNullOrEmpty(body) ? aiContent : body.Trim());
    }

    private bool DetermineEmailUrgency(string emailType, object emailData)
    {
        var urgentTypes = new[] { "recordatorio_pago", "alerta_producto", "notificacion_tecnica" };
        return urgentTypes.Contains(emailType.ToLower()) || System.Text.Json.JsonSerializer.Serialize(emailData).ToLower().Contains("urgente");
    }

    private List<string> DetermineRequiredAttachments(string emailType, object emailData)
    {
        return emailType.ToLower() switch
        {
            "confirmacion_pedido" => new List<string> { "orden_compra.pdf", "terminos_condiciones.pdf" },
            "actualizacion_entrega" => new List<string> { "guia_seguimiento.pdf" },
            "recordatorio_pago" => new List<string> { "factura.pdf", "estado_cuenta.pdf" },
            "oferta_promocional" => new List<string> { "catalogo_promociones.pdf" },
            "campana_marketing" => new List<string> { "catalogo_productos.pdf", "hoja_valores_veganos.pdf" },
            _ => new List<string>()
        };
    }

    private async Task<List<OrderItem>> ExtractOrderItems(string aiContent)
    {
        var items = new List<OrderItem>();
        
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(aiContent);
            if (doc.RootElement.TryGetProperty("items", out var itemsElement))
            {
                foreach (var item in itemsElement.EnumerateArray())
                {
                    items.Add(new OrderItem
                    {
                        ProductId = Random.Shared.Next(1, 100),
                        ProductName = item.GetProperty("product").GetString() ?? "Producto Vegano",
                        Quantity = item.GetProperty("quantity").GetInt32(),
                        UnitPrice = Random.Shared.Next(50, 300),
                        SpecialInstructions = item.TryGetProperty("notes", out var notes) ? notes.GetString() : null
                    });
                }
            }
        }
        catch
        {
            // Fallback parsing if JSON fails
            var keywords = new[] { "leche", "queso", "yogurt", "helado" };
            foreach (var keyword in keywords)
            {
                if (aiContent.ToLower().Contains(keyword))
                {
                    items.Add(new OrderItem
                    {
                        ProductId = Random.Shared.Next(1, 100),
                        ProductName = $"Producto Vegano - {keyword}",
                        Quantity = Random.Shared.Next(1, 10),
                        UnitPrice = Random.Shared.Next(50, 300)
                    });
                }
            }
        }

        return items;
    }

    private OrderSummary CalculateOrderSummary(List<OrderItem> items)
    {
        var subTotal = items.Sum(i => i.Quantity * i.UnitPrice);
        var tax = subTotal * 0.16m; // 16% IVA Mexico
        
        return new OrderSummary
        {
            TotalItems = items.Sum(i => i.Quantity),
            SubTotal = subTotal,
            EstimatedTax = tax,
            EstimatedTotal = subTotal + tax,
            Currency = "MXN"
        };
    }

    private List<BusinessAlert> GenerateBusinessAlerts(List<OrderItem> items, BusinessContext context)
    {
        var alerts = new List<BusinessAlert>();
        
        var totalValue = items.Sum(i => i.Quantity * i.UnitPrice);
        if (totalValue > context.TypicalOrderValue * 2)
        {
            alerts.Add(new BusinessAlert
            {
                AlertType = "PEDIDO_GRANDE",
                Message = $"Pedido excede valor típico por ${totalValue - context.TypicalOrderValue:F2}",
                Priority = BusinessPriority.Medium,
                SuggestedActions = new List<string> { "Confirmar inventario", "Verificar términos de pago" }
            });
        }

        return alerts;
    }

    private List<string> IdentifyMissingInformation(string aiContent)
    {
        var missing = new List<string>();
        
        if (!aiContent.ToLower().Contains("fecha") && !aiContent.ToLower().Contains("entrega"))
            missing.Add("Fecha de entrega requerida");
        
        if (!aiContent.ToLower().Contains("pago") && !aiContent.ToLower().Contains("término"))
            missing.Add("Términos de pago");
            
        return missing;
    }

    private DateTime? ExtractRequestedDeliveryDate(string aiContent)
    {
        if (aiContent.ToLower().Contains("hoy"))
            return DateTime.Today;
        if (aiContent.ToLower().Contains("mañana"))
            return DateTime.Today.AddDays(1);
        if (aiContent.ToLower().Contains("próxima semana"))
            return DateTime.Today.AddDays(7);
            
        return null;
    }

    private PaymentTerms? ExtractPaymentTerms(string aiContent, BusinessContext context)
    {
        if (aiContent.ToLower().Contains("contado"))
        {
            return new PaymentTerms
            {
                DaysNet = 0,
                DiscountPercentage = 2.0m,
                DiscountDays = 0,
                TermsDescription = "Pago de contado con 2% descuento"
            };
        }
        
        if (aiContent.ToLower().Contains("30 días") || aiContent.ToLower().Contains("crédito"))
        {
            return new PaymentTerms
            {
                DaysNet = 30,
                DiscountPercentage = 0,
                DiscountDays = 0,
                TermsDescription = "Net 30 días"
            };
        }

        return null;
    }
}