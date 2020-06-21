using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelResponse
{
    public class AckModelStudentInf
    {
        public int mssv { get; set; }
        public string nameStudent { get; set; }
        public string email { get; set; }
        public int? course { get; set; }
        public List<string> imageTrained { get; set; }
        public bool? isSuspended { get; set; }
        public int totalBuoiHoc { get; set; }
        public List<RollCall> ListRollCall { get; set; }
    }
    public class RollCall
    {
        public int lichGiangId { get; set; }
        public DateTime? ngayDay { get; set; }
        public string phongHoc { get; set; }
        public int? tuanThu { get; set; }
    }
}