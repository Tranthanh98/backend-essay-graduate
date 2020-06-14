using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelRequest
{
    public class GetClassByDay
    {
        public int teacherId { get; set; }
        public DateTime date { get; set; }
    }
}