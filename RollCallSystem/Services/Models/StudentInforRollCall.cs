using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Services.Models
{
    public class StudentInforRollCall : StudentInformation
    {
        public int maMon { get; set; }
        public string tenMon { get; set; }
        public bool? isSuspended { get; set; }
        public int countRollCall { get; set; }
    }
}