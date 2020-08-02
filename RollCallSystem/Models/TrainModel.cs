using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public class TrainModel
    {
        public TrainModel()
        {
        }

        public TrainModel(int studentId, int imageId, Image<Gray, byte> imageGray)
        {
            StudentId = studentId;
            ImageId = imageId;
            ImageGray = imageGray;
        }

        public int StudentId { get; set; }
        public int ImageId { get; set; }
        public Image<Gray,byte> ImageGray { get; set; }
    }
}