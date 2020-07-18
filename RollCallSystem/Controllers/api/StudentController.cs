using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RollCallSystem.Controllers.api
{
    public class StudentController : BaseApiController
    {
        [HttpGet]
        public async Task<ApiResult<List<Student>>> GetAllStudent()
        {
            return await RCSService.GetAllStudent();
        }
        [HttpPost]
        public async Task<ApiResult<TrainingImage>> TrainStudentFace(TrainingStudentFaceModel model)
        {
            return await RCSService.TrainStudentFace(model);
        }
        [HttpPost]
        public async Task<ApiResult<RecognizeModel>> RecognizeStudent(RecognizeModel model)
        {
            return await RCSService.RecognizeStudent(model);
        }
        [HttpGet]
        public async Task<ApiResult<List<Studying>>> GetAllSubjectOfStudent(int studentId)
        {
            return await RCSService.GetAllSubjectOfStudent(studentId);
        }
        [HttpGet]
        public async Task<ApiResult<Student>> GetStudentInfo(int studentId)
        {
            return await RCSService.GetStudentInfo(studentId);
        }
        [HttpGet]
        public async Task<ApiResult<List<TrainingImage>>> GetStudentTrainImages(int studentId)
        {
            return await RCSService.GetStudentTrainImages(studentId);
        }
    }
}
