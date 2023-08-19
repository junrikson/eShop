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

        // Begin of MasterBusinessUnitSupplier

        [HttpGet]
        [Authorize(Roles = "MasterSuppliersAdd")]
        public PartialViewResult MasterSuppliersGrid(int masterBusinessUnitId)
        {
            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterSuppliersGrid", db.Set<MasterBusinessUnitSupplier>().AsQueryable()
                    .Where(x => x.MasterBusinessUnitId == masterBusinessUnitId));
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

            MasterBusinessUnitSupplierSelection obj = new MasterBusinessUnitSupplierSelection
            {
                MasterBusinessUnitId = masterBusinessUnit.Id,
                MasterBusinessUnit = masterBusinessUnit
            };

            return PartialView("../Masters/MasterBusinessUnits/DataSelection/_MasterSuppliersAdd", obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MasterSuppliersAdd")]
        public ActionResult AddMasterSuppliers([Bind(Include = "MasterBusinessUnitId, MasterSupplierStartId, MasterSupplierEndId")] MasterBusinessUnitSupplierSelection obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MasterSupplierEndId == null)
                {
                    MasterBusinessUnitSupplier temp = db.MasterBusinessUnitSuppliers.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterSupplierId == obj.MasterSupplierStartId).FirstOrDefault();

                    if (temp == null)
                    {
                        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                MasterBusinessUnitSupplier masterBusinessUnitSupplier = new MasterBusinessUnitSupplier
                                {
                                    MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                    MasterSupplierId = obj.MasterSupplierStartId,
                                    Created = DateTime.Now,
                                    UserId = User.Identity.GetUserId<int>()
                                };

                                db.MasterBusinessUnitSuppliers.Add(masterBusinessUnitSupplier);
                                db.SaveChanges();

                                db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitSupplier, MenuId = masterBusinessUnitSupplier.MasterBusinessUnitId, MenuCode = masterBusinessUnitSupplier.MasterSupplierId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
                        MasterBusinessUnitSupplier temp = db.MasterBusinessUnitSuppliers.Where(x => x.MasterBusinessUnitId == obj.MasterBusinessUnitId && x.MasterSupplierId == masterSupplier.Id).FirstOrDefault();
                        if (temp == null)
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitSupplier masterBusinessUnitSupplier = new MasterBusinessUnitSupplier
                                    {
                                        MasterBusinessUnitId = obj.MasterBusinessUnitId,
                                        MasterSupplierId = masterSupplier.Id,
                                        Created = DateTime.Now,
                                        UserId = User.Identity.GetUserId<int>()
                                    };

                                    db.MasterBusinessUnitSuppliers.Add(masterBusinessUnitSupplier);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitSupplier, MenuId = masterBusinessUnitSupplier.MasterBusinessUnitId, MenuCode = masterBusinessUnitSupplier.MasterSupplierId.ToString(), Actions = EnumActions.CREATE, UserId = User.Identity.GetUserId<int>() });
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
        public ActionResult BatchDeleteSuppliers(int[] ids, int masterBusinessUnitId)
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
                        MasterBusinessUnitSupplier obj = db.MasterBusinessUnitSuppliers.Where(x => x.MasterBusinessUnitId == masterBusinessUnitId && x.MasterSupplierId == id).FirstOrDefault();
                        if (obj == null)
                            failed++;
                        else
                        {
                            using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    MasterBusinessUnitSupplier tmp = obj;
                                    db.MasterBusinessUnitSuppliers.Remove(obj);
                                    db.SaveChanges();

                                    db.SystemLogs.Add(new SystemLog { Date = DateTime.Now, MenuType = EnumMenuType.MasterBusinessUnitSupplier, MenuId = tmp.MasterBusinessUnitId, MenuCode = tmp.MasterSupplierId.ToString(), Actions = EnumActions.DELETE, UserId = User.Identity.GetUserId<int>() });
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

        // End of MasterBusinessUnitSupplier

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
