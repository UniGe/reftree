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
    
    public partial class DO_DOCREL_document_relation
    {
        public int DO_DOCREL_ID { get; set; }
        public Nullable<int> DO_DOCREL_DO_DOCUME_ID { get; set; }
        public string DO_DOCREL_TABLE_NAME { get; set; }
        public Nullable<int> DO_DOCREL_ID_RECORD { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<int> cUserId { get; set; }
    
        public virtual DO_DOCUME_documents DO_DOCUME_documents { get; set; }
    }
}
