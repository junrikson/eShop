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
    public class CashBankInsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Invoices
        [Authorize(Roles = "CashBankInsActive")]
        public ActionResult Index()
        {
            ViewBag.Resi = "";
            return View("../CashAndBank/CashBankIns/Index");
        }

        [HttpGet]
        [Authorize(Roles = "CashBankInsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            var masterRegions = user.MasterRegions.Select(x => x.Id).ToList();
            var masterBusinessUnits = user.MasterBusinessUnits.Select(x => x.Id).ToList();

            if (String.IsNullOrEmpty(search))
                return PartialView("../CashAndBank/CashBankIns/_IndexGrid", db.Set<CashBankTransaction>().Where(x => x.TransactionType == EnumCashBankTransactionType.In &&
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable());
            else
                return PartialView("../CashAndBank/CashBankIns/_IndexGrid", db.Set<CashBankTransaction>().Where(x => x.TransactionType == EnumCashBankTransactionType.In &&
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable()
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
                return db.CashBankTransactions.Any(x => x.Code == Code);
            }
            else
            {
                return db.CashBankTransactions.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: Invoices/Details/5
        [Authorize(Roles = "CashBankInsView")]
        public ActionResult Details(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(id);
            if (cashBankTransaction == null)
            {
                return HttpNotFound();
            }
            ViewBag.CashBankTransactionDetails = db.CashBankTransactionsDetails.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).ToList();
            ViewBag.CashBankTransactionDetailsHeader = db.CashBankTransactionsDetailsHeader.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).ToList();
            return PartialView("../CashAndBank/CashBankIns/_Details", cashBankTransaction);
        }

        // GET: Invoices/Create
        [Authorize(Roles = "CashBankInsAdd")]
        public ActionResult Create()
        {
            CashBankTransaction cashBankTransaction = new CashBankTransaction();
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    cashBankTransaction.Code = "temp/" + Guid.NewGuid().ToString();
                    cashBankTransaction.Date = DateTime.Now;
                    cashBankTransaction.MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id;
                    cashBankTransaction.MasterRegionId = db.MasterRegions.FirstOrDefault().Id;
                    cashBankTransaction.Total = 0;
                    cashBankTransaction.Active = false;
                    cashBankTransaction.IsPrint = false;
                    cashBankTransaction.TransactionType = EnumCashBankTransactionType.In;
                    cashBankTransaction.Created = DateTime.Now;
                    cashBankTransaction.Updated = DateTime.Now;
                    cashBankTransaction.UserId = User.Identity.GetUserId<int>();

                    db.CashBankTransactions.Add(cashBankTransaction);
                    db.SaveChanges();

                    Journal journal = SharedFunctions.CreateCashBankJournal(cashBankTransaction, db);
                    dbTran.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            cashBankTransaction.Active = true;
            cashBankTransaction.Code = "";
            cashBankTransaction.MasterBusinessUnitId = 0;
            cashBankTransaction.MasterRegionId = 0;

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name");
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes");
            ViewBag.Total = "0";
            ViewBag.TotalHeader = "0";
            return View("../CashAndBank/CashBankIns/Create", cashBankTransaction);
        }

        // POST: Invoices/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CashBankInsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,TransactionType,Notes,Total,Active")] CashBankTransaction cashBankTransaction)
        {
            cashBankTransaction.TransactionType = EnumCashBankTransactionType.In;

            if (ModelState.IsValid)
            {
                cashBankTransaction = GetModelState(cashBankTransaction);
            }

            if (ModelState.IsValid)
            {
                var cashBankTransactionsDetails = db.CashBankTransactionsDetails.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).ToList();
                var cashBankTransactionsDetailsHeader = db.CashBankTransactionsDetailsHeader.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).ToList();
                Journal journal = db.Journals.Where(x =>
                                        x.Type == EnumJournalType.CashBankTransaction &&
                                        x.CashBankTransactionId == cashBankTransaction.Id).FirstOrDefault();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(cashBankTransaction.Code)) cashBankTransaction.Code = cashBankTransaction.Code.ToUpper();
                        if (!string.IsNullOrEmpty(cashBankTransaction.Notes)) cashBankTransaction.Notes = cashBankTransaction.Notes.ToUpper();

                        cashBankTransaction.Total = SharedFunctions.GetTotalCashBankTransactionDetails(db, cashBankTransaction.Id);
                        cashBankTransaction.Created = DateTime.Now;
                        cashBankTransaction.Updated = DateTime.Now;
                        cashBankTransaction.UserId = User.Identity.GetUserId<int>();

                        db.Entry(cashBankTransaction).State = EntityState.Modified;
                        db.SaveChanges();

                        if (journal == null)
                        {
                            journal = SharedFunctions.CreateCashBankJournal(cashBankTransaction, db);
                            foreach (CashBankTransactionDetails cashBankTransactionDetails in cashBankTransactionsDetails)
                            {
                                SharedFunctions.CreateCashBankJournalDetails(cashBankTransactionDetails, journal, EnumCashBankTransactionType.In, db);
                            }

                            foreach (CashBankTransactionDetailsHeader cashBankTransactionDetailsHeader in cashBankTransactionsDetailsHeader)
                            {
                                SharedFunctions.CreateCashBankJournalDetailsHeader(cashBankTransactionDetailsHeader, journal, EnumCashBankTransactionType.In, db);
                            }
                        }
                        else
                            journal = SharedFunctions.UpdateCashBankJournal(journal, cashBankTransaction, EnumCashBankTransactionType.In, db);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankIns, MenuId = cashBankTransaction.Id, MenuCode = cashBankTransaction.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", cashBankTransaction.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", cashBankTransaction.MasterRegionId);
            ViewBag.Total = SharedFunctions.GetTotalCashBankTransactionDetails(db, cashBankTransaction.Id).ToString("N2");
            ViewBag.TotalHeader = SharedFunctions.GetTotalCashBankTransactionDetailsHeader(db, cashBankTransaction.Id).ToString("N2");
            return View("../CashAndBank/CashBankIns/Create", cashBankTransaction);
        }

        // GET: Invoices/Edit/5
        [Authorize(Roles = "CashBankInsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(id);
            if (cashBankTransaction == null)
            {
                return HttpNotFound();
            }

            if (cashBankTransaction.IsPrint)
            {
                VerificationHistory verificationHistory = new VerificationHistory
                {
                    CashBankTransactionId = cashBankTransaction.Id,
                    CashBankTransaction = cashBankTransaction,
                    Type = EnumVerificationType.EditCashBankTransaction
                };

                return View("../CashAndBank/CashBankIns/EditVerification", verificationHistory);
            }
            else
            {
                ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
                bool isHeaderExist = db.CashBankTransactionsDetailsHeader.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).Count() > 0 ? true : false;

                if (!isHeaderExist)
                {
                    var banks = db.CashBankTransactionsDetails.Where(x => x.CashBankTransactionId == cashBankTransaction.Id)
                        .Select(y => y.MasterCashBank).Distinct().ToList();

                    using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (MasterCashBank masterCashBank in banks)
                            {
                                CashBankTransactionDetailsHeader obj = new CashBankTransactionDetailsHeader
                                {
                                    CashBankTransactionId = cashBankTransaction.Id,
                                    Type = masterCashBank.Type,
                                    MasterCashBankId = masterCashBank.Id,
                                    Total = db.CashBankTransactionsDetails.Where(x => x.CashBankTransactionId == cashBankTransaction.Id && x.MasterCashBankId == masterCashBank.Id)
                                            .Sum(y => y.Total),
                                    Notes = masterCashBank.Name,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = user.Id
                                };

                                db.CashBankTransactionsDetailsHeader.Add(obj);
                                db.SaveChanges();
                            }

                            Journal journal = db.Journals.Where(y => y.Type == EnumJournalType.CashBankTransaction && y.CashBankTransactionId == cashBankTransaction.Id).FirstOrDefault();

                            if (journal != null)
                            {
                                var journalsDetails = db.JournalsDetails.Where(x => x.JournalId == journal.Id).ToList();

                                if (journalsDetails != null && journalsDetails.Count > 0)
                                {
                                    db.JournalsDetails.RemoveRange(journalsDetails);
                                    db.SaveChanges();
                                }

                                db.Journals.Remove(journal);
                                db.SaveChanges();
                            }

                            var cashBankTransactionsDetails = db.CashBankTransactionsDetails.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).ToList();
                            var cashBankTransactionsDetailsHeader = db.CashBankTransactionsDetailsHeader.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).ToList();

                            journal = SharedFunctions.CreateCashBankJournal(cashBankTransaction, db);
                            foreach (CashBankTransactionDetails cashBankTransactionDetails in cashBankTransactionsDetails)
                            {
                                SharedFunctions.CreateCashBankJournalDetails(cashBankTransactionDetails, journal, EnumCashBankTransactionType.In, db);
                            }

                            foreach (CashBankTransactionDetailsHeader cashBankTransactionDetailsHeader in cashBankTransactionsDetailsHeader)
                            {
                                SharedFunctions.CreateCashBankJournalDetailsHeader(cashBankTransactionDetailsHeader, journal, EnumCashBankTransactionType.In, db);
                            }

                            dbTran.Commit();
                        }
                        catch (DbEntityValidationException ex)
                        {
                            dbTran.Rollback();
                            throw ex;
                        }
                    }
                }

                ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", cashBankTransaction.MasterBusinessUnitId);
                ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", cashBankTransaction.MasterRegionId);
                ViewBag.Total = SharedFunctions.GetTotalCashBankTransactionDetails(db, cashBankTransaction.Id).ToString("N2");
                ViewBag.TotalHeader = SharedFunctions.GetTotalCashBankTransactionDetailsHeader(db, cashBankTransaction.Id).ToString("N2");
                return View("../CashAndBank/CashBankIns/Edit", cashBankTransaction);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CashBankInsPrint")]
        public ActionResult Verify([Bind(Include = "CashBankTransactionId,Verification,Type,Reason")] VerificationHistory verificationHistory)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CashBankInsEdit")]
        public ActionResult EditVerification([Bind(Include = "CashBankTransactionId,Type,Verification,Reason")] VerificationHistory verificationHistory)
        {
            verificationHistory.Type = EnumVerificationType.EditCashBankTransaction;
            verificationHistory.Created = DateTime.Now;
            verificationHistory.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(verificationHistory.CashBankTransactionId);

                if (SharedFunctions.CreateMD5(verificationHistory.Verification) == cashBankTransaction.Verification)
                {
                    using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            db.Entry(cashBankTransaction).State = EntityState.Modified;
                            cashBankTransaction.IsPrint = false;
                            cashBankTransaction.Verification = "-";
                            db.SaveChanges();

                            if (!string.IsNullOrEmpty(verificationHistory.Verification)) verificationHistory.Verification = verificationHistory.Verification.ToUpper();
                            if (!string.IsNullOrEmpty(verificationHistory.Reason)) verificationHistory.Reason = verificationHistory.Reason.ToUpper();

                            db.VerificationHistories.Add(verificationHistory);
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

                return RedirectToAction("Edit", new { id = verificationHistory.CashBankTransactionId });
            }

            return View("../CashAndBank/CashBankIns/EditVerification", verificationHistory);
        }

        // POST: Invoices/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CashBankInsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,TransactionType,Notes,Total,Active")] CashBankTransaction cashBankTransaction)
        {
            cashBankTransaction.Updated = DateTime.Now;
            cashBankTransaction.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                cashBankTransaction = GetModelState(cashBankTransaction);
            }

            if (ModelState.IsValid)
            {
                cashBankTransaction.Total = SharedFunctions.GetTotalCashBankTransactionDetails(db, cashBankTransaction.Id);
                var cashBankTransactionsDetails = db.CashBankTransactionsDetails.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).ToList();
                var cashBankTransactionsDetailsHeader = db.CashBankTransactionsDetailsHeader.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).ToList();
                Journal journal = db.Journals.Where(x =>
                                        x.Type == EnumJournalType.CashBankTransaction &&
                                        x.CashBankTransactionId == cashBankTransaction.Id).FirstOrDefault();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(cashBankTransaction.Code)) cashBankTransaction.Code = cashBankTransaction.Code.ToUpper();
                        if (!string.IsNullOrEmpty(cashBankTransaction.Notes)) cashBankTransaction.Notes = cashBankTransaction.Notes.ToUpper();

                        db.Entry(cashBankTransaction).State = EntityState.Unchanged;
                        db.Entry(cashBankTransaction).Property("Code").IsModified = true;
                        db.Entry(cashBankTransaction).Property("Date").IsModified = true;
                        db.Entry(cashBankTransaction).Property("MasterBusinessUnitId").IsModified = true;
                        db.Entry(cashBankTransaction).Property("MasterRegionId").IsModified = true;
                        db.Entry(cashBankTransaction).Property("Notes").IsModified = true;
                        db.Entry(cashBankTransaction).Property("Total").IsModified = true;
                        db.Entry(cashBankTransaction).Property("Active").IsModified = true;
                        db.Entry(cashBankTransaction).Property("Updated").IsModified = true;
                        db.Entry(cashBankTransaction).Property("UserId").IsModified = true;
                        db.SaveChanges();

                        if (journal == null)
                        {
                            journal = SharedFunctions.CreateCashBankJournal(cashBankTransaction, db);
                            foreach (CashBankTransactionDetails cashBankTransactionDetails in cashBankTransactionsDetails)
                            {
                                SharedFunctions.CreateCashBankJournalDetails(cashBankTransactionDetails, journal, EnumCashBankTransactionType.In, db);
                            }

                            foreach (CashBankTransactionDetailsHeader cashBankTransactionDetailsHeader in cashBankTransactionsDetailsHeader)
                            {
                                SharedFunctions.CreateCashBankJournalDetailsHeader(cashBankTransactionDetailsHeader, journal, EnumCashBankTransactionType.In, db);
                            }
                        }
                        else
                            journal = SharedFunctions.UpdateCashBankJournal(journal, cashBankTransaction, EnumCashBankTransactionType.In, db);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankIns, MenuId = cashBankTransaction.Id, MenuCode = cashBankTransaction.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", cashBankTransaction.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", cashBankTransaction.MasterRegionId);
            ViewBag.Total = SharedFunctions.GetTotalCashBankTransactionDetails(db, cashBankTransaction.Id).ToString("N2");
            ViewBag.TotalHeader = SharedFunctions.GetTotalCashBankTransactionDetailsHeader(db, cashBankTransaction.Id).ToString("N2");
            return View("../CashAndBank/CashBankIns/Edit", cashBankTransaction);
        }

        [Authorize(Roles = "CashBankInsPrint")]
        public ActionResult Print(int? id)
        {
            CashBankTransaction obj = db.CashBankTransactions.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }

            if (obj.IsPrint)
            {
                VerificationHistory verificationHistory = new VerificationHistory
                {
                    CashBankTransactionId = obj.Id,
                    CashBankTransaction = obj,
                    Type = EnumVerificationType.PrintCashBankTransaction
                };

                return View("../CashAndBank/CashBankIns/Print", verificationHistory);
            }
            else
            {
                using (ReportDocument rd = new ReportDocument())
                {
                    var cashBankTransactionsDetailsHeader = db.CashBankTransactionsDetailsHeader.Where(x => x.CashBankTransactionId == obj.Id).ToList();

                    rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormCashCashBankTransaction.rpt"));
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

                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankIns, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.PRINT, UserId = User.Identity.GetUserId<int>() });
                    db.SaveChanges();

                    return new CrystalReportPdfResult(rd, "BIN_" + obj.Code + ".pdf");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CashBankInsPrint")]
        public ActionResult Print([Bind(Include = "CashBankTransactionId,Type,Verification,Reason")] VerificationHistory verificationHistory)
        {
            verificationHistory.Type = EnumVerificationType.PrintCashBankTransaction;
            verificationHistory.Created = DateTime.Now;
            verificationHistory.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(verificationHistory.CashBankTransactionId);

                if (SharedFunctions.CreateMD5(verificationHistory.Verification) == cashBankTransaction.Verification)
                {
                    using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            db.Entry(cashBankTransaction).State = EntityState.Modified;
                            cashBankTransaction.IsPrint = false;
                            cashBankTransaction.Verification = "-";
                            db.SaveChanges();

                            if (!string.IsNullOrEmpty(verificationHistory.Verification)) verificationHistory.Verification = verificationHistory.Verification.ToUpper();
                            if (!string.IsNullOrEmpty(verificationHistory.Reason)) verificationHistory.Reason = verificationHistory.Reason.ToUpper();

                            db.VerificationHistories.Add(verificationHistory);
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

                return RedirectToAction("Print", new { id = verificationHistory.CashBankTransactionId });
            }

            return View("../CashAndBank/CashBankIns/Print", verificationHistory);
        }

        public JsonResult IsVerificationValid(string Verification, int? CashBankTransactionId)
        {
            string verification = SharedFunctions.CreateMD5(Verification);

            if (CashBankTransactionId != null || CashBankTransactionId != 0)
            {
                return Json(db.CashBankTransactions.Any(x => x.Id == (int)CashBankTransactionId && x.Verification == verification), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false);
            }
        }


        [Authorize(Roles = "BankVerificationActive")]
        public ActionResult GetVerificationCode(int? id)
        {
            string verificationCode = new Random().Next(1000, 9999).ToString();

            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(id);
            if (cashBankTransaction == null)
            {
                return HttpNotFound();
            }
            else
            {
                cashBankTransaction.Verification = SharedFunctions.CreateMD5(verificationCode);
                db.Entry(cashBankTransaction).State = EntityState.Modified;
                db.SaveChanges();
            }

            ViewBag.VerificationCode = verificationCode;
            return PartialView("../CashAndBank/CashBankIns/_VerificationCode");
        }

        [Authorize(Roles = "BankOutsActive")]
        private CashBankTransaction GetModelState(CashBankTransaction cashBankTransaction)
        {
            List<CashBankTransactionDetails> cashBankTransactionDetails = db.CashBankTransactionsDetails.Where(x => x.CashBankTransactionId == cashBankTransaction.Id).ToList();
            string masterBusinessName = db.MasterBusinessUnits.Find(cashBankTransaction.MasterBusinessUnitId).Name;
            string masterRegionCode = db.MasterRegions.Find(cashBankTransaction.MasterRegionId).Code;

            if (ModelState.IsValid)
            {
                if (IsAnyCode(cashBankTransaction.Code, cashBankTransaction.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (cashBankTransactionDetails == null || cashBankTransactionDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            if (ModelState.IsValid)
            {
                if (SharedFunctions.GetTotalCashBankTransactionDetailsHeader(db, cashBankTransaction.Id) != SharedFunctions.GetTotalCashBankTransactionDetails(db, cashBankTransaction.Id))
                    ModelState.AddModelError(string.Empty, "Jumlah debet dan kredit tidak sama!");
            }

            if (ModelState.IsValid)
            {
                foreach (CashBankTransactionDetails obj in cashBankTransactionDetails)
                {
                    if (obj.ChartOfAccount.MasterBusinessUnitId != cashBankTransaction.MasterBusinessUnitId)
                        ModelState.AddModelError(string.Empty, "Akun " + obj.ChartOfAccount.Code + " bukan milik " + masterBusinessName);
                }
            }

            return cashBankTransaction;
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "CashBankInsActive")]
        public ActionResult Cancel(int? id)
        {
            if (id != null || id == 0)
            {
                CashBankTransaction obj = db.CashBankTransactions.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                Journal journal = db.Journals.Where(y => y.Type == EnumJournalType.CashBankTransaction && y.CashBankTransactionId == obj.Id).FirstOrDefault();

                                SharedFunctions.DeleteCashBankJournals((int)id, journal, obj, db);

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
        [Authorize(Roles = "CashBankInsDelete")]
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
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                CashBankTransaction obj = db.CashBankTransactions.Find(id);
                                if (obj == null)
                                    failed++;
                                else
                                {
                                    CashBankTransaction tmp = obj;
                                    var journal = db.Journals.Where(y => y.Type == EnumJournalType.CashBankTransaction && y.CashBankTransactionId == obj.Id).FirstOrDefault();

                                    SharedFunctions.DeleteCashBankJournals(id, journal, obj, db);

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankIns, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                                    db.SaveChanges();

                                    dbTran.Commit();
                                }
                            }
                            catch (DbEntityValidationException ex)
                            {
                                dbTran.Rollback();
                                throw ex;
                            }
                        }
                    }
                    return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                }
            }
        }

        [HttpGet]
        [Authorize(Roles = "CashBankInsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../CashAndBank/CashBankIns/_DetailsGrid", db.CashBankTransactionsDetails
                .Where(x => x.CashBankTransactionId == Id).ToList());
        }

        [Authorize(Roles = "CashBankInsActive")]
        public ActionResult DetailsCreate(int? cashBankTransactionId)
        {
            if (cashBankTransactionId == null || cashBankTransactionId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(cashBankTransactionId);
            if (cashBankTransaction == null)
            {
                return HttpNotFound();
            }
            CashBankTransactionDetails cashBankTransactioneDetails = new CashBankTransactionDetails
            {
                CashBankTransactionId = (int)cashBankTransactionId
            };

            return PartialView("../CashAndBank/CashBankIns/_DetailsCreate", cashBankTransactioneDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CashBankInsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,CashBankTransactionId,Type,MasterCashBankId,ChartOfAccountId,Total,Notes,Created,Updated,UserId")] CashBankTransactionDetails cashBankTransactionDetails)
        {
            cashBankTransactionDetails.Created = DateTime.Now;
            cashBankTransactionDetails.Updated = DateTime.Now;
            cashBankTransactionDetails.DueDate = DateTime.Now;
            cashBankTransactionDetails.UserId = User.Identity.GetUserId<int>();
            cashBankTransactionDetails.MasterCashBankId = db.MasterCashBanks.Where(x => x.Active == true).FirstOrDefault().Id;

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(cashBankTransactionDetails.Notes)) cashBankTransactionDetails.Notes = cashBankTransactionDetails.Notes.ToUpper();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.CashBankTransactionsDetails.Add(cashBankTransactionDetails);
                        db.SaveChanges();

                        Journal journal = db.Journals.Where(x =>
                                                x.Type == EnumJournalType.CashBankTransaction &&
                                                x.CashBankTransactionId == cashBankTransactionDetails.CashBankTransactionId).FirstOrDefault();

                        if (journal != null)
                            SharedFunctions.CreateCashBankJournalDetails(cashBankTransactionDetails, journal, EnumCashBankTransactionType.In, db);

                        CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(cashBankTransactionDetails.CashBankTransactionId);
                        cashBankTransaction.Total = SharedFunctions.GetTotalCashBankTransactionDetails(db, cashBankTransaction.Id) + cashBankTransactionDetails.Total;

                        db.Entry(cashBankTransaction).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankInsDetails, MenuId = cashBankTransactionDetails.Id, MenuCode = cashBankTransactionDetails.ChartOfAccountId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../CashAndBank/CashBankIns/_DetailsCreate", cashBankTransactionDetails);
        }

        [Authorize(Roles = "CashBankInsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CashBankTransactionDetails obj = db.CashBankTransactionsDetails.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../CashAndBank/CashBankIns/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CashBankInsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,CashBankTransactionId,Type,MasterCashBankId,ChartOfAccountId,Total,Notes,Created,Updated,UserId")] CashBankTransactionDetails cashBankTransactionDetails)
        {
            cashBankTransactionDetails.Updated = DateTime.Now;
            cashBankTransactionDetails.DueDate = DateTime.Now;
            cashBankTransactionDetails.UserId = User.Identity.GetUserId<int>();
            cashBankTransactionDetails.MasterCashBankId = db.MasterCashBanks.Where(x => x.Active == true).FirstOrDefault().Id;

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(cashBankTransactionDetails.Notes)) cashBankTransactionDetails.Notes = cashBankTransactionDetails.Notes.ToUpper();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        SharedFunctions.UpdateCashBankJournalDetails(cashBankTransactionDetails, EnumCashBankTransactionType.In, db);

                        CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(cashBankTransactionDetails.CashBankTransactionId);
                        cashBankTransaction.Total = SharedFunctions.GetTotalCashBankTransactionDetails(db, cashBankTransaction.Id, cashBankTransactionDetails.Id) + cashBankTransactionDetails.Total;

                        db.Entry(cashBankTransaction).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(cashBankTransactionDetails).State = EntityState.Unchanged;
                        db.Entry(cashBankTransactionDetails).Property("Type").IsModified = true;
                        db.Entry(cashBankTransactionDetails).Property("MasterCashBankId").IsModified = true;
                        db.Entry(cashBankTransactionDetails).Property("ChartOfAccountId").IsModified = true;
                        db.Entry(cashBankTransactionDetails).Property("Total").IsModified = true;
                        db.Entry(cashBankTransactionDetails).Property("Notes").IsModified = true;
                        db.Entry(cashBankTransactionDetails).Property("Updated").IsModified = true;
                        db.Entry(cashBankTransactionDetails).Property("UserId").IsModified = true;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankInsDetails, MenuId = cashBankTransactionDetails.Id, MenuCode = cashBankTransactionDetails.ChartOfAccountId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../CashAndBank/CashBankIns/_DetailsEdit", cashBankTransactionDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "CashBankInsActive")]
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
                        CashBankTransactionDetails obj = db.CashBankTransactionsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    CashBankTransactionDetails tmp = obj;

                                    Journal journal = db.Journals.Where(x =>
                                                            x.Type == EnumJournalType.CashBankTransaction &&
                                                            x.CashBankTransactionId == obj.CashBankTransactionId).FirstOrDefault();

                                    if (journal != null)
                                        SharedFunctions.RemoveCashBankJournalDetails(obj, journal, db);

                                    CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(tmp.CashBankTransactionId);
                                    cashBankTransaction.Total = SharedFunctions.GetTotalCashBankTransactionDetails(db, cashBankTransaction.Id, tmp.Id);

                                    db.Entry(cashBankTransaction).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.CashBankTransactionsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankInsDetails, MenuId = tmp.Id, MenuCode = tmp.ChartOfAccountId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "CashBankInsActive")]
        public PartialViewResult DetailsHeaderGrid(int Id)
        {
            return PartialView("../CashAndBank/CashBankIns/_DetailsHeaderGrid", db.CashBankTransactionsDetailsHeader
                .Where(x => x.CashBankTransactionId == Id).ToList());
        }

        [Authorize(Roles = "CashBankInsActive")]
        public ActionResult DetailsHeaderCreate(int? cashBankTransactionId)
        {
            if (cashBankTransactionId == null || cashBankTransactionId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(cashBankTransactionId);
            if (cashBankTransaction == null)
            {
                return HttpNotFound();
            }
            CashBankTransactionDetailsHeader cashBankTransactioneDetailsHeader = new CashBankTransactionDetailsHeader
            {
                CashBankTransactionId = (int)cashBankTransactionId
            };

            return PartialView("../CashAndBank/CashBankIns/_DetailsHeaderCreate", cashBankTransactioneDetailsHeader);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CashBankInsActive")]
        public ActionResult DetailsHeaderCreate([Bind(Include = "Id,CashBankTransactionId,Type,GiroChequeId,MasterCashBankId,Total,Notes,Created,Updated,UserId")] CashBankTransactionDetailsHeader cashBankTransactionDetailsHeader)
        {
            cashBankTransactionDetailsHeader.Created = DateTime.Now;
            cashBankTransactionDetailsHeader.Updated = DateTime.Now;
            cashBankTransactionDetailsHeader.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(cashBankTransactionDetailsHeader.Notes)) cashBankTransactionDetailsHeader.Notes = cashBankTransactionDetailsHeader.Notes.ToUpper();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.CashBankTransactionsDetailsHeader.Add(cashBankTransactionDetailsHeader);
                        db.SaveChanges();

                        Journal journal = db.Journals.Where(x =>
                                                x.Type == EnumJournalType.CashBankTransaction &&
                                                x.CashBankTransactionId == cashBankTransactionDetailsHeader.CashBankTransactionId).FirstOrDefault();

                        if (journal != null)
                            SharedFunctions.CreateCashBankJournalDetailsHeader(cashBankTransactionDetailsHeader, journal, EnumCashBankTransactionType.In, db);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankInsDetails, MenuId = cashBankTransactionDetailsHeader.Id, MenuCode = cashBankTransactionDetailsHeader.MasterCashBankId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../CashAndBank/CashBankIns/_DetailsCreate", cashBankTransactionDetailsHeader);
        }

        [Authorize(Roles = "CashBankInsActive")]
        public ActionResult DetailsHeaderEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CashBankTransactionDetailsHeader obj = db.CashBankTransactionsDetailsHeader.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            return PartialView("../CashAndBank/CashBankIns/_DetailsHeaderEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CashBankInsActive")]
        public ActionResult DetailsHeaderEdit([Bind(Include = "Id,CashBankTransactionId,Type,MasterCashBankId,GiroChequeId,Total,Notes,Created,Updated,UserId")] CashBankTransactionDetailsHeader cashBankTransactionDetailsHeader)
        {
            cashBankTransactionDetailsHeader.Updated = DateTime.Now;
            cashBankTransactionDetailsHeader.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    if (!string.IsNullOrEmpty(cashBankTransactionDetailsHeader.Notes)) cashBankTransactionDetailsHeader.Notes = cashBankTransactionDetailsHeader.Notes.ToUpper();

                    try
                    {
                        SharedFunctions.UpdateCashBankJournalDetailsHeader(cashBankTransactionDetailsHeader, EnumCashBankTransactionType.In, db);

                        db.Entry(cashBankTransactionDetailsHeader).State = EntityState.Unchanged;
                        db.Entry(cashBankTransactionDetailsHeader).Property("Type").IsModified = true;
                        db.Entry(cashBankTransactionDetailsHeader).Property("MasterCashBankId").IsModified = true;
                        db.Entry(cashBankTransactionDetailsHeader).Property("GiroChequeId").IsModified = true;
                        db.Entry(cashBankTransactionDetailsHeader).Property("Total").IsModified = true;
                        db.Entry(cashBankTransactionDetailsHeader).Property("Notes").IsModified = true;
                        db.Entry(cashBankTransactionDetailsHeader).Property("Updated").IsModified = true;
                        db.Entry(cashBankTransactionDetailsHeader).Property("UserId").IsModified = true;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankInsDetails, MenuId = cashBankTransactionDetailsHeader.Id, MenuCode = cashBankTransactionDetailsHeader.MasterCashBankId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../CashAndBank/CashBankIns/_DetailsEdit", cashBankTransactionDetailsHeader);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "CashBankInsActive")]
        public ActionResult DetailsHeaderBatchDelete(int[] ids)
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
                        CashBankTransactionDetailsHeader obj = db.CashBankTransactionsDetailsHeader.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    CashBankTransactionDetailsHeader tmp = obj;

                                    Journal journal = db.Journals.Where(x =>
                                                            x.Type == EnumJournalType.CashBankTransaction &&
                                                            x.CashBankTransactionId == obj.CashBankTransactionId).FirstOrDefault();

                                    if (journal != null)
                                        SharedFunctions.RemoveCashBankJournalDetailsHeader(obj, journal, db);

                                    db.CashBankTransactionsDetailsHeader.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankInsDetails, MenuId = tmp.Id, MenuCode = tmp.CashBankTransactionId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "CashBankInsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);
            CashBankTransaction cashBankTransaction = db.CashBankTransactions.Find(id);

            if (masterBusinessUnit != null && masterRegion != null && cashBankTransaction != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);

                cashBankTransaction.MasterBusinessUnitId = masterBusinessUnitId;
                cashBankTransaction.MasterRegionId = masterRegionId;
                db.Entry(cashBankTransaction).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "CashBankInsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            DateTime date = DateTime.Now;

            string romanMonth = SharedFunctions.RomanNumeralFrom((int)date.Month);
            string code = "/" + Settings.Default.BankInCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + romanMonth + "/" + date.Year.ToString().Substring(2, 2);

            CashBankTransaction lastData = db.CashBankTransactions
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
        [Authorize(Roles = "CashBankInsActive")]
        public JsonResult GetTotal(int CashBankTransactionId)
        {
            return Json(SharedFunctions.GetTotalCashBankTransactionDetails(db, CashBankTransactionId).ToString("N2"));
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "CashBankInsActive")]
        public JsonResult GetTotalHeader(int CashBankTransactionId)
        {
            return Json(SharedFunctions.GetTotalCashBankTransactionDetailsHeader(db, CashBankTransactionId).ToString("N2"));
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
