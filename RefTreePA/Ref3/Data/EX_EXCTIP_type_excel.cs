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
    
    public partial class EX_EXCTIP_type_excel
    {
        public EX_EXCTIP_type_excel()
        {
            this.FL_FILINP_input_file = new HashSet<FL_FILINP_input_file>();
            this.FL_RECFIL_fields = new HashSet<FL_RECFIL_fields>();
        }
    
        public int EX_EXCTIP_ID { get; set; }
        public string EX_EXCTIP_CODE { get; set; }
        public string EX_EXCTIP_DESCRIPTION { get; set; }
    
        public virtual ICollection<FL_FILINP_input_file> FL_FILINP_input_file { get; set; }
        public virtual ICollection<FL_RECFIL_fields> FL_RECFIL_fields { get; set; }
    }
}
