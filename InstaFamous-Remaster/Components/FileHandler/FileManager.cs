using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;

namespace InstaFamous.Components.FileHandler
{
    class FileManager
    {
        public string WorkingDirectory { get; private set; }

        public FileManager(string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
        }

        /// <summary>
        /// Sets up the FileClient
        /// </summary>
        /// <returns>True if setup was complete</returns>
        public bool Setup()
        {
            if (!Directory.Exists(WorkingDirectory))
            {
                Directory.CreateDirectory(WorkingDirectory);
                return true;
            }

            return true;
        }

        /// <summary>
        /// Returns all of the images in the bot's image directory
        /// </summary>
        /// <returns>List of file paths</returns>
        public List<string> GetImageList()
        {
            return Directory.EnumerateFiles(WorkingDirectory).ToList();
        }

        /// <summary>
        /// Get all of the png images in the working directory
        /// </summary>
        /// <returns>List of file paths</returns>
        public List<string> GetPngImages()
        {
            return Directory.EnumerateFiles(WorkingDirectory).Where(file => file.EndsWith(".png")).ToList();
        }

        /// <summary>
        /// Removes the EXIf data, adds padding and resizes the image to 1080
        /// </summary>
        /// <param name="filePaths"></param>
        public void PrepareImage(string filePath)
        {

            
            AddImagePadding(filePath);
            using (var img = Image.FromFile(filePath))
            {
                if (img.Width > 1080 || img.Height > 1080)
                {
                    ResizeImage(filePath);
                }

                img.Dispose();
            }
            RemoveExif(filePath);
        }

        /// <summary>
        /// Converts a .png image to jpeg
        /// </summary>
        /// <param name="filePath">Filepath of the file to convert</param>
        /// <returns>True if the operation was succesfull</returns>
        public bool ChangePictureFormat(string filePath)
        {
            // Get the JPG file path
            var jpgFilePath = filePath.Replace(".png", ".jpg");

            // Check if the file we're trying to read exists
            if (File.Exists(filePath))
            {

                // Attempt to load the file and save it again as a jpg
                using (var pngImage = Image.FromFile(filePath))
                {
                    pngImage.Save(jpgFilePath, ImageFormat.Jpeg);
                    pngImage.Dispose();
                }

                return true;
            }
            else
            {
                // Unable to find the file, throw an exception
                throw new FileNotFoundException($"Unable to find {filePath}");
            }
        }

        /// <summary>
        /// Removes all the profiles and comments off an image
        /// </summary>
        /// <param name="filePath"></param>
        private void RemoveExif(string filePath)
        {
            using (MagickImage metaImage = new MagickImage())
            {
                metaImage.Read(filePath);
                metaImage.Strip();
                metaImage.Write(filePath);
                metaImage.Dispose();
            }
        }

        /// <summary>
        /// Creates a new square image and draws the original image in the center.
        /// </summary>
        /// <param name="filePath"></param>
        private void AddImagePadding(string filePath)
        {
            using (var originalImage = Image.FromFile(filePath))
            {
                // Check if the width or height is the largest
                var largestDimension = Math.Max(originalImage.Height, originalImage.Width);
                // Create a new square bitmap
                var squareImage = new Bitmap(largestDimension, largestDimension);
                // Create a new graphics element to draw the image on
                using (var graphics = Graphics.FromImage(squareImage))
                {
                    // Fill the image with a white background
                    graphics.FillRectangle(Brushes.White, 0, 0, largestDimension, largestDimension);
                    // Set the image to have only the highest of qualities
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    // Draw the original image
                    graphics.DrawImage(originalImage, (largestDimension / 2) - (originalImage.Width / 2), (largestDimension / 2) - (originalImage.Height / 2), originalImage.Width, originalImage.Height);

                    graphics.Dispose();
                }

                originalImage.Dispose();
                // Delete the old image and save the new one
                File.Delete(filePath);
                squareImage.Save(filePath);
                squareImage.Dispose();

            }
        }

        /// <summary>
        /// Resizes the image to 1080x1080
        /// </summary>
        /// <param name="filePath"></param>
        private void ResizeImage(string filePath)
        {
            using (var img = Image.FromFile(filePath))
            {
                var destinationRect = new Rectangle(0, 0, 1080, 1080);
                var destinationImg = new Bitmap(1080, 1080);

                using (var graphics = Graphics.FromImage(destinationImg))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(img, destinationRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, wrapMode);

                        wrapMode.Dispose();
                    }
                    graphics.Dispose();
                }
                img.Dispose();
                
            }
        }
    }
}
