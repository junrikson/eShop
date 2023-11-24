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
    public class ProductionWorkOrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SalesRequests
        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public ActionResult Index()
        {
            return View("../Manufacture/ProductionWorkOrders/Index");
        }

        [HttpGet]
        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Manufacture/ProductionWorkOrders/_IndexGrid", db.Set<ProductionWorkOrder>().AsQueryable());
            else
                return PartialView("../Manufacture/ProductionWorkOrders/_IndexGrid", db.Set<ProductionWorkOrder>().AsQueryable()
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
                return db.ProductionWorkOrders.Any(x => x.Code == Code);
            }
            else
            {
                return db.ProductionWorkOrders.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: SalesRequests/Details/
        [Authorize(Roles = "ProductionWorkOrdersView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductionWorkOrder ProductionWorkOrder = db.ProductionWorkOrders.Find(id);
            if (ProductionWorkOrder == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/ProductionWorkOrders/_Details", ProductionWorkOrder);
        }

        [HttpGet]
        [Authorize(Roles = "ProductionWorkOrdersView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Manufacture/ProductionWorkOrders/_ViewGrid", db.ProductionWorkOrdersDetails
                .Where(x => x.ProductionWorkOrderId == Id).ToList());
        }

        // GET: SalesRequests/Create
        [Authorize(Roles = "ProductionWorkOrdersAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            ProductionWorkOrder productionWorkOrder = new ProductionWorkOrder
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MasterCurrencyId = masterCurrency.Id,
                Rate = masterCurrency.Rate,
                HeaderMasterItemUnitId = db.MasterItemUnits.FirstOrDefault().Id,
                HeaderMasterItemId = db.MasterItems.FirstOrDefault().Id,
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
                    db.ProductionWorkOrders.Add(productionWorkOrder);
                    db.SaveChanges();

                    dbTran.Commit();

                    productionWorkOrder.Code = "";
                    productionWorkOrder.Active = true;
                    productionWorkOrder.MasterBusinessUnitId = 0;
                    productionWorkOrder.MasterRegionId = 0;
                    productionWorkOrder.HeaderMasterItemId = 0;
                    productionWorkOrder.HeaderMasterItemUnitId = 0;
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

            return View("../Manufacture/ProductionWorkOrders/Create", productionWorkOrder);
        }

        // POST: PurchaseRequests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionWorkOrdersAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,HeaderQuantity,HeaderMasterItemUnitId,HeaderMasterItemId,Notes,Active,Created,Updated,UserId")] ProductionWorkOrder productionWorkOrder)
        {
            productionWorkOrder.Created = DateTime.Now;
            productionWorkOrder.Updated = DateTime.Now;
            productionWorkOrder.UserId = User.Identity.GetUserId<int>();
            productionWorkOrder.Total = SharedFunctions.GetTotalProductionWorkOrder(db, productionWorkOrder.Id);
            productionWorkOrder.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(productionWorkOrder.Code)) productionWorkOrder.Code = productionWorkOrder.Code.ToUpper();
            if (!string.IsNullOrEmpty(productionWorkOrder.Notes)) productionWorkOrder.Notes = productionWorkOrder.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                productionWorkOrder = GetModelState(productionWorkOrder);
            }

            db.Entry(productionWorkOrder).State = EntityState.Unchanged;
            db.Entry(productionWorkOrder).Property("Code").IsModified = true;
            db.Entry(productionWorkOrder).Property("Date").IsModified = true;
            db.Entry(productionWorkOrder).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(productionWorkOrder).Property("MasterRegionId").IsModified = true;
            db.Entry(productionWorkOrder).Property("HeaderMasterItemUnitId").IsModified = true;
            db.Entry(productionWorkOrder).Property("HeaderMasterItemId").IsModified = true;            
            db.Entry(productionWorkOrder).Property("HeaderQuantity").IsModified = true;
            db.Entry(productionWorkOrder).Property("Total").IsModified = true;
            db.Entry(productionWorkOrder).Property("Notes").IsModified = true;
            db.Entry(productionWorkOrder).Property("Active").IsModified = true;
            db.Entry(productionWorkOrder).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionWorkOrder, MenuId = productionWorkOrder.Id, MenuCode = productionWorkOrder.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", productionWorkOrder.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalProductionWorkOrder(db, productionWorkOrder.Id).ToString("N2");

                return View("../Manufacture/ProductionWorkOrders/Create", productionWorkOrder);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionWorkOrdersAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                ProductionWorkOrder obj = db.ProductionWorkOrders.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.ProductionWorkOrdersDetails.Where(x => x.ProductionWorkOrderId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.ProductionWorkOrdersDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.ProductionWorkOrders.Remove(obj);
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

        // GET: ProductionWorkOrders/Edit/5
        [Authorize(Roles = "ProductionWorkOrdersEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductionWorkOrder productionWorkOrder = db.ProductionWorkOrders.Find(id);
            if (productionWorkOrder == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", productionWorkOrder.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalProductionWorkOrder(db, productionWorkOrder.Id).ToString("N2");

            return View("../Manufacture/ProductionWorkOrders/Edit", productionWorkOrder);
        }

        // POST: PurchaseRequests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionWorkOrdersEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,HeaderQuantity,HeaderMasterItemUnitId,HeaderMasterItemId,Notes,Active,Created,Updated,UserId")] ProductionWorkOrder productionWorkOrder)
        {
            productionWorkOrder.Updated = DateTime.Now;
            productionWorkOrder.UserId = User.Identity.GetUserId<int>();
            productionWorkOrder.Total = SharedFunctions.GetTotalProductionWorkOrder(db, productionWorkOrder.Id);
            productionWorkOrder.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(productionWorkOrder.Code)) productionWorkOrder.Code = productionWorkOrder.Code.ToUpper();
            if (!string.IsNullOrEmpty(productionWorkOrder.Notes)) productionWorkOrder.Notes = productionWorkOrder.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                productionWorkOrder = GetModelState(productionWorkOrder);
            }

            db.Entry(productionWorkOrder).State = EntityState.Unchanged;
            db.Entry(productionWorkOrder).Property("Code").IsModified = true;
            db.Entry(productionWorkOrder).Property("Date").IsModified = true;
            db.Entry(productionWorkOrder).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(productionWorkOrder).Property("MasterRegionId").IsModified = true;
            db.Entry(productionWorkOrder).Property("Total").IsModified = true;
            db.Entry(productionWorkOrder).Property("HeaderMasterItemUnitId").IsModified = true;
            db.Entry(productionWorkOrder).Property("HeaderMasterItemId").IsModified = true;
            db.Entry(productionWorkOrder).Property("HeaderQuantity").IsModified = true;
            db.Entry(productionWorkOrder).Property("Notes").IsModified = true;
            db.Entry(productionWorkOrder).Property("Active").IsModified = true;
            db.Entry(productionWorkOrder).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesRequest, MenuId = productionWorkOrder.Id, MenuCode = productionWorkOrder.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", productionWorkOrder.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalProductionWorkOrder(db, productionWorkOrder.Id).ToString("N2");

                return View("../Manufacture/ProductionWorkOrders/Edit", productionWorkOrder);
            }
        }

        [HttpPost]
        [Authorize(Roles = "ProductionWorkOrdersDelete")]
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
                            ProductionWorkOrder obj = db.ProductionWorkOrders.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                ProductionWorkOrder tmp = obj;

                                var details = db.ProductionWorkOrdersDetails.Where(x => x.ProductionWorkOrderId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.ProductionWorkOrdersDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.ProductionWorkOrders.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionWorkOrder, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "ProductionWorkOrdersPrint")]
        public ActionResult Print(int? id)
        {
            ProductionWorkOrder obj = db.ProductionWorkOrders.Find(id);

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

        [Authorize(Roles = "ProductionWorkOrdersActive")]
        private ProductionWorkOrder GetModelState(ProductionWorkOrder productionWorkOrder)
        {
            List<ProductionWorkOrderDetails> productionWorkOrderDetails = db.ProductionWorkOrdersDetails.Where(x => x.ProductionWorkOrderId == productionWorkOrder.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(productionWorkOrder.Code, productionWorkOrder.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (productionWorkOrderDetails == null || productionWorkOrderDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return productionWorkOrder;
        }

        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public ActionResult DetailsCreate(int productionWorkOrderId)
        {
            ProductionWorkOrder productionWorkOrder = db.ProductionWorkOrders.Find(productionWorkOrderId);

            if (productionWorkOrder == null)
            {
                return HttpNotFound();
            }

            ProductionWorkOrderDetails productionWorkOrderDetails = new ProductionWorkOrderDetails
            {
                ProductionWorkOrderId = productionWorkOrderId
            };

            return PartialView("../Manufacture/ProductionWorkOrders/_DetailsCreate", productionWorkOrderDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,ProductionWorkOrderId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] ProductionWorkOrderDetails productionWorkOrderDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(productionWorkOrderDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                productionWorkOrderDetails.Total = 0;
            else
                productionWorkOrderDetails.Total = productionWorkOrderDetails.Quantity * productionWorkOrderDetails.Price * masterItemUnit.MasterUnit.Ratio;

            productionWorkOrderDetails.Created = DateTime.Now;
            productionWorkOrderDetails.Updated = DateTime.Now;
            productionWorkOrderDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(productionWorkOrderDetails.Notes)) productionWorkOrderDetails.Notes = productionWorkOrderDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.ProductionWorkOrdersDetails.Add(productionWorkOrderDetails);
                        db.SaveChanges();

                        ProductionWorkOrder productionWorkOrder = db.ProductionWorkOrders.Find(productionWorkOrderDetails.ProductionWorkOrderId);
                        productionWorkOrder.Total = SharedFunctions.GetTotalProductionWorkOrder(db, productionWorkOrder.Id, productionWorkOrderDetails.Id) + productionWorkOrderDetails.Total;

                        db.Entry(productionWorkOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionWorkOrderDetails, MenuId = productionWorkOrderDetails.Id, MenuCode = productionWorkOrderDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Manufacture/ProductionWorkOrders/_DetailsCreate", productionWorkOrderDetails);
        }

        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductionWorkOrderDetails obj = db.ProductionWorkOrdersDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/ProductionWorkOrders/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,ProductionWorkOrderId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] ProductionWorkOrderDetails productionWorkOrderDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(productionWorkOrderDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                productionWorkOrderDetails.Total = 0;
            else
                productionWorkOrderDetails.Total = productionWorkOrderDetails.Quantity * productionWorkOrderDetails.Price * masterItemUnit.MasterUnit.Ratio;

            productionWorkOrderDetails.Updated = DateTime.Now;
            productionWorkOrderDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(productionWorkOrderDetails.Notes)) productionWorkOrderDetails.Notes = productionWorkOrderDetails.Notes.ToUpper();

            db.Entry(productionWorkOrderDetails).State = EntityState.Unchanged;
            db.Entry(productionWorkOrderDetails).Property("MasterItemId").IsModified = true;
            db.Entry(productionWorkOrderDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(productionWorkOrderDetails).Property("Quantity").IsModified = true;
            db.Entry(productionWorkOrderDetails).Property("Price").IsModified = true;
            db.Entry(productionWorkOrderDetails).Property("Total").IsModified = true;
            db.Entry(productionWorkOrderDetails).Property("Notes").IsModified = true;
            db.Entry(productionWorkOrderDetails).Property("Updated").IsModified = true;
            db.Entry(productionWorkOrderDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        ProductionWorkOrder productionWorkOrder = db.ProductionWorkOrders.Find(productionWorkOrderDetails.ProductionWorkOrderId);
                        productionWorkOrder.Total = SharedFunctions.GetTotalProductionWorkOrder(db, productionWorkOrder.Id, productionWorkOrderDetails.Id) + productionWorkOrderDetails.Total;

                        db.Entry(productionWorkOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionWorkOrderDetails, MenuId = productionWorkOrderDetails.Id, MenuCode = productionWorkOrderDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Manufacture/ProductionWorkOrders/_DetailsEdit", productionWorkOrderDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionWorkOrdersActive")]
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
                        ProductionWorkOrderDetails obj = db.ProductionWorkOrdersDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    ProductionWorkOrderDetails tmp = obj;

                                    ProductionWorkOrder productionWorkOrder = db.ProductionWorkOrders.Find(tmp.ProductionWorkOrderId);

                                    productionWorkOrder.Total = SharedFunctions.GetTotalProductionWorkOrder(db, productionWorkOrder.Id, tmp.Id);

                                    db.Entry(productionWorkOrder).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.ProductionWorkOrdersDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ProductionWorkOrderDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Manufacture/ProductionWorkOrders/_DetailsGrid", db.ProductionWorkOrdersDetails
                .Where(x => x.ProductionWorkOrderId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PackingWorkOrdersActive")]
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

        //[HttpPost]
        //[ValidateJsonAntiForgeryToken]
        //[Authorize(Roles = "PackingWorkOrdersActive")]
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


        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public ActionResult ChangeCurrency(int? productionWorkOrderId)
        {
            if (productionWorkOrderId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductionWorkOrder productionWorkOrder = db.ProductionWorkOrders.Find(productionWorkOrderId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = productionWorkOrder.Id,
                MasterCurrencyId = productionWorkOrder.MasterCurrencyId,
                Rate = productionWorkOrder.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Manufacture/ProductionWorkOrders/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public ActionResult ChangeCurrency([Bind(Include = "Id,MasterCurrencyId,Rate")] ChangeCurrency changeCurrency)
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Find(changeCurrency.MasterCurrencyId);

            ProductionWorkOrder productionWorkOrder = db.ProductionWorkOrders.Find(changeCurrency.Id);
            productionWorkOrder.MasterCurrencyId = changeCurrency.MasterCurrencyId;
            productionWorkOrder.Rate = changeCurrency.Rate;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var productionWorkOrdersDetails = db.ProductionWorkOrdersDetails.Where(x => x.ProductionWorkOrderId == productionWorkOrder.Id).ToList();

                        foreach (ProductionWorkOrderDetails productionWorkOrderDetails in productionWorkOrdersDetails)
                        {
                            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(productionWorkOrderDetails.MasterItemUnitId);

                            if (masterItemUnit == null)
                                productionWorkOrderDetails.Total = 0;
                            else
                                productionWorkOrderDetails.Total = productionWorkOrderDetails.Quantity * productionWorkOrderDetails.Price * masterItemUnit.MasterUnit.Ratio * productionWorkOrder.Rate  ;

                            db.Entry(productionWorkOrderDetails).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        productionWorkOrder.Total = SharedFunctions.GetTotalProductionWorkOrder(db, productionWorkOrder.Id);
                        db.Entry(productionWorkOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        dbTran.Commit();

                        var returnObject = new
                        {
                            Status = "success",
                            Message = masterCurrency.Code + " : " + productionWorkOrder.Rate.ToString("N2")
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

            return PartialView("../Manufacture/ProductionWorkOrders/_ChangeCurrency", changeCurrency);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            ProductionWorkOrder productionWorkOrder = db.ProductionWorkOrders.Find(id);

            if (masterBusinessUnit != null && productionWorkOrder != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                productionWorkOrder.MasterBusinessUnitId = masterBusinessUnitId;
                productionWorkOrder.MasterRegionId = masterRegionId;
                db.Entry(productionWorkOrder).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "ProductionWorkOrdersActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.ProductionWorkOrderCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            ProductionWorkOrder lastData = db.ProductionWorkOrders
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
        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public JsonResult GetTotal(int productionWorkOrderId)
        {
            return Json(SharedFunctions.GetTotalProductionWorkOrder(db, productionWorkOrderId).ToString("N2"));
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ProductionWorkOrdersActive")]
        public JsonResult PopulateDetails(int productionWorkOrderid, int productionBillofMaterialid)
        {
            ProductionWorkOrder productionWorkOrder = db.ProductionWorkOrders.Find(productionWorkOrderid);
            ProductionBillofMaterial productionBillofMaterial = db.ProductionBillofMaterials.Find(productionBillofMaterialid);

            if (productionWorkOrder != null && productionBillofMaterial != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var remove = db.ProductionWorkOrdersDetails.Where(x => x.ProductionWorkOrderId == productionWorkOrder.Id).ToList();

                        if (remove != null)
                        {
                            foreach (ProductionWorkOrderDetails temp in remove)
                            {
                                //SharedFunctions.RemovePurchaseJournalDetails(db, temp);

                                db.ProductionWorkOrdersDetails.Remove(temp);
                                db.SaveChanges();
                            }
                        }

                        var productionBillofMaterialsDetails = db.ProductionBillofMaterialsDetails.Where(x => x.ProductionBillofMaterialId == productionBillofMaterial.Id).ToList();

                        if (productionBillofMaterialsDetails != null)
                        {
                            foreach (ProductionBillofMaterialDetails productionBillofMaterialDetails in productionBillofMaterialsDetails)
                            {
                                ProductionWorkOrderDetails productionWorkOrderDetails = new ProductionWorkOrderDetails
                                {
                                    ProductionWorkOrderId = productionWorkOrder.Id,
                                    MasterItemId = productionBillofMaterialDetails.MasterItemId,
                                    MasterItemUnitId = productionBillofMaterialDetails.MasterItemUnitId,
                                    Quantity = productionBillofMaterialDetails.Quantity,
                                    Price = productionBillofMaterialDetails.Price,
                                    Total = productionBillofMaterialDetails.Total,
                                    Notes = productionBillofMaterialDetails.Notes,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.ProductionWorkOrdersDetails.Add(productionWorkOrderDetails);
                                db.SaveChanges();

                                //SharedFunctions.CreatePurchaseJournalDetails(db, purchaseDetails);
                            }
                        }

                        productionWorkOrder.ProductionBillofMaterialId = productionBillofMaterial.Id;
                        productionWorkOrder.MasterBusinessUnitId = productionBillofMaterial.MasterBusinessUnitId;
                        productionWorkOrder.MasterRegionId = productionBillofMaterial.MasterRegionId;
                        productionWorkOrder.MasterCurrencyId = productionBillofMaterial.MasterCurrencyId;
                        productionWorkOrder.Rate = productionBillofMaterial.Rate;
                        productionWorkOrder.HeaderMasterItemUnitId = productionBillofMaterial.HeaderMasterItemUnitId;
                        productionWorkOrder.HeaderMasterItemId = productionBillofMaterial.HeaderMasterItemId;                        
                        productionWorkOrder.HeaderQuantity = productionBillofMaterial.HeaderQuantity;
                        productionWorkOrder.Notes = productionBillofMaterial.Notes;
                        productionWorkOrder.Total = productionBillofMaterial.Total;

                        db.Entry(productionWorkOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(productionWorkOrder).Reload();

                        //SharedFunctions.UpdatePurchaseJournal(db, purchase);

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
                productionWorkOrder.MasterRegionId,
                productionWorkOrder.MasterBusinessUnitId,
                productionWorkOrder.Notes,
                productionWorkOrder.HeaderQuantity,
                productionWorkOrder.HeaderMasterItemUnitId,
                productionWorkOrder.HeaderMasterItemId,
                Total = productionWorkOrder.Total.ToString("N2"),
                productionWorkOrder.Date,
                Currency = productionWorkOrder.MasterCurrency.Code + " : " + productionWorkOrder.Rate.ToString("N2")
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
