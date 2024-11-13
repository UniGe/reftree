using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System.Data.SqlClient;
using MagicFramework.Helpers;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Dynamic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using Ionic.Zip;


namespace Ref3.Controllers
{
    public class BIM_PROJECTSController : ApiController
    {
        public class ProjectPoco
        {
            public int BIM_PROJECT_ID { get; set; }
            public string bimServerProjectId { get; set; }
        }
        /// <summary>
        /// Gets all the available data of the BIM for the last roid
        /// </summary>
        /// <param name="id">The projectID</param>
        /// <returns>the zipped file with available infos of all objects in the IFC file</returns>
        [HttpPost]
        public HttpResponseMessage downloadObjectDataAsZip(ProjectPoco data)
        {

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                var dbutils = new DatabaseCommandUtils();
                DataSet ds = dbutils.GetDataSetFromStoredProcedure("BIM.usp_GetProjectXml", data);
                ZipFile zip = new ZipFile();
                BL.FileGenerator fg = new BL.FileGenerator();
                string zipName = "Project_" + data.bimServerProjectId + "_";
                string filename = fg.GetFileNameForZip(true, zipName);
                string fname = fg.GetFileNameForZip(false, zipName);

                string mainInfo = String.Empty;
                string geomInfo = String.Empty;
                int i = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string main = dr["xmlContent"].ToString();
                    string geom = dr["geometryInfo"].ToString();

                    mainInfo = dr["ifcType"].ToString() + "_" + dr["text"].ToString() + "_" + dr["BIMSrv_rev_id"].ToString() + "_main_" + i.ToString() + "_";
                    geomInfo = dr["ifcType"].ToString() + "_" + dr["text"].ToString() + "_" + dr["BIMSrv_rev_id"].ToString() + "_geom_" + i.ToString() + "_";
                    i++;
                    File.WriteAllText(fg.GetXmlFileNameForFileToAdd(mainInfo), main);
                    File.WriteAllText(fg.GetXmlFileNameForFileToAdd(geomInfo), geom);
                    ZipEntry e = zip.AddFile(fg.GetXmlFileNameForFileToAdd(mainInfo), "");
                    e.Comment = "RefTreeBIM";
                    ZipEntry eg = zip.AddFile(fg.GetXmlFileNameForFileToAdd(geomInfo), "");
                    eg.Comment = "RefTreeBIM";
                }
                zip = fg.CloseAndReleaseZip(zip, filename);
                // scarica lo zip
                fg.AddDownloadToResponse(ref result, filename, fname);
            }
            catch (Exception ex)
            {
                result = MagicFramework.Helpers.Utils.GetErrorMessageForDownload(ex.Message);
            }
            return result;
        }
      
      
    }
}