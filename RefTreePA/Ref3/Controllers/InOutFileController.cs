using System.Linq;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using System.Linq.Dynamic;
using MagicFramework.Helpers;
using System.IO;
using System.Reflection;
using System.Data;
using System.Text.RegularExpressions;

namespace Ref3.Controllers
{

    public class InOutFileController : ApiController
    {            
        // the linq to sql context that provides the data access layer	  
        private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
        static string groupVisibilityField = ApplicationSettingsManager.GetVisibilityField().ToUpper();
        PropertyInfo propertyInfo = typeof(Data.FL_RECFIL_fields).GetProperty(groupVisibilityField);

        /// <summary>
        /// removes timestamp form filenames
        /// </summary>
        /// <param name="note"></param>
        /// <returns>the note without timestamp</returns>
        [HttpPost]
        public HttpResponseMessage GenerateFile(post data)     
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            Newtonsoft.Json.Linq.JObject jobject = Newtonsoft.Json.Linq.JObject.Parse(data.data);

            // TODO: reperire anche i FILINP = Criteri di filtro
            int TIPFIL_ID = (int)jobject["TIPFIL_ID"];

            // recupera tutte le info per produrre file e campi
            var fieldtogen = (from e in _context.FL_RECFIL_fields
                              join r in _context.EX_EXCTIP_type_excel on e.FL_RECFIL_EX_EXCTIP_ID equals r.EX_EXCTIP_ID
                              join f in _context.FL_TIPFIL_type_file on e.FL_RECFIL_FL_TIPFIL_ID equals f.FL_TIPFIL_ID
                              where f.FL_TIPFIL_ID == TIPFIL_ID
                              orderby e.FL_RECFIL_NR_FIELD
                              select e).ToList();

            bool fixlenght = false;
            bool heading = false;
            string filename = "";
            if (fieldtogen.Count > 0)
            {
                fixlenght = fieldtogen[0].FL_TIPFIL_type_file.FL_TIPFIL_LENGH_FIX;   
                heading = fieldtogen[0].FL_TIPFIL_type_file.FL_TIPFIL_FLAG_HEADING;  
                filename = fieldtogen[0].FL_TIPFIL_type_file.FL_TIPFIL_FILE_NAME + '.' + fieldtogen[0].FL_TIPFIL_type_file.FL_TIPFIL_EXTENSION; 
            }
            else
            {
                result = Utils.retInternalServerError("Errore nella configurazione dell'esportazione");
                return result;
            }

            string exportdir = MagicFramework.Helpers.Utils.retcompletepath(ApplicationSettingsManager.GetRootdirforupload());
            string filenamecomplete = Path.Combine(exportdir, filename); 

            string row = string.Empty;

            try
            {
                using (StreamWriter sw = new StreamWriter(filenamecomplete, false, System.Text.Encoding.UTF8))   
                {
                    if (heading)
                    {
                        foreach (Data.FL_RECFIL_fields f in fieldtogen)
                        {
                            if (fixlenght)
                            {
                                row += ManageFieldForLength(f.FL_RECFIL_NAME_HEADER, (int)f.FL_RECFIL_LENGHT);
                            }
                            else
                            {
                                row += f.FL_RECFIL_NAME_HEADER + f.FL_TIPFIL_type_file.FL_TIPFIL_DELIMITER;
                            }
                        }
                        sw.WriteLine(row);
                    }
                    row = string.Empty;

                    DatabaseCommandUtils db = new DatabaseCommandUtils();
                    DataSet ds = db.GetDataSetFromStoredProcedure("custom.SP_ZUCCHETTI_FAKE", data);

                    string fieldtoprint = string.Empty;

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        foreach (Data.FL_RECFIL_fields f in fieldtogen)
                        {
                            if (fixlenght)
                            {
                                if (f.FL_RECFIL_FILLER)
                                {
                                    row += ManageFieldForLength(" ", (int)f.FL_RECFIL_LENGHT);
                                }
                                else
                                {
                                    row += ManageFieldForLength(dr[f.FL_RECFIL_FIELD_NAME_EXCT].ToString(), (int)f.FL_RECFIL_LENGHT);
                                }
                            }
                            else
                            {
                                if (f.FL_RECFIL_FILLER)
                                {
                                    if (f.FL_RECFIL_DEFAULT != null)
                                    {
                                        row += f.FL_RECFIL_DEFAULT;
                                    }
                                }
                                else
                                {
                                    row += dr[f.FL_RECFIL_FIELD_NAME_EXCT].ToString();
                                }
                                row += f.FL_TIPFIL_type_file.FL_TIPFIL_DELIMITER;
                            }
                        }
                        sw.WriteLine(row);
                        row = string.Empty;                    }
                }
            }
            catch (System.Exception ex)
            {
                result = Utils.retInternalServerError(ex.Message);
                return result;
            }

            BL.FileGenerator fg = new BL.FileGenerator();
            fg.AddDownloadToResponse(ref result, filenamecomplete, filename);
            return result;

        }

        private string ManageFieldForLength(string text, int lenght)
        {
            if (text.Length > lenght)
            {
                return text.Substring(0, lenght);
            }
            else
            {
                return text.PadRight(lenght);
            }
        }


    }

    public class post
    {
        public string data { get; set; }
    }
   
    

}