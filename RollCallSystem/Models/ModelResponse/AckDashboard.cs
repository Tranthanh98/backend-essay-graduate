using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelResponse
{
    public class AckDashboard
    {
        public List<AckGetClass> listClassInDay { get; set; }
        public List<AckAllClass> listAllClass { get; set; }
    }
}