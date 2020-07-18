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
        TEACHER = 1,
        STUDENT = 2
    }
}