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
    public class MasterCashBanksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterCashBanks
        [Authorize(Roles = "MasterCashBanksActive")]
        public ActionResult Index()
        {
            return View("../CashAndBank/MasterCashBanks/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterCashBanksActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../CashAndBank/MasterCashBanks/_IndexGrid", db.Set<MasterCashBank>().AsQueryable());
            else
                return PartialView("../CashAndBank/MasterCashBanks/_IndexGrid", db.Set<MasterCashBank>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "MasterCashBanksActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterCashBanks.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterCashBanks.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterCashBanks/Details/5
        [Authorize(Roles = "MasterCashBanksView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterCashBank masterCashBank = db.MasterCashBanks.Find(id);
            if (masterCashBank == null)
            {
                return HttpNotFound();
            }
            return PartialView("../CashAndBank/MasterCashBanks/_Details", masterCashBank);
        }

        // GET: MasterCashBanks/Create
        [Authorize(Roles = "MasterCashBanksAdd")]
        public ActionResult Create()
        {
            MasterCashBank obj = new MasterCashBank
            {
                Active = true,
                Type = EnumCashBankType.Bank
            };

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name");
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions.Where(x => x.Active == true), "Id", "Notes");
            return PartialView("../CashAndBank/MasterCashBanks/_Create", obj);
        }

        // POST: MasterCashBanks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCashBanksAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,MasterRegionId,MasterBusinessUnitId,ChartOfAccountId,Type,OnBehalf,AccNumber,Notes,Active,Created,Updated,UserId")] MasterCashBank masterCashBank)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(masterCashBank.Code)) masterCashBank.Code = masterCashBank.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterCashBank.Name)) masterCashBank.Name = masterCashBank.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterCashBank.OnBehalf)) masterCashBank.OnBehalf = masterCashBank.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(masterCashBank.Notes)) masterCashBank.Notes = masterCashBank.Notes.ToUpper();

                masterCashBank.Created = DateTime.Now;
                masterCashBank.Updated = DateTime.Now;
                masterCashBank.UserId = User.Identity.GetUserId<int>();
                db.MasterCashBanks.Add(masterCashBank);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCashBank, MenuId = masterCashBank.Id, MenuCode = masterCashBank.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", masterCashBank.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions.Where(x => x.Active == true), "Id", "Notes", masterCashBank.MasterRegionId);
            return PartialView("../CashAndBank/MasterCashBanks/_Create", masterCashBank);
        }

        // GET: MasterCashBanks/Edit/5
        [Authorize(Roles = "MasterCashBanksEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterCashBank masterCashBank = db.MasterCashBanks.Find(id);
            if (masterCashBank == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", masterCashBank.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions.Where(x => x.Active == true), "Id", "Notes", masterCashBank.MasterRegionId);
            return PartialView("../CashAndBank/MasterCashBanks/_Edit", masterCashBank);
        }

        // POST: MasterCashBanks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCashBanksEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,MasterRegionId,MasterBusinessUnitId,ChartOfAccountId,Type,OnBehalf,AccNumber,Notes,Active,Created,Updated,UserId")] MasterCashBank masterCashBank)
        {
            masterCashBank.Updated = DateTime.Now;
            masterCashBank.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(masterCashBank.Code)) masterCashBank.Code = masterCashBank.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterCashBank.Name)) masterCashBank.Name = masterCashBank.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterCashBank.OnBehalf)) masterCashBank.OnBehalf = masterCashBank.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(masterCashBank.Notes)) masterCashBank.Notes = masterCashBank.Notes.ToUpper();

                db.Entry(masterCashBank).State = EntityState.Unchanged;
                db.Entry(masterCashBank).Property("Code").IsModified = true;
                db.Entry(masterCashBank).Property("Name").IsModified = true;
                db.Entry(masterCashBank).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(masterCashBank).Property("MasterRegionId").IsModified = true;
                db.Entry(masterCashBank).Property("ChartOfAccountId").IsModified = true;
                db.Entry(masterCashBank).Property("Type").IsModified = true;
                db.Entry(masterCashBank).Property("OnBehalf").IsModified = true;
                db.Entry(masterCashBank).Property("AccNumber").IsModified = true;
                db.Entry(masterCashBank).Property("Notes").IsModified = true;
                db.Entry(masterCashBank).Property("Active").IsModified = true;
                db.Entry(masterCashBank).Property("Updated").IsModified = true;
                db.Entry(masterCashBank).Property("UserId").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCashBank, MenuId = masterCashBank.Id, MenuCode = masterCashBank.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits.Where(x => x.Active == true), "Id", "Name", masterCashBank.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions.Where(x => x.Active == true), "Id", "Notes", masterCashBank.MasterRegionId);
            return PartialView("../CashAndBank/MasterCashBanks/_Edit", masterCashBank);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterCashBanksDelete")]
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
                        MasterCashBank obj = db.MasterCashBanks.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            MasterCashBank tmp = obj;
                            db.MasterCashBanks.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCashBank, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
