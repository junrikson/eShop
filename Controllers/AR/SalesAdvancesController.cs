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
    [Authorize]
    public class SalesAdvancesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SalesAdvances
        [Authorize(Roles = "SalesAdvancesActive")]
        public ActionResult Index()
        {
            ViewBag.Resi = "";
            return View("../AR/SalesAdvances/Index");
        }

        [HttpGet]
        [Authorize(Roles = "SalesAdvancesActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../AR/SalesAdvances/_IndexGrid", db.Set<SalesAdvance>().AsQueryable());
            else
                return PartialView("../AR/SalesAdvances/_IndexGrid", db.Set<SalesAdvance>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "SalesAdvancesActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.SalesAdvances.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.SalesAdvances.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: SalesAdvances/Details/5
        [Authorize(Roles = "SalesAdvancesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesAdvance salesAdvance = db.SalesAdvances.Find(id);
            if (salesAdvance == null)
            {
                return HttpNotFound();
            }
            return PartialView("../AR/SalesAdvances/_Details", salesAdvance);
        }

        // GET: SalesAdvances/Create
        [Authorize(Roles = "SalesAdvancesAdd")]
        public ActionResult Create()
        {
            SalesAdvance salesAdvance = new SalesAdvance
            {
                Code = "Sedang Input/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                IsPrint = false,
                Total = 0,
                Active = false,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UserId = User.Identity.GetUserId<int>()
            };

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    db.SalesAdvances.Add(salesAdvance);
                    db.SaveChanges();

                    Journal journal = SharedFunctions.CreateSalesAdvanceJournal(salesAdvance, db);
                    dbTran.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            salesAdvance.Code = "";
            salesAdvance.Active = true;
            salesAdvance.MasterBusinessUnitId = 0;

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name");
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes");
            return View("../AR/SalesAdvances/Create", salesAdvance);
        }

        // POST: SalesAdvances/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesAdvancesAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,Notes,Total,Active,Created,Updated,UserId")] SalesAdvance salesAdvance)
        {
            salesAdvance.Created = DateTime.Now;
            salesAdvance.Updated = DateTime.Now;
            salesAdvance.UserId = User.Identity.GetUserId<int>();
            salesAdvance.Total = SharedFunctions.GetTotalSalesAdvance(db, salesAdvance.Id);

            if (db.SalesAdvancesDetails.Where(x => x.SalesAdvanceId == salesAdvance.Id).FirstOrDefault() == null)
                ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");

            if (ModelState.IsValid)
            {
                List<SalesAdvanceDetails> salesAdvancesDetails = db.SalesAdvancesDetails.Where(x => x.SalesAdvanceId == salesAdvance.Id).ToList();
                Journal journal = db.Journals.Where(x =>
                                        x.Type == EnumJournalType.SalesAdvance &&
                                        x.SalesAdvanceId == salesAdvance.Id).FirstOrDefault();

                if (!string.IsNullOrEmpty(salesAdvance.Code)) salesAdvance.Code = salesAdvance.Code.ToUpper();
                if (!string.IsNullOrEmpty(salesAdvance.Notes)) salesAdvance.Notes = salesAdvance.Notes.ToUpper();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Entry(salesAdvance).State = EntityState.Modified;
                        db.SaveChanges();

                        if (journal == null)
                        {
                            journal = SharedFunctions.CreateSalesAdvanceJournal(salesAdvance, db);
                            foreach (SalesAdvanceDetails salesAdvanceDetails in salesAdvancesDetails)
                            {
                                SharedFunctions.CreateSalesAdvanceJournalDetails(salesAdvanceDetails, journal, db);
                            }
                        }
                        else
                            journal = SharedFunctions.UpdateSalesAdvanceJournal(journal, salesAdvance, db);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesAdvance, MenuId = salesAdvance.Id, MenuCode = salesAdvance.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return RedirectToAction("Index");

                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", salesAdvance.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", salesAdvance.MasterRegionId);
            return View("../AR/SalesAdvances/Create", salesAdvance);
        }

        // GET: SalesAdvances/Edit/5
        [Authorize(Roles = "SalesAdvancesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesAdvance salesAdvance = db.SalesAdvances.Find(id);
            if (salesAdvance == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", salesAdvance.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", salesAdvance.MasterRegionId);
            return View("../AR/SalesAdvances/Edit", salesAdvance);
        }

        // POST: SalesAdvances/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesAdvancesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,Notes,Total,Active,Created,Updated,UserId")] SalesAdvance salesAdvance)
        {
            salesAdvance.Updated = DateTime.Now;
            salesAdvance.UserId = User.Identity.GetUserId<int>();
            salesAdvance.Total = SharedFunctions.GetTotalSalesAdvance(db, salesAdvance.Id);

            if (db.SalesAdvancesDetails.Where(x => x.SalesAdvanceId == salesAdvance.Id).FirstOrDefault() == null)
                ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        List<SalesAdvanceDetails> salesAdvancesDetails = db.SalesAdvancesDetails.Where(x => x.SalesAdvanceId == salesAdvance.Id).ToList();
                        Journal journal = db.Journals.Where(x =>
                                                x.Type == EnumJournalType.SalesAdvance &&
                                                x.SalesAdvanceId == salesAdvance.Id).FirstOrDefault();

                        if (!string.IsNullOrEmpty(salesAdvance.Notes)) salesAdvance.Notes = salesAdvance.Notes.ToUpper();

                        db.Entry(salesAdvance).State = EntityState.Unchanged;
                        db.Entry(salesAdvance).Property("Date").IsModified = true;
                        db.Entry(salesAdvance).Property("MasterBusinessUnitId").IsModified = true;
                        db.Entry(salesAdvance).Property("MasterRegionId").IsModified = true;
                        db.Entry(salesAdvance).Property("Notes").IsModified = true;
                        db.Entry(salesAdvance).Property("Total").IsModified = true;
                        db.Entry(salesAdvance).Property("Active").IsModified = true;
                        db.Entry(salesAdvance).Property("Updated").IsModified = true;
                        db.Entry(salesAdvance).Property("UserId").IsModified = true;
                        db.SaveChanges();

                        if (journal == null)
                        {
                            journal = SharedFunctions.CreateSalesAdvanceJournal(salesAdvance, db);
                            foreach (SalesAdvanceDetails salesAdvanceDetails in salesAdvancesDetails)
                            {
                                SharedFunctions.CreateSalesAdvanceJournalDetails(salesAdvanceDetails, journal, db);
                            }
                        }
                        else
                            journal = SharedFunctions.UpdateSalesAdvanceJournal(journal, salesAdvance, db);


                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesAdvance, MenuId = salesAdvance.Id, MenuCode = salesAdvance.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return RedirectToAction("Index");

                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", salesAdvance.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", salesAdvance.MasterRegionId);
            return View("../AR/SalesAdvances/Edit", salesAdvance);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [Authorize(Roles = "SalesAdvancesActive")]
        public PartialViewResult OthersGrid(int? Id)
        {
            int salesAdvanceId = Id == null ? 0 : (int)Id;

            return PartialView("../AR/SalesAdvances/_OthersGrid", db.SalesAdvancesDetails
                .Where(x => x.SalesAdvanceId == salesAdvanceId).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesAdvancesActive")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                SalesAdvance obj = db.SalesAdvances.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                Journal journal = db.Journals.Where(y => y.Type == EnumJournalType.SalesAdvance && y.SalesAdvanceId == obj.Id).FirstOrDefault();

                                SharedFunctions.DeleteSalesAdvanceJournals(obj.Id, journal, db);

                                db.SalesAdvancesDetails.RemoveRange(db.SalesAdvancesDetails.Where(x => x.SalesAdvanceId == id));
                                db.SaveChanges();

                                db.SalesAdvances.Remove(obj);
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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesAdvancesDelete")]
        public ActionResult BatchDelete(int[] ids)
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
                        SalesAdvance obj = db.SalesAdvances.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            SalesAdvance tmp = obj;

                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    Journal journal = db.Journals.Where(y => y.Type == EnumJournalType.SalesAdvance && y.SalesAdvanceId == obj.Id).FirstOrDefault();

                                    SharedFunctions.DeleteSalesAdvanceJournals(obj.Id, journal, db);

                                    db.SalesAdvancesDetails.RemoveRange(db.SalesAdvancesDetails.Where(x => x.SalesAdvanceId == id));
                                    db.SaveChanges();

                                    db.SalesAdvances.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesAdvance, MenuId = tmp.Id, MenuCode = obj.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "SalesAdvancesActive")]
        public ActionResult DetailsCreate(int? salesAdvanceId)
        {
            if (salesAdvanceId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesAdvance salesAdvance = db.SalesAdvances.Find(salesAdvanceId);
            if (salesAdvance == null)
            {
                return HttpNotFound();
            }
            SalesAdvanceDetails salesAdvanceDetails = new SalesAdvanceDetails
            {
                SalesAdvanceId = salesAdvance.Id
            };
            return PartialView("../AR/SalesAdvances/_DetailsCreate", salesAdvanceDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesAdvancesActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,SalesAdvanceId,Type,MasterBankId,MasterCostId,GiroChequeId,Refference,Total,Notes,Created,Updated,UserId")] SalesAdvanceDetails salesAdvanceDetails)
        {
            salesAdvanceDetails.Created = DateTime.Now;
            salesAdvanceDetails.Updated = DateTime.Now;
            salesAdvanceDetails.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                salesAdvanceDetails = GetDetailsHeaderModelState(salesAdvanceDetails);
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(salesAdvanceDetails.Notes)) salesAdvanceDetails.Notes = salesAdvanceDetails.Notes.ToUpper();
                if (!string.IsNullOrEmpty(salesAdvanceDetails.Refference)) salesAdvanceDetails.Refference = salesAdvanceDetails.Refference.ToUpper();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SalesAdvancesDetails.Add(salesAdvanceDetails);
                        db.SaveChanges();

                        Journal journal = db.Journals.Where(x =>
                                                x.Type == EnumJournalType.SalesAdvance &&
                                                x.SalesAdvanceId == salesAdvanceDetails.SalesAdvanceId).FirstOrDefault();

                        if (journal != null)
                            SharedFunctions.CreateSalesAdvanceJournalDetails(salesAdvanceDetails, journal, db);

                        SalesAdvance salesAdvance = db.SalesAdvances.Find(salesAdvanceDetails.SalesAdvanceId);
                        salesAdvance.Total = SharedFunctions.GetTotalSalesAdvance(db, salesAdvance.Id) + salesAdvanceDetails.Total;

                        db.Entry(salesAdvance).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesAdvanceDetails, MenuId = salesAdvanceDetails.Id, MenuCode = salesAdvanceDetails.Id.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../AR/SalesAdvances/_DetailsCreate", salesAdvanceDetails);
        }

        [Authorize(Roles = "SalesAdvancesActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesAdvanceDetails obj = db.SalesAdvancesDetails.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../AR/SalesAdvances/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SalesAdvancesActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,SalesAdvanceId,Type,MasterBankId,MasterCostId,GiroChequeId,Refference,Total,Notes,Created,Updated,UserId")] SalesAdvanceDetails salesAdvanceDetails)
        {
            salesAdvanceDetails.Updated = DateTime.Now;
            salesAdvanceDetails.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                salesAdvanceDetails = GetDetailsHeaderModelState(salesAdvanceDetails);
            }

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(salesAdvanceDetails.Notes)) salesAdvanceDetails.Notes = salesAdvanceDetails.Notes.ToUpper();
                        if (!string.IsNullOrEmpty(salesAdvanceDetails.Refference)) salesAdvanceDetails.Refference = salesAdvanceDetails.Refference.ToUpper();

                        db.Entry(salesAdvanceDetails).State = EntityState.Unchanged;
                        db.Entry(salesAdvanceDetails).Property("Type").IsModified = true;
                        db.Entry(salesAdvanceDetails).Property("MasterBankId").IsModified = true;
                        db.Entry(salesAdvanceDetails).Property("MasterCostId").IsModified = true;
                        db.Entry(salesAdvanceDetails).Property("GiroChequeId").IsModified = true;
                        db.Entry(salesAdvanceDetails).Property("Refference").IsModified = true;
                        db.Entry(salesAdvanceDetails).Property("Total").IsModified = true;
                        db.Entry(salesAdvanceDetails).Property("Notes").IsModified = true;
                        db.Entry(salesAdvanceDetails).Property("Updated").IsModified = true;
                        db.Entry(salesAdvanceDetails).Property("UserId").IsModified = true;
                        db.SaveChanges();

                        SharedFunctions.UpdateSalesAdvanceJournalDetails(salesAdvanceDetails, db);

                        SalesAdvance salesAdvance = db.SalesAdvances.Find(salesAdvanceDetails.SalesAdvanceId);
                        salesAdvance.Total = SharedFunctions.GetTotalSalesAdvance(db, salesAdvance.Id) + salesAdvanceDetails.Total;

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesAdvanceDetails, MenuId = salesAdvanceDetails.Id, MenuCode = salesAdvanceDetails.Id.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../AR/SalesAdvances/_DetailsEdit", salesAdvanceDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesAdvancesActive")]
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
                        SalesAdvanceDetails obj = db.SalesAdvancesDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    SalesAdvanceDetails tmp = obj;

                                    Journal journal = db.Journals.Where(x =>
                                                            x.Type == EnumJournalType.SalesAdvance &&
                                                            x.SalesAdvanceId == obj.SalesAdvanceId).FirstOrDefault();

                                    if (journal != null)
                                        SharedFunctions.RemoveSalesAdvanceJournalDetails(obj, journal, db);

                                    SalesAdvance salesAdvance = db.SalesAdvances.Find(tmp.SalesAdvanceId);

                                    salesAdvance.Total = SharedFunctions.GetTotalSalesAdvance(db, salesAdvance.Id, tmp.Id);

                                    db.Entry(salesAdvance).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.SalesAdvancesDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesAdvanceDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "ARpaymentsActive")]
        private SalesAdvanceDetails GetDetailsHeaderModelState(SalesAdvanceDetails salesAdvanceDetails)
        {
            if (ModelState.IsValid)
            {
                if ((salesAdvanceDetails.Type == EnumSalesAdvanceType.Bank || salesAdvanceDetails.Type == EnumSalesAdvanceType.Cash) && (salesAdvanceDetails.MasterBankId == 0 || salesAdvanceDetails.MasterBankId == null))
                {
                    ModelState.AddModelError(string.Empty, "Master Kas / Bank belum diisi!");
                }
                else if (salesAdvanceDetails.Type == EnumSalesAdvanceType.Cheque && (salesAdvanceDetails.ChequeId == 0 || salesAdvanceDetails.ChequeId == null))
                {
                    ModelState.AddModelError(string.Empty, "Giro / Cek belum diisi!");
                }
                else if (salesAdvanceDetails.Type == EnumSalesAdvanceType.MasterCost && (salesAdvanceDetails.MasterCostId == 0 || salesAdvanceDetails.MasterCostId == null))
                {
                    ModelState.AddModelError(string.Empty, "Master Biaya belum diisi!");
                }
            }

            return salesAdvanceDetails;
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "SalesAdvancesActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);
            SalesAdvance salesAdvance = db.SalesAdvances.Find(id);

            if (masterBusinessUnit != null && salesAdvance != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                salesAdvance.Code = code;
                salesAdvance.MasterBusinessUnitId = masterBusinessUnitId;
                salesAdvance.MasterRegionId = masterRegionId;
                db.Entry(salesAdvance).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "SalesAdvancesActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.SalesAdvanceCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            SalesAdvance lastData = db.SalesAdvances
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
        [Authorize(Roles = "SalesAdvancesActive")]
        public JsonResult GetTotal(int SalesAdvanceId)
        {
            return Json(SharedFunctions.GetTotalSalesAdvance(db, SalesAdvanceId).ToString("N2"));
        }

        [Authorize(Roles = "SalesAdvancesPrint")]
        public ActionResult Print(int? id)
        {
            SalesAdvance obj = db.SalesAdvances.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }

            string details = "";
            string salesAdvanceDetails = "";

            details = details + obj.Notes;

            if (details != "") details = details + "\n";

            var temps = db.SalesAdvancesDetails.Where(x => x.SalesAdvanceId == obj.Id).ToList().OrderBy(y => y.Id);

            if (temps != null)
            {
                foreach (SalesAdvanceDetails temp in temps)
                {
                    if (salesAdvanceDetails != "") salesAdvanceDetails = salesAdvanceDetails + ",\n";

                    salesAdvanceDetails = salesAdvanceDetails + temp.Notes;
                }
            }

            if (salesAdvanceDetails != "") details = details + salesAdvanceDetails;

            using (ReportDocument rd = new ReportDocument())
            {
                rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormSalesAdvance.rpt"));
                rd.SetParameterValue("Code", obj.Code);
                rd.SetParameterValue("Details", details);
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

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.SalesAdvance, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.PRINT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return new CrystalReportPdfResult(rd, "CREDIT_NOTE_" + obj.Code);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
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
