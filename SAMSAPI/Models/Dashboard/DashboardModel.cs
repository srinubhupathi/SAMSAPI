using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAMSAPI.Models.Dashboard
{
    public class RoomSummary
    {
        public DateTime Date { get; set; }
        public List<FloorModel> Floors { get; set; }

        public RoomModel DaySummarhy { get; set; }
    }
    public class FloorModel
    {
        public string FloorNo { get; set; }
        public int Status { get; set; }
        public List<RoomModel> Rooms { get; set; }
    }
    public class RoomModel
    {
        public string RoomNo { get; set; }
        public int RoomStatus { get; set; }
        public string RoomType { get; set; }
    }

    public class RoomTypeSummary
    {
        public string TarridId { get; set; }
        public string RoomType { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
    }

    public class DaySummary
    {
        public int TodayBooking { get; set; }
        public int TotalRoomsBooked { get; set; }
        public int RoomsAvailable { get; set; }
        public int RoomsBlocked { get; set; }

    }

    public class RoomSummaryReport
    {
        public int RoomId { get; set; }
        public string RoomType { get; set; }
        public int TariffId { get; set; }
        public string TariffName { get; set; }
        public string RoomNo { get; set; }
        public int TotalDays { get; set; }
        public int DaysBooked { get; set; }
        public int DaysVacancy { get; set; }
        public int DaysBookedforDonor { get; set; }
        public int DaysBookedforGeneral { get; set; }
        public double RoomrRentTptal { get; set; }
        public double ExtraBedAmountTotal { get; set; }
        public double? TotalAmount { get; set; }
        public double? DonorBookingAmount { get; set; }
        public double? GeneralBookingAmount { get; set; }
        public RoomModel DaySummary { get; set; }
    }

    public class RoomCheckoutReport
    {
        public int RoomId { get; set; }
        public string RoomType { get; set; }
        public int TariffId { get; set; }
        public string TariffName { get; set; }
        public string RoomNo { get; set; }
        public double? RoomRent { get; set; }
        public double? ExtraBedAmount { get; set; }
        public int TotalDays { get; set; }
        public string Plan { get; set; }     
        public string Coupons { get; set; }
        public string MemberCode { get; set; }
        public string MemberName { get; set; }
        public double? TotalAmount { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string AdvanceReceipts { get; set; }
        public double? AdvanceAmount { get; set; }
        public int? BookingId { get; set; }
        public int? BookingNo { get; set; }

        public string CompanyName { get; set; }
        public string GSTNo { get; set; }
        public double? CGST { get; set; }
        public double? SGST { get; set; }

    }

    public class PaymentSummary
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double TotalAmount { get; set; }
        public double ByCash { get; set; }
        public double ByCard { get; set; }
        public double ByOnline { get; set; }
        public double ByDonorBookings { get; set; }
        public double ByGeneralBookings { get; set; }

        public List<PaymentDetail> Details { get; set; }

    }

    public class PaymentDetail{
        public string PaymentDate { get; set; }
        public double TotalAmount { get; set; }
        public double ByCash { get; set; }
        public double ByCard { get; set; }
        public double ByOnline { get; set; }
        public double ByDonorBookings { get; set; }
        public double ByGeneralBookings { get; set; }
    }

    public class PaymentHistory
    {

        public int RoomId { get; set; }
        public string RoomNo { get; set; }
        public int? TariffId { get; set; }
        public string TariffName { get; set; }
        public List<PaymentDetails> Payments { get; set; }
        public int BookingPaymentId { get; set; }
        public int? BookingId { get; set; }
        public int? BookingNo { get; set; }
        public string ReceiptNo { get; set; }
        public string PaymentType { get; set; }
        public string PaymentMode { get; set; }
        public string Details { get; set; }
        public string PaymentDate { get; set; }
        public double? Amount { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
    }

    public class PaymentDetails
    {


    }
}