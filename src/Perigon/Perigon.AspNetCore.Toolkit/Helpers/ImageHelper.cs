using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Perigon.AspNetCore.Toolkit.Helpers;

/// <summary>
/// 图形帮助类
/// </summary>
public class ImageHelper
{
    /// <summary>
    /// 生成图形验证码
    /// </summary>
    /// <returns>png文件的bytes</returns>
    public static byte[] GenerateImageCaptcha(
        string captchaText,
        int width = 80,
        int height = 40,
        int fontSize = 24
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(captchaText);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fontSize);

        var fontFamily = SystemFonts.Collection.Families.First();
        var font = fontFamily.CreateFont(fontSize);
        var random = Random.Shared;

        using var image = new Image<Rgba32>(width, height, Color.White);
        image.Mutate(context =>
        {
            context.DrawText(captchaText, font, Color.Black, new PointF(width * 0.2f, height * 0.2f));

            for (var i = 0; i < 8; i++)
            {
                context.DrawLine(
                    GetRandomColor(random),
                    1,
                    new PointF(random.Next(width), random.Next(height)),
                    new PointF(random.Next(width), random.Next(height))
                );
            }

            for (var i = 0; i < 100; i++)
            {
                context.Fill(GetRandomColor(random), new Rectangle(random.Next(width), random.Next(height), 1, 1));
            }
        });

        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    private static Color GetRandomColor(Random random) => Color.FromRgb(
        (byte)random.Next(256),
        (byte)random.Next(256),
        (byte)random.Next(256)
    );
}
