using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Carwash.Controllers
{
    public class NotificationController : Controller
    {
        [HttpGet]
        public ActionResult Notification()
        {
            return View();
        }
    }
}