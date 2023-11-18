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
                MasterCurrencyId = masterCurrency.Id,
                Rate = masterCurrency.Rate,
                //MasterCustomerId = db.MasterCustomers.FirstOrDefault().Id,
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
                    //finishedGoodSlip.MasterCustomerId = 0;
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
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,ProductionWorkOrderId,MasterWarehouseId,Notes,Active,Created,Updated,UserId")] FinishedGoodSlip finishedGoodSlip)
        {
            finishedGoodSlip.Created = DateTime.Now;
            finishedGoodSlip.Updated = DateTime.Now;
            finishedGoodSlip.UserId = User.Identity.GetUserId<int>();
            finishedGoodSlip.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id);

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
            db.Entry(finishedGoodSlip).Property("ProductionWorkOrderId").IsModified = true;
            //db.Entry(finishedGoodSlip).Property("MasterCustomerId").IsModified = true;
            db.Entry(finishedGoodSlip).Property("MasterWarehouseId").IsModified = true;
            db.Entry(finishedGoodSlip).Property("Total").IsModified = true;
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
                ViewBag.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id).ToString("N2");

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
            ViewBag.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id).ToString("N2");

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
            finishedGoodSlip.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id);

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
            // db.Entry(finishedGoodSlip).Property("MasterCustomerId").IsModified = true;
            db.Entry(finishedGoodSlip).Property("Total").IsModified = true;
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
                ViewBag.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id).ToString("N2");

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,FinishedGoodSlipId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] FinishedGoodSlipDetails finishedGoodSlipDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(finishedGoodSlipDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                finishedGoodSlipDetails.Total = 0;
            else
                finishedGoodSlipDetails.Total = finishedGoodSlipDetails.Quantity * finishedGoodSlipDetails.Price * masterItemUnit.MasterUnit.Ratio;

            finishedGoodSlipDetails.Created = DateTime.Now;
            finishedGoodSlipDetails.Updated = DateTime.Now;
            finishedGoodSlipDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(finishedGoodSlipDetails.Notes)) finishedGoodSlipDetails.Notes = finishedGoodSlipDetails.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.FinishedGoodSlipsDetails.Add(finishedGoodSlipDetails);
                        db.SaveChanges();

                        FinishedGoodSlip finishedGoodSlip = db.FinishedGoodSlips.Find(finishedGoodSlipDetails.FinishedGoodSlipId);
                        finishedGoodSlip.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id, finishedGoodSlipDetails.Id) + finishedGoodSlipDetails.Total;

                        db.Entry(finishedGoodSlip).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.FinishedGoodSlipDetails, MenuId = finishedGoodSlipDetails.Id, MenuCode = finishedGoodSlipDetails.MasterItemUnit.MasterUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Manufacture/FinishedGoodSlips/_DetailsCreate", finishedGoodSlipDetails);
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
        public ActionResult DetailsEdit([Bind(Include = "Id,FinishedGoodSlipId,MasterItemId,MasterItemUnitId,Quantity,Price,Notes,Created,Updated,UserId")] FinishedGoodSlipDetails finishedGoodSlipDetails)
        {
            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(finishedGoodSlipDetails.MasterItemUnitId);

            if (masterItemUnit == null)
                finishedGoodSlipDetails.Total = 0;
            else
                finishedGoodSlipDetails.Total = finishedGoodSlipDetails.Quantity * finishedGoodSlipDetails.Price * masterItemUnit.MasterUnit.Ratio;

            finishedGoodSlipDetails.Updated = DateTime.Now;
            finishedGoodSlipDetails.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(finishedGoodSlipDetails.Notes)) finishedGoodSlipDetails.Notes = finishedGoodSlipDetails.Notes.ToUpper();

            db.Entry(finishedGoodSlipDetails).State = EntityState.Unchanged;
            db.Entry(finishedGoodSlipDetails).Property("MasterItemId").IsModified = true;
            db.Entry(finishedGoodSlipDetails).Property("MasterItemUnitId").IsModified = true;
            db.Entry(finishedGoodSlipDetails).Property("Quantity").IsModified = true;
            db.Entry(finishedGoodSlipDetails).Property("Price").IsModified = true;
            db.Entry(finishedGoodSlipDetails).Property("Total").IsModified = true;
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
                        finishedGoodSlip.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id, finishedGoodSlipDetails.Id) + finishedGoodSlipDetails.Total;

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

                                    finishedGoodSlip.Total = SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlip.Id, tmp.Id);

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

        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public ActionResult ChangeCurrency(int? finishedGoodSlipId)
        {
            if (finishedGoodSlipId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            FinishedGoodSlip finishedGoodSlip = db.FinishedGoodSlips.Find(finishedGoodSlipId);

            ChangeCurrency obj = new ChangeCurrency
            {
                Id = finishedGoodSlip.Id,
                MasterCurrencyId = finishedGoodSlip.MasterCurrencyId,
                Rate = finishedGoodSlip.Rate
            };

            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Manufacture/FinishedGoodSlips/_ChangeCurrency", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public ActionResult ChangeCurrency([Bind(Include = "Id,MasterCurrencyId,Rate")] ChangeCurrency changeCurrency)
        {
            MasterCurrency masterCurrency = db.MasterCurrencies.Find(changeCurrency.MasterCurrencyId);

            FinishedGoodSlip finishedGoodSlip = db.FinishedGoodSlips.Find(changeCurrency.Id);
            finishedGoodSlip.MasterCurrencyId = changeCurrency.MasterCurrencyId;
            finishedGoodSlip.Rate = changeCurrency.Rate;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var finishedGoodSlipsDetails = db.FinishedGoodSlipsDetails.Where(x => x.FinishedGoodSlipId == finishedGoodSlip.Id).ToList();

                        foreach (FinishedGoodSlipDetails finishedGoodSlipDetails in finishedGoodSlipsDetails)
                        {
                            MasterItemUnit masterItemUnit = db.MasterItemUnits.Find(finishedGoodSlipDetails.MasterItemUnitId);

                            if (masterItemUnit == null)
                                finishedGoodSlipDetails.Total = 0;
                            else
                                finishedGoodSlipDetails.Total = finishedGoodSlipDetails.Quantity * finishedGoodSlipDetails.Price * masterItemUnit.MasterUnit.Ratio * finishedGoodSlip.Rate;

                            db.Entry(finishedGoodSlipDetails).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        finishedGoodSlip.Total = SharedFunctions.GetTotalSalesRequest(db, finishedGoodSlip.Id);
                        db.Entry(finishedGoodSlip).State = EntityState.Modified;
                        db.SaveChanges();

                        dbTran.Commit();

                        var returnObject = new
                        {
                            Status = "success",
                            Message = masterCurrency.Code + " : " + finishedGoodSlip.Rate.ToString("N2")
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

            return PartialView("../Manufacture/FinishedGoodSlips/_ChangeCurrency", changeCurrency);
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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public JsonResult GetTotal(int finishedGoodSlipId)
        {
            return Json(SharedFunctions.GetTotalFinishedGoodSlip(db, finishedGoodSlipId).ToString("N2"));
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public JsonResult PopulateDetails(int finishedGoodSlipid, int productionWorkOrderId)
        {
            FinishedGoodSlip finishedGoodSlip = db.FinishedGoodSlips.Find(finishedGoodSlipid);
            ProductionWorkOrder productionWorkOrder = db.ProductionWorkOrders.Find(productionWorkOrderId);

            if (finishedGoodSlip != null && productionWorkOrder != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var remove = db.FinishedGoodSlipsDetails.Where(x => x.FinishedGoodSlipId == finishedGoodSlip.Id).ToList();

                        if (remove != null)
                        {
                            db.FinishedGoodSlipsDetails.RemoveRange(remove);
                            db.SaveChanges();
                        }

                        var productionWorkOrdersDetails = db.ProductionWorkOrdersDetails.Where(x => x.ProductionWorkOrderId == productionWorkOrder.Id).ToList();

                        if (productionWorkOrdersDetails != null)
                        {
                            foreach (ProductionWorkOrderDetails productionWorkOrderDetails in productionWorkOrdersDetails)
                            {
                                FinishedGoodSlipDetails finishedGoodSlipDetails = new FinishedGoodSlipDetails
                                {
                                    FinishedGoodSlipId = finishedGoodSlip.Id,
                                    MasterItemId = productionWorkOrderDetails.MasterItemId,
                                    MasterItemUnitId = productionWorkOrderDetails.MasterItemUnitId,
                                    Quantity = productionWorkOrderDetails.Quantity,
                                    Price = productionWorkOrderDetails.Price,
                                    Total = productionWorkOrderDetails.Total,
                                    Notes = productionWorkOrderDetails.Notes,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.FinishedGoodSlipsDetails.Add(finishedGoodSlipDetails);
                                db.SaveChanges();
                            }
                        }

                        finishedGoodSlip.ProductionWorkOrderId = productionWorkOrder.Id;
                        finishedGoodSlip.MasterBusinessUnitId = productionWorkOrder.MasterBusinessUnitId;
                        finishedGoodSlip.MasterRegionId = productionWorkOrder.MasterRegionId;
                        finishedGoodSlip.MasterCurrencyId = productionWorkOrder.MasterCurrencyId;
                        finishedGoodSlip.Rate = productionWorkOrder.Rate;
                        finishedGoodSlip.Notes = productionWorkOrder.Notes;
                        finishedGoodSlip.Total = productionWorkOrder.Total;

                        db.Entry(finishedGoodSlip).State = EntityState.Modified;
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
                finishedGoodSlip.MasterRegionId,
                finishedGoodSlip.MasterBusinessUnitId,
                finishedGoodSlip.MasterWarehouseId,
                finishedGoodSlip.Notes,
                Total = finishedGoodSlip.Total.ToString("N2"),
                finishedGoodSlip.Date,
                Currency = finishedGoodSlip.MasterCurrency.Code + " : " + finishedGoodSlip.Rate.ToString("N2")
            });
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "FinishedGoodSlipsActive")]
        public JsonResult PopulatePackingDetails(int finishedGoodSlipid, int packingWorkOrderId)
        {
            FinishedGoodSlip finishedGoodSlip = db.FinishedGoodSlips.Find(finishedGoodSlipid);
            PackingWorkOrder packingWorkOrder = db.PackingWorkOrders.Find(packingWorkOrderId);

            if (finishedGoodSlip != null && packingWorkOrder != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var remove = db.FinishedGoodSlipsDetails.Where(x => x.FinishedGoodSlipId == finishedGoodSlip.Id).ToList();

                        if (remove != null)
                        {
                            db.FinishedGoodSlipsDetails.RemoveRange(remove);
                            db.SaveChanges();
                        }

                        var packingWorkOrdersDetails = db.PackingWorkOrdersDetails.Where(x => x.PackingWorkOrderId == packingWorkOrder.Id).ToList();

                        if (packingWorkOrdersDetails != null)
                        {
                            foreach (PackingWorkOrderDetails packingWorkOrderDetails in packingWorkOrdersDetails)
                            {
                                FinishedGoodSlipDetails finishedGoodSlipDetails = new FinishedGoodSlipDetails
                                {
                                    FinishedGoodSlipId = finishedGoodSlip.Id,
                                    MasterItemId = packingWorkOrderDetails.MasterItemId,
                                    MasterItemUnitId = packingWorkOrderDetails.MasterItemUnitId,
                                    Quantity = packingWorkOrderDetails.Quantity,
                                    Price = packingWorkOrderDetails.Price,
                                    Total = packingWorkOrderDetails.Total,
                                    Notes = packingWorkOrderDetails.Notes,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.FinishedGoodSlipsDetails.Add(finishedGoodSlipDetails);
                                db.SaveChanges();
                            }
                        }

                        finishedGoodSlip.PackingWorkOrderId = packingWorkOrder.Id;
                        finishedGoodSlip.MasterBusinessUnitId = packingWorkOrder.MasterBusinessUnitId;
                        finishedGoodSlip.MasterRegionId = packingWorkOrder.MasterRegionId;
                        finishedGoodSlip.MasterCurrencyId = packingWorkOrder.MasterCurrencyId;
                        finishedGoodSlip.Rate = packingWorkOrder.Rate;
                        finishedGoodSlip.Notes = packingWorkOrder.Notes;
                        finishedGoodSlip.Total = packingWorkOrder.Total;

                        db.Entry(finishedGoodSlip).State = EntityState.Modified;
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
                finishedGoodSlip.MasterRegionId,
                finishedGoodSlip.MasterBusinessUnitId,
                finishedGoodSlip.MasterWarehouseId,
                finishedGoodSlip.Notes,
                Total = finishedGoodSlip.Total.ToString("N2"),
                finishedGoodSlip.Date,
                Currency = finishedGoodSlip.MasterCurrency.Code + " : " + finishedGoodSlip.Rate.ToString("N2")
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
