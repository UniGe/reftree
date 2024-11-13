using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Dynamic;
using System.Linq.Dynamic;
using MagicFramework.Helpers;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Ionic.Zip;
using OfficeOpenXml;
using System.Web.UI.HtmlControls;
using System.Diagnostics;


namespace Ref3.Controllers
{

    public class DocumentaleController : ApiController
    {

        private const string documentFillFolder = "DocumentFill";

        public static class functioncaller
        {
            public static string DO_V_DOCUME { get { return "DO_V_DOCUME"; } }
            public static string DO_V_DOCUME_ENTITY { get { return "DO_V_DOCUME_ENTITY"; } }
        }

        // the linq to sql context that provides the data access layer	  
        //private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
        //static string groupVisibilityField = ApplicationSettingsManager.GetVisibilityField().ToUpper();
        //PropertyInfo propertyInfo = typeof(Data.DO_V_DOCUME).GetProperty(groupVisibilityField);

        #region Images
        private string MakeThumbnail(int documeID, string path,MagicFramework.Models.FileUploaded file)
        {
            string linkedthumbnail = String.Empty;
            BL.FileGenerator fg = new BL.FileGenerator();
            linkedthumbnail = fg.CreateThumbnail(documeID, path,file);
            return linkedthumbnail;
        }
        private string MakeThumbnail(int documeID, MagicFramework.Models.FileUploaded file)
        {
            string linkedthumbnail = String.Empty;
            BL.FileGenerator fg = new BL.FileGenerator();
            linkedthumbnail = fg.CreateThumbnail(documeID, file);
            return linkedthumbnail;
        }
        private void CreateDocumeThumbs(int id)
        {
            var dbutils = new DatabaseCommandUtils();
            var ds = dbutils.GetDataSet("SELECT DO_DOCVER_ID,DO_DOCVER_LINK_FILE FROM core.DO_V_DOCFIL where DO_CLADOC_FLAG_PHOTO = 1 AND DO_DOCFIL_DO_DOCUME_ID=" + id.ToString() + " AND DO_DOCVER_THUMBNAIL is null", DBConnectionManager.GetTargetConnection());
               var drows = ds.Tables[0].Rows;
               foreach (DataRow m in drows)
               {
                       MagicFramework.Models.FileUploaded f =   Newtonsoft.Json.JsonConvert.DeserializeObject<List<MagicFramework.Models.FileUploaded>>(m["DO_DOCVER_LINK_FILE"].ToString()).First();
                       string tl =  this.MakeThumbnail(id, f);
                       MFLog.LogInFile("Thumb created", MFLog.logtypes.INFO, "ThumbnailCreation.txt");
                       dbutils.buildAndExecDirectCommandNonQuery("UPDATE core.DO_DOCVER_versions set DO_DOCVER_THUMBNAIL_LINK='" + tl.Replace("'","''") + "' where DO_DOCVER_ID=" + m["DO_DOCVER_ID"].ToString());
                }
        }
        [HttpGet]
        public HttpResponseMessage CreateThumbs(int id)
        {
           HttpResponseMessage response = new HttpResponseMessage();
           try
           {
               this.CreateDocumeThumbs(id);
           }
           catch (Exception ex)
           {
                MFLog.LogInFile(ex.Message,MFLog.logtypes.ERROR,"ThumbnailCreation.txt");
                response = MagicFramework.Helpers.Utils.retInternalServerError("Error on creating thumbnail for document");
                return response;
           }
           response = MagicFramework.Helpers.Utils.retOkMessage();
           return response;
        }
        [HttpGet]
        public HttpResponseMessage CreateAssetThumbs(int id)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                //List<int> docIds = (from e in _context.DO_DOCREL_document_relation where e.DO_DOCREL_TABLE_NAME == "AS_ASSET_asset" && e.DO_DOCREL_ID_RECORD == id select e.DO_DOCREL_DO_DOCUME_ID ?? 0).ToList();

                var dbutils = new DatabaseCommandUtils();
                string connString = DBConnectionManager.GetTargetConnection();
                var ds = dbutils.GetDataSet(String.Format("SELECT isnull(DO_DOCREL_DO_DOCUME_ID,0) as Id from core.DO_DOCREL_document_relation where DO_DOCREL_TABLE_NAME='AS_ASSET_asset' and DO_DOCREL_ID_RECORD = {0}", id.ToString()), connString);
                foreach (DataRow dId in ds.Tables[0].Rows)
                    this.CreateDocumeThumbs(int.Parse(dId["Id"].ToString()));
            }
            catch (Exception ex) {
                MFLog.LogInFile(ex.Message, MFLog.logtypes.ERROR, "ThumbnailCreation.txt");
                response = MagicFramework.Helpers.Utils.retInternalServerError("Error on creating thumbnails for asset");
                return response;
            }
            response = MagicFramework.Helpers.Utils.retOkMessage();
            return response;
        }
        #endregion
        [HttpPost]
        public HttpResponseMessage MoveFilesToPath(dynamic data)
        {
            string extension = data.extension;
            string filename = "";
            string destpath = "";
            string startpath = MagicFramework.Helpers.ApplicationSettingsManager.GetRootdirforupload();
            HttpResponseMessage response = Utils.retOkJSONMessage("Files have been moved!");
            try
            {
                foreach (var file in data.files)
                {
                    filename = file.filename;
                    if (Path.GetExtension(filename) != extension)
                        filename = Path.GetFileNameWithoutExtension(filename) + extension;
                    destpath = file.path;
                    Directory.CreateDirectory(destpath);
                    File.Move(Path.Combine(startpath, filename), Path.Combine(destpath, filename));
                }
            }
            catch (Exception ex) {
                response = Utils.retInternalServerError(ex.Message);  
            }
            return response;
        }
       
        private void defaultDynamicDocumentIDs(dynamic data)
        {
            if (data.OBJECT_ID == null)
                data.OBJECT_ID = 0;
            if (data.OBJECT_TYPE_ID == null)
                data.OBJECT_TYPE_ID = 0;
        }
        /// <summary>
        /// removes timestamp form filenames
        /// </summary>
        /// <param name="note"></param>
        /// <returns>the note without timestamp</returns>
        private string cleanFileNote(string note)
        {
            if (String.IsNullOrEmpty(note) || String.IsNullOrWhiteSpace(note))
                return "";
            string res = note;
            int index = note.IndexOf('-');
            if (index >= 0)
            {
                res =  note.Substring(index + 1);
            }
            return Utils.SurroundWith(res, " (", ")");
        }
        private DatabaseCommandUtils.updateresult updateDocumeDataBase(dynamic data, string operation, ref List<MagicFramework.Models.FileUploaded> files, out string dirdest)
        {
            var dbutils = new DatabaseCommandUtils();
            //Get path for the given input
            dirdest = new BL.FileGenerator().getCompletePath(data, dbutils);
          
            if (!String.IsNullOrEmpty(data.fakeforlink.ToString()))
            {
                files = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MagicFramework.Models.FileUploaded>>(data.fakeforlink.ToString());
                //moves files to destinations..
                movefiletopath(dirdest, files);
            }
            var xmlInput = JsonUtils.DynamicToXmlInput_ins_upd_del(data, operation, data.cfgEntityName.ToString());
            DatabaseCommandUtils.updateresult res = dbutils.callStoredProcedurewithXMLInputwithOutputPars(xmlInput, "core.usp_upd_ins_del_docume");
            return res;
        }

        private DatabaseCommandUtils.updateresult updateDocFileDataBase(dynamic data, string operation, ref List<MagicFramework.Models.FileUploaded> files, out string dirdest)
        {
            var dbutils = new DatabaseCommandUtils();
            //Get path for the given input
            dirdest = new BL.FileGenerator().getCompletePath(data, dbutils);
            if (!String.IsNullOrEmpty(data.DO_DOCVER_LINK_FILE.ToString()))
            {
                files = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MagicFramework.Models.FileUploaded>>(data.DO_DOCVER_LINK_FILE.ToString());
                //moves files to destinations..
                movefiletopath(dirdest, files);
            }
            var xmlInput = JsonUtils.DynamicToXmlInput_ins_upd_del(data, operation, data.cfgEntityName.ToString());
            DatabaseCommandUtils.updateresult res = dbutils.callStoredProcedurewithXMLInputwithOutputPars(xmlInput, "core.usp_upd_ins_del_docfil");
            return res;
        }

        [HttpPost]
        public HttpResponseMessage PostU(int id, dynamic data)
        {
            // create a response message to send back
            var response = new HttpResponseMessage();
            try
                {
                    List<MagicFramework.Models.FileUploaded> files = new List<MagicFramework.Models.FileUploaded>();
                    //converto il modello in un oggetto dinamico in modo da poter accedere per nome colonna
                    MagicFramework.Models.GridModelParser modelp = new MagicFramework.Models.GridModelParser(data);
                    string dirdest = String.Empty;
                    //Fills xml container fields with their xml data
                    modelp.FillXMLValues();
                    DatabaseCommandUtils.updateresult res = updateDocumeDataBase(data, "update", ref files, out dirdest);
                    if (res.errorId != "0")
                         throw new System.ArgumentException(res.message);
                    //thumbnail for images
                    foreach (MagicFramework.Models.FileUploaded file in files)
                        MakeThumbnail(id, dirdest, file);
                    response = Utils.retOkJSONMessage(res.message);
                }
                catch (Exception ex)
                {
                    response = Utils.retInternalServerError(ex.Message);
                }
            // return the HTTP Response.
            
            return response;
        }
        //The grid will call this method in insert mode
        [HttpPost]
        public MagicFramework.Models.Response PostI(dynamic data)
        {
            int id = 0;

            // create a response message to send back
            var response = new HttpResponseMessage();
            try
            {
                List<MagicFramework.Models.FileUploaded> files = new List<MagicFramework.Models.FileUploaded>();
                //converto il modello in un oggetto dinamico in modo da poter accedere per nome colonna
                MagicFramework.Models.GridModelParser modelp = new MagicFramework.Models.GridModelParser(data);
                //Fills xml container fields with their xml data
                modelp.FillXMLValues();
                string dirdest = String.Empty;
                //Get path for the given input
                var res = updateDocumeDataBase(data, "create", ref files, out dirdest);
                if (res.errorId != "0")
                    throw new System.ArgumentException(res.message);
                id = int.Parse(res.pkValue);
                //thumbnail for images
                try
                {
                    foreach (MagicFramework.Models.FileUploaded file in files)
                        MakeThumbnail(id, dirdest, file);
                }
                catch (Exception ex) {
                    throw new System.ArgumentException("MakeThumbnail: " + ex.Message);
                }
                // return the HTTP Response.
                var dbres = new DatabaseCommandUtils().GetDataSet("SELECT * FROM core.DO_V_DOCUME WHERE DO_DOCUME_ID=" + id.ToString(), DBConnectionManager.GetTargetConnection()).Tables[0].Rows[0];
                return new MagicFramework.Models.Response(dbres.ItemArray, 1);
            }
            catch (Exception ex)
            {
                return new MagicFramework.Models.Response(String.Format("{0}", ex.Message));
            }
           
        }
        
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
        public List<BL.BoType> GetObjList(MagicFramework.Models.Request request)
        {
            // get inputs from client ss
            var f = request.filter;
            string filter = f.filters[0].value;
            // create data for stored parameters
            dynamic inputdata = new ExpandoObject();
            inputdata.user = MagicFramework.Helpers.SessionHandler.IdUser;
            inputdata.usergroup = MagicFramework.Helpers.SessionHandler.UserVisibilityGroup;
            inputdata.filter = filter;
      
            var dbutils = new DatabaseCommandUtils();
            DataSet dbresult = dbutils.GetDataSetFromStoredProcedure("core.usp_do_botypes_GetList", inputdata);

            List<BL.BoType> res = new List<BL.BoType>();
            foreach (DataRow dr in dbresult.Tables[0].Rows)
                res.Add(new BL.BoType(dr));
      
            return res;
        }



        private void movefiletopath(string dirdest, List<MagicFramework.Models.FileUploaded> files)
        {
            string diruploaded = ApplicationSettingsManager.GetRootdirforupload();
            if (String.IsNullOrEmpty(dirdest))
                throw new System.ArgumentException("movefiletopath: the destination path is null or empty");

            if (!Directory.Exists(dirdest))
            {
                try
                {
                    Directory.CreateDirectory(dirdest);
                }
                catch (Exception ex)
                {
                    throw new System.ArgumentException("movefiletopath create dir:" + ex.Message);
                }
            }
            try
            {
                JObject fileInfo = new JObject();
                JArray movedFiles = new JArray();
                bool trackOperation = ApplicationSettingsManager.trackFiles();

                foreach (MagicFramework.Models.FileUploaded f in files)
                {
                    string filefrom = Path.Combine(diruploaded, f.name);
                    string fileto = Path.Combine(dirdest, f.name);

                    try
                    {
                        File.Move(filefrom, fileto);
                        movedFiles.Add(fileto);
                    }
                    catch (Exception ex)
                    {
                        MagicFramework.Helpers.MFLog.LogInFile(ex.Message, MFLog.logtypes.ERROR);
                        if (trackOperation)
                        {
                            fileInfo["files"] = new JArray { fileto };
                            fileInfo["errorMessage"] = ex.Message;
                            MagicFramework.Controllers.MAGIC_SAVEFILEController.trackFileOperation("systemMoveError", fileInfo);
                        }
                        return;
                    }

                }

                if (trackOperation && movedFiles.Count > 0)
                {
                    fileInfo["files"] = movedFiles;
                    MagicFramework.Controllers.MAGIC_SAVEFILEController.trackFileOperation("systemMove", fileInfo);
                }
            }
            catch (Exception ex) {
                throw new System.ArgumentException("movefiletopath:" + ex.Message);
            }
            
        }
        private bool movefiletopath(int DOCUME_ID, List<MagicFramework.Models.FileUploaded> files)
        {
            string diruploaded = ApplicationSettingsManager.GetRootdirforupload();

            Data.DO_DOCUME_documents d = new Data.DO_DOCUME_documents();
            string dirdest = d.getPathComplete(DOCUME_ID);

            if (dirdest != null && !Directory.Exists(dirdest))
            {
                try
                {
                    Directory.CreateDirectory(dirdest);
                }
                catch (Exception ex)
                {
                    MagicFramework.Helpers.MFLog.LogInFile(ex.Message, MFLog.logtypes.ERROR);
                    return false;
                }
            }

            JObject fileInfo = new JObject();
            JArray movedFiles = new JArray();
            bool trackOperation = ApplicationSettingsManager.trackFiles();

            foreach (MagicFramework.Models.FileUploaded f in files)
            {
                string filefrom = Path.Combine(diruploaded, f.name);
                string fileto = Path.Combine(dirdest, f.name);

                try
                {
                    File.Move(filefrom, fileto);
                    movedFiles.Add(fileto);
                }
                catch (Exception ex)
                {
                    MagicFramework.Helpers.MFLog.LogInFile(ex.Message, MFLog.logtypes.ERROR);
                    if (trackOperation)
                    {
                        fileInfo["files"] = new JArray { fileto };
                        fileInfo["errorMessage"] = ex.Message;
                        MagicFramework.Controllers.MAGIC_SAVEFILEController.trackFileOperation("systemMoveError", fileInfo);
                    }
                    return false;
                }

            }

            if (trackOperation && movedFiles.Count > 0)
            {
                fileInfo["files"] = movedFiles;
                MagicFramework.Controllers.MAGIC_SAVEFILEController.trackFileOperation("systemMove", fileInfo);
            }

            return true;
        }
        [HttpPost]
        public MagicFramework.Models.Response PostIFile(dynamic data)
        {
            try
            {

                List<MagicFramework.Models.FileUploaded> files = new List<MagicFramework.Models.FileUploaded>();
                if (files == null)
                    return new MagicFramework.Models.Response("Nessun file caricato");
                string dirdest = String.Empty;
               
                var res = updateDocFileDataBase(data, "create", ref files, out dirdest);
                if (res.errorId != "0")
                    throw new System.ArgumentException(res.message);
                int id = int.Parse(res.pkValue);

                Data.DO_DOCVER_versions v = new Data.DO_DOCVER_versions();
                string thumbnaillink = String.Empty;
                try
                {
                   this.MakeThumbnail(id,dirdest, files.First());
                }
                catch (Exception ex)
                {
                    MFLog.LogInFile("create thumbnail: " + ex.Message, MFLog.logtypes.ERROR);
                }
                var dbres = new DatabaseCommandUtils().GetDataSet("SELECT * FROM core.DO_V_DOCFIL WHERE DO_DOCFIL_ID=" + id.ToString(), DBConnectionManager.GetTargetConnection()).Tables[0].Rows[0];
                return new MagicFramework.Models.Response(dbres.ItemArray, 1);
           
            }

            catch (Exception ex)
            {
                if (ex.InnerException.InnerException != null)
                {
                    MagicFramework.Helpers.MFLog.LogInFile(ex.InnerException.InnerException.Message, MFLog.logtypes.ERROR);
                }
                return new MagicFramework.Models.Response(ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage PostUFile(int id,dynamic data)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                List<MagicFramework.Models.FileUploaded> files = new List<MagicFramework.Models.FileUploaded>();
                if (files == null)
                    return Utils.retInternalServerError("Nessun file caricato");
                string dirdest = String.Empty;

                var res = updateDocFileDataBase(data, "update", ref files, out dirdest);
                if (res.errorId != "0")
                    throw new System.ArgumentException(res.message);
                try
                {
                    this.MakeThumbnail(id, dirdest, files.First());
                }
                catch (Exception ex)
                {
                    MFLog.LogInFile("create thumbnail: " + ex.Message, MFLog.logtypes.ERROR);
                }
                response = Utils.retOkJSONMessage(res.message);
            }
            catch (Exception ex)
            {
                response = Utils.retInternalServerError(ex.Message);
            }
            return response;
        }

       
        public class documentsData
        {
            public int tipmodId { get; set; }
            public string formData { get; set; }
        }

        public class post
        {
            public List<documents> list { get; set; }
        }

        public class documents
        {
            public int DO_DOCUME_ID { get; set; }
            public int OBJECT_ID { get; set; }
        }
        public class docfil
        {
            public int DO_DOCFIL_DO_DOCUME_ID { get; set; }
            public string DO_DOCVER_LINK_FILE { get; set; }
        }

        [HttpPost]
        public HttpResponseMessage ExportzipforDocument(Data.DO_V_DOCUME data)
        {
            JObject fileInfo = new JObject();
            bool trackOperation = ApplicationSettingsManager.trackFiles();
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                int DO_DOCUME_ID = data.DO_DOCUME_ID;

                ZipFile zip = new ZipFile();

                BL.FileGenerator fg = new BL.FileGenerator();
                string filename = fg.GetFileNameForZip(true);
                string fname = fg.GetFileNameForZip(false);
                JArray outfiles = new JArray();
                bool check = fg.AddDocumentToZip(DO_DOCUME_ID, ref zip, ref outfiles);
                if (!check)
                {
                    if (trackOperation)
                    {
                        fileInfo["files"] = new JArray{ outfiles.Last() };
                        MagicFramework.Controllers.MAGIC_SAVEFILEController.trackFileOperation("downloadError", fileInfo);
                    }
                    return MagicFramework.Helpers.Utils.GetErrorMessageForDownload("File Mancate: " + Path.GetFileName(outfiles.Last().ToString()));
                }
                zip = fg.CloseAndReleaseZip(zip, filename);

                // scarica lo zip                
                fg.AddDownloadToResponse(ref result, filename, fname);
                if (trackOperation)
                {
                    fileInfo["files"] = outfiles;
                    MagicFramework.Controllers.MAGIC_SAVEFILEController.trackFileOperation("download", fileInfo);
                }
            }
            catch (Exception ex)
            {
                result = MagicFramework.Helpers.Utils.GetErrorMessageForDownload("Errore nella produzione del file: " + ex.Message);
            }
            return result;
        }

        [HttpPost]
        public HttpResponseMessage ExportzipforDocumentList(post data)
        {
            JObject fileInfo = new JObject();
            bool trackOperation = ApplicationSettingsManager.trackFiles();
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                ZipFile zip = new ZipFile();
                //BL.FileGenerator fg = new BL.FileGenerator();
                BL.FileGenerator fg = new BL.FileGenerator(true, DBConnectionManager.GetTargetConnection());
                string filename = fg.GetFileNameForZip(true);
                string fname = fg.GetFileNameForZip(false);

                string excelname = fg.GetFileNameForExcel();
                FileInfo path = new FileInfo(excelname);
                ExcelPackage package = new ExcelPackage(path);
                JArray outfiles = new JArray();

                foreach (documents d in data.list)
                {
                    bool check = fg.AddDocumentToZip(d.DO_DOCUME_ID, ref zip, ref outfiles);
                    if (!check)
                    {
                        if (trackOperation)
                        {
                            fileInfo["files"] = new JArray { outfiles.Last() };
                            MagicFramework.Controllers.MAGIC_SAVEFILEController.trackFileOperation("downloadError", fileInfo);
                        }
                        return MagicFramework.Helpers.Utils.GetErrorMessageForDownload("File Mancante:" + Path.GetFileName(outfiles.Last().ToString()));
                    }
                    package = fg.AddDocumentToExcelList(package, d.DO_DOCUME_ID, d.OBJECT_ID);
                }

                package.Save();

                if (File.Exists(excelname))
                    zip = fg.AddFileToZip(excelname, zip);
                zip = fg.CloseAndReleaseZip(zip, filename);
                // scarica lo zip

                fg.AddDownloadToResponse(ref result, filename, fname);
                if (trackOperation)
                {
                    fileInfo["files"] = outfiles;
                    MagicFramework.Controllers.MAGIC_SAVEFILEController.trackFileOperation("download", fileInfo);
                }
            }
            catch (Exception ex)
            {
                result = MagicFramework.Helpers.Utils.GetErrorMessageForDownload("Errore nella produzione del file: " + ex.Message);
            }

            return result;
        }

        [HttpPost]
        public HttpResponseMessage ViewFile(docfil data)
        {
            bool trackOperation = ApplicationSettingsManager.trackFiles();
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                DatabaseCommandUtils dbutils = new DatabaseCommandUtils();
                BL.FileGenerator fg = new BL.FileGenerator();
                // path in cui sono contenuti i file del corrente DO_DOCUME_ID                
                string rootdirfordoc = fg.getCompletePath(data, dbutils);
                string fname = BL.FileGenerator.GetFilenameFromJson(data.DO_DOCVER_LINK_FILE);
                string pathcomplete = Path.Combine(rootdirfordoc, fname);

                JObject fileInfo = new JObject();
                fileInfo["files"] = new JArray { pathcomplete };
                if (File.Exists(@pathcomplete))
                {
                    fg.AddDownloadToResponse(ref result, pathcomplete, fname);
                    result.StatusCode = HttpStatusCode.OK;
                    if (trackOperation)
                        MagicFramework.Controllers.MAGIC_SAVEFILEController.trackFileOperation("download", fileInfo);
                }
                else
                {
                    result = MagicFramework.Helpers.Utils.GetErrorMessageForDownload("File mancante:" + fname);
                    if (trackOperation)
                        MagicFramework.Controllers.MAGIC_SAVEFILEController.trackFileOperation("downloadError", fileInfo);
                    return result;
                }
            }
            catch (Exception ex)
            {
                return MagicFramework.Helpers.Utils.GetErrorMessageForDownload(ex.Message);
            }
            
            var cookie = new CookieHeaderValue("fileDownload", "true");
            cookie.Expires = DateTimeOffset.Now.AddDays(1);
            cookie.Domain = Request.RequestUri.Host;
            cookie.Path = "/";
            result.Headers.AddCookies(new CookieHeaderValue[] { cookie });

            return result;
        }

        [HttpGet]
        public HttpResponseMessage GetDocumentPaths(string documentIDs)
        {
            var res = new HttpResponseMessage();
            res.StatusCode = HttpStatusCode.OK;
            try
            {
                res.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(Data.DO_DOCUME_documents.GetFilePathByDocumentIDs(documentIDs.Split(',').ToList().Select(int.Parse).ToList())));
            }
            catch(Exception e)
            {
                res.StatusCode = HttpStatusCode.InternalServerError;
                res.Content = new StringContent(e.Message);
            }
            return res;
        }

        [HttpPost]
        public string GetTabStrip(dynamic data)
        {
            HtmlGenericControl div = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.div, null, null, null, "tabstrip");
            HtmlGenericControl ul = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.ul, null, null, null, null);

            try
            {
                // vista per Tabstrip elements                                        
                //var res = _context.DO_GET_UI_ELEMENTS(SessionHandler.IdUser, SessionHandler.UserVisibilityGroup).ToList();
                var dbutils = new DatabaseCommandUtils();
                string cstmFiltervalue = data.actionfilter;
                if (cstmFiltervalue == null)
                    cstmFiltervalue = String.Empty;
                dynamic filter = JObject.FromObject(new {
                    actionfilter = cstmFiltervalue
                    });
                var res = dbutils.GetDataSetFromStoredProcedure("core.DO_GET_UI_ELEMENTS_FILTER",filter).Tables[0].Rows;

              
                bool first = true;
                foreach (DataRow t in res)
                {
                    string label = Utils.FirstCharToUpper(t["DO_CLADOC_DESCRIPTION"].ToString()); //+ " (<b>" + t.qtadoc.ToString() + "</b>/<font color='red'><b>" + t.qtasca.ToString() + "</b></font>)";
                    HtmlGenericControl li = HtmlControlsBuilder.GetHtmlControl(HtmlControlTypes.li, first ? "k-state-active" : null, "background-color: " + t["DO_CLADOC_COLOR"].ToString(), label, null);
                    li.Attributes.Add("data-DO_CLADOC_ID", t["DO_CLADOC_ID"].ToString());
                    first = false;
                    ul.Controls.Add(li);
                }
                div.Controls.AddAt(0, ul);
            }
            catch (Exception ex)
            {                
                return (string.Format("Failed with message -{0}", ex.Message));
            }
            return HtmlControlsBuilder.HtmlControlToText(div);
        }

        [HttpPost]
        public HttpResponseMessage BuildDocumentsFromModel(documentsData data)
        {
            try
            {
                dynamic formData = JsonConvert.DeserializeObject(data.formData);
                string connection = DBConnectionManager.GetConnectionFor("core.PS_GRIMOD_model_grid");
                using (SqlConnection PubsConn = new SqlConnection(connection))
                {
                    //EXT is launched after generation has completed (per document) and RET is launched in load 
                    SqlCommand command = new SqlCommand(@"SELECT
                        PS_TIPMOD_STORED_EXT,  
                        PS_TIPMOD_STORED_RET,
                        PS_TIPMOD_LINK_FILE,
                        PS_TIPMOD_FLAG_BATCH,
                        PS_TIPOUT_CODE
                        FROM core.PS_TIPMOD_type_model
                        INNSER JOIN core.PS_TIPOUT_type_output ON PS_TIPMOD_PS_TIPOUT_ID = PS_TIPOUT_ID
                        WHERE PS_TIPMOD_ID = " + data.tipmodId, PubsConn);
                    PubsConn.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();

                    FileInfo templateFile = new FileInfo(reader["PS_TIPMOD_LINK_FILE"].ToString());
                    if(!File.Exists(templateFile.FullName))
                        return Utils.retInternalServerError("File '" + templateFile.FullName + "' not exists");

                    DatabaseCommandUtils dbutils = new DatabaseCommandUtils();
                    DataSet ds = dbutils.GetDataSetFromStoredProcedure(reader["PS_TIPMOD_STORED_EXT"].ToString(), formData);
                    WordDocumentFiller docfiller = new WordDocumentFiller(templateFile.FullName, ds, formData);
                    docfiller.FillDictionariesAndMetaData();

                    if (docfiller.getRootBoCount() == 0)
                        return Utils.retInternalServerError(reader["PS_TIPMOD_STORED_EXT"].ToString() + " did not return any item");

                    MagicFramework.Data.MagicDBDataContext magicDBContext = new MagicFramework.Data.MagicDBDataContext(DBConnectionManager.GetTargetConnection());
                    MagicFramework.Data.Magic_DocumentFillSessions documentFillSession = new MagicFramework.Data.Magic_DocumentFillSessions {
                        InputData = formData.ToString(),
                        User_ID = SessionHandler.IdUser,
                        StartDate = DateTime.Now
                    };
                    magicDBContext.Magic_DocumentFillSessions.InsertOnSubmit(documentFillSession);
                    magicDBContext.SubmitChanges();

                    BL.FileGenerator fileGenerator = new BL.FileGenerator();
                    string directory = Path.Combine(fileGenerator.modelexportdir, documentFillFolder, documentFillSession.ID.ToString());
                    MFLog.LogInFile("check directory starts..", MFLog.logtypes.INFO);

                    Directory.CreateDirectory(directory);
                    if (!Directory.Exists(directory))
                        return Utils.retInternalServerError("Errore nella creazione directory: " + directory);
                    MFLog.LogInFile("check directory ends..", MFLog.logtypes.INFO);

                    bool isBatch = reader["PS_TIPMOD_FLAG_BATCH"] as bool? ?? false;
                    //DateTime expirationDate = DateTime.Now.AddDays(-30);
                    //MFLog.LogInFile("deleting old files starts...", MFLog.logtypes.INFO);
                    //foreach (string dir in Directory.GetDirectories(Path.Combine(fileGenerator.modelexportdir, documentFillFolder)))
                    //{
                    //    if (Directory.GetLastWriteTime(dir) <= expirationDate)
                    //        Directory.Delete(dir, true);
                    //}
                    //MFLog.LogInFile("deleting old files ends...", MFLog.logtypes.INFO);

                    OutputFilesData OutputFilesData = new OutputFilesData
                    {
                        magicDBContext = magicDBContext,
                        documentFillSession = documentFillSession,
                        fileGenerator = fileGenerator,
                        zipName = fileGenerator.GetFileNameForZip(false,true),
                        directory = directory,
                        docfiller = docfiller,
                        templateFile = templateFile,
                        saveAsPdf = reader["PS_TIPOUT_CODE"].ToString() == "pdf",
                        dbutils = dbutils,
                        STORED_RET = reader["PS_TIPMOD_STORED_RET"].ToString(),
                        createZip = !isBatch,
                        targetConnection = DBConnectionManager.GetTargetConnection(),
                        isPublicSite = new MFConfiguration(SessionHandler.ApplicationDomainURL).GetApplicationInstanceByID(SessionHandler.ApplicationDomainURL, SessionHandler.ApplicationInstanceId).includePDFOnly

                    };
                    MFLog.LogInFile("check public site starts..", MFLog.logtypes.INFO);

                    try
                    {
                        if (OutputFilesData.isPublicSite == false)
                            OutputFilesData.isPublicSite = bool.Parse(formData["isPublic"].ToString());
                    }
                    catch (Exception ex) {
                        Debug.WriteLine(ex.Message);
                    }
                    MFLog.LogInFile("check public site ends..", MFLog.logtypes.INFO);

                    OutputFilesSessionData OutputFilesSessionData = new OutputFilesSessionData {
                        idUser = SessionHandler.IdUser, //in the separate Thread the session is not available
                        idUserGroup = SessionHandler.UserVisibilityGroup,
                        connectionString = DBConnectionManager.GetTargetConnection()
                    };
                    bool isMSSQLFileActive = ApplicationSettingsManager.getMSSQLFileTable();
                    if (formData["synch"] == true)
                    {
                        MFLog.LogInFile("buildOutputFiles starts...", MFLog.logtypes.INFO);
                        buildOutputFiles(OutputFilesData, OutputFilesSessionData, isMSSQLFileActive);
                        MFLog.LogInFile("buildOutputFiles ends...", MFLog.logtypes.INFO);
                    }
                    else
                    {
                        Task task = new Task(() => buildOutputFiles(OutputFilesData, OutputFilesSessionData, isMSSQLFileActive));
                        task.Start();
                    }
                    
                    int summaryAddendum = (docfiller.getRootBoCount() == 1 ? 1 : 2);//if the item count is 1 i won't produce the summary file

                    JObject json = new JObject();
                    json.Add("documentFillSessionId", documentFillSession.ID);
                    json.Add("fileCount", docfiller.getRootBoCount() + summaryAddendum); //1 for each RootBo + combine + zip
                    json.Add("zipName", Path.GetFileName(OutputFilesData.zipName));

                    HttpResponseMessage r = new HttpResponseMessage();
                    r.StatusCode = HttpStatusCode.OK;
                    if(!isBatch)
                        r.Content = new StringContent(json.ToString());
                    return r;
                }
            }
            catch (Exception ex)
            {
                return Utils.retInternalServerError("Errore nella produzione dei file: " + ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetExportProgress(int documentFillSessionId, int fileCount, string zipName)
        {
            BL.FileGenerator fileGenerator = new BL.FileGenerator();
            string directory = Path.Combine(fileGenerator.modelexportdir, documentFillFolder, documentFillSessionId.ToString());

            if (File.Exists(Path.Combine(directory, "error.txt")))
                return Utils.retInternalServerError(File.ReadAllText(Path.Combine(directory, "error.txt")));

            float percent = 100;
            int count = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly).Length;

            percent = count > 0 ? ((float)count / (float)fileCount) * 100 : 0;
            if(percent >= 100 && !File.Exists(Path.Combine(directory, zipName)))
                percent = 99;

            return Utils.retOkMessage(percent.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        [HttpGet]
        public HttpResponseMessage GetExportedFile(int documentFillSessionId, string zipName)
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                BL.FileGenerator fileGenerator = new BL.FileGenerator();
                fileGenerator.AddDownloadToResponse(ref result, Path.Combine(fileGenerator.modelexportdir, documentFillFolder, documentFillSessionId.ToString(), zipName), zipName, true);
            }
            catch (Exception ex)
            {
                result = Utils.GetErrorMessageForDownload("Errore nel export del file '"+ zipName + "': " + ex.Message);
            }
            return result;
        }

        private void buildOutputFiles(OutputFilesData data, OutputFilesSessionData sessionData, bool isMSSQLFileActive)
        {
            data.docfiller.BuildOutputFiles(data.directory, data.documentFillSession, data.magicDBContext, data.dbutils, data.targetConnection, sessionData.idUser, sessionData.idUserGroup, data.saveAsPdf, null, 76, isMSSQLFileActive, data.templateFile);

            if (!string.IsNullOrEmpty(data.STORED_RET))
            {
                JObject storedData = new JObject();
                storedData.Add("documentFillSessionID", data.documentFillSession.ID);
                         data.dbutils.GetDataSetFromStoredProcedure(data.STORED_RET, storedData, sessionData.connectionString, null, sessionData.idUser, sessionData.idUserGroup);
            }
            if (data.createZip)
            {
                ZipFile zip = new ZipFile();
                //Se la richiesta viene da un public site e devo produrre PDF zippo solo i PDF
                if (data.saveAsPdf && data.isPublicSite)
                {
                    string[] files = Directory.GetFiles(data.directory,"*.pdf");
                    zip.AddFiles(files,""); 
                }
                else
                    zip.AddDirectory(data.directory);

                zip = data.fileGenerator.CloseAndReleaseZip(zip, Path.Combine(data.directory, data.zipName));
            }

            data.documentFillSession.EndDate = DateTime.Now;
            data.magicDBContext.SubmitChanges();
        }

    }

    public class OutputFilesData
    {
        public MagicFramework.Data.MagicDBDataContext magicDBContext { get; set; }
        public MagicFramework.Data.Magic_DocumentFillSessions documentFillSession { get; set; }
        public BL.FileGenerator fileGenerator { get; set; }
        public string zipName { get; set; }
        public string directory { get; set; }
        public WordDocumentFiller docfiller { get; set; }
        public FileInfo templateFile { get; set; }
        public DatabaseCommandUtils dbutils { get; set; }
        public string STORED_RET { get; set; }
        public bool saveAsPdf { get; set; }
        public bool createZip { get; set; }
        public string targetConnection { get; set; }
        public bool isPublicSite { get; set; }
    }
        public class OutputFilesSessionData
        {
            public int idUser { get; set; }
            public int idUserGroup { get; set; }
            public string connectionString { get; set; }
        }

    public class assets_return_values
    {
        public int AS_ASSET_ID { get; set; }
        public string AS_ASSET_CODE { get; set; }
        public string AS_ASSET_DESCRIZIONE { get; set; }
        public string DESCR_FOR_SEARCH { get; set; }
    }


}