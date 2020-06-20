using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelResponse
{
    public class AckTeacherInf
    {
        public string name { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public int? birthday { get; set; }
        public string numberPhone { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string khoa { get; set; }
    }
}