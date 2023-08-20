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
    public class SalesReturnsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SalesReturns
        [Authorize(Roles = "SalesReturnsActive")]
        public ActionResult Index()
        {
            return View("../Selling/SalesReturns/Index");
        }

        [HttpGet]
        [Authorize(Roles = "SalesReturnsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Selling/SalesReturns/_IndexGrid", db.Set<SalesReturn>().AsQueryable());
            else
                return PartialView("../Selling/SalesReturns/_IndexGrid", db.Set<SalesReturn>().AsQueryable()
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
                return db.SalesReturns.Any(x => x.Code == Code);
            }
            else
            {
                return db.SalesReturns.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: SalesReturns/Details/
        [Authorize(Roles = "SalesReturnsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesReturn SalesReturn = db.SalesReturns.Find(id);
            if (SalesReturn == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/SalesReturns/_Details", SalesReturn);
        }

        // GET: SalesReturns/Create
        [Authorize(Roles = "SalesReturnsAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            SalesReturn salesReturn = new SalesReturn
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MasterCurrencyId = masterCurrency.Id,
                Rate = masterCurrency.Rate,
                MasterCustomerId = db.MasterCustomers.FirstOrDefault().Id,
                MasterWarehouseId = db.MasterWarehouses.FirstOrDefault().Id,
                SaleId = db.Sales.FirstOrDefault().Id,
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
                    db.SalesReturns.Add(salesReturn);
                    db.SaveChanges();

                    dbTran.Commit();

                    salesReturn.Code = "";
                    salesReturn.Active = true;
                    salesReturn.MasterBusinessUnitId = 0;
                    salesReturn.MasterRegionId = 0;
                    salesReturn.MasterCustomerId = 0;
                    salesReturn.MasterWarehouseId = 0;
                    salesReturn.SaleId = 0;
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            ViewBag.Total = "0";

            return View("../Selling/SalesReturns/Create", salesReturn);
        }

        // POST: PurchaseReturns/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesReturnsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterCustomerId,Notes,Active,Created,Updated,UserId")] SalesReturn salesReturn)
        {
            salesReturn.Created = DateTime.Now;
            salesReturn.Updated = DateTime.Now;
            salesReturn.UserId = User.Identity.GetUserId<int>();
            salesReturn.Total = SharedFunctions.GetTotalSalesReturn(db, salesReturn.Id);

            if (!string.IsNullOrEmpty(salesReturn.Code)) salesReturn.Code = salesReturn.Code.ToUpper();
            if (!string.IsNullOrEmpty(salesReturn.Notes)) salesReturn.Notes = salesReturn.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                salesReturn = GetModelState(salesReturn);
            }

            db.Entry(salesReturn).State = EntityState.Modified;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesReturn, MenuId = salesReturn.Id, MenuCode = salesReturn.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", salesReturn.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalSalesReturn(db, salesReturn.Id).ToString("N2");

                return View("../Selling/SalesReturns/Create", salesReturn);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesReturnsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                SalesReturn obj = db.SalesReturns.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.SalesReturnsDetails.Where(x => x.SalesReturnId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.SalesReturnsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.SalesReturns.Remove(obj);
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

        // GET: SalesReturns/Edit/5
        [Authorize(Roles = "SalesReturnsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesReturn salesReturn = db.SalesReturns.Find(id);
            if (salesReturn == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", salesReturn.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalSalesReturn(db, salesReturn.Id).ToString("N2");

            return View("../selling/SalesReturns/Edit", salesReturn);
        }

        // POST: PurchaseReturns/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesReturnsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterCustomerId,Notes,Active,Created,Updated,UserId")] SalesReturn salesReturn)
        {
            salesReturn.Updated = DateTime.Now;
            salesReturn.UserId = User.Identity.GetUserId<int>();
            salesReturn.Total = SharedFunctions.GetTotalSalesReturn(db, salesReturn.Id);

            if (!string.IsNullOrEmpty(salesReturn.Code)) salesReturn.Code = salesReturn.Code.ToUpper();
            if (!string.IsNullOrEmpty(salesReturn.Notes)) salesReturn.Notes = salesReturn.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                salesReturn = GetModelState(salesReturn);
            }

            db.Entry(salesReturn).State = EntityState.Unchanged;
            db.Entry(salesReturn).Property("Code").IsModified = true;
            db.Entry(salesReturn).Property("Date").IsModified = true;
            db.Entry(salesReturn).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(salesReturn).Property("MasterRegionId").IsModified = true;
            // db.Entry(salesReturn).Property("PurchaseRequestId").IsModified = true;
            db.Entry(salesReturn).Property("MasterCustomerId").IsModified = true;
            db.Entry(salesReturn).Property("Total").IsModified = true;
            db.Entry(salesReturn).Property("Notes").IsModified = true;
            db.Entry(salesReturn).Property("Active").IsModified = true;
            db.Entry(salesReturn).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesReturn, MenuId = salesReturn.Id, MenuCode = salesReturn.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", salesReturn.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalSalesReturn(db, salesReturn.Id).ToString("N2");

                return View("../Selling/SalesReturns/Edit", salesReturn);
            }
        }

        [HttpPost]
        [Authorize(Roles = "SalesReturnsDelete")]
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
                            SalesReturn obj = db.SalesReturns.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                SalesReturn tmp = obj;

                                var details = db.SalesReturnsDetails.Where(x => x.SalesReturnId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.SalesReturnsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.SalesReturns.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesReturn, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "SalesReturnsPrint")]
        public ActionResult Print(int? id)
        {
            SalesReturn obj = db.SalesReturns.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormSalesReturn.rpt"));
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

        [Authorize(Roles = "SalesReturnsActive")]
        private SalesReturn GetModelState(SalesReturn salesReturn)
        {
            List<SalesReturnDetails> salesReturnDetails = db.SalesReturnsDetails.Where(x => x.SalesReturnId == salesReturn.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(salesReturn.Code, salesReturn.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (salesReturnDetails == null || salesReturnDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return salesReturn;
        }

        [Authorize(Roles = "SalesReturnsActive")]
        public ActionResult DetailsCreate(int salesReturnId)
        {
            SalesReturn salesReturn = db.SalesReturns.Find(salesReturnId);

            if (salesReturn == null)
            {
                return HttpNotFound();
            }

            SalesReturnDetails salesReturnDetails = new SalesReturnDetails
            {
                SalesReturnId = salesReturnId
            };

            return PartialView("../Selling/SalesReturns/_DetailsCreate", salesReturnDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesReturnsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,SalesReturnId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] SalesReturnDetails salesReturnDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(salesReturnDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                salesReturnDetails.Total = 0;
            else
                salesReturnDetails.Total = salesReturnDetails.Quantity * salesReturnDetails.Price * masterItemUnit.MasterUnit.Ratio;

            salesReturnDetails.Created = DateTime.Now;
            salesReturnDetails.Updated = DateTime.Now;
            salesReturnDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(salesReturnDetails.Notes)) salesReturnDetails.Notes = salesReturnDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SalesReturnsDetails.Add(salesReturnDetails);
                        db.SaveChanges();

                        SalesReturn salesReturn = db.SalesReturns.Find(salesReturnDetails.SalesReturnId);
                        salesReturn.Total = SharedFunctions.GetTotalSalesReturn(db, salesReturn.Id, salesReturnDetails.Id) + salesReturnDetails.Total;

                        db.Entry(salesReturn).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesReturnDetails, MenuId = salesReturnDetails.Id, MenuCode = salesReturnDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Selling/SalesReturns/_DetailsCreate", salesReturnDetails);
        }

        [Authorize(Roles = "SalesReturnsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SalesReturnDetails obj = db.SalesReturnsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/SalesReturns/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesReturnsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,SalesReturnId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] SalesReturnDetails salesReturnDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(salesReturnDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                salesReturnDetails.Total = 0;
            else
                salesReturnDetails.Total = salesReturnDetails.Quantity * salesReturnDetails.Price * masterItemUnit.MasterUnit.Ratio;

            salesReturnDetails.Updated = DateTime.Now;
            salesReturnDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(salesReturnDetails.Notes)) salesReturnDetails.Notes = salesReturnDetails.Notes.ToUpper();

            db.Entry(salesReturnDetails).State = EntityState.Unchanged;
            db.Entry(salesReturnDetails).Property("MasterItemId").IsModified = true;
            db.Entry(salesReturnDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(salesReturnDetails).Property("Quantity").IsModified = true;
            db.Entry(salesReturnDetails).Property("Price").IsModified = true;
            db.Entry(salesReturnDetails).Property("Total").IsModified = true;
            db.Entry(salesReturnDetails).Property("Notes").IsModified = true;
            db.Entry(salesReturnDetails).Property("Updated").IsModified = true;
            db.Entry(salesReturnDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        SalesReturn salesReturn = db.SalesReturns.Find(salesReturnDetails.SalesReturnId);
                        salesReturn.Total = SharedFunctions.GetTotalSalesReturn(db, salesReturn.Id, salesReturnDetails.Id) + salesReturnDetails.Total;

                        db.Entry(salesReturn).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesReturnDetails, MenuId = salesReturnDetails.Id, MenuCode = salesReturnDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Selling/SalesReturns/_DetailsEdit", salesReturnDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesReturnsActive")]
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
                        SalesReturnDetails obj = db.SalesReturnsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    SalesReturnDetails tmp = obj;

                                    SalesReturn salesReturn = db.SalesReturns.Find(tmp.SalesReturnId);

                                    salesReturn.Total = SharedFunctions.GetTotalSalesReturn(db, salesReturn.Id, tmp.Id);

                                    db.Entry(salesReturn).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.SalesReturnsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesReturnDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "SalesReturnsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Selling/SalesReturns/_DetailsGrid", db.SalesReturnsDetails
                .Where(x => x.SalesReturnId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesReturnsActive")]
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
        [Authorize(Roles = "SalesReturnsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            SalesReturn salesReturn = db.SalesReturns.Find(id);

            if (masterBusinessUnit != null && salesReturn != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                salesReturn.MasterBusinessUnitId = masterBusinessUnitId;
                salesReturn.MasterRegionId = masterRegionId;
                db.Entry(salesReturn).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "SalesReturnsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.SalesReturnCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            SalesReturn lastData = db.SalesReturns
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
        [Authorize(Roles = "SalesReturnsActive")]
        public JsonResult GetTotal(int salesReturnId)
        {
            return Json(SharedFunctions.GetTotalSalesReturn(db, salesReturnId).ToString("N2"));
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
