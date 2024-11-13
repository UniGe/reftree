using System;
using System.Collections.Generic;
using System.Linq;
using MagicFramework.Helpers;
using System.IO;


namespace Ref3.Data
{
    public partial class DO_DOCUME_documents
    {
        //public bool filldata(Data.DO_V_DOCUME inputdata, ref Data.DO_DOCUME_documents entityupdate)
        public bool filldata(Data.DO_V_DOCUME inputdata)
        {
            try
            {
                this.DO_DOCUME_DESCRIPTION = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_DESCRIPTION);
                this.DO_DOCUME_DO_STADOC_ID = inputdata.DO_DOCUME_DO_STADOC_ID;
                this.DO_DOCUME_DO_TIPDOC_ID = inputdata.DO_DOCUME_DO_TIPDOC_ID;
                this.DO_DOCUME_EXPIRY_DATE = inputdata.DO_DOCUME_EXPIRY_DATE;
                this.DO_DOCUME_FLAG_NO_ACTIVE = inputdata.DO_DOCUME_FLAG_NO_ACTIVE;
                this.DO_DOCUME_ISSUE_DATE = inputdata.DO_DOCUME_ISSUE_DATE;                
                this.DO_DOCUME_XML_CAMASS_VALUES = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_XML_CAMASS_VALUES);
                this.DO_DOCUME_XML_CAMUTE_VALUES = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_XML_CAMUTE_VALUES);
                this.DO_DOCUME_DATA_PROT = inputdata.DO_DOCUME_DATA_PROT;
                this.DO_DOCUME_NUM_PROT = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_NUM_PROT);
                this.DO_DOCUME_NOTE = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_NOTE);
                //Feature #261 D.T
                this.DO_DOCUME_NUM = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_NUM);
                this.DO_DOCUME_CODE = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_CODE);
                int DO_DOCUME_ID_PADRE = inputdata.DO_DOCUME_ID_PADRE ?? 0;
                this.DO_DOCUME_ID_PADRE = DO_DOCUME_ID_PADRE == 0 ? null : inputdata.DO_DOCUME_ID_PADRE;
                this.DO_DOCUME_FL_PRIVAT = inputdata.DO_DOCUME_FL_PRIVAT;
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }

        }

        //public bool filldata(Data.DO_V_DOCUME inputdata, ref Data.DO_DOCUME_documents entityupdate)
        public bool filldata(Data.DO_V_DOCUME_ENTITY inputdata)
        {
            try
            {
                this.DO_DOCUME_DESCRIPTION = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_DESCRIPTION);
                this.DO_DOCUME_DO_STADOC_ID = inputdata.DO_DOCUME_DO_STADOC_ID;
                this.DO_DOCUME_DO_TIPDOC_ID = inputdata.DO_DOCUME_DO_TIPDOC_ID;
                this.DO_DOCUME_EXPIRY_DATE = inputdata.DO_DOCUME_EXPIRY_DATE;
                this.DO_DOCUME_FLAG_NO_ACTIVE = (bool)inputdata.DO_DOCUME_FLAG_NO_ACTIVE;
                this.DO_DOCUME_ISSUE_DATE = inputdata.DO_DOCUME_ISSUE_DATE;
                this.DO_DOCUME_XML_CAMASS_VALUES = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_XML_CAMASS_VALUES);
                this.DO_DOCUME_XML_CAMUTE_VALUES = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_XML_CAMUTE_VALUES);
                this.DO_DOCUME_DATA_PROT = inputdata.DO_DOCUME_DATA_PROT;
                this.DO_DOCUME_NUM_PROT = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_NUM_PROT);
                this.DO_DOCUME_NOTE = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_NOTE);
                //Feature #261 D.T
                this.DO_DOCUME_NUM = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_NUM);
                this.DO_DOCUME_CODE = Utils.GetNullForEmptyString(inputdata.DO_DOCUME_CODE);
                int DO_DOCUME_ID_PADRE = inputdata.DO_DOCUME_ID_PADRE ?? 0;
                this.DO_DOCUME_ID_PADRE = DO_DOCUME_ID_PADRE == 0 ? null : inputdata.DO_DOCUME_ID_PADRE;
                this.DO_DOCUME_FL_PRIVAT = inputdata.DO_DOCUME_FL_PRIVAT;              
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }

        }

        public int getInitialstatus(int DO_TIPDOC_ID) 
        {
            Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
            //le join le fa EF
            //var r = (from t in _context.DO_TIPDOC_document_type
            //         join c in _context.DO_CLADOC_document_class on t.DO_TIPDOC_DO_CLADOC_ID equals c.DO_CLADOC_ID
            //         join s in _context.DO_CLASTA_class_status on c.DO_CLADOC_ID equals s.DO_CLASTA_DO_CLADOC_ID
            //         where t.DO_TIPDOC_ID == DO_TIPDOC_ID && s.DO_CLASTA_INITIAL == true 
            //         select s).FirstOrDefault();

            var r = (from e in _context.DO_TIPDOC_document_type.Where(x => x.DO_TIPDOC_ID == DO_TIPDOC_ID) select e).FirstOrDefault();

            try
            {
                int? stadoc = r.DO_CLADOC_document_class.DO_CLASTA_class_status.Where(y => y.DO_CLASTA_INITIAL == true).FirstOrDefault().DO_CLASTA_DO_STADOC_ID;
                return stadoc ?? 0;
            }

            catch {
                return 0;
            }
          
        }

        public bool getMandatoryExpires()
        {
            Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();

            var r = (from t in _context.DO_TIPDOC_document_type
                     where t.DO_TIPDOC_ID == this.DO_DOCUME_DO_TIPDOC_ID
                     select t).FirstOrDefault();

            return r != null ? (bool)r.DO_TIPDOC_FLAG_MANDATORY_EXP : false;

        }

        public string getPathComplete(int DO_DOCUME_ID)
        {
            Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();

            var r = (from d in _context.DO_DOCUME_documents
                     join t in _context.DO_TIPDOC_document_type on d.DO_DOCUME_DO_TIPDOC_ID equals t.DO_TIPDOC_ID
                     join c in _context.DO_CLADOC_document_class on t.DO_TIPDOC_DO_CLADOC_ID equals c.DO_CLADOC_ID
                     where d.DO_DOCUME_ID == DO_DOCUME_ID
                     select new { Pathdoc = t.DO_TIPDOC_PATH, Pathcla = c.DO_CLADOC_PATH }).FirstOrDefault();

            string filedir = ApplicationSettingsManager.GetRootdirforcustomer();
            return r != null ? Path.Combine(filedir, r.Pathcla,r.Pathdoc) : null;
        }

        public static List<Newtonsoft.Json.Linq.JObject> GetFilePathByDocumentIDs(List<int> DO_DOCUME_ID)
        {
            Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
            string filedir = ApplicationSettingsManager.GetRootdirforcustomer();
            return (from d in _context.DO_DOCUME_documents
                     join t in _context.DO_TIPDOC_document_type on d.DO_DOCUME_DO_TIPDOC_ID equals t.DO_TIPDOC_ID
                     join c in _context.DO_CLADOC_document_class on t.DO_TIPDOC_DO_CLADOC_ID equals c.DO_CLADOC_ID
                     where DO_DOCUME_ID.Contains(d.DO_DOCUME_ID)
                     select new { c = c, t = t, d = d })
                     .AsEnumerable()
                     .Select(e => Newtonsoft.Json.Linq.JObject.FromObject(new { DO_DOCUME_ID = e.d.DO_DOCUME_ID, path = Path.Combine(filedir, e.c.DO_CLADOC_PATH, e.t.DO_TIPDOC_PATH) })).ToList();
        }

        public static DO_DOCUME_RELATED_ASSET GetRelatedAsset(int DO_DOCUME_ID)
        {
            Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
            var r = (from e in _context.AS_ASSET_asset
                     join t in _context.DO_DOCREL_document_relation on e.AS_ASSET_ID equals t.DO_DOCREL_ID_RECORD
                     where t.DO_DOCREL_TABLE_NAME == "AS_ASSET_asset" && t.DO_DOCREL_DO_DOCUME_ID == DO_DOCUME_ID
                     select e).FirstOrDefault();

            DO_DOCUME_RELATED_ASSET related = new DO_DOCUME_RELATED_ASSET();
            if (r != null)
            {
                related.AS_ASSET_ID = r.AS_ASSET_ID;
                related.AS_ASSET_CODE = r.AS_ASSET_CODE;
                related.AS_ASSET_DESCRIPTION = r.AS_ASSET_DESCRIZIONE;
            }
            else
            {
                related.AS_ASSET_CODE = "NN";
                related.AS_ASSET_DESCRIPTION = "Asset non trovato";
            }

            return related;
        }
    }

    public class DO_DOCUME_RELATED_ASSET 
    {
        public int AS_ASSET_ID { get; set; }
        public string AS_ASSET_CODE { get; set; }
        public string AS_ASSET_DESCRIPTION { get; set; }
    }
}


