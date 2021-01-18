namespace Soap.PfBase.Api.Functions
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public static partial class PlatformFunctions
    {
        public static IActionResult GetLogo(HttpRequest req, ILogger log)
        {
            Serilog.ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                AzureFunctionContext.LoadAppConfig(out var appConfig);

                var bytes = ImageToPngByteArray(DrawText(appConfig.AppFriendlyName));
                return new FileContentResult(bytes, "image/png");
            }
            catch (Exception e)
            {
                logger?.Fatal(e, $"Could not execute function {nameof(GetJsonSchema)}");
                log.LogCritical(e.ToString());
                return new OkObjectResult(e.ToString());
            }
        }

        /// <summary>
        ///     Creates an image containing the given text.
        ///     NOTE: the image should be disposed after use.
        /// </summary>
        /// <param name="text">Text to draw</param>
        /// <param name="fontOptional">Font to use, defaults to Control.DefaultFont</param>
        /// <param name="textColorOptional">Text color, defaults to Black</param>
        /// <param name="backColorOptional">Background color, defaults to white</param>
        /// <param name="minSizeOptional">Minimum image size, defaults the size required to display the text</param>
        /// <returns>The image containing the text, which should be disposed after use</returns>
        private static Image DrawText(
            string text,
            Font fontOptional = null,
            Color? textColorOptional = null,
            Color? backColorOptional = null)
        {
            var font = new Font(FontFamily.GenericMonospace, 16, FontStyle.Bold);

            if (fontOptional != null)
            {
                font = fontOptional;
            }

            var textColor = Color.Black;
            if (textColorOptional != null)
            {
                textColor = (Color)textColorOptional;
            }

            var backColor = Color.White;
            if (backColorOptional != null)
            {
                backColor = (Color)backColorOptional;
            }

            var minSize = Size.Empty;
            // if (minSizeOptional != null)
            // {
            //     minSize = (Size)minSizeOptional;
            // }

            //first, create a dummy bitmap just to get a graphics object
            SizeF textSize;
            using (Image img = new Bitmap(1, 1))
            {
                using (var drawing = Graphics.FromImage(img))
                    //measure the string to see how big the image needs to be
                    textSize = drawing.MeasureString(text, font);
            }

            //create a new image of the right size
            Image retImg = new Bitmap((int)textSize.Width + 50, (int)textSize.Height + 50);
            using (var drawing = Graphics.FromImage(retImg))
            {
                drawing.SmoothingMode = SmoothingMode.AntiAlias;
                drawing.InterpolationMode = InterpolationMode.HighQualityBicubic;
                drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
                drawing.CompositingQuality = CompositingQuality.HighQuality;

                //paint the background

                drawing.Clear(backColor);

                //create a brush for the text
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    drawing.DrawString(
                        text,
                        font,
                        textBrush,
                        (textSize.Width + 50) / 2 - textSize.Width / 2,
                        (textSize.Height + 50) / 2 - textSize.Height / 2);
                    drawing.Save();
                }
            }

            return retImg;
        }

        private static byte[] ImageToPngByteArray(Image imageIn)
        {
            using var ms = new MemoryStream();
            imageIn.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
