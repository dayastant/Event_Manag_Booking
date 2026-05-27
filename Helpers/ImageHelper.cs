using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace Event_Management_System.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Converts an uploaded image file to Base64 string with data URL prefix
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static async Task<string> ConvertToBase64(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                
                // Resize if image is too large
                imageBytes = ResizeImage(imageBytes, 300, 300);
                
                string base64String = Convert.ToBase64String(imageBytes);
                string contentType = file.ContentType ?? "image/png";
                
                return $"data:{contentType};base64,{base64String}";
            }
        }

        /// <summary>
        /// Generates a profile picture with user's initials
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static string GenerateInitialsImage(string firstName, string lastName)
        {
            // Get initials
            string initials = "";
            if (!string.IsNullOrEmpty(firstName))
                initials += firstName[0];
            if (!string.IsNullOrEmpty(lastName))
                initials += lastName[0];
            
            initials = initials.ToUpper();
            if (string.IsNullOrEmpty(initials))
                initials = "U"; // Default for "User"

            // Color palette for backgrounds
            var colors = new[]
            {
                Color.FromArgb(79, 70, 229),   // Purple
                Color.FromArgb(59, 130, 246),  // Blue
                Color.FromArgb(16, 185, 129),  // Green
                Color.FromArgb(245, 158, 11),  // Orange
                Color.FromArgb(239, 68, 68),   // Red
                Color.FromArgb(168, 85, 247),  // Violet
                Color.FromArgb(20, 184, 166),  // Teal
            };

            // Select color based on first letter
            int colorIndex = (initials[0] % colors.Length);
            Color backgroundColor = colors[colorIndex];

            // Create bitmap
            int size = 150;
            using (var bitmap = new Bitmap(size, size))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    // Set high quality
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    // Draw background
                    using (var brush = new SolidBrush(backgroundColor))
                    {
                        graphics.FillRectangle(brush, 0, 0, size, size);
                    }

                    // Draw initials
                    using (var font = new Font("Arial", 60, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.White))
                    {
                        var stringFormat = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };

                        graphics.DrawString(initials, font, textBrush, 
                            new RectangleF(0, 0, size, size), stringFormat);
                    }
                }

                // Convert to base64
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    byte[] imageBytes = memoryStream.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);
                    return $"data:image/png;base64,{base64String}";
                }
            }
        }

        /// <summary>
        /// Resizes an image to fit within the specified dimensions while maintaining aspect ratio
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static byte[] ResizeImage(byte[] imageBytes, int maxWidth, int maxHeight)
        {
            using (var inputStream = new MemoryStream(imageBytes))
            using (var image = Image.FromStream(inputStream))
            {
                // Calculate new dimensions
                int newWidth = image.Width;
                int newHeight = image.Height;
                
                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    double ratioX = (double)maxWidth / image.Width;
                    double ratioY = (double)maxHeight / image.Height;
                    double ratio = Math.Min(ratioX, ratioY);
                    
                    newWidth = (int)(image.Width * ratio);
                    newHeight = (int)(image.Height * ratio);
                }

                using (var bitmap = new Bitmap(newWidth, newHeight))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        
                        graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                    }

                    using (var outputStream = new MemoryStream())
                    {
                        bitmap.Save(outputStream, ImageFormat.Png);
                        return outputStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Converts a base64 data URL to byte array and extracts content type
        /// </summary>
        public static (byte[] bytes, string contentType) ConvertDataUrlToBytes(string dataUrl)
        {
            // Format: data:image/png;base64,iVBORw0KG...
            if (string.IsNullOrEmpty(dataUrl) || !dataUrl.StartsWith("data:"))
            {
                throw new ArgumentException("Invalid data URL format");
            }

            // Extract content type and base64 data
            var parts = dataUrl.Split(new[] { ';', ',' }, 3);
            if (parts.Length < 3)
            {
                throw new ArgumentException("Invalid data URL format");
            }

            string contentType = parts[0].Substring(5); // Remove "data:"
            string base64Data = parts[2];

            byte[] bytes = Convert.FromBase64String(base64Data);
            
            return (bytes, contentType);
        }
    }
}
