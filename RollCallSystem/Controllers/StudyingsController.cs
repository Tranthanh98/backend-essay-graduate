using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RollCallSystem.Models;

namespace RollCallSystem.Controllers
{
    public class StudyingsController : Controller
    {
        private EntitiesDB db = new EntitiesDB();

        // GET: Studyings
        public ActionResult Index()
        {
            var studyings = db.Studyings.Include(s => s.Class).Include(s => s.Student);
            return View(studyings.ToList());
        }

        // GET: Studyings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Studying studying = db.Studyings.Find(id);
            if (studying == null)
            {
                return HttpNotFound();
            }
            return View(studying);
        }

        // GET: Studyings/Create
        public ActionResult Create()
        {
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Name");
            ViewBag.StudentId = new SelectList(db.Students, "Id", "Name");
            return View();
        }

        // POST: Studyings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,StudentId,ClassId")] Studying studying)
        {
            if (ModelState.IsValid)
            {
                db.Studyings.Add(studying);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Name", studying.ClassId);
            ViewBag.StudentId = new SelectList(db.Students, "Id", "Name", studying.StudentId);
            return View(studying);
        }

        // GET: Studyings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Studying studying = db.Studyings.Find(id);
            if (studying == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Name", studying.ClassId);
            ViewBag.StudentId = new SelectList(db.Students, "Id", "Name", studying.StudentId);
            return View(studying);
        }

        // POST: Studyings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,StudentId,ClassId")] Studying studying)
        {
            if (ModelState.IsValid)
            {
                db.Entry(studying).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClassId = new SelectList(db.Classes, "Id", "Name", studying.ClassId);
            ViewBag.StudentId = new SelectList(db.Students, "Id", "Name", studying.StudentId);
            return View(studying);
        }

        // GET: Studyings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Studying studying = db.Studyings.Find(id);
            if (studying == null)
            {
                return HttpNotFound();
            }
            return View(studying);
        }

        // POST: Studyings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Studying studying = db.Studyings.Find(id);
            db.Studyings.Remove(studying);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
