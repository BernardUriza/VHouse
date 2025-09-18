using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Moq;
using VHouse.Infrastructure.Services;
using System.Text;

namespace VHouse.Tests.Gallery;

/// <summary>
/// Unit tests for Gallery functionality
/// Ensures file validation, security, and storage operations work correctly
/// </summary>
public class GalleryUnitTests
{
    private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<LocalImageStorage>> _mockLogger;
    private readonly string _testWebRootPath;

    public GalleryUnitTests()
    {
        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<LocalImageStorage>>();

        // Setup test environment
        _testWebRootPath = Path.Combine(Path.GetTempPath(), "VHouse.Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testWebRootPath);

        _mockWebHostEnvironment.Setup(x => x.WebRootPath).Returns(_testWebRootPath);
    }

    [Fact]
    public void LocalImageStorage_Constructor_CreatesUploadsDirectory()
    {
        // Arrange & Act
        var service = new LocalImageStorage(_mockWebHostEnvironment.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Assert
        var uploadsPath = Path.Combine(_testWebRootPath, "uploads");
        Assert.True(Directory.Exists(uploadsPath));
    }

    [Fact]
    public async Task SaveAsync_ValidImage_ReturnsWebPath()
    {
        // Arrange
        var service = new LocalImageStorage(_mockWebHostEnvironment.Object, _mockConfiguration.Object, _mockLogger.Object);
        var albumSlug = "test-album";
        var fileName = "test-image.jpg";
        var fileContent = Encoding.UTF8.GetBytes("fake image data");

        using var stream = new MemoryStream(fileContent);

        // Act
        var result = await service.SaveAsync(albumSlug, stream, fileName);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("uploads/test-album/", result);
        Assert.EndsWith(".jpg", result);

        // Verify file was created
        var fullPath = service.GetFullPath(result);
        Assert.True(File.Exists(fullPath));
    }

    [Fact]
    public async Task SaveAsync_EmptyAlbumSlug_ThrowsArgumentException()
    {
        // Arrange
        var service = new LocalImageStorage(_mockWebHostEnvironment.Object, _mockConfiguration.Object, _mockLogger.Object);
        using var stream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync("", stream, "test.jpg"));
        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(null!, stream, "test.jpg"));
    }

    [Fact]
    public async Task DeleteAsync_ExistingFile_RemovesFile()
    {
        // Arrange
        var service = new LocalImageStorage(_mockWebHostEnvironment.Object, _mockConfiguration.Object, _mockLogger.Object);
        var albumSlug = "test-album";
        var fileName = "test-image.jpg";
        var fileContent = Encoding.UTF8.GetBytes("fake image data");

        using var stream = new MemoryStream(fileContent);
        var webPath = await service.SaveAsync(albumSlug, stream, fileName);

        // Verify file exists before deletion
        Assert.True(await service.ExistsAsync(webPath));

        // Act
        await service.DeleteAsync(webPath);

        // Assert
        Assert.False(await service.ExistsAsync(webPath));
    }

    [Fact]
    public async Task ExistsAsync_NonExistentFile_ReturnsFalse()
    {
        // Arrange
        var service = new LocalImageStorage(_mockWebHostEnvironment.Object, _mockConfiguration.Object, _mockLogger.Object);
        var nonExistentPath = "uploads/test/nonexistent.jpg";

        // Act
        var exists = await service.ExistsAsync(nonExistentPath);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void GetFullPath_ValidRelativePath_ReturnsAbsolutePath()
    {
        // Arrange
        var service = new LocalImageStorage(_mockWebHostEnvironment.Object, _mockConfiguration.Object, _mockLogger.Object);
        var relativePath = "uploads/test/file.jpg";

        // Act
        var fullPath = service.GetFullPath(relativePath);

        // Assert
        var expectedPath = Path.Combine(_testWebRootPath, "uploads", "test", "file.jpg");
        Assert.Equal(expectedPath, fullPath);
    }

    [Theory]
    [InlineData("malicious<script>.jpg", "malicious_script_.jpg")]
    [InlineData("file|with:invalid*chars?.jpg", "file_with_invalid_chars_.jpg")]
    [InlineData("normal-file.jpg", "normal-file.jpg")]
    [InlineData("file with spaces.jpg", "file with spaces.jpg")]
    public async Task SaveAsync_SanitizesFileName(string originalFileName, string expectedPattern)
    {
        // Arrange
        var service = new LocalImageStorage(_mockWebHostEnvironment.Object, _mockConfiguration.Object, _mockLogger.Object);
        var albumSlug = "test-album";
        var fileContent = Encoding.UTF8.GetBytes("fake image data");

        using var stream = new MemoryStream(fileContent);

        // Act
        var result = await service.SaveAsync(albumSlug, stream, originalFileName);

        // Assert
        Assert.NotNull(result);
        // File should not contain dangerous characters in the final path
        Assert.DoesNotContain("<script>", result);
        Assert.DoesNotContain("|", result);
        Assert.DoesNotContain(":", result);
        Assert.DoesNotContain("*", result);
        Assert.DoesNotContain("?", result);
    }

    [Fact]
    public async Task SaveAsync_CreatesDateBasedDirectory()
    {
        // Arrange
        var service = new LocalImageStorage(_mockWebHostEnvironment.Object, _mockConfiguration.Object, _mockLogger.Object);
        var albumSlug = "test-album";
        var fileName = "test-image.jpg";
        var fileContent = Encoding.UTF8.GetBytes("fake image data");

        using var stream = new MemoryStream(fileContent);

        // Act
        var result = await service.SaveAsync(albumSlug, stream, fileName);

        // Assert
        var now = DateTime.UtcNow;
        var expectedPathPattern = $"uploads/{albumSlug}/{now.Year}/{now.Month:00}/";
        Assert.Contains(expectedPathPattern, result);
    }

    [Fact]
    public async Task GenerateThumbnailAsync_ImageFile_ReturnsPath()
    {
        // Arrange
        var service = new LocalImageStorage(_mockWebHostEnvironment.Object, _mockConfiguration.Object, _mockLogger.Object);
        var albumSlug = "test-album";
        var fileName = "test-image.jpg";
        var fileContent = Encoding.UTF8.GetBytes("fake image data");

        using var stream = new MemoryStream(fileContent);
        var originalPath = await service.SaveAsync(albumSlug, stream, fileName);

        // Act
        var thumbnailPath = await service.GenerateThumbnailAsync(originalPath);

        // Assert
        Assert.NotNull(thumbnailPath);
        Assert.Contains("thumbnails", thumbnailPath);
        Assert.Contains("_thumb", thumbnailPath);
    }

    [Fact]
    public async Task GenerateThumbnailAsync_NonImageFile_ReturnsNull()
    {
        // Arrange
        var service = new LocalImageStorage(_mockWebHostEnvironment.Object, _mockConfiguration.Object, _mockLogger.Object);
        var albumSlug = "test-album";
        var fileName = "test-document.pdf";
        var fileContent = Encoding.UTF8.GetBytes("fake pdf data");

        using var stream = new MemoryStream(fileContent);
        var originalPath = await service.SaveAsync(albumSlug, stream, fileName);

        // Act
        var thumbnailPath = await service.GenerateThumbnailAsync(originalPath);

        // Assert
        Assert.Null(thumbnailPath);
    }

    [Fact]
    public async Task GenerateThumbnailAsync_NonExistentFile_ReturnsNull()
    {
        // Arrange
        var service = new LocalImageStorage(_mockWebHostEnvironment.Object, _mockConfiguration.Object, _mockLogger.Object);
        var nonExistentPath = "uploads/test/nonexistent.jpg";

        // Act
        var thumbnailPath = await service.GenerateThumbnailAsync(nonExistentPath);

        // Assert
        Assert.Null(thumbnailPath);
    }

    public void Dispose()
    {
        // Cleanup test directory
        if (Directory.Exists(_testWebRootPath))
        {
            Directory.Delete(_testWebRootPath, true);
        }
    }
}