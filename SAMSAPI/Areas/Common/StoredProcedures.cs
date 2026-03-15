using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAMSAPI.Areas.Common
{
    public class StoredProcedures
    {
        public const string USERS_LOGIN = "spUser_Login";
        public const string USERS_FORGOT_PASSWORD = "spUser_ForgotPassword";
        public const string USERS_RESET_PASSWORD = "spUser_ResetPassword";
        public const string USERS_CHANGE_PASSWORD = "spUser_ChangePassword";
        public const string USERS_LOGOUT = "spUser_Logout";
        public const string USERS_ADD = "spUsers_Create";
        public const string USERS_UPDATE = "spUsers_Update";
        public const string USERS_GET = "spUsersList_Get";
        public const string USERS_Delete = "spUser_Delete";
        public const string USERS_CHANGE_PROFILE_PIC = "spUser_UpdateProfilePic";

        //Customers
        public const string CUSTOMERS_GET = "spCustomersList_Get";
        public const string CUSTOMERS_ADD = "spCustomers_Create";
        public const string CUSTOMERS_UPDATE = "spCustomers_Save";
        public const string CUSTOMERS_DELETE = "spCustomers_Delete";
    }
}