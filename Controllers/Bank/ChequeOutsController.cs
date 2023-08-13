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
    public class ChequeOutsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ChequeOuts
        [Authorize(Roles = "ChequeOutsActive")]
        public ActionResult Index()
        {
            return View("../Bank/ChequeOuts/Index");
        }

        [HttpGet]
        [Authorize(Roles = "ChequeOutsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Bank/ChequeOuts/_IndexGrid", db.Set<Cheque>().Where(o => o.Type == EnumChequeType.ChequeOut).AsQueryable());
            else
                return PartialView("../Bank/ChequeOuts/_IndexGrid", db.Set<Cheque>().Where(o => o.Type == EnumChequeType.ChequeOut).AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "ChequeOutsActive")]
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

        // GET: ChequeOuts/Details/5
        [Authorize(Roles = "ChequeOutsView")]
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
            return PartialView("../Bank/ChequeOuts/_Details", Cheque);
        }

        // GET: ChequeOuts/Create
        [Authorize(Roles = "ChequeOutsAdd")]
        public ActionResult Create()
        {
            Cheque obj = new Cheque
            {
                Active = true,
                Type = EnumChequeType.ChequeOut,
                Date = DateTime.Now,
                DueDate = DateTime.Now
            };
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name");
            return PartialView("../Bank/ChequeOuts/_Create", obj);
        }

        // POST: ChequeOuts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ChequeOutsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Date,DueDate,Issued,MasterBusinessUnitId,Type,OnBehalf,AccNumber,Ammount,Notes,Active,Created,Updated,UserId")] Cheque ChequeOut)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(ChequeOut.Code)) ChequeOut.Code = ChequeOut.Code.ToUpper();
                if (!string.IsNullOrEmpty(ChequeOut.Issued)) ChequeOut.Issued = ChequeOut.Issued.ToUpper();
                if (!string.IsNullOrEmpty(ChequeOut.OnBehalf)) ChequeOut.OnBehalf = ChequeOut.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(ChequeOut.Notes)) ChequeOut.Notes = ChequeOut.Notes.ToUpper();

                ChequeOut.Type = EnumChequeType.ChequeOut;
                ChequeOut.Created = DateTime.Now;
                ChequeOut.Updated = DateTime.Now;
                ChequeOut.UserId = User.Identity.GetUserId<int>();
                db.Cheques.Add(ChequeOut);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ChequeOut, MenuId = ChequeOut.Id, MenuCode = ChequeOut.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", ChequeOut.MasterBusinessUnitId);
            return PartialView("../Bank/ChequeOuts/_Create", ChequeOut);
        }

        // GET: ChequeOuts/Edit/5
        [Authorize(Roles = "ChequeOutsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cheque ChequeOut = db.Cheques.Find(id);
            if (ChequeOut == null)
            {
                return HttpNotFound();
            }
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", ChequeOut.MasterBusinessUnitId);
            return PartialView("../Bank/ChequeOuts/_Edit", ChequeOut);
        }

        // POST: ChequeOuts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ChequeOutsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Date,DueDate,Issued,MasterBusinessUnitId,Type,OnBehalf,AccNumber,Ammount,Notes,Active,Created,Updated,UserId")] Cheque ChequeOut)
        {
            ChequeOut.Updated = DateTime.Now;
            ChequeOut.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(ChequeOut.Code)) ChequeOut.Code = ChequeOut.Code.ToUpper();
                if (!string.IsNullOrEmpty(ChequeOut.Issued)) ChequeOut.Issued = ChequeOut.Issued.ToUpper();
                if (!string.IsNullOrEmpty(ChequeOut.OnBehalf)) ChequeOut.OnBehalf = ChequeOut.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(ChequeOut.Notes)) ChequeOut.Notes = ChequeOut.Notes.ToUpper();

                db.Entry(ChequeOut).State = EntityState.Unchanged;
                db.Entry(ChequeOut).Property("Code").IsModified = true;
                db.Entry(ChequeOut).Property("Date").IsModified = true;
                db.Entry(ChequeOut).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(ChequeOut).Property("Issued").IsModified = true;
                db.Entry(ChequeOut).Property("OnBehalf").IsModified = true;
                db.Entry(ChequeOut).Property("DueDate").IsModified = true;
                db.Entry(ChequeOut).Property("AccNumber").IsModified = true;
                db.Entry(ChequeOut).Property("Ammount").IsModified = true;
                db.Entry(ChequeOut).Property("Notes").IsModified = true;
                db.Entry(ChequeOut).Property("Updated").IsModified = true;
                db.Entry(ChequeOut).Property("UserId").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ChequeOut, MenuId = ChequeOut.Id, MenuCode = ChequeOut.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            ViewBag.MasterBusinessUnitId = new SelectList(db.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", ChequeOut.MasterBusinessUnitId);
            return PartialView("../Bank/ChequeOuts/_Edit", ChequeOut);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ChequeOutsDelete")]
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

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ChequeOut, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                            db.SaveChanges();
                        }
                    }
                    return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                }
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ChequeOutsEdit")]
        public ActionResult Decline(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cheque ChequeOut = db.Cheques.Find(id);
            if (ChequeOut == null)
            {
                return HttpNotFound();
            }

            if (ChequeOut.Active)
                ChequeOut.Active = false;
            else
                ChequeOut.Active = true;

            db.Entry(ChequeOut).State = EntityState.Modified;
            db.SaveChanges();

            return Json("Giro nomor " + ChequeOut.Code + " berhasil diproses.");
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
