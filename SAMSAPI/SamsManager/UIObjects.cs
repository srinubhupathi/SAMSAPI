using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAMSData;
namespace SAMSAPI.Manager
{



    public class User: SAMSUser
    {
        public string token { get; set; }
    }

    public class MembershipSummary
    {
       public int TotalMembers { get; set; }
        public int DonorsCount { get; set; }
        public int LifeCount { get; set; }
        public int SrCitizenCount { get; set; }
        public int CorporateCount { get; set; }
        public int HonorCount { get; set; }
        
    }

    public class FeeStaus
    { 
        public string Year { get; set; }
        public string Status { get; set; }
    }

    public class MembeFeeStatus
    {
        public Member Member { get; set; }        
        public int DueFromYear { get; set; }
        public int DueToYear { get; set; }
        public int CurrentYear { get; set; }
        public float CurrentYearDue { get; set; }
        public string FeeDueStatus { get; set; }
        public float DueAmount { get; set; }
        public int FeeType { get; set; }
        public int Age { get; set; }
        public bool IsSeniorCitizenEligible { get; set; }
        public string SeniorCitizenStatus { get; set; }
        public int SeniorCitizenDueCutoffYear { get; set; }
        public List<FeeStaus> FeeStatusList { get; set; }
    }

   
    public class MembeFeeDueSummary
    {
        public Member Member { get; set; }
        public int DueFromYear { get; set; }
        public int DueToYear { get; set; }
        public string FeeDueStatus { get; set; }
        public float DueAmount { get; set; }        
        public int SMSAlertStatus { get; set; }
        public DateTime SMSAlertSent { get; set; }
    }

    public class MembershipFeeDueSummary
    {
        public int TotalMembers { get; set; }
        public int DonorsCount { get; set; }
        public int LifeCount { get; set; }
        public int SrCitizenCount { get; set; }
        public int CorporateCount { get; set; }
    }
    public class MemberDasboard
    {
        public MembershipSummary MembershipSummary { get; set; }
        public MembershipSummary ExpiredMembersSummary { get; set; }
        public MembershipFeeDueSummary MemberFeeDueSummary { get; set; }
    }

    public class RoomDonorsTransactions
    {
        public Member Donor { get; set; }
        public int TotalCoupons { get; set; }
        public int StartCoupon { get; set; }
        public int EndCoupon { get; set; }
        public int UsedCoupons { get; set; }
        public string Details { get; set; }

    }


    public class MemberFeeView: MembershipFeeTransaction
    {
        public int StartYear { get; set; }
        public int EndYear { get; set; }
    }

    public class MemberFeeDueDetailsDto
    {
        public DateTime? PaidFromDate { get; set; }

        public DateTime? PaidToDate { get; set; }

        public DateTime? DueFromDate { get; set; }

        public DateTime? DueToDate { get; set; }

        public int FeeTypeId { get; set; }

        public Member Member{ get; set; }
    }

    public class RoomView : Room
    {
        public string RoomStatusName { get; set; }
       public String TariffName { get; set; }
       public String TariffType { get; set; }
    }

    public class PurchseDetailsView:PurchaseDetail 
    {
        public int Sno { get; set; }        
        public string ProductName { get; set; }
        public string GroupName { get; set; }
        public double? Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string PackName { get; set; }
        public string CompanyName { get; set; }
    }

    public class PurchaseView : Purchase
    {
        public string Vendor;
        public string GroupName;
        public List<PurchseDetailsView> PurchaseDetailsView;
    }

    public class GroupDetail:LedgerGroup
    {
        public string GroupDescription { get; set; }
    }




   

 public class SaleDetailsView : SaleDetail 
    {
        public int Sno { get; set; }
        public string PackName { get; set; }
        public double Amount { get; set; }
        public string BarCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public string CompanyName { get; set; }
        public int billNo { get; set; }
    }

 public class SaleDetailsTaxView
 {
     public int Sno { get; set; }
     //public string ProductName { get; set; }
     public double Amount { get; set; }
     public string BarCode { get; set; }
     public DateTime TransactionDate { get; set; }
     public int StartBillNo { get; set; }
     public int EndBillNo { get; set; }
     public double Tax0Amount { get; set; }
     public double Tax5Amount { get; set; }
     public double Tax14Amount { get; set; }
     public double Discount { get; set; }
 }




 public class StockSaleDetails
 {
     public int Sno { get; set; }
     public int ProductID { get; set; }
     public string ProductName { get; set; }
     public string CompanyName { get; set; }
     public string Model { get; set; }
     public string Size { get; set; }
     public double? Quantity { get; set; }     
     public double? SalePrice { get; set; }
     public double? Amount { get; set; }
     public double? Discount { get; set; }
     public double? Netamount { get; set; }
     public double? MRP { get; set; }
 }

    class ProdcutGroupView 
    {
        public int ProductGroupID { get; set; }
        public string GroupName { get; set; }
        
    }

    class BarCodeItem
    {        
        public string CodeName { get; set; }
        public string CodeText { get; set; }
        public double  PrintCount { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
    }

    class ProductStockDetails
    {
        public int Sno { get; set; }
        public int ProductID { get; set; }
        public string ProdcutName { get; set; }
        public string Model { get; set; }
        public string Size { get; set; }
        public string MfgDate { get; set; }
        public string ExpDate { get; set; }
        public double? OB { get; set; }
        public double? Purchase { get; set; }
        public double? Sale { get; set; }
        public double? Balance { get; set; }
        public double? MRP { get; set; }
        public double? Production { get; set; }
        public double? Return { get; set; }
        public double? Tin { get; set; }
        public double? Tout { get; set; }
        public double? PReturn { get; set; }
        public double? PurchasePrice { get; set; }
        public double? StockValue { get; set; }
        public string RefType { get; set; }
        public string CompanyName { get; set; }
        public double? UnitType { get; set; }
        public string Cateogry { get; set; }
        public int? CompanyId { get; set; }
        public int? MeasurementId { get; set; }
        public double? BalQtyValue { get; set; }
        public double? TaxValue { get; set; }
    }
    public class LedgerView : Ledger
    {
        public bool? MarkSelect { get; set; }
        public string GroupName { get; set; }
        public string OBTypeName { get; set; }
    }


    public class ProductView : Product
    {
      
    }



    public class LedgerJournalWithInterest : LedgerJournal
    {
        public double NoDays { get; set; }
        public double DebitInterest { get; set; }
        public double CreditInterest { get; set; }
    }

    public class LedgerJournalView : LedgerJournal
    {
        public int Sno { get; set; }
       
        public Ledger Ledger { get; set; }
    }

    public class Daybook: LedgerJournal
    {
        public string LedgerName { get; set; }
        public string Place { get; set; }
        public double DrAmount1 { get; set; }
        public double CrAmount1{ get; set; }
        public int Type { get; set; }
    }

    public class LedgerOutstanding
    {
        public int LedgerId { get; set; }
        public string LedgerName { get; set; }
        public string Place { get; set; }
        public double OB { get; set; }
        public double Sale { get; set; }
        public double SaleReturn { get; set; }
        public double CreditNotes { get; set; }
        public double DebitNotes { get; set; }
        public double Payments { get; set; }
        public double Receipts { get; set; }
        public double Balance { get; set; }
        public double Purchase { get; set; }
        public double PurchaseReturn { get; set; }
    }

   
    class CustomerStockDetails
    {
        public int LedgerId { get; set; }
        public string CustomerName { get; set; }
        public string Place { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime TransactionDate { get; set; }
      //  public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string BatchNumber { get; set; }
        public string Size { get; set; }
        public string MfgDate { get; set; }
        public string ExpDate { get; set; }
        public double? Quantity { get; set; }
        public double? NoCases { get; set; }
        public double? TotalValue { get; set; }
        public string Details { get; set; }
    }

    
    class CustomerStockDetails1
    {
        public string CustomerName { get; set; }
        public string Place { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime TransactionDate { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string BatchNumber { get; set; }
        public string Size { get; set; }
        public string MfgDate { get; set; }
        public string ExpDate { get; set; }
        public double? Quantity { get; set; }
        public double? NoCases { get; set; }
        public double? TotalValue { get; set; }
    }

    class ProductHistory
    {
        public string CustomerName { get; set; }
        public string Place { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime TransactionDate { get; set; }
        public int ProductID { get; set; }
        public int CompanyID { get; set; }
        public string ProductName { get; set; }
        public string CompanyName { get; set; }
        public string Size { get; set; }
        public double? OBQuantity { get; set; }
        public double? InQuantity { get; set; }
        public double? OutQuantity { get; set; }
        public double? Balance { get; set; }
        public string RefType { get; set; }
        public double? Price { get; set; }        
        public int MeasurementId { get; set; }
        public double? measurementValue { get; set; }
        public double? measurementMasterValue { get; set; }
        public double? InQuanityValue { get; set; }
        public double? OutQuantityValue { get; set; }
        public int LedgerId { get; set; }
        public double? OutQtySaleValue { get; set; }
        public double? InQtySaleValue { get; set; }
    }

    

    class SaleGroupDetails
    {
        public string CustomerName { get; set; }
        public string Place { get; set; }
        public int ProductID { get; set; }
        public int CompanyID { get; set; }
        public string ProductName { get; set; }
        public string CompanyName { get; set; }
        public string Size { get; set; }
        public double? SaleQuantity { get; set; }
        public double? ReturnQuantity { get; set; }
        public int MeasurementId { get; set; }
        public double? measurementValue { get; set; }
        public double? SaleQuanityinLts { get; set; }
        public double? ReturnQuanityinLts { get; set; }
        public double? BalanceQunatityinLts { get; set; }
        public double? Price { get; set; }
        public int LedgerId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    class VATDetails
    {
        public string TaxType { get; set; }
        public float Tax { get; set; }
        public double VATExcludedAmount { get; set; }
        public double VATAmount { get; set; }
    }


}

