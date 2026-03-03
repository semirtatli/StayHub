using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.Abstractions;

namespace StayHub.Services.Hotel.Infrastructure.Storage;

/// <summary>
/// Local file system storage implementation.
/// Stores uploaded files in wwwroot/uploads/{folder}/{guid}{extension}.
///
/// This is the development/single-server implementation.
/// For production, replace with AzureBlobStorageService or S3StorageService
/// by swapping the DI registration — no application code changes needed.
///
/// File URL format: /uploads/{folder}/{guid}{extension}
/// Static file middleware serves these from wwwroot.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private const string UploadsRoot = "uploads";

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string folder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueName = $"{Guid.NewGuid()}{extension}";

        // Build physical path: wwwroot/uploads/{folder}/{guid}.ext
        var folderPath = Path.Combine(_environment.WebRootPath, UploadsRoot, folder);
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, uniqueName);

        await using var fileStreamOut = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOut, cancellationToken);

        // Return relative URL (served by static file middleware)
        var relativeUrl = $"/{UploadsRoot}/{folder}/{uniqueName}";

        _logger.LogInformation(
            "File uploaded: {FileName} → {Url}",
            fileName, relativeUrl);

        return relativeUrl;
    }

    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return Task.CompletedTask;

        // Convert URL back to physical path
        // URL format: /uploads/folder/file.ext → wwwroot/uploads/folder/file.ext
        var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);

            _logger.LogInformation("File deleted: {Url}", fileUrl);
        }
        else
        {
            _logger.LogWarning("File not found for deletion: {Url}", fileUrl);
        }

        return Task.CompletedTask;
    }
}
