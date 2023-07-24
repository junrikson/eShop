using System.Web.Optimization;

namespace eShop
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Styles
            bundles.Add(new StyleBundle("~/style/bootsrap").Include(
                        "~/bower_components/bootstrap/dist/css/bootstrap.min.css"));

            bundles.Add(new StyleBundle("~/style/font-awesome").Include(
                        "~/bower_components/font-awesome/css/font-awesome.min.css"));

            bundles.Add(new StyleBundle("~/style/ionicons").Include(
                        "~/bower_components/Ionicons/css/ionicons.min.css"));

            bundles.Add(new StyleBundle("~/style/AdminLTE").Include(
                        "~/dist/css/AdminLTE.css"));

            bundles.Add(new StyleBundle("~/style/iCheck").Include(
                        "~/plugins/iCheck/square/blue.css"));

            bundles.Add(new StyleBundle("~/style/GoogleFont").Include(
                        "~/fonts/source-sans-pro.css"));

            bundles.Add(new StyleBundle("~/style/Select2").Include(
                        "~/bower_components/select2/dist/css/select2.min.css"));

            bundles.Add(new StyleBundle("~/style/datatables").Include(
                      "~/Content/datatables.min.css"));

            bundles.Add(new StyleBundle("~/style/loading").Include(
                      "~/Content/loading.css"));

            bundles.Add(new StyleBundle("~/style/mvcgrid").Include(
                      "~/Content/MvcGrid/mvc-grid.css"));

            bundles.Add(new StyleBundle("~/style/MvcDatalist").Include(
                      "~/Content/MvcDatalist/mvc-datalist.css"));

            bundles.Add(new StyleBundle("~/style/Common").Include(
                      "~/Content/common.css"));

            // Scripts
            bundles.Add(new ScriptBundle("~/script/Common").Include(
                        "~/Scripts/common.js"));

            bundles.Add(new ScriptBundle("~/script/tableHeadFixer").Include(
                        "~/Scripts/tableHeadFixer.js"));

            bundles.Add(new ScriptBundle("~/script/InputMask").Include(
                        "~/plugins/input-mask/jquery.inputmask.js"));

            bundles.Add(new ScriptBundle("~/script/jQuery").Include(
                        "~/bower_components/jquery/dist/jquery.min.js"));

            bundles.Add(new ScriptBundle("~/script/bootstrap").Include(
                        "~/bower_components/bootstrap/dist/js/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/script/iCheck").Include(
                        "~/plugins/iCheck/icheck.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/unobtrusive").Include(
                        "~/Scripts/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/script/bootbox").Include(
                        "~/Scripts/bootbox.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                        "~/Scripts/datatables.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/mvcgrid").Include(
                        "~/Scripts/MvcGrid/mvc-grid.js"));

            bundles.Add(new ScriptBundle("~/script/SlimScroll").Include(
                        "~/bower_components/jquery-slimscroll/jquery.slimscroll.min.js"));

            bundles.Add(new ScriptBundle("~/script/FastClick").Include(
                        "~/bower_components/fastclick/lib/fastclick.js"));

            bundles.Add(new ScriptBundle("~/script/AdminLTE").Include(
                        "~/dist/js/adminlte.min.js"));

            bundles.Add(new ScriptBundle("~/script/demo").Include(
                        "~/dist/js/demo.js"));

            bundles.Add(new ScriptBundle("~/bundles/screenfull").Include(
                        "~/Scripts/screenfull.js"));

            bundles.Add(new ScriptBundle("~/script/cookie").Include(
                        "~/Scripts/cookie.min.js"));

            bundles.Add(new ScriptBundle("~/script/Select2").Include(
                        "~/bower_components/select2/dist/js/select2.full.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/MvcDatalist").Include(
                        "~/Scripts/MvcDatalist/mvc-datalist.js"));
        }
    }
}
