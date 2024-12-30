using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RestoranSiteV2
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // Her gelen istek için çağrılır
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var httpContext = HttpContext.Current;

            // HttpContext ve Session'ın null olup olmadığını kontrol et
            if (httpContext != null && httpContext.Session != null && httpContext.Session["User"] == null
                && !httpContext.Request.Url.AbsolutePath.Contains("/Account/Login"))
            {
                httpContext.Response.Redirect("/Account/Login");
            }
        }


    }
}
