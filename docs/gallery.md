# VHouse Gallery System

## Purpose

The Gallery system is a critical component of VHouse designed to help Bernard organize and manage business documentation efficiently. It provides secure file storage for:

- **Product Photos**: Visual catalog for Mona la Dona's donuts, Sano Market's products
- **Sales Receipts**: Customer transaction records
- **Purchase Receipts**: Supplier payment documentation
- **Invoices**: Client billing documents
- **Supplier Documentation**: Contracts, certificates, product sheets
- **Customer Documentation**: Agreements, special requirements, photos
- **Miscellaneous**: Any other business-related files

## Features

### 🏗️ **Pre-configured Albums**
Seven essential albums are automatically created:
- **Products** - Product catalog photos
- **Sales Receipts** - Customer sales documentation
- **Purchase Receipts** - Supplier purchase records
- **Invoices** - Client invoices and billing
- **Suppliers** - Supplier documentation and photos
- **Customers** - Customer-related documentation
- **Misc** - Miscellaneous files

### 📤 **Secure Multi-File Upload**
- **Drag & drop interface** with visual feedback
- **Multiple file selection** for batch uploads
- **Real-time file validation** with user-friendly error messages
- **Progress indication** during upload process
- **Optional captions** for file context

### 🖼️ **Smart File Handling**
- **Automatic thumbnails** for images (configurable)
- **Secure file naming** to prevent conflicts and security issues
- **Content type validation** to ensure only safe files
- **File size limits** to prevent storage abuse
- **Date-organized storage** (`uploads/album/YYYY/MM/`)

### 🔒 **Security Features**
- **Authentication required** - Gallery access restricted to logged-in users
- **File type restrictions** - Only approved content types allowed
- **Filename sanitization** - Dangerous characters removed/replaced
- **Path validation** - Prevents directory traversal attacks
- **Anti-forgery tokens** - CSRF protection on all forms

## Supported File Types

### Images
- **JPEG** (`.jpg`, `.jpeg`) - Photos, product images
- **PNG** (`.png`) - Graphics with transparency
- **WebP** (`.webp`) - Modern web format

### Documents
- **PDF** (`.pdf`) - Receipts, invoices, contracts

## Configuration

### File Size Limits
```json
{
  "Uploads": {
    "MaxSizeMB": 10
  }
}
```

### Allowed Content Types
```json
{
  "Uploads": {
    "AllowedContentTypes": [
      "image/jpeg",
      "image/png",
      "image/webp",
      "application/pdf"
    ]
  }
}
```

### Thumbnails
```json
{
  "Uploads": {
    "EnableThumbnails": true,
    "ThumbnailMaxSize": 1024
  }
}
```

## File Storage Structure

```
VHouse.Web/wwwroot/uploads/
├── products/
│   ├── 2024/
│   │   ├── 01/
│   │   │   ├── a1b2c3d4_1704067200000.jpg
│   │   │   └── thumbnails/
│   │   │       └── a1b2c3d4_1704067200000_thumb.jpg
│   │   └── 02/
│   └── 2025/
├── sales-receipts/
├── purchase-receipts/
├── invoices/
├── suppliers/
├── customers/
└── misc/
```

### File Naming Convention
Files are renamed using a secure hash-based system:
- Format: `{hash}_{timestamp}{extension}`
- Example: `a1b2c3d4e5f6_1704067200000.jpg`
- **Benefits**: Prevents conflicts, avoids dangerous characters, maintains chronological order

## Usage Guide

### 📸 **Uploading Files**

1. **Navigate to Gallery** - Click "Galería" in the sidebar
2. **Choose Upload** - Click "Upload Files" button
3. **Select Album** - Choose appropriate album from dropdown
4. **Add Files** - Drag & drop or click to browse files
5. **Add Caption** (optional) - Describe the files
6. **Upload** - Click "Upload Files" to process

### 👁️ **Viewing Files**

1. **Browse Albums** - Gallery index shows all albums with counts
2. **Enter Album** - Click "View Album" to see files
3. **View Images** - Click image thumbnails for full-size preview
4. **Download PDFs** - Click PDF files to open in new tab

### 🗑️ **Managing Files**

1. **Delete Files** - Use dropdown menu on each file
2. **Confirmation** - System requires confirmation before deletion
3. **Cleanup** - Both original file and thumbnail are removed

## Adding New Albums

To add new albums beyond the default seven:

### 1. Database Migration
Create a new migration to add albums:

```csharp
// In a new migration file
migrationBuilder.InsertData(
    table: "Albums",
    columns: new[] { "Name", "Slug", "Description", "CreatedAt" },
    values: new object[] { "New Album", "new-album", "Description", DateTime.UtcNow }
);
```

### 2. Run Migration
```bash
dotnet ef migrations add AddNewAlbums --project src/VHouse.Infrastructure --startup-project VHouse.Web
dotnet ef database update --project src/VHouse.Infrastructure --startup-project VHouse.Web
```

### 3. Manual Database Insert
Alternatively, insert directly into database:
```sql
INSERT INTO Albums (Name, Slug, Description, CreatedAt)
VALUES ('Marketing Materials', 'marketing', 'Promotional content and advertisements', datetime('now'));
```

## Cloud Storage Migration

The system is designed for easy migration to cloud storage:

### 1. Implement Cloud Provider
Create new implementation of `IImageStorage`:
```csharp
public class AzureBlobImageStorage : IImageStorage
{
    // Implementation for Azure Blob Storage
}

public class S3ImageStorage : IImageStorage
{
    // Implementation for AWS S3
}
```

### 2. Update Service Registration
```csharp
// In ServiceCollectionExtensions.cs
services.AddScoped<IImageStorage, AzureBlobImageStorage>();
```

### 3. Configuration
```json
{
  "Storage": {
    "Provider": "AzureBlob",
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...",
    "ContainerName": "vhouse-gallery"
  }
}
```

## Performance Considerations

### 🚀 **Optimization Strategies**

1. **Thumbnail Generation**
   - Images automatically get thumbnails for faster loading
   - Thumbnails stored in `thumbnails/` subdirectory
   - Consider using ImageSharp for production thumbnail generation

2. **Pagination**
   - Album views paginated (24 items per page)
   - Prevents performance issues with large albums
   - Configurable page size

3. **Lazy Loading**
   - Images can be configured for lazy loading
   - Reduces initial page load time

4. **CDN Integration**
   - Static files can be served via CDN
   - Reduces server load and improves global performance

## Cleanup and Retention

### 🧹 **Maintenance Tasks**

1. **Orphaned Files**
   - Files may become orphaned if database records are deleted
   - Implement background job to clean up orphaned files
   - Check file existence vs database records

2. **Thumbnail Regeneration**
   - Add command to regenerate thumbnails
   - Useful when changing thumbnail settings
   - Can be implemented as background job

3. **Storage Monitoring**
   - Monitor disk space usage
   - Alert when approaching storage limits
   - Consider automatic archival of old files

### Example Cleanup Script
```csharp
public class FileCleanupService
{
    public async Task CleanupOrphanedFiles()
    {
        var dbFiles = await _context.Photos.Select(p => p.FileName).ToListAsync();
        var diskFiles = Directory.GetFiles(_uploadsPath, "*", SearchOption.AllDirectories);

        var orphanedFiles = diskFiles.Where(f => !dbFiles.Contains(GetRelativePath(f)));

        foreach (var orphan in orphanedFiles)
        {
            File.Delete(orphan);
        }
    }
}
```

## Security Best Practices

### 🔐 **Implementation Details**

1. **File Validation**
   - Content-type checking (not just extension)
   - File size limits enforced
   - Malicious filename patterns blocked

2. **Storage Security**
   - Files stored outside webroot when possible
   - Direct file access requires application routing
   - No script execution in upload directories

3. **Access Control**
   - Authentication required for all gallery operations
   - Consider role-based access for different albums
   - Audit logging for file operations

4. **Data Protection**
   - Consider encryption at rest for sensitive documents
   - Backup strategy for important business documents
   - GDPR compliance for customer-related files

## Integration Points

### 🔗 **System Connections**

1. **Product Catalog**
   - Link product photos to product records
   - Display gallery images in product pages
   - Bulk upload for product catalogs

2. **Order Management**
   - Attach receipt photos to orders
   - Link invoice documents to deliveries
   - Visual confirmation of completed orders

3. **Client Portal**
   - Clients can view their specific invoices
   - Product catalogs customized per client
   - Document sharing with restricted access

4. **Reporting**
   - Include document counts in business metrics
   - Storage usage reporting
   - File upload audit trails

## Troubleshooting

### 🔧 **Common Issues**

1. **Upload Failures**
   - Check file size limits
   - Verify content type permissions
   - Ensure disk space available
   - Check write permissions on upload directory

2. **Missing Thumbnails**
   - Verify thumbnail generation is enabled
   - Check for image processing errors in logs
   - Ensure sufficient disk space for thumbnails

3. **Performance Issues**
   - Consider implementing image optimization
   - Add CDN for static file serving
   - Optimize database queries for large albums

4. **Storage Full**
   - Implement file archival strategy
   - Move old files to cold storage
   - Implement file compression

## Future Enhancements

### 🚀 **Roadmap**

1. **Advanced Search**
   - Full-text search in file names and captions
   - Date range filtering
   - Content type filtering

2. **Batch Operations**
   - Bulk file deletion
   - Batch caption editing
   - Album migration tools

3. **Image Processing**
   - Automatic image optimization
   - Multiple thumbnail sizes
   - Watermarking for product photos

4. **Integration**
   - Direct camera upload from mobile
   - Email attachment import
   - Scanner integration

---

**Remember**: This gallery system is specifically designed for Bernard's vegan distribution business. Every feature serves the mission of efficient business operations, better client service, and ultimately advancing vegan adoption. Each organized photo and document helps VHouse serve Mona la Dona, Sano Market, and other clients more effectively. 🌱