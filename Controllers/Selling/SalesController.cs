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
    public class SalesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Sales
        [Authorize(Roles = "SalesActive")]
        public ActionResult Index()
        {
            return View("../Selling/Sales/Index");
        }

        [HttpGet]
        [Authorize(Roles = "SalesActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Selling/Sales/_IndexGrid", db.Set<Sale>().AsQueryable());
            else
                return PartialView("../Selling/Sales/_IndexGrid", db.Set<Sale>().AsQueryable()
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
                return db.Sales.Any(x => x.Code == Code);
            }
            else
            {
                return db.Sales.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: Sales/Details/
        [Authorize(Roles = "SalesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sale Sale = db.Sales.Find(id);
            if (Sale == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/Sales/_Details", Sale);
        }

        // GET: Sales/Create
        [Authorize(Roles = "SalesAdd")]
        public ActionResult Create()
        {
            Sale sale = new Sale
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MasterCustomerId = db.MasterCustomers.FirstOrDefault().Id,
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
                    db.Sales.Add(sale);
                    db.SaveChanges();

                    dbTran.Commit();

                    sale.Code = "";
                    sale.Active = true;
                    sale.MasterBusinessUnitId = 0;
                    sale.MasterRegionId = 0;
                    sale.MasterCustomerId = 0;
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

            return View("../Selling/Sales/Create", sale);
        }

        // POST: Purchase/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterCustomerId,Notes,Active,Created,Updated,UserId")] Sale sale)
        {
            sale.Created = DateTime.Now;
            sale.Updated = DateTime.Now;
            sale.UserId = User.Identity.GetUserId<int>();
            sale.Total = SharedFunctions.GetTotalSale(db, sale.Id);

            if (!string.IsNullOrEmpty(sale.Code)) sale.Code = sale.Code.ToUpper();
            if (!string.IsNullOrEmpty(sale.Notes)) sale.Notes = sale.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                sale = GetModelState(sale);
            }

            db.Entry(sale).State = EntityState.Modified;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Sale, MenuId = sale.Id, MenuCode = sale.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", sale.MasterBusinessUnitId);
                ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", sale.MasterRegionId);
                ViewBag.Total = SharedFunctions.GetTotalSale(db, sale.Id).ToString("N2");

                return View("../Selling/Sales/Create", sale);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                Sale obj = db.Sales.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.SalesDetails.RemoveRange(db.SalesDetails.Where(x => x.SaleId == obj.Id));
                                db.SaveChanges();

                                db.Sales.Remove(obj);
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

        // GET: Sales/Edit/5
        [Authorize(Roles = "SalesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sale sales = db.Sales.Find(id);
            if (sales == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", sales.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", sales.MasterRegionId);
            ViewBag.Total = SharedFunctions.GetTotalSale(db, sales.Id).ToString("N2");

            return View("../selling/Sales/Edit", sales);
        }

        // POST: Purchase/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterCustomerId,Notes,Active,Created,Updated,UserId")] Sale sales)
        {
            sales.Updated = DateTime.Now;
            sales.UserId = User.Identity.GetUserId<int>();
            sales.Total = SharedFunctions.GetTotalSale(db, sales.Id);

            if (!string.IsNullOrEmpty(sales.Code)) sales.Code = sales.Code.ToUpper();
            if (!string.IsNullOrEmpty(sales.Notes)) sales.Notes = sales.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                sales = GetModelState(sales);
            }

            db.Entry(sales).State = EntityState.Unchanged;
            db.Entry(sales).Property("Code").IsModified = true;
            db.Entry(sales).Property("Date").IsModified = true;
            db.Entry(sales).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(sales).Property("MasterRegionId").IsModified = true;
            db.Entry(sales).Property("MasterCustomerId").IsModified = true;
            db.Entry(sales).Property("Total").IsModified = true;
            db.Entry(sales).Property("Notes").IsModified = true;
            db.Entry(sales).Property("Active").IsModified = true;
            db.Entry(sales).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Sale, MenuId = sales.Id, MenuCode = sales.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", sales.MasterBusinessUnitId);
                ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", sales.MasterRegionId);
                ViewBag.Total = SharedFunctions.GetTotalSale(db, sales.Id).ToString("N2");

                return View("../Selling/Sales/Edit", sales);
            }
        }

        [HttpPost]
        [Authorize(Roles = "SalesDelete")]
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
                            Sale obj = db.Sales.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                Sale tmp = obj;

                                db.SalesDetails.RemoveRange(db.SalesDetails.Where(x => x.SaleId == obj.Id));
                                db.SaveChanges();

                                db.Sales.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Sale, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "SalesPrint")]
        public ActionResult Print(int? id)
        {
            Sale obj = db.Sales.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormSales.rpt"));
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

        [Authorize(Roles = "SalesActive")]
        private Sale GetModelState(Sale sales)
        {
            List<SaleDetails> salesDetails = db.SalesDetails.Where(x => x.SaleId == sales.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(sales.Code, sales.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (salesDetails == null || salesDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return sales;
        }

        [Authorize(Roles = "SalesActive")]
        public ActionResult DetailsCreate(int salesId)
        {
            Sale sales = db.Sales.Find(salesId);

            if (sales == null)
            {
                return HttpNotFound();
            }

            SaleDetails salesDetails = new SaleDetails
            {
                SaleId = salesId
            };

            return PartialView("../Selling/Sales/_DetailsCreate", salesDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,SaleId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] SaleDetails salesDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(salesDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                salesDetails.Total = 0;
            else
                salesDetails.Total = salesDetails.Quantity * salesDetails.Price * masterItemUnit.MasterUnit.Ratio;

            salesDetails.Created = DateTime.Now;
            salesDetails.Updated = DateTime.Now;
            salesDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(salesDetails.Notes)) salesDetails.Notes = salesDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SalesDetails.Add(salesDetails);
                        db.SaveChanges();

                        Sale sales = db.Sales.Find(salesDetails.SaleId);
                        sales.Total = SharedFunctions.GetTotalSale(db, sales.Id, salesDetails.Id) + salesDetails.Total;

                        db.Entry(sales).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SaleDetails, MenuId = salesDetails.Id, MenuCode = salesDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Selling/Sales/_DetailsCreate", salesDetails);
        }

        [Authorize(Roles = "SalesActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SaleDetails obj = db.SalesDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/Sales/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,SaleId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] SaleDetails salesDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(salesDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                salesDetails.Total = 0;
            else
                salesDetails.Total = salesDetails.Quantity * salesDetails.Price * masterItemUnit.MasterUnit.Ratio;

            salesDetails.Updated = DateTime.Now;
            salesDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(salesDetails.Notes)) salesDetails.Notes = salesDetails.Notes.ToUpper();

            db.Entry(salesDetails).State = EntityState.Unchanged;
            db.Entry(salesDetails).Property("MasterItemId").IsModified = true;
            db.Entry(salesDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(salesDetails).Property("Quantity").IsModified = true;
            db.Entry(salesDetails).Property("Price").IsModified = true;
            db.Entry(salesDetails).Property("Total").IsModified = true;
            db.Entry(salesDetails).Property("Notes").IsModified = true;
            db.Entry(salesDetails).Property("Updated").IsModified = true;
            db.Entry(salesDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        Sale sales = db.Sales.Find(salesDetails.SaleId);
                        sales.Total = SharedFunctions.GetTotalSale(db, sales.Id, salesDetails.Id) + salesDetails.Total;

                        db.Entry(sales).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SaleDetails, MenuId = salesDetails.Id, MenuCode = salesDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Selling/Sales/_DetailsEdit", salesDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesActive")]
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
                        SaleDetails obj = db.SalesDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    SaleDetails tmp = obj;

                                    Sale sales = db.Sales.Find(tmp.SaleId);

                                    sales.Total = SharedFunctions.GetTotalSale(db, sales.Id, tmp.Id);

                                    db.Entry(sales).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.SalesDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SaleDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "SalesActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Selling/Sales/_DetailsGrid", db.SalesDetails
                .Where(x => x.SaleId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesActive")]
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
        [Authorize(Roles = "SalesActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            Sale sales = db.Sales.Find(id);

            if (masterBusinessUnit != null && sales != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                sales.MasterBusinessUnitId = masterBusinessUnitId;
                sales.MasterRegionId = masterRegionId;
                db.Entry(sales).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "SalesActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.SalesCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            Sale lastData = db.Sales
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
        [Authorize(Roles = "SalesActive")]
        public JsonResult GetTotal(int salesId)
        {
            return Json(SharedFunctions.GetTotalSale(db, salesId).ToString("N2"));
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
