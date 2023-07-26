using eShop.Models;
using eShop.Properties;
using Microsoft.AspNet.Identity;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;


namespace eShop.Controllers
{
    public class PurchaseOrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PurchaseOrders
        [Authorize(Roles = "PurchaseOrdersActive")]
        public ActionResult Index()
        {
            return View("../Buying/PurchaseOrders/Index");
        }

        [HttpGet]
        [Authorize(Roles = "PurchaseOrdersActive")]
        public PartialViewResult IndexGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Buying/PurchaseOrders/_IndexGrid", db.Set<PurchaseOrder>().AsQueryable());
            else
                return PartialView("../Buying/PurchaseOrders/_IndexGrid", db.Set<PurchaseOrder>().AsQueryable()
                    .Where(x => x.Code.Contains(search)));
        }

        [Authorize(Roles = "PurchaseOrdersActive")]
        public JsonResult IsCodeExists(string Code, int? Id)
        {
            if (Id == null)
            {
                return Json(!db.PurchaseOrders.Any(x => x.Code == Code), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(!db.PurchaseOrders.Any(x => x.Code == Code && x.Id != Id), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: PurchaseOrders/Details/
        [Authorize(Roles = "PurchaseOrdersView")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder PurchaseOrder = db.PurchaseOrders.Find(id);
            if (PurchaseOrder == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/PurchaseOrders/_Details", PurchaseOrder);
        }

        // GET: PurchaseOrders/Create
        [Authorize(Roles = "PurchaseOrdersAdd")]
        public ActionResult Create()
        {
            PurchaseOrder PurchaseOrder = new PurchaseOrder();
            PurchaseOrder.Active = true;

            string code = Settings.Default.PurchaseOrderCode + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + "/";
            var lastData = db.PurchaseOrders.Where(x => x.Code.StartsWith(code)).OrderByDescending(x => x.Code).FirstOrDefault();
            if (lastData == null)
                PurchaseOrder.Code = code + "0001";
            else
                PurchaseOrder.Code = code + (Convert.ToInt32(lastData.Code.Substring(lastData.Code.Length - 4, 4)) + 1).ToString("D4");

            return PartialView("../Buying/PurchaseOrders/_Create", PurchaseOrder);
        }

        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersAdd")]
        public ActionResult Create([Bind(Include = "Id,Code,Notes,Active,Created,Updated,UserId")] PurchaseOrder PurchaseOrder)
        {
            if (!string.IsNullOrEmpty(PurchaseOrder.Code)) PurchaseOrder.Code = PurchaseOrder.Code.ToUpper();
           // if (!string.IsNullOrEmpty(PurchaseOrder.Name)) PurchaseOrder.Name = PurchaseOrder.Name.ToUpper();
            if (!string.IsNullOrEmpty(PurchaseOrder.Notes)) PurchaseOrder.Notes = PurchaseOrder.Notes.ToUpper();

            PurchaseOrder.Created = DateTime.Now;
            PurchaseOrder.Updated = DateTime.Now;
            PurchaseOrder.UserId = User.Identity.GetUserId<int>();

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.PurchaseOrders.Add(PurchaseOrder);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrder, MenuId = PurchaseOrder.Id, MenuCode = PurchaseOrder.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }

                return PartialView("../Buying/PurchaseOrders/_Create", PurchaseOrder);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null)
            {
                PurchaseOrder obj = db.PurchaseOrders.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        db.PurchaseOrders.Remove(obj);
                        db.SaveChanges();
                    }
                }
            }
            return Json(id);
        }

        // GET: PurchaseOrders/Edit/5
        [Authorize(Roles = "PurchaseOrdersEdit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder PurchaseOrder = db.PurchaseOrders.Find(id);
            if (PurchaseOrder == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Buying/PurchaseOrders/_Edit", PurchaseOrder);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PurchaseOrdersEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Notes,Active,Updated,UserId")] PurchaseOrder PurchaseOrder)
        {
            PurchaseOrder.Updated = DateTime.Now;
            PurchaseOrder.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(PurchaseOrder.Code)) PurchaseOrder.Code = PurchaseOrder.Code.ToUpper();
          //  if (!string.IsNullOrEmpty(PurchaseOrder.Name)) PurchaseOrder.Name = PurchaseOrder.Name.ToUpper();
            if (!string.IsNullOrEmpty(PurchaseOrder.Notes)) PurchaseOrder.Notes = PurchaseOrder.Notes.ToUpper();

            db.Entry(PurchaseOrder).State = EntityState.Unchanged;
            db.Entry(PurchaseOrder).Property("Code").IsModified = true;
            db.Entry(PurchaseOrder).Property("Name").IsModified = true;
            db.Entry(PurchaseOrder).Property("Notes").IsModified = true;
            db.Entry(PurchaseOrder).Property("Active").IsModified = true;
            db.Entry(PurchaseOrder).Property("Updated").IsModified = true;

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrder, MenuId = PurchaseOrder.Id, MenuCode = PurchaseOrder.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
                return PartialView("../Buying/PurchaseOrders/_Edit", PurchaseOrder);

            }
        }



        [HttpPost]
        [Authorize(Roles = "PurchaseOrdersDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDelete(int[] ids)
        {
            if (ids == null || ids.Length <= 0)
                return Json("Pilih salah satu data yang akan dihapus.");
            else
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        int failed = 0;
                        foreach (int id in ids)
                        {
                            PurchaseOrder obj = db.PurchaseOrders.Find(id);

                            if (obj == null)
                                failed++;
                            else
                            {
                                PurchaseOrder tmp = obj;

                                db.PurchaseOrders.Remove(obj);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.PurchaseOrder, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                                db.SaveChanges();

                                dbTran.Commit();
                            }
                        }
                        return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
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
