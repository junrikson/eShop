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
    public class PurchaseOrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PurchaseOrders
        [Authorize(Roles = "PurchaseOrdersActive")]
        public ActionResult Index()
        {
            return View("../Buying/PurchaseOrders/Index");
        }

        [HttpGet]
        [Authorize(Roles = "PurchaseOrdersActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Buying/PurchaseOrders/_IndexGrid", db.Set<PurchaseOrder>().AsQueryable());
            else
                return PartialView("../Buying/PurchaseOrders/_IndexGrid", db.Set<PurchaseOrder>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        public JsonResult IsCodeExists(string Code, int? Id)
        {
            return Json(!IsAnyCode(Code, Id), JsonRequestBehavior.AllowGet);
        }

        private bool IsAnyCode(string Code, int? Id)
        {
            if (Id == null || Id == 0)
            {
                return db.PurchaseOrders.Any(x => x.Code == Code);
            }
            else
            {
                return db.PurchaseOrders.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: PurchaseOrders/Details/
        [Authorize(Roles = "PurchaseOrdersView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder PurchaseOrder = db.PurchaseOrders.Find(id);
            if (PurchaseOrder == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/PurchaseOrders/_Details", PurchaseOrder);
        }

        // GET: PurchaseOrders/Create
        [Authorize(Roles = "PurchaseOrdersAdd")]
        public ActionResult Create()
        {
            PurchaseOrder purchaseOrder = new PurchaseOrder
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MasterSupplierId = db.MasterSuppliers.FirstOrDefault().Id,
                IsPrint = false,
                Active = false,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UserId = User.Identity.GetUserId<int>()
            };

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    db.PurchaseOrders.Add(purchaseOrder);
                    db.SaveChanges();


                         PurchaseOrderDetails   purchaseOrderDetails = new PurchaseOrderDetails
                         {
                            PurchaseOrderId = purchaseOrder.Id,
                            MasterItemId = MasterItem.Id,
                            Default = false,
                            Active = true,
                            Created = DateTime.Now,
                            Updated = DateTime.Now,
                            UserId = User.Identity.GetUserId<int>()
                        };

                        db.MasterItemUnits.Add(masterItemUnit);
                        db.SaveChanges();
                  

                    dbTran.Commit();

                    purchaseOrder.Code = "";
                    purchaseOrder.Active = true;
                    purchaseOrder.MasterBusinessUnitId = 0;
                    purchaseOrder.MasterRegionId = 0;
                    purchaseOrder.MasterSupplierId = 0;
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name");
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes");
            ViewBag.Total = "0";

            return View("../Buying/PurchaseOrders/Create", purchaseOrder);
        }

        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Notes,Active,Created,Updated,UserId")] PurchaseOrder purchaseOrder)
        {
            if (!string.IsNullOrEmpty(purchaseOrder.Code)) purchaseOrder.Code = purchaseOrder.Code.ToUpper();
            if (!string.IsNullOrEmpty(purchaseOrder.Notes)) purchaseOrder.Notes = purchaseOrder.Notes.ToUpper();

            purchaseOrder.Created = DateTime.Now;
            purchaseOrder.Updated = DateTime.Now;
            purchaseOrder.UserId = User.Identity.GetUserId<int>();

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.PurchaseOrders.Add(purchaseOrder);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrder, MenuId = purchaseOrder.Id, MenuCode = purchaseOrder.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return PartialView("../Buying/PurchaseOrders/_Create", purchaseOrder);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                PurchaseOrder obj = db.PurchaseOrders.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.PurchaseOrdersDetails.RemoveRange(db.PurchaseOrdersDetails.Where(x => x.PurchaseOrderId == obj.Id));
                                db.SaveChanges();

                                db.PurchaseOrders.Remove(obj);
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

        // GET: PurchaseOrders/Edit/5
        [Authorize(Roles = "PurchaseOrdersEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder PurchaseOrder = db.PurchaseOrders.Find(id);
            if (PurchaseOrder == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/PurchaseOrders/_Edit", PurchaseOrder);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Notes,Active,Updated,UserId")] PurchaseOrder PurchaseOrder)
        {
            PurchaseOrder.Updated = DateTime.Now;
            PurchaseOrder.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(PurchaseOrder.Code)) PurchaseOrder.Code = PurchaseOrder.Code.ToUpper();
          //  if (!string.IsNullOrEmpty(PurchaseOrder.Name)) PurchaseOrder.Name = PurchaseOrder.Name.ToUpper();
            if (!string.IsNullOrEmpty(PurchaseOrder.Notes)) PurchaseOrder.Notes = PurchaseOrder.Notes.ToUpper();

            db.Entry(PurchaseOrder).State = EntityState.Unchanged;
            db.Entry(PurchaseOrder).Property("Code").IsModified = true;
            db.Entry(PurchaseOrder).Property("Name").IsModified = true;
            db.Entry(PurchaseOrder).Property("Notes").IsModified = true;
            db.Entry(PurchaseOrder).Property("Active").IsModified = true;
            db.Entry(PurchaseOrder).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrder, MenuId = PurchaseOrder.Id, MenuCode = PurchaseOrder.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
                return PartialView("../Buying/PurchaseOrders/_Edit", PurchaseOrder);

            }
        }

        [HttpPost]
        [Authorize(Roles = "PurchaseOrdersDelete")]
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
                            PurchaseOrder obj = db.PurchaseOrders.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                PurchaseOrder tmp = obj;

                                db.PurchaseOrdersDetails.RemoveRange(db.PurchaseOrdersDetails.Where(x => x.PurchaseOrderId == obj.Id));
                                db.SaveChanges();

                                db.PurchaseOrders.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrder, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "PurchaseOrdersActive")]
        private PurchaseOrder GetModelState(PurchaseOrder purchase)
        {
            List<PurchaseOrderDetails> purchaseOrderDetails = db.PurchaseOrdersDetails.Where(x => x.PurchaseOrderId == purchase.Id).ToList();
            
            if (ModelState.IsValid)
            {
                if (IsAnyCode(purchase.Code, purchase.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (purchaseOrderDetails == null || purchaseOrderDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return purchase;
        }

        [Authorize(Roles = "PurchaseOrdersActive")]
        public ActionResult DetailsCreate(int purchaseOrderId)
        {
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(purchaseOrderId);

            if (purchaseOrder == null)
            {
                return HttpNotFound();
            }

            PurchaseOrderDetails purchaseOrderDetails = new PurchaseOrderDetails
            {
                PurchaseOrderId = purchaseOrderId
            };

            return PartialView("../Buying/PurchaseOrders/_DetailsCreate", purchaseOrderDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,PurchaseOrderId,MasterItemId,MasterItemUnitsId,Quantity,Price,Notes,Created,Updated,UserId")] PurchaseOrderDetails purchaseOrderDetails)
        {
            purchaseOrderDetails.Created = DateTime.Now;
            purchaseOrderDetails.Updated = DateTime.Now;
            purchaseOrderDetails.UserId = User.Identity.GetUserId<int>();
            purchaseOrderDetails.Total = purchaseOrderDetails.Quantity * purchaseOrderDetails.Price * purchaseOrderDetails.MasterItemUnit.MasterUnit.Ratio;

            if (!string.IsNullOrEmpty(purchaseOrderDetails.Notes)) purchaseOrderDetails.Notes = purchaseOrderDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.PurchaseOrdersDetails.Add(purchaseOrderDetails);
                        db.SaveChanges();

                        PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(purchaseOrderDetails.PurchaseOrderId);
                        purchaseOrder.Total = SharedFunctions.GetTotalPurchaseOrder(db, purchaseOrder.Id, purchaseOrderDetails.Id) + purchaseOrderDetails.Total;

                        db.Entry(purchaseOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrderDetails, MenuId = purchaseOrderDetails.Id, MenuCode = purchaseOrderDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Buying/PurchaseOrders/_DetailsCreate", purchaseOrderDetails);
        }

        [Authorize(Roles = "PurchaseOrdersActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PurchaseOrderDetails obj = db.PurchaseOrdersDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/PurchaseOrders/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,PurchaseOrderId,MasterItemId,MasterItemUnitsId,Quantity,Price,Notes,Created,Updated,UserId")] PurchaseOrderDetails purchaseOrderDetails)
        {
            purchaseOrderDetails.Updated = DateTime.Now;
            purchaseOrderDetails.UserId = User.Identity.GetUserId<int>();
            purchaseOrderDetails.Total = purchaseOrderDetails.Quantity * purchaseOrderDetails.Price * purchaseOrderDetails.MasterItemUnit.MasterUnit.Ratio;

            if (!string.IsNullOrEmpty(purchaseOrderDetails.Notes)) purchaseOrderDetails.Notes = purchaseOrderDetails.Notes.ToUpper();

            db.Entry(purchaseOrderDetails).State = EntityState.Unchanged;
            db.Entry(purchaseOrderDetails).Property("MasterItemId").IsModified = true;
            db.Entry(purchaseOrderDetails).Property("MasterItemUnitsId").IsModified = true;
            db.Entry(purchaseOrderDetails).Property("Quantity").IsModified = true;
            db.Entry(purchaseOrderDetails).Property("Price").IsModified = true;
            db.Entry(purchaseOrderDetails).Property("Total").IsModified = true;
            db.Entry(purchaseOrderDetails).Property("Notes").IsModified = true;
            db.Entry(purchaseOrderDetails).Property("Updated").IsModified = true;
            db.Entry(purchaseOrderDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(purchaseOrderDetails.PurchaseOrderId);
                        purchaseOrder.Total = SharedFunctions.GetTotalPurchaseOrder(db, purchaseOrder.Id, purchaseOrderDetails.Id) + purchaseOrderDetails.Total;

                        db.Entry(purchaseOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrderDetails, MenuId = purchaseOrderDetails.Id, MenuCode = purchaseOrderDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Buying/PurchaseOrders/_DetailsEdit", purchaseOrderDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersActive")]
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
                        PurchaseOrderDetails obj = db.PurchaseOrdersDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    PurchaseOrderDetails tmp = obj;

                                    PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(tmp.PurchaseOrderId);

                                    purchaseOrder.Total = SharedFunctions.GetTotalPurchaseOrder(db, purchaseOrder.Id, tmp.Id);

                                    db.Entry(purchaseOrder).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.PurchaseOrdersDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrderDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "PurchaseOrdersActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Buying/PurchaseOrders/_DetailsGrid", db.PurchaseOrdersDetails
                .Where(x => x.PurchaseOrderId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersActive")]
        public JsonResult GetMasterUnit(int id)
        {
            int masterItemUnitId = 0;
            MasterItem masterItem = db.MasterItems.Find(id);

            if(masterItem != null)
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
        [Authorize(Roles = "PurchaseOrdersActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(id);

            if (masterBusinessUnit != null && purchaseOrder != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                purchaseOrder.MasterBusinessUnitId = masterBusinessUnitId;
                purchaseOrder.MasterRegionId = masterRegionId;
                db.Entry(purchaseOrder).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "PurchaseOrdersActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.PurchaseOrderCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            PurchaseOrder lastData = db.PurchaseOrders
                .Where(x => (x.Code.Contains(code)))
                .OrderByDescending(z => z.Code).FirstOrDefault();

            if (lastData == null)
                code = "0001" + code;
            else
                code = (Convert.ToInt32(lastData.Code.Substring(0, 4)) + 1).ToString("D4") + code;

            return code;
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersActive")]
        public JsonResult GetTotal(int purchaseOrderId)
        {
            return Json(SharedFunctions.GetTotalPurchaseOrder(db, purchaseOrderId).ToString("N2"));
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
