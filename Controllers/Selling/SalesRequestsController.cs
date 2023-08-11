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
    public class SalesRequestsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SalesRequests
        [Authorize(Roles = "SalesRequestsActive")]
        public ActionResult Index()
        {
            return View("../Selling/SalesRequests/Index");
        }

        [HttpGet]
        [Authorize(Roles = "SalesRequestsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Selling/SalesRequests/_IndexGrid", db.Set<SalesRequest>().AsQueryable());
            else
                return PartialView("../Selling/SalesRequests/_IndexGrid", db.Set<SalesRequest>().AsQueryable()
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
                return db.SalesRequests.Any(x => x.Code == Code);
            }
            else
            {
                return db.SalesRequests.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: SalesRequests/Details/
        [Authorize(Roles = "SalesRequestsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesRequest SalesRequest = db.SalesRequests.Find(id);
            if (SalesRequest == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/SalesRequests/_Details", SalesRequest);
        }

        // GET: SalesRequests/Create
        [Authorize(Roles = "SalesRequestsAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            SalesRequest salesRequest = new SalesRequest
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MasterCurrencyId = masterCurrency.Id,
                Rate = masterCurrency.Rate,
                MasterCustomerId = db.MasterCustomers.FirstOrDefault().Id,
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
                    db.SalesRequests.Add(salesRequest);
                    db.SaveChanges();

                    dbTran.Commit();

                    salesRequest.Code = "";
                    salesRequest.Active = true;
                    salesRequest.MasterBusinessUnitId = 0;
                    salesRequest.MasterRegionId = 0;
                    salesRequest.MasterCustomerId = 0;
                    salesRequest.MasterWarehouseId = 0;
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

            return View("../Selling/SalesRequests/Create", salesRequest);
        }

        // POST: PurchaseRequests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesRequestsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterCustomerId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] SalesRequest salesRequest)
        {
            salesRequest.Created = DateTime.Now;
            salesRequest.Updated = DateTime.Now;
            salesRequest.UserId = User.Identity.GetUserId<int>();
            salesRequest.Total = SharedFunctions.GetTotalSalesRequest(db, salesRequest.Id);
            salesRequest.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(salesRequest.Code)) salesRequest.Code = salesRequest.Code.ToUpper();
            if (!string.IsNullOrEmpty(salesRequest.Notes)) salesRequest.Notes = salesRequest.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                salesRequest = GetModelState(salesRequest);
            }

            db.Entry(salesRequest).State = EntityState.Unchanged;
            db.Entry(salesRequest).Property("Code").IsModified = true;
            db.Entry(salesRequest).Property("Date").IsModified = true;
            db.Entry(salesRequest).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(salesRequest).Property("MasterRegionId").IsModified = true;
            db.Entry(salesRequest).Property("MasterCustomerId").IsModified = true;
            db.Entry(salesRequest).Property("MasterWarehouseId").IsModified = true;
            db.Entry(salesRequest).Property("Total").IsModified = true;
            db.Entry(salesRequest).Property("Notes").IsModified = true;
            db.Entry(salesRequest).Property("Active").IsModified = true;
            db.Entry(salesRequest).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesRequest, MenuId = salesRequest.Id, MenuCode = salesRequest.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", salesRequest.MasterBusinessUnitId);
                ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", salesRequest.MasterRegionId);
                ViewBag.Total = SharedFunctions.GetTotalSalesRequest(db, salesRequest.Id).ToString("N2");

                return View("../Selling/SalesRequests/Create", salesRequest);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesRequestsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                SalesRequest obj = db.SalesRequests.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.SalesRequestsDetails.Where(x => x.SalesRequestId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.SalesRequestsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.SalesRequests.Remove(obj);
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

        // GET: SalesRequests/Edit/5
        [Authorize(Roles = "SalesRequestsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesRequest salesRequest = db.SalesRequests.Find(id);
            if (salesRequest == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", salesRequest.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", salesRequest.MasterRegionId);
            ViewBag.Total = SharedFunctions.GetTotalSalesRequest(db, salesRequest.Id).ToString("N2");

            return View("../selling/SalesRequests/Edit", salesRequest);
        }

        // POST: PurchaseRequests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesRequestsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterCustomerId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] SalesRequest salesRequest)
        {
            salesRequest.Updated = DateTime.Now;
            salesRequest.UserId = User.Identity.GetUserId<int>();
            salesRequest.Total = SharedFunctions.GetTotalSalesRequest(db, salesRequest.Id);
            salesRequest.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(salesRequest.Code)) salesRequest.Code = salesRequest.Code.ToUpper();
            if (!string.IsNullOrEmpty(salesRequest.Notes)) salesRequest.Notes = salesRequest.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                salesRequest = GetModelState(salesRequest);
            }

            db.Entry(salesRequest).State = EntityState.Unchanged;
            db.Entry(salesRequest).Property("Code").IsModified = true;
            db.Entry(salesRequest).Property("Date").IsModified = true;
            db.Entry(salesRequest).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(salesRequest).Property("MasterRegionId").IsModified = true;
            db.Entry(salesRequest).Property("MasterCustomerId").IsModified = true;
            db.Entry(salesRequest).Property("MasterWarehouseId").IsModified = true;
            db.Entry(salesRequest).Property("Total").IsModified = true;
            db.Entry(salesRequest).Property("Notes").IsModified = true;
            db.Entry(salesRequest).Property("Active").IsModified = true;
            db.Entry(salesRequest).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesRequest, MenuId = salesRequest.Id, MenuCode = salesRequest.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", salesRequest.MasterBusinessUnitId);
                ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", salesRequest.MasterRegionId);
                ViewBag.Total = SharedFunctions.GetTotalSalesRequest(db, salesRequest.Id).ToString("N2");

                return View("../Selling/SalesRequests/Edit", salesRequest);
            }
        }

        [HttpPost]
        [Authorize(Roles = "SalesRequestsDelete")]
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
                            SalesRequest obj = db.SalesRequests.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                SalesRequest tmp = obj;

                                var details = db.SalesRequestsDetails.Where(x => x.SalesRequestId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.SalesRequestsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.SalesRequests.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesRequest, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "SalesRequestsPrint")]
        public ActionResult Print(int? id)
        {
            SalesRequest obj = db.SalesRequests.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormSalesRequest.rpt"));
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

                        return new CrystalReportPdfResult(rd, "SR_" + obj.Code + ".pdf");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }
        }

        [Authorize(Roles = "SalesRequestsActive")]
        private SalesRequest GetModelState(SalesRequest salesRequest)
        {
            List<SalesRequestDetails> salesRequestDetails = db.SalesRequestsDetails.Where(x => x.SalesRequestId == salesRequest.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(salesRequest.Code, salesRequest.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (salesRequestDetails == null || salesRequestDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return salesRequest;
        }

        [Authorize(Roles = "SalesRequestsActive")]
        public ActionResult DetailsCreate(int salesRequestId)
        {
            SalesRequest salesRequest = db.SalesRequests.Find(salesRequestId);

            if (salesRequest == null)
            {
                return HttpNotFound();
            }

            SalesRequestDetails salesRequestDetails = new SalesRequestDetails
            {
                SalesRequestId = salesRequestId
            };

            return PartialView("../Selling/SalesRequests/_DetailsCreate", salesRequestDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesRequestsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,SalesRequestId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] SalesRequestDetails salesRequestDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(salesRequestDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                salesRequestDetails.Total = 0;
            else
                salesRequestDetails.Total = salesRequestDetails.Quantity * salesRequestDetails.Price * masterItemUnit.MasterUnit.Ratio;

            salesRequestDetails.Created = DateTime.Now;
            salesRequestDetails.Updated = DateTime.Now;
            salesRequestDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(salesRequestDetails.Notes)) salesRequestDetails.Notes = salesRequestDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SalesRequestsDetails.Add(salesRequestDetails);
                        db.SaveChanges();

                        SalesRequest salesRequest = db.SalesRequests.Find(salesRequestDetails.SalesRequestId);
                        salesRequest.Total = SharedFunctions.GetTotalSalesRequest(db, salesRequest.Id, salesRequestDetails.Id) + salesRequestDetails.Total;

                        db.Entry(salesRequest).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesRequestDetails, MenuId = salesRequestDetails.Id, MenuCode = salesRequestDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Selling/SalesRequests/_DetailsCreate", salesRequestDetails);
        }

        [Authorize(Roles = "SalesRequestsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SalesRequestDetails obj = db.SalesRequestsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/SalesRequests/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesRequestsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,SalesRequestId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] SalesRequestDetails salesRequestDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(salesRequestDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                salesRequestDetails.Total = 0;
            else
                salesRequestDetails.Total = salesRequestDetails.Quantity * salesRequestDetails.Price * masterItemUnit.MasterUnit.Ratio;

            salesRequestDetails.Updated = DateTime.Now;
            salesRequestDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(salesRequestDetails.Notes)) salesRequestDetails.Notes = salesRequestDetails.Notes.ToUpper();

            db.Entry(salesRequestDetails).State = EntityState.Unchanged;
            db.Entry(salesRequestDetails).Property("MasterItemId").IsModified = true;
            db.Entry(salesRequestDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(salesRequestDetails).Property("Quantity").IsModified = true;
            db.Entry(salesRequestDetails).Property("Price").IsModified = true;
            db.Entry(salesRequestDetails).Property("Total").IsModified = true;
            db.Entry(salesRequestDetails).Property("Notes").IsModified = true;
            db.Entry(salesRequestDetails).Property("Updated").IsModified = true;
            db.Entry(salesRequestDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        SalesRequest salesRequest = db.SalesRequests.Find(salesRequestDetails.SalesRequestId);
                        salesRequest.Total = SharedFunctions.GetTotalSalesRequest(db, salesRequest.Id, salesRequestDetails.Id) + salesRequestDetails.Total;

                        db.Entry(salesRequest).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesRequestDetails, MenuId = salesRequestDetails.Id, MenuCode = salesRequestDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Selling/SalesRequests/_DetailsEdit", salesRequestDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesRequestsActive")]
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
                        SalesRequestDetails obj = db.SalesRequestsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    SalesRequestDetails tmp = obj;

                                    SalesRequest salesRequest = db.SalesRequests.Find(tmp.SalesRequestId);

                                    salesRequest.Total = SharedFunctions.GetTotalSalesRequest(db, salesRequest.Id, tmp.Id);

                                    db.Entry(salesRequest).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.SalesRequestsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesRequestDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "SalesRequestsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Selling/SalesRequests/_DetailsGrid", db.SalesRequestsDetails
                .Where(x => x.SalesRequestId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesRequestsActive")]
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

        [Authorize(Roles = "SalesRequestsActive")]
        public ActionResult ChangeCurrency(int? salesRequestId)
        {
            if (salesRequestId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SalesRequest salesRequest = db.SalesRequests.Find(salesRequestId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = salesRequest.Id,
                MasterCurrencyId = salesRequest.MasterCurrencyId,
                Rate = salesRequest.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Selling/SalesRequests/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesRequestsActive")]
        public ActionResult ChangeCurrency([Bind(Include = "Id,MasterCurrencyId,Rate")] ChangeCurrency changeCurrency)
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Find(changeCurrency.MasterCurrencyId);

            SalesRequest salesRequest = db.SalesRequests.Find(changeCurrency.Id);
            salesRequest.MasterCurrencyId = changeCurrency.MasterCurrencyId;
            salesRequest.Rate = changeCurrency.Rate;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var salesRequestsDetails = db.SalesRequestsDetails.Where(x => x.SalesRequestId == salesRequest.Id).ToList();

                        foreach (SalesRequestDetails salesRequestDetails in salesRequestsDetails)
                        {
                            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(salesRequestDetails.MasterItemUnitId);

                            if (masterItemUnit == null)
                                salesRequestDetails.Total = 0;
                            else
                                salesRequestDetails.Total = salesRequestDetails.Quantity * salesRequestDetails.Price * masterItemUnit.MasterUnit.Ratio * salesRequest.Rate;

                            db.Entry(salesRequestDetails).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        salesRequest.Total = SharedFunctions.GetTotalSalesRequest(db, salesRequest.Id);
                        db.Entry(salesRequest).State = EntityState.Modified;
                        db.SaveChanges();

                        dbTran.Commit();

                        var returnObject = new
                        {
                            Status = "success",
                            Message = masterCurrency.Code + " : " + salesRequest.Rate.ToString("N2")
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

            return PartialView("../Selling/SalesRequests/_ChangeCurrency", changeCurrency);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesRequestsActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesRequestsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            SalesRequest salesRequest = db.SalesRequests.Find(id);

            if (masterBusinessUnit != null && salesRequest != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                salesRequest.MasterBusinessUnitId = masterBusinessUnitId;
                salesRequest.MasterRegionId = masterRegionId;
                db.Entry(salesRequest).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "SalesRequestsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.SalesRequestCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            SalesRequest lastData = db.SalesRequests
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
        [Authorize(Roles = "SalesRequestsActive")]
        public JsonResult GetTotal(int salesRequestId)
        {
            return Json(SharedFunctions.GetTotalSalesRequest(db, salesRequestId).ToString("N2"));
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
