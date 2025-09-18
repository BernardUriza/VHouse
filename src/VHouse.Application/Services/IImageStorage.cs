namespace VHouse.Application.Services;

/// <summary>
/// Service for handling image and file storage with abstraction for cloud providers
/// </summary>
public interface IImageStorage
{
    /// <summary>
    /// Saves a file to the specified album and returns the relative web path
    /// </summary>
    /// <param name="albumSlug">Album slug identifier</param>
    /// <param name="file">File to upload</param>
    /// <param name="originalFileName">Original file name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Relative web path to the stored file</returns>
    Task<string> SaveAsync(string albumSlug, Stream file, string originalFileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="filePath">File path to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists in storage
    /// </summary>
    /// <param name="filePath">File path to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file exists</returns>
    Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full file system path for a relative web path
    /// </summary>
    /// <param name="relativeWebPath">Relative web path</param>
    /// <returns>Full file system path</returns>
    string GetFullPath(string relativeWebPath);

    /// <summary>
    /// Generates a thumbnail for an image file
    /// </summary>
    /// <param name="sourceFilePath">Source image file path</param>
    /// <param name="maxSize">Maximum thumbnail size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Relative web path to the thumbnail</returns>
    Task<string?> GenerateThumbnailAsync(string sourceFilePath, int maxSize = 1024, CancellationToken cancellationToken = default);
}