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
        public Image<Gray,byte> TrainingImageGray { get; set; }
        public int StudentId { get; set; }
    }
}