using RollCallSystem.Models.ModelResponse;
using RollCallSystem.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelRequest
{
    public class NowClass
    {
        public List<StudentInforRollCall> listStudent { get; set; }
        public AckGetClass nowClass { get; set; }

    }
}