using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAMSAPI.Areas.Common
{
    public class ConstantVariables
    {
        public const int VALID_USER_LOGIN_STATUS = 2;

        //Define the language 
        public const string DEFAULT_LANGUAGE_ID = "1";
        public const string DEFAULT_LANGUAGE = "en";
        // Added for maximun time for sleep process in the process list of my sql to clear Pool 
        public const int MAX_CONNCTION_TIME = 300;
        public const int MAX_CONNCTION_COUNT = 150;

        //Define the path 
        public const string PATH_IMAGE = "Uploads/Images";
        public const string PATH_THUMBNAIL = "Uploads/Thumbnails";
        public const string PATH_AUDIOS = "Uploads/Audios";
        public const string PATH_VIDEOS = "Uploads/Videos";
        public const string PATH_VIDEO_LIBRARY = "Libraries";
        public const string PATH_TEMP = "Uploads/Temp";
        public const string PATH_OM_REPORT = "Uploads/Report/Om";

        //Profile Image
        public const string PATH_PROFILE_IMAGE = "Uploads/Profile/Images";

        //Inspection Image
        public const string PATH_INSPECTION_IMAGE = "Uploads/Inspection";

        //Define the timestamp of video whre from create the thumnbail
        public const int VIDEO_THUMBNAIL_TIMESTAMP = 0;

        //Definethe height and width of the thumbnail
        public const int THUMBNAIL_HEIGHT = 256;
        public const int THUMBNAIL_WIDTH = 256;


        public const string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
        public const string DATE_FORMAT = "yyyy-MM-dd";
        public const string DATE_MIL_FORMAT = "MM/dd/yyyy";
        public const string DATE_TIME_MIL_FORMAT = "MM/dd/yyyy HHmm";

        public const int GENERATE_PASSWORD_LENGTH = 10;

        public const string VALID_IMAGE_FILE_EXT = "jpg,jpeg,png";
        public const string VALID_VIDEO_FILE_EXT = "mov";
        public const string VALID_AUDIO_FILE_EXT = "mp3";






    }
}