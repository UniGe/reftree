using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using MagicFramework.Helpers;
using Newtonsoft.Json.Linq;

namespace Ref3.Controllers
{
    public class DWGController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Viewer (string q)
        {
            var res = new HttpResponseMessage();
            try
            {
                string key = ConfigurationManager.AppSettings["refAesKey"];
                if (string.IsNullOrEmpty(key)) {
                    throw new Exception("missing settings in web.config");
                }
                byte[] byteKey = Convert.FromBase64String(key);

                string decryptetJSON = Encoding.UTF8.GetString(Crypto.AESCBCDecryptByteArray(Convert.FromBase64String(q), byteKey));

                JObject info = JObject.Parse(decryptetJSON);

                if (info["files"] == null)
                {
                    res.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return res;
                }
                string includesVersion = "/version" + ConfigurationManager.AppSettings["includesVersion"];
                string applicationVersion = Utils.version();

                string html = File.ReadAllText(Utils.GetBasePath() + "/Views/3/dxf-viewer.html");

                // building header
                var header = "<script>window.dxfViewerInfo = " + decryptetJSON + "; window.includesVersion = '" + includesVersion + "'; window.applicationVersion = '" + applicationVersion + "'</script>";
                header += "<script src='" + includesVersion + "/Magic/v/" + applicationVersion + "/Scripts/require.js'></script>";
                header += "<script src='" + includesVersion + "/Magic/v/" + applicationVersion + "/Scripts/config.js'></script>";
                header += "<script src='" + includesVersion + "/Custom/3/Scripts/config.js'></script>";
                header += "<link href='" + includesVersion + "/Magic/v/" + applicationVersion + "/Styles/3rd-party/font-awesome.min.css' rel='stylesheet' />";
                html = html.Replace(
                    "<!--{header}-->"
                    , header
                );
                res.Content = new StringContent(html);
                res.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
            }
            catch (Exception e)
            {
                res.Content = new StringContent(e.ToString());
            }

            return res;
        }
    }
}