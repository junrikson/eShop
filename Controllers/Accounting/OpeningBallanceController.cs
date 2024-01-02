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
    public class OpeningBalanceController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: OpeningBalance
        [Authorize(Roles = "OpeningBalanceActive")]
        public ActionResult Index()
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            return View("../Accounting/OpeningBalance/Index");
        }

        [HttpGet]
        [Authorize(Roles = "OpeningBalanceActive")]
        public PartialViewResult IndexGrid(int masterBusinessUnitId = 0, int masterRegionId = 0)
        {
                return PartialView("../Accounting/OpeningBalance/_IndexGrid", db.Set<OpeningBalance>().Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId).AsQueryable());
        }

        // GET: OpeningBalance/Edit/5
        [Authorize(Roles = "OpeningBalanceEdit")]
        public ActionResult Edit(string id)
        {
            if (id == "" || id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string[] ids = id.Split('_');
            int chartOfAccountId = Int32.Parse(ids[0]);
            int masterBusinessUnitId = Int32.Parse(ids[1]);
            int masterRegionId = Int32.Parse(ids[2]);

            OpeningBalance obj = db.OpeningBalances.Where(x => x.ChartOfAccountId == chartOfAccountId && x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId).FirstOrDefault();
            if (obj == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Accounting/OpeningBalance/_Edit", obj);
        }

        // POST: OpeningBalance/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "OpeningBalanceEdit")]
        public ActionResult Edit([Bind(Include = "ChartOfAccountId,MasterBusinessUnitId,MasterRegionId,Total")] OpeningBalance obj)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Entry(obj).State = EntityState.Modified;
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

            return PartialView("../Accounting/OpeningBalance/_Edit", obj);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "OpeningBalanceActive")]
        public JsonResult Generate(int masterBusinessUnitId, int masterRegionId)
        {
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);
            if (!masterBusinessUnit.IsStarted)
            {
                masterBusinessUnit.IsStarted = true;

                db.Entry(masterBusinessUnit).State = EntityState.Modified;
                db.SaveChanges();
            }

            var objs = db.MasterBusinessRegionAccounts.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId).ToList();

            foreach (MasterBusinessRegionAccount account in objs)
            {
                if (account.ChartOfAccount.IsHeader == false)
                {
                    OpeningBalance temp = db.OpeningBalances.Where(x => x.ChartOfAccountId == account.ChartOfAccountId && x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId).FirstOrDefault();

                    if (temp == null)
                    {
                        OpeningBalance OpeningBalance = new OpeningBalance
                        {
                            MasterBusinessUnitId = masterBusinessUnitId,
                            MasterRegionId = masterRegionId,
                            ChartOfAccountId = account.ChartOfAccountId,
                            Total = 0
                        };

                        db.OpeningBalances.Add(OpeningBalance);
                        db.SaveChanges();
                    }
                }
            }

            return Json("success");
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
