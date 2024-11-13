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
using System.Net.Mail;
using System.Xml;
using System.Diagnostics;
using MagicFramework.Helpers;
using MagicFramework.MemberShip;
using System.Dynamic;
using System.Data;
using Newtonsoft.Json.Linq;


namespace Ref3.Controllers
{


    public class RefTreeSchedulerController : ApiController
    {
        private System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(ApplicationSettingsManager.GetDirectoryForLog());
        #region notifications
        //private MagicFramework.Helpers.DatabaseCommandUtils.updateresult sendSMS(int taskid, string action)
        //{
        //    //send SP 
        //    dynamic input = new ExpandoObject(); //creo un oggetto dinamicamente

        //    input.userID = SessionHandler.IdUser;
        //    input.TaskID = taskid;
        //    input.action = action;
        //    //creazione xml
        //    string json = Newtonsoft.Json.JsonConvert.SerializeObject(input);
        //    var xml = MagicFramework.Helpers.JsonUtils.Json2Xml(json);

        //    var dbutils = new MagicFramework.Helpers.DatabaseCommandUtils();
        //    //chiamo stored
        //    MagicFramework.Helpers.DatabaseCommandUtils.updateresult res = dbutils.callStoredProcedurewithXMLInputwithOutputPars(xml, "dbo.CoursesFromCalendar_SendSMS");
        //    return res;
        //}



        ///// <summary>
        ///// It closes the course and sends mails to supervisors.
        ///// </summary>
        ///// <param name="data">{ "ID":idofthecourse }</param>
        ///// <returns>message</returns>
        //[HttpPost]
        //public HttpResponseMessage CloseCourseAndNotifySupervisors(dynamic data)
        //{
        //    var response = new HttpResponseMessage();
        //    try
        //    {
        //        Data.MyBiz_Entities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.MyBiz_Entities(DBConnectionManager.GetEFManagerConnection()) : new Data.MyBiz_Entities();

        //        int corforid = data.ID;

        //        Data.CORFOR_corsi cor = (from e in _context.CORFOR_corsi where e.CORFOR_ID == corforid select e).FirstOrDefault();

        //        if (cor.T_STACOR_stato_corso.T_STACOR_CODICE == "CHI")
        //        {
        //            response = Utils.retWarning("Il corso risulta essere chiuso. Nessuna notifica inviata");
        //            return response;
        //        }
        //        cor.CORFOR_DATA_TERMINE_CORSO = DateTime.Now;
        //        //Aggiorno la data termine corso e lo stato a chiuso
        //        Data.T_STACOR_stato_corso sta = (from e in _context.T_STACOR_stato_corso where e.T_STACOR_CODICE == "CHI" select e).FirstOrDefault();
        //        cor.T_STACOR_stato_corso = sta;

        //        _context.SaveChanges();
        //        //mando la mail a tutti i supervisors a sistema
        //        List<string> supervisors = (from e in _context.Magic_Mmb_UsersProfiles where e.Magic_Mmb_Profiles.ProfileName == profilosupervisor select e.Magic_Mmb_Users.Email).ToList();
        //        Data.MESEMA_messaggi_email em = (from e in _context.MESEMA_messaggi_email where e.MESEMA_CODICE == "FINECORSO" select e).FirstOrDefault();
        //        string destlist = String.Join(",", supervisors);

        //        SendEventNotification(em.MESEMA_TESTO.Replace("[CORFOR_DESCRIZIONE]",
        //            cor.CORFOR_DESCRIZIONE),
        //            destlist,
        //            null,
        //            em.MESEMA_OGGETTO.Replace("[CORFOR_DESCRIZIONE]", cor.CORFOR_DESCRIZIONE), true, em.MESEMA_FROM);

        //        response = MagicFramework.Helpers.Utils.retOkJSONMessage("Chiusura corso notificata");
        //    }
        //    catch (Exception ex)
        //    {
        //        response = Utils.retInternalServerError(ex.Message);
        //    }

        //    return response;
        //}


        ///// <summary>
        ///// It sends SMS or emails to teachers or students
        ///// </summary>
        ///// <param name="data">{ "ID":"id corso","channel":"SMS/MAIL" , "dest":"TEACHER/STUDENT"  }</param>
        ///// <returns></returns>
        //[HttpPost]
        //public HttpResponseMessage NotifyAll(dynamic data)
        //{
        //    // create a response message to send back
        //    var response = new HttpResponseMessage();

        //    try
        //    {
        //        string message = "";
        //        string msgType = "";
        //        int corforid = int.Parse(data.ID.ToString());
        //        string dest = data.dest;
        //        string channel = data.channel;
        //        Data.MyBiz_Entities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.MyBiz_Entities(DBConnectionManager.GetEFManagerConnection()) : new Data.MyBiz_Entities();

        //        Data.CORDAT_date_corsi cordat = (from e in _context.CORDAT_date_corsi where e.CORDAT_CORFOR_ID == corforid select e).OrderBy(y => y.CORDAT_DATA_DAL).FirstOrDefault();
        //        if (cordat == null)
        //        {
        //            response = Utils.retWarning("Nessuna data presente per il corso selezionato");
        //            return response;
        //        }
        //        Data.CORFOR_corsi corso = cordat.CORFOR_corsi;

        //        string cordatid = cordat.CORDAT_ID.ToString();
        //        int? taskid = (from e in _context.Magic_Calendar where e.LinkedSourceObjectEntity_ID == cordatid && e.Magic_CalendarSourceObjects.DataBaseTable == "dbo.CORDAT_date_corsi" select e.taskId).FirstOrDefault();

        //        if (taskid == null)
        //        {
        //            response = Utils.retInternalServerError("Data corso non associata al calendario");
        //            return response;
        //        }
        //        DatabaseCommandUtils.updateresult res;
        //        if (channel == "SMS")
        //        {
        //            res = this.sendSMS(taskid ?? 0, "notifyall" + dest);
        //            message = res.message;
        //            msgType = res.msgType;
        //            if (dest == "STUDENT")
        //                corso.CORFOR_SMS_DISCENTI = DateTime.Now;
        //            else
        //                corso.CORFOR_SMS_DOCENTI = DateTime.Now;
        //            _context.SaveChanges();
        //        }
        //        if (channel == "MAIL")
        //        {
        //            bool sent = SendMails(corforid, dest);
        //            if (!sent)
        //            {
        //                message = "Mail non inviate. Destinatari non trovati";
        //                msgType = "WARN";
        //            }
        //            if (sent == true)
        //            {
        //                message = "Mail inviate";
        //                if (dest == "STUDENT")
        //                    corso.CORFOR_MAIL_DISCENTI = DateTime.Now;
        //                else
        //                    corso.CORFOR_MAIL_DOCENTI = DateTime.Now;
        //                _context.SaveChanges();
        //            }
        //        }

        //        if (msgType == "WARN")
        //            return Utils.retWarning(message);
        //        else
        //            return Utils.retOkJSONMessage(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        response = Utils.retInternalServerError(ex.Message);
        //    }

        //    return response;

        //}
        //private bool SendMails(int courseid, string dest)
        //{
        //    Data.MyBiz_Entities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.MyBiz_Entities(DBConnectionManager.GetEFManagerConnection()) : new Data.MyBiz_Entities();

        //    var message = (from e in _context.MESEMA_messaggi_email where e.MESEMA_CODICE == "PROGCORSO" select e).FirstOrDefault();
        //    string msgbody = message.MESEMA_TESTO;
        //    int ix = msgbody.IndexOf("</tr>") + 5; //posizione dopo la prima riga
        //    string row = "<tr><td>[CORDAT_DATA_DAL]</td><td>[CORDAT_DATA_AL]</td><td>(ElencoDocenti)</td><td>[LocationDescription]</td></tr>";
        //    string bodyinsertion = String.Empty;
        //    var cordates = (from e in _context.CORDAT_date_corsi where e.CORDAT_CORFOR_ID == courseid select e).ToList();
        //    string descrizionecorso = cordates.FirstOrDefault().CORFOR_corsi.CORFOR_DESCRIZIONE;
        //    string datadocenti = String.Empty;
        //    HashSet<string> listofdestinationaddress = new HashSet<string>();

        //    if (dest == "STUDENT")
        //    {
        //        var students = (from e in _context.CORDIS_discenti where e.CORDIS_CORFOR_ID == courseid select e).ToList();
        //        if (students.Count == 0)
        //        {
        //            return false;
        //        }
        //        foreach (var s in students)
        //        {
        //            if (s.CORDIS_ANAGRA_ID_NOT != null)  //se esiste una persona deputata a ricevere le notifiche in vece del discente
        //            {
        //                if (s.ANAGRA_anagrafica3.ANAGRA_EMAIL != null)
        //                    listofdestinationaddress.Add(s.ANAGRA_anagrafica3.ANAGRA_EMAIL);
        //            }
        //            else
        //                if (s.ANAGRA_anagrafica.ANAGRA_EMAIL != null)
        //                    listofdestinationaddress.Add(s.ANAGRA_anagrafica.ANAGRA_EMAIL);
        //        }
        //    }
        //    foreach (var d in cordates)
        //    {
        //        var assistenti = d.M_DATAST_datecorsi_assistenti.ToList().Select(e => e.ANAGRA_anagrafica).ToList();
        //        var docente = (from e in _context.ANAGRA_anagrafica where e.ANAGRA_ID == d.CORDAT_ANAGRA_ID select e).FirstOrDefault();
        //        string location = d.Magic_CalendarLocations.Description;
        //        List<string> docenti = new List<string>();
        //        foreach (var a in assistenti)
        //        {
        //            if (a.ANAGRA_EMAIL != null && dest == "TEACHER")
        //                listofdestinationaddress.Add(a.ANAGRA_EMAIL);
        //            docenti.Add(a.ANAGRA_NOME + " " + a.ANAGRA_COGNOME + "<small>(assistente)</small>");
        //        }
        //        if (docente.ANAGRA_EMAIL != null && dest == "TEACHER")
        //            listofdestinationaddress.Add(docente.ANAGRA_EMAIL);
        //        docenti.Add(docente.ANAGRA_NOME + " " + docente.ANAGRA_COGNOME);
        //        datadocenti = String.Join(",", docenti);
        //        DateTime from = d.CORDAT_DATA_DAL ?? DateTime.Now;
        //        DateTime to = d.CORDAT_DATA_AL ?? DateTime.Now;
        //        string froms = from.ToLocalTime().ToString("dd-MM-yyyy hh:mm");
        //        string tos = to.ToLocalTime().ToString("dd-MM-yyyy hh:mm");

        //        bodyinsertion += row
        //            .Replace("[CORDAT_DATA_DAL]", froms)
        //           .Replace("[CORDAT_DATA_AL]", tos)
        //           .Replace("(ElencoDocenti)", datadocenti)
        //           .Replace("[LocationDescription]", location);

        //    }
        //    msgbody = msgbody.Insert(ix, bodyinsertion).Replace("[CORFOR_DESCRIZIONE]", descrizionecorso);
        //    string displayname = message.MESEMA_FROM;
        //    if (displayname == null) //se non e' definito a livello DB lo prendo dal web config per questa app 
        //        displayname = ConfigurationManager.AppSettings["App_7_MailFrom"];
        //    SendEventNotification(msgbody, string.Join(",", listofdestinationaddress.ToArray()), null, message.MESEMA_OGGETTO.Replace("[CORFOR_DESCRIZIONE]", descrizionecorso), true, displayname);

        //    List<string> cordatsid = cordates.Select(x => x.CORDAT_ID.ToString()).ToList();

        //    //all the events in the calendar must be set as sent.
        //    if (dest == "STUDENT")
        //    {
        //        var caltasks = (from e in _context.Magic_Calendar where cordatsid.Contains(e.LinkedSourceObjectEntity_ID) && e.Magic_CalendarSourceObjects.DataBaseTable == "dbo.CORDAT_date_corsi" select e).ToList();

        //        foreach (var c in caltasks)
        //        {
        //            c.AlertSent = true;
        //            c.AlertLastDate = DateTime.Now;
        //        }
        //        _context.SaveChanges();
        //    }
        //    return true;

        //}
        //private void SendEventNotification(string body, string Tolist, string CCList, string title, bool ishtml, string senderDisplayName)
        //{
        //    System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
        //    //TODO commentato il form ed il to solo per DEBUG!!!
        //    //msg.To.Add(Tolist); 
        //    msg.To.Add("dario.tortone80@gmail.com,adriano.nardin@ilosgroup.com");
        //    msg.From = new MailAddress(ConfigurationManager.AppSettings["App_7_MailFrom"], senderDisplayName);
        //    msg.Priority = MailPriority.Normal;

        //    msg.Subject = title;
        //    msg.Body = body;
        //    msg.IsBodyHtml = ishtml;
        //    try
        //    {
        //        string mailsmtp = ConfigurationManager.AppSettings["App_7_MailSmtp"];
        //        System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["App_7_SmtpUser"], ConfigurationManager.AppSettings["App_7_SmtpPassword"]);

        //        SmtpClient client = new SmtpClient(mailsmtp);
        //        client.Credentials = credentials;
        //        client.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["App_7_SmtpEnableSsl"] == null ? "true" : ConfigurationManager.AppSettings["App_7_SmtpEnableSsl"]);
        //        client.Port = int.Parse(ConfigurationManager.AppSettings["App_7_SmtpPort"]);

        //        client.Send(msg);
        //    }
        //    catch (Exception ex)
        //    {
        //        MFLog.LogInFile("Error sending mail during notification of the course program event saving method ::SendEventNotification::" + ex.Message, MFLog.logtypes.ERROR);
        //    }

        //}
        #endregion
        #region CRUD

        [HttpPost]
        public MagicFramework.Models.Response Select(MagicFramework.Models.Request request)
        {
            MagicFramework.Helpers.RequestParser rp = new MagicFramework.Helpers.RequestParser(request);

            string order = "start";
            String sqlwherecondition = "1=1";
            if (request.filter != null)
            {
                //crea una where condition che posso applicare in una stored procedure TSQL
                sqlwherecondition = "(" + rp.BuildWhereCondition(typeof(MagicFramework.Models.Magic_Calendar), true) + ")";
                sqlwherecondition = sqlwherecondition.Replace("end", " [end] ");
                sqlwherecondition = sqlwherecondition.Replace("description", " c.[description] ");
                sqlwherecondition = sqlwherecondition.Replace("==", "=");
            }
            // Un utente vede gli eventi del proprio group e dei suoi figli
            sqlwherecondition = "(" + sqlwherecondition + ")";

            if (request.sort != null && request.sort.Count > 0)
                order = rp.BuildOrderCondition();

            var listofgroups = MagicFramework.UserVisibility.UserVisibiltyInfo.GetUserGroupVisibiltyChildrenSet();

            var dbhandler = new MagicFramework.Helpers.DatabaseCommandUtils();

            var xml = MagicFramework.Models.Magic_Calendar.createSelectionPayload(listofgroups, sqlwherecondition, order);
            var dbres = dbhandler.callStoredProcedurewithXMLInput(xml, "CUSTOM.Magic_AppendBOEventsToCalendar");
            List<MagicFramework.Models.Magic_Calendar> retobjlist = new List<MagicFramework.Models.Magic_Calendar>();
            var dbappendedevent = dbres.table.Rows;

            List<int> eventids = new List<int>();

            foreach (DataRow u in dbappendedevent)
            {

                int taskid = int.Parse(u[0].ToString());
                eventids.Add(taskid);

                string title = u["title"].ToString();
                DateTime start = Convert.ToDateTime(u["start"].ToString());
                DateTime end = Convert.ToDateTime(u["end"].ToString());
                string startTimezone = u["startTimezone"].ToString();
                string endTimezone = u["endTimezone"].ToString();
                string description = u["description"].ToString();
                int? recurrenceId = u["recurrenceId"].ToString() == "" ? (int?)null : int.Parse(u["recurrenceId"].ToString());
                string recurrenceRule = u["recurrenceRule"].ToString();
                string recurrenceException = u["recurrenceException"].ToString();
                int? ownerId = u["ownerId"].ToString() == "" ? (int?)null : int.Parse(u["ownerId"].ToString());
                bool isAllDay = Boolean.Parse(u["isAllDay"].ToString());
                int taskType_ID = int.Parse(u["taskType_ID"].ToString());
                int creatorId = int.Parse(u["creatorId"].ToString());
                string OwnerName = u["OwnerName"].ToString();
                int? BusinessObject_ID = u["BusinessObject_ID"].ToString() == "" ? (int?)null : int.Parse(u["BusinessObject_ID"].ToString());
                string BusinessObjectType = u["BusinessObjectType"].ToString();
                int? LinkedModelActivity_ID = u["LinkedModelActivity_ID"].ToString() == "" ? (int?)null : int.Parse(u["LinkedModelActivity_ID"].ToString());
                bool taskActivityCompleted = Boolean.Parse(u["taskActivityCompleted"].ToString() == "" ? "false" : u["taskActivityCompleted"].ToString());
                int? LinkedModelWorkflow_ID = u["LinkedModelWorkflow_ID"].ToString() == "" ? (int?)null : int.Parse(u["LinkedModelWorkflow_ID"].ToString());
                int? LinkedActualWorkflow_ID = u["LinkedActualWorkflow_ID"].ToString() == "" ? (int?)null : int.Parse(u["LinkedActualWorkflow_ID"].ToString());
                string BusinessObjectDescription = u["BusinessObjectDescription"].ToString();
                int? TaskStatusId = u["TaskStatusId"].ToString() == "" ? (int?)null : int.Parse(u["TaskStatusId"].ToString());
                string Notes = u["Notes"].ToString();
                bool Editable = Boolean.Parse(u["Editable"].ToString() == "" ? "false" : u["Editable"].ToString());
                int? LinkedSourceObject_ID = u["LinkedSourceObject_ID"].ToString() == "" ? (int?)null : int.Parse(u["LinkedSourceObject_ID"].ToString());
                string LinkedSourceObjectEntity_ID = u["LinkedSourceObjectEntity_ID"].ToString();
                int? Function_ID = u["Function_ID"].ToString() == "" ? (int?)null : int.Parse(u["Function_ID"].ToString());
                int? location_ID = u["location_ID"].ToString() == "" ? (int?)null : int.Parse(u["location_ID"].ToString());
                string LocationDescription = u["LocationDescription"].ToString();
                string LocationColor = u["LocationColor"].ToString();
                string LocationCode = u["LocationCode"].ToString();
                string att = u["Attendees"].ToString();
                bool alert = bool.Parse(u["Alert"].ToString());
                bool alertsent = bool.Parse(u["AlertSent"].ToString());
                DateTime? alertsentlastdate = u["AlertLastDate"].ToString() == "" ? (DateTime?)null : DateTime.Parse(u["AlertLastDate"].ToString());
                int? usergroupvisibilityid = u["US_AREVIS_ID"].ToString() == "" ? (int?)null : int.Parse(u["US_AREVIS_ID"].ToString());
                DateTime? workflowEnd = null;
                bool has_notifications = false;
                try
                {
                    has_notifications = bool.Parse(u["has_notifications"].ToString());
                    workflowEnd = u["WorkflowEstimatedEnd"].ToString() == "" ? (DateTime?)null : DateTime.Parse(u["WorkflowEstimatedEnd"].ToString());
                }
                catch { }

                List<int?> attendees = new List<int?>();
                if (att.Length > 0)
                    foreach (var a in att.Split(','))
                        attendees.Add(int.Parse(a));

                retobjlist.Add(new MagicFramework.Models.Magic_Calendar(taskid, title, start, end, startTimezone, endTimezone, description, recurrenceId, recurrenceRule,
                    recurrenceException, ownerId, isAllDay, null, taskType_ID, creatorId,
                    OwnerName, BusinessObject_ID, BusinessObjectType, LinkedModelActivity_ID, taskActivityCompleted, LinkedModelWorkflow_ID, LinkedActualWorkflow_ID,
                    BusinessObjectDescription, TaskStatusId, Notes, Editable, attendees, LinkedSourceObject_ID, LinkedSourceObjectEntity_ID, Function_ID,
                    location_ID, LocationDescription, LocationColor, LocationCode, alert, alertsent, alertsentlastdate,usergroupvisibilityid,workflowEnd,has_notifications));
            }

            return new MagicFramework.Models.Response(retobjlist.ToArray(), 0);

        }


        [HttpPost]
        public HttpResponseMessage PostU(int id, dynamic data)
        {
            // create a response message to send back
            var response = new HttpResponseMessage();

            try
            {
                DatabaseCommandUtils dbutils = new DatabaseCommandUtils();
                data.teammembersattendees = Newtonsoft.Json.JsonConvert.SerializeObject(data.teammembersattendees);
                XmlDocument xml = JsonUtils.Json2Xml(data.ToString(), "update", "dbo.Magic_Calendar", id.ToString(), id.ToString(), 0, 0, -1, null, null, null, -1, null,null);

                MagicFramework.Helpers.DatabaseCommandUtils.updateresult updres =  dbutils.callStoredProcedurewithXMLInputwithOutputPars(xml,"core.usp_wf_Calendar_upd_ins_del_stmnt");

                if (updres.errorId != "0")
                {  if (updres.msgType!="WARN")
                        response = Utils.retInternalServerError(updres.message);
                        else
                            response = Utils.retWarning(updres.message);

                        return response;
                }

                response = Utils.retOkMessage();
            }
            catch (Exception ex)
            {
                response = Utils.retInternalServerError(ex.Message);
            }

            return response;

        }


        //The grid will call this method in insert mode

        [HttpPost]
        public HttpResponseMessage PostI(dynamic data)
        {
            // create a response message to send back
            var response = new HttpResponseMessage();

            try
            {
                DatabaseCommandUtils dbutils = new DatabaseCommandUtils();
                data.teammembersattendees = Newtonsoft.Json.JsonConvert.SerializeObject(data.teammembersattendees);
                XmlDocument xml = JsonUtils.Json2Xml(data.ToString(), "create", "dbo.Magic_Calendar", "0","0", 0, 0, -1, null, null, null, -1, null, null);

                MagicFramework.Helpers.DatabaseCommandUtils.updateresult updres = dbutils.callStoredProcedurewithXMLInputwithOutputPars(xml, "core.usp_wf_Calendar_upd_ins_del_stmnt");

                if (updres.errorId != "0")
                {
                    if (updres.msgType != "WARN")
                        response = Utils.retInternalServerError(updres.message);
                    else
                        response = Utils.retWarning(updres.message);

                    return response;
                }
                response = Utils.retOkMessage();

            }
            catch (Exception ex)
            {
                response = Utils.retInternalServerError(ex.Message);
            }

            return response;
        }

        [HttpPost]
        public HttpResponseMessage PostD(int id, dynamic data)
        {
            // create a response message to send back
            var response = new HttpResponseMessage();
            DatabaseCommandUtils dbutils = new DatabaseCommandUtils();
            try
            {

                     XmlDocument xml = JsonUtils.Json2Xml(data.ToString(), "destroy", "dbo.Magic_Calendar", "0", "0", 0, 0, -1, null, null, null, -1, null, null);

                    MagicFramework.Helpers.DatabaseCommandUtils.updateresult updres = dbutils.callStoredProcedurewithXMLInputwithOutputPars(xml, "core.usp_wf_Calendar_upd_ins_del_stmnt");

                    if (updres.errorId != "0")
                    {
                        if (updres.msgType != "WARN")
                            response = Utils.retInternalServerError(updres.message);
                        else
                            response = Utils.retWarning(updres.message);
                        return response;
                    }

                response = Utils.retOkMessage();


            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent(string.Format("Magic_Calendar:The database delete failed with message -{0}", ex.Message));
            }

            // return the HTTP Response.

            return response;
        }
        #endregion
  
    }




}