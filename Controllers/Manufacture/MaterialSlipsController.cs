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
    public class MaterialSlipsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MaterialSlips
        [Authorize(Roles = "MaterialSlipsActive")]
        public ActionResult Index()
        {
            return View("../Manufacture/MaterialSlips/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MaterialSlipsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Manufacture/MaterialSlips/_IndexGrid", db.Set<MaterialSlip>().AsQueryable());
            else
                return PartialView("../Manufacture/MaterialSlips/_IndexGrid", db.Set<MaterialSlip>().AsQueryable()
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
                return db.MaterialSlips.Any(x => x.Code == Code);
            }
            else
            {
                return db.MaterialSlips.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: MaterialSlips/Details/
        [Authorize(Roles = "MaterialSlipsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaterialSlip MaterialSlip = db.MaterialSlips.Find(id);
            if (MaterialSlip == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/MaterialSlips/_Details", MaterialSlip);
        }

        [HttpGet]
        [Authorize(Roles = "MaterialSlipsView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Manufacture/MaterialSlips/_ViewGrid", db.MaterialSlipsDetails
                .Where(x => x.MaterialSlipId == Id).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "MaterialSlipsActive")]
        public PartialViewResult BoMGrid(int Id)
        {
            return PartialView("../Manufacture/MaterialSlips/_BoMGrid", db.MaterialSlipsBillOfMaterials
                .Where(x => x.MaterialSlipId == Id).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "MaterialSlipsActive")]
        public PartialViewResult WoGrid(int Id)
        {
            return PartialView("../Manufacture/MaterialSlips/_WoGrid", db.MaterialSlipProductionWorkOrders
                .Where(x => x.MaterialSlipId == Id).ToList());
        }

        // GET: MaterialSlips/Create
        [Authorize(Roles = "MaterialSlipsAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            MaterialSlip materialSlip = new MaterialSlip
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                //MasterCurrencyId = masterCurrency.Id,
                //HeaderMasterItemUnitId = db.MasterItemUnits.FirstOrDefault().Id,
                //HeaderMasterItemId = db.MasterItems.FirstOrDefault().Id,
                //Rate = masterCurrency.Rate,
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
                    db.MaterialSlips.Add(materialSlip);
                    db.SaveChanges();

                    dbTran.Commit();

                    materialSlip.Code = "";
                    materialSlip.Active = true;
                    materialSlip.MasterBusinessUnitId = 0;
                    materialSlip.MasterRegionId = 0;
                    //materialSlip.HeaderMasterItemId = 0;
                    //materialSlip.HeaderMasterItemUnitId = 0;
                    materialSlip.MasterWarehouseId = 0;
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

            return View("../Manufacture/MaterialSlips/Create", materialSlip);
        }

        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MaterialSlipsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,ProductionWorkOrderId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] MaterialSlip materialSlip)
        {
            materialSlip.Created = DateTime.Now;
            materialSlip.Updated = DateTime.Now;
            materialSlip.UserId = User.Identity.GetUserId<int>();
            materialSlip.Total = SharedFunctions.GetTotalMaterialSlip(db, materialSlip.Id);

            if (!string.IsNullOrEmpty(materialSlip.Code)) materialSlip.Code = materialSlip.Code.ToUpper();
            if (!string.IsNullOrEmpty(materialSlip.Notes)) materialSlip.Notes = materialSlip.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                materialSlip = GetModelState(materialSlip);
            }

            db.Entry(materialSlip).State = EntityState.Unchanged;
            db.Entry(materialSlip).Property("Code").IsModified = true;
            db.Entry(materialSlip).Property("Date").IsModified = true;
            db.Entry(materialSlip).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(materialSlip).Property("MasterRegionId").IsModified = true;
            db.Entry(materialSlip).Property("ProductionWorkOrderId").IsModified = true;
            db.Entry(materialSlip).Property("MasterWarehouseId").IsModified = true;
            db.Entry(materialSlip).Property("Total").IsModified = true;
            db.Entry(materialSlip).Property("Notes").IsModified = true;
            db.Entry(materialSlip).Property("Active").IsModified = true;
            db.Entry(materialSlip).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MaterialSlip, MenuId = materialSlip.Id, MenuCode = materialSlip.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", materialSlip.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalMaterialSlip(db, materialSlip.Id).ToString("N2");

                return View("../Manufacture/MaterialSlips/Create", materialSlip);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialSlipsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                MaterialSlip obj = db.MaterialSlips.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.MaterialSlipsDetails.Where(x => x.MaterialSlipId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.MaterialSlipsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.MaterialSlips.Remove(obj);
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

        // GET: MaterialSlips/Edit/5
        [Authorize(Roles = "MaterialSlipsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaterialSlip materialSlip = db.MaterialSlips.Find(id);
            if (materialSlip == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", materialSlip.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalMaterialSlip(db, materialSlip.Id).ToString("N2");

            return View("../Manufacture/MaterialSlips/Edit", materialSlip);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MaterialSlipsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] MaterialSlip materialSlip)
        {
            materialSlip.Updated = DateTime.Now;
            materialSlip.UserId = User.Identity.GetUserId<int>();
            materialSlip.Total = SharedFunctions.GetTotalMaterialSlip(db, materialSlip.Id);

            if (!string.IsNullOrEmpty(materialSlip.Code)) materialSlip.Code = materialSlip.Code.ToUpper();
            if (!string.IsNullOrEmpty(materialSlip.Notes)) materialSlip.Notes = materialSlip.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                materialSlip = GetModelState(materialSlip);
            }

            db.Entry(materialSlip).State = EntityState.Unchanged;
            db.Entry(materialSlip).Property("Code").IsModified = true;
            db.Entry(materialSlip).Property("Date").IsModified = true;
            db.Entry(materialSlip).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(materialSlip).Property("MasterRegionId").IsModified = true;
            db.Entry(materialSlip).Property("MasterWarehouseId").IsModified = true;
            db.Entry(materialSlip).Property("Total").IsModified = true;
            db.Entry(materialSlip).Property("Notes").IsModified = true;
            db.Entry(materialSlip).Property("Active").IsModified = true;
            db.Entry(materialSlip).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MaterialSlip, MenuId = materialSlip.Id, MenuCode = materialSlip.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", materialSlip.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalMaterialSlip(db, materialSlip.Id).ToString("N2");

                return View("../Manufacture/MaterialSlips/Edit", materialSlip);
            }
        }

        [HttpPost]
        [Authorize(Roles = "MaterialSlipsDelete")]
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
                            MaterialSlip obj = db.MaterialSlips.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                MaterialSlip tmp = obj;

                                var details = db.MaterialSlipsDetails.Where(x => x.MaterialSlipId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.MaterialSlipsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                var productionWorkOrders = db.MaterialSlipProductionWorkOrders.Where(x => x.MaterialSlipId == obj.Id).ToList();

                                if (productionWorkOrders != null)
                                {
                                    db.MaterialSlipProductionWorkOrders.RemoveRange(productionWorkOrders);
                                    db.SaveChanges();
                                }

                                var billOfMaterials = db.MaterialSlipsBillOfMaterials.Where(x => x.MaterialSlipId == obj.Id).ToList();

                                if (billOfMaterials != null)
                                {
                                    db.MaterialSlipsBillOfMaterials.RemoveRange(billOfMaterials);
                                    db.SaveChanges();
                                }

                                db.MaterialSlips.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MaterialSlip, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "MaterialSlipsPrint")]
        public ActionResult Print(int? id)
        {
            MaterialSlip obj = db.MaterialSlips.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormMaterialSlip.rpt"));
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

        [Authorize(Roles = "MaterialSlipsActive")]
        private MaterialSlip GetModelState(MaterialSlip materialSlip)
        {
            List<MaterialSlipDetails> materialSlipDetails = db.MaterialSlipsDetails.Where(x => x.MaterialSlipId == materialSlip.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(materialSlip.Code, materialSlip.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (materialSlipDetails == null || materialSlipDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return materialSlip;
        }

        [Authorize(Roles = "MaterialSlipsActive")]
        public ActionResult WorkOrdersCreate(int materialSlipId)
        {
            MaterialSlip materialSlip = db.MaterialSlips.Find(materialSlipId);

            if (materialSlip == null)
            {
                return HttpNotFound();
            }

            MaterialSlipProductionWorkOrder materialSlipProductionWorkOrder = new MaterialSlipProductionWorkOrder
            {
                MaterialSlipId = materialSlipId
            };

            return PartialView("../Manufacture/MaterialSlips/_WorkOrdersCreate", materialSlipProductionWorkOrder);
        }

        [Authorize(Roles = "MaterialSlipsActive")]
        public ActionResult DetailsCreate(int materialSlipId)
        {
            MaterialSlip materialSlip = db.MaterialSlips.Find(materialSlipId);

            if (materialSlip == null)
            {
                return HttpNotFound();
            }

            MaterialSlipDetails materialSlipDetails = new MaterialSlipDetails
            {
                MaterialSlipId = materialSlipId
            };

            return PartialView("../Manufacture/MaterialSlips/_DetailsCreate", materialSlipDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public ActionResult WorkOrdersCreate([Bind(Include = "Id,MaterialSlipId,ProductionWorkOrderId,Notes,Created,Updated,UserId")] MaterialSlipProductionWorkOrder obj)
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
                        db.MaterialSlipProductionWorkOrders.Add(obj);
                        db.SaveChanges();

                        var productionWorkOrderBillOfMaterials = db.ProductionWorkOrderBillOfMaterials
                        .Where(p => p.ProductionWorkOrderId == obj.ProductionWorkOrderId)
                        .ToList();

                        if (productionWorkOrderBillOfMaterials.Count > 0)
                        {
                            foreach (var productionWorkOrderBillOfMaterial in productionWorkOrderBillOfMaterials)
                            {
                                // Now, set the ProductionBillOfMaterialId in MaterialSlipBillOfMaterials
                                MaterialSlipBillOfMaterial materialSlipBillOfMaterial = new MaterialSlipBillOfMaterial
                                {
                                    MaterialSlipId = obj.MaterialSlipId,
                                    ProductionBillOfMaterialId = productionWorkOrderBillOfMaterial.ProductionBillOfMaterialId,
                                    Quantity = productionWorkOrderBillOfMaterial.Quantity,
                                    //MasterItemId = productionWorkOrderBillOfMaterial.
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MaterialSlipsBillOfMaterials.Add(materialSlipBillOfMaterial);
                                db.SaveChanges();
                            }
                        }

                        var productionWorkOrdersDetails = db.ProductionWorkOrdersDetails.Where(x => x.ProductionWorkOrderId == obj.ProductionWorkOrderId).ToList();
                        if (productionWorkOrdersDetails.Count > 0)
                        {
                            foreach (ProductionWorkOrderDetails productionWorkOrderDetails in productionWorkOrdersDetails)
                            {
                                MaterialSlipDetails materialSlipDetails = new MaterialSlipDetails
                                {
                                    MaterialSlipId = obj.MaterialSlipId,
                                    MaterialSlipProductionWorkOrderId = obj.Id,
                                    MasterItemId = productionWorkOrderDetails.MasterItemId,
                                    MasterItemUnitId = productionWorkOrderDetails.MasterItemUnitId,
                                    Quantity = productionWorkOrderDetails.Quantity,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MaterialSlipsDetails.Add(materialSlipDetails);
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

            return PartialView("../Manufacture/ProductionWorkOrders/_WorkOrdersCreate", obj);
        }



        [Authorize(Roles = "MaterialSlipsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MaterialSlipDetails obj = db.MaterialSlipsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/MaterialSlips/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MaterialSlipsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,MaterialSlipId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] MaterialSlipDetails materialSlipDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(materialSlipDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                materialSlipDetails.Total = 0;
            else
                materialSlipDetails.Total = materialSlipDetails.Quantity * materialSlipDetails.Price * masterItemUnit.MasterUnit.Ratio;

            materialSlipDetails.Updated = DateTime.Now;
            materialSlipDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(materialSlipDetails.Notes)) materialSlipDetails.Notes = materialSlipDetails.Notes.ToUpper();

            db.Entry(materialSlipDetails).State = EntityState.Unchanged;
            db.Entry(materialSlipDetails).Property("MasterItemId").IsModified = true;
            db.Entry(materialSlipDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(materialSlipDetails).Property("Quantity").IsModified = true;
            db.Entry(materialSlipDetails).Property("Price").IsModified = true;
            db.Entry(materialSlipDetails).Property("Total").IsModified = true;
            db.Entry(materialSlipDetails).Property("Notes").IsModified = true;
            db.Entry(materialSlipDetails).Property("Updated").IsModified = true;
            db.Entry(materialSlipDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        MaterialSlip materialSlip = db.MaterialSlips.Find(materialSlipDetails.MaterialSlipId);
                        materialSlip.Total = SharedFunctions.GetTotalMaterialSlip(db, materialSlip.Id, materialSlipDetails.Id) + materialSlipDetails.Total;

                        db.Entry(materialSlip).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MaterialSlipDetails, MenuId = materialSlipDetails.Id, MenuCode = materialSlipDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Manufacture/MaterialSlips/_DetailsEdit", materialSlipDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialSlipsActive")]
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
                        MaterialSlipDetails obj = db.MaterialSlipsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MaterialSlipDetails tmp = obj;

                                    MaterialSlip materialSlip = db.MaterialSlips.Find(tmp.MaterialSlipId);

                                    materialSlip.Total = SharedFunctions.GetTotalMaterialSlip(db, materialSlip.Id, tmp.Id);

                                    db.Entry(materialSlip).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.MaterialSlipsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MaterialSlipDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "MaterialSlipsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Manufacture/MaterialSlips/_DetailsGrid", db.MaterialSlipsDetails
                .Where(x => x.MaterialSlipId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialSlipsActive")]
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

        [Authorize(Roles = "MaterialSlipsActive")]
        public ActionResult ChangeCurrency(int? materialSlipId)
        {
            if (materialSlipId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MaterialSlip materialSlip = db.MaterialSlips.Find(materialSlipId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = materialSlip.Id,
                //MasterCurrencyId = materialSlip.MasterCurrencyId,
                //Rate = materialSlip.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Manufacture/MaterialSlips/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialSlipsActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }


        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialSlipsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            MaterialSlip materialSlip = db.MaterialSlips.Find(id);

            if (masterBusinessUnit != null && materialSlip != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                materialSlip.MasterBusinessUnitId = masterBusinessUnitId;
                materialSlip.MasterRegionId = masterRegionId;
                db.Entry(materialSlip).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "MaterialSlipsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.MaterialSlipCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            MaterialSlip lastData = db.MaterialSlips
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
        [Authorize(Roles = "MaterialSlipsActive")]
        public JsonResult GetTotal(int materialSlipId)
        {
            return Json(SharedFunctions.GetTotalMaterialSlip(db, materialSlipId).ToString("N2"));
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialSlipsActive")]
        public JsonResult PopulateDetails(int materialSlipid, int productionWorkOrderId)
        {
            MaterialSlip materialSlip = db.MaterialSlips.Find(materialSlipid);
            ProductionWorkOrder productionWorkOrder = db.ProductionWorkOrders.Find(productionWorkOrderId);

            if (materialSlip != null && productionWorkOrder != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var remove = db.MaterialSlipsDetails.Where(x => x.MaterialSlipId == materialSlip.Id).ToList();

                        if (remove != null)
                        {
                            db.MaterialSlipsDetails.RemoveRange(remove);
                            db.SaveChanges();
                        }

                        var productionWorkOrdersDetails = db.ProductionWorkOrdersDetails.Where(x => x.ProductionWorkOrderId == productionWorkOrder.Id).ToList();

                        if (productionWorkOrdersDetails != null)
                        {
                            foreach (ProductionWorkOrderDetails productionWorkOrderDetails in productionWorkOrdersDetails)
                            {
                                MaterialSlipDetails materialSlipDetails = new MaterialSlipDetails
                                {
                                    MaterialSlipId = materialSlip.Id,
                                    MasterItemId = productionWorkOrderDetails.MasterItemId,
                                    MasterItemUnitId = productionWorkOrderDetails.MasterItemUnitId,
                                    Quantity = productionWorkOrderDetails.Quantity,
                                    Notes = productionWorkOrderDetails.Notes,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MaterialSlipsDetails.Add(materialSlipDetails);
                                db.SaveChanges();
                            }
                        }

                        materialSlip.ProductionWorkOrderId = productionWorkOrder.Id;
                        materialSlip.MasterBusinessUnitId = productionWorkOrder.MasterBusinessUnitId;
                        materialSlip.MasterRegionId = productionWorkOrder.MasterRegionId;
                        materialSlip.Notes = productionWorkOrder.Notes;
                        //materialSlip.Total = productionWorkOrder.Total;

                        db.Entry(materialSlip).State = EntityState.Modified;
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
                materialSlip.MasterRegionId,
                materialSlip.MasterBusinessUnitId,
                //materialSlip.MasterWarehouseId,
                //materialSlip.HeaderQuantity,
                //materialSlip.HeaderMasterItemUnitId,
                //materialSlip.HeaderMasterItemId,
                materialSlip.Notes,
                //Total = materialSlip.Total.ToString("N2"),
                materialSlip.Date,
                //Currency = materialSlip.MasterCurrency.Code + " : " + materialSlip.Rate.ToString("N2")
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
