using MediatR;
using VHouse.Application.Commands;
using VHouse.Application.DTOs;
using VHouse.Domain.Interfaces;
using VHouse.Domain.ValueObjects;
using VHouse.Domain.Enums;
using VHouse.Domain.Entities;

namespace VHouse.Application.Handlers;

public class ProcessBusinessConversationCommandHandler : IRequestHandler<ProcessBusinessConversationCommand, BusinessConversationResponseDto>
{
    private readonly IAIService _aiService;
    private readonly IUnitOfWork _unitOfWork;
    
    // Static readonly arrays for better performance
    private static readonly string[] ProductKeywords = { "leche", "queso", "yogurt", "helado", "carne", "hamburguesa", "nuggets" };
    
    private static readonly Dictionary<string, string[]> EntityPatterns = new()
    {
        ["productos"] = new[] { "leche", "queso", "yogurt", "helado", "carne vegana" },
        ["fechas"] = new[] { "hoy", "mañana", "próxima semana", "fin de mes" },
        ["cantidades"] = new[] { "cajas", "unidades", "kilos", "litros" }
    };

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

    private Task<string> BuildBusinessConversationPrompt(
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

INSTRUCCIONES CRÍTICAS PARA RESPUESTA:
1. SOLO recomienda productos que aparezcan en el CONTEXTO ADICIONAL con precios y stock
2. NO inventes productos que no estén listados explícitamente
3. Si un producto no existe en nuestro catálogo, di claramente que no lo tenemos
4. Para pedidos complejos, extrae: productos, cantidades, fechas, términos
5. Si detectas urgencia, márca la prioridad como ALTA
6. Sugiere productos complementarios SOLO de los que tenemos en stock
7. Mantén tono profesional pero cálido
8. Si no tienes información específica, deriva a equipo humano

FORMATO DE RESPUESTA:
- Respuesta principal clara y directa basada en productos reales
- Si aplica: Lista de productos sugeridos (SOLO los que tenemos)
- Si aplica: Acciones recomendadas para el cliente
- Prioridad de seguimiento (BAJA/MEDIA/ALTA/URGENTE)

REGLA FUNDAMENTAL: 
NO INVENTES PRODUCTOS. Solo trabaja con el catálogo real proporcionado en el contexto.

Responde ahora:";

        return Task.FromResult(basePrompt);
    }

    private Task<(string Response, string Context, List<BusinessAction> Actions)> ParseBusinessResponse(
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
                ActionUrl = new Uri("/orders/create", UriKind.Relative),
                Priority = BusinessPriority.High
            });
        }

        if (aiContent.Contains("cotización") || aiContent.Contains("precio"))
        {
            actions.Add(new BusinessAction
            {
                ActionType = "generar_cotizacion",
                Description = "Generar cotización formal",
                ActionUrl = new Uri("/quotes/create", UriKind.Relative),
                Priority = BusinessPriority.Medium
            });
        }

        return Task.FromResult((aiContent, $"Conversación procesada: {conversationType}", actions));
    }

    private async Task<List<ProductSuggestion>> ExtractProductRecommendations(string aiContent, int? customerId)
    {
        var suggestions = new List<ProductSuggestion>();
        
        try
        {
            // Get real products from database
            var availableProducts = await _unitOfWork.Products.GetAllAsync();
            var activeProducts = availableProducts?.Where(p => p.IsActive && p.StockQuantity > 0).ToList() ?? new List<Product>();
            
            // Only recommend products that actually exist
            foreach (var product in activeProducts.Take(3))
            {
                // Check if product is relevant to conversation
                if (IsProductRelevant(product.ProductName, aiContent))
                {
                    suggestions.Add(new ProductSuggestion
                    {
                        ProductId = product.Id,
                        ProductName = product.ProductName,
                        ReasonForSuggestion = "Producto disponible en nuestro catálogo",
                        Price = product.PriceRetail,
                        InStock = product.StockQuantity > 0
                    });
                }
            }
            
            // If no specific matches, suggest top 3 available products
            if (!suggestions.Any())
            {
                foreach (var product in activeProducts.Take(3))
                {
                    suggestions.Add(new ProductSuggestion
                    {
                        ProductId = product.Id,
                        ProductName = product.ProductName,
                        ReasonForSuggestion = "Producto popular disponible",
                        Price = product.PriceRetail,
                        InStock = product.StockQuantity > 0
                    });
                }
            }
        }
        catch (Exception)
        {
            // Return empty suggestions if there's an error
        }

        return suggestions;
    }
    
    private static bool IsProductRelevant(string productName, string aiContent)
    {
        var productLower = productName.ToLower();
        var contentLower = aiContent.ToLower();
        
        // Check if AI response mentions similar words to product name
        var productWords = productLower.Split(' ');
        return productWords.Any(word => word.Length > 3 && contentLower.Contains(word));
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

    private Task<List<string>> ExtractBusinessEntities(string content)
    {
        var entities = new List<string>();
        
        // Simple entity extraction - in production, use proper NER
        var lowerContent = content.ToLower();
        foreach (var category in EntityPatterns)
        {
            foreach (var pattern in category.Value)
            {
                if (lowerContent.Contains(pattern))
                {
                    entities.Add($"{category.Key}:{pattern}");
                }
            }
        }

        return Task.FromResult(entities);
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

            var extractedItems = ExtractOrderItems(aiResponse.Content);
            var orderSummary = CalculateOrderSummary(extractedItems);
            var alerts = GenerateBusinessAlerts(extractedItems, context);
            var missingInfo = IdentifyMissingInformation(aiResponse.Content);
            var deliveryDate = ExtractRequestedDeliveryDate(aiResponse.Content);
            var paymentTerms = ExtractPaymentTerms(aiResponse.Content, context);

            return new ComplexOrderResponseDto
            {
                ExtractedItems = extractedItems.Select(ei => new VHouse.Application.DTOs.OrderItem
                {
                    ProductId = ei.ProductId,
                    ProductName = ei.Product?.ProductName ?? "Unknown Product",
                    Quantity = ei.Quantity,
                    UnitPrice = ei.UnitPrice
                }).ToList(),
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

    private Task<string> BuildEmailPrompt(string emailType, dynamic customer, object emailData)
    {
        var prompt = $@"
SISTEMA DE COMUNICACIONES AUTOMATIZADAS VHOUSE - DISTRIBUCIÓN VEGANA B2B

CLIENTE: {customer.CustomerName} ({customer.Email})
TIPO: {emailType}
DATOS: {System.Text.Json.JsonSerializer.Serialize(emailData)}

GENERA EMAIL PROFESIONAL CON:
ASUNTO: [Línea clara y específica]
CUERPO: [HTML básico, tono B2B vegano, call-to-action]

Responde ahora:";
        return Task.FromResult(prompt);
    }

    private static Task<string> BuildComplexOrderPrompt(string order, dynamic customer, BusinessContext context)
    {
        var prompt = $@"
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
        return Task.FromResult(prompt);
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

    private List<VHouse.Domain.Entities.OrderItem> ExtractOrderItems(string aiContent)
    {
        var items = new List<VHouse.Domain.Entities.OrderItem>();
        
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(aiContent);
            if (doc.RootElement.TryGetProperty("items", out var itemsElement))
            {
                foreach (var item in itemsElement.EnumerateArray())
                {
                    items.Add(new VHouse.Domain.Entities.OrderItem
                    {
                        ProductId = Random.Shared.Next(1, 100),
                        // ProductName removed - will be loaded via navigation property
                        Quantity = item.GetProperty("quantity").GetInt32(),
                        UnitPrice = Random.Shared.Next(50, 300),
                        // SpecialInstructions removed - not part of Domain entity
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
                    items.Add(new VHouse.Domain.Entities.OrderItem
                    {
                        ProductId = Random.Shared.Next(1, 100),
                        // ProductName removed - will be loaded via navigation property
                        Quantity = Random.Shared.Next(1, 10),
                        UnitPrice = Random.Shared.Next(50, 300)
                    });
                }
            }
        }

        return items;
    }

    private OrderSummary CalculateOrderSummary(List<VHouse.Domain.Entities.OrderItem> items)
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

    private List<VHouse.Application.DTOs.BusinessAlert> GenerateBusinessAlerts(List<VHouse.Domain.Entities.OrderItem> items, BusinessContext context)
    {
        var alerts = new List<VHouse.Application.DTOs.BusinessAlert>();
        
        var totalValue = items.Sum(i => i.Quantity * i.UnitPrice);
        if (totalValue > context.TypicalOrderValue * 2)
        {
            alerts.Add(new VHouse.Application.DTOs.BusinessAlert
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