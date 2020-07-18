using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Text;
using Microsoft.Ajax.Utilities;
using RollCallSystem.Helper;
using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RollCallSystem.Services
{
    public class RCSService : BaseSevice
    {
        private static bool isFirstTrained = false;
        public RCSService() : base()
        {
            if (!isFirstTrained)
            {
                var trainModels = getAllTrainModel();
                if (trainModels.Count > 0)
                {
                    DetectService.TrainFace(trainModels);
                    isFirstTrained = true;
                }
            }
        }
        public async Task<ApiResult<User>> Login(LoginModel login)
        {
            var apiResult = new ApiResult<User>();
            apiResult.IsSuccess = false;
            if (login == null)
            {
                apiResult.Messages.Add("Login model invalid.");
                return apiResult;
            }
            var user = await RCSContext.Users.Where(u => u.Username == login.Username).FirstOrDefaultAsync();
            if (user == null)
            {
                apiResult.Messages.Add("Tài khoản không tồn tại.");
                return apiResult;
            }
            var pass = parseToMD5Byte(login.Password);
            if (!compareByteArray(pass, user.Password))
            {
                apiResult.Messages.Add("Sai mật khẩu.");
                return apiResult;
            }
            if (!user.IsActive)
            {
                apiResult.Messages.Add("Tài khoản bị khóa.");
                return apiResult;
            }
            var userInfo = new User()
            {
                Id = user.Id,
                Role = user.Role,
                IsActive = user.IsActive,
            };
            if (user.Role == (int)EUserRole.STUDENT)
            {
                userInfo.Student = await RCSContext.Students.Where(s => s.UserId == userInfo.Id).FirstOrDefaultAsync();
                if (userInfo.Student != null)
                {
                    userInfo.Student.User = null;
                    userInfo.Student.TrainingImages = null;
                }
                else
                {
                    apiResult.IsSuccess = false;
                    apiResult.Messages.Add("Tài khoản này chưa liên kết với sinh viên.");
                    return apiResult;
                }
            }
            else
            {
                userInfo.Teacher = await RCSContext.Teachers.Where(t => t.UserId == userInfo.Id).FirstOrDefaultAsync();
                if (userInfo.Teacher != null)
                    userInfo.Teacher.User = null;
                else
                {
                    apiResult.IsSuccess = false;
                    apiResult.Messages.Add("Tài khoản này chưa liên kết với giáo viên.");
                    return apiResult;
                }
            }
            apiResult.Data = userInfo;
            apiResult.IsSuccess = true;
            apiResult.Messages.Add("Login successfully");
            return apiResult;
        }
        public async Task<ApiResult<List<User>>> CreateAccount(List<UserCreateModel> newUsers)
        {
            var apiResult = new ApiResult<List<User>>();
            var users = new List<User>();
            newUsers.ForEach(u =>
            {
                users.Add(new User()
                {
                    Password = parseToMD5Byte(u.Password),
                    Username = u.Username,
                    Role = u.Role,
                    IsActive = true,
                });
            });
            RCSContext.Users.AddRange(users);
            try
            {
                await RCSContext.SaveChangesAsync();
                apiResult.IsSuccess = true;
            }
            catch (Exception e)
            {
                apiResult.IsSuccess = false;
            }
            return apiResult;
        }
        public async Task<ApiResult<User>> GetCurrentUser()
        {
            var apiResult = new ApiResult<User>();
            var id = getCurrentUserId();
            var user = await RCSContext.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
            apiResult.Data = new User()
            {
                Id = user.Id,
                Role = user.Role,
                IsActive = user.IsActive
            };
            apiResult.IsSuccess = true;
            return apiResult;
        }
        public async Task<ApiResult<TrainingImage>> TrainStudentFace(TrainingStudentFaceModel model)
        {
            var ar = new ApiResult<TrainingImage>();
            var trainingImage = new TrainingImage();
            var student = await RCSContext.Students.Where(s => s.Id == model.StudentId).FirstOrDefaultAsync();
            if (student == null)
            {
                ar.IsSuccess = false;
                ar.Messages.Add("Mã số sinh viên không tồn tại.");
            }
            else
            {
                var f = DetectService.DetectFace(model.Base64Image.base64ToImageGray());
                if (f.Count() <= 0)
                {
                    ar.IsSuccess = false;
                    ar.Messages.Add("Không nhận diện được khuôn mặt, vui lòng chọn nơi có ánh sáng đầy đủ và điều chỉnh hướng nhìn.");
                }
                else if (f.Count() == 1)
                {
                    var trainImage = saveImageTraining(model.StudentId, model.Base64Image, f[0]);
                    var trainModels = new List<TrainModel>();
                    trainModels.Add(new TrainModel
                    {
                        StudentId = trainImage.StudentId,
                        TrainingImageGray = trainImage.FileAttachment.FileData.Data.byteArrToImageGray()
                    });
                    DetectService.TrainFace(trainModels);
                    trainImage.FileAttachment = null;
                    trainImage.Student = null;
                    ar.Data = trainImage;
                    ar.IsSuccess = true;
                    ar.Messages.Add("Training thành công.");
                }
                else
                {
                    ar.IsSuccess = false;
                    ar.Messages.Add("Chỉ được có một khuôn mặt khi training.");
                }
            }
            return ar;
        }
        private TrainingImage saveImageTraining(int studentId, string base64Image, Rectangle r)
        {
            var image = base64Image.base64ToImage();
            var bitmap = new Bitmap(image);
            bitmap = bitmap.cropAtRect(r);
            bitmap = new Bitmap(bitmap, new Size(100, 100));
            var fileData = new FileData()
            {
                Data = bitmap.bitmapToByteArr()
            };
            var file = new FileAttachment()
            {
                CreateDate = DateTime.Now,
                Height = image.Height,
                Width = image.Width,
                Extension = "jpg",
                Type = (int)EFileType.ImageJPG,
                Name = Guid.NewGuid().ToString(),
                FileData = fileData
            };
            var trainingImage = new TrainingImage()
            {
                CreatedDate = DateTime.Now,
                StudentId = studentId,
                FileAttachment = file
            };
            RCSContext.TrainingImages.Add(trainingImage);
            try
            {
                RCSContext.SaveChanges();
            }
            catch (Exception e)
            {
                var a = 1;
            }
            return trainingImage;
        }
        public async Task<ApiResult<RecognizeModel>> RecognizeStudent(RecognizeModel model)
        {
            var ar = new ApiResult<RecognizeModel>();
            var recognizeModel = new RecognizeModel();
            var studentIds = new List<int>();
            var image = model.Base64Image.base64ToImage();
            var bitmap = new Bitmap(image);
            var imageGray = model.Base64Image.base64ToImageGray();
            var rectangles = DetectService.DetectFace(imageGray);
            if (rectangles.Count() <= 0)
            {
                ar.IsSuccess = false;
                ar.Messages.Add("Không có khuôn mặt nào trong ảnh được phát hiện.");
                return ar;
            }
            rectangles.ForEach(r =>
            {
                var b = bitmap.cropAtRect(r);
                imageGray = new Image<Gray, byte>(b);
                var studentId = DetectService.RecognizeFace(imageGray);
                if (studentId != -1) studentIds.Add(studentId);
            });
            if (studentIds.Count <= 0)
            {
                ar.IsSuccess = false;
                ar.Messages.Add("Các sinh viên không thuộc lớp học này.");
                return ar;
            }
            var students = await RCSContext.Students.Where(s => studentIds.Any(st => st == s.Id)).ToListAsync();
            students.ForEach(s =>
            {
                s.User = null;
                s.TrainingImages = null;
                s.Studyings = null;
                s.Course = null;
            });
            recognizeModel = model;
            recognizeModel.Students = students;
            ar.Data = recognizeModel;
            ar.IsSuccess = true;
            return ar;
        }
        public void rollCall(int onClassId, List<int> studentIds)
        {
            //var studentOnClasses = new List<StudentOnClass>();
            //studentIds.ForEach(s =>
            //{
            //    studentOnClasses.Add(new StudentOnClass
            //    {
            //        StudentId = s,
            //        OnClassId = onClassId,
            //        CreatedDate = DateTime.Now
            //    });
            //});
            //RCSContext.StudentOnClasses.AddRange(studentOnClasses);
            //try
            //{
            //    RCSContext.SaveChanges();
            //}
            //catch(Exception e)
            //{

            //}
        }
        public List<TrainModel> getAllTrainModel()
        {
            var trainModels = new List<TrainModel>();
            var ar = new ApiResult<List<Student>>();
            var datas = (from s in RCSContext.Students
                         join t in RCSContext.TrainingImages on s.Id equals t.StudentId into t1
                         from t in t1.DefaultIfEmpty()
                         join f in RCSContext.FileAttachments on t.FileId equals f.Id into f1
                         from f in f1.DefaultIfEmpty()
                         join fd in RCSContext.FileDatas on f.Id equals fd.FileId into fd1
                         from fd in fd1.DefaultIfEmpty()
                         select new
                         {
                             student = s,
                             trainimage = t,
                             file = f,
                             fileData = fd
                         }).ToList();
            var students = datas.Select(d => d.student).Distinct().ToList();
            datas.ForEach(d =>
            {
                if (d.fileData != null)
                {
                    trainModels.Add(new TrainModel()
                    {
                        StudentId = d.student.Id,
                        TrainingImageGray = d.fileData.Data.byteArrToImageGray()
                    }); ;
                }
            });
            return trainModels;
        }
        public async Task<ApiResult<List<Student>>> GetAllStudent()
        {
            var ar = new ApiResult<List<Student>>();
            var a = getAllTrainModel();
            ar.IsSuccess = true;
            return ar;
        }
        public async Task<ApiResult<List<Studying>>> GetAllSubjectOfStudent(int studentId)
        {
            var ar = new ApiResult<List<Studying>>();
            var studyings = new List<Studying>();
            var data = await (from s in RCSContext.Studyings
                              where s.StudentId == studentId
                              join c in RCSContext.Classes on s.ClassId equals c.Id into c1
                              from c in c1.DefaultIfEmpty()
                              join sj in RCSContext.Subjects on c.SubjectId equals sj.Id into sj1
                              from sj in sj1.DefaultIfEmpty()
                              join t in RCSContext.Teachers on c.TeacherId equals t.Id into t1
                              from t in t1.DefaultIfEmpty()
                              join cs in RCSContext.ClassSchedules on c.Id equals cs.ClassId into cs1
                              from cs in cs1.DefaultIfEmpty()
                              join rc in RCSContext.StudentRollCalls on cs.Id equals rc.ClassScheduleId into rc1
                              from rc in rc1.DefaultIfEmpty()
                              select new { s, c, sj, cs, rc,t }).ToListAsync();
            studyings = data.Select(d => d.s).ToList();
            studyings.ForEach(s =>
            {
                s.Class.Studyings = null;
                s.Class.Subject.Classes = null;
                s.Class.Teacher.Classes = null;
                s.Class.ClassSchedules.ForEach(c =>
                {
                    c.Class = null;
                    c.StudentRollCalls.ForEach(rc =>
                    {
                        rc.ClassSchedule = null;
                    });
                });
            });
            ar.Data = studyings;
            ar.IsSuccess = true;
            return ar;
        }
        public async Task<ApiResult<Student>> GetStudentInfo(int studentId)
        {
            var ar = new ApiResult<Student>();
            var student1 = await (from s in RCSContext.Students
                                  where s.Id == studentId
                                  join c in RCSContext.Courses on s.CourseId equals c.Id into c1
                                  from c in c1.DefaultIfEmpty()
                                  join m in RCSContext.Majors on s.MajorId equals m.Id into m1
                                  from m in m1.DefaultIfEmpty()
                                  join ms in RCSContext.MajorSpecialties on s.MajorSpecialtyId equals ms.Id into ms1
                                  from ms in ms1.DefaultIfEmpty()
                                  join f in RCSContext.Faculties on m.FacultyId equals f.Id into f1
                                  from f in f1.DefaultIfEmpty()
                                  join t in RCSContext.TrainingImages on s.Id equals t.StudentId into t1
                                  select new { s, c, m, ms, f, t1 }
                       ).FirstOrDefaultAsync();
            var student = student1.s;
            if (student == null)
            {
                ar.IsSuccess = false;
                ar.Messages.Add("Sinh viên không tồn tại");
            }
            else
            {
                student.TrainingImages.ForEach(i => { i.Student = null; i.FileAttachment = null; });
                student.MajorSpecialty.Students = null;
                student.Major.Students = null;
                student.Major.MajorSpecialties = null;
                student.Course.Students = null;
                student.Major.Faculty.Majors = null;
                ar.Data = student;
                ar.IsSuccess = true;
            }
            return ar;
        }
        public async Task<ApiResult<List<TrainingImage>>> GetStudentTrainImages(int studentId)
        {
            var ar = new ApiResult<List<TrainingImage>>();
            var trainImages = new List<TrainingImage>();
            trainImages = await RCSContext.TrainingImages.Where(t => t.StudentId == studentId).ToListAsync();
            trainImages.ForEach(t =>
            {
                t.Student = null;
                t.FileAttachment = null;
            });
            ar.IsSuccess = true;
            ar.Data = trainImages;
            return ar;
        }
        public HttpResponseMessage GetFileDataById(int fileId)
        {
            var fileData = new FileData();
            try
            {
                fileData =  RCSContext.FileDatas.Where(f => f.FileId == fileId).FirstOrDefault();
            }
            catch (Exception e)
            {
                var a = 0;
            }
            if (fileData == null)
            {
                return null;
            }
            MemoryStream ms = new MemoryStream(fileData.Data);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(ms);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");
            return response;
        }
        public ApiResult<int> DeleteFile(int fileId)
        {
            var ar = new ApiResult<int>();
            var trainingImage = RCSContext.TrainingImages.Where(t => t.FileId == fileId).FirstOrDefault();
            if (trainingImage == null)
            {
                ar.Data = fileId;
                ar.IsSuccess = false;
                ar.Messages.Add("Không tìm thấy file.");
            }
            else
            {
                ar.Data = fileId;
                RCSContext.TrainingImages.Remove(trainingImage);
                RCSContext.SaveChanges();
                ar.IsSuccess = true;
                ar.Messages.Add("Xóa file thành công.");
            }
            return ar;
        }
    }
}