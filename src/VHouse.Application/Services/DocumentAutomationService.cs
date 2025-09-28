using VHouse.Domain.Interfaces;
using VHouse.Domain.ValueObjects;
using VHouse.Domain.Enums;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VHouse.Application.Services
{
    /// <summary>
    /// Fase 2: Automatización de Procesos Documentales
    /// Extracción automática de información de facturas, órdenes de compra y contratos
    /// </summary>
    public class IntelligentDocumentProcessor
    {
        private readonly IAIService _aiService;
        
        // Static readonly array for better performance
        private static readonly string[] CommonContractTerms = { "pago", "entrega", "garantía", "responsabilidad", "vigencia", "cancelación" };

        public IntelligentDocumentProcessor(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<ExtractedInvoice> ProcessInvoice(string invoiceText)
        {
            try
            {
                var prompt = BuildInvoiceExtractionPrompt(invoiceText);

                var aiRequest = new AIRequest
                {
                    Prompt = prompt,
                    MaxTokens = 800,
                    Temperature = 0.1, // Baja temperatura para precisión en extracción
                    PreferredProvider = AIProvider.Claude
                };

                var aiResponse = await _aiService.GenerateTextAsync(aiRequest);
                
                if (aiResponse.IsSuccessful)
                {
                    return ParseInvoiceResponse(aiResponse.Content);
                }

                // Fallback: extracción manual con regex
                return ExtractInvoiceWithRegex(invoiceText);
            }
            catch (Exception ex) when (ex is JsonException or HttpRequestException or TaskCanceledException)
            {
                return ExtractInvoiceWithRegex(invoiceText);
            }
        }

        public async Task<ExtractedPurchaseOrder> ProcessPurchaseOrder(string poText)
        {
            try
            {
                var prompt = BuildPurchaseOrderPrompt(poText);

                var aiRequest = new AIRequest
                {
                    Prompt = prompt,
                    MaxTokens = 800,
                    Temperature = 0.1,
                    PreferredProvider = AIProvider.Claude
                };

                var aiResponse = await _aiService.GenerateTextAsync(aiRequest);
                
                if (aiResponse.IsSuccessful)
                {
                    return ParsePurchaseOrderResponse(aiResponse.Content);
                }

                return ExtractPurchaseOrderWithRegex(poText);
            }
            catch (Exception ex) when (ex is JsonException or HttpRequestException or TaskCanceledException)
            {
                return ExtractPurchaseOrderWithRegex(poText);
            }
        }

        public async Task<ContractAnalysis> AnalyzeContract(string contractText)
        {
            var prompt = BuildContractAnalysisPrompt(contractText);

            var aiRequest = new AIRequest
            {
                Prompt = prompt,
                MaxTokens = 1000,
                Temperature = 0.2,
                PreferredProvider = AIProvider.Claude
            };

            var aiResponse = await _aiService.GenerateTextAsync(aiRequest);
            
            if (aiResponse.IsSuccessful)
            {
                return ParseContractAnalysis(aiResponse.Content);
            }

            return new ContractAnalysis
            {
                KeyTerms = ExtractBasicContractTerms(contractText),
                RiskFactors = new List<string> { "Análisis automático no disponible" },
                PaymentTerms = "No detectado",
                DeliveryTerms = "No detectado",
                Summary = "Contrato requiere revisión manual"
            };
        }

        private string BuildInvoiceExtractionPrompt(string invoiceText)
        {
            return $@"
EXTRACTOR INTELIGENTE DE FACTURAS - VHOUSE DISTRIBUCIÓN VEGANA

DOCUMENTO A PROCESAR:
{invoiceText}

INSTRUCCIONES:
Extrae la siguiente información de la factura de manera precisa:
1. Número de factura
2. Nombre del cliente
3. Fecha de emisión
4. Lista de productos con cantidades, precios unitarios y totales
5. Subtotal, impuestos (IVA) y total final
6. Términos de pago
7. Información de contacto si está disponible

FORMATO DE RESPUESTA REQUERIDO (JSON):
{{
    ""invoiceNumber"": ""número de factura"",
    ""customerName"": ""nombre del cliente"",
    ""date"": ""YYYY-MM-DD"",
    ""items"": [
        {{
            ""product"": ""nombre del producto"",
            ""quantity"": cantidad_numerica,
            ""unitPrice"": precio_unitario_decimal,
            ""total"": total_linea_decimal
        }}
    ],
    ""subtotal"": subtotal_decimal,
    ""tax"": impuesto_decimal,
    ""total"": total_final_decimal,
    ""paymentTerms"": ""términos de pago"",
    ""contactInfo"": ""información de contacto""
}}

Extrae solo información que esté claramente presente en el documento. Si algo no está disponible, usa null o cadena vacía.";
        }

        private string BuildPurchaseOrderPrompt(string poText)
        {
            return $@"
PROCESADOR DE ÓRDENES DE COMPRA - SISTEMA VHOUSE B2B

ORDEN DE COMPRA:
{poText}

EXTRAE LA SIGUIENTE INFORMACIÓN:
1. Número de orden de compra
2. Cliente/Comprador
3. Proveedor (VHouse)
4. Fecha de la orden
5. Fecha de entrega requerida
6. Lista detallada de productos solicitados
7. Cantidades y especificaciones
8. Precios acordados (si están disponibles)
9. Dirección de entrega
10. Términos y condiciones especiales

RESPUESTA EN FORMATO JSON:
{{
    ""poNumber"": ""número PO"",
    ""buyer"": ""nombre del comprador"",
    ""supplier"": ""VHouse"",
    ""orderDate"": ""YYYY-MM-DD"",
    ""requiredDeliveryDate"": ""YYYY-MM-DD"",
    ""deliveryAddress"": ""dirección completa"",
    ""items"": [
        {{
            ""product"": ""nombre producto"",
            ""quantity"": cantidad,
            ""specifications"": ""especificaciones especiales"",
            ""agreedPrice"": precio_decimal_opcional
        }}
    ],
    ""specialTerms"": ""términos especiales"",
    ""totalEstimatedValue"": valor_total_estimado
}}";
        }

        private string BuildContractAnalysisPrompt(string contractText)
        {
            return $@"
ANALIZADOR INTELIGENTE DE CONTRATOS COMERCIALES - VHOUSE B2B

CONTRATO A ANALIZAR:
{contractText}

ANÁLISIS REQUERIDO:
1. Identifica términos clave del contrato
2. Detecta posibles riesgos o cláusulas problemáticas
3. Extrae términos de pago y condiciones financieras
4. Identifica términos de entrega y logística
5. Resuma los puntos más importantes
6. Señala cualquier término inusual o que requiera atención especial

FORMATO DE RESPUESTA:
{{
    ""keyTerms"": [""término1"", ""término2"", ""término3""],
    ""riskFactors"": [""riesgo1"", ""riesgo2""],
    ""paymentTerms"": ""descripción de términos de pago"",
    ""deliveryTerms"": ""descripción de términos de entrega"",
    ""summary"": ""resumen ejecutivo del contrato"",
    ""unusualClauses"": [""cláusula1"", ""cláusula2""],
    ""recommendedActions"": [""acción1"", ""acción2""]
}}

Prioriza la identificación de términos que puedan afectar la operación comercial de VHouse.";
        }

        private ExtractedInvoice ParseInvoiceResponse(string aiContent)
        {
            try
            {
                using var doc = JsonDocument.Parse(aiContent);
                var root = doc.RootElement;

                var invoice = new ExtractedInvoice
                {
                    InvoiceNumber = root.GetProperty("invoiceNumber").GetString() ?? "",
                    CustomerName = root.GetProperty("customerName").GetString() ?? "",
                    Total = root.GetProperty("total").GetDecimal(),
                    PaymentTerms = root.GetProperty("paymentTerms").GetString() ?? "",
                    Items = new List<InvoiceItem>()
                };

                if (root.TryGetProperty("items", out var itemsElement))
                {
                    foreach (var item in itemsElement.EnumerateArray())
                    {
                        invoice.Items.Add(new InvoiceItem
                        {
                            Product = item.GetProperty("product").GetString() ?? "",
                            Quantity = item.GetProperty("quantity").GetInt32(),
                            UnitPrice = item.GetProperty("unitPrice").GetDecimal()
                        });
                    }
                }

                return invoice;
            }
            catch (Exception)
            {
                return new ExtractedInvoice { InvoiceNumber = "Parse Error", Items = new List<InvoiceItem>() };
            }
        }

        private ExtractedPurchaseOrder ParsePurchaseOrderResponse(string aiContent)
        {
            try
            {
                using var doc = JsonDocument.Parse(aiContent);
                var root = doc.RootElement;

                return new ExtractedPurchaseOrder
                {
                    PONumber = root.GetProperty("poNumber").GetString() ?? "",
                    Buyer = root.GetProperty("buyer").GetString() ?? "",
                    OrderDate = DateTime.TryParse(root.GetProperty("orderDate").GetString(), out var orderDate) ? orderDate : DateTime.MinValue,
                    RequiredDeliveryDate = DateTime.TryParse(root.GetProperty("requiredDeliveryDate").GetString(), out var deliveryDate) ? deliveryDate : DateTime.MinValue,
                    DeliveryAddress = root.GetProperty("deliveryAddress").GetString() ?? "",
                    SpecialTerms = root.GetProperty("specialTerms").GetString() ?? "",
                    Items = ExtractPOItems(root)
                };
            }
            catch (Exception)
            {
                return new ExtractedPurchaseOrder { PONumber = "Parse Error" };
            }
        }

        private List<PurchaseOrderItem> ExtractPOItems(JsonElement root)
        {
            var items = new List<PurchaseOrderItem>();
            
            if (root.TryGetProperty("items", out var itemsElement))
            {
                foreach (var item in itemsElement.EnumerateArray())
                {
                    items.Add(new PurchaseOrderItem
                    {
                        Product = item.GetProperty("product").GetString() ?? "",
                        Quantity = item.GetProperty("quantity").GetInt32(),
                        Specifications = item.TryGetProperty("specifications", out var spec) ? spec.GetString() ?? "" : "",
                        AgreedPrice = item.TryGetProperty("agreedPrice", out var price) ? price.GetDecimal() : null
                    });
                }
            }

            return items;
        }

        private ContractAnalysis ParseContractAnalysis(string aiContent)
        {
            try
            {
                using var doc = JsonDocument.Parse(aiContent);
                var root = doc.RootElement;

                return new ContractAnalysis
                {
                    KeyTerms = ExtractStringArray(root, "keyTerms"),
                    RiskFactors = ExtractStringArray(root, "riskFactors"),
                    PaymentTerms = root.GetProperty("paymentTerms").GetString() ?? "",
                    DeliveryTerms = root.GetProperty("deliveryTerms").GetString() ?? "",
                    Summary = root.GetProperty("summary").GetString() ?? "",
                    UnusualClauses = ExtractStringArray(root, "unusualClauses"),
                    RecommendedActions = ExtractStringArray(root, "recommendedActions")
                };
            }
            catch (Exception)
            {
                return new ContractAnalysis
                {
                    KeyTerms = new List<string> { "Error en análisis automático" },
                    Summary = "Requiere revisión manual"
                };
            }
        }

        private static List<string> ExtractStringArray(JsonElement root, string propertyName)
        {
            var result = new List<string>();
            
            if (root.TryGetProperty(propertyName, out var arrayElement))
            {
                foreach (var item in arrayElement.EnumerateArray())
                {
                    var value = item.GetString();
                    if (!string.IsNullOrEmpty(value))
                        result.Add(value);
                }
            }

            return result;
        }

        // Métodos de fallback con regex para cuando la AI no esté disponible
        private static ExtractedInvoice ExtractInvoiceWithRegex(string invoiceText)
        {
            var invoice = new ExtractedInvoice { Items = new List<InvoiceItem>() };

            // Extraer número de factura
            var invoiceNumberMatch = Regex.Match(invoiceText, @"(?:FACTURA|INVOICE|#)\s*[:\-#]?\s*(\w+)", RegexOptions.IgnoreCase);
            if (invoiceNumberMatch.Success)
                invoice.InvoiceNumber = invoiceNumberMatch.Groups[1].Value;

            // Extraer nombre del cliente
            var customerMatch = Regex.Match(invoiceText, @"(?:Cliente|Customer)[:\-]?\s*([^\n\r]+)", RegexOptions.IgnoreCase);
            if (customerMatch.Success)
                invoice.CustomerName = customerMatch.Groups[1].Value.Trim();

            // Extraer total
            var totalMatch = Regex.Match(invoiceText, @"(?:Total|TOTAL)[:\-]?\s*\$?(\d+[.,]?\d*)", RegexOptions.IgnoreCase);
            if (totalMatch.Success && decimal.TryParse(totalMatch.Groups[1].Value.Replace(",", ""), out var total))
                invoice.Total = total;

            // Extraer términos de pago
            var paymentTermsMatch = Regex.Match(invoiceText, @"(?:Términos|Terms)[:\-]?\s*([^\n\r]+)", RegexOptions.IgnoreCase);
            if (paymentTermsMatch.Success)
                invoice.PaymentTerms = paymentTermsMatch.Groups[1].Value.Trim();

            return invoice;
        }

        private static ExtractedPurchaseOrder ExtractPurchaseOrderWithRegex(string poText)
        {
            var po = new ExtractedPurchaseOrder();

            var poNumberMatch = Regex.Match(poText, @"(?:PO|P\.O\.|Order)\s*[#:\-]?\s*(\w+)", RegexOptions.IgnoreCase);
            if (poNumberMatch.Success)
                po.PONumber = poNumberMatch.Groups[1].Value;

            var buyerMatch = Regex.Match(poText, @"(?:Buyer|Cliente|From)[:\-]?\s*([^\n\r]+)", RegexOptions.IgnoreCase);
            if (buyerMatch.Success)
                po.Buyer = buyerMatch.Groups[1].Value.Trim();

            return po;
        }

        private static List<string> ExtractBasicContractTerms(string contractText)
        {
            var terms = new List<string>();

            // Buscar términos comunes en contratos
            foreach (var term in CommonContractTerms)
            {
                if (contractText.ToLower().Contains(term))
                    terms.Add($"Contiene términos de {term}");
            }

            return terms.Any() ? terms : new List<string> { "Términos no identificados automáticamente" };
        }
    }

    public class InventoryOptimizationService
    {
        private readonly IAIService _aiService;

        public InventoryOptimizationService(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<StockOptimization> OptimizeStockLevel(dynamic product, List<DailySales> salesHistory)
        {
            if (!salesHistory.Any())
            {
                return new StockOptimization
                {
                    RecommendedStockLevel = (int)product.StockQuantity * 2,
                    ReorderPoint = (int)product.StockQuantity,
                    SafetyStock = 10,
                    Reasoning = "Sin historial de ventas disponible - usando estimación conservadora",
                    ShouldReorder = (int)product.StockQuantity < 20,
                    UrgencyLevel = "Medium"
                };
            }

            var prompt = BuildInventoryOptimizationPrompt(product, salesHistory);

            var aiRequest = new AIRequest
            {
                Prompt = prompt,
                MaxTokens = 600,
                Temperature = 0.2,
                PreferredProvider = AIProvider.Claude
            };

            var aiResponse = await _aiService.GenerateTextAsync(aiRequest);
            
            if (aiResponse.IsSuccessful)
            {
                return ParseOptimizationResponse(aiResponse.Content, (int)product.StockQuantity);
            }

            // Fallback: cálculo manual
            return CalculateOptimizationManually(product, salesHistory);
        }

        private string BuildInventoryOptimizationPrompt(dynamic product, List<DailySales> salesHistory)
        {
            var avgDailySales = salesHistory.Average(s => s.QuantitySold);
            var maxDailySales = salesHistory.Max(s => s.QuantitySold);
            var salesTrend = CalculateSalesTrend(salesHistory);

            return $@"
OPTIMIZADOR DE INVENTARIO INTELIGENTE - VHOUSE DISTRIBUCIÓN VEGANA

PRODUCTO: {product.ProductName}
STOCK ACTUAL: {product.StockQuantity} unidades
PRECIO: ${product.PricePublic}

ANÁLISIS DE VENTAS (ÚLTIMOS {salesHistory.Count} DÍAS):
- Ventas promedio diarias: {avgDailySales:F1} unidades
- Ventas máximas en un día: {maxDailySales} unidades  
- Tendencia de ventas: {(salesTrend > 0 ? "Creciente" : salesTrend < 0 ? "Decreciente" : "Estable")} ({salesTrend:P1})

PARÁMETROS DE OPTIMIZACIÓN:
- Lead time típico: 5-7 días
- Nivel de servicio objetivo: 95%
- Costo de mantener inventario: 15% anual
- Costo de faltante: Alto (pérdida de ventas B2B)

CALCULA Y RECOMIENDA:
1. Nivel óptimo de inventario
2. Punto de reorden
3. Stock de seguridad
4. Si se debe reordenar inmediatamente

FORMATO DE RESPUESTA JSON:
{{
    ""recommendedStockLevel"": nivel_optimo_numerico,
    ""reorderPoint"": punto_reorden_numerico,
    ""averageDailySales"": promedio_ventas_diario,
    ""leadTimeDays"": dias_lead_time,
    ""safetyStock"": stock_seguridad_numerico,
    ""reasoning"": ""explicación de la recomendación""
}}";
        }

        private static double CalculateSalesTrend(List<DailySales> salesHistory)
        {
            if (salesHistory.Count < 7) return 0;

            var firstHalf = salesHistory.Take(salesHistory.Count / 2).Average(s => s.QuantitySold);
            var secondHalf = salesHistory.Skip(salesHistory.Count / 2).Average(s => s.QuantitySold);

            return firstHalf > 0 ? (secondHalf - firstHalf) / firstHalf : 0;
        }

        private StockOptimization ParseOptimizationResponse(string aiContent, int currentStock)
        {
            try
            {
                using var doc = JsonDocument.Parse(aiContent);
                var root = doc.RootElement;

                var recommendedLevel = root.GetProperty("recommendedStockLevel").GetInt32();
                var reorderPoint = root.GetProperty("reorderPoint").GetInt32();

                return new StockOptimization
                {
                    RecommendedStockLevel = recommendedLevel,
                    ReorderPoint = reorderPoint,
                    SafetyStock = root.GetProperty("safetyStock").GetInt32(),
                    Reasoning = root.GetProperty("reasoning").GetString() ?? "",
                    ShouldReorder = currentStock <= reorderPoint,
                    UrgencyLevel = DetermineUrgencyLevel(currentStock, reorderPoint)
                };
            }
            catch (Exception)
            {
                return new StockOptimization
                {
                    RecommendedStockLevel = currentStock * 2,
                    ReorderPoint = (int)(currentStock * 0.3),
                    SafetyStock = 15,
                    Reasoning = "Error en cálculo automático - usando estimación conservadora",
                    ShouldReorder = currentStock < 30,
                    UrgencyLevel = "Medium"
                };
            }
        }

        private StockOptimization CalculateOptimizationManually(dynamic product, List<DailySales> salesHistory)
        {
            var avgDailySales = salesHistory.Average(s => s.QuantitySold);
            var maxDailySales = salesHistory.Max(s => s.QuantitySold);
            var leadTimeDays = 7; // Estimación estándar

            var safetyStock = (int)Math.Ceiling((maxDailySales - avgDailySales) * leadTimeDays * 1.5);
            var reorderPoint = (int)Math.Ceiling(avgDailySales * leadTimeDays + safetyStock);
            var recommendedLevel = reorderPoint + (int)(avgDailySales * 14); // 2 semanas extra

            return new StockOptimization
            {
                RecommendedStockLevel = recommendedLevel,
                ReorderPoint = reorderPoint,
                SafetyStock = safetyStock,
                Reasoning = $"Basado en ventas promedio de {avgDailySales:F1} unidades/día con lead time de {leadTimeDays} días",
                ShouldReorder = (int)product.StockQuantity <= reorderPoint,
                UrgencyLevel = DetermineUrgencyLevel((int)product.StockQuantity, reorderPoint)
            };
        }

        private static string DetermineUrgencyLevel(int currentStock, int reorderPoint)
        {
            if (currentStock <= reorderPoint * 0.5) return "Critical";
            if (currentStock <= reorderPoint * 0.7) return "High";
            if (currentStock <= reorderPoint) return "Medium";
            return "Low";
        }
    }

    // DTOs para Document Automation
    public class ExtractedInvoice
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public ICollection<InvoiceItem> Items { get; init; } = new List<InvoiceItem>();
        public decimal Total { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
    }

    public class InvoiceItem
    {
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class ExtractedPurchaseOrder
    {
        public string PONumber { get; set; } = string.Empty;
        public string Buyer { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime RequiredDeliveryDate { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string SpecialTerms { get; set; } = string.Empty;
        public ICollection<PurchaseOrderItem> Items { get; init; } = new List<PurchaseOrderItem>();
    }

    public class PurchaseOrderItem
    {
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Specifications { get; set; } = string.Empty;
        public decimal? AgreedPrice { get; set; }
    }

    public class ContractAnalysis
    {
        public ICollection<string> KeyTerms { get; init; } = new List<string>();
        public ICollection<string> RiskFactors { get; init; } = new List<string>();
        public string PaymentTerms { get; set; } = string.Empty;
        public string DeliveryTerms { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public ICollection<string> UnusualClauses { get; init; } = new List<string>();
        public ICollection<string> RecommendedActions { get; init; } = new List<string>();
    }

    public class StockOptimization
    {
        public int RecommendedStockLevel { get; set; }
        public int ReorderPoint { get; set; }
        public int SafetyStock { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public bool ShouldReorder { get; set; }
        public string UrgencyLevel { get; set; } = string.Empty;
    }

    public class DailySales
    {
        public DateTime Date { get; set; }
        public int QuantitySold { get; set; }
    }
}