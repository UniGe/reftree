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
    public class RefTreeSSOController : ApiController
    {
        /// <summary>
        /// Logins a user which is present in a Magic_mmb_Users of a central database with a certain Name-ApplicationName which is corresponding to a target user by Name == Name - and ApplicationName == ApplicationName
        /// </summary>
        /// <param name="token">a unique token present in Magic_mmb_Tokens of the central db</param>
        /// <param name="app">the target app instance</param>
        /// <returns></returns>
        [HttpGet]
        public void LoginByTokenAndMFConnectionUserId(string token, string app)
        {
           
            try
            {
                MFConfiguration.Application configs = new MFConfiguration().GetAppSettings();
                MFConfiguration.ApplicationInstanceConfiguration config = configs.listOfInstances.Where(a => a.appInstancename.Equals(app)).FirstOrDefault();
                //Get the the data from the central db
                MagicFramework.Helpers.Sql.DBQuery query = new MagicFramework.Helpers.Sql.DBQuery("SELECT * FROM dbo.Magic_Mmb_Tokens");
                query.AddWhereCondition("Token = @token", token);
                query.AddWhereCondition("active=1",null);

                query.connectionString = config.MagicDBConnectionString;
                System.Data.DataTable result = query.Execute();
                string uname = String.Empty;

                if (result != null && result.Rows.Count > 0)
                {
                    //Get the username from central DB 
                    string uid = result.Rows[0]["user_id"].ToString();
                    MagicFramework.Helpers.Sql.DBQuery queryUser = new MagicFramework.Helpers.Sql.DBQuery("SELECT * FROM dbo.Magic_Mmb_Users");
                    queryUser.AddWhereCondition("UserID = @uid", uid);
                    queryUser.connectionString = config.MagicDBConnectionString;
                    System.Data.DataTable result2 = queryUser.Execute();
                    uname = result2.Rows[0]["Name"].ToString();
                    if (result2.Rows.Count == 0 || result2 == null)
                        HttpContext.Current.Server.Transfer("/error.aspx?e=UserNotFoundInMagicDBConnection");
                    //Token is found and consumed...
                    using (SqlConnection conn = new SqlConnection(config.MagicDBConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("UPDATE Magic_Mmb_Tokens set active=0,updated_at=getdate() where token='"+ token +"'", conn))
                        {
                            cmd.Connection.Open();
                            cmd.ExecuteNonQuery();
                            cmd.Connection.Close();
                        }
                    }

                    //now let's get the target user 
                    int targetUserId = 0;
                    MagicFramework.Helpers.Sql.DBQuery queryUserTarget = new MagicFramework.Helpers.Sql.DBQuery("SELECT * FROM dbo.Magic_Mmb_Users");
                    queryUserTarget.AddWhereCondition("[Name] = @uname", uname);
                    queryUserTarget.AddWhereCondition("[ApplicationName] = @app", app);
                    queryUserTarget.connectionString = config.TargetDBconn;
                    System.Data.DataTable result2target = queryUserTarget.Execute();
                    if (result2target.Rows.Count == 0 || result2target==null)
                        HttpContext.Current.Server.Transfer("/error.aspx?e=UserNotFoundInTargetConnection");
                    targetUserId = int.Parse(result2target.Rows[0]["UserID"].ToString());
                    this.loginUser(app, targetUserId);
                }
                else
                    HttpContext.Current.Server.Transfer("/error.aspx?e=TokenNotFound");
         
            }
            catch (Exception ex) {
                HttpContext.Current.Server.Transfer("/error.aspx?e=" + HttpUtility.UrlEncode(ex.Message)); ;
            }
        }
        private  void loginUser(string applicationInstance,int userId) {
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
            MagicFramework.Data.Magic_Mmb_Users user = context.Magic_Mmb_Users.Where(u => u.UserID.Equals(userId) && u.ApplicationName.Equals(applicationInstance)).FirstOrDefault();
            if (user == null)
                HttpContext.Current.Server.Transfer("/error.aspx?e=UserNotFound");
            member.SetUserInfosSession(user, context);
            FormsAuthentication.SetAuthCookie(user.Username, true);
            HttpCookie authCookie = HttpContext.Current.Response.Cookies[System.Web.Security.FormsAuthentication.FormsCookieName];
            HttpContext.Current.Cache.Insert(authCookie.Value.ToString(), config.id, null, authCookie.Expires, System.Web.Caching.Cache.NoSlidingExpiration);
            //HttpContext.Current.Cache.Insert(authCookie.Value.ToString() + "_FILTER", filters, null, authCookie.Expires, System.Web.Caching.Cache.NoSlidingExpiration);

            MagicFramework.MemberShip.EFMembershipProvider.ActivateChatAndNotifications(config, user.Username);
            //MagicFramework.Helpers.SessionHandler.Filters = filters;
            HttpContext.Current.Response.Redirect("/" + config.appMainURL);
        }

       
    }
}
