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
    public class US_V_CROSS_AREGRIController : ApiController
    {

        // the linq to sql context that provides the data access layer
        private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
     

        //The grid will call this method in read mode
        [HttpPost]
        public MagicFramework.Models.Response Select(MagicFramework.Models.Request request)
        {
            MagicFramework.Helpers.RequestParser rp = new MagicFramework.Helpers.RequestParser(request);
            string magicgridname = rp.recurinfilters(typeof(Models.US_V_AREGRI_CROSS), request.filter, "MagicGridName");

            string order = "US_AREGRI_ID";
            String wherecondition = "1=1";
            if (request.filter != null)
                wherecondition = rp.BuildWhereCondition(typeof(Models.US_V_AREGRI_CROSS));
            if (request.sort != null && request.sort.Count > 0)
                order = rp.BuildOrderCondition();

            int magicgridid = MagicFramework.Models.Magic_Grids.GetIDFromName(magicgridname);
            try {
        
            var dbres = (from e in _context.utf_us_aregrid_cross(magicgridname,magicgridid)
                                               .Where(wherecondition)
                                               .OrderBy(order.ToString())
                                               .Skip(request.skip)
                                               .Take(request.take)
                         select e).ToArray();
                 return new MagicFramework.Models.Response(dbres, _context.utf_us_aregrid_cross(magicgridname,magicgridid).Where(wherecondition).Count());
                }
            catch (Exception ex) {
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
                string magicgridname  = data.MagicGridName;
                int proareid = data.US_AREGRI_US_PROARE_ID;

                Guid? guid = MagicFramework.Models.Magic_Grids.GetGUIDFromGridName(magicgridname);
                int gridid = MagicFramework.Models.Magic_Grids.GetIDFromName(magicgridname);

                // select the item from the database where the id
                var entityupdate = (from e in _context.US_AREGRI_Arepro_Grids
                                    where e.MagicGridName == magicgridname && e.US_AREGRI_US_PROARE_ID == proareid
                                    select e).FirstOrDefault();

                if (entityupdate != null)
                {
                    _context.US_AREGRI_Arepro_Grids.Remove(entityupdate);
                    _context.SaveChanges();
                }
                else
                {
                    var aregri = new Data.US_AREGRI_Arepro_Grids();
                    aregri.US_AREGRI_US_PROARE_ID = proareid;
                    aregri.US_AREGRI_MagicGridID = gridid;
                    aregri.MagicGridName = magicgridname;
                    aregri.GridGUID = (Guid)guid;
                    _context.US_AREGRI_Arepro_Grids.Add(aregri);
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