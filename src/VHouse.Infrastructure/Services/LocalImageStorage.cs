using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VHouse.Application.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace VHouse.Infrastructure.Services;

/// <summary>
/// Local file system implementation of IImageStorage
/// Stores files in wwwroot/uploads with secure file naming
/// </summary>
public class LocalImageStorage : IImageStorage
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocalImageStorage> _logger;
    private readonly string _uploadsPath;
    private readonly string _webRootPath;

    public LocalImageStorage(
        IWebHostEnvironment webHostEnvironment,
        IConfiguration configuration,
        ILogger<LocalImageStorage> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _configuration = configuration;
        _logger = logger;
        _webRootPath = _webHostEnvironment.WebRootPath ?? throw new InvalidOperationException("WebRootPath is null");
        _uploadsPath = Path.Combine(_webRootPath, "uploads");

        // Ensure uploads directory exists
        Directory.CreateDirectory(_uploadsPath);
    }

    public async Task<string> SaveAsync(string albumSlug, Stream file, string originalFileName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate album slug
            if (string.IsNullOrWhiteSpace(albumSlug))
                throw new ArgumentException("Album slug cannot be empty", nameof(albumSlug));

            // Sanitize original filename
            var sanitizedFileName = SanitizeFileName(originalFileName);
            var fileExtension = Path.GetExtension(sanitizedFileName).ToLowerInvariant();

            // Generate collision-proof filename
            var fileName = GenerateSecureFileName(sanitizedFileName);

            // Create album directory structure with date-based organization
            var now = DateTime.UtcNow;
            var albumPath = Path.Combine(_uploadsPath, albumSlug, now.Year.ToString(), now.Month.ToString("00"));
            Directory.CreateDirectory(albumPath);

            // Full file path
            var fullPath = Path.Combine(albumPath, fileName);

            // Validate file doesn't already exist (very unlikely with secure naming)
            if (File.Exists(fullPath))
            {
                fileName = GenerateSecureFileName(sanitizedFileName, true);
                fullPath = Path.Combine(albumPath, fileName);
            }

            // Save file
            using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            await file.CopyToAsync(fileStream, cancellationToken);

            // Return relative web path
            var relativePath = Path.Combine("uploads", albumSlug, now.Year.ToString(), now.Month.ToString("00"), fileName);
            var webPath = relativePath.Replace('\\', '/'); // Ensure web-compatible path separators

            _logger.LogInformation("File saved successfully: {FileName} -> {WebPath}", originalFileName, webPath);
            return webPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file {FileName} to album {AlbumSlug}", originalFileName, albumSlug);
            throw;
        }
    }

    public async Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = GetFullPath(filePath);

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath), cancellationToken);
                _logger.LogInformation("File deleted: {FilePath}", filePath);

                // Also delete thumbnail if it exists
                var thumbnailPath = GetThumbnailPath(filePath);
                var fullThumbnailPath = GetFullPath(thumbnailPath);
                if (File.Exists(fullThumbnailPath))
                {
                    await Task.Run(() => File.Delete(fullThumbnailPath), cancellationToken);
                    _logger.LogInformation("Thumbnail deleted: {ThumbnailPath}", thumbnailPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = GetFullPath(filePath);
            return await Task.Run(() => File.Exists(fullPath), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if file exists: {FilePath}", filePath);
            return false;
        }
    }

    public string GetFullPath(string relativeWebPath)
    {
        // Remove leading slash and convert web separators to OS separators
        var cleanPath = relativeWebPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_webRootPath, cleanPath);
    }

    public async Task<string?> GenerateThumbnailAsync(string sourceFilePath, int maxSize = 1024, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullSourcePath = GetFullPath(sourceFilePath);

            if (!File.Exists(fullSourcePath))
            {
                _logger.LogWarning("Source file not found for thumbnail generation: {SourcePath}", sourceFilePath);
                return null;
            }

            // Check if file is an image
            var extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();
            if (!IsImageFile(extension))
            {
                _logger.LogDebug("File is not an image, skipping thumbnail generation: {SourcePath}", sourceFilePath);
                return null;
            }

            // Generate thumbnail path
            var thumbnailWebPath = GetThumbnailPath(sourceFilePath);
            var fullThumbnailPath = GetFullPath(thumbnailWebPath);

            // Create thumbnail directory if it doesn't exist
            var thumbnailDir = Path.GetDirectoryName(fullThumbnailPath);
            if (!string.IsNullOrEmpty(thumbnailDir))
            {
                Directory.CreateDirectory(thumbnailDir);
            }

            // For now, just copy the original file as placeholder
            // In a real implementation, you would use an image processing library like ImageSharp
            await Task.Run(() => File.Copy(fullSourcePath, fullThumbnailPath, true), cancellationToken);

            _logger.LogInformation("Thumbnail generated: {SourcePath} -> {ThumbnailPath}", sourceFilePath, thumbnailWebPath);
            return thumbnailWebPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate thumbnail for: {SourcePath}", sourceFilePath);
            return null;
        }
    }

    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "unknown";

        // Remove path characters and invalid filename characters
        var sanitized = Path.GetFileName(fileName);
        var invalidChars = Path.GetInvalidFileNameChars();

        foreach (var invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        // Remove additional dangerous characters
        sanitized = Regex.Replace(sanitized, @"[<>:""|?*]", "_");

        // Limit length
        if (sanitized.Length > 100)
        {
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);
            sanitized = nameWithoutExtension.Substring(0, 100 - extension.Length) + extension;
        }

        return sanitized;
    }

    private string GenerateSecureFileName(string originalFileName, bool forceUnique = false)
    {
        var extension = Path.GetExtension(originalFileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

        // Generate a secure hash-based name
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var uniqueData = forceUnique ? Guid.NewGuid().ToString("N")[..8] : "";
        var hashInput = $"{nameWithoutExtension}_{timestamp}_{uniqueData}";

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
        var hashString = Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();

        return $"{hashString}_{timestamp}{extension}";
    }

    private string GetThumbnailPath(string originalPath)
    {
        var directory = Path.GetDirectoryName(originalPath) ?? "";
        var fileName = Path.GetFileNameWithoutExtension(originalPath);
        var extension = Path.GetExtension(originalPath);

        var thumbnailFileName = $"{fileName}_thumb{extension}";
        var thumbnailPath = Path.Combine(directory, "thumbnails", thumbnailFileName);

        return thumbnailPath.Replace('\\', '/');
    }

    private static bool IsImageFile(string extension)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
        return imageExtensions.Contains(extension);
    }
}