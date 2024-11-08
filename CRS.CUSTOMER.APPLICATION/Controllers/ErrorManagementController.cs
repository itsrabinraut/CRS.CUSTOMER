using System.Web.Mvc;

namespace CRS.CUSTOMER.APPLICATION.Controllers
{
    [OverrideActionFilters]
    public class ErrorManagementController : CustomController
    {
        public ActionResult Index()
        {
            var id = Session["ErrorId"]?.ToString();
            if (string.IsNullOrEmpty(id)) return RedirectToAction("LogOff", "Home");
            ViewBag.ErrorId = id;
            return View();
        }
        public ActionResult Error_403()
        {
            return View();
        }
        public ActionResult Error_404()
        {
            return View();
        }
    }
}