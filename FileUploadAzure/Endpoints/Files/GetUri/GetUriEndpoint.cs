using FileUploadAzure.Abstractions;

namespace FileUploadAzure.Endpoints.Files.GetOne;

public class GetUriEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/uri/{fileId}", async (string fileId, IStorageService storageService) =>
        {
            var uri = await storageService.GetFileUriAsync(fileId, StorageContainer.Temp);
            return uri.ToString();
        })
        .WithTags("Files");
    }
}
