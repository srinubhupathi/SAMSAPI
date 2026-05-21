using Newtonsoft.Json;
using SAMSAPI.WhatsApp.Models;
using SAMSData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static System.Net.WebRequestMethods;

namespace SAMSAPI.WhatsApp.Service
{
    /// <summary>
    /// Core WhatsApp service — integrates with md.enotify.app REST API.
    ///
    /// md.enotify.app API details:
    ///   Base URL  : https://md.enotify.app
    ///   Endpoint  : POST /api/send-message
    ///   Auth      : instance_id + access_token in JSON body
    ///
    /// Required credentials (store in Web.config / appsettings):
    ///   WhatsApp_InstanceId    — from enotify.app dashboard → Devices
    ///   WhatsApp_AccessToken   — from enotify.app dashboard → API Settings
    ///   WhatsApp_ApiBaseUrl    — https://md.enotify.app  (configurable)
    ///   WhatsApp_CountryCode   — 91  (India)
    ///   WhatsApp_TemplateFile  — absolute path to whatsapp-templates.json
    /// </summary>
    public class WhatsAppService
    {
        // ── Configuration ──────────────────────────────────────────
        private readonly string _instanceId;
        private readonly string _accessToken;
        private readonly string _apiBaseUrl;
        private readonly string _countryCode;
        private readonly string _templateFilePath;
        private readonly string _qrFilePath;
      

        // ── Cached template config ──────────────────────────────────
        private static WhatsAppTemplateConfig _templateCache;
        private static DateTime _templateCacheTime = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private static readonly object _cacheLock = new object();

        // ── HTTP client (static for connection pooling) ─────────────
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public WhatsAppService()
        {
            _instanceId      = System.Configuration.ConfigurationManager.AppSettings["WhatsApp_InstanceId"]    ?? "";
            _accessToken     = System.Configuration.ConfigurationManager.AppSettings["WhatsApp_AccessToken"]   ?? "";
            _apiBaseUrl      = System.Configuration.ConfigurationManager.AppSettings["WhatsApp_ApiBaseUrl"]    ?? "";
            _countryCode     = System.Configuration.ConfigurationManager.AppSettings["WhatsApp_CountryCode"]   ?? "91";
            _templateFilePath = HttpRuntime.AppDomainAppPath + System.Configuration.ConfigurationManager.AppSettings["WhatsApp_TemplateFile"];
                               

            _qrFilePath = HttpRuntime.AppDomainAppPath + @"Content\Config\qrCode.png";
          

          
        }

     

        public async Task<WhatsAppApiResponse> SendTextMessageAsync(WhatsAppPaymentContext ctx)
        {


            // var membershipFeePaidTemplate = System.Configuration.ConfigurationManager.AppSettings["MembershipFeePaid_Template"] ?? "membership_fee_success";
            //var genericFeePaidTemplate = System.Configuration.ConfigurationManager.AppSettings["GenericFeePaid_Template"] ?? "generic_fee_success";
            // var sportsFeePaidTemplate = System.Configuration.ConfigurationManager.AppSettings["SportsFeePaid_Template"] ?? "sports_fee_success";
            //var lockerFeePaidTemplate = System.Configuration.ConfigurationManager.AppSettings["LockerFeePaid_Template"] ?? "locker_fee_success";
            //var feeReminderTemplate = System.Configuration.ConfigurationManager.AppSettings["FeeReminder_Template"] ?? "membership_fee_reminder";

           // var membershipFeePaidTemplate = "";

            if (ctx.TemplateKey == "1")
            {
                if(ctx.FeeType == "Membership")
                    ctx.TemplateKey = "membership_fee_success"; 
                else //if (ctx.FeeType == "Sports")
                    ctx.TemplateKey = "generic_fee_success"; 
                //else
                  //  ctx.TemplateKey = lockerFeePaidTemplate;
            }
            else if (ctx.TemplateKey == "2")
                ctx.TemplateKey = "membership_fee_reminder";
            else if (ctx.TemplateKey == "3")
                ctx.TemplateKey = "birthday_notification";

            var placeHolders = new Dictionary<string, string>
                    {
                        { "MemberName",  ctx.MemberName?? "" },
                        { "MemberCode",  ctx.MemberCode ?? "" },
                        { "FeeType",     ctx.FeeType ?? "" },
                        { "PaidDate",    ctx.MembershipYear ?? "" },
                         { "DueYear",    ctx.MembershipYear ?? "" },
                        { "Amount",    ctx.Amount ?? "" },
                        { "ReceiptNo",    ctx.ReceiptNo?? "" },
                        { "DueAmount",    ctx.Amount?? "" },
                        { "Period",    ctx.Period?? "" },
                        { "ClubName",    ctx.ClubName?? "" }
                    };
            var phone =  ctx.PhoneNumber;
           
            var message = BuildMessageFromPlaceholders(ctx.TemplateKey,"both", placeHolders);
        //    string message = "Hello" + ctx.MemberName + "Received your paymnet of " + ctx.Amount + " for the year " + ctx.MembershipYear;
            string encodedMessage = Uri.EscapeDataString(message);
          
            string url = _apiBaseUrl +
                         $"?token={_instanceId}" +
                         $"&phone={phone}" +
                         $"&message={encodedMessage}";

            System.Net.ServicePointManager.SecurityProtocol =
                        System.Net.SecurityProtocolType.Tls12;
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return new WhatsAppApiResponse
            {
                Success = true,
                Message = "Template message sent successfully",
                SentCount = 1
            };
        }

        public async Task SendMessageWithQrAsync(string phoneNumber)
        {
            // Change file to jpg
            string qrPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Content",
                "Config",
                "qrCode.jpg");

            if (!System.IO.File.Exists(qrPath))
                throw new FileNotFoundException(
                    "QR image not found",
                    qrPath);

            string url =
                $"https://enotify.app/api/sendFileWithCaption" +
                $"?token={_instanceId}" +
                $"&phone={phoneNumber}" +
                $"&message={Uri.EscapeDataString("Scan QR Code")}";
            System.Net.ServicePointManager.SecurityProtocol =
                     System.Net.SecurityProtocolType.Tls12;
            using (var form = new MultipartFormDataContent())
            using (var fs = System.IO.File.OpenRead(qrPath))
            using (var fileContent = new StreamContent(fs))
            {
                // Change mime type to JPG
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                // filename should match extension
                form.Add(fileContent, "file", "qrCode.jpg");

                var response = await _httpClient.PostAsync(url, form);

                string result = await response.Content.ReadAsStringAsync();

                Console.WriteLine(result); // inspect API response

                response.EnsureSuccessStatusCode();
            }
        }
        //public async Task SendMessageWithQrAsync(string phoneNumber)
        //{
        //    string url =
        //      $"https://enotify.app/api/sendFileWithCaption" +
        //      $"?token={_instanceId}" +
        //      $"&phone={phoneNumber}" +
        //      $"&message=Scan%20QR";

        //    using (var form = new MultipartFormDataContent())
        //    using (var fs = System.IO.File.OpenRead(_qrFilePath))
        //    {
        //        var fileContent = new StreamContent(fs);

        //        // Try removing this if it fails
        //        fileContent.Headers.ContentType =
        //            new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

        //        form.Add(fileContent, "file", "qrcode.png");

        //        var response = await _httpClient.PostAsync(url, form);

        //        var body = await response.Content.ReadAsStringAsync();

        //        //Console.WriteLine(body); // IMPORTANT inspect response

        //        response.EnsureSuccessStatusCode();
        //    }
        //}
        //public async Task SendMessageWithQrAsync(string phoneNumber)
        //{
        //    if (!System.IO.File.Exists(_qrFilePath))
        //        throw new FileNotFoundException(
        //            "QR file not found",
        //            _qrFilePath);

        //    // token + phone in query string (IMPORTANT)
        //    string url =
        //        $"https://enotify.app/api/sendFiles" +
        //        $"?token={Uri.EscapeDataString(_instanceId)}" +
        //        $"&phone={Uri.EscapeDataString(phoneNumber)}";



        //    using (var form = new MultipartFormDataContent())
        //    using (var fileStream = System.IO.File.OpenRead(_qrFilePath))
        //    using (var fileContent = new StreamContent(fileStream))
        //    {
        //        fileContent.Headers.ContentType =
        //            new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

        //        // ONLY file in form-data body
        //        form.Add(fileContent, "file", "QRCode.png");

        //        var response = await _httpClient.PostAsync(url, form);

        //        string result = await response.Content.ReadAsStringAsync();

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            throw new Exception(
        //                $"Enotify error: {response.StatusCode} - {result}");
        //        }
        //    }
        //}


        //public async Task SendMessageWithQrAsync(
        //string phoneNumber)
        //{
        //    if (!System.IO.File.Exists(_qrFilePath))
        //        throw new FileNotFoundException(
        //            "QR file not found",
        //            _qrFilePath);


        //    using (var form = new MultipartFormDataContent())
        //    using (var fileStream = System.IO.File.OpenRead(_qrFilePath))
        //    {
        //        form.Add(new StringContent(_instanceId), "token");
        //        form.Add(new StringContent(phoneNumber), "phone");
        //        form.Add(new StringContent("Scan QR Code"), "caption");

        //        var fileContent = new StreamContent(fileStream);
        //        fileContent.Headers.ContentType =
        //            MediaTypeHeaderValue.Parse("image/png");

        //        form.Add(fileContent, "file", "QRCode.png");

        //        var url = _apiBaseUrl.Replace("sendText", "sendFiles");
        //        var mediaResponse = await _httpClient.PostAsync(
        //          url,                
        //            form);

        //        mediaResponse.EnsureSuccessStatusCode();
        //    }
        //}




        private string BuildMessageFromPlaceholders(
            string templateKey, string langCode,
            Dictionary<string, string> placeholders)
        {
            var config = LoadTemplateConfig();
            if (!config.Templates.ContainsKey(templateKey)) return null;

            var pair = config.Templates[templateKey];
            string clubName = config.Club?.name ?? "Club";
            string clubTe   = config.Club?.name_telugu ?? "క్లబ్";

            // Inject club name if not supplied
            if (!placeholders.ContainsKey("ClubName"))
                placeholders["ClubName"] = clubName;

            MessageLanguage lang = langCode == "te" ? MessageLanguage.Telugu
                                 : langCode == "en" ? MessageLanguage.English
                                 : MessageLanguage.Both;

            return AssembleMessage(pair, lang, placeholders,
                                   config.Settings, clubName, clubTe);
        }

        /// <summary>
        /// Combines English and/or Telugu template bodies with placeholder substitution.
        /// </summary>
        private string AssembleMessage(
            TemplatePair pair,
            MessageLanguage lang,
            Dictionary<string, string> ph,
            TemplateSettings settings,
            string clubNameEn, string clubNameTe)
        {
            string separator = settings?.language_separator ?? "\n\n─────────────\n\n";
            bool both = lang == MessageLanguage.Both
                     || (settings?.send_both_languages == true && lang == MessageLanguage.Both);

            string enMsg = null, teMsg = null;

            if (lang == MessageLanguage.English || lang == MessageLanguage.Both)
                enMsg = FillPlaceholders(pair.en?.body ?? "", ph);

            if (lang == MessageLanguage.Telugu || lang == MessageLanguage.Both)
            {
                // Replace ClubName with Telugu club name in Telugu body
                var tePh = new Dictionary<string, string>(ph, StringComparer.OrdinalIgnoreCase)
                {
                    ["ClubName"] = clubNameTe
                };
                teMsg = FillPlaceholders(pair.te?.body ?? "", tePh);
            }

            if (lang == MessageLanguage.Both && enMsg != null && teMsg != null)
                return enMsg + separator + teMsg;

            return enMsg ?? teMsg ?? "";
        }

        private string FillPlaceholders(string template, Dictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(template)) return template;
            foreach (var kv in values)
                template = template.Replace("{" + kv.Key + "}", kv.Value ?? "");
            return template;
        }

      
        private WhatsAppTemplateConfig LoadTemplateConfig()
        {
            lock (_cacheLock)
            {
                if (_templateCache != null
                    && DateTime.Now - _templateCacheTime < CacheDuration)
                    return _templateCache;

                try
                {
                    string json = System.IO.File.ReadAllText(_templateFilePath, Encoding.UTF8);
                    _templateCache = JsonConvert.DeserializeObject<WhatsAppTemplateConfig>(json);
                    _templateCacheTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    // Return empty config so callers don't crash
                    _templateCache = new WhatsAppTemplateConfig
                    {
                        Club      = new ClubInfo { name = "Club" },
                        Templates = new Dictionary<string, TemplatePair>(),
                        Settings  = new TemplateSettings
                        {
                            default_language    = "en",
                            send_both_languages = true,
                            language_separator  = "\n\n─────────────────\n\n",
                            retry_on_failure    = true,
                            max_retries         = 3,
                            retry_delay_seconds = 5
                        }
                    };
                }

                return _templateCache;
            }
        }

        /// <summary>Force-reload templates from disk (useful after admin edits the JSON).</summary>
        public void ReloadTemplates()
        {
            lock (_cacheLock)
            {
                _templateCache = null;
                _templateCacheTime = DateTime.MinValue;
            }
            LoadTemplateConfig();
        }



        // ═══════════════════════════════════════════════════════════
        //  PUBLIC HIGH-LEVEL METHODS  (called from SAMSManager or Controller)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Send payment-success WhatsApp message after fee is saved.
        /// Triggered automatically from SAMSManager.SaveMemberFee / SaveSportsFee.
        /// </summary>
        //public async Task<WhatsAppApiResponse> SendPaymentSuccessMessageAsync(WhatsAppPaymentContext ctx)
        //{
        //    try
        //    {
        //        string messageBody = BuildPaymentSuccessMessage(ctx);
        //        if (string.IsNullOrWhiteSpace(messageBody))
        //            return Error("Template not found for key: " + ctx.TemplateKey);

        //        string phone = NormalisePhone(ctx.PhoneNumber);
        //        if (string.IsNullOrWhiteSpace(phone))
        //            return Error("Invalid phone number: " + ctx.PhoneNumber);

        //        var result = await SendMessageAsync(phone, messageBody);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return Error("SendPaymentSuccessMessage error: " + ex.Message);
        //    }
        //}

        /// <summary>
        /// Send a payment-reminder WhatsApp message to a single member.
        /// </summary>
        //public async Task<WhatsAppApiResponse> SendPaymentReminderMessageAsync(WhatsAppReminderContext ctx)
        //{
        //    try
        //    {
        //        string messageBody = BuildReminderMessage(ctx);
        //        if (string.IsNullOrWhiteSpace(messageBody))
        //            return Error("Template not found for key: " + ctx.TemplateKey);

        //        string phone = NormalisePhone(ctx.PhoneNumber);
        //        if (string.IsNullOrWhiteSpace(phone))
        //            return Error("Invalid phone number: " + ctx.PhoneNumber);

        //        return await SendMessageAsync(phone, messageBody);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Error("SendPaymentReminderMessage error: " + ex.Message);
        //    }
        //}

        /// <summary>
        /// Send a message using raw placeholders dictionary  
        /// (used by WhatsAppController for direct Angular calls).
        /// </summary>
        //public async Task<WhatsAppApiResponse> SendTemplatedMessageAsync(WhatsAppSendRequest request)
        //{
        //    try
        //    {
        //        string phone = NormalisePhone(request.PhoneNumber);
        //        if (string.IsNullOrWhiteSpace(phone))
        //            return Error("Invalid phone number: " + request.PhoneNumber);

        //        string langCode = (request.Language ?? "both").ToLower();
        //        string messageBody = BuildMessageFromPlaceholders(
        //            request.TemplateKey, langCode, request.Placeholders);

        //        if (string.IsNullOrWhiteSpace(messageBody))
        //            return Error("Could not build message for template: " + request.TemplateKey);

        //        return await SendMessageAsync(phone, messageBody);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Error("SendTemplatedMessage error: " + ex.Message);
        //    }
        //}

        /// <summary>
        /// Send bulk payment reminders to a list of members.
        /// Returns aggregated success/fail counts.
        /// </summary>
        //public async Task<WhatsAppApiResponse> SendBulkRemindersAsync(
        //    List<WhatsAppReminderContext> members)
        //{
        //    var response = new WhatsAppApiResponse();

        //    foreach (var ctx in members)
        //    {
        //        var result = await SendPaymentReminderMessageAsync(ctx);
        //        if (result.Success)
        //        {
        //            response.SentCount++;
        //        }
        //        else
        //        {
        //            response.FailedCount++;
        //        }

        //        // Add per-member detail
        //        response.Details.Add(new WhatsAppDeliveryDetail
        //        {
        //            MemberCode  = ctx.MemberCode,
        //            MemberName  = ctx.MemberName,
        //            PhoneNumber = ctx.PhoneNumber,
        //            Sent        = result.Success,
        //            Error       = result.Message,
        //            SentAt      = DateTime.Now
        //        });

        //        // Throttle — be polite to the API (200 ms between messages)
        //        if (members.Count > 1)
        //            Thread.Sleep(200);
        //    }

        //    response.Success = response.FailedCount == 0;
        //    response.Message = $"Sent: {response.SentCount}, Failed: {response.FailedCount}";
        //    return response;
        //}

        // ═══════════════════════════════════════════════════════════
        //  CORE HTTP SEND METHOD
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Calls md.enotify.app  POST /api/send-message
        /// Retries up to max_retries times on failure.
        /// </summary>
        //private async Task<WhatsAppApiResponse> SendMessageAsync(string normalisedPhone, string messageBody)
        //{
        //    var cfg     = LoadTemplateConfig();
        //    int maxRetry = cfg?.Settings?.max_retries ?? 3;
        //    int delayMs  = (cfg?.Settings?.retry_delay_seconds ?? 5) * 1000;




        //    var client = new HttpClient();

        //    var request = new HttpRequestMessage(
        //        HttpMethod.Post,
        //        "https://api.interakt.ai/v1/public/message/"
        //    );
        //    request.Headers.Add("Authorization", "Basic czRnb0RmR1QyQ0pQZ2M5VmJyUGZXV0Q1X3RJLUR5NWpTalAyc2FBVjhVQTo=");

        //    var content = new StringContent("{\r\n    \r\n    \"userId\": \"\",\r\n    \"fullPhoneNumber\":\"919949688844\",\r\n    \"callbackData\": \"some_callback_data\",\r\n    \"type\": \"Text\",\r\n    \"data\": {\r\n        \"message\": \"This msg is sent via API\"\r\n    }\r\n}", null, "application/json");
        //    request.Content = content;
        //    var response = await client.SendAsync(request);
        //    response.EnsureSuccessStatusCode();

        //    return new WhatsAppApiResponse
        //    {
        //        Success = true,
        //        Message = "Message sent successfully",
        //        SentCount = 1
        //    };


        //}


        //        public async Task<WhatsAppApiResponse> SendPaymentReminderTemplateAsync(
        //WhatsAppPaymentContext ctx
        //)
        //        {
        //            try
        //            {
        //                //  var url = "https://api.interakt.ai/v1/public/message/";
        //                System.Net.ServicePointManager.SecurityProtocol =
        //                            System.Net.SecurityProtocolType.Tls12;
        //                var client = new HttpClient();
        //                client.Timeout = TimeSpan.FromSeconds(30);

        //                var request = new HttpRequestMessage(HttpMethod.Post, _apiBaseUrl);

        //                // request.Headers.Add("Authorization", "Basic czRnb0RmR1QyQ0pQZ2M5VmJyUGZXV0Q1X3RJLUR5NWpTalAyc2FBVjhVQTo=");
        //                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _accessToken);

        //                var template = System.Configuration.ConfigurationManager.AppSettings["FeeReminder_Template"] ?? "fee_reminder_notification";

        //                var payload = new
        //                {
        //                    fullPhoneNumber = ctx.PhoneNumber,
        //                    callbackData = "payment_reminder",
        //                    type = "Template",
        //                    template = new
        //                    {
        //                        name = template,
        //                        languageCode = ctx.Language,
        //                        bodyValues = new string[]
        //                        {
        //                            ctx.MemberName,
        //                            ctx.Amount,
        //                            ctx.FeeType,
        //                           ctx.MembershipYear
        //                        }
        //                    }
        //                };

        //                // var json = System.Text.Json.JsonSerializer.Serialize(payload);
        //                var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
        //                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        //                var response = await client.SendAsync(request);
        //                var responseContent = await response.Content.ReadAsStringAsync();

        //                if (!response.IsSuccessStatusCode)
        //                {
        //                    return new WhatsAppApiResponse
        //                    {
        //                        Success = false,
        //                        Message = responseContent
        //                    };
        //                }

        //            }
        //            catch (
        //            Exception ex)
        //            {
        //                Console.WriteLine(ex.InnerException.ToString());
        //            }
        //            return new WhatsAppApiResponse
        //            {
        //                Success = true,
        //                Message = "Template message sent successfully",
        //                SentCount = 1
        //            };
        //        }


        //        public async Task<WhatsAppApiResponse> SendPaymentTemplateAsync(
        //   WhatsAppPaymentContext ctx
        //)
        //        {
        //            try
        //            {
        //              //  var url = "https://api.interakt.ai/v1/public/message/";
        //                System.Net.ServicePointManager.SecurityProtocol =
        //                            System.Net.SecurityProtocolType.Tls12;
        //                var client = new HttpClient();
        //                client.Timeout = TimeSpan.FromSeconds(30);

        //                var request = new HttpRequestMessage(HttpMethod.Post, _apiBaseUrl);

        //               // request.Headers.Add("Authorization", "Basic czRnb0RmR1QyQ0pQZ2M5VmJyUGZXV0Q1X3RJLUR5NWpTalAyc2FBVjhVQTo=");
        //                request.Headers.Authorization =  new AuthenticationHeaderValue("Basic", _accessToken);

        //                var template = System.Configuration.ConfigurationManager.AppSettings["FeePaid_Template"] ?? "fee_paid_notification";

        //                var payload = new
        //                {
        //                    fullPhoneNumber = ctx.PhoneNumber,
        //                    callbackData = "payment_notification",
        //                    type = "Template",
        //                    template = new
        //                    {
        //                        name = template, 
        //                        languageCode = ctx.Language,
        //                        bodyValues = new string[]
        //                        {
        //                            ctx.MemberName,
        //                            ctx.Amount,
        //                            ctx.FeeType,
        //                           ctx.MembershipYear
        //                        }
        //                    }
        //                };

        //                // var json = System.Text.Json.JsonSerializer.Serialize(payload);
        //                var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
        //                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        //                var response = await client.SendAsync(request);
        //                var responseContent = await response.Content.ReadAsStringAsync();

        //                if (!response.IsSuccessStatusCode)
        //                {
        //                    return new WhatsAppApiResponse
        //                    {
        //                        Success = false,
        //                        Message = responseContent
        //                    };
        //                }

        //            }
        //            catch (
        //            Exception ex)
        //            {
        //                Console.WriteLine(ex.InnerException.ToString());
        //            }
        //            return new WhatsAppApiResponse
        //            {
        //                Success = true,
        //                Message = "Template message sent successfully",
        //                SentCount = 1
        //            };
        //        }

        // ═══════════════════════════════════════════════════════════
        //  UTILITY
        // ═══════════════════════════════════════════════════════════
        // ═══════════════════════════════════════════════════════════
        //  PHONE NUMBER NORMALISATION
        // ═══════════════════════════════════════════════════════════

        //private string NormalisePhone(string phone)
        //{
        //    if (string.IsNullOrWhiteSpace(phone)) return null;

        //    // Strip all non-digits
        //    string digits = System.Text.RegularExpressions.Regex.Replace(phone, @"\D", "");

        //    if (digits.Length == 0) return null;

        //    // If starts with country code already (e.g. 919XXXXXXXXX)
        //    if (digits.Length == 12 && digits.StartsWith(_countryCode))
        //        return digits;

        //    // If 10-digit Indian number — prepend country code
        //    if (digits.Length == 10)
        //        return _countryCode + digits;

        //    // If already has + prefix stripped (e.g. 919246668725)
        //    if (digits.Length == 11 || digits.Length == 12)
        //        return digits;

        //    return digits;
        //}


        //        public async Task<WhatsAppApiResponse> SendPaymentTemplateAsync1(
        //  WhatsAppPaymentContext ctx
        //)
        //        {
        //            try
        //            {
        //                 var url = "https://graph.facebook.com/v19.0/919246668725/messages";
        //                System.Net.ServicePointManager.SecurityProtocol =
        //                            System.Net.SecurityProtocolType.Tls12;
        //                var client = new HttpClient();
        //                client.Timeout = TimeSpan.FromSeconds(30);

        //                var request = new HttpRequestMessage(HttpMethod.Post, url);

        //                // request.Headers.Add("Authorization", "Basic czRnb0RmR1QyQ0pQZ2M5VmJyUGZXV0Q1X3RJLUR5NWpTalAyc2FBVjhVQTo=");
        //                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _accessToken);

        //                var template = System.Configuration.ConfigurationManager.AppSettings["FeePaid_Template"] ?? "fee_paid_notification";

        //                var payload = new
        //                {
        //                    fullPhoneNumber = ctx.PhoneNumber,
        //                    callbackData = "payment_notification",
        //                    type = "Template",
        //                    template = new
        //                    {
        //                        name = template,
        //                        languageCode = ctx.Language,
        //                        bodyValues = new string[]
        //                        {
        //                            ctx.MemberName,
        //                            ctx.Amount,
        //                            ctx.FeeType,
        //                           ctx.MembershipYear
        //                        }
        //                    }
        //                };

        //                // var json = System.Text.Json.JsonSerializer.Serialize(payload);
        //                var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
        //                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        //                var response = await client.SendAsync(request);
        //                var responseContent = await response.Content.ReadAsStringAsync();

        //                if (!response.IsSuccessStatusCode)
        //                {
        //                    return new WhatsAppApiResponse
        //                    {
        //                        Success = false,
        //                        Message = responseContent
        //                    };
        //                }

        //            }
        //            catch (
        //            Exception ex)
        //            {
        //                Console.WriteLine(ex.InnerException.ToString());
        //            }
        //            return new WhatsAppApiResponse
        //            {
        //                Success = true,
        //                Message = "Template message sent successfully",
        //                SentCount = 1
        //            };
        //        }
        // ═══════════════════════════════════════════════════════════
        //  MESSAGE BUILDING HELPERS
        // ═══════════════════════════════════════════════════════════

        //private string BuildPaymentSuccessMessage(WhatsAppPaymentContext ctx)
        //{
        //    var config = LoadTemplateConfig();
        //    string key = ctx.TemplateKey.ToString();

        //    if (!config.Templates.ContainsKey(key)) return null;

        //    var pair = config.Templates[key];
        //    string clubName = config.Club?.name ?? "Club";
        //    string clubTe   = config.Club?.name_telugu ?? "క్లబ్";

        //    var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        //    {
        //        { "MemberName",    ctx.MemberName   ?? "" },
        //        { "MemberCode",    ctx.MemberCode   ?? "" },
        //        { "Amount",        ctx.Amount       ?? "" },
        //        { "FeeType",       ctx.FeeType      ?? "" },
        //        //{ "SportName",     ctx.SportName    ?? "" },
        //        { "PaidDate",      ctx.PaidDate     ?? "" },
        //        { "ReceiptNo",     ctx.ReceiptNo    ?? "" },
        //        { "PaymentMode",   ctx.PaymentMode  ?? "" },
        //        { "ClubName",      clubName }
        //    };

        //    return AssembleMessage(pair, ctx.Language, placeholders,
        //                           config.Settings, clubName, clubTe);
        //}

        //private string BuildReminderMessage(WhatsAppReminderContext ctx)
        //{
        //    var config = LoadTemplateConfig();
        //    string key = ctx.TemplateKey.ToString();

        //    if (!config.Templates.ContainsKey(key)) return null;

        //    var pair    = config.Templates[key];
        //    string clubName = config.Club?.name ?? "Club";
        //    string clubTe   = config.Club?.name_telugu ?? "క్లబ్";

        //    var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        //    {
        //        { "MemberName",  ctx.MemberName  ?? "" },
        //        { "MemberCode",  ctx.MemberCode  ?? "" },
        //        { "DueAmount",   ctx.DueAmount   ?? "" },
        //        { "DueYear",     ctx.DueYear     ?? "" },
        //        { "DueDate",     ctx.DueDate     ?? "" },
        //        { "FeeType",     ctx.FeeType     ?? "" },
        //        { "SportName",   ctx.SportName   ?? "" },
        //        { "ClubName",    clubName }
        //    };

        //    return AssembleMessage(pair, ctx.Language, placeholders,
        //                           config.Settings, clubName, clubTe);
        //}

        // ═══════════════════════════════════════════════════════════
        //  TEMPLATE CONFIG LOADER (with cache)
        // ═══════════════════════════════════════════════════════════

        //private static WhatsAppApiResponse Error(string msg) =>
        //    new WhatsAppApiResponse { Success = false, Message = msg, FailedCount = 1 };
    }
}
