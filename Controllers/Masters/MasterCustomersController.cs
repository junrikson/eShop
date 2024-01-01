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
    public class MasterCustomersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MasterCustomers
        [Authorize(Roles = "MasterCustomersActive")]
        public ActionResult Index()
        {
            return View("../Masters/MasterCustomers/Index");
        }

        [HttpGet]
        [Authorize(Roles = "MasterCustomersActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Masters/MasterCustomers/_IndexGrid", db.Set<MasterCustomer>().AsQueryable());
            else
                return PartialView("../Masters/MasterCustomers/_IndexGrid", db.Set<MasterCustomer>().AsQueryable().Where(y => y.Code.Contains(search) || y.Name.Contains(search)));
        }

        [Authorize(Roles = "MasterCustomersActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterCustomers.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterCustomers.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "MasterCustomersActive")]
        public JsonResult IsNameExists(string Name, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.MasterCustomers.Any(x => x.Name == Name), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.MasterCustomers.Any(x => x.Name == Name && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: MasterCustomers/Details/5
        [Authorize(Roles = "MasterCustomersView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterCustomer masterCustomer = db.MasterCustomers.Find(id);
            if (masterCustomer == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterCustomers/_Details", masterCustomer);
        }

        // GET: MasterCustomers/Create
        [Authorize(Roles = "MasterCustomersAdd")]
        public ActionResult Create()
        {
            MasterCustomer masterCustomer = new MasterCustomer();
            masterCustomer.CustomerType = EnumCustomerType.Company;
            masterCustomer.CompanyType = EnumCompanyType.PT;
            masterCustomer.Gender = EnumGender.Male;
            masterCustomer.TOP = null;
            masterCustomer.Active = true;

            string code = Settings.Default.CustomerCode + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + "/";
            var lastData = db.MasterCustomers.Where(x => x.Code.StartsWith(code)).OrderByDescending(x => x.Code).FirstOrDefault();
            if (lastData == null)
                masterCustomer.Code = code + "0001";
            else
                masterCustomer.Code = code + (Convert.ToInt32(lastData.Code.Substring(lastData.Code.Length - 4, 4)) + 1).ToString("D4");

            return PartialView("../Masters/MasterCustomers/_Create", masterCustomer);
        }

        // POST: MasterCustomers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "MasterCustomersAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Birthday,CustomerType,ContactPerson,CompanyType,Gender,Address,City,SourceCity,Postal,Phone1,Phone2,Mobile,Fax,TOP,Email,IDCard,TaxID,TaxName,TaxAddress,TaxID2,TaxName2,TaxAddress2,TaxID3,TaxName3,TaxAddress3,MasterDestinationId,Notes,Active,Created,Updated")] MasterCustomer masterCustomer)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(masterCustomer.TaxName)) masterCustomer.TaxName = masterCustomer.Name;
                if (string.IsNullOrEmpty(masterCustomer.TaxAddress)) masterCustomer.TaxAddress = masterCustomer.Address;
                if (string.IsNullOrEmpty(masterCustomer.TaxID)) masterCustomer.TaxID = "00.000.000.0-000.000";

                if (!string.IsNullOrEmpty(masterCustomer.Code)) masterCustomer.Code = masterCustomer.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.Name)) masterCustomer.Name = masterCustomer.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.ContactPerson)) masterCustomer.ContactPerson = masterCustomer.ContactPerson.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.Address)) masterCustomer.Address = masterCustomer.Address.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.City)) masterCustomer.City = masterCustomer.City.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.SourceCity)) masterCustomer.SourceCity = masterCustomer.SourceCity.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxName)) masterCustomer.TaxName = masterCustomer.TaxName.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxAddress)) masterCustomer.TaxAddress = masterCustomer.TaxAddress.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxName2)) masterCustomer.TaxName2 = masterCustomer.TaxName2.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxAddress2)) masterCustomer.TaxAddress2 = masterCustomer.TaxAddress2.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxName3)) masterCustomer.TaxName3 = masterCustomer.TaxName3.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxAddress3)) masterCustomer.TaxAddress3 = masterCustomer.TaxAddress3.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.Notes)) masterCustomer.Notes = masterCustomer.Notes.ToUpper();
                masterCustomer.MasterDestinationId = masterCustomer.MasterDestinationId;
                

                if (masterCustomer.CustomerType == EnumCustomerType.Company)
                    masterCustomer.FullName = masterCustomer.CompanyType.ToString() + ". " + masterCustomer.Name;
                else
                    masterCustomer.FullName = masterCustomer.Name;

                masterCustomer.Created = DateTime.Now;
                masterCustomer.Updated = DateTime.Now;
                db.MasterCustomers.Add(masterCustomer);
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCustomer, MenuId = masterCustomer.Id, MenuCode = masterCustomer.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            return PartialView("../Masters/MasterCustomers/_Create", masterCustomer);
        }

        // GET: MasterCustomers/Edit/5
        [Authorize(Roles = "MasterCustomersEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MasterCustomer masterCustomer = db.MasterCustomers.Find(id);
            if (masterCustomer == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Masters/MasterCustomers/_Edit", masterCustomer);
        }

        // POST: MasterCustomers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCustomersEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Birthday,CustomerType,ContactPerson,CompanyType,Gender,Address,City,SourceCity,Postal,Phone1,Phone2,Mobile,Fax,TOP,Email,IDCard,TaxID,TaxName,TaxAddress,TaxID2,TaxName2,TaxAddress2,TaxID3,TaxName3,TaxAddress3,MasterDestinationId,Notes,Active")] MasterCustomer masterCustomer)
        {
            masterCustomer.Updated = DateTime.Now;

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(masterCustomer.TaxName)) masterCustomer.TaxName = masterCustomer.Name;
                if (string.IsNullOrEmpty(masterCustomer.TaxAddress)) masterCustomer.TaxAddress = masterCustomer.Address;
                if (string.IsNullOrEmpty(masterCustomer.TaxID)) masterCustomer.TaxID = "00.000.000.0-000.000";

                if (!string.IsNullOrEmpty(masterCustomer.Code)) masterCustomer.Code = masterCustomer.Code.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.Name)) masterCustomer.Name = masterCustomer.Name.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.ContactPerson)) masterCustomer.ContactPerson = masterCustomer.ContactPerson.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.Address)) masterCustomer.Address = masterCustomer.Address.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.City)) masterCustomer.City = masterCustomer.City.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.SourceCity)) masterCustomer.SourceCity = masterCustomer.SourceCity.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxName)) masterCustomer.TaxName = masterCustomer.TaxName.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxAddress)) masterCustomer.TaxAddress = masterCustomer.TaxAddress.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxName2)) masterCustomer.TaxName2 = masterCustomer.TaxName2.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxAddress2)) masterCustomer.TaxAddress2 = masterCustomer.TaxAddress2.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxName3)) masterCustomer.TaxName3 = masterCustomer.TaxName3.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.TaxAddress3)) masterCustomer.TaxAddress3 = masterCustomer.TaxAddress3.ToUpper();
                if (!string.IsNullOrEmpty(masterCustomer.Notes)) masterCustomer.Notes = masterCustomer.Notes.ToUpper();
                masterCustomer.MasterDestinationId = masterCustomer.MasterDestinationId;

                if (masterCustomer.CustomerType == EnumCustomerType.Company)
                    masterCustomer.FullName = masterCustomer.CompanyType.ToString() + ". " + masterCustomer.Name;
                else
                    masterCustomer.FullName = masterCustomer.Name;

                db.Entry(masterCustomer).State = EntityState.Unchanged;
                db.Entry(masterCustomer).Property("Code").IsModified = true;
                db.Entry(masterCustomer).Property("Name").IsModified = true;
                db.Entry(masterCustomer).Property("FullName").IsModified = true;
                db.Entry(masterCustomer).Property("Birthday").IsModified = true;
                db.Entry(masterCustomer).Property("CustomerType").IsModified = true;
                db.Entry(masterCustomer).Property("ContactPerson").IsModified = true;
                db.Entry(masterCustomer).Property("CompanyType").IsModified = true;
                db.Entry(masterCustomer).Property("Gender").IsModified = true;
                db.Entry(masterCustomer).Property("Address").IsModified = true;
                db.Entry(masterCustomer).Property("City").IsModified = true;
                db.Entry(masterCustomer).Property("SourceCity").IsModified = true;
                db.Entry(masterCustomer).Property("Postal").IsModified = true;
                db.Entry(masterCustomer).Property("Phone1").IsModified = true;
                db.Entry(masterCustomer).Property("Phone2").IsModified = true;
                db.Entry(masterCustomer).Property("Mobile").IsModified = true;
                db.Entry(masterCustomer).Property("Fax").IsModified = true;
                db.Entry(masterCustomer).Property("Email").IsModified = true;
                db.Entry(masterCustomer).Property("TOP").IsModified = true;
                db.Entry(masterCustomer).Property("IDCard").IsModified = true;
                db.Entry(masterCustomer).Property("TaxID").IsModified = true;
                db.Entry(masterCustomer).Property("TaxName").IsModified = true;
                db.Entry(masterCustomer).Property("TaxAddress").IsModified = true;
                db.Entry(masterCustomer).Property("TaxID2").IsModified = true;
                db.Entry(masterCustomer).Property("TaxName2").IsModified = true;
                db.Entry(masterCustomer).Property("TaxAddress2").IsModified = true;
                db.Entry(masterCustomer).Property("TaxID3").IsModified = true;
                db.Entry(masterCustomer).Property("TaxName3").IsModified = true;
                db.Entry(masterCustomer).Property("TaxAddress3").IsModified = true;
                db.Entry(masterCustomer).Property("Notes").IsModified = true;
                db.Entry(masterCustomer).Property("MasterDestinationId").IsModified = true;
                db.Entry(masterCustomer).Property("Active").IsModified = true;
                db.Entry(masterCustomer).Property("Updated").IsModified = true;
                db.SaveChanges();

                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCustomer, MenuId = masterCustomer.Id, MenuCode = masterCustomer.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                db.SaveChanges();

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            return PartialView("../Masters/MasterCustomers/_Edit", masterCustomer);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterCustomersDelete")]
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
                        MasterCustomer obj = db.MasterCustomers.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            MasterCustomer tmp = obj;
                            db.MasterCustomers.Remove(obj);
                            db.SaveChanges();

                            db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterCustomer, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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
