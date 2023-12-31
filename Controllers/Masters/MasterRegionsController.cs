﻿using eShop.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace eShop.Controllers
{
    public class MasterRegionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterRegions
        [Authorize(Roles = "MasterRegionsActive")]
        public ActionResult Index()
        {
            return View("../Masters/MasterRegions/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterRegionsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Masters/MasterRegions/_IndexGrid", db.Set<MasterRegion>().AsQueryable());
            else
                return PartialView("../Masters/MasterRegions/_IndexGrid", db.Set<MasterRegion>().AsQueryable()
                    .Where(x => x.Code.Contains(search) || x.Notes.Contains(search)));
        }

        [Authorize(Roles = "MasterRegionsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterRegions.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterRegions.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterRegions/Details/5
        [Authorize(Roles = "MasterRegionsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterRegion masterRegion = db.MasterRegions.Find(id);
            if (masterRegion == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterRegions/_Details", masterRegion);
        }

        // GET: MasterRegions/Create
        [Authorize(Roles = "MasterRegionsAdd")]
        public ActionResult Create()
        {
            MasterRegion masterRegion = new MasterRegion();
            masterRegion.Active = true;

            return PartialView("../Masters/MasterRegions/_Create", masterRegion);
        }

        // POST: MasterRegions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterRegionsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Notes,Active")] MasterRegion masterRegion)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(masterRegion.Code)) masterRegion.Code = masterRegion.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterRegion.Notes)) masterRegion.Notes = masterRegion.Notes.ToUpper();

                db.MasterRegions.Add(masterRegion);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterRegion, MenuId = masterRegion.Id, MenuCode = masterRegion.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Masters/MasterRegions/_Create", masterRegion);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterRegionsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                MasterRegion obj = db.MasterRegions.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        db.MasterRegions.Remove(obj);
                        db.SaveChanges();
                    }
                }
            }
            return Json(id);
        }

        // GET: MasterRegions/Edit/5
        [Authorize(Roles = "MasterRegionsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterRegion masterRegion = db.MasterRegions.Find(id);
            if (masterRegion == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterRegions/_Edit", masterRegion);
        }

        // POST: MasterRegions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterRegionsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Notes,Active")] MasterRegion masterRegion)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(masterRegion.Code)) masterRegion.Code = masterRegion.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterRegion.Notes)) masterRegion.Notes = masterRegion.Notes.ToUpper();

                db.Entry(masterRegion).State = EntityState.Unchanged;
                db.Entry(masterRegion).Property("Code").IsModified = true;
                db.Entry(masterRegion).Property("Notes").IsModified = true;
                db.Entry(masterRegion).Property("Active").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterRegion, MenuId = masterRegion.Id, MenuCode = masterRegion.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Masters/MasterRegions/_Edit", masterRegion);
        }

        [HttpPost]
        [Authorize(Roles = "MasterRegionsDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDelete(int[] ids)
        {
            if (ids == null || ids.Length <= 0)
                return Json("Pilih salah satu data yang akan dihapus.");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (int id in ids)
                    {
                        MasterRegion obj = db.MasterRegions.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            MasterRegion tmp = obj;
                            db.MasterRegions.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterRegion, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                            db.SaveChanges();
                        }
                    }
                    return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                }
            }
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
