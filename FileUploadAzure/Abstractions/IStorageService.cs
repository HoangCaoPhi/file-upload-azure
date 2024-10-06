namespace FileUploadAzure.Abstractions;

public interface IStorageService
{
    Task<AppFile> UploadAsync(Stream stream, AppFile file, StorageContainer container, CancellationToken cancellationToken = default);

    Task<FileResponse> DownloadAsync(string fileId, StorageContainer container, CancellationToken cancellationToken = default);

    Task DeleteAsync(string fileId, StorageContainer container, CancellationToken cancellationToken = default);

    Task CopyTempToAsync(string fileId, StorageContainer container, CancellationToken cancellationToken = default);

    Task<Uri?> GetFileUriAsync(string fileId, StorageContainer container, CancellationToken cancellationToken = default);
}