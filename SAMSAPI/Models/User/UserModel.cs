using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SAMSAPI.Areas.Common;
namespace SAMSAPI.Models.User
{
    public class UsersModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //public string location { get; set; }
        public string ProfilePicture { get; set; }
        public string ProfilePictureThumbnail { get; set; }
        public string CellNumber { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string MobileNumber { get; set; }
        public string Token { get; set; }
        public int Status { get; set; }
        public string CreatedDateTime { get; set; }
        public string UpdatedDateTime { get; set; }
        public Role Roles { get; set; }
        public string Sites { get; set; }
        public string SiteNames { get; set; }


        public UsersModel()
        {
            Id = 0;
            Email = String.Empty;
            FirstName = String.Empty;
            LastName = String.Empty;
            //location = String.Empty;
            ProfilePicture = String.Empty;
            ProfilePictureThumbnail = String.Empty;
            CellNumber = String.Empty;
            MobileNumber = String.Empty;
            Token = String.Empty;
            Status = 0;
            CreatedDateTime = String.Empty;
            UpdatedDateTime = String.Empty;
            Roles = new Role();
            Sites = "";
            SiteNames = "";
        }
    }
    public class Users
    {
        public string username { get; set; }
        public string password { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string AccecssToken { get; set; }
        public string DeviceId { get; set; }
        public string DeviceToken { get; set; }
        public string LoginDateTime { get; set; }
        public ConstantEnums.DeviceType DeviceType { get; set; }


        public Users()
        {
            Email = String.Empty;
            Password = String.Empty;
            NewPassword = String.Empty;
            AccecssToken = String.Empty;
            DeviceId = String.Empty;
            DeviceToken = String.Empty;
            LoginDateTime = String.Empty;

        }
    }
}