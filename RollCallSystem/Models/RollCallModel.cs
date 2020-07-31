using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public partial class RollCall
    {
        public Bitmap Image { get; set; }
    }
    public class RollCallModel
    {
        public RollCallModel()
        {
            RollCalls = new List<RollCall>();
        }
        public int Type { get; set; }
        public int ClassScheduleId { get; set; }
        public string Base64Image { get; set; }
        public HttpPostedFileBase Image { get; set; }
        public List<RollCall> RollCalls { get; set; }
    }
}