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
                             rollCalled = (int?)g.Key.rollCallId
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
                             std.name_student
                         } into g
                         select new
                         {
                             g.Key.mssv,
                             g.Key.name_student,
                             g.Key.ma_mon,
                             g.Key.ten_mon,
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
                data.Add(st);
               
            }
            return data;
        }
    }
}