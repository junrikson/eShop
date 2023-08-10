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
    public class JournalsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Journals
        [Authorize(Roles = "JournalsActive")]
        public ActionResult Index()
        {
            ViewBag.Resi = "";
            return View("../Accounting/Journals/Index");
        }

        [HttpGet]
        [Authorize(Roles = "JournalsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Accounting/Journals/_IndexGrid", db.Set<Journal>().Where(o => o.Type == EnumJournalType.General).AsQueryable());
            else
                return PartialView("../Accounting/Journals/_IndexGrid", db.Set<Journal>().Where(o => o.Type == EnumJournalType.General).AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [HttpGet]
        [Authorize(Roles = "JournalsActive")]
        public PartialViewResult OthersGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Accounting/Journals/_OthersGrid", db.Set<Journal>().Where(o => o.Type != EnumJournalType.General).AsQueryable());
            else
                return PartialView("../Accounting/Journals/_OthersGrid", db.Set<Journal>().Where(o => o.Type != EnumJournalType.General).AsQueryable()
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
                return db.Journals.Any(x => x.Code == Code);
            }
            else
            {
                return db.Journals.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: Journals/Details/5
        [Authorize(Roles = "JournalsView")]
        public ActionResult Details(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Journal journal = db.Journals.Find(id);
            if (journal == null)
            {
                return HttpNotFound();
            }
            ViewBag.JournalDetails = db.JournalsDetails.Where(x => x.JournalId == journal.Id).ToList();
            return PartialView("../Accounting/Journals/_Details", journal);
        }

        // GET: Journals/Create
        [Authorize(Roles = "JournalsAdd")]
        public ActionResult Create()
        {
            Journal journal = new Journal
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                Debit = 0,
                Credit = 0,
                Active = false,
                Type = EnumJournalType.General,
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UserId = User.Identity.GetUserId<int>()
            };

            db.Journals.Add(journal);
            db.SaveChanges();

            journal.Code = "";
            journal.Active = true;
            journal.MasterBusinessUnitId = 0;

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", journal.MasterBusinessUnitId);
            return View("../Accounting/Journals/Create", journal);
        }

        // POST: Journals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "JournalsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,Type,Debit,Credit,Notes,Active,Created,Updated,UserId")] Journal journal)
        {
            journal.Type = EnumJournalType.General;
            journal.Created = DateTime.Now;
            journal.Updated = DateTime.Now;
            journal.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                journal = GetModelState(journal);
            }
                    ;
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(journal.Code)) journal.Code = journal.Code.ToUpper();
                        if (!string.IsNullOrEmpty(journal.Notes)) journal.Notes = journal.Notes.ToUpper();

                        journal.Debit = SharedFunctions.GetTotalJournalDebit(db, journal.Id);
                        journal.Credit = SharedFunctions.GetTotalJournalCredit(db, journal.Id);

                        db.Entry(journal).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Journals, MenuId = journal.Id, MenuCode = journal.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", journal.MasterBusinessUnitId);
            return View("../Accounting/Journals/Create", journal);
        }

        // GET: Journals/Edit/5
        [Authorize(Roles = "JournalsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Journal journal = db.Journals.Find(id);
            if (journal == null || journal.Type != EnumJournalType.General)
            {
                return HttpNotFound();
            }

            if (journal.Active == false && journal.UserId != User.Identity.GetUserId<int>())
            {
                ViewBag.Resi = journal.Code + " sedang diinput oleh " + journal.User.UserName;
                return View("Index");
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", journal.MasterBusinessUnitId);
            return View("../Accounting/Journals/Edit", journal);
        }

        // POST: Journals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "JournalsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,Type,Debit,Credit,Notes,Active,Created,Updated,UserId")] Journal journal)
        {
            journal.Updated = DateTime.Now;
            journal.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                journal = GetModelState(journal);
            }

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(journal.Code)) journal.Code = journal.Code.ToUpper();
                        if (!string.IsNullOrEmpty(journal.Notes)) journal.Notes = journal.Notes.ToUpper();

                        journal.Debit = SharedFunctions.GetTotalJournalDebit(db, journal.Id);
                        journal.Credit = SharedFunctions.GetTotalJournalCredit(db, journal.Id);

                        db.Entry(journal).State = EntityState.Unchanged;
                        db.Entry(journal).Property("Code").IsModified = true;
                        db.Entry(journal).Property("Date").IsModified = true;
                        db.Entry(journal).Property("MasterBusinessUnitId").IsModified = true;
                        db.Entry(journal).Property("Notes").IsModified = true;
                        db.Entry(journal).Property("Debit").IsModified = true;
                        db.Entry(journal).Property("Credit").IsModified = true;
                        db.Entry(journal).Property("Active").IsModified = true;
                        db.Entry(journal).Property("Updated").IsModified = true;
                        db.Entry(journal).Property("UserId").IsModified = true;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Journals, MenuId = journal.Id, MenuCode = journal.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", journal.MasterBusinessUnitId);
            return View("../Accounting/Journals/Edit", journal);
        }

        // GET: MasterBusinessUnits/Edit/5
        [Authorize(Roles = "JournalsEdit")]
        public ActionResult DateEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Journal journal = db.Journals.Find(id);
            if (journal == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Accounting/Journals/_DateEdit", journal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "JournalsEdit")]
        public ActionResult DateEdit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,Type,Debit,Credit,Notes,Active,Created,Updated,UserId")] Journal journal)
        {
            journal.Updated = DateTime.Now;
            journal.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Entry(journal).State = EntityState.Unchanged;
                        db.Entry(journal).Property("Date").IsModified = true;
                        db.Entry(journal).Property("Updated").IsModified = true;
                        db.Entry(journal).Property("UserId").IsModified = true;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Journals, MenuId = journal.Id, MenuCode = journal.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Accounting/Journals/_DateEdit", journal);
        }

        [Authorize(Roles = "JournalsActive")]
        private Journal GetModelState(Journal journal)
        {
            List<JournalDetails> journalDetails = db.JournalsDetails.Where(x => x.JournalId == journal.Id).ToList();
            string masterBusinessName = db.MasterBusinessUnits.Find(journal.MasterBusinessUnitId).Name;

            if (ModelState.IsValid)
            {
                if (IsAnyCode(journal.Code, journal.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (journalDetails == null || journalDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            if (ModelState.IsValid)
            {
                if (SharedFunctions.GetTotalJournalDebit(db, journal.Id) != SharedFunctions.GetTotalJournalCredit(db, journal.Id))
                    ModelState.AddModelError(string.Empty, "Jurnal tidak ballance!");
            }

            if (ModelState.IsValid)
            {
                foreach (JournalDetails obj in journalDetails)
                {
                    if (obj.ChartOfAccount.MasterBusinessUnitId != journal.MasterBusinessUnitId)
                        ModelState.AddModelError(string.Empty, "Akun " + obj.ChartOfAccount.Code + " bukan milik " + masterBusinessName);
                }
            }

            return journal;
        }

        [Authorize(Roles = "JournalsPrint")]
        public ActionResult Print(int? id)
        {
            Journal obj = db.Journals.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }

            using (ReportDocument rd = new ReportDocument())
            {

                rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormJournal.rpt"));
                rd.SetParameterValue("Code", obj.Code);

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

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Journals, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.PRINT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return new CrystalReportPdfResult(rd, "JOURNAL_" + obj.Code + ".pdf");
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "JournalsActive")]
        public ActionResult Cancel(int? id)
        {
            if (id != null || id == 0)
            {
                Journal obj = db.Journals.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.JournalsDetails.RemoveRange(db.JournalsDetails.Where(x => x.JournalId == id));
                                db.SaveChanges();

                                db.Journals.Remove(obj);
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
        [Authorize(Roles = "JournalsDelete")]
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
                        Journal obj = db.Journals.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    Journal tmp = obj;

                                    db.JournalsDetails.RemoveRange(db.JournalsDetails.Where(x => x.JournalId == id));
                                    db.SaveChanges();

                                    db.Journals.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Journals, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "JournalsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Accounting/Journals/_DetailsGrid", db.JournalsDetails
                .Where(x => x.JournalId == Id).ToList());
        }

        [Authorize(Roles = "JournalsActive")]
        public ActionResult DetailsCreate(int JournalId)
        {
            Journal Journal = db.Journals.Find(JournalId);
            if (Journal == null)
            {
                return HttpNotFound();
            }
            JournalDetails journalDetails = new JournalDetails
            {
                JournalId = JournalId
            };
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes");
            return PartialView("../Accounting/Journals/_DetailsCreate", journalDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "JournalsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,JournalId,ChartOfAccountId,MasterRegionId,Debit,Credit,Notes,Created,Updated,UserId")] JournalDetails journalDetails)
        {
            journalDetails.Created = DateTime.Now;
            journalDetails.Updated = DateTime.Now;
            journalDetails.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(journalDetails.Notes)) journalDetails.Notes = journalDetails.Notes.ToUpper();

                        db.JournalsDetails.Add(journalDetails);
                        db.SaveChanges();

                        Journal journal = db.Journals.Find(journalDetails.JournalId);
                        journal.Debit = SharedFunctions.GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
                        journal.Credit = SharedFunctions.GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;

                        db.Entry(journal).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.JournalsDetails, MenuId = journalDetails.Id, MenuCode = journalDetails.ChartOfAccountId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", journalDetails.MasterRegionId);
            return PartialView("../Accounting/Journals/_DetailsCreate", journalDetails);
        }

        [Authorize(Roles = "JournalsActive")]
        public ActionResult DetailsEdit(int id)
        {
            JournalDetails journalDetails = db.JournalsDetails.Find(id);
            if (journalDetails == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", journalDetails.MasterRegionId);
            return PartialView("../Accounting/Journals/_DetailsEdit", journalDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "JournalsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,JournalId,ChartOfAccountId,MasterRegionId,Debit,Credit,Notes,Created,Updated,UserId")] JournalDetails journalDetails)
        {
            journalDetails.Updated = DateTime.Now;
            journalDetails.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(journalDetails.Notes)) journalDetails.Notes = journalDetails.Notes.ToUpper();

                        db.Entry(journalDetails).State = EntityState.Unchanged;
                        db.Entry(journalDetails).Property("ChartOfAccountId").IsModified = true;
                        db.Entry(journalDetails).Property("MasterRegionId").IsModified = true;
                        db.Entry(journalDetails).Property("Debit").IsModified = true;
                        db.Entry(journalDetails).Property("Credit").IsModified = true;
                        db.Entry(journalDetails).Property("Notes").IsModified = true;
                        db.Entry(journalDetails).Property("Updated").IsModified = true;
                        db.Entry(journalDetails).Property("UserId").IsModified = true;
                        db.SaveChanges();

                        Journal journal = db.Journals.Find(journalDetails.JournalId);
                        journal.Debit = SharedFunctions.GetTotalJournalDebit(db, journal.Id, journalDetails.Id) + journalDetails.Debit;
                        journal.Credit = SharedFunctions.GetTotalJournalCredit(db, journal.Id, journalDetails.Id) + journalDetails.Credit;

                        db.Entry(journal).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.JournalsDetails, MenuId = journalDetails.Id, MenuCode = journalDetails.ChartOfAccountId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", journalDetails.MasterRegionId);
            return PartialView("../Accounting/Journals/_DetailsEdit", journalDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "JournalsActive")]
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
                        JournalDetails obj = db.JournalsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    JournalDetails tmp = obj;

                                    Journal journal = db.Journals.Find(tmp.JournalId);
                                    journal.Debit = SharedFunctions.GetTotalJournalDebit(db, journal.Id, tmp.Id);
                                    journal.Credit = SharedFunctions.GetTotalJournalCredit(db, journal.Id, tmp.Id);

                                    db.Entry(journal).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.JournalsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.JournalsDetails, MenuId = tmp.Id, MenuCode = tmp.ChartOfAccountId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "JournalsActive")]
        public JsonResult GetCode(int? MasterBusinessUnitId)
        {
            string code = null;

            if (MasterBusinessUnitId != 0 && MasterBusinessUnitId != null)
            {
                MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(MasterBusinessUnitId);

                code = GetCode(masterBusinessUnit);
            }

            return Json(code);
        }

        [Authorize(Roles = "JournalsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit)
        {
            DateTime date = DateTime.Now;

            string romanMonth = SharedFunctions.RomanNumeralFrom((int)date.Month);
            string code = "/" + Settings.Default.JournalCode + masterBusinessUnit.Code + "/" + romanMonth + "/" + date.Year.ToString().Substring(2, 2);

            Journal lastData = db.Journals
                .Where(x => (x.Code.Contains(code)))
                .OrderByDescending(z => z.Code).FirstOrDefault();

            if (lastData == null)
                code = "0001" + code;
            else
                code = (Convert.ToInt32(lastData.Code.Substring(0, 4)) + 1).ToString("D4") + code;

            return code;
        }

        private class JournalDebtCredit
        {
            public string Debit { get; set; }
            public string Credit { get; set; }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "JournalsActive")]
        public JsonResult GetTotal(int JournalId)
        {
            return Json(new List<JournalDebtCredit>()
                {
                    new JournalDebtCredit {
                        Debit = SharedFunctions.GetTotalJournalDebit(db, JournalId).ToString("N2"),
                        Credit = SharedFunctions.GetTotalJournalCredit(db, JournalId).ToString("N2")
                    }
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
