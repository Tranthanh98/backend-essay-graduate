using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RollCallSystem.Controllers.api
{
    public class ClassController : BaseApiController
    {
        [HttpPost]
        public ApiResult<RollCallReponseModel> RollCall(RollCallModel model)
        {
            return RCSService.RollCall(model);
        }
        [HttpGet]
        public ApiResult<ClassSchedule> OpenClass(int classScheduleId)
        {
            return RCSService.OpenClass(classScheduleId);
        }
        [HttpGet]
        public ApiResult<ClassSchedule> CloseClass(int classScheduleId)
        {
            return RCSService.CloseClass(classScheduleId);
        }
        [HttpGet]
        public ApiResult<ClassSchedule> GetClassScheduleFullData(int classScheduleId)
        {
            return RCSService.GetClassScheduleFullData(classScheduleId);
        }
    }
}
