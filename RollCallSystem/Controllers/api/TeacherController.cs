using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RollCallSystem.Controllers.api
{
    public class TeacherController : BaseApiController
    {
        [HttpPost]
        public ApiResult<List<Class>> GetAllClassByTeacherId(ClassSeachModel seachModel)
        {
            return RCSService.GetAllClassByTeacherId(seachModel);
        }
        [HttpGet]
        public ApiResult<Teacher> GetTeacherInfo(int teacherId)
        {
            return RCSService.GetTeacherInfo(teacherId);
        }
        [HttpGet]
        public ApiResult<Class> GetClass(int classId)
        {
            return RCSService.GetClass(classId);
        }
        [HttpPost]
        public ApiResult<ClassSchedule> UpdateClassSchedule(ClassSchedule classSchedule)
        {
            return RCSService.UpdateClassSchedule(classSchedule);
        }
        [HttpGet]
        public ApiResult<RollCall> ChangeRollCall(int classScheduleId, int studentId)
        {
            return RCSService.ChangeRollCall(classScheduleId, studentId);
        }
    }
}
