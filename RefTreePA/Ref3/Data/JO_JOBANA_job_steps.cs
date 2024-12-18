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
    
    public partial class JO_JOBANA_job_steps
    {
        public JO_JOBANA_job_steps()
        {
            this.JO_JOBINP_job_input = new HashSet<JO_JOBINP_job_input>();
            this.JO_JOBNOT_notification = new HashSet<JO_JOBNOT_notification>();
            this.JO_JOBREC_job_record = new HashSet<JO_JOBREC_job_record>();
            this.JO_ANAPRO_jobana_proare = new HashSet<JO_ANAPRO_jobana_proare>();
        }
    
        public int JO_JOBANA_ID { get; set; }
        public int JO_JOBANA_JO_JOBEVE_ID { get; set; }
        public int JO_JOBANA_JO_EXEORD_ID { get; set; }
        public Nullable<System.DateTime> JO_JOBANA_START_DATE { get; set; }
        public Nullable<System.DateTime> JO_JOBANA_END_DATE { get; set; }
        public string JO_JOBANA_ERROR { get; set; }
        public Nullable<System.DateTime> JO_JOBANA_CANCELLATION_DATE { get; set; }
        public string JO_JOBANA_XMLPARAMS { get; set; }
        public Nullable<int> JO_JOBANA_USER_ID { get; set; }
        public Nullable<short> JO_JOBANA_STATUS { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<int> cUserId { get; set; }
    
        public virtual JO_JOBEVE_jobs JO_JOBEVE_jobs { get; set; }
        public virtual JO_EXEORD_execution_order JO_EXEORD_execution_order { get; set; }
        public virtual ICollection<JO_JOBINP_job_input> JO_JOBINP_job_input { get; set; }
        public virtual ICollection<JO_JOBNOT_notification> JO_JOBNOT_notification { get; set; }
        public virtual ICollection<JO_JOBREC_job_record> JO_JOBREC_job_record { get; set; }
        public virtual ICollection<JO_ANAPRO_jobana_proare> JO_ANAPRO_jobana_proare { get; set; }
    }
}
