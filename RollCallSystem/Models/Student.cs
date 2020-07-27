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
    
    public partial class Student
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Student()
        {
            this.RollCalls = new HashSet<RollCall>();
            this.Studyings = new HashSet<Studying>();
            this.TrainingImages = new HashSet<TrainingImage>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public System.DateTime Birthday { get; set; }
        public string Hometown { get; set; }
        public int CourseId { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> MajorSpecialtyId { get; set; }
        public int MajorId { get; set; }
        public int Gender { get; set; }
    
        public virtual Course Course { get; set; }
        public virtual Major Major { get; set; }
        public virtual MajorSpecialty MajorSpecialty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RollCall> RollCalls { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Studying> Studyings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TrainingImage> TrainingImages { get; set; }
    }
}
