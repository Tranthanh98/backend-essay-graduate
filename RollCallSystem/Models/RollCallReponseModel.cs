using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public class RollCallReponseModel
    {
        public RollCallReponseModel()
        {
            RollCalls = new List<RollCall>();
        }

        public string Base64Image { get; set; }
        public  List<RollCall> RollCalls { get; set; }
    }
}