This project is Image Resizer for Linux machins.
  
This project is used from SkiaSharp library and SkiaSharp.NativeAssets.Linux library.





        public static void ResizeImage(Stream fileContents,
        int maxWidth, int maxHeight, string filePath,
        SKFilterQuality quality = SKFilterQuality.Medium)
        {
            fileContents.Seek(0, SeekOrigin.Begin);
            using SKBitmap sourceBitmap = SKBitmap.Decode(fileContents);

            using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(maxWidth, maxHeight), quality);
            using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
            using SKData data = scaledImage.Encode();
            using (var fileStream = System.IO.File.Create(filePath))
            {
                var myStream = data.AsStream();
                myStream.Seek(0, SeekOrigin.Begin);
                myStream.CopyTo(fileStream);
            }
        }
        
        
        
