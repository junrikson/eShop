using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Datalist;
using eShop.Models;
using System.IO;
using System.Threading;
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
        public JsonResult AllMasterBusinessUnitRegion(DatalistFilter filter, int MasterBusinessUnitId = 0)
        {
            MasterBusinessUnitRegionDatalist datalist = new MasterBusinessUnitRegionDatalist(db);

            filter.AdditionalFilters["MasterRegionActive"] = true;
            filter.AdditionalFilters["MasterBusinessUnitId"] = MasterBusinessUnitId;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllChartOfAccount(DatalistFilter filter)
        {
            ChartOfAccountDatalist datalist = new ChartOfAccountDatalist(db);

            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterBusinessRegionAccount(DatalistFilter filter, int MasterBusinessUnitId = 0, int MasterRegionId = 0, bool IsHeader = false)
        {
            MasterBusinessRegionAccountDatalist datalist = new MasterBusinessRegionAccountDatalist(db);

            filter.AdditionalFilters["Active"] = true;
            filter.AdditionalFilters["MasterBusinessUnitId"] = MasterBusinessUnitId;
            filter.AdditionalFilters["MasterRegionId"] = MasterRegionId;

            if(!IsHeader)
                filter.AdditionalFilters["IsHeader"] = false;

            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterBankRegion(DatalistFilter filter, int MasterBusinessUnitId = 0, int MasterRegionId = 0, EnumBankType? Type = null)
        {
            MasterBankRegionDatalist datalist = new MasterBankRegionDatalist(db);

            filter.AdditionalFilters["Active"] = true;
            filter.AdditionalFilters["Type"] = Type;
            filter.AdditionalFilters["MasterBusinessUnitId"] = MasterBusinessUnitId;
            filter.AdditionalFilters["MasterRegionId"] = MasterRegionId;
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
        public JsonResult AllMasterItem(DatalistFilter filter)
        {
            AllMasterItemDatalist datalist = new AllMasterItemDatalist(db);
            filter.AdditionalFilters["Active"] = true;
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
        public JsonResult AllMasterSalesPerson(DatalistFilter filter)
        {
            MasterSalesPersonDatalist datalist = new MasterSalesPersonDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterWarehouse(DatalistFilter filter)
        {
            MasterWarehouseDatalist datalist = new MasterWarehouseDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllMasterBusinessRegionWarehouse(DatalistFilter filter, int MasterBusinessUnitId = 0, int MasterRegionId = 0)
        {
            MasterBusinessWarehouseDatalist datalist = new MasterBusinessWarehouseDatalist(db);

            filter.AdditionalFilters["MasterBusinessUnitId"] = MasterBusinessUnitId;
            filter.AdditionalFilters["MasterRegionId"] = MasterRegionId;
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
        public JsonResult AllOutstandingPurchaseRequest(DatalistFilter filter, int? masterRegionId = 0, int? masterBusinessUnitId = 0)
        {
            OutstandingPurchaseRequestDatalist datalist = new OutstandingPurchaseRequestDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            filter.AdditionalFilters["MasterRegionId"] = masterRegionId;
            filter.AdditionalFilters["MasterBusinessUnitId"] = masterBusinessUnitId;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllOutstandingPurchaseOrder(DatalistFilter filter, int? masterRegionId = 0, int? masterBusinessUnitId = 0)
        {
            OutstandingPurchaseOrderDatalist datalist = new OutstandingPurchaseOrderDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            filter.AdditionalFilters["MasterRegionId"] = masterRegionId;
            filter.AdditionalFilters["MasterBusinessUnitId"] = masterBusinessUnitId;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllOutstandingPurchase(DatalistFilter filter, int? masterRegionId = 0, int? masterBusinessUnitId = 0)
        {
            OutstandingPurchaseDatalist datalist = new OutstandingPurchaseDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            filter.AdditionalFilters["MasterRegionId"] = masterRegionId;
            filter.AdditionalFilters["MasterBusinessUnitId"] = masterBusinessUnitId;
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
        public JsonResult AllOutstandingSalesRequest(DatalistFilter filter, int? masterRegionId = 0, int? masterBusinessUnitId = 0)
        {
            OutstandingSalesRequestDatalist datalist = new OutstandingSalesRequestDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            filter.AdditionalFilters["MasterRegionId"] = masterRegionId;
            filter.AdditionalFilters["MasterBusinessUnitId"] = masterBusinessUnitId;
            datalist.Filter = filter;

            return Json(datalist.GetData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AllOutstandingSalesOrder(DatalistFilter filter, int? masterRegionId = 0, int? masterBusinessUnitId = 0)
        {
            OutstandingSalesOrderDatalist datalist = new OutstandingSalesOrderDatalist(db);
            filter.AdditionalFilters["Active"] = true;
            filter.AdditionalFilters["MasterRegionId"] = masterRegionId;
            filter.AdditionalFilters["MasterBusinessUnitId"] = masterBusinessUnitId;
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