namespace FileUploadAzure.Abstractions;


public class StorageContainer
{
    public const string TempValue = "temp";
    public const string KYCValue = "qt-investor-kyc";
    public const string EmailAttachmentValue = "qt-email-attachments";

    private readonly static StorageContainer _temp = new(TempValue);
    private readonly static StorageContainer _KYC = new(KYCValue);
    private readonly static StorageContainer _emailAttachment = new(EmailAttachmentValue);

    public static StorageContainer Temp => _temp;
    public static StorageContainer KYC => _KYC;
    public static StorageContainer EmailAttachment => _emailAttachment;

    public string Value { get; private set; }
    private StorageContainer(string value) => Value = value;

    public static StorageContainer FromString(string value)
    {
        return value switch
        {
            TempValue => Temp,
            KYCValue => KYC,
            EmailAttachmentValue => EmailAttachment,
            _ => throw new ArgumentException($"Invalid container value: {value}")
        };
    }
}

