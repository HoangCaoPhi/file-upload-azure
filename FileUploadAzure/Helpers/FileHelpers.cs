using FileUploadAzure.Abstractions;
using System.Net;

namespace FileUploadAzure.Helpers;

public static class FileHelpers
{
    // If you require a check on specific characters in the IsValidFileExtensionAndSignature
    // method, supply the characters in the _allowedChars field.
    private static readonly byte[] _allowedChars = { };
    // For more file signatures, see the File Signatures Database (https://www.filesignatures.net/)
    // and the official specifications for the file types you wish to add.
    private static readonly Dictionary<string, List<byte[]>> _fileSignature = new()
    {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".zip", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
    };

    public static AppFile ProcessFormFile(
      IFormFile formFile,
      Stream stream,
      string[] permittedExtensions,
      long sizeLimit)
    {
        var trustedFileNameForDisplay = WebUtility.HtmlEncode(formFile.FileName);

        if (formFile.Length > sizeLimit)
        {
            var megabyteSizeLimit = sizeLimit / 1048576;
            throw new Exception($"Your file exceeds the maximum size limit of {megabyteSizeLimit} MB. Please reduce the file size and try again.");
        }
 
        if (stream.Length == 0)
        {
            throw new InvalidOperationException(
                $"({trustedFileNameForDisplay}) is empty.");
        }

        if (!IsValidFileExtensionAndSignature(formFile.FileName, stream, permittedExtensions))
        {
            throw new InvalidOperationException(
                $"({trustedFileNameForDisplay}) file " +
                "type isn't permitted or the file's signature " +
                "doesn't match the file's extension.");
        }

        stream.Position = 0;

        var ext = Path.GetExtension(formFile.FileName).ToLowerInvariant();
        var appFile = new AppFile()
        {
            FileId = $"{Guid.NewGuid()}{ext}",
            FileName = trustedFileNameForDisplay,
            FileSize = formFile.Length,
            ContentType = formFile.ContentType            
        };

        return appFile;
    }



    private static bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
    {
        if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
        {
            return false;
        }

        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
        {
            return false;
        }

        data.Position = 0;

        var reader = new BinaryReader(data);
        if (ext.Equals(".txt") || ext.Equals(".csv") || ext.Equals(".prn"))
        {
            if (_allowedChars.Length == 0)
            {
                // Limits characters to ASCII encoding.
                for (var i = 0; i < data.Length; i++)
                {
                    if (reader.ReadByte() > sbyte.MaxValue)
                    {
                        return false;
                    }
                }
            }
            else
            {
                // Limits characters to ASCII encoding and
                // values of the _allowedChars array.
                for (var i = 0; i < data.Length; i++)
                {
                    var b = reader.ReadByte();
                    if (b > sbyte.MaxValue ||
                        !_allowedChars.Contains(b))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        // Uncomment the following code block if you must permit
        // files whose signature isn't provided in the _fileSignature
        // dictionary. We recommend that you add file signatures
        // for files (when possible) for all file types you intend
        // to allow on the system and perform the file signature
        // check.
        /*
        if (!_fileSignature.ContainsKey(ext))
        {
            return true;
        }
        */

        // File signature check
        // --------------------
        // With the file signatures provided in the _fileSignature
        // dictionary, the following code tests the input content's
        // file signature.
        var signatures = _fileSignature[ext];
        var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

        return signatures.Any(signature =>
            headerBytes.Take(signature.Length).SequenceEqual(signature));
    }
}
