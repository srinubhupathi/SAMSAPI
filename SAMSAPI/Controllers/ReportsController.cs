using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SAMSAPI.Models.Dashboard;
using SAMSAPI.Manager;
using System.Web.Http.Cors;
namespace SAMSAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReportsController : ApiController
    {
        [HttpGet]
        [ActionName("RoomSummaryData")]
        public List<RoomSummaryReport> GetRoomSummaryData(DateTime fDate, DateTime tDate)
        {
            DateTime tdate = DateTime.Now;
            return new SAMSManager().GetRoomSummaryReportData(fDate, tDate);
        }

        [HttpGet]
        [ActionName("RoomCheckoutData")]
        public List<RoomCheckoutReport> GetRoomCheckoutData(DateTime fDate, DateTime tDate)
        {
            DateTime tdate = DateTime.Now;
            return new SAMSManager().GetRoomCheckoutData(fDate, tDate);
        }

        [HttpGet]
        [ActionName("PaymentHistoryData")]
        public List<PaymentHistory> GetPaymentHistoryData(DateTime fDate, DateTime tDate)
        {
            return new SAMSManager().GetPaymentHistoryData(fDate, tDate);
        }

        [HttpGet]
        [ActionName("PaymentSummary")]
        public PaymentSummary GetPaymentSummaryData(DateTime fDate, DateTime tDate)
        {
            return new SAMSManager().GetPaymentDetails(fDate, tDate);
        }
    }
}
