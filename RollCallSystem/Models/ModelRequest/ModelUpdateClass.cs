using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelRequest
{
    public class ModelUpdateClass : GetClassByDay
    {
        public int idClass { get; set; }
        public int status { get; set; }
    }
}