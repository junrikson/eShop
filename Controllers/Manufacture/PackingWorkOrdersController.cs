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
    public class PackingWorkOrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SalesRequests
        [Authorize(Roles = "PackingWorkOrdersActive")]
        public ActionResult Index()
        {
            return View("../Manufacture/PackingWorkOrders/Index");
        }

        [HttpGet]
        [Authorize(Roles = "PackingWorkOrdersActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Manufacture/PackingWorkOrders/_IndexGrid", db.Set<PackingWorkOrder>().AsQueryable());
            else
                return PartialView("../Manufacture/PackingWorkOrders/_IndexGrid", db.Set<PackingWorkOrder>().AsQueryable()
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
                return db.PackingWorkOrders.Any(x => x.Code == Code);
            }
            else
            {
                return db.PackingWorkOrders.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: SalesRequests/Details/
        [Authorize(Roles = "PackingWorkOrdersView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PackingWorkOrder PackingWorkOrder = db.PackingWorkOrders.Find(id);
            if (PackingWorkOrder == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/PackingWorkOrders/_Details", PackingWorkOrder);
        }

        [HttpGet]
        [Authorize(Roles = "PackingWorkOrdersView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Manufacture/PackingWorkOrders/_ViewGrid", db.PackingWorkOrdersDetails
                .Where(x => x.PackingWorkOrderId == Id).ToList());
        }

        // GET: SalesRequests/Create
        [Authorize(Roles = "PackingWorkOrdersAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            PackingWorkOrder packingWorkOrder = new PackingWorkOrder
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
                    db.PackingWorkOrders.Add(packingWorkOrder);
                    db.SaveChanges();

                    dbTran.Commit();

                    packingWorkOrder.Code = "";
                    packingWorkOrder.Active = true;
                    packingWorkOrder.MasterBusinessUnitId = 0;
                    packingWorkOrder.MasterRegionId = 0;
                    packingWorkOrder.HeaderMasterItemUnitId = 0;
                    packingWorkOrder.HeaderMasterItemId = 0;
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

            return View("../Manufacture/PackingWorkOrders/Create", packingWorkOrder);
        }

        // POST: PurchaseRequests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PackingWorkOrdersAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,HeaderQuantity,HeaderMasterItemUnitId,HeaderMasterItemId,Notes,Active,Created,Updated,UserId")] PackingWorkOrder packingWorkOrder)
        {
            packingWorkOrder.Created = DateTime.Now;
            packingWorkOrder.Updated = DateTime.Now;
            packingWorkOrder.UserId = User.Identity.GetUserId<int>();
            packingWorkOrder.Total = SharedFunctions.GetTotalPackingWorkOrder(db, packingWorkOrder.Id);
            packingWorkOrder.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(packingWorkOrder.Code)) packingWorkOrder.Code = packingWorkOrder.Code.ToUpper();
            if (!string.IsNullOrEmpty(packingWorkOrder.Notes)) packingWorkOrder.Notes = packingWorkOrder.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                packingWorkOrder = GetModelState(packingWorkOrder);
            }

            db.Entry(packingWorkOrder).State = EntityState.Unchanged;
            db.Entry(packingWorkOrder).Property("Code").IsModified = true;
            db.Entry(packingWorkOrder).Property("Date").IsModified = true;
            db.Entry(packingWorkOrder).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(packingWorkOrder).Property("MasterRegionId").IsModified = true;
            db.Entry(packingWorkOrder).Property("HeaderMasterItemUnitId").IsModified = true;
            db.Entry(packingWorkOrder).Property("HeaderMasterItemId").IsModified = true;
            db.Entry(packingWorkOrder).Property("HeaderQuantity").IsModified = true;
            db.Entry(packingWorkOrder).Property("Total").IsModified = true;
            db.Entry(packingWorkOrder).Property("Notes").IsModified = true;
            db.Entry(packingWorkOrder).Property("Active").IsModified = true;
            db.Entry(packingWorkOrder).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PackingWorkOrder, MenuId = packingWorkOrder.Id, MenuCode = packingWorkOrder.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", packingWorkOrder.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalPackingWorkOrder(db, packingWorkOrder.Id).ToString("N2");

                return View("../Manufacture/PackingWorkOrders/Create", packingWorkOrder);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PackingWorkOrdersAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                PackingWorkOrder obj = db.PackingWorkOrders.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.PackingWorkOrdersDetails.Where(x => x.PackingWorkOrderId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PackingWorkOrdersDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.PackingWorkOrders.Remove(obj);
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

        // GET: PackingWorkOrders/Edit/5
        [Authorize(Roles = "PackingWorkOrdersEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PackingWorkOrder packingWorkOrder = db.PackingWorkOrders.Find(id);
            if (packingWorkOrder == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", packingWorkOrder.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalPackingWorkOrder(db, packingWorkOrder.Id).ToString("N2");

            return View("../Manufacture/PackingWorkOrders/Edit", packingWorkOrder);
        }

        // POST: PurchaseRequests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PackingWorkOrdersEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,HeaderQuantity,HeaderMasterItemUnitId,HeaderMasterItemId,Notes,Active,Created,Updated,UserId")] PackingWorkOrder packingWorkOrder)
        {
            packingWorkOrder.Updated = DateTime.Now;
            packingWorkOrder.UserId = User.Identity.GetUserId<int>();
            packingWorkOrder.Total = SharedFunctions.GetTotalPackingWorkOrder(db, packingWorkOrder.Id);
            packingWorkOrder.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(packingWorkOrder.Code)) packingWorkOrder.Code = packingWorkOrder.Code.ToUpper();
            if (!string.IsNullOrEmpty(packingWorkOrder.Notes)) packingWorkOrder.Notes = packingWorkOrder.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                packingWorkOrder = GetModelState(packingWorkOrder);
            }

            db.Entry(packingWorkOrder).State = EntityState.Unchanged;
            db.Entry(packingWorkOrder).Property("Code").IsModified = true;
            db.Entry(packingWorkOrder).Property("Date").IsModified = true;
            db.Entry(packingWorkOrder).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(packingWorkOrder).Property("MasterRegionId").IsModified = true;
            db.Entry(packingWorkOrder).Property("Total").IsModified = true;
            db.Entry(packingWorkOrder).Property("HeaderMasterItemUnitId").IsModified = true;
            db.Entry(packingWorkOrder).Property("HeaderMasterItemId").IsModified = true;
            db.Entry(packingWorkOrder).Property("HeaderQuantity").IsModified = true;
            db.Entry(packingWorkOrder).Property("Notes").IsModified = true;
            db.Entry(packingWorkOrder).Property("Active").IsModified = true;
            db.Entry(packingWorkOrder).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PackingWorkOrder, MenuId = packingWorkOrder.Id, MenuCode = packingWorkOrder.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", packingWorkOrder.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalPackingWorkOrder(db, packingWorkOrder.Id).ToString("N2");

                return View("../Manufacture/PackingWorkOrders/Edit", packingWorkOrder);
            }
        }

        [HttpPost]
        [Authorize(Roles = "PackingWorkOrdersDelete")]
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
                            PackingWorkOrder obj = db.PackingWorkOrders.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                PackingWorkOrder tmp = obj;

                                var details = db.PackingWorkOrdersDetails.Where(x => x.PackingWorkOrderId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PackingWorkOrdersDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.PackingWorkOrders.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PackingWorkOrder, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "PackingWorkOrdersPrint")]
        public ActionResult Print(int? id)
        {
            PackingWorkOrder obj = db.PackingWorkOrders.Find(id);

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

        [Authorize(Roles = "PackingWorkOrdersActive")]
        private PackingWorkOrder GetModelState(PackingWorkOrder packingWorkOrder)
        {
            List<PackingWorkOrderDetails> packingWorkOrderDetails = db.PackingWorkOrdersDetails.Where(x => x.PackingWorkOrderId == packingWorkOrder.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(packingWorkOrder.Code, packingWorkOrder.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (packingWorkOrderDetails == null || packingWorkOrderDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return packingWorkOrder;
        }

        [Authorize(Roles = "PackingWorkOrdersActive")]
        public ActionResult DetailsCreate(int packingWorkOrderId)
        {
            PackingWorkOrder packingWorkOrder = db.PackingWorkOrders.Find(packingWorkOrderId);

            if (packingWorkOrder == null)
            {
                return HttpNotFound();
            }

            PackingWorkOrderDetails packingWorkOrderDetails = new PackingWorkOrderDetails
            {
                PackingWorkOrderId = packingWorkOrderId
            };

            return PartialView("../Manufacture/PackingWorkOrders/_DetailsCreate", packingWorkOrderDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PackingWorkOrdersActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,PackingWorkOrderId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PackingWorkOrderDetails packingWorkOrderDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(packingWorkOrderDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                packingWorkOrderDetails.Total = 0;
            else
                packingWorkOrderDetails.Total = packingWorkOrderDetails.Quantity * packingWorkOrderDetails.Price * masterItemUnit.MasterUnit.Ratio;

            packingWorkOrderDetails.Created = DateTime.Now;
            packingWorkOrderDetails.Updated = DateTime.Now;
            packingWorkOrderDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(packingWorkOrderDetails.Notes)) packingWorkOrderDetails.Notes = packingWorkOrderDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.PackingWorkOrdersDetails.Add(packingWorkOrderDetails);
                        db.SaveChanges();

                        PackingWorkOrder packingWorkOrder = db.PackingWorkOrders.Find(packingWorkOrderDetails.PackingWorkOrderId);
                        packingWorkOrder.Total = SharedFunctions.GetTotalPackingWorkOrder(db, packingWorkOrder.Id, packingWorkOrderDetails.Id) + packingWorkOrderDetails.Total;

                        db.Entry(packingWorkOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PackingWorkOrderDetails, MenuId = packingWorkOrderDetails.Id, MenuCode = packingWorkOrderDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Manufacture/PackingWorkOrders/_DetailsCreate", packingWorkOrderDetails);
        }

        [Authorize(Roles = "PackingWorkOrdersActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PackingWorkOrderDetails obj = db.PackingWorkOrdersDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/PackingWorkOrders/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PackingWorkOrdersActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,PackingWorkOrderId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PackingWorkOrderDetails packingWorkOrderDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(packingWorkOrderDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                packingWorkOrderDetails.Total = 0;
            else
                packingWorkOrderDetails.Total = packingWorkOrderDetails.Quantity * packingWorkOrderDetails.Price * masterItemUnit.MasterUnit.Ratio;

            packingWorkOrderDetails.Updated = DateTime.Now;
            packingWorkOrderDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(packingWorkOrderDetails.Notes)) packingWorkOrderDetails.Notes = packingWorkOrderDetails.Notes.ToUpper();

            db.Entry(packingWorkOrderDetails).State = EntityState.Unchanged;
            db.Entry(packingWorkOrderDetails).Property("MasterItemId").IsModified = true;
            db.Entry(packingWorkOrderDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(packingWorkOrderDetails).Property("Quantity").IsModified = true;
            db.Entry(packingWorkOrderDetails).Property("Price").IsModified = true;
            db.Entry(packingWorkOrderDetails).Property("Total").IsModified = true;
            db.Entry(packingWorkOrderDetails).Property("Notes").IsModified = true;
            db.Entry(packingWorkOrderDetails).Property("Updated").IsModified = true;
            db.Entry(packingWorkOrderDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        PackingWorkOrder packingWorkOrder = db.PackingWorkOrders.Find(packingWorkOrderDetails.PackingWorkOrderId);
                        packingWorkOrder.Total = SharedFunctions.GetTotalPackingWorkOrder(db, packingWorkOrder.Id, packingWorkOrderDetails.Id) + packingWorkOrderDetails.Total;

                        db.Entry(packingWorkOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PackingWorkOrderDetails, MenuId = packingWorkOrderDetails.Id, MenuCode = packingWorkOrderDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Manufacture/PackingWorkOrders/_DetailsEdit", packingWorkOrderDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PackingWorkOrdersActive")]
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
                        PackingWorkOrderDetails obj = db.PackingWorkOrdersDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    PackingWorkOrderDetails tmp = obj;

                                    PackingWorkOrder packingWorkOrder = db.PackingWorkOrders.Find(tmp.PackingWorkOrderId);

                                    packingWorkOrder.Total = SharedFunctions.GetTotalPackingWorkOrder(db, packingWorkOrder.Id, tmp.Id);

                                    db.Entry(packingWorkOrder).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.PackingWorkOrdersDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PackingWorkOrderDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "PackingWorkOrdersActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Manufacture/PackingWorkOrders/_DetailsGrid", db.PackingWorkOrdersDetails
                .Where(x => x.PackingWorkOrderId == Id).ToList());
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

        [Authorize(Roles = "PackingWorkOrdersActive")]
        public ActionResult ChangeCurrency(int? packingWorkOrderId)
        {
            if (packingWorkOrderId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PackingWorkOrder packingWorkOrder = db.PackingWorkOrders.Find(packingWorkOrderId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = packingWorkOrder.Id,
                MasterCurrencyId = packingWorkOrder.MasterCurrencyId,
                Rate = packingWorkOrder.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Manufacture/PackingWorkOrders/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PackingWorkOrdersActive")]
        public ActionResult ChangeCurrency([Bind(Include = "Id,MasterCurrencyId,Rate")] ChangeCurrency changeCurrency)
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Find(changeCurrency.MasterCurrencyId);

            PackingWorkOrder packingWorkOrder = db.PackingWorkOrders.Find(changeCurrency.Id);
            packingWorkOrder.MasterCurrencyId = changeCurrency.MasterCurrencyId;
            packingWorkOrder.Rate = changeCurrency.Rate;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var packingWorkOrdersDetails = db.PackingWorkOrdersDetails.Where(x => x.PackingWorkOrderId == packingWorkOrder.Id).ToList();

                        foreach (PackingWorkOrderDetails packingWorkOrderDetails in packingWorkOrdersDetails)
                        {
                            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(packingWorkOrderDetails.MasterItemUnitId);

                            if (masterItemUnit == null)
                                packingWorkOrderDetails.Total = 0;
                            else
                                packingWorkOrderDetails.Total = packingWorkOrderDetails.Quantity * packingWorkOrderDetails.Price * masterItemUnit.MasterUnit.Ratio * packingWorkOrder.Rate;

                            db.Entry(packingWorkOrderDetails).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        packingWorkOrder.Total = SharedFunctions.GetTotalPackingWorkOrder(db, packingWorkOrder.Id);
                        db.Entry(packingWorkOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        dbTran.Commit();

                        var returnObject = new
                        {
                            Status = "success",
                            Message = masterCurrency.Code + " : " + packingWorkOrder.Rate.ToString("N2")
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

            return PartialView("../Manufacture/PackingWorkOrders/_ChangeCurrency", changeCurrency);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PackingWorkOrdersActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PackingWorkOrdersActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            PackingWorkOrder packingWorkOrder = db.PackingWorkOrders.Find(id);

            if (masterBusinessUnit != null && packingWorkOrder != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                packingWorkOrder.MasterBusinessUnitId = masterBusinessUnitId;
                packingWorkOrder.MasterRegionId = masterRegionId;
                db.Entry(packingWorkOrder).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "PackingWorkOrdersActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.PackingWorkOrderCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            PackingWorkOrder lastData = db.PackingWorkOrders
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
        [Authorize(Roles = "PackingWorkOrdersActive")]
        public JsonResult GetTotal(int packingWorkOrderId)
        {
            return Json(SharedFunctions.GetTotalPackingWorkOrder(db, packingWorkOrderId).ToString("N2"));
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PackingWorkOrdersActive")]
        public JsonResult PopulateDetails(int packingWorkOrderid, int packingBillofMaterialid)
        {
            PackingWorkOrder packingWorkOrder = db.PackingWorkOrders.Find(packingWorkOrderid);
            PackingBillofMaterial packingBillofMaterial = db.PackingBillofMaterials.Find(packingBillofMaterialid);

            if (packingWorkOrder != null && packingBillofMaterial != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var remove = db.PackingWorkOrdersDetails.Where(x => x.PackingWorkOrderId == packingWorkOrder.Id).ToList();

                        if (remove != null)
                        {
                            foreach (PackingWorkOrderDetails temp in remove)
                            {
                                //SharedFunctions.RemovePurchaseJournalDetails(db, temp);

                                db.PackingWorkOrdersDetails.Remove(temp);
                                db.SaveChanges();
                            }
                        }

                        var packingBillofMaterialsDetails = db.PackingBillofMaterialsDetails.Where(x => x.PackingBillofMaterialId == packingBillofMaterial.Id).ToList();

                        if (packingBillofMaterialsDetails != null)
                        {
                            foreach (PackingBillofMaterialDetails packingBillofMaterialDetails in packingBillofMaterialsDetails)
                            {
                                PackingWorkOrderDetails packingWorkOrderDetails = new PackingWorkOrderDetails
                                {
                                    PackingWorkOrderId = packingWorkOrder.Id,
                                    MasterItemId = packingBillofMaterialDetails.MasterItemId,
                                    MasterItemUnitId = packingBillofMaterialDetails.MasterItemUnitId,
                                    Quantity = packingBillofMaterialDetails.Quantity,
                                    Price = packingBillofMaterialDetails.Price,
                                    Total = packingBillofMaterialDetails.Total,
                                    Notes = packingBillofMaterialDetails.Notes,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.PackingWorkOrdersDetails.Add(packingWorkOrderDetails);
                                db.SaveChanges();

                                //SharedFunctions.CreatePurchaseJournalDetails(db, purchaseDetails);
                            }
                        }

                        packingWorkOrder.PackingBillofMaterialId = packingBillofMaterial.Id;
                        packingWorkOrder.MasterBusinessUnitId = packingBillofMaterial.MasterBusinessUnitId;
                        packingWorkOrder.MasterRegionId = packingBillofMaterial.MasterRegionId;
                        packingWorkOrder.MasterCurrencyId = packingBillofMaterial.MasterCurrencyId;
                        packingWorkOrder.Rate = packingBillofMaterial.Rate;
                        packingWorkOrder.HeaderMasterItemId = packingBillofMaterial.HeaderMasterItemId;
                        packingWorkOrder.HeaderMasterItemUnitId = packingBillofMaterial.HeaderMasterItemUnitId;
                        packingWorkOrder.HeaderQuantity = packingBillofMaterial.HeaderQuantity;
                        packingWorkOrder.Notes = packingBillofMaterial.Notes;
                        packingWorkOrder.Total = packingBillofMaterial.Total;

                        db.Entry(packingWorkOrder).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(packingWorkOrder).Reload();

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
                packingWorkOrder.MasterRegionId,
                packingWorkOrder.MasterBusinessUnitId,
                packingWorkOrder.HeaderMasterItemId,
                packingWorkOrder.HeaderQuantity,
                packingWorkOrder.HeaderMasterItemUnitId,
                packingWorkOrder.Notes,
                Total = packingWorkOrder.Total.ToString("N2"),
                packingWorkOrder.Date,
                Currency = packingWorkOrder.MasterCurrency.Code + " : " + packingWorkOrder.Rate.ToString("N2")
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
