namespace FileUploadAzure.Abstractions;

public record FileResponse(Stream Stream, string ContentType);
