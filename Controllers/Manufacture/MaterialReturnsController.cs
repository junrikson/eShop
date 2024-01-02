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
    public class MaterialReturnsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MaterialReturns
        [Authorize(Roles = "MaterialReturnsActive")]
        public ActionResult Index()
        {
            return View("../Manufacture/MaterialReturns/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MaterialReturnsActive")]
        public PartialViewResult IndexGrid(string search)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            var masterRegions = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterRegionId).Distinct().ToList();
            var masterBusinessUnits = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnitId).Distinct().ToList();

            if (string.IsNullOrEmpty(search))
            {
                return PartialView("../Manufacture/MaterialReturns/_IndexGrid", db.Set<MaterialReturn>().Where(x =>
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable());
            }
            else
            {
                return PartialView("../Manufacture/MaterialReturns/_IndexGrid", db.Set<MaterialReturn>().Where(x =>
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable()
                        .Where(x => x.Code.Contains(search)));

            }
        }

        public JsonResult IsCodeExists(string Code, int? Id)
        {
            return Json(!IsAnyCode(Code, Id), JsonRequestBehavior.AllowGet);
        }

        private bool IsAnyCode(string Code, int? Id)
        {
            if (Id == null || Id == 0)
            {
                return db.MaterialReturns.Any(x => x.Code == Code);
            }
            else
            {
                return db.MaterialReturns.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: MaterialReturns/Details/
        [Authorize(Roles = "MaterialReturnsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaterialReturn MaterialReturn = db.MaterialReturns.Find(id);
            if (MaterialReturn == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/MaterialReturns/_Details", MaterialReturn);
        }

        [HttpGet]
        [Authorize(Roles = "MaterialReturnsView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Manufacture/MaterialReturns/_ViewGrid", db.MaterialReturnsDetails
                .Where(x => x.MaterialReturnId == Id).ToList());
        }

        // GET: MaterialReturns/Create
        [Authorize(Roles = "MaterialReturnsAdd")]
        public ActionResult Create()
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Where(x => x.Active && x.Default).FirstOrDefault();

            MaterialReturn materialReturn = new MaterialReturn
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                MasterCurrencyId = masterCurrency.Id,
                Rate = masterCurrency.Rate,
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
                    db.MaterialReturns.Add(materialReturn);
                    db.SaveChanges();

                    dbTran.Commit();

                    materialReturn.Code = "";
                    materialReturn.Active = true;
                    materialReturn.MasterBusinessUnitId = 0;
                    materialReturn.MasterRegionId = 0;
                    materialReturn.MasterWarehouseId = 0;
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

            return View("../Manufacture/MaterialReturns/Create", materialReturn);
        }

        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MaterialReturnsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MaterialSlipId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] MaterialReturn materialReturn)
        {
            materialReturn.Created = DateTime.Now;
            materialReturn.Updated = DateTime.Now;
            materialReturn.UserId = User.Identity.GetUserId<int>();
            materialReturn.Total = SharedFunctions.GetTotalMaterialReturn(db, materialReturn.Id);

            if (!string.IsNullOrEmpty(materialReturn.Code)) materialReturn.Code = materialReturn.Code.ToUpper();
            if (!string.IsNullOrEmpty(materialReturn.Notes)) materialReturn.Notes = materialReturn.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                materialReturn = GetModelState(materialReturn);
            }

            db.Entry(materialReturn).State = EntityState.Unchanged;
            db.Entry(materialReturn).Property("Code").IsModified = true;
            db.Entry(materialReturn).Property("Date").IsModified = true;
            db.Entry(materialReturn).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(materialReturn).Property("MasterRegionId").IsModified = true;
            db.Entry(materialReturn).Property("MaterialSlipId").IsModified = true;
            db.Entry(materialReturn).Property("MasterWarehouseId").IsModified = true;
            db.Entry(materialReturn).Property("Total").IsModified = true;
            db.Entry(materialReturn).Property("Notes").IsModified = true;
            db.Entry(materialReturn).Property("Active").IsModified = true;
            db.Entry(materialReturn).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MaterialReturn, MenuId = materialReturn.Id, MenuCode = materialReturn.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", materialReturn.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalMaterialReturn(db, materialReturn.Id).ToString("N2");

                return View("../Manufacture/MaterialReturns/Create", materialReturn);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialReturnsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                MaterialReturn obj = db.MaterialReturns.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var details = db.MaterialReturnsDetails.Where(x => x.MaterialReturnId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.MaterialReturnsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.MaterialReturns.Remove(obj);
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

        // GET: MaterialReturns/Edit/5
        [Authorize(Roles = "MaterialReturnsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MaterialReturn materialReturn = db.MaterialReturns.Find(id);
            if (materialReturn == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", materialReturn.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalMaterialReturn(db, materialReturn.Id).ToString("N2");

            return View("../Manufacture/MaterialReturns/Edit", materialReturn);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MaterialReturnsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,MasterWarehouseId,MasterCustomerId,Notes,Active,Created,Updated,UserId")] MaterialReturn materialReturn)
        {
            materialReturn.Updated = DateTime.Now;
            materialReturn.UserId = User.Identity.GetUserId<int>();
            materialReturn.Total = SharedFunctions.GetTotalMaterialReturn(db, materialReturn.Id);

            if (!string.IsNullOrEmpty(materialReturn.Code)) materialReturn.Code = materialReturn.Code.ToUpper();
            if (!string.IsNullOrEmpty(materialReturn.Notes)) materialReturn.Notes = materialReturn.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                materialReturn = GetModelState(materialReturn);
            }

            db.Entry(materialReturn).State = EntityState.Unchanged;
            db.Entry(materialReturn).Property("Code").IsModified = true;
            db.Entry(materialReturn).Property("Date").IsModified = true;
            db.Entry(materialReturn).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(materialReturn).Property("MasterRegionId").IsModified = true;
            db.Entry(materialReturn).Property("MasterWarehouseId").IsModified = true;
            db.Entry(materialReturn).Property("Total").IsModified = true;
            db.Entry(materialReturn).Property("Notes").IsModified = true;
            db.Entry(materialReturn).Property("Active").IsModified = true;
            db.Entry(materialReturn).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MaterialReturn, MenuId = materialReturn.Id, MenuCode = materialReturn.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", materialReturn.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalMaterialReturn(db, materialReturn.Id).ToString("N2");

                return View("../Manufacture/MaterialReturns/Edit", materialReturn);
            }
        }

        [HttpPost]
        [Authorize(Roles = "MaterialReturnsDelete")]
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
                            MaterialReturn obj = db.MaterialReturns.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                MaterialReturn tmp = obj;

                                var details = db.MaterialReturnsDetails.Where(x => x.MaterialReturnId == obj.Id).ToList();

                                if (details != null)
                                {
                                    db.MaterialReturnsDetails.RemoveRange(details);
                                    db.SaveChanges();
                                }

                                db.MaterialReturns.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MaterialReturn, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "MaterialReturnsPrint")]
        public ActionResult Print(int? id)
        {
            MaterialReturn obj = db.MaterialReturns.Find(id);

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
                        rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormMaterialReturn.rpt"));
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

        [Authorize(Roles = "MaterialReturnsActive")]
        private MaterialReturn GetModelState(MaterialReturn materialReturn)
        {
            List<MaterialReturnDetails> materialReturnDetails = db.MaterialReturnsDetails.Where(x => x.MaterialReturnId == materialReturn.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(materialReturn.Code, materialReturn.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (materialReturnDetails == null || materialReturnDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return materialReturn;
        }

        [Authorize(Roles = "MaterialReturnsActive")]
        public ActionResult DetailsCreate(int materialReturnId)
        {
            MaterialReturn materialReturn = db.MaterialReturns.Find(materialReturnId);

            if (materialReturn == null)
            {
                return HttpNotFound();
            }

            MaterialReturnDetails materialReturnDetails = new MaterialReturnDetails
            {
                MaterialReturnId = materialReturnId
            };

            return PartialView("../Manufacture/MaterialReturns/_DetailsCreate", materialReturnDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MaterialReturnsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,MaterialReturnId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] MaterialReturnDetails materialReturnDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(materialReturnDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                materialReturnDetails.Total = 0;
            else
                materialReturnDetails.Total = materialReturnDetails.Quantity * materialReturnDetails.Price * masterItemUnit.MasterUnit.Ratio;

            materialReturnDetails.Created = DateTime.Now;
            materialReturnDetails.Updated = DateTime.Now;
            materialReturnDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(materialReturnDetails.Notes)) materialReturnDetails.Notes = materialReturnDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.MaterialReturnsDetails.Add(materialReturnDetails);
                        db.SaveChanges();

                        MaterialReturn materialReturn = db.MaterialReturns.Find(materialReturnDetails.MaterialReturnId);
                        materialReturn.Total = SharedFunctions.GetTotalMaterialReturn(db, materialReturn.Id, materialReturnDetails.Id) + materialReturnDetails.Total;

                        db.Entry(materialReturn).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MaterialReturnDetails, MenuId = materialReturnDetails.Id, MenuCode = materialReturnDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Manufacture/MaterialReturns/_DetailsCreate", materialReturnDetails);
        }

        [Authorize(Roles = "MaterialReturnsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MaterialReturnDetails obj = db.MaterialReturnsDetails.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/MaterialReturns/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MaterialReturnsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,MaterialReturnId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] MaterialReturnDetails materialReturnDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(materialReturnDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                materialReturnDetails.Total = 0;
            else
                materialReturnDetails.Total = materialReturnDetails.Quantity * materialReturnDetails.Price * masterItemUnit.MasterUnit.Ratio;

            materialReturnDetails.Updated = DateTime.Now;
            materialReturnDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(materialReturnDetails.Notes)) materialReturnDetails.Notes = materialReturnDetails.Notes.ToUpper();

            db.Entry(materialReturnDetails).State = EntityState.Unchanged;
            db.Entry(materialReturnDetails).Property("MasterItemId").IsModified = true;
            db.Entry(materialReturnDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(materialReturnDetails).Property("Quantity").IsModified = true;
            db.Entry(materialReturnDetails).Property("Price").IsModified = true;
            db.Entry(materialReturnDetails).Property("Total").IsModified = true;
            db.Entry(materialReturnDetails).Property("Notes").IsModified = true;
            db.Entry(materialReturnDetails).Property("Updated").IsModified = true;
            db.Entry(materialReturnDetails).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        MaterialReturn materialReturn = db.MaterialReturns.Find(materialReturnDetails.MaterialReturnId);
                        materialReturn.Total = SharedFunctions.GetTotalMaterialReturn(db, materialReturn.Id, materialReturnDetails.Id) + materialReturnDetails.Total;

                        db.Entry(materialReturn).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MaterialReturnDetails, MenuId = materialReturnDetails.Id, MenuCode = materialReturnDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Manufacture/MaterialReturns/_DetailsEdit", materialReturnDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialReturnsActive")]
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
                        MaterialReturnDetails obj = db.MaterialReturnsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MaterialReturnDetails tmp = obj;

                                    MaterialReturn materialReturn = db.MaterialReturns.Find(tmp.MaterialReturnId);

                                    materialReturn.Total = SharedFunctions.GetTotalMaterialReturn(db, materialReturn.Id, tmp.Id);

                                    db.Entry(materialReturn).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.MaterialReturnsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MaterialReturnDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "MaterialReturnsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Manufacture/MaterialReturns/_DetailsGrid", db.MaterialReturnsDetails
                .Where(x => x.MaterialReturnId == Id).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialReturnsActive")]
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

        [Authorize(Roles = "MaterialReturnsActive")]
        public ActionResult ChangeCurrency(int? materialReturnId)
        {
            if (materialReturnId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MaterialReturn materialReturn = db.MaterialReturns.Find(materialReturnId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = materialReturn.Id,
                MasterCurrencyId = materialReturn.MasterCurrencyId,
                Rate = materialReturn.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Manufacture/MaterialReturns/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MaterialReturnsActive")]
        public ActionResult ChangeCurrency([Bind(Include = "Id,MasterCurrencyId,Rate")] ChangeCurrency changeCurrency)
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Find(changeCurrency.MasterCurrencyId);

            MaterialReturn materialReturn = db.MaterialReturns.Find(changeCurrency.Id);
            materialReturn.MasterCurrencyId = changeCurrency.MasterCurrencyId;
            materialReturn.Rate = changeCurrency.Rate;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var materialReturnsDetails = db.MaterialReturnsDetails.Where(x => x.MaterialReturnId == materialReturn.Id).ToList();

                        foreach (MaterialReturnDetails materialReturnDetails in materialReturnsDetails)
                        {
                            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(materialReturnDetails.MasterItemUnitId);

                            if (masterItemUnit == null)
                                materialReturnDetails.Total = 0;
                            else
                                materialReturnDetails.Total = materialReturnDetails.Quantity * materialReturnDetails.Price * masterItemUnit.MasterUnit.Ratio * materialReturn.Rate;

                            db.Entry(materialReturnDetails).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        materialReturn.Total = SharedFunctions.GetTotalSalesRequest(db, materialReturn.Id);
                        db.Entry(materialReturn).State = EntityState.Modified;
                        db.SaveChanges();

                        dbTran.Commit();

                        var returnObject = new
                        {
                            Status = "success",
                            Message = masterCurrency.Code + " : " + materialReturn.Rate.ToString("N2")
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

            return PartialView("../Manufacture/MaterialReturns/_ChangeCurrency", changeCurrency);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialReturnsActive")]
        public JsonResult GetCurrencyRate(int id)
        {
            return Json(db.MasterCurrencies.Find(id).Rate);
        }


        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialReturnsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            MaterialReturn materialReturn = db.MaterialReturns.Find(id);

            if (masterBusinessUnit != null && materialReturn != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                materialReturn.MasterBusinessUnitId = masterBusinessUnitId;
                materialReturn.MasterRegionId = masterRegionId;
                db.Entry(materialReturn).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "MaterialReturnsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.MaterialReturnCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            MaterialReturn lastData = db.MaterialReturns
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
        [Authorize(Roles = "MaterialReturnsActive")]
        public JsonResult GetTotal(int materialReturnId)
        {
            return Json(SharedFunctions.GetTotalMaterialReturn(db, materialReturnId).ToString("N2"));
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MaterialReturnsActive")]
        public JsonResult PopulateDetails(int materialReturnid, int materialSlipId)
        {
            MaterialReturn materialReturn = db.MaterialReturns.Find(materialReturnid);
            MaterialSlip materialSlip = db.MaterialSlips.Find(materialSlipId);

            if (materialReturn != null && materialSlip != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var remove = db.MaterialReturnsDetails.Where(x => x.MaterialReturnId == materialReturn.Id).ToList();

                        if (remove != null)
                        {
                            db.MaterialReturnsDetails.RemoveRange(remove);
                            db.SaveChanges();
                        }

                        var materialSlipsDetails = db.MaterialSlipsDetails.Where(x => x.MaterialSlipId == materialSlip.Id).ToList();

                        if (materialSlipsDetails != null)
                        {
                            foreach (MaterialSlipDetails materialSlipDetails in materialSlipsDetails)
                            {
                                MaterialReturnDetails materialReturnDetails = new MaterialReturnDetails
                                {
                                    MaterialReturnId = materialReturn.Id,
                                    MasterItemId = materialSlipDetails.MasterItemId,
                                    MasterItemUnitId = materialSlipDetails.MasterItemUnitId,
                                    Quantity = materialSlipDetails.Quantity,
                                    //Price = materialSlipDetails.Price,
                                    Total = materialSlipDetails.Total,
                                    Notes = materialSlipDetails.Notes,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MaterialReturnsDetails.Add(materialReturnDetails);
                                db.SaveChanges();
                            }
                        }

                        materialReturn.MaterialSlipId = materialSlip.Id;
                        materialReturn.MasterBusinessUnitId = materialSlip.MasterBusinessUnitId;
                        materialReturn.MasterRegionId = materialSlip.MasterRegionId;
                        //materialReturn.MasterCurrencyId = materialSlip.MasterCurrencyId;
                        //materialReturn.Rate = materialSlip.Rate;
                        materialReturn.Notes = materialSlip.Notes;
                        materialReturn.Total = materialSlip.Total;

                        db.Entry(materialReturn).State = EntityState.Modified;
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
                materialReturn.MasterRegionId,
                materialReturn.MasterBusinessUnitId,
                materialReturn.MasterWarehouseId,
                materialReturn.Notes,
                Total = materialReturn.Total.ToString("N2"),
                materialReturn.Date,
                Currency = materialReturn.MasterCurrency.Code + " : " + materialReturn.Rate.ToString("N2")
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
