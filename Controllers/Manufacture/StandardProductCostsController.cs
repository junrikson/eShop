using eShop.Models;
using eShop.Properties;
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
    public class StandardProductCostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: StandardProductCosts
        [Authorize(Roles = "StandardProductCostsActive")]
        public ActionResult Index()
        {
            return View("../Manufacture/StandardProductCosts/Index");
        }

        [HttpGet]
        [Authorize(Roles = "StandardProductCostsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Manufacture/StandardProductCosts/_IndexGrid", db.Set<StandardProductCost>().AsQueryable());
            else
                return PartialView("../Manufacture/StandardProductCosts/_IndexGrid", db.Set<StandardProductCost>().AsQueryable()
                    .Where(x => x.Code.Contains(search) || x.Name.Contains(search)));
        }

        [Authorize(Roles = "StandardProductCostsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.StandardProductCosts.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.StandardProductCosts.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: StandardProductCosts/Details/5
        [Authorize(Roles = "StandardProductCostsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StandardProductCost standardProductCost = db.StandardProductCosts.Find(id);
            if (standardProductCost == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/StandardProductCosts/_Details", standardProductCost);
        }

        // GET: StandardProductCosts/Create
        [Authorize(Roles = "StandardProductCostsAdd")]
        public ActionResult Create()
        {
            StandardProductCost obj = new StandardProductCost();
            obj.Active = true;

            string code = Settings.Default.StandardProductCostCode + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + "/";
            var lastData = db.StandardProductCosts.Where(x => x.Code.StartsWith(code)).OrderByDescending(x => x.Code).FirstOrDefault();
            if (lastData == null)
                obj.Code = code + "0001";
            else
                obj.Code = code + (Convert.ToInt32(lastData.Code.Substring(lastData.Code.Length - 4, 4)) + 1).ToString("D4");

            return PartialView("../Manufacture/StandardProductCosts/_Create", obj);
        }

        // POST: StandardProductCosts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "StandardProductCostsAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Quantity,price,MasterUnitId,Notes,Active,Created,Updated,UserId")] StandardProductCost standardProductCost)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(standardProductCost.Code)) standardProductCost.Code = standardProductCost.Code.ToUpper();
                if (!string.IsNullOrEmpty(standardProductCost.Name)) standardProductCost.Name = standardProductCost.Name.ToUpper();
                standardProductCost.Quantity = standardProductCost.Quantity;
                standardProductCost.Price = standardProductCost.Price;
                standardProductCost.MasterUnitId = standardProductCost.MasterUnitId;

                standardProductCost.Created = DateTime.Now;
                standardProductCost.Updated = DateTime.Now;
                standardProductCost.UserId = User.Identity.GetUserId<int>();
                db.StandardProductCosts.Add(standardProductCost);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.StandardProductCosts, MenuId = standardProductCost.Id, MenuCode = standardProductCost.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Manufacture/StandardProductCosts/_Create", standardProductCost);
        }

        // GET: StandardProductCosts/Edit/5
        [Authorize(Roles = "StandardProductCostsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StandardProductCost standardProductCost = db.StandardProductCosts.Find(id);
            if (standardProductCost == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Manufacture/StandardProductCosts/_Edit", standardProductCost);
        }

        // POST: StandardProductCosts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "StandardProductCostsEdit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Location,Notes,Active,Created,Updated,UserId")] StandardProductCost standardProductCost)
        {
            standardProductCost.Updated = DateTime.Now;
            standardProductCost.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(standardProductCost.Code)) standardProductCost.Code = standardProductCost.Code.ToUpper();
                if (!string.IsNullOrEmpty(standardProductCost.Name)) standardProductCost.Name = standardProductCost.Name.ToUpper();
                //if (!string.IsNullOrEmpty(standardProductCost.Location)) standardProductCost.Location = standardProductCost.Location.ToUpper();
                if (!string.IsNullOrEmpty(standardProductCost.Notes)) standardProductCost.Notes = standardProductCost.Notes.ToUpper();

                db.Entry(standardProductCost).State = EntityState.Unchanged;
                db.Entry(standardProductCost).Property("Code").IsModified = true;
                db.Entry(standardProductCost).Property("Name").IsModified = true;
                db.Entry(standardProductCost).Property("Location").IsModified = true;
                //db.Entry(standardProductCost).Property("MasterPortId").IsModified = true;
                db.Entry(standardProductCost).Property("Notes").IsModified = true;
                db.Entry(standardProductCost).Property("Active").IsModified = true;
                db.Entry(standardProductCost).Property("Updated").IsModified = true;
                db.Entry(standardProductCost).Property("UserId").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.StandardProductCosts, MenuId = standardProductCost.Id, MenuCode = standardProductCost.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Manufacture/StandardProductCosts/_Edit", standardProductCost);
        }

        [HttpPost]
        [Authorize(Roles = "StandardProductCostsDelete")]
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
                        StandardProductCost obj = db.StandardProductCosts.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            StandardProductCost tmp = obj;

                            db.StandardProductCosts.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.StandardProductCosts, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
