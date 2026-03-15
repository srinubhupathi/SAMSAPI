using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using SAMSAPI.Models.Dashboard;
using SAMSAPI.Manager;
namespace SAMSAPI.Controllers
{
     [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class DashboardController : ApiController
    {
         [HttpGet]
         [ActionName("GetRoomsSummary")]
         public RoomSummary GetRoom()
         {
            DateTime tdate = DateTime.Now;
            return new SAMSManager().GetRoomSummary(tdate); 
         }

    }
}
