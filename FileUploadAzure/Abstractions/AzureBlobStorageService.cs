
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace FileUploadAzure.Abstractions;

public class AzureBlobStorageService(BlobServiceClient blobServiceClient) : IStorageService
{
    public async Task DeleteAsync(string fileId,
                                  string containerName,
                                  CancellationToken cancellationToken = default)
    {
        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName); 
        BlobClient blobClient = blobContainerClient.GetBlobClient(fileId.ToString());
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<FileResponse> DownloadAsync(string fileId,
                                                  string containerName,
                                                  CancellationToken cancellationToken = default)
    {
        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = blobContainerClient.GetBlobClient(fileId.ToString());
        var response = await blobClient.DownloadContentAsync(cancellationToken: cancellationToken);
        return new FileResponse(response.Value.Content.ToStream(), response.Value.Details.ContentType);
    }

    public async Task<AppFile> UploadAsync(Stream stream, 
                                           AppFile file,
                                           string containerName,
                                           CancellationToken cancellationToken = default)
    {
        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName); 
        BlobClient blobClient = blobContainerClient.GetBlobClient(file.FileId);

        await blobClient.UploadAsync(stream,
            new BlobUploadOptions()
            {
                HttpHeaders = new BlobHttpHeaders()
                {
                    ContentType = file.ContentType
                }
            }, cancellationToken);

        return file;
    }

    public async Task CopyTempToAsync(
        string fileId,
        string containerName)
    {
        BlobContainerClient sourceBlobContainer = blobServiceClient.GetBlobContainerClient(StorageContainer.Temp);
        BlobClient sourceBlob = sourceBlobContainer.GetBlobClient(fileId);

        // Lease the source blob to prevent changes during the copy operation
        BlobLeaseClient sourceBlobLease = new(sourceBlob);

        BlobContainerClient destBlockContainer = blobServiceClient.GetBlobContainerClient(containerName);
        BlockBlobClient destinationBlob = destBlockContainer.GetBlockBlobClient(fileId);

        try
        {
            await sourceBlobLease.AcquireAsync(BlobLeaseClient.InfiniteLeaseDuration);

            // Start the copy operation and wait for it to complete
            CopyFromUriOperation copyOperation = await destinationBlob.StartCopyFromUriAsync(sourceBlobLease.Uri);
            await copyOperation.WaitForCompletionAsync();
        }
        catch (RequestFailedException ex)
        {

        }
        finally
        {
            // Release the lease once the copy operation completes
            await sourceBlobLease.ReleaseAsync();
        }
    } 
}
