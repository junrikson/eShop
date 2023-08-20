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
    public class MasterBanksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterBanks
        [Authorize(Roles = "MasterBanksActive")]
        public ActionResult Index()
        {
            return View("../Bank/MasterBanks/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterBanksActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Bank/MasterBanks/_IndexGrid", db.Set<MasterBank>().AsQueryable());
            else
                return PartialView("../Bank/MasterBanks/_IndexGrid", db.Set<MasterBank>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "MasterBanksActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterBanks.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterBanks.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterBanks/Details/5
        [Authorize(Roles = "MasterBanksView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterBank masterBank = db.MasterBanks.Find(id);
            if (masterBank == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Bank/MasterBanks/_Details", masterBank);
        }

        // GET: MasterBanks/Create
        [Authorize(Roles = "MasterBanksAdd")]
        public ActionResult Create()
        {
            MasterBank obj = new MasterBank
            {
                Active = true,
                Type = EnumBankType.Bank
            };

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            return PartialView("../Bank/MasterBanks/_Create", obj);
        }

        // POST: MasterBanks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterBanksAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,MasterRegionId,MasterBusinessUnitId,ChartOfAccountId,Type,OnBehalf,AccNumber,Notes,Active,Created,Updated,UserId")] MasterBank masterBank)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(masterBank.Code)) masterBank.Code = masterBank.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterBank.Name)) masterBank.Name = masterBank.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterBank.OnBehalf)) masterBank.OnBehalf = masterBank.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(masterBank.Notes)) masterBank.Notes = masterBank.Notes.ToUpper();

                masterBank.Created = DateTime.Now;
                masterBank.Updated = DateTime.Now;
                masterBank.UserId = User.Identity.GetUserId<int>();
                db.MasterBanks.Add(masterBank);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBank, MenuId = masterBank.Id, MenuCode = masterBank.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", masterBank.MasterBusinessUnitId);
            return PartialView("../Bank/MasterBanks/_Create", masterBank);
        }

        // GET: MasterBanks/Edit/5
        [Authorize(Roles = "MasterBanksEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterBank masterBank = db.MasterBanks.Find(id);
            if (masterBank == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", masterBank.MasterBusinessUnitId);
            return PartialView("../Bank/MasterBanks/_Edit", masterBank);
        }

        // POST: MasterBanks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterBanksEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,MasterRegionId,MasterBusinessUnitId,ChartOfAccountId,Type,OnBehalf,AccNumber,Notes,Active,Created,Updated,UserId")] MasterBank masterBank)
        {
            masterBank.Updated = DateTime.Now;
            masterBank.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(masterBank.Code)) masterBank.Code = masterBank.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterBank.Name)) masterBank.Name = masterBank.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterBank.OnBehalf)) masterBank.OnBehalf = masterBank.OnBehalf.ToUpper();
                if (!string.IsNullOrEmpty(masterBank.Notes)) masterBank.Notes = masterBank.Notes.ToUpper();

                db.Entry(masterBank).State = EntityState.Unchanged;
                db.Entry(masterBank).Property("Code").IsModified = true;
                db.Entry(masterBank).Property("Name").IsModified = true;
                db.Entry(masterBank).Property("MasterBusinessUnitId").IsModified = true;
                db.Entry(masterBank).Property("MasterRegionId").IsModified = true;
                db.Entry(masterBank).Property("ChartOfAccountId").IsModified = true;
                db.Entry(masterBank).Property("Type").IsModified = true;
                db.Entry(masterBank).Property("OnBehalf").IsModified = true;
                db.Entry(masterBank).Property("AccNumber").IsModified = true;
                db.Entry(masterBank).Property("Notes").IsModified = true;
                db.Entry(masterBank).Property("Active").IsModified = true;
                db.Entry(masterBank).Property("Updated").IsModified = true;
                db.Entry(masterBank).Property("UserId").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBank, MenuId = masterBank.Id, MenuCode = masterBank.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name", masterBank.MasterBusinessUnitId);
            return PartialView("../Bank/MasterBanks/_Edit", masterBank);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterBanksDelete")]
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
                        MasterBank obj = db.MasterBanks.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            MasterBank tmp = obj;
                            db.MasterBanks.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBank, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
