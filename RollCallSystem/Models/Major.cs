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
    
     using System.Runtime.Serialization;
     using Newtonsoft.Json;
    
     [DataContract(IsReference =true)]
     [JsonObject(MemberSerialization.OptOut)]
    public partial class Major
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Major()
        {
            this.MajorSpecialties = new List<MajorSpecialty>();
            this.Students = new List<Student>();
        }
    
    	[DataMember]
        public int Id { get; set; }
    	[DataMember]
        public string Name { get; set; }
    	[DataMember]
        public int FacultyId { get; set; }
    
    	[DataMember]
        public virtual Faculty Faculty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    	[DataMember]
        public virtual List<MajorSpecialty> MajorSpecialties { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    	[DataMember]
        public virtual List<Student> Students { get; set; }
    }
}