using Microsoft.EntityFrameworkCore;
using VHouse.Core.Entities;
using VHouse.Infrastructure.Data;

namespace VHouse.Tests
{
    public class CustomerManagementTests
    {
        private VHouseDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<VHouseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new VHouseDbContext(options);
        }

        [Fact]
        public async Task CreateCustomer_WithValidData_ShouldSucceed()
        {
            using var context = GetInMemoryContext();

            var customer = new Customer
            {
                FullName = "Maria Garcia",
                Email = "maria@example.com",
                Phone = "555-9876",
                Address = "789 Pine St",
                IsRetail = true
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var savedCustomer = await context.Customers
                .FirstOrDefaultAsync(c => c.Email == "maria@example.com");

            Assert.NotNull(savedCustomer);
            Assert.Equal("Maria Garcia", savedCustomer.FullName);
            Assert.True(savedCustomer.IsRetail);
        }

        [Fact]
        public async Task CreateCustomer_WithDuplicateEmail_ShouldThrowException()
        {
            using var context = GetInMemoryContext();

            var customer1 = new Customer
            {
                FullName = "John Doe",
                Email = "duplicate@example.com",
                Phone = "555-1111"
            };

            var customer2 = new Customer
            {
                FullName = "Jane Smith",
                Email = "duplicate@example.com",
                Phone = "555-2222"
            };

            context.Customers.Add(customer1);
            await context.SaveChangesAsync();

            context.Customers.Add(customer2);
            
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task GetCustomer_WithOrders_ShouldIncludeOrderHistory()
        {
            using var context = GetInMemoryContext();

            var customer = new Customer
            {
                CustomerId = 1,
                FullName = "Customer With Orders",
                Email = "orders@example.com",
                Phone = "555-0000"
            };

            var product = new Product
            {
                ProductId = 1,
                ProductName = "Test Product",
                PriceCost = 10.00m,
                PriceRetail = 15.00m,
                IsActive = true
            };

            var orders = new List<Order>
            {
                new Order
                {
                    CustomerId = customer.CustomerId,
                    OrderDate = DateTime.UtcNow.AddDays(-7),
                    TotalAmount = 45.00m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = product.ProductId,
                            ProductName = product.ProductName,
                            Price = 15.00m,
                            Quantity = 3
                        }
                    }
                },
                new Order
                {
                    CustomerId = customer.CustomerId,
                    OrderDate = DateTime.UtcNow.AddDays(-3),
                    TotalAmount = 30.00m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = product.ProductId,
                            ProductName = product.ProductName,
                            Price = 15.00m,
                            Quantity = 2
                        }
                    }
                }
            };

            context.Customers.Add(customer);
            context.Products.Add(product);
            context.Orders.AddRange(orders);
            await context.SaveChangesAsync();

            var customerWithOrders = await context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

            Assert.NotNull(customerWithOrders);
            Assert.Equal(2, customerWithOrders.Orders.Count);
            Assert.Equal(75.00m, customerWithOrders.Orders.Sum(o => o.TotalAmount));
        }

        [Fact]
        public async Task UpdateCustomer_WithNewAddress_ShouldUpdateSuccessfully()
        {
            using var context = GetInMemoryContext();

            var customer = new Customer
            {
                FullName = "Update Test Customer",
                Email = "update@example.com",
                Phone = "555-1234",
                Address = "Old Address"
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            customer.Address = "New Updated Address";
            customer.Phone = "555-9999";

            context.Customers.Update(customer);
            await context.SaveChangesAsync();

            var updatedCustomer = await context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

            Assert.NotNull(updatedCustomer);
            Assert.Equal("New Updated Address", updatedCustomer.Address);
            Assert.Equal("555-9999", updatedCustomer.Phone);
        }

        [Fact]
        public async Task DeleteCustomer_ShouldSetOrdersCustomerIdToNull()
        {
            using var context = GetInMemoryContext();

            var customer = new Customer
            {
                CustomerId = 1,
                FullName = "Delete Test Customer",
                Email = "delete@example.com",
                Phone = "555-DELETE"
            };

            var order = new Order
            {
                CustomerId = customer.CustomerId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 25.00m
            };

            context.Customers.Add(customer);
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            context.Customers.Remove(customer);
            await context.SaveChangesAsync();

            var orphanedOrder = await context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            Assert.NotNull(orphanedOrder);
            Assert.Null(orphanedOrder.CustomerId);
        }

        [Fact]
        public async Task GetRetailCustomers_ShouldReturnOnlyRetailCustomers()
        {
            using var context = GetInMemoryContext();

            var customers = new List<Customer>
            {
                new Customer
                {
                    FullName = "Retail Customer 1",
                    Email = "retail1@example.com",
                    Phone = "555-R001",
                    IsRetail = true
                },
                new Customer
                {
                    FullName = "B2B Customer 1",
                    Email = "b2b1@example.com",
                    Phone = "555-B001",
                    IsRetail = false
                },
                new Customer
                {
                    FullName = "Retail Customer 2",
                    Email = "retail2@example.com",
                    Phone = "555-R002",
                    IsRetail = true
                }
            };

            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();

            var retailCustomers = await context.Customers
                .Where(c => c.IsRetail)
                .ToListAsync();

            Assert.Equal(2, retailCustomers.Count);
            Assert.All(retailCustomers, c => Assert.True(c.IsRetail));
        }

        [Fact]
        public async Task CreateCustomerWithInventory_ShouldInitializeInventory()
        {
            using var context = GetInMemoryContext();

            var customer = new Customer
            {
                CustomerId = 1,
                FullName = "Inventory Customer",
                Email = "inventory@example.com",
                Phone = "555-INV",
                Inventory = new Inventory
                {
                    CustomerId = 1,
                    IsGeneralInventory = false,
                    Items = new List<InventoryItem>()
                }
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var customerWithInventory = await context.Customers
                .Include(c => c.Inventory)
                .ThenInclude(i => i.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

            Assert.NotNull(customerWithInventory);
            Assert.NotNull(customerWithInventory.Inventory);
            Assert.Equal(customer.CustomerId, customerWithInventory.Inventory.CustomerId);
            Assert.False(customerWithInventory.Inventory.IsGeneralInventory);
        }
    }
}