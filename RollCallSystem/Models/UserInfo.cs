using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public partial class User
    {
        public string Name { get; set; }
        public Student Student { get; set; }
        public Teacher Teacher { get; set; }
    }
}