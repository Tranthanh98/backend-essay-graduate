using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
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
                    DetectService.AddTrainModels(trainModels);
                    isFirstTrained = true;
                }
            }
        }
        public ApiResult<User> Login(LoginModel login)
        {
            var apiResult = new ApiResult<User>();
            if (login == null)
            {
                apiResult.Messages.Add("Lỗi đăng nhập.");
                return apiResult;
            }
            var user = RCSContext.Users.Where(u => u.Username == login.Username).FirstOrDefault();
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
                userInfo.Student = RCSContext.Students.Where(s => s.UserId == userInfo.Id).FirstOrDefault();
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
                userInfo.Teacher = RCSContext.Teachers.Where(t => t.UserId == userInfo.Id).FirstOrDefault();
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
            apiResult.Messages.Add("Đăng nhập thành công.");
            return apiResult;
        }
        public ApiResult<List<User>> CreateAccount(List<UserCreateModel> newUsers)
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
                RCSContext.SaveChanges();
                apiResult.IsSuccess = true;
            }
            catch (Exception e)
            {
                apiResult.IsSuccess = false;
            }
            return apiResult;
        }
        public ApiResult<User> GetCurrentUser()
        {
            var apiResult = new ApiResult<User>();
            var id = getCurrentUserId();
            var user = RCSContext.Users.Where(u => u.Id == id).FirstOrDefault();
            apiResult.Data = new User()
            {
                Id = user.Id,
                Role = user.Role,
                IsActive = user.IsActive
            };
            apiResult.IsSuccess = true;
            return apiResult;
        }
        public ApiResult<TrainFaceModel> TrainStudentFace(TrainFaceModel model)
        {
            var ar = new ApiResult<TrainFaceModel>();
            var student = RCSContext.Students.Where(s => s.Id == model.StudentId).FirstOrDefault();
            if (student == null)
            {
                ar.IsSuccess = false;
                ar.Messages.Add("Sinh viên không tồn tại.");
            }
            else
            {
                var image = model.Base64Image.base64ToImage();
                var bitmap = new Bitmap(image);
                var imageGray = model.Base64Image.base64ToImageGray();
                imageGray._EqualizeHist();
                var rectangles = DetectService.DetectFaceTrain(imageGray);
                if (rectangles.Count() <= 0)
                {
                    ar.IsSuccess = false;
                    ar.Messages.Add("Không nhận diện được khuôn mặt, vui lòng chọn nơi có ánh sáng đầy đủ và điều chỉnh hướng nhìn.");
                    var data = new TrainFaceModel()
                    {
                        Base64Image = Convert.ToBase64String(bitmap.bitmapToByteArr())
                    };
                    ar.Data = data;
                }
                else if (rectangles.Count() == 1)
                {
                    var trainImage = saveImageTraining(model.StudentId, model.Base64Image, rectangles[0]);
                    var trainModels = new List<TrainModel>();
                    trainModels.Add(new TrainModel
                    {
                        StudentId = trainImage.StudentId,
                        ImageId = trainImage.FileId,
                        ImageGray = trainImage.FileAttachment.FileData.Data.byteArrToImageGray(),
                        
                    });
                    DetectService.AddTrainModels(trainModels);
                    trainImage.FileAttachment.TrainingImages = null;
                    trainImage.FileAttachment.FileData.FileAttachment = null;
                    trainImage.Student = null;
                    var data = new TrainFaceModel()
                    {
                        Base64Image = Convert.ToBase64String(drawFaceAndNoteOnBitmap(bitmap, rectangles[0], "", Color.Green).bitmapToByteArr()),
                        StudentId = trainImage.StudentId,
                        FileId = trainImage.FileId
                    };
                    ar.Data = data;
                    ar.IsSuccess = true;
                    ar.Messages.Add("Training thành công.");
                }
                else
                {
                    rectangles.ForEach(r =>
                    {
                        bitmap = drawFaceAndNoteOnBitmap(bitmap, r, "", Color.Red);
                    });
                    var data = new TrainFaceModel()
                    {
                        Base64Image = Convert.ToBase64String(bitmap.bitmapToByteArr())
                    };
                    ar.Data = data;
                    ar.IsSuccess = false;
                    ar.Messages.Add("Ảnh training chỉ được có một khuôn mặt.");
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
            var trainingImage = createTrainImage(studentId, bitmap);
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
        public TrainingImage createTrainImage(int studentId, Bitmap bitmap)
        {
            var fileData = new FileData()
            {
                Data = bitmap.bitmapToByteArr()
            };
            var file = new FileAttachment()
            {
                CreateDate = DateTime.Now,
                Height = bitmap.Height,
                Width = bitmap.Width,
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
            return trainingImage;
        }
        private Bitmap drawFaceAndNoteOnBitmap(Bitmap bitmap, Rectangle rectangle, string text, Color color)
        {
            var i = new Image<Bgr, byte>(bitmap);
            i.Draw(rectangle, new Bgr(color), 2);
            CvInvoke.PutText(i, text, new Point(rectangle.X, rectangle.Y - 10), Emgu.CV.CvEnum.FontFace.HersheyTriplex, 0.7, new Bgr(color).MCvScalar, 1);
            var b = i.Bitmap;
            return b;
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
                        ImageId = d.fileData.FileId,
                        ImageGray = imageGray
                    });
                }
            });
            return trainModels;
        }
        public ApiResult<List<Student>> GetAllStudent()
        {
            var ar = new ApiResult<List<Student>>();
            var a = getAllTrainModel();
            ar.IsSuccess = true;
            return ar;
        }
        public ApiResult<List<Studying>> GetAllSubject(int studentId)
        {
            var ar = new ApiResult<List<Studying>>();
            try
            {
                var studyings = new List<Studying>();
                var data = (from s in RCSContext.Studyings
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
                            select new { s, c, sj, cs, rc, t }).ToList();
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
        public ApiResult<Student> GetStudentInfo(int studentId)
        {
            var ar = new ApiResult<Student>();
            var student1 = (from s in RCSContext.Students
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
                       ).FirstOrDefault();
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
        public ApiResult<List<TrainingImage>> GetStudentTrainImages(int studentId)
        {
            var ar = new ApiResult<List<TrainingImage>>();
            var trainImages = new List<TrainingImage>();
            trainImages = RCSContext.TrainingImages.Where(t => t.StudentId == studentId).ToList();
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
                return ar;
            }
            ar.Data = fileId;
            RCSContext.TrainingImages.Remove(trainingImage);
            RCSContext.SaveChanges();
            DetectService.RemoveTrainsByImageId(trainingImage.FileId);
            ar.IsSuccess = true;
            ar.Messages.Add("Xóa file thành công.");
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
            var temp = (from cs in RCSContext.ClassSchedules
                        where cs.Id == classSchedule.Id
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
        public ApiResult<RollCallReponseModel> RollCall(RollCallModel model)
        {
            var r = new ApiResult<RollCallReponseModel>();
            r.Data = new RollCallReponseModel();
            r.Data.Base64Image = model.Base64Image;
            var rollCallData = new ApiResult<List<RollCall>>();
            var classSchedule = RCSContext.ClassSchedules.Where(cs => cs.Id == model.ClassScheduleId).FirstOrDefault();
            //check thông tin lớp học
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
            var imageGray = model.Base64Image.base64ToImageGray();
            var image = model.Base64Image.base64ToImage();
            var bitmap = new Bitmap(image);
            var bitmapResponse = new Bitmap(bitmap);
            imageGray._EqualizeHist();
            var rectangles = DetectService.DetectFaceRollCall(imageGray);
            if (rectangles.Count() == 0)
            {
                r.Messages.Add("Không có sinh viên nào trong ảnh.");
                return r;
            }
            var classId = RCSContext.ClassSchedules.Where(cs => cs.Id == model.ClassScheduleId).FirstOrDefault().ClassId;
            var studyings = RCSContext.Studyings.Where(st => st.ClassId == classId).ToList();
            var newRollCalls = new List<RollCall>();
            var oldRollCalls = new List<RollCall>();
            //Detect khuôn mặt
            rectangles.ForEach(rec =>
            {
                var face = bitmap.cropAtRect(rec);
                var faceGray = new Image<Gray, byte>(face);
                var studentId = DetectService.RecognizeFace(faceGray);
                if (studentId == -1)
                {
                    var faceGrayFlip = faceGray.Flip(FlipType.Horizontal);
                    studentId = DetectService.RecognizeFace(faceGrayFlip);
                }
                if (studentId != -1)
                {
                    if (studyings.Any(st => st.StudentId == studentId))
                    {
                        bitmapResponse = drawFaceAndNoteOnBitmap(bitmapResponse, rec, studentId.ToString(), Color.Green);
                        var rc = RCSContext.RollCalls.Where(i => i.StudentId == studentId && i.ClassScheduleId == model.ClassScheduleId).FirstOrDefault();
                        var fileData = new FileData
                        {
                            Data = face.bitmapToByteArr(),
                        };
                        var fileAttachment = new FileAttachment
                        {
                            CreateDate = DateTime.Now,
                            Height = face.Height,
                            Width = face.Width,
                            Extension = "jpg",
                            Type = (int)EFileType.ImageJPG,
                            Name = Guid.NewGuid().ToString(),
                            FileData = fileData
                        };
                        if (rc == null)
                        {
                            var rollCall = new RollCall();
                            rollCall = new RollCall
                            {
                                ClassScheduleId = model.ClassScheduleId,
                                StudentId = studentId,
                                CreatedDate = DateTime.Now,
                                Type = (int)ERollCallType.Auto,
                                IsActive = true
                            };
                            newRollCalls.Add(rollCall);
                        }
                        else
                        {
                            rc.IsActive = true;
                            rc.CreatedDate = DateTime.Now;
                            rc.FileAttachment = fileAttachment;
                            oldRollCalls.Add(rc);
                        }
                    }
                }
                else
                {
                    bitmapResponse = drawFaceAndNoteOnBitmap(bitmapResponse, rec, "unknown", Color.Red);
                }
            });
            if (newRollCalls.Count == 0 && oldRollCalls.Count == 0)
            {
                r.Messages.Add("Các sinh viên không thuộc lớp này.");
                r.Data.Base64Image = Convert.ToBase64String(bitmapResponse.bitmapToByteArr());
                return r;
            }
            RCSContext.RollCalls.AddRange(newRollCalls);
            try
            {
                RCSContext.SaveChanges();
            }
            catch (Exception e) { r.Messages.Add(e.Message); return r; }
            newRollCalls.AddRange(oldRollCalls);
            newRollCalls.ForEach(i =>
            {
                i.ClassSchedule.RollCalls = null;
                i.FileAttachment = null;
                i.Student = null;
                i.Image = null;
            });
            r.Messages.Add("Điểm danh thành công.");
            r.IsSuccess = true;
            r.Data.Base64Image = Convert.ToBase64String(bitmapResponse.bitmapToByteArr());
            r.Data.RollCalls = newRollCalls;
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
            if (classSchedule.Status == (int)EClassStatus.Schedule)
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
                rollCall.Type = (int)ERollCallType.Manually;
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
            var rl = (from rc in RCSContext.RollCalls
                      where (rc.ClassScheduleId == classScheduleId && rc.StudentId == studentId)
                      join st in RCSContext.Students on rc.StudentId equals st.Id into st1
                      from st in st1.DefaultIfEmpty()
                      select new { rc, st }).FirstOrDefault().rc;

            rl.Student.RollCalls = null;
            rl.Student.Studyings = null;
            rl.ClassSchedule = null;
            rl.Student.TrainingImages = null;
            r.IsSuccess = true;
            r.Data = rl;
            return r;
        }
        public HttpResponseMessage GetClassReport(int classId)
        {
            MemoryStream ms = new MemoryStream();
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(ms);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");
            return response;
        }
        public ApiResult<ClassSchedule> OpenClass(int classScheduleId)
        {
            ApiResult<ClassSchedule> r = new ApiResult<ClassSchedule>();
            ClassSchedule cs = RCSContext.ClassSchedules.Where(i => i.Id == classScheduleId).FirstOrDefault();
            if (cs == null)
            {
                r.Messages.Add("Lớp học không tồn tại.");
                return r;
            }
            if (cs.Status == (int)EClassStatus.Opening)
            {
                r.Messages.Add("Lớp học đang được mở.");
                return r;
            }
            if (cs.Status == (int)EClassStatus.Closed)
            {
                r.Messages.Add("Lớp học đã kết thúc.");
                return r;
            }
            cs.Status = (int)EClassStatus.Opening;
            cs.StartDatetime = DateTime.Now;
            RCSContext.SaveChanges();
            r.IsSuccess = true;
            r.Data = cs;
            r.Messages.Add("Mở lớp học thành công.");
            return r;
        }
        public ApiResult<ClassSchedule> CloseClass(int classScheduleId)
        {
            ApiResult<ClassSchedule> r = new ApiResult<ClassSchedule>();
            ClassSchedule cs = RCSContext.ClassSchedules.Where(i => i.Id == classScheduleId).FirstOrDefault();
            if (cs == null)
            {
                r.Messages.Add("Lớp học không tồn tại.");
                return r;
            }
            if (cs.Status == (int)EClassStatus.Schedule)
            {
                r.Messages.Add("Lớp học chưa mở.");
                return r;
            }
            if (cs.Status == (int)EClassStatus.Closed)
            {
                r.Messages.Add("Lớp học đã kết thúc.");
                return r;
            }
            cs.Status = (int)EClassStatus.Closed;
            cs.EndDatetime = DateTime.Now;
            RCSContext.SaveChanges();
            r.IsSuccess = true;
            r.Data = cs;
            r.Messages.Add("Kết thúc lớp học thành công.");
            return r;
        }
        public ApiResult<ClassSchedule> GetClassScheduleFullData(int classScheduleId)
        {
            ApiResult<ClassSchedule> r = new ApiResult<ClassSchedule>();
            var temp = (from cs in RCSContext.ClassSchedules
                        where cs.Id == classScheduleId
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
            ClassSchedule classSchedule = temp.Select(t => t.cs).Distinct().FirstOrDefault();
            if (classSchedule == null)
            {
                r.IsSuccess = false;
                r.Messages.Add("Không tìm thấy lớp học.");
                return r;
            }
            classSchedule.Class.ClassSchedules = null;
            classSchedule.Class.Subject.Classes = null;
            classSchedule.Class.Studyings.ForEach(s =>
            {
                s.Class = null;
                s.Student.Studyings = null;
                s.Student.TrainingImages = null;
            });
            classSchedule.RollCalls.ForEach(rc =>
            {
                rc.ClassSchedule = null;
                rc.Student.RollCalls = null;
            });
            r.Data = classSchedule;
            r.IsSuccess = true;
            return r;
        }
    }
}