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
    
    public partial class FL_CLAFIL_class_file
    {
        public FL_CLAFIL_class_file()
        {
            this.FL_TIPFIL_type_file = new HashSet<FL_TIPFIL_type_file>();
        }
    
        public int FL_CLAFIL_ID { get; set; }
        public string FL_CLAFIL_CODE { get; set; }
        public string FL_CLAFIL_DESCRIPTION { get; set; }
        public string FL_CLAFIL_COLOR { get; set; }
    
        public virtual ICollection<FL_TIPFIL_type_file> FL_TIPFIL_type_file { get; set; }
    }
}
