using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VHouse.Infrastructure.Data;
using VHouse.Domain.Entities;
using VHouse.Application.Services;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace VHouse.Web.Controllers;

/// <summary>
/// Gallery controller for managing photo albums and file uploads
/// Essential for Bernard's business documentation (product photos, receipts, invoices)
/// </summary>
[Authorize]
public class GalleryController : Controller
{
    private readonly VHouseDbContext _context;
    private readonly IImageStorage _imageStorage;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GalleryController> _logger;

    public GalleryController(
        VHouseDbContext context,
        IImageStorage imageStorage,
        IConfiguration configuration,
        ILogger<GalleryController> logger)
    {
        _context = context;
        _imageStorage = imageStorage;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Display gallery index with all albums
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var albums = await _context.Albums
            .Select(a => new AlbumViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Slug = a.Slug,
                Description = a.Description,
                PhotoCount = a.Photos.Count(),
                LatestPhotoDate = a.Photos.OrderByDescending(p => p.UploadedUtc).FirstOrDefault() != null
                    ? a.Photos.OrderByDescending(p => p.UploadedUtc).First().UploadedUtc
                    : (DateTime?)null,
                CoverPhotoPath = a.Photos.OrderByDescending(p => p.UploadedUtc).FirstOrDefault() != null
                    ? a.Photos.OrderByDescending(p => p.UploadedUtc).First().ThumbnailPath ?? a.Photos.OrderByDescending(p => p.UploadedUtc).First().FileName
                    : null
            })
            .OrderBy(a => a.Name)
            .ToListAsync();

        return View(albums);
    }

    /// <summary>
    /// Display photos in a specific album with pagination
    /// </summary>
    [Route("Gallery/Album/{slug}")]
    public async Task<IActionResult> Album(string slug, int page = 1, int pageSize = 24)
    {
        if (string.IsNullOrEmpty(slug))
            return BadRequest("Album slug is required");

        var album = await _context.Albums
            .FirstOrDefaultAsync(a => a.Slug == slug);

        if (album == null)
            return NotFound($"Album '{slug}' not found");

        var totalPhotos = await _context.Photos
            .Where(p => p.AlbumId == album.Id)
            .CountAsync();

        var photos = await _context.Photos
            .Where(p => p.AlbumId == album.Id)
            .OrderByDescending(p => p.UploadedUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var viewModel = new AlbumDetailViewModel
        {
            Album = album,
            Photos = photos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPhotos = totalPhotos,
            TotalPages = (int)Math.Ceiling((double)totalPhotos / pageSize)
        };

        return View(viewModel);
    }

    /// <summary>
    /// Display upload form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Upload()
    {
        var albums = await _context.Albums
            .OrderBy(a => a.Name)
            .ToListAsync();

        var viewModel = new UploadViewModel
        {
            Albums = albums,
            MaxSizeMB = _configuration.GetValue<int>("Uploads:MaxSizeMB", 10),
            AllowedContentTypes = _configuration.GetSection("Uploads:AllowedContentTypes").Get<string[]>() ?? new[] { "image/jpeg", "image/png", "application/pdf" }
        };

        return View(viewModel);
    }

    /// <summary>
    /// Process file uploads
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload albums for the view
            model.Albums = await _context.Albums.OrderBy(a => a.Name).ToListAsync();
            return View(model);
        }

        var album = await _context.Albums.FindAsync(model.SelectedAlbumId);
        if (album == null)
        {
            ModelState.AddModelError("SelectedAlbumId", "Selected album not found");
            model.Albums = await _context.Albums.OrderBy(a => a.Name).ToListAsync();
            return View(model);
        }

        if (model.Files == null || !model.Files.Any())
        {
            ModelState.AddModelError("Files", "Please select at least one file");
            model.Albums = await _context.Albums.OrderBy(a => a.Name).ToListAsync();
            return View(model);
        }

        var maxSizeBytes = _configuration.GetValue<int>("Uploads:MaxSizeMB", 10) * 1024 * 1024;
        var allowedContentTypes = _configuration.GetSection("Uploads:AllowedContentTypes").Get<string[]>() ?? new[] { "image/jpeg", "image/png", "application/pdf" };
        var uploadedPhotos = new List<Photo>();

        foreach (var file in model.Files)
        {
            // Validate file
            var validationErrors = ValidateFile(file, maxSizeBytes, allowedContentTypes);
            if (validationErrors.Any())
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError("Files", $"{file.FileName}: {error}");
                }
                continue;
            }

            try
            {
                // Save file
                var filePath = await _imageStorage.SaveAsync(album.Slug, file.OpenReadStream(), file.FileName);

                // Generate thumbnail for images
                string? thumbnailPath = null;
                if (file.ContentType.StartsWith("image/") && _configuration.GetValue<bool>("Uploads:EnableThumbnails", true))
                {
                    thumbnailPath = await _imageStorage.GenerateThumbnailAsync(filePath);
                }

                // Create photo record
                var photo = new Photo
                {
                    AlbumId = album.Id,
                    FileName = filePath,
                    OriginalName = file.FileName,
                    ContentType = file.ContentType,
                    SizeBytes = file.Length,
                    UploadedUtc = DateTime.UtcNow,
                    Caption = model.Caption,
                    ThumbnailPath = thumbnailPath
                };

                _context.Photos.Add(photo);
                uploadedPhotos.Add(photo);

                _logger.LogInformation("File uploaded successfully: {FileName} to album {AlbumName}", file.FileName, album.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file: {FileName}", file.FileName);
                ModelState.AddModelError("Files", $"{file.FileName}: Upload failed");
            }
        }

        if (uploadedPhotos.Any())
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Successfully uploaded {uploadedPhotos.Count} file(s) to {album.Name}";
            return RedirectToAction(nameof(Album), new { slug = album.Slug });
        }

        model.Albums = await _context.Albums.OrderBy(a => a.Name).ToListAsync();
        return View(model);
    }

    /// <summary>
    /// Delete a photo
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var photo = await _context.Photos
            .Include(p => p.Album)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (photo == null)
            return NotFound();

        try
        {
            // Delete file from storage
            await _imageStorage.DeleteAsync(photo.FileName);

            // Delete from database
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Photo deleted successfully";
            _logger.LogInformation("Photo deleted: {PhotoId} - {FileName}", photo.Id, photo.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete photo: {PhotoId}", photo.Id);
            TempData["ErrorMessage"] = "Failed to delete photo";
        }

        return RedirectToAction(nameof(Album), new { slug = photo.Album.Slug });
    }

    private static List<string> ValidateFile(IFormFile file, long maxSizeBytes, string[] allowedContentTypes)
    {
        var errors = new List<string>();

        if (file.Length == 0)
            errors.Add("File is empty");

        if (file.Length > maxSizeBytes)
            errors.Add($"File size exceeds {maxSizeBytes / (1024 * 1024)}MB limit");

        if (!allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            errors.Add($"File type '{file.ContentType}' is not allowed");

        if (string.IsNullOrWhiteSpace(file.FileName))
            errors.Add("File name is required");

        return errors;
    }
}

// View Models
public class AlbumViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PhotoCount { get; set; }
    public DateTime? LatestPhotoDate { get; set; }
    public string? CoverPhotoPath { get; set; }
}

public class AlbumDetailViewModel
{
    public Album Album { get; set; } = null!;
    public List<Photo> Photos { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPhotos { get; set; }
    public int TotalPages { get; set; }
}

public class UploadViewModel
{
    [Required(ErrorMessage = "Please select an album")]
    public int SelectedAlbumId { get; set; }

    [Required(ErrorMessage = "Please select at least one file")]
    public List<IFormFile> Files { get; set; } = new();

    [MaxLength(500, ErrorMessage = "Caption cannot exceed 500 characters")]
    public string? Caption { get; set; }

    public List<Album> Albums { get; set; } = new();
    public int MaxSizeMB { get; set; }
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
}