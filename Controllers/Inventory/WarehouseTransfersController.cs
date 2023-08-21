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
    public class WarehouseTransfersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PurchaseOrders
        [Authorize(Roles = "WarehouseTransfersActive")]
        public ActionResult Index()
        {
            return View("../Inventory/WarehouseTransfers/Index");
        }

        [HttpGet]
        [Authorize(Roles = "WarehouseTransfersActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Inventory/WarehouseTransfers/_IndexGrid", db.Set<WarehouseTransfer>().AsQueryable());
            else
                return PartialView("../Inventory/WarehouseTransfers/_IndexGrid", db.Set<WarehouseTransfer>().AsQueryable()
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
                return db.WarehouseTransfers.Any(x => x.Code == Code);
            }
            else
            {
                return db.WarehouseTransfers.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: PurchaseOrders/Details/
        [Authorize(Roles = "WarehouseTransfersView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WarehouseTransfer WarehouseTransfer = db.WarehouseTransfers.Find(id);
            if (WarehouseTransfer == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/WarehouseTransfers/_Details", WarehouseTransfer);
        }

        // GET: PurchaseOrders/Create
        [Authorize(Roles = "WarehouseTransfersAdd")]
        public ActionResult Create()
        {
            WarehouseTransfer warehouseTransfer = new WarehouseTransfer
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                OriginWarehouseId = db.MasterWarehouses.FirstOrDefault().Id,
                DestinationWarehouseId = db.MasterWarehouses.FirstOrDefault().Id,
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
                    db.WarehouseTransfers.Add(warehouseTransfer);
                    db.SaveChanges();

                    dbTran.Commit();

                    warehouseTransfer.Code = "";
                    warehouseTransfer.Active = true;
                    warehouseTransfer.MasterBusinessUnitId = 0;
                    warehouseTransfer.OriginWarehouseId = 0;
                    warehouseTransfer.DestinationWarehouseId = 0;
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name");
            ViewBag.Total = "0";

            return View("../Inventory/WarehouseTransfers/Create", warehouseTransfer);
        }

        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "WarehouseTransfersAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,OriginWarehouseId,DestinationWarehouseId,Notes,Active,Created,Updated,UserId")] WarehouseTransfer warehouseTransfer)
        {
            warehouseTransfer.Created = DateTime.Now;
            warehouseTransfer.Updated = DateTime.Now;
            warehouseTransfer.UserId = User.Identity.GetUserId<int>();
            // warehouseTransfer.Total = SharedFunctions.GetTotalWarehouseTransfer(db, warehouseTransfer.Id);

            if (!string.IsNullOrEmpty(warehouseTransfer.Code)) warehouseTransfer.Code = warehouseTransfer.Code.ToUpper();
            if (!string.IsNullOrEmpty(warehouseTransfer.Notes)) warehouseTransfer.Notes = warehouseTransfer.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                warehouseTransfer = GetModelState(warehouseTransfer);
            }

            db.Entry(warehouseTransfer).State = EntityState.Modified;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.WarehouseTransfer, MenuId = warehouseTransfer.Id, MenuCode = warehouseTransfer.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", warehouseTransfer.MasterBusinessUnitId);
                //  ViewBag.Total = SharedFunctions.GetTotalWarehouseTransfer(db, warehouseTransfer.Id).ToString("N2");

                return View("../Inventory/WarehouseTransfers/Create", warehouseTransfer);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "WarehouseTransfersAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                WarehouseTransfer obj = db.WarehouseTransfers.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.WarehouseTransfersDetails.RemoveRange(db.WarehouseTransfersDetails.Where(x => x.WarehouseTransferId == obj.Id));
                                db.SaveChanges();

                                db.WarehouseTransfers.Remove(obj);
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
        [Authorize(Roles = "WarehouseTransfersEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WarehouseTransfer warehouseTransfer = db.WarehouseTransfers.Find(id);
            if (warehouseTransfer == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", warehouseTransfer.MasterBusinessUnitId);
            // ViewBag.Total = SharedFunctions.GetTotalWarehouseTransfer(db, warehouseTransfer.Id).ToString("N2");

            return View("../Inventory/WarehouseTransfers/Edit", warehouseTransfer);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "WarehouseTransfersEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] WarehouseTransfer warehouseTransfer)
        {
            warehouseTransfer.Updated = DateTime.Now;
            warehouseTransfer.UserId = User.Identity.GetUserId<int>();
            // warehouseTransfer.Total = SharedFunctions.GetTotalWarehouseTransfer(db, warehouseTransfer.Id);

            if (!string.IsNullOrEmpty(warehouseTransfer.Code)) warehouseTransfer.Code = warehouseTransfer.Code.ToUpper();
            if (!string.IsNullOrEmpty(warehouseTransfer.Notes)) warehouseTransfer.Notes = warehouseTransfer.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                warehouseTransfer = GetModelState(warehouseTransfer);
            }

            db.Entry(warehouseTransfer).State = EntityState.Unchanged;
            db.Entry(warehouseTransfer).Property("Code").IsModified = true;
            db.Entry(warehouseTransfer).Property("Date").IsModified = true;
            db.Entry(warehouseTransfer).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(warehouseTransfer).Property("MasterRegionId").IsModified = true;
            db.Entry(warehouseTransfer).Property("MasterWarehouseId").IsModified = true;
            db.Entry(warehouseTransfer).Property("Total").IsModified = true;
            db.Entry(warehouseTransfer).Property("Notes").IsModified = true;
            db.Entry(warehouseTransfer).Property("Active").IsModified = true;
            db.Entry(warehouseTransfer).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.WarehouseTransferDetails, MenuId = warehouseTransfer.Id, MenuCode = warehouseTransfer.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", warehouseTransfer.MasterBusinessUnitId);
                //  ViewBag.Total = SharedFunctions.GetTotalWarehouseTransfer(db, warehouseTransfer.Id).ToString("N2");

                return View("../Inventory/WarehouseTransfers/Edit", warehouseTransfer);
            }
        }

        [HttpPost]
        [Authorize(Roles = "WarehouseTransfersDelete")]
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
                            WarehouseTransfer obj = db.WarehouseTransfers.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                WarehouseTransfer tmp = obj;

                                db.WarehouseTransfersDetails.RemoveRange(db.WarehouseTransfersDetails.Where(x => x.WarehouseTransferId == obj.Id));
                                db.SaveChanges();

                                db.WarehouseTransfers.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.WarehouseTransfer, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "WarehouseTransfersPrint")]
        public ActionResult Print(int? id)
        {
            WarehouseTransfer obj = db.WarehouseTransfers.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormWarehouseTransfer.rpt"));
                        rd.SetParameterValue("Code", obj.Code);
                        //  rd.SetParameterValue("Terbilang", "# " + TerbilangExtension.Terbilang(Math.Floor(obj.Total)).ToUpper() + " RUPIAH #");

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

        [Authorize(Roles = "WarehouseTransfersActive")]
        private WarehouseTransfer GetModelState(WarehouseTransfer warehouseTransfer)
        {
            List<WarehouseTransferDetails> warehouseTransferDetails = db.WarehouseTransfersDetails.Where(x => x.WarehouseTransferId == warehouseTransfer.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(warehouseTransfer.Code, warehouseTransfer.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (warehouseTransferDetails == null || warehouseTransferDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return warehouseTransfer;
        }

        [Authorize(Roles = "WarehouseTransfersActive")]
        public ActionResult DetailsCreate(int warehouseTransferId)
        {
            WarehouseTransfer warehouseTransfer = db.WarehouseTransfers.Find(warehouseTransferId);

            if (warehouseTransfer == null)
            {
                return HttpNotFound();
            }

            WarehouseTransferDetails warehouseTransferDetails = new WarehouseTransferDetails
            {
                WarehouseTransferId = warehouseTransferId
            };

            return PartialView("../Inventory/warehouseTransfers/_DetailsCreate", warehouseTransferDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "WarehouseTransfersActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,WarehouseTransferId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] WarehouseTransferDetails warehouseTransferDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(warehouseTransferDetails.MasterItemUnitId);

            //if(masterItemUnit == null)
            //    warehouseTransferDetails.Total = 0;
            //else
            //    warehouseTransferDetails.Total = warehouseTransferDetails.Quantity * warehouseTransferDetails.Price * masterItemUnit.MasterUnit.Ratio;

            warehouseTransferDetails.Created = DateTime.Now;
            warehouseTransferDetails.Updated = DateTime.Now;
            warehouseTransferDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(warehouseTransferDetails.Notes)) warehouseTransferDetails.Notes = warehouseTransferDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.WarehouseTransfersDetails.Add(warehouseTransferDetails);
                        db.SaveChanges();

                        WarehouseTransfer warehouseTransfer = db.WarehouseTransfers.Find(warehouseTransferDetails.WarehouseTransferId);
                        //  warehouseTransfer.Total = SharedFunctions.GetTotalWarehouseTransfer(db, warehouseTransfer.Id, warehouseTransferDetails.Id) + warehouseTransferDetails.Total;

                        db.Entry(warehouseTransfer).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.WarehouseTransferDetails, MenuId = warehouseTransferDetails.Id, MenuCode = warehouseTransferDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Inventory/WarehouseTransfers/_DetailsCreate", warehouseTransferDetails);
        }

        [Authorize(Roles = "WarehouseTransfersActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            WarehouseTransferDetails obj = db.WarehouseTransfersDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/WarehouseTransfers/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "WarehouseTransfersActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,WarehouseTransferId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] WarehouseTransferDetails warehouseTransferDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(warehouseTransferDetails.MasterItemUnitId);

            //if (masterItemUnit == null)
            //    warehouseTransferDetails.Total = 0;
            //else
            //    warehouseTransferDetails.Total = warehouseTransferDetails.Quantity * warehouseTransferDetails.Price * masterItemUnit.MasterUnit.Ratio;

            warehouseTransferDetails.Updated = DateTime.Now;
            warehouseTransferDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(warehouseTransferDetails.Notes)) warehouseTransferDetails.Notes = warehouseTransferDetails.Notes.ToUpper();

            db.Entry(warehouseTransferDetails).State = EntityState.Unchanged;
            db.Entry(warehouseTransferDetails).Property("MasterItemId").IsModified = true;
            db.Entry(warehouseTransferDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(warehouseTransferDetails).Property("Quantity").IsModified = true;
            db.Entry(warehouseTransferDetails).Property("Price").IsModified = true;
            db.Entry(warehouseTransferDetails).Property("Total").IsModified = true;
            db.Entry(warehouseTransferDetails).Property("Notes").IsModified = true;
            db.Entry(warehouseTransferDetails).Property("Updated").IsModified = true;
            db.Entry(warehouseTransferDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        WarehouseTransfer warehouseTransfer = db.WarehouseTransfers.Find(warehouseTransferDetails.WarehouseTransferId);
                        // warehouseTransfer.Total = SharedFunctions.GetTotalWarehouseTransfer(db, warehouseTransfer.Id, warehouseTransferDetails.Id) + warehouseTransferDetails.Total;

                        db.Entry(warehouseTransfer).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.WarehouseTransferDetails, MenuId = warehouseTransferDetails.Id, MenuCode = warehouseTransferDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Inventory/WarehouseTransfers/_DetailsEdit", warehouseTransferDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "WarehouseTransfersActive")]
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
                        WarehouseTransferDetails obj = db.WarehouseTransfersDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    WarehouseTransferDetails tmp = obj;

                                    WarehouseTransfer warehouseTransfer = db.WarehouseTransfers.Find(tmp.WarehouseTransferId);

                                    // warehouseTransfer.Total = SharedFunctions.GetTotalWarehouseTransfer(db, warehouseTransfer.Id, tmp.Id);

                                    db.Entry(warehouseTransfer).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.WarehouseTransfersDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.WarehouseTransferDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "WarehouseTransfersActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Inventory/WarehouseTransfers/_DetailsGrid", db.WarehouseTransfersDetails
                .Where(x => x.WarehouseTransferId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "WarehouseTransfersActive")]
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
        [Authorize(Roles = "WarehouseTransfersActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);

            WarehouseTransfer warehouseTransfer = db.WarehouseTransfers.Find(id);

            if (masterBusinessUnit != null && warehouseTransfer != null)
            {
                code = GetCode(masterBusinessUnit);
                warehouseTransfer.MasterBusinessUnitId = masterBusinessUnitId;
                db.Entry(warehouseTransfer).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "WarehouseTransfersActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.WarehouseTransferCode + masterBusinessUnit.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            PurchaseOrder lastData = db.PurchaseOrders
                .Where(x => (x.Code.Contains(code)))
                .OrderByDescending(z => z.Code).FirstOrDefault();

            if (lastData == null)
                code = "0001" + code;
            else
                code = (Convert.ToInt32(lastData.Code.Substring(0, 4)) + 1).ToString("D4") + code;

            return code;
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
