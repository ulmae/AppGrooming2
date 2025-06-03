using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppGrooming.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
    }
}