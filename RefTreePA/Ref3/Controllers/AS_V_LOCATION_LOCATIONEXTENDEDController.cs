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
    public class AS_V_LOCATION_locationextendedController :ApiController
    {  
	 
      // the linq to sql context that provides the data access layer      
      private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();
	 

	//get a single object 
	[HttpGet]
        public List<Data.AS_V_LOCATION_locationextended> Get(int id)
        {
            var resobj = (from e in _context.AS_V_LOCATION_locationextended.Where(x=> x.LOCATION_ID == id)
                          select e).ToList();
            return resobj;
        }


      //The grid will call this method in read mode
     [HttpPost]
      public MagicFramework.Models.Response Select(MagicFramework.Models.Request request)
      {
          MagicFramework.Helpers.RequestParser rp = new MagicFramework.Helpers.RequestParser(request);
          string order = "Location_ID";
          String wherecondition = "1=1";
          if (request.filter!=null)
              wherecondition = rp.BuildWhereCondition(typeof(Data.AS_V_LOCATION_locationextended));
		  if (request.sort != null && request.sort.Count > 0)
			  order = rp.BuildOrderCondition();

		  
           var dbres= (from e in _context.AS_V_LOCATION_locationextended
							.Where(wherecondition)
							.OrderBy(order.ToString())
							.Skip(request.skip)
							.Take(request.take)                                            
                       select e).ToArray();  
		   
           return new MagicFramework.Models.Response(dbres, _context.AS_V_LOCATION_locationextended.Where(wherecondition).Count());
      }
      [HttpPost]
      public HttpResponseMessage DownloadStaticMaps(dynamic data)
      {
          HttpResponseMessage response = Utils.retOkJSONMessage("Download completed");
          try {
              foreach (var location in data)
              {
                  string zoom = location.zoom;
                  string width = location.width;
                  string height = location.height;

                  int locID = location.LOCATION_ID;
                  DatabaseCommandUtils dbutils = new DatabaseCommandUtils();
                  var ds = dbutils.GetDataSet("Select * FROM core.LOCATION where LOCATION_ID = " + locID.ToString(), DBConnectionManager.GetTargetConnection());
                  string latitude = ds.Tables[0].Rows[0]["LOCATION_LATITUDE"].ToString();
                  string longitude = ds.Tables[0].Rows[0]["LOCATION_LONGITUDE"].ToString();

                  string latlng = latitude + "," + longitude;
                  string url = "http://maps.googleapis.com/maps/api/staticmap?center=" + latlng +
                     "&zoom={2}&size={0}x{1}&maptype=roadmap&markers=color:blue%7Clabel:S%7C" +
                     latlng;
                  url = String.Format(url, width, height, zoom);
                  string filepath = locID.ToString() + ".png";
                  string relativepath = Path.Combine("GOOGLE_STATIC_MAPS", filepath);
                  string path = Path.Combine(MagicFramework.Helpers.Utils.retRootPathForFiles(),relativepath);
                   Directory.CreateDirectory(Path.Combine(MagicFramework.Helpers.Utils.retRootPathForFiles(), "GOOGLE_STATIC_MAPS"));
                  using (WebClient wc = new WebClient())
                  {
                      wc.DownloadFile(url, path);
                      dbutils.buildAndExecDirectCommandNonQuery("UPDATE core.LOCATION set LOCATION_GOOGLE_MAP='" + filepath + "' where LOCATION_ID=" + locID.ToString());
                  }
              }
          
          }
          catch (Exception ex) {
              response = Utils.retInternalServerError(ex.Message);
          }
          return response;
      }
          
    }
}