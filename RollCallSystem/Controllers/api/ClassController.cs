using Microsoft.AspNetCore.Http;
using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Web;
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
        [HttpPost]
        public ApiResult<RollCallReponseModel> RollCallByImageUpload()
        {
            var r = new ApiResult<RollCallReponseModel>();
            RollCallModel rollCallModel = new RollCallModel();
            var httpRequest = System.Web.HttpContext.Current.Request;
            if (httpRequest.Files.Count < 1)
            {
                r.Messages.Add("Vui lòng upload ảnh");
            }
            else if (httpRequest.Files.Count > 1)
            {
                r.Messages.Add("Vui lòng chỉ uplaod 1 ảnh duy nhât");
            }
            else
            {
                HttpPostedFile file = httpRequest.Files[0];
                byte[] thePictureAsBytes = new byte[file.ContentLength];
                using (BinaryReader theReader = new BinaryReader(file.InputStream))
                {
                    thePictureAsBytes = theReader.ReadBytes(file.ContentLength);
                }
                rollCallModel.ClassScheduleId = Int32.Parse(httpRequest.Files.AllKeys[0]);
                rollCallModel.Base64Image = Convert.ToBase64String(thePictureAsBytes);
                r = RCSService.RollCall(rollCallModel);
            }
            return r;
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
