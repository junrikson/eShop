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
    public class AccountTypesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AccountTypes
        [Authorize(Roles = "AccountTypesActive")]
        public ActionResult Index()
        {
            return View("../Accounting/Configurations/AccountTypes/Index");
        }

        [HttpGet]
        [Authorize(Roles = "AccountTypesActive")]
        public PartialViewResult IndexGrid()
        {
            return PartialView("../Accounting/Configurations/AccountTypes/_IndexGrid", db.Set<AccountType>().Where(o => o.Active).AsQueryable());
        }

        [Authorize(Roles = "AccountTypesActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.AccountTypes.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.AccountTypes.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: AccountTypes/Details/5
        [Authorize(Roles = "AccountTypesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountType AccountType = db.AccountTypes.Find(id);
            if (AccountType == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Accounting/Configurations/AccountTypes/_Details", AccountType);
        }

        // GET: AccountTypes/Create
        [Authorize(Roles = "AccountTypesAdd")]
        public ActionResult Create()
        {
            AccountType obj = new AccountType
            {
                Active = true,
                IsHeader = false,
            };

            return PartialView("../Accounting/Configurations/AccountTypes/_Create", obj);
        }

        // POST: AccountTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AccountTypesAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,IsHeader,SubAccountTypeId,Notes,Active,Created,Updated,UserId")] AccountType AccountType)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(AccountType.Code)) AccountType.Code = AccountType.Code.ToUpper();
                if (!string.IsNullOrEmpty(AccountType.Name)) AccountType.Name = AccountType.Name.ToUpper();
                if (!string.IsNullOrEmpty(AccountType.Notes)) AccountType.Notes = AccountType.Notes.ToUpper();

                AccountType.Created = DateTime.Now;
                AccountType.Updated = DateTime.Now;
                AccountType.UserId = User.Identity.GetUserId<int>();
                db.AccountTypes.Add(AccountType);
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Accounting/Configurations/AccountTypes/_Create", AccountType);
        }

        // GET: AccountTypes/Edit/5
        [Authorize(Roles = "AccountTypesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountType AccountType = db.AccountTypes.Find(id);
            if (AccountType == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Accounting/Configurations/AccountTypes/_Edit", AccountType);
        }

        // POST: AccountTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AccountTypesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,IsHeader,SubAccountTypeId,Notes,Active,Created,Updated,UserId")] AccountType AccountType)
        {
            AccountType.Updated = DateTime.Now;
            AccountType.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(AccountType.Code)) AccountType.Code = AccountType.Code.ToUpper();
                if (!string.IsNullOrEmpty(AccountType.Name)) AccountType.Name = AccountType.Name.ToUpper();
                if (!string.IsNullOrEmpty(AccountType.Notes)) AccountType.Notes = AccountType.Notes.ToUpper();

                db.Entry(AccountType).State = EntityState.Unchanged;
                db.Entry(AccountType).Property("Code").IsModified = true;
                db.Entry(AccountType).Property("Name").IsModified = true;
                db.Entry(AccountType).Property("IsHeader").IsModified = true;
                db.Entry(AccountType).Property("SubAccountTypeId").IsModified = true;
                db.Entry(AccountType).Property("Notes").IsModified = true;
                db.Entry(AccountType).Property("Active").IsModified = true;
                db.Entry(AccountType).Property("Updated").IsModified = true;
                db.Entry(AccountType).Property("UserId").IsModified = true;
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Accounting/Configurations/AccountTypes/_Edit", AccountType);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "AccountTypesDelete")]
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
                        AccountType obj = db.AccountTypes.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            AccountType tmp = obj;
                            db.AccountTypes.Remove(obj);
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
