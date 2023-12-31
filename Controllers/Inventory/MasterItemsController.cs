﻿using eShop.Models;
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
                return PartialView("../Inventory/MasterItems/_IndexGrid", db.Set<MasterItem>().AsQueryable()
                    .Where(y => y.Code.Contains(search) || y.Name.Contains(search)));
        }

        [HttpGet]
        [Authorize(Roles = "MasterItemsActive")]
        public PartialViewResult OthersGrid(String search)
        {
            if (String.IsNullOrEmpty(search))
                return PartialView("../Inventory/MasterItems/_OthersGrid", db.Set<MasterItem>().AsQueryable());
            else
                return PartialView("../Inventory/MasterItems/_OthersGrid", db.Set<MasterItem>().AsQueryable()
                    .Where(y => y.Code.Contains(search) || y.Name.Contains(search)));
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

        [HttpGet]
        [Authorize(Roles = "MasterItemsView")]
        public PartialViewResult ViewGrid(int Id)
        {
            return PartialView("../Inventory/MasterItems/_ViewGrid", db.MasterItemUnits
                .Where(x => x.MasterItemId == Id).ToList());
        }

        // GET: MasterItems/Create
        [Authorize(Roles = "MasterItemsAdd")]
        public ActionResult Create()
        {

            MasterItem masterItem = new MasterItem
            {
                Code = "temp/" + Guid.NewGuid().ToString(),
                Name = "",
                MasterCategoryId = db.MasterCategories.FirstOrDefault().Id,
                MasterBrandId = db.MasterBrands.FirstOrDefault().Id,
                Notes = "",
                Active = false,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UserId = User.Identity.GetUserId<int>()
            };

            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
            {
                try
                {
                    db.MasterItems.Add(masterItem);
                    db.SaveChanges();

                    var masterUnits = db.MasterUnits.Where(x => x.Default && x.Active).ToList();

                    if (masterUnits != null)
                    {
                        foreach (MasterUnit masterUnit in masterUnits)
                        {
                            MasterItemUnit masterItemUnit = new MasterItemUnit
                            {
                                MasterItemId = masterItem.Id,
                                MasterUnitId = masterUnit.Id,
                                Default = false,
                                Active = true,
                                Created = DateTime.Now,
                                Updated = DateTime.Now,
                                UserId = User.Identity.GetUserId<int>()
                            };

                            db.MasterItemUnits.Add(masterItemUnit);
                            db.SaveChanges();
                        }
                    }

                    dbTran.Commit();


                    masterItem.Code = "";
                    masterItem.Active = true;
                    masterItem.MasterCategoryId = 0;
                    masterItem.MasterBrandId = 0;

                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    throw ex;
                }
            }

            return View("../Inventory/MasterItems/Create", masterItem);
        }

        // POST: MasterItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "MasterItemsAdd")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Name,MasterCategoryId,MasterBrandId,ItemType,Notes,Active,Created,Updated,UserId")] MasterItem masterItem)
        {
            masterItem.UserId = User.Identity.GetUserId<int>();
            masterItem.Created = DateTime.Now;
            masterItem.Updated = DateTime.Now;

            if (!string.IsNullOrEmpty(masterItem.Code)) masterItem.Code = masterItem.Code.ToUpper();
            if (!string.IsNullOrEmpty(masterItem.Name)) masterItem.Name = masterItem.Name.ToUpper();
            if (!string.IsNullOrEmpty(masterItem.Notes)) masterItem.Notes = masterItem.Notes.ToUpper();

            if (ModelState.IsValid)
            {
                db.Entry(masterItem).State = EntityState.Modified;

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItem, MenuId = masterItem.Id, MenuCode = masterItem.Code, Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return RedirectToAction("Index");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            return View("../Inventory/MasterItems/Create", masterItem);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterItemsAdd")]
        public ActionResult Cancel(int? id)
        {
            if (id != null || id == 0)
            {
                MasterItem obj = db.MasterItems.Find(id);
                if (obj != null)
                {
                    if (!obj.Active)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                db.MasterItemUnits.RemoveRange(db.MasterItemUnits.Where(x => x.MasterItemId == obj.Id));
                                db.SaveChanges();

                                db.MasterItems.Remove(obj);
                                db.SaveChanges();

                                dbTran.Commit();
                            }
                            catch (DbEntityValidationException ex)
                            {
                                dbTran.Rollback();
                                throw ex;
                            }
                        }
                    }
                }
            }
            return Json(id);
        }

        // GET: MasterItems/Edit/5
        [Authorize(Roles = "MasterItemsEdit")]
        public ActionResult Edit(int? id, bool isCopy = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterItem masterItem = db.MasterItems.Find(id);

            if (masterItem == null)
            {
                return HttpNotFound();
            }

            if (isCopy)
            {
                masterItem.Code = "";
                masterItem.Active = true;
            };

            return View("../Inventory/MasterItems/Edit", masterItem);
        }

        // POST: MasterItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsEdit")]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,MasterCategoryId,MasterBrandId,Notes,Active,Created,Updated,UserId")] MasterItem masterItem)
        {
            masterItem.Updated = DateTime.Now;
            masterItem.UserId = User.Identity.GetUserId<int>();

            if (!string.IsNullOrEmpty(masterItem.Code)) masterItem.Code = masterItem.Code.ToUpper();
            if (!string.IsNullOrEmpty(masterItem.Name)) masterItem.Name = masterItem.Name.ToUpper();
            if (!string.IsNullOrEmpty(masterItem.Notes)) masterItem.Notes = masterItem.Notes.ToUpper();

            db.Entry(masterItem).State = EntityState.Unchanged;
            db.Entry(masterItem).Property("Code").IsModified = true;
            db.Entry(masterItem).Property("Name").IsModified = true;
            db.Entry(masterItem).Property("MasterCategoryId").IsModified = true;
            db.Entry(masterItem).Property("MasterBrandId").IsModified = true;
            db.Entry(masterItem).Property("Notes").IsModified = true;
            db.Entry(masterItem).Property("Active").IsModified = true;
            db.Entry(masterItem).Property("Updated").IsModified = true;
            db.Entry(masterItem).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItem, MenuId = masterItem.Id, MenuCode = masterItem.Code, Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return RedirectToAction("Index");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            return View("../Inventory/MasterItems/Edit", masterItem);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterItemsAdd")]
        public ActionResult Copy(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterItem obj = db.MasterItems.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            else
            {
                MasterItem masterItem = new MasterItem
                {
                    Code = "temp/" + Guid.NewGuid().ToString(),
                    Name = obj.Name,
                    MasterCategoryId = obj.MasterCategoryId,
                    MasterBrandId = obj.MasterBrandId,
                    Notes = obj.Notes,
                    Active = false,
                    Created = DateTime.Now,
                    Updated = DateTime.Now,
                    UserId = User.Identity.GetUserId<int>()
                };

                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.MasterItems.Add(masterItem);
                        db.SaveChanges();

                        var masterItemUnits = db.MasterItemUnits.Where(x => x.MasterItemId == obj.Id && x.Active).ToList();

                        if (masterItemUnits != null)
                        {
                            foreach (MasterItemUnit temp in masterItemUnits)
                            {
                                MasterItemUnit masterItemUnit = new MasterItemUnit
                                {
                                    MasterItemId = masterItem.Id,
                                    MasterUnitId = temp.MasterUnitId,
                                    Default = temp.Default,
                                    Active = true,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MasterItemUnits.Add(masterItemUnit);
                                db.SaveChanges();
                            }
                        }

                        dbTran.Commit();

                        masterItem.Code = "";
                        masterItem.Active = true;

                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }

                return Json(masterItem.Id);
            }
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
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterItem tmp = obj;

                                    db.MasterItemUnits.RemoveRange(db.MasterItemUnits.Where(x => x.MasterItemId == obj.Id));
                                    db.SaveChanges();

                                    db.MasterItems.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItem, MenuId = tmp.Id, MenuCode = tmp.Code, Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                                    db.SaveChanges();

                                    dbTran.Commit();
                                }
                                catch (DbEntityValidationException ex)
                                {
                                    dbTran.Rollback();
                                    throw ex;
                                }
                            }
                        }
                    }
                    return Json((ids.Length - failed).ToString() + " data berhasil dihapus.");
                }
            }
        }

        [HttpGet]
        [Authorize(Roles = "MasterItemsActive")]
        public PartialViewResult DetailsGrid(int Id)
        {
            return PartialView("../Inventory/MasterItems/_DetailsGrid", db.MasterItemUnits
                .Where(x => x.MasterItemId == Id).ToList());
        }

        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult DetailsCreate(int? masterItemId)
        {
            if (masterItemId == null || masterItemId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterItem masterItem = db.MasterItems.Find(masterItemId);

            if (masterItem == null)
            {
                return HttpNotFound();
            }

            MasterItemUnit masterItemUnit = new MasterItemUnit
            {
                MasterItemId = masterItem.Id,
                Default = false,
                Active = true
            };

            return PartialView("../Inventory/MasterItems/_DetailsCreate", masterItemUnit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult DetailsCreate([Bind(Include = "MasterItemId,MasterUnitId,Default,Active,Created,Updated,UserId")] MasterItemUnit masterItemUnit)
        {
            masterItemUnit.Created = DateTime.Now;
            masterItemUnit.Updated = DateTime.Now;
            masterItemUnit.UserId = User.Identity.GetUserId<int>();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.MasterItemUnits.Add(masterItemUnit);
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItemUnits, MenuId = masterItemUnit.MasterUnitId, MenuCode = masterItemUnit.MasterItemId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return Json("success", JsonRequestBehavior.AllowGet);

                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }
            return PartialView("../Inventory/MasterItems/_DetailsCreate", masterItemUnit);
        }

        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult DetailsEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterItemUnit obj = db.MasterItemUnits.Find(id);

            if (obj == null)
            {
                return HttpNotFound();
            }
            return PartialView("../Inventory/MasterItems/_DetailsEdit", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult DetailsEdit([Bind(Include = "Id,MasterItemId,MasterUnitId,Default,Active,Created,Updated,UserId")] MasterItemUnit masterItemUnit)
        {
            masterItemUnit.Updated = DateTime.Now;
            masterItemUnit.UserId = User.Identity.GetUserId<int>();

            db.Entry(masterItemUnit).State = EntityState.Unchanged;
            db.Entry(masterItemUnit).Property("MasterUnitId").IsModified = true;
            db.Entry(masterItemUnit).Property("Default").IsModified = true;
            db.Entry(masterItemUnit).Property("Active").IsModified = true;
            db.Entry(masterItemUnit).Property("Updated").IsModified = true;
            db.Entry(masterItemUnit).Property("UserId").IsModified = true;

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();

                        db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItemUnits, MenuId = masterItemUnit.MasterUnitId, MenuCode = masterItemUnit.MasterItemId.ToString(), Actions = EnumActions.EDIT, UserId = User.Identity.GetUserId<int>() });
                        db.SaveChanges();

                        dbTran.Commit();

                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            }

            return PartialView("../Inventory/MasterItems/_DetailsEdit", masterItemUnit);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Authorize(Roles = "MasterItemsActive")]
        public ActionResult DetailsBatchDelete(int[] ids)
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
                        MasterItemUnit obj = db.MasterItemUnits.Find(id);
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterItemUnit tmp = obj;

                                    db.MasterItemUnits.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterItemUnits, MenuId = tmp.MasterUnitId, MenuCode = tmp.MasterItemId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
                                    db.SaveChanges();

                                    dbTran.Commit();
                                }
                                catch (DbEntityValidationException ex)
                                {
                                    dbTran.Rollback();
                                    throw ex;
                                }
                            }
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
