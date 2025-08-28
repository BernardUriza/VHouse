using Microsoft.EntityFrameworkCore;
using VHouse.Core.Entities;

namespace VHouse.Tests
{
    public class SimpleOrderTests
    {
        private DbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new DbContext(options);
            
            // Setup DbSets manually for testing
            return context;
        }

        [Fact]
        public void Order_CalculateTotalPrice_ShouldReturnCorrectSum()
        {
            // Arrange
            var order = new Order
            {
                Items = new List<OrderItem>
                {
                    new OrderItem { Price = 10.00m, Quantity = 2 },
                    new OrderItem { Price = 15.50m, Quantity = 1 },
                    new OrderItem { Price = 8.25m, Quantity = 3 }
                }
            };

            // Act
            var totalCalculated = order.Items.Sum(i => i.TotalPrice);
            
            // Assert
            Assert.Equal(60.25m, totalCalculated); // (10*2) + (15.50*1) + (8.25*3)
        }

        [Fact]
        public void OrderItem_TotalPrice_ShouldMultiplyPriceAndQuantity()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                Price = 12.50m,
                Quantity = 4
            };

            // Act
            var totalPrice = orderItem.TotalPrice;

            // Assert
            Assert.Equal(50.00m, totalPrice);
        }

        [Fact]
        public void Customer_ShouldCreateWithDefaults()
        {
            // Arrange & Act
            var customer = new Customer
            {
                FullName = "Test Customer",
                Email = "test@example.com",
                Phone = "555-1234"
            };

            // Assert
            Assert.NotNull(customer);
            Assert.Equal("Test Customer", customer.FullName);
            Assert.Equal("test@example.com", customer.Email);
            Assert.False(customer.IsRetail); // Default value
            Assert.Empty(customer.Orders);
        }

        [Fact]
        public void Product_ShouldCreateWithValidProperties()
        {
            // Arrange & Act
            var product = new Product
            {
                ProductName = "Test Vegan Product",
                Emoji = "🌱",
                SKU = "TEST-001",
                PriceCost = 10.00m,
                PriceRetail = 15.00m,
                PriceSuggested = 20.00m,
                PricePublic = 18.00m,
                Description = "Test product description",
                IsActive = true,
                Score = 85
            };

            // Assert
            Assert.NotNull(product);
            Assert.Equal("Test Vegan Product", product.ProductName);
            Assert.True(product.IsActive);
            Assert.Equal(85, product.Score);
            Assert.True(product.PriceRetail > product.PriceCost);
            Assert.True(product.PriceSuggested > product.PriceRetail);
        }

        [Fact]
        public void AIOrder_SelectHighScoringProducts_ShouldWork()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "High Score", Score = 95, PricePublic = 25.00m, IsActive = true },
                new Product { ProductId = 2, ProductName = "Medium Score", Score = 75, PricePublic = 20.00m, IsActive = true },
                new Product { ProductId = 3, ProductName = "Low Score", Score = 45, PricePublic = 15.00m, IsActive = true }
            };

            // Act - Simulate AI selection logic
            var aiSelected = products
                .Where(p => p.IsActive && p.Score >= 80)
                .OrderByDescending(p => p.Score)
                .Take(2)
                .ToList();

            // Assert
            Assert.Single(aiSelected); // Only one product has score >= 80
            Assert.Equal("High Score", aiSelected.First().ProductName);
            Assert.Equal(95, aiSelected.First().Score);
        }

        [Fact]
        public void AIOrder_OptimizeForBudget_ShouldSelectBestValue()
        {
            // Arrange
            var budget = 100.00m;
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Premium", PricePublic = 30.00m, Score = 95, IsActive = true },
                new Product { ProductId = 2, ProductName = "Standard", PricePublic = 15.00m, Score = 85, IsActive = true },
                new Product { ProductId = 3, ProductName = "Basic", PricePublic = 8.00m, Score = 70, IsActive = true }
            };

            // Act - Simulate AI budget optimization
            var optimizedSelection = new List<(Product product, int quantity)>();
            var remainingBudget = budget;

            // AI logic: Select products by value (score per dollar) within budget
            var sortedByValue = products
                .Where(p => p.IsActive && p.PricePublic <= remainingBudget)
                .OrderByDescending(p => p.Score / p.PricePublic)
                .ToList();

            foreach (var product in sortedByValue)
            {
                var maxQuantity = (int)(remainingBudget / product.PricePublic);
                if (maxQuantity > 0)
                {
                    var optimalQuantity = Math.Min(maxQuantity, 2); // Limit for variety
                    optimizedSelection.Add((product, optimalQuantity));
                    remainingBudget -= optimalQuantity * product.PricePublic;
                }
            }

            var totalCost = optimizedSelection.Sum(s => s.product.PricePublic * s.quantity);

            // Assert
            Assert.True(totalCost <= budget);
            Assert.True(optimizedSelection.Count > 1); // Multiple products selected
            Assert.Contains(optimizedSelection, s => s.product.ProductName == "Standard"); // Best value should be included
        }
    }
}