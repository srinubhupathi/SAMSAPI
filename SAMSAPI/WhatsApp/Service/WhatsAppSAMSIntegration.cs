using System;
using System.Configuration;
using SAMSData;
using SAMSAPI.WhatsApp.Models;

namespace SAMSAPI.WhatsApp.Service
{
    /// <summary>
    /// Integration helpers — bridge between SAMSManager DB objects and WhatsAppService.
    ///
    /// Usage inside SAMSManager (or anywhere):
    ///
    ///   // After SaveMemberFee succeeds:
    ///   WhatsAppSAMSIntegration.NotifyMembershipFeePaid(member, txn);
    ///
    ///   // After SaveSportsFee succeeds:
    ///   WhatsAppSAMSIntegration.NotifySportsFeePaid(member, sportName, amount, receiptNo, paymentMode);
    /// </summary>
    public static class WhatsAppSAMSIntegration
    {
        private static string ClubName =>
            ConfigurationManager.AppSettings["WhatsApp_ClubName"] ?? "Cosmopolitan Club";

        // ═══════════════════════════════════════════════════════════
        //  MEMBERSHIP FEE PAID
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Call this right after a membership fee transaction is saved to DB.
        /// It is fire-and-forget; errors are swallowed to not break the main flow.
        /// </summary>
        //public static void NotifyMembershipFeePaid(
        //    Member member,
        //    MembershipFeeTransaction txn)
        //{
        //    if (!IsWhatsAppEnabled()) return;
        //    if (member == null || txn == null) return;
        //    if (string.IsNullOrWhiteSpace(member.Phone)) return;

        //    try
        //    {
        //        var ctx = new WhatsAppPaymentContext
        //        {
        //            MemberName  = member.FirstName ?? "",
        //            MemberCode  = member.MemberCode ?? "",
        //            PhoneNumber = member.Phone,
        //            Amount      = (txn.MembershipFee ?? 0).ToString("N0"),
        //            FeeType     = ResolveFeeTypeName(txn.FeeType),
        //            PaidDate    = (txn.PaidDate ?? DateTime.Now).ToString("dd-MMM-yyyy"),
        //            ReceiptNo   = txn.FeeReceiptNo.ToString() ?? "-",
        //            PaymentMode = txn.PaymentMode ?? "Cash",
        //            ClubName    = ClubName,
        //            Language    = MessageLanguage.Both,
        //            TemplateKey = WhatsAppTemplateKey.membership_fee_success
        //        };

        //        // Run async — do NOT await; don't block the HTTP response
        //        System.Threading.Tasks.Task.Run(() =>
        //        {
        //            new WhatsAppService().SendPaymentSuccessMessageAsync(ctx);
        //        });
        //    }
        //    catch { /* never crash the main flow */ }
        //}

        // ═══════════════════════════════════════════════════════════
        //  SPORTS FEE PAID
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Call after a sports-fee payment is saved.
        /// </summary>
        //public static void NotifySportsFeePaid(
        //    Member member,
        //    string sportName,
        //    decimal amount,
        //    string receiptNo   = "",
        //    string paymentMode = "Cash")
        //{
        //    if (!IsWhatsAppEnabled()) return;
        //    if (member == null) return;
        //    if (string.IsNullOrWhiteSpace(member.Phone)) return;

        //    try
        //    {
        //        var ctx = new WhatsAppPaymentContext
        //        {
        //            MemberName  = member.FirstName ?? "",
        //            MemberCode  = member.MemberCode ?? "",
        //            PhoneNumber = member.Phone,
        //            Amount      = amount.ToString("N0"),
        //            FeeType     = "Sports Fee",
        //            SportName   = sportName ?? "",
        //            PaidDate    = DateTime.Now.ToString("dd-MMM-yyyy"),
        //            ReceiptNo   = receiptNo ?? "-",
        //            PaymentMode = paymentMode ?? "Cash",
        //            ClubName    = ClubName,
        //            Language    = MessageLanguage.Both,
        //            TemplateKey = WhatsAppTemplateKey.sports_fee_success
        //        };

        //        System.Threading.Tasks.Task.Run(() =>
        //        {
        //            new WhatsAppService().SendPaymentSuccessMessageAsync(ctx);
        //        });
        //    }
        //    catch { }
        //}

        // ═══════════════════════════════════════════════════════════
        //  PAYMENT REMINDER (single member)
        // ═══════════════════════════════════════════════════════════

        //public static void SendMembershipReminder(
        //    Member member,
        //    MembeFeeStatus feeStatus)
        //{
        //    if (!IsWhatsAppEnabled()) return;
        //    if (member == null || feeStatus == null) return;
        //    if (string.IsNullOrWhiteSpace(member.Phone)) return;

        //    try
        //    {
        //        var ctx = new WhatsAppReminderContext
        //        {
        //            MemberName  = member.MemberName ?? "",
        //            MemberCode  = member.MemberCode ?? "",
        //            PhoneNumber = member.Phone,
        //            DueAmount   = feeStatus.DueAmount?.ToString("N0") ?? "0",
        //            DueYear     = (feeStatus.DueFromYear + " - " + feeStatus.DueToYear),
        //            DueDate     = "31-Mar-" + feeStatus.DueToYear,
        //            FeeType     = "Membership Fee",
        //            ClubName    = ClubName,
        //            Language    = MessageLanguage.Both,
        //            TemplateKey = WhatsAppTemplateKey.membership_fee_reminder
        //        };

        //        System.Threading.Tasks.Task.Run(() =>
        //        {
        //            new WhatsAppService().SendPaymentReminderMessage(ctx);
        //        });
        //    }
        //    catch { }
        //}

        // ═══════════════════════════════════════════════════════════
        //  WELCOME MESSAGE
        // ═══════════════════════════════════════════════════════════

        //public static void SendWelcomeMessage(Member member, string membershipType)
        //{
        //    if (!IsWhatsAppEnabled()) return;
        //    if (member == null) return;
        //    if (string.IsNullOrWhiteSpace(member.Phone)) return;

        //    try
        //    {
        //        var req = new WhatsAppSendRequest
        //        {
        //            PhoneNumber  = member.Phone,
        //            TemplateKey  = WhatsAppTemplateKey.welcome_member.ToString(),
        //            Language     = "both",
        //            Placeholders = new System.Collections.Generic.Dictionary<string, string>
        //            {
        //                { "MemberName",  member.FirstName ?? "" },
        //                { "MemberCode",  member.MemberCode ?? "" },
        //                { "FeeType",     membershipType ?? "" },
        //                { "PaidDate",    DateTime.Now.ToString("dd-MMM-yyyy") }
        //            }
        //        };

        //        System.Threading.Tasks.Task.Run(() =>
        //        {
        //            new WhatsAppService().SendTemplatedMessageAsync(req);
        //        });
        //    }
        //    catch { }
        //}

        // ─── Private helpers ────────────────────────────────────────

        private static bool IsWhatsAppEnabled()
        {
            string val = ConfigurationManager.AppSettings["WhatsApp_Enabled"] ?? "true";
            return val.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveFeeTypeName(int? feeTypeCode)
        {
            switch (feeTypeCode)
            {
                case 1:  return "Membership Fee";
                case 2:  return "Entrance Fee";
                case 3:  return "Sports Fee";
                case 4:  return "Special Levy";
                default: return "Club Fee";
            }
        }
    }
}
