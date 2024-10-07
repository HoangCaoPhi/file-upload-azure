using Azure.Storage.Blobs;
using FileUploadAzure.Abstractions;

namespace FileUploadAzure.Endpoints.Files.GetSasToken;

public class GetSasTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("sas", async (IStorageService storageService,
                                       BlobServiceClient blobServiceClient) =>
        {
            var containerClient = blobServiceClient.GetBlobContainerClient("temp");
            var result = await storageService.CreateServiceSASContainer(containerClient);
            return result;
        });
    }
}
