using eShop.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace eShop.Controllers
{
    [Authorize]
    public class SystemLogsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SystemLogs
        [Authorize(Roles = "SystemLogsActive")]
        public ActionResult Index()
        {
            return View("../System/SystemLogs/Index");
        }

        [HttpGet]
        [Authorize(Roles = "SystemLogsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../System/SystemLogs/_IndexGrid", db.Set<SystemLog>().AsQueryable());
            else
                return PartialView("../System/SystemLogs/_IndexGrid", db.Set<SystemLog>().AsQueryable()
                    .Where(x => x.MenuCode.Contains(search)));
        }

        // GET: MasterUnits/Create
        [Authorize(Roles = "SystemLogsDelete")]
        public ActionResult Delete()
        {
            List<SelectListItem> deleteRange = new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "Lebih dari 7 hari", Value = "7"
                },
                new SelectListItem
                {
                    Text = "Lebih dari 30 hari", Value = "30"
                },
                new SelectListItem
                {
                    Text = "Lebih dari 90 hari", Value = "90"
                },
                new SelectListItem
                {
                    Text = "Lebih dari 1 tahun", Value = "365"
                },
                new SelectListItem
                {
                    Text = "Hapus semua log", Value = "0"
                }
            };

            ViewBag.DeleteRange = deleteRange;

            return PartialView("../System/SystemLogs/_Delete");
        }


        [HttpPost]
        [Authorize(Roles = "SystemLogsDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string DeleteRange)
        {
            int range = 999999;
            try
            {
                range = int.Parse(DeleteRange);

                if (range >= 0)
                {
                    using (db)
                    {
                        db.SystemLogs.RemoveRange(db.SystemLogs.Where(x => x.Date <= (DbFunctions.AddDays(DateTime.Now, range * -1))));
                        db.SaveChanges();
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (InvalidCastException e)
            {
                return Json(e, JsonRequestBehavior.AllowGet);
            }

            return PartialView("../System/SystemLogs/_Delete");
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
