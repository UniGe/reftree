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
    
    public partial class DO_DOSUTE_dossier_user
    {
        public int DO_DOSUTE_ID { get; set; }
        public int DO_DOSUTE_DO_DOSSIE_ID { get; set; }
        public int DO_DOSUTE_US_GROUPS_ID { get; set; }
        public int DO_DOSUTE_US_DEPROL_ID { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<int> cUserId { get; set; }
    
        public virtual DO_DOSSIE_dossier DO_DOSSIE_dossier { get; set; }
        public virtual US_GROUPS_groups US_GROUPS_groups { get; set; }
    }
}