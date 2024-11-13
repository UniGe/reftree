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
using OfficeOpenXml;
using System.Xml;

namespace Ref3.Controllers
{
    public class ExeclUploadController : ApiController
    {

        // the linq to sql context that provides the data access layer
        private Data.RefTreeEntities _context = DBConnectionManager.GetEFManagerConnection() != null ? new Data.RefTreeEntities(DBConnectionManager.GetEFManagerConnection()) : new Data.RefTreeEntities();



        private DataSet callExctractSP(string storedprocedure,XmlDocument data)
        {
            DataSet tables = new DataSet();
            string error = String.Empty;
            string connection = DBConnectionManager.GetTargetConnection();
            using (SqlConnection con = new SqlConnection(connection))
            {
                using (SqlCommand cmd = new SqlCommand(storedprocedure, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300; //5 mins
                    //cmd.Parameters.Add("@XMLDOCUMENT", SqlDbType.Xml).Value = doc.InnerXml;
                    cmd.Parameters.Add("@XMLDATA", SqlDbType.Xml).Value = data.InnerXml;
                    SqlParameter err = cmd.Parameters.Add("@errormsg", SqlDbType.VarChar);
                    err.Direction  = ParameterDirection.Output;
                    err.Size = 4000;
                    SqlDataAdapter da = new SqlDataAdapter(); con.Open();
                    da.SelectCommand = cmd;
                    da.Fill(tables);
                    error = err.Value.ToString();
                    if (error != String.Empty)
                        throw new System.ArgumentException(error);
                    da.Dispose();
                    
                }
            }
            return tables;
        }

        [HttpPost]
        public HttpResponseMessage ProcessExcel(dynamic data)
        {
            var response = new HttpResponseMessage();
            DataSet outputs;
            try {
                //vado a prendere il file nella dir di upload
                string diruploaded = ApplicationSettingsManager.GetRootdirforupload();
                string filename = Newtonsoft.Json.JsonConvert.DeserializeObject(data.file.ToString())[0].name;
                var dbu = new DatabaseCommandUtils();
                string modid = data.exctractModelId.ToString();
                DataSet ds = dbu.GetDataSet("SELECT * FROM core.EX_MODEXC_excel_model where EX_MODEXC_ID=" + modid, DBConnectionManager.GetTargetConnection());
                DataRow r = ds.Tables[0].Rows[0];

                int take = int.Parse(r["EX_MODEXC_NR_ROWS_MAX"].ToString() == "" ? "100000" : r["EX_MODEXC_NR_ROWS_MAX"].ToString());
                int skip = int.Parse(r["EX_MODEXC_START_ROW"].ToString() == "" ? "0" : r["EX_MODEXC_START_ROW"].ToString());
                int takecols = int.Parse(r["EX_MODEXC_NR_COLUMNS"].ToString() == "" ? "50" : r["EX_MODEXC_NR_COLUMNS"].ToString());
                int sheetidx = int.Parse(r["EX_MODEXC_SHEET_IDX"].ToString() == "" ? "1" : r["EX_MODEXC_SHEET_IDX"].ToString());
                string sp = r["EX_MODEXC_SP_NO_STANDARD"].ToString() == "" ? "core.EX_PROCESS_XML_EXCEL" : r["EX_MODEXC_SP_NO_STANDARD"].ToString();

                string key = System.Guid.NewGuid().ToString();
                ReadFileAndPushInTable(diruploaded, filename, sheetidx, skip, take, takecols,key);
                
                data.iduser = SessionHandler.IdUser;
                data.AREVIS_ID = SessionHandler.UserVisibilityGroup;

                data.EX_EXCIMP_ID = key;
                XmlDocument inputs = Utils.ConvertDynamicToXML(data);

                outputs = callExctractSP(sp, inputs);
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent(JsonUtils.convertDataSetToJsonString(outputs));
            }
            catch (Exception ex) {
                response = Utils.retInternalServerError(ex.Message);
                return response;
            }
            return response;
        }

        private  void ReadFileAndPushInTable(string path,string filename,int worksheetidx,int skip,int take,int takecols,string key)
        {
            XmlDocument xdoc = new XmlDocument();
            
            DirectoryInfo d = new DirectoryInfo(path);

            FileInfo f = new FileInfo(Path.Combine(path, filename));

            using (var package = new ExcelPackage(f))
            {
                //ExcelWorkbook workBook = package.Workbook;                    
                // per ogni foglio di lavoro nel file excel 
                //foreach (ExcelWorksheet ws in workBook.Worksheets)

                ExcelWorksheet ws = package.Workbook.Worksheets[worksheetidx];
                int maxrows = ws.Dimension.Rows;
                int maxcols = ws.Dimension.Columns;
                int limitrows = maxrows;
                if (skip>maxrows)
                    throw new ArgumentException("skip is greater than number of rows in the worksheet");
                if (takecols>maxcols)
                    takecols = maxcols;
                if ((skip+take)<=maxrows)
                    limitrows = skip+take;
                   
                DataTable dt = ToDataTable(ws,skip,take,takecols,false);
                string tablename = "CUSTOM.TB_" + DateTime.Now.Ticks.ToString();
                //dt.TableName = "TB_"+ DateTime.Now.Ticks.ToString();
                //create a SQL table in the custom schema
                string sql = CreateTABLE(tablename, dt);
                var dbutils = new DatabaseCommandUtils().buildAndExecDirectCommandNonQuery(sql, new Object[] { });

                DataTable table = new DataTable();
                table.TableName = "EX_EXCIMP_excel_import_tables";

                // Declare DataColumn and DataRow variables.
                DataColumn column;
                DataRow row;

                // Create new DataColumn, set DataType, ColumnName and add to DataTable.
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");
                column.ColumnName = "EX_EXCIMP_ID";
                table.Columns.Add(column);

                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");
                column.ColumnName = "EX_EXCIMP_TABLE";
                table.Columns.Add(column);

                row = table.NewRow();

                row["EX_EXCIMP_ID"] = key;
                row["EX_EXCIMP_TABLE"] = tablename;
                table.Rows.Add(row);

                SqlBulkCopy sbc = new SqlBulkCopy(DBConnectionManager.GetTargetConnection());
                sbc.DestinationTableName = "core.EX_EXCIMP_excel_import_tables";
                sbc.BulkCopyTimeout = 300;
                sbc.WriteToServer(table);
                sbc.Close();


                SqlBulkCopy sbc2 = new SqlBulkCopy(DBConnectionManager.GetTargetConnection());
                sbc2.DestinationTableName =tablename;
                sbc2.BulkCopyTimeout = 300;
                sbc2.WriteToServer(dt);
                sbc2.Close();


            }
     
        }
        private  DataTable ToDataTable(ExcelWorksheet ws, int skip,int take,int takecols,bool hasHeaderRow = true)
        {
            var tbl = new DataTable();

            if (take > ws.Dimension.Rows)
                take = ws.Dimension.Rows;
            if (takecols > ws.Dimension.Columns)
                takecols = ws.Dimension.Columns;
            int firstRowidx = 1;
            if (skip != 0)
                firstRowidx = skip;

            foreach (var firstRowCell in ws.Cells[firstRowidx, 1, firstRowidx, takecols]) tbl.Columns.Add(hasHeaderRow ? firstRowCell.Text : string.Format("EXCEL_C{0}", firstRowCell.Start.Column));
           

            var startRow = hasHeaderRow ? 2 + skip : 1+skip;
            for (var rowNum = startRow; rowNum <= take; rowNum++)
            {
                var wsRow = ws.Cells[rowNum, 1, rowNum, takecols];
                var row = tbl.NewRow();
                foreach (var cell in wsRow) row[cell.Start.Column - 1] = cell.Text;
                tbl.Rows.Add(row);
            }
            return tbl;
        }

        private static string CreateTABLE(string tableName, DataTable table)
        {
            string sqlsc;
            sqlsc = "CREATE TABLE " + tableName + "(";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sqlsc += "\n [" + table.Columns[i].ColumnName + "] ";
                string columnType = table.Columns[i].DataType.ToString();
                switch (columnType)
                {
                    case "System.Int32":
                        sqlsc += " int ";
                        break;
                    case "System.Int64":
                        sqlsc += " bigint ";
                        break;
                    case "System.Int16":
                        sqlsc += " smallint";
                        break;
                    case "System.Byte":
                        sqlsc += " tinyint";
                        break;
                    case "System.Decimal":
                        sqlsc += " decimal ";
                        break;
                    case "System.DateTime":
                        sqlsc += " datetime ";
                        break;
                    case "System.String":
                    default:
                        sqlsc += string.Format(" nvarchar({0}) ", table.Columns[i].MaxLength == -1 ? "max" : table.Columns[i].MaxLength.ToString());
                        break;
                }
                if (table.Columns[i].AutoIncrement)
                    sqlsc += " IDENTITY(" + table.Columns[i].AutoIncrementSeed.ToString() + "," + table.Columns[i].AutoIncrementStep.ToString() + ") ";
                if (!table.Columns[i].AllowDBNull)
                    sqlsc += " NOT NULL ";
                sqlsc += ",";
            }
            return sqlsc.Substring(0, sqlsc.Length - 1) + "\n)";
        }


    }
}