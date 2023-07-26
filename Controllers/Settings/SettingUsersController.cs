using eShop.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace eShop.Controllers
{
    [Authorize]
    public class SettingUsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: SettingUsers
        [Authorize(Roles = "SettingUsersActive")]
        public ActionResult Index()
        {
            return View("../Settings/SettingUsers/Index");
        }

        [HttpGet]
        [Authorize(Roles = "SettingUsersActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Settings/SettingUsers/_IndexGrid", db.Set<ApplicationUser>().Where(x => x.IsCustomer != true).AsQueryable());
            else
                return PartialView("../Settings/SettingUsers/_IndexGrid", db.Set<ApplicationUser>().Where(x => x.IsCustomer != true).AsQueryable()
                    .Where(x => x.Email.Contains(search) || x.UserName.Contains(search) || x.Email.Contains(search)));
        }

        [HttpGet]
        [Authorize(Roles = "SettingUsersActive")]
        public PartialViewResult CustomerGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Settings/SettingUsers/_CustomerGrid", db.Set<ApplicationUser>().Where(x => x.IsCustomer == true).AsQueryable());
            else
                return PartialView("../Settings/SettingUsers/_CustomerGrid", db.Set<ApplicationUser>().Where(x => x.IsCustomer == true).AsQueryable()
                    .Where(x => x.Email.Contains(search) || x.UserName.Contains(search) || x.Email.Contains(search)));
        }

        // GET: SettingUsers/Details/5
        [Authorize(Roles = "SettingUsersView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            PopulateAssignedBusinessUnit(user);
            return PartialView("../Settings/SettingUsers/_Details", user);
        }

        // GET: SettingUsers/Edit/5
        [Authorize(Roles = "SettingUsersEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users
                .Include(i => i.MasterBusinessUnits)
                .Where(i => i.Id == id)
                .Single();

            PopulateAssignedBusinessUnit(user);

            PopulateAssignedRegion(user);

            if (user == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Settings/SettingUsers/_Edit", user);
        }

        [Authorize(Roles = "SettingUsersActive")]
        private void PopulateAssignedBusinessUnit(ApplicationUser user)
        {
            var allBusinessUnits = db.MasterBusinessUnits;
            var userBusinessUnits = new HashSet<int>(user.MasterBusinessUnits.Select(c => c.Id));
            var viewModel = new List<AssignedBusinessUnit>();
            foreach (var obj in allBusinessUnits)
            {
                viewModel.Add(new AssignedBusinessUnit
                {
                    Id = obj.Id,
                    Title = obj.Code + " - " + obj.Name,
                    Assigned = userBusinessUnits.Contains(obj.Id)
                });
            }
            ViewBag.BusinessUnits = viewModel;
        }

        [Authorize(Roles = "SettingUsersActive")]
        private void PopulateAssignedRegion(ApplicationUser user)
        {
            var allRegions = db.MasterRegions;
            var userRegions = new HashSet<int>(user.MasterRegions.Select(c => c.Id));
            var viewModel = new List<AssignedRegion>();
            foreach (var obj in allRegions)
            {
                viewModel.Add(new AssignedRegion
                {
                    Id = obj.Id,
                    Title = obj.Code,
                    Assigned = userRegions.Contains(obj.Id)
                });
            }
            ViewBag.Regions = viewModel;
        }

        // POST: SettingUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "SettingUsersEdit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? Id, string Email, int? AuthorizationId, int? MasterRegionId, int? MasterCustomerId, int[] selectedBusinessUnits, int[] selectedRegions)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userToUpdate = db.Users
                .Include(i => i.MasterBusinessUnits)
                .Where(i => i.Id == Id)
                .Single();

            try
            {
                UpdateUserBusinessUnits(selectedBusinessUnits, userToUpdate);

                UpdateUserRegions(selectedRegions, userToUpdate);

                Models.Authorization authorization = db.Authorizations.Find(AuthorizationId);
                if (authorization != null)
                {
                    UpdateUserRoles(userToUpdate, authorization);
                }

                userToUpdate.Email = Email;
                userToUpdate.AuthorizationId = AuthorizationId;
                userToUpdate.MasterCustomerId = MasterCustomerId;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterUser, MenuId = userToUpdate.Id, MenuCode = userToUpdate.UserName, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            PopulateAssignedBusinessUnit(userToUpdate);

            PopulateAssignedRegion(userToUpdate);

            return PartialView("../Settings/SettingUsers/_Edit", userToUpdate);
        }

        [Authorize(Roles = "SettingUsersActive")]
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
                    UserManager.RemoveFromRole(user.Id, role.Name);
                if (isAuthorization && !isUserHaveRole)
                    UserManager.AddToRole(user.Id, role.Name);
            }
            db.SaveChanges();
        }

        [Authorize(Roles = "SettingUsersActive")]
        private void UpdateUserBusinessUnits(int[] selectedBusinessUnits, ApplicationUser userToUpdate)
        {
            if (selectedBusinessUnits == null)
            {
                userToUpdate.MasterBusinessUnits = new List<MasterBusinessUnit>();
                return;
            }

            var selectedBusinessUnitsHS = new HashSet<int>(selectedBusinessUnits);
            var userBusinessUnits = new HashSet<int>
                (userToUpdate.MasterBusinessUnits.Select(c => c.Id));
            foreach (var obj in db.MasterBusinessUnits)
            {
                if (selectedBusinessUnitsHS.Contains(obj.Id))
                {
                    if (!userBusinessUnits.Contains(obj.Id))
                    {
                        userToUpdate.MasterBusinessUnits.Add(obj);
                    }
                }
                else
                {
                    if (userBusinessUnits.Contains(obj.Id))
                    {
                        userToUpdate.MasterBusinessUnits.Remove(obj);
                    }
                }
            }
        }

        [Authorize(Roles = "SettingUsersActive")]
        private void UpdateUserRegions(int[] selectedRegions, ApplicationUser userToUpdate)
        {
            if (selectedRegions == null)
            {
                userToUpdate.MasterRegions = new List<MasterRegion>();
                return;
            }

            var selectedRegionsHS = new HashSet<int>(selectedRegions);
            var userRegions = new HashSet<int>
                (userToUpdate.MasterRegions.Select(c => c.Id));
            foreach (var obj in db.MasterRegions)
            {
                if (selectedRegionsHS.Contains(obj.Id))
                {
                    if (!userRegions.Contains(obj.Id))
                    {
                        userToUpdate.MasterRegions.Add(obj);
                    }
                }
                else
                {
                    if (userRegions.Contains(obj.Id))
                    {
                        userToUpdate.MasterRegions.Remove(obj);
                    }
                }
            }
        }

        // GET: SettingUsers/ChangePassword
        [Authorize(Roles = "SettingUsersEdit")]
        public ActionResult ChangePassword(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ApplicationUser user = db.Users.Find(id);
            SettingUsersChangePassword changePassword = new SettingUsersChangePassword();

            if (user == null)
            {
                return HttpNotFound();
            }
            else
            {
                changePassword.Id = user.Id;
                changePassword.UserName = user.UserName;
                changePassword.Email = user.Email;
                changePassword.Password = "";
            }
            return PartialView("../Settings/SettingUsers/_ChangePassword", changePassword);
        }

        // POST: SettingUsers/ChangePassword
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "SettingUsersEdit")]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Include = "Id,UserName,Email,Password")] SettingUsersChangePassword obj)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = db.Users.Find(obj.Id);

                if (user == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    if (user.PasswordHash != null)
                    {
                        UserManager.RemovePassword(user.Id);
                    }

                    UserManager.AddPassword(user.Id, obj.Password);
                }

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterUser, MenuId = obj.Id, MenuCode = obj.UserName, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Settings/SettingUsers/_ChangePassword", obj);
        }

        // GET: SettingUsers/ChangePassword
        [Authorize(Roles = "GroupAuthorizationsEdit")]
        public ActionResult ChangeGroup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ApplicationUser user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Settings/SettingUsers/_ChangeGroup", user);
        }

        // GET: SettingUsers/Delete/5
        [Authorize(Roles = "SettingUsersDelete")]
        public ActionResult Delete(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return PartialView("../Settings/SettingUsers/_Delete", user);
        }

        // POST: SettingUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "SettingUsersDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ApplicationUser user = db.Users.Find(id);
            db.Users.Remove(user);

            try
            {
                db.SaveChanges();
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch { }
            return Json("error", JsonRequestBehavior.AllowGet);
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