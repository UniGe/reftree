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
    
    public partial class DO_V_DOCFIL
    {
        public int DO_DOCFIL_ID { get; set; }
        public int DO_DOCFIL_DO_DOCUME_ID { get; set; }
        public string DO_DOCFIL_NOTE { get; set; }
        public System.DateTime DO_DOCFIL_DATE { get; set; }
        public int DO_DOCFIL_USERID { get; set; }
        public int DO_DOCVER_COUNTER { get; set; }
        public string DO_DOCVER_NOTE { get; set; }
        public string DO_DOCVER_LINK_FILE { get; set; }
        public string Username { get; set; }
        public int DO_TIPDOC_ID { get; set; }
        public string DO_TIPDOC_CODE { get; set; }
        public string DO_TIPDOC_DESCRIPTION { get; set; }
        public string DO_TIPDOC_PATH { get; set; }
        public int DO_CLADOC_ID { get; set; }
        public string DO_CLADOC_CODE { get; set; }
        public string DO_CLADOC_DESCRIPTION { get; set; }
        public string DO_CLADOC_PATH { get; set; }
        public string DO_DOCUME_DESCRIPTION { get; set; }
        public Nullable<System.DateTime> DO_DOCUME_ISSUE_DATE { get; set; }
        public Nullable<System.DateTime> DO_DOCUME_EXPIRY_DATE { get; set; }
        public int DO_DOCUME_DO_STADOC_ID { get; set; }
        public bool DO_DOCUME_FLAG_NO_ACTIVE { get; set; }
        public string DO_DOCUME_XML_CAMASS_VALUES { get; set; }
        public string DO_DOCUME_XML_CAMUTE_VALUES { get; set; }
        public Nullable<System.DateTime> DO_DOCUME_DATA_PROT { get; set; }
        public string DO_DOCUME_NUM_PROT { get; set; }
        public string DO_DOCUME_NOTE { get; set; }
        public string DO_DOCUME_NUM { get; set; }
        public string DO_DOCUME_CODE { get; set; }
        public Nullable<int> DO_DOCUME_ID_PADRE { get; set; }
        public Nullable<bool> DO_DOCFIL_FLAG_MAIN_ASSET { get; set; }
        public string DO_TIPDOC_EXTENSIONS { get; set; }
        public Nullable<bool> DO_CLADOC_FLAG_PHOTO { get; set; }
        public string DO_DOCVER_THUMBNAIL { get; set; }
        public int DO_DOCVER_ID { get; set; }
    }
}