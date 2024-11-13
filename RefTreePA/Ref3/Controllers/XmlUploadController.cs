using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using AttributeRouting.Web.Http;
using System.Linq.Dynamic;
using System.Configuration;
using MagicFramework;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using MagicFramework.Helpers;
using System.Dynamic;
using OfficeOpenXml;
using System.Xml;
using System.Data.SqlTypes;

namespace Ref3.Controllers
{
    public class XmlUploadController : ApiController
    {

        // the linq to sql context that provides the data access layer
        private DataSet callExctractSP(string storedprocedure, XmlDocument data)
        {
            DataSet tables = new DataSet();
            string message = String.Empty;
            string connection = DBConnectionManager.GetTargetConnection();
            using (SqlConnection con = new SqlConnection(connection))
            {
                using (SqlCommand cmd = new SqlCommand(storedprocedure, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@xmlInput", SqlDbType.Xml).Value = data.InnerXml;
                    SqlParameter msg = cmd.Parameters.Add("@msg", SqlDbType.VarChar);
                    msg.Direction = ParameterDirection.Output;
                    msg.Size = 4000;
                    SqlParameter errId = cmd.Parameters.Add("@errId", SqlDbType.Int);
                    errId.Direction = ParameterDirection.Output;

                    SqlDataAdapter da = new SqlDataAdapter(); con.Open();
                    da.SelectCommand = cmd;
                    da.Fill(tables);
                    if (errId.Value.ToString() != "0")
                    {
                        message = msg.Value.ToString();
                        throw new System.ArgumentException(message);
                    }
                    da.Dispose();

                }
            }
            return tables;
        }

        [HttpPost]
        public HttpResponseMessage ProcessXml(dynamic data)
        {
            var response = new HttpResponseMessage();
            DataSet outputs;
            try {
                //vado a prendere il file nella dir di upload
                string diruploaded = ApplicationSettingsManager.GetRootdirforupload();
                var  files = Newtonsoft.Json.JsonConvert.DeserializeObject(data.file.ToString());
                List<string> filenames = new List<string>();
                foreach (var f in files)
                {
                    if (Path.GetExtension(f.name.ToString())==".xml")
                    filenames.Add(f.name.ToString());
                }
                var dbu = new DatabaseCommandUtils();
                int modid = int.Parse(data.modxml_id.ToString());
                DataSet ds = dbu.GetDataSet("SELECT * FROM core.XM_XMLMOD_ModelXml where XM_XMLMOD_ID=" + modid, DBConnectionManager.GetTargetConnection());
                DataRow r = ds.Tables[0].Rows[0];

                string sp = r["XM_XMLMOD_PROCESS_SP"].ToString();
                List<string> keys = new List<string>();
                System.Guid key;
                foreach (string filename in filenames)
                {
                    key = System.Guid.NewGuid();
                    keys.Add(key.ToString());
                    ReadFileAndPushInTable(diruploaded, filename,key,modid);
                }
                data.XM_XMLMOD_ID = modid;
                data.XM_XMLIMP_KEYS = String.Join(",", keys);

                XmlDocument inputs = Utils.ConvertDynamicToXML(data);
                XmlNode sessionvars = inputs.CreateNode("element", "SESSIONVARS", "");
                XmlAttribute iduser = inputs.CreateAttribute("iduser");
                iduser.Value = SessionHandler.IdUser.ToString();
                XmlAttribute idbusinessunit = inputs.CreateAttribute("idbusinessunit");
                idbusinessunit.Value = SessionHandler.UserVisibilityGroup.ToString();

                sessionvars.Attributes.Append(iduser);
                sessionvars.Attributes.Append(idbusinessunit);
                inputs.SelectSingleNode("//SQLP").AppendChild(sessionvars);
                //richiamo della stored corrispondente al modello 
                outputs = callExctractSP(sp, inputs);
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent(JsonUtils.convertDataSetToJsonString(outputs));
            }
            catch (Exception ex) {
                response = Utils.retInternalServerError(ex.Message);
                return response;
            }
            return response;
        }

        private  void ReadFileAndPushInTable(string path,string filename,System.Guid key,int modelId)
        {
            XmlDocument doc = new XmlDocument();
            SqlXml newXml = new SqlXml(new XmlTextReader(Path.Combine(path,filename)));

            using (SqlConnection PubsConn = new SqlConnection(DBConnectionManager.GetTargetConnection()))
            {
                using (SqlCommand CMD = new SqlCommand())
                {
                    CMD.Connection = PubsConn;
                    CMD.Parameters.Add("@XM_XMLIMP_CONTENT", SqlDbType.Xml);
                    CMD.Parameters["@XM_XMLIMP_CONTENT"].Value = newXml;
                    CMD.Parameters.Add("@XM_XMLIMP_FILE", SqlDbType.NVarChar);
                    CMD.Parameters["@XM_XMLIMP_FILE"].Value = filename;
                    CMD.Parameters.Add("@XM_XMLIMP_User_ID", SqlDbType.Int);
                    CMD.Parameters["@XM_XMLIMP_User_ID"].Value = SessionHandler.IdUser;
                    CMD.Parameters.Add("@XM_XMLIMP_XM_XMLMOD_ID", SqlDbType.Int);
                    CMD.Parameters["@XM_XMLIMP_XM_XMLMOD_ID"].Value = modelId;
                    CMD.Parameters.Add("@XM_XMLIMP_GUID", SqlDbType.UniqueIdentifier);
                    CMD.Parameters["@XM_XMLIMP_GUID"].Value = key;
                
                    CMD.CommandText = @"INSERT INTO core.XM_XMLIMP_ImportedXML
                                                            (XM_XMLIMP_CONTENT,
                                                             XM_XMLIMP_FILE,
                                                             XM_XMLIMP_User_ID,
                                                             XM_XMLIMP_XM_XMLMOD_ID,
                                                             XM_XMLIMP_GUID) VALUES 
                                                            (@XM_XMLIMP_CONTENT,
                                                             @XM_XMLIMP_FILE,
                                                             @XM_XMLIMP_User_ID,
                                                             @XM_XMLIMP_XM_XMLMOD_ID,
                                                             @XM_XMLIMP_GUID)";
                   CMD.CommandType = CommandType.Text;
                   PubsConn.Open();
                   CMD.ExecuteNonQuery();
                                                      
                }
                
            }

        }
    
   

    }
}