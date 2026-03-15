using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SAMSAPI.Manager;
using SAMSData;
using System.Web.Http.Cors;
namespace SAMSAPI.Controllers
{
     [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class RoomsController : ApiController
    {
        // GET: api/Rooms
        //public IEnumerable<Room> Get()
        //{
        //    var list = new SAMSManager().GetRoomsList();
        //    return list;
        //}
         [HttpGet]
         [ActionName("GetRoom")]      
         public Room GetRoom(int id)
         {
             Room room = new SAMSManager().GetRoom(id);
             return room;
         }

        [HttpGet]
        [ActionName("RoomsList")]
        public IEnumerable<Room> Get(string status)
        {
            var list = new SAMSManager().GetRoomsList(status);
            return list;
        }
        [HttpGet]
        [ActionName("RoomsTariffs")]
        public IEnumerable<Tariff> GetRoomsTariffs()
        {
            var list = new SAMSManager().GetRoomTariffs();
            return list;
        }
        [HttpGet]
        [ActionName("AvailableRoomsList")]
        public IEnumerable<Room> GetAvailableRoomsList(DateTime dtCheckIn)
        {

            var list = new SAMSManager().GetAvailabelRoomsforBooking(dtCheckIn);
            return list;
        }
        [HttpPost]
        [ActionName("SaveRoom")]
        public Room SaveRoom(Room room)
        {
            var list = new SAMSManager().SaveRoom(room);
            return list;
        }


        [HttpGet]
        [ActionName("DonorsTransactions")]
        public List<RoomDonorsTransactions> GetDonotTransactions(string fromDate, string toDate)
        {
            var list = new SAMSManager().GetDonorsData(Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate));
            return list;
        }

    }
}
