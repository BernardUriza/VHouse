using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using VHouse.Infrastructure.Data;
using VHouse.Domain.Entities;

namespace VHouse.Tests.Gallery;

/// <summary>
/// Integration tests for Gallery functionality
/// Tests the complete flow from HTTP requests to database operations
/// </summary>
public class GalleryIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GalleryIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app DbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<VHouseDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<VHouseDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"InMemoryTestDb_{Guid.NewGuid()}");
                });

                // Ensure database is created and seeded
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<VHouseDbContext>();
                context.Database.EnsureCreated();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Gallery_Index_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/Gallery");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Gallery", content);
        Assert.Contains("📸", content); // Gallery emoji
    }

    [Fact]
    public async Task Gallery_Index_ShowsSeededAlbums()
    {
        // Act
        var response = await _client.GetAsync("/Gallery");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        // Check for seeded albums
        Assert.Contains("Products", content);
        Assert.Contains("Sales Receipts", content);
        Assert.Contains("Purchase Receipts", content);
        Assert.Contains("Invoices", content);
        Assert.Contains("Suppliers", content);
        Assert.Contains("Customers", content);
        Assert.Contains("Misc", content);
    }

    [Fact]
    public async Task Gallery_Album_ValidSlug_ReturnsAlbumPage()
    {
        // Act
        var response = await _client.GetAsync("/Gallery/Album/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Products", content);
        Assert.Contains("Product catalog photos", content);
        Assert.Contains("Upload Files", content);
    }

    [Fact]
    public async Task Gallery_Album_InvalidSlug_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/Gallery/Album/nonexistent-album");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Gallery_Upload_Get_ReturnsUploadForm()
    {
        // Act
        var response = await _client.GetAsync("/Gallery/Upload");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Upload Files", content);
        Assert.Contains("Select Album", content);
        Assert.Contains("Select Files", content);
        Assert.Contains("form", content);
    }

    [Fact]
    public async Task Gallery_Upload_Post_NoFiles_ReturnsFormWithError()
    {
        // Arrange
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("1"), "SelectedAlbumId");
        formData.Add(new StringContent("Test caption"), "Caption");

        // Get anti-forgery token first
        var getResponse = await _client.GetAsync("/Gallery/Upload");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(getContent);

        formData.Add(new StringContent(token), "__RequestVerificationToken");

        // Act
        var response = await _client.PostAsync("/Gallery/Upload", formData);

        // Assert
        response.EnsureSuccessStatusCode(); // Should return form with validation error
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Please select at least one file", content);
    }

    [Fact]
    public async Task Gallery_Upload_Post_ValidImageFile_RedirectsToAlbum()
    {
        // Arrange
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("1"), "SelectedAlbumId");
        formData.Add(new StringContent("Test image caption"), "Caption");

        // Create a fake image file
        var imageContent = Encoding.UTF8.GetBytes("fake image data");
        var fileContent = new ByteArrayContent(imageContent);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        formData.Add(fileContent, "Files", "test-image.jpg");

        // Get anti-forgery token
        var getResponse = await _client.GetAsync("/Gallery/Upload");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(getContent);

        formData.Add(new StringContent(token), "__RequestVerificationToken");

        // Act
        var response = await _client.PostAsync("/Gallery/Upload", formData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Gallery/Album/products", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Gallery_Upload_Post_InvalidFileType_ReturnsFormWithError()
    {
        // Arrange
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("1"), "SelectedAlbumId");

        // Create a fake file with invalid type
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("fake exe data"));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/exe");
        formData.Add(fileContent, "Files", "malicious.exe");

        // Get anti-forgery token
        var getResponse = await _client.GetAsync("/Gallery/Upload");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(getContent);

        formData.Add(new StringContent(token), "__RequestVerificationToken");

        // Act
        var response = await _client.PostAsync("/Gallery/Upload", formData);

        // Assert
        response.EnsureSuccessStatusCode(); // Should return form with validation error
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("not allowed", content);
    }

    [Fact]
    public async Task Gallery_Upload_Post_FileTooLarge_ReturnsFormWithError()
    {
        // Arrange
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent("1"), "SelectedAlbumId");

        // Create a large fake file (larger than 10MB limit)
        var largeContent = new byte[11 * 1024 * 1024]; // 11MB
        var fileContent = new ByteArrayContent(largeContent);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        formData.Add(fileContent, "Files", "large-image.jpg");

        // Get anti-forgery token
        var getResponse = await _client.GetAsync("/Gallery/Upload");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(getContent);

        formData.Add(new StringContent(token), "__RequestVerificationToken");

        // Act
        var response = await _client.PostAsync("/Gallery/Upload", formData);

        // Assert
        response.EnsureSuccessStatusCode(); // Should return form with validation error
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("exceeds", content);
    }

    [Fact]
    public async Task Database_HasSeededAlbums()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VHouseDbContext>();

        // Act
        var albums = await context.Albums.ToListAsync();

        // Assert
        Assert.Equal(7, albums.Count);
        Assert.Contains(albums, a => a.Slug == "products");
        Assert.Contains(albums, a => a.Slug == "sales-receipts");
        Assert.Contains(albums, a => a.Slug == "purchase-receipts");
        Assert.Contains(albums, a => a.Slug == "invoices");
        Assert.Contains(albums, a => a.Slug == "suppliers");
        Assert.Contains(albums, a => a.Slug == "customers");
        Assert.Contains(albums, a => a.Slug == "misc");
    }

    [Fact]
    public async Task Album_Slug_IsUnique()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VHouseDbContext>();

        // Act
        var slugs = await context.Albums.Select(a => a.Slug).ToListAsync();

        // Assert
        Assert.Equal(slugs.Count, slugs.Distinct().Count());
    }

    [Fact]
    public async Task Photo_CascadeDelete_RemovesPhotosWhenAlbumDeleted()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VHouseDbContext>();

        var album = new Album
        {
            Name = "Test Album",
            Slug = "test-album-delete",
            Description = "Test album for cascade delete"
        };

        context.Albums.Add(album);
        await context.SaveChangesAsync();

        var photo = new Photo
        {
            AlbumId = album.Id,
            FileName = "test-file.jpg",
            OriginalName = "test.jpg",
            ContentType = "image/jpeg",
            SizeBytes = 1024,
            UploadedUtc = DateTime.UtcNow
        };

        context.Photos.Add(photo);
        await context.SaveChangesAsync();

        // Act
        context.Albums.Remove(album);
        await context.SaveChangesAsync();

        // Assert
        var remainingPhotos = await context.Photos.Where(p => p.AlbumId == album.Id).ToListAsync();
        Assert.Empty(remainingPhotos);
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        // Simple extraction of anti-forgery token from HTML
        // In a real implementation, you might want to use HtmlAgilityPack
        var tokenStart = html.IndexOf("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"");
        if (tokenStart == -1) return string.Empty;

        tokenStart += "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"".Length;
        var tokenEnd = html.IndexOf('"', tokenStart);

        return tokenEnd != -1 ? html.Substring(tokenStart, tokenEnd - tokenStart) : string.Empty;
    }
}