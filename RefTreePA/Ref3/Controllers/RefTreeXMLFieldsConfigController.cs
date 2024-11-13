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
using System.Data;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using MagicFramework.Models;
using System.Diagnostics;
using System.Xml;
using MagicFramework.Helpers;

namespace Ref3.Controllers
{
    public class RefTreeXMLFieldsConfigController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage PostU(int id, dynamic data)
        {
            // create a response message to send back
            var response = new HttpResponseMessage();

            try
            {
                if (data.UpdateUser_ID == null || data.UpdateUser_ID.ToString() == "0") //MagicFramework popola automaticamente un campo chiamato cosi' se nullo 
                    data.UpdateUser_ID = MagicFramework.Helpers.SessionHandler.IdUser;
                // select the item from the database where the id
                var readercmd = new MagicFramework.Helpers.DatabaseCommandUtils();
                GridModelParser gp = new GridModelParser(data);
                //Fills xml container fields with their xml data
                if (data.MagicBOLayerID != null)
                {
                    gp.FillXMLValues();
                }
                var retval = readercmd.execUpdateInsertController(data);
                string jsonreturnmsg = MagicFramework.Helpers.JsonUtils.buildJSONreturnHttpMessage(retval.msgType, retval.message);
                response.Content = new StringContent(jsonreturnmsg);
                response.StatusCode = HttpStatusCode.OK;

                MagicFramework.Helpers.CacheHandler.EmptyCacheForPrefix(MagicFramework.Helpers.CacheHandler.Grids);

            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent(string.Format("Generic Controller:-{0}", ex.Message));
            }

            // return the HTTP Response.
            return response;
        }


        //The grid will call this method in insert mode

        [HttpPost]
        public MagicFramework.Models.Response PostI(dynamic data)
        {
            try
            {

                if (data.CreationUser_ID == null || data.CreationUser_ID.ToString() == "0") //MagicFramework popola automaticamente un campo chiamato cosi' se nullo 
                    data.CreationUser_ID = MagicFramework.Helpers.SessionHandler.IdUser;
                var readercmd = new MagicFramework.Helpers.DatabaseCommandUtils();
                string commandname = data.cfgDataSourceCustomParam;
                GridModelParser gp = new GridModelParser(data);
                //Fills xml container fields with their xml data
                if (data.cfglayerID != null && data.cfglayerID != 0)
                {
                    gp.FillXMLValues();
                }
                var retval = readercmd.execUpdateInsertController(data);
                string operation = data.cfgoperation;
                string entityname = data.cfgEntityName;
                int functionid = data.cfgfunctionID;
                int userid = MagicFramework.Helpers.SessionHandler.IdUser;
                int layer = 0;
                int.TryParse(data.cfglayerID.ToString(), out layer);
                int businessunit = MagicFramework.Helpers.SessionHandler.UserVisibilityGroup;

                string pkname = data.cfgpkName;
                List<string> columns = new List<string>();
                foreach (var c in data.cfgColumns)
                    columns.Add(c.ToString());
                String columnlist = String.Join(",", columns.ToArray());
                string resultpk = retval.pkValue.ToString();
                string whereconditionid = buildwhereconditionid(resultpk, pkname);
                var dbres = readercmd.getGenericControllerReader(entityname, layer, 0, resultpk.Split(',').Length, commandname, whereconditionid, null, functionid, columnlist, null, false, null, null);
                var result = dbres.table.AsEnumerable().ToList().ToArray();
                MagicFramework.Helpers.CacheHandler.EmptyCacheForPrefix(MagicFramework.Helpers.CacheHandler.Grids);
                //se non specificato assumo l' ok 
                if (retval.msgType == null)
                    retval.msgType = "OK";
                if (retval.msgType.ToString() == "WARN")
                    return new MagicFramework.Models.Response(result, result.Length, retval.message);
                else
                    return new MagicFramework.Models.Response(result, result.Length);
            }
            catch (Exception ex)
            {
                return new MagicFramework.Models.Response(ex.Message);
            }
        }

        public string buildwhereconditionid(string retval, string pkname)
        {
            string result = String.Empty;
            List<string> list = new List<string>();
            foreach (var x in retval.Split(','))
            {
                list.Add("'" + x + "'"); //TODO:tratto numerici e stringhe allo stesso modo , da capire con Oracle
            }
            result = String.Join(" OR " + pkname + " = ", list);
            return ("(" + pkname + "=" + result + ")");
        }

        [HttpPost]
        public HttpResponseMessage PostD(int id, dynamic data)
        {
            // create a response message to send back
            var response = new HttpResponseMessage();

            try
            {

                var readercmd = new MagicFramework.Helpers.DatabaseCommandUtils();
                var retval = readercmd.execUpdateInsertController(data);
                MagicFramework.Helpers.CacheHandler.EmptyCacheForPrefix(MagicFramework.Helpers.CacheHandler.Grids);
                response.StatusCode = HttpStatusCode.OK;
                string jsonreturnmsg = MagicFramework.Helpers.JsonUtils.buildJSONreturnHttpMessage(retval.msgType, retval.message);
                response.Content = new StringContent(jsonreturnmsg);
          }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent(string.Format("Generic Controller:The database delete failed with message -{0}", ex.Message));
            }

            // return the HTTP Response.
            return response;
        }
    }
}