using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using SAMSData;
using SAMSAPI.Models.Inventory;

namespace SAMSAPI.Manager
{
    /// <summary>
    /// Handles all database operations for the Inventory module.
    /// Uses the existing SAMSEntities EF ObjectContext (EF 4 / ObjectContext pattern).
    ///
    /// Simple CRUD operations use ObjectSet&lt;T&gt; (StockItems, InventoryPurchases, etc.).
    /// Complex cross-table queries (views / stored procedures) use
    /// ObjectContext.ExecuteStoreQuery&lt;T&gt;() which maps a raw SQL result set to a POCO.
    /// </summary>
    public class InventoryManager
    {
        // ═════════════════════════════════════════════════════════════════════════
        // 1. STOCK ITEMS
        // ═════════════════════════════════════════════════════════════════════════

        public List<StockItemDto> GetStockItems()
        {
            // vw_StockAvailability already filters IsActive = 1 and computes AvailableStock
            using (var se = new SAMSEntities())
            {
                return se.ExecuteStoreQuery<StockItemDto>(
                    @"SELECT StockItemId, ItemName, Category,
                             Unit          AS UnitOfMeasurement,
                             ReorderLevel,
                             CAST(1 AS BIT) AS IsActive,
                             AvailableStock AS CurrentStock,
                             NULL           AS Description
                      FROM   vw_StockAvailability
                      ORDER  BY Category, ItemName").ToList();
            }
        }

        public StockItemDto GetStockItem(int id)
        {
            using (var se = new SAMSEntities())
            {
                var item = se.StockItems.FirstOrDefault(s => s.StockItemId == id);
                if (item == null) return null;

                // Get current computed stock from view
                double currentStock = se.ExecuteStoreQuery<double>(
                    "SELECT ISNULL(AvailableStock, 0) FROM vw_StockAvailability WHERE StockItemId = @p0",
                    id).FirstOrDefault();

                return new StockItemDto
                {
                    StockItemId       = item.StockItemId,
                    ItemName          = item.ItemName,
                    Category          = item.Category,
                    UnitOfMeasurement = item.UnitOfMeasurement,
                    ReorderLevel      = item.ReorderLevel ?? 0,
                    Description       = item.Description,
                    IsActive          = item.IsActive,
                    CurrentStock      = currentStock
                };
            }
        }

        public StockItemDto SaveStockItem(StockItemDto dto, int userId)
        {
            using (var se = new SAMSEntities())
            {
                if (dto.StockItemId == 0)
                {
                    var item = new StockItem
                    {
                        ItemName          = dto.ItemName,
                        Category          = dto.Category,
                        UnitOfMeasurement = dto.UnitOfMeasurement,
                        ReorderLevel      = dto.ReorderLevel,
                        Description       = dto.Description,
                        IsActive          = dto.IsActive,
                        CreatedBy         = userId,
                        CreatedOn         = DateTime.Now
                    };
                    se.StockItems.AddObject(item);
                    se.SaveChanges();
                    dto.StockItemId = item.StockItemId;
                }
                else
                {
                    var item = se.StockItems.FirstOrDefault(s => s.StockItemId == dto.StockItemId);
                    if (item == null) return null;

                    item.ItemName          = dto.ItemName;
                    item.Category          = dto.Category;
                    item.UnitOfMeasurement = dto.UnitOfMeasurement;
                    item.ReorderLevel      = dto.ReorderLevel;
                    item.Description       = dto.Description;
                    item.IsActive          = dto.IsActive;
                    item.UpdatedBy         = userId;
                    item.UpdatedOn         = DateTime.Now;
                    se.SaveChanges();
                }
            }
            return GetStockItem(dto.StockItemId);
        }

        public string DeleteStockItem(int id)
        {
            using (var se = new SAMSEntities())
            {
                bool hasTransactions = se.RoomStockTransactions.Any(r => r.StockItemId == id);
                bool hasPurchases    = se.InventoryPurchaseDetails.Any(p => p.StockItemId == id);

                var item = se.StockItems.FirstOrDefault(s => s.StockItemId == id);
                if (item == null) return "Item not found.";

                if (hasTransactions || hasPurchases)
                {
                    // Soft delete — item is in use
                    item.IsActive  = false;
                    item.UpdatedOn = DateTime.Now;
                    se.SaveChanges();
                    return "Item deactivated (soft-deleted) as it has existing transactions.";
                }
                else
                {
                    se.StockItems.DeleteObject(item);
                    se.SaveChanges();
                    return "Item deleted successfully.";
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 2. ROOM TYPE STOCK CONFIGS
        // ═════════════════════════════════════════════════════════════════════════

        public List<RoomTypeStockConfigDto> GetRoomTypeStockConfigs()
        {
            using (var se = new SAMSEntities())
            {
                // Join with Tariffs and StockItems to return names
                return (from rtsc in se.RoomTypeStockConfigs
                        join t  in se.Tariffs    on rtsc.TariffId    equals t.TariffId
                        join si in se.StockItems  on rtsc.StockItemId equals si.StockItemId
                        orderby t.TariffName, si.ItemName
                        select new RoomTypeStockConfigDto
                        {
                            RoomTypeStockConfigId = rtsc.RoomTypeStockConfigId,
                            TariffId              = rtsc.TariffId,
                            TariffName            = t.TariffName,
                            StockItemId           = rtsc.StockItemId,
                            ItemName              = si.ItemName,
                            Unit                  = si.UnitOfMeasurement,
                            Quantity              = rtsc.Quantity,
                            DailyReplenishQty     = rtsc.DailyReplenishQty ?? 0
                        }).ToList();
            }
        }

        public RoomTypeStockConfigDto SaveRoomTypeStockConfig(SaveRoomTypeStockConfigRequest req)
        {
            using (var se = new SAMSEntities())
            {
                if (req.RoomTypeStockConfigId == 0)
                {
                    var config = new RoomTypeStockConfig
                    {
                        TariffId          = req.TariffId,
                        StockItemId       = req.StockItemId,
                        Quantity          = req.Quantity,
                        DailyReplenishQty = req.DailyReplenishQty,
                        CreatedBy         = req.CreatedBy,
                        CreatedOn         = DateTime.Now
                    };
                    se.RoomTypeStockConfigs.AddObject(config);
                    se.SaveChanges();
                    req.RoomTypeStockConfigId = config.RoomTypeStockConfigId;
                }
                else
                {
                    var config = se.RoomTypeStockConfigs
                        .FirstOrDefault(c => c.RoomTypeStockConfigId == req.RoomTypeStockConfigId);
                    if (config == null) return null;

                    config.TariffId          = req.TariffId;
                    config.StockItemId       = req.StockItemId;
                    config.Quantity          = req.Quantity;
                    config.DailyReplenishQty = req.DailyReplenishQty;
                    se.SaveChanges();
                }
            }

            return GetRoomTypeStockConfigs()
                .FirstOrDefault(c => c.RoomTypeStockConfigId == req.RoomTypeStockConfigId);
        }

        public void DeleteRoomTypeStockConfig(int id)
        {
            using (var se = new SAMSEntities())
            {
                var config = se.RoomTypeStockConfigs
                    .FirstOrDefault(c => c.RoomTypeStockConfigId == id);
                if (config != null)
                {
                    se.RoomTypeStockConfigs.DeleteObject(config);
                    se.SaveChanges();
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 3. PURCHASES
        // ═════════════════════════════════════════════════════════════════════════

        public List<PurchaseSummaryDto> GetPurchases(DateTime fromDate, DateTime toDate)
        {
            using (var se = new SAMSEntities())
            {
                DateTime from = fromDate.Date;
                DateTime to   = toDate.Date;

                return (from ip in se.InventoryPurchases
                        where ip.PurchaseDate >= from && ip.PurchaseDate <= to
                        orderby ip.PurchaseDate descending
                        let itemCount = se.InventoryPurchaseDetails
                            .Count(d => d.PurchaseId == ip.PurchaseId)
                        select new PurchaseSummaryDto
                        {
                            PurchaseId    = ip.PurchaseId,
                            PurchaseDate  = ip.PurchaseDate,
                            InvoiceNumber = ip.InvoiceNumber,
                            VendorName    = ip.VendorName,
                            TotalAmount   = ip.TotalAmount ?? 0,
                            Remarks       = ip.Remarks,
                            ItemCount     = itemCount
                        }).ToList();
            }
        }

        public PurchaseDto GetPurchase(int id)
        {
            using (var se = new SAMSEntities())
            {
                var ip = se.InventoryPurchases.FirstOrDefault(p => p.PurchaseId == id);
                if (ip == null) return null;

                var lines = (from d  in se.InventoryPurchaseDetails
                             join si in se.StockItems on d.StockItemId equals si.StockItemId
                             where d.PurchaseId == id
                             select new PurchaseLineDto
                             {
                                 PurchaseDetailId = d.PurchaseDetailId,
                                 StockItemId      = d.StockItemId,
                                 ItemName         = si.ItemName,
                                 Unit             = si.UnitOfMeasurement,
                                 Quantity         = d.Quantity,
                                 UnitPrice        = d.UnitPrice  ?? 0,
                                 TotalAmount      = d.TotalAmount ?? 0
                             }).ToList();

                return new PurchaseDto
                {
                    PurchaseId    = ip.PurchaseId,
                    PurchaseDate  = ip.PurchaseDate,
                    InvoiceNumber = ip.InvoiceNumber,
                    VendorName    = ip.VendorName,
                    TotalAmount   = ip.TotalAmount ?? 0,
                    Remarks       = ip.Remarks,
                    Details       = lines
                };
            }
        }

        public PurchaseDto SavePurchase(PurchaseDto dto, int userId)
        {
            using (var se = new SAMSEntities())
            {
                int purchaseId;

                if (dto.PurchaseId == 0)
                {
                    var ip = new InventoryPurchase
                    {
                        PurchaseDate  = dto.PurchaseDate.Date,
                        InvoiceNumber = dto.InvoiceNumber,
                        VendorName    = dto.VendorName,
                        TotalAmount   = dto.TotalAmount,
                        Remarks       = dto.Remarks,
                        CreatedBy     = userId,
                        CreatedOn     = DateTime.Now
                    };
                    se.InventoryPurchases.AddObject(ip);
                    se.SaveChanges();
                    purchaseId = ip.PurchaseId;
                }
                else
                {
                    purchaseId = dto.PurchaseId;
                    var ip = se.InventoryPurchases.FirstOrDefault(p => p.PurchaseId == purchaseId);
                    if (ip == null) return null;

                    ip.PurchaseDate  = dto.PurchaseDate.Date;
                    ip.InvoiceNumber = dto.InvoiceNumber;
                    ip.VendorName    = dto.VendorName;
                    ip.TotalAmount   = dto.TotalAmount;
                    ip.Remarks       = dto.Remarks;
                    ip.UpdatedBy     = userId;
                    ip.UpdatedOn     = DateTime.Now;

                    // Remove and re-insert lines
                    var existingLines = se.InventoryPurchaseDetails
                        .Where(d => d.PurchaseId == purchaseId).ToList();
                    foreach (var line in existingLines)
                        se.InventoryPurchaseDetails.DeleteObject(line);

                    se.SaveChanges();
                }

                // Insert lines
                if (dto.Details != null)
                {
                    foreach (var line in dto.Details)
                    {
                        se.InventoryPurchaseDetails.AddObject(new InventoryPurchaseDetail
                        {
                            PurchaseId  = purchaseId,
                            StockItemId = line.StockItemId,
                            Quantity    = line.Quantity,
                            UnitPrice   = line.UnitPrice,
                            TotalAmount = line.TotalAmount
                        });
                    }
                    se.SaveChanges();
                }

                dto.PurchaseId = purchaseId;
            }
            return GetPurchase(dto.PurchaseId);
        }

        public string DeletePurchase(int id)
        {
            using (var se = new SAMSEntities())
            {
                // Check: if any stock item from this purchase has been issued to rooms
                var purchasedItems = se.InventoryPurchaseDetails
                    .Where(d => d.PurchaseId == id)
                    .Select(d => d.StockItemId)
                    .Distinct().ToList();

                bool anyIssued = se.RoomStockTransactions
                    .Any(r => purchasedItems.Contains(r.StockItemId));

                if (anyIssued)
                    return "Cannot delete: stock from this purchase has already been issued to rooms.";

                var lines = se.InventoryPurchaseDetails
                    .Where(d => d.PurchaseId == id).ToList();
                foreach (var line in lines)
                    se.InventoryPurchaseDetails.DeleteObject(line);

                var ip = se.InventoryPurchases.FirstOrDefault(p => p.PurchaseId == id);
                if (ip != null)
                    se.InventoryPurchases.DeleteObject(ip);

                se.SaveChanges();
                return "Purchase deleted successfully.";
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 4. ROOM STOCK MANAGEMENT
        // ═════════════════════════════════════════════════════════════════════════

        public List<RoomStockDto> GetRoomStock(int bookingDetailId)
        {
            // Delegate to SP which calculates ReturnedQuantity and BalanceQuantity
            using (var se = new SAMSEntities())
            {
                return se.ExecuteStoreQuery<RoomStockDto>(
                    "EXEC usp_GetRoomStockBalance @BookingDetailId = {0}",
                    bookingDetailId).ToList();
            }
        }

        public string AddRoomStock(AddRoomStockRequest req)
        {
            using (var se = new SAMSEntities())
            {
                // Resolve BookingId and RoomId from BookingDetailId
                var bd = se.BookingDetails.FirstOrDefault(b => b.BookingDetailId == req.BookingDetailId);
                if (bd == null) return "Error: BookingDetail not found.";

                int bookingId = (int)(bd.BookingId ?? 0);
                int roomId    = (int)(bd.RoomId    ?? 0);

                // Validate available stock per item using the view
                foreach (var item in req.Items)
                {
                    double available = se.ExecuteStoreQuery<double>(
                        "SELECT ISNULL(AvailableStock, 0) FROM vw_StockAvailability WHERE StockItemId = {0}",
                        item.StockItemId).FirstOrDefault();

                    if (item.Quantity > available)
                        return string.Format(
                            "Insufficient stock for StockItemId {0}. Available: {1}, Requested: {2}",
                            item.StockItemId, available, item.Quantity);
                }

                // Insert transactions
                foreach (var item in req.Items)
                {
                    se.RoomStockTransactions.AddObject(new RoomStockTransaction
                    {
                        BookingDetailId  = req.BookingDetailId,
                        BookingId        = bookingId,
                        RoomId           = roomId,
                        StockItemId      = item.StockItemId,
                        StockType        = "Additional",
                        IssuedQuantity   = item.Quantity,
                        ReturnedQuantity = 0,
                        Remarks          = req.Remarks,
                        CreatedBy        = req.CreatedBy,
                        CreatedOn        = DateTime.Now,
                        IssueDate        = DateTime.Now
                    });
                }
                se.SaveChanges();
                return "Stock added successfully.";
            }
        }

        public string ReturnRoomStock(ReturnRoomStockRequest req)
        {
            using (var se = new SAMSEntities())
            {
                var rst = se.RoomStockTransactions
                    .FirstOrDefault(r => r.RoomStockId == req.RoomStockId);
                if (rst == null) return "Error: RoomStockTransaction not found.";

                // Calculate current balance
                double returned = se.RoomStockReturns
                    .Where(r => r.RoomStockId == req.RoomStockId)
                    .Sum(r => (double?)r.ReturnQuantity) ?? 0;
                double balance = rst.IssuedQuantity - returned;

                if (req.ReturnQuantity > balance)
                    return string.Format(
                        "Error: Return quantity ({0}) exceeds balance ({1}).",
                        req.ReturnQuantity, balance);

                // Insert return record
                se.RoomStockReturns.AddObject(new RoomStockReturn
                {
                    RoomStockId     = req.RoomStockId,
                    BookingDetailId = req.BookingDetailId,
                    StockItemId     = req.StockItemId,
                    ReturnQuantity  = req.ReturnQuantity,
                    Reason          = req.Reason,
                    CreatedBy       = req.CreatedBy,
                    CreatedOn       = DateTime.Now,
                    ReturnDate      = DateTime.Now
                });

                // Update running total on parent transaction
                rst.ReturnedQuantity = rst.ReturnedQuantity + req.ReturnQuantity;

                se.SaveChanges();
                return "Stock returned successfully.";
            }
        }

        public string AssignCheckinStock(int bookingDetailId, int createdBy)
        {
            // Use the stored procedure which handles all the join/insert logic atomically
            // SP returns: SELECT @@ROWCOUNT AS ItemsAssigned
            using (var se = new SAMSEntities())
            {
                var result = se.ExecuteStoreQuery<AssignCheckinStockResult>(
                    "EXEC usp_AssignCheckinStock @BookingDetailId = {0}, @CreatedBy = {1}",
                    bookingDetailId, createdBy).FirstOrDefault();

                int assigned = result != null ? result.ItemsAssigned : 0;
                return string.Format("{0} stock item(s) assigned for check-in.", assigned);
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 5. REPORTS
        // ═════════════════════════════════════════════════════════════════════════

        public List<StockAvailabilityDto> GetAvailableStockReport()
        {
            using (var se = new SAMSEntities())
            {
                return se.ExecuteStoreQuery<StockAvailabilityDto>(
                    "SELECT StockItemId, ItemName, Category, Unit, ReorderLevel, " +
                    "TotalPurchased, TotalIssued, TotalReturned, AvailableStock " +
                    "FROM vw_StockAvailability ORDER BY Category, ItemName").ToList();
            }
        }

        public List<LowStockDto> GetLowStockReport()
        {
            using (var se = new SAMSEntities())
            {
                return se.ExecuteStoreQuery<LowStockDto>(
                    "SELECT StockItemId, ItemName, Category, Unit, ReorderLevel, " +
                    "TotalPurchased, TotalIssued, TotalReturned, AvailableStock, " +
                    "LastPurchaseId, LastPurchaseDate " +
                    "FROM vw_LowStockItems ORDER BY AvailableStock ASC").ToList();
            }
        }

        public List<StockMovementDto> GetStockMovementReport(DateTime fromDate, DateTime toDate)
        {
            using (var se = new SAMSEntities())
            {
                return se.ExecuteStoreQuery<StockMovementDto>(
                    "EXEC usp_GetStockMovementReport @FromDate = {0}, @ToDate = {1}",
                    fromDate.Date, toDate.Date).ToList();
            }
        }

        public List<RoomStockSummaryDto> GetRoomStockSummaryReport(DateTime fromDate, DateTime toDate)
        {
            using (var se = new SAMSEntities())
            {
                return se.ExecuteStoreQuery<RoomStockSummaryDto>(
                    "EXEC usp_GetRoomStockSummaryReport @FromDate = {0}, @ToDate = {1}",
                    fromDate.Date, toDate.Date).ToList();
            }
        }
    }
}
