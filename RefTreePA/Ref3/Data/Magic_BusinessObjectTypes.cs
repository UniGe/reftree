//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ref3.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Magic_BusinessObjectTypes
    {
        public Magic_BusinessObjectTypes()
        {
            this.DO_DOSSIE_dossier = new HashSet<DO_DOSSIE_dossier>();
            this.DO_OBTIDO_type_document = new HashSet<DO_OBTIDO_type_document>();
        }
    
        public int ID { get; set; }
        public string BusinessObjectType { get; set; }
        public string Description { get; set; }
        public int MagicGrid_Id { get; set; }
        public bool Active { get; set; }
        public string BODescriptionQuery { get; set; }
        public string TAGQuery { get; set; }
        public string EntityName { get; set; }
        public string MagicGridName { get; set; }
    
        public virtual ICollection<DO_DOSSIE_dossier> DO_DOSSIE_dossier { get; set; }
        public virtual ICollection<DO_OBTIDO_type_document> DO_OBTIDO_type_document { get; set; }
    }
}