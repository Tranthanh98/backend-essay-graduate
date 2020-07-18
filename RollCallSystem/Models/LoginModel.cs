using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public class LoginModel
    {
        public LoginModel()
        {
        }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsRemember { get; set; }
    }
}