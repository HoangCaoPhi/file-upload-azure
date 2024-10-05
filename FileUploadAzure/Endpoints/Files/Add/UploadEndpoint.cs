using FileUploadAzure.Abstractions;
using FileUploadAzure.Helpers;

namespace FileUploadAzure.Endpoints.Files.Add;

public class UploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files", async (IFormFile file, IStorageService storageService) =>
        { 
            var stream = file.OpenReadStream();
            var appFile = FileHelpers.ProcessFormFile(file, stream, [".png", ".jpg"], 10485768);

            var id = await storageService.UploadAsync(stream, appFile);
            return Results.Ok(id);
        })
        .WithTags("Files")
        .DisableAntiforgery();
    }
}
