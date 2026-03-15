using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAMSAPI.Areas.Common
{
    public class ConstantEnums
    {
        public enum UserStatus
        {
            DirectoryUsers = 0,//Not an app user
            UserInvited = 1, //User invited for app but email not verified
            UserActive = 2, //User verified his email and is active
            UserInactive = 3, // Inactive user
            UserDeleted = 4, // user deleted
            UserContractor = 5 // Contrator User
        }
        /// <summary>
        /// Define User Action
        /// </summary>
        public enum UsersAction
        {
            USERS_GET,
            USERS_ADD,
            USERS_UPDATE,
            USERS_LOGIN,
            USERS_LOGOUT,
            USERS_FORGOT_PASSWORD,
            USERS_RESET_PASSWORD,
            USERS_CHANGE_PASSWORD,
            USERS_DELETE,
            USERS_INVITE,
            USERS_CHANGE_STATUS,
            USERS_PROFILE_IMAGE_UPDATE,
            USERS_UPDATE_DEVICE_TOKEN
        }
        public enum ContactStatus
        {
            UserInvited = 1, //User invited for app but email not verified
            UserActive = 2, //User verified his email and is active
            UserInactive = 3, // Inactive user
            UserDeleted = 4, // user deleted
        }
        public enum DeviceType
        {
            IOS = 1,
            Web = 2
        }



        /// <summary>
        /// Define Setup Action
        /// </summary>
        public enum SetupAction
        {
            CUSTOMERS_GET,
            CUSTOMERS_ADD,
            CUSTOMERS_UPDATE,
            CUSTOMERS_DELETE,
            SITES_GET,
            SITES_ADD,
            SITES_UPDATE,
            SITES_DELETE,
            MACHINES_GET,
            MACHINES_ADD,
            MACHINES_UPDATE,
            MACHINES_DELETE,
            MACHINESITES_GET,
            COUNTRIES_GET,
            REGIONS_GET
        }



        /// <summary>
        /// Define EquipmentSetup Contact Action
        /// </summary>
        public enum EquipmentSetupAction
        {
            EQUIPMENTTYPES_GET,
            EQUIPMENTTOOLS_GET,
            PRODUCTTYPES_GET,
            TOOLMEASURMENTS_GET,
            TOOLTYPES_GET

        }

        /// <summary>
        /// Define Equipments Contact Action
        /// </summary>
        public enum EquipmentAction
        {
            EQUIPMENT_GET,
            EQUIPMENT_SAVE,
            EQUIPMENT_DELETE,
            EQUIPMENT_CONFIGURATIONS,
            EQUIPMENTMACHINES_GET
        }
        /// <summary>
        /// Define Inspections Status
        /// </summary>
        public enum InspectionsStatus
        {
            NotStarted = 0,//Inspection not started           
            InProgress = 1, //Inspection in progress
            Completed = 2, // Inspection completed
            InspectionDeleted = 3, //Inspection deleted            
        }
        /// <summary>
        /// Define Inspections Action
        /// </summary>
        public enum InspectionsAction
        {
            INSPECTIONS_GET,
            INSPECTIONS_SAVE,
            INSPECTIONDETAILS_UPDATE,
            INSPECTIONS_DELETE,
            INSPECTIONSDETAILS_GET,
            INSPECTIONSAMPLES_GET,
            INSPECTIONSAMPLES_SAVE,
            INSPECTIONSAMPLES_DELETE,
            INSPECTIONPARTMEASUREMENT_SAVE,
            PARTMEASUREMENTS_GET
        }
        public enum InspectionStatus
        {
            NotStarted = 0, //Inspection not started
            InProgress = 1, //Inspection started
            Completed = 2, // Inspection completed
        }
        /// <summary>
        /// Define Master Data Action
        /// </summary>
        public enum MasterDataAction
        {
            EquipmentMasterData_GET = 1,
            RegiontMasterData_GET = 2
        }
        /// <summary>
        /// Define Master Data Action
        /// </summary>
        public enum SyncDataAction
        {
            SetupData_Sync = 0,
            InspectionsData_Get = 1,
            InspectionsData_Sync = 2,
            Partmeasurements_Get = 3,
        }
        /// <summary>
        /// Define Report Data Action
        /// </summary>
        public enum ReportDataAction
        {
            Report_EquipmentSummary = 0,
            Report_EquipmentSearch = 1,
            Report_InspectionDetails = 2
        }
    }

}