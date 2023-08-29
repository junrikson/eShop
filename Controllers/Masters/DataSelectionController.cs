using eShop.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;
using System.Web.Mvc;

namespace eShop.Controllers
{
    [Authorize]
    public class DataSelectionController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Begin of MasterBusinessUnitRegion

        [HttpGet]
        [Authorize(Roles = "MasterRegionsAdd")]
        public PartialViewResult MasterRegionsGrid(int masterBusinessUnitId)
        {
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterRegionsGrid", db.Set<MasterBusinessUnitRegion>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == masterBusinessUnitId));
        }

        [Authorize(Roles = "MasterRegionsAdd")]
        public ActionResult AddMasterRegions(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(id);

            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }

            MasterBusinessUnitRegionSelection obj = new MasterBusinessUnitRegionSelection
            {
                MasterBusinessUnitId = masterBusinessUnit.Id,
                MasterBusinessUnit = masterBusinessUnit
            };

            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterRegionsAdd", obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterRegionsAdd")]
        public ActionResult AddMasterRegions([Bind(Include = "MasterBusinessUnitId, MasterRegionStartId, MasterRegionEndId")] MasterBusinessUnitRegionSelection obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MasterRegionEndId == null)
                {
                    MasterBusinessUnitRegion temp = db.MasterBusinessUnitRegions.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterRegionId == obj.MasterRegionStartId).FirstOrDefault();

                    if (temp == null)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                MasterBusinessUnitRegion masterBusinessUnitRegion = new MasterBusinessUnitRegion
                                {
                                    MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                    MasterRegionId = obj.MasterRegionStartId,
                                    Created = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MasterBusinessUnitRegions.Add(masterBusinessUnitRegion);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitRegion, MenuId = masterBusinessUnitRegion.MasterBusinessUnitId, MenuCode = masterBusinessUnitRegion.MasterRegionId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
                else
                {
                    MasterRegion masterRegionStart = db.MasterRegions.Find(obj.MasterRegionStartId);
                    MasterRegion masterRegionEnd = db.MasterRegions.Find(obj.MasterRegionEndId);

                    var masterRegions = db.MasterRegions.Where(x => x.Code.CompareTo(masterRegionStart.Code) >= 0 && x.Code.CompareTo(masterRegionEnd.Code) <= 0).ToList();

                    foreach (MasterRegion masterRegion in masterRegions)
                    {
                        MasterBusinessUnitRegion temp = db.MasterBusinessUnitRegions.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterRegionId == masterRegion.Id).FirstOrDefault();
                        if (temp == null)
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitRegion masterBusinessUnitRegion = new MasterBusinessUnitRegion
                                    {
                                        MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                        MasterRegionId = masterRegion.Id,
                                        Created = DateTime.Now,
                                        UserId = User.Identity.GetUserId<int>()
                                    };

                                    db.MasterBusinessUnitRegions.Add(masterBusinessUnitRegion);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitRegion, MenuId = masterBusinessUnitRegion.MasterBusinessUnitId, MenuCode = masterBusinessUnitRegion.MasterRegionId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(obj.MasterBusinessUnitId);
            obj.MasterBusinessUnit = masterBusinessUnit;
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterRegionsAdd", obj);
        }

        [HttpPost]
        [Authorize(Roles = "MasterRegionsDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDeleteRegions(int[] ids, int masterBusinessUnitId)
        {
            if (ids == null || ids.Length <= 0 || masterBusinessUnitId == 0)
                return Json("Pilih salah satu data yang akan dihapus.");

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);

            if (masterBusinessUnit == null)
                return Json("Kesalahan system. Mohon reload halaman ini!");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (int id in ids)
                    {
                        MasterBusinessUnitRegion obj = db.MasterBusinessUnitRegions.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == id).FirstOrDefault();
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitRegion tmp = obj;
                                    db.MasterBusinessUnitRegions.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitRegion, MenuId = tmp.MasterBusinessUnitId, MenuCode = tmp.MasterRegionId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        // End of MasterBusinessUnitRegion

        // Begin of MasterBusinessRegionSupplier

        [HttpGet]
        [Authorize(Roles = "MasterSuppliersAdd")]
        public PartialViewResult MasterSuppliersGrid(int masterBusinessUnitId, int masterRegionId)
        {
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterSuppliersGrid", db.Set<MasterBusinessRegionSupplier>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId));
        }

        [Authorize(Roles = "MasterSuppliersAdd")]
        public ActionResult AddMasterSuppliers(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(id);

            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }

            MasterBusinessRegionSupplierSelection obj = new MasterBusinessRegionSupplierSelection
            {
                MasterBusinessUnitId = masterBusinessUnit.Id,
                MasterBusinessUnit = masterBusinessUnit
            };

            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterSuppliersAdd", obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterSuppliersAdd")]
        public ActionResult AddMasterSuppliers([Bind(Include = "MasterBusinessUnitId, MasterRegionId, MasterSupplierStartId, MasterSupplierEndId")] MasterBusinessRegionSupplierSelection obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MasterSupplierEndId == null)
                {
                    MasterBusinessRegionSupplier temp = db.MasterBusinessRegionSuppliers.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterRegionId == obj.MasterRegionId && x.MasterSupplierId == obj.MasterSupplierStartId).FirstOrDefault();

                    if (temp == null)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                MasterBusinessRegionSupplier masterBusinessRegionSupplier = new MasterBusinessRegionSupplier
                                {
                                    MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                    MasterRegionId = obj.MasterRegionId,
                                    MasterSupplierId = obj.MasterSupplierStartId,
                                    Created = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MasterBusinessRegionSuppliers.Add(masterBusinessRegionSupplier);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionSupplier, MenuId = masterBusinessRegionSupplier.MasterBusinessUnitId, MenuCode = masterBusinessRegionSupplier.MasterSupplierId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
                else
                {
                    MasterSupplier masterSupplierStart = db.MasterSuppliers.Find(obj.MasterSupplierStartId);
                    MasterSupplier masterSupplierEnd = db.MasterSuppliers.Find(obj.MasterSupplierEndId);

                    var masterSuppliers = db.MasterSuppliers.Where(x => x.Code.CompareTo(masterSupplierStart.Code) >= 0 && x.Code.CompareTo(masterSupplierEnd.Code) <= 0).ToList();

                    foreach (MasterSupplier masterSupplier in masterSuppliers)
                    {
                        MasterBusinessRegionSupplier temp = db.MasterBusinessRegionSuppliers.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterRegionId == obj.MasterRegionId && x.MasterSupplierId == masterSupplier.Id).FirstOrDefault();
                        if (temp == null)
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessRegionSupplier masterBusinessRegionSupplier = new MasterBusinessRegionSupplier
                                    {
                                        MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                        MasterRegionId = obj.MasterRegionId,
                                        MasterSupplierId = masterSupplier.Id,
                                        Created = DateTime.Now,
                                        UserId = User.Identity.GetUserId<int>()
                                    };

                                    db.MasterBusinessRegionSuppliers.Add(masterBusinessRegionSupplier);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionSupplier, MenuId = masterBusinessRegionSupplier.MasterBusinessUnitId, MenuCode = masterBusinessRegionSupplier.MasterSupplierId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(obj.MasterBusinessUnitId);
            obj.MasterBusinessUnit = masterBusinessUnit;
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterSuppliersAdd", obj);
        }

        [HttpPost]
        [Authorize(Roles = "MasterSuppliersDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDeleteSuppliers(int[] ids, int masterBusinessUnitId, int masterRegionId)
        {
            if (ids == null || ids.Length <= 0 || masterBusinessUnitId == 0 || masterRegionId == 0)
                return Json("Pilih salah satu data yang akan dihapus.");

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);

            if (masterBusinessUnit == null)
                return Json("Kesalahan system. Mohon reload halaman ini!");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (int id in ids)
                    {
                        MasterBusinessRegionSupplier obj = db.MasterBusinessRegionSuppliers.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId && x.MasterSupplierId == id).FirstOrDefault();
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessRegionSupplier tmp = obj;
                                    db.MasterBusinessRegionSuppliers.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionSupplier, MenuId = tmp.MasterBusinessUnitId, MenuCode = tmp.MasterSupplierId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        // End of MasterBusinessRegionSupplier 

        // Begin of MasterBusinessUnitSalesPerson

        [HttpGet]
        [Authorize(Roles = "MasterSalesPersonsAdd")]
        public PartialViewResult MasterSalesPersonsGrid(int masterBusinessUnitId)
        {
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterSalesPersonsGrid", db.Set<MasterBusinessUnitSalesPerson>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == masterBusinessUnitId));
        }

        [Authorize(Roles = "MasterSalesPersonsAdd")]
        public ActionResult AddMasterSalesPersons(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(id);

            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }

            MasterBusinessUnitSalesPersonSelection obj = new MasterBusinessUnitSalesPersonSelection
            {
                MasterBusinessUnitId = masterBusinessUnit.Id,
                MasterBusinessUnit = masterBusinessUnit
            };

            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterSalesPersonsAdd", obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterSalesPersonsAdd")]
        public ActionResult AddMasterSalesPersons([Bind(Include = "MasterBusinessUnitId, MasterSalesPersonStartId, MasterSalesPersonEndId")] MasterBusinessUnitSalesPersonSelection obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MasterSalesPersonEndId == null)
                {
                    MasterBusinessUnitSalesPerson temp = db.MasterBusinessUnitSalesPersons.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterSalesPersonId == obj.MasterSalesPersonStartId).FirstOrDefault();

                    if (temp == null)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                MasterBusinessUnitSalesPerson masterBusinessUnitSalesPerson = new MasterBusinessUnitSalesPerson
                                {
                                    MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                    MasterSalesPersonId = obj.MasterSalesPersonStartId,
                                    Created = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MasterBusinessUnitSalesPersons.Add(masterBusinessUnitSalesPerson);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitSalesPerson, MenuId = masterBusinessUnitSalesPerson.MasterBusinessUnitId, MenuCode = masterBusinessUnitSalesPerson.MasterSalesPersonId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
                else
                {
                    MasterSalesPerson masterSalesPersonStart = db.MasterSalesPersons.Find(obj.MasterSalesPersonStartId);
                    MasterSalesPerson masterSalesPersonEnd = db.MasterSalesPersons.Find(obj.MasterSalesPersonEndId);

                    var masterSalesPersons = db.MasterSalesPersons.Where(x => x.Code.CompareTo(masterSalesPersonStart.Code) >= 0 && x.Code.CompareTo(masterSalesPersonEnd.Code) <= 0).ToList();

                    foreach (MasterSalesPerson masterSalesPerson in masterSalesPersons)
                    {
                        MasterBusinessUnitSalesPerson temp = db.MasterBusinessUnitSalesPersons.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterSalesPersonId == masterSalesPerson.Id).FirstOrDefault();
                        if (temp == null)
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitSalesPerson masterBusinessUnitSalesPerson = new MasterBusinessUnitSalesPerson
                                    {
                                        MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                        MasterSalesPersonId = masterSalesPerson.Id,
                                        Created = DateTime.Now,
                                        UserId = User.Identity.GetUserId<int>()
                                    };

                                    db.MasterBusinessUnitSalesPersons.Add(masterBusinessUnitSalesPerson);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitSalesPerson, MenuId = masterBusinessUnitSalesPerson.MasterBusinessUnitId, MenuCode = masterBusinessUnitSalesPerson.MasterSalesPersonId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(obj.MasterBusinessUnitId);
            obj.MasterBusinessUnit = masterBusinessUnit;
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterSalesPersonsAdd", obj);
        }

        [HttpPost]
        [Authorize(Roles = "MasterSalesPersonsDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDeleteSalesPersons(int[] ids, int masterBusinessUnitId)
        {
            if (ids == null || ids.Length <= 0 || masterBusinessUnitId == 0)
                return Json("Pilih salah satu data yang akan dihapus.");

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);

            if (masterBusinessUnit == null)
                return Json("Kesalahan system. Mohon reload halaman ini!");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (int id in ids)
                    {
                        MasterBusinessUnitSalesPerson obj = db.MasterBusinessUnitSalesPersons.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterSalesPersonId == id).FirstOrDefault();
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitSalesPerson tmp = obj;
                                    db.MasterBusinessUnitSalesPersons.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitSalesPerson, MenuId = tmp.MasterBusinessUnitId, MenuCode = tmp.MasterSalesPersonId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        // End of MasterBusinessUnitSalesPerson

        // Begin of MasterBusinessUnitBrand

        [HttpGet]
        [Authorize(Roles = "MasterBrandsAdd")]
        public PartialViewResult MasterBrandsGrid(int masterBusinessUnitId)
        {
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterBrandsGrid", db.Set<MasterBusinessUnitBrand>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == masterBusinessUnitId));
        }

        [Authorize(Roles = "MasterBrandsAdd")]
        public ActionResult AddMasterBrands(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(id);

            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }

            MasterBusinessUnitBrandSelection obj = new MasterBusinessUnitBrandSelection
            {
                MasterBusinessUnitId = masterBusinessUnit.Id,
                MasterBusinessUnit = masterBusinessUnit
            };

            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterBrandsAdd", obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterBrandsAdd")]
        public ActionResult AddMasterBrands([Bind(Include = "MasterBusinessUnitId, MasterBrandStartId, MasterBrandEndId")] MasterBusinessUnitBrandSelection obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MasterBrandEndId == null)
                {
                    MasterBusinessUnitBrand temp = db.MasterBusinessUnitBrands.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterBrandId == obj.MasterBrandStartId).FirstOrDefault();

                    if (temp == null)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                MasterBusinessUnitBrand masterBusinessUnitBrand = new MasterBusinessUnitBrand
                                {
                                    MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                    MasterBrandId = obj.MasterBrandStartId,
                                    Created = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MasterBusinessUnitBrands.Add(masterBusinessUnitBrand);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitBrand, MenuId = masterBusinessUnitBrand.MasterBusinessUnitId, MenuCode = masterBusinessUnitBrand.MasterBrandId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
                else
                {
                    MasterBrand masterBrandStart = db.MasterBrands.Find(obj.MasterBrandStartId);
                    MasterBrand masterBrandEnd = db.MasterBrands.Find(obj.MasterBrandEndId);

                    var masterBrands = db.MasterBrands.Where(x => x.Code.CompareTo(masterBrandStart.Code) >= 0 && x.Code.CompareTo(masterBrandEnd.Code) <= 0).ToList();

                    foreach (MasterBrand masterBrand in masterBrands)
                    {
                        MasterBusinessUnitBrand temp = db.MasterBusinessUnitBrands.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterBrandId == masterBrand.Id).FirstOrDefault();
                        if (temp == null)
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitBrand masterBusinessUnitBrand = new MasterBusinessUnitBrand
                                    {
                                        MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                        MasterBrandId = masterBrand.Id,
                                        Created = DateTime.Now,
                                        UserId = User.Identity.GetUserId<int>()
                                    };

                                    db.MasterBusinessUnitBrands.Add(masterBusinessUnitBrand);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitBrand, MenuId = masterBusinessUnitBrand.MasterBusinessUnitId, MenuCode = masterBusinessUnitBrand.MasterBrandId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(obj.MasterBusinessUnitId);
            obj.MasterBusinessUnit = masterBusinessUnit;
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterBrandsAdd", obj);
        }

        [HttpPost]
        [Authorize(Roles = "MasterBrandsDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDeleteBrands(int[] ids, int masterBusinessUnitId)
        {
            if (ids == null || ids.Length <= 0 || masterBusinessUnitId == 0)
                return Json("Pilih salah satu data yang akan dihapus.");

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);

            if (masterBusinessUnit == null)
                return Json("Kesalahan system. Mohon reload halaman ini!");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (int id in ids)
                    {
                        MasterBusinessUnitBrand obj = db.MasterBusinessUnitBrands.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterBrandId == id).FirstOrDefault();
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitBrand tmp = obj;
                                    db.MasterBusinessUnitBrands.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitBrand, MenuId = tmp.MasterBusinessUnitId, MenuCode = tmp.MasterBrandId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        // End of MasterBusinessUnitBrand

        // Begin of MasterBusinessUnitItem

        [HttpGet]
        [Authorize(Roles = "MasterItemsAdd")]
        public PartialViewResult MasterItemsGrid(int masterBusinessUnitId)
        {
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterItemsGrid", db.Set<MasterBusinessUnitItem>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == masterBusinessUnitId));
        }

        [Authorize(Roles = "MasterItemsAdd")]
        public ActionResult AddMasterItems(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(id);

            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }

            MasterBusinessUnitItemSelection obj = new MasterBusinessUnitItemSelection
            {
                MasterBusinessUnitId = masterBusinessUnit.Id,
                MasterBusinessUnit = masterBusinessUnit
            };

            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterItemsAdd", obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterItemsAdd")]
        public ActionResult AddMasterItems([Bind(Include = "MasterBusinessUnitId, MasterItemStartId, MasterItemEndId")] MasterBusinessUnitItemSelection obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MasterItemEndId == null)
                {
                    MasterBusinessUnitItem temp = db.MasterBusinessUnitItems.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterItemId == obj.MasterItemStartId).FirstOrDefault();

                    if (temp == null)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                MasterBusinessUnitItem masterBusinessUnitItem = new MasterBusinessUnitItem
                                {
                                    MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                    MasterItemId = obj.MasterItemStartId,
                                    Created = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MasterBusinessUnitItems.Add(masterBusinessUnitItem);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitItem, MenuId = masterBusinessUnitItem.MasterBusinessUnitId, MenuCode = masterBusinessUnitItem.MasterItemId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
                else
                {
                    MasterItem masterItemStart = db.MasterItems.Find(obj.MasterItemStartId);
                    MasterItem masterItemEnd = db.MasterItems.Find(obj.MasterItemEndId);

                    var masterItems = db.MasterItems.Where(x => x.Code.CompareTo(masterItemStart.Code) >= 0 && x.Code.CompareTo(masterItemEnd.Code) <= 0).ToList();

                    foreach (MasterItem masterItem in masterItems)
                    {
                        MasterBusinessUnitItem temp = db.MasterBusinessUnitItems.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterItemId == masterItem.Id).FirstOrDefault();
                        if (temp == null)
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitItem masterBusinessUnitItem = new MasterBusinessUnitItem
                                    {
                                        MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                        MasterItemId = masterItem.Id,
                                        Created = DateTime.Now,
                                        UserId = User.Identity.GetUserId<int>()
                                    };

                                    db.MasterBusinessUnitItems.Add(masterBusinessUnitItem);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitItem, MenuId = masterBusinessUnitItem.MasterBusinessUnitId, MenuCode = masterBusinessUnitItem.MasterItemId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(obj.MasterBusinessUnitId);
            obj.MasterBusinessUnit = masterBusinessUnit;
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterItemsAdd", obj);
        }

        [HttpPost]
        [Authorize(Roles = "MasterItemsDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDeleteItems(int[] ids, int masterBusinessUnitId)
        {
            if (ids == null || ids.Length <= 0 || masterBusinessUnitId == 0)
                return Json("Pilih salah satu data yang akan dihapus.");

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);

            if (masterBusinessUnit == null)
                return Json("Kesalahan system. Mohon reload halaman ini!");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (int id in ids)
                    {
                        MasterBusinessUnitItem obj = db.MasterBusinessUnitItems.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterItemId == id).FirstOrDefault();
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitItem tmp = obj;
                                    db.MasterBusinessUnitItems.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitItem, MenuId = tmp.MasterBusinessUnitId, MenuCode = tmp.MasterItemId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        // End of MasterBusinessUnitItem

        // Begin of MasterBusinessUnitCategory

        [HttpGet]
        [Authorize(Roles = "MasterCategoriesAdd")]
        public PartialViewResult MasterCategoriesGrid(int masterBusinessUnitId)
        {
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterCategoriesGrid", db.Set<MasterBusinessUnitCategory>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == masterBusinessUnitId));
        }

        [Authorize(Roles = "MasterCategoriesAdd")]
        public ActionResult AddMasterCategories(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(id);

            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }

            MasterBusinessUnitCategorySelection obj = new MasterBusinessUnitCategorySelection
            {
                MasterBusinessUnitId = masterBusinessUnit.Id,
                MasterBusinessUnit = masterBusinessUnit
            };

            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterCategoriesAdd", obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCategoriesAdd")]
        public ActionResult AddMasterCategories([Bind(Include = "MasterBusinessUnitId, MasterCategoryStartId, MasterCategoryEndId")] MasterBusinessUnitCategorySelection obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MasterCategoryEndId == null)
                {
                    MasterBusinessUnitCategory temp = db.MasterBusinessUnitCategories.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterCategoryId == obj.MasterCategoryStartId).FirstOrDefault();

                    if (temp == null)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                MasterBusinessUnitCategory masterBusinessUnitCategory = new MasterBusinessUnitCategory
                                {
                                    MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                    MasterCategoryId = obj.MasterCategoryStartId,
                                    Created = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MasterBusinessUnitCategories.Add(masterBusinessUnitCategory);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitCategory, MenuId = masterBusinessUnitCategory.MasterBusinessUnitId, MenuCode = masterBusinessUnitCategory.MasterCategoryId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
                else
                {
                    MasterCategory masterCategoriestart = db.MasterCategories.Find(obj.MasterCategoryStartId);
                    MasterCategory masterCategoryEnd = db.MasterCategories.Find(obj.MasterCategoryEndId);

                    var masterCategories = db.MasterCategories.Where(x => x.Code.CompareTo(masterCategoriestart.Code) >= 0 && x.Code.CompareTo(masterCategoryEnd.Code) <= 0).ToList();

                    foreach (MasterCategory masterCategory in masterCategories)
                    {
                        MasterBusinessUnitCategory temp = db.MasterBusinessUnitCategories.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterCategoryId == masterCategory.Id).FirstOrDefault();
                        if (temp == null)
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitCategory masterBusinessUnitCategory = new MasterBusinessUnitCategory
                                    {
                                        MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                        MasterCategoryId = masterCategory.Id,
                                        Created = DateTime.Now,
                                        UserId = User.Identity.GetUserId<int>()
                                    };

                                    db.MasterBusinessUnitCategories.Add(masterBusinessUnitCategory);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitCategory, MenuId = masterBusinessUnitCategory.MasterBusinessUnitId, MenuCode = masterBusinessUnitCategory.MasterCategoryId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(obj.MasterBusinessUnitId);
            obj.MasterBusinessUnit = masterBusinessUnit;
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterCategoriesAdd", obj);
        }

        [HttpPost]
        [Authorize(Roles = "MasterCategoriesDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDeleteCategories(int[] ids, int masterBusinessUnitId)
        {
            if (ids == null || ids.Length <= 0 || masterBusinessUnitId == 0)
                return Json("Pilih salah satu data yang akan dihapus.");

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);

            if (masterBusinessUnit == null)
                return Json("Kesalahan system. Mohon reload halaman ini!");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (int id in ids)
                    {
                        MasterBusinessUnitCategory obj = db.MasterBusinessUnitCategories.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterCategoryId == id).FirstOrDefault();
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitCategory tmp = obj;
                                    db.MasterBusinessUnitCategories.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitCategory, MenuId = tmp.MasterBusinessUnitId, MenuCode = tmp.MasterCategoryId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        // End of MasterBusinessUnitCategory

        // Begin of MasterBusinessRegionWarehouse

        [HttpGet]
        [Authorize(Roles = "MasterWarehousesAdd")]
        public PartialViewResult MasterWarehousesGrid(int masterBusinessUnitId, int masterRegionId)
        {
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterWarehousesGrid", db.Set<MasterBusinessRegionWarehouse>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId));
        }

        [Authorize(Roles = "MasterWarehousesAdd")]
        public ActionResult AddMasterWarehouses(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(id);

            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }

            MasterBusinessRegionWarehouseSelection obj = new MasterBusinessRegionWarehouseSelection
            {
                MasterBusinessUnitId = masterBusinessUnit.Id,
                MasterBusinessUnit = masterBusinessUnit
            };

            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterWarehousesAdd", obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterWarehousesAdd")]
        public ActionResult AddMasterWarehouses([Bind(Include = "MasterBusinessUnitId, MasterRegionId, MasterWarehouseStartId, MasterWarehouseEndId")] MasterBusinessRegionWarehouseSelection obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MasterWarehouseEndId == null)
                {
                    MasterBusinessRegionWarehouse temp = db.MasterBusinessRegionWarehouses.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterRegionId == obj.MasterRegionId && x.MasterWarehouseId == obj.MasterWarehouseStartId).FirstOrDefault();

                    if (temp == null)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                MasterBusinessRegionWarehouse masterBusinessRegionWarehouse = new MasterBusinessRegionWarehouse
                                {
                                    MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                    MasterRegionId = obj.MasterRegionId,
                                    MasterWarehouseId = obj.MasterWarehouseStartId,
                                    Created = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MasterBusinessRegionWarehouses.Add(masterBusinessRegionWarehouse);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionWarehouse, MenuId = masterBusinessRegionWarehouse.MasterBusinessUnitId, MenuCode = masterBusinessRegionWarehouse.MasterWarehouseId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
                else
                {
                    MasterWarehouse masterWarehouseStart = db.MasterWarehouses.Find(obj.MasterWarehouseStartId);
                    MasterWarehouse masterWarehouseEnd = db.MasterWarehouses.Find(obj.MasterWarehouseEndId);

                    var masterWarehouses = db.MasterWarehouses.Where(x => x.Code.CompareTo(masterWarehouseStart.Code) >= 0 && x.Code.CompareTo(masterWarehouseEnd.Code) <= 0).ToList();

                    foreach (MasterWarehouse masterWarehouse in masterWarehouses)
                    {
                        MasterBusinessRegionWarehouse temp = db.MasterBusinessRegionWarehouses.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterRegionId == obj.MasterRegionId && x.MasterWarehouseId == masterWarehouse.Id).FirstOrDefault();
                        if (temp == null)
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessRegionWarehouse masterBusinessRegionWarehouse = new MasterBusinessRegionWarehouse
                                    {
                                        MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                        MasterRegionId = obj.MasterRegionId,
                                        MasterWarehouseId = masterWarehouse.Id,
                                        Created = DateTime.Now,
                                        UserId = User.Identity.GetUserId<int>()
                                    };

                                    db.MasterBusinessRegionWarehouses.Add(masterBusinessRegionWarehouse);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionWarehouse, MenuId = masterBusinessRegionWarehouse.MasterBusinessUnitId, MenuCode = masterBusinessRegionWarehouse.MasterWarehouseId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(obj.MasterBusinessUnitId);
            obj.MasterBusinessUnit = masterBusinessUnit;
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterWarehousesAdd", obj);
        }

        [HttpPost]
        [Authorize(Roles = "MasterWarehousesDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDeleteWarehouses(int[] ids, int masterBusinessUnitId, int masterRegionId)
        {
            if (ids == null || ids.Length <= 0 || masterBusinessUnitId == 0 || masterRegionId == 0)
                return Json("Pilih salah satu data yang akan dihapus.");

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);

            if (masterBusinessUnit == null)
                return Json("Kesalahan system. Mohon reload halaman ini!");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (int id in ids)
                    {
                        MasterBusinessRegionWarehouse obj = db.MasterBusinessRegionWarehouses.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId && x.MasterWarehouseId == id).FirstOrDefault();
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessRegionWarehouse tmp = obj;
                                    db.MasterBusinessRegionWarehouses.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionWarehouse, MenuId = tmp.MasterBusinessUnitId, MenuCode = tmp.MasterWarehouseId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        // End of MasterBusinessRegionWarehouse

        // Begin of MasterBusinessRegionAccount

        [HttpGet]
        [Authorize(Roles = "ChartOfAccountsAdd")]
        public PartialViewResult MasterAccountsGrid(int masterBusinessUnitId, int masterRegionId)
        {
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterAccountsGrid", db.Set<MasterBusinessRegionAccount>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId));
        }

        [Authorize(Roles = "ChartOfAccountsAdd")]
        public ActionResult AddMasterAccounts(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(id);

            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }

            MasterBusinessRegionAccountSelection obj = new MasterBusinessRegionAccountSelection
            {
                MasterBusinessUnitId = masterBusinessUnit.Id,
                MasterBusinessUnit = masterBusinessUnit
            };

            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterAccountsAdd", obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ChartOfAccountsAdd")]
        public ActionResult AddMasterAccounts([Bind(Include = "MasterBusinessUnitId, MasterRegionId, ChartOfAccountStartId, ChartOfAccountEndId")] MasterBusinessRegionAccountSelection obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.ChartOfAccountEndId == null)
                {
                    MasterBusinessRegionAccount temp = db.MasterBusinessRegionAccounts.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterRegionId == obj.MasterRegionId && x.ChartOfAccountId == obj.ChartOfAccountStartId).FirstOrDefault();

                    if (temp == null)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                MasterBusinessRegionAccount masterBusinessRegionAccount = new MasterBusinessRegionAccount
                                {
                                    MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                    MasterRegionId = obj.MasterRegionId,
                                    ChartOfAccountId= obj.ChartOfAccountStartId,
                                    Created = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MasterBusinessRegionAccounts.Add(masterBusinessRegionAccount);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionAccount, MenuId = masterBusinessRegionAccount.MasterBusinessUnitId, MenuCode = masterBusinessRegionAccount.ChartOfAccountId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
                else
                {
                    ChartOfAccount chartOfAccountStart = db.ChartOfAccounts.Find(obj.ChartOfAccountStartId);
                    ChartOfAccount chartOfAccountEnd = db.ChartOfAccounts.Find(obj.ChartOfAccountEndId);

                    var chartOfAccounts = db.ChartOfAccounts.Where(x => x.Code.CompareTo(chartOfAccountStart.Code) >= 0 && x.Code.CompareTo(chartOfAccountEnd.Code) <= 0).ToList();

                    foreach (ChartOfAccount chartOfAccount in chartOfAccounts)
                    {
                        MasterBusinessRegionAccount temp = db.MasterBusinessRegionAccounts.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterRegionId == obj.MasterRegionId && x.ChartOfAccountId == chartOfAccount.Id).FirstOrDefault();
                        if (temp == null)
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessRegionAccount masterBusinessRegionAccount = new MasterBusinessRegionAccount
                                    {
                                        MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                        MasterRegionId = obj.MasterRegionId,
                                        ChartOfAccountId = chartOfAccount.Id,
                                        Created = DateTime.Now,
                                        UserId = User.Identity.GetUserId<int>()
                                    };

                                    db.MasterBusinessRegionAccounts.Add(masterBusinessRegionAccount);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionAccount, MenuId = masterBusinessRegionAccount.MasterBusinessUnitId, MenuCode = masterBusinessRegionAccount.ChartOfAccountId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(obj.MasterBusinessUnitId);
            obj.MasterBusinessUnit = masterBusinessUnit;
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterRegionsAdd", obj);
        }

        [HttpPost]
        [Authorize(Roles = "ChartOfAccountsDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDeleteAccounts(int[] ids, int masterBusinessUnitId, int masterRegionId)
        {
            if (ids == null || ids.Length <= 0 || masterBusinessUnitId == 0 || masterRegionId == 0)
                return Json("Pilih salah satu data yang akan dihapus.");

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);

            if (masterBusinessUnit == null)
                return Json("Kesalahan system. Mohon reload halaman ini!");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (int id in ids)
                    {
                        MasterBusinessRegionAccount obj = db.MasterBusinessRegionAccounts.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId && x.ChartOfAccountId ==id).FirstOrDefault();
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessRegionAccount tmp = obj;
                                    db.MasterBusinessRegionAccounts.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionAccount, MenuId = tmp.MasterBusinessUnitId, MenuCode = tmp.ChartOfAccountId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        // End of MasterBusinessRegionAccount

        // Begin of MasterBusinessRegionCustomer

        [HttpGet]
        [Authorize(Roles = "MasterCustomersAdd")]
        public PartialViewResult MasterCustomersGrid(int masterBusinessUnitId, int masterRegionId)
        {
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterCustomersGrid", db.Set<MasterBusinessRegionCustomer>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId));
        }

        [Authorize(Roles = "MasterCustomersAdd")]
        public ActionResult AddMasterCustomers(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(id);

            if (masterBusinessUnit == null)
            {
                return HttpNotFound();
            }

            MasterBusinessRegionCustomerSelection obj = new MasterBusinessRegionCustomerSelection
            {
                MasterBusinessUnitId = masterBusinessUnit.Id,
                MasterBusinessUnit = masterBusinessUnit
            };

            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterCustomersAdd", obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterCustomersAdd")]
        public ActionResult AddMasterCustomers([Bind(Include = "MasterBusinessUnitId, MasterRegionId, MasterCustomerStartId, MasterCustomerEndId")] MasterBusinessRegionCustomerSelection obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MasterCustomerEndId == null)
                {
                    MasterBusinessRegionCustomer temp = db.MasterBusinessRegionCustomers.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterRegionId == obj.MasterRegionId && x.MasterCustomerId == obj.MasterCustomerStartId).FirstOrDefault();

                    if (temp == null)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                MasterBusinessRegionCustomer masterBusinessRegionCustomer = new MasterBusinessRegionCustomer
                                {
                                    MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                    MasterRegionId = obj.MasterRegionId,
                                    MasterCustomerId = obj.MasterCustomerStartId,
                                    Created = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MasterBusinessRegionCustomers.Add(masterBusinessRegionCustomer);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionCustomer, MenuId = masterBusinessRegionCustomer.MasterBusinessUnitId, MenuCode = masterBusinessRegionCustomer.MasterCustomerId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
                else
                {
                    MasterCustomer masterCustomerStart = db.MasterCustomers.Find(obj.MasterCustomerStartId);
                    MasterCustomer masterCustomerEnd = db.MasterCustomers.Find(obj.MasterCustomerEndId);

                    var masterCustomers = db.MasterCustomers.Where(x => x.Code.CompareTo(masterCustomerStart.Code) >= 0 && x.Code.CompareTo(masterCustomerEnd.Code) <= 0).ToList();

                    foreach (MasterCustomer masterCustomer in masterCustomers)
                    {
                        MasterBusinessRegionCustomer temp = db.MasterBusinessRegionCustomers.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterRegionId == obj.MasterRegionId && x.MasterCustomerId == masterCustomer.Id).FirstOrDefault();
                        if (temp == null)
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessRegionCustomer masterBusinessRegionCustomer = new MasterBusinessRegionCustomer
                                    {
                                        MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                        MasterRegionId = obj.MasterRegionId,
                                        MasterCustomerId = masterCustomer.Id,
                                        Created = DateTime.Now,
                                        UserId = User.Identity.GetUserId<int>()
                                    };

                                    db.MasterBusinessRegionCustomers.Add(masterBusinessRegionCustomer);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionCustomer, MenuId = masterBusinessRegionCustomer.MasterBusinessUnitId, MenuCode = masterBusinessRegionCustomer.MasterCustomerId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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

                return Json("success", JsonRequestBehavior.AllowGet);
            }

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(obj.MasterBusinessUnitId);
            obj.MasterBusinessUnit = masterBusinessUnit;
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterCustomersAdd", obj);
        }

        [HttpPost]
        [Authorize(Roles = "MasterCustomersDelete")]
        [ValidateJsonAntiForgeryToken]
        public ActionResult BatchDeleteCustomers(int[] ids, int masterBusinessUnitId, int masterRegionId)
        {
            if (ids == null || ids.Length <= 0 || masterBusinessUnitId == 0 || masterRegionId == 0)
                return Json("Pilih salah satu data yang akan dihapus.");

            MasterBusinessUnit masterBusinessUnit = db.MasterBusinessUnits.Find(masterBusinessUnitId);

            if (masterBusinessUnit == null)
                return Json("Kesalahan system. Mohon reload halaman ini!");
            else
            {
                using (db)
                {
                    int failed = 0;
                    foreach (int id in ids)
                    {
                        MasterBusinessRegionCustomer obj = db.MasterBusinessRegionCustomers.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterRegionId == masterRegionId && x.MasterCustomerId == id).FirstOrDefault();
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessRegionCustomer tmp = obj;
                                    db.MasterBusinessRegionCustomers.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessRegionCustomer, MenuId = tmp.MasterBusinessUnitId, MenuCode = tmp.MasterCustomerId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        // End of MasterBusinessRegionCustomer

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
