using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace Event_Management_System.Helpers
{
    public class QRCodeService
    {
        [SupportedOSPlatform("windows")]
        public byte[] GenerateQRCode(string text)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
            using (QRCode qrCode = new QRCode(qrCodeData))
            {
                using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                using (MemoryStream ms = new MemoryStream())
                {
                    qrCodeImage.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

        [SupportedOSPlatform("windows")]
        public string GenerateQRCodeBase64(string text)
        {
            byte[] qrBytes = GenerateQRCode(text);
            return Convert.ToBase64String(qrBytes);
        }
    }
}
