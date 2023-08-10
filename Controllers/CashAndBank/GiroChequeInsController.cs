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
    public class GiroChequeInsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: GiroChequeIns
        [Authorize(Roles = "GiroChequeInsActive")]
        public ActionResult Index()
        {
            return View("../CashAndBank/GiroChequeIns/Index");
        }

        [HttpGet]
        [Authorize(Roles = "GiroChequeInsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../CashAndBank/GiroChequeIns/_IndexGrid", db.Set<GiroCheque>().Where(o => o.Type == EnumGiroChequeType.GiroChequeIn).AsQueryable());
            else
                return PartialView("../CashAndBank/GiroChequeIns/_IndexGrid", db.Set<GiroCheque>().Where(o => o.Type == EnumGiroChequeType.GiroChequeIn).AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "GiroChequeInsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.GiroCheques.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.GiroCheques.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: GiroChequeIns/Details/5
        [Authorize(Roles = "GiroChequeInsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiroCheque GiroCheque = db.GiroCheques.Find(id);
            if (GiroCheque == null)
            {
                return HttpNotFound();
            }
            return PartialView("../CashAndBank/GiroChequeIns/_Details", GiroCheque);
        }

        // GET: GiroChequeIns/Create
        [Authorize(Roles = "GiroChequeInsAdd")]
        public ActionResult Create()
        {
            GiroCheque obj = new GiroCheque
            {
                Active = true,
                Type = EnumGiroChequeType.GiroChequeIn,
                Date = DateTime.Now,
                DueDate = DateTime.Now
            };
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name");
            return PartialView("../CashAndBank/GiroChequeIns/_Create", obj);
        }

        // POST: GiroChequeIns/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GiroChequeInsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,DueDate,Issued,MasterBusinessUnitId,Type,OnBehalf,AccNumber,Ammount,Notes,Active,Created,Updated,UserId")] GiroCheque GiroChequeIn)
        {
            GiroChequeIn.Type = EnumGiroChequeType.GiroChequeIn;
            GiroChequeIn.Cashed = false;
            GiroChequeIn.Active = true;
            GiroChequeIn.Created = DateTime.Now;
            GiroChequeIn.Updated = DateTime.Now;
            GiroChequeIn.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(GiroChequeIn.Code)) GiroChequeIn.Code = GiroChequeIn.Code.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeIn.Issued)) GiroChequeIn.Issued = GiroChequeIn.Issued.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeIn.OnBehalf)) GiroChequeIn.OnBehalf = GiroChequeIn.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeIn.Notes)) GiroChequeIn.Notes = GiroChequeIn.Notes.ToUpper();

                db.GiroCheques.Add(GiroChequeIn);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GiroChequeIn, MenuId = GiroChequeIn.Id, MenuCode = GiroChequeIn.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", GiroChequeIn.MasterBusinessUnitId);
            return PartialView("../CashAndBank/GiroChequeIns/_Create", GiroChequeIn);
        }

        // GET: GiroChequeIns/Edit/5
        [Authorize(Roles = "GiroChequeInsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiroCheque GiroChequeIn = db.GiroCheques.Find(id);
            if (GiroChequeIn == null)
            {
                return HttpNotFound();
            }
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", GiroChequeIn.MasterBusinessUnitId);
            return PartialView("../CashAndBank/GiroChequeIns/_Edit", GiroChequeIn);
        }

        // POST: GiroChequeIns/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GiroChequeInsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,DueDate,Issued,MasterBusinessUnitId,Type,OnBehalf,AccNumber,Ammount,Notes,Active,Created,Updated,UserId")] GiroCheque GiroChequeIn)
        {
            GiroChequeIn.Updated = DateTime.Now;
            GiroChequeIn.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(GiroChequeIn.Code)) GiroChequeIn.Code = GiroChequeIn.Code.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeIn.Issued)) GiroChequeIn.Issued = GiroChequeIn.Issued.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeIn.OnBehalf)) GiroChequeIn.OnBehalf = GiroChequeIn.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeIn.Notes)) GiroChequeIn.Notes = GiroChequeIn.Notes.ToUpper();

                db.Entry(GiroChequeIn).State = EntityState.Unchanged;
                db.Entry(GiroChequeIn).Property("Code").IsModified = true;
                db.Entry(GiroChequeIn).Property("Date").IsModified = true;
                db.Entry(GiroChequeIn).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(GiroChequeIn).Property("Issued").IsModified = true;
                db.Entry(GiroChequeIn).Property("OnBehalf").IsModified = true;
                db.Entry(GiroChequeIn).Property("DueDate").IsModified = true;
                db.Entry(GiroChequeIn).Property("AccNumber").IsModified = true;
                db.Entry(GiroChequeIn).Property("Ammount").IsModified = true;
                db.Entry(GiroChequeIn).Property("Notes").IsModified = true;
                db.Entry(GiroChequeIn).Property("Updated").IsModified = true;
                db.Entry(GiroChequeIn).Property("UserId").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GiroChequeIn, MenuId = GiroChequeIn.Id, MenuCode = GiroChequeIn.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", GiroChequeIn.MasterBusinessUnitId);
            return PartialView("../CashAndBank/GiroChequeIns/_Edit", GiroChequeIn);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "GiroChequeInsDelete")]
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
                        GiroCheque obj = db.GiroCheques.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            GiroCheque tmp = obj;
                            db.GiroCheques.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GiroChequeIn, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                            db.SaveChanges();
                        }
                    }
                    return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                }
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "GiroChequeInsEdit")]
        public ActionResult Decline(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiroCheque GiroChequeIn = db.GiroCheques.Find(id);
            if (GiroChequeIn == null)
            {
                return HttpNotFound();
            }

            if (GiroChequeIn.Active)
                GiroChequeIn.Active = false;
            else
                GiroChequeIn.Active = true;

            db.Entry(GiroChequeIn).State = EntityState.Modified;
            db.SaveChanges();

            return Json("Giro nomor " + GiroChequeIn.Code + " berhasil diproses.");
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
