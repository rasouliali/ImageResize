using ImageResizer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageResizer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public HomeController(ILogger<HomeController> logger, IHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult UploadImage(IFormFile myFile)
        {
            var res = "";
            var res2 = "";
            var res3 = "";
            using (var st = myFile.OpenReadStream())
            {
                var path = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "myfile.jpg");
                res = ResizeImage(st, 400, 400, path, SKFilterQuality.Low);
            }
            using (var st = myFile.OpenReadStream())
            {
                var path2 = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "myfile2.jpg");
                res2 = dotnetResizeImage(st, 400, 400, path2);
            }
            using (var st = myFile.OpenReadStream())
            {
                var path3 = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "myfile3.jpg");
                res3 = SixLabResizeImage(st, 400, 400, path3);
            }
            return View("Index",
                res == "ok" && res2 == "ok" ?
                "Images resize successfully!"
                : "skia image:" + res + ";dot not images:" + res2+ ";sixlab images:" + res3);
        }

        public string ResizeImage(Stream fileContents,
        int maxWidth, int maxHeight, string filePath,
        SKFilterQuality quality = SKFilterQuality.Medium)
        {
            try
            {

                fileContents.Seek(0, SeekOrigin.Begin);
                using SKBitmap sourceBitmap = SKBitmap.Decode(fileContents);

                maxWidth = maxWidth == 0 ? sourceBitmap.Width : maxWidth;
                maxWidth = maxWidth > 600 ? 600 : maxWidth;
                maxHeight = maxHeight == 0 ? sourceBitmap.Height : maxHeight;
                maxHeight = maxHeight > 600 ? 600 : maxHeight;
                if (Math.Abs((sourceBitmap.Width * 1.0 / maxWidth) - (sourceBitmap.Height * 1.0 / maxHeight)) > 0.1)
                {
                    var w = (sourceBitmap.Width * 1.0 / maxWidth);
                    var newHeight = (sourceBitmap.Height * 1.0 / w);
                    maxHeight = (int)newHeight;
                }
                int height = Math.Min(maxHeight, sourceBitmap.Height);
                int width = Math.Min(maxWidth, sourceBitmap.Width);

                using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), quality);
                using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
                using SKData data = scaledImage.Encode();
                //if (System.IO.File.Exists(filePath))
                //    System.IO.File.Delete(filePath);
                using (var fileStream = System.IO.File.Create(filePath))
                {
                    var myStream = data.AsStream();
                    myStream.Seek(0, SeekOrigin.Begin);
                    myStream.CopyTo(fileStream);
                }
                _logger.LogInformation("ok");
                return "ok";
            }
            catch (Exception err)
            {
                _logger.LogInformation(err.ToString());
                return err.Message;

            }
        }
        public string dotnetResizeImage(Stream fileContents,
        int Width, int Height, string saveFilePath)
        {
            try
            {
                //var imgFormat = GetEncoder(saveFilePath);
                //using var sourceImage = Image.Load(ToByteArray(streamImg), out (SixLabors.ImageSharp.Formats.IImageFormat)imgFormat);

                Bitmap sourceImage = new Bitmap(fileContents);
                //sourceImage.Mutate(x => x.Resize(Width, Height));
                //sourceImage.Save(saveFilePath, GetEncoder(saveFilePath));

                using (Bitmap objBitmap = new Bitmap(Width, Height))
                {
                    if (sourceImage.HorizontalResolution > 72)
                        objBitmap.SetResolution(72, 72);
                    else
                        objBitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
                    using (Graphics objGraphics = Graphics.FromImage(objBitmap))
                    {
                        //objGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                        //objGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

                        // Set the graphic format for better result cropping   
                        //objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        //objGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                        objGraphics.DrawImage(sourceImage, 0, 0, Width, Height);

                        // Save the file path, note we use png format to support png file   
                        objBitmap.Save(saveFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);// GetImageFormat(saveFilePath));
                    }
                }
                _logger.LogInformation("ok");
                return "ok";
            }
            catch (Exception err)
            {
                _logger.LogInformation(err.ToString());
                return err.Message;
            }
        }
        
        public string SixLabResizeImage(Stream fileContents,
        int Width, int Height, string saveFilePath)
        {
            try
            {
                using (FileStream output = System.IO.File.OpenWrite(saveFilePath))
                {
                    SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(fileContents);
                        image.Mutate(
                            i => i.Resize(Width, Height)
                                  );
                    
                        image.Save(output, GetEncoder(saveFilePath));
                }

                _logger.LogInformation("ok");
                return "ok";
            }
            catch (Exception err)
            {
                _logger.LogInformation(err.ToString());
                return err.Message;
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        private static SixLabors.ImageSharp.Formats.IImageEncoder GetEncoder(string saveFilePath)
        {
            var ext = saveFilePath.Substring(saveFilePath.LastIndexOf('.')).ToLower();
            switch (ext)//".jpg", ".jpeg", ".bmp", ".png", ".gif", ".tiff"
            {
                case ".jpg":
                case ".jpeg":
                    return new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
                case ".bmp":
                    return new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder();
                case ".png":
                    return new SixLabors.ImageSharp.Formats.Png.PngEncoder();
                case ".gif":
                    return new SixLabors.ImageSharp.Formats.Gif.GifEncoder();
                case ".tiff":
                case ".tif":
                    return new SixLabors.ImageSharp.Formats.Tiff.TiffEncoder();
                default:
                    return new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
            }
        }
    }
}
