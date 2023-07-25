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
    public class MasterBrandsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterBrands
        [Authorize(Roles = "MasterBrandsActive")]
        public ActionResult Index()
        {
            return View("../Inventory/MasterBrands/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterBrandsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Inventory/MasterBrands/_IndexGrid", db.Set<MasterBrand>().AsQueryable());
            else
                return PartialView("../Inventory/MasterBrands/_IndexGrid", db.Set<MasterBrand>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "MasterBrandsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterBrands.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterBrands.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterBrands/Details/
        [Authorize(Roles = "MasterBrandsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterBrand masterBrand = db.MasterBrands.Find(id);
            if (masterBrand == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/MasterBrands/_Details", masterBrand);
        }

        // GET: MasterBrands/Create
        [Authorize(Roles = "MasterBrandsAdd")]
        public ActionResult Create()
        {
            MasterBrand masterBrand = new MasterBrand();
            masterBrand.Active = true;

            return PartialView("../Inventory/MasterBrands/_Create", masterBrand);
        }

        // POST: MasterBrands/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterBrandsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,notes,Active,Created,Updated,UserId")] MasterBrand masterBrand)
        {
            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (!string.IsNullOrEmpty(masterBrand.Code)) masterBrand.Code = masterBrand.Code.ToUpper();
                        if (!string.IsNullOrEmpty(masterBrand.Name)) masterBrand.Name = masterBrand.Name.ToUpper();
                        if (!string.IsNullOrEmpty(masterBrand.Notes)) masterBrand.Notes = masterBrand.Notes.ToUpper();


                        masterBrand.Created = DateTime.Now;
                        masterBrand.Updated = DateTime.Now;
                        masterBrand.UserId = User.Identity.GetUserId<int>();
                        db.MasterBrands.Add(masterBrand);
                        db.SaveChanges();
                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBrand, MenuId = masterBrand.Id, MenuCode = masterBrand.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return PartialView("../Inventory/MasterBrands/_Create", masterBrand);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterBrandsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                MasterBrand obj = db.MasterBrands.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        db.MasterBrands.Remove(obj);
                        db.SaveChanges();
                    }
                }
            }
            return Json(id);
        }

        // GET: MasterBrands/Edit/5
        [Authorize(Roles = "MasterBrandsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterBrand masterBrand = db.MasterBrands.Find(id);
            if (masterBrand == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/MasterBrands/_Edit", masterBrand);
        }

        // POST: MasterBrands/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterBrandsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,notes,Active,updated,UserId")] MasterBrand masterBrand)
        {
            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    masterBrand.Updated = DateTime.Now;
                    if (ModelState.IsValid)
                    {
                        if (!string.IsNullOrEmpty(masterBrand.Code)) masterBrand.Code = masterBrand.Code.ToUpper();
                        if (!string.IsNullOrEmpty(masterBrand.Name)) masterBrand.Name = masterBrand.Name.ToUpper();
                        if (!string.IsNullOrEmpty(masterBrand.Notes)) masterBrand.Notes = masterBrand.Notes.ToUpper();

                        db.Entry(masterBrand).State = EntityState.Unchanged;
                        db.Entry(masterBrand).Property("Code").IsModified = true;
                        db.Entry(masterBrand).Property("Name").IsModified = true;
                        db.Entry(masterBrand).Property("Notes").IsModified = true;
                        db.Entry(masterBrand).Property("Active").IsModified = true;
                        db.Entry(masterBrand).Property("Updated").IsModified = true;
                        masterBrand.UserId = User.Identity.GetUserId<int>();
                        db.SaveChanges();
                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBrand, MenuId = masterBrand.Id, MenuCode = masterBrand.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
                return PartialView("../Inventory/MasterBrands/_Edit", masterBrand);

            }
        }



        [HttpPost]
        [Authorize(Roles = "MasterBrandsDelete")]
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
                            MasterBrand obj = db.MasterBrands.Find(id);
                            if (obj == null)
                                failed++;
                            else
                            {
                                MasterBrand tmp = obj;
                                db.MasterBrands.Remove(obj);
                                db.SaveChanges();
                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBrand, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
