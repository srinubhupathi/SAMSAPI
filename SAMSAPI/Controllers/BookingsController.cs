using SAMSAPI.Manager;
using SAMSAPI.Models.Bookings;
using SAMSData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SAMSAPI.Controllers
{

    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class BookingsController : ApiController
    {
        // GET: api/Bookings
        public IEnumerable<Booking> Get()
        {
            var list = new SAMSManager().GetBookingsList();
            return list;
        }


        // GET: api/Bookings/5
        [HttpGet]
        [ActionName("Booking")]
        public Booking Get(int id)
        {
            return new SAMSManager().GetBooking(id);
        }


        // GET: api/Bookings?status=pending
        [HttpGet]
        [ActionName("GetActiveCheckins")]
        public List<Booking> GetActiveCheckIns(string status = "")
        {
            return new SAMSManager().GetBookingsList("CheckIn");
        }

        // GET: api/Bookings?status=pending
        [HttpGet]
        [ActionName("GetCheckedInRooms")]
        public List<CheckedInRoomDetails> GetCheckedInRoomsDetails()
        {
            return new SAMSManager().GetCheckedInRoomsDetails();
        }


        [HttpGet]
        [ActionName("GetBookingList")]
        public List<Booking> GetBookingList(DateTime fromDate, DateTime toDate,string status = "")
        {
            return new SAMSManager().GetBookingsList(fromDate,toDate, status);
        }


        // GET: api/Bookings/5
        [HttpGet]
        [ActionName("CheckoutDetails")]
        public Booking BookingWithCheckout(int id, string coupons="")
        {
            return new SAMSManager().GetBookingWithCheckoutDetails(id);
        }

        [HttpGet]
        [ActionName("RoomServices")]
        public List<RoomService> GetRoomServices(int bookingId)
        {
            return new SAMSManager().GetRoomServices(bookingId);
        }

        [HttpGet]
        [ActionName("RoomReceipts")]
        public List<BookingPayment> GetRoomReceipts(int bookingId)
        {
            return new SAMSManager().GetRoomPayments(bookingId);
        }
        [HttpGet]
        [ActionName("GetBookingDetails")]
        public List<BookingDetail> GetBookingDetails(int bookingId)
        {
            return new SAMSManager().GetBookingDetails(bookingId);
        }
        // POST: api/Bookings
        [HttpPost]
        [ActionName("SaveBooking")]
        public void SaveBooking([FromBody] Booking booking)
        {
            new SAMSManager().SaveBooking(booking);
        }

        // POST: api/Bookings
        [HttpPost]
        [ActionName("RoomCheckOut")]
        public void RoomCheckOut([FromBody] BookingDetail bookingDetail)
        {
            new SAMSManager().RoomCheckOut(bookingDetail);
        }
        // POST: api/Bookings
        [HttpPost]
        [ActionName("DeleteBooking")]
        public void DeleteBooking([FromBody] Booking booking)
        {
            new SAMSManager().DeleteBooking(booking);
        }


        [HttpPost]
        [ActionName("SaveReceipt")]
        // POST: api/Bookings/service
        public BookingPayment SaveReceipt([FromBody] BookingPayment receipt)
        {
            return new SAMSManager().SaveReceipt(receipt);
        }


        [HttpPost]
        [ActionName("DeleteReceipt")]
        // POST: api/Bookings/service
        public string DeleteReceipt([FromBody] BookingPayment receipt)
        {
            return new SAMSManager().DeleteReceipt(receipt);
        }

        [HttpPost]
        [ActionName("SaveService")]
        // POST: api/Bookings/service
        public RoomService SaveService([FromBody] RoomService service)
        {
            return new SAMSManager().SaveService(service);
        }

        [HttpPost]
        [ActionName("DeleteService")]
        // POST: api/Bookings/service
        public string DeleteService([FromBody] RoomService service)
        {
            return new SAMSManager().DeleteService(service);
        }

        // PUT: api/Bookings/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Bookings/5
        public void Delete(int id)
        {
        }
    }
}
