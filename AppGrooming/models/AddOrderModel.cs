using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace AppGrooming.Models
{
    public class AddOrderModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public string CustomerId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una mascota")]
        public string PetId { get; set; }

        [Required(ErrorMessage = "Debe asignar la orden a un groomer")]
        public string AssignedToId { get; set; }

        [Required(ErrorMessage = "Debe especificar la hora estimada de finalización")]
        public DateTime EstimatedReady { get; set; }

        [Required(ErrorMessage = "Debe seleccionar al menos un servicio")]
        public List<int> ServiceIds { get; set; }

        public string Comments { get; set; }
    }
}