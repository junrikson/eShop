﻿using CrystalDecisions.CrystalReports.Engine;
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
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web.Mvc;


namespace eShop.Controllers
{
    public class PurchasesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Purchases
        [Authorize(Roles = "PurchasesActive")]
        public ActionResult Index()
        {
            return View("../Buying/Purchases/Index");
        }

        [HttpGet]
        [Authorize(Roles = "PurchasesActive")]
        public PartialViewResult IndexGrid(string search)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            var masterRegions = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterRegionId).Distinct().ToList();
            var masterBusinessUnits = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnitId).Distinct().ToList();

            if (string.IsNullOrEmpty(search))
            {
                return PartialView("../Buying/Purchases/_IndexGrid", db.Set<Purchase>().Where(x =>
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable());
            }
            else
            {
                return PartialView("../Buying/Purchases/_IndexGrid", db.Set<Purchase>().Where(x =>
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
                return db.Purchases.Any(x => x.Code == Code);
            }
            else
            {
                return db.Purchases.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: Purchases/Details/
        [Authorize(Roles = "PurchasesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchase Purchase = db.Purchases.Find(id);
            if (Purchase == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/Purchases/_Details", Purchase);
        }

        [HttpGet]
        [Authorize(Roles = "PurchasesView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Buying/Purchases/_ViewGrid", db.PurchasesDetails
                .Where(x => x.PurchaseId == Id).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "PurchasesView")]
        public PartialViewResult ViewCostGrid(int Id)
        {
            return PartialView("../Buying/Purchases/_ViewGrid", db.PurchasesCostsDetails
                .Where(x => x.PurchaseId == Id).ToList());
        }

        // GET: Purchases/Create
        [Authorize(Roles = "PurchasesAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            Purchase purchase = new Purchase
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MasterCurrencyId = masterCurrency.Id,
                Rate = masterCurrency.Rate,
                MasterSupplierId = db.MasterSuppliers.FirstOrDefault().Id,
                MasterWarehouseId = db.MasterWarehouses.FirstOrDefault().Id,
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
                    db.Purchases.Add(purchase);
                    db.SaveChanges();

                    SharedFunctions.CreatePurchaseJournal(db, purchase);

                    dbTran.Commit();

                    purchase.Code = "";
                    purchase.Active = true;
                    purchase.MasterBusinessUnitId = 0;
                    purchase.MasterRegionId = 0;
                    purchase.MasterSupplierId = 0;
                    purchase.MasterWarehouseId = 0;
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

            return View("../Buying/Purchases/Create", purchase);
        }

        // POST: Purchases/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchasesAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterCurrencyId,MasterWarehouseId,PurchaseOrderId,MasterSupplierId,Rate,MasterCurrencyId,Notes,Active,Created,Updated,UserId")] Purchase purchase)
        {
            purchase.Created = DateTime.Now;
            purchase.Updated = DateTime.Now;
            purchase.UserId = User.Identity.GetUserId<int>();
            purchase.Total = SharedFunctions.GetTotalPurchase(db, purchase.Id);
            purchase.MasterCurrency = db.MasterCurrencies.Find(purchase.MasterCurrencyId);

            if (!string.IsNullOrEmpty(purchase.Code)) purchase.Code = purchase.Code.ToUpper();
            if (!string.IsNullOrEmpty(purchase.Notes)) purchase.Notes = purchase.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                purchase = GetModelState(purchase);
            }

            db.Entry(purchase).State = EntityState.Unchanged;
            db.Entry(purchase).Property("Code").IsModified = true;
            db.Entry(purchase).Property("Date").IsModified = true;
            db.Entry(purchase).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(purchase).Property("MasterRegionId").IsModified = true;
            db.Entry(purchase).Property("PurchaseOrderId").IsModified = true;
            db.Entry(purchase).Property("MasterSupplierId").IsModified = true;
            db.Entry(purchase).Property("MasterWarehouseId").IsModified = true;
            db.Entry(purchase).Property("Total").IsModified = true;
            db.Entry(purchase).Property("Notes").IsModified = true;
            db.Entry(purchase).Property("Active").IsModified = true;
            db.Entry(purchase).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.Entry(purchase).Reload();

                        SharedFunctions.UpdatePurchaseJournal(db, purchase);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Purchase, MenuId = purchase.Id, MenuCode = purchase.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", purchase.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalPurchase(db, purchase.Id).ToString("N2");

                return View("../Buying/Purchases/Create", purchase);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchasesAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                Purchase obj = db.Purchases.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                SharedFunctions.DeletePurchaseJournals(db, obj);

                                var details = db.PurchasesDetails.Where(x => x.PurchaseId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PurchasesDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.Purchases.Remove(obj);
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

        // GET: Purchases/Edit/5
        [Authorize(Roles = "PurchasesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchase purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", purchase.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalPurchase(db, purchase.Id).ToString("N2");

            return View("../Buying/Purchases/Edit", purchase);
        }

        // POST: Purchases/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchasesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterCurrencyId,MasterWarehouseId,PurchaseOrderId,MasterSupplierId,Notes,Active,Created,Updated,UserId")] Purchase purchase)
        {
            purchase.Updated = DateTime.Now;
            purchase.UserId = User.Identity.GetUserId<int>();
            purchase.Total = SharedFunctions.GetTotalPurchase(db, purchase.Id);
            purchase.MasterCurrency = db.MasterCurrencies.Find(purchase.MasterCurrencyId);

            if (!string.IsNullOrEmpty(purchase.Code)) purchase.Code = purchase.Code.ToUpper();
            if (!string.IsNullOrEmpty(purchase.Notes)) purchase.Notes = purchase.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                purchase = GetModelState(purchase);
            }

            db.Entry(purchase).State = EntityState.Unchanged;
            db.Entry(purchase).Property("Code").IsModified = true;
            db.Entry(purchase).Property("Date").IsModified = true;
            db.Entry(purchase).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(purchase).Property("MasterRegionId").IsModified = true;
            db.Entry(purchase).Property("MasterSupplierId").IsModified = true;
            db.Entry(purchase).Property("MasterWarehouseId").IsModified = true;
            db.Entry(purchase).Property("Total").IsModified = true;
            db.Entry(purchase).Property("Notes").IsModified = true;
            db.Entry(purchase).Property("Active").IsModified = true;
            db.Entry(purchase).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.Entry(purchase).Reload();

                        SharedFunctions.UpdatePurchaseJournal(db, purchase);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Purchase, MenuId = purchase.Id, MenuCode = purchase.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", purchase.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalPurchase(db, purchase.Id).ToString("N2");

                return View("../Buying/Purchases/Edit", purchase);
            }
        }

        [HttpPost]
        [Authorize(Roles = "PurchasesDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDelete(int[] ids)
        {
            if (ids == null || ids.Length <= 0)
                return Json("Pilih salah satu data yang akan dihapus.");
            else
            {
                int failed = 0;
                foreach (int id in ids)
                {
                    Purchase obj = db.Purchases.Find(id);

                    if (obj == null)
                        failed++;
                    else
                    {
                        Purchase tmp = obj;

                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                SharedFunctions.DeletePurchaseJournals(db, obj);

                                var details = db.PurchasesDetails.Where(x => x.PurchaseId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PurchasesDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.Purchases.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Purchase, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "PurchasesPrint")]
        public ActionResult Print(int? id)
        {
            Purchase obj = db.Purchases.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormPurchase.rpt"));
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

        [Authorize(Roles = "PurchasesActive")]
        private Purchase GetModelState(Purchase purchase)
        {
            List<PurchaseDetails> purchaseDetails = db.PurchasesDetails.Where(x => x.PurchaseId == purchase.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(purchase.Code, purchase.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (purchaseDetails == null || purchaseDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return purchase;
        }

        [Authorize(Roles = "PurchasesActive")]
        public ActionResult DetailsCreate(int purchaseId)
        {
            Purchase purchase = db.Purchases.Find(purchaseId);

            if (purchase == null)
            {
                return HttpNotFound();
            }

            PurchaseDetails purchaseDetails = new PurchaseDetails
            {
                PurchaseId = purchaseId
            };

            return PartialView("../Buying/Purchases/_DetailsCreate", purchaseDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchasesActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,PurchaseId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PurchaseDetails purchaseDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseDetails.MasterItemUnitId);
            Purchase purchase = db.Purchases.Find(purchaseDetails.PurchaseId);

            if (masterItemUnit == null)
                purchaseDetails.Total = 0;
            else
                purchaseDetails.Total = purchaseDetails.Quantity * purchaseDetails.Price * masterItemUnit.MasterUnit.Ratio * purchase.Rate;

            purchaseDetails.Created = DateTime.Now;
            purchaseDetails.Updated = DateTime.Now;
            purchaseDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(purchaseDetails.Notes)) purchaseDetails.Notes = purchaseDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.PurchasesDetails.Add(purchaseDetails);
                        db.SaveChanges();

                        SharedFunctions.CreatePurchaseJournalDetails(db, purchaseDetails);

                        purchase.Total = SharedFunctions.GetTotalPurchase(db, purchase.Id, purchaseDetails.Id) + purchaseDetails.Total;

                        db.Entry(purchase).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseDetails, MenuId = purchaseDetails.Id, MenuCode = purchaseDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Buying/Purchases/_DetailsCreate", purchaseDetails);
        }

        [Authorize(Roles = "PurchasesActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PurchaseDetails obj = db.PurchasesDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/Purchases/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchasesActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,PurchaseId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PurchaseDetails purchaseDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseDetails.MasterItemUnitId);
            Purchase purchase = db.Purchases.Find(purchaseDetails.PurchaseId);

            if (masterItemUnit == null)
                purchaseDetails.Total = 0;
            else
                purchaseDetails.Total = purchaseDetails.Quantity * purchaseDetails.Price * masterItemUnit.MasterUnit.Ratio * purchase.Rate;

            purchaseDetails.Updated = DateTime.Now;
            purchaseDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(purchaseDetails.Notes)) purchaseDetails.Notes = purchaseDetails.Notes.ToUpper();

            db.Entry(purchaseDetails).State = EntityState.Unchanged;
            db.Entry(purchaseDetails).Property("MasterItemId").IsModified = true;
            db.Entry(purchaseDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(purchaseDetails).Property("Quantity").IsModified = true;
            db.Entry(purchaseDetails).Property("Price").IsModified = true;
            db.Entry(purchaseDetails).Property("Total").IsModified = true;
            db.Entry(purchaseDetails).Property("Notes").IsModified = true;
            db.Entry(purchaseDetails).Property("Updated").IsModified = true;
            db.Entry(purchaseDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();
                        db.Entry(purchaseDetails).Reload();

                        SharedFunctions.UpdatePurchaseJournalDetails(db, purchaseDetails);

                        purchase.Total = SharedFunctions.GetTotalPurchase(db, purchase.Id, purchaseDetails.Id) + purchaseDetails.Total;

                        db.Entry(purchase).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseDetails, MenuId = purchaseDetails.Id, MenuCode = purchaseDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Buying/Purchases/_DetailsEdit", purchaseDetails);
        }

        [Authorize(Roles = "PurchasesActive")]
        public ActionResult CostsCreate(int purchaseId)
        {
            Purchase purchase = db.Purchases.Find(purchaseId);

            if (purchase == null)
            {
                return HttpNotFound();
            }

            PurchaseCostDetails purchaseCostDetails = new PurchaseCostDetails
            {
                PurchaseId = purchaseId
            };

            return PartialView("../Buying/Purchases/_CostsCreate", purchaseCostDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchasesActive")]
        public ActionResult CostsCreate([Bind(Include = "Id,PurchaseId,MasterCostId,Total,Notes,Created,Updated,UserId")] PurchaseCostDetails purchaseCostDetails)
        {
            purchaseCostDetails.Created = DateTime.Now;
            purchaseCostDetails.Updated = DateTime.Now;
            purchaseCostDetails.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(purchaseCostDetails.Notes)) purchaseCostDetails.Notes = purchaseCostDetails.Notes.ToUpper();

                Purchase purchase = db.Purchases.Find(purchaseCostDetails.PurchaseId);
                // bankTransaction.Total = SharedFunctions.GetTotalBankTransactionDetails(db, bankTransaction.Id) + bankTransactionDetails.Total;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.PurchasesCostsDetails.Add(purchaseCostDetails);
                        db.SaveChanges();

                        // SharedFunctions.CreateBankJournalDetails(db, bankTransactionDetails);

                        db.Entry(purchaseCostDetails).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseCostDetails, MenuId = purchaseCostDetails.Id, MenuCode = purchaseCostDetails.MasterCostId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            return PartialView("../Buying/Purchases/_CostsCreate", purchaseCostDetails);
        }

        [Authorize(Roles = "PurchasesActive")]
        public ActionResult CostsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PurchaseCostDetails obj = db.PurchasesCostsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/Purchases/_CostsEdit", obj);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchasesActive")]
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
                        PurchaseDetails obj = db.PurchasesDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    PurchaseDetails tmp = obj;

                                    SharedFunctions.RemovePurchaseJournalDetails(db, obj);

                                    Purchase purchase = db.Purchases.Find(tmp.PurchaseId);

                                    purchase.Total = SharedFunctions.GetTotalPurchase(db, purchase.Id, tmp.Id);

                                    db.Entry(purchase).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.PurchasesDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "PurchasesActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Buying/Purchases/_DetailsGrid", db.PurchasesDetails
                .Where(x => x.PurchaseId == Id).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "PurchasesActive")]
        public PartialViewResult CostsGrid(int Id)
        {
            return PartialView("../Buying/Purchases/_CostsGrid", db.PurchasesCostsDetails
                .Where(x => x.PurchaseId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchasesActive")]
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

        [Authorize(Roles = "PurchasesActive")]
        public ActionResult ChangeCurrency(int? purchaseId)
        {
            if (purchaseId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Purchase purchase = db.Purchases.Find(purchaseId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = purchase.Id,
                MasterCurrencyId = purchase.MasterCurrencyId,
                Rate = purchase.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Buying/Purchases/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchasesActive")]
        public ActionResult ChangeCurrency([Bind(Include = "Id,MasterCurrencyId,Rate")] ChangeCurrency changeCurrency)
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Find(changeCurrency.MasterCurrencyId);

            Purchase purchase = db.Purchases.Find(changeCurrency.Id);
            purchase.MasterCurrencyId = changeCurrency.MasterCurrencyId;
            purchase.Rate = changeCurrency.Rate;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var purchasesDetails = db.PurchasesDetails.Where(x => x.PurchaseId == purchase.Id).ToList();

                        foreach (PurchaseDetails purchaseDetails in purchasesDetails)
                        {
                            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseDetails.MasterItemUnitId);

                            if (masterItemUnit == null)
                                purchaseDetails.Total = 0;
                            else
                                purchaseDetails.Total = purchaseDetails.Quantity * purchaseDetails.Price * masterItemUnit.MasterUnit.Ratio * purchase.Rate;

                            db.Entry(purchaseDetails).State = EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(purchaseDetails).Reload();

                            SharedFunctions.UpdatePurchaseJournalDetails(db, purchaseDetails);
                        }

                        purchase.Total = SharedFunctions.GetTotalPurchase(db, purchase.Id);
                        db.Entry(purchase).State = EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(purchase).Reload();

                        SharedFunctions.UpdatePurchaseJournal(db, purchase);

                        dbTran.Commit();

                        var returnObject = new
                        {
                            Status = "success",
                            Message = masterCurrency.Code + " : " + purchase.Rate.ToString("N2")
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

            return PartialView("../Buying/Purchases/_ChangeCurrency", changeCurrency);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchasesActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchasesActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            Purchase purchase = db.Purchases.Find(id);

            if (masterBusinessUnit != null && purchase != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                purchase.MasterBusinessUnitId = masterBusinessUnitId;
                purchase.MasterRegionId = masterRegionId;
                db.Entry(purchase).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "PurchasesActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.PurchaseCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            Purchase lastData = db.Purchases
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
        [Authorize(Roles = "PurchasesActive")]
        public JsonResult GetPrice(int masterBusinessUnitId, int masterRegionId, int masterItemId, int masterItemUnitId, int masterCustomerId)
        {
            return Json(SharedFunctions.GetSellingPrice(db, masterBusinessUnitId, masterRegionId, masterItemId, masterItemUnitId, masterCustomerId));
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchasesActive")]
        public JsonResult GetTotal(int purchaseId)
        {
            return Json(SharedFunctions.GetTotalPurchase(db, purchaseId).ToString("N2"));
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchasesActive")]
        public JsonResult PopulateDetails(int purchaseid, int purchaseOrderid)
        {
            Purchase purchase = db.Purchases.Find(purchaseid);
            PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(purchaseOrderid);

            if (purchase != null && purchaseOrder != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var remove = db.PurchasesDetails.Where(x => x.PurchaseId == purchase.Id).ToList();

                        if (remove != null)
                        {
                            foreach(PurchaseDetails temp in remove)
                            {
                                SharedFunctions.RemovePurchaseJournalDetails(db, temp);

                                db.PurchasesDetails.Remove(temp);
                                db.SaveChanges();
                            }
                        }

                        var purchaseOrdersDetails = db.PurchaseOrdersDetails.Where(x => x.PurchaseOrderId == purchaseOrder.Id).ToList();

                        if (purchaseOrdersDetails != null)
                        {
                            foreach (PurchaseOrderDetails purchaseOrderDetails in purchaseOrdersDetails)
                            {
                                PurchaseDetails purchaseDetails = new PurchaseDetails
                                {
                                    PurchaseId = purchase.Id,
                                    MasterItemId = purchaseOrderDetails.MasterItemId,
                                    MasterItemUnitId = purchaseOrderDetails.MasterItemUnitId,
                                    Quantity = purchaseOrderDetails.Quantity,
                                    Price = purchaseOrderDetails.Price,
                                    Total = purchaseOrderDetails.Total,
                                    Notes = purchaseOrderDetails.Notes,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.PurchasesDetails.Add(purchaseDetails);
                                db.SaveChanges();

                                SharedFunctions.CreatePurchaseJournalDetails(db, purchaseDetails);
                            }
                        }

                        purchase.PurchaseOrderId = purchaseOrder.Id;
                        purchase.MasterBusinessUnitId = purchaseOrder.MasterBusinessUnitId;
                        purchase.MasterRegionId = purchaseOrder.MasterRegionId;
                        purchase.MasterCurrencyId = purchaseOrder.MasterCurrencyId;
                        purchase.Rate = purchaseOrder.Rate;
                        purchase.MasterSupplierId = purchaseOrder.MasterSupplierId;
                        purchase.MasterWarehouseId = purchaseOrder.MasterWarehouseId;
                        purchase.Notes = purchaseOrder.Notes;
                        purchase.Total = purchaseOrder.Total;

                        db.Entry(purchase).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(purchase).Reload();

                        SharedFunctions.UpdatePurchaseJournal(db, purchase);

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
                purchase.MasterRegionId,
                purchase.MasterBusinessUnitId,
                purchase.MasterSupplierId,
                purchase.MasterWarehouseId,
                purchase.Notes,
                Total = purchase.Total.ToString("N2"),
                purchase.Date,
                Currency = purchase.MasterCurrency.Code + " : " + purchase.Rate.ToString("N2")
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
