using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using SAMSAPI.Areas.Interface;
using SAMSAPI.Areas.Common;

namespace SAMSAPI.Models.User
{
    public class UsersDBHandler : IDatabaseOperation
    {
        public int id { get; set; }
        public int pageNumber { get; set; }
        public int totalPageCount { get; set; }
        public int pageItemNumber { get; set; }
        public int unitId { get; set; }
        public int departmentId { get; set; }
        public string searchKey { get; set; }
        public string passResetToken { get; set; }
        public int isEmailExist { get; set; }
        public int userLoginStatus { get; set; }
        public int designationId { get; set; }
        public string profileImage { get; set; }
        public string profileImageThumbnail { get; set; }
        public string roleId { get; set; }
        public List<UsersModel> userList { get; set; }
        public Users loginUser { get; set; }
        public Users cUser { get; set; }
        public string errorMessage { get; set; }
        public string profile { get; set; }
        public string strProfilePic { get; set; }
        public string strthumbnailPic { get; set; }
        public string strUserEmail { get; set; }
        public ConstantEnums.UsersAction Action { get; set; }
        public UsersDBHandler()
        {
            id = 0;
            pageNumber = 0;
            totalPageCount = 0;
            pageItemNumber = 0;
            unitId = 0;
            departmentId = 0;
            isEmailExist = 0;
            userLoginStatus = 0;
            designationId = 0;
            roleId = String.Empty;
            errorMessage = String.Empty;
            passResetToken = String.Empty;
            profileImage = String.Empty;
            profileImageThumbnail = String.Empty;
            userList = new List<UsersModel>();
            loginUser = new Users();
        }

        public bool Execute(SqlConnection sqlConnection)
        {
            bool status = false;
            if (sqlConnection.State != System.Data.ConnectionState.Open)
            {
                return false;
            }
            else
            {
                switch (Action)
                {
                    case ConstantEnums.UsersAction.USERS_LOGIN:
                        status = LoginUser(sqlConnection);
                        break;
                    case ConstantEnums.UsersAction.USERS_ADD:
                        status = AddUser(sqlConnection);
                        break;
                    case ConstantEnums.UsersAction.USERS_UPDATE:
                        status = UpdateUser(sqlConnection);
                        break;
                    case ConstantEnums.UsersAction.USERS_GET:
                        status = GetUsersList(sqlConnection);
                        break;
                    case ConstantEnums.UsersAction.USERS_DELETE:
                        status = DeleteUser(sqlConnection);
                        break;
                    case ConstantEnums.UsersAction.USERS_RESET_PASSWORD:
                        status = ResetPassword(sqlConnection);
                        break;
                    case ConstantEnums.UsersAction.USERS_FORGOT_PASSWORD:
                        status = ForgotPassword(sqlConnection);
                        break;
                    case ConstantEnums.UsersAction.USERS_CHANGE_PASSWORD:
                        status = ChangePassword(sqlConnection);
                        break;
                    case ConstantEnums.UsersAction.USERS_PROFILE_IMAGE_UPDATE:
                        status = ChangeProfilePicture(sqlConnection);
                        break;
                }
            }
            return status;
        }

        /// <summary>
        /// Login the user
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public bool LoginUser(SqlConnection sqlConnection)
        {
            try
            {
                totalPageCount = 0;
                SqlCommand command = new SqlCommand(StoredProcedures.USERS_LOGIN, sqlConnection);
                command.Parameters.Add(new SqlParameter("@Email", loginUser.Email));
                command.Parameters.Add(new SqlParameter("@Password", loginUser.Password));
                command.Parameters.Add(new SqlParameter("@AccessToken", loginUser.AccecssToken));
                command.Parameters.Add(new SqlParameter("@DeviceType", loginUser.DeviceType));
                command.Parameters.Add(new SqlParameter("@LoginTime", loginUser.LoginDateTime));
                command.Parameters.Add(new SqlParameter("@DeviceID", loginUser.DeviceId));
                command.Parameters.Add(new SqlParameter("@DeviceToken", loginUser.DeviceToken));

                command.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        UsersModel user;
                        CommonMethod cm = new CommonMethod();
                        while (rdr.Read())
                        {
                            userLoginStatus = Convert.ToInt32(Convert.ToString(rdr["LoginStatus"]));
                            if (userLoginStatus == ConstantVariables.VALID_USER_LOGIN_STATUS)
                            {
                                user = new UsersModel();
                                user.Id = Convert.ToInt32(Convert.ToString(rdr["PK_Users"]));
                                user.FirstName = Convert.ToString(rdr["DF_Users_FirstName"]);
                                user.LastName = Convert.ToString(rdr["DF_Users_LastName"]);
                                user.Email = Convert.ToString(rdr["DF_Users_Email"]);

                                user.MobileNumber = Convert.ToString(rdr["DF_Users_MobileNumber"]);
                                user.Location = Convert.ToString(rdr["DF_Users_Location"]);
                                user.PhoneNumber = Convert.ToString(rdr["DF_Users_Phone"]);
                                string filePath = Convert.ToString(rdr["DF_Users_ProfilePicture"]);
                                user.ProfilePicture = cm.ImageToBase64(filePath);
                                user.ProfilePictureThumbnail = Convert.ToString(rdr["DF_Users_ProfilePicThumb"]);
                                user.Status = Convert.ToInt32(Convert.ToString(rdr["DF_Users_Status"]));
                                user.Roles.Id = Convert.ToInt32(Convert.ToString(rdr["FK_Roles_Users"]));
                                user.Roles.Title = Convert.ToString(rdr["DF_Roles_Title"]);
                           
                                if (rdr["DF_CustomerUsers_Sites"].ToString() != "")
                                {
                                    user.Sites = Convert.ToString(rdr["DF_CustomerUsers_Sites"]);
                                    user.SiteNames = Convert.ToString(rdr["DF_CustomerUsers_Sites_Names"]);
                                }
                                user.Token = loginUser.AccecssToken;


                                userList.Add(user);
                            }

                        }
                    }
                    rdr.NextResult();
                    if (rdr.HasRows)
                    {


                    }

                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            return true;
        }

        public bool Logout(SqlConnection sqlConnection)
        {
            try
            {
                totalPageCount = 0;
                SqlCommand command = new SqlCommand(StoredProcedures.USERS_LOGOUT, sqlConnection);
                command.Parameters.Add(new SqlParameter("@AccessToken ", SingletoneClass.authorizationToken));
                command.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader rdr = command.ExecuteReader())
                {

                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Save the user details in database.
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public bool AddUser(SqlConnection sqlConnection)
        {
            try
            {
                totalPageCount = 0;
                SqlCommand command = new SqlCommand(StoredProcedures.USERS_ADD, sqlConnection);
                command.Parameters.Add(new SqlParameter("@FirstName", userList[0].FirstName));
                command.Parameters.Add(new SqlParameter("@LastName", userList[0].LastName));
                command.Parameters.Add(new SqlParameter("@Email", userList[0].Email));
                command.Parameters.Add(new SqlParameter("@MobileNumber", userList[0].MobileNumber));
                command.Parameters.Add(new SqlParameter("@PhoneNumber", userList[0].MobileNumber));
                command.Parameters.Add(new SqlParameter("@Location", userList[0].MobileNumber));
                command.Parameters.Add(new SqlParameter("@Password", userList[0].Password));
                command.Parameters.Add(new SqlParameter("@RoleID", userList[0].Roles.Id));

                command.Parameters.Add(new SqlParameter("@Status", userList[0].Status));
                command.Parameters.Add(new SqlParameter("@PassResetToken", passResetToken));
             

                command.CommandType = CommandType.StoredProcedure;


                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        UsersModel user;
                        CommonMethod cm = new CommonMethod();
                        while (rdr.Read())
                        {
                            isEmailExist = Convert.ToInt32(Convert.ToString(rdr["IsUserExist"]));
                            if (isEmailExist == 0)
                            {
                                user = new UsersModel();
                                user.Id = Convert.ToInt32(Convert.ToString(rdr["PK_Users"]));
                                user.FirstName = Convert.ToString(rdr["DF_Users_FirstName"]);
                                user.LastName = Convert.ToString(rdr["DF_Users_LastName"]);
                                user.Email = Convert.ToString(rdr["DF_Users_Email"]);

                                user.MobileNumber = Convert.ToString(rdr["DF_Users_MobileNumber"]);
                                user.Location = Convert.ToString(rdr["DF_Users_Location"]);
                                user.PhoneNumber = Convert.ToString(rdr["DF_Users_Phone"]);
                                user.ProfilePicture = Convert.ToString(rdr["DF_Users_ProfilePicture"]);

                                user.Roles.Id = Convert.ToInt32(Convert.ToString(rdr["FK_Roles_Users"]));
                                user.Roles.Title = Convert.ToString(rdr["DF_Roles_Title"]);


                                user.Status = cm.ConvertToInt(Convert.ToString(rdr["DF_Users_Status"]));
                               


                                //totalPageCount = cm.ConvertToInt(Convert.ToString(rdr["TotalCount"]));
                                userList[0] = user;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Change the password
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public bool ChangePassword(SqlConnection sqlConnection)
        {
            try
            {
                totalPageCount = 0;
                SqlCommand command = new SqlCommand(StoredProcedures.USERS_CHANGE_PASSWORD, sqlConnection);
                command.Parameters.Add(new SqlParameter("@OldPassword ", cUser.Password));
                command.Parameters.Add(new SqlParameter("@Password", cUser.NewPassword));
                command.Parameters.Add(new SqlParameter("@Email", cUser.Email));
                command.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        CommonMethod cm = new CommonMethod();
                        while (rdr.Read())
                        {
                            isEmailExist = Convert.ToInt32(Convert.ToString(rdr["PasswordChangeStatus"]));
                            if (isEmailExist == 1)
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            return true;
        }// eo ChangePassword()


        /// <summary>
        /// Reset the password
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public bool ResetPassword(SqlConnection sqlConnection)
        {
            try
            {
                totalPageCount = 0;
                SqlCommand command = new SqlCommand(StoredProcedures.USERS_RESET_PASSWORD, sqlConnection);

                command.Parameters.Add(new SqlParameter("@Password", cUser.NewPassword));
                command.Parameters.Add(new SqlParameter("@PassResetToken", passResetToken));
                command.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        CommonMethod cm = new CommonMethod();
                        while (rdr.Read())
                        {
                            isEmailExist = Convert.ToInt32(Convert.ToString(rdr["PasswordResetStatus"]));
                            if (isEmailExist == 1)
                            {
                                //UsersModel user = new UsersModel();
                                //user.FirstName = Convert.ToString(rdr["DF_Users_FirstName"]);
                                //user.LastName = Convert.ToString(rdr["DF_Users_LastName"]);
                                //user.Email = Convert.ToString(rdr["DF_Users_Email"]);
                                //userList.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            return true;
        }// eo ChangePassword()
        public bool ForgotPassword(SqlConnection sqlConnection)
        {
            try
            {
                totalPageCount = 0;
                SqlCommand command = new SqlCommand(StoredProcedures.USERS_FORGOT_PASSWORD, sqlConnection);
                command.Parameters.Add(new SqlParameter("@Email", cUser.Email));
                command.Parameters.Add(new SqlParameter("@PassResetToken", cUser.AccecssToken));
                command.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            isEmailExist = Convert.ToInt32(Convert.ToString(rdr["ForgotPasswordStatus"]));
                            if (isEmailExist == 2)
                            {
                                UsersModel contacts = new UsersModel();
                                contacts.FirstName = Convert.ToString(rdr["DF_Users_FirstName"]);
                                contacts.LastName = Convert.ToString(rdr["DF_Users_LastName"]);
                                contacts.Email = Convert.ToString(rdr["DF_Users_Email"]);
                                userList.Add(contacts);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public bool UpdateUser(SqlConnection sqlConnection)
        {
            try
            {
                totalPageCount = 0;
                SqlCommand command = new SqlCommand(StoredProcedures.USERS_UPDATE, sqlConnection);
                command.Parameters.Add(new SqlParameter("@FirstName", userList[0].FirstName));
                command.Parameters.Add(new SqlParameter("@LastName", userList[0].LastName));
                command.Parameters.Add(new SqlParameter("@UserID", userList[0].Id));
                command.Parameters.Add(new SqlParameter("@MobileNumber", userList[0].MobileNumber));
                command.Parameters.Add(new SqlParameter("@PhoneNumber", userList[0].PhoneNumber));
                command.Parameters.Add(new SqlParameter("@Location", userList[0].Location));
                command.Parameters.Add(new SqlParameter("@RoleID", userList[0].Roles.Id));
                command.Parameters.Add(new SqlParameter("@Status", userList[0].Status));
                command.Parameters.Add(new SqlParameter("@Sites", userList[0].Sites));
                // command.Parameters.Add(new SqlParameter("@UpdatedDateTime", userList[0].updatedDateTime));

                //command.Parameters.Add(new SqlParameter("@Status", contactsList[0].status));
                //command.Parameters.Add(new SqlParameter("@PassResetToken ", passResetToken));
                //command.Parameters.Add(new SqlParameter("@AccessToken", singletoneClass.authorizationToken));

                command.CommandType = CommandType.StoredProcedure;


                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        UsersModel user;
                        CommonMethod cm = new CommonMethod();
                        while (rdr.Read())
                        {
                            //isEmailExist =  Convert.ToInt32(Convert.ToString(rdr["IsUserExist"]));
                            //if (isEmailExist == 0)
                            //{
                            user = new UsersModel();
                            user.Id = Convert.ToInt32(Convert.ToString(rdr["PK_Users"]));
                            user.FirstName = Convert.ToString(rdr["DF_Users_FirstName"]);
                            user.LastName = Convert.ToString(rdr["DF_Users_LastName"]);
                            user.Email = Convert.ToString(rdr["DF_Users_Email"]);

                            user.MobileNumber = Convert.ToString(rdr["DF_Users_MobileNumber"]);
                            user.Location = Convert.ToString(rdr["DF_Users_Location"]);
                            user.PhoneNumber = Convert.ToString(rdr["DF_Users_Phone"]);
                            user.ProfilePicture = Convert.ToString(rdr["DF_Users_ProfilePicture"]);

                            user.Roles.Id = Convert.ToInt32(Convert.ToString(rdr["FK_Roles_Users"]));
                            user.Roles.Title = Convert.ToString(rdr["DF_Roles_Title"]);

                            user.Status = cm.ConvertToInt(Convert.ToString(rdr["DF_Users_Status"]));
                          
                            //totalPageCount = cm.ConvertToInt(Convert.ToString(rdr["TotalCount"]));
                            userList[0] = user;
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            return true;
        }//eo UpdateContact()

        /// <summary>
        /// Delete the user
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public bool DeleteUser(SqlConnection sqlConnection)
        {
            try
            {
                totalPageCount = 0;
                SqlCommand command = new SqlCommand(StoredProcedures.USERS_Delete, sqlConnection);
                command.Parameters.Add(new SqlParameter("@UserID ", id));
                command.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader rdr = command.ExecuteReader())
                {

                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            return true;
        }//eo DeleteContact()

        /// <summary>
        /// Fetch the data for contacts from database
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public bool GetUsersList(SqlConnection sqlConnection)
        {
            try
            {
                totalPageCount = 0;
                SqlCommand command = new SqlCommand(StoredProcedures.USERS_GET, sqlConnection);
                command.Parameters.Add(new SqlParameter("@SearchText ", searchKey));
                command.Parameters.Add(new SqlParameter("@RoleID", roleId));

                command.CommandType = CommandType.StoredProcedure;


                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    //if (rdr.HasRows)
                    //{
                    //    while (rdr.Read())
                    //    {
                    //        totalPageCount = Convert.ToInt32(Convert.ToString(rdr["TotalCount"]));
                    //    }
                    //}
                    //rdr.NextResult();
                    if (rdr.HasRows)
                    {
                        UsersModel user;
                        CommonMethod cm = new CommonMethod();
                        while (rdr.Read())
                        {
                            user = new UsersModel();
                            user.Id = Convert.ToInt32(Convert.ToString(rdr["PK_Users"]));
                            user.FirstName = Convert.ToString(rdr["DF_Users_FirstName"]);
                            user.LastName = Convert.ToString(rdr["DF_Users_LastName"]);
                            user.Email = Convert.ToString(rdr["DF_Users_Email"]);
                            user.MobileNumber = Convert.ToString(rdr["DF_Users_MobileNumber"]);
                            user.Location = Convert.ToString(rdr["DF_Users_Location"]);
                            user.PhoneNumber = Convert.ToString(rdr["DF_Users_Phone"]);
                            user.ProfilePicture = Convert.ToString(rdr["DF_Users_ProfilePicture"]);
                            user.ProfilePictureThumbnail = Convert.ToString(rdr["DF_Users_ProfilePicThumb"]);
                            user.Roles.Id = Convert.ToInt32(Convert.ToString(rdr["FK_Roles_Users"]));
                            user.Roles.Title = Convert.ToString(rdr["DF_Roles_Title"]);
                            user.Status = cm.ConvertToInt(Convert.ToString(rdr["DF_Users_Status"]));
                        
                            if (rdr["DF_CustomerUsers_Sites"].ToString() != "")
                            {
                                user.Sites = Convert.ToString(rdr["DF_CustomerUsers_Sites"]);
                                user.SiteNames = Convert.ToString(rdr["DF_CustomerUsers_Sites_Names"]);
                            }
                            user.Token = passResetToken;

                            userList.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            return true;
        }

        public bool ChangeProfilePicture(SqlConnection sqlConnection)
        {
            try
            {
                totalPageCount = 0;
                SqlCommand command = new SqlCommand(StoredProcedures.USERS_CHANGE_PROFILE_PIC, sqlConnection);
                command.Parameters.Add(new SqlParameter("@ProfilePicUrl ", strProfilePic));
                command.Parameters.Add(new SqlParameter("@ProfileThumbUrl", strthumbnailPic));
                command.Parameters.Add(new SqlParameter("@Email", strUserEmail));
                command.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        UsersModel user = new UsersModel();
                        CommonMethod cm = new CommonMethod();
                        while (rdr.Read())
                        {
                            user.Id = Convert.ToInt32(Convert.ToString(rdr["PK_Users"]));
                            user.ProfilePicture = Convert.ToString(rdr["DF_Users_ProfilePicture"]);
                            user.ProfilePictureThumbnail = Convert.ToString(rdr["DF_Users_ProfilePicThumb"]);
                            userList.Add(user);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            return true;
        }

    }
}