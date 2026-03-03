namespace StayHub.Services.Hotel.Application.Abstractions;

/// <summary>
/// Abstraction for file storage operations.
/// The Application layer defines the contract; Infrastructure provides the implementation.
///
/// Current implementation: LocalFileStorageService (stores in wwwroot/uploads/).
/// Future: swap to AzureBlobStorageService or S3StorageService without touching application code.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file and return its publicly accessible URL.
    /// </summary>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="fileName">Original file name (used for extension detection).</param>
    /// <param name="folder">Logical folder path (e.g., "hotels/{id}" or "rooms/{id}").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The public URL of the uploaded file.</returns>
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string folder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file by its URL.
    /// No-op if the file does not exist.
    /// </summary>
    /// <param name="fileUrl">The URL returned from UploadAsync.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
}
