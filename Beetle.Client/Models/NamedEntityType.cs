//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Beetle.Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    [MetadataType(typeof(NamedEntityTypeMetadata))]
    public partial class NamedEntityType
    {
        public NamedEntityType()
        {
            this.NamedEntities = new HashSet<NamedEntity>();
        }
    
        public System.Guid Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<NamedEntity> NamedEntities { get; set; }
    }
    
    public partial class NamedEntityTypeMetadata { }
}