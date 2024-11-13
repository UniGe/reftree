using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Diagnostics;


namespace Ref3.Data
{
    public  class DatabaseEntityAutomations
    {
        //oggetti per cui usare una connessione a config db ("MagicDB")
        public static readonly HashSet<string> ConfigTables = new HashSet<string>
        {
           "core.AS_CAMACT_FIELDS_act",
           "core.AS_CAMACU_FIELDS_act_user",
           "core.AS_CAMASS_campi_asset",
           "core.AS_CAMSTR_struct_ext_fields",
           "core.DO_DOCFIE_fields_document",
           "core.EV_STACOL_stage_columns",
           "core.LE_CAMASS_TIPACT_fields",
           "core.LE_CAMASS_TIPWAR_fields",
           "core.LE_CAMREF_field_refere",
           "core.PL_CAMASS_campi_plant_asset",
           "core.TK_CAMACT_fields_activity",
           "core.AS_V_CAMASS_campi_asset",
           "core.AS_V_CAMSTR_campi_strutture",
           "core.DO_V_DOCFIE_class_doc_fields",
           "core.DO_V_DOCFIE_type_doc_fields",
           "core.PL_V_CAMASS_plant_exted_fields",
           "core.TK_V_CAMACT_fields_activity",
           "config.ViewElements",
           "core.V_US_MAGICGRIDS_griglie",
           "core.V_US_MAGICFUNCTIONS"
        };
        public static readonly Dictionary<string, string> automationMethods = new Dictionary<string, string> 
        {
           //{ "core.US_V_CROSS_AREFUN","AREFUNFillFunctionGUIDFromID" },
           //{ "core.US_V_CROSS_AREGRI","AREGRIFillMagicGridNameFromID" },
           { "core.EV_ACTENA_action_entity_name","ACTENAFillMagicGridNameFromID" },
           { "core.EV_STAGRI_stage_grid", "STAGRIFillMagicGridNameFromID"},
           { "core.WF_Activities","WF_ACTFillFunctionGUIDFromID"},
           { "core.PS_GRIMOD_model_grid" , "GRIMODFillMagicGridGUIDFromID"},
           { "core.MB_V_BUTFUN" , "BUTFUNFillFunctionGUIDFromID"},
           { "core.MB_V_BUTGRI" , "BUTGRIFillMagicGridNameFromID"},
           { "core.DO_VI_USER_L" , "EncryptPassword"},
           { "core.TKU_USER_L" , "EncryptPassword"}
        };

        //public  void AREGRIFillMagicGridNameFromID(dynamic input)
        //{
        //    int id = 0;
        //    if (input.US_AREGRI_MagicGridID != null)
        //        id = input.US_AREGRI_MagicGridID;
        //    if (id == 0)
        //    {
        //        input.MagicGridName = null;
        //        input.GridGUID = null;
        //        return;
        //    }
        //    input.MagicGridName = MagicFramework.Models.Magic_Grids.GetGridNameFromID(id);
        //    input.GridGUID = MagicFramework.Models.Magic_Grids.GetGUIDFromID(id);
        //}
        //public  void AREFUNFillFunctionGUIDFromID(dynamic input)
        //{
        //    int id = 0;
        //    if (input.US_AREFUN_FunctionID != null)
        //        id = input.US_AREFUN_FunctionID;
        //    if (id == 0)
        //    {
        //        input.FunctionGUID = null;
        //        return;
        //    }
        //    input.FunctionGUID = MagicFramework.Models.Magic_Functions.GetGUIDFromID(id);
        //}
        public void WF_ACTFillFunctionGUIDFromID(dynamic input)
        {
            int id = 0;
            if (input.ProcessFunction_ID != null)
                id = input.ProcessFunction_ID;
            if (id == 0)
            {
                input.FunctionGUID = null;
                return;
            }
            input.FunctionGUID = MagicFramework.Models.Magic_Functions.GetGUIDFromID(id);
        }
        public void BUTFUNFillFunctionGUIDFromID(dynamic input)
        {
            int id = 0;
            if (input.MB_BUTFUN_FunctionID != null)
                id = input.MB_BUTFUN_FunctionID;
            if (id == 0)
            {
                input.FunctionGUID = null;
                return;
            }
            input.FunctionGUID = MagicFramework.Models.Magic_Functions.GetGUIDFromID(id);
        }
        public  void ACTENAFillMagicGridNameFromID(dynamic input)
        {
            int id = 0;
            if (input.EV_ACTENA_mgcGridId != null)
                id = input.EV_ACTENA_mgcGridId;
            if (id == 0)
            {
                input.MagicGridName = null;
                return;
            }
            input.MagicGridName = MagicFramework.Models.Magic_Grids.GetGridNameFromID(id);
        }
        public void BUTGRIFillMagicGridNameFromID(dynamic input)
        {
            int id = 0;
            if (input.MB_BUTGRI_MagicGridID != null)
                id = input.MB_BUTGRI_MagicGridID;
            if (id == 0)
            {
                input.MagicGridName = null;
                return;
            }
            input.MagicGridName = MagicFramework.Models.Magic_Grids.GetGridNameFromID(id);
        }

        public  void STAGRIFillMagicGridNameFromID(dynamic input)
        {
            int id = 0;
            if (input.EV_STAGRI_GRID_ID != null)
                id = input.EV_STAGRI_GRID_ID;
            if (id == 0)
            {
                input.MagicGridName = null;
                return;
            }
            input.MagicGridName = MagicFramework.Models.Magic_Grids.GetGridNameFromID(id);
        }
        public void GRIMODFillMagicGridGUIDFromID(dynamic input)
        {
            int id = 0;
            if (input.PS_GRIMOD_GridId != null)
                id = input.PS_GRIMOD_GridId;
            if (id == 0)
            {
                input.PS_GRIMOD_GridGUID = null;
                return;
            }
            input.PS_GRIMOD_GridGUID = MagicFramework.Models.Magic_Grids.GetGUIDFromID(id);
        }
        private static byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }
        public void EncryptPassword(dynamic input)
        {
            if (input.Password != null)
            {
                string pwd = input.Password;
                input.Password = MagicFramework.MemberShip.EFMembershipProvider.encodeHMACSHA1Pwd(pwd);
                input.Clear_PWD = pwd;
            }
        }

    }
}