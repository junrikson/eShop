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
    public class ProductionBillofMaterialsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SalesRequests
        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public ActionResult Index()
        {
            return View("../Manufacture/ProductionBillofMaterials/Index");
        }

        [HttpGet]
        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Manufacture/ProductionBillofMaterials/_IndexGrid", db.Set<ProductionBillofMaterial>().AsQueryable());
            else
                return PartialView("../Manufacture/ProductionBillofMaterials/_IndexGrid", db.Set<ProductionBillofMaterial>().AsQueryable()
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
                return db.ProductionBillofMaterials.Any(x => x.Code == Code);
            }
            else
            {
                return db.ProductionBillofMaterials.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: SalesRequests/Details/
        [Authorize(Roles = "ProductionBillofMaterialsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductionBillofMaterial ProductionBillofMaterial = db.ProductionBillofMaterials.Find(id);
            if (ProductionBillofMaterial == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/ProductionBillofMaterials/_Details", ProductionBillofMaterial);
        }

        [HttpGet]
        [Authorize(Roles = "ProductionBillofMaterialsView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Manufacture/ProductionBillofMaterials/_ViewGrid", db.ProductionBillofMaterialsDetails
                .Where(x => x.ProductionBillofMaterialId == Id).ToList());
        }

        // GET: SalesRequests/Create
        [Authorize(Roles = "ProductionBillofMaterialsAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            ProductionBillofMaterial productionBillofMaterial = new ProductionBillofMaterial
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MasterCurrencyId = masterCurrency.Id,
                Rate = masterCurrency.Rate,
                HeaderMasterItemId = db.MasterItems.FirstOrDefault().Id,
                HeaderMasterItemUnitId = db.MasterItemUnits.FirstOrDefault().Id,
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
                    db.ProductionBillofMaterials.Add(productionBillofMaterial);
                    db.SaveChanges();

                    dbTran.Commit();

                    productionBillofMaterial.Code = "";
                    productionBillofMaterial.Active = true;
                    productionBillofMaterial.MasterBusinessUnitId = 0;
                    productionBillofMaterial.MasterRegionId = 0;
                    productionBillofMaterial.HeaderMasterItemUnitId = 0;
                    productionBillofMaterial.HeaderMasterItemId = 0;

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

            return View("../Manufacture/ProductionBillofMaterials/Create", productionBillofMaterial);
        }

        // POST: PurchaseRequests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionBillofMaterialsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,HeaderQuantity,HeaderMasterItemUnitId,HeaderMasterItemId,Notes,Active,Created,Updated,UserId")] ProductionBillofMaterial productionBillofMaterial)
        {
            productionBillofMaterial.Created = DateTime.Now;
            productionBillofMaterial.Updated = DateTime.Now;
            productionBillofMaterial.UserId = User.Identity.GetUserId<int>();
            productionBillofMaterial.Total = SharedFunctions.GetTotalProductionBillofMaterial(db, productionBillofMaterial.Id);
            productionBillofMaterial.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(productionBillofMaterial.Code)) productionBillofMaterial.Code = productionBillofMaterial.Code.ToUpper();
            if (!string.IsNullOrEmpty(productionBillofMaterial.Notes)) productionBillofMaterial.Notes = productionBillofMaterial.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                productionBillofMaterial = GetModelState(productionBillofMaterial);
            }

            db.Entry(productionBillofMaterial).State = EntityState.Unchanged;
            db.Entry(productionBillofMaterial).Property("Code").IsModified = true;
            db.Entry(productionBillofMaterial).Property("Date").IsModified = true;
            db.Entry(productionBillofMaterial).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(productionBillofMaterial).Property("MasterRegionId").IsModified = true;
            db.Entry(productionBillofMaterial).Property("HeaderMasterItemUnitId").IsModified = true;
            db.Entry(productionBillofMaterial).Property("HeaderMasterItemId").IsModified = true;
            db.Entry(productionBillofMaterial).Property("HeaderQuantity").IsModified = true;
            db.Entry(productionBillofMaterial).Property("Total").IsModified = true;
            db.Entry(productionBillofMaterial).Property("Notes").IsModified = true;
            db.Entry(productionBillofMaterial).Property("Active").IsModified = true;
            db.Entry(productionBillofMaterial).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillofMaterial, MenuId = productionBillofMaterial.Id, MenuCode = productionBillofMaterial.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", productionBillofMaterial.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalProductionBillofMaterial(db, productionBillofMaterial.Id).ToString("N2");

                return View("../Manufacture/ProductionBillofMaterials/Create", productionBillofMaterial);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionBillofMaterialsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                ProductionBillofMaterial obj = db.ProductionBillofMaterials.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.ProductionBillofMaterialsDetails.Where(x => x.ProductionBillofMaterialId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.ProductionBillofMaterialsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.ProductionBillofMaterials.Remove(obj);
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

        // GET: ProductionBillofMaterials/Edit/5
        [Authorize(Roles = "ProductionBillofMaterialsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductionBillofMaterial productionBillofMaterial = db.ProductionBillofMaterials.Find(id);
            if (productionBillofMaterial == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", productionBillofMaterial.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalProductionBillofMaterial(db, productionBillofMaterial.Id).ToString("N2");

            return View("../Manufacture/ProductionBillofMaterials/Edit", productionBillofMaterial);
        }

        // POST: PurchaseRequests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionBillofMaterialsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,HeaderQuantity,HeaderMasterItemUnitId,HeaderMasterItemId,Notes,Active,Created,Updated,UserId")] ProductionBillofMaterial productionBillofMaterial)
        {
            productionBillofMaterial.Updated = DateTime.Now;
            productionBillofMaterial.UserId = User.Identity.GetUserId<int>();
            productionBillofMaterial.Total = SharedFunctions.GetTotalProductionBillofMaterial(db, productionBillofMaterial.Id);
            productionBillofMaterial.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(productionBillofMaterial.Code)) productionBillofMaterial.Code = productionBillofMaterial.Code.ToUpper();
            if (!string.IsNullOrEmpty(productionBillofMaterial.Notes)) productionBillofMaterial.Notes = productionBillofMaterial.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                productionBillofMaterial = GetModelState(productionBillofMaterial);
            }

            db.Entry(productionBillofMaterial).State = EntityState.Unchanged;
            db.Entry(productionBillofMaterial).Property("Code").IsModified = true;
            db.Entry(productionBillofMaterial).Property("Date").IsModified = true;
            db.Entry(productionBillofMaterial).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(productionBillofMaterial).Property("MasterRegionId").IsModified = true;
            db.Entry(productionBillofMaterial).Property("Total").IsModified = true;
            db.Entry(productionBillofMaterial).Property("HeaderMasterItemUnitId").IsModified = true;
            db.Entry(productionBillofMaterial).Property("HeaderMasterItemId").IsModified = true;
            db.Entry(productionBillofMaterial).Property("HeaderQuantity").IsModified = true;
            db.Entry(productionBillofMaterial).Property("Notes").IsModified = true;
            db.Entry(productionBillofMaterial).Property("Active").IsModified = true;
            db.Entry(productionBillofMaterial).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillofMaterial, MenuId = productionBillofMaterial.Id, MenuCode = productionBillofMaterial.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", productionBillofMaterial.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalProductionBillofMaterial(db, productionBillofMaterial.Id).ToString("N2");

                return View("../Manufacture/ProductionBillofMaterials/Edit", productionBillofMaterial);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ProductionBillofMaterialsDelete")]
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
                            ProductionBillofMaterial obj = db.ProductionBillofMaterials.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                ProductionBillofMaterial tmp = obj;

                                var details = db.ProductionBillofMaterialsDetails.Where(x => x.ProductionBillofMaterialId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.ProductionBillofMaterialsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.ProductionBillofMaterials.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillofMaterial, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "ProductionBillofMaterialsPrint")]
        public ActionResult Print(int? id)
        {
            ProductionBillofMaterial obj = db.ProductionBillofMaterials.Find(id);

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

        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        private ProductionBillofMaterial GetModelState(ProductionBillofMaterial productionBillofMaterial)
        {
            List<ProductionBillofMaterialDetails> productionBillofMaterialDetails = db.ProductionBillofMaterialsDetails.Where(x => x.ProductionBillofMaterialId == productionBillofMaterial.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(productionBillofMaterial.Code, productionBillofMaterial.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (productionBillofMaterialDetails == null || productionBillofMaterialDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return productionBillofMaterial;
        }

        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public ActionResult DetailsCreate(int productionBillofMaterialId)
        {
            ProductionBillofMaterial productionBillofMaterial = db.ProductionBillofMaterials.Find(productionBillofMaterialId);

            if (productionBillofMaterial == null)
            {
                return HttpNotFound();
            }

            ProductionBillofMaterialDetails productionBillofMaterialDetails = new ProductionBillofMaterialDetails
            {
                ProductionBillofMaterialId = productionBillofMaterialId
            };

            return PartialView("../Manufacture/ProductionBillofMaterials/_DetailsCreate", productionBillofMaterialDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,ProductionBillofMaterialId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] ProductionBillofMaterialDetails productionBillofMaterialDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(productionBillofMaterialDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                productionBillofMaterialDetails.Total = 0;
            else
                productionBillofMaterialDetails.Total = productionBillofMaterialDetails.Quantity * productionBillofMaterialDetails.Price * masterItemUnit.MasterUnit.Ratio;

            productionBillofMaterialDetails.Created = DateTime.Now;
            productionBillofMaterialDetails.Updated = DateTime.Now;
            productionBillofMaterialDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(productionBillofMaterialDetails.Notes)) productionBillofMaterialDetails.Notes = productionBillofMaterialDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.ProductionBillofMaterialsDetails.Add(productionBillofMaterialDetails);
                        db.SaveChanges();

                        ProductionBillofMaterial productionBillofMaterial = db.ProductionBillofMaterials.Find(productionBillofMaterialDetails.ProductionBillofMaterialId);
                        productionBillofMaterial.Total = SharedFunctions.GetTotalProductionBillofMaterial(db, productionBillofMaterial.Id, productionBillofMaterialDetails.Id) + productionBillofMaterialDetails.Total;

                        db.Entry(productionBillofMaterial).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillofMaterialDetails, MenuId = productionBillofMaterialDetails.Id, MenuCode = productionBillofMaterialDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Manufacture/ProductionBillofMaterials/_DetailsCreate", productionBillofMaterialDetails);
        }

        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductionBillofMaterialDetails obj = db.ProductionBillofMaterialsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/ProductionBillofMaterials/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,ProductionBillofMaterialId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] ProductionBillofMaterialDetails productionBillofMaterialDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(productionBillofMaterialDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                productionBillofMaterialDetails.Total = 0;
            else
                productionBillofMaterialDetails.Total = productionBillofMaterialDetails.Quantity * productionBillofMaterialDetails.Price * masterItemUnit.MasterUnit.Ratio;

            productionBillofMaterialDetails.Updated = DateTime.Now;
            productionBillofMaterialDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(productionBillofMaterialDetails.Notes)) productionBillofMaterialDetails.Notes = productionBillofMaterialDetails.Notes.ToUpper();

            db.Entry(productionBillofMaterialDetails).State = EntityState.Unchanged;
            db.Entry(productionBillofMaterialDetails).Property("MasterItemId").IsModified = true;
            db.Entry(productionBillofMaterialDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(productionBillofMaterialDetails).Property("Quantity").IsModified = true;
            db.Entry(productionBillofMaterialDetails).Property("Price").IsModified = true;
            db.Entry(productionBillofMaterialDetails).Property("Total").IsModified = true;
            db.Entry(productionBillofMaterialDetails).Property("Notes").IsModified = true;
            db.Entry(productionBillofMaterialDetails).Property("Updated").IsModified = true;
            db.Entry(productionBillofMaterialDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        ProductionBillofMaterial productionBillofMaterial = db.ProductionBillofMaterials.Find(productionBillofMaterialDetails.ProductionBillofMaterialId);
                        productionBillofMaterial.Total = SharedFunctions.GetTotalProductionBillofMaterial(db, productionBillofMaterial.Id, productionBillofMaterialDetails.Id) + productionBillofMaterialDetails.Total;

                        db.Entry(productionBillofMaterial).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillofMaterialDetails, MenuId = productionBillofMaterialDetails.Id, MenuCode = productionBillofMaterialDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Manufacture/ProductionBillofMaterials/_DetailsEdit", productionBillofMaterialDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionBillofMaterialsActive")]
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
                        ProductionBillofMaterialDetails obj = db.ProductionBillofMaterialsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    ProductionBillofMaterialDetails tmp = obj;

                                    ProductionBillofMaterial productionBillofMaterial = db.ProductionBillofMaterials.Find(tmp.ProductionBillofMaterialId);

                                    productionBillofMaterial.Total = SharedFunctions.GetTotalProductionBillofMaterial(db, productionBillofMaterial.Id, tmp.Id);

                                    db.Entry(productionBillofMaterial).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.ProductionBillofMaterialsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionBillofMaterialDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Manufacture/ProductionBillofMaterials/_DetailsGrid", db.ProductionBillofMaterialsDetails
                .Where(x => x.ProductionBillofMaterialId == Id).ToList());
        }

        //[HttpPost]
        //[ValidateJsonAntiForgeryToken]
        //[Authorize(Roles = "ProductionBillofMaterialsActive")]
        //public JsonResult GetHeaderMasterUnit(int id)
        //{
        //    int headermasterItemUnitId = 0;
        //    MasterItem masterItem = db.MasterItems.Find(id);

        //    if (masterItem != null)
        //    {
        //        MasterItemUnit masterItemUnit = db.MasterItemUnits.Where(x => x.MasterItemId == masterItem.Id && x.Default).FirstOrDefault();

        //        if (masterItemUnit != null)
        //            headermasterItemUnitId = masterItemUnit.Id;
        //        else
        //        {
        //            masterItemUnit = db.MasterItemUnits.Where(x => x.MasterItemId == masterItem.Id).FirstOrDefault();

        //            if (masterItemUnit != null)
        //                headermasterItemUnitId = masterItemUnit.Id;
        //        }
        //    }

        //    return Json(headermasterItemUnitId);
        //}

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionBillofMaterialsActive")]
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

        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public ActionResult ChangeCurrency(int? productionBillofMaterialId)
        {
            if (productionBillofMaterialId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductionBillofMaterial productionBillofMaterial = db.ProductionBillofMaterials.Find(productionBillofMaterialId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = productionBillofMaterial.Id,
                MasterCurrencyId = productionBillofMaterial.MasterCurrencyId,
                Rate = productionBillofMaterial.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Manufacture/ProductionBillofMaterials/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public ActionResult ChangeCurrency([Bind(Include = "Id,MasterCurrencyId,Rate")] ChangeCurrency changeCurrency)
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Find(changeCurrency.MasterCurrencyId);

            ProductionBillofMaterial productionBillofMaterial = db.ProductionBillofMaterials.Find(changeCurrency.Id);
            productionBillofMaterial.MasterCurrencyId = changeCurrency.MasterCurrencyId;
            productionBillofMaterial.Rate = changeCurrency.Rate;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var productionBillofMaterialsDetails = db.ProductionBillofMaterialsDetails.Where(x => x.ProductionBillofMaterialId == productionBillofMaterial.Id).ToList();

                        foreach (ProductionBillofMaterialDetails productionBillofMaterialDetails in productionBillofMaterialsDetails)
                        {
                            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(productionBillofMaterialDetails.MasterItemUnitId);

                            if (masterItemUnit == null)
                                productionBillofMaterialDetails.Total = 0;
                            else
                                productionBillofMaterialDetails.Total = productionBillofMaterialDetails.Quantity * productionBillofMaterialDetails.Price * masterItemUnit.MasterUnit.Ratio * productionBillofMaterial.Rate;

                            db.Entry(productionBillofMaterialDetails).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        productionBillofMaterial.Total = SharedFunctions.GetTotalProductionWorkOrder(db, productionBillofMaterial.Id);
                        db.Entry(productionBillofMaterial).State = EntityState.Modified;
                        db.SaveChanges();

                        dbTran.Commit();

                        var returnObject = new
                        {
                            Status = "success",
                            Message = masterCurrency.Code + " : " + productionBillofMaterial.Rate.ToString("N2")
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

            return PartialView("../Manufacture/ProductionBillofMaterials/_ChangeCurrency", changeCurrency);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            ProductionBillofMaterial productionBillofMaterial = db.ProductionBillofMaterials.Find(id);

            if (masterBusinessUnit != null && productionBillofMaterial != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                productionBillofMaterial.MasterBusinessUnitId = masterBusinessUnitId;
                productionBillofMaterial.MasterRegionId = masterRegionId;
                db.Entry(productionBillofMaterial).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.ProductionBillofMaterialCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            ProductionBillofMaterial lastData = db.ProductionBillofMaterials
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
        [Authorize(Roles = "ProductionBillofMaterialsActive")]
        public JsonResult GetTotal(int productionBillofMaterialId)
        {
            return Json(SharedFunctions.GetTotalProductionBillofMaterial(db, productionBillofMaterialId).ToString("N2"));
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
