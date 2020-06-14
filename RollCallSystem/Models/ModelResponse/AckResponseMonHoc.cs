using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelResponse
{
    public class AckResponseMonHoc
    {
        public int? ma_mon { get; set; }
        public string ten_mon { get; set; }
        public int? teacher_id { get; set; }
        public string ten_giang_vien { get; set; }
    }
}