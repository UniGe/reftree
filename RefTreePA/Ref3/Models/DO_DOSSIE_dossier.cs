using System;
using System.Collections.Generic;
using System.Linq;
using MagicFramework.Helpers;


namespace Ref3.Data
{
    public partial class DO_DOSSIE_dossier
    {
        public string getdescription(int DO_DOSSIE_ID)
        {
            Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
            var r = (from e in _context.DO_DOSSIE_dossier
                     where e.DO_DOSSIE_ID == DO_DOSSIE_ID
                     select e).FirstOrDefault();

            return r != null ? r.DO_DOSSIE_CODE : "EmptyDescriptionForDossier";
        }

    }
}