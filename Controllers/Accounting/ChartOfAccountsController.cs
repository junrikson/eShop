﻿using eShop.Models;
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
    public class ChartOfAccountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ChartOfAccounts
        [Authorize(Roles = "ChartOfAccountsActive")]
        public ActionResult Index()
        {
            return View("../Accounting/ChartOfAccounts/Index");
        }

        [HttpGet]
        [Authorize(Roles = "ChartOfAccountsActive")]
        public PartialViewResult IndexGrid()
        {
            return PartialView("../Accounting/ChartOfAccounts/_IndexGrid", db.Set<ChartOfAccount>().Where(o => o.Active).AsQueryable());
        }

        [HttpGet]
        [Authorize(Roles = "ChartOfAccountsActive")]
        public PartialViewResult OtherGrid()
        {
            return PartialView("../Accounting/ChartOfAccounts/_OtherGrid", db.Set<ChartOfAccount>().Where(o => !o.Active).AsQueryable());
        }

        [Authorize(Roles = "ChartOfAccountsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.ChartOfAccounts.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.ChartOfAccounts.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ChartOfAccounts/Details/5
        [Authorize(Roles = "ChartOfAccountsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChartOfAccount chartOfAccount = db.ChartOfAccounts.Find(id);
            if (chartOfAccount == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Accounting/ChartOfAccounts/_Details", chartOfAccount);
        }

        // GET: ChartOfAccounts/Create
        [Authorize(Roles = "ChartOfAccountsAdd")]
        public ActionResult Create()
        {
            ChartOfAccount obj = new ChartOfAccount
            {
                Active = true,
                IsHeader = false,
            };

            return PartialView("../Accounting/ChartOfAccounts/_Create", obj);
        }

        // POST: ChartOfAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ChartOfAccountsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,IsHeader,SubChartOfAccountId,AccountTypeId,Notes,Active,Created,Updated,UserId")] ChartOfAccount chartOfAccount)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(chartOfAccount.Code)) chartOfAccount.Code = chartOfAccount.Code.ToUpper();
                if (!string.IsNullOrEmpty(chartOfAccount.Name)) chartOfAccount.Name = chartOfAccount.Name.ToUpper();
                if (!string.IsNullOrEmpty(chartOfAccount.Notes)) chartOfAccount.Notes = chartOfAccount.Notes.ToUpper();

                chartOfAccount.Created = DateTime.Now;
                chartOfAccount.Updated = DateTime.Now;
                chartOfAccount.UserId = User.Identity.GetUserId<int>();
                db.ChartOfAccounts.Add(chartOfAccount);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ChartOfAccount, MenuId = chartOfAccount.Id, MenuCode = chartOfAccount.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Accounting/ChartOfAccounts/_Create", chartOfAccount);
        }

        // GET: ChartOfAccounts/Edit/5
        [Authorize(Roles = "ChartOfAccountsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChartOfAccount chartOfAccount = db.ChartOfAccounts.Find(id);
            if (chartOfAccount == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Accounting/ChartOfAccounts/_Edit", chartOfAccount);
        }

        // POST: ChartOfAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ChartOfAccountsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,IsHeader,SubChartOfAccountId,AccountTypeId,Notes,Active,Created,Updated,UserId")] ChartOfAccount chartOfAccount)
        {
            chartOfAccount.Updated = DateTime.Now;
            chartOfAccount.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(chartOfAccount.Code)) chartOfAccount.Code = chartOfAccount.Code.ToUpper();
                if (!string.IsNullOrEmpty(chartOfAccount.Name)) chartOfAccount.Name = chartOfAccount.Name.ToUpper();
                if (!string.IsNullOrEmpty(chartOfAccount.Notes)) chartOfAccount.Notes = chartOfAccount.Notes.ToUpper();

                db.Entry(chartOfAccount).State = EntityState.Unchanged;
                db.Entry(chartOfAccount).Property("Code").IsModified = true;
                db.Entry(chartOfAccount).Property("Name").IsModified = true;
                db.Entry(chartOfAccount).Property("IsHeader").IsModified = true;
                db.Entry(chartOfAccount).Property("SubChartOfAccountId").IsModified = true;
                db.Entry(chartOfAccount).Property("AccountTypeId").IsModified = true;
                db.Entry(chartOfAccount).Property("Notes").IsModified = true;
                db.Entry(chartOfAccount).Property("Active").IsModified = true;
                db.Entry(chartOfAccount).Property("Updated").IsModified = true;
                db.Entry(chartOfAccount).Property("UserId").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ChartOfAccount, MenuId = chartOfAccount.Id, MenuCode = chartOfAccount.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Accounting/ChartOfAccounts/_Edit", chartOfAccount);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "ChartOfAccountsDelete")]
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
                        ChartOfAccount obj = db.ChartOfAccounts.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            ChartOfAccount tmp = obj;
                            db.ChartOfAccounts.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.ChartOfAccount, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
