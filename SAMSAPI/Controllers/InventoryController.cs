using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using SAMSAPI.Manager;
using SAMSAPI.Models.Inventory;

namespace SAMSAPI.Controllers
{
    /// <summary>
    /// Inventory Module — all endpoints under /api/inventory/
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class InventoryController : ApiController
    {
        private readonly InventoryManager _mgr = new InventoryManager();

        // Helper: read userId from query-string (consistent with rest of the project)
        private int GetUserId()
        {
            var qs = Request.GetQueryNameValuePairs();
            foreach (var kv in qs)
                if (kv.Key.Equals("userId", StringComparison.OrdinalIgnoreCase))
                    return int.TryParse(kv.Value, out int uid) ? uid : 0;
            return 0;
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 1.1  STOCK ITEMS
        // ═════════════════════════════════════════════════════════════════════════

        /// <summary>GET /api/inventory/StockItems</summary>
        [HttpGet]
        [ActionName("StockItems")]
        public IHttpActionResult GetStockItems()
        {
            try
            {
                return Ok(_mgr.GetStockItems());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>GET /api/inventory/StockItem/{id}</summary>
        [HttpGet]
        [ActionName("StockItem")]
        public IHttpActionResult GetStockItem(int id)
        {
            try
            {
                var item = _mgr.GetStockItem(id);
                if (item == null) return NotFound();
                return Ok(item);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>POST /api/inventory/SaveStockItem</summary>
        [HttpPost]
        [ActionName("SaveStockItem")]
        public IHttpActionResult SaveStockItem([FromBody] StockItemDto item)
        {
            if (item == null) return BadRequest("Request body is required.");
            try
            {
                var saved = _mgr.SaveStockItem(item, GetUserId());
                return Ok(saved);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>DELETE /api/inventory/DeleteStockItem/{id}</summary>
        [HttpDelete]
        [ActionName("DeleteStockItem")]
        public IHttpActionResult DeleteStockItem(int id)
        {
            try
            {
                string result = _mgr.DeleteStockItem(id);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 1.2  ROOM TYPE STOCK CONFIGS
        // ═════════════════════════════════════════════════════════════════════════

        /// <summary>GET /api/inventory/RoomTypeStockConfigs</summary>
        [HttpGet]
        [ActionName("RoomTypeStockConfigs")]
        public IHttpActionResult GetRoomTypeStockConfigs()
        {
            try
            {
                return Ok(_mgr.GetRoomTypeStockConfigs());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>POST /api/inventory/SaveRoomTypeStockConfig</summary>
        [HttpPost]
        [ActionName("SaveRoomTypeStockConfig")]
        public IHttpActionResult SaveRoomTypeStockConfig([FromBody] SaveRoomTypeStockConfigRequest req)
        {
            if (req == null) return BadRequest("Request body is required.");
            try
            {
                var saved = _mgr.SaveRoomTypeStockConfig(req);
                return Ok(saved);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>DELETE /api/inventory/DeleteRoomTypeStockConfig/{id}</summary>
        [HttpDelete]
        [ActionName("DeleteRoomTypeStockConfig")]
        public IHttpActionResult DeleteRoomTypeStockConfig(int id)
        {
            try
            {
                _mgr.DeleteRoomTypeStockConfig(id);
                return Ok(new { Message = "Configuration deleted successfully." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 1.3  PURCHASES
        // ═════════════════════════════════════════════════════════════════════════

        /// <summary>GET /api/inventory/Purchases?fromDate=YYYY-MM-DD&amp;toDate=YYYY-MM-DD</summary>
        [HttpGet]
        [ActionName("Purchases")]
        public IHttpActionResult GetPurchases(string fromDate = null, string toDate = null)
        {
            try
            {
                DateTime from = string.IsNullOrEmpty(fromDate)
                    ? DateTime.Today.AddMonths(-1)
                    : DateTime.Parse(fromDate);
                DateTime to   = string.IsNullOrEmpty(toDate)
                    ? DateTime.Today
                    : DateTime.Parse(toDate);

                return Ok(_mgr.GetPurchases(from, to));
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>GET /api/inventory/Purchase/{id}</summary>
        [HttpGet]
        [ActionName("Purchase")]
        public IHttpActionResult GetPurchase(int id)
        {
            try
            {
                var purchase = _mgr.GetPurchase(id);
                if (purchase == null) return NotFound();
                return Ok(purchase);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>POST /api/inventory/SavePurchase</summary>
        [HttpPost]
        [ActionName("SavePurchase")]
        public IHttpActionResult SavePurchase([FromBody] PurchaseDto purchase)
        {
            if (purchase == null) return BadRequest("Request body is required.");
            try
            {
                var saved = _mgr.SavePurchase(purchase, GetUserId());
                return Ok(saved);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>DELETE /api/inventory/DeletePurchase/{id}</summary>
        [HttpDelete]
        [ActionName("DeletePurchase")]
        public IHttpActionResult DeletePurchase(int id)
        {
            try
            {
                string result = _mgr.DeletePurchase(id);
                if (result.StartsWith("Cannot"))
                    return BadRequest(result);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 1.4  ROOM STOCK MANAGEMENT
        // ═════════════════════════════════════════════════════════════════════════

        /// <summary>GET /api/inventory/RoomStock?bookingDetailId=</summary>
        [HttpGet]
        [ActionName("RoomStock")]
        public IHttpActionResult GetRoomStock(int bookingDetailId)
        {
            try
            {
                return Ok(_mgr.GetRoomStock(bookingDetailId));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>POST /api/inventory/AddRoomStock</summary>
        [HttpPost]
        [ActionName("AddRoomStock")]
        public IHttpActionResult AddRoomStock([FromBody] AddRoomStockRequest req)
        {
            if (req == null || req.Items == null || req.Items.Count == 0)
                return BadRequest("Request body with at least one item is required.");
            try
            {
                string result = _mgr.AddRoomStock(req);
                if (result.StartsWith("Error") || result.StartsWith("Insufficient"))
                    return BadRequest(result);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>POST /api/inventory/ReturnRoomStock</summary>
        [HttpPost]
        [ActionName("ReturnRoomStock")]
        public IHttpActionResult ReturnRoomStock([FromBody] ReturnRoomStockRequest req)
        {
            if (req == null) return BadRequest("Request body is required.");
            try
            {
                string result = _mgr.ReturnRoomStock(req);
                if (result.StartsWith("Error"))
                    return BadRequest(result);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>POST /api/inventory/AssignCheckinStock</summary>
        [HttpPost]
        [ActionName("AssignCheckinStock")]
        public IHttpActionResult AssignCheckinStock([FromBody] AssignCheckinStockRequest req)
        {
            if (req == null || req.BookingDetailId == 0)
                return BadRequest("bookingDetailId is required.");
            try
            {
                string result = _mgr.AssignCheckinStock(req.BookingDetailId, req.CreatedBy);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // 1.5  REPORTS
        // ═════════════════════════════════════════════════════════════════════════

        /// <summary>GET /api/inventory/AvailableStockReport</summary>
        [HttpGet]
        [ActionName("AvailableStockReport")]
        public IHttpActionResult GetAvailableStockReport()
        {
            try
            {
                return Ok(_mgr.GetAvailableStockReport());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>GET /api/inventory/LowStockReport</summary>
        [HttpGet]
        [ActionName("LowStockReport")]
        public IHttpActionResult GetLowStockReport()
        {
            try
            {
                return Ok(_mgr.GetLowStockReport());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>GET /api/inventory/StockMovementReport?fromDate=YYYY-MM-DD&amp;toDate=YYYY-MM-DD</summary>
        [HttpGet]
        [ActionName("StockMovementReport")]
        public IHttpActionResult GetStockMovementReport(string fromDate = null, string toDate = null)
        {
            try
            {
                DateTime from = string.IsNullOrEmpty(fromDate)
                    ? DateTime.Today.AddMonths(-1)
                    : DateTime.Parse(fromDate);
                DateTime to   = string.IsNullOrEmpty(toDate)
                    ? DateTime.Today
                    : DateTime.Parse(toDate);

                return Ok(_mgr.GetStockMovementReport(from, to));
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>GET /api/inventory/RoomStockSummaryReport?fromDate=YYYY-MM-DD&amp;toDate=YYYY-MM-DD</summary>
        [HttpGet]
        [ActionName("RoomStockSummaryReport")]
        public IHttpActionResult GetRoomStockSummaryReport(string fromDate = null, string toDate = null)
        {
            try
            {
                DateTime from = string.IsNullOrEmpty(fromDate)
                    ? DateTime.Today.AddMonths(-1)
                    : DateTime.Parse(fromDate);
                DateTime to   = string.IsNullOrEmpty(toDate)
                    ? DateTime.Today
                    : DateTime.Parse(toDate);

                return Ok(_mgr.GetRoomStockSummaryReport(from, to));
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
