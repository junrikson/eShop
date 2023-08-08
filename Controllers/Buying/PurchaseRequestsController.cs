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
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;


namespace eShop.Controllers
{
    public class PurchaseRequestsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PurchaseRequests
        [Authorize(Roles = "PurchaseRequestsActive")]
        public ActionResult Index()
        {
            return View("../Buying/PurchaseRequests/Index");
        }

        [HttpGet]
        [Authorize(Roles = "PurchaseRequestsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Buying/PurchaseRequests/_IndexGrid", db.Set<PurchaseRequest>().AsQueryable());
            else
                return PartialView("../Buying/PurchaseRequests/_IndexGrid", db.Set<PurchaseRequest>().AsQueryable()
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
                return db.PurchaseRequests.Any(x => x.Code == Code);
            }
            else
            {
                return db.PurchaseRequests.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: PurchaseRequests/Details/
        [Authorize(Roles = "PurchaseRequestsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseRequest PurchaseRequest = db.PurchaseRequests.Find(id);
            if (PurchaseRequest == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/PurchaseRequests/_Details", PurchaseRequest);
        }

        // GET: PurchaseRequests/Create
        [Authorize(Roles = "PurchaseRequestsAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            PurchaseRequest purchaseRequest = new PurchaseRequest
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
                    db.PurchaseRequests.Add(purchaseRequest);
                    db.SaveChanges();

                    dbTran.Commit();

                    purchaseRequest.Code = "";
                    purchaseRequest.Active = true;
                    purchaseRequest.MasterBusinessUnitId = 0;
                    purchaseRequest.MasterRegionId = 0;
                    purchaseRequest.MasterSupplierId = 0;
                    purchaseRequest.MasterWarehouseId = 0;
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

            return View("../Buying/PurchaseRequests/Create", purchaseRequest);
        }

        // POST: PurchaseRequests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseRequestsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterSupplierId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] PurchaseRequest purchaseRequest)
        {
            purchaseRequest.Created = DateTime.Now;
            purchaseRequest.Updated = DateTime.Now;
            purchaseRequest.UserId = User.Identity.GetUserId<int>();
            purchaseRequest.Total = SharedFunctions.GetTotalPurchaseRequest(db, purchaseRequest.Id);
            purchaseRequest.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(purchaseRequest.Code)) purchaseRequest.Code = purchaseRequest.Code.ToUpper();
            if (!string.IsNullOrEmpty(purchaseRequest.Notes)) purchaseRequest.Notes = purchaseRequest.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                purchaseRequest = GetModelState(purchaseRequest);
            }

            db.Entry(purchaseRequest).State = EntityState.Unchanged;
            db.Entry(purchaseRequest).Property("Code").IsModified = true;
            db.Entry(purchaseRequest).Property("Date").IsModified = true;
            db.Entry(purchaseRequest).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(purchaseRequest).Property("MasterRegionId").IsModified = true;
            db.Entry(purchaseRequest).Property("MasterSupplierId").IsModified = true;
            db.Entry(purchaseRequest).Property("MasterWarehouseId").IsModified = true;
            db.Entry(purchaseRequest).Property("Total").IsModified = true;
            db.Entry(purchaseRequest).Property("Notes").IsModified = true;
            db.Entry(purchaseRequest).Property("Active").IsModified = true;
            db.Entry(purchaseRequest).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseRequest, MenuId = purchaseRequest.Id, MenuCode = purchaseRequest.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", purchaseRequest.MasterBusinessUnitId);
                ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", purchaseRequest.MasterRegionId);
                ViewBag.Total = SharedFunctions.GetTotalPurchaseRequest(db, purchaseRequest.Id).ToString("N2");

                return View("../Buying/PurchaseRequests/Create", purchaseRequest);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseRequestsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                PurchaseRequest obj = db.PurchaseRequests.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.PurchaseRequestsDetails.Where(x => x.PurchaseRequestId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PurchaseRequestsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.PurchaseRequests.Remove(obj);
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

        // GET: PurchaseRequests/Edit/5
        [Authorize(Roles = "PurchaseRequestsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseRequest purchaseRequest = db.PurchaseRequests.Find(id);
            if (purchaseRequest == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", purchaseRequest.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", purchaseRequest.MasterRegionId);
            ViewBag.Total = SharedFunctions.GetTotalPurchaseRequest(db, purchaseRequest.Id).ToString("N2");

            return View("../Buying/PurchaseRequests/Edit", purchaseRequest);
        }

        // POST: PurchaseRequests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseRequestsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterSupplierId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] PurchaseRequest purchaseRequest)
        {
            purchaseRequest.Updated = DateTime.Now;
            purchaseRequest.UserId = User.Identity.GetUserId<int>();
            purchaseRequest.Total = SharedFunctions.GetTotalPurchaseRequest(db, purchaseRequest.Id);
            purchaseRequest.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(purchaseRequest.Code)) purchaseRequest.Code = purchaseRequest.Code.ToUpper();
            if (!string.IsNullOrEmpty(purchaseRequest.Notes)) purchaseRequest.Notes = purchaseRequest.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                purchaseRequest = GetModelState(purchaseRequest);
            }

            db.Entry(purchaseRequest).State = EntityState.Unchanged;
            db.Entry(purchaseRequest).Property("Code").IsModified = true;
            db.Entry(purchaseRequest).Property("Date").IsModified = true;
            db.Entry(purchaseRequest).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(purchaseRequest).Property("MasterRegionId").IsModified = true;
            db.Entry(purchaseRequest).Property("MasterSupplierId").IsModified = true;
            db.Entry(purchaseRequest).Property("MasterWarehouseId").IsModified = true;
            db.Entry(purchaseRequest).Property("Total").IsModified = true;
            db.Entry(purchaseRequest).Property("Notes").IsModified = true;
            db.Entry(purchaseRequest).Property("Active").IsModified = true;
            db.Entry(purchaseRequest).Property("Updated").IsModified = true;


                    using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseRequest, MenuId = purchaseRequest.Id, MenuCode = purchaseRequest.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", purchaseRequest.MasterBusinessUnitId);
                ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", purchaseRequest.MasterRegionId);
                ViewBag.Total = SharedFunctions.GetTotalPurchaseRequest(db, purchaseRequest.Id).ToString("N2");

                return View("../Buying/PurchaseRequests/Edit", purchaseRequest);
            }
        }

        [HttpPost]
        [Authorize(Roles = "PurchaseRequestsDelete")]
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
                            PurchaseRequest obj = db.PurchaseRequests.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                PurchaseRequest tmp = obj;

                                var details = db.PurchaseRequestsDetails.Where(x => x.PurchaseRequestId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PurchaseRequestsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.PurchaseRequests.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseRequest, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "PurchaseRequestsPrint")]
        public ActionResult Print(int? id)
        {
            PurchaseRequest obj = db.PurchaseRequests.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormPurchaseRequest.rpt"));
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

        [Authorize(Roles = "PurchaseRequestsActive")]
        private PurchaseRequest GetModelState(PurchaseRequest purchase)
        {
            List<PurchaseRequestDetails> purchaseRequestDetails = db.PurchaseRequestsDetails.Where(x => x.PurchaseRequestId == purchase.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(purchase.Code, purchase.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (purchaseRequestDetails == null || purchaseRequestDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return purchase;
        }

        [Authorize(Roles = "PurchaseRequestsActive")]
        public ActionResult DetailsCreate(int purchaseRequestId)
        {
            PurchaseRequest purchaseRequest = db.PurchaseRequests.Find(purchaseRequestId);

            if (purchaseRequest == null)
            {
                return HttpNotFound();
            }

            PurchaseRequestDetails purchaseRequestDetails = new PurchaseRequestDetails
            {
                PurchaseRequestId = purchaseRequestId
            };

            return PartialView("../Buying/PurchaseRequests/_DetailsCreate", purchaseRequestDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseRequestsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,PurchaseRequestId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PurchaseRequestDetails purchaseRequestDetails)
        {
            PurchaseRequest purchaseRequest = db.PurchaseRequests.Find(purchaseRequestDetails.PurchaseRequestId);
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseRequestDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                purchaseRequestDetails.Total = 0;
            else
                purchaseRequestDetails.Total = purchaseRequestDetails.Quantity * purchaseRequestDetails.Price * masterItemUnit.MasterUnit.Ratio * purchaseRequest.Rate;

            purchaseRequestDetails.Created = DateTime.Now;
            purchaseRequestDetails.Updated = DateTime.Now;
            purchaseRequestDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(purchaseRequestDetails.Notes)) purchaseRequestDetails.Notes = purchaseRequestDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.PurchaseRequestsDetails.Add(purchaseRequestDetails);
                        db.SaveChanges();

                        purchaseRequest.Total = SharedFunctions.GetTotalPurchaseRequest(db, purchaseRequest.Id, purchaseRequestDetails.Id) + purchaseRequestDetails.Total;

                        db.Entry(purchaseRequest).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseRequestDetails, MenuId = purchaseRequestDetails.Id, MenuCode = purchaseRequestDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Buying/PurchaseRequests/_DetailsCreate", purchaseRequestDetails);
        }

        [Authorize(Roles = "PurchaseRequestsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PurchaseRequestDetails obj = db.PurchaseRequestsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/PurchaseRequests/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseRequestsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,PurchaseRequestId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PurchaseRequestDetails purchaseRequestDetails)
        {
            PurchaseRequest purchaseRequest = db.PurchaseRequests.Find(purchaseRequestDetails.PurchaseRequestId);
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseRequestDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                purchaseRequestDetails.Total = 0;
            else
                purchaseRequestDetails.Total = purchaseRequestDetails.Quantity * purchaseRequestDetails.Price * masterItemUnit.MasterUnit.Ratio * purchaseRequest.Rate;

            purchaseRequestDetails.Updated = DateTime.Now;
            purchaseRequestDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(purchaseRequestDetails.Notes)) purchaseRequestDetails.Notes = purchaseRequestDetails.Notes.ToUpper();

            db.Entry(purchaseRequestDetails).State = EntityState.Unchanged;
            db.Entry(purchaseRequestDetails).Property("MasterItemId").IsModified = true;
            db.Entry(purchaseRequestDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(purchaseRequestDetails).Property("Quantity").IsModified = true;
            db.Entry(purchaseRequestDetails).Property("Price").IsModified = true;
            db.Entry(purchaseRequestDetails).Property("Total").IsModified = true;
            db.Entry(purchaseRequestDetails).Property("Notes").IsModified = true;
            db.Entry(purchaseRequestDetails).Property("Updated").IsModified = true;
            db.Entry(purchaseRequestDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        purchaseRequest.Total = SharedFunctions.GetTotalPurchaseRequest(db, purchaseRequest.Id, purchaseRequestDetails.Id) + purchaseRequestDetails.Total;

                        db.Entry(purchaseRequest).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseRequestDetails, MenuId = purchaseRequestDetails.Id, MenuCode = purchaseRequestDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Buying/PurchaseRequests/_DetailsEdit", purchaseRequestDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseRequestsActive")]
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
                        PurchaseRequestDetails obj = db.PurchaseRequestsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    PurchaseRequestDetails tmp = obj;

                                    PurchaseRequest purchaseRequest = db.PurchaseRequests.Find(tmp.PurchaseRequestId);

                                    purchaseRequest.Total = SharedFunctions.GetTotalPurchaseRequest(db, purchaseRequest.Id, tmp.Id);

                                    db.Entry(purchaseRequest).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.PurchaseRequestsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseRequestDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "PurchaseRequestsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Buying/PurchaseRequests/_DetailsGrid", db.PurchaseRequestsDetails
                .Where(x => x.PurchaseRequestId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseRequestsActive")]
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

        [Authorize(Roles = "PurchaseRequestsActive")]
        public ActionResult ChangeCurrency(int? purchaseRequestId)
        {
            if (purchaseRequestId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PurchaseRequest purchaseRequest = db.PurchaseRequests.Find(purchaseRequestId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = purchaseRequest.Id,
                MasterCurrencyId = purchaseRequest.MasterCurrencyId,
                Rate = purchaseRequest.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Buying/PurchaseRequests/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseRequestsActive")]
        public ActionResult ChangeCurrency([Bind(Include = "Id,MasterCurrencyId,Rate")] ChangeCurrency changeCurrency)
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Find(changeCurrency.MasterCurrencyId);

            PurchaseRequest purchaseRequest = db.PurchaseRequests.Find(changeCurrency.Id);
            purchaseRequest.MasterCurrencyId = changeCurrency.MasterCurrencyId;
            purchaseRequest.Rate = changeCurrency.Rate;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var purchaseRequestsDetails = db.PurchaseRequestsDetails.Where(x => x.PurchaseRequestId == purchaseRequest.Id).ToList();
                        
                        foreach( PurchaseRequestDetails purchaseRequestDetails in  purchaseRequestsDetails)
                        {
                            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(purchaseRequestDetails.MasterItemUnitId);

                            if (masterItemUnit == null)
                                purchaseRequestDetails.Total = 0;
                            else
                                purchaseRequestDetails.Total = purchaseRequestDetails.Quantity * purchaseRequestDetails.Price * masterItemUnit.MasterUnit.Ratio * purchaseRequest.Rate;

                            db.Entry(purchaseRequestDetails).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        purchaseRequest.Total = SharedFunctions.GetTotalPurchaseRequest(db, purchaseRequest.Id);
                        db.Entry(purchaseRequest).State = EntityState.Modified;
                        db.SaveChanges();

                        dbTran.Commit();

                        var returnObject = new 
                        {
                            Status = "success",
                            Message = masterCurrency.Code + " : " + purchaseRequest.Rate.ToString("N2")
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

            return PartialView("../Buying/PurchaseRequests/_ChangeCurrency", changeCurrency);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseRequestsActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseRequestsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            PurchaseRequest purchaseRequest = db.PurchaseRequests.Find(id);

            if (masterBusinessUnit != null && purchaseRequest != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                purchaseRequest.MasterBusinessUnitId = masterBusinessUnitId;
                purchaseRequest.MasterRegionId = masterRegionId;
                db.Entry(purchaseRequest).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "PurchaseRequestsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.PurchaseRequestCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            PurchaseRequest lastData = db.PurchaseRequests
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
        [Authorize(Roles = "PurchaseRequestsActive")]
        public JsonResult GetTotal(int purchaseRequestId)
        {
            return Json(SharedFunctions.GetTotalPurchaseRequest(db, purchaseRequestId).ToString("N2"));
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
