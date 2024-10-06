using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace FileUploadAzure.Abstractions;

public class AzureBlobStorageService(BlobServiceClient blobServiceClient) : IStorageService
{
    public async Task DeleteAsync(string fileId,
                                  StorageContainer container,
                                  CancellationToken cancellationToken = default)
    {
        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(container.Value);
        BlobClient blobClient = blobContainerClient.GetBlobClient(fileId.ToString());
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<FileResponse> DownloadAsync(string fileId,
                                                  StorageContainer container,
                                                  CancellationToken cancellationToken = default)
    {
        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(container.Value);
        BlobClient blobClient = blobContainerClient.GetBlobClient(fileId.ToString());
        var response = await blobClient.DownloadContentAsync(cancellationToken: cancellationToken);
        return new FileResponse(response.Value.Content.ToStream(), response.Value.Details.ContentType);
    }

    public async Task<AppFile> UploadAsync(Stream stream,
                                           AppFile file,
                                           StorageContainer container,
                                           CancellationToken cancellationToken = default)
    {
        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(container.Value);
        BlobClient blobClient = blobContainerClient.GetBlobClient(file.FileId);

        var blobUploadOptions = new BlobUploadOptions()
        {
            HttpHeaders = new BlobHttpHeaders()
            {
                ContentType = file.ContentType
            },
            Metadata = new Dictionary<string, string>()
            {
                ["x-file-name"] = file.FileName,
                ["x-file-size"] = file.FileSize.ToString()
            }
        };

        await blobClient.UploadAsync(stream, blobUploadOptions, cancellationToken);

        return file;
    }

    public async Task CopyTempToAsync(
        string fileId,
        StorageContainer container,
        CancellationToken cancellationToken = default)
    {
        BlobContainerClient sourceBlobContainer = blobServiceClient.GetBlobContainerClient(StorageContainer.Temp.Value);
        BlobClient sourceBlob = sourceBlobContainer.GetBlobClient(fileId);

        BlobLeaseClient sourceBlobLease = new(sourceBlob);

        BlobContainerClient destBlockContainer = blobServiceClient.GetBlobContainerClient(container.Value);
        BlockBlobClient destinationBlob = destBlockContainer.GetBlockBlobClient(fileId);

        await sourceBlobLease.AcquireAsync(BlobLeaseClient.InfiniteLeaseDuration, cancellationToken: cancellationToken);
        CopyFromUriOperation copyOperation = await destinationBlob.StartCopyFromUriAsync(sourceBlobLease.Uri, cancellationToken: cancellationToken);
        await copyOperation.WaitForCompletionAsync(cancellationToken);

        await sourceBlobLease.ReleaseAsync(cancellationToken: cancellationToken);
    }

    public async Task<Uri?> GetFileUriAsync(string fileId,
                                       StorageContainer container,
                                       CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(container.Value);
        BlobClient blobClient = containerClient.GetBlobClient(fileId);

        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }

        return blobClient.Uri;
    }
}
