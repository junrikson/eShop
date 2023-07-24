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
    public class MasterSuppliersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterSuppliers
        [Authorize(Roles = "MasterSuppliersActive")]
        public ActionResult Index()
        {
            return View("../Masters/MasterSuppliers/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterSuppliersActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Masters/MasterSuppliers/_IndexGrid", db.Set<MasterSupplier>().AsQueryable());
            else
                return PartialView("../Masters/MasterSuppliers/_IndexGrid", db.Set<MasterSupplier>().AsQueryable().Where(y => y.Code.Contains(search) || y.Name.Contains(search)));
        }

        [Authorize(Roles = "MasterSuppliersActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterSuppliers.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterSuppliers.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "MasterSuppliersActive")]
        public JsonResult IsNameExists(string Name, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterSuppliers.Any(x => x.Name == Name), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterSuppliers.Any(x => x.Name == Name && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterSuppliers/Details/5
        [Authorize(Roles = "MasterSuppliersView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterSupplier masterSupplier = db.MasterSuppliers.Find(id);
            if (masterSupplier == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterSuppliers/_Details", masterSupplier);
        }

        // GET: MasterSuppliers/Create
        [Authorize(Roles = "MasterSuppliersAdd")]
        public ActionResult Create()
        {
            MasterSupplier masterSupplier = new MasterSupplier();
            masterSupplier.SupplierType = EnumSupplierType.Company;
            masterSupplier.CompanyType = EnumCompanyType.PT;
            masterSupplier.Gender = EnumGender.Male;
            masterSupplier.Active = true;

            string code = "SP/" + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + "/";
            var lastData = db.MasterSuppliers.Where(x => x.Code.StartsWith(code)).OrderByDescending(x => x.Code).FirstOrDefault();
            if (lastData == null)
                masterSupplier.Code = code + "0001";
            else
                masterSupplier.Code = code + (Convert.ToInt32(lastData.Code.Substring(lastData.Code.Length - 4, 4)) + 1).ToString("D4");

            return PartialView("../Masters/MasterSuppliers/_Create", masterSupplier);
        }

        // POST: MasterSuppliers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "MasterSuppliersAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Birthday,SupplierType,ContactPerson,CompanyType,Gender,Address,City,SourceCity,Postal,Phone1,Phone2,Mobile,Fax,Email,IDCard,TaxID,TaxName,TaxAddress,Notes,Active,Created,Updated")] MasterSupplier masterSupplier)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(masterSupplier.TaxName)) masterSupplier.TaxName = masterSupplier.Name;
                if (string.IsNullOrEmpty(masterSupplier.TaxAddress)) masterSupplier.TaxAddress = masterSupplier.Address;
                if (string.IsNullOrEmpty(masterSupplier.TaxID)) masterSupplier.TaxID = "00.000.000.0-000.000";

                if (!string.IsNullOrEmpty(masterSupplier.Code)) masterSupplier.Code = masterSupplier.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.Name)) masterSupplier.Name = masterSupplier.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.ContactPerson)) masterSupplier.ContactPerson = masterSupplier.ContactPerson.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.Address)) masterSupplier.Address = masterSupplier.Address.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.City)) masterSupplier.City = masterSupplier.City.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.SourceCity)) masterSupplier.SourceCity = masterSupplier.SourceCity.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.TaxName)) masterSupplier.TaxName = masterSupplier.TaxName.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.TaxAddress)) masterSupplier.TaxAddress = masterSupplier.TaxAddress.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.Notes)) masterSupplier.Notes = masterSupplier.Notes.ToUpper();

                if (masterSupplier.SupplierType == EnumSupplierType.Company)
                    masterSupplier.FullName = masterSupplier.CompanyType.ToString() + ". " + masterSupplier.Name;
                else
                    masterSupplier.FullName = masterSupplier.Name;

                masterSupplier.Created = DateTime.Now;
                masterSupplier.Updated = DateTime.Now;
                db.MasterSuppliers.Add(masterSupplier);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterSupplier, MenuId = masterSupplier.Id, MenuCode = masterSupplier.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Masters/MasterSuppliers/_Create", masterSupplier);
        }

        // GET: MasterSuppliers/Edit/5
        [Authorize(Roles = "MasterSuppliersEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterSupplier masterSupplier = db.MasterSuppliers.Find(id);
            if (masterSupplier == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterSuppliers/_Edit", masterSupplier);
        }

        // POST: MasterSuppliers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterSuppliersEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Birthday,SupplierType,ContactPerson,CompanyType,Gender,Address,City,SourceCity,Postal,Phone1,Phone2,Mobile,Fax,Email,IDCard,TaxID,TaxName,TaxAddress,Notes,Active")] MasterSupplier masterSupplier)
        {
            masterSupplier.Updated = DateTime.Now;

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(masterSupplier.TaxName)) masterSupplier.TaxName = masterSupplier.Name;
                if (string.IsNullOrEmpty(masterSupplier.TaxAddress)) masterSupplier.TaxAddress = masterSupplier.Address;
                if (string.IsNullOrEmpty(masterSupplier.TaxID)) masterSupplier.TaxID = "00.000.000.0-000.000";

                if (!string.IsNullOrEmpty(masterSupplier.Code)) masterSupplier.Code = masterSupplier.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.Name)) masterSupplier.Name = masterSupplier.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.ContactPerson)) masterSupplier.ContactPerson = masterSupplier.ContactPerson.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.Address)) masterSupplier.Address = masterSupplier.Address.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.City)) masterSupplier.City = masterSupplier.City.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.SourceCity)) masterSupplier.SourceCity = masterSupplier.SourceCity.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.TaxName)) masterSupplier.TaxName = masterSupplier.TaxName.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.TaxAddress)) masterSupplier.TaxAddress = masterSupplier.TaxAddress.ToUpper();
                if (!string.IsNullOrEmpty(masterSupplier.Notes)) masterSupplier.Notes = masterSupplier.Notes.ToUpper();

                if (masterSupplier.SupplierType == EnumSupplierType.Company)
                    masterSupplier.FullName = masterSupplier.CompanyType.ToString() + ". " + masterSupplier.Name;
                else
                    masterSupplier.FullName = masterSupplier.Name;

                db.Entry(masterSupplier).State = EntityState.Unchanged;
                db.Entry(masterSupplier).Property("Code").IsModified = true;
                db.Entry(masterSupplier).Property("Name").IsModified = true;
                db.Entry(masterSupplier).Property("FullName").IsModified = true;
                db.Entry(masterSupplier).Property("Birthday").IsModified = true;
                db.Entry(masterSupplier).Property("SupplierType").IsModified = true;
                db.Entry(masterSupplier).Property("ContactPerson").IsModified = true;
                db.Entry(masterSupplier).Property("SupplierType").IsModified = true;
                db.Entry(masterSupplier).Property("Gender").IsModified = true;
                db.Entry(masterSupplier).Property("Address").IsModified = true;
                db.Entry(masterSupplier).Property("City").IsModified = true;
                db.Entry(masterSupplier).Property("SourceCity").IsModified = true;
                db.Entry(masterSupplier).Property("Postal").IsModified = true;
                db.Entry(masterSupplier).Property("Phone1").IsModified = true;
                db.Entry(masterSupplier).Property("Phone2").IsModified = true;
                db.Entry(masterSupplier).Property("Mobile").IsModified = true;
                db.Entry(masterSupplier).Property("Fax").IsModified = true;
                db.Entry(masterSupplier).Property("Email").IsModified = true;
                db.Entry(masterSupplier).Property("IDCard").IsModified = true;
                db.Entry(masterSupplier).Property("TaxID").IsModified = true;
                db.Entry(masterSupplier).Property("TaxName").IsModified = true;
                db.Entry(masterSupplier).Property("TaxAddress").IsModified = true;
                db.Entry(masterSupplier).Property("Notes").IsModified = true;
                db.Entry(masterSupplier).Property("Active").IsModified = true;
                db.Entry(masterSupplier).Property("Updated").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterSupplier, MenuId = masterSupplier.Id, MenuCode = masterSupplier.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Masters/MasterSuppliers/_Edit", masterSupplier);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterSuppliersDelete")]
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
                        MasterSupplier obj = db.MasterSuppliers.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            MasterSupplier tmp = obj;
                            db.MasterSuppliers.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterSupplier, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
