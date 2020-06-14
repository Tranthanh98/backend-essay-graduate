using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using RollCallSystem.Models.ModelResponse;
using RollCallSystem.Services.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace RollCallSystem.Services
{
    public class HandleRecognitionFace
    {
        public bool checkSuccess = false;

        Image<Bgr, byte> image;
        Image<Gray, byte> grayImage;
        List<Image<Gray, byte>> listImageTrained = new List<Image<Gray, byte>>();
        List<string> listName = new List<string>();
        //CascadeClassifier detectFace = new CascadeClassifier(System.IO.Path.GetFullPath(@"../App_Data/FaceRecogintion/haarcascade_frontalface_default.xml"));
        CascadeClassifier detectFace = new CascadeClassifier("D:\\LuanVanTotNghiep\\RollCallSystem\\RollCallSystem\\App_Data\\FaceRecognition\\haarcascade_frontalface_default.xml");
        //CascadeClassifier detectEyes = new CascadeClassifier("D:\\LuanVanTotNghiep\\RollUpStudent\\RollUpStudent\\App_Data\\FaceRecogintion\\haarcascade_eye.xml");
        EigenFaceRecognizer eigenFaceRecognizer;
        LBPHFaceRecognizer lBPHFaceRecognizer;
        FaceRecognizer fishserFaceRecognizer;

        public static string headerBase64 = "";

        public string DetectFaceFromImageBase64(string stringBase64)
        {

            Image decodeBase64 = Base64ToImage(stringBase64);
            Bitmap bitmap = new Bitmap(decodeBase64);
            image = new Image<Bgr, byte>(bitmap);
            grayImage = image.Convert<Gray, byte>();

            Rectangle[] rectangles = detectFace.DetectMultiScale(grayImage, 1.2, 10, new Size(20, 20), Size.Empty);
            string imageReturn;
            if (rectangles.Count() == 0)
            {
                //image.Draw("Không phat hiện được khuôn mặt", ref font)
                imageReturn = ConvertImageBitmapToBase64(bitmap);
                return imageReturn;
            }
            int i = 0;
            foreach (Rectangle f in rectangles)
            {
                i++;
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    using (Pen pen = new Pen(Color.Green, 5))
                    {
                        graphics.DrawRectangle(pen, f);
                    }
                }
                grayImage.ROI = f;
                grayImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic).ToBitmap();


                image = new Image<Bgr, byte>(bitmap);
                CvInvoke.PutText(image, //image drawed
                    "me", //text printed
                    new Point(f.X, f.Y - 10), // coordinates display the first character
                    Emgu.CV.CvEnum.FontFace.HersheyTriplex, //type font
                    0.7, // ratio text to image
                    new Bgr(Color.Green).MCvScalar, //Color
                    1 //border
                    );
            }
            imageReturn = ConvertImageBitmapToBase64(image.ToBitmap());
            return imageReturn;
        }
        public Image Base64ToImage(string base64String)
        {
            int indexString = base64String.IndexOf(",");
            headerBase64 = base64String.Substring(0, indexString + 1);
            string newBase64String = base64String.Substring(indexString + 1);
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(newBase64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        public string ConvertImageBitmapToBase64(Bitmap imageBitmap)
        {
            Bitmap bImage = imageBitmap;  // Your Bitmap Image
            System.IO.MemoryStream ms = new MemoryStream();
            bImage.Save(ms, ImageFormat.Jpeg);
            byte[] byteImage = ms.ToArray();
            var SigBase64 = Convert.ToBase64String(byteImage);
            return headerBase64 + SigBase64;
        }
        public string SaveImageInDatabase(string stringBase64, int studentId)
        {
            try
            {
                double timestamp = HandleDateTime.GetTimeStampNow();
                string fileName = studentId.ToString() + "_" + timestamp.ToString();
                var ctx = HttpContext.Current;
                var root = ctx.Server.MapPath("~/App_Data/FaceTraining/");

                Image decodeBase64 = Base64ToImage(stringBase64);
                Bitmap bitmap = new Bitmap(decodeBase64);
                image = new Image<Bgr, byte>(bitmap);
                grayImage = image.Convert<Gray, byte>();
                //grayImage._EqualizeHist();

                Rectangle[] rectangles = detectFace.DetectMultiScale(grayImage, 1.2, 10, new Size(20, 20), Size.Empty);
                if (rectangles.Count() == 0)
                {
                    fileName = "Không phát hiện được khuôn mặt";
                    return fileName;
                }
                if(rectangles.Count() > 1)
                {
                    fileName = "Hình phải có duy nhất một khuôn mặt";
                    return fileName;
                }
                foreach (Rectangle f in rectangles)
                {
                    grayImage.ROI = f;

                    break;
                }
                string fileFace = root + fileName + ".bmp";
                string fileFaceFlip = root + fileName + "_flip.bmp";

                var flipFImage = grayImage.Flip(FlipType.Horizontal);

                flipFImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic).ToBitmap().Save(fileFaceFlip);
                grayImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic).ToBitmap().Save(fileFace);
                this.checkSuccess = true;
                return fileFace + "," + fileFaceFlip;
            }
            catch (Exception ex)
            {
                return "Lỗi";
            }
        }
        public ModelRecognitionFace RecognitionFace(string imageBase64, List<string> listPathImage, List<string> listIdLabels)
        {
            ModelRecognitionFace modelRecognition = new ModelRecognitionFace();
            modelRecognition.checkSuccess = false;
            //modelRecognition.listStudent = new List<int>();
            if (listPathImage.Count == 0 || listIdLabels.Count == 0)
            {
                
                modelRecognition.messageError = "không có ảnh nào được train trước đó";
                return modelRecognition;
            }
            //double rolateImage = 0;
            int eigenTrainedImageCounter = 0;
            List<int> eigenIntlabels = new List<int>();
            foreach (var fileName in listPathImage)
            {
                Image<Gray, Byte> TrainedImage = new Image<Gray, Byte>(fileName);
                listImageTrained.Add(TrainedImage);
                eigenIntlabels.Add(eigenTrainedImageCounter);
                eigenTrainedImageCounter++;
            }
            //this code for recognition face by EigenFaceRecognizer
            eigenFaceRecognizer = new EigenFaceRecognizer(eigenTrainedImageCounter, 4200);
            eigenFaceRecognizer.Train(listImageTrained.ToArray(), eigenIntlabels.ToArray());

            //this code for recognition face by LBPHFaceRecognizer
            lBPHFaceRecognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 95);
            lBPHFaceRecognizer.Train(listImageTrained.ToArray(), eigenIntlabels.ToArray());

            //this code for recognition face by FishserFaceRecognizer
            fishserFaceRecognizer = new FisherFaceRecognizer(0, 4000);
            fishserFaceRecognizer.Train(listImageTrained.ToArray(), eigenIntlabels.ToArray());

            Image decodeBase64 = Base64ToImage(imageBase64);
            Bitmap bitmap = new Bitmap(decodeBase64);

            image = new Image<Bgr, byte>(bitmap);
            grayImage = image.Convert<Gray, byte>();
            //grayImage._EqualizeHist();

            Rectangle[] rectangles = detectFace.DetectMultiScale(grayImage, 1.2, 10, new Size(20, 20), Size.Empty);
            if(rectangles.Count() == 0)
            {
                modelRecognition.messageError = "Không phát hiện được khuôn mặt!";
                return modelRecognition;
            }

            foreach (Rectangle f in rectangles)
            {
                image.Draw(f, new Bgr(Color.Green), 3);

                grayImage.ROI = f;

                //Rectangle[] eyesRectangles = detectEyes.DetectMultiScale(grayImage);
                //foreach (Rectangle eye in eyesRectangles)
                //{
                //    Rectangle e = eye;
                //    e.Offset(f.X, f.Y);
                //    image.Draw(e, new Bgr(Color.Red), 2);
                //}
                //if (eyesRectangles.Length == 2)
                //{
                //    //rolate image by eyes
                //    var deltaY = (eyesRectangles[0].Y + eyesRectangles[0].Height / 2) - (eyesRectangles[1].Y + eyesRectangles[1].Height / 2);
                //    var deltaX = (eyesRectangles[0].X + eyesRectangles[0].Width / 2) - (eyesRectangles[1].X + eyesRectangles[1].Width / 2);
                //    double degrees = (Math.Atan2(deltaY, deltaX) * 180) / Math.PI;
                //    if(degrees < 45)
                //    {
                //        var rolate = 180 - degrees;
                //        rolateImage = rolate;
                //        grayImage = grayImage.Rotate(rolate, new Gray());
                //    }

                //}

                // CvInvoke.cvResetImageROI(grayImage);

                //recognition by EigenFace
                FaceRecognizer.PredictionResult result = eigenFaceRecognizer.Predict(grayImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic));
                //recognition by LBHP
                FaceRecognizer.PredictionResult lbhpRecognition = lBPHFaceRecognizer.Predict(grayImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic));
                //recognition by Fisher
                FaceRecognizer.PredictionResult fisherResult = fishserFaceRecognizer.Predict(grayImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic));

                var distanceEigen = (double)result.Distance == double.MaxValue;
                var distanceLBHP = (double)lbhpRecognition.Distance == double.MaxValue;
                var distanceFisher = (double)fisherResult.Distance == double.MaxValue;
                string labelName;
                //image = new Image<Bgr, byte>(bitmap);
                modelRecognition.listStudent = new List<int>();
                if (lbhpRecognition.Label != -1)
                {
                    labelName = listIdLabels[lbhpRecognition.Label].ToString();
                    modelRecognition.listStudent.Add(int.Parse(labelName));
                    CvInvoke.PutText(image, //image drawed
                        labelName, //text printed
                        new Point(f.X, f.Y - 10), // coordinates display the first character
                        Emgu.CV.CvEnum.FontFace.HersheyTriplex, //type font
                        0.5, // ratio text to image
                        new Bgr(Color.Red).MCvScalar, //Color
                        1,
                        LineType.EightConnected//border
                        );
                    string imageReturn = ConvertImageBitmapToBase64(image.ToBitmap());
                    modelRecognition.checkSuccess = false;
                    modelRecognition.imageReturn = imageReturn;
                    //return modelRecognition;
                }
                //modelRecognition.listStudent = listIdStudent;
                //else if(!distanceFisher && !distanceEigen)
                //{
                //    labelName = listIdLabels[fisherResult.Label].ToString();
                //}
                //else if(!distanceEigen)
                //{
                //    labelName = listIdLabels[result.Label].ToString();
                //}
                else
                {
                    labelName = "Unknown";
                    CvInvoke.PutText(image, //image drawed
                        labelName, //text printed
                        new Point(f.X, f.Y - 10), // coordinates display the first character
                        Emgu.CV.CvEnum.FontFace.HersheyTriplex, //type font
                        0.5, // ratio text to image
                        new Bgr(Color.Red).MCvScalar, //Color
                        1,
                        LineType.EightConnected//border
                        );
                    string imageReturn = ConvertImageBitmapToBase64(image.ToBitmap());
                    modelRecognition.checkSuccess = false;
                    modelRecognition.imageReturn = imageReturn;
                    //return modelRecognition;
                }
                //CvInvoke.PutText(image, //image drawed
                //        labelName, //text printed
                //        new Point(f.X, f.Y - 10), // coordinates display the first character
                //        Emgu.CV.CvEnum.FontFace.HersheyTriplex, //type font
                //        0.5, // ratio text to image
                //        new Bgr(Color.Red).MCvScalar, //Color
                //        1,
                //        LineType.EightConnected//border
                //        );
            }
            //image.Rotate(rolateImage, new Bgr(Color.Gray));
            //string imageReturn = ConvertImageBitmapToBase64(image.ToBitmap());
            //modelRecognition.checkSuccess = true;
            //modelRecognition.imageReturn = imageReturn;
            return modelRecognition;
        }
    }
}