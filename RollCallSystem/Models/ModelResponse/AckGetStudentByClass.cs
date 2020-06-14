using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelResponse
{
    public class AckGetStudentByClass
    {
        public int MaMon { get; set; }
        public string TenMon { get; set; }
        public int Mssv { get; set; }
        public string NameStudent { get; set; }
        public int LichGiangId { get; set; }
        public DateTime? NgayDay { get; set; }
        public TimeSpan? GioDay { get; set; }
        public int? totalFaceTrained { get; set; }
        public int? isRollCalled { get; set; }
    }
}