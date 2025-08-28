using Xunit;
using VHouse.Domain.Entities;
using VHouse.Domain.Enums;
using VHouse.Domain.ValueObjects;
using VHouse.Application.DTOs;
using VHouse.Domain.Interfaces;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace VHouse.Tests
{
    /// <summary>
    /// TDD Tests para Fase 2: Sistema de Recomendaciones y Automatización Inteligente
    /// Implementa análisis de patrones de compra, cross-selling y up-selling
    /// </summary>
    public class Phase2RecommendationSystemTests
    {
        private readonly Mock<IAIService> _mockAIService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<Customer>> _mockCustomerRepo;
        private readonly Mock<IRepository<Order>> _mockOrderRepo;
        private readonly Mock<IRepository<Product>> _mockProductRepo;

        public Phase2RecommendationSystemTests()
        {
            _mockAIService = new Mock<IAIService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCustomerRepo = new Mock<IRepository<Customer>>();
            _mockOrderRepo = new Mock<IRepository<Order>>();
            _mockProductRepo = new Mock<IRepository<Product>>();
            
            _mockUnitOfWork.Setup(x => x.Customers).Returns(_mockCustomerRepo.Object);
            _mockUnitOfWork.Setup(x => x.Orders).Returns(_mockOrderRepo.Object);
            _mockUnitOfWork.Setup(x => x.Products).Returns(_mockProductRepo.Object);
        }

        [Fact]
        public async Task GenerateProductRecommendations_BasedOnPurchaseHistory_ShouldReturnRelevantProducts()
        {
            // Arrange - TDD: Define expected behavior first
            var customerId = 1;
            var customer = new Customer
            {
                Id = customerId,
                CustomerName = "VegaStore Premium",
                IsVeganPreferred = true,
                IsActive = true
            };

            var previousOrders = new List<Order>
            {
                new Order
                {
                    Id = 100,
                    CustomerId = customerId,
                    Status = OrderStatus.Completed,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem { ProductId = 1, Quantity = 5, UnitPrice = 25 }, // Leche Avena
                        new OrderItem { ProductId = 2, Quantity = 3, UnitPrice = 45 }  // Queso Vegano
                    }
                },
                new Order
                {
                    Id = 101,
                    CustomerId = customerId,
                    Status = OrderStatus.Completed,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem { ProductId = 1, Quantity = 8, UnitPrice = 25 }, // Leche Avena again
                        new OrderItem { ProductId = 3, Quantity = 2, UnitPrice = 35 }  // Yogurt Coco
                    }
                }
            };

            var availableProducts = new List<Product>
            {
                new Product { Id = 1, ProductName = "Leche Avena Orgánica", IsVegan = true, PricePublic = 25, StockQuantity = 50 },
                new Product { Id = 2, ProductName = "Queso Vegano Artesanal", IsVegan = true, PricePublic = 45, StockQuantity = 30 },
                new Product { Id = 3, ProductName = "Yogurt Coco Premium", IsVegan = true, PricePublic = 35, StockQuantity = 25 },
                new Product { Id = 4, ProductName = "Helado Vegano Chocolate", IsVegan = true, PricePublic = 55, StockQuantity = 20 }, // NEW
                new Product { Id = 5, ProductName = "Mantequilla Vegana", IsVegan = true, PricePublic = 40, StockQuantity = 35 }    // NEW
            };

            // Act - Implementation will be created based on these tests
            var recommendationService = new IntelligentRecommendationService(_mockAIService.Object, _mockUnitOfWork.Object);
            var recommendations = await recommendationService.GenerateRecommendations(customerId);

            // Assert - Define what we expect
            Assert.NotNull(recommendations);
            Assert.True(recommendations.Count >= 2);
            Assert.True(recommendations.Count <= 5); // Max 5 recommendations
            
            // Should recommend products customer hasn't bought but are complementary
            Assert.Contains(recommendations, r => r.ProductName.Contains("Helado") || r.ProductName.Contains("Mantequilla"));
            
            // All recommendations should be vegan (customer preference)
            Assert.All(recommendations, r => Assert.True(r.IsVegan));
            
            // Should have confidence scores
            Assert.All(recommendations, r => Assert.True(r.ConfidenceScore > 0 && r.ConfidenceScore <= 1));
            
            // Should include reasoning
            Assert.All(recommendations, r => Assert.False(string.IsNullOrEmpty(r.ReasonForRecommendation)));
        }

        [Theory]
        [InlineData("Leche Avena", new[] { "Cereal Granola", "Café Vegano", "Galletas Avena" })]
        [InlineData("Queso Vegano", new[] { "Pan Artesanal", "Tomates Cherry", "Aceite Oliva" })]
        [InlineData("Helado Vegano", new[] { "Conos Waffle", "Frutas Frescas", "Chocolate Vegano" })]
        public async Task CrossSellingAnalysis_ShouldIdentifyComplementaryProducts(string baseProduct, string[] expectedComplements)
        {
            // Arrange - TDD for cross-selling logic
            var baseProductId = 1;
            var aiResponse = new AIResponse
            {
                Content = $"Los productos complementarios para {baseProduct} incluyen: {string.Join(", ", expectedComplements)}. Estas combinaciones aumentan el valor del carrito y mejoran la experiencia del cliente.",
                IsSuccessful = true,
                UsedProvider = AIProvider.Claude
            };

            _mockAIService.Setup(x => x.AnalyzeIntentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(aiResponse);

            // Act
            var crossSellService = new CrossSellingAnalysisService(_mockAIService.Object);
            var complementaryProducts = await crossSellService.FindComplementaryProducts(baseProductId);

            // Assert
            Assert.NotEmpty(complementaryProducts);
            Assert.True(complementaryProducts.Count >= 2);
            foreach (var expectedComplement in expectedComplements)
            {
                Assert.Contains(complementaryProducts, cp => cp.ProductName.Contains(expectedComplement.Split(' ')[0]));
            }
        }

        [Fact]
        public async Task UpSellingOpportunities_ShouldSuggestPremiumAlternatives()
        {
            // Arrange - TDD for up-selling logic
            var currentProduct = new Product
            {
                Id = 1,
                ProductName = "Leche Avena Básica",
                PricePublic = 20,
                IsVegan = true
            };

            var premiumAlternatives = new List<Product>
            {
                new Product
                {
                    Id = 10,
                    ProductName = "Leche Avena Orgánica Premium",
                    PricePublic = 35,
                    IsVegan = true
                },
                new Product
                {
                    Id = 11,
                    ProductName = "Leche Avena con Proteína Extra",
                    PricePublic = 42,
                    IsVegan = true
                }
            };

            // Act
            var upSellService = new UpSellingService(_mockAIService.Object, _mockUnitOfWork.Object);
            var upSellSuggestions = await upSellService.FindPremiumAlternatives(currentProduct.Id);

            // Assert
            Assert.NotEmpty(upSellSuggestions);
            Assert.All(upSellSuggestions, suggestion => 
            {
                Assert.True(suggestion.Price > currentProduct.PricePublic);
                Assert.Contains("Premium", suggestion.ProductName);
                Assert.True(suggestion.ValueProposition.Length > 10); // Should have compelling reason
            });
        }

        [Fact]
        public async Task PurchasePatternAnalysis_ShouldIdentifyCustomerBehaviorTrends()
        {
            // Arrange - TDD for pattern analysis
            var customerId = 1;
            var orderHistory = new List<Order>
            {
                // Monthly pattern - always orders on first week
                new Order { Id = 1, CustomerId = customerId, CreatedAt = new DateTime(2024, 11, 5), TotalAmount = 150 },
                new Order { Id = 2, CustomerId = customerId, CreatedAt = new DateTime(2024, 12, 3), TotalAmount = 175 },
                new Order { Id = 3, CustomerId = customerId, CreatedAt = new DateTime(2025, 1, 7), TotalAmount = 200 },
                
                // Increasing order value pattern
                new Order { Id = 4, CustomerId = customerId, CreatedAt = new DateTime(2025, 2, 4), TotalAmount = 225 }
            };

            // Act
            var patternAnalyzer = new PurchasePatternAnalyzer(_mockAIService.Object);
            var patterns = await patternAnalyzer.AnalyzeCustomerPatterns(customerId, orderHistory);

            // Assert
            Assert.NotNull(patterns);
            Assert.True(patterns.OrderFrequency > 0); // Should detect monthly frequency
            Assert.Equal("Monthly", patterns.OrderingCycle);
            Assert.True(patterns.AverageOrderValue > 0);
            Assert.True(patterns.GrowthTrend > 0); // Increasing order values
            Assert.Equal("First Week", patterns.PreferredOrderingTime);
            
            // Should predict next order
            Assert.NotNull(patterns.PredictedNextOrderDate);
            Assert.True(patterns.PredictedNextOrderDate > DateTime.Now);
        }

        [Theory]
        [InlineData(150, "Budget-Conscious", 0.8)] // Low average order
        [InlineData(500, "Premium Customer", 0.9)] // High average order  
        [InlineData(50, "Price-Sensitive", 0.7)]   // Very low average order
        public async Task CustomerSegmentation_ShouldCategorizeBySpendingBehavior(
            decimal averageOrderValue, string expectedSegment, double expectedConfidence)
        {
            // Arrange - TDD for customer segmentation
            var customer = new Customer
            {
                Id = 1,
                CustomerName = "Test Customer",
                IsVeganPreferred = true
            };

            var orderHistory = new List<Order>
            {
                new Order { CustomerId = 1, TotalAmount = averageOrderValue },
                new Order { CustomerId = 1, TotalAmount = averageOrderValue * 0.9m },
                new Order { CustomerId = 1, TotalAmount = averageOrderValue * 1.1m }
            };

            // Act
            var segmentationService = new CustomerSegmentationService(_mockAIService.Object);
            var segment = await segmentationService.CategorizeCustomer(customer.Id, orderHistory);

            // Assert
            Assert.NotNull(segment);
            Assert.Equal(expectedSegment, segment.SegmentName);
            Assert.True(segment.Confidence >= expectedConfidence);
            Assert.NotEmpty(segment.RecommendedStrategies);
            Assert.True(segment.PredictedLifetimeValue > 0);
        }

        [Fact]
        public async Task DocumentAutomation_ShouldExtractInvoiceInformation()
        {
            // Arrange - TDD for document processing
            var invoiceText = @"
                FACTURA #12345
                Cliente: VegaStore Premium
                Fecha: 2025-01-15
                
                PRODUCTOS:
                - Leche Avena Orgánica x 10 unidades @ $25.00 = $250.00
                - Queso Vegano Artesanal x 5 unidades @ $45.00 = $225.00
                - Yogurt Coco Premium x 8 unidades @ $35.00 = $280.00
                
                Subtotal: $755.00
                IVA (16%): $120.80
                Total: $875.80
                
                Términos: Net 30 días
            ";

            var aiResponse = new AIResponse
            {
                Content = @"{
                    ""invoiceNumber"": ""12345"",
                    ""customerName"": ""VegaStore Premium"",
                    ""date"": ""2025-01-15"",
                    ""items"": [
                        {""product"": ""Leche Avena Orgánica"", ""quantity"": 10, ""unitPrice"": 25.00, ""total"": 250.00},
                        {""product"": ""Queso Vegano Artesanal"", ""quantity"": 5, ""unitPrice"": 45.00, ""total"": 225.00},
                        {""product"": ""Yogurt Coco Premium"", ""quantity"": 8, ""unitPrice"": 35.00, ""total"": 280.00}
                    ],
                    ""subtotal"": 755.00,
                    ""tax"": 120.80,
                    ""total"": 875.80,
                    ""paymentTerms"": ""Net 30 días""
                }",
                IsSuccessful = true,
                UsedProvider = AIProvider.Claude
            };

            _mockAIService.Setup(x => x.GenerateTextAsync(It.IsAny<AIRequest>()))
                .ReturnsAsync(aiResponse);

            // Act
            var documentProcessor = new IntelligentDocumentProcessor(_mockAIService.Object);
            var extractedInvoice = await documentProcessor.ProcessInvoice(invoiceText);

            // Assert
            Assert.NotNull(extractedInvoice);
            Assert.Equal("12345", extractedInvoice.InvoiceNumber);
            Assert.Equal("VegaStore Premium", extractedInvoice.CustomerName);
            Assert.Equal(3, extractedInvoice.Items.Count);
            Assert.Equal(875.80m, extractedInvoice.Total);
            Assert.Equal("Net 30 días", extractedInvoice.PaymentTerms);
            
            // Verify items extraction
            var lecheItem = extractedInvoice.Items.First(i => i.Product.Contains("Leche"));
            Assert.Equal(10, lecheItem.Quantity);
            Assert.Equal(25.00m, lecheItem.UnitPrice);
        }

        [Fact]
        public async Task PredictiveAnalytics_ShouldForecastCustomerChurn()
        {
            // Arrange - TDD for churn prediction
            var customerId = 1;
            var customer = new Customer
            {
                Id = customerId,
                CustomerName = "At-Risk Customer",
                CreatedAt = DateTime.Now.AddYears(-2) // Long-time customer
            };

            var recentOrders = new List<Order>
            {
                // Decreasing frequency and value - churn indicators
                new Order { CustomerId = customerId, CreatedAt = DateTime.Now.AddDays(-90), TotalAmount = 200 },
                new Order { CustomerId = customerId, CreatedAt = DateTime.Now.AddDays(-150), TotalAmount = 180 },
                new Order { CustomerId = customerId, CreatedAt = DateTime.Now.AddDays(-180), TotalAmount = 220 }
                // Gap in recent orders - high churn risk
            };

            // Act
            var churnPredictor = new CustomerChurnPredictor(_mockAIService.Object);
            var churnRisk = await churnPredictor.PredictChurnRisk(customerId, customer, recentOrders);

            // Assert
            Assert.NotNull(churnRisk);
            Assert.True(churnRisk.ChurnProbability >= 0.7); // High risk due to order gap
            Assert.Equal("High", churnRisk.RiskLevel);
            Assert.Contains("recent orders", churnRisk.RiskFactors);
            Assert.NotEmpty(churnRisk.RetentionStrategies);
            Assert.Contains(churnRisk.RetentionStrategies, strategy => 
                strategy.Contains("discount") || strategy.Contains("contact"));
        }

        [Fact]
        public async Task SeasonalityAnalysis_ShouldIdentifyProductTrends()
        {
            // Arrange - TDD for seasonal pattern detection
            var productId = 1; // Hot chocolate product
            var salesData = new List<MonthlySales>
            {
                new MonthlySales { Month = 1, Year = 2024, Quantity = 50 },  // January - High
                new MonthlySales { Month = 6, Year = 2024, Quantity = 10 },  // June - Low  
                new MonthlySales { Month = 12, Year = 2024, Quantity = 80 }, // December - Very High
                new MonthlySales { Month = 1, Year = 2025, Quantity = 55 }   // January - High again
            };

            // Act
            var seasonalityAnalyzer = new SeasonalityAnalyzer(_mockAIService.Object);
            var seasonalPattern = await seasonalityAnalyzer.AnalyzeProductSeasonality(productId, salesData);

            // Assert
            Assert.NotNull(seasonalPattern);
            Assert.Equal("Winter", seasonalPattern.PeakSeason);
            Assert.Equal("Summer", seasonalPattern.LowSeason);
            Assert.True(seasonalPattern.SeasonalityStrength > 0.6); // Strong seasonal pattern
            Assert.NotEmpty(seasonalPattern.PredictedDemand);
            
            // Should predict high demand for upcoming winter months
            var winterPrediction = seasonalPattern.PredictedDemand
                .FirstOrDefault(p => p.Month == 12 && p.Year == DateTime.Now.Year);
            if (winterPrediction != null)
            {
                Assert.True(winterPrediction.EstimatedQuantity > 50);
            }
        }

        [Fact]
        public async Task InventoryOptimization_ShouldSuggestOptimalStockLevels()
        {
            // Arrange - TDD for inventory optimization
            var product = new Product
            {
                Id = 1,
                ProductName = "Leche Avena Popular",
                StockQuantity = 25, // Current stock
                PricePublic = 30
            };

            var salesHistory = new List<DailySales>
            {
                new DailySales { Date = DateTime.Now.AddDays(-30), QuantitySold = 5 },
                new DailySales { Date = DateTime.Now.AddDays(-29), QuantitySold = 8 },
                new DailySales { Date = DateTime.Now.AddDays(-28), QuantitySold = 6 },
                // ... pattern shows average 7 units/day
            };

            var aiResponse = new AIResponse
            {
                Content = @"{
                    ""recommendedStockLevel"": 150,
                    ""reorderPoint"": 50,
                    ""averageDailySales"": 7.2,
                    ""leadTimeDays"": 5,
                    ""safetyStock"": 35,
                    ""reasoning"": ""Basado en ventas promedio de 7.2 unidades/día con lead time de 5 días, se recomienda mantener 150 unidades con punto de reorden en 50 unidades""
                }",
                IsSuccessful = true,
                UsedProvider = AIProvider.Claude
            };

            _mockAIService.Setup(x => x.GenerateTextAsync(It.IsAny<AIRequest>()))
                .ReturnsAsync(aiResponse);

            // Act
            var inventoryOptimizer = new InventoryOptimizationService(_mockAIService.Object);
            var optimization = await inventoryOptimizer.OptimizeStockLevel(product, salesHistory);

            // Assert
            Assert.NotNull(optimization);
            Assert.True(optimization.RecommendedStockLevel > product.StockQuantity);
            Assert.True(optimization.ReorderPoint < optimization.RecommendedStockLevel);
            Assert.True(optimization.SafetyStock > 0);
            Assert.NotEmpty(optimization.Reasoning);
            
            // Should trigger reorder alert if current stock below reorder point
            if (product.StockQuantity <= optimization.ReorderPoint)
            {
                Assert.True(optimization.ShouldReorder);
                Assert.NotEmpty(optimization.UrgencyLevel);
            }
        }
    }

    // DTOs y clases necesarias para los tests
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

    public class PurchasePattern
    {
        public double OrderFrequency { get; set; }
        public string OrderingCycle { get; set; } = string.Empty;
        public decimal AverageOrderValue { get; set; }
        public double GrowthTrend { get; set; }
        public string PreferredOrderingTime { get; set; } = string.Empty;
        public DateTime? PredictedNextOrderDate { get; set; }
    }

    public class CustomerSegment
    {
        public string SegmentName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public List<string> RecommendedStrategies { get; set; } = new();
        public decimal PredictedLifetimeValue { get; set; }
    }

    public class ExtractedInvoice
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public List<InvoiceItem> Items { get; set; } = new();
        public decimal Total { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
    }

    public class InvoiceItem
    {
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class ChurnRisk
    {
        public double ChurnProbability { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<string> RiskFactors { get; set; } = new();
        public List<string> RetentionStrategies { get; set; } = new();
    }

    public class SeasonalPattern
    {
        public string PeakSeason { get; set; } = string.Empty;
        public string LowSeason { get; set; } = string.Empty;
        public double SeasonalityStrength { get; set; }
        public List<SeasonalPrediction> PredictedDemand { get; set; } = new();
    }

    public class SeasonalPrediction
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int EstimatedQuantity { get; set; }
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

    public class MonthlySales
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int Quantity { get; set; }
    }

    public class DailySales
    {
        public DateTime Date { get; set; }
        public int QuantitySold { get; set; }
    }

    // Service interfaces que serán implementadas
    public interface IIntelligentRecommendationService
    {
        Task<List<IntelligentRecommendation>> GenerateRecommendations(int customerId);
    }

    public interface ICrossSellingAnalysisService
    {
        Task<List<ComplementaryProduct>> FindComplementaryProducts(int productId);
    }

    public interface IUpSellingService
    {
        Task<List<UpSellSuggestion>> FindPremiumAlternatives(int productId);
    }
}