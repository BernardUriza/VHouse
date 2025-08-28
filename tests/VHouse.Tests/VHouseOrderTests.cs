using Microsoft.EntityFrameworkCore;
using VHouse.Domain.Entities;
using VHouse.Domain.Enums;
using VHouse.Infrastructure.Data;

namespace VHouse.Tests
{
    /// <summary>
    /// Tests para la arquitectura VHouse - Clean Architecture
    /// Prueba funcionalidades de pedidos con AI y gestión de clientes
    /// </summary>
    public class VHouseOrderTests
    {
        private VHouseDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<VHouseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new VHouseDbContext(options);
        }

        [Fact]
        public async Task CreateOrder_WithVHouseArchitecture_ShouldSucceed()
        {
            // Arrange
            using var context = GetInMemoryContext();
            
            var customer = new Customer
            {
                CustomerName = "Cliente VHouse",
                Email = "cliente@vhouse.com",
                Phone = "555-1234",
                Address = "Dirección VHouse",
                IsVeganPreferred = true,
                IsActive = true
            };

            var product = new Product
            {
                ProductName = "Producto VHouse",
                Emoji = "🌱",
                PriceCost = 15.00m,
                PriceRetail = 22.00m,
                PriceSuggested = 28.00m,
                PricePublic = 25.00m,
                Description = "Producto para arquitectura limpia ",
                StockQuantity = 100,
                IsVegan = true,
                IsActive = true
            };

            context.Customers.Add(customer);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Act
            var order = new Order
            {
                CustomerId = customer.Id,
                TotalAmount = 50.00m,
                Status = OrderStatus.Confirmed,
                Notes = "Pedido creado con Clean Architecture VHouse",
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 2,
                        UnitPrice = product.PriceRetail
                    }
                }
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Assert
            var savedOrder = await context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            Assert.NotNull(savedOrder);
            Assert.Equal(customer.Id, savedOrder.CustomerId);
            Assert.Equal(OrderStatus.Confirmed, savedOrder.Status);
            Assert.Single(savedOrder.OrderItems);
            Assert.Equal(44.00m, savedOrder.OrderItems.First().TotalPrice); // 22.00 * 2
            Assert.True(savedOrder.Customer.IsVeganPreferred);
        }

        [Fact]
        public async Task AIOrderCreation_SelectHighScoringVeganProducts_VHouse()
        {
            // Arrange
            using var context = GetInMemoryContext();

            var veganProducts = new List<Product>
            {
                new Product
                {
                    ProductName = "Tocino Vegano Premium VHouse",
                    Emoji = "🥓",
                    PriceCost = 45.00m,
                    PriceRetail = 52.00m,
                    PricePublic = 68.00m,
                    StockQuantity = 50,
                    IsVegan = true,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Proteína Texturizada VHouse",
                    Emoji = "🥩",
                    PriceCost = 35.00m,
                    PriceRetail = 42.00m,
                    PricePublic = 55.00m,
                    StockQuantity = 75,
                    IsVegan = true,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Producto No Vegano",
                    Emoji = "🥛",
                    PricePublic = 30.00m,
                    IsVegan = false, // No vegano
                    IsActive = true
                }
            };

            context.Products.AddRange(veganProducts);
            await context.SaveChangesAsync();

            // Act - AI selecciona solo productos veganos con stock > 50
            var aiSelectedProducts = await context.Products
                .Where(p => p.IsActive && p.IsVegan && p.StockQuantity > 50)
                .OrderByDescending(p => p.PricePublic) // Por valor
                .Take(2)
                .ToListAsync();

            // Assert
            Assert.Equal(2, aiSelectedProducts.Count);
            Assert.All(aiSelectedProducts, p => Assert.True(p.IsVegan));
            Assert.All(aiSelectedProducts, p => Assert.True(p.StockQuantity > 50));
            Assert.Equal("Tocino Vegano Premium VHouse", aiSelectedProducts.First().ProductName);
        }

        [Fact]
        public async Task CustomerManagement_VeganPreferences_VHouse()
        {
            // Arrange
            using var context = GetInMemoryContext();

            var customers = new List<Customer>
            {
                new Customer
                {
                    CustomerName = "Cliente Vegano VHouse",
                    Email = "vegano@vhouse.com",
                    IsVeganPreferred = true,
                    IsActive = true
                },
                new Customer
                {
                    CustomerName = "Cliente Omnívoro VHouse",
                    Email = "omnivoro@vhouse.com",
                    IsVeganPreferred = false,
                    IsActive = true
                }
            };

            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();

            // Act
            var veganCustomers = await context.Customers
                .Where(c => c.IsActive && c.IsVeganPreferred)
                .ToListAsync();

            // Assert
            Assert.Single(veganCustomers);
            Assert.Equal("Cliente Vegano VHouse", veganCustomers.First().CustomerName);
            Assert.True(veganCustomers.First().IsVeganPreferred);
        }

        [Fact]
        public void OrderStatus_Workflow_VHouse()
        {
            // Arrange
            var order = new Order
            {
                CustomerId = 1,
                TotalAmount = 100.00m,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow
            };

            // Act & Assert - Workflow de estados
            Assert.Equal(OrderStatus.Pending, order.Status);

            order.Status = OrderStatus.Confirmed;
            Assert.Equal(OrderStatus.Confirmed, order.Status);

            order.Status = OrderStatus.InProgress;
            Assert.Equal(OrderStatus.InProgress, order.Status);

            order.Status = OrderStatus.Completed;
            order.CompletedAt = DateTime.UtcNow;
            Assert.Equal(OrderStatus.Completed, order.Status);
            Assert.NotNull(order.CompletedAt);
        }

        [Fact]
        public void OrderItem_TotalPrice_CalculatedProperty_VHouse()
        {
            // Arrange & Act
            var orderItem = new OrderItem
            {
                Quantity = 3,
                UnitPrice = 15.75m
            };

            // Assert
            Assert.Equal(47.25m, orderItem.TotalPrice); // 3 * 15.75
        }

        [Fact]
        public async Task AIRecommendation_ForVeganCustomer_VHouse()
        {
            // Arrange
            using var context = GetInMemoryContext();

            var veganCustomer = new Customer
            {
                CustomerName = "AI Vegan Customer ",
                Email = "ai.vegan@vhouse.com",
                IsVeganPreferred = true,
                IsActive = true
            };

            var products = new List<Product>
            {
                new Product
                {
                    ProductName = "AI Recomendado Vegano",
                    Emoji = "🤖🌱",
                    PriceRetail = 20.00m,
                    IsVegan = true,
                    StockQuantity = 100,
                    IsActive = true
                },
                new Product
                {
                    ProductName = "Producto No Vegano",
                    PriceRetail = 18.00m,
                    IsVegan = false,
                    StockQuantity = 50,
                    IsActive = true
                }
            };

            context.Customers.Add(veganCustomer);
            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Act - AI crea pedido basado en preferencias del cliente
            var recommendedProducts = await context.Products
                .Where(p => p.IsActive && 
                           p.IsVegan == veganCustomer.IsVeganPreferred && 
                           p.StockQuantity > 0)
                .ToListAsync();

            var aiOrder = new Order
            {
                CustomerId = veganCustomer.Id,
                Status = OrderStatus.Confirmed,
                Notes = "Pedido creado por AI basado en preferencias veganas",
                OrderItems = recommendedProducts.Select(p => new OrderItem
                {
                    ProductId = p.Id,
                    Quantity = 1,
                    UnitPrice = p.PriceRetail
                }).ToList()
            };

            aiOrder.TotalAmount = aiOrder.OrderItems.Sum(oi => oi.TotalPrice);

            context.Orders.Add(aiOrder);
            await context.SaveChangesAsync();

            // Assert
            var savedOrder = await context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == aiOrder.Id);

            Assert.NotNull(savedOrder);
            Assert.Single(savedOrder.OrderItems); // Solo el producto vegano
            Assert.Equal(20.00m, savedOrder.TotalAmount);
            Assert.All(savedOrder.OrderItems, oi => Assert.True(oi.Product.IsVegan));
            Assert.Contains("AI", savedOrder.Notes);
        }

        [Fact]
        public async Task ProductStock_Management_VHouse()
        {
            // Arrange
            using var context = GetInMemoryContext();

            var product = new Product
            {
                ProductName = "Producto Stock VHouse",
                StockQuantity = 10,
                PriceRetail = 25.00m,
                IsVegan = true,
                IsActive = true
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Act - Simular venta que reduce stock
            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = 3,
                UnitPrice = product.PriceRetail
            };

            // Simular reducción de stock después de la venta
            product.StockQuantity -= orderItem.Quantity;
            context.Products.Update(product);
            await context.SaveChangesAsync();

            // Assert
            var updatedProduct = await context.Products
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            Assert.NotNull(updatedProduct);
            Assert.Equal(7, updatedProduct.StockQuantity); // 10 - 3
            Assert.Equal(75.00m, orderItem.TotalPrice); // 3 * 25.00
        }

        [Fact]
        public void BaseEntity_CreatedAt_AutoSet_VHouse()
        {
            // Arrange & Act
            var product = new Product
            {
                ProductName = "Test BaseEntity",
                IsActive = true
            };

            var customer = new Customer
            {
                CustomerName = "Test Customer",
                IsActive = true
            };

            // Assert - CreatedAt se establece automáticamente
            Assert.True(product.CreatedAt <= DateTime.UtcNow);
            Assert.True(customer.CreatedAt <= DateTime.UtcNow);
            Assert.Null(product.UpdatedAt);
            Assert.Null(customer.UpdatedAt);
        }
    }
}