using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

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

        UserDelegationKey userDelegationKey =
        await blobServiceClient.GetUserDelegationKeyAsync(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(1));

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

    public static async Task CreateStoredAccessPolicyAsync(BlobContainerClient containerClient)
    {
        // Create a stored access policy with read and write permissions, valid for one day
        List<BlobSignedIdentifier> signedIdentifiers = new List<BlobSignedIdentifier>
        {
            new() {
                Id = "sample-read-write-policy",
                AccessPolicy = new BlobAccessPolicy
                {
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddDays(1),
                    Permissions = "rw"
                }
            },
            new() {
                Id = "sample-read-policy",
                AccessPolicy = new BlobAccessPolicy
                {
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddDays(1),
                    Permissions = "r"
                }
            }
        };

        // Set the container's access policy
        await containerClient.SetAccessPolicyAsync(permissions: signedIdentifiers);
    }

    public async Task<Uri> CreateServiceSASContainer(BlobContainerClient containerClient,
                                                     string storedPolicyName = null)
    {
        // Check if BlobContainerClient object has been authorized with Shared Key
        if (containerClient.CanGenerateSasUri)
        {
            // Create a SAS token that's valid for one day
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerClient.Name,
                Resource = "c"
            };

            if (storedPolicyName == null)
            {
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddDays(1);
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Write);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            Uri sasURI = containerClient.GenerateSasUri(sasBuilder);

            return sasURI;
        }
        else
        {
            // Client object is not authorized via Shared Key
            return null;
        }
    }

    public async Task<Uri> CreateServiceSASBlob(BlobClient blobClient, string storedPolicyName = null)
    {
        // Check if BlobContainerClient object has been authorized with Shared Key
        if (blobClient.CanGenerateSasUri)
        {
            // Create a SAS token that's valid for one day
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = "b"
            };

            if (storedPolicyName == null)
            {
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddDays(1);
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            Uri sasURI = blobClient.GenerateSasUri(sasBuilder);

            return sasURI;
        }
        else
        {
            // Client object is not authorized via Shared Key
            return null;
        }
    }
}
