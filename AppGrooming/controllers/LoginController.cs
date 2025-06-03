using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration;
using AppGrooming.Models;

namespace AppGrooming.Controllers
{
    public class LoginController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = ValidateUser(model.Username, model.Password);
                if (user != null)
                {
                    // Crear cookie de autenticación
                    FormsAuthentication.SetAuthCookie(user.Email, false);

                    // Guardar información del usuario en sesión
                    Session["UserId"] = user.Id;
                    Session["UserName"] = user.FullName;
                    Session["UserRole"] = user.Role;

                    // Redirigir según el rol
                    switch (user.Role.ToLower())
                    {
                        case "receptionist":
                            return RedirectToAction("Index", "Receptionist");
                        case "groomer":
                            return RedirectToAction("Index", "Groomer");
                        case "admin":
                            return RedirectToAction("Index", "Admin");
                        default:
                            return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                }
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index");
        }

        private UserModel ValidateUser(string email, string password)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"SELECT id, full_name, role, email, password_hash 
                             FROM users 
                             WHERE email = @email AND active = 1";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var storedHash = reader["password_hash"].ToString();
                            // Aquí deberías implementar la verificación real del hash
                            // Por simplicidad, comparamos directamente
                            if (storedHash == password || storedHash == "hash_" + password)
                            {
                                return new UserModel
                                {
                                    Id = reader["id"].ToString(),
                                    FullName = reader["full_name"].ToString(),
                                    Role = reader["role"].ToString(),
                                    Email = reader["email"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}