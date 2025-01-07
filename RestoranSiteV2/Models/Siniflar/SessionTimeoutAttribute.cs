using System;
using System.Web;
using System.Web.Mvc;

public class SessionTimeoutAttribute : ActionFilterAttribute
{
    private readonly int _timeoutInMinutes = 60; // 1 saat

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if (HttpContext.Current.Session["UserLoggedIn"] != null)
        {
            var lastLoginTime = (DateTime)HttpContext.Current.Session["LastLoginTime"];
            if (DateTime.Now - lastLoginTime > TimeSpan.FromMinutes(_timeoutInMinutes))
            {
                // Eğer kullanıcı 1 saatten fazla süredir aktif değilse, logout işlemi yapılır
                HttpContext.Current.Session.Clear();
                filterContext.Result = new RedirectResult("/Account/Login");
            }
        }
        else
        {
            // Kullanıcı giriş yapmamışsa, login sayfasına yönlendirilir
            filterContext.Result = new RedirectResult("/Account/Login");
        }

        base.OnActionExecuting(filterContext);
    }
}
