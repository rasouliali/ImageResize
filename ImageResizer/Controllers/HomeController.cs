using ImageResizer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            using (var st = myFile.OpenReadStream())
            {
                var path = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "myfile.jpg");
                ResizeImage(st, 400, 400, path, SKFilterQuality.Low);
            }
            return View("Index","Image resize successfully!");
        }

        public static void ResizeImage(Stream fileContents,
        int maxWidth, int maxHeight, string filePath,
        SKFilterQuality quality = SKFilterQuality.Medium)
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
            using (var fileStream = System.IO.File.Create(filePath))
            {
                var myStream = data.AsStream();
                myStream.Seek(0, SeekOrigin.Begin);
                myStream.CopyTo(fileStream);
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
    }
}
