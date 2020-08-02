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
using System.Web.Configuration;

namespace RollCallSystem.Services
{
    public class DetectService
    {
        private CascadeClassifier detectFaceTrain;
        private CascadeClassifier detectFaceRollCall;
        private static LBPHFaceRecognizer recognizer;
        private static readonly string haarLikePath = WebConfigurationManager.AppSettings["haarLikePath"];
        private List<TrainModel> trainData;
        DetectService()
        {
            detectFaceTrain = new CascadeClassifier(haarLikePath);
            detectFaceRollCall = new CascadeClassifier(haarLikePath);
            recognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 95);
            trainData = new List<TrainModel>();
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
        public Rectangle[] DetectFaceTrain(Image<Gray, byte> grayImage)
        {
            Rectangle[] faces = detectFaceTrain.DetectMultiScale(grayImage, 1.2, 10, new Size(20, 20), Size.Empty);
            return faces;
        }
        public Rectangle[] DetectFaceRollCall(Image<Gray, byte> grayImage)
        {
            Rectangle[] faces = detectFaceRollCall.DetectMultiScale(grayImage, 1.2, 8, new Size(20, 20), Size.Empty);
            return faces;
        }
        private void train()
        {
            int[] studentIds = trainData.Select(t => t.StudentId).ToArray();
            Image<Gray, byte>[] imageGrays = trainData.Select(t => t.ImageGray).ToArray();
            recognizer.Train(imageGrays, studentIds);
        }
        private bool addTrainModel(TrainModel trainModel)
        {
            if (trainModel != null)
            {
                if (trainModel.ImageGray != null)
                {
                    trainData.Add(trainModel);
                }
            }
            return false;
        }
        public void AddTrainModels(TrainModel trainModel, bool retrain = true)
        {
            addTrainModel(trainModel);
            if (retrain)
            {
                train();
            }
        }
        public void AddTrainModels(List<TrainModel> trainModels, bool retrain = true)
        {
            trainModels.ForEach(t =>
            {
                addTrainModel(t);
            });
            if (retrain)
            {
                train();
            }
        }
        public int RecognizeFace(Image<Gray, byte> grayImage)
        {

            FaceRecognizer.PredictionResult result = recognizer.Predict(grayImage.Resize(100, 100, Inter.Cubic));
            if (result.Label != -1)
            {
                return result.Label;
            }
            return -1;
        }
        private void removeTrainByImageId(int imageId)
        {
            var trainModel = trainData.Where(t => t.ImageId == imageId).FirstOrDefault();
            if (trainModel != null)
            {
                trainData.Remove(trainModel);
            }
        }
        public void RemoveTrainsByImageId(int imageId, bool retrain = true)
        {
            removeTrainByImageId(imageId);
            if (retrain)
            {
                train();
            }
        }
        public void RemoveTrainsByImageIds(List<int> imageIds, bool retrain = true)
        {
            imageIds.ForEach(i =>
            {
                removeTrainByImageId(i);
            });
            if (retrain)
            {
                train();
            }
        }
    }
}