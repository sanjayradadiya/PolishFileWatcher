using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;

namespace PolishFileWatcher
{
    class SqlDataHelper
    {
        string constr = "";
        public SqlDataHelper()
        {
            constr = ConfigurationManager.ConnectionStrings["polishdatacon"].ConnectionString;
        }

        public void InsertBulkData(string pTableName, Dictionary<string, string> pMappings, DataTable pDt)
        {
            using (SqlConnection connection =  new SqlConnection(constr))
            {
                SqlBulkCopy bulkCopy = new SqlBulkCopy
                (
                   connection,
                   SqlBulkCopyOptions.TableLock |
                   SqlBulkCopyOptions.FireTriggers |
                   SqlBulkCopyOptions.UseInternalTransaction,
                   null
                );

                foreach (var item in pMappings)
                {
                    bulkCopy.ColumnMappings.Add(item.Key, item.Value);
                }

                bulkCopy.DestinationTableName = pTableName;
                connection.Open();

                bulkCopy.WriteToServer(pDt);
                connection.Close();
            }
        }
    }
}
