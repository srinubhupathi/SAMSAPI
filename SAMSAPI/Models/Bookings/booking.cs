using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAMSAPI.Models.Bookings
{

    public class CheckedInRoomDetails
    {
        public int BookingId { get; set; }
        public int BookingDetailId { get; set; }
        public int RoomId { get; set; }
        public string RoomNo { get; set; }
        public string Guest { get; set; }
        public DateTime CheckInDate { get; set; }
    }




}