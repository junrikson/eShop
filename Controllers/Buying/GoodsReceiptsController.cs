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
    public class GoodsReceiptsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: GoodsReceipts
        [Authorize(Roles = "GoodsReceiptsActive")]
        public ActionResult Index()
        {
            return View("../Buying/GoodsReceipts/Index");
        }

        [HttpGet]
        [Authorize(Roles = "GoodsReceiptsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Buying/GoodsReceipts/_IndexGrid", db.Set<GoodsReceipt>().AsQueryable());
            else
                return PartialView("../Buying/GoodsReceipts/_IndexGrid", db.Set<GoodsReceipt>().AsQueryable()
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
                return db.GoodsReceipts.Any(x => x.Code == Code);
            }
            else
            {
                return db.GoodsReceipts.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: GoodsReceipts/Details/
        [Authorize(Roles = "GoodsReceiptsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GoodsReceipt GoodsReceipt = db.GoodsReceipts.Find(id);
            if (GoodsReceipt == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/GoodsReceipts/_Details", GoodsReceipt);
        }

        [HttpGet]
        [Authorize(Roles = "GoodsReceiptsView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Buying/GoodsReceipts/_ViewGrid", db.GoodsReceiptsDetails
                .Where(x => x.GoodsReceiptId == Id).ToList());
        }

        // GET: GoodsReceipts/Create
        [Authorize(Roles = "GoodsReceiptsAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            GoodsReceipt goodsReceipt = new GoodsReceipt
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
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
                    db.GoodsReceipts.Add(goodsReceipt);
                    db.SaveChanges();

                    dbTran.Commit();

                    goodsReceipt.Code = "";
                    goodsReceipt.Active = true;
                    goodsReceipt.MasterBusinessUnitId = 0;
                    goodsReceipt.MasterRegionId = 0;
                    goodsReceipt.MasterSupplierId = 0;
                    goodsReceipt.MasterWarehouseId = 0;
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

            return View("../Buying/GoodsReceipts/Create", goodsReceipt);
        }

        // POST: GoodsReceipts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,PurchaseId,MasterSupplierId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] GoodsReceipt goodsReceipt)
        {
            goodsReceipt.Created = DateTime.Now;
            goodsReceipt.Updated = DateTime.Now;
            goodsReceipt.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(goodsReceipt.Code)) goodsReceipt.Code = goodsReceipt.Code.ToUpper();
            if (!string.IsNullOrEmpty(goodsReceipt.Notes)) goodsReceipt.Notes = goodsReceipt.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                goodsReceipt = GetModelState(goodsReceipt);
            }

            db.Entry(goodsReceipt).State = EntityState.Unchanged;
            db.Entry(goodsReceipt).Property("Code").IsModified = true;
            db.Entry(goodsReceipt).Property("Date").IsModified = true;
            db.Entry(goodsReceipt).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(goodsReceipt).Property("MasterRegionId").IsModified = true;
            db.Entry(goodsReceipt).Property("PurchaseId").IsModified = true;
            db.Entry(goodsReceipt).Property("MasterSupplierId").IsModified = true;
            db.Entry(goodsReceipt).Property("MasterWarehouseId").IsModified = true;
            db.Entry(goodsReceipt).Property("Notes").IsModified = true;
            db.Entry(goodsReceipt).Property("Active").IsModified = true;
            db.Entry(goodsReceipt).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GoodsReceipt, MenuId = goodsReceipt.Id, MenuCode = goodsReceipt.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", goodsReceipt.MasterBusinessUnitId);
                
                return View("../Buying/GoodsReceipts/Create", goodsReceipt);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "GoodsReceiptsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                GoodsReceipt obj = db.GoodsReceipts.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.GoodsReceiptsDetails.Where(x => x.GoodsReceiptId == obj.Id).ToList();

                                foreach (var detail in details)
                                {
                                    SharedFunctions.UpdateStock(db, EnumMenuType.GoodsReceiptDetails, EnumActions.DELETE, obj.MasterBusinessUnitId, obj.MasterRegionId, obj.MasterWarehouseId, detail.MasterItemId, 0, detail.Id);

                                    db.GoodsReceiptsDetails.Remove(detail);
                                    db.SaveChanges();
                                }

                                db.GoodsReceipts.Remove(obj);
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

        // GET: GoodsReceipts/Edit/5
        [Authorize(Roles = "GoodsReceiptsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GoodsReceipt goodsReceipt = db.GoodsReceipts.Find(id);
            if (goodsReceipt == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", goodsReceipt.MasterBusinessUnitId);
            
            return View("../Buying/GoodsReceipts/Edit", goodsReceipt);
        }

        // POST: GoodsReceipts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GoodsReceiptsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,PurchaseId,MasterSupplierId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] GoodsReceipt goodsReceipt)
        {
            goodsReceipt.Updated = DateTime.Now;
            goodsReceipt.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(goodsReceipt.Code)) goodsReceipt.Code = goodsReceipt.Code.ToUpper();
            if (!string.IsNullOrEmpty(goodsReceipt.Notes)) goodsReceipt.Notes = goodsReceipt.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                goodsReceipt = GetModelState(goodsReceipt);
            }

            db.Entry(goodsReceipt).State = EntityState.Unchanged;
            db.Entry(goodsReceipt).Property("Code").IsModified = true;
            db.Entry(goodsReceipt).Property("Date").IsModified = true;
            db.Entry(goodsReceipt).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(goodsReceipt).Property("MasterRegionId").IsModified = true;
            db.Entry(goodsReceipt).Property("MasterWarehouseId").IsModified = true;
            db.Entry(goodsReceipt).Property("Notes").IsModified = true;
            db.Entry(goodsReceipt).Property("Active").IsModified = true;
            db.Entry(goodsReceipt).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GoodsReceipt, MenuId = goodsReceipt.Id, MenuCode = goodsReceipt.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", goodsReceipt.MasterBusinessUnitId);
                
                return View("../Buying/GoodsReceipts/Edit", goodsReceipt);
            }
        }

        [HttpPost]
        [Authorize(Roles = "GoodsReceiptsDelete")]
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
                            GoodsReceipt obj = db.GoodsReceipts.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                GoodsReceipt tmp = obj;

                                var details = db.GoodsReceiptsDetails.Where(x => x.GoodsReceiptId == obj.Id).ToList();

                                foreach (var detail in details)
                                {
                                    SharedFunctions.UpdateStock(db, EnumMenuType.GoodsReceiptDetails, EnumActions.DELETE, obj.MasterBusinessUnitId, obj.MasterRegionId, obj.MasterWarehouseId, detail.MasterItemId, 0, detail.Id);

                                    db.GoodsReceiptsDetails.Remove(detail);
                                    db.SaveChanges();
                                }

                                db.GoodsReceipts.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GoodsReceipt, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "GoodsReceiptsPrint")]
        public ActionResult Print(int? id)
        {
            GoodsReceipt obj = db.GoodsReceipts.Find(id);

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

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GoodsReceipt, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.PRINT, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "GoodsReceiptsActive")]
        private GoodsReceipt GetModelState(GoodsReceipt purchase)
        {
            List<GoodsReceiptDetails> goodsReceiptDetails = db.GoodsReceiptsDetails.Where(x => x.GoodsReceiptId == purchase.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(purchase.Code, purchase.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (goodsReceiptDetails == null || goodsReceiptDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return purchase;
        }

        [Authorize(Roles = "GoodsReceiptsActive")]
        public ActionResult DetailsCreate(int goodsReceiptId)
        {
            GoodsReceipt goodsReceipt = db.GoodsReceipts.Find(goodsReceiptId);

            if (goodsReceipt == null)
            {
                return HttpNotFound();
            }

            GoodsReceiptDetails goodsReceiptDetails = new GoodsReceiptDetails
            {
                GoodsReceiptId = goodsReceiptId
            };

            return PartialView("../Buying/GoodsReceipts/_DetailsCreate", goodsReceiptDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GoodsReceiptsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,GoodsReceiptId,MasterItemId,MasterItemUnitId,Quantity,Notes,Created,Updated,UserId")] GoodsReceiptDetails goodsReceiptDetails)
        {
            goodsReceiptDetails.Created = DateTime.Now;
            goodsReceiptDetails.Updated = DateTime.Now;
            goodsReceiptDetails.UserId = User.Identity.GetUserId<int>();

            MasterItemUnit unit = db.MasterItemUnits.Find(goodsReceiptDetails.MasterItemUnitId);
            GoodsReceipt goodsReceipt = db.GoodsReceipts.Find(goodsReceiptDetails.GoodsReceiptId);

            if (!string.IsNullOrEmpty(goodsReceiptDetails.Notes)) goodsReceiptDetails.Notes = goodsReceiptDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.GoodsReceiptsDetails.Add(goodsReceiptDetails);
                        db.SaveChanges();

                        SharedFunctions.UpdateStock(db, EnumMenuType.GoodsReceiptDetails, EnumActions.CREATE, goodsReceipt.MasterBusinessUnitId, goodsReceipt.MasterRegionId, goodsReceipt.MasterWarehouseId, goodsReceiptDetails.MasterItemId, (int)(goodsReceiptDetails.Quantity * unit.MasterUnit.Ratio), goodsReceiptDetails.Id);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GoodsReceiptDetails, MenuId = goodsReceiptDetails.Id, MenuCode = goodsReceiptDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Buying/GoodsReceipts/_DetailsCreate", goodsReceiptDetails);
        }

        [Authorize(Roles = "GoodsReceiptsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            GoodsReceiptDetails obj = db.GoodsReceiptsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/GoodsReceipts/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GoodsReceiptsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,GoodsReceiptId,MasterItemId,MasterItemUnitId,Quantity,Notes,Created,Updated,UserId")] GoodsReceiptDetails goodsReceiptDetails)
        {
            GoodsReceipt goodsReceipt = db.GoodsReceipts.Find(goodsReceiptDetails.GoodsReceiptId);

            goodsReceiptDetails.Updated = DateTime.Now;
            goodsReceiptDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(goodsReceiptDetails.Notes)) goodsReceiptDetails.Notes = goodsReceiptDetails.Notes.ToUpper();

            db.Entry(goodsReceiptDetails).State = EntityState.Unchanged;
            db.Entry(goodsReceiptDetails).Property("MasterItemId").IsModified = true;
            db.Entry(goodsReceiptDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(goodsReceiptDetails).Property("Quantity").IsModified = true;
            db.Entry(goodsReceiptDetails).Property("Notes").IsModified = true;
            db.Entry(goodsReceiptDetails).Property("Updated").IsModified = true;
            db.Entry(goodsReceiptDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        MasterItemUnit unit = db.MasterItemUnits.Find(goodsReceiptDetails.MasterItemUnitId);
                        SharedFunctions.UpdateStock(db, EnumMenuType.GoodsReceiptDetails, EnumActions.EDIT, goodsReceipt.MasterBusinessUnitId, goodsReceipt.MasterRegionId, goodsReceipt.MasterWarehouseId, goodsReceiptDetails.MasterItemId, (int)(goodsReceiptDetails.Quantity * unit.MasterUnit.Ratio), goodsReceiptDetails.Id);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GoodsReceiptDetails, MenuId = goodsReceiptDetails.Id, MenuCode = goodsReceiptDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Buying/GoodsReceipts/_DetailsEdit", goodsReceiptDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "GoodsReceiptsActive")]
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
                        GoodsReceiptDetails obj = db.GoodsReceiptsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    GoodsReceiptDetails tmp = obj;
                                    GoodsReceipt goodsReceipt = db.GoodsReceipts.Find(obj.GoodsReceiptId);

                                    SharedFunctions.UpdateStock(db, EnumMenuType.GoodsReceiptDetails, EnumActions.DELETE, goodsReceipt.MasterBusinessUnitId, goodsReceipt.MasterRegionId, goodsReceipt.MasterWarehouseId, obj.MasterItemId, 0, obj.Id);

                                    db.GoodsReceiptsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GoodsReceiptDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "GoodsReceiptsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Buying/GoodsReceipts/_DetailsGrid", db.GoodsReceiptsDetails
                .Where(x => x.GoodsReceiptId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "GoodsReceiptsActive")]
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
        [Authorize(Roles = "GoodsReceiptsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            GoodsReceipt goodsReceipt = db.GoodsReceipts.Find(id);

            if (masterBusinessUnit != null && goodsReceipt != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                goodsReceipt.MasterBusinessUnitId = masterBusinessUnitId;
                goodsReceipt.MasterRegionId = masterRegionId;
                db.Entry(goodsReceipt).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "GoodsReceiptsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.GoodsReceiptCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            GoodsReceipt lastData = db.GoodsReceipts
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
        [Authorize(Roles = "GoodsReceiptsActive")]
        public JsonResult PopulateDetails(int goodsReceiptid, int purchaseId)
        {
            GoodsReceipt goodsReceipt = db.GoodsReceipts.Find(goodsReceiptid);
            Purchase purchase = db.Purchases.Find(purchaseId);

            if (goodsReceipt != null && purchase != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var remove = db.GoodsReceiptsDetails.Where(x => x.GoodsReceiptId == goodsReceipt.Id).ToList();

                        foreach (var obj in remove)
                        {
                            SharedFunctions.UpdateStock(db, EnumMenuType.GoodsReceiptDetails, EnumActions.DELETE, goodsReceipt.MasterBusinessUnitId, goodsReceipt.MasterRegionId, goodsReceipt.MasterWarehouseId, obj.MasterItemId, 0, obj.Id);

                            db.GoodsReceiptsDetails.Remove(obj);
                            db.SaveChanges();
                        }

                        var purchasesDetails = db.PurchasesDetails.Where(x => x.PurchaseId == purchase.Id).ToList();

                        if (purchasesDetails != null)
                        {
                            foreach (PurchaseDetails purchaseDetails in purchasesDetails)
                            {
                                GoodsReceiptDetails goodsReceiptDetails = new GoodsReceiptDetails
                                {
                                    GoodsReceiptId = goodsReceipt.Id,
                                    MasterItemId = purchaseDetails.MasterItemId,
                                    MasterItemUnitId = purchaseDetails.MasterItemUnitId,
                                    Quantity = purchaseDetails.Quantity,
                                    Notes = purchaseDetails.Notes,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.GoodsReceiptsDetails.Add(goodsReceiptDetails);
                                db.SaveChanges();

                                MasterItemUnit unit = db.MasterItemUnits.Find(goodsReceiptDetails.MasterItemUnitId);

                                SharedFunctions.UpdateStock(db, EnumMenuType.GoodsReceiptDetails, EnumActions.CREATE, goodsReceipt.MasterBusinessUnitId, goodsReceipt.MasterRegionId, goodsReceipt.MasterWarehouseId, goodsReceiptDetails.MasterItemId, (int)(goodsReceiptDetails.Quantity * unit.MasterUnit.Ratio), goodsReceiptDetails.Id);
                            }
                        }

                        goodsReceipt.PurchaseId = purchase.Id;
                        goodsReceipt.MasterBusinessUnitId = purchase.MasterBusinessUnitId;
                        goodsReceipt.MasterRegionId = purchase.MasterRegionId;
                        goodsReceipt.MasterSupplierId = purchase.MasterSupplierId;
                        goodsReceipt.MasterWarehouseId = purchase.MasterWarehouseId;
                        goodsReceipt.Notes = purchase.Notes;

                        db.Entry(goodsReceipt).State = EntityState.Modified;
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
                goodsReceipt.MasterRegionId,
                goodsReceipt.MasterBusinessUnitId,
                goodsReceipt.MasterSupplierId,
                goodsReceipt.MasterWarehouseId,
                goodsReceipt.Notes,
                goodsReceipt.Date
            });
        }



        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "GoodsReceiptsActive")]
        public JsonResult WarehouseChange(int goodsReceiptid, int masterWarehouseId)
        {
            GoodsReceipt goodsReceipt = db.GoodsReceipts.Find(goodsReceiptid);

            if(goodsReceipt != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        goodsReceipt.MasterWarehouseId = masterWarehouseId;

                        db.Entry(goodsReceipt).State = EntityState.Modified;
                        db.SaveChanges();

                        var goodsReceiptsDetails = db.GoodsReceiptsDetails.Where(x => x.GoodsReceiptId == goodsReceiptid);
                        foreach (GoodsReceiptDetails goodsReceiptDetails in goodsReceiptsDetails)
                        {
                            MasterItemUnit unit = db.MasterItemUnits.Find(goodsReceiptDetails.MasterItemUnitId);
                            SharedFunctions.UpdateStock(db, EnumMenuType.GoodsReceiptDetails, EnumActions.EDIT, goodsReceipt.MasterBusinessUnitId, goodsReceipt.MasterRegionId, goodsReceipt.MasterWarehouseId, goodsReceiptDetails.MasterItemId, (int)(goodsReceiptDetails.Quantity * unit.MasterUnit.Ratio), goodsReceiptDetails.Id);
                        }

                        dbTran.Commit();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }

                return Json("success");
            }

            return Json("Penerimaan Barang tidak ditemukan!");
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
