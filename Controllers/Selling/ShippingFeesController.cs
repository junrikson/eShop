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
    public class ShippingFeesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterItems
        [Authorize(Roles = "ShippingFeesActive")]
        public ActionResult Index()
        {
            return View("../Selling/ShippingFees/Index");
        }

        [HttpGet]
        [Authorize(Roles = "ShippingFeesActive")]
        public PartialViewResult IndexGrid(string search)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            var masterRegions = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterRegionId).Distinct().ToList();
            var masterBusinessUnits = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnitId).Distinct().ToList();

            if (string.IsNullOrEmpty(search))
            {
                return PartialView("../Selling/ShippingFees/_IndexGrid", db.Set<ShippingFee>().Where(x =>
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable());
            }
            else
            {
                return PartialView("../Selling/ShippingFees/_IndexGrid", db.Set<ShippingFee>().Where(x =>
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
                return db.ShippingFees.Any(x => x.Code == Code);
            }
            else
            {
                return db.ShippingFees.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        [HttpGet]
        [Authorize(Roles = "ShippingFeesActive")]
        public PartialViewResult OthersGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Selling/ShippingFees/_OthersGrid", db.Set<ShippingFee>().AsQueryable());
            else
                return PartialView("../Selling/ShippingFees/_OthersGrid", db.Set<ShippingFee>().AsQueryable()
                    .Where(y => y.Code.Contains(search)));
        }

        // GET: MasterItems/Details/5
        [Authorize(Roles = "ShippingFeesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShippingFee ShippingFee = db.ShippingFees.Find(id);
            if (ShippingFee == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/ShippingFees/_Details", ShippingFee);
        }

        [HttpGet]
        [Authorize(Roles = "ShippingFeesView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Selling/ShippingFees/_ViewGrid", db.ShippingFeeCustomers
                .Where(x => x.ShippingFeeId == Id).ToList());
        }

        // GET: MasterItems/Create
        [Authorize(Roles = "ShippingFeesAdd")]
        public ActionResult Create()
        {

            ShippingFee shippingFee = new ShippingFee
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MasterDestinationId = db.MasterDestinations.FirstOrDefault().Id,
                MasterCostId = db.MasterCosts.FirstOrDefault().Id,
                Notes = "",
                AllCustomer = false,
                Active = false,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UserId = User.Identity.GetUserId<int>()
            };

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    db.ShippingFees.Add(shippingFee);
                    db.SaveChanges();

                    dbTran.Commit();

                    shippingFee.Code = "";
                    shippingFee.Active = true;
                    shippingFee.MasterBusinessUnitId = 0;
                    shippingFee.MasterRegionId = 0;
                    shippingFee.MasterDestinationId = 0;
                    shippingFee.MasterCostId = 0;

                }

                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            return View("../Selling/ShippingFees/Create", shippingFee);
        }

        // POST: MasterItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "ShippingFeesAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,MasterBusinessUnitId,MasterRegionId,MasterCostId,MasterDestinationId,AllCustomer,Total,Notes,Active,Created,Updated,UserId")] ShippingFee obj)
        {
            obj.UserId = User.Identity.GetUserId<int>();
            obj.Created = DateTime.Now;
            obj.Updated = DateTime.Now;

            if (!string.IsNullOrEmpty(obj.Code)) obj.Code = obj.Code.ToUpper();
            if (!string.IsNullOrEmpty(obj.Notes)) obj.Notes = obj.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                db.Entry(obj).State = EntityState.Unchanged;
                db.Entry(obj).Property("Code").IsModified = true;
                db.Entry(obj).Property("Notes").IsModified = true;
                db.Entry(obj).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(obj).Property("MasterRegionId").IsModified = true;
                db.Entry(obj).Property("MasterCostId").IsModified = true;
                db.Entry(obj).Property("MasterDestinationId").IsModified = true;
                db.Entry(obj).Property("AllCustomer").IsModified = true;
                db.Entry(obj).Property("Total").IsModified = true;
                db.Entry(obj).Property("Active").IsModified = true;
                db.Entry(obj).Property("Updated").IsModified = true;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ShippingFees, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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


            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", obj.MasterBusinessUnitId);
            return View("../Selling/ShippingFees/Create", obj);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ShippingFeesAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null || id == 0)
            {
                ShippingFee obj = db.ShippingFees.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.ShippingFeeCustomers.RemoveRange(db.ShippingFeeCustomers.Where(x => x.ShippingFeeId == obj.Id));
                                db.SaveChanges();

                                db.ShippingFees.Remove(obj);
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
        [Authorize(Roles = "ShippingFeesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShippingFee shippingFee = db.ShippingFees.Find(id);
            if (shippingFee == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", shippingFee.MasterBusinessUnitId);

            return View("../Selling/ShippingFees/Edit", shippingFee);
        }

        // POST: MasterItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ShippingFeesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,MasterBusinessUnitId,MasterRegionId,MasterCostId,MasterDestinationId,Total,AllCustomer,Notes,Active,Updated,UserId")] ShippingFee obj)
        {
            obj.Updated = DateTime.Now;
            obj.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(obj.Code)) obj.Code = obj.Code.ToUpper();
            if (!string.IsNullOrEmpty(obj.Notes)) obj.Notes = obj.Notes.ToUpper();

            db.Entry(obj).State = EntityState.Unchanged;
            db.Entry(obj).Property("Code").IsModified = true;
            db.Entry(obj).Property("Notes").IsModified = true;
            db.Entry(obj).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(obj).Property("MasterRegionId").IsModified = true;
            db.Entry(obj).Property("MasterCostId").IsModified = true;
            db.Entry(obj).Property("MasterDestinationId").IsModified = true;
            db.Entry(obj).Property("AllCustomer").IsModified = true;
            db.Entry(obj).Property("Total").IsModified = true;
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

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ShippingFees, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", obj.MasterBusinessUnitId);
            return View("../Selling/ShippingFees/Edit", obj);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ShippingFeesDelete")]
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
                        ShippingFee obj = db.ShippingFees.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    ShippingFee tmp = obj;

                                    db.ShippingFeeCustomers.RemoveRange(db.ShippingFeeCustomers.Where(x => x.ShippingFeeId == obj.Id));
                                    db.SaveChanges();

                                    db.ShippingFees.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ShippingFees, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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


        [HttpGet]
        [Authorize(Roles = "ShippingFeesActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Selling/ShippingFees/_DetailsGrid", db.ShippingFeeCustomers
                .Where(x => x.ShippingFeeId == Id).ToList());
        }

        [Authorize(Roles = "ShippingFeesActive")]
        public ActionResult DetailsCreate(int shippingFeeId)
        {
            ShippingFee shippingFee = db.ShippingFees.Find(shippingFeeId);

            if (shippingFee == null)
            {
                return HttpNotFound();
            }

            ShippingFeeCustomer shippingFeeCustomer = new ShippingFeeCustomer
            {
                ShippingFeeId = shippingFeeId
            };

            return PartialView("../Selling/ShippingFees/_DetailsCreate", shippingFeeCustomer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ShippingFeesActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,ShippingFeeId,MasterCustomerId,Created,Updated,UserId")] ShippingFeeCustomer obj)
        {
            obj.Created = DateTime.Now;
            obj.Updated = DateTime.Now;
            obj.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.ShippingFeeCustomers.Add(obj);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ShippingFees, MenuId = obj.ShippingFeeId, MenuCode = obj.MasterCustomerId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Selling/ShippingFees/_DetailsCreate", obj);
        }

        [Authorize(Roles = "ShippingFeesActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ShippingFeeCustomer obj = db.ShippingFeeCustomers.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/ShippingFees/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,ShippingFeeId,MasterCustomerId,Updated,UserId")] ShippingFeeCustomer obj)
        {
            obj.Updated = DateTime.Now;
            obj.UserId = User.Identity.GetUserId<int>();


            db.Entry(obj).State = EntityState.Unchanged;
            db.Entry(obj).Property("MasterSupplierId").IsModified = true;
            db.Entry(obj).Property("Updated").IsModified = true;
            db.Entry(obj).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ShippingFees, MenuId = obj.MasterCustomerId, MenuCode = obj.MasterCustomerId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Selling/ShippingFees/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ShippingFeesActive")]
        public ActionResult DetailsBatchDelete(int[] ids)
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
                        ShippingFeeCustomer obj = db.ShippingFeeCustomers.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    ShippingFeeCustomer tmp = obj;

                                    db.ShippingFeeCustomers.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ShippingFees, MenuId = tmp.MasterCustomerId, MenuCode = tmp.MasterCustomerId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "ShippingFeesActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            ShippingFee shippingFee = db.ShippingFees.Find(id);

            if (masterBusinessUnit != null && shippingFee != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                shippingFee.MasterBusinessUnitId = masterBusinessUnitId;
                shippingFee.MasterRegionId = masterRegionId;
                db.Entry(shippingFee).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "ShippingFeesActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.ShippingFeeCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            ShippingFee lastData = db.ShippingFees
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
