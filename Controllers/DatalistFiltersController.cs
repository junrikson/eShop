﻿using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Datalist;
using eShop.Models;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace eShop.Controllers
{
    [Authorize]
    public class DatalistFiltersController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        public JsonResult AllMasterRegion(DatalistFilter filter)
        {
            MasterRegionDatalist datalist = new MasterRegionDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterSupplier(DatalistFilter filter)
        {
            MasterSupplierDatalist datalist = new MasterSupplierDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterBrand(DatalistFilter filter)
        {
            MasterBrandDatalist datalist = new MasterBrandDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterCategory(DatalistFilter filter)
        {
            MasterCategoryDatalist datalist = new MasterCategoryDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterUnit(DatalistFilter filter)
        {
            MasterUnitDatalist datalist = new MasterUnitDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterCurrency(DatalistFilter filter)
        {
            MasterCurrencyDatalist datalist = new MasterCurrencyDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterItemSupplier(DatalistFilter filter, int? masterSupplierId = 0)
        {
            MasterItemSupplierDatalist datalist = new MasterItemSupplierDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            filter.AdditionalFilters["MasterSupplierId"] = masterSupplierId;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterCustomer(DatalistFilter filter)
        {
            MasterCustomerDatalist datalist = new MasterCustomerDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterWarehouseRegion(DatalistFilter filter, int? masterRegionId = 0)
        {
            MasterWarehouseRegionDatalist datalist = new MasterWarehouseRegionDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            filter.AdditionalFilters["MasterRegionId"] = masterRegionId;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterItemUnit(DatalistFilter filter, int? masterUnitId = 0)
        {
            MasterItemUnitDatalist datalist = new MasterItemUnitDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            filter.AdditionalFilters["MasterUnitId"] = masterUnitId;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllPurchase(DatalistFilter filter)
        {
            PurchaseDatalist datalist = new PurchaseDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllSale(DatalistFilter filter)
        {
            SaleDatalist datalist = new SaleDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllAuthorization(DatalistFilter filter)
        {
            AuthorizationDatalist datalist = new AuthorizationDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }
    }

    public class CrystalReportPdfResult : ActionResult
    {
        private readonly byte[] _contentBytes;
        private readonly string fileName = "";

        public CrystalReportPdfResult(ReportDocument reportDocument, string fileName)
        {
            _contentBytes = StreamToBytes(reportDocument.ExportToStream(ExportFormatType.PortableDocFormat));
            this.fileName = fileName;
        }

        public override void ExecuteResult(ControllerContext context)
        {

            var response = context.HttpContext.ApplicationInstance.Response;
            response.Clear();
            response.Buffer = false;
            response.ClearContent();
            response.ClearHeaders();
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.ContentType = "application/pdf";
            response.AppendHeader("content-disposition", string.Format("inline;FileName=\"{0}\"", fileName));

            using (var stream = new MemoryStream(_contentBytes))
            {
                stream.WriteTo(response.OutputStream);
                stream.Flush();
            }
        }

        private static byte[] StreamToBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }

    public class CrystalReportExcelResult : ActionResult
    {
        private readonly byte[] _contentBytes;
        private readonly string fileName = "";

        public CrystalReportExcelResult(ReportDocument reportDocument, string fileName)
        {
            _contentBytes = StreamToBytes(reportDocument.ExportToStream(ExportFormatType.Excel));
            this.fileName = fileName;
        }

        public override void ExecuteResult(ControllerContext context)
        {

            var response = context.HttpContext.ApplicationInstance.Response;
            response.Clear();
            response.Buffer = false;
            response.ClearContent();
            response.ClearHeaders();
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.ContentType = "application/vnd.ms-excel";
            response.AppendHeader("content-disposition", string.Format("inline;FileName=\"{0}\"", fileName));

            using (var stream = new MemoryStream(_contentBytes))
            {
                stream.WriteTo(response.OutputStream);
                stream.Flush();
            }
        }

        private static byte[] StreamToBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
    public class CrystalReportCsvResult : ActionResult
    {
        private readonly byte[] _contentBytes;
        private readonly string fileName = "";

        public CrystalReportCsvResult(ReportDocument reportDocument, string fileName)
        {
            _contentBytes = StreamToBytes(reportDocument.ExportToStream(ExportFormatType.CharacterSeparatedValues));
            this.fileName = fileName;
        }

        public override void ExecuteResult(ControllerContext context)
        {

            var response = context.HttpContext.ApplicationInstance.Response;
            response.Clear();
            response.Buffer = false;
            response.ClearContent();
            response.ClearHeaders();
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.ContentType = "text/csv";
            response.AppendHeader("content-disposition", string.Format("attachment;FileName=\"{0}\"", fileName));

            using (var stream = new MemoryStream(_contentBytes))
            {
                stream.WriteTo(response.OutputStream);
                stream.Flush();
            }
        }

        private static byte[] StreamToBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}