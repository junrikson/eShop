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
    public class SalesOrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SalesOrders
        [Authorize(Roles = "SalesOrdersActive")]
        public ActionResult Index()
        {
            return View("../Selling/SalesOrders/Index");
        }

        [HttpGet]
        [Authorize(Roles = "SalesOrdersActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Selling/SalesOrders/_IndexGrid", db.Set<SalesOrder>().AsQueryable());
            else
                return PartialView("../Selling/SalesOrders/_IndexGrid", db.Set<SalesOrder>().AsQueryable()
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
                return db.SalesOrders.Any(x => x.Code == Code);
            }
            else
            {
                return db.SalesOrders.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: SalesOrders/Details/
        [Authorize(Roles = "SalesOrdersView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesOrder SalesOrder = db.SalesOrders.Find(id);
            if (SalesOrder == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/SalesOrders/_Details", SalesOrder);
        }

        // GET: SalesOrders/Create
        [Authorize(Roles = "SalesOrdersAdd")]
        public ActionResult Create()
        {
            SalesOrder salesOrder = new SalesOrder
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
                    db.SalesOrders.Add(salesOrder);
                    db.SaveChanges();

                    dbTran.Commit();

                    salesOrder.Code = "";
                    salesOrder.Active = true;
                    salesOrder.MasterBusinessUnitId = 0;
                    salesOrder.MasterRegionId = 0;
                    salesOrder.MasterCustomerId = 0;
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

            return View("../Selling/SalesOrders/Create", salesOrder);
        }

        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesOrdersAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterCustomerId,Notes,Active,Created,Updated,UserId")] SalesOrder salesOrder)
        {
            salesOrder.Created = DateTime.Now;
            salesOrder.Updated = DateTime.Now;
            salesOrder.UserId = User.Identity.GetUserId<int>();
            salesOrder.Total = SharedFunctions.GetTotalSalesOrder(db, salesOrder.Id);

            if (!string.IsNullOrEmpty(salesOrder.Code)) salesOrder.Code = salesOrder.Code.ToUpper();
            if (!string.IsNullOrEmpty(salesOrder.Notes)) salesOrder.Notes = salesOrder.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                salesOrder = GetModelState(salesOrder);
            }

            db.Entry(salesOrder).State = EntityState.Modified;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesOrder, MenuId = salesOrder.Id, MenuCode = salesOrder.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", salesOrder.MasterBusinessUnitId);
                ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", salesOrder.MasterRegionId);
                ViewBag.Total = SharedFunctions.GetTotalSalesOrder(db, salesOrder.Id).ToString("N2");

                return View("../Selling/SalesOrders/Create", salesOrder);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesOrdersAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                SalesOrder obj = db.SalesOrders.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.SalesOrdersDetails.RemoveRange(db.SalesOrdersDetails.Where(x => x.SalesOrderId == obj.Id));
                                db.SaveChanges();

                                db.SalesOrders.Remove(obj);
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

        // GET: SalesOrders/Edit/5
        [Authorize(Roles = "SalesOrdersEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesOrder salesOrder = db.SalesOrders.Find(id);
            if (salesOrder == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", salesOrder.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", salesOrder.MasterRegionId);
            ViewBag.Total = SharedFunctions.GetTotalSalesOrder(db, salesOrder.Id).ToString("N2");

            return View("../selling/SalesOrders/Edit", salesOrder);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesOrdersEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterCustomerId,Notes,Active,Created,Updated,UserId")] SalesOrder salesOrder)
        {
            salesOrder.Updated = DateTime.Now;
            salesOrder.UserId = User.Identity.GetUserId<int>();
            salesOrder.Total = SharedFunctions.GetTotalSalesOrder(db, salesOrder.Id);

            if (!string.IsNullOrEmpty(salesOrder.Code)) salesOrder.Code = salesOrder.Code.ToUpper();
            if (!string.IsNullOrEmpty(salesOrder.Notes)) salesOrder.Notes = salesOrder.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                salesOrder = GetModelState(salesOrder);
            }

            db.Entry(salesOrder).State = EntityState.Unchanged;
            db.Entry(salesOrder).Property("Code").IsModified = true;
            db.Entry(salesOrder).Property("Date").IsModified = true;
            db.Entry(salesOrder).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(salesOrder).Property("MasterRegionId").IsModified = true;
            db.Entry(salesOrder).Property("PurchaseRequestId").IsModified = true;
            db.Entry(salesOrder).Property("MasterSupplierId").IsModified = true;
            db.Entry(salesOrder).Property("Total").IsModified = true;
            db.Entry(salesOrder).Property("Notes").IsModified = true;
            db.Entry(salesOrder).Property("Active").IsModified = true;
            db.Entry(salesOrder).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesOrder, MenuId = salesOrder.Id, MenuCode = salesOrder.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", salesOrder.MasterBusinessUnitId);
                ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", salesOrder.MasterRegionId);
                ViewBag.Total = SharedFunctions.GetTotalSalesOrder(db, salesOrder.Id).ToString("N2");

                return View("../Selling/SalesOrders/Edit", salesOrder);
            }
        }

        [HttpPost]
        [Authorize(Roles = "SalesOrdersDelete")]
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
                            SalesOrder obj = db.SalesOrders.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                SalesOrder tmp = obj;

                                db.SalesOrdersDetails.RemoveRange(db.SalesOrdersDetails.Where(x => x.SalesOrderId == obj.Id));
                                db.SaveChanges();

                                db.SalesOrders.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesOrder, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "SalesOrdersPrint")]
        public ActionResult Print(int? id)
        {
            SalesOrder obj = db.SalesOrders.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormSalesOrder.rpt"));
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

        [Authorize(Roles = "SalesOrdersActive")]
        private SalesOrder GetModelState(SalesOrder salesOrder)
        {
            List<SalesOrderDetails> salesOrderDetails = db.SalesOrdersDetails.Where(x => x.SalesOrderId == salesOrder.Id).ToList();
            
            if (ModelState.IsValid)
            {
                if (IsAnyCode(salesOrder.Code, salesOrder.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (salesOrderDetails == null || salesOrderDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return salesOrder;
        }

        [Authorize(Roles = "SalesOrdersActive")]
        public ActionResult DetailsCreate(int salesOrderId)
        {
            SalesOrder salesOrder = db.SalesOrders.Find(salesOrderId);

            if (salesOrder == null)
            {
                return HttpNotFound();
            }

            SalesOrderDetails salesOrderDetails = new SalesOrderDetails
            {
                SalesOrderId = salesOrderId
            };

            return PartialView("../Selling/SalesOrders/_DetailsCreate", salesOrderDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesOrdersActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,SalesOrderId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] SalesOrderDetails salesOrderDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(salesOrderDetails.MasterItemUnitId);

            if(masterItemUnit == null)
                salesOrderDetails.Total = 0;
            else
                salesOrderDetails.Total = salesOrderDetails.Quantity * salesOrderDetails.Price * masterItemUnit.MasterUnit.Ratio;

            salesOrderDetails.Created = DateTime.Now;
            salesOrderDetails.Updated = DateTime.Now;
            salesOrderDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(salesOrderDetails.Notes)) salesOrderDetails.Notes = salesOrderDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SalesOrdersDetails.Add(salesOrderDetails);
                        db.SaveChanges();

                        SalesOrder salesOrder = db.SalesOrders.Find(salesOrderDetails.SalesOrderId);
                        salesOrder.Total = SharedFunctions.GetTotalSalesOrder(db, salesOrder.Id, salesOrderDetails.Id) + salesOrderDetails.Total;

                        db.Entry(salesOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesOrderDetails, MenuId = salesOrderDetails.Id, MenuCode = salesOrderDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Selling/SalesOrders/_DetailsCreate", salesOrderDetails);
        }

        [Authorize(Roles = "SalesOrdersActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SalesOrderDetails obj = db.SalesOrdersDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/SalesOrders/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesOrdersActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,SalesOrderId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] SalesOrderDetails salesOrderDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(salesOrderDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                salesOrderDetails.Total = 0;
            else
                salesOrderDetails.Total = salesOrderDetails.Quantity * salesOrderDetails.Price * masterItemUnit.MasterUnit.Ratio;

            salesOrderDetails.Updated = DateTime.Now;
            salesOrderDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(salesOrderDetails.Notes)) salesOrderDetails.Notes = salesOrderDetails.Notes.ToUpper();

            db.Entry(salesOrderDetails).State = EntityState.Unchanged;
            db.Entry(salesOrderDetails).Property("MasterItemId").IsModified = true;
            db.Entry(salesOrderDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(salesOrderDetails).Property("Quantity").IsModified = true;
            db.Entry(salesOrderDetails).Property("Price").IsModified = true;
            db.Entry(salesOrderDetails).Property("Total").IsModified = true;
            db.Entry(salesOrderDetails).Property("Notes").IsModified = true;
            db.Entry(salesOrderDetails).Property("Updated").IsModified = true;
            db.Entry(salesOrderDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        SalesOrder salesOrder = db.SalesOrders.Find(salesOrderDetails.SalesOrderId);
                        salesOrder.Total = SharedFunctions.GetTotalSalesOrder(db, salesOrder.Id, salesOrderDetails.Id) + salesOrderDetails.Total;

                        db.Entry(salesOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesOrderDetails, MenuId = salesOrderDetails.Id, MenuCode = salesOrderDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Selling/SalesOrders/_DetailsEdit", salesOrderDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesOrdersActive")]
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
                        SalesOrderDetails obj = db.SalesOrdersDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    SalesOrderDetails tmp = obj;

                                    SalesOrder salesOrder = db.SalesOrders.Find(tmp.SalesOrderId);

                                    salesOrder.Total = SharedFunctions.GetTotalSalesOrder(db, salesOrder.Id, tmp.Id);

                                    db.Entry(salesOrder).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.SalesOrdersDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesOrderDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "SalesOrdersActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Selling/SalesOrders/_DetailsGrid", db.SalesOrdersDetails
                .Where(x => x.SalesOrderId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesOrdersActive")]
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
        [Authorize(Roles = "SalesOrdersActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            SalesOrder salesOrder = db.SalesOrders.Find(id);

            if (masterBusinessUnit != null && salesOrder != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                salesOrder.MasterBusinessUnitId = masterBusinessUnitId;
                salesOrder.MasterRegionId = masterRegionId;
                db.Entry(salesOrder).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "SalesOrdersActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.SalesOrderCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            SalesOrder lastData = db.SalesOrders
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
        [Authorize(Roles = "SalesOrdersActive")]
        public JsonResult GetTotal(int salesOrderId)
        {
            return Json(SharedFunctions.GetTotalSalesOrder(db, salesOrderId).ToString("N2"));
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
