using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;

namespace AppGrooming.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "admin")
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.UserName = Session["UserName"];
            ViewBag.Role = "Administrador";
            return View();
        }
    }
}