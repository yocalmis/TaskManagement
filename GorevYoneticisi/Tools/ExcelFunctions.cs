using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace GorevYoneticisi.Tools
{
    public class ExcelFunctions
    {
        public static DataTable ExcelToDataTable(string pathName, string sheetName)
        {
            try
            {
                DataTable tbContainer = new DataTable();
                string strConn = string.Empty;
                FileInfo file = new FileInfo(pathName);

                

                if (!file.Exists) { throw new Exception("Error, file doesn't exists!"); }
                string extension = file.Extension;
                switch (extension)
                {
                    case ".xls":
                        strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                        break;
                    case ".xlsx":
                        strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathName + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;'";
                        break;
                    default:
                        strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathName + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                        break;
                }

                string firstSheetName = "Sayfa1";
                using (OleDbConnection conn = new OleDbConnection(strConn))
                {
                    conn.Open();
                    DataTable dbSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);                    
                    dbSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                    if (dbSchema.Rows.Count >= 1)
                    {
                        firstSheetName = dbSchema.Rows[0].Field<string>("TABLE_NAME");
                    }                    
                }

                OleDbConnection cnnxls = new OleDbConnection(strConn);
                OleDbDataAdapter oda = new OleDbDataAdapter(string.Format("select * from [{0}]", firstSheetName), cnnxls);
                DataSet ds = new DataSet();
                oda.Fill(tbContainer);
                return tbContainer;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}