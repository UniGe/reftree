using System;
using System.Collections.Generic;
using System.Linq;
using MagicFramework.Helpers;


namespace Ref3.Data
{
    public partial class DO_DOCVER_versions
    {
        public int getCounter(int? DO_DOCFIL_ID)
        {
            Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
            var r = (from e in _context.DO_DOCVER_versions
                     where e.DO_DOCVER_DO_DOCFIL_ID == DO_DOCFIL_ID
                     select e).FirstOrDefault();

            return r == null ? 1 : r.DO_DOCVER_COUNTER + 1;
        }

        public bool filldata(int? DO_DOCFIL_ID, string DO_DOCVER_LINK_FILE, string DO_DOCVER_NOTE, int DO_DOCVER_COUNTER,string DO_DOCVER_THUMBNAIL_LINK,string multifilename = "")
        {
            try
            {
                //this.DO_DOCVER_COUNTER = this.getCounter(DO_DOCFIL_ID);
                this.DO_DOCVER_COUNTER = DO_DOCVER_COUNTER + 1;
                this.DO_DOCVER_DATE = DateTime.Now;
                this.DO_DOCVER_LINK_FILE = Utils.GetNullForEmptyString(DO_DOCVER_LINK_FILE);
                this.DO_DOCVER_NOTE = Utils.GetNullForEmptyString(DO_DOCVER_NOTE) + multifilename;
                this.DO_DOCVER_USERID = MagicFramework.Helpers.SessionHandler.IdUser;
                this.DO_DOCVER_THUMBNAIL_LINK = Utils.GetNullForEmptyString(DO_DOCVER_THUMBNAIL_LINK);
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


