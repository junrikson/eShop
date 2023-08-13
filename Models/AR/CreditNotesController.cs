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
    public class CreditNotesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CreditNotes
        [Authorize(Roles = "CreditNotesActive")]
        public ActionResult Index()
        {
            ViewBag.Resi = "";
            return View("../AR/CreditNotes/Index");
        }

        [HttpGet]
        [Authorize(Roles = "CreditNotesActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../AR/CreditNotes/_IndexGrid", db.Set<CreditNote>().AsQueryable());
            else
                return PartialView("../AR/CreditNotes/_IndexGrid", db.Set<CreditNote>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "CreditNotesActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.CreditNotes.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.CreditNotes.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: CreditNotes/Details/5
        [Authorize(Roles = "CreditNotesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditNote creditNote = db.CreditNotes.Find(id);
            if (creditNote == null)
            {
                return HttpNotFound();
            }
            return PartialView("../AR/CreditNotes/_Details", creditNote);
        }

        // GET: CreditNotes/Create
        [Authorize(Roles = "CreditNotesAdd")]
        public ActionResult Create()
        {
            CreditNote creditNote = new CreditNote
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
                    db.CreditNotes.Add(creditNote);
                    db.SaveChanges();

                    Journal journal = SharedFunctions.CreateCreditNoteJournal(creditNote, db);
                    dbTran.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            creditNote.Code = "";
            creditNote.Active = true;
            creditNote.MasterBusinessUnitId = 0;

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name");
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes");
            return View("../AR/CreditNotes/Create", creditNote);
        }

        // POST: CreditNotes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CreditNotesAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,Notes,Total,Active,Created,Updated,UserId")] CreditNote creditNote)
        {
            creditNote.Created = DateTime.Now;
            creditNote.Updated = DateTime.Now;
            creditNote.UserId = User.Identity.GetUserId<int>();
            creditNote.Total = SharedFunctions.GetTotalCreditNote(db, creditNote.Id);

            if (db.CreditNotesDetails.Where(x => x.CreditNoteId == creditNote.Id).FirstOrDefault() == null)
                ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");

            if (ModelState.IsValid)
            {
                List<CreditNoteDetails> creditNotesDetails = db.CreditNotesDetails.Where(x => x.CreditNoteId == creditNote.Id).ToList();
                Journal journal = db.Journals.Where(x =>
                                        x.Type == EnumJournalType.CreditNote &&
                                        x.CreditNoteId == creditNote.Id).FirstOrDefault();

                if (!string.IsNullOrEmpty(creditNote.Code)) creditNote.Code = creditNote.Code.ToUpper();
                if (!string.IsNullOrEmpty(creditNote.Notes)) creditNote.Notes = creditNote.Notes.ToUpper();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Entry(creditNote).State = EntityState.Modified;
                        db.SaveChanges();

                        if (journal == null)
                        {
                            journal = SharedFunctions.CreateCreditNoteJournal(creditNote, db);
                            foreach (CreditNoteDetails creditNoteDetails in creditNotesDetails)
                            {
                                SharedFunctions.CreateCreditNoteJournalDetails(creditNoteDetails, journal, db);
                            }
                        }
                        else
                            journal = SharedFunctions.UpdateCreditNoteJournal(journal, creditNote, db);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.CreditNote, MenuId = creditNote.Id, MenuCode = creditNote.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", creditNote.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", creditNote.MasterRegionId);
            return View("../AR/CreditNotes/Create", creditNote);
        }

        // GET: CreditNotes/Edit/5
        [Authorize(Roles = "CreditNotesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditNote creditNote = db.CreditNotes.Find(id);
            if (creditNote == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", creditNote.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", creditNote.MasterRegionId);
            return View("../AR/CreditNotes/Edit", creditNote);
        }

        // POST: CreditNotes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CreditNotesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,Notes,Total,Active,Created,Updated,UserId")] CreditNote creditNote)
        {
            creditNote.Updated = DateTime.Now;
            creditNote.UserId = User.Identity.GetUserId<int>();
            creditNote.Total = SharedFunctions.GetTotalCreditNote(db, creditNote.Id);

            if (db.CreditNotesDetails.Where(x => x.CreditNoteId == creditNote.Id).FirstOrDefault() == null)
                ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        List<CreditNoteDetails> creditNotesDetails = db.CreditNotesDetails.Where(x => x.CreditNoteId == creditNote.Id).ToList();
                        Journal journal = db.Journals.Where(x =>
                                                x.Type == EnumJournalType.CreditNote &&
                                                x.CreditNoteId == creditNote.Id).FirstOrDefault();

                        if (!string.IsNullOrEmpty(creditNote.Notes)) creditNote.Notes = creditNote.Notes.ToUpper();

                        db.Entry(creditNote).State = EntityState.Unchanged;
                        db.Entry(creditNote).Property("Date").IsModified = true;
                        db.Entry(creditNote).Property("MasterBusinessUnitId").IsModified = true;
                        db.Entry(creditNote).Property("MasterRegionId").IsModified = true;
                        db.Entry(creditNote).Property("Notes").IsModified = true;
                        db.Entry(creditNote).Property("Total").IsModified = true;
                        db.Entry(creditNote).Property("Active").IsModified = true;
                        db.Entry(creditNote).Property("Updated").IsModified = true;
                        db.Entry(creditNote).Property("UserId").IsModified = true;
                        db.SaveChanges();

                        if (journal == null)
                        {
                            journal = SharedFunctions.CreateCreditNoteJournal(creditNote, db);
                            foreach (CreditNoteDetails creditNoteDetails in creditNotesDetails)
                            {
                                SharedFunctions.CreateCreditNoteJournalDetails(creditNoteDetails, journal, db);
                            }
                        }
                        else
                            journal = SharedFunctions.UpdateCreditNoteJournal(journal, creditNote, db);


                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.CreditNote, MenuId = creditNote.Id, MenuCode = creditNote.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", creditNote.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", creditNote.MasterRegionId);
            return View("../AR/CreditNotes/Edit", creditNote);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [Authorize(Roles = "CreditNotesActive")]
        public PartialViewResult OthersGrid(int? Id)
        {
            int creditNoteId = Id == null ? 0 : (int)Id;

            return PartialView("../AR/CreditNotes/_OthersGrid", db.CreditNotesDetails
                .Where(x => x.CreditNoteId == creditNoteId).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "CreditNotesActive")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                CreditNote obj = db.CreditNotes.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                Journal journal = db.Journals.Where(y => y.Type == EnumJournalType.CreditNote && y.CreditNoteId == obj.Id).FirstOrDefault();

                                SharedFunctions.DeleteCreditNoteJournals(obj.Id, journal, db);

                                db.CreditNotesDetails.RemoveRange(db.CreditNotesDetails.Where(x => x.CreditNoteId == id));
                                db.SaveChanges();

                                db.CreditNotes.Remove(obj);
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
        [Authorize(Roles = "CreditNotesDelete")]
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
                        CreditNote obj = db.CreditNotes.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            CreditNote tmp = obj;

                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    Journal journal = db.Journals.Where(y => y.Type == EnumJournalType.CreditNote && y.CreditNoteId == obj.Id).FirstOrDefault();

                                    SharedFunctions.DeleteCreditNoteJournals(obj.Id, journal, db);

                                    db.CreditNotesDetails.RemoveRange(db.CreditNotesDetails.Where(x => x.CreditNoteId == id));
                                    db.SaveChanges();

                                    db.CreditNotes.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.CreditNote, MenuId = tmp.Id, MenuCode = obj.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "CreditNotesActive")]
        public ActionResult DetailsCreate(int? creditNoteId)
        {
            if (creditNoteId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditNote creditNote = db.CreditNotes.Find(creditNoteId);
            if (creditNote == null)
            {
                return HttpNotFound();
            }
            CreditNoteDetails creditNoteDetails = new CreditNoteDetails
            {
                CreditNoteId = creditNote.Id
            };
            return PartialView("../AR/CreditNotes/_DetailsCreate", creditNoteDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CreditNotesActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,CreditNoteId,Type,MasterBankId,MasterCostId,GiroChequeId,Refference,Total,Notes,Created,Updated,UserId")] CreditNoteDetails creditNoteDetails)
        {
            creditNoteDetails.Created = DateTime.Now;
            creditNoteDetails.Updated = DateTime.Now;
            creditNoteDetails.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                creditNoteDetails = GetDetailsHeaderModelState(creditNoteDetails);
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(creditNoteDetails.Notes)) creditNoteDetails.Notes = creditNoteDetails.Notes.ToUpper();
                if (!string.IsNullOrEmpty(creditNoteDetails.Refference)) creditNoteDetails.Refference = creditNoteDetails.Refference.ToUpper();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.CreditNotesDetails.Add(creditNoteDetails);
                        db.SaveChanges();

                        Journal journal = db.Journals.Where(x =>
                                                x.Type == EnumJournalType.CreditNote &&
                                                x.CreditNoteId == creditNoteDetails.CreditNoteId).FirstOrDefault();

                        if (journal != null)
                            SharedFunctions.CreateCreditNoteJournalDetails(creditNoteDetails, journal, db);

                        CreditNote creditNote = db.CreditNotes.Find(creditNoteDetails.CreditNoteId);
                        creditNote.Total = SharedFunctions.GetTotalCreditNote(db, creditNote.Id) + creditNoteDetails.Total;

                        db.Entry(creditNote).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.CreditNoteDetails, MenuId = creditNoteDetails.Id, MenuCode = creditNoteDetails.Id.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../AR/CreditNotes/_DetailsCreate", creditNoteDetails);
        }

        [Authorize(Roles = "CreditNotesActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditNoteDetails obj = db.CreditNotesDetails.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../AR/CreditNotes/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CreditNotesActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,CreditNoteId,Type,MasterBankId,MasterCostId,GiroChequeId,Refference,Total,Notes,Created,Updated,UserId")] CreditNoteDetails creditNoteDetails)
        {
            creditNoteDetails.Updated = DateTime.Now;
            creditNoteDetails.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                creditNoteDetails = GetDetailsHeaderModelState(creditNoteDetails);
            }

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(creditNoteDetails.Notes)) creditNoteDetails.Notes = creditNoteDetails.Notes.ToUpper();
                        if (!string.IsNullOrEmpty(creditNoteDetails.Refference)) creditNoteDetails.Refference = creditNoteDetails.Refference.ToUpper();

                        db.Entry(creditNoteDetails).State = EntityState.Unchanged;
                        db.Entry(creditNoteDetails).Property("Type").IsModified = true;
                        db.Entry(creditNoteDetails).Property("MasterBankId").IsModified = true;
                        db.Entry(creditNoteDetails).Property("MasterCostId").IsModified = true;
                        db.Entry(creditNoteDetails).Property("GiroChequeId").IsModified = true;
                        db.Entry(creditNoteDetails).Property("Refference").IsModified = true;
                        db.Entry(creditNoteDetails).Property("Total").IsModified = true;
                        db.Entry(creditNoteDetails).Property("Notes").IsModified = true;
                        db.Entry(creditNoteDetails).Property("Updated").IsModified = true;
                        db.Entry(creditNoteDetails).Property("UserId").IsModified = true;
                        db.SaveChanges();

                        SharedFunctions.UpdateCreditNoteJournalDetails(creditNoteDetails, db);

                        CreditNote creditNote = db.CreditNotes.Find(creditNoteDetails.CreditNoteId);
                        creditNote.Total = SharedFunctions.GetTotalCreditNote(db, creditNote.Id) + creditNoteDetails.Total;

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.CreditNoteDetails, MenuId = creditNoteDetails.Id, MenuCode = creditNoteDetails.Id.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../AR/CreditNotes/_DetailsEdit", creditNoteDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "CreditNotesActive")]
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
                        CreditNoteDetails obj = db.CreditNotesDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    CreditNoteDetails tmp = obj;

                                    Journal journal = db.Journals.Where(x =>
                                                            x.Type == EnumJournalType.CreditNote &&
                                                            x.CreditNoteId == obj.CreditNoteId).FirstOrDefault();

                                    if (journal != null)
                                        SharedFunctions.RemoveCreditNoteJournalDetails(obj, journal, db);

                                    CreditNote creditNote = db.CreditNotes.Find(tmp.CreditNoteId);

                                    creditNote.Total = SharedFunctions.GetTotalCreditNote(db, creditNote.Id, tmp.Id);

                                    db.Entry(creditNote).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.CreditNotesDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.CreditNoteDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        private CreditNoteDetails GetDetailsHeaderModelState(CreditNoteDetails creditNoteDetails)
        {
            if (ModelState.IsValid)
            {
                if ((creditNoteDetails.Type == EnumCreditNoteType.Bank || creditNoteDetails.Type == EnumCreditNoteType.Cash) && (creditNoteDetails.MasterBankId == 0 || creditNoteDetails.MasterBankId == null))
                {
                    ModelState.AddModelError(string.Empty, "Master Kas / Bank belum diisi!");
                }
                else if (creditNoteDetails.Type == EnumCreditNoteType.Cheque && (creditNoteDetails.ChequeId == 0 || creditNoteDetails.ChequeId == null))
                {
                    ModelState.AddModelError(string.Empty, "Giro / Cek belum diisi!");
                }
                else if (creditNoteDetails.Type == EnumCreditNoteType.MasterCost && (creditNoteDetails.MasterCostId == 0 || creditNoteDetails.MasterCostId == null))
                {
                    ModelState.AddModelError(string.Empty, "Master Biaya belum diisi!");
                }
            }

            return creditNoteDetails;
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "CreditNotesActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);
            CreditNote creditNote = db.CreditNotes.Find(id);

            if (masterBusinessUnit != null && creditNote != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                creditNote.Code = code;
                creditNote.MasterBusinessUnitId = masterBusinessUnitId;
                creditNote.MasterRegionId = masterRegionId;
                db.Entry(creditNote).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "CreditNotesActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.CreditNoteCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            CreditNote lastData = db.CreditNotes
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
        [Authorize(Roles = "CreditNotesActive")]
        public JsonResult GetTotal(int CreditNoteId)
        {
            return Json(SharedFunctions.GetTotalCreditNote(db, CreditNoteId).ToString("N2"));
        }

        [Authorize(Roles = "CreditNotesPrint")]
        public ActionResult Print(int? id)
        {
            CreditNote obj = db.CreditNotes.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }

            string details = "";
            string creditNoteDetails = "";

            details = details + obj.Notes;

            if (details != "") details = details + "\n";

            var temps = db.CreditNotesDetails.Where(x => x.CreditNoteId == obj.Id).ToList().OrderBy(y => y.Id);

            if (temps != null)
            {
                foreach (CreditNoteDetails temp in temps)
                {
                    if (creditNoteDetails != "") creditNoteDetails = creditNoteDetails + ",\n";

                    creditNoteDetails = creditNoteDetails + temp.Notes;
                }
            }

            if (creditNoteDetails != "") details = details + creditNoteDetails;

            using (ReportDocument rd = new ReportDocument())
            {
                rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormCreditNote.rpt"));
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

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.CreditNote, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.PRINT, UserId = User.Identity.GetUserId<int>() });
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
