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
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;


namespace eShop.Controllers
{
    public class PurchaseReturnsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PurchaseReturns
        [Authorize(Roles = "PurchaseReturnsActive")]
        public ActionResult Index()
        {
            return View("../Buying/PurchaseReturns/Index");
        }

        [HttpGet]
        [Authorize(Roles = "PurchaseReturnsActive")]
        public PartialViewResult IndexGrid(string search)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            var masterRegions = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterRegionId).Distinct().ToList();
            var masterBusinessUnits = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnitId).Distinct().ToList();

            if (string.IsNullOrEmpty(search))
            {
                return PartialView("../Buying/PurchaseReturns/_IndexGrid", db.Set<PurchaseReturn>().Where(x =>
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable());
            }
            else
            {
                return PartialView("../Buying/PurchaseReturns/_IndexGrid", db.Set<PurchaseReturn>().Where(x =>
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
                return db.PurchaseReturns.Any(x => x.Code == Code);
            }
            else
            {
                return db.PurchaseReturns.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: PurchaseReturns/Details/
        [Authorize(Roles = "PurchaseReturnsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseReturn PurchaseReturn = db.PurchaseReturns.Find(id);
            if (PurchaseReturn == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/PurchaseReturns/_Details", PurchaseReturn);
        }

        [HttpGet]
        [Authorize(Roles = "PurchaseReturnsView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Buying/PurchaseReturns/_ViewGrid", db.PurchaseReturnsDetails
                .Where(x => x.PurchaseReturnId == Id).ToList());
        }

        // GET: PurchaseReturns/Create
        [Authorize(Roles = "PurchaseReturnsAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            PurchaseReturn purchaseReturn = new PurchaseReturn
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
                    db.PurchaseReturns.Add(purchaseReturn);
                    db.SaveChanges();

                    dbTran.Commit();

                    purchaseReturn.Code = "";
                    purchaseReturn.Active = true;
                    purchaseReturn.MasterBusinessUnitId = 0;
                    purchaseReturn.MasterRegionId = 0;
                    purchaseReturn.MasterSupplierId = 0;
                    purchaseReturn.MasterWarehouseId = 0;
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

            return View("../Buying/PurchaseReturns/Create", purchaseReturn);
        }

        // POST: PurchaseReturns/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseReturnsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,PurchaseId,MasterSupplierId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] PurchaseReturn purchaseReturn)
        {
            purchaseReturn.Created = DateTime.Now;
            purchaseReturn.Updated = DateTime.Now;
            purchaseReturn.UserId = User.Identity.GetUserId<int>();
            purchaseReturn.Total = SharedFunctions.GetTotalPurchaseReturn(db, purchaseReturn.Id);
            purchaseReturn.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(purchaseReturn.Code)) purchaseReturn.Code = purchaseReturn.Code.ToUpper();
            if (!string.IsNullOrEmpty(purchaseReturn.Notes)) purchaseReturn.Notes = purchaseReturn.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                purchaseReturn = GetModelState(purchaseReturn);
            }

            db.Entry(purchaseReturn).State = EntityState.Unchanged;
            db.Entry(purchaseReturn).Property("Code").IsModified = true;
            db.Entry(purchaseReturn).Property("Date").IsModified = true;
            db.Entry(purchaseReturn).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(purchaseReturn).Property("MasterRegionId").IsModified = true;
            db.Entry(purchaseReturn).Property("PurchaseId").IsModified = true;
            db.Entry(purchaseReturn).Property("MasterSupplierId").IsModified = true;
            db.Entry(purchaseReturn).Property("MasterWarehouseId").IsModified = true;
            db.Entry(purchaseReturn).Property("Total").IsModified = true;
            db.Entry(purchaseReturn).Property("Notes").IsModified = true;
            db.Entry(purchaseReturn).Property("Active").IsModified = true;
            db.Entry(purchaseReturn).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseReturn, MenuId = purchaseReturn.Id, MenuCode = purchaseReturn.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", purchaseReturn.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalPurchaseReturn(db, purchaseReturn.Id).ToString("N2");

                return View("../Buying/PurchaseReturns/Create", purchaseReturn);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseReturnsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                PurchaseReturn obj = db.PurchaseReturns.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.PurchaseReturnsDetails.Where(x => x.PurchaseReturnId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PurchaseReturnsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.PurchaseReturns.Remove(obj);
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

        // GET: PurchaseReturns/Edit/5
        [Authorize(Roles = "PurchaseReturnsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseReturn purchaseReturn = db.PurchaseReturns.Find(id);
            if (purchaseReturn == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", purchaseReturn.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalPurchaseReturn(db, purchaseReturn.Id).ToString("N2");

            return View("../Buying/PurchaseReturns/Edit", purchaseReturn);
        }

        // POST: PurchaseReturns/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseReturnsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,PurchaseId,MasterSupplierId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] PurchaseReturn purchaseReturn)
        {
            purchaseReturn.Updated = DateTime.Now;
            purchaseReturn.UserId = User.Identity.GetUserId<int>();
            purchaseReturn.Total = SharedFunctions.GetTotalPurchaseReturn(db, purchaseReturn.Id);
            purchaseReturn.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(purchaseReturn.Code)) purchaseReturn.Code = purchaseReturn.Code.ToUpper();
            if (!string.IsNullOrEmpty(purchaseReturn.Notes)) purchaseReturn.Notes = purchaseReturn.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                purchaseReturn = GetModelState(purchaseReturn);
            }

            db.Entry(purchaseReturn).State = EntityState.Unchanged;
            db.Entry(purchaseReturn).Property("Code").IsModified = true;
            db.Entry(purchaseReturn).Property("Date").IsModified = true;
            db.Entry(purchaseReturn).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(purchaseReturn).Property("MasterRegionId").IsModified = true;
            db.Entry(purchaseReturn).Property("PurchaseId").IsModified = true;
            db.Entry(purchaseReturn).Property("MasterWarehouseId").IsModified = true;
            db.Entry(purchaseReturn).Property("Total").IsModified = true;
            db.Entry(purchaseReturn).Property("Notes").IsModified = true;
            db.Entry(purchaseReturn).Property("Active").IsModified = true;
            db.Entry(purchaseReturn).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseReturn, MenuId = purchaseReturn.Id, MenuCode = purchaseReturn.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", purchaseReturn.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalPurchaseReturn(db, purchaseReturn.Id).ToString("N2");

                return View("../Buying/PurchaseReturns/Edit", purchaseReturn);
            }
        }

        [HttpPost]
        [Authorize(Roles = "PurchaseReturnsDelete")]
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
                            PurchaseReturn obj = db.PurchaseReturns.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                PurchaseReturn tmp = obj;

                                var details = db.PurchaseReturnsDetails.Where(x => x.PurchaseReturnId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PurchaseReturnsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.PurchaseReturns.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseReturn, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "PurchaseReturnsPrint")]
        public ActionResult Print(int? id)
        {
            PurchaseReturn obj = db.PurchaseReturns.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormPurchaseReturn.rpt"));
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

        [Authorize(Roles = "PurchaseReturnsActive")]
        private PurchaseReturn GetModelState(PurchaseReturn purchase)
        {
            List<PurchaseReturnDetails> purchaseReturnDetails = db.PurchaseReturnsDetails.Where(x => x.PurchaseReturnId == purchase.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(purchase.Code, purchase.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (purchaseReturnDetails == null || purchaseReturnDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return purchase;
        }

        [Authorize(Roles = "PurchaseReturnsActive")]
        public ActionResult DetailsCreate(int purchaseReturnId)
        {
            PurchaseReturn purchaseReturn = db.PurchaseReturns.Find(purchaseReturnId);

            if (purchaseReturn == null)
            {
                return HttpNotFound();
            }

            PurchaseReturnDetails purchaseReturnDetails = new PurchaseReturnDetails
            {
                PurchaseReturnId = purchaseReturnId
            };

            return PartialView("../Buying/PurchaseReturns/_DetailsCreate", purchaseReturnDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseReturnsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,PurchaseReturnId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PurchaseReturnDetails purchaseReturnDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseReturnDetails.MasterItemUnitId);
            PurchaseReturn purchaseReturn = db.PurchaseReturns.Find(purchaseReturnDetails.PurchaseReturnId);

            if (masterItemUnit == null)
                purchaseReturnDetails.Total = 0;
            else
                purchaseReturnDetails.Total = purchaseReturnDetails.Quantity * purchaseReturnDetails.Price * masterItemUnit.MasterUnit.Ratio * purchaseReturn.Rate;

            purchaseReturnDetails.Created = DateTime.Now;
            purchaseReturnDetails.Updated = DateTime.Now;
            purchaseReturnDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(purchaseReturnDetails.Notes)) purchaseReturnDetails.Notes = purchaseReturnDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.PurchaseReturnsDetails.Add(purchaseReturnDetails);
                        db.SaveChanges();

                        purchaseReturn.Total = SharedFunctions.GetTotalPurchaseReturn(db, purchaseReturn.Id, purchaseReturnDetails.Id) + purchaseReturnDetails.Total;

                        db.Entry(purchaseReturn).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseReturnDetails, MenuId = purchaseReturnDetails.Id, MenuCode = purchaseReturnDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Buying/PurchaseReturns/_DetailsCreate", purchaseReturnDetails);
        }

        [Authorize(Roles = "PurchaseReturnsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PurchaseReturnDetails obj = db.PurchaseReturnsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/PurchaseReturns/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseReturnsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,PurchaseReturnId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PurchaseReturnDetails purchaseReturnDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseReturnDetails.MasterItemUnitId);
            PurchaseReturn purchaseReturn = db.PurchaseReturns.Find(purchaseReturnDetails.PurchaseReturnId);

            if (masterItemUnit == null)
                purchaseReturnDetails.Total = 0;
            else
                purchaseReturnDetails.Total = purchaseReturnDetails.Quantity * purchaseReturnDetails.Price * masterItemUnit.MasterUnit.Ratio * purchaseReturn.Rate;

            purchaseReturnDetails.Updated = DateTime.Now;
            purchaseReturnDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(purchaseReturnDetails.Notes)) purchaseReturnDetails.Notes = purchaseReturnDetails.Notes.ToUpper();

            db.Entry(purchaseReturnDetails).State = EntityState.Unchanged;
            db.Entry(purchaseReturnDetails).Property("MasterItemId").IsModified = true;
            db.Entry(purchaseReturnDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(purchaseReturnDetails).Property("Quantity").IsModified = true;
            db.Entry(purchaseReturnDetails).Property("Price").IsModified = true;
            db.Entry(purchaseReturnDetails).Property("Total").IsModified = true;
            db.Entry(purchaseReturnDetails).Property("Notes").IsModified = true;
            db.Entry(purchaseReturnDetails).Property("Updated").IsModified = true;
            db.Entry(purchaseReturnDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        purchaseReturn.Total = SharedFunctions.GetTotalPurchaseReturn(db, purchaseReturn.Id, purchaseReturnDetails.Id) + purchaseReturnDetails.Total;

                        db.Entry(purchaseReturn).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseReturnDetails, MenuId = purchaseReturnDetails.Id, MenuCode = purchaseReturnDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Buying/PurchaseReturns/_DetailsEdit", purchaseReturnDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseReturnsActive")]
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
                        PurchaseReturnDetails obj = db.PurchaseReturnsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    PurchaseReturnDetails tmp = obj;

                                    PurchaseReturn purchaseReturn = db.PurchaseReturns.Find(tmp.PurchaseReturnId);

                                    purchaseReturn.Total = SharedFunctions.GetTotalPurchaseReturn(db, purchaseReturn.Id, tmp.Id);

                                    db.Entry(purchaseReturn).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.PurchaseReturnsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseReturnDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "PurchaseReturnsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Buying/PurchaseReturns/_DetailsGrid", db.PurchaseReturnsDetails
                .Where(x => x.PurchaseReturnId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseReturnsActive")]
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

        [Authorize(Roles = "PurchaseReturnsActive")]
        public ActionResult ChangeCurrency(int? purchaseReturnId)
        {
            if (purchaseReturnId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PurchaseReturn purchaseReturn = db.PurchaseReturns.Find(purchaseReturnId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = purchaseReturn.Id,
                MasterCurrencyId = purchaseReturn.MasterCurrencyId,
                Rate = purchaseReturn.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Buying/PurchaseReturns/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseReturnsActive")]
        public ActionResult ChangeCurrency([Bind(Include = "Id,MasterCurrencyId,Rate")] ChangeCurrency changeCurrency)
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Find(changeCurrency.MasterCurrencyId);

            PurchaseReturn purchaseReturn = db.PurchaseReturns.Find(changeCurrency.Id);
            purchaseReturn.MasterCurrencyId = changeCurrency.MasterCurrencyId;
            purchaseReturn.Rate = changeCurrency.Rate;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var purchaseReturnsDetails = db.PurchaseReturnsDetails.Where(x => x.PurchaseReturnId == purchaseReturn.Id).ToList();

                        foreach (PurchaseReturnDetails purchaseReturnDetails in purchaseReturnsDetails)
                        {
                            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseReturnDetails.MasterItemUnitId);

                            if (masterItemUnit == null)
                                purchaseReturnDetails.Total = 0;
                            else
                                purchaseReturnDetails.Total = purchaseReturnDetails.Quantity * purchaseReturnDetails.Price * masterItemUnit.MasterUnit.Ratio * purchaseReturn.Rate;

                            db.Entry(purchaseReturnDetails).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        purchaseReturn.Total = SharedFunctions.GetTotalPurchaseReturn(db, purchaseReturn.Id);
                        db.Entry(purchaseReturn).State = EntityState.Modified;
                        db.SaveChanges();

                        dbTran.Commit();

                        var returnObject = new
                        {
                            Status = "success",
                            Message = masterCurrency.Code + " : " + purchaseReturn.Rate.ToString("N2")
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

            return PartialView("../Buying/PurchaseReturns/_ChangeCurrency", changeCurrency);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseReturnsActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseReturnsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            PurchaseReturn purchaseReturn = db.PurchaseReturns.Find(id);

            if (masterBusinessUnit != null && purchaseReturn != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                purchaseReturn.MasterBusinessUnitId = masterBusinessUnitId;
                purchaseReturn.MasterRegionId = masterRegionId;
                db.Entry(purchaseReturn).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "PurchaseReturnsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.PurchaseReturnCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            PurchaseReturn lastData = db.PurchaseReturns
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
        [Authorize(Roles = "PurchaseReturnsActive")]
        public JsonResult GetTotal(int purchaseReturnId)
        {
            return Json(SharedFunctions.GetTotalPurchaseReturn(db, purchaseReturnId).ToString("N2"));
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseReturnsActive")]
        public JsonResult PopulateDetails(int purchaseReturnid, int purchaseId)
        {
            PurchaseReturn purchaseReturn = db.PurchaseReturns.Find(purchaseReturnid);
            Purchase purchase = db.Purchases.Find(purchaseId);

            if (purchaseReturn != null && purchase != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var remove = db.PurchaseReturnsDetails.Where(x => x.PurchaseReturnId == purchaseReturn.Id).ToList();

                        if (remove != null)
                        {
                            db.PurchaseReturnsDetails.RemoveRange(remove);
                            db.SaveChanges();
                        }

                        var purchasesDetails = db.PurchasesDetails.Where(x => x.PurchaseId == purchase.Id).ToList();

                        if (purchasesDetails != null)
                        {
                            foreach (PurchaseDetails purchaseDetails in purchasesDetails)
                            {
                                PurchaseReturnDetails purchaseReturnDetails = new PurchaseReturnDetails
                                {
                                    PurchaseReturnId = purchaseReturn.Id,
                                    MasterItemId = purchaseDetails.MasterItemId,
                                    MasterItemUnitId = purchaseDetails.MasterItemUnitId,
                                    Quantity = purchaseDetails.Quantity,
                                    Price = purchaseDetails.Price,
                                    Total = purchaseDetails.Total,
                                    Notes = purchaseDetails.Notes,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.PurchaseReturnsDetails.Add(purchaseReturnDetails);
                                db.SaveChanges();
                            }
                        }

                        purchaseReturn.PurchaseId = purchase.Id;
                        purchaseReturn.MasterBusinessUnitId = purchase.MasterBusinessUnitId;
                        purchaseReturn.MasterRegionId = purchase.MasterRegionId;
                        purchaseReturn.MasterCurrencyId = purchase.MasterCurrencyId;
                        purchaseReturn.Rate = purchase.Rate;
                        purchaseReturn.MasterSupplierId = purchase.MasterSupplierId;
                        purchaseReturn.MasterWarehouseId = purchase.MasterWarehouseId;
                        purchaseReturn.Notes = purchase.Notes;
                        purchaseReturn.Total = purchase.Total;

                        db.Entry(purchaseReturn).State = EntityState.Modified;
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
                purchaseReturn.MasterRegionId,
                purchaseReturn.MasterBusinessUnitId,
                purchaseReturn.MasterSupplierId,
                purchaseReturn.MasterWarehouseId,
                purchaseReturn.Notes,
                Total = purchaseReturn.Total.ToString("N2"),
                purchaseReturn.Date,
                Currency = purchaseReturn.MasterCurrency.Code + " : " + purchaseReturn.Rate.ToString("N2")
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
