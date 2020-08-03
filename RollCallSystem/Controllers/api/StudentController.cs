using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace RollCallSystem.Controllers.api
{
    public class StudentController : BaseApiController
    {
        [HttpGet]
        public ApiResult<List<Student>> GetAllStudent()
        {
            return RCSService.GetAllStudent();
        }
        [HttpPost]
        public ApiResult<TrainFaceModel> TrainStudentFace(TrainFaceModel model)
        {
            return RCSService.TrainStudentFace(model);
        }
        [HttpPost]
        public ApiResult<TrainFaceModel> TrainStudentFaceByImageUpload()
        {
            var r = new ApiResult<TrainFaceModel>();
            TrainFaceModel trainFaceModel = new TrainFaceModel();
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
                trainFaceModel.StudentId = Int32.Parse(httpRequest.Files.AllKeys[0]);
                trainFaceModel.Base64Image = Convert.ToBase64String(thePictureAsBytes);
                r = RCSService.TrainStudentFace(trainFaceModel);
            }
            return r;
        }
        //[HttpPost]
        //public ApiResult<RecognizeModel> RecognizeStudent(RecognizeModel model)
        //{
        //    return RCSService.RecognizeStudent(model);
        //}
        [HttpGet]
        public ApiResult<List<Studying>> GetAllSubject(int studentId)
        {
            return RCSService.GetAllSubject(studentId);
        }
        [HttpGet]
        public ApiResult<Student> GetStudentInfo(int studentId)
        {
            return RCSService.GetStudentInfo(studentId);
        }
        [HttpGet]
        public ApiResult<List<TrainingImage>> GetStudentTrainImages(int studentId)
        {
            return RCSService.GetStudentTrainImages(studentId);
        }
    }
}
