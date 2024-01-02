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
    public class BankOutsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Invoices
        [Authorize(Roles = "BankOutsActive")]
        public ActionResult Index()
        {
            ViewBag.Resi = "";
            return View("../Bank/BankOuts/Index");
        }

        [HttpGet]
        [Authorize(Roles = "BankOutsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            var masterRegions = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterRegionId).Distinct().ToList();
            var masterBusinessUnits = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnitId).Distinct().ToList();

            if (String.IsNullOrEmpty(search))
                return PartialView("../Bank/BankOuts/_IndexGrid", db.Set<BankTransaction>().Where(x => x.TransactionType == EnumBankTransactionType.Out &&
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable());
            else
                return PartialView("../Bank/BankOuts/_IndexGrid", db.Set<BankTransaction>().Where(x => x.TransactionType == EnumBankTransactionType.Out &&
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
                return db.BankTransactions.Any(x => x.Code == Code);
            }
            else
            {
                return db.BankTransactions.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        // GET: Invoices/Details/5
        [Authorize(Roles = "BankOutsView")]
        public ActionResult Details(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankTransaction bankTransaction = db.BankTransactions.Find(id);
            if (bankTransaction == null)
            {
                return HttpNotFound();
            }
            ViewBag.BankTransactionDetails = db.BankTransactionsDetails.Where(x => x.BankTransactionId == bankTransaction.Id).ToList();
            ViewBag.BankTransactionDetailsHeader = db.BankTransactionsDetailsHeader.Where(x => x.BankTransactionId == bankTransaction.Id).ToList();
            return PartialView("../Bank/BankOuts/_Details", bankTransaction);
        }

        [HttpGet]
        [Authorize(Roles = "BankOutsView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Bank/BankOuts/_ViewGrid", db.BankTransactionsDetails
                .Where(x => x.BankTransactionId == Id).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "BankOutsView")]
        public PartialViewResult OtherGrid(int Id)
        {
            return PartialView("../Bank/BankOuts/_ViewHeaderGrid", db.BankTransactionsDetailsHeader
                .Where(x => x.BankTransactionId == Id).ToList());
        }

        // GET: Invoices/Create
        [Authorize(Roles = "BankOutsAdd")]
        public ActionResult Create()
        {
            BankTransaction bankTransaction = new BankTransaction();
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    bankTransaction.Code = "temp/" + Guid.NewGuid().ToString();
                    bankTransaction.Date = DateTime.Now;
                    bankTransaction.MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id;
                    bankTransaction.MasterRegionId = db.MasterRegions.FirstOrDefault().Id;
                    bankTransaction.Total = 0;
                    bankTransaction.Active = false;
                    bankTransaction.IsPrint = false;
                    bankTransaction.TransactionType = EnumBankTransactionType.Out;
                    bankTransaction.Created = DateTime.Now;
                    bankTransaction.Updated = DateTime.Now;
                    bankTransaction.UserId = User.Identity.GetUserId<int>();

                    db.BankTransactions.Add(bankTransaction);
                    db.SaveChanges();

                    SharedFunctions.CreateBankJournal(db, bankTransaction);
                    dbTran.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            bankTransaction.Active = true;
            bankTransaction.Code = "";
            bankTransaction.MasterBusinessUnitId = 0;
            bankTransaction.MasterRegionId = 0;

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            ViewBag.Total = "0";
            ViewBag.TotalHeader = "0";
            return View("../Bank/BankOuts/Create", bankTransaction);
        }

        // POST: Invoices/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BankOutsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,TransactionType,Notes,Total,Active")] BankTransaction bankTransaction)
        {
            bankTransaction.TransactionType = EnumBankTransactionType.Out;

            if (ModelState.IsValid)
            {
                bankTransaction = GetModelState(bankTransaction);
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(bankTransaction.Code)) bankTransaction.Code = bankTransaction.Code.ToUpper();
                if (!string.IsNullOrEmpty(bankTransaction.Notes)) bankTransaction.Notes = bankTransaction.Notes.ToUpper();

                bankTransaction.Total = SharedFunctions.GetTotalBankTransactionDetails(db, bankTransaction.Id);
                bankTransaction.Created = DateTime.Now;
                bankTransaction.Updated = DateTime.Now;
                bankTransaction.UserId = User.Identity.GetUserId<int>();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Entry(bankTransaction).State = EntityState.Modified;
                        db.SaveChanges();

                        SharedFunctions.UpdateBankJournal(db, bankTransaction);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankOuts, MenuId = bankTransaction.Id, MenuCode = bankTransaction.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", bankTransaction.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalBankTransactionDetails(db, bankTransaction.Id).ToString("N2");
            ViewBag.TotalHeader = SharedFunctions.GetTotalBankTransactionDetailsHeader(db, bankTransaction.Id).ToString("N2");
            return View("../Bank/BankOuts/Create", bankTransaction);
        }

        // GET: Invoices/Edit/5
        [Authorize(Roles = "BankOutsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankTransaction bankTransaction = db.BankTransactions.Find(id);
            if (bankTransaction == null)
            {
                return HttpNotFound();
            }

            if (bankTransaction.IsPrint)
            {
                VerificationHistory verificationHistory = new VerificationHistory
                {
                    BankTransactionId = bankTransaction.Id,
                    BankTransaction = bankTransaction,
                    Type = EnumVerificationType.EditBankTransaction
                };

                return View("../Bank/BankOuts/EditVerification", verificationHistory);
            }
            else
            {
                ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
                
                ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", bankTransaction.MasterBusinessUnitId);
                ViewBag.Total = SharedFunctions.GetTotalBankTransactionDetails(db, bankTransaction.Id).ToString("N2");
                ViewBag.TotalHeader = SharedFunctions.GetTotalBankTransactionDetailsHeader(db, bankTransaction.Id).ToString("N2");
                return View("../Bank/BankOuts/Edit", bankTransaction);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BankOutsPrint")]
        public ActionResult Verify([Bind(Include = "BankTransactionId,Verification,Type,Reason")] VerificationHistory verificationHistory)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BankOutsEdit")]
        public ActionResult EditVerification([Bind(Include = "BankTransactionId,Type,Verification,Reason")] VerificationHistory verificationHistory)
        {
            verificationHistory.Type = EnumVerificationType.EditBankTransaction;
            verificationHistory.Created = DateTime.Now;
            verificationHistory.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                BankTransaction bankTransaction = db.BankTransactions.Find(verificationHistory.BankTransactionId);

                if (SharedFunctions.CreateMD5(verificationHistory.Verification) == bankTransaction.Verification)
                {
                    using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            db.Entry(bankTransaction).State = EntityState.Modified;
                            bankTransaction.IsPrint = false;
                            bankTransaction.Verification = "-";
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

                return RedirectToAction("Edit", new { id = verificationHistory.BankTransactionId });
            }

            return View("../Bank/BankOuts/EditVerification", verificationHistory);
        }

        // POST: Invoices/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BankOutsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,TransactionType,Notes,Total,Active")] BankTransaction bankTransaction)
        {
            bankTransaction.Updated = DateTime.Now;
            bankTransaction.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                bankTransaction = GetModelState(bankTransaction);
            }

            if (ModelState.IsValid)
            {
                bankTransaction.Total = SharedFunctions.GetTotalBankTransactionDetails(db, bankTransaction.Id);

                if (!string.IsNullOrEmpty(bankTransaction.Code)) bankTransaction.Code = bankTransaction.Code.ToUpper();
                if (!string.IsNullOrEmpty(bankTransaction.Notes)) bankTransaction.Notes = bankTransaction.Notes.ToUpper();

                db.Entry(bankTransaction).State = EntityState.Unchanged;
                db.Entry(bankTransaction).Property("Code").IsModified = true;
                db.Entry(bankTransaction).Property("Date").IsModified = true;
                db.Entry(bankTransaction).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(bankTransaction).Property("MasterRegionId").IsModified = true;
                db.Entry(bankTransaction).Property("Notes").IsModified = true;
                db.Entry(bankTransaction).Property("Total").IsModified = true;
                db.Entry(bankTransaction).Property("Active").IsModified = true;
                db.Entry(bankTransaction).Property("Updated").IsModified = true;
                db.Entry(bankTransaction).Property("UserId").IsModified = true;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.Entry(bankTransaction).Reload();

                        SharedFunctions.UpdateBankJournal(db, bankTransaction);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankOuts, MenuId = bankTransaction.Id, MenuCode = bankTransaction.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", bankTransaction.MasterBusinessUnitId);
            ViewBag.Total = SharedFunctions.GetTotalBankTransactionDetails(db, bankTransaction.Id).ToString("N2");
            ViewBag.TotalHeader = SharedFunctions.GetTotalBankTransactionDetailsHeader(db, bankTransaction.Id).ToString("N2");
            return View("../Bank/BankOuts/Edit", bankTransaction);
        }

        [Authorize(Roles = "BankOutsPrint")]
        public ActionResult Print(int? id)
        {
            BankTransaction obj = db.BankTransactions.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }

            if (obj.IsPrint)
            {
                VerificationHistory verificationHistory = new VerificationHistory
                {
                    BankTransactionId = obj.Id,
                    BankTransaction = obj,
                    Type = EnumVerificationType.PrintBankTransaction
                };

                return View("../Bank/BankOuts/Print", verificationHistory);
            }
            else
            {
                using (ReportDocument rd = new ReportDocument())
                {
                    var bankTransactionsDetailsHeader = db.BankTransactionsDetailsHeader.Where(x => x.BankTransactionId == obj.Id).ToList();

                    rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormCashBankTransaction.rpt"));
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

                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankOuts, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.PRINT, UserId = User.Identity.GetUserId<int>() });
                    db.SaveChanges();

                    return new CrystalReportPdfResult(rd, "BOU_" + obj.Code + ".pdf");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BankOutsPrint")]
        public ActionResult Print([Bind(Include = "BankTransactionId,Type,Verification,Reason")] VerificationHistory verificationHistory)
        {
            verificationHistory.Type = EnumVerificationType.PrintBankTransaction;
            verificationHistory.Created = DateTime.Now;
            verificationHistory.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                BankTransaction bankTransaction = db.BankTransactions.Find(verificationHistory.BankTransactionId);

                if (SharedFunctions.CreateMD5(verificationHistory.Verification) == bankTransaction.Verification)
                {
                    using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            db.Entry(bankTransaction).State = EntityState.Modified;
                            bankTransaction.IsPrint = false;
                            bankTransaction.Verification = "-";
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

                return RedirectToAction("Print", new { id = verificationHistory.BankTransactionId });
            }

            return View("../Bank/BankOuts/Print", verificationHistory);
        }

        public JsonResult IsVerificationValid(string Verification, int? BankTransactionId)
        {
            string verification = SharedFunctions.CreateMD5(Verification);

            if (BankTransactionId != null || BankTransactionId != 0)
            {
                return Json(db.BankTransactions.Any(x => x.Id == (int)BankTransactionId && x.Verification == verification), JsonRequestBehavior.AllowGet);
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

            BankTransaction bankTransaction = db.BankTransactions.Find(id);
            if (bankTransaction == null)
            {
                return HttpNotFound();
            }
            else
            {
                bankTransaction.Verification = SharedFunctions.CreateMD5(verificationCode);
                db.Entry(bankTransaction).State = EntityState.Modified;
                db.SaveChanges();
            }

            ViewBag.VerificationCode = verificationCode;
            return PartialView("../Bank/BankOuts/_VerificationCode");
        }

        [Authorize(Roles = "BankOutsActive")]
        private BankTransaction GetModelState(BankTransaction bankTransaction)
        {
            List<BankTransactionDetails> bankTransactionDetails = db.BankTransactionsDetails.Where(x => x.BankTransactionId == bankTransaction.Id).ToList();
            string masterBusinessName = db.MasterBusinessUnits.Find(bankTransaction.MasterBusinessUnitId).Name;
            string masterRegionCode = db.MasterRegions.Find(bankTransaction.MasterRegionId).Code;

            if (ModelState.IsValid)
            {
                if (IsAnyCode(bankTransaction.Code, bankTransaction.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (bankTransactionDetails == null || bankTransactionDetails.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            if (ModelState.IsValid)
            {
                if (SharedFunctions.GetTotalBankTransactionDetailsHeader(db, bankTransaction.Id) != SharedFunctions.GetTotalBankTransactionDetails(db, bankTransaction.Id))
                    ModelState.AddModelError(string.Empty, "Jumlah debet dan kredit tidak sama!");
            }

            return bankTransaction;
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "BankOutsActive")]
        public ActionResult Cancel(int? id)
        {
            if (id != null || id == 0)
            {
                BankTransaction obj = db.BankTransactions.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                SharedFunctions.DeleteBankJournals(db, obj);

                                db.BankTransactionsDetails.RemoveRange(db.BankTransactionsDetails.Where(x => x.BankTransactionId == id));
                                db.SaveChanges();

                                db.BankTransactionsDetailsHeader.RemoveRange(db.BankTransactionsDetailsHeader.Where(x => x.BankTransactionId == id));
                                db.SaveChanges();

                                db.BankTransactions.Remove(obj);
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
        [Authorize(Roles = "BankOutsDelete")]
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
                                BankTransaction obj = db.BankTransactions.Find(id);
                                if (obj == null)
                                    failed++;
                                else
                                {
                                    BankTransaction tmp = obj;

                                    SharedFunctions.DeleteBankJournals(db, obj);

                                    db.BankTransactionsDetails.RemoveRange(db.BankTransactionsDetails.Where(x => x.BankTransactionId == id));
                                    db.SaveChanges();

                                    db.BankTransactionsDetailsHeader.RemoveRange(db.BankTransactionsDetailsHeader.Where(x => x.BankTransactionId == id));
                                    db.SaveChanges();

                                    db.BankTransactions.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankOuts, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "BankOutsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Bank/BankOuts/_DetailsGrid", db.BankTransactionsDetails
                .Where(x => x.BankTransactionId == Id).ToList());
        }

        [Authorize(Roles = "BankOutsActive")]
        public ActionResult DetailsCreate(int? bankTransactionId)
        {
            if (bankTransactionId == null || bankTransactionId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankTransaction bankTransaction = db.BankTransactions.Find(bankTransactionId);
            if (bankTransaction == null)
            {
                return HttpNotFound();
            }
            BankTransactionDetails bankTransactioneDetails = new BankTransactionDetails
            {
                BankTransactionId = (int)bankTransactionId
            };

            return PartialView("../Bank/BankOuts/_DetailsCreate", bankTransactioneDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BankOutsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,BankTransactionId,Type,MasterBankId,ChartOfAccountId,Total,Notes,Created,Updated,UserId")] BankTransactionDetails bankTransactionDetails)
        {
            bankTransactionDetails.Created = DateTime.Now;
            bankTransactionDetails.Updated = DateTime.Now;
            bankTransactionDetails.UserId = User.Identity.GetUserId<int>();
            bankTransactionDetails.MasterBankId = db.MasterBanks.Where(x => x.Active == true).FirstOrDefault().Id;

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(bankTransactionDetails.Notes)) bankTransactionDetails.Notes = bankTransactionDetails.Notes.ToUpper();

                BankTransaction bankTransaction = db.BankTransactions.Find(bankTransactionDetails.BankTransactionId);
                bankTransaction.Total = SharedFunctions.GetTotalBankTransactionDetails(db, bankTransaction.Id) + bankTransactionDetails.Total;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.BankTransactionsDetails.Add(bankTransactionDetails);
                        db.SaveChanges();

                        SharedFunctions.CreateBankJournalDetails(db, bankTransactionDetails);

                        db.Entry(bankTransaction).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankOutsDetails, MenuId = bankTransactionDetails.Id, MenuCode = bankTransactionDetails.ChartOfAccountId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Bank/BankOuts/_DetailsCreate", bankTransactionDetails);
        }

        [Authorize(Roles = "BankOutsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankTransactionDetails obj = db.BankTransactionsDetails.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Bank/BankOuts/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BankOutsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,BankTransactionId,Type,MasterBankId,ChartOfAccountId,Total,Notes,Created,Updated,UserId")] BankTransactionDetails bankTransactionDetails)
        {
            bankTransactionDetails.Updated = DateTime.Now;
            bankTransactionDetails.UserId = User.Identity.GetUserId<int>();
            bankTransactionDetails.MasterBankId = db.MasterBanks.Where(x => x.Active == true).FirstOrDefault().Id;

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(bankTransactionDetails.Notes)) bankTransactionDetails.Notes = bankTransactionDetails.Notes.ToUpper();

                BankTransaction bankTransaction = db.BankTransactions.Find(bankTransactionDetails.BankTransactionId);
                bankTransaction.Total = SharedFunctions.GetTotalBankTransactionDetails(db, bankTransaction.Id, bankTransactionDetails.Id) + bankTransactionDetails.Total;

                db.Entry(bankTransactionDetails).State = EntityState.Unchanged;
                db.Entry(bankTransactionDetails).Property("Type").IsModified = true;
                db.Entry(bankTransactionDetails).Property("MasterBankId").IsModified = true;
                db.Entry(bankTransactionDetails).Property("ChartOfAccountId").IsModified = true;
                db.Entry(bankTransactionDetails).Property("Total").IsModified = true;
                db.Entry(bankTransactionDetails).Property("Notes").IsModified = true;
                db.Entry(bankTransactionDetails).Property("Updated").IsModified = true;
                db.Entry(bankTransactionDetails).Property("UserId").IsModified = true;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.Entry(bankTransaction).Reload();

                        SharedFunctions.UpdateBankJournalDetails(db, bankTransactionDetails);

                        db.Entry(bankTransaction).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankOutsDetails, MenuId = bankTransactionDetails.Id, MenuCode = bankTransactionDetails.ChartOfAccountId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Bank/BankOuts/_DetailsEdit", bankTransactionDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "BankOutsActive")]
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
                        BankTransactionDetails obj = db.BankTransactionsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    BankTransactionDetails tmp = obj;

                                    SharedFunctions.RemoveBankJournalDetails(db, obj);

                                    BankTransaction bankTransaction = db.BankTransactions.Find(tmp.BankTransactionId);
                                    bankTransaction.Total = SharedFunctions.GetTotalBankTransactionDetails(db, bankTransaction.Id, tmp.Id);

                                    db.Entry(bankTransaction).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.BankTransactionsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankOutsDetails, MenuId = tmp.Id, MenuCode = tmp.ChartOfAccountId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "BankOutsActive")]
        public PartialViewResult DetailsHeaderGrid(int Id)
        {
            return PartialView("../Bank/BankOuts/_DetailsHeaderGrid", db.BankTransactionsDetailsHeader
                .Where(x => x.BankTransactionId == Id).ToList());
        }

        [Authorize(Roles = "BankOutsActive")]
        public ActionResult DetailsHeaderCreate(int? bankTransactionId)
        {
            if (bankTransactionId == null || bankTransactionId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankTransaction bankTransaction = db.BankTransactions.Find(bankTransactionId);
            if (bankTransaction == null)
            {
                return HttpNotFound();
            }
            BankTransactionDetailsHeader bankTransactioneDetailsHeader = new BankTransactionDetailsHeader
            {
                BankTransactionId = (int)bankTransactionId
            };

            return PartialView("../Bank/BankOuts/_DetailsHeaderCreate", bankTransactioneDetailsHeader);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BankOutsActive")]
        public ActionResult DetailsHeaderCreate([Bind(Include = "Id,BankTransactionId,Type,MasterBankId,GiroChequeId,DueDate,Voyage,Total,Notes,Created,Updated,UserId")] BankTransactionDetailsHeader bankTransactionDetailsHeader)
        {
            bankTransactionDetailsHeader.Created = DateTime.Now;
            bankTransactionDetailsHeader.Updated = DateTime.Now;
            bankTransactionDetailsHeader.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(bankTransactionDetailsHeader.Notes)) bankTransactionDetailsHeader.Notes = bankTransactionDetailsHeader.Notes.ToUpper();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.BankTransactionsDetailsHeader.Add(bankTransactionDetailsHeader);
                        db.SaveChanges();

                        SharedFunctions.CreateBankJournalDetailsHeader(db, bankTransactionDetailsHeader);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankOutsDetails, MenuId = bankTransactionDetailsHeader.Id, MenuCode = bankTransactionDetailsHeader.MasterBankId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Bank/BankOuts/_DetailsCreate", bankTransactionDetailsHeader);
        }

        [Authorize(Roles = "BankOutsActive")]
        public ActionResult DetailsHeaderEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankTransactionDetailsHeader obj = db.BankTransactionsDetailsHeader.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            return PartialView("../Bank/BankOuts/_DetailsHeaderEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BankOutsActive")]
        public ActionResult DetailsHeaderEdit([Bind(Include = "Id,BankTransactionId,Type,MasterBankId,Total,Notes,Created,Updated,UserId")] BankTransactionDetailsHeader bankTransactionDetailsHeader)
        {
            bankTransactionDetailsHeader.Updated = DateTime.Now;
            bankTransactionDetailsHeader.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    if (!string.IsNullOrEmpty(bankTransactionDetailsHeader.Notes)) bankTransactionDetailsHeader.Notes = bankTransactionDetailsHeader.Notes.ToUpper();

                    db.Entry(bankTransactionDetailsHeader).State = EntityState.Unchanged;
                    db.Entry(bankTransactionDetailsHeader).Property("Type").IsModified = true;
                    db.Entry(bankTransactionDetailsHeader).Property("MasterBankId").IsModified = true;
                    db.Entry(bankTransactionDetailsHeader).Property("Total").IsModified = true;
                    db.Entry(bankTransactionDetailsHeader).Property("Notes").IsModified = true;
                    db.Entry(bankTransactionDetailsHeader).Property("Updated").IsModified = true;
                    db.Entry(bankTransactionDetailsHeader).Property("UserId").IsModified = true;

                    try
                    {
                        db.SaveChanges();

                        db.Entry(bankTransactionDetailsHeader).Reload();

                        SharedFunctions.UpdateBankJournalDetailsHeader(db, bankTransactionDetailsHeader);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankOutsDetails, MenuId = bankTransactionDetailsHeader.Id, MenuCode = bankTransactionDetailsHeader.MasterBankId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Bank/BankOuts/_DetailsEdit", bankTransactionDetailsHeader);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "BankOutsActive")]
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
                        BankTransactionDetailsHeader obj = db.BankTransactionsDetailsHeader.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    BankTransactionDetailsHeader tmp = obj;

                                    SharedFunctions.RemoveBankJournalDetailsHeader(db, obj);

                                    db.BankTransactionsDetailsHeader.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.BankOutsDetails, MenuId = tmp.Id, MenuCode = tmp.BankTransactionId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "BankOutsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);
            BankTransaction bankTransaction = db.BankTransactions.Find(id);

            if (masterBusinessUnit != null && masterRegion != null && bankTransaction != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);

                bankTransaction.MasterBusinessUnitId = masterBusinessUnitId;
                bankTransaction.MasterRegionId = masterRegionId;
                db.Entry(bankTransaction).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "BankOutsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            DateTime date = DateTime.Now;

            string romanMonth = SharedFunctions.RomanNumeralFrom((int)date.Month);
            string code = "/" + Settings.Default.BankOutCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + romanMonth + "/" + date.Year.ToString().Substring(2, 2);

            BankTransaction lastData = db.BankTransactions
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
        [Authorize(Roles = "BankOutsActive")]
        public JsonResult GetTotal(int BankTransactionId)
        {
            return Json(SharedFunctions.GetTotalBankTransactionDetails(db, BankTransactionId).ToString("N2"));
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "BankOutsActive")]
        public JsonResult GetTotalHeader(int BankTransactionId)
        {
            return Json(SharedFunctions.GetTotalBankTransactionDetailsHeader(db, BankTransactionId).ToString("N2"));
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
