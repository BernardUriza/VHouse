using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using VHouse.Domain.Entities;
using VHouse.Domain.Interfaces;
using VHouse.Infrastructure.Data;
using VHouse.Infrastructure.Repositories;
using VHouse.Infrastructure.Services;

namespace VHouse.Tests
{
    /// <summary>
    /// TDD Tests para Repository - Red, Green, Refactor
    /// Estos tests DEBEN FALLAR primero (Red) para seguir TDD correctamente
    /// </summary>
    public class RepositoryTests
    {
        private VHouseDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<VHouseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new VHouseDbContext(options);
        }

        [Fact]
        public async Task GetRecentOrdersByCustomerAsync_ShouldReturnRecentOrders_TDD_Red()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var repository = new Repository<Order>(context);
            var unitOfWork = new UnitOfWork(context);

            var customer = new Customer
            {
                CustomerName = "Test Customer",
                Email = "test@example.com",
                IsActive = true
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var orders = new List<Order>
            {
                new Order
                {
                    CustomerId = customer.Id,
                    TotalAmount = 100.00m,
                    OrderDate = DateTime.UtcNow.AddDays(-1), // Ayer
                    Status = Domain.Enums.OrderStatus.Completed
                },
                new Order
                {
                    CustomerId = customer.Id,
                    TotalAmount = 200.00m,
                    OrderDate = DateTime.UtcNow.AddDays(-5), // Hace 5 días
                    Status = Domain.Enums.OrderStatus.Completed
                },
                new Order
                {
                    CustomerId = customer.Id,
                    TotalAmount = 300.00m,
                    OrderDate = DateTime.UtcNow.AddDays(-15), // Hace 15 días
                    Status = Domain.Enums.OrderStatus.Completed
                }
            };

            context.Orders.AddRange(orders);
            await context.SaveChangesAsync();

            // Act & Assert - DEBE FALLAR (RED) porque el método no existe
            // Este test debe fallar para seguir TDD correctamente
            var orderRepository = unitOfWork.Orders;
            
            // Este método NO EXISTE aún - por eso el test falla (RED)
            var recentOrders = await orderRepository.GetRecentOrdersByCustomerAsync(customer.Id, 7); // Últimos 7 días

            // Assert
            Assert.Equal(2, recentOrders.Count()); // Solo 2 pedidos de los últimos 7 días
            Assert.All(recentOrders, order => Assert.True(order.OrderDate >= DateTime.UtcNow.AddDays(-7)));
        }

        [Fact]
        public async Task GetOrderCountByCustomerAsync_ShouldReturnCorrectCount_TDD_Red()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var unitOfWork = new UnitOfWork(context);

            var customer = new Customer
            {
                CustomerName = "Count Test Customer",
                Email = "count@example.com",
                IsActive = true
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var orders = new List<Order>
            {
                new Order { CustomerId = customer.Id, TotalAmount = 50.00m },
                new Order { CustomerId = customer.Id, TotalAmount = 75.00m },
                new Order { CustomerId = customer.Id, TotalAmount = 100.00m }
            };

            context.Orders.AddRange(orders);
            await context.SaveChangesAsync();

            // Act & Assert - DEBE FALLAR (RED) porque el método no existe
            var orderRepository = unitOfWork.Orders;
            
            // Este método NO EXISTE aún - por eso el test falla (RED)
            var orderCount = await orderRepository.GetOrderCountByCustomerAsync(customer.Id);

            // Assert
            Assert.Equal(3, orderCount);
        }

        [Fact]
        public async Task Repository_BasicOperations_ShouldWork_TDD_Green()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var repository = new Repository<Customer>(context);

            var customer = new Customer
            {
                CustomerName = "Basic Test Customer",
                Email = "basic@example.com",
                IsActive = true
            };

            // Act
            await repository.AddAsync(customer);
            await context.SaveChangesAsync();

            var retrievedCustomer = await repository.GetByIdAsync(customer.Id);

            // Assert
            Assert.NotNull(retrievedCustomer);
            Assert.Equal("Basic Test Customer", retrievedCustomer.CustomerName);
        }

        [Fact]
        public async Task UnitOfWork_Properties_ShouldBeAccessible_TDD_Green()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var unitOfWork = new UnitOfWork(context);

            // Act & Assert
            Assert.NotNull(unitOfWork.Products);
            Assert.NotNull(unitOfWork.Customers);
            Assert.NotNull(unitOfWork.Orders);
            
            // Verify types
            Assert.IsAssignableFrom<IProductRepository>(unitOfWork.Products);
            Assert.IsAssignableFrom<ICustomerRepository>(unitOfWork.Customers);
            Assert.IsAssignableFrom<IOrderRepository>(unitOfWork.Orders);
        }

        [Fact]
        public async Task ProductRepository_GetActiveProducts_ShouldWork_TDD_Green()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var unitOfWork = new UnitOfWork(context);

            var products = new List<Product>
            {
                new Product
                {
                    ProductName = "Active Product 1",
                    IsActive = true,
                    IsVegan = true,
                    PricePublic = 25.00m
                },
                new Product
                {
                    ProductName = "Inactive Product",
                    IsActive = false,
                    IsVegan = true,
                    PricePublic = 20.00m
                },
                new Product
                {
                    ProductName = "Active Product 2",
                    IsActive = true,
                    IsVegan = true,
                    PricePublic = 30.00m
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Act
            var activeProducts = await unitOfWork.Products.GetActiveProductsAsync();

            // Assert
            Assert.Equal(2, activeProducts.Count());
            Assert.All(activeProducts, p => Assert.True(p.IsActive));
        }

        [Fact]
        public async Task AIService_PredictDemandAsync_ShouldReturnForecast_TDD_Red()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var mockLogger = new Mock<ILogger<AIService>>();
            var mockConfig = new Mock<IConfiguration>();
            var mockHttp = new Mock<HttpClient>();
            
            var aiService = new AIService(mockHttp.Object, mockLogger.Object, mockConfig.Object);
            
            var historicalData = new List<object>
            {
                new { Date = DateTime.UtcNow.AddDays(-30), Quantity = 100 },
                new { Date = DateTime.UtcNow.AddDays(-20), Quantity = 120 },
                new { Date = DateTime.UtcNow.AddDays(-10), Quantity = 110 }
            };

            // Act & Assert - DEBE FALLAR (RED) porque el método no está implementado
            var forecast = await aiService.PredictDemandAsync(1, 7, historicalData);
            
            // Assert
            Assert.NotNull(forecast);
            Assert.Equal(1, forecast.ProductId);
            Assert.Equal(7, forecast.DaysPredicted);
            Assert.True(forecast.Predictions.Count > 0);
            Assert.True(forecast.ConfidenceScore >= 0 && forecast.ConfidenceScore <= 1);
        }

        [Fact]
        public async Task AIService_OptimizeInventoryAsync_ShouldReturnOptimization_TDD_Red()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var mockLogger = new Mock<ILogger<AIService>>();
            var mockConfig = new Mock<IConfiguration>();
            var mockHttp = new Mock<HttpClient>();
            
            var aiService = new AIService(mockHttp.Object, mockLogger.Object, mockConfig.Object);
            
            var inventoryData = new List<object>
            {
                new { ProductId = 1, CurrentStock = 50, MinStock = 10 },
                new { ProductId = 2, CurrentStock = 5, MinStock = 15 }
            };
            
            var salesData = new List<object>
            {
                new { ProductId = 1, WeeklySales = 20 },
                new { ProductId = 2, WeeklySales = 25 }
            };

            // Act & Assert - DEBE FALLAR (RED) porque el método no está implementado
            var optimization = await aiService.OptimizeInventoryAsync(inventoryData, salesData);
            
            // Assert
            Assert.NotNull(optimization);
            Assert.True(optimization.Recommendations.Count > 0);
            Assert.True(optimization.OptimizationScore >= 0);
            Assert.Contains("restock", optimization.Summary.ToLower());
        }

        [Fact] 
        public async Task AIService_GenerateBusinessInsightsAsync_ShouldReturnInsights_TDD_Red()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var mockLogger = new Mock<ILogger<AIService>>();
            var mockConfig = new Mock<IConfiguration>();
            var mockHttp = new Mock<HttpClient>();
            
            var aiService = new AIService(mockHttp.Object, mockLogger.Object, mockConfig.Object);
            
            var businessData = new 
            {
                TotalSales = 15000.00m,
                TopProducts = new[] { "Oat Milk", "Vegan Cheese" },
                MonthlyGrowth = 12.5,
                CustomerCount = 450
            };

            // Act & Assert - DEBE FALLAR (RED) porque el método no está implementado  
            var insights = await aiService.GenerateBusinessInsightsAsync(businessData);
            
            // Assert
            Assert.NotNull(insights);
            Assert.True(insights.KeyInsights.Count > 0);
            Assert.True(insights.RecommendedActions.Count > 0);
            Assert.NotEmpty(insights.Summary);
            Assert.True(insights.AnalysisScore >= 0);
        }
    }
}