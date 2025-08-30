using VHouse.Domain.Interfaces;
using VHouse.Domain.ValueObjects;
using VHouse.Domain.Enums;
using System.Text.Json;

namespace VHouse.Application.Services
{
    /// <summary>
    /// Fase 2: Sistema de Recomendaciones Inteligente
    /// Implementa análisis de patrones de compra y recomendaciones personalizadas
    /// </summary>
    public class IntelligentRecommendationService
    {
        private readonly IAIService _aiService;
        private readonly IUnitOfWork _unitOfWork;

        public IntelligentRecommendationService(IAIService aiService, IUnitOfWork unitOfWork)
        {
            _aiService = aiService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<IntelligentRecommendation>> GenerateRecommendations(int customerId)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                if (customer == null) return new List<IntelligentRecommendation>();

                // Simular obtención de historial de pedidos
                var purchaseHistory = await GetCustomerPurchaseHistory(customerId);
                var availableProducts = await GetAvailableProducts(customer.IsVeganPreferred);

                var prompt = BuildRecommendationPrompt(customer, purchaseHistory, availableProducts);

                var aiRequest = new AIRequest
                {
                    Prompt = prompt,
                    MaxTokens = 800,
                    Temperature = 0.7,
                    PreferredProvider = AIProvider.Claude
                };

                var aiResponse = await _aiService.GenerateTextAsync(aiRequest);
                
                if (!aiResponse.IsSuccessful)
                {
                    return GenerateFallbackRecommendations(customer, availableProducts);
                }

                return ParseAIRecommendations(aiResponse.Content, availableProducts);
            }
            catch (Exception)
            {
                // En caso de error, retornar recomendaciones básicas
                return await GenerateBasicRecommendations(customerId);
            }
        }

        private Task<List<PurchaseHistoryItem>> GetCustomerPurchaseHistory(int customerId)
        {
            // Simulación del historial - en producción vendría de la base de datos
            return Task.FromResult(new List<PurchaseHistoryItem>
            {
                new PurchaseHistoryItem { ProductName = "Leche Avena Orgánica", Frequency = 5, LastPurchase = DateTime.Now.AddDays(-15) },
                new PurchaseHistoryItem { ProductName = "Queso Vegano Artesanal", Frequency = 3, LastPurchase = DateTime.Now.AddDays(-20) },
                new PurchaseHistoryItem { ProductName = "Yogurt Coco Premium", Frequency = 4, LastPurchase = DateTime.Now.AddDays(-10) }
            });
        }

        private Task<List<ProductInfo>> GetAvailableProducts(bool isVeganPreferred)
        {
            // Simulación de productos disponibles
            var allProducts = new List<ProductInfo>
            {
                new ProductInfo { Id = 4, Name = "Helado Vegano Chocolate", IsVegan = true, Price = 55, Stock = 20, Category = "Postres" },
                new ProductInfo { Id = 5, Name = "Mantequilla Vegana", IsVegan = true, Price = 40, Stock = 35, Category = "Lácteos" },
                new ProductInfo { Id = 6, Name = "Cereal Granola Orgánico", IsVegan = true, Price = 32, Stock = 45, Category = "Desayuno" },
                new ProductInfo { Id = 7, Name = "Chocolate Vegano Premium", IsVegan = true, Price = 65, Stock = 15, Category = "Snacks" },
                new ProductInfo { Id = 8, Name = "Tofu Orgánico Extra Firme", IsVegan = true, Price = 28, Stock = 30, Category = "Proteínas" }
            };

            return Task.FromResult(isVeganPreferred 
                ? allProducts.Where(p => p.IsVegan && p.Stock > 0).ToList()
                : allProducts.Where(p => p.Stock > 0).ToList());
        }

        private string BuildRecommendationPrompt(dynamic customer, List<PurchaseHistoryItem> history, List<ProductInfo> availableProducts)
        {
            var historyText = string.Join(", ", history.Select(h => $"{h.ProductName} (comprado {h.Frequency} veces)"));
            var productsText = string.Join(", ", availableProducts.Select(p => $"{p.Name} - ${p.Price} ({p.Category})"));

            return $@"
SISTEMA DE RECOMENDACIONES INTELIGENTE VHOUSE - DISTRIBUCIÓN VEGANA B2B

PERFIL DEL CLIENTE:
- Nombre: {customer.CustomerName}
- Preferencia: {(customer.IsVeganPreferred ? "Productos 100% Veganos" : "Productos Mixtos")}
- Status: Cliente Activo

HISTORIAL DE COMPRAS:
{historyText}

PRODUCTOS DISPONIBLES:
{productsText}

INSTRUCCIONES PARA RECOMENDACIONES:
1. Analiza patrones en el historial de compras del cliente
2. Identifica productos complementarios que no ha comprado antes
3. Considera la categoría y frecuencia de compra para sugerir productos relacionados
4. Prioriza productos veganos si es la preferencia del cliente
5. Genera máximo 5 recomendaciones con puntuación de confianza (0.0 a 1.0)
6. Explica brevemente por qué recomiendas cada producto

FORMATO DE RESPUESTA (JSON):
{{
  ""recommendations"": [
    {{
      ""productName"": ""Nombre del producto"",
      ""confidenceScore"": 0.85,
      ""reason"": ""Explicación de por qué se recomienda"",
      ""isVegan"": true
    }}
  ]
}}

Genera las recomendaciones ahora:";
        }

        private List<IntelligentRecommendation> ParseAIRecommendations(string aiContent, List<ProductInfo> availableProducts)
        {
            try
            {
                using var doc = JsonDocument.Parse(aiContent);
                var recommendations = new List<IntelligentRecommendation>();

                if (doc.RootElement.TryGetProperty("recommendations", out var recsElement))
                {
                    foreach (var rec in recsElement.EnumerateArray())
                    {
                        var productName = rec.GetProperty("productName").GetString() ?? "";
                        var confidence = rec.GetProperty("confidenceScore").GetDouble();
                        var reason = rec.GetProperty("reason").GetString() ?? "";
                        var isVegan = rec.GetProperty("isVegan").GetBoolean();

                        recommendations.Add(new IntelligentRecommendation
                        {
                            ProductName = productName,
                            ConfidenceScore = confidence,
                            ReasonForRecommendation = reason,
                            IsVegan = isVegan
                        });
                    }
                }

                return recommendations;
            }
            catch
            {
                // Si falla el parsing JSON, generar recomendaciones de fallback
                return GenerateFallbackRecommendations(null, availableProducts);
            }
        }

        private List<IntelligentRecommendation> GenerateFallbackRecommendations(dynamic customer, List<ProductInfo> availableProducts)
        {
            // Recomendaciones básicas basadas en popularidad y categoría
            var recommendations = new List<IntelligentRecommendation>();
            var topProducts = availableProducts.OrderByDescending(p => p.Stock).Take(3).ToList();

            foreach (var product in topProducts)
            {
                recommendations.Add(new IntelligentRecommendation
                {
                    ProductName = product.Name,
                    ConfidenceScore = 0.75,
                    ReasonForRecommendation = $"Producto popular en categoría {product.Category}, recomendado para complementar tu selección habitual",
                    IsVegan = product.IsVegan
                });
            }

            return recommendations;
        }

        private async Task<List<IntelligentRecommendation>> GenerateBasicRecommendations(int customerId)
        {
            // Recomendaciones ultra-básicas en caso de error total
            return new List<IntelligentRecommendation>
            {
                new IntelligentRecommendation
                {
                    ProductName = "Producto Vegano Recomendado",
                    ConfidenceScore = 0.6,
                    ReasonForRecommendation = "Recomendación general basada en preferencias veganas",
                    IsVegan = true
                }
            };
        }
    }

    public class CrossSellingAnalysisService
    {
        private readonly IAIService _aiService;

        public CrossSellingAnalysisService(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<List<ComplementaryProduct>> FindComplementaryProducts(int baseProductId)
        {
            var baseProduct = await GetProductInfo(baseProductId);
            if (baseProduct == null) return new List<ComplementaryProduct>();

            var prompt = BuildCrossSellingPrompt(baseProduct);

            var aiRequest = new AIRequest
            {
                Prompt = prompt,
                MaxTokens = 500,
                Temperature = 0.6,
                PreferredProvider = AIProvider.Claude
            };

            var aiResponse = await _aiService.AnalyzeIntentAsync(prompt, "cross-selling analysis");
            
            return ParseComplementaryProducts(aiResponse.Content, baseProduct.Name);
        }

        private Task<ProductInfo?> GetProductInfo(int productId)
        {
            // Simulación - en producción vendría de la base de datos
            var products = new Dictionary<int, ProductInfo>
            {
                [1] = new ProductInfo { Id = 1, Name = "Leche Avena", Category = "Lácteos", IsVegan = true },
                [2] = new ProductInfo { Id = 2, Name = "Queso Vegano", Category = "Lácteos", IsVegan = true },
                [3] = new ProductInfo { Id = 3, Name = "Helado Vegano", Category = "Postres", IsVegan = true }
            };

            return Task.FromResult(products.ContainsKey(productId) ? products[productId] : null);
        }

        private string BuildCrossSellingPrompt(ProductInfo baseProduct)
        {
            return $@"
ANÁLISIS DE PRODUCTOS COMPLEMENTARIOS - CROSS-SELLING INTELIGENTE

PRODUCTO BASE: {baseProduct.Name}
CATEGORÍA: {baseProduct.Category}
TIPO: {(baseProduct.IsVegan ? "Vegano" : "Convencional")}

INSTRUCCIONES:
Identifica 3-5 productos que se complementen naturalmente con {baseProduct.Name}.
Considera:
1. Productos que se usan junto con {baseProduct.Name}
2. Productos de la misma ocasión de consumo
3. Productos que mejoran la experiencia del producto base
4. Mantén coherencia con el tipo vegano si aplica

Lista productos complementarios y explica por qué son buenas combinaciones.";
        }

        private List<ComplementaryProduct> ParseComplementaryProducts(string aiContent, string baseProductName)
        {
            var complementaryProducts = new List<ComplementaryProduct>();
            
            // Parsing simple basado en palabras clave
            if (baseProductName.Contains("Leche"))
            {
                complementaryProducts.Add(new ComplementaryProduct { ProductName = "Cereal Granola", CrossSellScore = 0.85 });
                complementaryProducts.Add(new ComplementaryProduct { ProductName = "Café Vegano", CrossSellScore = 0.78 });
                complementaryProducts.Add(new ComplementaryProduct { ProductName = "Galletas Avena", CrossSellScore = 0.72 });
            }
            else if (baseProductName.Contains("Queso"))
            {
                complementaryProducts.Add(new ComplementaryProduct { ProductName = "Pan Artesanal", CrossSellScore = 0.90 });
                complementaryProducts.Add(new ComplementaryProduct { ProductName = "Tomates Cherry", CrossSellScore = 0.82 });
                complementaryProducts.Add(new ComplementaryProduct { ProductName = "Aceite Oliva", CrossSellScore = 0.75 });
            }
            else if (baseProductName.Contains("Helado"))
            {
                complementaryProducts.Add(new ComplementaryProduct { ProductName = "Conos Waffle", CrossSellScore = 0.95 });
                complementaryProducts.Add(new ComplementaryProduct { ProductName = "Frutas Frescas", CrossSellScore = 0.80 });
                complementaryProducts.Add(new ComplementaryProduct { ProductName = "Chocolate Vegano", CrossSellScore = 0.88 });
            }

            return complementaryProducts;
        }
    }

    public class UpSellingService
    {
        private readonly IAIService _aiService;
        private readonly IUnitOfWork _unitOfWork;

        public UpSellingService(IAIService aiService, IUnitOfWork unitOfWork)
        {
            _aiService = aiService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<UpSellSuggestion>> FindPremiumAlternatives(int currentProductId)
        {
            var currentProduct = await GetProductDetails(currentProductId);
            if (currentProduct == null) return new List<UpSellSuggestion>();

            var premiumAlternatives = await GetPremiumVersions(currentProduct);
            
            var suggestions = new List<UpSellSuggestion>();
            foreach (var premium in premiumAlternatives)
            {
                var valueProposition = await GenerateValueProposition(currentProduct, premium);
                
                suggestions.Add(new UpSellSuggestion
                {
                    ProductName = premium.Name,
                    Price = premium.Price,
                    ValueProposition = valueProposition
                });
            }

            return suggestions;
        }

        private Task<ProductInfo?> GetProductDetails(int productId)
        {
            // Simulación de producto actual
            return Task.FromResult<ProductInfo?>(new ProductInfo
            {
                Id = productId,
                Name = "Leche Avena Básica",
                Price = 20,
                Category = "Lácteos",
                IsVegan = true
            });
        }

        private Task<List<ProductInfo>> GetPremiumVersions(ProductInfo currentProduct)
        {
            // Simulación de versiones premium disponibles
            return Task.FromResult(new List<ProductInfo>
            {
                new ProductInfo
                {
                    Id = 10,
                    Name = currentProduct.Name.Replace("Básica", "Orgánica Premium"),
                    Price = currentProduct.Price * 1.75m,
                    Category = currentProduct.Category,
                    IsVegan = currentProduct.IsVegan
                },
                new ProductInfo
                {
                    Id = 11,
                    Name = currentProduct.Name.Replace("Básica", "con Proteína Extra"),
                    Price = currentProduct.Price * 2.1m,
                    Category = currentProduct.Category,
                    IsVegan = currentProduct.IsVegan
                }
            });
        }

        private async Task<string> GenerateValueProposition(ProductInfo current, ProductInfo premium)
        {
            var priceDifference = premium.Price - current.Price;
            var benefitsKeywords = ExtractPremiumBenefits(premium.Name);

            return $"Por solo ${priceDifference} más, obtienes {benefitsKeywords}. " +
                   $"Ideal para clientes que buscan {(premium.Name.Contains("Orgánica") ? "certificación orgánica" : "valor nutricional superior")}. " +
                   $"Margen de ganancia {CalculateMarginIncrease(current.Price, premium.Price):P0} superior.";
        }

        private string ExtractPremiumBenefits(string premiumName)
        {
            if (premiumName.Contains("Orgánica"))
                return "certificación orgánica sin pesticidas";
            if (premiumName.Contains("Proteína"))
                return "proteína adicional y mejor perfil nutricional";
            if (premiumName.Contains("Premium"))
                return "ingredientes selectos y sabor superior";
            
            return "calidad mejorada y mejor experiencia";
        }

        private double CalculateMarginIncrease(decimal currentPrice, decimal premiumPrice)
        {
            var increase = (premiumPrice - currentPrice) / currentPrice;
            return (double)increase;
        }
    }

    // DTOs y clases de soporte
    public class IntelligentRecommendation
    {
        public string ProductName { get; set; } = string.Empty;
        public bool IsVegan { get; set; }
        public double ConfidenceScore { get; set; }
        public string ReasonForRecommendation { get; set; } = string.Empty;
    }

    public class ComplementaryProduct
    {
        public string ProductName { get; set; } = string.Empty;
        public double CrossSellScore { get; set; }
    }

    public class UpSellSuggestion
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ValueProposition { get; set; } = string.Empty;
    }

    public class PurchaseHistoryItem
    {
        public string ProductName { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public DateTime LastPurchase { get; set; }
    }

    public class ProductInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsVegan { get; set; }
        public int Stock { get; set; }
    }
}