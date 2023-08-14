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
    public class MasterSalesPersonsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterSalesPersons
        [Authorize(Roles = "MasterSalesPersonsActive")]
        public ActionResult Index()
        {
            return View("../Masters/MasterSalesPersons/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterSalesPersonsActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Masters/MasterSalesPersons/_IndexGrid", db.Set<MasterSalesPerson>().AsQueryable());
            else
                return PartialView("../Masters/MasterSalesPersons/_IndexGrid", db.Set<MasterSalesPerson>().AsQueryable().Where(y => y.Code.Contains(search) || y.Name.Contains(search)));
        }

        [Authorize(Roles = "MasterSalesPersonsActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterSalesPersons.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterSalesPersons.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "MasterSalesPersonsActive")]
        public JsonResult IsNameExists(string Name, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterSalesPersons.Any(x => x.Name == Name), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterSalesPersons.Any(x => x.Name == Name && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterSalesPersons/Details/5
        [Authorize(Roles = "MasterSalesPersonsView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterSalesPerson masterSalesPerson = db.MasterSalesPersons.Find(id);
            if (masterSalesPerson == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterSalesPersons/_Details", masterSalesPerson);
        }

        // GET: MasterSalesPersons/Create
        [Authorize(Roles = "MasterSalesPersonsAdd")]
        public ActionResult Create()
        {
            MasterSalesPerson masterSalesPerson = new MasterSalesPerson
            {
                Gender = EnumGender.Male,
                Active = true
            };

            string code = Settings.Default.SalesPersonCode + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + "/";
            var lastData = db.MasterSalesPersons.Where(x => x.Code.StartsWith(code)).OrderByDescending(x => x.Code).FirstOrDefault();
            if (lastData == null)
                masterSalesPerson.Code = code + "0001";
            else
                masterSalesPerson.Code = code + (Convert.ToInt32(lastData.Code.Substring(lastData.Code.Length - 4, 4)) + 1).ToString("D4");

            return PartialView("../Masters/MasterSalesPersons/_Create", masterSalesPerson);
        }

        // POST: MasterSalesPersons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "MasterSalesPersonsAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Birthday,Gender,Address,City,SourceCity,Postal,Phone1,Phone2,Mobile,Fax,TOP,Email,IDCard,TaxID,TaxName,TaxAddress,Notes,Active,Created,Updated")] MasterSalesPerson masterSalesPerson)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(masterSalesPerson.TaxName)) masterSalesPerson.TaxName = masterSalesPerson.Name;
                if (string.IsNullOrEmpty(masterSalesPerson.TaxAddress)) masterSalesPerson.TaxAddress = masterSalesPerson.Address;
                if (string.IsNullOrEmpty(masterSalesPerson.TaxID)) masterSalesPerson.TaxID = "00.000.000.0-000.000";
                if (!string.IsNullOrEmpty(masterSalesPerson.Code)) masterSalesPerson.Code = masterSalesPerson.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterSalesPerson.Name)) masterSalesPerson.Name = masterSalesPerson.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterSalesPerson.Address)) masterSalesPerson.Address = masterSalesPerson.Address.ToUpper();
                if (!string.IsNullOrEmpty(masterSalesPerson.City)) masterSalesPerson.City = masterSalesPerson.City.ToUpper();
                if (!string.IsNullOrEmpty(masterSalesPerson.TaxAddress)) masterSalesPerson.TaxAddress = masterSalesPerson.TaxAddress.ToUpper();
                if (!string.IsNullOrEmpty(masterSalesPerson.Notes)) masterSalesPerson.Notes = masterSalesPerson.Notes.ToUpper();

                masterSalesPerson.Created = DateTime.Now;
                masterSalesPerson.Updated = DateTime.Now;
                db.MasterSalesPersons.Add(masterSalesPerson);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterSalesPerson, MenuId = masterSalesPerson.Id, MenuCode = masterSalesPerson.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Masters/MasterSalesPersons/_Create", masterSalesPerson);
        }

        // GET: MasterSalesPersons/Edit/5
        [Authorize(Roles = "MasterSalesPersonsEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterSalesPerson masterSalesPerson = db.MasterSalesPersons.Find(id);
            if (masterSalesPerson == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterSalesPersons/_Edit", masterSalesPerson);
        }

        // POST: MasterSalesPersons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterSalesPersonsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Birthday,Gender,Address,City,Postal,Phone1,Phone2,Mobile,Fax,TOP,Email,IDCard,TaxID,TaxName,TaxAddress,Notes,Active")] MasterSalesPerson masterSalesPerson)
        {
            masterSalesPerson.Updated = DateTime.Now;

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(masterSalesPerson.TaxName)) masterSalesPerson.TaxName = masterSalesPerson.Name;
                if (string.IsNullOrEmpty(masterSalesPerson.TaxAddress)) masterSalesPerson.TaxAddress = masterSalesPerson.Address;
                if (string.IsNullOrEmpty(masterSalesPerson.TaxID)) masterSalesPerson.TaxID = "00.000.000.0-000.000";
                if (!string.IsNullOrEmpty(masterSalesPerson.Code)) masterSalesPerson.Code = masterSalesPerson.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterSalesPerson.Name)) masterSalesPerson.Name = masterSalesPerson.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterSalesPerson.Address)) masterSalesPerson.Address = masterSalesPerson.Address.ToUpper();
                if (!string.IsNullOrEmpty(masterSalesPerson.City)) masterSalesPerson.City = masterSalesPerson.City.ToUpper();
                if (!string.IsNullOrEmpty(masterSalesPerson.TaxAddress)) masterSalesPerson.TaxAddress = masterSalesPerson.TaxAddress.ToUpper();
                if (!string.IsNullOrEmpty(masterSalesPerson.Notes)) masterSalesPerson.Notes = masterSalesPerson.Notes.ToUpper();


                db.Entry(masterSalesPerson).State = EntityState.Unchanged;
                db.Entry(masterSalesPerson).Property("Code").IsModified = true;
                db.Entry(masterSalesPerson).Property("Name").IsModified = true;
                db.Entry(masterSalesPerson).Property("FullName").IsModified = true;
                db.Entry(masterSalesPerson).Property("Birthday").IsModified = true;
                db.Entry(masterSalesPerson).Property("Gender").IsModified = true;
                db.Entry(masterSalesPerson).Property("Address").IsModified = true;
                db.Entry(masterSalesPerson).Property("City").IsModified = true;
                db.Entry(masterSalesPerson).Property("Postal").IsModified = true;
                db.Entry(masterSalesPerson).Property("Phone1").IsModified = true;
                db.Entry(masterSalesPerson).Property("Phone2").IsModified = true;
                db.Entry(masterSalesPerson).Property("Mobile").IsModified = true;
                db.Entry(masterSalesPerson).Property("Email").IsModified = true;
                db.Entry(masterSalesPerson).Property("IDCard").IsModified = true;
                db.Entry(masterSalesPerson).Property("TaxID").IsModified = true;
                db.Entry(masterSalesPerson).Property("TaxName").IsModified = true;
                db.Entry(masterSalesPerson).Property("TaxAddress").IsModified = true;
                db.Entry(masterSalesPerson).Property("Notes").IsModified = true;
                db.Entry(masterSalesPerson).Property("Active").IsModified = true;
                db.Entry(masterSalesPerson).Property("Updated").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterSalesPerson, MenuId = masterSalesPerson.Id, MenuCode = masterSalesPerson.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Masters/MasterSalesPersons/_Edit", masterSalesPerson);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterSalesPersonsDelete")]
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
                        MasterSalesPerson obj = db.MasterSalesPersons.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            MasterSalesPerson tmp = obj;
                            db.MasterSalesPersons.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterSalesPerson, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
