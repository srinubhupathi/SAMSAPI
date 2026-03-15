using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace SAMSAPI.Areas.Interface
{
    public interface IDatabaseOperation
    {
        /// <summary>
        /// Executes the query
        /// </summary>
        /// <param name="mySqlConnection">MySql Connection on which the operation is to be performed.</param>
        /// <returns>true if execution is successful otherwise false</returns>
        Boolean Execute(SqlConnection sqlConnection);
    }
}