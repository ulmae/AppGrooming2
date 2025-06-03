using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppGrooming.Models
{
    public class WorkOrderViewModel
    {
        public string Id { get; set; }
        public string PetName { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public string Services { get; set; }
        public DateTime? EstimatedReady { get; set; }
        public DateTime? ReadyAt { get; set; }

        public string StatusDisplay
        {
            get
            {
                switch (Status?.ToLower())
                {
                    case "pending": return "Pendiente";
                    case "in_progress": return "En Proceso";
                    case "completed": return "Finalizado";
                    case "cancelled": return "Cancelado";
                    default: return Status;
                }
            }
        }

        public string StatusClass
        {
            get
            {
                switch (Status?.ToLower())
                {
                    case "pending": return "status-pending";
                    case "in_progress": return "status-progress";
                    case "completed": return "status-completed";
                    case "cancelled": return "status-cancelled";
                    default: return "";
                }
            }
        }
    }
}