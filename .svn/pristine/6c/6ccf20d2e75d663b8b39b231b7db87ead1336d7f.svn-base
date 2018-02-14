
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyAuth.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [CustomAuthorize(Roles = "Sales, Admin")]
        //[Authorize(Roles = "Sales, Admin")]
        public ActionResult RestrictedIndex()
        {
            return View();
        }
    }
}