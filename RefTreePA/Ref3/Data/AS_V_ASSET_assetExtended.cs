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
    
    public partial class AS_V_ASSET_assetExtended
    {
        public int AS_ASSET_ID { get; set; }
        public string AS_ASSET_CODE { get; set; }
        public Nullable<int> AS_ASSET_ID_PADRE { get; set; }
        public string AS_ASSET_DESCRIZIONE { get; set; }
        public Nullable<int> AS_ASSET_LOCATION_ID { get; set; }
        public int AS_ASSET_TIPASS_ID { get; set; }
        public int AS_ASSET_TIPSAS_ID { get; set; }
        public Nullable<int> AS_ASSET_SASSPE_ID { get; set; }
        public Nullable<System.DateTime> AS_ASSET_DATA_INIZIO { get; set; }
        public Nullable<System.DateTime> AS_ASSET_DATA_FINE { get; set; }
        public Nullable<decimal> AS_ASSET_SUPERFICIE { get; set; }
        public string AS_ASSET_CODE_OLD { get; set; }
        public Nullable<int> AS_ASSET_ID_ASSET_ORIGINE { get; set; }
        public string AS_ASSET_ADDRESS { get; set; }
        public string LOCATION_NUMBER { get; set; }
        public Nullable<int> LOCATION_STREET_ID { get; set; }
        public Nullable<decimal> LOCATION_LATITUDE { get; set; }
        public Nullable<decimal> LOCATION_LONGITUDE { get; set; }
        public string P_CODE { get; set; }
        public string AS_ASSET_XML_CAMASS_VALUES { get; set; }
        public string AS_ASSET_XML_CAMUSE_VALUES { get; set; }
        public string GROUPS_LIST { get; set; }
        public int US_AREVIS_ID { get; set; }
        public Nullable<int> MagicBOLayerID { get; set; }
        public Nullable<int> DEFAULT_GROUP_ID { get; set; }
        public Nullable<int> GROUP_TO_SEARCH { get; set; }
        public Nullable<int> AS_ASSET_EV_STAGE_ID { get; set; }
        public string AS_TIPSAS_DESCRIPTION { get; set; }
        public string DESCR_FOR_SEARCH { get; set; }
        public string Visible_group { get; set; }
        public string Main_Image_Thumbnail { get; set; }
        public int HasImage { get; set; }
        public string AS_ASSET_LONG_DESC { get; set; }
    }
}