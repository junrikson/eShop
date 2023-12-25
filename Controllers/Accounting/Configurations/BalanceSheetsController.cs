using eShop.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace eShop.Controllers
{
    public class BalanceSheetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AccountTypes
        [Authorize(Roles = "BalanceSheetsActive")]
        public ActionResult Index()
        {
            return View("../Accounting/Configurations/BalanceSheets/Index");
        }

        [HttpGet]
        [Authorize(Roles = "BalanceSheetsActive")]
        public PartialViewResult IndexGrid()
        {
            return PartialView("../Accounting/Configurations/BalanceSheets/_IndexGrid", db.Set<BalanceSheet>().AsQueryable());
        }

        [Authorize(Roles = "BalanceSheetsActive")]
        public JsonResult IsCodeExists(int Id)
        {
            {
                return Json(!db.BalanceSheets.Any(x => x.Id == Id), JsonRequestBehavior.AllowGet);
            }

        }

        // GET: AccountTypes/Details/5
        [Authorize(Roles = "BalanceSheetsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BalanceSheet BalanceSheet = db.BalanceSheets.Find(id);
            if (BalanceSheet == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Accounting/Configurations/BalanceSheets/_Details", BalanceSheet);
        }

        // GET: AccountTypes/Create
        [Authorize(Roles = "BalanceSheetsAdd")]
        public ActionResult Create()
        {
            BalanceSheet obj = new BalanceSheet
            {

            };

            return PartialView("../Accounting/Configurations/BalanceSheets/_Create", obj);
        }

        // POST: AccountTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BalanceSheetsAdd")]
        public ActionResult Create([Bind(Include = "Id")] BalanceSheet BalanceSheet)
        {
            if (ModelState.IsValid)
            {
                BalanceSheet.Id = 0;

                db.BalanceSheets.Add(BalanceSheet);
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Accounting/Configurations/BalanceSheets/_Create", BalanceSheet);
        }

        // GET: AccountTypes/Edit/5
        [Authorize(Roles = "BalanceSheetsEdit")]
        public ActionResult Edit(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BalanceSheet BalanceSheet = db.BalanceSheets.Find(Id);
            if (BalanceSheet == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Accounting/Configurations/BalanceSheets/_Edit", BalanceSheet);
        }

        // POST: AccountTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BalanceSheetsEdit")]
        public ActionResult Edit([Bind(Include = "Id")] BalanceSheet BalanceSheet)
        {

            if (ModelState.IsValid)
            {

                db.Entry(BalanceSheet).State = EntityState.Unchanged;
                db.Entry(BalanceSheet).Property("Id").IsModified = true;

                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Accounting/Configurations/BalanceSheets/_Edit", BalanceSheet);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "BalanceSheetsDelete")]
        public ActionResult BatchDelete(int[] ids)
        {
            if (ids == null || ids.Length <= 0)
                return Json("Pilih salah satu data yang akan dihapus.");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (int AccountTypeId in ids)
                    {
                        BalanceSheet obj = db.BalanceSheets.Find(AccountTypeId);
                        if (obj == null)
                            failed++;
                        else
                        {
                            BalanceSheet tmp = obj;
                            db.BalanceSheets.Remove(obj);
                            db.SaveChanges();
                        }
                    }
                    return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                }
            }
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
