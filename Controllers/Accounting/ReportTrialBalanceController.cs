using eShop.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Windows.Controls;


namespace eShop.Controllers
{
    public class ReportTrialBalanceController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterBrands
        [Authorize(Roles = "ReportTrialBalanceActive")]
        public ActionResult Index()
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.Year = new SelectList(db.Set<AccountBalance>().Select(x => x.Year).Distinct());
            ViewBag.Month = new SelectList(db.Set<AccountBalance>().Select(x => x.Month).Distinct());
            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            ViewBag.MasterRegionId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterRegion).Distinct(), "Id", "Notes");
            return View("../Accounting/Reports/ReportTrialBalance/Index");
        }

        [HttpGet]
        [Authorize(Roles = "ReportTrialBalanceActive")]
        public PartialViewResult IndexGrid(int masterBusinessUnitId = 0, int masterRegionId = 0, int year = 0, int month = 0)
        {
            DateTime startDate = DateTime.Now;
                
            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);

            if(masterBusinessUnit != null)
                startDate = masterBusinessUnit.StartDate;

            return PartialView("../Accounting/Reports/ReportTrialBalance/_IndexGrid", db.Set<AccountBalance>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId
                    && x.Year == year && x.Month == month).Select(x => new TrialBalanceViewModel
                    {
                        Id = x.Id,
                        Year = x.Year,
                        Month = x.Month,
                        MasterBusinessUnit = x.MasterBusinessUnit,
                        MasterBusinessUnitId = x.MasterBusinessUnitId,
                        MasterRegionId = x.MasterRegionId,
                        MasterRegion = x.MasterRegion,
                        ChartOfAccountId = x.ChartOfAccountId,
                        ChartOfAccount = x.ChartOfAccount,
                        BeginningBalance = (db.JournalsDetails.Where(y => y.Journal.MasterBusinessUnitId == x.MasterBusinessUnitId
                                                && y.MasterRegionId == x.MasterRegionId && y.ChartOfAccountId == x.ChartOfAccountId
                                                && y.Journal.Date < new DateTime(year, month, 1) && y.Journal.Date >= startDate).FirstOrDefault() == null ? 0 :
                                                db.JournalsDetails.Where(y => y.Journal.MasterBusinessUnitId == x.MasterBusinessUnitId
                                                && y.MasterRegionId == x.MasterRegionId && y.ChartOfAccountId == x.ChartOfAccountId
                                                && y.Journal.Date < new DateTime(year, month, 1) && y.Journal.Date >= startDate).Sum(z => (z.Debit - z.Credit)))
                                                + (db.OpeningBalances.Where(o => o.MasterBusinessUnitId == x.MasterBusinessUnitId
                                                && o.MasterRegionId == x.MasterRegionId && o.ChartOfAccountId == x.ChartOfAccountId).FirstOrDefault() == null ? 0 :
                                                db.OpeningBalances.Where(o => o.MasterBusinessUnitId == x.MasterBusinessUnitId
                                                && o.MasterRegionId == x.MasterRegionId && o.ChartOfAccountId == x.ChartOfAccountId).FirstOrDefault().Total),
                        Debit = x.Debit,
                        Credit = x.Credit,
                        EndBalance = (db.JournalsDetails.Where(y => y.Journal.MasterBusinessUnitId == x.MasterBusinessUnitId
                                                && y.MasterRegionId == x.MasterRegionId && y.ChartOfAccountId == x.ChartOfAccountId
                                                && y.Journal.Date < new DateTime(year, month, 1) && y.Journal.Date >= startDate).FirstOrDefault() == null ? 0 :
                                                db.JournalsDetails.Where(y => y.Journal.MasterBusinessUnitId == x.MasterBusinessUnitId
                                                && y.MasterRegionId == x.MasterRegionId && y.ChartOfAccountId == x.ChartOfAccountId
                                                && y.Journal.Date < new DateTime(year, month, 1) && y.Journal.Date >= startDate).Sum(z => (z.Debit - z.Credit)))
                                                + (db.OpeningBalances.Where(o => o.MasterBusinessUnitId == x.MasterBusinessUnitId
                                                && o.MasterRegionId == x.MasterRegionId && o.ChartOfAccountId == x.ChartOfAccountId).FirstOrDefault() == null ? 0 :
                                                db.OpeningBalances.Where(o => o.MasterBusinessUnitId == x.MasterBusinessUnitId
                                                && o.MasterRegionId == x.MasterRegionId && o.ChartOfAccountId == x.ChartOfAccountId).FirstOrDefault().Total) + x.Debit - x.Credit,
                    }));
        }

        // GET: MasterBrands/Details/
        [Authorize(Roles = "ReportTrialBalanceActive")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AccountBalance AccountBalance = db.AccountBalances.Find(id);

            if (AccountBalance == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Accounting/Reports/ReportTrialBalance/_Details", AccountBalance);
        }

        [HttpGet]
        [Authorize(Roles = "ReportTrialBalanceActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            AccountBalance AccountBalance = db.AccountBalances.Find(Id);
            
            return PartialView("../Accounting/Reports/ReportTrialBalance/_DetailsGrid", db.Set<JournalDetails>().AsQueryable()
                    .Where(x => x.ChartOfAccount.Id == AccountBalance.ChartOfAccountId
                    && x.Journal.MasterBusinessUnitId == AccountBalance.MasterBusinessUnitId
                    && x.MasterRegionId == AccountBalance.MasterRegionId
                    && x.Journal.Date.Year == AccountBalance.Year
                    && x.Journal.Date.Month == AccountBalance.Month));
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
