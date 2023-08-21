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
    public class MasterCostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterCosts
        [Authorize(Roles = "MasterCostsActive")]
        public ActionResult Index()
        {
            return View("../Masters/MasterCosts/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterCostsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Masters/MasterCosts/_IndexGrid", db.Set<MasterCost>().AsQueryable());
            else
                return PartialView("../Masters/MasterCosts/_IndexGrid", db.Set<MasterCost>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "MasterCostsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterCosts.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterCosts.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterCosts/Details/5
        [Authorize(Roles = "MasterCostsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterCost masterCost = db.MasterCosts.Find(id);
            if (masterCost == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterCosts/_Details", masterCost);
        }

        // GET: MasterCosts/Create
        [Authorize(Roles = "MasterCostsAdd")]
        public ActionResult Create()
        {
            MasterCost obj = new MasterCost
            {
                Active = true
            };

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name");
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions.Where(x => x.Active == true), "Id", "Notes");
            return PartialView("../Masters/MasterCosts/_Create", obj);
        }

        // POST: MasterCosts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCostsAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Name,MasterBusinessUnitId,MasterRegionId,ChartOfAccountId,Notes,Active,Created,Updated,UserId")] MasterCost masterCost)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(masterCost.Code)) masterCost.Code = masterCost.Code.ToUpper();
                        if (!string.IsNullOrEmpty(masterCost.Name)) masterCost.Name = masterCost.Name.ToUpper();
                        if (!string.IsNullOrEmpty(masterCost.Notes)) masterCost.Notes = masterCost.Notes.ToUpper();

                        masterCost.Created = DateTime.Now;
                        masterCost.Updated = DateTime.Now;
                        masterCost.UserId = User.Identity.GetUserId<int>();
                        db.MasterCosts.Add(masterCost);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCost, MenuId = masterCost.Id, MenuCode = masterCost.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", masterCost.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions.Where(x => x.Active == true), "Id", "Notes", masterCost.MasterRegionId);
            return PartialView("../Masters/MasterCosts/_Create", masterCost);
        }

        // GET: MasterCosts/Edit/5
        [Authorize(Roles = "MasterCostsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterCost masterCost = db.MasterCosts.Find(id);
            if (masterCost == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", masterCost.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions.Where(x => x.Active == true), "Id", "Notes", masterCost.MasterRegionId);
            return PartialView("../Masters/MasterCosts/_Edit", masterCost);
        }

        // POST: MasterCosts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCostsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,MasterBusinessUnitId,MasterRegionId,ChartOfAccountId,Notes,Active,Created,Updated,UserId")] MasterCost masterCost)
        {
            masterCost.Updated = DateTime.Now;
            masterCost.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(masterCost.Code)) masterCost.Code = masterCost.Code.ToUpper();
                        if (!string.IsNullOrEmpty(masterCost.Name)) masterCost.Name = masterCost.Name.ToUpper();
                        if (!string.IsNullOrEmpty(masterCost.Notes)) masterCost.Notes = masterCost.Notes.ToUpper();

                        db.Entry(masterCost).State = EntityState.Unchanged;
                        db.Entry(masterCost).Property("Code").IsModified = true;
                        db.Entry(masterCost).Property("Name").IsModified = true;
                        db.Entry(masterCost).Property("MasterBusinessUnitId").IsModified = true;
                        db.Entry(masterCost).Property("MasterRegionId").IsModified = true;
                        db.Entry(masterCost).Property("ChartOfAccountId").IsModified = true;
                        db.Entry(masterCost).Property("Notes").IsModified = true;
                        db.Entry(masterCost).Property("Active").IsModified = true;
                        db.Entry(masterCost).Property("Updated").IsModified = true;
                        db.Entry(masterCost).Property("UserId").IsModified = true;
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCost, MenuId = masterCost.Id, MenuCode = masterCost.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnits, "Id", "Name", masterCost.MasterBusinessUnitId);
            ViewBag.MasterRegionId = new SelectList(user.MasterRegions.Where(x => x.Active == true), "Id", "Notes", masterCost.MasterRegionId);
            return PartialView("../Masters/MasterCosts/_Edit", masterCost);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterCostsDelete")]
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
                        MasterCost obj = db.MasterCosts.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterCost tmp = obj;
                                    db.MasterCosts.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCost, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
