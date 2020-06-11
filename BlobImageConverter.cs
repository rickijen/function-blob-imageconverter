using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AZBlue.Function
{
    public class BlobImageConverter
    {
        [FunctionName("BlobImageConverter")]
        public static void Run([BlobTrigger("sample-images/{name}", Connection = "InputBlobStorage")] Stream image,
            [Blob("sample-images-sm/{name}", FileAccess.Write, Connection = "OutputBlobStorage")] Stream imageSmall,
            [Blob("sample-images-md/{name}", FileAccess.Write, Connection = "OutputBlobStorage")] Stream imageMedium,
            string name,
            ILogger log)
        {
            IImageFormat format;

            using (Image<Rgba32> input = Image.Load<Rgba32>(image, out format))
            {
                ResizeImage(input, imageSmall, ImageSize.Small, format);
            }

            image.Position = 0;
            using (Image<Rgba32> input = Image.Load<Rgba32>(image, out format))
            {
                ResizeImage(input, imageMedium, ImageSize.Medium, format);
            }

            log.LogInformation($"BlobImageConverter function Processed image\n Name:{name} \n Size: {image.Length} Bytes");
        }

        public static void ResizeImage(Image<Rgba32> input, Stream output, ImageSize size, IImageFormat format)
        {
            var dimensions = imageDimensionsTable[size];

            input.Mutate(x => x.Resize(dimensions.Item1, dimensions.Item2));
            input.Save(output, format);
        }

        public enum ImageSize { ExtraSmall, Small, Medium }

        private static Dictionary<ImageSize, (int, int)> imageDimensionsTable = new Dictionary<ImageSize, (int, int)>() {
            { ImageSize.ExtraSmall, (320, 200) },
            { ImageSize.Small,      (640, 400) },
            { ImageSize.Medium,     (800, 600) }
        };

    }
}
