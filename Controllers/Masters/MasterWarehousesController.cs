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
    public class MasterWarehousesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterWarehouses
        [Authorize(Roles = "MasterWarehousesActive")]
        public ActionResult Index()
        {
            return View("../Masters/MasterWarehouses/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterWarehousesActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Masters/MasterWarehouses/_IndexGrid", db.Set<MasterWarehouse>().AsQueryable());
            else
                return PartialView("../Masters/MasterWarehouses/_IndexGrid", db.Set<MasterWarehouse>().AsQueryable()
                    .Where(x => x.Code.Contains(search) || x.Name.Contains(search) || x.Location.Contains(search)));
        }

        [Authorize(Roles = "MasterWarehousesActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterWarehouses.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterWarehouses.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterWarehouses/Details/5
        [Authorize(Roles = "MasterWarehousesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterWarehouse masterWarehouse = db.MasterWarehouses.Find(id);
            if (masterWarehouse == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterWarehouses/_Details", masterWarehouse);
        }

        // GET: MasterWarehouses/Create
        [Authorize(Roles = "MasterWarehousesAdd")]
        public ActionResult Create()
        {
            MasterWarehouse obj = new MasterWarehouse();
            obj.Active = true;

            string code = Settings.Default.WarehouseCode + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + "/";
            var lastData = db.MasterWarehouses.Where(x => x.Code.StartsWith(code)).OrderByDescending(x => x.Code).FirstOrDefault();
            if (lastData == null)
                obj.Code = code + "0001";
            else
                obj.Code = code + (Convert.ToInt32(lastData.Code.Substring(lastData.Code.Length - 4, 4)) + 1).ToString("D4");

            return PartialView("../Masters/MasterWarehouses/_Create", obj);
        }

        // POST: MasterWarehouses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "MasterWarehousesAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Name,MasterRegionId,Location,MasterPortId,Notes,Active,Created,Updated,UserId")] MasterWarehouse masterWarehouse)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(masterWarehouse.Code)) masterWarehouse.Code = masterWarehouse.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterWarehouse.Name)) masterWarehouse.Name = masterWarehouse.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterWarehouse.Location)) masterWarehouse.Location = masterWarehouse.Location.ToUpper();
                if (!string.IsNullOrEmpty(masterWarehouse.Notes)) masterWarehouse.Notes = masterWarehouse.Notes.ToUpper();

                masterWarehouse.Created = DateTime.Now;
                masterWarehouse.Updated = DateTime.Now;
                masterWarehouse.UserId = User.Identity.GetUserId<int>();
                db.MasterWarehouses.Add(masterWarehouse);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterWarehouse, MenuId = masterWarehouse.Id, MenuCode = masterWarehouse.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Masters/MasterWarehouses/_Create", masterWarehouse);
        }

        // GET: MasterWarehouses/Edit/5
        [Authorize(Roles = "MasterWarehousesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterWarehouse masterWarehouse = db.MasterWarehouses.Find(id);
            if (masterWarehouse == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterWarehouses/_Edit", masterWarehouse);
        }

        // POST: MasterWarehouses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "MasterWarehousesEdit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,MasterRegionId,Location,MasterPortId,Notes,Active,Created,Updated,UserId")] MasterWarehouse masterWarehouse)
        {
            masterWarehouse.Updated = DateTime.Now;
            masterWarehouse.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(masterWarehouse.Code)) masterWarehouse.Code = masterWarehouse.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterWarehouse.Name)) masterWarehouse.Name = masterWarehouse.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterWarehouse.Location)) masterWarehouse.Location = masterWarehouse.Location.ToUpper();
                if (!string.IsNullOrEmpty(masterWarehouse.Notes)) masterWarehouse.Notes = masterWarehouse.Notes.ToUpper();

                db.Entry(masterWarehouse).State = EntityState.Unchanged;
                db.Entry(masterWarehouse).Property("Code").IsModified = true;
                db.Entry(masterWarehouse).Property("Name").IsModified = true;
                db.Entry(masterWarehouse).Property("MasterRegionId").IsModified = true;
                db.Entry(masterWarehouse).Property("Location").IsModified = true;
                db.Entry(masterWarehouse).Property("MasterPortId").IsModified = true;
                db.Entry(masterWarehouse).Property("Notes").IsModified = true;
                db.Entry(masterWarehouse).Property("Active").IsModified = true;
                db.Entry(masterWarehouse).Property("Updated").IsModified = true;
                db.Entry(masterWarehouse).Property("UserId").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterWarehouse, MenuId = masterWarehouse.Id, MenuCode = masterWarehouse.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Masters/MasterWarehouses/_Edit", masterWarehouse);
        }

        [HttpPost]
        [Authorize(Roles = "MasterWarehousesDelete")]
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
                        MasterWarehouse obj = db.MasterWarehouses.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            MasterWarehouse tmp = obj;

                            db.MasterWarehouses.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterWarehouse, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
