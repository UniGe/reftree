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

namespace Ref3.Controllers
{
    public class M_TIPSAS_TIPASSController : ApiController
    {

        // the linq to sql context that provides the data access layer
        private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
       // static string groupVisibilityField = System.Configuration.ConfigurationManager.AppSettings["groupVisibilityField"];
       // PropertyInfo propertyInfo = typeof(Data.M_TIPSAS_TIPASS).GetProperty(groupVisibilityField);




        //The grid will call this method in read mode
        [HttpPost]
        public MagicFramework.Models.Response Select(MagicFramework.Models.Request request)
        {
            MagicFramework.Helpers.RequestParser rp = new MagicFramework.Helpers.RequestParser(request);
            string order = "viewID";
            String wherecondition = "1=1";
            if (request.filter != null)
                wherecondition = rp.BuildWhereCondition(typeof(Data.M_TIPSAS_TIPASS));
            if (request.sort != null && request.sort.Count > 0)
                order = rp.BuildOrderCondition();

            //if (propertyInfo != null)
            //{
            //    wherecondition = MagicFramework.UserVisibility.UserVisibiltyInfo.getWhereCondition("M_TIPSAS_TIPASS", wherecondition);
            //}

            var dbres = (from e in _context.M_TIPSAS_TIPASS
                              .Where(wherecondition)
                              .OrderBy(order.ToString())
                              .Skip(request.skip)
                              .Take(request.take)
                         select e).ToArray();

            return new MagicFramework.Models.Response(dbres, _context.M_TIPSAS_TIPASS.Where(wherecondition).Count());
        }


        //The grid will call this method in update mode
        [HttpPost]
        public HttpResponseMessage PostU(long id, dynamic data)
        {
            // create a response message to send back
            var response = new HttpResponseMessage();

            try
            {
                int tipassID = data.AS_TIPASS_ID;
                int tipsasID = data.AS_TIPSAS_ID;

                // select the item from the database where the id
                var entityupdate = (from e in _context.AS_SUBTIP_asset_subtipo
                                    where e.AS_SUBTIP_TIPASS_ID == tipassID && e.AS_SUBTIP_TIPSAS_ID == tipsasID
                                    select e).FirstOrDefault();

                if (entityupdate != null)
                {
                    _context.AS_SUBTIP_asset_subtipo.Remove(entityupdate);
                    _context.SaveChanges();
                }
                else
                {
                    var subtip = new Data.AS_SUBTIP_asset_subtipo();
                    subtip.AS_SUBTIP_TIPASS_ID = tipassID;
                    subtip.AS_SUBTIP_TIPSAS_ID = tipsasID;
                    _context.AS_SUBTIP_asset_subtipo.Add(subtip);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent(string.Format("The database updated failed: {0}", ex.Message));
            }

            // return the HTTP Response.
            return response;
        }



    }
}