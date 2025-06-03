using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppGrooming.Controllers
{
    public abstract class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Verificar si el usuario está autenticado (excepto para Login)
            if (filterContext.Controller.GetType() != typeof(LoginController))
            {
                if (Session["UserId"] == null)
                {
                    filterContext.Result = new RedirectResult("~/Login");
                    return;
                }
            }

            // Pasar información del usuario a la vista
            ViewBag.UserName = Session["UserName"];
            ViewBag.UserRole = Session["UserRole"];

            base.OnActionExecuting(filterContext);
        }
    }
}