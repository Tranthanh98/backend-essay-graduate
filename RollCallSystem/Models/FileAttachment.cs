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
    public partial class FileAttachment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FileAttachment()
        {
            this.TrainingImages = new List<TrainingImage>();
        }
    
    	[DataMember]
        public int Id { get; set; }
    	[DataMember]
        public string Name { get; set; }
    	[DataMember]
        public string Extension { get; set; }
    	[DataMember]
        public int Width { get; set; }
    	[DataMember]
        public int Height { get; set; }
    	[DataMember]
        public int Type { get; set; }
    	[DataMember]
        public System.DateTime CreateDate { get; set; }
    
    	[DataMember]
        public virtual FileData FileData { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    	[DataMember]
        public virtual List<TrainingImage> TrainingImages { get; set; }
    }
}
