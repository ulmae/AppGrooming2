using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppGrooming.Controllers
{
    public class GroomerController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "groomer")
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.UserName = Session["UserName"];
            ViewBag.Role = "Encargado de Grooming";
            return View();
        }
    }
}