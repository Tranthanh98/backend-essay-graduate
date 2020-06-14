using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelResponse
{
    public class AckRollCallStudent
    {
        public List<AckGetStudentByClass> listStudentRollCall { get; set; }
        public string imageAfterRecognition { get; set; }
    }
}