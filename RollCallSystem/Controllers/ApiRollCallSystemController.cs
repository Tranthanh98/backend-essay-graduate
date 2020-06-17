using Microsoft.Ajax.Utilities;
using RollCallSystem.Models;
using RollCallSystem.Models.ModelRequest;
using RollCallSystem.Models.ModelResponse;
using RollCallSystem.Services;
using RollCallSystem.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web.Http;
using System.Web.Routing;

namespace RollCallSystem.Controllers
{
    [RoutePrefix("api/ApiRollCallSystem")]
    public class ApiRollCallSystemController : ApiController
    {
        private ServiceContext serviceContext = ServiceContext.Instance();
        private MD5 md5hash = MD5.Create();
        EntitiesDB db = new EntitiesDB();

        [HttpPost]
        [Route("login-teacher")]
        public AcknowledgementResponse<AckLogin> Login(ModelLogin modelLogin)
        {

            var ack = new AcknowledgementResponse<AckLogin>();
            ack.Data = new AckLogin();
            var result = db.TeacherInformations.Where(x => x.user_name == modelLogin.userName && x.password == modelLogin.password).FirstOrDefault();
            if(result != null)
            {
                ack.isSuccess = true;
                string now = HandleDateTime.GetTimeStampNow().ToString();
                MD5 md5Hash = MD5.Create();
                string token = MD5Hash.GetMd5Hash(md5Hash, modelLogin.userName + now);
                result.token = token;
                db.SaveChanges();
                AckLogin t = new AckLogin();
                t.address = result.address;
                t.user_name = result.user_name;
                t.token = result.token;
                t.name = result.name;
                t.ma_khoa = result.ma_khoa;
                t.id = result.id;
                ack.Data = t;
            }
            else
            {
                ack.isSuccess = false;
                ack.AddErrorMessage("user name hoặc password không đúng!");
            }
            
            return ack;
        }

        [HttpPost]
        [Route("logout-teacher")]
        public BaseResponse LogoutTeacher(ModelLogout logoutModel) {
            BaseResponse res = new BaseResponse();
            var result = db.TeacherInformations.Where(x => x.token == logoutModel.token).FirstOrDefault();
            if(result != null)
            {
                res.isSuccess = true;
                result.token = null;
                db.SaveChanges();
            }
            else
            {
                res.isSuccess = false;
                res.AddErrorMessage("token invalid");
            }
            return res;
        }
        [HttpPost]
        [Route("search-class")]
        public AcknowledgementResponse<List<AckResponseMonHoc>> SeachClass(ModelSearchClass modelSearchClass)
        {
            var ack = new AcknowledgementResponse<List<AckResponseMonHoc>>();
            ack.Data = new List<AckResponseMonHoc>();
            List<AckResponseMonHoc> listMonHoc = new List<AckResponseMonHoc>();
            var query = (from mh in db.MonHocs
                        where mh.ten_mon.Contains(modelSearchClass.className)
                        select mh).ToList();
            var testQuery = (from lgd in db.ScheduleTeaches
                             join mh in db.MonHocs on lgd.ma_mon equals mh.ma_mon
                             join tch in db.TeacherInformations on lgd.teacher_id equals tch.id
                             group mh by new
                             {
                                 mh.ma_mon,
                                 mh.ten_mon,
                                 tch.id,
                                 tch.name
                             } into g
                             where g.Key.id == modelSearchClass.teacherId && g.Key.ten_mon.Contains(modelSearchClass.className)
                             select new {
                                    g.Key.ma_mon,
                                    g.Key.ten_mon,
                                    teacherId = g.Key.id,
                                    g.Key.name
                                }).ToList();
            foreach (var item in testQuery)
            {
                AckResponseMonHoc m = new AckResponseMonHoc();
                m.ma_mon = item.ma_mon;
                m.ten_mon = item.ten_mon;
                m.teacher_id = item.teacherId;
                m.ten_giang_vien = item.name;
                listMonHoc.Add(m);
            }
            ack.isSuccess = true;
            ack.Data = listMonHoc;
            return ack;
        }
        [HttpPost]
        [Route("get-student-by-class")]
        public AcknowledgementResponse<List<AckGetStudentByClass>> GetStudentByClass(ModelGetStudent modelGetStudent)
        {
            var testDate = HandleDateTime.CreateDateTimeFromString("2020-05-28");
            var ack = new AcknowledgementResponse<List<AckGetStudentByClass>>();
            ack.isSuccess = false;
            ack.Data = new List<AckGetStudentByClass>();
            
            var data = this.serviceContext.QueryGetStudent(modelGetStudent);
            if(data.Count == 0)
            {
                ack.AddErrorMessage("Bạn không thể điểm danh lớp học này vào ngày hôm nay!");
                return ack;
            }
            else
            {
                ack.Data = data;
            }
            ack.isSuccess = true;
            return ack;
        }
        [HttpPost]
        [Route("get-class-by-day")]
        public AcknowledgementResponse<List<AckGetClass>> GetClassByDay(GetClassByDay getClassByDay)
        {
            var date = getClassByDay.date.Date;
            var ack = new AcknowledgementResponse<List<AckGetClass>>();
            ack.isSuccess = false;
            ack.Data = new List<AckGetClass>();

            var data = this.serviceContext.GetClass(getClassByDay);
            if(data.Count == 0)
            {
                ack.AddErrorMessage("Bạn không có lớp dạy nào ngày hôm này!");
                return ack;
            }
            ack.Data = data;
            ack.isSuccess = true;
            return ack;
        }

        [HttpPost]
        [Route("training-face")]
        public AcknowledgementResponse<string> TrainingFace(ModelTrainingFace modelTrainingFace)
        {
            var ack = new AcknowledgementResponse<string>();
            ack.isSuccess = false;

            var checkStudent = db.StudentInformations.Where(x => x.mssv == modelTrainingFace.Mssv).FirstOrDefault();
            if(checkStudent == null)
            {
                ack.AddErrorMessage("MSSV không hợp lệ!");
                return ack;
            }
            try
            {
                HandleRecognitionFace handleRecognitionFace = new HandleRecognitionFace();
                string fileName = handleRecognitionFace.SaveImageInDatabase(modelTrainingFace.stringImage, modelTrainingFace.Mssv);
                if (handleRecognitionFace.checkSuccess)
                {
                    string[] pathFile = fileName.Split(',');
                    foreach (string path in pathFile)
                    {
                        FaceTrainedStudent newTrain = new FaceTrainedStudent();
                        newTrain.mssv = modelTrainingFace.Mssv;
                        newTrain.link_image = path;

                        db.FaceTrainedStudents.Add(newTrain);
                    }

                    db.SaveChanges();
                }
                else
                {
                    ack.AddErrorMessage(fileName);
                    return ack;
                }
            }catch(Exception ex)
            {
                ack.AddErrorMessage(ex.Message);
            }
            ack.isSuccess = true;
            ack.Data = "Training khuôn mặt sinh viên " + modelTrainingFace.Mssv +" thành công";
            return ack;
        }
        [HttpPost]
        [Route("recognition-student")]
        public AcknowledgementResponse<AckRollCallStudent> RecognitionStudent(ModelRollCall modelRollCall)
        {
            var ack = new AcknowledgementResponse<AckRollCallStudent>();
            ack.isSuccess = false;
            ack.Data = new AckRollCallStudent();

            List<string> pathImageTrained = new List<string>();
            List<string> listIdStudent = new List<string>();

            var getStudentofClass = db.FaceTrainedStudents.Where(x => modelRollCall.listMssv.Contains(x.mssv)).ToList();
            foreach(var item in getStudentofClass)
            {
                pathImageTrained.Add(item.link_image);
                listIdStudent.Add(item.mssv.ToString());
            }
            HandleRecognitionFace handleRecognitionFace = new HandleRecognitionFace();
            
            var recognitionFace = handleRecognitionFace.RecognitionFace(modelRollCall.stringImage, pathImageTrained, listIdStudent);
            
            if (recognitionFace.checkSuccess)
            {

                //query get id of subject class
                var queryGetIdTeach = db.ScheduleTeaches.Where(x => x.date_teach == modelRollCall.date && x.teacher_id == modelRollCall.teacherId && x.ma_mon == modelRollCall.MaMon).FirstOrDefault();

               
                var listMssv = recognitionFace.listStudent.Distinct().ToList();

                //query get students is roll called
                var listStudentOfClass = db.RollCallStudents.Where(x => x.lich_giang_id == queryGetIdTeach.id).ToList();

                var listMssvClone = new List<int>(listMssv);

                foreach(var mssv in listMssv)
                {
                    foreach(var item in listStudentOfClass)
                    {
                        if(item.mssv == mssv)
                        {
                            listMssvClone.Remove((int)item.mssv);
                        }
                    }
                }
                foreach(var mssv in listMssvClone)
                {
                    RollCallStudent rc = new RollCallStudent();
                    rc.mssv = mssv;
                    rc.lich_giang_id = queryGetIdTeach.id;
                    db.RollCallStudents.Add(rc);
                    db.SaveChanges();
                }
                //query get student information roll called
                ack.isSuccess = true;
                var getStudentInf = db.StudentInformations.Where(x => listMssv.Contains(x.mssv)).ToList();
                ack.Data.listStudentRollCall = new List<AckGetStudentByClass>();

                var data = this.serviceContext.QueryGetStudent(modelRollCall);
                ack.Data.listStudentRollCall = data;
                ack.Data.imageAfterRecognition = recognitionFace.imageReturn;
                return ack;
            }
            else
            {
                ack.isSuccess = false;
                ack.AddErrorMessage(recognitionFace.messageError);
                ack.Data.imageAfterRecognition = recognitionFace.imageReturn;
                return ack;
            }
            
        }
        [HttpPost]
        [Route("roll-call-student")]
        public AcknowledgementResponse<List<AckGetStudentByClass>> RollCallStudent(CheckRollCall checkRollCall)
        {

            var queryGetIdTeach = db.ScheduleTeaches.Where(x => x.date_teach == checkRollCall.date && x.teacher_id == checkRollCall.teacherId && x.ma_mon == checkRollCall.MaMon).FirstOrDefault();
            var ack = new AcknowledgementResponse<List<AckGetStudentByClass>>();
            ack.isSuccess = false;
            ack.Data = new List<AckGetStudentByClass>();

            var checkStudent = db.RollCallStudents.Where(x => x.mssv == checkRollCall.mssv && x.lich_giang_id == queryGetIdTeach.id).FirstOrDefault();
            if(checkStudent != null)
            {
                db.RollCallStudents.Remove(checkStudent);
            }
            else
            {

                RollCallStudent rc = new RollCallStudent();
                rc.mssv = checkRollCall.mssv;
                rc.lich_giang_id = queryGetIdTeach.id;
                db.RollCallStudents.Add(rc);
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ack.AddErrorMessage("không thể điểm danh sinh viên này!");
                return ack;
            }
            

            var data = this.serviceContext.QueryGetStudent(checkRollCall);
            if(data.Count == 0)
            {
                ack.AddErrorMessage("xảy ra lỗi!");
                return ack;
            }
            ack.Data = data;
            ack.isSuccess = true;

            return ack;
        }
        [HttpPost]
        [Route("update-status-class")]
        public AcknowledgementResponse<List<AckGetClass>> UpdateStatusClass(ModelUpdateClass modelUpdateClass)
        {
            var ack = new AcknowledgementResponse<List<AckGetClass>>();
            ack.isSuccess = false;
            ack.Data = new List<AckGetClass>();
            var scheduleTeach = db.ScheduleTeaches.Where(x => x.id == modelUpdateClass.idClass).SingleOrDefault();
            if(scheduleTeach == null)
            {
                ack.AddErrorMessage("id môn học không tồn tại!");
                return ack;
            }
            scheduleTeach.status_id = modelUpdateClass.status;
            db.SaveChanges();

            var data = this.serviceContext.GetClass(modelUpdateClass);
            if(data.Count == 0)
            {
                ack.AddErrorMessage("error!");
                return ack;
            }
            //TODO :  check sinh viên bị đình chỉ
            ack.Data = data;
            ack.isSuccess = true;
            return ack;
        }
        [HttpPost]
        [Route("get-student-rollcall")]
        public AcknowledgementResponse<List<StudentInforRollCall>> GetInfStudentRollCall(ModelGetStudent modelGetStudent)
        {
            var ack = new AcknowledgementResponse<List<StudentInforRollCall>>();
            ack.isSuccess = false;
            ack.Data = new List<StudentInforRollCall>();
            var data = this.serviceContext.GetInfStudentRollCall(modelGetStudent.MaMon, modelGetStudent.teacherId);
            
            ack.Data = data;
            ack.isSuccess = true;
            return ack;
        }
        [HttpPost]
        [Route("get-all-class-of-teacher")]
        public AcknowledgementResponse<List<AckAllClass>> GetAllClassOfTeacher(GetClassByDay getClassByDay)
        {
            var ack = new AcknowledgementResponse<List<AckAllClass>>();
            ack.isSuccess = false;
            ack.Data = new List<AckAllClass>();

            var data = this.serviceContext.GetAllClassTeacher(getClassByDay.teacherId);
            if(data.Count == 0)
            {
                ack.AddErrorMessage("không có lớp nào");
                return ack;
            }
            ack.Data = data;
            ack.isSuccess = true;
            return ack;
        }
        [HttpPost]
        [Route("get-all-student-subject")]
        public AcknowledgementResponse<List<AckGetAllStudent>> GetAllStudentSubject(ModelGetStudent modelGetStudent)
        {
            var ack = new AcknowledgementResponse<List<AckGetAllStudent>>();
            ack.isSuccess = false;
            ack.Data = new List<AckGetAllStudent>();

            var query = this.serviceContext.GetAllStudentOfClass(modelGetStudent);
            if(query.Count == 0)
            {
                ack.AddErrorMessage("Không có sinh viên nào học lớp này!");
            }
            else
            {
                ack.Data = query;
                ack.isSuccess = true;
            }
            return ack;
        }
        [HttpPost]
        [Route("get-information-student")]
        public AcknowledgementResponse<AckModelStudentInf> GetInformationStudent(ModelRollCall model)
        {
            var ack = new AcknowledgementResponse<AckModelStudentInf>();
            ack.isSuccess = false;

            var query = db.StudentInformations.Where(x => x.mssv == model.Mssv).FirstOrDefault();
            if(query == null)
            {
                ack.AddErrorMessage("Mã số sinh viên không tồn tại");
                return ack;
            }
            AckModelStudentInf a = new AckModelStudentInf();
            a.mssv = query.mssv;
            a.nameStudent = query.name_student;
            a.email = query.email;
            a.course = query.course;
            a.imageTrained = new List<string>();
            HandleRecognitionFace handle = new HandleRecognitionFace();
            foreach(var item in query.FaceTrainedStudents)
            {
                string pathImage = handle.ConvertPathImageToBase64(item.link_image);
                a.imageTrained.Add(pathImage);
            }
            a.ListRollCall = new List<RollCall>();
            var listDayRollCall = (from rc in query.RollCallStudents
                                   join lgd in db.ScheduleTeaches on rc.lich_giang_id equals lgd.id
                                   where lgd.ma_mon == model.MaMon && lgd.teacher_id == model.teacherId
                                   select new { 
                                    rc.lich_giang_id,
                                    lgd.date_teach,
                                    lgd.phong_hoc,
                                    lgd.buoi
                                   }).ToList();
            foreach(var item in listDayRollCall)
            {
                RollCall rc = new RollCall();
                rc.lichGiangId = item.lich_giang_id;
                rc.ngayDay = item.date_teach;
                rc.phongHoc = item.phong_hoc;
                rc.tuanThu = item.buoi;
                a.ListRollCall.Add(rc);
            }
            ack.Data = a;
            ack.isSuccess = true;

            return ack;
        }
    }
   
}
