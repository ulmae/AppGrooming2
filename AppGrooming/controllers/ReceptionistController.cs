using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Configuration;
using AppGrooming.Models;

namespace AppGrooming.Controllers
{
    public class ReceptionistController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ActionResult Index()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "receptionist")
            {
                return RedirectToAction("Index", "Login");
            }

            var model = new ReceptionistDashboardModel
            {
                PendingOrders = GetPendingOrdersCount(),
                InProgressOrders = GetInProgressOrdersCount(),
                ReadyOrders = GetReadyOrdersCount(),
                WorkOrders = GetWorkOrders(),
                Customers = GetCustomers(),
                Groomers = GetGroomers(),
                Services = GetServices()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddOrder(AddOrderModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var orderId = CreateWorkOrder(model);
                    TempData["SuccessMessage"] = "Orden creada exitosamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error al crear la orden: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Por favor complete todos los campos requeridos.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult CancelOrder(string orderId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "UPDATE work_orders SET status = 'cancelled' WHERE id = @orderId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@orderId", orderId);
                        connection.Open();
                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Orden cancelada exitosamente." });
                        }
                        else
                        {
                            return Json(new { success = false, message = "No se encontró la orden especificada." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cancelar la orden: " + ex.Message });
            }
        }

        public JsonResult GetPetsByCustomer(string customerId)
        {
            var pets = new List<object>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "SELECT id, name, species, breed FROM pets WHERE customer_id = @customerId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@customerId", customerId);
                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pets.Add(new
                                {
                                    Id = reader["id"].ToString(),
                                    Name = reader["name"].ToString(),
                                    Species = reader["species"].ToString(),
                                    Breed = reader["breed"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }

            return Json(pets, JsonRequestBehavior.AllowGet);
        }

        private string CreateWorkOrder(AddOrderModel model)
        {
            var orderId = Guid.NewGuid().ToString();
            var currentUserId = Session["UserId"].ToString();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Crear la orden de trabajo
                        var insertOrderQuery = @"
                            INSERT INTO work_orders (id, pet_id, created_by_id, assigned_to_id, status, estimated_ready, comments, created_at)
                            VALUES (@id, @petId, @createdById, @assignedToId, 'pending', @estimatedReady, @comments, GETDATE())";

                        using (var command = new SqlCommand(insertOrderQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", orderId);
                            command.Parameters.AddWithValue("@petId", model.PetId);
                            command.Parameters.AddWithValue("@createdById", currentUserId);
                            command.Parameters.AddWithValue("@assignedToId", model.AssignedToId);
                            command.Parameters.AddWithValue("@estimatedReady", model.EstimatedReady);
                            command.Parameters.AddWithValue("@comments", model.Comments ?? "");
                            command.ExecuteNonQuery();
                        }

                        // Agregar servicios a la orden
                        for (int i = 0; i < model.ServiceIds.Count; i++)
                        {
                            var insertServiceQuery = @"
                                INSERT INTO work_order_services (work_order_id, service_id, order_index)
                                VALUES (@workOrderId, @serviceId, @orderIndex)";

                            using (var command = new SqlCommand(insertServiceQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@workOrderId", orderId);
                                command.Parameters.AddWithValue("@serviceId", model.ServiceIds[i]);
                                command.Parameters.AddWithValue("@orderIndex", i + 1);
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return orderId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private List<CustomerModel> GetCustomers()
        {
            var customers = new List<CustomerModel>();

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT id, full_name, phone_number, email FROM customers ORDER BY full_name";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new CustomerModel
                            {
                                Id = reader["id"].ToString(),
                                FullName = reader["full_name"].ToString(),
                                PhoneNumber = reader["phone_number"].ToString(),
                                Email = reader["email"]?.ToString()
                            });
                        }
                    }
                }
            }

            return customers;
        }

        private List<UserModel> GetGroomers()
        {
            var groomers = new List<UserModel>();

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT id, full_name FROM users WHERE role = 'groomer' AND active = 1 ORDER BY full_name";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            groomers.Add(new UserModel
                            {
                                Id = reader["id"].ToString(),
                                FullName = reader["full_name"].ToString()
                            });
                        }
                    }
                }
            }

            return groomers;
        }

        private List<ServiceModel> GetServices()
        {
            var services = new List<ServiceModel>();

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT id, name, description, duration_min FROM grooming_services ORDER BY name";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            services.Add(new ServiceModel
                            {
                                Id = (int)reader["id"],
                                Name = reader["name"].ToString(),
                                Description = reader["description"]?.ToString(),
                                DurationMin = reader["duration_min"] as int?
                            });
                        }
                    }
                }
            }

            return services;
        }

        private int GetPendingOrdersCount()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"SELECT COUNT(DISTINCT wo.id) 
                             FROM work_orders wo
                             INNER JOIN work_order_services wos ON wo.id = wos.work_order_id
                             INNER JOIN grooming_services gs ON wos.service_id = gs.id
                             WHERE wo.status = 'pending' 
                             AND (gs.name LIKE '%Baño%' OR gs.name LIKE '%Paquete%')";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
        }

        private int GetInProgressOrdersCount()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"SELECT COUNT(DISTINCT wo.id) 
                             FROM work_orders wo
                             INNER JOIN work_order_services wos ON wo.id = wos.work_order_id
                             INNER JOIN grooming_services gs ON wos.service_id = gs.id
                             WHERE wo.status = 'in_progress' 
                             AND (gs.name LIKE '%Corte%' OR gs.name LIKE '%Paquete%')";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
        }

        private int GetReadyOrdersCount()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"SELECT COUNT(DISTINCT wo.id) 
                             FROM work_orders wo
                             INNER JOIN work_order_services wos ON wo.id = wos.work_order_id
                             INNER JOIN grooming_services gs ON wos.service_id = gs.id
                             WHERE wo.status = 'completed' 
                             AND gs.name LIKE '%Paquete%'";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
        }

        private List<WorkOrderViewModel> GetWorkOrders()
        {
            var orders = new List<WorkOrderViewModel>();

            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    SELECT wo.id, p.name as pet_name, wo.status, wo.estimated_ready, wo.ready_at,
                           c.full_name as customer_name, c.phone_number, wo.created_at
                    FROM work_orders wo
                    INNER JOIN pets p ON wo.pet_id = p.id
                    INNER JOIN customers c ON p.customer_id = c.id
                    WHERE wo.status IN ('pending', 'in_progress', 'completed', 'cancelled')
                    ORDER BY 
                        CASE wo.status 
                            WHEN 'pending' THEN 1 
                            WHEN 'in_progress' THEN 2 
                            WHEN 'completed' THEN 3 
                            WHEN 'cancelled' THEN 4
                        END,
                        wo.created_at";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var workOrderId = reader["id"].ToString();
                            orders.Add(new WorkOrderViewModel
                            {
                                Id = workOrderId,
                                PetName = reader["pet_name"].ToString(),
                                CustomerName = reader["customer_name"].ToString(),
                                PhoneNumber = reader["phone_number"].ToString(),
                                Status = reader["status"].ToString(),
                                Services = GetServicesForOrder(workOrderId),
                                EstimatedReady = reader["estimated_ready"] as DateTime?,
                                ReadyAt = reader["ready_at"] as DateTime?
                            });
                        }
                    }
                }
            }

            return orders;
        }

        private string GetServicesForOrder(string workOrderId)
        {
            var services = new List<string>();

            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    SELECT gs.name
                    FROM work_order_services wos
                    INNER JOIN grooming_services gs ON wos.service_id = gs.id
                    WHERE wos.work_order_id = @workOrderId
                    ORDER BY wos.order_index";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@workOrderId", workOrderId);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            services.Add(reader["name"].ToString());
                        }
                    }
                }
            }

            return string.Join(", ", services);
        }
    }
}