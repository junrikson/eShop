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
    public class FixedAssetsCategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterDestinations
        [Authorize(Roles = "FixedAssetsCategoriesActive")]
        public ActionResult Index()
        {
            return View("../Asset/FixedAssetsCategories/Index");
        }

        [HttpGet]
        [Authorize(Roles = "FixedAssetsCategoriesActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Asset/FixedAssetsCategories/_IndexGrid", db.Set<FixedAssetCategory>().AsQueryable());
            else
                return PartialView("../Asset/FixedAssetsCategories/_IndexGrid", db.Set<FixedAssetCategory>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        // GET: MasterDestinations/Details/
        [Authorize(Roles = "FixedAssetsCategoriesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FixedAssetCategory obj = db.FixedAssetCategories.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Asset/FixedAssetsCategories/_Details", obj);
        }

        public JsonResult IsCodeExists(string Code, int? Id)
        {
            return Json(!IsAnyCode(Code, Id), JsonRequestBehavior.AllowGet);
        }

        private bool IsAnyCode(string Code, int? Id)
        {
            if (Id == null || Id == 0)
            {
                return db.FixedAssetCategories.Any(x => x.Code == Code);
            }
            else
            {
                return db.FixedAssetCategories.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: MasterDestinations/Create
        [Authorize(Roles = "FixedAssetsCategoriesAdd")]
        public ActionResult Create()
        {
            FixedAssetCategory obj = new FixedAssetCategory
            { Active = true 
            
            };

            return PartialView("../Asset/FixedAssetsCategories/_Create", obj);
        }

        // POST: MasterDestinations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FixedAssetsCategoriesAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Notes,Active,Created,Updated,UserId")] FixedAssetCategory obj)
        {
            obj.Created = DateTime.Now;
            obj.Updated = DateTime.Now;
            obj.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(obj.Code)) obj.Code = obj.Code.ToUpper();
            if (!string.IsNullOrEmpty(obj.Name)) obj.Name = obj.Name.ToUpper();
            if (!string.IsNullOrEmpty(obj.Notes)) obj.Notes = obj.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.FixedAssetCategories.Add(obj);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterDestination, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Asset/FixedAssetsCategories/_Create", obj);
        }

        // GET: MasterDestinations/Edit/5
        [Authorize(Roles = "FixedAssetsCategoriesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FixedAssetCategory obj = db.FixedAssetCategories.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Asset/FixedAssetsCategories/_Edit", obj);
        }

        // POST: MasterDestinations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FixedAssetsCategoriesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Notes,Active,Updated,UserId")] FixedAssetCategory obj)
        {
            obj.UserId = User.Identity.GetUserId<int>();
            obj.Updated = DateTime.Now;

            if (!string.IsNullOrEmpty(obj.Code)) obj.Code = obj.Code.ToUpper();
            if (!string.IsNullOrEmpty(obj.Name)) obj.Name = obj.Name.ToUpper();
            if (!string.IsNullOrEmpty(obj.Notes)) obj.Notes = obj.Notes.ToUpper();

            db.Entry(obj).State = EntityState.Unchanged;
            db.Entry(obj).Property("Code").IsModified = true;
            db.Entry(obj).Property("Name").IsModified = true;
            db.Entry(obj).Property("Notes").IsModified = true;
            db.Entry(obj).Property("Active").IsModified = true;
            db.Entry(obj).Property("Updated").IsModified = true;
            db.Entry(obj).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.FixedAssetsCategory, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Asset/FixedAssetsCategories/_Edit", obj);
        }


        [HttpPost]
        [Authorize(Roles = "FixedAssetsCategoriesDelete")]
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
                            FixedAssetCategory obj = db.FixedAssetCategories.Find(id);
                            if (obj == null)
                                failed++;
                            else
                            {
                                FixedAssetCategory tmp = obj;

                                db.FixedAssetCategories.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.FixedAssetsCategory, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
