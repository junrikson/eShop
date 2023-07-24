using eShop.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace eShop.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        public ActionResult Index()
        {
            int? id = User.Identity.GetUserId<int>();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser obj = db.Users.Find(id);
            if (obj == null)
            {
                return HttpNotFound();
            }

            ViewBag.Logs = db.SystemLogs.Where(x => x.UserId == (int)id
            && x.Date.Year == DateTime.Now.Year
            && x.Date.Month == DateTime.Now.Month
            && x.Date.Day == DateTime.Now.Day).OrderByDescending(y => y.Date).ToList();
            ViewBag.Date = DateTime.Now.ToString("dd MMMM yyyy");
            return View(obj);
        }

        public ActionResult ChangePassword()
        {
            return PartialView("_ChangePassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string UserName, string FullName, DateTime? BirthDay, string Email)
        {
            int? id = User.Identity.GetUserId<int>();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser obj = db.Users.Find(id);

            obj.UserName = UserName.ToUpper();
            obj.FullName = FullName;
            obj.BirthDay = BirthDay;
            obj.Email = Email;
            db.Entry(obj).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.Logs = db.SystemLogs.Where(x => x.UserId == (int)id
            && x.Date.Year == DateTime.Now.Year
            && x.Date.Month == DateTime.Now.Month
            && x.Date.Day == DateTime.Now.Day).OrderByDescending(y => y.Date).ToList();
            ViewBag.Date = DateTime.Now.ToString("dd MMMM yyyy");
            return View(obj);
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId<int>(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            AddErrors(result);
            return PartialView("_ChangePassword", model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEducation(string Education)
        {
            int? id = User.Identity.GetUserId<int>();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser obj = db.Users.Find(id);

            obj.Education = Education;
            db.Entry(obj).State = EntityState.Modified;
            db.SaveChanges();

            return Json(Education, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAddress(string Address)
        {
            int? id = User.Identity.GetUserId<int>();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser obj = db.Users.Find(id);

            obj.Address = Address;
            db.Entry(obj).State = EntityState.Modified;
            db.SaveChanges();

            return Json(Address, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNotes(string Notes)
        {
            int? id = User.Identity.GetUserId<int>();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser obj = db.Users.Find(id);

            obj.Notes = Notes;
            db.Entry(obj).State = EntityState.Modified;
            db.SaveChanges();

            return Json(Notes, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult ToggleSidebar(bool? status)
        {
            if (status != null)
            {
                try
                {
                    ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
                    user.ToggleSidebar = status;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    var Identity = HttpContext.User.Identity as ClaimsIdentity;
                    Identity.RemoveClaim(Identity.FindFirst("ToggleSidebar"));
                    Identity.AddClaim(new Claim("ToggleSidebar", status.ToString()));
                    var authenticationManager = System.Web.HttpContext.Current.GetOwinContext().Authentication;
                    authenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(Identity), new AuthenticationProperties() { IsPersistent = true });

                    return Json("success");
                }
                catch { }
            }

            return Json("error");
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult SkinChange(string color)
        {
            if (color != null)
            {
                try
                {
                    ApplicationUser user = db.Users.Find(User.Identity.GetUserId<int>());
                    user.Skin = color;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    var Identity = HttpContext.User.Identity as ClaimsIdentity;
                    Identity.RemoveClaim(Identity.FindFirst("Skin"));
                    Identity.AddClaim(new Claim("Skin", color));
                    var authenticationManager = System.Web.HttpContext.Current.GetOwinContext().Authentication;
                    authenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(Identity), new AuthenticationProperties() { IsPersistent = true });

                    return Json("success");
                }
                catch { }
            }

            return Json("error");
        }
    }
}