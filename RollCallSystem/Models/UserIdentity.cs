using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace RollCallSystem.Models
{
    public class UserIdentity : IIdentity
    {
        public UserIdentity()
        {
        }
        public string Name { get; set; }
        public string AuthenticationType { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}