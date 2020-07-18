using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public class UserCreateModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }
        public bool IsActive { get; set; }
    }
}