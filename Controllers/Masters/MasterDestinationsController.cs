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
    public class MasterDestinationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterDestinations
        [Authorize(Roles = "MasterDestinationsActive")]
        public ActionResult Index()
        {
            return View("../Masters/MasterDestinations/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterDestinationsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Masters/MasterDestinations/_IndexGrid", db.Set<MasterDestination>().AsQueryable());
            else
                return PartialView("../Masters/MasterDestinations/_IndexGrid", db.Set<MasterDestination>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        // GET: MasterDestinations/Details/
        [Authorize(Roles = "MasterDestinationsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterDestination masterDestination = db.MasterDestinations.Find(id);
            if (masterDestination == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterDestinations/_Details", masterDestination);
        }

        // GET: MasterDestinations/Create
        [Authorize(Roles = "MasterDestinationsAdd")]
        public ActionResult Create()
        {
            MasterDestination masterDestination = new MasterDestination();
            masterDestination.Active = true;

            return PartialView("../Masters/MasterDestinations/_Create", masterDestination);
        }

        // POST: MasterDestinations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterDestinationsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Default,Notes,Active,Created,Updated,UserId")] MasterDestination masterDestination)
        {
            masterDestination.Created = DateTime.Now;
            masterDestination.Updated = DateTime.Now;
            masterDestination.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(masterDestination.Code)) masterDestination.Code = masterDestination.Code.ToUpper();
            if (!string.IsNullOrEmpty(masterDestination.Name)) masterDestination.Name = masterDestination.Name.ToUpper();
            if (!string.IsNullOrEmpty(masterDestination.Notes)) masterDestination.Notes = masterDestination.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.MasterDestinations.Add(masterDestination);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterDestination, MenuId = masterDestination.Id, MenuCode = masterDestination.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            return PartialView("../Masters/MasterDestinations/_Create", masterDestination);
        }

        // GET: MasterDestinations/Edit/5
        [Authorize(Roles = "MasterDestinationsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterDestination masterDestination = db.MasterDestinations.Find(id);
            if (masterDestination == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterDestinations/_Edit", masterDestination);
        }

        // POST: MasterDestinations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterDestinationsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Default,Notes,Active,Updated,UserId")] MasterDestination masterDestination)
        {
            masterDestination.UserId = User.Identity.GetUserId<int>();
            masterDestination.Updated = DateTime.Now;

            if (!string.IsNullOrEmpty(masterDestination.Code)) masterDestination.Code = masterDestination.Code.ToUpper();
            if (!string.IsNullOrEmpty(masterDestination.Name)) masterDestination.Name = masterDestination.Name.ToUpper();
            if (!string.IsNullOrEmpty(masterDestination.Notes)) masterDestination.Notes = masterDestination.Notes.ToUpper();

            db.Entry(masterDestination).State = EntityState.Unchanged;
            db.Entry(masterDestination).Property("Code").IsModified = true;
            db.Entry(masterDestination).Property("Name").IsModified = true;
            db.Entry(masterDestination).Property("Notes").IsModified = true;
            db.Entry(masterDestination).Property("Default").IsModified = true;
            db.Entry(masterDestination).Property("Active").IsModified = true;
            db.Entry(masterDestination).Property("Updated").IsModified = true;
            db.Entry(masterDestination).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterDestination, MenuId = masterDestination.Id, MenuCode = masterDestination.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            return PartialView("../Masters/MasterDestinations/_Edit", masterDestination);
        }


        [HttpPost]
        [Authorize(Roles = "MasterDestinationsDelete")]
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
                            MasterDestination obj = db.MasterDestinations.Find(id);
                            if (obj == null)
                                failed++;
                            else
                            {
                                MasterDestination tmp = obj;

                                db.MasterDestinations.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterDestination, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
