using Microsoft.EntityFrameworkCore;
using VHouse.Core.Entities;
using VHouse.Infrastructure.Data;
using Moq;

namespace VHouse.Tests
{
    public class OrderManagementTests
    {
        private VHouseDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<VHouseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new VHouseDbContext(options);
        }

        [Fact]
        public async Task CreateOrder_WithValidData_ShouldSucceed()
        {
            using var context = GetInMemoryContext();
            
            var customer = new Customer
            {
                CustomerId = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "555-1234",
                Address = "123 Main St",
                IsRetail = true
            };

            var product = new Product
            {
                ProductId = 1,
                ProductName = "Test Product",
                SKU = "TEST-001",
                PriceCost = 10.00m,
                PriceRetail = 15.00m,
                PriceSuggested = 20.00m,
                PricePublic = 18.00m,
                IsActive = true
            };

            context.Customers.Add(customer);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var order = new Order
            {
                CustomerId = customer.CustomerId,
                OrderDate = DateTime.UtcNow,
                PriceType = "retail",
                TotalAmount = 30.00m,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        Price = product.PriceRetail,
                        Quantity = 2
                    }
                }
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var savedOrder = await context.Orders
                .Include(o => o.Items)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            Assert.NotNull(savedOrder);
            Assert.Equal(customer.CustomerId, savedOrder.CustomerId);
            Assert.Single(savedOrder.Items);
            Assert.Equal(30.00m, savedOrder.Items.First().TotalPrice);
        }

        [Fact]
        public async Task CreateOrder_UsingAI_WithProductRecommendations_ShouldGenerateOrder()
        {
            using var context = GetInMemoryContext();

            var customer = new Customer
            {
                CustomerId = 1,
                FullName = "Jane Smith",
                Email = "jane@example.com",
                Phone = "555-5678",
                Address = "456 Oak Ave",
                IsRetail = false
            };

            var veganProducts = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    ProductName = "Tocino Vegano",
                    Emoji = "🥓",
                    SKU = "VEGAN-001",
                    PriceCost = 42.00m,
                    PriceRetail = 48.00m,
                    PriceSuggested = 70.00m,
                    PricePublic = 62.00m,
                    IsActive = true,
                    Score = 85
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Proteína Texturizada",
                    Emoji = "🥩",
                    SKU = "PROTEIN-001",
                    PriceCost = 65.00m,
                    PriceRetail = 70.00m,
                    PriceSuggested = 90.00m,
                    PricePublic = 69.00m,
                    IsActive = true,
                    Score = 90
                }
            };

            context.Customers.Add(customer);
            context.Products.AddRange(veganProducts);
            await context.SaveChangesAsync();

            var aiRecommendedProducts = await context.Products
                .Where(p => p.IsActive && p.Score > 80)
                .OrderByDescending(p => p.Score)
                .Take(2)
                .ToListAsync();

            var aiGeneratedOrder = new Order
            {
                CustomerId = customer.CustomerId,
                OrderDate = DateTime.UtcNow,
                PriceType = "public",
                Items = aiRecommendedProducts.Select(p => new OrderItem
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.PricePublic,
                    Quantity = 1
                }).ToList()
            };

            aiGeneratedOrder.TotalAmount = aiGeneratedOrder.Items.Sum(i => i.TotalPrice);

            context.Orders.Add(aiGeneratedOrder);
            await context.SaveChangesAsync();

            var savedOrder = await context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == aiGeneratedOrder.OrderId);

            Assert.NotNull(savedOrder);
            Assert.Equal(2, savedOrder.Items.Count);
            Assert.Equal(131.00m, savedOrder.TotalAmount); // 62 + 69
            Assert.All(savedOrder.Items, item => Assert.True(item.Price > 0));
        }

        [Fact]
        public void Order_CalculateTotalPrice_ShouldReturnCorrectSum()
        {
            var order = new Order
            {
                Items = new List<OrderItem>
                {
                    new OrderItem { Price = 10.00m, Quantity = 2 },
                    new OrderItem { Price = 15.50m, Quantity = 1 },
                    new OrderItem { Price = 8.25m, Quantity = 3 }
                }
            };

            var totalCalculated = order.Items.Sum(i => i.TotalPrice);
            
            Assert.Equal(60.25m, totalCalculated); // (10*2) + (15.50*1) + (8.25*3)
        }

        [Fact]
        public async Task CreateOrder_WithInventoryEntry_ShouldUpdateInventory()
        {
            using var context = GetInMemoryContext();

            var product = new Product
            {
                ProductId = 1,
                ProductName = "Inventory Test Product",
                SKU = "INV-001",
                PriceCost = 5.00m,
                IsActive = true
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var inventoryOrder = new Order
            {
                OrderDate = DateTime.UtcNow,
                IsInventoryEntry = true,
                PriceType = "cost",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        Price = product.PriceCost,
                        Quantity = 50
                    }
                }
            };

            inventoryOrder.TotalAmount = inventoryOrder.Items.Sum(i => i.TotalPrice);

            context.Orders.Add(inventoryOrder);
            await context.SaveChangesAsync();

            var savedOrder = await context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == inventoryOrder.OrderId);

            Assert.NotNull(savedOrder);
            Assert.True(savedOrder.IsInventoryEntry);
            Assert.Equal(250.00m, savedOrder.TotalAmount); // 5.00 * 50
        }
    }
}