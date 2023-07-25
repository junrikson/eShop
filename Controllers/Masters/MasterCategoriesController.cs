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
    public class MasterCategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterCategories
        [Authorize(Roles = "MasterCategoriesActive")]
        public ActionResult Index()
        {
            return View("../Masters/MasterCategories/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterCategoriesActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Masters/MasterCategories/_IndexGrid", db.Set<MasterCategorie>().AsQueryable());
            else
                return PartialView("../Masters/MasterCategories/_IndexGrid", db.Set<MasterCategorie>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "MasterCategoriesActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterCategories.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterCategories.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterUnits/Details/
        [Authorize(Roles = "MasterCategoriesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterCategorie masterCategorie = db.MasterCategories.Find(id);
            if (masterCategorie == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterCategories/_Details", masterCategorie);
        }

        // GET: MasterCategories/Create
        [Authorize(Roles = "MasterCategoriesAdd")]
        public ActionResult Create()
        {
            MasterCategorie masterCategorie = new MasterCategorie();
            masterCategorie.Active = true;

            return PartialView("../Masters/MasterCategories/_Create", masterCategorie);
        }

        // POST: MasterUnits/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCategoriesAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,notes,Active,Created,Updated,UserId")] MasterCategorie masterCategorie)
        {
            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (!string.IsNullOrEmpty(masterCategorie.Code)) masterCategorie.Code = masterCategorie.Code.ToUpper();
                        if (!string.IsNullOrEmpty(masterCategorie.Name)) masterCategorie.Name = masterCategorie.Name.ToUpper();
                        if (!string.IsNullOrEmpty(masterCategorie.Notes)) masterCategorie.Notes = masterCategorie.Notes.ToUpper();
                        
                        masterCategorie.Created = DateTime.Now;
                        masterCategorie.Updated = DateTime.Now;
                        masterCategorie.UserId = User.Identity.GetUserId<int>();
                        db.MasterCategories.Add(masterCategorie);
                        db.SaveChanges();
                      //  db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCategorie, MenuId = masterCategorie.Id, MenuCode = masterCategorie.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return PartialView("../Masters/MasterCategories/_Create", masterCategorie);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterCategoriesAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                MasterCategorie obj = db.MasterCategories.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        db.MasterCategories.Remove(obj);
                        db.SaveChanges();
                    }
                }
            }
            return Json(id);
        }

        // GET: MasterCategories/Edit/5
        [Authorize(Roles = "MasterCategoriesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterCategorie masterCategorie = db.MasterCategories.Find(id);
            if (masterCategorie == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterCategories/_Edit", masterCategorie);
        }

        // POST: MasterCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCategoriesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,notes,Active,updated")] MasterCategorie masterCategorie)
        {
            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    masterCategorie.Updated = DateTime.Now;
                    if (ModelState.IsValid)
                    {
                        if (!string.IsNullOrEmpty(masterCategorie.Code)) masterCategorie.Code = masterCategorie.Code.ToUpper();
                        if (!string.IsNullOrEmpty(masterCategorie.Name)) masterCategorie.Name = masterCategorie.Name.ToUpper();
                        if (!string.IsNullOrEmpty(masterCategorie.Notes)) masterCategorie.Notes = masterCategorie.Notes.ToUpper();

                        db.Entry(masterCategorie).State = EntityState.Unchanged;
                        db.Entry(masterCategorie).Property("Code").IsModified = true;
                        db.Entry(masterCategorie).Property("Name").IsModified = true;
                        db.Entry(masterCategorie).Property("Notes").IsModified = true;
                        db.Entry(masterCategorie).Property("Active").IsModified = true;
                        db.Entry(masterCategorie).Property("Updated").IsModified = true;
                        masterCategorie.UserId = User.Identity.GetUserId<int>();
                        db.SaveChanges();
                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterUnit, MenuId = masterCategorie.Id, MenuCode = masterCategorie.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
                return PartialView("../Masters/MasterCategories/_Edit", masterCategorie);

            }
        }



        [HttpPost]
        [Authorize(Roles = "MasterCategoriesDelete")]
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
                            MasterCategorie obj = db.MasterCategories.Find(id);
                            if (obj == null)
                                failed++;
                            else
                            {
                                MasterCategorie tmp = obj;
                                db.MasterCategories.Remove(obj);
                                db.SaveChanges();
                               // db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCategorie, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
