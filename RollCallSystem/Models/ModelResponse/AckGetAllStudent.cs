using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelResponse
{
    public class AckGetAllStudent :StudentInformation
    {
        public int maMon { get; set; }
        public string tenMon { get; set; }
        public string tenKhoa { get; set; }
        public int totalFaceTrained { get; set; }
        public bool? isSuspended { get; set; }
    }
}