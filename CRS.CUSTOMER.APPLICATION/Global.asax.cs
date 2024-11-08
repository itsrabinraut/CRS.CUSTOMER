using CRS.CUSTOMER.BUSINESS.LogManagement.ErrorLogManagement;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CRS.CUSTOMER.APPLICATION
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Bootstrapper.Initialise();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            ErrorLogManagementBusiness buss = new ErrorLogManagementBusiness();
            Exception exception = HttpContext.Current.Error;

            var err = Server.GetLastError();
            if (err != null)
            {
                //var redirect = ConfigurationManager.AppSettings["CustomRedirect"]?.ToString()?.ToLower() ?? "n";
                var page = HttpContext.Current.Request.Url.ToString();
                var id = buss.LogError(err, page);
                //if (redirect == "y")
                //{
                //    HttpContext.Current.Response.Redirect("/Error/Index?Id=" + id);
                //    HttpContext.Current.Response.End();
                //}
                //else
                //{
                //    HttpContext.Current.Response.Redirect("/Error/Index?Id=" + id);
                //    HttpContext.Current.Response.End();
                //}
                //HttpContext.Current.Response.Redirect("/Error/Index?Id=" + id);
                //HttpContext.Current.Response.Redirect("/ErrorManagement/Index?Id=" + id);
                Session["ErrorId"] = id;
                HttpContext.Current.Response.Redirect("/ErrorManagement/Index");
                HttpContext.Current.Response.End();
            }
        }
    }
}
