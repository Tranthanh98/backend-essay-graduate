using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public class RecognizeModel
    {
        public string Base64Image { get; set; }
        public List<Student> Students { get; set; }
        public int ClassScheduleId { get; set; }
    }
}