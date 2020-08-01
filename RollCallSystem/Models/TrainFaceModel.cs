using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public class TrainFaceModel : TrainingImage
    {
        public string Base64Image { get; set; }
        public HttpPostedFileBase Image { get; set; }
    }
}