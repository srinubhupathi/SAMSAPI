using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SAMSAPI.Areas.Interface;
using SAMSAPI.Areas.Common;
namespace SAMSAPI.Areas.Database
{
    public class DatabaseHandler
    {
        private String _databaseConnectionString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;
        private SqlConnection _databaseConnection;

        private static DatabaseHandler _instance = null;
        /// <summary>
        /// Gets a single instance of Database Handler. If instance is already created, same instance will be returned.
        /// </summary>
        public static DatabaseHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatabaseHandler();
                }



                return _instance;
            }
        }

        /// <summary>
        /// Initalizes and open the database connection
        /// TO-DO: Handle server and connections states 
        /// </summary>
        public DatabaseHandler()
        {

            _databaseConnection = new SqlConnection(_databaseConnectionString);
            if (_databaseConnection.State == ConnectionState.Open)
            {
                CloseConnection();
            }

            SqlConnection.ClearPool(_databaseConnection);


            GC.Collect();
            _databaseConnection.Open();
        }
        public void CloseConnection()
        {
            if (_databaseConnection.State == ConnectionState.Open && _databaseConnection != null)
            {
                _databaseConnection.Close();

                //_databaseConnection = null;
                SqlConnection.ClearPool(_databaseConnection);

                GC.Collect();
                //ClearDatabaseConnctionPool();
            }


        }
        public void ClearDatabaseConnctionPool()
        {

            string queryString = "select coalesce(getProcessCount('" + ConstantVariables.MAX_CONNCTION_TIME + "'),'0')";
            SqlCommand comHash = new SqlCommand(queryString, GetConnection());
            int returnFlag = comHash.ExecuteNonQuery();
            int currentPoolcount = 0;
            SqlDataReader databaseReader = comHash.ExecuteReader();

            while (databaseReader.Read())
            {
                currentPoolcount = Convert.ToInt16(databaseReader.GetString(0));
            }

            /*       CloseConnection();
                   if (_databaseConnection.State == ConnectionState.Open && _databaseConnection != null)
                   {
                       _databaseConnection.Dispose();
                   }*/
            if (currentPoolcount >= ConstantVariables.MAX_CONNCTION_COUNT)
            {
                SqlConnection.ClearPool(_databaseConnection);
            }
        }


        /// <summary>
        /// Returns the current cunnection state of the SqlConnection.
        /// </summary>
        /// <returns>String representing the sql connection state</returns>
        public ConnectionState GetConnectionState()
        {
            return _databaseConnection.State;
        }


        public SqlConnection GetConnection()
        {

            if (isConnectionOpen() == false)
            {
                _databaseConnection.Open();
            }
            return _databaseConnection;
        }

        /// <summary>
        /// Executes the database operation of the classes who have implemented IDatabaseOperation
        /// </summary>
        /// <param name="operation">Class implementing IDatabaseOperation</param>
        /// <returns>True of success otherwise false</returns>
        public Boolean ExecuteOperation(IDatabaseOperation operation)
        {
            if (_databaseConnection.State == ConnectionState.Closed)
            {
                _databaseConnection.Open();
            }
            return operation.Execute(_databaseConnection);
        }

        public bool isConnectionOpen()
        {
            if (_databaseConnection.State == ConnectionState.Open)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}