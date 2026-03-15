using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading;
using SAMSAPI.Areas.Database;
using SAMSAPI.Areas.Common;
using SAMSAPI.Models.User;
using System.Web.Http.Cors;
using SAMSAPI.Manager;
using SAMSData;

namespace SAMSAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserController : ApiController
    {
        private DatabaseHandler _mainDatabaseHandler;
        private string UserLanguage = string.Empty;
        private string LanguageID = String.Empty;
        private JwtAuthMethods jwtAuthMothods;
        private HttpResponseMessage response;
        private CommonResponse commonResponse;
        private CommonMethod commonMethod;
        private SendEmail sendEmail;
        public UserController()
        {
            //commonResponse = new CommonResponse();
            //_mainDatabaseHandler = new DatabaseHandler();
            //commonMethod = new CommonMethod();
            //sendEmail = new SendEmail();
            //jwtAuthMothods = new JwtAuthMethods();

            //LanguageID = ConstantVariables.DEFAULT_LANGUAGE_ID;
            //UserLanguage = ConstantVariables.DEFAULT_LANGUAGE;

            //Thread.CurrentThread.CurrentCulture = new CultureInfo(UserLanguage);
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo(UserLanguage);
        }


        [HttpPost]
        [ActionName("UserLogin")]
        public User Login(User user)
        {
            var userDetails = new SAMSManager().ValidateUser(user);

            if (userDetails != null)
            {
                userDetails.token = ""; //Generate token
            }

            return userDetails;
        }
        [HttpGet]
        [ActionName("RoomsList")]
        public IEnumerable<Room> Get(string status)
        {

            var list = new SAMSManager().GetRoomsList(status);
            return list;
        }
        //[HttpPost]
        //[ActionName("SaveRoom")]
        //public Room SaveRoom(Room room)
        //{
        //    var list = new SAMSManager().SaveRoom(room);
        //    return list;
        //}
        //public HttpResponseMessage Login(Users user)
        //{
        //    try
        //    {
        //        bool isEmailEmpty = String.IsNullOrEmpty(user.Email);
        //        bool isPasswordEmpty = String.IsNullOrEmpty(user.Password);
        //        if (isEmailEmpty || isPasswordEmpty)
        //        {
        //            string title = Resources.AppMessages.ERROR_TITLE;
        //            string message = Resources.AppMessages.REQUIRED_PARAMETER_MISSING;
        //            object errorResponse = commonResponse.getErrorResponse(ResponseCodes.REQUIRED_PARAMETER_MISSING, title, message);
        //            response = Request.CreateResponse(HttpStatusCode.OK, errorResponse);
        //            return response;
        //        }
        //        else
        //        {
        //          //  string token = jwtAuthMothods.createToken(user.Email);
        //            UsersDBHandler resultSet = new UsersDBHandler();
        //            resultSet.Action = ConstantEnums.UsersAction.USERS_LOGIN;
        //            user.Password = commonMethod.GetHashedPassword(user.Password);
        //            user.AccecssToken = "";//token;
        //            //user.loginDateTime = DateTime.UtcNow.loginToString(ConstantVariables.DATE_TIME_FORMAT);
        //            resultSet.loginUser = user;



        //            if (_mainDatabaseHandler.ExecuteOperation(resultSet))
        //            {
        //                string title;
        //                string message;
        //                ResponseCodes responseCode;
        //                Object responseObj;
        //                switch (resultSet.userLoginStatus)
        //                {
        //                    case (int)ConstantEnums.UserStatus.UserActive:
        //                        title = Resources.AppMessages.SUCCESS_TITLE;
        //                        message = Resources.AppMessages.NOTIFICATION_MARK_AS_READ_SUCCESS;
        //                        responseCode = ResponseCodes.USER_LOGIN_SUCCESS;
        //                        responseObj = commonResponse.getDataResponse(resultSet.userList, 0);
        //                        response = Request.CreateResponse(HttpStatusCode.OK, responseObj);
        //                        break;                        
        //                    case (int)ConstantEnums.UserStatus.UserInvited:
        //                        title = Resources.AppMessages.ERROR_TITLE;
        //                        message = Resources.AppMessages.USER_LOGIN_EMAIL_NOT_VERIFIED;
        //                        responseCode = ResponseCodes.USER_LOGIN_INACTIVE_USER;
        //                        responseObj = commonResponse.getErrorResponse(responseCode, title, message);
        //                        response = Request.CreateResponse(HttpStatusCode.OK, responseObj);
        //                        break;
        //                    case (int)ConstantEnums.UserStatus.UserInactive:
        //                        title = Resources.AppMessages.ERROR_TITLE;
        //                        message = Resources.AppMessages.USER_LOGIN_INACTIVE_USER;
        //                        responseCode = ResponseCodes.USER_LOGIN_INACTIVE_USER;
        //                        responseObj = commonResponse.getErrorResponse(responseCode, title, message);
        //                        response = Request.CreateResponse(HttpStatusCode.OK, responseObj);
        //                        break;
        //                    case 101:
        //                        title = Resources.AppMessages.ERROR_TITLE;
        //                        message = Resources.AppMessages.USER_LOGIN_EMAIL_NOT_MATCHED;
        //                        responseCode = ResponseCodes.USER_LOGIN_EMAIL_NOT_MATCHED;
        //                        responseObj = commonResponse.getErrorResponse(responseCode, title, message);
        //                        response = Request.CreateResponse(HttpStatusCode.OK, responseObj);
        //                        break;
        //                    case 102:
        //                        title = Resources.AppMessages.ERROR_TITLE;
        //                        message = Resources.AppMessages.USER_LOGIN_PASSWORD_NOT_MATCHED;
        //                        responseCode = ResponseCodes.USER_LOGIN_PASSWORD_NOT_MATCHED;
        //                        responseObj = commonResponse.getErrorResponse(responseCode, title, message);
        //                        response = Request.CreateResponse(HttpStatusCode.OK, responseObj);
        //                        break;
        //                    case 103:
        //                        title = Resources.AppMessages.ERROR_TITLE;
        //                        message = Resources.AppMessages.USER_LOGIN_MAX_ATTEMPT_RITCHED;
        //                        responseCode = ResponseCodes.USER_LOGIN_MAX_ATTEMPT_RITCHED;
        //                        responseObj = commonResponse.getErrorResponse(responseCode, title, message);
        //                        response = Request.CreateResponse(HttpStatusCode.OK, responseObj);
        //                        break;
        //                }
        //            }
        //            else
        //            {
        //                string title = Resources.AppMessages.ERROR_TITLE;
        //                string message = resultSet.errorMessage;
        //                object errorResponse = commonResponse.getErrorResponse(ResponseCodes.DB_EXCEPTION, title, message);
        //                response = Request.CreateResponse(HttpStatusCode.OK, errorResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string title = Resources.AppMessages.ERROR_TITLE;
        //        string message = ex.Message;
        //        object errorResponse = commonResponse.getErrorResponse(ResponseCodes.DB_EXCEPTION, title, message);
        //        response = Request.CreateResponse(HttpStatusCode.OK, errorResponse);
        //    }
        //    finally
        //    {
        //        _mainDatabaseHandler.CloseConnection();
        //    }
        //    return response;
        //}//eo Login()
    }
}
