using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAMSAPI.Manager;
using SAMSAPI.Models.Dashboard;
using SAMSAPI.Models.Membership;
using SAMSData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;

namespace SAMSAPI.Controllers
{
     [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MembersController : ApiController
    {
        // GET: api/Rooms
        //public IEnumerable<Room> Get()
        //{
        //    var list = new SAMSManager().GetRoomsList();
        //    return list;
        //}

        [HttpPost]
        [ActionName("ValidateMember")]
        public Member ValidateMember(string userId, string password)
        {
            var _member = new SAMSManager().VaidateUser(userId,password);
            return _member;
        }

        [HttpPost]
        [ActionName("UpdatePassword")]
        public Member UpdatePassword(string userId, string password)
        {
            var _member = new SAMSManager().VaidateUser(userId, password);
            return _member;
        }

        [HttpGet]
         [ActionName("GetMember")]      
         public Member GetMember(int id)
         {
             Member member = new SAMSManager().GetMember(id);
             return member;
         }

        [HttpGet]
        [ActionName("GetMemberDues")]
        public MembeFeeStatus GetMemberDues(int id)
        {
            SAMSManager sm = new SAMSManager();
            Member member = sm.GetMember(id);
            var membershipStatus = sm.GetMemberFeeDetails(member);
            return membershipStatus;
        }

        [HttpGet]
        [ActionName("MembersList")]
        public IEnumerable<Member> Get(string status)
        {
            var list = new SAMSManager().GetMembersList(status);
            return list;
        }
        [HttpGet]
        [ActionName("MembershipTypes")]
        public IEnumerable<MembershipType> GetMembershipTypes()
        {
            var list = new SAMSManager().GetMembershipTypes();
            return list;
        }

        [HttpGet]
        [ActionName("FeeTypes")]
        public IEnumerable<FeeType> GetFeeTypes()
        {
            var list = new SAMSManager().GetFeeTypes();
            return list;
        }

        [HttpPost]
        [ActionName("SaveMember")]
        public Member SaveRoom(Member member)
        {
            var _member = new SAMSManager().SaveMember(member);
            return _member;
        }

        [HttpPost]
        [ActionName("SavePassword")]
        public int SaveRoom([FromBody] int meberId, [FromBody] string password)
        {
            var _member = new SAMSManager().SavePassword(meberId, password);
            return _member;
        }

        [HttpPost]
        [ActionName("SaveMemberFee")]
        public MembershipFeeTransaction SaveMemberFee(MembershipFeeTransaction memberFee)
        {            
            var _member = new SAMSManager().SaveMemberFee(memberFee);
            return _member;            
        }

        [HttpPost]
        [ActionName("DeleteMemberFee")]
        // POST: api/Bookings/service
        public string DeleteFee([FromBody] MembershipFeeTransaction item)
        {
            return new SAMSManager().DeleteMemberFee(item);
        }

        [HttpGet]
        [ActionName("MemberFeeList")]
        public IEnumerable<MembershipFeeTransaction> GetMemberFees()
        {
            var list = new SAMSManager().GetMemberFees();
            return list;
        }

        [HttpGet]
        [ActionName("GetMemberFees")]
        public IEnumerable<MembershipFeeTransaction> GetMemberFees1(DateTime fromDate, DateTime toDate)
        {
            var list = new SAMSManager().GetMemberFees(fromDate, toDate); 
            return list;
        }
        [HttpGet]
        [ActionName("MemberFees")]
        public IEnumerable<MembershipFeeTransaction> GetMemberFees(int memberId)
        {
            var list = new SAMSManager().GetMemberFees(memberId);
            return list;
        }

        

        [HttpGet]
        [ActionName("RoomsAvailability")]
        public List<RoomTypeSummary> GetRoomsAvailability()
        {
            var list = new SAMSManager().GetRoomsAvailability();
            return list;
        }


        [HttpGet]
        [ActionName("DashboardSummary")]
        public MemberDasboard GetDashboardSummary()
        {
            var list = new SAMSManager().GetDashboardSummary();
            return list;
        }



        [HttpGet]
        [ActionName("MemberFeeDues")]
        public List<MembeFeeStatus> GetMembersFeeStatus()
        {
            var list = new SAMSManager().GetMembersFeeStatus();
            return list;
        }

        [HttpGet]
        [ActionName("MemberFeeDuesSummary")]
        public List<MembeFeeDueSummary> GetMemberFeeDuesSummary()
        {
            var list = new SAMSManager().GetMembersFeeSummary();
            return list;
        }

        [HttpGet]
        [ActionName("SportsFeeMembers")]
        public List<MembeFeeStatus> GetSportsFeeMembers(int feeTypeId = 3)
        {
            var list = new SAMSManager().GetSportsMembersFeeStatus(feeTypeId);
            return list;
        }


        [HttpGet]
        [ActionName("Alerts")]
        public List<SMSAlert> GetMemberAlerts(string alertType, string alertStatus)
        {
            var list = new SAMSManager().GetMemberSMSAlerts(alertType, alertStatus);
            return list;
        }


        [HttpPost]
        [ActionName("SendAlert")]
        public int SendAlert([FromBody] SMSAlert item)
        {
            return new SAMSManager().SendSMSAlert(item);
        }

        [HttpPost]
        [ActionName("DeleteAlert")]
        public int DeleteAlert([FromBody] SMSAlert item)
        {
            return new SAMSManager().DeleteSMSAlert(item);
        }


        [HttpPost]
        [ActionName("PhotoUpload")]
        public int UploadPhoto([FromBody] FileUpload obj)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    //var filePath = HttpContext.Current.Server.MapPath("~/UploadFile/" + postedFile.FileName);
                    //postedFile.SaveAs(filePath);
                }
            }
            return 1;
        }

        [HttpPost]
        [ActionName("GenerateAlerts")]
        public int GenerateAlerts()
        {
             new SAMSManager().GenerateFeeAlerts();
            return 1;
        }


        [HttpGet]
        [ActionName("AppData")]
        public JObject GetAppData()
        {
            var data = ReadJSONData("https://cosmoclub.blob.core.windows.net/jsons/happdata.json");
            return data;
        }
        public JObject ReadJSONData(string jsonFilename)
        {
            try
            {
                JObject jObject;
                // Read JSON directly from a file    
                using (StreamReader file = System.IO.File.OpenText(jsonFilename))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    jObject = (JObject)JToken.ReadFrom(reader);
                }
                return jObject;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occurred : " + ex.Message);
                return null;
            }
        }
    }
}
