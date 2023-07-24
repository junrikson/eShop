using CrystalDecisions.CrystalReports.Engine;
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