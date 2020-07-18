using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public class StudentInfo
    {
        public StudentInfo()
        {
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Birthday { get; set; }
        public string PhoneNumber { get; set; }
        public string Hometown { get; set; }
        public string Address { get; set; }
        public Faculty Faculty { get; set; }
        public Course Course { get; set; }
        public User User { get; set; }
    }
}