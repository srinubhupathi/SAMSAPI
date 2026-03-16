using System;
using System.Collections.Generic;

namespace SAMSAPI.Models.Inventory
{
    // ─── Stock Items ─────────────────────────────────────────────────────────────

    public class StockItemDto
    {
        public int    StockItemId       { get; set; }
        public string ItemName          { get; set; }
        public string Category          { get; set; }
        public string UnitOfMeasurement { get; set; }
        public double ReorderLevel      { get; set; }
        public string Description       { get; set; }
        public bool   IsActive          { get; set; }
        public double CurrentStock      { get; set; }   // computed from vw_StockAvailability
    }

    // ─── Room Type Stock Config ───────────────────────────────────────────────────

    public class RoomTypeStockConfigDto
    {
        public int    RoomTypeStockConfigId { get; set; }
        public int    TariffId              { get; set; }
        public string TariffName            { get; set; }
        public int    StockItemId           { get; set; }
        public string ItemName              { get; set; }
        public string Unit                  { get; set; }
        public double Quantity              { get; set; }
        public double DailyReplenishQty     { get; set; }
    }

    public class SaveRoomTypeStockConfigRequest
    {
        public int    RoomTypeStockConfigId { get; set; }
        public int    TariffId              { get; set; }
        public int    StockItemId           { get; set; }
        public double Quantity              { get; set; }
        public double DailyReplenishQty     { get; set; }
        public int    CreatedBy             { get; set; }
    }

    // ─── Purchases ───────────────────────────────────────────────────────────────

    public class PurchaseSummaryDto
    {
        public int      PurchaseId     { get; set; }
        public DateTime PurchaseDate   { get; set; }
        public string   InvoiceNumber  { get; set; }
        public string   VendorName     { get; set; }
        public double   TotalAmount    { get; set; }
        public string   Remarks        { get; set; }
        public int      ItemCount      { get; set; }
    }

    public class PurchaseDto
    {
        public int                   PurchaseId    { get; set; }
        public DateTime              PurchaseDate  { get; set; }
        public string                InvoiceNumber { get; set; }
        public string                VendorName    { get; set; }
        public double                TotalAmount   { get; set; }
        public string                Remarks       { get; set; }
        public List<PurchaseLineDto> Details       { get; set; }
    }

    public class PurchaseLineDto
    {
        public int    PurchaseDetailId { get; set; }
        public int    StockItemId      { get; set; }
        public string ItemName         { get; set; }
        public string Unit             { get; set; }
        public double Quantity         { get; set; }
        public double UnitPrice        { get; set; }
        public double TotalAmount      { get; set; }
    }

    // ─── Room Stock ──────────────────────────────────────────────────────────────

    public class RoomStockDto
    {
        public int      RoomStockId       { get; set; }
        public int      StockItemId       { get; set; }
        public string   ItemName          { get; set; }
        public string   Category          { get; set; }
        public string   Unit              { get; set; }
        public string   StockType         { get; set; }
        public double   IssuedQuantity    { get; set; }
        public double   ReturnedQuantity  { get; set; }
        public double   BalanceQuantity   { get; set; }
        public DateTime IssueDate         { get; set; }
    }

    public class AddRoomStockRequest
    {
        public int                BookingDetailId { get; set; }
        public string             Remarks         { get; set; }
        public int                CreatedBy       { get; set; }
        public List<StockLineItem> Items          { get; set; }
    }

    public class StockLineItem
    {
        public int    StockItemId { get; set; }
        public double Quantity    { get; set; }
    }

    public class ReturnRoomStockRequest
    {
        public int    RoomStockId     { get; set; }
        public int    BookingDetailId { get; set; }
        public int    StockItemId     { get; set; }
        public double ReturnQuantity  { get; set; }
        public string Reason          { get; set; }
        public int    CreatedBy       { get; set; }
    }

    public class AssignCheckinStockRequest
    {
        public int BookingDetailId { get; set; }
        public int CreatedBy       { get; set; }
    }

    // ─── Reports ─────────────────────────────────────────────────────────────────

    public class StockAvailabilityDto
    {
        public int    StockItemId    { get; set; }
        public string ItemName       { get; set; }
        public string Category       { get; set; }
        public string Unit           { get; set; }
        public double ReorderLevel   { get; set; }
        public double TotalPurchased { get; set; }
        public double TotalIssued    { get; set; }
        public double TotalReturned  { get; set; }
        public double AvailableStock { get; set; }
    }

    public class LowStockDto : StockAvailabilityDto
    {
        public int?      LastPurchaseId   { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
    }

    public class StockMovementDto
    {
        public DateTime TransactionDate  { get; set; }
        public string   ItemName         { get; set; }
        public string   Category         { get; set; }
        public string   TransactionType  { get; set; }
        public string   RoomNo           { get; set; }
        public double   InQty            { get; set; }
        public double   OutQty           { get; set; }
        public double   Balance          { get; set; }
        public string   Reference        { get; set; }
    }

    public class RoomStockSummaryDto
    {
        public string    RoomNo        { get; set; }
        public string    GuestName     { get; set; }
        public DateTime? CheckInDate   { get; set; }
        public DateTime? CheckOutDate  { get; set; }
        public double    TotalIssued   { get; set; }
        public double    TotalReturned { get; set; }
        public double    NetConsumed   { get; set; }
    }
}
