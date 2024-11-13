using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MagicFramework.Helpers;

namespace Ref3.Controllers
{
    public class EV_eventsController : ApiController
    {
        private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();

        [HttpPost]
        public List<Data.EV_GRIEVE_grid_events> GetGridEvents(dynamic data)
        {
            int id = data.id;
            var pars = (from e in _context.EV_GRIEVE_grid_events.Where(x => x.EV_GRIEVE_grid_id == id)
                        select e).ToList();
            return pars;
            
        }

        [HttpPost]
        public List<Data.EV_GET_FUNCTIONS_FOR_STATUS_Result> GetFunctionsforStatus(dynamic data)
        { 
            int id = data.record_id;
            string table_name = data.table_name;

            var funcs = _context.EV_GET_FUNCTIONS_FOR_STATUS(table_name, id.ToString()).ToList();

            return funcs;
        }
  
    }
}