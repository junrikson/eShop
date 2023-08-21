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
    public class AdvanceRepaymentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AdvanceRepayments
        [Authorize(Roles = "AdvanceRepaymentsActive")]
        public ActionResult Index()
        {
            ViewBag.Resi = "";
            return View("../AR/AdvanceRepayments/Index");
        }

        [HttpGet]
        [Authorize(Roles = "AdvanceRepaymentsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../AR/AdvanceRepayments/_IndexGrid", db.Set<AdvanceRepayment>().AsQueryable());
            else
                return PartialView("../AR/AdvanceRepayments/_IndexGrid", db.Set<AdvanceRepayment>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "AdvanceRepaymentsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.AdvanceRepayments.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.AdvanceRepayments.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: AdvanceRepayments/Details/5
        [Authorize(Roles = "AdvanceRepaymentsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdvanceRepayment advanceRepayment = db.AdvanceRepayments.Find(id);
            if (advanceRepayment == null)
            {
                return HttpNotFound();
            }
            return PartialView("../AR/AdvanceRepayments/_Details", advanceRepayment);
        }

        // GET: AdvanceRepayments/Create
        [Authorize(Roles = "AdvanceRepaymentsAdd")]
        public ActionResult Create()
        {
            AdvanceRepayment advanceRepayment = new AdvanceRepayment
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
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
                    db.AdvanceRepayments.Add(advanceRepayment);
                    db.SaveChanges();

                    SharedFunctions.CreateAdvanceRepaymentJournal(db, advanceRepayment);

                    dbTran.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            advanceRepayment.Code = "";
            advanceRepayment.Active = true;
            advanceRepayment.MasterBusinessUnitId = 0;

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name");
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes");
            return View("../AR/AdvanceRepayments/Create", advanceRepayment);
        }

        // POST: AdvanceRepayments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AdvanceRepaymentsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,Notes,Total,Active,Created,Updated,UserId")] AdvanceRepayment advanceRepayment)
        {
            advanceRepayment.Created = DateTime.Now;
            advanceRepayment.Updated = DateTime.Now;
            advanceRepayment.UserId = User.Identity.GetUserId<int>();
            advanceRepayment.Total = SharedFunctions.GetTotalAdvanceRepayment(db, advanceRepayment.Id);

            if (db.AdvanceRepaymentsDetails.Where(x => x.AdvanceRepaymentId == advanceRepayment.Id).FirstOrDefault() == null)
                ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(advanceRepayment.Code)) advanceRepayment.Code = advanceRepayment.Code.ToUpper();
                if (!string.IsNullOrEmpty(advanceRepayment.Notes)) advanceRepayment.Notes = advanceRepayment.Notes.ToUpper();

                db.Entry(advanceRepayment).State = EntityState.Modified;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.Entry(advanceRepayment).Reload();

                        SharedFunctions.UpdateAdvanceRepaymentJournal(db, advanceRepayment);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.AdvanceRepayment, MenuId = advanceRepayment.Id, MenuCode = advanceRepayment.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", advanceRepayment.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", advanceRepayment.MasterRegionId);
            return View("../AR/AdvanceRepayments/Create", advanceRepayment);
        }

        // GET: AdvanceRepayments/Edit/5
        [Authorize(Roles = "AdvanceRepaymentsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdvanceRepayment advanceRepayment = db.AdvanceRepayments.Find(id);
            if (advanceRepayment == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", advanceRepayment.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", advanceRepayment.MasterRegionId);
            return View("../AR/AdvanceRepayments/Edit", advanceRepayment);
        }

        // POST: AdvanceRepayments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AdvanceRepaymentsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,MasterBusinessUnitId,MasterRegionId,Notes,Total,Active,Created,Updated,UserId")] AdvanceRepayment advanceRepayment)
        {
            advanceRepayment.Updated = DateTime.Now;
            advanceRepayment.UserId = User.Identity.GetUserId<int>();
            advanceRepayment.Total = SharedFunctions.GetTotalAdvanceRepayment(db, advanceRepayment.Id);

            if (db.AdvanceRepaymentsDetails.Where(x => x.AdvanceRepaymentId == advanceRepayment.Id).FirstOrDefault() == null)
                ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(advanceRepayment.Code)) advanceRepayment.Code = advanceRepayment.Code.ToUpper();
                if (!string.IsNullOrEmpty(advanceRepayment.Notes)) advanceRepayment.Notes = advanceRepayment.Notes.ToUpper();

                db.Entry(advanceRepayment).State = EntityState.Unchanged;
                db.Entry(advanceRepayment).Property("Code").IsModified = true;
                db.Entry(advanceRepayment).Property("Date").IsModified = true;
                db.Entry(advanceRepayment).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(advanceRepayment).Property("MasterRegionId").IsModified = true;
                db.Entry(advanceRepayment).Property("Notes").IsModified = true;
                db.Entry(advanceRepayment).Property("Total").IsModified = true;
                db.Entry(advanceRepayment).Property("Active").IsModified = true;
                db.Entry(advanceRepayment).Property("Updated").IsModified = true;
                db.Entry(advanceRepayment).Property("UserId").IsModified = true;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.Entry(advanceRepayment).Reload();

                        SharedFunctions.UpdateAdvanceRepaymentJournal(db, advanceRepayment);

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.AdvanceRepayment, MenuId = advanceRepayment.Id, MenuCode = advanceRepayment.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", advanceRepayment.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Notes", advanceRepayment.MasterRegionId);
            return View("../AR/AdvanceRepayments/Edit", advanceRepayment);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [Authorize(Roles = "AdvanceRepaymentsActive")]
        public PartialViewResult OthersGrid(int? Id)
        {
            int advanceRepaymentId = Id == null ? 0 : (int)Id;

            return PartialView("../AR/AdvanceRepayments/_OthersGrid", db.AdvanceRepaymentsDetails
                .Where(x => x.AdvanceRepaymentId == advanceRepaymentId).ToList());
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "AdvanceRepaymentsActive")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                AdvanceRepayment obj = db.AdvanceRepayments.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                SharedFunctions.DeleteAdvanceRepaymentJournals(db, obj);

                                db.AdvanceRepaymentsDetails.RemoveRange(db.AdvanceRepaymentsDetails.Where(x => x.AdvanceRepaymentId == id));
                                db.SaveChanges();

                                db.AdvanceRepayments.Remove(obj);
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
        [Authorize(Roles = "AdvanceRepaymentsDelete")]
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
                        AdvanceRepayment obj = db.AdvanceRepayments.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            AdvanceRepayment tmp = obj;

                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    SharedFunctions.DeleteAdvanceRepaymentJournals(db, obj);

                                    db.AdvanceRepaymentsDetails.RemoveRange(db.AdvanceRepaymentsDetails.Where(x => x.AdvanceRepaymentId == id));
                                    db.SaveChanges();

                                    db.AdvanceRepayments.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.AdvanceRepayment, MenuId = tmp.Id, MenuCode = obj.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "AdvanceRepaymentsActive")]
        public ActionResult DetailsCreate(int? advanceRepaymentId)
        {
            if (advanceRepaymentId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdvanceRepayment advanceRepayment = db.AdvanceRepayments.Find(advanceRepaymentId);
            if (advanceRepayment == null)
            {
                return HttpNotFound();
            }
            AdvanceRepaymentDetails advanceRepaymentDetails = new AdvanceRepaymentDetails
            {
                AdvanceRepaymentId = advanceRepayment.Id
            };
            return PartialView("../AR/AdvanceRepayments/_DetailsCreate", advanceRepaymentDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AdvanceRepaymentsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,AdvanceRepaymentId,Type,MasterBankId,MasterCostId,ChequeId,Refference,Total,Notes,Created,Updated,UserId")] AdvanceRepaymentDetails advanceRepaymentDetails)
        {
            advanceRepaymentDetails.Created = DateTime.Now;
            advanceRepaymentDetails.Updated = DateTime.Now;
            advanceRepaymentDetails.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                advanceRepaymentDetails = GetDetailsHeaderModelState(advanceRepaymentDetails);
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(advanceRepaymentDetails.Notes)) advanceRepaymentDetails.Notes = advanceRepaymentDetails.Notes.ToUpper();
                if (!string.IsNullOrEmpty(advanceRepaymentDetails.Refference)) advanceRepaymentDetails.Refference = advanceRepaymentDetails.Refference.ToUpper();

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.AdvanceRepaymentsDetails.Add(advanceRepaymentDetails);
                        db.SaveChanges();

                        SharedFunctions.CreateAdvanceRepaymentJournalDetails(db, advanceRepaymentDetails);

                        AdvanceRepayment advanceRepayment = db.AdvanceRepayments.Find(advanceRepaymentDetails.AdvanceRepaymentId);
                        advanceRepayment.Total = SharedFunctions.GetTotalAdvanceRepayment(db, advanceRepayment.Id) + advanceRepaymentDetails.Total;

                        db.Entry(advanceRepayment).State = EntityState.Modified;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.AdvanceRepaymentDetails, MenuId = advanceRepaymentDetails.Id, MenuCode = advanceRepaymentDetails.Id.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../AR/AdvanceRepayments/_DetailsCreate", advanceRepaymentDetails);
        }

        [Authorize(Roles = "AdvanceRepaymentsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdvanceRepaymentDetails obj = db.AdvanceRepaymentsDetails.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../AR/AdvanceRepayments/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AdvanceRepaymentsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,AdvanceRepaymentId,Type,MasterBankId,MasterCostId,ChequeId,Refference,Total,Notes,Created,Updated,UserId")] AdvanceRepaymentDetails advanceRepaymentDetails)
        {
            advanceRepaymentDetails.Updated = DateTime.Now;
            advanceRepaymentDetails.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                advanceRepaymentDetails = GetDetailsHeaderModelState(advanceRepaymentDetails);
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(advanceRepaymentDetails.Notes)) advanceRepaymentDetails.Notes = advanceRepaymentDetails.Notes.ToUpper();
                if (!string.IsNullOrEmpty(advanceRepaymentDetails.Refference)) advanceRepaymentDetails.Refference = advanceRepaymentDetails.Refference.ToUpper();

                db.Entry(advanceRepaymentDetails).State = EntityState.Unchanged;
                db.Entry(advanceRepaymentDetails).Property("Type").IsModified = true;
                db.Entry(advanceRepaymentDetails).Property("MasterBankId").IsModified = true;
                db.Entry(advanceRepaymentDetails).Property("MasterCostId").IsModified = true;
                db.Entry(advanceRepaymentDetails).Property("ChequeId").IsModified = true;
                db.Entry(advanceRepaymentDetails).Property("Refference").IsModified = true;
                db.Entry(advanceRepaymentDetails).Property("Total").IsModified = true;
                db.Entry(advanceRepaymentDetails).Property("Notes").IsModified = true;
                db.Entry(advanceRepaymentDetails).Property("Updated").IsModified = true;
                db.Entry(advanceRepaymentDetails).Property("UserId").IsModified = true;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();
                        db.Entry(advanceRepaymentDetails).Reload();

                        SharedFunctions.UpdateAdvanceRepaymentJournalDetails(db, advanceRepaymentDetails);

                        AdvanceRepayment advanceRepayment = db.AdvanceRepayments.Find(advanceRepaymentDetails.AdvanceRepaymentId);
                        advanceRepayment.Total = SharedFunctions.GetTotalAdvanceRepayment(db, advanceRepayment.Id) + advanceRepaymentDetails.Total;

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.AdvanceRepaymentDetails, MenuId = advanceRepaymentDetails.Id, MenuCode = advanceRepaymentDetails.Id.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../AR/AdvanceRepayments/_DetailsEdit", advanceRepaymentDetails);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "AdvanceRepaymentsActive")]
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
                        AdvanceRepaymentDetails obj = db.AdvanceRepaymentsDetails.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    AdvanceRepaymentDetails tmp = obj;

                                    SharedFunctions.RemoveAdvanceRepaymentJournalDetails(db, obj);

                                    AdvanceRepayment advanceRepayment = db.AdvanceRepayments.Find(tmp.AdvanceRepaymentId);

                                    advanceRepayment.Total = SharedFunctions.GetTotalAdvanceRepayment(db, advanceRepayment.Id, tmp.Id);

                                    db.Entry(advanceRepayment).State = EntityState.Modified;
                                    db.SaveChanges();

                                    db.AdvanceRepaymentsDetails.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.AdvanceRepaymentDetails, MenuId = tmp.Id, MenuCode = tmp.Id.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        private AdvanceRepaymentDetails GetDetailsHeaderModelState(AdvanceRepaymentDetails advanceRepaymentDetails)
        {
            if (ModelState.IsValid)
            {
                if ((advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.Bank || advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.Cash) && (advanceRepaymentDetails.MasterBankId == 0 || advanceRepaymentDetails.MasterBankId == null))
                {
                    ModelState.AddModelError(string.Empty, "Master Kas / Bank belum diisi!");
                }
                else if (advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.Cheque && (advanceRepaymentDetails.ChequeId == 0 || advanceRepaymentDetails.ChequeId == null))
                {
                    ModelState.AddModelError(string.Empty, "Giro / Cek belum diisi!");
                }
                else if (advanceRepaymentDetails.Type == EnumAdvanceRepaymentType.MasterCost && (advanceRepaymentDetails.MasterCostId == 0 || advanceRepaymentDetails.MasterCostId == null))
                {
                    ModelState.AddModelError(string.Empty, "Master Biaya belum diisi!");
                }
            }

            return advanceRepaymentDetails;
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "AdvanceRepaymentsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);
            AdvanceRepayment advanceRepayment = db.AdvanceRepayments.Find(id);

            if (masterBusinessUnit != null && advanceRepayment != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                advanceRepayment.Code = code;
                advanceRepayment.MasterBusinessUnitId = masterBusinessUnitId;
                advanceRepayment.MasterRegionId = masterRegionId;
                db.Entry(advanceRepayment).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "AdvanceRepaymentsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.AdvanceRepaymentCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            AdvanceRepayment lastData = db.AdvanceRepayments
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
        [Authorize(Roles = "AdvanceRepaymentsActive")]
        public JsonResult GetTotal(int AdvanceRepaymentId)
        {
            return Json(SharedFunctions.GetTotalAdvanceRepayment(db, AdvanceRepaymentId).ToString("N2"));
        }

        [Authorize(Roles = "AdvanceRepaymentsPrint")]
        public ActionResult Print(int? id)
        {
            AdvanceRepayment obj = db.AdvanceRepayments.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }

            string details = "";
            string advanceRepaymentDetails = "";

            details = details + obj.Notes;

            if (details != "") details = details + "\n";

            var temps = db.AdvanceRepaymentsDetails.Where(x => x.AdvanceRepaymentId == obj.Id).ToList().OrderBy(y => y.Id);

            if (temps != null)
            {
                foreach (AdvanceRepaymentDetails temp in temps)
                {
                    if (advanceRepaymentDetails != "") advanceRepaymentDetails = advanceRepaymentDetails + ",\n";

                    advanceRepaymentDetails = advanceRepaymentDetails + temp.Notes;
                }
            }

            if (advanceRepaymentDetails != "") details = details + advanceRepaymentDetails;

            using (ReportDocument rd = new ReportDocument())
            {
                rd.Load(Path.Combine(Server.MapPath("~/CrystalReports"), "FormAdvanceRepayment.rpt"));
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

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.AdvanceRepayment, MenuId = obj.Id, MenuCode = obj.Code, Actions = EnumActions.PRINT, UserId = User.Identity.GetUserId<int>() });
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
