using FileUploadAzure.Abstractions;
using FileUploadAzure.Helpers;
using Microsoft.Extensions.Options;

namespace FileUploadAzure.Endpoints.Files.Add;

public class UploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files", async (IFormFile file, 
                                    IStorageService storageService,
                                    IOptions<StorageOptions> options) =>
        {
            var blobOptions = options.Value;

            var stream = file.OpenReadStream();

            var appFile = FileHelpers.ProcessFormFile(file, 
                                                      stream, 
                                                      blobOptions.PermittedExtensionsDefault,
                                                      blobOptions.MaximumUploadFileSizeDefault);

            var result = await storageService.UploadAsync(stream, appFile, StorageContainer.Temp);
            return Results.Ok(result);
        })
        .WithTags("Files")
        .DisableAntiforgery();
    }
}
