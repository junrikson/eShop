using eShop.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Web.Mvc;

namespace eShop.Controllers
{
    [Authorize]
    public class AuthorizationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private readonly UserManager<ApplicationUser, int> userManager = new UserManager<ApplicationUser, int>(
            new CustomUserStore(new ApplicationDbContext()));

        [Authorize(Roles = "AuthorizationsActive")]
        public ActionResult Index()
        {
            return View("../Settings/Authorizations/Index");
        }

        [HttpGet]
        [Authorize(Roles = "AuthorizationsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Settings/Authorizations/_IndexGrid", db.Set<Models.Authorization>().AsQueryable());
            else
                return PartialView("../Settings/Authorizations/_IndexGrid", db.Set<Models.Authorization>().AsQueryable()
                    .Where(x => x.Code.Contains(search) || x.Notes.Contains(search)));
        }

        [Authorize(Roles = "AuthorizationsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.Authorizations.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.Authorizations.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Authorizations/Details/5
        [Authorize(Roles = "AuthorizationsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.Authorization authorization = db.Authorizations.Find(id);
            if (authorization == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Settings/Authorizations/_Details", authorization);
        }

        [Authorize(Roles = "AuthorizationsAdd")]
        public ActionResult Create()
        {
            Models.Authorization obj = new Models.Authorization
            {
                Active = true
            };
            PopulateAssignedMenus(obj);
            return PartialView("../Settings/Authorizations/_Create", obj);
        }

        // POST: Authorizations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "AuthorizationsAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Notes,Active")] Models.Authorization authorization)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(authorization.Code)) authorization.Code = authorization.Code.ToUpper();
                if (!string.IsNullOrEmpty(authorization.Notes)) authorization.Notes = authorization.Notes.ToUpper();

                db.Authorizations.Add(authorization);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Authorization, MenuId = authorization.Id, MenuCode = authorization.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Settings/Authorizations/_Create", authorization);
        }

        [Authorize(Roles = "AuthorizationsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.Authorization authorization = db.Authorizations
                .Include(i => i.Roles)
                .Where(i => i.Id == id)
                .Single();

            PopulateAssignedMenus(authorization);

            if (authorization == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Settings/Authorizations/_Edit", authorization);
        }

        [Authorize(Roles = "AuthorizationsActive")]
        private void PopulateAssignedMenus(Models.Authorization authorization)
        {
            var authorizationRoles = new HashSet<int>(authorization.Roles.Select(c => c.Id));
            var viewModel = new List<AssignedRoles>();
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());

            var allRoles = db.Roles.ToList();

            foreach (var obj in allRoles)
            {
                viewModel.Add(new AssignedRoles
                {
                    Order = obj.Order,
                    Id = obj.Id,
                    Code = obj.Name,
                    Notes = obj.Notes,
                    Active = obj.Active,
                    Assigned = authorizationRoles.Contains(obj.Id)
                });
            }

            ViewBag.Roles = viewModel.OrderBy(e => e.Order).ToList();
        }

        // POST: SettingUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AuthorizationsEdit")]
        public ActionResult Edit(int? Id, string Code, string Notes, bool Active, int[] selectedMenus)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var authorizationToUpdate = db.Authorizations
                .Include(i => i.Roles)
                .Where(i => i.Id == Id)
                .Single();

            try
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        UpdateAuthorizationMenus(selectedMenus, authorizationToUpdate);

                        if (!string.IsNullOrEmpty(Code)) Code = Code.ToUpper();
                        if (!string.IsNullOrEmpty(Notes)) Notes = Notes.ToUpper();

                        authorizationToUpdate.Code = Code;
                        authorizationToUpdate.Notes = Notes;
                        authorizationToUpdate.Active = Active;
                        db.SaveChanges();

                        var users = db.Users.Where(u => u.AuthorizationId == authorizationToUpdate.Id).ToList();
                        foreach (ApplicationUser user in users)
                        {
                            UpdateUserRoles(user, authorizationToUpdate);
                        }

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Authorization, MenuId = (int)Id, MenuCode = authorizationToUpdate.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            PopulateAssignedMenus(authorizationToUpdate);
            return PartialView("../Settings/Authorizations/_Edit", authorizationToUpdate);
        }

        [Authorize(Roles = "AuthorizationsActive")]
        private void UpdateAuthorizationMenus(int[] selectedRoles, Models.Authorization authorizationToUpdate)
        {
            if (selectedRoles != null)
            {
                var roles = db.Roles.ToList();
                var selectedRolesHS = new HashSet<int>(selectedRoles);
                var userRoles = new HashSet<int>(authorizationToUpdate.Roles.Select(c => c.Id));

                foreach (var obj in roles)
                {
                    if (selectedRolesHS.Contains(obj.Id))
                    {
                        if (!userRoles.Contains(obj.Id))
                        {
                            authorizationToUpdate.Roles.Add(obj);
                        }
                    }
                    else
                    {
                        if (userRoles.Contains(obj.Id))
                        {
                            authorizationToUpdate.Roles.Remove(obj);
                        }
                    }
                }
            }
        }

        [Authorize(Roles = "AuthorizationsActive")]
        private void UpdateUserRoles(ApplicationUser user, Models.Authorization authorization)
        {
            var roles = db.Roles.ToList();
            var userRoles = user.Roles.ToList();
            var authorizationRoles = authorization.Roles.ToList();

            foreach (CustomRole role in roles)
            {
                bool isUserHaveRole = userRoles.Any(c => c.RoleId == role.Id);
                bool isAuthorization = authorizationRoles.Any(c => c.Id == role.Id);

                if (isUserHaveRole && !isAuthorization)
                    userManager.RemoveFromRole(user.Id, role.Name);
                if (isAuthorization && !isUserHaveRole)
                    userManager.AddToRole(user.Id, role.Name);
            }
            db.SaveChanges();
        }

        [HttpPost]
        [Authorize(Roles = "AuthorizationsDelete")]
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
                        Models.Authorization obj = db.Authorizations.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            Models.Authorization tmp = obj;

                            db.Authorizations.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.Authorization, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
