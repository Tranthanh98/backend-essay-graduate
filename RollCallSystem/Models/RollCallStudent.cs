//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RollCallSystem.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RollCallStudent
    {
        public int id { get; set; }
        public Nullable<int> mssv { get; set; }
        public int lich_giang_id { get; set; }
    
        public virtual ScheduleTeach ScheduleTeach { get; set; }
        public virtual StudentInformation StudentInformation { get; set; }
    }
}
