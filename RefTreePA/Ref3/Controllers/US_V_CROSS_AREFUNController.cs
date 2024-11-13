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
    public class US_V_CROSS_AREFUNController : ApiController
    {

        // the linq to sql context that provides the data access layer
        private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
       
        //The grid will call this method in read mode
        [HttpPost]
        public MagicFramework.Models.Response Select(MagicFramework.Models.Request request)
        {
            MagicFramework.Helpers.RequestParser rp = new MagicFramework.Helpers.RequestParser(request);
            int functionid = int.Parse(rp.recurinfilters(typeof(Models.US_V_AREFUN_CROSS), request.filter, "US_AREFUN_FunctionID"));
            Guid? functionguid = MagicFramework.Models.Magic_Functions.GetGUIDFromID(functionid);

            string order = "US_AREFUN_ID";
            String wherecondition = "1=1";
            if (request.filter != null)
                wherecondition = rp.BuildWhereCondition(typeof(Models.US_V_AREFUN_CROSS));
            if (request.sort != null && request.sort.Count > 0)
                order = rp.BuildOrderCondition();
            try
            {
                var dbres = (from e in _context.utf_us_arefun_cross(functionid, functionguid)
                                                   .Where(wherecondition)
                                                   .OrderBy(order.ToString())
                                                   .Skip(request.skip)
                                                   .Take(request.take)
                             select e).ToArray();
                return new MagicFramework.Models.Response(dbres, _context.utf_us_arefun_cross(functionid, functionguid).Where(wherecondition).Count());
            }
            catch (Exception ex)
            {
                MFLog.LogInFile("problems reading arefun: "+ex.Message, MFLog.logtypes.ERROR);
                return new MagicFramework.Models.Response(ex.Message);
            }
            
        }


        //The grid will call this method in update mode
        [HttpPost]
        public HttpResponseMessage PostU(long id, dynamic data)
        {
            // create a response message to send back
            var response = new HttpResponseMessage();

            try
            {
                Guid functionguid  = Guid.Parse(data.FunctionGUID.ToString());
                int proareid = data.US_AREFUN_US_PROARE_ID;

           
                // select the item from the database where the id
                var entityupdate = (from e in _context.US_AREFUN__Arepro_Functions
                                    where e.FunctionGUID == functionguid && e.US_AREFUN_US_PROARE_ID == proareid
                                    select e).FirstOrDefault();

                if (entityupdate != null)
                {
                    _context.US_AREFUN__Arepro_Functions.Remove(entityupdate);
                    _context.SaveChanges();
                }
                else
                {
                    var arefun = new Data.US_AREFUN__Arepro_Functions();
                    arefun.US_AREFUN_US_PROARE_ID = proareid;
                    arefun.FunctionGUID = functionguid;
                    arefun.US_AREFUN_FunctionID = data.US_AREFUN_FunctionID;
                    _context.US_AREFUN__Arepro_Functions.Add(arefun);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent(string.Format("The database update failed: {0}", ex.Message));
            }

            // return the HTTP Response.
            return response;
        }



    }
}