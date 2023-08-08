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
    public class MasterCurrenciesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterCurrencies
        [Authorize(Roles = "MasterCurrenciesActive")]
        public ActionResult Index()
        {
            return View("../Masters/MasterCurrencies/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterCurrenciesActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Masters/MasterCurrencies/_IndexGrid", db.Set<MasterCurrency>().AsQueryable());
            else
                return PartialView("../Masters/MasterCurrencies/_IndexGrid", db.Set<MasterCurrency>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "MasterCurrenciesActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterCurrencies.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterCurrencies.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterCurrencies/Details/5
        [Authorize(Roles = "MasterCurrenciesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterCurrency MasterCurrency = db.MasterCurrencies.Find(id);
            if (MasterCurrency == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterCurrencies/_Details", MasterCurrency);
        }

        // GET: MasterCurrencies/Create
        [Authorize(Roles = "MasterCurrenciesAdd")]
        public ActionResult Create()
        {
            MasterCurrency obj = new MasterCurrency();
            obj.Active = true;

            return PartialView("../Masters/MasterCurrencies/_Create", obj);
        }

        // POST: MasterCurrencies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCurrenciesAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Notes,Rate,Default,Active,Created,Updated,UserId")] MasterCurrency MasterCurrency)
        {
            if (ModelState.IsValid)
            {
                MasterCurrency.Created = DateTime.Now;
                MasterCurrency.Updated = DateTime.Now;
                MasterCurrency.UserId = User.Identity.GetUserId<int>();
                db.MasterCurrencies.Add(MasterCurrency);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCurrency, MenuId = MasterCurrency.Id, MenuCode = MasterCurrency.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Masters/MasterCurrencies/_Create", MasterCurrency);
        }

        // GET: MasterCurrencies/Edit/5
        [Authorize(Roles = "MasterCurrenciesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterCurrency MasterCurrency = db.MasterCurrencies.Find(id);
            if (MasterCurrency == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterCurrencies/_Edit", MasterCurrency);
        }

        // POST: MasterCurrencies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCurrenciesEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Notes,Rate,Default,Active,Created,Updated,UserId")] MasterCurrency MasterCurrency)
        {
            MasterCurrency.Updated = DateTime.Now;
            MasterCurrency.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                db.Entry(MasterCurrency).State = EntityState.Unchanged;
                db.Entry(MasterCurrency).Property("Code").IsModified = true;
                db.Entry(MasterCurrency).Property("Name").IsModified = true;
                db.Entry(MasterCurrency).Property("Rate").IsModified = true;
                db.Entry(MasterCurrency).Property("Notes").IsModified = true;
                db.Entry(MasterCurrency).Property("Rate").IsModified = true;
                db.Entry(MasterCurrency).Property("Default").IsModified = true;
                db.Entry(MasterCurrency).Property("Active").IsModified = true;
                db.Entry(MasterCurrency).Property("Updated").IsModified = true;
                db.Entry(MasterCurrency).Property("UserId").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCurrency, MenuId = MasterCurrency.Id, MenuCode = MasterCurrency.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Masters/MasterCurrencies/_Edit", MasterCurrency);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterCurrenciesDelete")]
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
                        MasterCurrency obj = db.MasterCurrencies.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            MasterCurrency tmp = obj;
                            db.MasterCurrencies.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCurrency, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
