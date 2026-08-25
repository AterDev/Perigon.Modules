using System.Globalization;
using System.Linq;
using CMSMod.Models.ArticleDtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Share.Exceptions;

namespace CMSMod.Services;

/// <summary>
/// Stores public article images under the CMS module web root.
/// </summary>
public sealed class ArticleImageStorageService(
    IWebHostEnvironment environment,
    ILogger<ArticleImageStorageService> logger
)
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> AllowedTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".gif"] = "image/gif",
            [".webp"] = "image/webp",
        };

    public async Task<ArticleImageUploadDto> SaveAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            throw new BusinessException("文章图片不能为空", StatusCodes.Status400BadRequest);

        if (file.Length > MaxFileSize)
            throw new BusinessException("文章图片不能超过10MB", StatusCodes.Status413PayloadTooLarge);

        string extension = Path.GetExtension(Path.GetFileName(file.FileName));
        if (!AllowedTypes.TryGetValue(extension, out string? expectedContentType)
            || !string.Equals(file.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("文章图片格式不支持", StatusCodes.Status400BadRequest);
        }

        if (!await HasValidSignatureAsync(file, extension, cancellationToken))
            throw new BusinessException("文章图片内容无效", StatusCodes.Status400BadRequest);

        string datePath = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string relativeDirectory = Path.Combine("article", datePath);
        string directory = Path.Combine(environment.WebRootPath, relativeDirectory);
        Directory.CreateDirectory(directory);

        string fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        string filePath = Path.Combine(directory, fileName);

        await using (Stream source = file.OpenReadStream())
        await using (FileStream target = new(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await source.CopyToAsync(target, cancellationToken);
        }

        string publicPath = $"/article/{datePath}/{fileName}";
        logger.LogInformation("Stored article image at {ArticleImagePath}", publicPath);
        return new ArticleImageUploadDto { Path = publicPath };
    }

    private static async Task<bool> HasValidSignatureAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        byte[] header = new byte[12];
        await using Stream stream = file.OpenReadStream();
        int read = await stream.ReadAsync(header.AsMemory(), cancellationToken);

        return extension.ToLowerInvariant() switch
        {
            ".png" => read >= 8 && header[..8].SequenceEqual(
                new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".jpg" or ".jpeg" => read >= 3
                && header[0] == 0xFF
                && header[1] == 0xD8
                && header[2] == 0xFF,
            ".gif" => read >= 6
                && (header[..6].SequenceEqual("GIF87a"u8)
                    || header[..6].SequenceEqual("GIF89a"u8)),
            ".webp" => read >= 12
                && header[..4].SequenceEqual("RIFF"u8)
                && header[8..12].SequenceEqual("WEBP"u8),
            _ => false,
        };
    }
}
