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
    public class OpeningBallanceController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: OpeningBallance
        [Authorize(Roles = "OpeningBallanceActive")]
        public ActionResult Index()
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            OpeningBallanceViewModel openingBallanceViewModel = new OpeningBallanceViewModel
            {
                MasterBusinessUnitId = user.MasterBusinessUnitRegions.Select(x => x.MasterBusinessUnitId).Distinct().FirstOrDefault()
            };

            ViewBag.MasterBusinessUnitId = new SelectList(user.MasterBusinessUnitRegions.Select(x => x.MasterBusinessUnitId).Distinct(), "Id", "Name", openingBallanceViewModel.MasterBusinessUnitId);
            return View("../Accounting/OpeningBallance/Index");
        }

        [HttpGet]
        [Authorize(Roles = "OpeningBallanceActive")]
        public PartialViewResult IndexGrid()
        {
                return PartialView("../Accounting/OpeningBallance/_IndexGrid", db.Set<AccountBallance>().AsQueryable());
        }

        // GET: OpeningBallance/Edit/5
        [Authorize(Roles = "OpeningBallanceEdit")]
        public ActionResult Edit(string id)
        {
            if (id == "" || id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string[] ids = id.Split('_');
            int chartOfAccountId = Int32.Parse(ids[0]);
            int month = Int32.Parse(ids[1]);
            int year = Int32.Parse(ids[2]);

            AccountBallance accountBallance = db.AccountBallances.Where(x => x.ChartOfAccountId == chartOfAccountId && x.Month == month && x.Year == year).FirstOrDefault();
            if (accountBallance == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Accounting/OpeningBallance/_Edit", accountBallance);
        }

        // POST: OpeningBallance/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "OpeningBallanceEdit")]
        public ActionResult Edit([Bind(Include = "ChartOfAccountId,Month,Year,BeginningBallance")] AccountBallance accountBallance)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Entry(accountBallance).State = EntityState.Modified;
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

            return PartialView("../Accounting/OpeningBallance/_Edit", accountBallance);
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
