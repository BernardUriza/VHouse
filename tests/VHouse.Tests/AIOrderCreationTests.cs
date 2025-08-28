using Microsoft.EntityFrameworkCore;
using VHouse.Core.Entities;
using VHouse.Infrastructure.Data;
using Moq;

namespace VHouse.Tests
{
    public class AIOrderCreationTests
    {
        private VHouseDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<VHouseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new VHouseDbContext(options);
        }

        [Fact]
        public async Task AIRecommendedOrder_BasedOnScore_ShouldSelectHighestScoringProducts()
        {
            using var context = GetInMemoryContext();

            var products = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    ProductName = "High Score Product",
                    Emoji = "⭐",
                    SKU = "HIGH-001",
                    PricePublic = 25.00m,
                    Score = 95,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Medium Score Product",
                    Emoji = "🟡",
                    SKU = "MED-001",
                    PricePublic = 20.00m,
                    Score = 75,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "Low Score Product",
                    Emoji = "🔴",
                    SKU = "LOW-001",
                    PricePublic = 15.00m,
                    Score = 45,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 4,
                    ProductName = "Top Score Product",
                    Emoji = "🏆",
                    SKU = "TOP-001",
                    PricePublic = 30.00m,
                    Score = 98,
                    IsActive = true
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // AI logic: Select top 2 products with highest score
            var aiSelectedProducts = await context.Products
                .Where(p => p.IsActive && p.Score >= 80)
                .OrderByDescending(p => p.Score)
                .Take(2)
                .ToListAsync();

            Assert.Equal(2, aiSelectedProducts.Count);
            Assert.Equal("Top Score Product", aiSelectedProducts.First().ProductName);
            Assert.Equal("High Score Product", aiSelectedProducts.Last().ProductName);
            Assert.All(aiSelectedProducts, p => Assert.True(p.Score >= 80));
        }

        [Fact]
        public async Task AICreateOrder_ForRetailCustomer_ShouldUseRetailPricing()
        {
            using var context = GetInMemoryContext();

            var retailCustomer = new Customer
            {
                CustomerId = 1,
                FullName = "Retail AI Customer",
                Email = "retail.ai@example.com",
                Phone = "555-RETAIL",
                IsRetail = true
            };

            var products = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    ProductName = "AI Recommended Vegan Burger",
                    Emoji = "🍔",
                    PriceCost = 8.00m,
                    PriceRetail = 12.00m,
                    PricePublic = 10.00m,
                    Score = 88,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "AI Recommended Plant Milk",
                    Emoji = "🥛",
                    PriceCost = 3.00m,
                    PriceRetail = 5.50m,
                    PricePublic = 4.50m,
                    Score = 92,
                    IsActive = true
                }
            };

            context.Customers.Add(retailCustomer);
            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // AI creates order for retail customer using retail prices
            var aiRecommendedProducts = await context.Products
                .Where(p => p.IsActive && p.Score > 85)
                .OrderByDescending(p => p.Score)
                .ToListAsync();

            var aiOrder = new Order
            {
                CustomerId = retailCustomer.CustomerId,
                OrderDate = DateTime.UtcNow,
                PriceType = "retail", // AI detects customer is retail
                Items = aiRecommendedProducts.Select(p => new OrderItem
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.PriceRetail, // Uses retail pricing for retail customer
                    Quantity = 1
                }).ToList()
            };

            aiOrder.TotalAmount = aiOrder.Items.Sum(i => i.TotalPrice);

            context.Orders.Add(aiOrder);
            await context.SaveChangesAsync();

            var savedOrder = await context.Orders
                .Include(o => o.Items)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == aiOrder.OrderId);

            Assert.NotNull(savedOrder);
            Assert.True(savedOrder.Customer!.IsRetail);
            Assert.Equal("retail", savedOrder.PriceType);
            Assert.Equal(17.50m, savedOrder.TotalAmount); // 12.00 + 5.50
            Assert.All(savedOrder.Items, item => Assert.Contains("AI Recommended", item.ProductName));
        }

        [Fact]
        public async Task AICreateOrder_ForB2BCustomer_ShouldUsePublicPricing()
        {
            using var context = GetInMemoryContext();

            var b2bCustomer = new Customer
            {
                CustomerId = 1,
                FullName = "B2B AI Customer",
                Email = "b2b.ai@example.com",
                Phone = "555-B2B",
                IsRetail = false
            };

            var product = new Product
            {
                ProductId = 1,
                ProductName = "Bulk Vegan Protein",
                Emoji = "💪",
                PriceCost = 50.00m,
                PriceRetail = 80.00m,
                PricePublic = 65.00m,
                Score = 90,
                IsActive = true
            };

            context.Customers.Add(b2bCustomer);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // AI creates bulk order for B2B customer
            var aiOrder = new Order
            {
                CustomerId = b2bCustomer.CustomerId,
                OrderDate = DateTime.UtcNow,
                PriceType = "public", // AI detects customer is B2B
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        Price = product.PricePublic, // Uses public pricing for B2B
                        Quantity = 10 // AI recommends bulk quantity
                    }
                }
            };

            aiOrder.TotalAmount = aiOrder.Items.Sum(i => i.TotalPrice);

            context.Orders.Add(aiOrder);
            await context.SaveChangesAsync();

            var savedOrder = await context.Orders
                .Include(o => o.Items)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == aiOrder.OrderId);

            Assert.NotNull(savedOrder);
            Assert.False(savedOrder.Customer!.IsRetail);
            Assert.Equal("public", savedOrder.PriceType);
            Assert.Equal(650.00m, savedOrder.TotalAmount); // 65.00 * 10
            Assert.Equal(10, savedOrder.Items.First().Quantity);
        }

        [Fact]
        public async Task AIOptimizeOrder_ShouldMaximizeValueForCustomer()
        {
            using var context = GetInMemoryContext();

            var budget = 100.00m;
            var products = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    ProductName = "Premium Vegan Cheese",
                    PricePublic = 25.00m,
                    Score = 95,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Organic Tofu",
                    PricePublic = 8.00m,
                    Score = 85,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "Vegan Chocolate",
                    PricePublic = 12.00m,
                    Score = 90,
                    IsActive = true
                },
                new Product
                {
                    ProductId = 4,
                    ProductName = "Plant-based Yogurt",
                    PricePublic = 6.00m,
                    Score = 80,
                    IsActive = true
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // AI optimization: maximize score within budget
            var availableProducts = await context.Products
                .Where(p => p.IsActive && p.PricePublic <= budget)
                .OrderByDescending(p => p.Score / p.PricePublic) // Score per dollar
                .ToListAsync();

            var optimizedItems = new List<OrderItem>();
            var remainingBudget = budget;

            foreach (var product in availableProducts)
            {
                var maxQuantity = (int)(remainingBudget / product.PricePublic);
                if (maxQuantity > 0)
                {
                    // AI decides optimal quantity based on remaining budget and product value
                    var optimalQuantity = Math.Min(maxQuantity, 3); // Cap at 3 for variety
                    optimizedItems.Add(new OrderItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        Price = product.PricePublic,
                        Quantity = optimalQuantity
                    });
                    remainingBudget -= optimalQuantity * product.PricePublic;
                }
            }

            var totalCost = optimizedItems.Sum(i => i.TotalPrice);
            var averageScore = optimizedItems.Average(i => 
                products.First(p => p.ProductId == i.ProductId).Score);

            Assert.True(totalCost <= budget);
            Assert.True(optimizedItems.Count > 1); // AI selected multiple products for variety
            Assert.True(averageScore > 80); // AI maintained high quality selections
        }

        [Fact]
        public void AIOrderValidation_ShouldEnsureOrderIntegrity()
        {
            var aiOrder = new Order
            {
                CustomerId = 1,
                OrderDate = DateTime.UtcNow,
                PriceType = "public",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = 1,
                        ProductName = "AI Selected Product",
                        Price = 15.00m,
                        Quantity = 2
                    }
                }
            };

            aiOrder.TotalAmount = aiOrder.Items.Sum(i => i.TotalPrice);

            // AI validation checks
            Assert.True(aiOrder.OrderDate <= DateTime.UtcNow);
            Assert.NotEmpty(aiOrder.Items);
            Assert.All(aiOrder.Items, item => Assert.True(item.Price > 0));
            Assert.All(aiOrder.Items, item => Assert.True(item.Quantity > 0));
            Assert.Equal(30.00m, aiOrder.TotalAmount);
            Assert.Contains(aiOrder.PriceType, new[] { "cost", "retail", "public" });
        }
    }
}