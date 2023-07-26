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
            return View("../Inventory/MasterCategories/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterCategoriesActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Inventory/MasterCategories/_IndexGrid", db.Set<MasterCategory>().AsQueryable());
            else
                return PartialView("../Inventory/MasterCategories/_IndexGrid", db.Set<MasterCategory>().AsQueryable()
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
            MasterCategory masterCategory = db.MasterCategories.Find(id);
            if (masterCategory == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/MasterCategories/_Details", masterCategory);
        }

        // GET: MasterCategories/Create
        [Authorize(Roles = "MasterCategoriesAdd")]
        public ActionResult Create()
        {
            MasterCategory masterCategory = new MasterCategory();
            masterCategory.Active = true;

            return PartialView("../Inventory/MasterCategories/_Create", masterCategory);
        }

        // POST: MasterUnits/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCategoriesAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Notes,Active,Created,Updated,UserId")] MasterCategory masterCategory)
        {
            masterCategory.Created = DateTime.Now;
            masterCategory.Updated = DateTime.Now;
            masterCategory.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(masterCategory.Code)) masterCategory.Code = masterCategory.Code.ToUpper();
            if (!string.IsNullOrEmpty(masterCategory.Name)) masterCategory.Name = masterCategory.Name.ToUpper();
            if (!string.IsNullOrEmpty(masterCategory.Notes)) masterCategory.Notes = masterCategory.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.MasterCategories.Add(masterCategory);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCategory, MenuId = masterCategory.Id, MenuCode = masterCategory.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Inventory/MasterCategories/_Create", masterCategory);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterCategoriesAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                MasterCategory obj = db.MasterCategories.Find(id);
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
            MasterCategory masterCategory = db.MasterCategories.Find(id);
            if (masterCategory == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/MasterCategories/_Edit", masterCategory);
        }

        // POST: MasterCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCategoriesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Notes,Active,Updated")] MasterCategory masterCategory)
        {
            masterCategory.UserId = User.Identity.GetUserId<int>();
            masterCategory.Updated = DateTime.Now;

            if (!string.IsNullOrEmpty(masterCategory.Code)) masterCategory.Code = masterCategory.Code.ToUpper();
            if (!string.IsNullOrEmpty(masterCategory.Name)) masterCategory.Name = masterCategory.Name.ToUpper();
            if (!string.IsNullOrEmpty(masterCategory.Notes)) masterCategory.Notes = masterCategory.Notes.ToUpper();
            
            db.Entry(masterCategory).State = EntityState.Unchanged;
            db.Entry(masterCategory).Property("Code").IsModified = true;
            db.Entry(masterCategory).Property("Name").IsModified = true;
            db.Entry(masterCategory).Property("Notes").IsModified = true;
            db.Entry(masterCategory).Property("Active").IsModified = true;
            db.Entry(masterCategory).Property("Updated").IsModified = true;
            db.Entry(masterCategory).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCategory, MenuId = masterCategory.Id, MenuCode = masterCategory.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Inventory/MasterCategories/_Edit", masterCategory);
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
                            MasterCategory obj = db.MasterCategories.Find(id);
                            if (obj == null)
                                failed++;
                            else
                            {
                                MasterCategory tmp = obj;

                                db.MasterCategories.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCategory, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
