using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.DataProtection;
using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace RollCallSystem.Helper
{
    public static class CastService
    {
        public static StudentInfo ToStudentInfo(this Student student)
        {
            var s = new StudentInfo()
            {
                Id = student.Id,
                Name = student.Name,
                Birthday = student.Birthday,
                Hometown = student.Hometown,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                Address = student.Address,
            };
            return s;
        }
        public static Image base64ToImage(this string src)
        {
            Image image;
            var imageByteArr = Convert.FromBase64String(src);
            using (var ms = new MemoryStream(imageByteArr, 0, imageByteArr.Length))
            {
                image = Image.FromStream(ms, true);
            }
            return image;
        }
        public static byte[] bitmapToByteArr(this Bitmap bitmap)
        {
            byte[] byteArr;
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Jpeg);
                byteArr = memoryStream.ToArray();
            }
            return byteArr;
        }
        public static Bitmap cropAtRect(this Bitmap src, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            using (Graphics g = Graphics.FromImage(nb))
            {
                g.DrawImage(src, -r.X, -r.Y);
                return nb;
            }
        }
        public static Image<Gray, byte> base64ToImageGray(this string base64)
        {
            var image = base64.base64ToImage();
            var bitmap = new Bitmap(image);
            var gray = (new Image<Gray, byte>(bitmap));
            return gray;
        }
        public static Image<Gray, byte> byteArrToImageGray(this byte[] byteArr)
        {
            var binaryImage = byteArr;
            var base64 = Convert.ToBase64String(binaryImage);
            return base64.base64ToImageGray();
        }
    }
}