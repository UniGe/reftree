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
    
    public partial class DO_DOCFIE_fields_document
    {
        public int DO_DOCFIE_ID { get; set; }
        public Nullable<int> DO_DOCFIE_DO_CLADOC_ID { get; set; }
        public Nullable<int> DO_DOCFIE_DO_TIPDOC_ID { get; set; }
        public string DO_DOCFIE_FIELD_DB { get; set; }
        public Nullable<int> DO_DOCFIE_CF_FIELDS_DATA_TYPE_ID { get; set; }
        public Nullable<int> DO_DOCFIE_MagicColumnID { get; set; }
    
        public virtual CF_FIELDS_DATA_TYPE CF_FIELDS_DATA_TYPE { get; set; }
        public virtual DO_CLADOC_document_class DO_CLADOC_document_class { get; set; }
        public virtual DO_TIPDOC_document_type DO_TIPDOC_document_type { get; set; }
    }
}
