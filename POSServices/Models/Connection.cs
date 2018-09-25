using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace POSServices.Models
{
    public class Connection
    {
        public SqlConnection connection;

        public void Initialize()
        {            
            string connectionString;
            connectionString = "Server=CONTROL;Database=GRUPOPASEO78LQNVEN;User Id=servicios; Password=123;MultipleActiveResultSets=True";

            connection = new SqlConnection(connectionString);      
        }

        public bool OpenConnection()
        {
            Initialize();
            try
            {
                connection.Open();
                System.Diagnostics.Debug.WriteLine(connection.ToString());
                return true;
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message.ToString());
                return false;
            }
        }

        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message.ToString());
                return false;
            }
        }
    }
}