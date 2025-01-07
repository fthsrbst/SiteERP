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

            // Logout isteğini bypass et
            if (httpContext.Request.Url.AbsolutePath.Contains("/Account/Logout"))
            {
                System.Diagnostics.Debug.WriteLine("Logout isteği algılandı, Application_BeginRequest atlanıyor.");
                return;
            }

            // Şifre sıfırlama yollarını bypass et
            if (httpContext.Request.Url.AbsolutePath.Contains("/Account/ForgotPassword") ||
                httpContext.Request.Url.AbsolutePath.Contains("/Account/ResetPassword"))
            {
                System.Diagnostics.Debug.WriteLine("Şifre sıfırlama isteği algılandı, Application_BeginRequest atlanıyor.");
                return;
            }

            // Cookie kontrolü
            var authCookie = httpContext.Request.Cookies["AuthCookie"];
            if (authCookie != null)
            {
                var cookieValue = authCookie.Value.Split('|');
                var lastLoginTime = DateTime.Parse(cookieValue[1]);

                System.Diagnostics.Debug.WriteLine("AuthCookie bulundu. Son giriş zamanı: " + lastLoginTime);

                if (DateTime.Now - lastLoginTime > TimeSpan.FromMinutes(60))
                {
                    // Cookie süresi dolmuşsa
                    httpContext.Response.Cookies["AuthCookie"].Expires = DateTime.Now.AddMinutes(-1);
                    httpContext.Response.Cookies["AuthCookie"].Value = null;

                    System.Diagnostics.Debug.WriteLine("Cookie süresi dolmuş ve silindi.");
                    httpContext.Response.Redirect("/Account/Login", false);
                    httpContext.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    // Kullanıcı geçerli oturumla giriş yapmışsa
                    if (!httpContext.Request.Url.AbsolutePath.Contains("/Dashboard/Index"))
                    {
                        System.Diagnostics.Debug.WriteLine("Geçerli oturum ile Dashboard'a yönlendirme yapılıyor.");
                        httpContext.Response.Redirect("/Dashboard/Index", false);
                        httpContext.ApplicationInstance.CompleteRequest();
                    }
                }
            }
            else
            {
                // Cookie yoksa login sayfasına yönlendir
                System.Diagnostics.Debug.WriteLine("AuthCookie bulunamadı, login sayfasına yönlendiriliyor.");
                if (!httpContext.Request.Url.AbsolutePath.Contains("/Account/Login"))
                {
                    httpContext.Response.Redirect("/Account/Login", false);
                    httpContext.ApplicationInstance.CompleteRequest();
                }
            }
        }









    }
}
