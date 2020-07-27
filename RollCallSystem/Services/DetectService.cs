using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Ocl;
using Emgu.CV.Structure;
using RollCallSystem.Helper;
using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace RollCallSystem.Services
{
    public class DetectService
    {
        internal class Train
        {
            public Train()
            {
                TrainingImageGrays = new List<Image<Gray, byte>>();
                Labels = new List<int>();
            }

            public List<Image<Gray,byte>> TrainingImageGrays { get; set; }
            public List<int> Labels { get; set; }
        }
        private CascadeClassifier detectFace;
        private static LBPHFaceRecognizer recognizer;
        private Train train; 
        DetectService()
        {
            detectFace = new CascadeClassifier("D:\\GitHub\\backend-essay-graduate\\RollCallSystem\\App_Data\\FaceRecognition\\haarcascade_frontalface_default.xml");
            recognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 100);
            train = new Train();
        }
        private static readonly object padlock = new object();
        private static DetectService instance = null;
        public static DetectService Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new DetectService();
                    }
                    return instance;
                }
            }
        }
        public Rectangle[] DetectFace(Image<Gray, byte> grayImage)
        {
            Rectangle[] faces = detectFace.DetectMultiScale(grayImage, 1.2, 10, new Size(20, 20), Size.Empty);
            return faces;
        }
        public void TrainFace(List<TrainModel> trainModels)
        {
            trainModels.ForEach(t =>
            {
                train.TrainingImageGrays.Add(t.TrainingImageGray);
                train.Labels.Add(t.StudentId);
            });
            recognizer.Train(train.TrainingImageGrays.ToArray(), train.Labels.ToArray());
        }
        public int RecognizeFace(Image<Gray,byte> grayImage)
        {
            FaceRecognizer.PredictionResult result = recognizer.Predict(grayImage.Resize(100, 100, Inter.Cubic));
            if (result.Label != -1)
            {
                return result.Label;
            }
            return -1;
        }
    }
}