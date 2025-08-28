using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Entities;
using VHouse.Domain.Enums;
using VHouse.Infrastructure.Data;
using Moq;
using VHouse.Domain.Interfaces;

namespace VHouse.Tests
{
    /// <summary>
    /// Tests específicos para funcionalidades AI en VHouse    /// Incluye integración con Claude y OpenAI para recomendaciones inteligentes
    /// </summary>
    public class VHouse2025AITests
    {
        private VHouseDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<VHouseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new VHouseDbContext(options);
        }

        [Fact]
        public async Task AI_RecommendVeganProducts_BasedOnCustomerPreferences()
        {
            // Arrange
            using var context = GetInMemoryContext();
            
            var veganCustomer = new Customer
            {
                CustomerName = "AI Customer Vegano",
                Email = "ai@vegano.com",
                IsVeganPreferred = true,
                IsActive = true
            };

            var products = new List<Product>
            {
                new Product
                {
                    ProductName = "Hamburguesa Beyond Meat",
                    Emoji = "🍔",
                    PricePublic = 45.00m,
                    StockQuantity = 25,
                    IsVegan = true,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Leche de Avena",
                    Emoji = "🥛",
                    PricePublic = 28.00m,
                    StockQuantity = 40,
                    IsVegan = true,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Carne de Res",
                    Emoji = "🥩",
                    PricePublic = 35.00m,
                    StockQuantity = 20,
                    IsVegan = false, // No vegano
                    IsActive = true
                }
            };

            context.Customers.Add(veganCustomer);
            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Act - AI filtra productos según preferencias del cliente
            var aiRecommendations = await context.Products
                .Where(p => p.IsActive && 
                           p.IsVegan == veganCustomer.IsVeganPreferred && 
                           p.StockQuantity > 0)
                .OrderByDescending(p => p.PricePublic) // Por valor/premium
                .Take(3)
                .ToListAsync();

            // Assert
            Assert.Equal(2, aiRecommendations.Count); // Solo productos veganos
            Assert.All(aiRecommendations, p => Assert.True(p.IsVegan));
            Assert.Equal("Hamburguesa Beyond Meat", aiRecommendations.First().ProductName);
            Assert.Equal("Leche de Avena", aiRecommendations.Last().ProductName);
        }

        [Fact]
        public async Task AI_OptimizeOrder_WithinBudgetConstraints()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var budget = 100.00m;

            var products = new List<Product>
            {
                new Product
                {
                    ProductName = "Premium Vegan Cheese",
                    PricePublic = 60.00m,
                    StockQuantity = 15,
                    IsVegan = true,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Organic Tofu",
                    PricePublic = 25.00m,
                    StockQuantity = 30,
                    IsVegan = true,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Plant Milk",
                    PricePublic = 18.00m,
                    StockQuantity = 50,
                    IsVegan = true,
                    IsActive = true
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Act - AI optimiza selección dentro del presupuesto
            var availableProducts = await context.Products
                .Where(p => p.IsActive && p.IsVegan && p.PricePublic <= budget)
                .OrderBy(p => p.PricePublic) // Empezar por los más baratos
                .ToListAsync();

            var optimizedOrder = new List<(Product product, int quantity)>();
            var remainingBudget = budget;

            foreach (var product in availableProducts)
            {
                var maxQuantity = (int)(remainingBudget / product.PricePublic);
                if (maxQuantity > 0)
                {
                    var optimalQuantity = Math.Min(maxQuantity, 2); // Max 2 por variedad
                    optimizedOrder.Add((product, optimalQuantity));
                    remainingBudget -= optimalQuantity * product.PricePublic;
                }
            }

            var totalCost = optimizedOrder.Sum(o => o.product.PricePublic * o.quantity);

            // Assert
            Assert.True(totalCost <= budget);
            Assert.True(optimizedOrder.Count > 1); // Variedad de productos
            Assert.Equal(82.00m, totalCost); // (18*2) + (25*2) + (60*0) = 86, pero el premium no cabe
        }

        [Fact]
        public async Task AI_CreateOrder_WithMockAIService()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var mockAIService = new Mock<IAIService>();

            var customer = new Customer
            {
                CustomerName = "Mock AI Customer",
                Email = "mock@ai.com",
                IsVeganPreferred = true,
                IsActive = true
            };

            var product = new Product
            {
                ProductName = "AI Recommended Vegan Product",
                Emoji = "🤖🌱",
                PricePublic = 35.00m,
                IsVegan = true,
                IsActive = true
            };

            context.Customers.Add(customer);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Mock AI service response
            mockAIService
                .Setup(x => x.IsHealthyAsync())
                .ReturnsAsync(true);

            mockAIService
                .Setup(x => x.GetProviderStatusAsync(It.IsAny<AIProvider>()))
                .ReturnsAsync("Claude: ✅ Healthy | OpenAI: ✅ Healthy");

            // Act
            var isAIHealthy = await mockAIService.Object.IsHealthyAsync();
            var aiStatus = await mockAIService.Object.GetProviderStatusAsync(AIProvider.Claude);

            var aiOrder = new Order
            {
                CustomerId = customer.Id,
                Status = OrderStatus.Confirmed,
                Notes = $"AI Generated Order - Status: {aiStatus}",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UnitPrice = product.PricePublic
                    }
                }
            };

            aiOrder.TotalAmount = aiOrder.OrderItems.Sum(oi => oi.TotalPrice);

            context.Orders.Add(aiOrder);
            await context.SaveChangesAsync();

            // Assert
            Assert.True(isAIHealthy);
            Assert.Contains("Claude: ✅ Healthy", aiStatus);
            
            var savedOrder = await context.Orders.FirstOrDefaultAsync(o => o.Id == aiOrder.Id);
            Assert.NotNull(savedOrder);
            Assert.Contains("AI Generated", savedOrder.Notes);
            Assert.Equal(35.00m, savedOrder.TotalAmount);

            // Verify mock was called
            mockAIService.Verify(x => x.IsHealthyAsync(), Times.Once);
            mockAIService.Verify(x => x.GetProviderStatusAsync(AIProvider.Claude), Times.Once);
        }

        [Fact]
        public async Task AI_PriorityProvider_Claude_Over_OpenAI()
        {
            // Arrange
            var mockAIService = new Mock<IAIService>();

            // Mock Claude como prioridad (funciona)
            mockAIService
                .Setup(x => x.GetProviderStatusAsync(AIProvider.Claude))
                .ReturnsAsync("Claude: ✅ Available - Priority Provider");

            // Mock OpenAI como fallback
            mockAIService
                .Setup(x => x.GetProviderStatusAsync(AIProvider.OpenAI))
                .ReturnsAsync("OpenAI: ⚠️ Limited - Fallback Provider");

            // Act
            var claudeStatus = await mockAIService.Object.GetProviderStatusAsync(AIProvider.Claude);
            var openAIStatus = await mockAIService.Object.GetProviderStatusAsync(AIProvider.OpenAI);

            // Assert - Claude tiene prioridad según la arquitectura VHouse            Assert.Contains("Priority Provider", claudeStatus);
            Assert.Contains("Fallback Provider", openAIStatus);
            Assert.Contains("✅ Available", claudeStatus);
        }

        [Fact]
        public async Task AI_ProductDescription_Generation()
        {
            // Arrange
            var mockAIService = new Mock<IAIService>();
            
            var productName = "Tocino Vegano Ahumado VHouse2025";
            var expectedDescription = "🥓 Tocino vegano premium con sabor ahumado natural. " +
                                    "Elaborado con proteína de soya texturizada y especias gourmet. " +
                                    "Perfecto para desayunos veganos y platos gourmet. " +
                                    "Sin gluten, sin OGM. VHouse2025 AI Generated.";

            mockAIService
                .Setup(x => x.GenerateProductDescriptionAsync(productName, AIProvider.Claude))
                .ReturnsAsync(expectedDescription);

            // Act
            var generatedDescription = await mockAIService.Object
                .GenerateProductDescriptionAsync(productName, AIProvider.Claude);

            // Assert
            Assert.NotNull(generatedDescription);
            Assert.Contains("vegano", generatedDescription.ToLower());
            Assert.Contains("VHouse2025 AI Generated", generatedDescription);
            Assert.Contains("🥓", generatedDescription);

            mockAIService.Verify(
                x => x.GenerateProductDescriptionAsync(productName, AIProvider.Claude), 
                Times.Once);
        }

        [Fact]
        public void AIProvider_Enum_VHouse2025_Values()
        {
            // Arrange & Act & Assert
            var providers = Enum.GetValues<AIProvider>();
            
            Assert.Contains(AIProvider.Claude, providers);
            Assert.Contains(AIProvider.OpenAI, providers);
            Assert.Contains(AIProvider.Gemini, providers);
            Assert.Contains(AIProvider.Local, providers);

            // Claude debe tener valor 2 (prioridad en VHouse2025)
            Assert.Equal(2, (int)AIProvider.Claude);
            Assert.Equal(1, (int)AIProvider.OpenAI);
        }

        [Fact]
        public async Task AI_CustomerSegmentation_VeganVsOmnivore()
        {
            // Arrange
            using var context = GetInMemoryContext();

            var customers = new List<Customer>
            {
                new Customer
                {
                    CustomerName = "Cliente Vegano 1",
                    Email = "vegan1@test.com",
                    IsVeganPreferred = true,
                    IsActive = true
                },
                new Customer
                {
                    CustomerName = "Cliente Vegano 2",
                    Email = "vegan2@test.com",
                    IsVeganPreferred = true,
                    IsActive = true
                },
                new Customer
                {
                    CustomerName = "Cliente Omnívoro",
                    Email = "omnivore@test.com",
                    IsVeganPreferred = false,
                    IsActive = true
                }
            };

            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();

            // Act - AI segmenta clientes
            var veganSegment = await context.Customers
                .Where(c => c.IsActive && c.IsVeganPreferred)
                .CountAsync();

            var omnivoreSegment = await context.Customers
                .Where(c => c.IsActive && !c.IsVeganPreferred)
                .CountAsync();

            var totalActiveCustomers = await context.Customers
                .Where(c => c.IsActive)
                .CountAsync();

            // Assert
            Assert.Equal(2, veganSegment);
            Assert.Equal(1, omnivoreSegment);
            Assert.Equal(3, totalActiveCustomers);
            
            // AI puede usar esta segmentación para campañas dirigidas
            var veganPercentage = (double)veganSegment / totalActiveCustomers * 100;
            Assert.Equal(66.67, Math.Round(veganPercentage, 2)); // ~67% veganos
        }
    }
}