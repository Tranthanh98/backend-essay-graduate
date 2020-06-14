using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelResponse
{
    public class AckLogin
    {
        public int id { get; set; }
        public string name { get; set; }
        public string user_name { get; set; }
        public string password { get; set; }
        public string token { get; set; }
        public int? age { get; set; }
        public string number_phone { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public int? ma_khoa { get; set; }
    }
}