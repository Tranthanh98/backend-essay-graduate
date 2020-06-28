using Microsoft.Ajax.Utilities;
using RollCallSystem.Models;
using RollCallSystem.Models.ModelRequest;
using RollCallSystem.Models.ModelResponse;
using RollCallSystem.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Services
{
    public class ServiceContext
    {
        private ServiceContext()
        {
            this.db = new EntitiesDB();
        }
        private static ServiceContext instance = null;
        public static ServiceContext Instance()
        {
            if (instance == null)
            {
                instance = new ServiceContext();
            }
            return instance;
        }
        EntitiesDB db { get; set; }
        public List<AckGetStudentByClass> QueryGetStudent(ModelGetStudent modelGetStudent)
        {
            var data = new List<AckGetStudentByClass>();
            var query = (from mh in db.MonHocs
                         join lgd in db.ScheduleTeaches on mh.ma_mon equals lgd.ma_mon
                         join smh in db.StudentMHs on mh.ma_mon equals smh.ma_mon
                         join std in db.StudentInformations on smh.mssv equals std.mssv
                         join rc in db.RollCallStudents on new { X1 = (int?)smh.mssv, X2 = lgd.id } equals new { X1 = rc.mssv, X2 = rc.lich_giang_id } into tempRollCall
                         from mapRollCall in tempRollCall.DefaultIfEmpty()
                         join f in db.FaceTrainedStudents on std.mssv equals f.mssv into tmpFace
                         from mapping in tmpFace.DefaultIfEmpty()
                         where mh.ma_mon == modelGetStudent.MaMon && lgd.date_teach == modelGetStudent.date && lgd.teacher_id == modelGetStudent.teacherId
                         group new { mapping, mapRollCall } by new
                         {
                             smh.is_suspended,
                             mh.ma_mon,
                             mh.ten_mon,
                             std.mssv,
                             std.name_student,
                             lgd.id,
                             lgd.date_teach,
                             lgd.time_teach,
                             rollCallId = mapRollCall.id
                         } into g
                         select new
                         {
                             g.Key.ma_mon,
                             g.Key.ten_mon,
                             g.Key.mssv,
                             g.Key.name_student,
                             MaLichGiang = g.Key.id,
                             NgayDay = g.Key.date_teach,
                             GioDay = g.Key.time_teach,
                             totalFaceTrained = g.Count(t => (int?)t.mapping.mssv != null),
                             rollCalled = (int?)g.Key.rollCallId,
                             g.Key.is_suspended
                         }).ToList();
            foreach (var item in query)
            {
                AckGetStudentByClass st = new AckGetStudentByClass();
                st.MaMon = item.ma_mon;
                st.TenMon = st.TenMon;
                st.Mssv = item.mssv;
                st.NameStudent = item.name_student;
                st.LichGiangId = item.MaLichGiang;
                st.NgayDay = item.NgayDay;
                st.GioDay = item.GioDay;
                st.totalFaceTrained = item.totalFaceTrained;
                st.isRollCalled = item.rollCalled != null ? 1 : 0;
                st.isSuppended = item.is_suspended;
                data.Add(st);
            }
            return data;
        }
        public List<AckGetClass> GetClass(GetClassByDay getClassByDay)
        {
            var data = new List<AckGetClass>();
            var query = (from lgd in db.ScheduleTeaches
                         join mh in db.MonHocs on lgd.ma_mon equals mh.ma_mon
                         join stmh in db.StudentMHs on mh.ma_mon equals stmh.ma_mon
                         where
                             lgd.date_teach == getClassByDay.date
                             &&
                             lgd.teacher_id == getClassByDay.teacherId
                         group stmh by new
                         {
                             lgd.id,
                             lgd.date_teach,
                             lgd.time_teach,
                             lgd.ma_mon,
                             lgd.teacher_id,
                             lgd.phong_hoc,
                             lgd.status_id,
                             mh.ten_mon,
                             lgd.buoi
                         } into g
                         select new
                         {
                             g.Key.id,
                             g.Key.date_teach,
                             g.Key.time_teach,
                             g.Key.ma_mon,
                             g.Key.teacher_id,
                             g.Key.phong_hoc,
                             g.Key.ten_mon,
                             g.Key.status_id,
                             g.Key.buoi,
                             totalSV = g.Count()
                         }).ToList();
            foreach (var item in query)
            {
                AckGetClass a = new AckGetClass();
                a.id = item.id;
                a.date = item.date_teach;
                a.time = item.time_teach;
                a.ma_mon = item.ma_mon;
                a.teacher_id = item.teacher_id;
                a.phong_hoc = item.phong_hoc;
                a.ten_mon = item.ten_mon;
                a.totalSV = item.totalSV;
                a.status = item.status_id;
                a.buoi = item.buoi;
                data.Add(a);
            }
            return data;
        }
        public List<StudentInforRollCall> GetInfStudentRollCall(int maMon, int teacherId)
        {
            var data = new List<StudentInforRollCall>();
            var query = (from mh in db.MonHocs
                         join lgd in db.ScheduleTeaches on mh.ma_mon equals lgd.ma_mon
                         join stmh in db.StudentMHs on mh.ma_mon equals stmh.ma_mon
                         join std in db.StudentInformations on stmh.mssv equals std.mssv
                         join rc in db.RollCallStudents on new { X1 = (int?)stmh.mssv, X2 = lgd.id } equals new { X1 = rc.mssv, X2 = rc.lich_giang_id } into temp
                         from mapping in temp.DefaultIfEmpty()
                         where mh.ma_mon == maMon && lgd.teacher_id == teacherId
                         group mapping by new
                         {
                             mh.ma_mon,
                             mh.ten_mon,
                             std.mssv,
                             std.name_student,
                             stmh.is_suspended
                         } into g
                         select new
                         {
                             g.Key.mssv,
                             g.Key.name_student,
                             g.Key.ma_mon,
                             g.Key.ten_mon,
                             g.Key.is_suspended,
                             countRollCall = g.Count(t => t.mssv != null)
                         }).ToList();
            foreach (var item in query)
            {
                StudentInforRollCall st = new StudentInforRollCall();
                st.mssv = item.mssv;
                st.name_student = item.name_student;
                st.maMon = item.ma_mon;
                st.tenMon = item.ten_mon;
                st.countRollCall = item.countRollCall;
                st.isSuspended = item.is_suspended;
                data.Add(st);
               
            }
            return data;
        }
        public List<AckAllClass> GetAllClassTeacher (int teacherId)
        {
            var data = new List<AckAllClass>();
            var query = (from mh in db.MonHocs
                         join tmp in (
                                    from lgd in db.ScheduleTeaches
                                    where lgd.teacher_id == teacherId && lgd.status_id == 3
                                    group lgd by new
                                    {
                                        lgd.ma_mon,
                                        lgd.teacher_id
                                    } into t
                                    select new
                                    {
                                        t.Key.ma_mon,
                                        t.Key.teacher_id,
                                        buoiDaDay = t.Count()
                                    }
                         ) on mh.ma_mon equals tmp.ma_mon
                         join stmh in db.StudentMHs on tmp.ma_mon equals stmh.ma_mon
                         where tmp.teacher_id == teacherId
                         group mh by new
                         {
                             mh.ma_mon,
                             mh.ten_mon,
                             tmp.buoiDaDay
                         } into temp
                         select new
                         {
                             temp.Key.ma_mon,
                             temp.Key.ten_mon,
                             temp.Key.buoiDaDay,
                             totalStudent = temp.Count()
                         }).ToList();
            foreach (var item in query)
            {
                AckAllClass a = new AckAllClass();
                a.maMon = item.ma_mon;
                a.tenMon = item.ten_mon;
                a.soBuoiDaDay = item.buoiDaDay;
                a.totalStudent = item.totalStudent;
                data.Add(a);
            }
            return data;
        }
        public List<AckGetAllStudent> GetAllStudentOfClass(ModelGetStudent modelGetStudent)
        {
            var data = new List<AckGetAllStudent>();

            var query = (from mh in db.MonHocs
                         join lgd in (from t in db.ScheduleTeaches
                                      where t.ma_mon == modelGetStudent.MaMon && t.teacher_id == modelGetStudent.teacherId
                                      group t by new
                                      {
                                          t.ma_mon,
                                          t.teacher_id
                                      } into g
                                      select new { 
                                      g.Key.ma_mon,
                                      g.Key.teacher_id}) on mh.ma_mon equals lgd.ma_mon
                         join stmh in db.StudentMHs on mh.ma_mon equals stmh.ma_mon
                         join std in db.StudentInformations on stmh.mssv equals std.mssv
                         join f in db.FaceTrainedStudents on std.mssv equals f.mssv into temp
                         from mapping in temp.DefaultIfEmpty()
                         join k in db.Khoas on std.ma_khoa equals k.ma_khoa
                         where mh.ma_mon == modelGetStudent.MaMon && lgd.teacher_id == modelGetStudent.teacherId
                         group mapping by new
                         {
                             mh.ma_mon,
                             mh.ten_mon,
                             std.mssv,
                             std.name_student,
                             k.ten_khoa,
                             k.ma_khoa,
                             stmh.is_suspended,
                         } into g
                         select new
                         {
                             g.Key.ma_mon,
                             g.Key.ten_mon,
                             g.Key.mssv,
                             g.Key.ten_khoa,
                             g.Key.name_student,
                             g.Key.ma_khoa,
                             totalFaceTrained = g.Count(t => t.mssv !=null),
                             g.Key.is_suspended
                         }
                         ).ToList();
            foreach(var item in query)
            {
                AckGetAllStudent a = new AckGetAllStudent();
                a.maMon = item.ma_mon;
                a.tenMon = item.ten_mon;
                a.mssv = item.mssv;
                a.tenKhoa = item.ten_khoa;
                a.name_student = item.name_student;
                a.ma_khoa = item.ma_khoa;
                a.totalFaceTrained = item.totalFaceTrained;
                a.isSuspended = item.is_suspended;
                data.Add(a);
            }
            return data;
        }
        public AckGetClass GetNowClass(int teacherId)
        {
            var item = (from lgd in db.ScheduleTeaches
                         join mh in db.MonHocs on lgd.ma_mon equals mh.ma_mon
                         join stmh in db.StudentMHs on mh.ma_mon equals stmh.ma_mon
                         where
                             lgd.status_id == 2
                             &&
                             lgd.teacher_id == teacherId
                         group stmh by new
                         {
                             lgd.id,
                             lgd.date_teach,
                             lgd.time_teach,
                             lgd.ma_mon,
                             lgd.teacher_id,
                             lgd.phong_hoc,
                             lgd.status_id,
                             mh.ten_mon,
                             lgd.buoi
                         } into g
                         select new
                         {
                             g.Key.id,
                             g.Key.date_teach,
                             g.Key.time_teach,
                             g.Key.ma_mon,
                             g.Key.teacher_id,
                             g.Key.phong_hoc,
                             g.Key.ten_mon,
                             g.Key.status_id,
                             g.Key.buoi,
                             totalSV = g.Count()
                         }).FirstOrDefault();
            if(item == null)
            {
                return null;
            }
                AckGetClass a = new AckGetClass();
                a.id = item.id;
                a.date = item.date_teach;
                a.time = item.time_teach;
                a.ma_mon = item.ma_mon;
                a.teacher_id = item.teacher_id;
                a.phong_hoc = item.phong_hoc;
                a.ten_mon = item.ten_mon;
                a.totalSV = item.totalSV;
                a.status = item.status_id;
                a.buoi = item.buoi;
            
            return a;
        }
    }
}