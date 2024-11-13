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
    public class STREET_V_STREETEXTENDEDController : ApiController
    {

        // the linq to sql context that provides the data access layer
        private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
     

        //get a single object 
        [HttpGet]
        public List<Data.STREET_V_streetExtended> Get(int id)
        {
            var resobj = (from e in _context.STREET_V_streetExtended.Where(x => x.STREET_ID == id)
                          select e).ToList();
            return resobj;
        }


        //The grid will call this method in read mode
        [HttpPost]
        public MagicFramework.Models.Response Select(MagicFramework.Models.Request request)
        {
            MagicFramework.Helpers.RequestParser rp = new MagicFramework.Helpers.RequestParser(request);
            string order = "STREET_ID";
            String wherecondition = "1=1";
            if (request.filter != null)
                wherecondition = rp.BuildWhereCondition(typeof(Data.AS_V_LOCATION_locationextended));
            if (request.sort != null && request.sort.Count > 0)
                order = rp.BuildOrderCondition();

            
            var dbres = (from e in _context.STREET_V_streetExtended
                              .Where(wherecondition)
                              .OrderBy(order.ToString())
                              .Skip(request.skip)
                              .Take(request.take)
                         select e).ToArray();

            return new MagicFramework.Models.Response(dbres, _context.STREET_V_streetExtended.Where(wherecondition).Count());
        }



    }
}