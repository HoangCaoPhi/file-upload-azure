using FileUploadAzure.Abstractions;

namespace FileUploadAzure.Endpoints.Files.MoveTemp;
 
public class MoveTempEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/{fileId}", async (string fileId, IStorageService storageService) =>
        {
            await storageService.CopyTempToAsync(fileId, StorageContainer.EmailAttachment);
            return Results.NoContent();
        })
        .WithTags("Files");
    }
}
