using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public class ClassSeachModel
    {
        public ClassSeachModel()
        {
        }

        public ClassSeachModel(int teacherId, DateTime date)
        {
            TeacherId = teacherId;
            Date = date;
        }

        public int TeacherId { get; set; }
        public DateTime? Date { get; set; }
    }
}