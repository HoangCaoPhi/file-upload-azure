namespace FileUploadAzure.Abstractions;

public class AppFile
{
    public string FileId { get; set; }

    public string FileName { get; set; }

    public long FileSize { get; set; }

    public string ContentType { get; set; }
}
