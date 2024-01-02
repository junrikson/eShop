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
    public class ReportStockBalancesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterBrands
        [Authorize(Roles = "ReportStockBalancesActive")]
        public ActionResult Index()
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            ViewBag.MasterBusinessUnitId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterBusinessUnit).Distinct(), "Id", "Name");
            ViewBag.MasterRegionId = new SelectList(user.ApplicationUserMasterBusinessUnitRegions.Select(x => x.MasterRegion).Distinct(), "Id", "Notes");
            return View("../Inventory/Reports/ReportStockBalances/Index");
        }

        [HttpGet]
        [Authorize(Roles = "ReportStockBalancesActive")]
        public PartialViewResult IndexGrid(int masterBusinessUnitId = 0, int masterRegionId = 0)
        {

                return PartialView("../Inventory/Reports/ReportStockBalances/_IndexGrid", db.Set<StockBalance>().Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId).AsQueryable());
        }

        // GET: MasterBrands/Details/
        [Authorize(Roles = "ReportStockBalancesActive")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            StockBalance stockBalance = db.StockBalances.Find(id);

            if (stockBalance == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/Reports/ReportStockBalances/_Details", stockBalance);
        }

        [HttpGet]
        [Authorize(Roles = "ReportStockBalancesActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            StockBalance stockBalance = db.StockBalances.Find(Id);
            
            return PartialView("../Inventory/Reports/ReportStockBalances/_DetailsGrid", db.Set<StockCard>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == stockBalance.MasterBusinessUnitId
                    && x.MasterRegionId == stockBalance.MasterRegionId
                    && x.MasterWarehouseId == stockBalance.MasterWarehouseId
                    && x.MasterItemId == stockBalance.MasterItemId));
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
