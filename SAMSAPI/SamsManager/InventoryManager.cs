using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SAMSAPI.Models.Inventory;

namespace SAMSAPI.Manager
{
    /// <summary>
    /// Handles all database operations for the Inventory module.
    /// Uses raw ADO.NET (SqlConnection) because the new inventory tables
    /// are not part of the existing Entity Framework model.
    /// Connection string is read from the same SAMSEntities entry in Web.config.
    /// </summary>
    public class InventoryManager
    {
        // ─── Connection helper ────────────────────────────────────────────────────

        private SqlConnection GetConnection()
        {
            // Extract the raw SQL Server connection string from the EF connection string
            string efConnStr = ConfigurationManager.ConnectionStrings["SAMSEntities"].ConnectionString;

            // The EF connection string wraps the SQL provider connection string inside
            // provider connection string="...".  Extract it.
            var builder = new System.Data.EntityClient.EntityConnectionStringBuilder(efConnStr);
            string sqlConnStr = builder.ProviderConnectionString;

            return new SqlConnection(sqlConnStr);
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 1. STOCK ITEMS
        // ═════════════════════════════════════════════════════════════════════════

        public List<StockItemDto> GetStockItems()
        {
            var list = new List<StockItemDto>();
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(
                    @"SELECT StockItemId, ItemName, Category, Unit AS UnitOfMeasurement,
                             ReorderLevel, AvailableStock AS CurrentStock
                      FROM   vw_StockAvailability
                      ORDER  BY Category, ItemName", con);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new StockItemDto
                        {
                            StockItemId       = (int)dr["StockItemId"],
                            ItemName          = dr["ItemName"].ToString(),
                            Category          = dr["Category"] == DBNull.Value ? null : dr["Category"].ToString(),
                            UnitOfMeasurement = dr["UnitOfMeasurement"].ToString(),
                            ReorderLevel      = dr["ReorderLevel"] == DBNull.Value ? 0 : Convert.ToDouble(dr["ReorderLevel"]),
                            CurrentStock      = dr["CurrentStock"] == DBNull.Value ? 0 : Convert.ToDouble(dr["CurrentStock"]),
                            IsActive          = true
                        });
                    }
                }
            }
            return list;
        }

        public StockItemDto GetStockItem(int id)
        {
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(
                    @"SELECT si.StockItemId, si.ItemName, si.Category, si.UnitOfMeasurement,
                             si.ReorderLevel, si.Description, si.IsActive,
                             ISNULL(v.AvailableStock, 0) AS CurrentStock
                      FROM   StockItems si
                      LEFT   JOIN vw_StockAvailability v ON v.StockItemId = si.StockItemId
                      WHERE  si.StockItemId = @id", con);
                cmd.Parameters.AddWithValue("@id", id);

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return new StockItemDto
                        {
                            StockItemId       = (int)dr["StockItemId"],
                            ItemName          = dr["ItemName"].ToString(),
                            Category          = dr["Category"] == DBNull.Value ? null : dr["Category"].ToString(),
                            UnitOfMeasurement = dr["UnitOfMeasurement"].ToString(),
                            ReorderLevel      = dr["ReorderLevel"] == DBNull.Value ? 0 : Convert.ToDouble(dr["ReorderLevel"]),
                            Description       = dr["Description"] == DBNull.Value ? null : dr["Description"].ToString(),
                            IsActive          = (bool)dr["IsActive"],
                            CurrentStock      = Convert.ToDouble(dr["CurrentStock"])
                        };
                    }
                }
            }
            return null;
        }

        public StockItemDto SaveStockItem(StockItemDto item, int userId)
        {
            using (var con = GetConnection())
            {
                con.Open();
                if (item.StockItemId == 0)
                {
                    var cmd = new SqlCommand(
                        @"INSERT INTO StockItems
                            (ItemName, Category, UnitOfMeasurement, ReorderLevel, Description, IsActive, CreatedBy, CreatedOn)
                          OUTPUT INSERTED.StockItemId
                          VALUES
                            (@ItemName, @Category, @UOM, @ReorderLevel, @Description, @IsActive, @CreatedBy, GETDATE())", con);
                    cmd.Parameters.AddWithValue("@ItemName",    item.ItemName);
                    cmd.Parameters.AddWithValue("@Category",    (object)item.Category    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UOM",         item.UnitOfMeasurement);
                    cmd.Parameters.AddWithValue("@ReorderLevel",item.ReorderLevel);
                    cmd.Parameters.AddWithValue("@Description", (object)item.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive",    item.IsActive);
                    cmd.Parameters.AddWithValue("@CreatedBy",   userId);
                    item.StockItemId = (int)cmd.ExecuteScalar();
                }
                else
                {
                    var cmd = new SqlCommand(
                        @"UPDATE StockItems
                          SET    ItemName          = @ItemName,
                                 Category          = @Category,
                                 UnitOfMeasurement = @UOM,
                                 ReorderLevel      = @ReorderLevel,
                                 Description       = @Description,
                                 IsActive          = @IsActive,
                                 UpdatedBy         = @UpdatedBy,
                                 UpdatedOn         = GETDATE()
                          WHERE  StockItemId = @StockItemId", con);
                    cmd.Parameters.AddWithValue("@ItemName",    item.ItemName);
                    cmd.Parameters.AddWithValue("@Category",    (object)item.Category    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UOM",         item.UnitOfMeasurement);
                    cmd.Parameters.AddWithValue("@ReorderLevel",item.ReorderLevel);
                    cmd.Parameters.AddWithValue("@Description", (object)item.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive",    item.IsActive);
                    cmd.Parameters.AddWithValue("@UpdatedBy",   userId);
                    cmd.Parameters.AddWithValue("@StockItemId", item.StockItemId);
                    cmd.ExecuteNonQuery();
                }
            }
            return GetStockItem(item.StockItemId);
        }

        public string DeleteStockItem(int id)
        {
            using (var con = GetConnection())
            {
                con.Open();
                // Check if item has been used in any transactions
                var checkCmd = new SqlCommand(
                    @"SELECT COUNT(*) FROM RoomStockTransactions WHERE StockItemId = @id
                      UNION ALL
                      SELECT COUNT(*) FROM InventoryPurchaseDetails WHERE StockItemId = @id", con);
                checkCmd.Parameters.AddWithValue("@id", id);

                int usageCount = 0;
                using (var dr = checkCmd.ExecuteReader())
                {
                    while (dr.Read()) usageCount += (int)dr[0];
                }

                if (usageCount > 0)
                {
                    // Soft delete
                    var softCmd = new SqlCommand(
                        "UPDATE StockItems SET IsActive = 0 WHERE StockItemId = @id", con);
                    softCmd.Parameters.AddWithValue("@id", id);
                    softCmd.ExecuteNonQuery();
                    return "Item deactivated (soft-deleted) as it has existing transactions.";
                }
                else
                {
                    var hardCmd = new SqlCommand(
                        "DELETE FROM StockItems WHERE StockItemId = @id", con);
                    hardCmd.Parameters.AddWithValue("@id", id);
                    hardCmd.ExecuteNonQuery();
                    return "Item deleted successfully.";
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 2. ROOM TYPE STOCK CONFIGS
        // ═════════════════════════════════════════════════════════════════════════

        public List<RoomTypeStockConfigDto> GetRoomTypeStockConfigs()
        {
            var list = new List<RoomTypeStockConfigDto>();
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(
                    @"SELECT rtsc.RoomTypeStockConfigId, rtsc.TariffId, t.TariffName,
                             rtsc.StockItemId, si.ItemName, si.UnitOfMeasurement AS Unit,
                             rtsc.Quantity, rtsc.DailyReplenishQty
                      FROM   RoomTypeStockConfigs rtsc
                      INNER  JOIN Tariffs t  ON t.TariffId    = rtsc.TariffId
                      INNER  JOIN StockItems si ON si.StockItemId = rtsc.StockItemId
                      ORDER  BY t.TariffName, si.ItemName", con);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new RoomTypeStockConfigDto
                        {
                            RoomTypeStockConfigId = (int)dr["RoomTypeStockConfigId"],
                            TariffId              = (int)dr["TariffId"],
                            TariffName            = dr["TariffName"].ToString(),
                            StockItemId           = (int)dr["StockItemId"],
                            ItemName              = dr["ItemName"].ToString(),
                            Unit                  = dr["Unit"].ToString(),
                            Quantity              = Convert.ToDouble(dr["Quantity"]),
                            DailyReplenishQty     = dr["DailyReplenishQty"] == DBNull.Value ? 0 : Convert.ToDouble(dr["DailyReplenishQty"])
                        });
                    }
                }
            }
            return list;
        }

        public RoomTypeStockConfigDto SaveRoomTypeStockConfig(SaveRoomTypeStockConfigRequest req)
        {
            using (var con = GetConnection())
            {
                con.Open();
                if (req.RoomTypeStockConfigId == 0)
                {
                    var cmd = new SqlCommand(
                        @"INSERT INTO RoomTypeStockConfigs
                            (TariffId, StockItemId, Quantity, DailyReplenishQty, CreatedBy, CreatedOn)
                          OUTPUT INSERTED.RoomTypeStockConfigId
                          VALUES
                            (@TariffId, @StockItemId, @Quantity, @DailyReplenishQty, @CreatedBy, GETDATE())", con);
                    cmd.Parameters.AddWithValue("@TariffId",          req.TariffId);
                    cmd.Parameters.AddWithValue("@StockItemId",        req.StockItemId);
                    cmd.Parameters.AddWithValue("@Quantity",           req.Quantity);
                    cmd.Parameters.AddWithValue("@DailyReplenishQty",  req.DailyReplenishQty);
                    cmd.Parameters.AddWithValue("@CreatedBy",          req.CreatedBy);
                    req.RoomTypeStockConfigId = (int)cmd.ExecuteScalar();
                }
                else
                {
                    var cmd = new SqlCommand(
                        @"UPDATE RoomTypeStockConfigs
                          SET    TariffId         = @TariffId,
                                 StockItemId      = @StockItemId,
                                 Quantity         = @Quantity,
                                 DailyReplenishQty= @DailyReplenishQty
                          WHERE  RoomTypeStockConfigId = @Id", con);
                    cmd.Parameters.AddWithValue("@TariffId",          req.TariffId);
                    cmd.Parameters.AddWithValue("@StockItemId",        req.StockItemId);
                    cmd.Parameters.AddWithValue("@Quantity",           req.Quantity);
                    cmd.Parameters.AddWithValue("@DailyReplenishQty",  req.DailyReplenishQty);
                    cmd.Parameters.AddWithValue("@Id",                 req.RoomTypeStockConfigId);
                    cmd.ExecuteNonQuery();
                }
            }

            // Return the saved row with lookup names
            var all = GetRoomTypeStockConfigs();
            return all.Find(x => x.RoomTypeStockConfigId == req.RoomTypeStockConfigId);
        }

        public void DeleteRoomTypeStockConfig(int id)
        {
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(
                    "DELETE FROM RoomTypeStockConfigs WHERE RoomTypeStockConfigId = @id", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 3. PURCHASES
        // ═════════════════════════════════════════════════════════════════════════

        public List<PurchaseSummaryDto> GetPurchases(DateTime fromDate, DateTime toDate)
        {
            var list = new List<PurchaseSummaryDto>();
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(
                    @"SELECT ip.PurchaseId, ip.PurchaseDate, ip.InvoiceNumber, ip.VendorName,
                             ip.TotalAmount, ip.Remarks,
                             COUNT(ipd.PurchaseDetailId) AS ItemCount
                      FROM   InventoryPurchases ip
                      LEFT   JOIN InventoryPurchaseDetails ipd ON ipd.PurchaseId = ip.PurchaseId
                      WHERE  ip.PurchaseDate BETWEEN @fromDate AND @toDate
                      GROUP  BY ip.PurchaseId, ip.PurchaseDate, ip.InvoiceNumber,
                               ip.VendorName, ip.TotalAmount, ip.Remarks
                      ORDER  BY ip.PurchaseDate DESC", con);
                cmd.Parameters.AddWithValue("@fromDate", fromDate.Date);
                cmd.Parameters.AddWithValue("@toDate",   toDate.Date);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new PurchaseSummaryDto
                        {
                            PurchaseId    = (int)dr["PurchaseId"],
                            PurchaseDate  = Convert.ToDateTime(dr["PurchaseDate"]),
                            InvoiceNumber = dr["InvoiceNumber"] == DBNull.Value ? null : dr["InvoiceNumber"].ToString(),
                            VendorName    = dr["VendorName"]    == DBNull.Value ? null : dr["VendorName"].ToString(),
                            TotalAmount   = dr["TotalAmount"]   == DBNull.Value ? 0    : Convert.ToDouble(dr["TotalAmount"]),
                            Remarks       = dr["Remarks"]       == DBNull.Value ? null : dr["Remarks"].ToString(),
                            ItemCount     = (int)dr["ItemCount"]
                        });
                    }
                }
            }
            return list;
        }

        public PurchaseDto GetPurchase(int id)
        {
            PurchaseDto purchase = null;
            using (var con = GetConnection())
            {
                con.Open();
                // Header
                var headerCmd = new SqlCommand(
                    @"SELECT PurchaseId, PurchaseDate, InvoiceNumber, VendorName, TotalAmount, Remarks
                      FROM   InventoryPurchases
                      WHERE  PurchaseId = @id", con);
                headerCmd.Parameters.AddWithValue("@id", id);
                using (var dr = headerCmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        purchase = new PurchaseDto
                        {
                            PurchaseId    = (int)dr["PurchaseId"],
                            PurchaseDate  = Convert.ToDateTime(dr["PurchaseDate"]),
                            InvoiceNumber = dr["InvoiceNumber"] == DBNull.Value ? null : dr["InvoiceNumber"].ToString(),
                            VendorName    = dr["VendorName"]    == DBNull.Value ? null : dr["VendorName"].ToString(),
                            TotalAmount   = dr["TotalAmount"]   == DBNull.Value ? 0    : Convert.ToDouble(dr["TotalAmount"]),
                            Remarks       = dr["Remarks"]       == DBNull.Value ? null : dr["Remarks"].ToString(),
                            Details       = new List<PurchaseLineDto>()
                        };
                    }
                }

                if (purchase == null) return null;

                // Lines
                var lineCmd = new SqlCommand(
                    @"SELECT ipd.PurchaseDetailId, ipd.StockItemId, si.ItemName,
                             si.UnitOfMeasurement AS Unit,
                             ipd.Quantity, ipd.UnitPrice, ipd.TotalAmount
                      FROM   InventoryPurchaseDetails ipd
                      INNER  JOIN StockItems si ON si.StockItemId = ipd.StockItemId
                      WHERE  ipd.PurchaseId = @id", con);
                lineCmd.Parameters.AddWithValue("@id", id);
                using (var dr = lineCmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        purchase.Details.Add(new PurchaseLineDto
                        {
                            PurchaseDetailId = (int)dr["PurchaseDetailId"],
                            StockItemId      = (int)dr["StockItemId"],
                            ItemName         = dr["ItemName"].ToString(),
                            Unit             = dr["Unit"].ToString(),
                            Quantity         = Convert.ToDouble(dr["Quantity"]),
                            UnitPrice        = dr["UnitPrice"]   == DBNull.Value ? 0 : Convert.ToDouble(dr["UnitPrice"]),
                            TotalAmount      = dr["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDouble(dr["TotalAmount"])
                        });
                    }
                }
            }
            return purchase;
        }

        public PurchaseDto SavePurchase(PurchaseDto purchase, int userId)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        int purchaseId;
                        if (purchase.PurchaseId == 0)
                        {
                            var headerCmd = new SqlCommand(
                                @"INSERT INTO InventoryPurchases
                                    (PurchaseDate, InvoiceNumber, VendorName, TotalAmount, Remarks, CreatedBy, CreatedOn)
                                  OUTPUT INSERTED.PurchaseId
                                  VALUES
                                    (@PurchaseDate, @InvoiceNumber, @VendorName, @TotalAmount, @Remarks, @CreatedBy, GETDATE())", con, tran);
                            headerCmd.Parameters.AddWithValue("@PurchaseDate",   purchase.PurchaseDate.Date);
                            headerCmd.Parameters.AddWithValue("@InvoiceNumber",  (object)purchase.InvoiceNumber ?? DBNull.Value);
                            headerCmd.Parameters.AddWithValue("@VendorName",     (object)purchase.VendorName    ?? DBNull.Value);
                            headerCmd.Parameters.AddWithValue("@TotalAmount",    purchase.TotalAmount);
                            headerCmd.Parameters.AddWithValue("@Remarks",        (object)purchase.Remarks       ?? DBNull.Value);
                            headerCmd.Parameters.AddWithValue("@CreatedBy",      userId);
                            purchaseId = (int)headerCmd.ExecuteScalar();
                        }
                        else
                        {
                            purchaseId = purchase.PurchaseId;
                            var headerCmd = new SqlCommand(
                                @"UPDATE InventoryPurchases
                                  SET    PurchaseDate  = @PurchaseDate,
                                         InvoiceNumber = @InvoiceNumber,
                                         VendorName    = @VendorName,
                                         TotalAmount   = @TotalAmount,
                                         Remarks       = @Remarks,
                                         UpdatedBy     = @UpdatedBy,
                                         UpdatedOn     = GETDATE()
                                  WHERE  PurchaseId    = @PurchaseId", con, tran);
                            headerCmd.Parameters.AddWithValue("@PurchaseDate",   purchase.PurchaseDate.Date);
                            headerCmd.Parameters.AddWithValue("@InvoiceNumber",  (object)purchase.InvoiceNumber ?? DBNull.Value);
                            headerCmd.Parameters.AddWithValue("@VendorName",     (object)purchase.VendorName    ?? DBNull.Value);
                            headerCmd.Parameters.AddWithValue("@TotalAmount",    purchase.TotalAmount);
                            headerCmd.Parameters.AddWithValue("@Remarks",        (object)purchase.Remarks       ?? DBNull.Value);
                            headerCmd.Parameters.AddWithValue("@UpdatedBy",      userId);
                            headerCmd.Parameters.AddWithValue("@PurchaseId",     purchaseId);
                            headerCmd.ExecuteNonQuery();

                            // Remove existing lines before re-inserting
                            var delLines = new SqlCommand(
                                "DELETE FROM InventoryPurchaseDetails WHERE PurchaseId = @PurchaseId", con, tran);
                            delLines.Parameters.AddWithValue("@PurchaseId", purchaseId);
                            delLines.ExecuteNonQuery();
                        }

                        // Insert lines
                        if (purchase.Details != null)
                        {
                            foreach (var line in purchase.Details)
                            {
                                var lineCmd = new SqlCommand(
                                    @"INSERT INTO InventoryPurchaseDetails
                                        (PurchaseId, StockItemId, Quantity, UnitPrice, TotalAmount)
                                      VALUES
                                        (@PurchaseId, @StockItemId, @Quantity, @UnitPrice, @TotalAmount)", con, tran);
                                lineCmd.Parameters.AddWithValue("@PurchaseId",  purchaseId);
                                lineCmd.Parameters.AddWithValue("@StockItemId", line.StockItemId);
                                lineCmd.Parameters.AddWithValue("@Quantity",    line.Quantity);
                                lineCmd.Parameters.AddWithValue("@UnitPrice",   line.UnitPrice);
                                lineCmd.Parameters.AddWithValue("@TotalAmount", line.TotalAmount);
                                lineCmd.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                        purchase.PurchaseId = purchaseId;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
            return GetPurchase(purchase.PurchaseId);
        }

        public string DeletePurchase(int id)
        {
            using (var con = GetConnection())
            {
                con.Open();
                // Validate: no stock from this purchase can have been issued
                var checkCmd = new SqlCommand(
                    @"SELECT COUNT(*)
                      FROM   RoomStockTransactions rst
                      INNER  JOIN InventoryPurchaseDetails ipd ON ipd.StockItemId = rst.StockItemId
                      WHERE  ipd.PurchaseId = @id", con);
                checkCmd.Parameters.AddWithValue("@id", id);
                int usedCount = (int)checkCmd.ExecuteScalar();

                if (usedCount > 0)
                    return "Cannot delete: stock from this purchase has already been issued to rooms.";

                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        var delLines = new SqlCommand(
                            "DELETE FROM InventoryPurchaseDetails WHERE PurchaseId = @id", con, tran);
                        delLines.Parameters.AddWithValue("@id", id);
                        delLines.ExecuteNonQuery();

                        var delHeader = new SqlCommand(
                            "DELETE FROM InventoryPurchases WHERE PurchaseId = @id", con, tran);
                        delHeader.Parameters.AddWithValue("@id", id);
                        delHeader.ExecuteNonQuery();

                        tran.Commit();
                        return "Purchase deleted successfully.";
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 4. ROOM STOCK MANAGEMENT
        // ═════════════════════════════════════════════════════════════════════════

        public List<RoomStockDto> GetRoomStock(int bookingDetailId)
        {
            var list = new List<RoomStockDto>();
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand("usp_GetRoomStockBalance", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@BookingDetailId", bookingDetailId);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new RoomStockDto
                        {
                            RoomStockId      = (int)dr["RoomStockId"],
                            StockItemId      = (int)dr["StockItemId"],
                            ItemName         = dr["ItemName"].ToString(),
                            Category         = dr["Category"] == DBNull.Value ? null : dr["Category"].ToString(),
                            Unit             = dr["Unit"].ToString(),
                            StockType        = dr["StockType"].ToString(),
                            IssuedQuantity   = Convert.ToDouble(dr["IssuedQuantity"]),
                            ReturnedQuantity = Convert.ToDouble(dr["ReturnedQuantity"]),
                            BalanceQuantity  = Convert.ToDouble(dr["BalanceQuantity"]),
                            IssueDate        = Convert.ToDateTime(dr["IssueDate"])
                        });
                    }
                }
            }
            return list;
        }

        public string AddRoomStock(AddRoomStockRequest req)
        {
            using (var con = GetConnection())
            {
                con.Open();

                // Resolve BookingId and RoomId from BookingDetailId
                int bookingId = 0, roomId = 0;
                var resolveCmd = new SqlCommand(
                    "SELECT BookingId, RoomId FROM BookingDetails WHERE BookingDetailId = @id", con);
                resolveCmd.Parameters.AddWithValue("@id", req.BookingDetailId);
                using (var dr = resolveCmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        bookingId = (int)dr["BookingId"];
                        roomId    = (int)dr["RoomId"];
                    }
                    else
                    {
                        return "Error: BookingDetail not found.";
                    }
                }

                // Validate available stock for each item
                foreach (var item in req.Items)
                {
                    var stockCmd = new SqlCommand(
                        "SELECT ISNULL(AvailableStock, 0) FROM vw_StockAvailability WHERE StockItemId = @id", con);
                    stockCmd.Parameters.AddWithValue("@id", item.StockItemId);
                    var available = stockCmd.ExecuteScalar();
                    double avail  = available == null || available == DBNull.Value ? 0 : Convert.ToDouble(available);
                    if (item.Quantity > avail)
                        return string.Format("Insufficient stock for StockItemId {0}. Available: {1}, Requested: {2}",
                            item.StockItemId, avail, item.Quantity);
                }

                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in req.Items)
                        {
                            var insertCmd = new SqlCommand(
                                @"INSERT INTO RoomStockTransactions
                                    (BookingDetailId, BookingId, RoomId, StockItemId,
                                     StockType, IssuedQuantity, Remarks, CreatedBy, CreatedOn)
                                  VALUES
                                    (@BookingDetailId, @BookingId, @RoomId, @StockItemId,
                                     'Additional', @Qty, @Remarks, @CreatedBy, GETDATE())", con, tran);
                            insertCmd.Parameters.AddWithValue("@BookingDetailId", req.BookingDetailId);
                            insertCmd.Parameters.AddWithValue("@BookingId",       bookingId);
                            insertCmd.Parameters.AddWithValue("@RoomId",          roomId);
                            insertCmd.Parameters.AddWithValue("@StockItemId",     item.StockItemId);
                            insertCmd.Parameters.AddWithValue("@Qty",             item.Quantity);
                            insertCmd.Parameters.AddWithValue("@Remarks",         (object)req.Remarks ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@CreatedBy",       req.CreatedBy);
                            insertCmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                        return "Stock added successfully.";
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public string ReturnRoomStock(ReturnRoomStockRequest req)
        {
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand("usp_ReturnRoomStock", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@RoomStockId",    req.RoomStockId);
                cmd.Parameters.AddWithValue("@ReturnQuantity", req.ReturnQuantity);
                cmd.Parameters.AddWithValue("@Reason",         (object)req.Reason     ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy",      req.CreatedBy);

                try
                {
                    cmd.ExecuteNonQuery();
                    return "Stock returned successfully.";
                }
                catch (SqlException ex)
                {
                    return "Error: " + ex.Message;
                }
            }
        }

        public string AssignCheckinStock(int bookingDetailId, int createdBy)
        {
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand("usp_AssignCheckinStock", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@BookingDetailId", bookingDetailId);
                cmd.Parameters.AddWithValue("@CreatedBy",       createdBy);

                int itemsAssigned = 0;
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) itemsAssigned = (int)dr["ItemsAssigned"];
                }
                return string.Format("{0} stock item(s) assigned for check-in.", itemsAssigned);
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 5. REPORTS
        // ═════════════════════════════════════════════════════════════════════════

        public List<StockAvailabilityDto> GetAvailableStockReport()
        {
            var list = new List<StockAvailabilityDto>();
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(
                    "SELECT * FROM vw_StockAvailability ORDER BY Category, ItemName", con);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new StockAvailabilityDto
                        {
                            StockItemId    = (int)dr["StockItemId"],
                            ItemName       = dr["ItemName"].ToString(),
                            Category       = dr["Category"] == DBNull.Value ? null : dr["Category"].ToString(),
                            Unit           = dr["Unit"].ToString(),
                            ReorderLevel   = dr["ReorderLevel"] == DBNull.Value ? 0 : Convert.ToDouble(dr["ReorderLevel"]),
                            TotalPurchased = Convert.ToDouble(dr["TotalPurchased"]),
                            TotalIssued    = Convert.ToDouble(dr["TotalIssued"]),
                            TotalReturned  = Convert.ToDouble(dr["TotalReturned"]),
                            AvailableStock = Convert.ToDouble(dr["AvailableStock"])
                        });
                    }
                }
            }
            return list;
        }

        public List<LowStockDto> GetLowStockReport()
        {
            var list = new List<LowStockDto>();
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(
                    "SELECT * FROM vw_LowStockItems ORDER BY AvailableStock ASC", con);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new LowStockDto
                        {
                            StockItemId      = (int)dr["StockItemId"],
                            ItemName         = dr["ItemName"].ToString(),
                            Category         = dr["Category"] == DBNull.Value ? null : dr["Category"].ToString(),
                            Unit             = dr["Unit"].ToString(),
                            ReorderLevel     = dr["ReorderLevel"] == DBNull.Value ? 0 : Convert.ToDouble(dr["ReorderLevel"]),
                            TotalPurchased   = Convert.ToDouble(dr["TotalPurchased"]),
                            TotalIssued      = Convert.ToDouble(dr["TotalIssued"]),
                            TotalReturned    = Convert.ToDouble(dr["TotalReturned"]),
                            AvailableStock   = Convert.ToDouble(dr["AvailableStock"]),
                            LastPurchaseId   = dr["LastPurchaseId"]   == DBNull.Value ? (int?)null      : (int)dr["LastPurchaseId"],
                            LastPurchaseDate = dr["LastPurchaseDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["LastPurchaseDate"])
                        });
                    }
                }
            }
            return list;
        }

        public List<StockMovementDto> GetStockMovementReport(DateTime fromDate, DateTime toDate)
        {
            var list = new List<StockMovementDto>();
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand("usp_GetStockMovementReport", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate",   toDate.Date);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new StockMovementDto
                        {
                            TransactionDate = Convert.ToDateTime(dr["TransactionDate"]),
                            ItemName        = dr["ItemName"].ToString(),
                            Category        = dr["Category"] == DBNull.Value ? null : dr["Category"].ToString(),
                            TransactionType = dr["TransactionType"].ToString(),
                            RoomNo          = dr["RoomNo"]    == DBNull.Value ? null : dr["RoomNo"].ToString(),
                            InQty           = Convert.ToDouble(dr["InQty"]),
                            OutQty          = Convert.ToDouble(dr["OutQty"]),
                            Balance         = Convert.ToDouble(dr["Balance"]),
                            Reference       = dr["Reference"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        public List<RoomStockSummaryDto> GetRoomStockSummaryReport(DateTime fromDate, DateTime toDate)
        {
            var list = new List<RoomStockSummaryDto>();
            using (var con = GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand("usp_GetRoomStockSummaryReport", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate",   toDate.Date);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new RoomStockSummaryDto
                        {
                            RoomNo        = dr["RoomNo"].ToString(),
                            GuestName     = dr["GuestName"]    == DBNull.Value ? null             : dr["GuestName"].ToString(),
                            CheckInDate   = dr["CheckInDate"]  == DBNull.Value ? (DateTime?)null  : Convert.ToDateTime(dr["CheckInDate"]),
                            CheckOutDate  = dr["CheckOutDate"] == DBNull.Value ? (DateTime?)null  : Convert.ToDateTime(dr["CheckOutDate"]),
                            TotalIssued   = Convert.ToDouble(dr["TotalIssued"]),
                            TotalReturned = Convert.ToDouble(dr["TotalReturned"]),
                            NetConsumed   = Convert.ToDouble(dr["NetConsumed"])
                        });
                    }
                }
            }
            return list;
        }
    }
}
