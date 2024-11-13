using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.Http;
using System.Xml;
using System.Threading;
using MagicFramework.Helpers;

namespace Ref3.Controllers
{

    public class ProcedureController : ApiController
    {
        private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
               
        [HttpPost]
        public HttpResponseMessage GetTabStripData(dynamic data)
        {
            var response = new HttpResponseMessage();

            try
            {
                //var res = _context.JO_GET_UI_ELEMENTS(MagicFramework.Helpers.SessionHandler.IdUser).Where(x => x.US_AREVIS_ID == MagicFramework.Helpers.SessionHandler.UserVisibilityGroup).ToList();
                var res = _context.PP_GET_UI_ELEMENTS(MagicFramework.Helpers.SessionHandler.IdUser).ToList();
                response.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(res));
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent(string.Format("Failed with message -{0}", ex.Message));
            }
            return response;
        }

      
    }
}