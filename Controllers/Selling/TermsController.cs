using eShop.Extensions;
using eShop.Models;
using eShop.Properties;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace eShop.Controllers
{
    [Authorize]
    public class TermsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterItems
        [Authorize(Roles = "TermsActive")]
        public ActionResult Index()
        {
            return View("../Selling/Terms/Index");
        }

        [HttpGet]
        [Authorize(Roles = "TermsActive")]
        public PartialViewResult IndexGrid(string search)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            var masterRegions = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterRegionId).Distinct().ToList();
            var masterBusinessUnits = user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnitId).Distinct().ToList();

            if (string.IsNullOrEmpty(search))
            {
                return PartialView("../Selling/Terms/_IndexGrid", db.Set<Term>().Where(x =>
                        masterRegions.Contains(x.MasterRegionId) &&
                        masterBusinessUnits.Contains(x.MasterBusinessUnitId)).AsQueryable());
            }
            else
            {
                return PartialView("../Selling/Terms/_IndexGrid", db.Set<Term>().Where(x =>
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
                return db.Terms.Any(x => x.Code == Code);
            }
            else
            {
                return db.Terms.Any(x => x.Code == Code && x.Id != Id);
            }
        }

        [HttpGet]
        [Authorize(Roles = "TermsActive")]
        public PartialViewResult OthersGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Selling/Terms/_OthersGrid", db.Set<Term>().AsQueryable());
            else
                return PartialView("../Selling/Terms/_OthersGrid", db.Set<Term>().AsQueryable()
                    .Where(y => y.Code.Contains(search)));
        }

        // GET: MasterItems/Details/5
        [Authorize(Roles = "TermsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Term Term = db.Terms.Find(id);
            if (Term == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/Terms/_Details", Term);
        }

        [HttpGet]
        [Authorize(Roles = "TermsView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Selling/Terms/_ViewGrid", db.TermSalesPersons
                .Where(x => x.TermId == Id).ToList());
        }

        // GET: MasterItems/Create
        [Authorize(Roles = "TermsAdd")]
        public ActionResult Create()
        {

            Term term = new Term
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                MasterBusinessUnitId = db.MasterBusinessUnits.FirstOrDefault().Id,
                MasterRegionId = db.MasterRegions.FirstOrDefault().Id,
                Notes = "",
                Top = 0,
                Topx = 0,
                CreditLimit = 0,    
                InvoiceLimit = 0,
                Active = false,
                AllCustomer = false,
                AllSalesPerson = false,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UserId = User.Identity.GetUserId<int>()
            };

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    db.Terms.Add(term);
                    db.SaveChanges();

                    dbTran.Commit();

                    term.Code = "";
                    term.Active = true;
                    term.AllSalesPerson = false;
                    term.AllCustomer = false;   
                    term.MasterBusinessUnitId = 0;
                    term.MasterRegionId = 0;
                    term.Top = 0;
                    term.Topx = 0;
                    term.CreditLimit = 0;
                    term.InvoiceLimit = 0;

                }

                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            return View("../Selling/Terms/Create", term);
        }

        // POST: MasterItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "TermsAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,MasterBusinessUnitId,MasterRegionId,Top,Topx,CreditLimit,InvoiceLimit,AllSalesPerson,AllCustomer,Notes,Active,Created,Updated,UserId")] Term term)
        {
            term.UserId = User.Identity.GetUserId<int>();
            term.Created = DateTime.Now;
            term.Updated = DateTime.Now;

            if (!string.IsNullOrEmpty(term.Code)) term.Code = term.Code.ToUpper();
            if (!string.IsNullOrEmpty(term.Notes)) term.Notes = term.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                db.Entry(term).State = EntityState.Unchanged;
                db.Entry(term).Property("Code").IsModified = true;
                db.Entry(term).Property("Notes").IsModified = true;
                db.Entry(term).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(term).Property("MasterRegionId").IsModified = true;
                db.Entry(term).Property("Top").IsModified = true;
                db.Entry(term).Property("Topx").IsModified = true;
                db.Entry(term).Property("CreditLimit").IsModified = true;
                db.Entry(term).Property("InvoiceLimit").IsModified = true;
                db.Entry(term).Property("AllSalesPerson").IsModified = true;
                db.Entry(term).Property("AllCustomer").IsModified = true;
                db.Entry(term).Property("Active").IsModified = true;
                db.Entry(term).Property("Updated").IsModified = true;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Terms, MenuId = term.Id, MenuCode = term.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
                }
            }

            return View("../Selling/Terms/Create", term);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "TermsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null || id == 0)
            {
                Term obj = db.Terms.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.TermSalesPersons.RemoveRange(db.TermSalesPersons.Where(x => x.TermId == obj.Id));
                                db.SaveChanges();

                                db.TermCustomers.RemoveRange(db.TermCustomers.Where(x => x.TermId == obj.Id));
                                db.SaveChanges();

                                db.Terms.Remove(obj);
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

        // GET: MasterItems/Edit/5
        [Authorize(Roles = "TermsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Term term = db.Terms.Find(id);
            if (term == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", term.MasterBusinessUnitId);

            return View("../Selling/Terms/Edit", term);
        }

        // POST: MasterItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TermsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,MasterBusinessUnitId,MasterRegionId,Top,Topx,CreditLimit,InvoiceLimit,AllSalesPerson,AllCustomer,Notes,Active,Updated,UserId")] Term term)
        {
            term.Updated = DateTime.Now;
            term.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(term.Code)) term.Code = term.Code.ToUpper();
            if (!string.IsNullOrEmpty(term.Notes)) term.Notes = term.Notes.ToUpper();

            db.Entry(term).State = EntityState.Unchanged;
            db.Entry(term).Property("Code").IsModified = true;
            db.Entry(term).Property("Notes").IsModified = true;
            db.Entry(term).Property("MasterBusinessUnitId").IsModified = true;
            db.Entry(term).Property("MasterRegionId").IsModified = true;
            db.Entry(term).Property("Active").IsModified = true;
            db.Entry(term).Property("Top").IsModified = true;
            db.Entry(term).Property("Topx").IsModified = true;
            db.Entry(term).Property("CreditLimit").IsModified = true;
            db.Entry(term).Property("InvoiceLimit").IsModified = true;
            db.Entry(term).Property("AllSalesPerson").IsModified = true;
            db.Entry(term).Property("AllCustomer").IsModified = true;
            db.Entry(term).Property("Updated").IsModified = true;
            db.Entry(term).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Terms, MenuId = term.Id, MenuCode = term.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", term.MasterBusinessUnitId);

            return View("../Selling/Terms/Edit", term);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "TermsDelete")]
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
                        Term obj = db.Terms.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    Term tmp = obj;

                                    db.TermSalesPersons.RemoveRange(db.TermSalesPersons.Where(x => x.TermId == obj.Id));
                                    db.SaveChanges();

                                    db.TermCustomers.RemoveRange(db.TermCustomers.Where(x => x.TermId == obj.Id));
                                    db.SaveChanges();

                                    db.Terms.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Terms, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        [Authorize(Roles = "TermsActive")]
        private Term GetModelState(Term term)
        {
            List<TermSalesPerson> termSalesPerson = db.TermSalesPersons.Where(x => x.TermId == term.Id).ToList();
            List<TermCustomer> termCustomer = db.TermCustomers.Where(x => x.TermId == term.Id).ToList();

            if (ModelState.IsValid)
            {
                if (IsAnyCode(term.Code, term.Id))
                    ModelState.AddModelError(string.Empty, "Nomor transaksi sudah dipakai!");
            }

            if (ModelState.IsValid)
            {
                if (termSalesPerson == null || termSalesPerson.Count == 0 || termCustomer == null || termCustomer.Count == 0)
                    ModelState.AddModelError(string.Empty, "Data masih kosong, mohon isi detail terlebih dahulu!");
            }

            return term;
        }

        [HttpGet]
        [Authorize(Roles = "TermsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Selling/Terms/_DetailsGrid", db.TermSalesPersons
                .Where(x => x.TermId == Id).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "TermsActive")]
        public PartialViewResult CustomersGrid(int Id)
        {
            return PartialView("../Selling/Terms/_CustomersGrid", db.TermCustomers
                .Where(x => x.TermId == Id).ToList());
        }


        [Authorize(Roles = "TermsActive")]
        public ActionResult DetailsCreate(int termId)
        {
            Term term = db.Terms.Find(termId);

            if (term == null)
            {
                return HttpNotFound();
            }

            TermSalesPerson termSalesPerson = new TermSalesPerson
            {
                TermId = termId
            };

            return PartialView("../Selling/Terms/_DetailsCreate", termSalesPerson);
        }

        [Authorize(Roles = "TermsActive")]
        public ActionResult CustomersCreate(int termId)
        {
            Term term = db.Terms.Find(termId);

            if (term == null)
            {
                return HttpNotFound();
            }

            TermCustomer termCustomer = new TermCustomer
            {
                TermId = termId
            };

            return PartialView("../Selling/Terms/_CustomersCreate", termCustomer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TermsActive")]
        public ActionResult DetailsCreate([Bind(Include = "Id,TermId,MasterSalespersonId,Created,Updated,UserId")] TermSalesPerson obj)
        {
            obj.Created = DateTime.Now;
            obj.Updated = DateTime.Now;
            obj.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.TermSalesPersons.Add(obj);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.TermSalesPersons, MenuId = obj.TermId, MenuCode = obj.MasterSalesPersonId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Selling/Terms/_DetailsCreate", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TermsActive")]
        public ActionResult CustomersCreate([Bind(Include = "Id,TermId,MasterCustomerId,Created,Updated,UserId")] TermCustomer obj)
        {
            obj.Created = DateTime.Now;
            obj.Updated = DateTime.Now;
            obj.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.TermCustomers.Add(obj);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.TermCustomers, MenuId = obj.TermId, MenuCode = obj.MasterCustomerId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
            return PartialView("../Selling/Terms/_CustomersCreate", obj);
        }

        [Authorize(Roles = "TermsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TermSalesPerson obj = db.TermSalesPersons.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/Terms/_DetailsEdit", obj);
        }

        [Authorize(Roles = "TermsActive")]
        public ActionResult CustomersEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TermCustomer obj = db.TermCustomers.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Selling/Terms/_CustomersEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,TermId,MasterSalesPersonId,Updated,UserId")] TermSalesPerson obj)
        {
            obj.Updated = DateTime.Now;
            obj.UserId = User.Identity.GetUserId<int>();


            db.Entry(obj).State = EntityState.Unchanged;
            db.Entry(obj).Property("MasterSalesPersonId").IsModified = true;
            db.Entry(obj).Property("Updated").IsModified = true;
            db.Entry(obj).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.TermSalesPersons, MenuId = obj.MasterSalesPersonId, MenuCode = obj.MasterSalesPersonId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Selling/Terms/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult CustomersEdit([Bind(Include = "Id,TermId,MasterCustomerId,Updated,UserId")] TermCustomer obj)
        {
            obj.Updated = DateTime.Now;
            obj.UserId = User.Identity.GetUserId<int>();

            db.Entry(obj).State = EntityState.Unchanged;
            db.Entry(obj).Property("MasterCustomerId").IsModified = true;
            db.Entry(obj).Property("Updated").IsModified = true;
            db.Entry(obj).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.TermSalesPersons, MenuId = obj.MasterCustomerId, MenuCode = obj.MasterCustomerId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
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

            return PartialView("../Selling/Terms/_CustomersEdit", obj);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "TermsActive")]
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
                        TermSalesPerson obj = db.TermSalesPersons.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    TermSalesPerson tmp = obj;

                                    db.TermSalesPersons.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.TermSalesPersons, MenuId = tmp.MasterSalesPersonId, MenuCode = tmp.MasterSalesPersonId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "TermsActive")]
        public ActionResult CustomersBatchDelete(int[] ids)
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
                        TermCustomer obj = db.TermCustomers.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    TermCustomer tmp = obj;

                                    db.TermCustomers.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.TermCustomers, MenuId = tmp.MasterCustomerId, MenuCode = tmp.MasterCustomerId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
        [Authorize(Roles = "TermsActive")]
        public JsonResult GetCode(int id, int masterBusinessUnitId, int masterRegionId)
        {
            string code = null;
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            MasterRegion masterRegion = db.MasterRegions.Find(masterRegionId);

            Term term = db.Terms.Find(id);

            if (masterBusinessUnit != null && term != null && masterRegion != null)
            {
                code = GetCode(masterBusinessUnit, masterRegion);
                term.MasterBusinessUnitId = masterBusinessUnitId;
                term.MasterRegionId = masterRegionId;
                db.Entry(term).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(code);
        }

        [Authorize(Roles = "TermsActive")]
        private string GetCode(MasterBusinessUnit masterBusinessUnit, MasterRegion masterRegion)
        {
            string romanMonth = SharedFunctions.RomanNumeralFrom((int)DateTime.Now.Month);
            string code = "/" + Settings.Default.TermCode + masterBusinessUnit.Code + "/" + masterRegion.Code + "/" + SharedFunctions.RomanNumeralFrom(DateTime.Now.Month) + "/" + DateTime.Now.Year.ToString().Substring(2, 2);

            Term lastData = db.Terms
                .Where(x => (x.Code.Contains(code)))
                .OrderByDescending(z => z.Code).FirstOrDefault();

            if (lastData == null)
                code = "0001" + code;
            else
                code = (Convert.ToInt32(lastData.Code.Substring(0, 4)) + 1).ToString("D4") + code;

            return code;
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
