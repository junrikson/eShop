using eShop.Models;
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
    public class MasterUnitsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterUnits
        [Authorize(Roles = "MasterUnitsActive")]
        public ActionResult Index()
        {
            return View("../Inventory/MasterUnits/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterUnitsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Inventory/MasterUnits/_IndexGrid", db.Set<MasterUnit>().AsQueryable());
            else
                return PartialView("../Inventory/MasterUnits/_IndexGrid", db.Set<MasterUnit>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "MasterUnitsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterUnits.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterUnits.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterUnits/Details/
        [Authorize(Roles = "MasterUnitsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterUnit masterUnit = db.MasterUnits.Find(id);
            if (masterUnit == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/MasterUnits/_Details", masterUnit);
        }

        // GET: MasterUnits/Create
        [Authorize(Roles = "MasterUnitsAdd")]
        public ActionResult Create()
        {
            MasterUnit masterUnit = new MasterUnit();
            masterUnit.Active = true;

            return PartialView("../Inventory/MasterUnits/_Create", masterUnit);
        }

        // POST: MasterUnits/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterUnitsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,notes,Ratio,Active,Created,Updated,UserId")] MasterUnit masterUnit)
        {
            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (!string.IsNullOrEmpty(masterUnit.Code)) masterUnit.Code = masterUnit.Code.ToUpper();
                        if (!string.IsNullOrEmpty(masterUnit.Name)) masterUnit.Name = masterUnit.Name.ToUpper();
                        if (!string.IsNullOrEmpty(masterUnit.Notes)) masterUnit.Notes = masterUnit.Notes.ToUpper();
                        masterUnit.Ratio = masterUnit.Ratio;

                        masterUnit.Created = DateTime.Now;
                        masterUnit.Updated = DateTime.Now;
                        masterUnit.UserId = User.Identity.GetUserId<int>();
                        db.MasterUnits.Add(masterUnit);
                        db.SaveChanges();
                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterUnit, MenuId = masterUnit.Id, MenuCode = masterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }

                return PartialView("../Inventory/MasterUnits/_Create", masterUnit);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterUnitsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                MasterUnit obj = db.MasterUnits.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        db.MasterUnits.Remove(obj);
                        db.SaveChanges();
                    }
                }
            }
            return Json(id);
        }

        // GET: MasterUnits/Edit/5
        [Authorize(Roles = "MasterUnitsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterUnit masterUnit = db.MasterUnits.Find(id);
            if (masterUnit == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/MasterUnits/_Edit", masterUnit);
        }

        // POST: MasterUnits/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterUnitsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,notes,Ratio,Active,updated")] MasterUnit masterUnit)
        {
            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    masterUnit.Updated = DateTime.Now;
                    if (ModelState.IsValid)
                    {
                        if (!string.IsNullOrEmpty(masterUnit.Code)) masterUnit.Code = masterUnit.Code.ToUpper();
                        if (!string.IsNullOrEmpty(masterUnit.Name)) masterUnit.Name = masterUnit.Name.ToUpper();
                        if (!string.IsNullOrEmpty(masterUnit.Notes)) masterUnit.Notes = masterUnit.Notes.ToUpper();
                        masterUnit.Ratio = masterUnit.Ratio;

                        db.Entry(masterUnit).State = EntityState.Unchanged;
                        db.Entry(masterUnit).Property("Code").IsModified = true;
                        db.Entry(masterUnit).Property("Name").IsModified = true;
                        db.Entry(masterUnit).Property("Notes").IsModified = true;
                        db.Entry(masterUnit).Property("Ratio").IsModified = true;
                        db.Entry(masterUnit).Property("Active").IsModified = true;
                        db.Entry(masterUnit).Property("Updated").IsModified = true;
                        masterUnit.UserId = User.Identity.GetUserId<int>();
                        db.SaveChanges();
                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterUnit, MenuId = masterUnit.Id, MenuCode = masterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
                return PartialView("../Inventory/MasterUnits/_Edit", masterUnit);

            }
        }



        [HttpPost]
        [Authorize(Roles = "MasterUnitsDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDelete(int[] ids)
        {
            if (ids == null || ids.Length <= 0)
                return Json("Pilih salah satu data yang akan dihapus.");
            else
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        int failed = 0;
                        foreach (int id in ids)
                        {
                            MasterUnit obj = db.MasterUnits.Find(id);
                            if (obj == null)
                                failed++;
                            else
                            {
                                MasterUnit tmp = obj;
                                db.MasterUnits.Remove(obj);
                                db.SaveChanges();
                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterUnit, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                                db.SaveChanges();

                                dbTran.Commit();

                            }
                        }
                        return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }


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
