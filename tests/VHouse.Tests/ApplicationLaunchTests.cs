using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VHouse.Infrastructure.Data;
using Xunit;

namespace VHouse.Tests;

/// <summary>
/// Tests to ensure the application starts correctly and handles Entity Framework properly
/// Following TDD approach to identify and fix startup issues
/// </summary>
public class ApplicationLaunchTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ApplicationLaunchTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Application_Should_Start_Successfully()
    {
        // Arrange & Act - This will fail if the application doesn't start
        var client = _factory.CreateClient();
        
        // Assert - If we get here, the application started successfully
        Assert.NotNull(client);
        
        // Verify we can make a basic request
        var response = await client.GetAsync("/");
        
        // Should get some kind of response (even if it's a redirect or error page)
        Assert.NotNull(response);
    }

    [Fact]
    public void DbContext_Should_Be_Configured_Correctly()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        
        // Act - This will fail if there are EF configuration issues
        var context = scope.ServiceProvider.GetRequiredService<VHouseDbContext>();
        
        // Assert
        Assert.NotNull(context);
        
        // Verify the model can be created without errors
        var model = context.Model;
        Assert.NotNull(model);
        
        // Verify OrderItem entity is configured correctly
        var orderItemEntity = model.FindEntityType(typeof(VHouse.Domain.Entities.OrderItem));
        Assert.NotNull(orderItemEntity);
        
        // Verify TotalPrice is NOT mapped (should be NotMapped)
        var totalPriceProperty = orderItemEntity.FindProperty("TotalPrice");
        Assert.Null(totalPriceProperty); // Should be null because it's [NotMapped]
    }

    [Fact]
    public void OrderItem_TotalPrice_Should_Calculate_Correctly()
    {
        // Arrange
        var orderItem = new VHouse.Domain.Entities.OrderItem
        {
            Quantity = 3,
            UnitPrice = 10.50m
        };

        // Act
        var totalPrice = orderItem.TotalPrice;

        // Assert
        Assert.Equal(31.50m, totalPrice);
    }

    [Fact]
    public async Task Application_Should_Handle_Multiple_Requests_Concurrently()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Create multiple concurrent requests
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(client.GetAsync("/"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - All requests should complete successfully
        Assert.Equal(5, responses.Length);
        foreach (var response in responses)
        {
            Assert.NotNull(response);
        }
    }
}