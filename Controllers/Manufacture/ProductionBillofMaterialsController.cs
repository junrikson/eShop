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
    public class ProductionBillOfMaterialsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SalesRequests
        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public ActionResult Index()
        {
            return View("../Manufacture/ProductionBillOfMaterials/Index");
        }

        [HttpGet]
        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Manufacture/ProductionBillOfMaterials/_IndexGrid", db.Set<ProductionBillOfMaterial>().AsQueryable());
            else
                return PartialView("../Manufacture/ProductionBillOfMaterials/_IndexGrid", db.Set<ProductionBillOfMaterial>().AsQueryable()
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
                return db.ProductionBillOfMaterials.Any(x => x.Code == Code);
            }
            else
            {
                return db.ProductionBillOfMaterials.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: SalesRequests/Details/
        [Authorize(Roles = "ProductionBillOfMaterialsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductionBillOfMaterial ProductionBillOfMaterial = db.ProductionBillOfMaterials.Find(id);
            if (ProductionBillOfMaterial == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/ProductionBillOfMaterials/_Details", ProductionBillOfMaterial);
        }

        [HttpGet]
        [Authorize(Roles = "ProductionBillOfMaterialsView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Manufacture/ProductionBillOfMaterials/_ViewGrid", db.ProductionBillOfMaterialsDetails
                .Where(x => x.ProductionBillOfMaterialId == Id).ToList());
        }

        // GET: SalesRequests/Create
        [Authorize(Roles = "ProductionBillOfMaterialsAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            ProductionBillOfMaterial productionBillOfMaterial = new ProductionBillOfMaterial
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                Rate = masterCurrency.Rate,
                MasterItemId = db.MasterItems.FirstOrDefault().Id,
                MasterItemUnitId = db.MasterItemUnits.FirstOrDefault().Id,
                BillOfMaterialType = EnumBillOfMaterialType.FinishedGood,
                IsPrint = false,
                Active = false,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UserId = User.Identity.GetUserId<int>()
            };

            ProductionBillOfMaterialViewModel obj = new ProductionBillOfMaterialViewModel
            {
                Id = 0,
                Code = "",
                Date = DateTime.Now,
                Active = true,
                MasterBusinessUnitId = 0,
                MasterRegionId = 0,
                MasterItemUnitId = 0,
                HeaderMasterItemId = 0,
                BillOfMaterialType = EnumBillOfMaterialType.FinishedGood,
            };

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    db.ProductionBillOfMaterials.Add(productionBillOfMaterial);
                    db.SaveChanges();

                    dbTran.Commit();

                    obj.Id = productionBillOfMaterial.Id;
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

            return View("../Manufacture/ProductionBillOfMaterials/Create", obj);
        }

        // POST: PurchaseRequests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionBillOfMaterialsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterItemUnitId,HeaderMasterItemId,BillOfMaterialType,Notes,Active,Created,Updated,UserId")] ProductionBillOfMaterialViewModel obj)
        {
            ProductionBillOfMaterial productionBillOfMaterial = db.ProductionBillOfMaterials.Find(obj.Id);

            if (ModelState.IsValid)
            {
                productionBillOfMaterial = GetModelState(productionBillOfMaterial);
            }

            productionBillOfMaterial.Created = DateTime.Now;
            productionBillOfMaterial.Updated = DateTime.Now;
            productionBillOfMaterial.UserId = User.Identity.GetUserId<int>();
            productionBillOfMaterial.Code = obj.Code.ToUpper();
            productionBillOfMaterial.Notes = string.IsNullOrEmpty(obj.Notes) ? "" : obj.Notes.ToUpper();
            productionBillOfMaterial.Date = obj.Date;
            productionBillOfMaterial.MasterBusinessUnitId = obj.MasterBusinessUnitId;
            productionBillOfMaterial.MasterRegionId = obj.MasterRegionId;
            productionBillOfMaterial.MasterItemUnitId = obj.MasterItemUnitId;
            productionBillOfMaterial.MasterItemId = obj.HeaderMasterItemId;
            productionBillOfMaterial.BillOfMaterialType = obj.BillOfMaterialType;
            productionBillOfMaterial.Active = obj.Active;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillOfMaterial, MenuId = productionBillOfMaterial.Id, MenuCode = productionBillOfMaterial.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", productionBillOfMaterial.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalProductionBillOfMaterial(db, productionBillOfMaterial.Id).ToString("N2");

                return View("../Manufacture/ProductionBillOfMaterials/Create", obj);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionBillOfMaterialsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                ProductionBillOfMaterial obj = db.ProductionBillOfMaterials.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.ProductionBillOfMaterialsDetails.Where(x => x.ProductionBillOfMaterialId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.ProductionBillOfMaterialsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.ProductionBillOfMaterials.Remove(obj);
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

        // GET: ProductionBillOfMaterials/Edit/5
        [Authorize(Roles = "ProductionBillOfMaterialsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductionBillOfMaterial productionBillOfMaterial = db.ProductionBillOfMaterials.Find(id);

            ProductionBillOfMaterialViewModel obj = new ProductionBillOfMaterialViewModel
            {
                Id = productionBillOfMaterial.Id,
                Code = productionBillOfMaterial.Code,
                Date = productionBillOfMaterial.Date,
                Active = productionBillOfMaterial.Active,
                MasterBusinessUnitId = productionBillOfMaterial.MasterBusinessUnitId,
                MasterRegionId = productionBillOfMaterial.MasterRegionId,
                MasterItemUnitId = productionBillOfMaterial.MasterItemUnitId,
                HeaderMasterItemId = productionBillOfMaterial.MasterItemId,
                BillOfMaterialType = productionBillOfMaterial.BillOfMaterialType,
                Notes = productionBillOfMaterial.Notes
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", productionBillOfMaterial.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalProductionBillOfMaterial(db, productionBillOfMaterial.Id).ToString("N2");

            return View("../Manufacture/ProductionBillOfMaterials/Edit", obj);
        }

        // POST: PurchaseRequests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionBillOfMaterialsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterItemUnitId,HeaderMasterItemId,BillOfMaterialType,Notes,Active,Created,Updated,UserId")] ProductionBillOfMaterialViewModel obj)
        {
            ProductionBillOfMaterial productionBillOfMaterial = db.ProductionBillOfMaterials.Find(obj.Id);

            if (ModelState.IsValid)
            {
                productionBillOfMaterial = GetModelState(productionBillOfMaterial);
            }

            productionBillOfMaterial.Updated = DateTime.Now;
            productionBillOfMaterial.UserId = User.Identity.GetUserId<int>();
            productionBillOfMaterial.Code = obj.Code.ToUpper();
            productionBillOfMaterial.Notes = string.IsNullOrEmpty(obj.Notes) ? "" : obj.Notes.ToUpper();
            productionBillOfMaterial.Date = obj.Date;
            productionBillOfMaterial.MasterBusinessUnitId = obj.MasterBusinessUnitId;
            productionBillOfMaterial.MasterRegionId = obj.MasterRegionId;
            productionBillOfMaterial.MasterItemUnitId = obj.MasterItemUnitId;
            productionBillOfMaterial.MasterItemId = obj.HeaderMasterItemId;
            productionBillOfMaterial.BillOfMaterialType = obj.BillOfMaterialType;
            productionBillOfMaterial.Active = obj.Active;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillOfMaterial, MenuId = productionBillOfMaterial.Id, MenuCode = productionBillOfMaterial.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", productionBillOfMaterial.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalProductionBillOfMaterial(db, obj.Id).ToString("N2");

                return View("../Manufacture/ProductionBillOfMaterials/Edit", obj);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ProductionBillOfMaterialsDelete")]
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
                            ProductionBillOfMaterial obj = db.ProductionBillOfMaterials.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                ProductionBillOfMaterial tmp = obj;

                                var details = db.ProductionBillOfMaterialsDetails.Where(x => x.ProductionBillOfMaterialId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.ProductionBillOfMaterialsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.ProductionBillOfMaterials.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillOfMaterial, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "ProductionBillOfMaterialsPrint")]
        public ActionResult Print(int? id)
        {
            ProductionBillOfMaterial obj = db.ProductionBillOfMaterials.Find(id);

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

        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        private ProductionBillOfMaterial GetModelState(ProductionBillOfMaterial productionBillOfMaterial)
        {
            List<ProductionBillOfMaterialDetails> productionBillOfMaterialDetails = db.ProductionBillOfMaterialsDetails.Where(x => x.ProductionBillOfMaterialId == productionBillOfMaterial.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(productionBillOfMaterial.Code, productionBillOfMaterial.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (productionBillOfMaterialDetails == null || productionBillOfMaterialDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return productionBillOfMaterial;
        }

        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public ActionResult DetailsCreate(int productionBillOfMaterialId)
        {
            ProductionBillOfMaterial productionBillOfMaterial = db.ProductionBillOfMaterials.Find(productionBillOfMaterialId);

            if (productionBillOfMaterial == null)
            {
                return HttpNotFound();
            }

            ProductionBillOfMaterialDetails productionBillOfMaterialDetails = new ProductionBillOfMaterialDetails
            {
                ProductionBillOfMaterialId = productionBillOfMaterialId
            };

            return PartialView("../Manufacture/ProductionBillOfMaterials/_DetailsCreate", productionBillOfMaterialDetails);
        }

        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public ActionResult CostsCreate(int productionBillOfMaterialId)
        {
            ProductionBillOfMaterial productionBillOfMaterial = db.ProductionBillOfMaterials.Find(productionBillOfMaterialId);

            if (productionBillOfMaterial == null)
            {
                return HttpNotFound();
            }

            ProductionBillOfMaterialCostDetails productionBillOfMaterialCostDetails = new ProductionBillOfMaterialCostDetails
            {
                ProductionBillOfMaterialId = productionBillOfMaterialId
            };

            return PartialView("../Manufacture/ProductionBillOfMaterials/_CostsCreate", productionBillOfMaterialCostDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,ProductionBillOfMaterialId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] ProductionBillOfMaterialDetails productionBillOfMaterialDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(productionBillOfMaterialDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                productionBillOfMaterialDetails.Total = 0;
            else
                productionBillOfMaterialDetails.Total = productionBillOfMaterialDetails.Quantity * productionBillOfMaterialDetails.Price * masterItemUnit.MasterUnit.Ratio;

            productionBillOfMaterialDetails.Created = DateTime.Now;
            productionBillOfMaterialDetails.Updated = DateTime.Now;
            productionBillOfMaterialDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(productionBillOfMaterialDetails.Notes)) productionBillOfMaterialDetails.Notes = productionBillOfMaterialDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.ProductionBillOfMaterialsDetails.Add(productionBillOfMaterialDetails);
                        db.SaveChanges();

                        ProductionBillOfMaterial productionBillOfMaterial = db.ProductionBillOfMaterials.Find(productionBillOfMaterialDetails.ProductionBillOfMaterialId);
                        productionBillOfMaterial.Total = SharedFunctions.GetTotalProductionBillOfMaterial(db, productionBillOfMaterial.Id, productionBillOfMaterialDetails.Id) + productionBillOfMaterialDetails.Total;

                        db.Entry(productionBillOfMaterial).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillOfMaterialDetails, MenuId = productionBillOfMaterialDetails.Id, MenuCode = productionBillOfMaterialDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Manufacture/ProductionBillOfMaterials/_DetailsCreate", productionBillOfMaterialDetails);
        }

        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductionBillOfMaterialDetails obj = db.ProductionBillOfMaterialsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/ProductionBillOfMaterials/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,ProductionBillOfMaterialId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] ProductionBillOfMaterialDetails productionBillOfMaterialDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(productionBillOfMaterialDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                productionBillOfMaterialDetails.Total = 0;
            else
                productionBillOfMaterialDetails.Total = productionBillOfMaterialDetails.Quantity * productionBillOfMaterialDetails.Price * masterItemUnit.MasterUnit.Ratio;

            productionBillOfMaterialDetails.Updated = DateTime.Now;
            productionBillOfMaterialDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(productionBillOfMaterialDetails.Notes)) productionBillOfMaterialDetails.Notes = productionBillOfMaterialDetails.Notes.ToUpper();

            db.Entry(productionBillOfMaterialDetails).State = EntityState.Unchanged;
            db.Entry(productionBillOfMaterialDetails).Property("MasterItemId").IsModified = true;
            db.Entry(productionBillOfMaterialDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(productionBillOfMaterialDetails).Property("Quantity").IsModified = true;
            db.Entry(productionBillOfMaterialDetails).Property("Price").IsModified = true;
            db.Entry(productionBillOfMaterialDetails).Property("Total").IsModified = true;
            db.Entry(productionBillOfMaterialDetails).Property("Notes").IsModified = true;
            db.Entry(productionBillOfMaterialDetails).Property("Updated").IsModified = true;
            db.Entry(productionBillOfMaterialDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        ProductionBillOfMaterial productionBillOfMaterial = db.ProductionBillOfMaterials.Find(productionBillOfMaterialDetails.ProductionBillOfMaterialId);
                        productionBillOfMaterial.Total = SharedFunctions.GetTotalProductionBillOfMaterial(db, productionBillOfMaterial.Id, productionBillOfMaterialDetails.Id) + productionBillOfMaterialDetails.Total;

                        db.Entry(productionBillOfMaterial).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillOfMaterialDetails, MenuId = productionBillOfMaterialDetails.Id, MenuCode = productionBillOfMaterialDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Manufacture/ProductionBillOfMaterials/_DetailsEdit", productionBillOfMaterialDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
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
                        ProductionBillOfMaterialDetails obj = db.ProductionBillOfMaterialsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    ProductionBillOfMaterialDetails tmp = obj;

                                    ProductionBillOfMaterial productionBillOfMaterial = db.ProductionBillOfMaterials.Find(tmp.ProductionBillOfMaterialId);

                                    productionBillOfMaterial.Total = SharedFunctions.GetTotalProductionBillOfMaterial(db, productionBillOfMaterial.Id, tmp.Id);

                                    db.Entry(productionBillOfMaterial).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.ProductionBillOfMaterialsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillOfMaterialDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Manufacture/ProductionBillOfMaterials/_DetailsGrid", db.ProductionBillOfMaterialsDetails
                .Where(x => x.ProductionBillOfMaterialId == Id).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public PartialViewResult CostsGrid(int Id)
        {
            return PartialView("../Manufacture/ProductionBillOfMaterials/_CostsGrid", db.ProductionBillOfMaterialsCostsDetails
                .Where(x => x.ProductionBillOfMaterialId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
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
        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            ProductionBillOfMaterial productionBillOfMaterial = db.ProductionBillOfMaterials.Find(id);

            if (masterBusinessUnit != null && productionBillOfMaterial != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                productionBillOfMaterial.MasterBusinessUnitId = masterBusinessUnitId;
                productionBillOfMaterial.MasterRegionId = masterRegionId;
                db.Entry(productionBillOfMaterial).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.ProductionBillOfMaterialCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            ProductionBillOfMaterial lastData = db.ProductionBillOfMaterials
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
        [Authorize(Roles = "ProductionBillOfMaterialsActive")]
        public JsonResult GetTotal(int productionBillOfMaterialId)
        {
            return Json(SharedFunctions.GetTotalProductionBillOfMaterial(db, productionBillOfMaterialId).ToString("N2"));
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
