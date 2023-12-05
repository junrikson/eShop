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
    public class FinishedGoodSlipsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: FinishedGoodSlips
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public ActionResult Index()
        {
            return View("../Manufacture/FinishedGoodSlips/Index");
        }

        [HttpGet]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Manufacture/FinishedGoodSlips/_IndexGrid", db.Set<FinishedGoodSlip>().AsQueryable());
            else
                return PartialView("../Manufacture/FinishedGoodSlips/_IndexGrid", db.Set<FinishedGoodSlip>().AsQueryable()
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
                return db.FinishedGoodSlips.Any(x => x.Code == Code);
            }
            else
            {
                return db.FinishedGoodSlips.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: FinishedGoodSlips/Details/
        [Authorize(Roles = "FinishedGoodSlipsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinishedGoodSlip FinishedGoodSlip = db.FinishedGoodSlips.Find(id);
            if (FinishedGoodSlip == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/FinishedGoodSlips/_Details", FinishedGoodSlip);
        }

        [HttpGet]
        [Authorize(Roles = "FinishedGoodSlipsView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Manufacture/FinishedGoodSlips/_ViewGrid", db.FinishedGoodSlipsDetails
                .Where(x => x.FinishedGoodSlipId == Id).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "MaterialSlipsActive")]
        public PartialViewResult WoGrid(int Id)
        {
            return PartialView("../Manufacture/FinishedGoodSlips/_WoGrid", db.FinishedGoodSlipProductionWorkOrders
                .Where(x => x.FinishedGoodSlipId == Id).ToList());
        }

        // GET: FinishedGoodSlips/Create
        [Authorize(Roles = "FinishedGoodSlipsAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            FinishedGoodSlip finishedGoodSlip = new FinishedGoodSlip
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
                    db.FinishedGoodSlips.Add(finishedGoodSlip);
                    db.SaveChanges();

                    dbTran.Commit();

                    finishedGoodSlip.Code = "";
                    finishedGoodSlip.Active = true;
                    finishedGoodSlip.MasterBusinessUnitId = 0;
                    finishedGoodSlip.MasterRegionId = 0;
                    finishedGoodSlip.MasterWarehouseId = 0;
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

            return View("../Manufacture/FinishedGoodSlips/Create", finishedGoodSlip);
        }

        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] FinishedGoodSlip finishedGoodSlip)
        {
            finishedGoodSlip.Created = DateTime.Now;
            finishedGoodSlip.Updated = DateTime.Now;
            finishedGoodSlip.UserId = User.Identity.GetUserId<int>();
            //finishedGoodSlip.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id);

            if (!string.IsNullOrEmpty(finishedGoodSlip.Code)) finishedGoodSlip.Code = finishedGoodSlip.Code.ToUpper();
            if (!string.IsNullOrEmpty(finishedGoodSlip.Notes)) finishedGoodSlip.Notes = finishedGoodSlip.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                finishedGoodSlip = GetModelState(finishedGoodSlip);
            }

            db.Entry(finishedGoodSlip).State = EntityState.Unchanged;
            db.Entry(finishedGoodSlip).Property("Code").IsModified = true;
            db.Entry(finishedGoodSlip).Property("Date").IsModified = true;
            db.Entry(finishedGoodSlip).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(finishedGoodSlip).Property("MasterRegionId").IsModified = true;
            //db.Entry(finishedGoodSlip).Property("ProductionWorkOrderId").IsModified = true;
            db.Entry(finishedGoodSlip).Property("MasterWarehouseId").IsModified = true;
            //db.Entry(finishedGoodSlip).Property("Total").IsModified = true;
            db.Entry(finishedGoodSlip).Property("Notes").IsModified = true;
            db.Entry(finishedGoodSlip).Property("Active").IsModified = true;
            db.Entry(finishedGoodSlip).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.FinishedGoodSlip, MenuId = finishedGoodSlip.Id, MenuCode = finishedGoodSlip.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", finishedGoodSlip.MasterBusinessUnitId);
                //ViewBag.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id).ToString("N2");

                return View("../Manufacture/FinishedGoodSlips/Create", finishedGoodSlip);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                FinishedGoodSlip obj = db.FinishedGoodSlips.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.FinishedGoodSlipsDetails.Where(x => x.FinishedGoodSlipId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.FinishedGoodSlipsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                var finishedGoodSlipProductionWorkOrders = db.FinishedGoodSlipProductionWorkOrders.Where(x => x.FinishedGoodSlipId == obj.Id).ToList();

                                if (finishedGoodSlipProductionWorkOrders != null)
                                {
                                    db.FinishedGoodSlipProductionWorkOrders.RemoveRange(finishedGoodSlipProductionWorkOrders);
                                    db.SaveChanges();
                                }

                                db.FinishedGoodSlips.Remove(obj);
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

        // GET: FinishedGoodSlips/Edit/5
        [Authorize(Roles = "FinishedGoodSlipsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinishedGoodSlip finishedGoodSlip = db.FinishedGoodSlips.Find(id);
            if (finishedGoodSlip == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", finishedGoodSlip.MasterBusinessUnitId);
            //ViewBag.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id).ToString("N2");

            return View("../Manufacture/FinishedGoodSlips/Edit", finishedGoodSlip);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterWarehouseId,MasterCustomerId,Notes,Active,Created,Updated,UserId")] FinishedGoodSlip finishedGoodSlip)
        {
            finishedGoodSlip.Updated = DateTime.Now;
            finishedGoodSlip.UserId = User.Identity.GetUserId<int>();
            //finishedGoodSlip.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id);

            if (!string.IsNullOrEmpty(finishedGoodSlip.Code)) finishedGoodSlip.Code = finishedGoodSlip.Code.ToUpper();
            if (!string.IsNullOrEmpty(finishedGoodSlip.Notes)) finishedGoodSlip.Notes = finishedGoodSlip.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                finishedGoodSlip = GetModelState(finishedGoodSlip);
            }

            db.Entry(finishedGoodSlip).State = EntityState.Unchanged;
            db.Entry(finishedGoodSlip).Property("Code").IsModified = true;
            db.Entry(finishedGoodSlip).Property("Date").IsModified = true;
            db.Entry(finishedGoodSlip).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(finishedGoodSlip).Property("MasterRegionId").IsModified = true;
            db.Entry(finishedGoodSlip).Property("MasterWarehouseId").IsModified = true;
            //db.Entry(finishedGoodSlip).Property("Total").IsModified = true;
            db.Entry(finishedGoodSlip).Property("Notes").IsModified = true;
            db.Entry(finishedGoodSlip).Property("Active").IsModified = true;
            db.Entry(finishedGoodSlip).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.FinishedGoodSlip, MenuId = finishedGoodSlip.Id, MenuCode = finishedGoodSlip.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", finishedGoodSlip.MasterBusinessUnitId);
                //ViewBag.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id).ToString("N2");

                return View("../Manufacture/FinishedGoodSlips/Edit", finishedGoodSlip);
            }
        }



        [HttpPost]
        [Authorize(Roles = "FinishedGoodSlipsDelete")]
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
                            FinishedGoodSlip obj = db.FinishedGoodSlips.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                FinishedGoodSlip tmp = obj;

                                var details = db.FinishedGoodSlipsDetails.Where(x => x.FinishedGoodSlipId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.FinishedGoodSlipsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                var finishedGoodSlipProductionWorkOrders = db.FinishedGoodSlipProductionWorkOrders.Where(x => x.FinishedGoodSlipId == obj.Id).ToList();

                                if (finishedGoodSlipProductionWorkOrders != null)
                                {
                                    db.FinishedGoodSlipProductionWorkOrders.RemoveRange(finishedGoodSlipProductionWorkOrders);
                                    db.SaveChanges();
                                }

                                db.FinishedGoodSlips.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.FinishedGoodSlip, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "FinishedGoodSlipsPrint")]
        public ActionResult Print(int? id)
        {
            FinishedGoodSlip obj = db.FinishedGoodSlips.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormFinishedGoodSlip.rpt"));
                        rd.SetParameterValue("Code", obj.Code);
                       // rd.SetParameterValue("Terbilang", "# " + TerbilangExtension.Terbilang(Math.Floor(obj.Total)).ToUpper() + " RUPIAH #");

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

        [Authorize(Roles = "FinishedGoodSlipsActive")]
        private FinishedGoodSlip GetModelState(FinishedGoodSlip finishedGoodSlip)
        {
            List<FinishedGoodSlipDetails> finishedGoodSlipDetails = db.FinishedGoodSlipsDetails.Where(x => x.FinishedGoodSlipId == finishedGoodSlip.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(finishedGoodSlip.Code, finishedGoodSlip.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (finishedGoodSlipDetails == null || finishedGoodSlipDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return finishedGoodSlip;
        }

        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public ActionResult DetailsCreate(int finishedGoodSlipId)
        {
            FinishedGoodSlip finishedGoodSlip = db.FinishedGoodSlips.Find(finishedGoodSlipId);

            if (finishedGoodSlip == null)
            {
                return HttpNotFound();
            }

            FinishedGoodSlipDetails finishedGoodSlipDetails = new FinishedGoodSlipDetails
            {
                FinishedGoodSlipId = finishedGoodSlipId
            };

            return PartialView("../Manufacture/FinishedGoodSlips/_DetailsCreate", finishedGoodSlipDetails);
        }

        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public ActionResult WorkOrdersCreate(int finishedGoodSlipId)
        {
            FinishedGoodSlip finishedGoodSlip = db.FinishedGoodSlips.Find(finishedGoodSlipId);

            if (finishedGoodSlip == null)
            {
                return HttpNotFound();
            }

            FinishedGoodSlipProductionWorkOrder finishedGoodSlipProductionWorkOrder = new FinishedGoodSlipProductionWorkOrder
            {
                FinishedGoodSlipId = finishedGoodSlipId
            };

            return PartialView("../Manufacture/FinishedGoodSlips/_WorkOrdersCreate", finishedGoodSlipProductionWorkOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public ActionResult WorkOrdersCreate([Bind(Include = "Id,FinishedGoodSlipId,ProductionWorkOrderId,Notes,Created,Updated,UserId")] FinishedGoodSlipProductionWorkOrder obj)
        {
            var productionWorkOrder = db.ProductionWorkOrders.Find(obj.ProductionWorkOrderId);
            if (productionWorkOrder != null)
            {
                obj.Created = productionWorkOrder.Created;
                obj.Updated = productionWorkOrder.Updated;
            }

            obj.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(obj.Notes)) obj.Notes = obj.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.FinishedGoodSlipProductionWorkOrders.Add(obj);
                        db.SaveChanges();

                        var productionWorkOrderBillOfMaterials = db.ProductionWorkOrderBillOfMaterials
                        .Where(p => p.ProductionWorkOrderId == obj.ProductionWorkOrderId)
                        .ToList();

                        //var productionWorkOrdersDetails = db.ProductionWorkOrdersDetails.Where(x => x.ProductionWorkOrderId == obj.ProductionWorkOrderId).ToList();
                        if (productionWorkOrderBillOfMaterials.Count > 0)
                        {
                            foreach (ProductionWorkOrderBillOfMaterial productionWorkOrderBillOfMaterial in productionWorkOrderBillOfMaterials)
                            {
                                FinishedGoodSlipDetails finishedGoodSlipDetails = new FinishedGoodSlipDetails
                                {
                                    FinishedGoodSlipId = obj.FinishedGoodSlipId,
                                    MasterItemId = productionWorkOrderBillOfMaterial.ProductionBillOfMaterial.MasterItemId,
                                    MasterItemUnitId = productionWorkOrderBillOfMaterial.ProductionBillOfMaterial.MasterItemUnitId,
                                    Quantity = productionWorkOrderBillOfMaterial.Quantity,
                                    QuantitySpk = productionWorkOrderBillOfMaterial.Quantity,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.FinishedGoodSlipsDetails.Add(finishedGoodSlipDetails);
                                db.SaveChanges();
                            }
                        }

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionWorkOrderDetails, MenuId = obj.Id, MenuCode = obj.ProductionWorkOrderId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Manufacture/FinishedGoodSlips/_WorkOrdersCreate", obj);
        }


        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            FinishedGoodSlipDetails obj = db.FinishedGoodSlipsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/FinishedGoodSlips/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,FinishedGoodSlipId,MasterItemId,MasterItemUnitId,QuantitySpk,Quantity,Notes,Created,Updated,UserId")] FinishedGoodSlipDetails finishedGoodSlipDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(finishedGoodSlipDetails.MasterItemUnitId);

            finishedGoodSlipDetails.Updated = DateTime.Now;
            finishedGoodSlipDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(finishedGoodSlipDetails.Notes)) finishedGoodSlipDetails.Notes = finishedGoodSlipDetails.Notes.ToUpper();

            db.Entry(finishedGoodSlipDetails).State = EntityState.Unchanged;
            db.Entry(finishedGoodSlipDetails).Property("MasterItemId").IsModified = true;
            db.Entry(finishedGoodSlipDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(finishedGoodSlipDetails).Property("QuantitySpk").IsModified = true;
            db.Entry(finishedGoodSlipDetails).Property("Quantity").IsModified = true;
            db.Entry(finishedGoodSlipDetails).Property("Notes").IsModified = true;
            db.Entry(finishedGoodSlipDetails).Property("Updated").IsModified = true;
            db.Entry(finishedGoodSlipDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        FinishedGoodSlip finishedGoodSlip = db.FinishedGoodSlips.Find(finishedGoodSlipDetails.FinishedGoodSlipId);
                        //finishedGoodSlip.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id, finishedGoodSlipDetails.Id) + finishedGoodSlipDetails.Total;

                        db.Entry(finishedGoodSlip).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.FinishedGoodSlipDetails, MenuId = finishedGoodSlipDetails.Id, MenuCode = finishedGoodSlipDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Manufacture/FinishedGoodSlips/_DetailsEdit", finishedGoodSlipDetails);
        }

        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public ActionResult WorkOrdersEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            FinishedGoodSlipProductionWorkOrder obj = db.FinishedGoodSlipProductionWorkOrders.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/FinishedGoodSlips/_WorkOrdersEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public ActionResult WorkOrdersEdit([Bind(Include = "Id,FinishedGoodSlipId,ProductionWorkOrderId,Created,Updated,UserId")] FinishedGoodSlipProductionWorkOrder obj)
        {
            obj.Updated = DateTime.Now;
            obj.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(obj.Notes)) obj.Notes = obj.Notes.ToUpper();

            db.Entry(obj).State = EntityState.Unchanged;
            db.Entry(obj).Property("ProductionWorkOrderId").IsModified = true;
            db.Entry(obj).Property("Updated").IsModified = true;
            db.Entry(obj).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        FinishedGoodSlipProductionWorkOrder finishedGoodSlipProductionWorkOrder = db.FinishedGoodSlipProductionWorkOrders.Find(obj.FinishedGoodSlipId);

                        db.Entry(finishedGoodSlipProductionWorkOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        //db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.FinishedGoodSlipDetails, MenuId = finishedGoodSlipDetails.Id, MenuCode = finishedGoodSlipDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Manufacture/FinishedGoodSlips/_WorkOrdersEdit", obj);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
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
                        FinishedGoodSlipDetails obj = db.FinishedGoodSlipsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    FinishedGoodSlipDetails tmp = obj;

                                    FinishedGoodSlip finishedGoodSlip = db.FinishedGoodSlips.Find(tmp.FinishedGoodSlipId);

                                   // finishedGoodSlip.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id, tmp.Id);

                                    db.Entry(finishedGoodSlip).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.FinishedGoodSlipsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.FinishedGoodSlipDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Manufacture/FinishedGoodSlips/_DetailsGrid", db.FinishedGoodSlipsDetails
                .Where(x => x.FinishedGoodSlipId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
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
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }


        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            FinishedGoodSlip finishedGoodSlip = db.FinishedGoodSlips.Find(id);

            if (masterBusinessUnit != null && finishedGoodSlip != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                finishedGoodSlip.MasterBusinessUnitId = masterBusinessUnitId;
                finishedGoodSlip.MasterRegionId = masterRegionId;
                db.Entry(finishedGoodSlip).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "FinishedGoodSlipsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.FinishedGoodSlipCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            FinishedGoodSlip lastData = db.FinishedGoodSlips
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
