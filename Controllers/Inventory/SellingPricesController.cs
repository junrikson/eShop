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
    public class SellingPricesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterItems
        [Authorize(Roles = "SellingPricesActive")]
        public ActionResult Index()
        {
            return View("../Inventory/SellingPrices/Index");
        }

        [HttpGet]
        [Authorize(Roles = "SellingPricesActive")]
        public PartialViewResult IndexGrid(string search)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            var masterRegions = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterRegionId).Distinct().ToList();
            var masterBusinessUnits = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnitId).Distinct().ToList();

            if (string.IsNullOrEmpty(search))
            {
                return PartialView("../Inventory/SellingPrices/_IndexGrid", db.Set<SellingPrice>().Where(x =>
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable());
            }
            else
            {
                return PartialView("../Inventory/SellingPrices/_IndexGrid", db.Set<SellingPrice>().Where(x =>
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
                return db.SellingPrices.Any(x => x.Code == Code);
            }
            else
            {
                return db.SellingPrices.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        [HttpGet]
        [Authorize(Roles = "SellingPricesActive")]
        public PartialViewResult OthersGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Inventory/SellingPrices/_OthersGrid", db.Set<SellingPrice>().AsQueryable());
            else
                return PartialView("../Inventory/SellingPrices/_OthersGrid", db.Set<SellingPrice>().AsQueryable()
                    .Where(y => y.Code.Contains(search)));
        }

        // GET: MasterItems/Details/5
        [Authorize(Roles = "SellingPricesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SellingPrice SellingPrice = db.SellingPrices.Find(id);
            if (SellingPrice == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/SellingPrices/_Details", SellingPrice);
        }

        [HttpGet]
        [Authorize(Roles = "SellingPricesView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Inventory/SellingPrices/_ViewGrid", db.SellingPricesCustomers
                .Where(x => x.SellingPriceId == Id).ToList());
        }

        // GET: MasterItems/Create
        [Authorize(Roles = "SellingPricesAdd")]
        public ActionResult Create()
        {

            SellingPrice sellingPrice = new SellingPrice
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                Notes = "",
                Active = false,
                AllCustomer = false,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UserId = User.Identity.GetUserId<int>()
            };

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    db.SellingPrices.Add(sellingPrice);
                    db.SaveChanges();

                    dbTran.Commit();

                    sellingPrice.Code = "";
                    sellingPrice.Active = true;
                    sellingPrice.MasterBusinessUnitId = 0;
                    sellingPrice.MasterRegionId = 0;

                }

                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            return View("../Inventory/SellingPrices/Create", sellingPrice);
        }

        // POST: MasterItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "SellingPricesAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,MasterBusinessUnitId,MasterRegionId,Notes,Active,AllCustomer,Created,Updated,UserId")] SellingPrice sellingPrice)
        {
            sellingPrice.UserId = User.Identity.GetUserId<int>();
            sellingPrice.AllCustomer = sellingPrice.AllCustomer;
            sellingPrice.Created = DateTime.Now;
            sellingPrice.Updated = DateTime.Now;

            if (!string.IsNullOrEmpty(sellingPrice.Code)) sellingPrice.Code = sellingPrice.Code.ToUpper();
            if (!string.IsNullOrEmpty(sellingPrice.Notes)) sellingPrice.Notes = sellingPrice.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                db.Entry(sellingPrice).State = EntityState.Unchanged;
                db.Entry(sellingPrice).Property("Code").IsModified = true;
                db.Entry(sellingPrice).Property("Notes").IsModified = true;
                db.Entry(sellingPrice).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(sellingPrice).Property("MasterRegionId").IsModified = true;
                db.Entry(sellingPrice).Property("Active").IsModified = true;
                db.Entry(sellingPrice).Property("AllCustomer").IsModified = true;
                db.Entry(sellingPrice).Property("Updated").IsModified = true;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItem, MenuId = sellingPrice.Id, MenuCode = sellingPrice.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return View("../Inventory/SellingPrices/Create", sellingPrice);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SellingPricesAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null || id == 0)
            {
                SellingPrice obj = db.SellingPrices.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.SellingPricesCustomers.RemoveRange(db.SellingPricesCustomers.Where(x => x.SellingPriceId == obj.Id));
                                db.SaveChanges();

                                db.SellingPricesItems.RemoveRange(db.SellingPricesItems.Where(x => x.SellingPriceId == obj.Id));
                                db.SaveChanges();

                                db.SellingPrices.Remove(obj);
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
        [Authorize(Roles = "SellingPricesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SellingPrice sellingPrice = db.SellingPrices.Find(id);
            if (sellingPrice == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", sellingPrice.MasterBusinessUnitId);

            return View("../Inventory/SellingPrices/Edit", sellingPrice);
        }

        // POST: MasterItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SellingPricesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,MasterBusinessUnitId,MasterRegionId,Notes,Active,AllCustomer,Updated,UserId")] SellingPrice sellingPrice)
        {
            sellingPrice.Updated = DateTime.Now;
            sellingPrice.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(sellingPrice.Code)) sellingPrice.Code = sellingPrice.Code.ToUpper();
            if (!string.IsNullOrEmpty(sellingPrice.Notes)) sellingPrice.Notes = sellingPrice.Notes.ToUpper();

            db.Entry(sellingPrice).State = EntityState.Unchanged;
            db.Entry(sellingPrice).Property("Code").IsModified = true;
            db.Entry(sellingPrice).Property("Notes").IsModified = true;
            db.Entry(sellingPrice).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(sellingPrice).Property("MasterRegionId").IsModified = true;
            db.Entry(sellingPrice).Property("Active").IsModified = true;
            db.Entry(sellingPrice).Property("AllCustomer").IsModified = true;
            db.Entry(sellingPrice).Property("Updated").IsModified = true;
            db.Entry(sellingPrice).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SellingPrices, MenuId = sellingPrice.Id, MenuCode = sellingPrice.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", sellingPrice.MasterBusinessUnitId);

            return View("../Inventory/SellingPrices/Edit", sellingPrice);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SellingPricesDelete")]
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
                        SellingPrice obj = db.SellingPrices.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    SellingPrice tmp = obj;

                                    db.SellingPricesCustomers.RemoveRange(db.SellingPricesCustomers.Where(x => x.SellingPriceId == obj.Id));
                                    db.SaveChanges();

                                    db.SellingPricesItems.RemoveRange(db.SellingPricesItems.Where(x => x.SellingPriceId == obj.Id));
                                    db.SaveChanges();

                                    db.SellingPrices.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SellingPrices, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        private SellingPrice GetModelState(SellingPrice sellingPrice)
        {
            List<SellingPriceCustomer> sellingPriceCustomer = db.SellingPricesCustomers.Where(x => x.SellingPriceId == sellingPrice.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(sellingPrice.Code, sellingPrice.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (sellingPriceCustomer == null || sellingPriceCustomer.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return sellingPrice;
        }

        [HttpGet]
        [Authorize(Roles = "SellingPricesActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Inventory/SellingPrices/_DetailsGrid", db.SellingPricesCustomers
                .Where(x => x.SellingPriceId == Id).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "SellingPricesActive")]
        public PartialViewResult ItemsGrid(int Id)
        {
            return PartialView("../Inventory/SellingPrices/_ItemsGrid", db.SellingPricesItems
                .Where(x => x.SellingPriceId == Id).ToList());
        }

        [Authorize(Roles = "SellingPricesActive")]
        public ActionResult DetailsCreate(int sellingPriceId)
        {
            SellingPrice sellingPrice = db.SellingPrices.Find(sellingPriceId);

            if (sellingPrice == null)
            {
                return HttpNotFound();
            }

            SellingPriceCustomer sellingPriceCustomer = new SellingPriceCustomer
            {
                SellingPriceId = sellingPriceId
            };

            return PartialView("../Inventory/SellingPrices/_DetailsCreate", sellingPriceCustomer);
        }

        [Authorize(Roles = "SellingPricesActive")]
        public ActionResult ItemsCreate(int sellingPriceId)
        {
            SellingPrice sellingPrice = db.SellingPrices.Find(sellingPriceId);

            if (sellingPrice == null)
            {
                return HttpNotFound();
            }

            SellingPriceItem sellingPriceItem = new SellingPriceItem
            {
                SellingPriceId = sellingPriceId
            };

            return PartialView("../Inventory/SellingPrices/_ItemsCreate", sellingPriceItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SellingPricesActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,SellingPriceId,MasterCustomerId,Created,Updated,UserId")] SellingPriceCustomer sellingPriceCustomer)
        {
            sellingPriceCustomer.Created = DateTime.Now;
            sellingPriceCustomer.Updated = DateTime.Now;
            sellingPriceCustomer.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SellingPricesCustomers.Add(sellingPriceCustomer);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SellingPricesCustomers, MenuId = sellingPriceCustomer.SellingPriceId, MenuCode = sellingPriceCustomer.MasterCustomerId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Inventory/SellingPrices/_DetailsCreate", sellingPriceCustomer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SellingPricesActive")]
        public ActionResult ItemsCreate([Bind(Include = "Id,SellingPriceId,MasterItemId,MasterItemUnitId,Price,Created,Updated,UserId")] SellingPriceItem sellingPriceItem)
        {
            sellingPriceItem.Created = DateTime.Now;
            sellingPriceItem.Updated = DateTime.Now;
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(sellingPriceItem.MasterItemUnitId);
            sellingPriceItem.Price = sellingPriceItem.Price;
            sellingPriceItem.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SellingPricesItems.Add(sellingPriceItem);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SellingPricesItems, MenuId = sellingPriceItem.SellingPriceId, MenuCode = sellingPriceItem.MasterItemId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Inventory/SellingPrices/_ItemsCreate", sellingPriceItem);
        }

        [Authorize(Roles = "SellingPricesActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SellingPriceCustomer obj = db.SellingPricesCustomers.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/SellingPrices/_DetailsEdit", obj);
        }

        [Authorize(Roles = "SellingPricesActive")]
        public ActionResult ItemsEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SellingPriceItem obj = db.SellingPricesItems.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/SellingPrices/_ItemsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,SellingPriceId,MasterCustomerId,Updated,UserId")] SellingPriceCustomer sellingPriceCustomer)
        {
            sellingPriceCustomer.Updated = DateTime.Now;
            sellingPriceCustomer.UserId = User.Identity.GetUserId<int>();

            db.Entry(sellingPriceCustomer).State = EntityState.Unchanged;
            db.Entry(sellingPriceCustomer).Property("MasterCustomerId").IsModified = true;
            db.Entry(sellingPriceCustomer).Property("Updated").IsModified = true;
            db.Entry(sellingPriceCustomer).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SellingPricesCustomers, MenuId = sellingPriceCustomer.MasterCustomerId, MenuCode = sellingPriceCustomer.MasterCustomerId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Inventory/SellingPrices/_DetailsEdit", sellingPriceCustomer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult ItemsEdit([Bind(Include = "Id,SellingPriceId,MasterItemId,MasterItemUnitId,Price,Updated,UserId")] SellingPriceItem sellingPriceItem)
        {
            sellingPriceItem.Updated = DateTime.Now;
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(sellingPriceItem.MasterItemUnitId);
            sellingPriceItem.UserId = User.Identity.GetUserId<int>();

            db.Entry(sellingPriceItem).State = EntityState.Unchanged;
            db.Entry(sellingPriceItem).Property("MasterItemId").IsModified = true;
            db.Entry(sellingPriceItem).Property("MasterItemUnitId").IsModified = true;
            db.Entry(sellingPriceItem).Property("Price").IsModified = true;
            db.Entry(sellingPriceItem).Property("Updated").IsModified = true;
            db.Entry(sellingPriceItem).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SellingPricesItems, MenuId = sellingPriceItem.MasterItemId, MenuCode = sellingPriceItem.MasterItemId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Inventory/SellingPrices/_ItemsEdit", sellingPriceItem);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SellingPricesActive")]
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
                        SellingPriceCustomer obj = db.SellingPricesCustomers.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    SellingPriceCustomer tmp = obj;

                                    db.SellingPricesCustomers.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SellingPricesCustomers, MenuId = tmp.MasterCustomerId, MenuCode = tmp.MasterCustomerId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "SellingPricesActive")]
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
                        SellingPriceItem obj = db.SellingPricesItems.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    SellingPriceItem tmp = obj;

                                    db.SellingPricesItems.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SellingPricesItems, MenuId = tmp.MasterItemId, MenuCode = tmp.MasterItemId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "SellingPricesActive")]
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
        [Authorize(Roles = "SellingPricesActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            SellingPrice sellingPrice = db.SellingPrices.Find(id);

            if (masterBusinessUnit != null && sellingPrice != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                sellingPrice.MasterBusinessUnitId = masterBusinessUnitId;
                sellingPrice.MasterRegionId = masterRegionId;
                db.Entry(sellingPrice).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "SellingPricesActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.SellingPriceCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            SellingPrice lastData = db.SellingPrices
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
