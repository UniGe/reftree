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
    
    public partial class DO_CLAUTE_class_document_user
    {
        public int DO_CLAUTE_ID { get; set; }
        public int DO_CLAUTE_CLADOC_ID { get; set; }
        public int DO_CLAUTE_US_GROUPS_ID { get; set; }
        public Nullable<int> DO_CLAUTE_US_DEPROL_ID { get; set; }
        public Nullable<bool> DO_CLAUTE_FLAG_WRITE { get; set; }
        public Nullable<bool> DO_CLAUTE_FLAG_MONITOR { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<int> cUserId { get; set; }
        public Nullable<bool> DO_CLAUTE_FLAG_ALERT_MAIL { get; set; }
    
        public virtual US_GROUPS_groups US_GROUPS_groups { get; set; }
        public virtual DO_CLADOC_document_class DO_CLADOC_document_class { get; set; }
    }
}
