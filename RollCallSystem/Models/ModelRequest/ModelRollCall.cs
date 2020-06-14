using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelRequest
{
    public class ModelRollCall : ModelGetStudent
    {
        public int Mssv { get; set; }
        public string stringImage { get; set; }
        public List<int?> listMssv { get; set; }
        //public int MaMon { get; set; }
        //public int teacherId { get; set; }
        //public DateTime date { get; set; }
    }
}