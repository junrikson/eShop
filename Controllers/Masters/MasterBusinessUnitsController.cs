using eShop.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace eShop.Controllers
{
    [Authorize]
    public class MasterBusinessUnitsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterBusinessUnits
        [Authorize(Roles = "MasterBusinessUnitsActive")]
        public ActionResult Index()
        {
            return View("../Masters/MasterBusinessUnits/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterBusinessUnitsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Masters/MasterBusinessUnits/_IndexGrid", db.Set<MasterBusinessUnit>().AsQueryable());
            else
                return PartialView("../Masters/MasterBusinessUnits/_IndexGrid", db.Set<MasterBusinessUnit>().AsQueryable()
                    .Where(x => x.Code.Contains(search) || x.Name.Contains(search)));
        }

        [Authorize(Roles = "MasterBusinessUnitsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterBusinessUnits.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterBusinessUnits.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterBusinessUnits/Details/5
        [Authorize(Roles = "MasterBusinessUnitsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(id);
            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterBusinessUnits/_Details", masterBusinessUnit);
        }

        // GET: MasterBusinessUnits/Create
        [Authorize(Roles = "MasterBusinessUnitsAdd")]
        public ActionResult Create()
        {
            MasterBusinessUnit obj = new MasterBusinessUnit
            {
                Active = true
            };
            return PartialView("../Masters/MasterBusinessUnits/_Create", obj);
        }

        // POST: MasterBusinessUnits/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterBusinessUnitsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Management,Address,Phone1,Phone2,Mobile,Fax,Email,Notes,Active,IsPPN,PPNRate,FCLLoadFee,LCLLoadFee")] MasterBusinessUnit masterBusinessUnit)
        {
            masterBusinessUnit.StartDate = DateTime.Now;
            masterBusinessUnit.IsPPN = false;
            masterBusinessUnit.PPNRate = 0;

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(masterBusinessUnit.Code)) masterBusinessUnit.Code = masterBusinessUnit.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Name)) masterBusinessUnit.Name = masterBusinessUnit.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Management)) masterBusinessUnit.Management = masterBusinessUnit.Management.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Address)) masterBusinessUnit.Address = masterBusinessUnit.Address.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Phone1)) masterBusinessUnit.Phone1 = masterBusinessUnit.Phone1.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Phone2)) masterBusinessUnit.Phone2 = masterBusinessUnit.Phone2.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Mobile)) masterBusinessUnit.Mobile = masterBusinessUnit.Mobile.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Fax)) masterBusinessUnit.Fax = masterBusinessUnit.Fax.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Notes)) masterBusinessUnit.Notes = masterBusinessUnit.Notes.ToUpper();

                db.MasterBusinessUnits.Add(masterBusinessUnit);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnit, MenuId = masterBusinessUnit.Id, MenuCode = masterBusinessUnit.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Masters/MasterBusinessUnits/_Create", masterBusinessUnit);
        }

        // GET: MasterBusinessUnits/Edit/5
        [Authorize(Roles = "MasterBusinessUnitsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(id);
            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterBusinessUnits/_Edit", masterBusinessUnit);
        }

        // POST: MasterBusinessUnits/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterBusinessUnitsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Management,Address,Phone1,Phone2,Mobile,Fax,Email,Notes,StartDate,Active,IsPPN,PPNRate,FCLLoadFee,LCLLoadFee,EmptyLoadFee")] MasterBusinessUnit masterBusinessUnit)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(masterBusinessUnit.Code)) masterBusinessUnit.Code = masterBusinessUnit.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Name)) masterBusinessUnit.Name = masterBusinessUnit.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Management)) masterBusinessUnit.Management = masterBusinessUnit.Management.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Address)) masterBusinessUnit.Address = masterBusinessUnit.Address.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Phone1)) masterBusinessUnit.Phone1 = masterBusinessUnit.Phone1.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Phone2)) masterBusinessUnit.Phone2 = masterBusinessUnit.Phone2.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Mobile)) masterBusinessUnit.Mobile = masterBusinessUnit.Mobile.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Fax)) masterBusinessUnit.Fax = masterBusinessUnit.Fax.ToUpper();
                if (!string.IsNullOrEmpty(masterBusinessUnit.Notes)) masterBusinessUnit.Notes = masterBusinessUnit.Notes.ToUpper();

                db.Entry(masterBusinessUnit).State = EntityState.Unchanged;
                db.Entry(masterBusinessUnit).Property("Code").IsModified = true;
                db.Entry(masterBusinessUnit).Property("Name").IsModified = true;
                db.Entry(masterBusinessUnit).Property("Management").IsModified = true;
                db.Entry(masterBusinessUnit).Property("Address").IsModified = true;
                db.Entry(masterBusinessUnit).Property("Phone1").IsModified = true;
                db.Entry(masterBusinessUnit).Property("Phone2").IsModified = true;
                db.Entry(masterBusinessUnit).Property("Mobile").IsModified = true;
                db.Entry(masterBusinessUnit).Property("Fax").IsModified = true;
                db.Entry(masterBusinessUnit).Property("Email").IsModified = true;
                db.Entry(masterBusinessUnit).Property("Notes").IsModified = true;
                db.Entry(masterBusinessUnit).Property("StartDate").IsModified = true;
                db.Entry(masterBusinessUnit).Property("PPNRate").IsModified = true;
                db.Entry(masterBusinessUnit).Property("IsPPN").IsModified = true;
                db.Entry(masterBusinessUnit).Property("FCLLoadFee").IsModified = true;
                db.Entry(masterBusinessUnit).Property("LCLLoadFee").IsModified = true;
                db.Entry(masterBusinessUnit).Property("EmptyLoadFee").IsModified = true;
                db.Entry(masterBusinessUnit).Property("Active").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnit, MenuId = masterBusinessUnit.Id, MenuCode = masterBusinessUnit.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Masters/MasterBusinessUnits/_Edit", masterBusinessUnit);
        }

        [HttpPost]
        [Authorize(Roles = "MasterBusinessUnitsDelete")]
        [ValidateJsonAntiForgeryToken]
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
                        MasterBusinessUnit obj = db.MasterBusinessUnits.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnit tmp = obj;

                                    db.MasterBusinessUnits.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnit, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                                    db.SaveChanges();

                                    dbTran.Commit();
                                }
                                catch (DbEntityValidationException ex)
                                {
                                    dbTran.Rollback();
                                    throw ex;
                                }
                            }
                        }
                    }
                    return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                }
            }
        }

        [HttpGet]
        [Authorize(Roles = "MasterBusinessUnitsEdit")]
        public PartialViewResult AccountsGrid(int Id)
        {
            return PartialView("../Masters/MasterBusinessUnits/_AccountsGrid", db.MasterBusinessUnitsAccounts
                .Where(x => x.MasterBusinessUnitId == Id).ToList());
        }

        [Authorize(Roles = "MasterBusinessUnitsEdit")]
        public ActionResult AccountsCreate(int masterBusinessUnitId)
        {
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }
            MasterBusinessUnitAccount masterBusinessUnitAccount = new MasterBusinessUnitAccount
            {
                MasterBusinessUnitId = masterBusinessUnitId
            };
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Code");
            return PartialView("../Masters/MasterBusinessUnits/_AccountsCreate", masterBusinessUnitAccount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterBusinessUnitsEdit")]
        public ActionResult AccountsCreate([Bind(Include = "MasterBusinessUnitId,Type,MasterRegionId,ChartOfAccountId")] MasterBusinessUnitAccount masterBusinessUnitAccount)
        {
            if (ModelState.IsValid)
            {
                db.MasterBusinessUnitsAccounts.Add(masterBusinessUnitAccount);
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Code", masterBusinessUnitAccount.MasterRegionId);
            return PartialView("../Masters/MasterBusinessUnits/_AccountsCreate", masterBusinessUnitAccount);
        }

        [Authorize(Roles = "MasterBusinessUnitsEdit")]
        public ActionResult AccountsEdit(int masterBusinessUnitId, int masterRegionId, EnumBusinessUnitAccountType type)
        {
            MasterBusinessUnitAccount obj = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId && x.Type == type).FirstOrDefault();
            if (obj == null)
            {
                return HttpNotFound();
            }
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Code", obj.MasterRegionId);
            return PartialView("../Masters/MasterBusinessUnits/_AccountsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterBusinessUnitsEdit")]
        public ActionResult AccountsEdit([Bind(Include = "Id,MasterBusinessUnitId,Type,MasterRegionId,ChartOfAccountId")] MasterBusinessUnitAccount masterBusinessUnitAccount)
        {
            if (ModelState.IsValid)
            {
                db.Entry(masterBusinessUnitAccount).State = EntityState.Unchanged;
                db.Entry(masterBusinessUnitAccount).Property("Type").IsModified = true;
                db.Entry(masterBusinessUnitAccount).Property("MasterRegionId").IsModified = true;
                db.Entry(masterBusinessUnitAccount).Property("ChartOfAccountId").IsModified = true;
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterRegionId = new SelectList(user.MasterRegions, "Id", "Code", masterBusinessUnitAccount.MasterRegionId);
            return PartialView("../Masters/MasterBusinessUnits/_AccountsEdit", masterBusinessUnitAccount);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterBusinessUnitsEdit")]
        public ActionResult AccountsBatchDelete(string[] ids)
        {
            if (ids == null || ids.Length <= 0)
                return Json("Pilih salah satu data yang akan dihapus.");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (string id in ids)
                    {
                        string[] vars = id.Split('_');
                        int masterBusinessUnitId = int.Parse(vars[0]);
                        int masterRegionId = int.Parse(vars[1]);
                        Enum.TryParse(vars[2], out EnumBusinessUnitAccountType enumBusinessUnitAccountType);

                        MasterBusinessUnitAccount obj = db.MasterBusinessUnitsAccounts.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId && x.Type == enumBusinessUnitAccountType).FirstOrDefault();
                        if (obj == null)
                            failed++;
                        else
                        {
                            db.MasterBusinessUnitsAccounts.Remove(obj);
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
