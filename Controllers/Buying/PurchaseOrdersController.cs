using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using eShop.Extensions;
using eShop.Models;
using eShop.Properties;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
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

        [HttpGet]
        [Authorize(Roles = "PurchaseOrdersView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Buying/PurchaseOrders/_ViewGrid", db.PurchaseOrdersDetails
                .Where(x => x.PurchaseOrderId == Id).ToList());
        }

        // GET: PurchaseOrders/Create
        [Authorize(Roles = "PurchaseOrdersAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            PurchaseOrder purchaseOrder = new PurchaseOrder
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MasterCurrencyId = masterCurrency.Id,
                Rate = masterCurrency.Rate,
                MasterSupplierId = db.MasterSuppliers.FirstOrDefault().Id,
                MasterWarehouseId = db.MasterWarehouses.FirstOrDefault().Id,
                MasterCustomerId = db.MasterCustomers.FirstOrDefault().Id,
                //MasterSalesPersonId = db.MasterSalesPersons.FirstOrDefault().Id,
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

                    dbTran.Commit();

                    purchaseOrder.Code = "";
                    purchaseOrder.Active = true;
                    purchaseOrder.MasterBusinessUnitId = 0;
                    purchaseOrder.MasterRegionId = 0;
                    purchaseOrder.MasterSupplierId = 0;
                    purchaseOrder.MasterWarehouseId = 0;
                    purchaseOrder.MasterCustomerId = 0;
                   // purchaseOrder.MasterSalesPersonId = 0;
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            ViewBag.Total = "0";

            return View("../Buying/PurchaseOrders/Create", purchaseOrder);
        }

        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,PurchaseRequestId,MasterSupplierId,MasterWarehouseId,SalesOrderId,MasterCustomerId,Notes,Active,Created,Updated,UserId")] PurchaseOrder purchaseOrder)
        {
            purchaseOrder.Created = DateTime.Now;
            purchaseOrder.Updated = DateTime.Now;
            purchaseOrder.UserId = User.Identity.GetUserId<int>();
            purchaseOrder.Total = SharedFunctions.GetTotalPurchaseOrder(db, purchaseOrder.Id);
            purchaseOrder.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(purchaseOrder.Code)) purchaseOrder.Code = purchaseOrder.Code.ToUpper();
            if (!string.IsNullOrEmpty(purchaseOrder.Notes)) purchaseOrder.Notes = purchaseOrder.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                purchaseOrder = GetModelState(purchaseOrder);
            }

            db.Entry(purchaseOrder).State = EntityState.Unchanged;
            db.Entry(purchaseOrder).Property("Code").IsModified = true;
            db.Entry(purchaseOrder).Property("Date").IsModified = true;
            db.Entry(purchaseOrder).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(purchaseOrder).Property("MasterRegionId").IsModified = true;
            db.Entry(purchaseOrder).Property("PurchaseRequestId").IsModified = true;
            db.Entry(purchaseOrder).Property("MasterSupplierId").IsModified = true;
            db.Entry(purchaseOrder).Property("MasterWarehouseId").IsModified = true;
            db.Entry(purchaseOrder).Property("SalesOrderId").IsModified = true;
            db.Entry(purchaseOrder).Property("MasterCustomerId").IsModified = true;
            db.Entry(purchaseOrder).Property("Total").IsModified = true;
            db.Entry(purchaseOrder).Property("Notes").IsModified = true;
            db.Entry(purchaseOrder).Property("Active").IsModified = true;
            db.Entry(purchaseOrder).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrder, MenuId = purchaseOrder.Id, MenuCode = purchaseOrder.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", purchaseOrder.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalPurchaseOrder(db, purchaseOrder.Id).ToString("N2");

                return View("../Buying/PurchaseOrders/Create", purchaseOrder);
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
                                var details = db.PurchaseOrdersDetails.Where(x => x.PurchaseOrderId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PurchaseOrdersDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

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
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(id);
            if (purchaseOrder == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", purchaseOrder.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalPurchaseOrder(db, purchaseOrder.Id).ToString("N2");

            return View("../Buying/PurchaseOrders/Edit", purchaseOrder);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,PurchaseRequestId,MasterSupplierId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] PurchaseOrder purchaseOrder)
        {
            purchaseOrder.Updated = DateTime.Now;
            purchaseOrder.UserId = User.Identity.GetUserId<int>();
            purchaseOrder.Total = SharedFunctions.GetTotalPurchaseOrder(db, purchaseOrder.Id);
            purchaseOrder.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(purchaseOrder.Code)) purchaseOrder.Code = purchaseOrder.Code.ToUpper();
            if (!string.IsNullOrEmpty(purchaseOrder.Notes)) purchaseOrder.Notes = purchaseOrder.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                purchaseOrder = GetModelState(purchaseOrder);
            }

            db.Entry(purchaseOrder).State = EntityState.Unchanged;
            db.Entry(purchaseOrder).Property("Code").IsModified = true;
            db.Entry(purchaseOrder).Property("Date").IsModified = true;
            db.Entry(purchaseOrder).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(purchaseOrder).Property("MasterRegionId").IsModified = true;
            db.Entry(purchaseOrder).Property("MasterWarehouseId").IsModified = true;
            db.Entry(purchaseOrder).Property("Total").IsModified = true;
            db.Entry(purchaseOrder).Property("Notes").IsModified = true;
            db.Entry(purchaseOrder).Property("Active").IsModified = true;
            db.Entry(purchaseOrder).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrder, MenuId = purchaseOrder.Id, MenuCode = purchaseOrder.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", purchaseOrder.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalPurchaseOrder(db, purchaseOrder.Id).ToString("N2");

                return View("../Buying/PurchaseOrders/Edit", purchaseOrder);
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

                                var details = db.PurchaseOrdersDetails.Where(x => x.PurchaseOrderId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PurchaseOrdersDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.PurchaseOrders.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrder, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                                db.SaveChanges();
                            }
                        }

                        dbTran.Commit();

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

        [Authorize(Roles = "PurchaseOrdersPrint")]
        public ActionResult Print(int? id)
        {
            PurchaseOrder obj = db.PurchaseOrders.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }

            using (ReportDocument rd = new ReportDocument())
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormPurchaseOrder.rpt"));
                        rd.SetParameterValue("Code", obj.Code);
                        rd.SetParameterValue("Terbilang", "# " + TerbilangExtension.Terbilang(Math.Floor(obj.Total)).ToUpper() + " RUPIAH #");

                        string connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connString);
                        ConnectionInfo crConnectionInfo = new ConnectionInfo
                        {
                            ServerName = builder.DataSource,
                            DatabaseName = builder.InitialCatalog,
                            UserID = builder.UserID,
                            Password = builder.Password
                        };

                        TableLogOnInfos crTableLogonInfos = new TableLogOnInfos();
                        TableLogOnInfo crTableLogonInfo = new TableLogOnInfo();
                        Tables crTables = rd.Database.Tables;
                        foreach (Table crTable in crTables)
                        {
                            crTableLogonInfo = crTable.LogOnInfo;
                            crTableLogonInfo.ConnectionInfo = crConnectionInfo;
                            crTable.ApplyLogOnInfo(crTableLogonInfo);
                        }

                        obj.IsPrint = true;
                        db.Entry(obj).Property("IsPrint").IsModified = true;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Repayment, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.PRINT, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return new CrystalReportPdfResult(rd, "PO_" + obj.Code + ".pdf");
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
        public ActionResult DetailsCreate([Bind(Include = "Id,PurchaseOrderId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PurchaseOrderDetails purchaseOrderDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseOrderDetails.MasterItemUnitId);
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(purchaseOrderDetails.PurchaseOrderId);

            if (masterItemUnit == null)
                purchaseOrderDetails.Total = 0;
            else
                purchaseOrderDetails.Total = purchaseOrderDetails.Quantity * purchaseOrderDetails.Price * masterItemUnit.MasterUnit.Ratio * purchaseOrder.Rate;

            purchaseOrderDetails.Created = DateTime.Now;
            purchaseOrderDetails.Updated = DateTime.Now;
            purchaseOrderDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(purchaseOrderDetails.Notes)) purchaseOrderDetails.Notes = purchaseOrderDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.PurchaseOrdersDetails.Add(purchaseOrderDetails);
                        db.SaveChanges();

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
        public ActionResult DetailsEdit([Bind(Include = "Id,PurchaseOrderId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PurchaseOrderDetails purchaseOrderDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseOrderDetails.MasterItemUnitId);
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(purchaseOrderDetails.PurchaseOrderId);

            if (masterItemUnit == null)
                purchaseOrderDetails.Total = 0;
            else
                purchaseOrderDetails.Total = purchaseOrderDetails.Quantity * purchaseOrderDetails.Price * masterItemUnit.MasterUnit.Ratio * purchaseOrder.Rate;

            purchaseOrderDetails.Updated = DateTime.Now;
            purchaseOrderDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(purchaseOrderDetails.Notes)) purchaseOrderDetails.Notes = purchaseOrderDetails.Notes.ToUpper();

            db.Entry(purchaseOrderDetails).State = EntityState.Unchanged;
            db.Entry(purchaseOrderDetails).Property("MasterItemId").IsModified = true;
            db.Entry(purchaseOrderDetails).Property("MasterItemUnitId").IsModified = true;
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

        [Authorize(Roles = "PurchaseOrdersActive")]
        public ActionResult ChangeCurrency(int? purchaseOrderId)
        {
            if (purchaseOrderId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(purchaseOrderId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = purchaseOrder.Id,
                MasterCurrencyId = purchaseOrder.MasterCurrencyId,
                Rate = purchaseOrder.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Buying/PurchaseOrders/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersActive")]
        public ActionResult ChangeCurrency([Bind(Include = "Id,MasterCurrencyId,Rate")] ChangeCurrency changeCurrency)
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Find(changeCurrency.MasterCurrencyId);

            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(changeCurrency.Id);
            purchaseOrder.MasterCurrencyId = changeCurrency.MasterCurrencyId;
            purchaseOrder.Rate = changeCurrency.Rate;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var purchaseOrdersDetails = db.PurchaseOrdersDetails.Where(x => x.PurchaseOrderId == purchaseOrder.Id).ToList();

                        foreach (PurchaseOrderDetails purchaseOrderDetails in purchaseOrdersDetails)
                        {
                            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseOrderDetails.MasterItemUnitId);

                            if (masterItemUnit == null)
                                purchaseOrderDetails.Total = 0;
                            else
                                purchaseOrderDetails.Total = purchaseOrderDetails.Quantity * purchaseOrderDetails.Price * masterItemUnit.MasterUnit.Ratio * purchaseOrder.Rate;

                            db.Entry(purchaseOrderDetails).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        purchaseOrder.Total = SharedFunctions.GetTotalPurchaseOrder(db, purchaseOrder.Id);
                        db.Entry(purchaseOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        dbTran.Commit();

                        var returnObject = new
                        {
                            Status = "success",
                            Message = masterCurrency.Code + " : " + purchaseOrder.Rate.ToString("N2")
                        };

                        return Json(returnObject, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            return PartialView("../Buying/PurchaseOrders/_ChangeCurrency", changeCurrency);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersActive")]
        public JsonResult PopulateDetails(int purchaseOrderid, int purchaseRequestId)
        {
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(purchaseOrderid);
            PurchaseRequest purchaseRequest = db.PurchaseRequests.Find(purchaseRequestId);

            if (purchaseOrder != null && purchaseRequest != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var remove = db.PurchaseOrdersDetails.Where(x => x.PurchaseOrderId == purchaseOrder.Id).ToList();

                        if (remove != null)
                        {
                            db.PurchaseOrdersDetails.RemoveRange(remove);
                            db.SaveChanges();
                        }

                        var purchaseRequestsDetails = db.PurchaseRequestsDetails.Where(x => x.PurchaseRequestId == purchaseRequest.Id).ToList();

                        if (purchaseRequestsDetails != null)
                        {
                            foreach (PurchaseRequestDetails purchaseRequestDetails in purchaseRequestsDetails)
                            {
                                PurchaseOrderDetails purchaseOrderDetails = new PurchaseOrderDetails
                                {
                                    PurchaseOrderId = purchaseOrder.Id,
                                    MasterItemId = purchaseRequestDetails.MasterItemId,
                                    MasterItemUnitId = purchaseRequestDetails.MasterItemUnitId,
                                    Quantity = purchaseRequestDetails.Quantity,
                                    Price = purchaseRequestDetails.Price,
                                    Total = purchaseRequestDetails.Total,
                                    Notes = purchaseRequestDetails.Notes,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.PurchaseOrdersDetails.Add(purchaseOrderDetails);
                                db.SaveChanges();
                            }
                        }

                        purchaseOrder.PurchaseRequestId = purchaseRequest.Id;
                        purchaseOrder.MasterBusinessUnitId = purchaseRequest.MasterBusinessUnitId;
                        purchaseOrder.MasterRegionId = purchaseRequest.MasterRegionId;
                        purchaseOrder.MasterCurrencyId = purchaseRequest.MasterCurrencyId;
                        purchaseOrder.Rate = purchaseRequest.Rate;
                        purchaseOrder.MasterSupplierId = purchaseRequest.MasterSupplierId;
                        purchaseOrder.MasterWarehouseId = purchaseRequest.MasterWarehouseId;
                        purchaseOrder.Notes = purchaseRequest.Notes;
                        purchaseOrder.Total = purchaseRequest.Total;

                        db.Entry(purchaseOrder).State = EntityState.Modified;
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

            return Json(new
            {
                purchaseOrder.MasterRegionId,
                purchaseOrder.MasterBusinessUnitId,
                purchaseOrder.MasterSupplierId,
                purchaseOrder.MasterWarehouseId,
                purchaseOrder.Notes,
                Total = purchaseOrder.Total.ToString("N2"),
                purchaseOrder.Date,
                Currency = purchaseOrder.MasterCurrency.Code + " : " + purchaseOrder.Rate.ToString("N2")
            });
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersActive")]
        public JsonResult PopulateSalesOrderDetails(int purchaseOrderid, int salesOrderId)
        {
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(purchaseOrderid);
            SalesOrder salesOrder = db.SalesOrders.Find(salesOrderId);

            if (purchaseOrder != null && salesOrder != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var remove = db.PurchaseOrdersDetails.Where(x => x.PurchaseOrderId == purchaseOrder.Id).ToList();

                        if (remove != null)
                        {
                            db.PurchaseOrdersDetails.RemoveRange(remove);
                            db.SaveChanges();
                        }

                        var salesOrdersDetails = db.SalesOrdersDetails.Where(x => x.SalesOrderId == salesOrder.Id).ToList();

                        if (salesOrdersDetails != null)
                        {
                            foreach (SalesOrderDetails salesOrderDetails in salesOrdersDetails)
                            {
                                PurchaseOrderDetails purchaseOrderDetails = new PurchaseOrderDetails
                                {
                                    PurchaseOrderId = purchaseOrder.Id,
                                    MasterItemId = salesOrderDetails.MasterItemId,
                                    MasterItemUnitId = salesOrderDetails.MasterItemUnitId,
                                    Quantity = salesOrderDetails.Quantity,
                                    Price = salesOrderDetails.Price,
                                    Total = salesOrderDetails.Total,
                                    Notes = salesOrderDetails.Notes,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.PurchaseOrdersDetails.Add(purchaseOrderDetails);
                                db.SaveChanges();
                            }
                        }

                        purchaseOrder.SalesOrderId = salesOrder.Id;
                        purchaseOrder.SalesOrderId = salesOrder.MasterBusinessUnitId;
                        purchaseOrder.SalesOrderId = salesOrder.MasterRegionId;
                        purchaseOrder.SalesOrderId = salesOrder.MasterCurrencyId;
                        purchaseOrder.Rate = salesOrder.Rate;
                        purchaseOrder.MasterCustomerId = salesOrder.MasterCustomerId;
                        purchaseOrder.MasterWarehouseId = salesOrder.MasterWarehouseId;
                        purchaseOrder.Notes = salesOrder.Notes;
                        purchaseOrder.Total = salesOrder.Total;

                        db.Entry(purchaseOrder).State = EntityState.Modified;
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

            return Json(new
            {
                purchaseOrder.MasterRegionId,
                purchaseOrder.MasterBusinessUnitId,
                purchaseOrder.MasterCustomerId,
                purchaseOrder.MasterWarehouseId,
                purchaseOrder.Notes,
                Total = purchaseOrder.Total.ToString("N2"),
                purchaseOrder.Date,
                Currency = purchaseOrder.MasterCurrency.Code + " : " + purchaseOrder.Rate.ToString("N2")
            });
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
