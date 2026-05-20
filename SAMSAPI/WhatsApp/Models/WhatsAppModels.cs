using System;
using System.Collections.Generic;

namespace SAMSAPI.WhatsApp.Models
{
    // ─────────────────────────────────────────────────────────────
    //  Enumerations
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Language preference for WhatsApp messages
    /// </summary>
    public enum MessageLanguage
    {
        English = 0,
        Telugu = 1,
        Both = 2      // sends English + Telugu in one message
    }

    /// <summary>
    /// Type of fee / event that triggered the message
    /// </summary>
    public enum FeeMessageType
    {
        MembershipFee,
        SportsFee,
        EntranceFee,
        SpecialFee,
        General
    }

    /// <summary>
    /// Template key that maps to whatsapp-templates.json
    /// </summary>
    public enum WhatsAppTemplateKey
    {
        membership_fee_success,
        membership_fee_reminder,
        sports_fee_success,
        sports_fee_reminder,
        general_fee_success,
        bulk_reminder,
        welcome_member
    }

    // ─────────────────────────────────────────────────────────────
    //  md.enotify.app API  — Request / Response models
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Payload sent to md.enotify.app  POST /api/send-message
    /// Field names match the provider's documented JSON contract.
    /// </summary>
    public class ENotifyMessageRequest
    {
        /// <summary>Instance ID / Device ID from md.enotify.app dashboard</summary>
        public string instance_id { get; set; }

        /// <summary>API Access Token from md.enotify.app dashboard</summary>
        public string access_token { get; set; }

        /// <summary>Recipient WhatsApp number  — country code + number, e.g. "919246668725"</summary>
        public string number { get; set; }

        /// <summary>Plain-text or WhatsApp-markdown message body</summary>
        public string message { get; set; }
    }

    /// <summary>
    /// Response returned by md.enotify.app
    /// </summary>
    public class ENotifyMessageResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public string data { get; set; }
        public string error { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    //  Internal service-layer models
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// All context variables needed to build a payment-success WhatsApp message.
    /// Populate from your MembershipFeeTransaction / SAMSManager result.
    /// </summary>
    public class WhatsAppPaymentContext
    {
        public string MemberName    { get; set; }
        public string MemberCode    { get; set; }
        public string PhoneNumber   { get; set; }   // mobile number of the member
        public string Amount        { get; set; }   // e.g. "5000"
        public string FeeType       { get; set; }   // e.g. "Membership Fee", "Sports Fee"
        public string PaidDate      { get; set; }   // e.g. "26-Mar-2026"
        public string ReceiptNo     { get; set; }
        public string PaymentMode   { get; set; }   // Cash / Card / Online
        public string ClubName      { get; set; }
        public string Language { get; set; }
        public string TemplateKey { get; set; }
        public string MembershipYear { get; set; }
        public string Period{ get; set; }
    }


    public class InputToken {
        public string token { get; set; }
        public string phone { get; set; }
        public string message { get; set; }

    }

    public class WhatsBizButton
    {
        public string id { get; set; }
        public string title { get; set; }
    }

    public class WhatsBizSendMessageRequest
    {
        public string token { get; set; }
        public string phone { get; set; }
        public string message { get; set; }

        public string header { get; set; }
        public string footer { get; set; }

        public List<WhatsBizButton> buttons { get; set; }
    }


    /// <summary>
    /// All context variables for a payment-reminder WhatsApp message.
    /// </summary>
    public class WhatsAppReminderContext
    {
        public int    MemberId      { get; set; }   // optional — for reference
        public string MemberName    { get; set; }
        public string MemberCode    { get; set; }
        public string PhoneNumber   { get; set; }
        public string DueAmount     { get; set; }   // e.g. "5000"
        public string DueYear       { get; set; }   // e.g. "2025-2026"
        public string DueDate       { get; set; }   // e.g. "31-Mar-2026"
        public string FeeType       { get; set; }   // e.g. "Membership Fee"
        public string SportName     { get; set; }
        public string ClubName      { get; set; }
        public MessageLanguage Language { get; set; } = MessageLanguage.Both;
        public WhatsAppTemplateKey TemplateKey { get; set; } = WhatsAppTemplateKey.membership_fee_reminder;
    }

    /// <summary>
    /// Generic single message request (used by the Angular front-end API endpoints).
    /// </summary>
    public class WhatsAppSendRequest
    {
        public string PhoneNumber   { get; set; }
        public string TemplateKey   { get; set; }   // maps to WhatsAppTemplateKey enum
        public string Language      { get; set; }   // "en" | "te" | "both"

        // Placeholder values — Angular sends these as key-value pairs
        public Dictionary<string, string> Placeholders { get; set; }
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public class PaymentNotificationRequest
    {
        public int MemberId { get; set; }
        public int FeeTransactionId { get; set; }   
        public int FeeType { get; set; }
        public int NotificationType { get; set; }
     
    }

    /// <summary>
    /// Bulk reminder request — Angular passes a list of member IDs.
    /// </summary>
    public class WhatsAppBulkReminderRequest
    {
        public List<int> MemberIds    { get; set; } = new List<int>();
        public string FeeType         { get; set; }   // "Membership" | "Sports"
        public string Language        { get; set; }   // "en" | "te" | "both"
        public string CustomMessage   { get; set; }   // optional override text
    }

    /// <summary>
    /// Unified result returned from the API to Angular.
    /// </summary>
    public class WhatsAppApiResponse
    {
        public bool   Success       { get; set; }
        public string Message       { get; set; }
        public int    SentCount     { get; set; }
        public int    FailedCount   { get; set; }
        public List<WhatsAppDeliveryDetail> Details { get; set; }
            = new List<WhatsAppDeliveryDetail>();
    }

    /// <summary>
    /// Per-member delivery result in a bulk operation.
    /// </summary>
    public class WhatsAppDeliveryDetail
    {
        public int    MemberId    { get; set; }
        public string MemberCode  { get; set; }
        public string MemberName  { get; set; }
        public string PhoneNumber { get; set; }
        public bool   Sent        { get; set; }
        public string Error       { get; set; }
        public DateTime SentAt    { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    //  Template JSON binding models (deserialised from
    //  Content/Config/whatsapp-templates.json)
    // ─────────────────────────────────────────────────────────────

    public class WhatsAppTemplateConfig
    {
        public ClubInfo Club { get; set; }
        public Dictionary<string, TemplatePair> Templates { get; set; }
        public TemplateSettings Settings { get; set; }
    }

    public class ClubInfo
    {
        public string name         { get; set; }
        public string name_telugu  { get; set; }
    }

    public class TemplatePair
    {
        public TemplateBody en { get; set; }
        public TemplateBody te { get; set; }
    }

    public class TemplateBody
    {
        public string subject { get; set; }
        public string body    { get; set; }
    }

    public class TemplateSettings
    {
        public string default_language      { get; set; }
        public bool   send_both_languages   { get; set; }
        public string language_separator    { get; set; }
        public bool   retry_on_failure      { get; set; }
        public int    max_retries           { get; set; }
        public int    retry_delay_seconds   { get; set; }
    }
}
