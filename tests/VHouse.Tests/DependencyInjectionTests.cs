using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace VHouse.Tests;

/// <summary>
/// TDD tests for Dependency Injection configuration issues
/// These tests ensure all required services are properly registered
/// </summary>
public class DependencyInjectionTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DependencyInjectionTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Should_Register_ResponseCompression_Services()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Assert - This will fail if ResponseCompression services are not registered
        var responseCompressionProvider = serviceProvider.GetService<IResponseCompressionProvider>();
        
        Assert.NotNull(responseCompressionProvider);
    }

    [Fact]
    public void Should_Create_ResponseCompression_Middleware_Without_Error()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Act & Assert - This should not throw an exception
        var exception = Record.Exception(() =>
        {
            RequestDelegate next = (context) => Task.CompletedTask;
            var middleware = ActivatorUtilities.CreateInstance<ResponseCompressionMiddleware>(
                serviceProvider, next);
            
            Assert.NotNull(middleware);
        });

        Assert.Null(exception);
    }

    [Fact]
    public async Task Application_Should_Start_Without_DI_Exceptions()
    {
        // Arrange & Act - This will fail if there are DI configuration issues
        var client = _factory.CreateClient();
        
        // Assert - Making a request should not throw DI exceptions
        var response = await client.GetAsync("/");
        
        // We don't care about the response status, just that no DI exceptions were thrown
        Assert.NotNull(response);
    }

    [Fact]
    public void Should_Have_All_Required_Compression_Services_Registered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Act & Assert - Check for all compression-related services
        var compressionProvider = services.GetService<IResponseCompressionProvider>();
        var compressionOptions = services.GetService<Microsoft.Extensions.Options.IOptions<ResponseCompressionOptions>>();

        Assert.NotNull(compressionProvider);
        Assert.NotNull(compressionOptions);
    }

    [Theory]
    [InlineData(typeof(IResponseCompressionProvider))]
    [InlineData(typeof(Microsoft.Extensions.Options.IOptions<ResponseCompressionOptions>))]
    public void Should_Register_Required_Service_Type(Type serviceType)
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Act
        var service = serviceProvider.GetService(serviceType);

        // Assert
        Assert.NotNull(service);
    }
}