using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net;
using System.Web;

namespace SAMSAPI.Areas.Common
{
    public class SendEmail
    {
        public string errorMessage { get; set; }
        /// <summary>
        /// Sends Email
        /// </summary>
        /// <param name="emailTo"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        //public bool SendMail(string emailTo, string subject, string body)
        //{
        //    try
        //    {
        //        var mailFromEmail = ConfigurationManager.AppSettings["FromEmail"].ToString();
        //        var mailFromFullName = ConfigurationManager.AppSettings["FromName"].ToString();
        //        var smtpServer = ConfigurationManager.AppSettings["SMTPServer"].ToString();
        //        var smtpEmail = ConfigurationManager.AppSettings["SMTPEmail"].ToString();
        //        var smtpUserName = ConfigurationManager.AppSettings["SMTPUserName"].ToString();
        //        var smtpPassword = ConfigurationManager.AppSettings["SMTPPassword"].ToString();
        //        var smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"].ToString());
        //        MimeMessage mailMsg = new MimeMessage();

        //        mailMsg.To.Add(new MailboxAddress(emailTo, emailTo));
        //        // From
        //        mailMsg.From.Add(new MailboxAddress(mailFromFullName, mailFromEmail));

        //        // Subject and multipart/alternative Body
        //        mailMsg.Subject = subject;


        //        var builder = new BodyBuilder();
        //        builder.HtmlBody = string.Format(body);

        //        mailMsg.Body = builder.ToMessageBody();

        //        HeaderId[] headersToSign = new HeaderId[] { HeaderId.From, HeaderId.Subject, HeaderId.Date };
        //        string domain = "bradken.com";
        //        string selector = "ewit";
        //        string path = System.Web.Hosting.HostingEnvironment.MapPath("/Uploads/my-dkim-key.pem");
        //        DkimSigner signer = new DkimSigner(path, domain, selector)
        //        {
        //            SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha1,
        //            AgentOrUserIdentifier = "@bradken.com",
        //            QueryMethod = "dns/txt",
        //        };

        //        mailMsg.Sign(signer, headersToSign, DkimCanonicalizationAlgorithm.Relaxed, DkimCanonicalizationAlgorithm.Simple);

        //        System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(smtpUserName, smtpPassword);


        //        // Init SmtpClient and send
        //        using (var client = new MailKit.Net.Smtp.SmtpClient())
        //        {
        //            client.Connect(smtpServer, smtpPort, SecureSocketOptions.None);
        //            client.AuthenticationMechanisms.Remove("XOAUTH2");
        //            client.Authenticate(credentials);
        //            client.Send(mailMsg);
        //            client.Disconnect(true);
        //        }


        //        //using (MailMessage mail = new MailMessage())
        //        //{

        //        //    mail.From = new MailAddress(mailFromEmail, mailFromFullName);
        //        //    mail.To.Add(emailTo);
        //        //    mail.Subject = subject;
        //        //    mail.Body = body;
        //        //    mail.IsBodyHtml = true;
        //        //    //using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
        //        //    //{
        //        //    //    // smtp.Credentials = new NetworkCredential(smtpEmail, smtpPassword);
        //        //    //    smtp.EnableSsl = false;
        //        //    //    smtp.Send(mail);
        //        //    //}

        //        //    //using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
        //        //    //{
        //        //    //    smtp.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
        //        //    //    smtp.EnableSsl = true;
        //        //    //    smtp.Send(mail);
        //        //    //}
        //        //    System.Net.NetworkCredential myCred = new System.Net.NetworkCredential(smtpUserName, smtpPassword);
        //        //    System.Net.CredentialCache myCache = new System.Net.CredentialCache();
        //        //    if (myCred != null)
        //        //    {
        //        //        myCache.Add(smtpServer, smtpPort, "Digest", myCred);
        //        //        myCache.Add(smtpServer, smtpPort, "Cram-MD5", myCred);
        //        //        //myCache.Add(smtpServer, smtpPort, "Cram-MD5", myCred);
        //        //        //myCache.Add(smtpServer, smtpPort, "Login", myCred);
        //        //        // myCache.Add(Utils.Settings.SMTPHost, Utils.Settings.SMTPPort, "NTLM", myCred);
        //        //    }
        //        //    using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
        //        //    {
        //        //        smtp.EnableSsl = false;
        //        //        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        //        smtp.UseDefaultCredentials = false;
        //        //        smtp.Credentials = myCache;
        //        //        smtp.Send(mail);
        //        //    }



        //        //}



        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //        //errorMessage = ex.Message;

        //        //return false;
        //    }

        //}//eo SendMail();

        //public bool SendGMail(string emailTo, string subject, string body)
        //{
        //    try
        //    {
        //        MimeMessage mailMsg = new MimeMessage();

        //        //TESTING ENVIRONMENT ONLY!!!
        //        mailMsg.To.Add(new MailboxAddress("RK B", "ramakrishna.vvv@gmail.com"));

        //        // From
        //        mailMsg.From.Add(new MailboxAddress("RK B", "ramakrishna.vvv@gmail.com"));

        //        // Subject and multipart/alternative Body
        //        mailMsg.Subject = "DKIM test Email";
        //        string html = "Test Email";

        //        HeaderId[] headersToSign = new HeaderId[] { HeaderId.From, HeaderId.Subject, HeaderId.Date };
        //        string domain = "bradken.com";
        //        string selector = "ewit";
        //        string path = System.Web.Hosting.HostingEnvironment.MapPath("/Uploads/my-dkim-key.pem");
        //        DkimSigner signer = new DkimSigner(path, domain, selector)
        //        {
        //            SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha1,
        //            AgentOrUserIdentifier = "@bradken.com",
        //            QueryMethod = "dns/txt",
        //        };

        //        mailMsg.Sign(signer, headersToSign, DkimCanonicalizationAlgorithm.Relaxed, DkimCanonicalizationAlgorithm.Simple);

        //        System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("ramakrishna.vvv@gmail.com", "Vignesh@444");


        //        // Init SmtpClient and send
        //        using (var client = new MailKit.Net.Smtp.SmtpClient())
        //        {
        //            client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        //            client.AuthenticationMechanisms.Remove("XOAUTH2");
        //            client.Authenticate(credentials);
        //            client.Send(mailMsg);
        //            client.Disconnect(true);
        //        }



        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        errorMessage = ex.Message;
        //        throw ex;
        //        //return false;
        //    }
        //}

        //public bool SendBMail(string emailTo, string subject, string body)
        //{
        //    try
        //    {
        //        var mailFromEmail = ConfigurationManager.AppSettings["FromEmail"].ToString();
        //        var mailFromFullName = ConfigurationManager.AppSettings["FromName"].ToString();
        //        var smtpServer = ConfigurationManager.AppSettings["SMTPServer"].ToString();
        //        var smtpEmail = ConfigurationManager.AppSettings["SMTPEmail"].ToString();
        //        var smtpUserName = ConfigurationManager.AppSettings["SMTPUserName"].ToString();
        //        var smtpPassword = ConfigurationManager.AppSettings["SMTPPassword"].ToString();
        //        var smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"].ToString());
        //        MimeMessage mailMsg = new MimeMessage();

        //        mailMsg.To.Add(new MailboxAddress(emailTo, emailTo));
        //        // From
        //        mailMsg.From.Add(new MailboxAddress(mailFromFullName, mailFromEmail));

        //        // Subject and multipart/alternative Body
        //        mailMsg.Subject = subject;


        //        var builder = new BodyBuilder();
        //        builder.HtmlBody = string.Format(body);

        //        mailMsg.Body = builder.ToMessageBody();

        //        HeaderId[] headersToSign = new HeaderId[] { HeaderId.From, HeaderId.Subject, HeaderId.Date };
        //        string domain = "bradken.com";
        //        string selector = "ewit";
        //        string path = System.Web.Hosting.HostingEnvironment.MapPath("/Uploads/my-dkim-key.pem");
        //        DkimSigner signer = new DkimSigner(path, domain, selector)
        //        {
        //            SignatureAlgorithm = DkimSignatureAlgorithm.RsaSha1,
        //            AgentOrUserIdentifier = "@bradken.com",
        //            QueryMethod = "dns/txt",
        //        };

        //        mailMsg.Sign(signer, headersToSign, DkimCanonicalizationAlgorithm.Relaxed, DkimCanonicalizationAlgorithm.Simple);

        //        System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("EWIT", "$WITm@!lR3laY");


        //        // Init SmtpClient and send
        //        using (var client = new MailKit.Net.Smtp.SmtpClient())
        //        {
        //            client.Connect(smtpServer, smtpPort, SecureSocketOptions.None);
        //            client.AuthenticationMechanisms.Remove("XOAUTH2");
        //            client.Authenticate(credentials);
        //            client.Send(mailMsg);
        //            client.Disconnect(true);
        //        }


        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //        //errorMessage = ex.Message;

        //        //return false;
        //    }

        //}


        //public bool sendInviteToUser(UsersModel user, string baseUrl, string accessToken)
        //{
        //    string body = Resources.AppMessages.USER_INVITE_EMAIL_BODY;
        //    string subject = Resources.AppMessages.USER_INVITE_EMAIL_SUBJECT;
        //    var logoUrl = ConfigurationManager.AppSettings["logoUrl"].ToString();
        //    var resetPasswordLink = ConfigurationManager.AppSettings["ResetPasswordLink"].ToString();
        //    baseUrl = ConfigurationManager.AppSettings["baseUrl"].ToString();
        //    logoUrl = baseUrl + "/" + logoUrl;
        //    baseUrl = baseUrl + "/" + resetPasswordLink + "?token=" + HttpUtility.UrlEncode(accessToken);
        //    body = body.Replace("{logoUrl}", logoUrl);
        //    body = body.Replace("{UserName}", user.FirstName + " " + user.LastName);
        //    body = body.Replace("{Url}", baseUrl);
        //    string strError = "";

        //    try
        //    {
        //        SendMail(user.Email, subject, body);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    try
        //    {
        //        //SendGMail(user.Email, subject, body);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //    return true;
        //}

        /// <summary>
        /// Send email for forgot password
        /// </summary>
        /// <param name="Users"></param>
        /// <param name="baseUrl"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        //public bool ForgotPasswordSendEmail(UsersModel user, string baseUrl, string accessToken)
        //{
        //    string body = Resources.AppMessages.USER_FORGOT_PASSWORD_BODY;
        //    string subject = Resources.AppMessages.USER_FORGOT_PASSWORD_SUBJECT;
        //    var logoUrl = ConfigurationManager.AppSettings["logoUrl"].ToString();
        //    var resetPasswordLink = ConfigurationManager.AppSettings["ResetPasswordLink"].ToString();
        //    baseUrl = ConfigurationManager.AppSettings["baseUrl"].ToString();
        //    logoUrl = baseUrl + "/" + logoUrl;
        //    baseUrl = baseUrl + "/" + resetPasswordLink + "?token=" + HttpUtility.UrlEncode(accessToken);
        //    //baseUrl = baseUrl + "/" + resetPasswordLink + "?token" + HttpUtility.UrlEncode(accessToken);
        //    body = body.Replace("{logoUrl}", logoUrl);
        //    body = body.Replace("{UserName}", user.FirstName + " " + user.LastName);
        //    body = body.Replace("{Url}", baseUrl);

        //    SendMail(user.Email, subject, body);
        //    return true;
        //}

    }
}