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
        [Authorize(Roles = "MasterSuppliersAdd")]
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
