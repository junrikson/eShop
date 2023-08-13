using eShop.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace eShop.Controllers
{
    [Authorize]
    public class ChequeInsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ChequeIns
        [Authorize(Roles = "ChequeInsActive")]
        public ActionResult Index()
        {
            return View("../CashAndBank/ChequeIns/Index");
        }

        [HttpGet]
        [Authorize(Roles = "ChequeInsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../CashAndBank/ChequeIns/_IndexGrid", db.Set<Cheque>().Where(o => o.Type == EnumChequeType.ChequeIn).AsQueryable());
            else
                return PartialView("../CashAndBank/ChequeIns/_IndexGrid", db.Set<Cheque>().Where(o => o.Type == EnumChequeType.ChequeIn).AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "ChequeInsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.Cheques.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.Cheques.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ChequeIns/Details/5
        [Authorize(Roles = "ChequeInsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cheque Cheque = db.Cheques.Find(id);
            if (Cheque == null)
            {
                return HttpNotFound();
            }
            return PartialView("../CashAndBank/ChequeIns/_Details", Cheque);
        }

        // GET: ChequeIns/Create
        [Authorize(Roles = "ChequeInsAdd")]
        public ActionResult Create()
        {
            Cheque obj = new Cheque
            {
                Active = true,
                Type = EnumChequeType.ChequeIn,
                Date = DateTime.Now,
                DueDate = DateTime.Now
            };
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name");
            return PartialView("../CashAndBank/ChequeIns/_Create", obj);
        }

        // POST: ChequeIns/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ChequeInsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,DueDate,Issued,MasterBusinessUnitId,Type,OnBehalf,AccNumber,Ammount,Notes,Active,Created,Updated,UserId")] Cheque ChequeIn)
        {
            ChequeIn.Type = EnumChequeType.ChequeIn;
            ChequeIn.Cashed = false;
            ChequeIn.Active = true;
            ChequeIn.Created = DateTime.Now;
            ChequeIn.Updated = DateTime.Now;
            ChequeIn.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(ChequeIn.Code)) ChequeIn.Code = ChequeIn.Code.ToUpper();
                if (!string.IsNullOrEmpty(ChequeIn.Issued)) ChequeIn.Issued = ChequeIn.Issued.ToUpper();
                if (!string.IsNullOrEmpty(ChequeIn.OnBehalf)) ChequeIn.OnBehalf = ChequeIn.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(ChequeIn.Notes)) ChequeIn.Notes = ChequeIn.Notes.ToUpper();

                db.Cheques.Add(ChequeIn);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ChequeIn, MenuId = ChequeIn.Id, MenuCode = ChequeIn.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", ChequeIn.MasterBusinessUnitId);
            return PartialView("../CashAndBank/ChequeIns/_Create", ChequeIn);
        }

        // GET: ChequeIns/Edit/5
        [Authorize(Roles = "ChequeInsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cheque ChequeIn = db.Cheques.Find(id);
            if (ChequeIn == null)
            {
                return HttpNotFound();
            }
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", ChequeIn.MasterBusinessUnitId);
            return PartialView("../CashAndBank/ChequeIns/_Edit", ChequeIn);
        }

        // POST: ChequeIns/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ChequeInsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,DueDate,Issued,MasterBusinessUnitId,Type,OnBehalf,AccNumber,Ammount,Notes,Active,Created,Updated,UserId")] Cheque ChequeIn)
        {
            ChequeIn.Updated = DateTime.Now;
            ChequeIn.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(ChequeIn.Code)) ChequeIn.Code = ChequeIn.Code.ToUpper();
                if (!string.IsNullOrEmpty(ChequeIn.Issued)) ChequeIn.Issued = ChequeIn.Issued.ToUpper();
                if (!string.IsNullOrEmpty(ChequeIn.OnBehalf)) ChequeIn.OnBehalf = ChequeIn.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(ChequeIn.Notes)) ChequeIn.Notes = ChequeIn.Notes.ToUpper();

                db.Entry(ChequeIn).State = EntityState.Unchanged;
                db.Entry(ChequeIn).Property("Code").IsModified = true;
                db.Entry(ChequeIn).Property("Date").IsModified = true;
                db.Entry(ChequeIn).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(ChequeIn).Property("Issued").IsModified = true;
                db.Entry(ChequeIn).Property("OnBehalf").IsModified = true;
                db.Entry(ChequeIn).Property("DueDate").IsModified = true;
                db.Entry(ChequeIn).Property("AccNumber").IsModified = true;
                db.Entry(ChequeIn).Property("Ammount").IsModified = true;
                db.Entry(ChequeIn).Property("Notes").IsModified = true;
                db.Entry(ChequeIn).Property("Updated").IsModified = true;
                db.Entry(ChequeIn).Property("UserId").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ChequeIn, MenuId = ChequeIn.Id, MenuCode = ChequeIn.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", ChequeIn.MasterBusinessUnitId);
            return PartialView("../CashAndBank/ChequeIns/_Edit", ChequeIn);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ChequeInsDelete")]
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
                        Cheque obj = db.Cheques.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            Cheque tmp = obj;
                            db.Cheques.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ChequeIn, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                            db.SaveChanges();
                        }
                    }
                    return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                }
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ChequeInsEdit")]
        public ActionResult Decline(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cheque ChequeIn = db.Cheques.Find(id);
            if (ChequeIn == null)
            {
                return HttpNotFound();
            }

            if (ChequeIn.Active)
                ChequeIn.Active = false;
            else
                ChequeIn.Active = true;

            db.Entry(ChequeIn).State = EntityState.Modified;
            db.SaveChanges();

            return Json("Giro nomor " + ChequeIn.Code + " berhasil diproses.");
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
