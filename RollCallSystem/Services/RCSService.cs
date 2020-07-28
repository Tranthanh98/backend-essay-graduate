using Emgu.CV;
using Emgu.CV.Cuda;
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
using System.Linq.Expressions;
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
            if (user.Role == (int)EUserRole.Student)
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
                var imageGray = model.Base64Image.base64ToImageGray();
                imageGray._EqualizeHist();
                var f = DetectService.DetectFace(imageGray);
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
            imageGray._EqualizeHist();
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
                         join t in RCSContext.TrainingImages on s.Id equals t.StudentId 
                         join f in RCSContext.FileAttachments on t.FileId equals f.Id 
                         join fd in RCSContext.FileDatas on f.Id equals fd.FileId 
                         select new
                         {
                             student = s,
                             trainimage = t,
                             file = f,
                             fileData = fd
                         }).ToList();
            datas.ForEach(d =>
            {
                if (d.fileData != null)
                {
                    var imageGray = d.fileData.Data.byteArrToImageGray();
                    imageGray._EqualizeHist();
                    trainModels.Add(new TrainModel()
                    {
                        StudentId = d.student.Id,
                        TrainingImageGray = imageGray
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
        public async Task<ApiResult<List<Studying>>> GetAllSubject(int studentId)
        {
            var ar = new ApiResult<List<Studying>>();
            try
            {
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
                                  join rc in RCSContext.RollCalls on new { cdId = cs.Id, stId = studentId } equals new { cdId = rc.ClassScheduleId, stId = rc.StudentId } into rc1
                                  from rc in rc1.DefaultIfEmpty()
                                  select new { s, c, sj, cs, rc, t }).ToListAsync();
                studyings = data.Select(d => d.s).Distinct().ToList();
                studyings.ForEach(s =>
                {
                    s.Class.Studyings = null;
                    s.Class.Subject.Classes = null;
                    s.Class.Teacher.Classes = null;
                    s.Class.ClassSchedules.ForEach(c =>
                    {
                        c.Class = null;
                        c.RollCalls.ForEach(rc =>
                        {
                            rc.ClassSchedule = null;
                        });
                    });
                });
                ar.Data = studyings;
                ar.IsSuccess = true;
            }
            catch (Exception e)
            {
                var a = 0;
            }
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
                fileData = RCSContext.FileDatas.Where(f => f.FileId == fileId).FirstOrDefault();
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
        /// Teacher
        public ApiResult<Teacher> GetTeacherInfo(int teacherId)
        {
            var r = new ApiResult<Teacher>();
            var today = DateTime.Now;
            var data = (from t in RCSContext.Teachers
                        where t.Id == teacherId
                        join f in RCSContext.Faculties on t.FacultyId equals f.Id into f1
                        from f in f1.DefaultIfEmpty()
                        select new { t, f }).FirstOrDefault();
            var d = data.t;
            d.Faculty.Teachers = null;
            r.Data = d;
            r.IsSuccess = true;
            return r;
        }
        public ApiResult<List<Class>> GetAllClassByTeacherId(int teacherId)
        {
            var r = new ApiResult<List<Class>>();
            var today = DateTime.Now;
            var temp = (from c in RCSContext.Classes
                        where c.TeacherId == teacherId
                        join s in RCSContext.Studyings on c.Id equals s.ClassId into s1
                        from s in s1.DefaultIfEmpty()
                        join sj in RCSContext.Subjects on c.SubjectId equals sj.Id into sj1
                        from sj in sj1.DefaultIfEmpty()
                        join cs in RCSContext.ClassSchedules on c.Id equals cs.ClassId into cs1
                        from cs in cs1.DefaultIfEmpty()
                        select new { c, cs, sj, s }).ToList();
            var classes = temp.Select(t => t.c).Distinct().ToList();
            classes.ForEach(c =>
            {
                c.Studyings.ForEach(s =>
                {
                    s.Class = null;
                });
                c.Subject.Classes = null;
                c.ClassSchedules.ForEach(cs =>
                {
                    cs.Class = null;
                });
            });
            r.Data = classes;
            r.IsSuccess = true;
            return r;
        }
        public ApiResult<Class> GetClass(int classId)
        {
            var r = new ApiResult<Class>();
            var temp = (from c in RCSContext.Classes
                        where c.Id == classId
                        join s in RCSContext.Studyings on c.Id equals s.ClassId into s1
                        from s in s1.DefaultIfEmpty()
                        join sj in RCSContext.Subjects on c.SubjectId equals sj.Id into sj1
                        from sj in sj1.DefaultIfEmpty()
                        join cs in RCSContext.ClassSchedules on c.Id equals cs.ClassId into cs1
                        from cs in cs1.DefaultIfEmpty()
                        join rc in RCSContext.RollCalls on cs.Id equals rc.ClassScheduleId into rc1
                        from rc in rc1.DefaultIfEmpty()
                        join src in RCSContext.Students on rc.StudentId equals src.Id into src1
                        from src in src1.DefaultIfEmpty()
                        join ss in RCSContext.Students on s.StudentId equals ss.Id into ss1
                        from ss in ss1.DefaultIfEmpty()
                        select new { c, s, sj, cs, rc, src, ss }
                        ).ToList();
            if (temp == null)
            {
                r.Messages.Add("Không tim thấy lớp học.");
                return r;
            }
            var data = temp.Select(d => d.c).Distinct().FirstOrDefault();
            data.Studyings.ForEach(s =>
            {
                s.Class = null;
                s.Student.Studyings = null;
            });
            data.Subject.Classes = null;
            data.ClassSchedules.ForEach(cs =>
            {
                cs.Class = null;
                cs.RollCalls.ForEach(rc =>
                {
                    rc.ClassSchedule = null;
                    rc.Student.RollCalls = null;
                });
            });
            r.IsSuccess = true;
            r.Data = data;
            return r;
        }
        public ApiResult<ClassSchedule> UpdateClassSchedule(ClassSchedule classSchedule)
        {
            var r = new ApiResult<ClassSchedule>();
            var cs1 = RCSContext.ClassSchedules.Where(i => i.Id == classSchedule.Id).FirstOrDefault();
            if (cs1 == null)
            {
                r.IsSuccess = false;
                r.Messages.Add("Không tìm thấy lớp học.");
            }
            else
            {

            }
            cs1.Status = classSchedule.Status;
            cs1.Datetime = classSchedule.Datetime;
            try
            {
                RCSContext.SaveChanges();
            }
            catch (Exception e) { r.Messages.Add(e.Message); return r; }
            var temp = (from cs in RCSContext.ClassSchedules where cs.Id == classSchedule.Id
                        join c in RCSContext.Classes on cs.ClassId equals c.Id into c1
                        from c in c1.DefaultIfEmpty()
                        join s in RCSContext.Studyings on c.Id equals s.ClassId into s1
                        from s in s1.DefaultIfEmpty()
                        join sj in RCSContext.Subjects on c.SubjectId equals sj.Id into sj1
                        from sj in sj1.DefaultIfEmpty()
                        join rc in RCSContext.RollCalls on cs.Id equals rc.ClassScheduleId into rc1
                        from rc in rc1.DefaultIfEmpty()
                        join src in RCSContext.Students on rc.StudentId equals src.Id into src1
                        from src in src1.DefaultIfEmpty()
                        join ss in RCSContext.Students on s.StudentId equals ss.Id into ss1
                        from ss in ss1.DefaultIfEmpty()
                        select new { c, s, sj, cs, rc, src, ss }
                         ).ToList();
            var cs2 = temp.Select(t => t.cs).Distinct().FirstOrDefault();
            cs2.Class.ClassSchedules = null;
            cs2.Class.Subject.Classes = null;
            cs2.Class.Studyings.ForEach(s =>
            {
                s.Class = null;
                s.Student.Studyings = null;
            });
            cs2.RollCalls.ForEach(rc =>
            {
                rc.ClassSchedule = null;
                rc.Student.RollCalls = null;
            });
            r.Data = cs2;
            r.IsSuccess = true;
            return r;
        }
        public ApiResult<List<RollCall>> RollCall(RollCallModel model)
        {
            var r = new ApiResult<List<RollCall>>();
            var classSchedule = RCSContext.ClassSchedules.Where(cs => cs.Id == model.ClassScheduleId).FirstOrDefault();
            if (classSchedule == null)
            {
                r.Messages.Add("Lớp học không tồn tại.");
                return r;
            }
            if (classSchedule.Status == (int)EClassStatus.Schedule)
            {
                r.Messages.Add("Lớp học chưa mở.");
                return r;
            }
            if (classSchedule.Status == (int)EClassStatus.Closed)
            {
                r.Messages.Add("Lớp học đã kết thúc");
                return r;
            }
            if (model.Type == (int)ERollCallType.Manually)
            {
                return rollCall(model);
            }
            var imageGray = model.Base64Image.base64ToImageGray();
            var image = model.Base64Image.base64ToImage();
            var bitmap = new Bitmap(image);
            imageGray._EqualizeHist();
            var rectangles = DetectService.DetectFace(imageGray);
            if (rectangles.Count() == 0)
            {
                r.Messages.Add("Không phát hiện được sinh viên.");
                return r;
            }
            var rollCallModel = new RollCallModel
            {
                ClassScheduleId = model.ClassScheduleId
            };
            rectangles.ForEach(rec =>
            {
                var face = bitmap.cropAtRect(rec);
                var faceGray = new Image<Gray, byte>(face);
                var studentId = DetectService.RecognizeFace(faceGray);
                if (studentId != -1)
                {
                    rollCallModel.RollCalls.Add(new RollCall
                    {
                        StudentId = studentId,
                        Image = face
                    });
                }
            });
            return rollCall(rollCallModel);
        }
        private ApiResult<List<RollCall>> rollCall(RollCallModel model)
        {
            var r = new ApiResult<List<RollCall>>();
            var classId = RCSContext.ClassSchedules.Where(cs => cs.Id == model.ClassScheduleId).FirstOrDefault().ClassId;
            var studyings = RCSContext.Studyings.Where(st => st.ClassId == classId).ToList();
            studyings = studyings.Where(st => model.RollCalls.Any(rc => rc.StudentId == st.StudentId)).ToList();

            if (studyings.Count() == 0)
            {
                r.Messages.Add("Các học sinh không thuộc lớp học này");
                return r;
            }
            var rollCallsNew = new List<RollCall>();
            var rollCallsOld = new List<RollCall>();
            studyings.ForEach(st =>
            {
                var rc = RCSContext.RollCalls.Where(i => i.StudentId == st.StudentId && model.ClassScheduleId == i.ClassScheduleId).FirstOrDefault();
                if (rc == null)
                {
                    var rollCall = new RollCall();
                    if (model.Type == (int)ERollCallType.Manually)
                    {
                        rollCall = new RollCall
                        {
                            ClassScheduleId = model.ClassScheduleId,
                            StudentId = st.StudentId,
                            CreatedDate = DateTime.Now,
                            Type = (int)ERollCallType.Manually,
                            IsActive = true
                        };
                    }
                    else
                    {
                        var image = model.RollCalls.Find(i => i.StudentId == st.StudentId).Image;
                        var fileData = new FileData
                        {
                            Data = image.bitmapToByteArr(),
                        };
                        var fileAttachment = new FileAttachment
                        {
                            CreateDate = DateTime.Now,
                            Height = image.Height,
                            Width = image.Width,
                            Extension = "jpg",
                            Type = (int)EFileType.ImageJPG,
                            Name = Guid.NewGuid().ToString(),
                            FileData = fileData
                        }; rollCall = new RollCall
                        {
                            ClassScheduleId = model.ClassScheduleId,
                            StudentId = st.StudentId,
                            CreatedDate = DateTime.Now,
                            Type = (int)ERollCallType.Manually,
                            IsActive = true
                        };
                    }
                    rollCallsNew.Add(rollCall);
                }
                else if (!rc.IsActive)
                {
                    rc.IsActive = true;
                    rc.CreatedDate = DateTime.Now;
                    rollCallsOld.Add(rc);
                }
            });
            RCSContext.RollCalls.AddRange(rollCallsNew);
            try
            {
                RCSContext.SaveChanges();
                rollCallsNew.AddRange(rollCallsOld);
                rollCallsNew.ForEach(i =>
                {
                    i.ClassSchedule.RollCalls = null;
                });
                r.IsSuccess = true;
                r.Data = rollCallsNew;
            }
            catch (Exception e) { r.Messages.Add(e.Message); }
            return r;
        }
        public ApiResult<RollCall> ChangeRollCall(int classScheduleId, int studentId)
        {
            var r = new ApiResult<RollCall>();
            var classSchedule = RCSContext.ClassSchedules.Where(cs => cs.Id == classScheduleId).FirstOrDefault();
            if (classSchedule == null)
            {
                r.Messages.Add("Không tìm thấy lớp học.");
                return r;
            }
            if (classSchedule.Status==(int)EClassStatus.Schedule)
            {
                r.Messages.Add("Lớp chưa mở, bạn không thể điểm danh vào lúc này.");
                return r;
            }
            var studying = RCSContext.Studyings.Where(s => s.ClassId == classSchedule.ClassId && s.StudentId == studentId).FirstOrDefault();
            if (studying == null)
            {
                r.Messages.Add("Học sinh không thuộc lớp học này.");
                return r;
            }
            var rollCall = RCSContext.RollCalls.Where(rc => rc.StudentId == studentId && rc.ClassScheduleId == classScheduleId).FirstOrDefault();
            if (rollCall == null)
            {
                rollCall = new RollCall
                {
                    ClassScheduleId = classScheduleId,
                    StudentId = studentId,
                    CreatedDate = DateTime.Now,
                    Type = (int)ERollCallType.Manually,
                    IsActive = true
                };
                RCSContext.RollCalls.Add(rollCall);
            }
            else
            {
                rollCall.IsActive = !rollCall.IsActive;
                rollCall.CreatedDate = DateTime.Now;
            }
            try
            {
                RCSContext.SaveChanges();
                if (rollCall.IsActive == true)
                {
                    r.Messages.Add("Điểm danh thành công.");
                }
                else
                {
                    r.Messages.Add("Hủy điểm danh thành công");
                }

            }
            catch (Exception e)
            {
                r.Messages.Add(e.Message);
                return r;
            }
            var rl = (from rc in RCSContext.RollCalls where (rc.ClassScheduleId == classScheduleId && rc.StudentId == studentId)                      
                      join st in RCSContext.Students on rc.StudentId equals st.Id into st1
                      from st in st1.DefaultIfEmpty() 
                      select new { rc,st }).FirstOrDefault().rc;

            rl.Student.RollCalls = null;
            rl.Student.Studyings = null;
            rl.ClassSchedule = null;
            rl.Student.TrainingImages = null;
            r.IsSuccess = true;
            r.Data = rl;
            return r;
        }
        [HttpGet]
        public HttpResponseMessage GetClassReport(int classId)
        {
            MemoryStream ms = new MemoryStream();
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(ms);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");
            return response;
        }
    }
}