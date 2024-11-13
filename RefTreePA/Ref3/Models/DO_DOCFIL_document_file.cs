using System;
using System.Collections.Generic;
using System.Linq;
using MagicFramework.Helpers;

namespace Ref3.Data
{
    public partial class DO_DOCFIL_document_file
    {
        public bool filldata(Data.DO_V_DOCFIL inputdata)
        {
            try
            {
                this.DO_DOCFIL_DATE = DateTime.Now;
                this.DO_DOCFIL_USERID = MagicFramework.Helpers.SessionHandler.IdUser;
                this.DO_DOCFIL_DO_DOCUME_ID = inputdata.DO_DOCFIL_DO_DOCUME_ID;                
                this.DO_DOCFIL_NOTE = Utils.GetNullForEmptyString(inputdata.DO_DOCFIL_NOTE);
                this.DO_DOCFIL_FLAG_MAIN_ASSET = inputdata.DO_DOCFIL_FLAG_MAIN_ASSET;
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }
        }


        public bool filldata(Data.DO_DOCUME_documents inputdata)
        {
            try
            {
                this.DO_DOCFIL_DATE = DateTime.Now;
                this.DO_DOCFIL_USERID = MagicFramework.Helpers.SessionHandler.IdUser;
                this.DO_DOCFIL_DO_DOCUME_ID = inputdata.DO_DOCUME_ID;
                this.DO_DOCFIL_NOTE = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_DESCRIPTION);
                this.DO_DOCFIL_FLAG_MAIN_ASSET = false;
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }
        }
    }
}






