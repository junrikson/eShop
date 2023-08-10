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
    public class GiroChequeOutsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: GiroChequeOuts
        [Authorize(Roles = "GiroChequeOutsActive")]
        public ActionResult Index()
        {
            return View("../CashAndBank/GiroChequeOuts/Index");
        }

        [HttpGet]
        [Authorize(Roles = "GiroChequeOutsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../CashAndBank/GiroChequeOuts/_IndexGrid", db.Set<GiroCheque>().Where(o => o.Type == EnumGiroChequeType.GiroChequeOut).AsQueryable());
            else
                return PartialView("../CashAndBank/GiroChequeOuts/_IndexGrid", db.Set<GiroCheque>().Where(o => o.Type == EnumGiroChequeType.GiroChequeOut).AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "GiroChequeOutsActive")]
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

        // GET: GiroChequeOuts/Details/5
        [Authorize(Roles = "GiroChequeOutsView")]
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
            return PartialView("../CashAndBank/GiroChequeOuts/_Details", GiroCheque);
        }

        // GET: GiroChequeOuts/Create
        [Authorize(Roles = "GiroChequeOutsAdd")]
        public ActionResult Create()
        {
            GiroCheque obj = new GiroCheque
            {
                Active = true,
                Type = EnumGiroChequeType.GiroChequeOut,
                Date = DateTime.Now,
                DueDate = DateTime.Now
            };
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name");
            return PartialView("../CashAndBank/GiroChequeOuts/_Create", obj);
        }

        // POST: GiroChequeOuts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GiroChequeOutsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,DueDate,Issued,MasterBusinessUnitId,Type,OnBehalf,AccNumber,Ammount,Notes,Active,Created,Updated,UserId")] GiroCheque GiroChequeOut)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(GiroChequeOut.Code)) GiroChequeOut.Code = GiroChequeOut.Code.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeOut.Issued)) GiroChequeOut.Issued = GiroChequeOut.Issued.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeOut.OnBehalf)) GiroChequeOut.OnBehalf = GiroChequeOut.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeOut.Notes)) GiroChequeOut.Notes = GiroChequeOut.Notes.ToUpper();

                GiroChequeOut.Type = EnumGiroChequeType.GiroChequeOut;
                GiroChequeOut.Created = DateTime.Now;
                GiroChequeOut.Updated = DateTime.Now;
                GiroChequeOut.UserId = User.Identity.GetUserId<int>();
                db.GiroCheques.Add(GiroChequeOut);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GiroChequeOut, MenuId = GiroChequeOut.Id, MenuCode = GiroChequeOut.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", GiroChequeOut.MasterBusinessUnitId);
            return PartialView("../CashAndBank/GiroChequeOuts/_Create", GiroChequeOut);
        }

        // GET: GiroChequeOuts/Edit/5
        [Authorize(Roles = "GiroChequeOutsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiroCheque GiroChequeOut = db.GiroCheques.Find(id);
            if (GiroChequeOut == null)
            {
                return HttpNotFound();
            }
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", GiroChequeOut.MasterBusinessUnitId);
            return PartialView("../CashAndBank/GiroChequeOuts/_Edit", GiroChequeOut);
        }

        // POST: GiroChequeOuts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GiroChequeOutsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,DueDate,Issued,MasterBusinessUnitId,Type,OnBehalf,AccNumber,Ammount,Notes,Active,Created,Updated,UserId")] GiroCheque GiroChequeOut)
        {
            GiroChequeOut.Updated = DateTime.Now;
            GiroChequeOut.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(GiroChequeOut.Code)) GiroChequeOut.Code = GiroChequeOut.Code.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeOut.Issued)) GiroChequeOut.Issued = GiroChequeOut.Issued.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeOut.OnBehalf)) GiroChequeOut.OnBehalf = GiroChequeOut.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(GiroChequeOut.Notes)) GiroChequeOut.Notes = GiroChequeOut.Notes.ToUpper();

                db.Entry(GiroChequeOut).State = EntityState.Unchanged;
                db.Entry(GiroChequeOut).Property("Code").IsModified = true;
                db.Entry(GiroChequeOut).Property("Date").IsModified = true;
                db.Entry(GiroChequeOut).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(GiroChequeOut).Property("Issued").IsModified = true;
                db.Entry(GiroChequeOut).Property("OnBehalf").IsModified = true;
                db.Entry(GiroChequeOut).Property("DueDate").IsModified = true;
                db.Entry(GiroChequeOut).Property("AccNumber").IsModified = true;
                db.Entry(GiroChequeOut).Property("Ammount").IsModified = true;
                db.Entry(GiroChequeOut).Property("Notes").IsModified = true;
                db.Entry(GiroChequeOut).Property("Updated").IsModified = true;
                db.Entry(GiroChequeOut).Property("UserId").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GiroChequeOut, MenuId = GiroChequeOut.Id, MenuCode = GiroChequeOut.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", GiroChequeOut.MasterBusinessUnitId);
            return PartialView("../CashAndBank/GiroChequeOuts/_Edit", GiroChequeOut);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "GiroChequeOutsDelete")]
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

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.GiroChequeOut, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                            db.SaveChanges();
                        }
                    }
                    return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                }
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "GiroChequeOutsEdit")]
        public ActionResult Decline(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GiroCheque GiroChequeOut = db.GiroCheques.Find(id);
            if (GiroChequeOut == null)
            {
                return HttpNotFound();
            }

            if (GiroChequeOut.Active)
                GiroChequeOut.Active = false;
            else
                GiroChequeOut.Active = true;

            db.Entry(GiroChequeOut).State = EntityState.Modified;
            db.SaveChanges();

            return Json("Giro nomor " + GiroChequeOut.Code + " berhasil diproses.");
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
