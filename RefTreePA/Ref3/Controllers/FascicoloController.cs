using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using MagicFramework;
using MagicFramework.Helpers;
using System.Dynamic;
using System.Data;
using System.Reflection;
using System.Xml;
using Newtonsoft.Json.Linq;
using System.IO;
using Ionic.Zip;
using OfficeOpenXml;


namespace Ref3.Controllers
{
    public class FascicoloController : ApiController
    {
       
        [HttpPost]
        public List<BL.DossierItem> GetList(MagicFramework.Models.Request request)
        {
            // get inputs from client s
            var f = request.filter;
            string filter = f.filters[0].value;
            // create data for stored parameters
            dynamic inputdata = new ExpandoObject();
            inputdata.user = MagicFramework.Helpers.SessionHandler.IdUser;
            inputdata.usergroup = MagicFramework.Helpers.SessionHandler.UserVisibilityGroup;
            inputdata.filter = filter;

            var dbutils = new DatabaseCommandUtils();
            DataSet dbresult = dbutils.GetDataSetFromStoredProcedure("core.usp_do_fascicolo_GetList", inputdata);

            List<BL.DossierItem> res = new List<BL.DossierItem>();
            foreach (DataRow dr in dbresult.Tables[0].Rows)
                res.Add(new BL.DossierItem(dr));

            return res;


        }
       
        [HttpPost]
        public List<obj_return_values> GetObjects(dynamic data)
        {
            // Json parameters 
            // {DO_DOSSIE_ID: 1, DO_DOSSIE_DESCRIPTION: "Rogito (F_ROG)", DO_Magic_BusinessObjectType_ID: 1}
            // [{value: "area ", field: "obj_description", operator: "startswith", ignoreCase: true}]

            // get inputs from client  
            DatabaseCommandUtils db = new DatabaseCommandUtils();

            int DO_Magic_BusinessObjectType_ID = data.DO_Magic_BusinessObjectType_ID;
            int AREVIS_ID = MagicFramework.Helpers.SessionHandler.UserVisibilityGroup;

            var f = data.filter;
            string filter = "";
            if (f != null)
            {
                if (f.filters.Count == 0)
                    throw new System.ArgumentException("No filter has been specified!");
                filter = f.filters[0].value.ToString();
            }

            DataSet ds1 = db.GetDataSet("SELECT * FROM Magic_BusinessObjectTypes where ID = " + DO_Magic_BusinessObjectType_ID.ToString(), DBConnectionManager.GetTargetConnection());


            List<obj_return_values> l = new List<obj_return_values>();

            if (ds1.Tables[0].Rows.Count>0)
            {
                var dbret = ds1.Tables[0].Rows[0];
                 // getquery for the current objectId
                string sql = dbret["TAGQuery"].ToString();
                string originalQuery = dbret["TAGQuery"].ToString();
                sql = string.Format(sql, filter, AREVIS_ID);
                //TODO: modifico la select della Magic_BusinessObjectTypes per aggiungere top 100 ed ordinamento, da verificare
                sql = sql.Replace("select", "select top 100");
                sql = sql += " ORDER BY 4";
                //D.T devo fare un' eccezione per gli assets perche' il formato della TAGQUERY e' stato cambiato ed e' fuori standard
                if (dbret["BusinessObjectType"].ToString() == "Asset")
                { 
                    sql = String.Empty;

                    string vcEntityName = "'DOSSIER'";
                    string vcValueRif = data.DO_DOSSIE_ID;
                    if (data.DO_DOSSIE_ID == null)  // non arrivo da funzione fascicolo, ma da elenco documentale
                    {
                        vcEntityName = "'ASSET'";
                        vcValueRif = "null";
                    }

                    sql = originalQuery.Replace("@vcEntityName", vcEntityName).Replace("@vcValueRif", vcValueRif).Replace("@Tag", "'" + filter + "'").Replace("@iArevis", AREVIS_ID.ToString());
                    sql += " ORDER BY 3";
                }
               
             
                System.Data.DataSet ds = db.GetDataSet(sql, null);

                //DataView view = ds.Tables[0].DefaultView;
                //view.Sort = "Description";

                //foreach (System.Data.DataRowView i in view)
                foreach (System.Data.DataRow i in ds.Tables[0].Rows)
                {
                    obj_return_values ret = new obj_return_values();
                    ret.obj_id = Convert.ToInt32(i["BOId"].ToString());
                    ret.obj_description = i["Description"].ToString();
                    ret.obj_type = i["BOType"].ToString();
                    l.Add(ret);
                }
            }

            return l;

        }


        [HttpPost]
        public MagicFramework.Models.Response GetDocuments(MagicFramework.Models.Request request)
        {
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(request.data);

            MagicFramework.Helpers.RequestParser rp = new MagicFramework.Helpers.RequestParser(request);
            string order = "DO_DOCUME_ID";
            String wherecondition = "1=1";
            if (request.filter != null)
                wherecondition = rp.BuildWhereCondition(typeof(Data.DO_V_DOCUME),true);
            if (request.sort != null && request.sort.Count > 0)
                order = rp.BuildOrderCondition();

            XmlDocument xml = JsonUtils.Json2Xml(data.ToString(), "read", "DO_V_DOCUME", "0", "0", request.skip, request.take, -1, "*", wherecondition, order.ToString(), -1, null, null);

            var db = new DatabaseCommandUtils();
            var dbres = db.callStoredProcedurewithXMLInput(xml, "core.DO_SP_DOCUMENTS_L");
            var result = dbres.table.AsEnumerable().ToList().ToArray();
            if (result.Length > 0)
                result = result.Take(1).ToArray();

            return new MagicFramework.Models.Response(result, dbres.counter);
        }

        [HttpPost]
        public MagicFramework.Models.Response GetMissing(MagicFramework.Models.Request request)
        {
            try
            {
                
                PropertyInfo propertyInfo = typeof(Data.DO_V_DOCUME).GetProperty(ApplicationSettingsManager.GetVisibilityField().ToUpper());
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(request.data);

                MagicFramework.Helpers.RequestParser rp = new MagicFramework.Helpers.RequestParser(request);
                string order = "DO_DOCUME_ID";
                String wherecondition = "1=1";
                if (request.filter != null)
                    wherecondition = rp.BuildWhereCondition(typeof(Data.DO_V_DOCUME), true);
                if (request.sort != null && request.sort.Count > 0)
                    order = rp.BuildOrderCondition();

                if (propertyInfo != null)
                {
                    wherecondition = MagicFramework.UserVisibility.UserVisibiltyInfo.getWhereCondition("DO_V_DOCUME", wherecondition);
                    wherecondition = rp.BuildWhereCondition(true);
                }

                XmlDocument xml = JsonUtils.Json2Xml(data.ToString(), "read", "DO_V_DOCUME", "0", "0", request.skip, request.take, -1, "*", wherecondition, order.ToString(), -1, null, null);

                var db = new DatabaseCommandUtils();
                var dbres = db.callStoredProcedurewithXMLInput(xml, "core.usp_do_dossier_GetMissingDocs");
                var result = dbres.table.AsEnumerable().ToList().ToArray();
                if (result.Length > 0)
                    result = result.Take(1).ToArray();

                return new MagicFramework.Models.Response(result, dbres.counter);
            }
            catch (Exception ex) {
                return new MagicFramework.Models.Response(ex.Message);
            }
        }


        public class DossierRequest
        {
            public int DOSSIE_ID { get; set; }
            public int DO_Magic_BusinessObjectType_ID { get; set; }
            public string OBJIDS { get; set; }
            public string request { get; set; }
            public string filters { get; set; }
            public string reqtype { get; set; }
            public string objtype { get; set; }
        }


        [HttpPost]
        public HttpResponseMessage GetDossier(DossierRequest data)
        {
            HttpResponseMessage result = MagicFramework.Helpers.Utils.retOkMessage("Dossier loaded");
            try
            {
                //dynamic d = Newtonsoft.Json.JsonConvert.DeserializeObject(data);
                var dbutils = new DatabaseCommandUtils();
                DossierRequest d = new DossierRequest();
                d.DOSSIE_ID = (int)data.DOSSIE_ID;
                d.DO_Magic_BusinessObjectType_ID = data.DO_Magic_BusinessObjectType_ID;
                d.OBJIDS = data.OBJIDS;
                d.reqtype = data.reqtype;
                d.objtype = data.objtype;

                // Init the file generator preloading all the xml fields for all doctypes and classes
                BL.FileGenerator fg = new BL.FileGenerator(true,DBConnectionManager.GetTargetConnection());
                Data.DO_DOSSIE_dossier dos = new Data.DO_DOSSIE_dossier();

                string fn = d.reqtype == "dossier" ? dos.getdescription(d.DOSSIE_ID) : d.objtype.ToString();
                string fname = fg.GetFileNameForDossier(fn, false);
                string filename = fg.GetFileNameForDossier(fn, true);

               

                JObject fileInfo = new JObject();
                bool trackOperation = ApplicationSettingsManager.trackFiles();
                //List<Data.usp_do_fascicolo_GetDocuments_Result> dbres = new List<Data.usp_do_fascicolo_GetDocuments_Result>();
                MagicFramework.Helpers.DatabaseCommandUtils.readresult dbres;
                XmlDocument xml = new XmlDocument();
              //  System.Data.Entity.Core.Objects.ObjectParameter count = new System.Data.Entity.Core.Objects.ObjectParameter("count", typeof(int));

                try // Recupero dati
                {

                    String wherecondition = "1=1";

                    if (data.filters != null)
                    {
                        MagicFramework.Models.Request req = new MagicFramework.Models.Request();
                        MagicFramework.Models.Filters f = new MagicFramework.Models.Filters(data.filters);
                        req.filter = f;
                        MagicFramework.Helpers.RequestParser rp = new MagicFramework.Helpers.RequestParser(req);
                        wherecondition = rp.BuildWhereCondition(typeof(Data.DO_V_DOCUME));
                        PropertyInfo propertyInfo = typeof(Data.DO_V_DOCUME).GetProperty(ApplicationSettingsManager.GetVisibilityField().ToUpper());
                        if (propertyInfo != null)
                        {
                            wherecondition = MagicFramework.UserVisibility.UserVisibiltyInfo.getWhereCondition("DO_V_DOCUME", wherecondition);
                            wherecondition = rp.BuildWhereCondition(true);
                        }
                    }

                    string datastring = Newtonsoft.Json.JsonConvert.SerializeObject(d);
                    xml = JsonUtils.Json2Xml(datastring, "read", "DO_V_DOCUME", "0", "0", 0, 0, -1, "*", wherecondition, "", -1, null, null);

                    //elenco documenti
                       dbres =  dbutils.callStoredProcedurewithXMLInput(xml, "core.usp_do_fascicolo_GetDocuments");
                }
                catch (Exception ex)
                {
                    throw new System.ArgumentException("Errore nel reperimento dati per il dossier: " + ex.Message);
                }

                ZipFile zip = new ZipFile();
                // limits the ZipFile to use a single thread for compression
                zip.ParallelDeflateThreshold = -1;
              //  string filelosts = fg.GetFileNameForFileToAdd("Filelosts_");

                string excelname = fg.GetFileNameForExcel();
                FileInfo path = new FileInfo(excelname);
                ExcelPackage package = new ExcelPackage(path);
                JArray outfiles = new JArray();
              //  JArray missingFiles = new JArray();
                int DO_DOCUME_ID;
                int? OBJECT_ID = null;
                string DO_CLADOC_DESCRIPTION, DO_TIPDOC_DESCRIPTION, DO_DOCUME_DESCRIPTION;

                foreach (DataRow doc in dbres.table.Rows)
                {
                    DO_DOCUME_ID = int.Parse(doc["DO_DOCUME_ID"].ToString());
                    OBJECT_ID = null;
                    if (!doc.IsNull("OBJECT_ID"))
                        OBJECT_ID = int.Parse(doc["OBJECT_ID"].ToString());
                    DO_CLADOC_DESCRIPTION = doc["DO_CLADOC_DESCRIPTION"].ToString();
                    DO_TIPDOC_DESCRIPTION = doc["DO_TIPDOC_DESCRIPTION"].ToString();
                    DO_DOCUME_DESCRIPTION = doc["DO_DOCUME_DESCRIPTION"].ToString();

                    bool check = fg.AddDocumentToZip(DO_DOCUME_ID, ref zip, ref outfiles);
                    if (check)
                    {
                        package = fg.AddDocumentToExcelList(package, DO_DOCUME_ID, OBJECT_ID);
                    }
                    
                }
                try
                {
                    package.Save();
                }
                catch (Exception ex)
                {
                    string error = "Error saving Excel: " + ex.Message;
                    if (ex.InnerException != null)
                        error += " " + ex.InnerException;
                    throw new System.ArgumentException(error);
                }
                zip = fg.AddFileToZip(excelname, zip);
                zip = fg.CloseAndReleaseZip(zip, filename);

                // scarica lo zip
                fg.AddDownloadToResponse(ref result, filename, fname);

                if (trackOperation)
                {
                    if (outfiles.Count > 0)
                    {
                        fileInfo["files"] = outfiles;
                        MagicFramework.Controllers.MAGIC_SAVEFILEController.trackFileOperation("download", fileInfo);
                    }
                }

            }
            catch (Exception ex) {
                result = MagicFramework.Helpers.Utils.retInternalServerError(ex.Message);
                return result;
            }
            return result;
        }

        private bool addzipdir(ref ZipFile z, string dirname)
        {
            try
            {
                z.AddDirectoryByName(dirname);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

    }

    public class obj_return_values
    {
        public int obj_id { get; set; }
        public string obj_description { get; set; }
        public string obj_type { get; set; }
    }


}