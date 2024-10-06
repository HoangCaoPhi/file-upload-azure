namespace FileUploadAzure.Abstractions;

public class StorageOptions
{
    public static string Section = "BlobStorage";

    public int MaximumUploadFileSizeDefault { get; set; }

    public string[] PermittedExtensionsDefault { get; set; } = [];

    public string ConnectionString { get; set; }
}
