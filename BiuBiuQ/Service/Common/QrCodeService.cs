using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BiuBiuQ.Service.Common
{
    public class QrCodeService
    {
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="content">要生成二维码的内容</param>
        /// <returns></returns>
        public static BitmapImage QrCodeCreate(string content)
        {
            QRCodeGenerator qrCodeGenerator = new QRCodeGenerator();
            var data = qrCodeGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(data);
            var bytes = qrCode.GetGraphic(5);
            BitmapImage image = ToBitmapImage(bytes);
            return image;
        }


        /// <summary>
        /// 字节数据转Bitmap
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Bitmap ToBitmap(byte[] bytes)
        {
            Bitmap bitmap = new Bitmap(1, 1);
            using (var memoryStream = new MemoryStream(bytes))
            {
                bitmap = new Bitmap(memoryStream);
            }
            return bitmap;
        }

        /// <summary>
        /// 字节数据转BitmapImage
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static BitmapImage ToBitmapImage(byte[] bytes)
        {
            BitmapImage img = new BitmapImage();

            img.BeginInit();
            img.StreamSource = new MemoryStream(bytes);
            img.EndInit();

            return img;
        }

        /// <summary>
        /// BitmapImage保存到文件
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Guid SavePhoto(BitmapImage image)
        {
            Guid photoID = System.Guid.NewGuid();
            string photolocation = photoID.ToString() + ".jpg";  //file name 

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (FileStream filestream = new FileStream(photolocation, FileMode.Create))
            {
                encoder.Save(filestream);
            }

            return photoID;
        }

    }
}
