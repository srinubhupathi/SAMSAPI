using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using SAMSAPI.Manager;
using SAMSAPI.WhatsApp.Models;
using SAMSAPI.WhatsApp.Service;
using SAMSData;

namespace SAMSAPI.Controllers
{
    /// <summary>
    /// WhatsApp API Controller for SAMS Club Management System
    ///
    /// Base Route : api/WhatsApp
    ///
    /// ┌─────────────────────────────────────────────────────────────────────┐
    /// │  ENDPOINT SUMMARY  (for Angular frontend)                           │
    /// ├───────────────────────────────┬─────────────────────────────────────┤
    /// │  POST SendPaymentSuccess      │ After fee payment is saved           │
    /// │  POST SendPaymentReminder     │ Send reminder to one member          │
    /// │  POST SendBulkReminders       │ Batch reminders to many members      │
    /// │  POST SendCustomMessage       │ Any template with custom placeholders │
    /// │  GET  GetTemplates            │ Return templates JSON to Angular      │
    /// │  POST ReloadTemplates         │ Force-refresh template cache          │
    /// │  POST SendMembershipFeeAlert  │ Shortcut — fee saved → WhatsApp       │
    /// │  POST SendSportsFeeAlert      │ Shortcut — sports fee saved → WA      │
    /// │  POST SendRemindersByFeeType  │ Auto-build reminders by fee type      │
    /// └───────────────────────────────┴─────────────────────────────────────┘
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class WhatsAppController : ApiController
    {
        private readonly WhatsAppService _whatsApp;
        private readonly SAMSManager     _mgr;

        public WhatsAppController()
        {
            _whatsApp = new WhatsAppService();
            _mgr      = new SAMSManager();
        }

        // ═══════════════════════════════════════════════════════════
        //  1.  SEND PAYMENT SUCCESS  (generic — any fee type)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// POST api/WhatsApp/SendPaymentSuccess
        /// 
        /// Angular body example:
        /// {
        ///   "PhoneNumber"  : "9246668725",
        ///   "TemplateKey"  : "membership_fee_success",
        ///   "Language"     : "both",
        ///   "Placeholders" : {
        ///     "MemberName"   : "Ravi Kumar",
        ///     "MemberCode"   : "COS-1234",
        ///     "Amount"       : "5000",
        ///     "FeeType"      : "Membership Fee",
        ///     "ReceiptNo"    : "REC-2026-001",
        ///     "PaidDate"     : "26-Mar-2026",
        ///     "PaymentMode"  : "Cash"
        ///   }
        /// }
        /// </summary>
        [HttpPost]
        [ActionName("SendPaymentNotification")]
        public async Task<WhatsAppApiResponse> SendPaymentSuccessAsync([FromBody] PaymentNotificationRequest request)
        {
            if (request == null)
                return BadRequest("Request body is empty.");

            _mgr.SendPaymentNotification(request);
            return Success("Ok");
        }

        // ═══════════════════════════════════════════════════════════
        //  2.  SEND PAYMENT REMINDER  (single member)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// POST api/WhatsApp/SendPaymentReminder
        ///
        /// Angular body example:
        /// {
        ///   "PhoneNumber"  : "9246668725",
        ///   "TemplateKey"  : "membership_fee_reminder",
        ///   "Language"     : "both",
        ///   "Placeholders" : {
        ///     "MemberName"  : "Ravi Kumar",
        ///     "MemberCode"  : "COS-1234",
        ///     "DueAmount"   : "5000",
        ///     "DueYear"     : "2025-2026",
        ///     "DueDate"     : "31-Mar-2026",
        ///     "FeeType"     : "Membership Fee"
        ///   }
        /// }
        /// </summary>
        //[HttpPost]
        //[ActionName("SendPaymentReminder")]
        //public async Task<WhatsAppApiResponse> SendPaymentReminderAsync([FromBody] WhatsAppSendRequest request)
        //{
        //    if (request == null)
        //        return BadRequest("Request body is empty.");

        //    if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        //        return BadRequest("PhoneNumber is required.");

        //    if (string.IsNullOrWhiteSpace(request.TemplateKey))
        //        request.TemplateKey = "membership_fee_reminder";

        //    if (string.IsNullOrWhiteSpace(request.Language))
        //        request.Language = "both";

        //    return await _whatsApp.SendTemplatedMessageAsync(request);
        //}

        // ═══════════════════════════════════════════════════════════
        //  3.  SEND BULK REMINDERS  (multiple members by fee type)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// POST api/WhatsApp/SendBulkReminders
        ///
        /// Angular body example:
        /// {
        ///   "MemberIds" : [101, 102, 103],
        ///   "FeeType"   : "Membership",
        ///   "Language"  : "both"
        /// }
        ///
        /// Looks up each member's dues from the database and sends reminders.
        /// </summary>
        //[HttpPost]
        //[ActionName("SendBulkReminders")]
        //public async Task<WhatsAppApiResponse> SendBulkRemindersAsync([FromBody] WhatsAppBulkReminderRequest request)
        //{
        //    if (request == null || request.MemberIds == null || request.MemberIds.Count == 0)
        //        return BadRequest("MemberIds list is required.");

        //    var contexts = new List<WhatsAppReminderContext>();
        //    string langCode = (request.Language ?? "both").ToLower();
        //    MessageLanguage lang = langCode == "te" ? MessageLanguage.Telugu
        //                         : langCode == "en" ? MessageLanguage.English
        //                         : MessageLanguage.Both;

        //    string clubName = System.Configuration.ConfigurationManager
        //                      .AppSettings["WhatsApp_ClubName"] ?? "Cosmopolitan Club";

        //    foreach (int memberId in request.MemberIds)
        //    {
        //        try
        //        {
        //            Member member = _mgr.GetMember(memberId);
        //            if (member == null) continue;

        //            MembeFeeStatus feeStatus = _mgr.GetMemberFeeDetails(member);

        //            bool isMembership = (request.FeeType ?? "").ToLower() == "membership"
        //                              || string.IsNullOrWhiteSpace(request.FeeType);
        //            bool isSports     = (request.FeeType ?? "").ToLower() == "sports";

        //            if (isMembership && feeStatus.FeeDueStatus == "Pending")
        //            {
        //                contexts.Add(new WhatsAppReminderContext
        //                {
        //                    MemberName  = member.FirstName ,
        //                    MemberCode  = member.MemberCode,
        //                    PhoneNumber = member.Phone,
        //                    DueAmount   = feeStatus.DueAmount.ToString("N0"),
        //                    DueYear     = (feeStatus.DueFromYear + " - " + feeStatus.DueToYear),
        //                    DueDate     = "31-Mar-" + feeStatus.DueToYear,
        //                    FeeType     = "Membership Fee",
        //                    ClubName    = clubName,
        //                    Language    = lang,
        //                    TemplateKey = WhatsAppTemplateKey.membership_fee_reminder
        //                });
        //            }
        //            else if (isSports)
        //            {
        //                // Sports fee reminder — same structure, different template
        //                contexts.Add(new WhatsAppReminderContext
        //                {
        //                    MemberName  = member.FirstName,
        //                    MemberCode  = member.MemberCode,
        //                    PhoneNumber = member.Phone,
        //                    DueAmount   = feeStatus.DueAmount.ToString("N0"),
        //                    DueYear     = (feeStatus.DueFromYear + " - " + feeStatus.DueToYear),
        //                    DueDate     = "31-Mar-" + feeStatus.DueToYear,
        //                    FeeType     = "Sports Fee",
        //                    ClubName    = clubName,
        //                    Language    = lang,
        //                    TemplateKey = WhatsAppTemplateKey.sports_fee_reminder
        //                });
        //            }
        //        }
        //        catch { /* skip individual failures */ }
        //    }

        //    if (contexts.Count == 0)
        //        return new WhatsAppApiResponse { Success = true, Message = "No pending dues found.", SentCount = 0 };

        //    return await _whatsApp.SendBulkRemindersAsync(contexts);
        //}

        // ═══════════════════════════════════════════════════════════
        //  4.  SEND CUSTOM / GENERIC MESSAGE
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// POST api/WhatsApp/SendCustomMessage
        /// Full flexibility — Angular passes any template key + placeholders.
        ///// </summary>
        //[HttpPost]
        //[ActionName("SendCustomMessage")]
        //public async Task<WhatsAppApiResponse> SendCustomMessageAsync([FromBody] WhatsAppSendRequest request)
        //{
        //    if (request == null)
        //        return BadRequest("Request body is empty.");
        //    if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        //        return BadRequest("PhoneNumber is required.");
        //    if (string.IsNullOrWhiteSpace(request.TemplateKey))
        //        return BadRequest("TemplateKey is required.");

        //    return await _whatsApp.SendTemplatedMessageAsync(request);
        //}

        // ═══════════════════════════════════════════════════════════
        //  5.  SHORTCUT — MEMBERSHIP FEE PAID  (pass memberId + txnId)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// POST api/WhatsApp/SendMembershipFeeAlert?memberId=101&txnId=55
        ///
        /// Fetches member and transaction from DB, then sends WhatsApp automatically.
        /// Designed to be called right after SaveMemberFee succeeds.
        /// </summary>
        //[HttpPost]
        //[ActionName("SendMembershipFeeAlert")]
        //public async Task<WhatsAppApiResponse> SendMembershipFeeAlertAsync(int memberId, int txnId)
        //{
        //    try
        //    {
        //        Member member = _mgr.GetMember(memberId);
        //        if (member == null) return BadRequest("Member not found.");

        //        var txnList = _mgr.GetMemberFees(memberId);
        //        MembershipFeeTransaction txn = txnList?.Find(x => x.MemershipFeeId == txnId);

        //        string clubName = System.Configuration.ConfigurationManager
        //                          .AppSettings["WhatsApp_ClubName"] ?? "Cosmopolitan Club";

        //        var ctx = new WhatsAppPaymentContext
        //        {
        //            MemberName  = member.FirstName,
        //            MemberCode  = member.MemberCode,
        //            PhoneNumber = member.Phone,
        //            Amount      = txn?.MembershipFee?.ToString("N0") ?? "0",
        //            FeeType     = "Membership Fee",
        //            PaidDate    = (txn?.PaidDate ?? DateTime.Now).ToString("dd-MMM-yyyy"),
        //           ReceiptNo   = txn?.FeeReceiptNo.ToString()?? "-",
        //            PaymentMode = txn?.PaymentMode ?? "Cash",
        //            ClubName    = clubName,
        //            Language    = MessageLanguage.Both,
        //            TemplateKey = WhatsAppTemplateKey.membership_fee_success
        //        };

        //        return await _whatsApp.SendPaymentSuccessMessageAsync(ctx);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest("Error: " + ex.Message);
        //    }
        //}

        // ═══════════════════════════════════════════════════════════
        //  6.  SHORTCUT — SPORTS FEE PAID
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// POST api/WhatsApp/SendSportsFeeAlert?memberId=101&sportName=Badminton&amount=2000&receiptNo=REC-001
        /// </summary>
        //[HttpPost]
        //[ActionName("SendSportsFeeAlert")]
        //public async Task<WhatsAppApiResponse> SendSportsFeeAlertAsync(
        //    int memberId, string sportName, decimal amount,
        //    string receiptNo = "", string paymentMode = "Cash")
        //{
        //    try
        //    {
        //        Member member = _mgr.GetMember(memberId);
        //        if (member == null) return BadRequest("Member not found.");

        //        string clubName = System.Configuration.ConfigurationManager
        //                          .AppSettings["WhatsApp_ClubName"] ?? "Cosmopolitan Club";

        //        var ctx = new WhatsAppPaymentContext
        //        {
        //            MemberName  = member.FirstName,
        //            MemberCode  = member.MemberCode,
        //            PhoneNumber = member.Phone,
        //            Amount      = amount.ToString("N0"),
        //            FeeType     = "Sports Fee",
        //            SportName   = sportName,
        //            PaidDate    = DateTime.Now.ToString("dd-MMM-yyyy"),
        //            ReceiptNo   = receiptNo,
        //            PaymentMode = paymentMode,
        //            ClubName    = clubName,
        //            Language    = MessageLanguage.Both,
        //            TemplateKey = WhatsAppTemplateKey.sports_fee_success
        //        };

        //        return await _whatsApp.SendPaymentSuccessMessageAsync(ctx);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest("Error: " + ex.Message);
        //    }
        //}

        // ═══════════════════════════════════════════════════════════
        //  7.  SEND REMINDERS BY FEE TYPE  (auto-fetch all due members)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// POST api/WhatsApp/SendRemindersByFeeType?feeType=Membership&language=both
        ///
        /// Auto-fetches ALL members with pending dues and sends reminders.
        /// </summary>
        //[HttpPost]
        //[ActionName("SendRemindersByFeeType")]
        //public async Task<WhatsAppApiResponse> SendRemindersByFeeTypeAsync(
        //    string feeType = "Membership", string language = "both")
        //{
        //    try
        //    {
        //        var dueMembers = _mgr.GetMembersFeeStatus();
        //        if (dueMembers == null || dueMembers.Count == 0)
        //            return new WhatsAppApiResponse { Success = true, Message = "No dues found." };

        //        var ids = new List<int>();
        //        foreach (var m in dueMembers)
        //        {
        //            if (m.FeeDueStatus == "Pending" && m.Member != null && m.Member.MemberId > 0)
        //                ids.Add(m.Member.MemberId);
        //        }

        //        var req = new WhatsAppBulkReminderRequest
        //        {
        //            MemberIds = ids,
        //            FeeType   = feeType,
        //            Language  = language
        //        };

        //        return await SendBulkRemindersAsync(req);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest("Error: " + ex.Message);
        //    }
        //}

        // ═══════════════════════════════════════════════════════════
        //  8.  GET TEMPLATES  (return JSON to Angular for preview)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// GET api/WhatsApp/GetTemplates
        /// Returns the full template config JSON so Angular can preview messages.
        /// </summary>
        //[HttpGet]
        //[ActionName("GetTemplates")]
        //public object GetTemplates()
        //{
        //    try
        //    {
        //        string path = System.Configuration.ConfigurationManager
        //                      .AppSettings["WhatsApp_TemplateFile"]
        //                      ?? System.Web.HttpRuntime.AppDomainAppPath
        //                         + @"Content\Config\whatsapp-templates.json";

        //        string json = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
        //        return Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { error = ex.Message };
        //    }
        //}

        // ═══════════════════════════════════════════════════════════
        //  9.  RELOAD TEMPLATES  (flush cache after admin edits JSON)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// POST api/WhatsApp/ReloadTemplates
        /// </summary>
        //[HttpPost]
        //[ActionName("ReloadTemplates")]
        //public WhatsAppApiResponse ReloadTemplates()
        //{
        //    try
        //    {
        //        _whatsApp.ReloadTemplates();
        //        return new WhatsAppApiResponse { Success = true, Message = "Templates reloaded successfully." };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new WhatsAppApiResponse { Success = false, Message = ex.Message };
        //    }
        //}

        // ═══════════════════════════════════════════════════════════
        //  10.  TEST CONNECTION  (verify credentials with enotify)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// POST api/WhatsApp/TestConnection?phone=9246668725
        /// Sends a test message to verify API credentials.
        /// </summary>
        //[HttpPost]
        //[ActionName("TestConnection")]
        //public async Task<WhatsAppApiResponse> TestConnectionAsync(string phone)
        //{
        //    if (string.IsNullOrWhiteSpace(phone))
        //        return BadRequest("Phone number is required for test.");

        //    var req = new WhatsAppSendRequest
        //    {
        //        PhoneNumber  = phone,
        //        TemplateKey  = "general_fee_success",
        //        Language     = "en",
        //        Placeholders = new System.Collections.Generic.Dictionary<string, string>
        //        {
        //            { "MemberName",  "Test Member" },
        //            { "MemberCode",  "TEST-001" },
        //            { "FeeType",     "Test Message" },
        //            { "Amount",      "0" },
        //            { "ReceiptNo",   "TEST" },
        //            { "PaidDate",    DateTime.Now.ToString("dd-MMM-yyyy") },
        //            { "PaymentMode", "Test" }
        //        }
        //    };

        //    return await _whatsApp.SendTemplatedMessageAsync(req);
        //}

        // ─── helpers ────────────────────────────────────────────────
        private static WhatsAppApiResponse BadRequest(string msg) =>
            new WhatsAppApiResponse { Success = false, Message = msg };

        private static WhatsAppApiResponse Success(string msg) =>
           new WhatsAppApiResponse { Success = true, Message = msg };
    }

}
