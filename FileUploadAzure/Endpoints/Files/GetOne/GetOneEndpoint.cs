using FileUploadAzure.Abstractions;

namespace FileUploadAzure.Endpoints.Files.GetOne;

public class GetOneEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{fileId}", async (string fileId, IStorageService storageService) =>
        {
            FileResponse fileResponse = await storageService.DownloadAsync(fileId, StorageContainer.Temp);
            return Results.File(fileResponse.Stream, fileResponse.ContentType);
        })
        .WithTags("Files");
    }
}
