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
    public class PackingBillofMaterialsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SalesRequests
        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public ActionResult Index()
        {
            return View("../Manufacture/PackingBillofMaterials/Index");
        }

        [HttpGet]
        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Manufacture/PackingBillofMaterials/_IndexGrid", db.Set<PackingBillofMaterial>().AsQueryable());
            else
                return PartialView("../Manufacture/PackingBillofMaterials/_IndexGrid", db.Set<PackingBillofMaterial>().AsQueryable()
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
                return db.PackingBillofMaterials.Any(x => x.Code == Code);
            }
            else
            {
                return db.PackingBillofMaterials.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: SalesRequests/Details/
        [Authorize(Roles = "PackingBillofMaterialsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PackingBillofMaterial PackingBillofMaterial = db.PackingBillofMaterials.Find(id);
            if (PackingBillofMaterial == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/PackingBillofMaterials/_Details", PackingBillofMaterial);
        }

        [HttpGet]
        [Authorize(Roles = "PackingBillofMaterialsView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Manufacture/PackingBillofMaterials/_ViewGrid", db.PackingBillofMaterialsDetails
                .Where(x => x.PackingBillofMaterialId == Id).ToList());
        }

        // GET: SalesRequests/Create
        [Authorize(Roles = "PackingBillofMaterialsAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            PackingBillofMaterial packingBillofMaterial = new PackingBillofMaterial
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MasterCurrencyId = masterCurrency.Id,
                Rate = masterCurrency.Rate,
                MasterUnitId = db.MasterUnits.FirstOrDefault().Id,
                // MasterWarehouseId = db.MasterWarehouses.FirstOrDefault().Id,
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
                    db.PackingBillofMaterials.Add(packingBillofMaterial);
                    db.SaveChanges();

                    dbTran.Commit();

                    packingBillofMaterial.Code = "";
                    packingBillofMaterial.Active = true;
                    packingBillofMaterial.MasterBusinessUnitId = 0;
                    packingBillofMaterial.MasterRegionId = 0;
                    // packingBillofMaterial.MasterCustomerId = 0;
                    // packingBillofMaterialt.MasterWarehouseId = 0;
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

            return View("../Manufacture/PackingBillofMaterials/Create", packingBillofMaterial);
        }

        // POST: PurchaseRequests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PackingBillofMaterialsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Date,MasterBusinessUnitId,MasterRegionId,Quantity,MasterUnitId,Notes,Active,Created,Updated,UserId")] PackingBillofMaterial packingBillofMaterial)
        {
            packingBillofMaterial.Created = DateTime.Now;
            packingBillofMaterial.Updated = DateTime.Now;
            packingBillofMaterial.UserId = User.Identity.GetUserId<int>();
            packingBillofMaterial.Total = SharedFunctions.GetTotalSalesRequest(db, packingBillofMaterial.Id);
            packingBillofMaterial.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(packingBillofMaterial.Code)) packingBillofMaterial.Code = packingBillofMaterial.Code.ToUpper();
            if (!string.IsNullOrEmpty(packingBillofMaterial.Notes)) packingBillofMaterial.Notes = packingBillofMaterial.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                packingBillofMaterial = GetModelState(packingBillofMaterial);
            }

            db.Entry(packingBillofMaterial).State = EntityState.Unchanged;
            db.Entry(packingBillofMaterial).Property("Code").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Name").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Date").IsModified = true;
            db.Entry(packingBillofMaterial).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(packingBillofMaterial).Property("MasterRegionId").IsModified = true;
            db.Entry(packingBillofMaterial).Property("MasterUnitId").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Quantity").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Total").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Notes").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Active").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PackingBillofMaterial, MenuId = packingBillofMaterial.Id, MenuCode = packingBillofMaterial.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", packingBillofMaterial.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalSalesRequest(db, packingBillofMaterial.Id).ToString("N2");

                return View("../Manufacture/PackingBillofMaterials/Create", packingBillofMaterial);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PackingBillofMaterialsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                PackingBillofMaterial obj = db.PackingBillofMaterials.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.PackingBillofMaterialsDetails.Where(x => x.PackingBillofMaterialId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PackingBillofMaterialsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.PackingBillofMaterials.Remove(obj);
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

        // GET: PackingBillofMaterials/Edit/5
        [Authorize(Roles = "PackingBillofMaterialsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PackingBillofMaterial packingBillofMaterial = db.PackingBillofMaterials.Find(id);
            if (packingBillofMaterial == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", packingBillofMaterial.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalSalesRequest(db, packingBillofMaterial.Id).ToString("N2");

            return View("../Manufacture/PackingBillofMaterials/Edit", packingBillofMaterial);
        }

        // POST: PurchaseRequests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PackingBillofMaterialsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Date,MasterBusinessUnitId,MasterRegionId,Quantity,MasterUnitId,Notes,Active,Created,Updated,UserId")] PackingBillofMaterial packingBillofMaterial)
        {
            packingBillofMaterial.Updated = DateTime.Now;
            packingBillofMaterial.UserId = User.Identity.GetUserId<int>();
            packingBillofMaterial.Total = SharedFunctions.GetTotalSalesRequest(db, packingBillofMaterial.Id);
            packingBillofMaterial.MasterCurrencyId = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault().Id;

            if (!string.IsNullOrEmpty(packingBillofMaterial.Code)) packingBillofMaterial.Code = packingBillofMaterial.Code.ToUpper();
            if (!string.IsNullOrEmpty(packingBillofMaterial.Notes)) packingBillofMaterial.Notes = packingBillofMaterial.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                packingBillofMaterial = GetModelState(packingBillofMaterial);
            }

            db.Entry(packingBillofMaterial).State = EntityState.Unchanged;
            db.Entry(packingBillofMaterial).Property("Code").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Name").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Date").IsModified = true;
            db.Entry(packingBillofMaterial).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(packingBillofMaterial).Property("MasterRegionId").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Total").IsModified = true;
            db.Entry(packingBillofMaterial).Property("MasterUnitId").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Quantity").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Notes").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Active").IsModified = true;
            db.Entry(packingBillofMaterial).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesRequest, MenuId = packingBillofMaterial.Id, MenuCode = packingBillofMaterial.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", packingBillofMaterial.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalSalesRequest(db, packingBillofMaterial.Id).ToString("N2");

                return View("../Manufacture/PackingBillofMaterials/Edit", packingBillofMaterial);
            }
        }

        [HttpPost]
        [Authorize(Roles = "PackingBillofMaterialsDelete")]
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
                            PackingBillofMaterial obj = db.PackingBillofMaterials.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                PackingBillofMaterial tmp = obj;

                                var details = db.PackingBillofMaterialsDetails.Where(x => x.PackingBillofMaterialId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.PackingBillofMaterialsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.PackingBillofMaterials.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PackingBillofMaterial, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "PackingBillofMaterialsPrint")]
        public ActionResult Print(int? id)
        {
            PackingBillofMaterial obj = db.PackingBillofMaterials.Find(id);

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

        [Authorize(Roles = "PackingBillofMaterialsActive")]
        private PackingBillofMaterial GetModelState(PackingBillofMaterial packingBillofMaterial)
        {
            List<PackingBillofMaterialDetails> packingBillofMaterialDetails = db.PackingBillofMaterialsDetails.Where(x => x.PackingBillofMaterialId == packingBillofMaterial.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(packingBillofMaterial.Code, packingBillofMaterial.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (packingBillofMaterialDetails == null || packingBillofMaterialDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return packingBillofMaterial;
        }

        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public ActionResult DetailsCreate(int packingBillofMaterialId)
        {
            PackingBillofMaterial packingBillofMaterial = db.PackingBillofMaterials.Find(packingBillofMaterialId);

            if (packingBillofMaterial == null)
            {
                return HttpNotFound();
            }

            PackingBillofMaterialDetails packingBillofMaterialDetails = new PackingBillofMaterialDetails
            {
                PackingBillofMaterialId = packingBillofMaterialId
            };

            return PartialView("../Manufacture/PackingBillofMaterials/_DetailsCreate", packingBillofMaterialDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,PackingBillofMaterialId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PackingBillofMaterialDetails packingBillofMaterialDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(packingBillofMaterialDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                packingBillofMaterialDetails.Total = 0;
            else
                packingBillofMaterialDetails.Total = packingBillofMaterialDetails.Quantity * packingBillofMaterialDetails.Price * masterItemUnit.MasterUnit.Ratio;

            packingBillofMaterialDetails.Created = DateTime.Now;
            packingBillofMaterialDetails.Updated = DateTime.Now;
            packingBillofMaterialDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(packingBillofMaterialDetails.Notes)) packingBillofMaterialDetails.Notes = packingBillofMaterialDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.PackingBillofMaterialsDetails.Add(packingBillofMaterialDetails);
                        db.SaveChanges();

                        PackingBillofMaterial packingBillofMaterial = db.PackingBillofMaterials.Find(packingBillofMaterialDetails.PackingBillofMaterialId);
                        packingBillofMaterial.Total = SharedFunctions.GetTotalSalesRequest(db, packingBillofMaterial.Id, packingBillofMaterialDetails.Id) + packingBillofMaterialDetails.Total;

                        db.Entry(packingBillofMaterial).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PackingBillofMaterialDetails, MenuId = packingBillofMaterialDetails.Id, MenuCode = packingBillofMaterialDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Manufacture/PackingBillofMaterials/_DetailsCreate", packingBillofMaterialDetails);
        }

        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PackingBillofMaterialDetails obj = db.PackingBillofMaterialsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/PackingBillofMaterials/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,PackingBillofMaterialId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] PackingBillofMaterialDetails packingBillofMaterialDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(packingBillofMaterialDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                packingBillofMaterialDetails.Total = 0;
            else
                packingBillofMaterialDetails.Total = packingBillofMaterialDetails.Quantity * packingBillofMaterialDetails.Price * masterItemUnit.MasterUnit.Ratio;

            packingBillofMaterialDetails.Updated = DateTime.Now;
            packingBillofMaterialDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(packingBillofMaterialDetails.Notes)) packingBillofMaterialDetails.Notes = packingBillofMaterialDetails.Notes.ToUpper();

            db.Entry(packingBillofMaterialDetails).State = EntityState.Unchanged;
            db.Entry(packingBillofMaterialDetails).Property("MasterItemId").IsModified = true;
            db.Entry(packingBillofMaterialDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(packingBillofMaterialDetails).Property("Quantity").IsModified = true;
            db.Entry(packingBillofMaterialDetails).Property("Price").IsModified = true;
            db.Entry(packingBillofMaterialDetails).Property("Total").IsModified = true;
            db.Entry(packingBillofMaterialDetails).Property("Notes").IsModified = true;
            db.Entry(packingBillofMaterialDetails).Property("Updated").IsModified = true;
            db.Entry(packingBillofMaterialDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        PackingBillofMaterial packingBillofMaterial = db.PackingBillofMaterials.Find(packingBillofMaterialDetails.PackingBillofMaterialId);
                        packingBillofMaterial.Total = SharedFunctions.GetTotalPackingBillofMaterial(db, packingBillofMaterial.Id, packingBillofMaterialDetails.Id) + packingBillofMaterialDetails.Total;

                        db.Entry(packingBillofMaterial).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PackingBillofMaterialDetails, MenuId = packingBillofMaterialDetails.Id, MenuCode = packingBillofMaterialDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Manufacture/PackingBillofMaterials/_DetailsEdit", packingBillofMaterialDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PackingBillofMaterialsActive")]
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
                        PackingBillofMaterialDetails obj = db.PackingBillofMaterialsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    PackingBillofMaterialDetails tmp = obj;

                                    PackingBillofMaterial packingBillofMaterial = db.PackingBillofMaterials.Find(tmp.PackingBillofMaterialId);

                                    packingBillofMaterial.Total = SharedFunctions.GetTotalPackingBillofMaterial(db, packingBillofMaterial.Id, tmp.Id);

                                    db.Entry(packingBillofMaterial).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.PackingBillofMaterialsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PackingBillofMaterialDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Manufacture/PackingBillofMaterials/_DetailsGrid", db.PackingBillofMaterialsDetails
                .Where(x => x.PackingBillofMaterialId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PackingBillofMaterialsActive")]
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

        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public ActionResult ChangeCurrency(int? packingBillofMaterialId)
        {
            if (packingBillofMaterialId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PackingBillofMaterial packingBillofMaterial = db.PackingBillofMaterials.Find(packingBillofMaterialId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = packingBillofMaterial.Id,
                MasterCurrencyId = packingBillofMaterial.MasterCurrencyId,
                Rate = packingBillofMaterial.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Manufacture/PackingBillofMaterials/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public ActionResult ChangeCurrency([Bind(Include = "Id,MasterCurrencyId,Rate")] ChangeCurrency changeCurrency)
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Find(changeCurrency.MasterCurrencyId);

            PackingBillofMaterial packingBillofMaterial = db.PackingBillofMaterials.Find(changeCurrency.Id);
            packingBillofMaterial.MasterCurrencyId = changeCurrency.MasterCurrencyId;
            packingBillofMaterial.Rate = changeCurrency.Rate;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var packingBillofMaterialsDetails = db.PackingBillofMaterialsDetails.Where(x => x.PackingBillofMaterialId == packingBillofMaterial.Id).ToList();

                        foreach (PackingBillofMaterialDetails packingBillofMaterialDetails in packingBillofMaterialsDetails)
                        {
                            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(packingBillofMaterialDetails.MasterItemUnitId);

                            if (masterItemUnit == null)
                                packingBillofMaterialDetails.Total = 0;
                            else
                                packingBillofMaterialDetails.Total = packingBillofMaterialDetails.Quantity * packingBillofMaterialDetails.Price * masterItemUnit.MasterUnit.Ratio * packingBillofMaterial.Rate;

                            db.Entry(packingBillofMaterialDetails).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        packingBillofMaterial.Total = SharedFunctions.GetTotalSalesRequest(db, packingBillofMaterial.Id);
                        db.Entry(packingBillofMaterial).State = EntityState.Modified;
                        db.SaveChanges();

                        dbTran.Commit();

                        var returnObject = new
                        {
                            Status = "success",
                            Message = masterCurrency.Code + " : " + packingBillofMaterial.Rate.ToString("N2")
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

            return PartialView("../Manufacture/PackingBillofMaterials/_ChangeCurrency", changeCurrency);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            PackingBillofMaterial packingBillofMaterial = db.PackingBillofMaterials.Find(id);

            if (masterBusinessUnit != null && packingBillofMaterial != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                packingBillofMaterial.MasterBusinessUnitId = masterBusinessUnitId;
                packingBillofMaterial.MasterRegionId = masterRegionId;
                db.Entry(packingBillofMaterial).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "PackingBillofMaterialsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.PackingBillofMaterialCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            PackingBillofMaterial lastData = db.PackingBillofMaterials
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
        [Authorize(Roles = "PackingBillofMaterialsActive")]
        public JsonResult GetTotal(int packingBillofMaterialId)
        {
            return Json(SharedFunctions.GetTotalPackingBillofMaterial(db, packingBillofMaterialId).ToString("N2"));
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
