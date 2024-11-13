using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ref3.Models
{
    public class AS_V_ASSET_assetgroups
    {
        public int  ID {get;set;}
        public bool Checked {get; set;}
        public int AS_ASSET_ID { get; set; }
        public int US_GROUPS_ID { get; set; }
        public int US_GROUPS_US_TIPGRU_ID { get; set; }
        public string US_GROUPS_DESCRIPTION { get; set; }
        public string US_TIPGRU_DESCRIPTION { get; set; }
        public int US_AREVIS_ID { get; set; }
        public int AS_ASSGRO_ID {get;set;}
    }
}

