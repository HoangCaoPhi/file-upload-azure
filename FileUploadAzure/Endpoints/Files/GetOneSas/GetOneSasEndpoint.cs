using Azure.Storage.Blobs;
using FileUploadAzure.Abstractions;

namespace FileUploadAzure.Endpoints.Files.GetOneSas;

public class GetOneSasEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sas/{fileId}", async (IStorageService storageService,
                                          BlobServiceClient blobServiceClient,
                                          string fileId) =>
        {
            var containerClient = blobServiceClient.GetBlobContainerClient("temp");
            var blob = containerClient.GetBlobClient(fileId);
            var result = await storageService.CreateServiceSASBlob(blob);
            return result;
        });
    }
}
