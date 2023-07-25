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
    public class MasterItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterItems
        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult Index()
        {
            return View("../Inventory/MasterItems/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterItemsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Inventory/MasterItems/_IndexGrid", db.Set<MasterItem>().AsQueryable());
            else
                return PartialView("../Inventory/MasterItems/_IndexGrid", db.Set<MasterItem>().AsQueryable().Where(y => y.Code.Contains(search) || y.Name.Contains(search)));
        }

        [Authorize(Roles = "MasterItemsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterItems.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterItems.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "MasterItemsActive")]
        public JsonResult IsNameExists(string Name, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterItems.Any(x => x.Name == Name), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterItems.Any(x => x.Name == Name && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterItems/Details/5
        [Authorize(Roles = "MasterItemsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterItem MasterItem = db.MasterItems.Find(id);
            if (MasterItem == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/MasterItems/_Details", MasterItem);
        }

        // GET: MasterItems/Create
        [Authorize(Roles = "MasterItemsAdd")]
        public ActionResult Create()
        {
            MasterItem MasterItem = new MasterItem();
            MasterItem.Active = true;

            string code = Settings.Default.CustomerCode + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + "/";
            var lastData = db.MasterItems.Where(x => x.Code.StartsWith(code)).OrderByDescending(x => x.Code).FirstOrDefault();
            if (lastData == null)
                MasterItem.Code = code + "0001";
            else
                MasterItem.Code = code + (Convert.ToInt32(lastData.Code.Substring(lastData.Code.Length - 4, 4)) + 1).ToString("D4");

            return PartialView("../Inventory/MasterItems/_Create", MasterItem);
        }

        // POST: MasterItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "MasterItemsAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Notes,Active,Created,Updated,UserId")] MasterItem MasterItem)
        {
            MasterItem.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(MasterItem.Code)) MasterItem.Code = MasterItem.Code.ToUpper();
                if (!string.IsNullOrEmpty(MasterItem.Name)) MasterItem.Name = MasterItem.Name.ToUpper();
                if (!string.IsNullOrEmpty(MasterItem.Notes)) MasterItem.Notes = MasterItem.Notes.ToUpper();

                MasterItem.Created = DateTime.Now;
                MasterItem.Updated = DateTime.Now;
                db.MasterItems.Add(MasterItem);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItem, MenuId = MasterItem.Id, MenuCode = MasterItem.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Inventory/MasterItems/_Create", MasterItem);
        }

        // GET: MasterItems/Edit/5
        [Authorize(Roles = "MasterItemsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterItem MasterItem = db.MasterItems.Find(id);
            if (MasterItem == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/MasterItems/_Edit", MasterItem);
        }

        // POST: MasterItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Notes,Active,Created,Updated,UserId")] MasterItem MasterItem)
        {
            MasterItem.Updated = DateTime.Now;
            MasterItem.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(MasterItem.Code)) MasterItem.Code = MasterItem.Code.ToUpper();
                if (!string.IsNullOrEmpty(MasterItem.Name)) MasterItem.Name = MasterItem.Name.ToUpper();
                if (!string.IsNullOrEmpty(MasterItem.Notes)) MasterItem.Notes = MasterItem.Notes.ToUpper();

                db.Entry(MasterItem).State = EntityState.Unchanged;
                db.Entry(MasterItem).Property("Code").IsModified = true;
                db.Entry(MasterItem).Property("Name").IsModified = true;
                db.Entry(MasterItem).Property("Notes").IsModified = true;
                db.Entry(MasterItem).Property("Active").IsModified = true;
                db.Entry(MasterItem).Property("Updated").IsModified = true;
                db.Entry(MasterItem).Property("UserId").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItem, MenuId = MasterItem.Id, MenuCode = MasterItem.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Inventory/MasterItems/_Edit", MasterItem);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterItemsDelete")]
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
                        MasterItem obj = db.MasterItems.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            MasterItem tmp = obj;
                            db.MasterItems.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItem, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
