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
    public class BuyingPricesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterItems
        [Authorize(Roles = "BuyingPricesActive")]
        public ActionResult Index()
        {
            return View("../Inventory/BuyingPrices/Index");
        }

        [HttpGet]
        [Authorize(Roles = "BuyingPricesActive")]
        public PartialViewResult IndexGrid(string search)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            var masterRegions = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterRegionId).Distinct().ToList();
            var masterBusinessUnits = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnitId).Distinct().ToList();

            if (string.IsNullOrEmpty(search))
            {
                return PartialView("../Inventory/BuyingPrices/_IndexGrid", db.Set<BuyingPrice>().Where(x =>
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable());
            }
            else
            {
                return PartialView("../Inventory/BuyingPrices/_IndexGrid", db.Set<BuyingPrice>().Where(x =>
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
                return db.BuyingPrices.Any(x => x.Code == Code);
            }
            else
            {
                return db.BuyingPrices.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        [HttpGet]
        [Authorize(Roles = "BuyingPricesActive")]
        public PartialViewResult OthersGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Inventory/BuyingPrices/_OthersGrid", db.Set<BuyingPrice>().AsQueryable());
            else
                return PartialView("../Inventory/BuyingPrices/_OthersGrid", db.Set<BuyingPrice>().AsQueryable()
                    .Where(y => y.Code.Contains(search)));
        }

        // GET: MasterItems/Details/5
        [Authorize(Roles = "BuyingPricesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BuyingPrice BuyingPrice = db.BuyingPrices.Find(id);
            if (BuyingPrice == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/BuyingPrices/_Details", BuyingPrice);
        }

        [HttpGet]
        [Authorize(Roles = "BuyingPricesView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Inventory/BuyingPrices/_ViewGrid", db.BuyingPricesSuppliers
                .Where(x => x.BuyingPriceId == Id).ToList());
        }

        // GET: MasterItems/Create
        [Authorize(Roles = "BuyingPricesAdd")]
        public ActionResult Create()
        {

            BuyingPrice buyingPrice = new BuyingPrice
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                Notes = "",
                Active = false,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UserId = User.Identity.GetUserId<int>()
            };

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    db.BuyingPrices.Add(buyingPrice);
                    db.SaveChanges();

                    dbTran.Commit();

                    buyingPrice.Code = "";
                    buyingPrice.Active = true;
                    buyingPrice.MasterBusinessUnitId = 0;
                    buyingPrice.MasterRegionId = 0;

                }

                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            return View("../Inventory/BuyingPrices/Create", buyingPrice);
        }

        // POST: MasterItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "BuyingPricesAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,MasterBusinessUnitId,MasterRegionId,Notes,Active,Created,Updated,UserId")] BuyingPrice buyingPrice)
        {
            buyingPrice.UserId = User.Identity.GetUserId<int>();
            buyingPrice.Created = DateTime.Now;
            buyingPrice.Updated = DateTime.Now;

            if (!string.IsNullOrEmpty(buyingPrice.Code)) buyingPrice.Code = buyingPrice.Code.ToUpper();
            if (!string.IsNullOrEmpty(buyingPrice.Notes)) buyingPrice.Notes = buyingPrice.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                db.Entry(buyingPrice).State = EntityState.Unchanged;
                db.Entry(buyingPrice).Property("Code").IsModified = true;
                db.Entry(buyingPrice).Property("Notes").IsModified = true;
                db.Entry(buyingPrice).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(buyingPrice).Property("MasterRegionId").IsModified = true;
                db.Entry(buyingPrice).Property("Active").IsModified = true;
                db.Entry(buyingPrice).Property("Updated").IsModified = true;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItem, MenuId = buyingPrice.Id, MenuCode = buyingPrice.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return View("../Inventory/BuyingPrices/Create", buyingPrice);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "BuyingPricesAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null || id == 0)
            {
                BuyingPrice obj = db.BuyingPrices.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.BuyingPricesSuppliers.RemoveRange(db.BuyingPricesSuppliers.Where(x => x.BuyingPriceId == obj.Id));
                                db.SaveChanges();

                                db.BuyingPricesItems.RemoveRange(db.BuyingPricesItems.Where(x => x.BuyingPriceId == obj.Id));
                                db.SaveChanges();

                                db.BuyingPrices.Remove(obj);
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
        [Authorize(Roles = "BuyingPricesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BuyingPrice buyingPrice = db.BuyingPrices.Find(id);
            if (buyingPrice == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", buyingPrice.MasterBusinessUnitId);

            return View("../Inventory/BuyingPrices/Edit", buyingPrice);
        }

        // POST: MasterItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BuyingPricesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,MasterBusinessUnitId,MasterRegionId,Notes,Active,Updated,UserId")] BuyingPrice buyingPrice)
        {
            buyingPrice.Updated = DateTime.Now;
            buyingPrice.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(buyingPrice.Code)) buyingPrice.Code = buyingPrice.Code.ToUpper();
            if (!string.IsNullOrEmpty(buyingPrice.Notes)) buyingPrice.Notes = buyingPrice.Notes.ToUpper();

            db.Entry(buyingPrice).State = EntityState.Unchanged;
            db.Entry(buyingPrice).Property("Code").IsModified = true;
            db.Entry(buyingPrice).Property("Notes").IsModified = true;
            db.Entry(buyingPrice).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(buyingPrice).Property("MasterRegionId").IsModified = true;
            db.Entry(buyingPrice).Property("Active").IsModified = true;
            db.Entry(buyingPrice).Property("Updated").IsModified = true;
            db.Entry(buyingPrice).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BuyingPrices, MenuId = buyingPrice.Id, MenuCode = buyingPrice.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", buyingPrice.MasterBusinessUnitId);

            return View("../Inventory/BuyingPrices/Edit", buyingPrice);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "BuyingPricesDelete")]
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
                        BuyingPrice obj = db.BuyingPrices.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    BuyingPrice tmp = obj;

                                    db.BuyingPricesSuppliers.RemoveRange(db.BuyingPricesSuppliers.Where(x => x.BuyingPriceId == obj.Id));
                                    db.SaveChanges();

                                    db.BuyingPricesItems.RemoveRange(db.BuyingPricesItems.Where(x => x.BuyingPriceId == obj.Id));
                                    db.SaveChanges();

                                    db.BuyingPrices.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BuyingPrices, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "SalesRequestsActive")]
        private BuyingPrice GetModelState(BuyingPrice buyingPrice)
        {
            List<BuyingPriceSupplier> buyingPriceSupplier = db.BuyingPricesSuppliers.Where(x => x.BuyingPriceId == buyingPrice.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(buyingPrice.Code, buyingPrice.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (buyingPriceSupplier == null || buyingPriceSupplier.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return buyingPrice;
        }

        [HttpGet]
        [Authorize(Roles = "BuyingPricesActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Inventory/BuyingPrices/_DetailsGrid", db.BuyingPricesSuppliers
                .Where(x => x.BuyingPriceId == Id).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "BuyingPricesActive")]
        public PartialViewResult ItemsGrid(int Id)
        {
            return PartialView("../Inventory/BuyingPrices/_ItemsGrid", db.BuyingPricesItems
                .Where(x => x.BuyingPriceId == Id).ToList());
        }

        [Authorize(Roles = "BuyingPricesActive")]
        public ActionResult DetailsCreate(int buyingPriceId)
        {
            BuyingPrice buyingPrice = db.BuyingPrices.Find(buyingPriceId);

            if (buyingPrice == null)
            {
                return HttpNotFound();
            }

            BuyingPriceSupplier buyingPriceSupplier = new BuyingPriceSupplier
            {
                BuyingPriceId = buyingPriceId
            };

            return PartialView("../Inventory/BuyingPrices/_DetailsCreate", buyingPriceSupplier);
        }

        [Authorize(Roles = "BuyingPricesActive")]
        public ActionResult ItemsCreate(int buyingPriceId)
        {
            BuyingPrice buyingPrice = db.BuyingPrices.Find(buyingPriceId);

            if (buyingPrice == null)
            {
                return HttpNotFound();
            }

            BuyingPriceItem buyingPriceItem = new BuyingPriceItem
            {
                BuyingPriceId = buyingPriceId
            };

            return PartialView("../Inventory/BuyingPrices/_ItemsCreate", buyingPriceItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BuyingPricesActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,BuyingPriceId,MasterSupplierId,Created,Updated,UserId")] BuyingPriceSupplier buyingPriceSupplier)
        {
            buyingPriceSupplier.Created = DateTime.Now;
            buyingPriceSupplier.Updated = DateTime.Now;
            buyingPriceSupplier.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.BuyingPricesSuppliers.Add(buyingPriceSupplier);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BuyingPricesSuppliers, MenuId = buyingPriceSupplier.BuyingPriceId, MenuCode = buyingPriceSupplier.MasterSupplierId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Inventory/BuyingPrices/_DetailsCreate", buyingPriceSupplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BuyingPricesActive")]
        public ActionResult ItemsCreate([Bind(Include = "Id,BuyingPriceId,MasterItemId,MasterItemUnitId,Price,Created,Updated,UserId")] BuyingPriceItem buyingPriceItem)
        {
            buyingPriceItem.Created = DateTime.Now;
            buyingPriceItem.Updated = DateTime.Now;
            buyingPriceItem.Price = buyingPriceItem.Price;
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(buyingPriceItem.MasterItemUnitId);
            buyingPriceItem.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.BuyingPricesItems.Add(buyingPriceItem);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BuyingPricesItems, MenuId = buyingPriceItem.BuyingPriceId, MenuCode = buyingPriceItem.MasterItemId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Inventory/BuyingPrices/_ItemsCreate", buyingPriceItem);
        }

        [Authorize(Roles = "BuyingPricesActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BuyingPriceSupplier obj = db.BuyingPricesSuppliers.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/BuyingPrices/_DetailsEdit", obj);
        }

        [Authorize(Roles = "BuyingPricesActive")]
        public ActionResult ItemsEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BuyingPriceItem obj = db.BuyingPricesItems.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/BuyingPrices/_ItemsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,BuyingPriceId,MasterSupplierId,Updated,UserId")] BuyingPriceSupplier buyingPriceSupplier)
        {
            buyingPriceSupplier.Updated = DateTime.Now;
            buyingPriceSupplier.UserId = User.Identity.GetUserId<int>();


            db.Entry(buyingPriceSupplier).State = EntityState.Unchanged;
            db.Entry(buyingPriceSupplier).Property("MasterSupplierId").IsModified = true;
            db.Entry(buyingPriceSupplier).Property("Updated").IsModified = true;
            db.Entry(buyingPriceSupplier).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BuyingPricesSuppliers, MenuId = buyingPriceSupplier.MasterSupplierId, MenuCode = buyingPriceSupplier.MasterSupplierId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Inventory/BuyingPrices/_DetailsEdit", buyingPriceSupplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult ItemsEdit([Bind(Include = "Id,BuyingPriceId,MasterItemId,MasterItemUnitId,Price,Updated,UserId")] BuyingPriceItem buyingPriceItem)
        {
            buyingPriceItem.Updated = DateTime.Now;
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(buyingPriceItem.MasterItemUnitId);
            buyingPriceItem.UserId = User.Identity.GetUserId<int>();

            db.Entry(buyingPriceItem).State = EntityState.Unchanged;
            db.Entry(buyingPriceItem).Property("MasterItemId").IsModified = true;
            db.Entry(buyingPriceItem).Property("MasterItemUnitId").IsModified = true;
            db.Entry(buyingPriceItem).Property("Price").IsModified = true;
            db.Entry(buyingPriceItem).Property("Updated").IsModified = true;
            db.Entry(buyingPriceItem).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BuyingPricesItems, MenuId = buyingPriceItem.MasterItemId, MenuCode = buyingPriceItem.MasterItemId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Inventory/BuyingPrices/_ItemsEdit", buyingPriceItem);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "BuyingPricesActive")]
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
                        BuyingPriceSupplier obj = db.BuyingPricesSuppliers.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    BuyingPriceSupplier tmp = obj;

                                    db.BuyingPricesSuppliers.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BuyingPricesSuppliers, MenuId = tmp.MasterSupplierId, MenuCode = tmp.MasterSupplierId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "BuyingPricesActive")]
        public ActionResult ItemsBatchDelete(int[] ids)
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
                        BuyingPriceItem obj = db.BuyingPricesItems.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    BuyingPriceItem tmp = obj;

                                    db.BuyingPricesItems.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BuyingPricesItems, MenuId = tmp.MasterItemId, MenuCode = tmp.MasterItemId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "BuyingPricesActive")]
        public JsonResult GetMasterUnit(int id)
        {
            int masterItemUnitId = 0;
            MasterItem masterItem = db.MasterItems.Find(id);

            if (masterItem != null)
            {
                MasterItemUnit masterItemUnit = db.MasterItemUnits.Where(x => x.MasterItemId == masterItem.Id && x.Default).FirstOrDefault();

                if (masterItemUnit != null)
                    masterItemUnitId = masterItemUnit.Id;
                else
                {
                    masterItemUnit = db.MasterItemUnits.Where(x => x.MasterItemId == masterItem.Id).FirstOrDefault();

                    if (masterItemUnit != null)
                        masterItemUnitId = masterItemUnit.Id;
                }
            }

            return Json(masterItemUnitId);
        }


        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "BuyingPricesActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            BuyingPrice buyingPrice = db.BuyingPrices.Find(id);

            if (masterBusinessUnit != null && buyingPrice != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                buyingPrice.MasterBusinessUnitId = masterBusinessUnitId;
                buyingPrice.MasterRegionId = masterRegionId;
                db.Entry(buyingPrice).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "BuyingPricesActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.BuyingPriceCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            BuyingPrice lastData = db.BuyingPrices
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
