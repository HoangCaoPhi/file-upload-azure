namespace FileUploadAzure.Abstractions;

public interface IStorageService
{
    Task<AppFile> UploadAsync(Stream stream, AppFile file, string containerName = StorageContainer.Temp, CancellationToken cancellationToken = default);

    Task<FileResponse> DownloadAsync(string fileId, string containerName = StorageContainer.Temp, CancellationToken cancellationToken = default);

    Task DeleteAsync(string fileId, string containerName = StorageContainer.Temp, CancellationToken cancellationToken= default);

    Task CopyTempToAsync(string fileId, string containerName);
}

