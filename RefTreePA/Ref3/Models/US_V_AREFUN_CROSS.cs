using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ref3.Models
{
    public class US_V_AREFUN_CROSS
    {
        public bool Checked {get; set;}
        public int US_AREFUN_ID { get; set; }
        public int US_AREFUN_FunctionID { get; set; }
        public int US_AREFUN_US_PROARE_ID { get; set; }
        public int US_PROARE_US_ARETYP_ID { get; set; }
        public string US_PROARE_CODE { get; set; }
        public string US_PROARE_DESCRIPTION { get; set; }
        public Guid? FunctionGUID { get; set; }
    }
}

