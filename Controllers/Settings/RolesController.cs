using eShop.Models;
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
    public class RolesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "RolesActive")]
        public ActionResult Index()
        {
            return View("../Settings/Roles/Index");
        }

        [HttpGet]
        [Authorize(Roles = "RolesActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Settings/Roles/_IndexGrid", db.Set<CustomRole>().AsQueryable());
            else
                return PartialView("../Settings/Roles/_IndexGrid", db.Set<CustomRole>().AsQueryable()
                    .Where(x => x.Name.Contains(search) || x.Notes.Contains(search)));
        }

        // GET: Menus/Details/5
        [Authorize(Roles = "RolesView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomRole obj = db.Roles.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Settings/Roles/_Details", obj);
        }

        [Authorize(Roles = "RolesAdd")]
        public ActionResult Create()
        {
            CustomRole obj = new CustomRole
            {
                Active = true
            };
            return PartialView("../Settings/Roles/_Create", obj);
        }

        // POST: Menus/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "RolesAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Order,Notes,Active")] CustomRole role)
        {
            if (ModelState.IsValid)
            {
                db.Roles.Add(role);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Role, MenuId = role.Id, MenuCode = role.Name, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Settings/Roles/_Create", role);
        }

        [Authorize(Roles = "RolesEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomRole obj = db.Roles.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Settings/Roles/_Edit", obj);
        }

        // POST: Menus/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "RolesEdit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Order,Notes,Active")] CustomRole role)
        {
            if (ModelState.IsValid)
            {
                db.Entry(role).State = EntityState.Unchanged;
                db.Entry(role).Property("Name").IsModified = true;
                db.Entry(role).Property("Order").IsModified = true;
                db.Entry(role).Property("Notes").IsModified = true;
                db.Entry(role).Property("Active").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Role, MenuId = role.Id, MenuCode = role.Name, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Settings/Roles/_Edit", role);
        }

        [HttpPost]
        [Authorize(Roles = "RolesDelete")]
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
                        CustomRole obj = db.Roles.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            CustomRole tmp = obj;

                            db.Roles.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Role, MenuId = tmp.Id, MenuCode = tmp.Name, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
