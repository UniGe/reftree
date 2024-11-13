using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Diagnostics;
using System.Linq;
using System.Web;
using MagicFramework.Helpers;

namespace Ref3.Data
{
    public partial class RefTreeEntities : DbContext
    {
        public RefTreeEntities
            (string connectionStr)
            : base(connectionStr)
        {
            DbInterception.Add(new RefTreeCommandInterceptor());
        }

        

    }
    public class RefTreeCommandInterceptor : IDbCommandInterceptor
    {
        public void setContextInfo(DbCommand command,DbCommandInterceptionContext<int> interceptionContext = null)
        {
            try
            {
                if (command == null)
                    return;
                if (!command.CommandText.Contains("SET CONTEXT_INFO") && (command.CommandText.Contains("INSERT") || command.CommandText.Contains("UPDATE")|| command.CommandText.Contains("DELETE")))
                {
                    //var context = interceptionContext.DbContexts.FirstOrDefault() as RefTreeEntities;
                    string applicationinfo = "{{ \"iduser\": {0}, \"name\":\"{1}\" , \"domain\":\"{2}\" }}";
                    applicationinfo = String.Format(applicationinfo, SessionHandler.IdUser.ToString(), ApplicationSettingsManager.GetAppInstanceName(), HttpContext.Current.Request.Url.ToString());
                    string dbscript = String.Format("DECLARE @temp AS VARBINARY(128); SET @temp = CAST('{0}' AS VARBINARY(128)); SET CONTEXT_INFO @temp;", applicationinfo);
                    command.CommandText = dbscript + command.CommandText;
                    //context.Database.ExecuteSqlCommand(dbscript);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problems during EF interception:" + ex.Message);
            }
        }
        public void NonQueryExecuting(
        DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            this.setContextInfo(command,interceptionContext);
        }

        public void NonQueryExecuted(
            DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
           
        }

        public void ReaderExecuting(
            DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            this.setContextInfo(command);
        }

        public void ReaderExecuted(
            DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
         
        }

        public void ScalarExecuting(
            DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            this.setContextInfo(command);
        }

        public void ScalarExecuted(
            DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            
        }
    }
}
    

   
