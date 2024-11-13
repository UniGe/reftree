using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ref3.Models
{
    public class US_V_AREGRI_CROSS
    {
        public bool Checked {get; set;}
        public int US_AREGRI_ID { get; set; }
        public int US_AREGRI_MagicGridID { get; set; }
        public int US_AREGRI_US_PROARE_ID { get; set; }
        public int US_PROARE_US_ARETYP_ID { get; set; }
        public string US_PROARE_CODE { get; set; }
        public string US_PROARE_DESCRIPTION { get; set; }
        public string MagicGridName { get; set; }
    }
}