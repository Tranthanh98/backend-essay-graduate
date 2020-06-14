using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Services.Models
{
    public class ModelRecognitionFace
    {
        public string imageReturn { get; set; }
        public List<int> listStudent { get; set; }
        public bool checkSuccess { get; set; }
        public string messageError { get; set; }
    }
}