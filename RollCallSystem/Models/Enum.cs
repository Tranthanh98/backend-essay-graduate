using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public enum EFileType : int
    {
        [Display(Description = "jpg")]
        ImageJPG = 1,
    }
    public enum EUserRole : int
    {
        Teacher = 1,
        Student = 2
    }
    public enum EClassStatus : int
    {
        Schedule = 1,
        Opening = 2,
        Closed=3,
    }
    public enum ERollCallType : int
    {
        Auto = 1,
        Manually = 2,
    }
}