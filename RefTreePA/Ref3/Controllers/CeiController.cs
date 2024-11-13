using MagicFramework.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using System.Xml;

namespace Ref3.Controllers
{
    public class CeiController : ApiController
    {
        [HttpGet]
        public void login(string user, string pwd, string token, string app)
        {
            try { 
                MFConfiguration.Application configs = new MFConfiguration().GetAppSettings();
                MFConfiguration.ApplicationInstanceConfiguration config = configs.listOfInstances.Where(a => a.appInstancename.Equals(app)).FirstOrDefault();

                string check = "";
                if (config == null)
                    check = "appConfigNotFound";
                if (String.IsNullOrEmpty(pwd))
                    check = "Empty password";
                      if (String.IsNullOrEmpty(token))
                    check = "Empty token";

                if (!String.IsNullOrEmpty(check))
                    HttpContext.Current.Server.Transfer("/error.aspx?e="+ check);


                if(config != null && !String.IsNullOrEmpty(pwd) && !String.IsNullOrEmpty(token))
                {
                    MagicFramework.MemberShip.EFMembershipProvider.SetConfigSessionAttributes(config, HttpContext.Current.Request);
                    MagicFramework.MemberShip.EFMembershipProvider member = (MagicFramework.MemberShip.EFMembershipProvider)System.Web.Security.Membership.Providers[app];
                    if (member == null)
                        HttpContext.Current.Server.Transfer("/error.aspx?e=membershipProvider");

                    if (member != null)
                    {
                        MagicFramework.Data.MagicDBDataContext context = new MagicFramework.Data.MagicDBDataContext(config.TargetDBconn);
                        MagicFramework.Data.Magic_Mmb_Users User = context.Magic_Mmb_Users.Where(u => u.Username.Equals(user) && u.ApplicationName.Equals(app) && u.IsLockedOut == false).FirstOrDefault();

                        if (User == null)
                            HttpContext.Current.Server.Transfer("/error.aspx?e=UserNotFound");
                        if (!(pwd.StartsWith(token) && pwd.EndsWith(token)))
                            HttpContext.Current.Server.Transfer("/error.aspx?e=TokenPositionOrIncoherent");
                      
                        
                        if (User != null && pwd.StartsWith(token) && pwd.EndsWith(token))
                         {
                                string alreadyEncryptedPwdWithoutToken = pwd.Replace(token, "");
                                if (User.Password.Equals(alreadyEncryptedPwdWithoutToken))
                                {
                                    MagicFramework.Helpers.Sql.DBQuery query = new MagicFramework.Helpers.Sql.DBQuery("SELECT * FROM Custom.UsedLoginTokens");
                                    query.AddWhereCondition("Token = @token", token);
                                    query.AddWhereCondition("UserId = @userId", User.UserID);
                                    query.connectionString = config.TargetDBconn;
                                    System.Data.DataTable result = query.Execute();

                                    if (result == null || result.Rows.Count == 0)
                                    {
                                        MagicFramework.Helpers.Sql.DBWriter writer = new MagicFramework.Helpers.Sql.DBWriter("Custom.UsedLoginTokens", new Dictionary<string, object> {
                                            { "Token", token },
                                            { "UserId", User.UserID },
                                        });
                                        writer.connectionString = config.TargetDBconn;
                                        writer.Write();

                                        //set session vars
                                        member.SetUserInfosSession(User, context, true);

                                        //set auth cookie
                                        System.Web.Security.FormsAuthentication.SetAuthCookie(SessionHandler.Username, true);
                                        HttpCookie authCookie = HttpContext.Current.Response.Cookies[System.Web.Security.FormsAuthentication.FormsCookieName];
                                        HttpContext.Current.Cache.Insert(authCookie.Value.ToString(), config.id, null, authCookie.Expires, System.Web.Caching.Cache.NoSlidingExpiration);

                                        MagicFramework.MemberShip.EFMembershipProvider.ActivateChatAndNotifications(config, SessionHandler.Username);

                                        HttpContext.Current.Response.Redirect("/" + config.appMainURL);
                                        return;
                                    }
                                    else
                                        HttpContext.Current.Server.Transfer("/error.aspx?e=TokenAlreadyUsed");
                                }
                                else
                                    HttpContext.Current.Server.Transfer("/error.aspx?e=pwdiswrong");
                        }
                    }
                }
            } catch(Exception ex) {
                HttpContext.Current.Server.Transfer("/error.aspx?e="+ HttpUtility.UrlEncode(ex.Message + " - " + ex.InnerException));
            }

            HttpContext.Current.Response.Redirect("/login");
        }
        [HttpGet]
        public void loginFromBce(string ID_UTENTE_BCEWEB, string ID_SESSIONE_SU_BCEWEB,string TIPO_CONTESTO_HS, string ID_IMMOBILE_SU_CEIIMMOBILI = null,string TIPO_IMMOBILE = null) {
            try {
                string sender = "BCE";
                string remoteEndPoint = System.Configuration.ConfigurationManager.AppSettings[TIPO_CONTESTO_HS + "_endpoint"];
                //get th sessionData for this user to BCEWEB and punt its contextData in SessionHandler. This data will affect grid filters in session
                var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}?idutente={1}&pass={2}", remoteEndPoint, ID_UTENTE_BCEWEB.ToString(),ID_SESSIONE_SU_BCEWEB.ToString()));
                HttpWebResponse resp = null;
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException we)  //if we got a valid http response, just pass it on to the client
                {
                    resp = (HttpWebResponse)we.Response;
                    if (resp == null)
                        throw we;
                }
       
                Stream response_stream = resp.GetResponseStream();

                StreamReader reader = new StreamReader(response_stream);
                string serialnumberJSON = reader.ReadToEnd();
                reader.Close();

                dynamic serialnumberOBJ = Newtonsoft.Json.JsonConvert.DeserializeObject(serialnumberJSON);
                string ser_number = serialnumberOBJ.serial_number;
                if (String.IsNullOrEmpty(ser_number))
                    HttpContext.Current.Server.Transfer("/error.aspx?e=serial number is null or empty!!!!");
                //get the connection string from web.config 
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[TIPO_CONTESTO_HS].ConnectionString;
                //ask the db for applicationInstance, URL of the function , filters to set  
                JObject data = JObject.FromObject(new
                {
                    id_utente_bce_web = ID_UTENTE_BCEWEB,
                    id_session_su_bce_web = ID_SESSIONE_SU_BCEWEB,
                    tipocontestohs = TIPO_CONTESTO_HS,
                    id_immobile_su_ceiimmobili = ID_IMMOBILE_SU_CEIIMMOBILI,
                    serial_number = ser_number,
                    Sender = sender,
                    tipo_immobile = TIPO_IMMOBILE
                });

                XmlDocument doc = (XmlDocument)Newtonsoft.Json.JsonConvert.DeserializeXmlNode(Newtonsoft.Json.JsonConvert.SerializeObject(data), "SQLP");
                DataSet tables = new DataSet();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand CMD = new SqlCommand("CUSTOM.CEI_LOGIN_FROM_EXTERNAL", connection))
                    {
                        CMD.CommandType = CommandType.StoredProcedure;
                        CMD.Parameters.Add("@XmlInput", SqlDbType.Xml).Value = doc.InnerXml;
                        SqlDataAdapter da = new SqlDataAdapter();
                        connection.Open();
                        da.SelectCommand = CMD;
                        da.Fill(tables);
                        da.Dispose();
                    }
                }

                //get the JSON from stored procedures
                string JSONstring = tables.Tables[0].Rows[0]["sessionData"].ToString();
                loginUser(JSONstring);
                return;
            }
            catch (Exception ex) {
                HttpContext.Current.Server.Transfer("/error.aspx?e=" + HttpUtility.UrlEncode(ex.Message + " - " + ex.InnerException));
            }


        }
        [HttpGet]
        public void loginFromScrivania(string idutente, string tmppassword, string tipocontestosso)
        {
            try
            {
                string sender = "SCRIVANIA";
                string remoteEndPoint = System.Configuration.ConfigurationManager.AppSettings[sender + "_" + tipocontestosso + "_endpoint"];
                //get th sessionData for this user to BCEWEB and punt its contextData in SessionHandler. This data will affect grid filters in session
                var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}?idutente={1}&password={2}", remoteEndPoint, idutente.ToString(), tmppassword.ToString()));
                HttpWebResponse resp = null;
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException we)  //if we got a valid http response, just pass it on to the client
                {
                    resp = (HttpWebResponse)we.Response;
                    if (resp == null)
                        throw we;
                }

                Stream response_stream = resp.GetResponseStream();

                StreamReader reader = new StreamReader(response_stream);
                string ser_number = reader.ReadToEnd();
                reader.Close();

                if (String.IsNullOrEmpty(ser_number))
                    HttpContext.Current.Server.Transfer("/error.aspx?e=serial number is null or empty!!!!");
                //get the connection string from web.config 
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[sender + "_" + tipocontestosso].ConnectionString;
                //ask the db for applicationInstance, URL of the function , filters to set  
                JObject data = JObject.FromObject(new
                {
                    IdUtente = idutente,
                    TmpPassword = tmppassword,
                    TipoContestoSso = tipocontestosso,
                    serial_number = ser_number,
                    Sender = sender
                });

                XmlDocument doc = (XmlDocument)Newtonsoft.Json.JsonConvert.DeserializeXmlNode(Newtonsoft.Json.JsonConvert.SerializeObject(data), "SQLP");
                DataSet tables = new DataSet();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand CMD = new SqlCommand("CUSTOM.CEI_LOGIN_FROM_EXTERNAL", connection))
                    {
                        CMD.CommandType = CommandType.StoredProcedure;
                        CMD.Parameters.Add("@XmlInput", SqlDbType.Xml).Value = doc.InnerXml;
                        SqlDataAdapter da = new SqlDataAdapter();
                        connection.Open();
                        da.SelectCommand = CMD;
                        da.Fill(tables);
                        da.Dispose();
                    }
                }

                //get the JSON from stored procedures
                string JSONstring = tables.Tables[0].Rows[0]["sessionData"].ToString();
                loginUser(JSONstring);
                return;
            }
            catch (Exception ex)
            {
                HttpContext.Current.Server.Transfer("/error.aspx?e=" + HttpUtility.UrlEncode(ex.Message + " - " + ex.InnerException));
            }


        }

        private void loginUser(string JSONFromDB) {
            dynamic dataFromDb = Newtonsoft.Json.JsonConvert.DeserializeObject(JSONFromDB);
            string applicationInstance = dataFromDb.applicationInstance;
            string functionURL = dataFromDb.functionURL;
            //string filters = String.Empty;
            int userId = int.Parse(dataFromDb.userid.ToString());
            //if (dataFromDb.filters != null)
            //    filters = Newtonsoft.Json.JsonConvert.SerializeObject(dataFromDb.filters);
            //Ok that's it let him enter the application 
            MFConfiguration.Application configs = new MFConfiguration().GetAppSettings();
            MFConfiguration.ApplicationInstanceConfiguration config = configs.listOfInstances.Where(a => a.appInstancename.Equals(applicationInstance)).FirstOrDefault();

            if (config == null)
                HttpContext.Current.Server.Transfer("/error.aspx?e=appConfigNotFound");
      
     
            SessionHandler.ApplicationInstanceId = config.id;
            SessionHandler.ApplicationInstanceName = config.appInstancename;
            SessionHandler.CustomFolderName = config.customFolderName != null ? config.customFolderName : config.id;
            SessionHandler.ApplicationDomainURL = Request.RequestUri.Authority;

            MagicFramework.MemberShip.EFMembershipProvider member = (MagicFramework.MemberShip.EFMembershipProvider)Membership.Providers[config.appInstancename];
            if (member == null)
                HttpContext.Current.Server.Transfer("/error.aspx?e=membershipProvider");
            MagicFramework.Data.MagicDBDataContext context = new MagicFramework.Data.MagicDBDataContext(config.TargetDBconn);
            MagicFramework.Data.Magic_Mmb_Users user = context.Magic_Mmb_Users.Where(u => u.UserID.Equals(userId) && u.ApplicationName.Equals(applicationInstance) && u.IsLockedOut == false).FirstOrDefault();
            if (user == null)
                HttpContext.Current.Server.Transfer("/error.aspx?e=UserNotFound");
            member.SetUserInfosSession(user, context);
            FormsAuthentication.SetAuthCookie(user.Username, true);
            HttpCookie authCookie = HttpContext.Current.Response.Cookies[System.Web.Security.FormsAuthentication.FormsCookieName];
            HttpContext.Current.Cache.Insert(authCookie.Value.ToString(), config.id, null, authCookie.Expires, System.Web.Caching.Cache.NoSlidingExpiration);
            //HttpContext.Current.Cache.Insert(authCookie.Value.ToString() + "_FILTER", filters, null, authCookie.Expires, System.Web.Caching.Cache.NoSlidingExpiration);

            MagicFramework.MemberShip.EFMembershipProvider.ActivateChatAndNotifications(config, user.Username);
            //MagicFramework.Helpers.SessionHandler.Filters = filters;
            HttpContext.Current.Response.Redirect("/" + functionURL != null ? functionURL : config.appMainURL);
        }

        [HttpGet]
        public HttpResponseMessage aFakeBceWeb(string idutente, string pass)
        {
            HttpResponseMessage resp = new HttpResponseMessage();
            resp.StatusCode = HttpStatusCode.OK;
            resp.Content = new StringContent("{\"serial_number\":1234123}");
            return resp;

        }

        [HttpGet]
        public HttpResponseMessage aFakeScrivania(string idutente, string password)
        {
            HttpResponseMessage resp = new HttpResponseMessage();
            resp.StatusCode = HttpStatusCode.OK;
            resp.Content = new StringContent("1234123");
            return resp;

        }

        [HttpGet]
        public HttpResponseMessage GetGridFiltersForLoggedUser()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            try
            {
                DatabaseCommandUtils dbutils = new DatabaseCommandUtils();
                dynamic data = JObject.FromObject(new { sessionId = SessionHandler.SessionID });
                var ds = dbutils.GetDataSetFromStoredProcedure("CUSTOM.Magic_GetUserFilter", data);
                var dt = ds.Tables[0];
                response.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(dt));

            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Content = new StringContent(ex.Message);
            }
            return response;
        }
    }
}
