using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text;

namespace ColinApp.Common.Captcha
{
    public class CaptchaHelper
    {
        private static readonly char[] _charset = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
        private static readonly Random _rand = new();

        public static (string Code, string ImageBase64) GenerateCaptchaImage(int length = 5)
        {
            string code = GenerateRandomCode(length);

            int width = 120;
            int height = 40;
            var image = new Image<Rgba32>(width, height);
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.White);

                // 加干扰线
                for (int i = 0; i < 5; i++)
                {
                    var p1 = new PointF(_rand.Next(width), _rand.Next(height));
                    var p2 = new PointF(_rand.Next(width), _rand.Next(height));
                    ctx.DrawLine(Color.LightGray, 1, p1, p2);
                }

                // 写字
                var font = SystemFonts.CreateFont("Arial", 24, FontStyle.Bold);
                ctx.DrawText(code, font, Color.Black, new PointF(10, 5));

                // 加干扰点
                for (int i = 0; i < 150; i++)
                {
                    int x = _rand.Next(width);
                    int y = _rand.Next(height);
                    var color = Color.FromRgb((byte)_rand.Next(255), (byte)_rand.Next(255), (byte)_rand.Next(255));
                    ctx.DrawLine(color, 1, new PointF(x, y), new PointF(x + 1, y + 1));
                }
            });

            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            var base64 = Convert.ToBase64String(ms.ToArray());
            return (code, $"data:image/png;base64,{base64}");
        }

        private static string GenerateRandomCode(int length)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(_charset[_rand.Next(_charset.Length)]);
            }
            return sb.ToString();
        }
    }
}
