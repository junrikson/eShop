using eShop.Extensions;
using eShop.Models;
using eShop.Properties;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace eShop.Controllers
{
    [Authorize]
    public class FixedAssetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterItems
        [Authorize(Roles = "FixedAssetsActive")]
        public ActionResult Index()
        {
            return View("../Asset/FixedAssets/Index");
        }

        [HttpGet]
        [Authorize(Roles = "FixedAssetsActive")]
        public PartialViewResult IndexGrid(string search)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            var masterRegions = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterRegionId).Distinct().ToList();
            var masterBusinessUnits = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnitId).Distinct().ToList();

            if (string.IsNullOrEmpty(search))
            {
                return PartialView("../Asset/FixedAssets/_IndexGrid", db.Set<FixedAsset>().Where(x =>
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable());
            }
            else
            {
                return PartialView("../Asset/FixedAssets/_IndexGrid", db.Set<FixedAsset>().Where(x =>
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable()
                        .Where(x => x.Code.Contains(search)));

            }
        }

        public JsonResult IsCodeExists(string Code, int? Id)
        {
            return Json(!IsAnyCode(Code, Id), JsonRequestBehavior.AllowGet);
        }

        private bool IsAnyCode(string Code, int? Id)
        {
            if (Id == null || Id == 0)
            {
                return db.FixedAssets.Any(x => x.Code == Code);
            }
            else
            {
                return db.FixedAssets.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: MasterItems/Create
        [Authorize(Roles = "FixedAssetsAdd")]
        public ActionResult Create()
        {

            FixedAsset fixedAsset = new FixedAsset
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MethodType = EnumMethodType.FixedDecliningBalance,
                Notes = "",
                Active = false,
                Tax = false,
                Purchasedate = DateTime.Now,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UserId = User.Identity.GetUserId<int>()
            };

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    db.FixedAssets.Add(fixedAsset);
                    db.SaveChanges();

                    dbTran.Commit();

                    fixedAsset.Code = "";
                    fixedAsset.Active = true;
                    fixedAsset.MasterBusinessUnitId = 0;
                    fixedAsset.MasterRegionId = 0;

                }

                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            return View("../Asset/FixedAssets/Create", fixedAsset);
        }

        // POST: MasterItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "FixedAssetsAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,MasterBusinessUnitId,MasterRegionId,Name,MethodType,Quantity,EstimatedLife,Purchasedate,Total,Tax,Notes,Active,Created,Updated,UserId")] FixedAsset fixedAsset)
        {
            fixedAsset.UserId = User.Identity.GetUserId<int>();
            fixedAsset.Created = DateTime.Now;
            fixedAsset.Updated = DateTime.Now;
            fixedAsset.Purchasedate = DateTime.Now;

            if (!string.IsNullOrEmpty(fixedAsset.Code)) fixedAsset.Code = fixedAsset.Code.ToUpper();
            if (!string.IsNullOrEmpty(fixedAsset.Notes)) fixedAsset.Notes = fixedAsset.Notes.ToUpper();
            if (!string.IsNullOrEmpty(fixedAsset.Name)) fixedAsset.Notes = fixedAsset.Name.ToUpper();

            if (ModelState.IsValid)
            {
                db.Entry(fixedAsset).State = EntityState.Unchanged;
                db.Entry(fixedAsset).Property("Code").IsModified = true;
                db.Entry(fixedAsset).Property("Notes").IsModified = true;
                db.Entry(fixedAsset).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(fixedAsset).Property("MasterRegionId").IsModified = true;
                db.Entry(fixedAsset).Property("Name").IsModified = true;
                db.Entry(fixedAsset).Property("MethodType").IsModified = true;
                db.Entry(fixedAsset).Property("Quantity").IsModified = true;
                db.Entry(fixedAsset).Property("EstimatedLife").IsModified = true;
                db.Entry(fixedAsset).Property("Purchasedate").IsModified = true;
                db.Entry(fixedAsset).Property("Total").IsModified = true;
                db.Entry(fixedAsset).Property("Tax").IsModified = true;
                db.Entry(fixedAsset).Property("Active").IsModified = true;
                db.Entry(fixedAsset).Property("Updated").IsModified = true;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItem, MenuId = fixedAsset.Id, MenuCode = fixedAsset.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                            db.SaveChanges();

                            dbTran.Commit();

                            return RedirectToAction("Index");
                        }
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            return View("../Asset/FixedAssets/Create", fixedAsset);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "FixedAssetsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null || id == 0)
            {
                FixedAsset obj = db.FixedAssets.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
     
                                db.FixedAssets.Remove(obj);
                                db.SaveChanges();

                                dbTran.Commit();
                            }
                            catch (DbEntityValidationException ex)
                            {
                                dbTran.Rollback();
                                throw ex;
                            }
                        }
                    }
                }
            }
            return Json(id);
        }

        // GET: MasterItems/Edit/5
        [Authorize(Roles = "FixedAssetsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FixedAsset fixedAsset = db.FixedAssets.Find(id);
            if (fixedAsset == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", fixedAsset.MasterBusinessUnitId);

            return View("../Asset/FixedAssets/Edit", fixedAsset);
        }

        // POST: MasterItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FixedAssetsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,MasterBusinessUnitId,MasterRegionId,Name,MethodType,Quantity,EstimatedLife,Purchasedate,Total,Tax,Notes,Active,Updated,UserId")] FixedAsset fixedAsset)
        {
            fixedAsset.Updated = DateTime.Now;
            fixedAsset.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(fixedAsset.Code)) fixedAsset.Code = fixedAsset.Code.ToUpper();
            if (!string.IsNullOrEmpty(fixedAsset.Notes)) fixedAsset.Notes = fixedAsset.Notes.ToUpper();

            db.Entry(fixedAsset).State = EntityState.Unchanged;
            db.Entry(fixedAsset).Property("Code").IsModified = true;
            db.Entry(fixedAsset).Property("Notes").IsModified = true;
            db.Entry(fixedAsset).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(fixedAsset).Property("MasterRegionId").IsModified = true;
            db.Entry(fixedAsset).Property("Name").IsModified = true;
            db.Entry(fixedAsset).Property("MethodType").IsModified = true;
            db.Entry(fixedAsset).Property("Quantity").IsModified = true;
            db.Entry(fixedAsset).Property("EstimatedLife").IsModified = true;
            db.Entry(fixedAsset).Property("Purchasedate").IsModified = true;
            db.Entry(fixedAsset).Property("Total").IsModified = true;
            db.Entry(fixedAsset).Property("Tax").IsModified = true;
            db.Entry(fixedAsset).Property("Active").IsModified = true;
            db.Entry(fixedAsset).Property("Updated").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.FixedAssets, MenuId = fixedAsset.Id, MenuCode = fixedAsset.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return RedirectToAction("Index");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", fixedAsset.MasterBusinessUnitId);

            return View("../Asset/FixedAssets/Edit", fixedAsset);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "FixedAssetsDelete")]
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
                        FixedAsset obj = db.FixedAssets.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    FixedAsset tmp = obj;


                                    db.FixedAssets.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.FixedAssets, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                                    db.SaveChanges();

                                    dbTran.Commit();
                                }
                                catch (DbEntityValidationException ex)
                                {
                                    dbTran.Rollback();
                                    throw ex;
                                }
                            }
                        }
                    }
                    return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                }
            }
        }





        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "FixedAssetsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            FixedAsset fixedAsset = db.FixedAssets.Find(id);

            if (masterBusinessUnit != null && fixedAsset != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                fixedAsset.MasterBusinessUnitId = masterBusinessUnitId;
                fixedAsset.MasterRegionId = masterRegionId;
                db.Entry(fixedAsset).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "FixedAssetsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.FixedAssetCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            FixedAsset lastData = db.FixedAssets
                .Where(x => (x.Code.Contains(code)))
                .OrderByDescending(z => z.Code).FirstOrDefault();

            if (lastData == null)
                code = "0001" + code;
            else
                code = (Convert.ToInt32(lastData.Code.Substring(0, 4)) + 1).ToString("D4") + code;

            return code;
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
