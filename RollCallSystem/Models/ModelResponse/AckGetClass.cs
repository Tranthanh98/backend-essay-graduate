using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelResponse
{
    public class AckGetClass : AckResponseMonHoc
    {
        public int id { get; set; }
        public TimeSpan? time { get; set; }
        public DateTime? date { get; set; }
        public string phong_hoc { get; set; }
        public int totalSV { get; set; }
        public int? status { get; set; }
        public int? buoi { get; set; }
    }
}