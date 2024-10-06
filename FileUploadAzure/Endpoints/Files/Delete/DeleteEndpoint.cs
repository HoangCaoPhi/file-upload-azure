using FileUploadAzure.Abstractions;

namespace FileUploadAzure.Endpoints.Files.Delete;

public class DeleteEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("files/{fileId}", async (string fileId, IStorageService storageService) =>
        {
            await storageService.DeleteAsync(fileId, StorageContainer.Temp);
            return Results.NoContent;
        })
       .WithTags("Files");
    }
}
