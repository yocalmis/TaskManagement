using GorevYoneticisi.Models;
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
    public class ExcelTabloExport
    {
        public static string Export(DataTable table, HttpContext context, string fileName)
        {
            context.Response.Clear();

            string query;
            OleDbCommand cmd;
            OleDbConnection cnn;

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            File.Copy(HostingEnvironment.ApplicationPhysicalPath + "public\\upload\\dosyalar\\excel\\Temp.xlsx", HostingEnvironment.ApplicationPhysicalPath + "public\\upload\\dosyalar\\excel\\" + fileName);
            //string cnStr = GetConnectionString(fileName, Types.Excel_97_2000_2003_xls, true, true);
            string cnStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + HostingEnvironment.ApplicationPhysicalPath + "public\\upload\\dosyalar\\excel\\" + fileName + ";Mode=ReadWrite;Extended Properties='Excel 12.0;HDR=Yes;IMEX=0;'";
            
            cnn = new OleDbConnection(cnStr);
            try
            {
                cnn.Open();
                foreach (DataRow row in table.Rows)
                {
                    string values = "(";
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        if (i + 1 == table.Columns.Count)
                        {
                            if (table.Columns[i].DataType == System.Type.GetType("System.Int32"))
                                values += String.IsNullOrEmpty(row[i].ToString()) ? "0)" : row[i] + ")";
                            else
                                values += "'" + System.Security.SecurityElement.Escape(row[i].ToString()) + "')";
                        }
                        else
                        {
                            if (table.Columns[i].DataType == System.Type.GetType("System.Int32"))
                                values += String.IsNullOrEmpty(row[i].ToString()) ? "0," : row[i] + ",";
                            else
                                values += "'" + System.Security.SecurityElement.Escape(row[i].ToString()) + "',";
                        }
                    }
                    query = String.Format("Insert into [Sayfa1] VALUES {0}", values);
                    cmd = new OleDbCommand(query, cnn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                context.Response.Write(ex.Message);
                return fileName;
            }
            finally
            {
                cmd = null;
                if (cnn != null)
                    cnn.Close();
            }
            return fileName;
        
        }
    }
}