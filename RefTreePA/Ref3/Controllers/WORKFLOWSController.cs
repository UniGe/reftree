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
using System.Dynamic;
using System.Xml;

namespace Ref3.Controllers
{
    public class WORKFLOWSController : ApiController
    {

        //{ id: 10, Typeid: 1, Type: "Censim. nuovo asset", Classid: 1, Class: "Censimento Asset", Catid: 12, actionid: 1, actionDescription: "inserisci asset", actiontype: "Open Stored Procedure", actiontypeid: 1, actioncommand: '{"SP":"dbo.prova"}' 
        public class Activity
        {
            public int actionid { get; set; }   //id attivita' instance
            public string actionDescription { get; set; } //descr attivita' instance
            public int Typeid { get; set; } //id tipo attivita'
            public string Type { get; set; }  // tipo attivita'
            public string Class { get; set; } // descrizione workflow 
            public int Classid { get; set; } // id workflow
            public DateTime StartDate { get; set; } // inizio attivita'
            public DateTime DueDate { get; set; } //scadenza attivita'
            public DateTime CreationDate { get; set; } // data creazione attivita'
            public DateTime AssignmentDate { get; set; }// data assegnamento attivita'
            public int taskId { get; set; } //id calendario
            public int UserID { get; set; } //owner attivita'
            public bool GenerateBO { get; set; } //attivita' genera BO (true)
        }

        [HttpPost]
        public HttpResponseMessage GetActivitiesForFunctionCounter(dynamic data)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            dynamic functionuser = new ExpandoObject();
            int funid = int.Parse(data.FunctionID.ToString());
            functionuser.FunctionID = data.FunctionID;
            functionuser.UserID = SessionHandler.IdUser;
            functionuser.UserGroupID = SessionHandler.UserVisibilityGroup;
            functionuser.FunctionGUID = MagicFramework.Models.Magic_Functions.GetGUIDFromID(funid);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(functionuser); 
            var xml = MagicFramework.Helpers.JsonUtils.Json2Xml(json);

            var dbutils = new MagicFramework.Helpers.DatabaseCommandUtils();

            var dbres = dbutils.callStoredProcedurewithXMLInput(xml, "core.usp_wf_get_pending_acts");
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("{ \"counter\": \""+ dbres.counter.ToString() +"\"  }");

            return response;
           
        }

        [HttpPost]
        public List<Activity> GetActivitiesForFunction(dynamic data)
        {
            List<Activity> activities = new List<Activity>();

            HttpResponseMessage response = new HttpResponseMessage();

            dynamic functionuser = new ExpandoObject();
            int funid = int.Parse(data.FunctionID.ToString());
            functionuser.FunctionID = data.FunctionID;
            functionuser.UserID = SessionHandler.IdUser;
            functionuser.UserGroupID = SessionHandler.UserVisibilityGroup;
            functionuser.FunctionGUID = MagicFramework.Models.Magic_Functions.GetGUIDFromID(funid);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(functionuser);
            var xml = MagicFramework.Helpers.JsonUtils.Json2Xml(json);

            var dbutils = new MagicFramework.Helpers.DatabaseCommandUtils();

            var dbres = dbutils.callStoredProcedurewithXMLInput(xml, "core.usp_wf_get_pending_acts");
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("{ \"counter\": \"" + dbres.counter.ToString() + "\"  }");

            var infos = dbres.table.AsEnumerable().ToList();

            foreach (var r in infos)
            {
                Activity a  = new Activity();
                a.actionid = int.Parse(r[0].ToString());
                a.actionDescription = r[1].ToString();
                a.Typeid = int.Parse(r[2].ToString());
                a.Type = r[3].ToString();
                a.Classid = int.Parse(r[4].ToString());
                a.Class = r[5].ToString();
                a.StartDate = DateTime.Parse(r[6].ToString());
                a.DueDate = DateTime.Parse(r[7].ToString());
                a.CreationDate = DateTime.Parse(r[8].ToString());
                a.AssignmentDate = DateTime.Parse(r[9].ToString());
                a.taskId = int.Parse(r[13].ToString());
                a.UserID = int.Parse(r[14].ToString());
                a.GenerateBO = bool.Parse(r[15].ToString());
                activities.Add(a);
            }


            return activities;

        }

        
        [HttpPost]
        public List<DataRow> GetAttendeesForTask(dynamic data)
        {
             var dbutils = new DatabaseCommandUtils();
             data.schedulerUser_ID = SessionHandler.IdUser;
            
             string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
             XmlDocument xml = MagicFramework.Helpers.JsonUtils.Json2Xml(json);

             var dbres = dbutils.callStoredProcedurewithXMLInput(xml, "core.usp_wf_calendar_guests");
             List<DataRow> result = dbres.table.AsEnumerable().ToList();
             return result;
                
        }
        [HttpPost]
        public List<DataRow> GetSchedulableUsersForTask(dynamic data)
        {
             var dbutils = new DatabaseCommandUtils();
             data.schedulerUser_ID = SessionHandler.IdUser;
            
             string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
             XmlDocument xml = MagicFramework.Helpers.JsonUtils.Json2Xml(json);

             var dbres = dbutils.callStoredProcedurewithXMLInput(xml,"core.usp_wf_get_task_resources");
             List<DataRow> result = dbres.table.AsEnumerable().ToList();
             return result;
                
        }

        [HttpPost]
        public List<DataRow> GetStartableActivities(dynamic data)
        {
            var dbutils = new DatabaseCommandUtils();
            data.schedulerUser_ID = SessionHandler.IdUser;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            XmlDocument xml = MagicFramework.Helpers.JsonUtils.Json2Xml(json);

            var dbres = dbutils.callStoredProcedurewithXMLInput(xml, "core.usp_wf_get_start_activities");
            List<DataRow> result = dbres.table.AsEnumerable().ToList();
            return result;

        }

        [HttpPost]
        public List<DataRow> GetPortfolios(dynamic data)
        {
            var dbutils = new DatabaseCommandUtils();
            data.schedulerUser_ID = SessionHandler.IdUser;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            XmlDocument xml = MagicFramework.Helpers.JsonUtils.Json2Xml(json);

            var dbres = dbutils.callStoredProcedurewithXMLInput(xml, "core.usp_wf_task_get_arevis");
            List<DataRow> result = dbres.table.AsEnumerable().ToList();
            return result;

        }

        [HttpPost]
        public HttpResponseMessage SaveActivityReport([FromBody] string data)
        {
            var r = new HttpResponseMessage { StatusCode = HttpStatusCode.OK};
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(data);
                XmlAttribute attr = doc.CreateAttribute("userId");
                attr.Value = SessionHandler.IdUser.ToString();
                doc.GetElementsByTagName("FORM").Item(0).Attributes.Append(attr);
                DatabaseCommandUtils dcu = new DatabaseCommandUtils();
                var res = dcu.callStoredProcedurewithXMLInputwithOutputPars(doc, "core.WF_SP_PutForm");
                if (res.errorId == "0") r.Content = new StringContent(res.message);
                else r.StatusCode = HttpStatusCode.InternalServerError; r.Content = new StringContent(res.message);

            }
            catch (Exception e)
            {
                r.StatusCode = HttpStatusCode.InternalServerError; r.Content = new StringContent(e.Message);
            }
            return r;
        }
    }
}