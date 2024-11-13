using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Dynamic;
using MagicFramework.Helpers;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Ref3.Controllers
{
    public class AssetThemesController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage List(string themeName = null)
        {
            var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            try
            {
                dynamic data = new ExpandoObject();
                data.themeName = themeName;
                var ds = new DatabaseCommandUtils().GetDataSetFromStoredProcedure("Custom.PDT_SP_GEO_ALYSSO2", data);
                JArray themes = CreateThemesArray(ds);
                response.Content = new StringContent(themes.ToString());
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent(ex.Message);
                return response;
            }
            return response;
        }
        private static int getIntFromDataRow(object o)
        {
            return int.Parse(o.ToString());
        }
        private static JArray CreateThemesArray(DataSet ds)
        {
            Dictionary<int, JObject> themes = new Dictionary<int, JObject>();
            List<string[]> columns = new List<string[]>();
            foreach (DataTable table in ds.Tables)
            {
                columns.Add(DBUtils.GetColumnsFromTable(table, new HashSet<string> { "theme_id" }));
            }
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var rowObject = DBUtils.GetObjectFromRow(row, columns[0]);
                rowObject["values"] = new JArray();
                rowObject["items"] = new JArray();
                themes.Add(getIntFromDataRow(row["theme_id"]), rowObject);
            }
            string[] doFor = new string[] { "values", "items" };
            for (int i = 0; i < doFor.Length; i++)
            {
                foreach (DataRow row in ds.Tables[i + 1].Rows)
                {
                    ((JArray)themes[getIntFromDataRow(row["theme_id"])][doFor[i]]).Add(DBUtils.GetObjectFromRow(row, columns[i + 1]));
                }
            }
            return JArray.FromObject(themes.Values);
        }

        // Perform the equivalent of posting a form with a filename and two files, in HTML:
        // <form action="{url}" method="post" enctype="multipart/form-data">
        //     <input type="text" name="filename" />
        //     <input type="file" name="file1" />
        //     <input type="file" name="file2" />
        // </form>
        private async Task<string> Upload(string url, string user, string pwd,string colsArray,string path,string filename)
        {
            // Convert each of the three inputs into HttpContent objects

            // Submit the form using HttpClient and 
            // create form data as Multipart (enctype="multipart/form-data")
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                // Add the HttpContent objects to the form data
                var values = new[]
                {
                    new KeyValuePair<string, string>("user", user),
                    new KeyValuePair<string, string>("pwd", pwd),
                    new KeyValuePair<string, string>("cols", colsArray),
                    new KeyValuePair<string,string>("op","A"),
                    new KeyValuePair<string,string>("skip_header","true")
                     //other values
                };

                foreach (var keyValuePair in values)
                {
                    formData.Add(new StringContent(keyValuePair.Value),
                        String.Format("\"{0}\"", keyValuePair.Key));
                }

                formData.Add(new ByteArrayContent(File.ReadAllBytes(path)),
                    '"' + "data" + '"',
                    '"' + "reftree.csv" + '"');
                string contents;
                try
                {
                    var dbutils = new DatabaseCommandUtils();
                    // Actually invoke the request to the server
                    var response = await client.PostAsync(url, formData);
                    contents = response.Content.ReadAsStringAsync().Result;
                    dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(contents);
                    dbutils.buildAndExecDirectCommandNonQuery(@"if not exists (select * from sys.tables t join sys.schemas s on (t.schema_id = s.schema_id) where s.name = 'CUSTOM' and t.name = 'SISTER_responses')
                                                                BEGIN
                                                                    CREATE TABLE CUSTOM.SISTER_responses(id_richiesta varchar(100),data_richiesta datetime,info nvarchar(max),appid int)
                                                                END
                                                                INSERT INTO CUSTOM.SISTER_responses
                                                               (id_richiesta
                                                               ,data_richiesta
                                                               ,info
                                                               ,appid
                                                               )
                                                            VALUES
                                                               ('" + data.id_richiesta + @"'
                                                               , getdate()
                                                               ,'" + data.info + @"'
                                                               ," + data.appid.ToString() + "')");

                    dbutils.buildAndExecDirectCommandNonQuery(@"INSERT INTO dbo.Magic_NotificationQueue
                                                               (creationDate
                                                               ,user_id
                                                               ,notificationType_ID
                                                               ,notified
                                                               ,send_attempts
                                                               ,message
                                                               )
                                                            VALUES
                                                               (getdate()
                                                               ," + SessionHandler.IdUser.ToString() + @"
                                                               ,2
                                                               ,0
                                                               ,0
                                                               ,'" + contents + "')");
                   

                }
                catch (Exception ex) {
                    throw new System.ArgumentException("Problems while calling Sister:"+ex.Message);
                }
                return contents;
            }
        }
        /// <summary>
        /// SISTER's CSV sender via web request 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> ExportIdeCatAndSendToSister(dynamic data)
        {
            HttpResponseMessage result = MagicFramework.Helpers.Utils.retOkJSONMessage("Sister invoked");
            try
            {
                string encoding = System.Configuration.ConfigurationManager.AppSettings["textencoding"];
                string filename = DateTime.Now.Ticks.ToString() + ".csv";
                Directory.CreateDirectory(Path.Combine(MagicFramework.Helpers.Utils.retRootPathForFiles(), "SISTER_SENT"));
                string path = Path.Combine(MagicFramework.Helpers.Utils.retRootPathForFiles(), "SISTER_SENT", filename);
                string colsArray = data.colsArray.ToString();
                string text = data.selected;
                //defaults forniti da Alysso
                string url = "http://213.21.154.232/sister/sister-crawler-web/m2m-listener";
                string username = "DNGMSL74A47H501D";
                string pass = "TitoeRocco28";
                //Button payload override
                if (data.url != null)
                    url = data.url;
                if (data.us_ != null)
                    username = data.us_;
                if (data.ps_ != null)
                    pass = data.ps_;
                File.WriteAllText(path, text, System.Text.Encoding.GetEncoding(encoding));
                await this.Upload(url, username, pass, colsArray, path, filename);
            }
            catch (Exception ex)
            {
                return Utils.retInternalServerError(ex.Message);
            }
            //immediately released reponse to client
            return result;
        }
    }
}