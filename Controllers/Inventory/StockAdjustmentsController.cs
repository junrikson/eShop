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
    public class StockAdjustmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PurchaseOrders
        [Authorize(Roles = "StockAdjustmentsActive")]
        public ActionResult Index()
        {
            return View("../Inventory/StockAdjustments/Index");
        }

        [HttpGet]
        [Authorize(Roles = "StockAdjustmentsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Inventory/StockAdjustments/_IndexGrid", db.Set<StockAdjustment>().AsQueryable());
            else
                return PartialView("../Inventory/StockAdjustments/_IndexGrid", db.Set<StockAdjustment>().AsQueryable()
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
                return db.StockAdjustments.Any(x => x.Code == Code);
            }
            else
            {
                return db.StockAdjustments.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: PurchaseOrders/Details/
        [Authorize(Roles = "StockAdjustmentsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockAdjustment StockAdjustment = db.StockAdjustments.Find(id);
            if (StockAdjustment == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/StockAdjustments/_Details", StockAdjustment);
        }

        // GET: PurchaseOrders/Create
        [Authorize(Roles = "StockAdjustmentsAdd")]
        public ActionResult Create()
        {
            StockAdjustment stockAdjustment = new StockAdjustment
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
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
                    db.StockAdjustments.Add(stockAdjustment);
                    db.SaveChanges();

                    dbTran.Commit();

                    stockAdjustment.Code = "";
                    stockAdjustment.Active = true;
                    stockAdjustment.MasterBusinessUnitId = 0;
                    stockAdjustment.MasterRegionId = 0;
                    stockAdjustment.MasterWarehouseId = 0;
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

            return View("../Inventory/StockAdjustments/Create", stockAdjustment);
        }

        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "StockAdjustmentsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] StockAdjustment stockAdjustment)
        {
            stockAdjustment.Created = DateTime.Now;
            stockAdjustment.Updated = DateTime.Now;
            stockAdjustment.UserId = User.Identity.GetUserId<int>();
            stockAdjustment.Total = SharedFunctions.GetTotalStockAdjustment(db, stockAdjustment.Id);

            if (!string.IsNullOrEmpty(stockAdjustment.Code)) stockAdjustment.Code = stockAdjustment.Code.ToUpper();
            if (!string.IsNullOrEmpty(stockAdjustment.Notes)) stockAdjustment.Notes = stockAdjustment.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                stockAdjustment = GetModelState(stockAdjustment);
            }

            db.Entry(stockAdjustment).State = EntityState.Modified;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.StockAdjustment, MenuId = stockAdjustment.Id, MenuCode = stockAdjustment.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", stockAdjustment.MasterBusinessUnitId);
                ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", stockAdjustment.MasterRegionId);
                ViewBag.Total = SharedFunctions.GetTotalStockAdjustment(db, stockAdjustment.Id).ToString("N2");

                return View("../Inventory/StockAdjustments/Create", stockAdjustment);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "StockAdjustmentsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                StockAdjustment obj = db.StockAdjustments.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.StockAdjustmentsDetails.RemoveRange(db.StockAdjustmentsDetails.Where(x => x.StockAdjustmentId == obj.Id));
                                db.SaveChanges();

                                db.StockAdjustments.Remove(obj);
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
        [Authorize(Roles = "StockAdjustmentsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockAdjustment stockAdjustment = db.StockAdjustments.Find(id);
            if (stockAdjustment == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", stockAdjustment.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", stockAdjustment.MasterRegionId);
            ViewBag.Total = SharedFunctions.GetTotalStockAdjustment(db, stockAdjustment.Id).ToString("N2");

            return View("../Inventory/StockAdjustments/Edit", stockAdjustment);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "StockAdjustmentsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] StockAdjustment stockAdjustment)
        {
            stockAdjustment.Updated = DateTime.Now;
            stockAdjustment.UserId = User.Identity.GetUserId<int>();
            stockAdjustment.Total = SharedFunctions.GetTotalStockAdjustment(db, stockAdjustment.Id);

            if (!string.IsNullOrEmpty(stockAdjustment.Code)) stockAdjustment.Code = stockAdjustment.Code.ToUpper();
            if (!string.IsNullOrEmpty(stockAdjustment.Notes)) stockAdjustment.Notes = stockAdjustment.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                stockAdjustment = GetModelState(stockAdjustment);
            }

            db.Entry(stockAdjustment).State = EntityState.Unchanged;
            db.Entry(stockAdjustment).Property("Code").IsModified = true;
            db.Entry(stockAdjustment).Property("Date").IsModified = true;
            db.Entry(stockAdjustment).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(stockAdjustment).Property("MasterRegionId").IsModified = true;
            db.Entry(stockAdjustment).Property("MasterWarehouseId").IsModified = true;
            db.Entry(stockAdjustment).Property("Total").IsModified = true;
            db.Entry(stockAdjustment).Property("Notes").IsModified = true;
            db.Entry(stockAdjustment).Property("Active").IsModified = true;
            db.Entry(stockAdjustment).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.StockAdjustmentDetails, MenuId = stockAdjustment.Id, MenuCode = stockAdjustment.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", stockAdjustment.MasterBusinessUnitId);
                ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", stockAdjustment.MasterRegionId);
                ViewBag.Total = SharedFunctions.GetTotalStockAdjustment(db, stockAdjustment.Id).ToString("N2");

                return View("../Inventory/StockAdjustments/Edit", stockAdjustment);
            }
        }

        [HttpPost]
        [Authorize(Roles = "StockAdjustmentsDelete")]
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
                            StockAdjustment obj = db.StockAdjustments.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                StockAdjustment tmp = obj;

                                db.StockAdjustmentsDetails.RemoveRange(db.StockAdjustmentsDetails.Where(x => x.StockAdjustmentId == obj.Id));
                                db.SaveChanges();

                                db.StockAdjustments.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.StockAdjustment, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "StockAdjustmentsPrint")]
        public ActionResult Print(int? id)
        {
            StockAdjustment obj = db.StockAdjustments.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormStockAdjustment.rpt"));
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

        [Authorize(Roles = "StockAdjustmentsActive")]
        private StockAdjustment GetModelState(StockAdjustment stockAdjustment)
        {
            List<StockAdjustmentDetails> stockAdjustmentDetails = db.StockAdjustmentsDetails.Where(x => x.StockAdjustmentId == stockAdjustment.Id).ToList();
            
            if (ModelState.IsValid)
            {
                if (IsAnyCode(stockAdjustment.Code, stockAdjustment.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (stockAdjustmentDetails == null || stockAdjustmentDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return stockAdjustment;
        }

        [Authorize(Roles = "StockAdjustmentsActive")]
        public ActionResult DetailsCreate(int stockAdjustmentId)
        {
            StockAdjustment stockAdjustment = db.StockAdjustments.Find(stockAdjustmentId);

            if (stockAdjustment == null)
            {
                return HttpNotFound();
            }

            StockAdjustmentDetails stockAdjustmentDetails = new StockAdjustmentDetails
            {
                StockAdjustmentId = stockAdjustmentId
            };

            return PartialView("../Inventory/stockAdjustments/_DetailsCreate", stockAdjustmentDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "StockAdjustmentsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,StockAdjustmentId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] StockAdjustmentDetails stockAdjustmentDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(stockAdjustmentDetails.MasterItemUnitId);

            if(masterItemUnit == null)
                stockAdjustmentDetails.Total = 0;
            else
                stockAdjustmentDetails.Total = stockAdjustmentDetails.Quantity * stockAdjustmentDetails.Price * masterItemUnit.MasterUnit.Ratio;

            stockAdjustmentDetails.Created = DateTime.Now;
            stockAdjustmentDetails.Updated = DateTime.Now;
            stockAdjustmentDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(stockAdjustmentDetails.Notes)) stockAdjustmentDetails.Notes = stockAdjustmentDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.StockAdjustmentsDetails.Add(stockAdjustmentDetails);
                        db.SaveChanges();

                        StockAdjustment stockAdjustment = db.StockAdjustments.Find(stockAdjustmentDetails.StockAdjustmentId);
                        stockAdjustment.Total = SharedFunctions.GetTotalStockAdjustment(db, stockAdjustment.Id, stockAdjustmentDetails.Id) + stockAdjustmentDetails.Total;

                        db.Entry(stockAdjustment).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.StockAdjustmentDetails, MenuId = stockAdjustmentDetails.Id, MenuCode = stockAdjustmentDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Inventory/StockAdjustments/_DetailsCreate", stockAdjustmentDetails);
        }

        [Authorize(Roles = "StockAdjustmentsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            StockAdjustmentDetails obj = db.StockAdjustmentsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/StockAdjustments/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "StockAdjustmentsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,StockAdjustmentId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] StockAdjustmentDetails stockAdjustmentDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(stockAdjustmentDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                stockAdjustmentDetails.Total = 0;
            else
                stockAdjustmentDetails.Total = stockAdjustmentDetails.Quantity * stockAdjustmentDetails.Price * masterItemUnit.MasterUnit.Ratio;

            stockAdjustmentDetails.Updated = DateTime.Now;
            stockAdjustmentDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(stockAdjustmentDetails.Notes)) stockAdjustmentDetails.Notes = stockAdjustmentDetails.Notes.ToUpper();

            db.Entry(stockAdjustmentDetails).State = EntityState.Unchanged;
            db.Entry(stockAdjustmentDetails).Property("MasterItemId").IsModified = true;
            db.Entry(stockAdjustmentDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(stockAdjustmentDetails).Property("Quantity").IsModified = true;
            db.Entry(stockAdjustmentDetails).Property("Price").IsModified = true;
            db.Entry(stockAdjustmentDetails).Property("Total").IsModified = true;
            db.Entry(stockAdjustmentDetails).Property("Notes").IsModified = true;
            db.Entry(stockAdjustmentDetails).Property("Updated").IsModified = true;
            db.Entry(stockAdjustmentDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        StockAdjustment stockAdjustment = db.StockAdjustments.Find(stockAdjustmentDetails.StockAdjustmentId);
                        stockAdjustment.Total = SharedFunctions.GetTotalStockAdjustment(db, stockAdjustment.Id, stockAdjustmentDetails.Id) + stockAdjustmentDetails.Total;

                        db.Entry(stockAdjustment).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.StockAdjustmentDetails, MenuId = stockAdjustmentDetails.Id, MenuCode = stockAdjustmentDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Inventory/StockAdjustments/_DetailsEdit", stockAdjustmentDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "StockAdjustmentsActive")]
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
                        StockAdjustmentDetails obj = db.StockAdjustmentsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    StockAdjustmentDetails tmp = obj;

                                    StockAdjustment stockAdjustment = db.StockAdjustments.Find(tmp.StockAdjustmentId);

                                    stockAdjustment.Total = SharedFunctions.GetTotalStockAdjustment(db, stockAdjustment.Id, tmp.Id);

                                    db.Entry(stockAdjustment).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.StockAdjustmentsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.StockAdjustmentDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "StockAdjustmentsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Inventory/StockAdjustments/_DetailsGrid", db.StockAdjustmentsDetails
                .Where(x => x.StockAdjustmentId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "StockAdjustmentsActive")]
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
        [Authorize(Roles = "StockAdjustmentsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            StockAdjustment stockAdjustment = db.StockAdjustments.Find(id);

            if (masterBusinessUnit != null && stockAdjustment != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                stockAdjustment.MasterBusinessUnitId = masterBusinessUnitId;
                stockAdjustment.MasterRegionId = masterRegionId;
                db.Entry(stockAdjustment).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "StockAdjustmentsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.StockAdjustmentCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

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
        [Authorize(Roles = "StockAdjustmentsActive")]
        public JsonResult GetTotal(int stockAdjustmentId)
        {
            return Json(SharedFunctions.GetTotalStockAdjustment(db, stockAdjustmentId).ToString("N2"));
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
