using SAMSAPI.Models.Bookings;
using SAMSAPI.Models.Dashboard;
using SAMSAPI.Models.Membership;
using SAMSAPI.WhatsApp.Models;
using SAMSAPI.WhatsApp.Service;
using SAMSData;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Web.Http.ModelBinding;

namespace SAMSAPI.Manager
{
    class SAMSManager
    {

        SAMSData.SAMSEntities se = new SAMSData.SAMSEntities();
        bool IsEnabledWhatsAppNotification = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableWhatsAppNotification"]);
        string countryCode = ConfigurationManager.AppSettings["WhatsApp_CountryCode"];
        string testWhatsAppPhone = ConfigurationManager.AppSettings["WhatsApp_TestPhone"];
        public SAMSManager()
        {
        }

        private int GetConfigInt(string key, int defaultValue)
        {
            int value;
            return int.TryParse(ConfigurationManager.AppSettings[key], out value) ? value : defaultValue;
        }


        #region Member

        public IQueryable<MembershipType> GetMembershipTypes()
        {
            IQueryable<MembershipType> msList = from c in se.MembershipTypes
                                                where c.MembershipTypeId < 6
                                                select c;

            return msList;
        }


        public List<FeeType> GetFeeTypes()
        {
            List<FeeType> feeTypes = new List<FeeType>
                        {
                            new FeeType { FeeTypeId = 1, FeeTypeName = "Membership" },
                            new FeeType { FeeTypeId = 2, FeeTypeName = "Sports(All Games)" },
                            new FeeType { FeeTypeId = 3, FeeTypeName = "Locker" },
                            new FeeType { FeeTypeId = 4, FeeTypeName = "Gym" },                           
                            new FeeType { FeeTypeId = 5, FeeTypeName = "Swimming Pool" },
                            new FeeType { FeeTypeId = 6, FeeTypeName = "Shuttle" },                           
                            new FeeType { FeeTypeId = 7, FeeTypeName = "Tennis" },                           
                            new FeeType { FeeTypeId = 8, FeeTypeName = "Billiards" },
                            new FeeType { FeeTypeId = 9 , FeeTypeName = "Development" },

                        };

            return feeTypes;
        }

        public MembershipType GetMembershipType(int id)
        {
            MembershipType mem = (from c in se.MembershipTypes
                                  where c.MembershipTypeId == id
                                  select c).FirstOrDefault();

            return mem;
        }

        public List<MembershipFeeTransaction> GetMemberFees()
        {
            IQueryable<MembershipFeeTransaction> feeList = from c in se.MembershipFeeTransactions
                                                           select c;
            return feeList.ToList();         
        }

        public List<MembershipFeeTransaction> GetMemberFees(DateTime fromDate, DateTime toDate)
        {
            IQueryable<MembershipFeeTransaction> feeList = from c in se.MembershipFeeTransactions
                                                           where c.PaidDate >= fromDate && c.PaidDate <= toDate
                                                           select c ;
            return feeList.OrderByDescending(x=>x.MemershipFeeId).ToList();
        }

        public List<MembershipFeeTransaction> GetMemberFees(int memberId)
        {
            IQueryable<MembershipFeeTransaction> feeList = from c in se.MembershipFeeTransactions
                                                      .Include("Members")
                                                           where c.MemberId == memberId
                                                           select c;
            return feeList.ToList();
        }

        public Member GetMember(int memberId)
        {
            Member member = (from c in se.Members
                             where c.MemberId == memberId
                             select c).FirstOrDefault<Member>();

            return member;
        }

        public void UpdatePassword(string userId, string password)
        {
            var member = (from c in se.Members where c.MemberCode == userId select c).FirstOrDefault();

            if (member != null)
            {
                member.OtherDetails1 = password;
                se.SaveChanges();
            }
        }

        public Member VaidateUser(string userId, string password)
        {
            var member = (from c in se.Members where c.MemberCode == userId && c.OtherDetails1 == password select c).FirstOrDefault();

            if (member != null)
            {
                member.MembershipType = GetMembershipType((int)member.MembershipTypeId);
            }

            var membershipStatus = GetMemberFeeDetails(member);
            if (membershipStatus.FeeDueStatus == "Cleared")
            {
                member.OtherDetails2 = "Membership Fee Due: Paid";
            }
            else
            {
                string status = "Membership Fee Due: " + membershipStatus.DueAmount + "/- for (" + membershipStatus.DueFromYear + " - " + membershipStatus.DueToYear + ")";
                member.OtherDetails2 = status;
            }

            member.InterestedGames = GetMemberSportsDetails(member);
            member.ResidentialAddress3 = "";
            return member;

        }

        public List<Member> GetMembersList(string status)
        {
            IQueryable<Member> membersList;
            if (status != null && status.Trim() != "")

            {
                membersList = from c in se.Members
                              select c;
            }
            else
            {
                membersList = from c in se.Members
                              select c;
            }
         
            return membersList.ToList(); ;
        }



        public List<Member> GetActiveMembersList(string status)
        {
            IQueryable<Member> membersList;
            if (status != null && status.Trim() != "")

            {
                membersList = from c in se.Members
                              where (c.MembershipStatus != 3 && c.MembershipStatus != 4 && c.MembershipTypeId != 6)
                              select c;
            }
            else
            {
                membersList = from c in se.Members
                              where (c.MembershipStatus != 3 && c.MembershipStatus != 4 && c.MembershipTypeId != 6)
                              select c;
            }

            return membersList.ToList(); ;
        }
        public List<Member> GetSportsMembers(int feeType)
        {
            var sportsMembers = (from c in se.MembershipFeeTransactions
                                 where c.FeeType == feeType
                                 select c.MemberId).Distinct();

            var members = from m in se.Members
                          where sportsMembers.Contains(m.MemberId)
                          select m;

            return members.ToList();
        }


        public List<Member> GetLockerMembers()
        {


            var members = (from m in se.Members
                           where m.Address3 != ""
                           select m);

            return members.ToList();
        }

        public int SavePassword(int memberId, string password)
        {
            if (password != null && password != "")
            {
                Member selMember = se.Members.FirstOrDefault(r => r.MemberId == memberId);
                selMember.OtherDetails1 = password;
                se.SaveChanges();
            }
            return memberId;
        }
        public Member SaveMember(Member nMember)
        {
            if (nMember.MemberId == 0)
            {
                se.Members.AddObject(nMember);
                se.SaveChanges();
                Member selMember = se.Members.FirstOrDefault(r => r.MemberId == nMember.MemberId);
                if (selMember != null)
                    nMember = selMember;
            }
            else
            {
                Member selMember = se.Members.FirstOrDefault(r => r.MemberId == nMember.MemberId);
                selMember.MemberCode = nMember.MemberCode;
                selMember.MembershipTypeId = nMember.MembershipTypeId;
                selMember.MembershipStatus = nMember.MembershipStatus;
                selMember.FatherName = nMember.FatherName;
                selMember.FirstName = nMember.FirstName;
                selMember.LastName = nMember.LastName;
                selMember.ICNO = nMember.ICNO;
                selMember.MemberQRCode = nMember.MemberQRCode;
                selMember.Phone = nMember.Phone;
                selMember.AlternatePhone = nMember.AlternatePhone;
                selMember.DOB = nMember.DOB;
                selMember.DOJ = nMember.DOJ;
                selMember.Gender = nMember.Gender;
                selMember.Address1 = nMember.Address1;
                selMember.Address2 = nMember.Address2;
                selMember.Address3 = nMember.Address3;
                selMember.Place = nMember.Place;
                selMember.PANNo = nMember.PANNo;
                selMember.Email = nMember.Email;
                selMember.Profession = nMember.Profession;
                selMember.EducationalQualification = nMember.EducationalQualification;
                selMember.Comments = nMember.Comments;
                selMember.ReceiptNo = nMember.ReceiptNo;
                selMember.Reference1MemberCode = nMember.Reference1MemberCode;
                selMember.Reference2MemberCode = nMember.Reference2MemberCode;
                selMember.Reference3MemberCode = nMember.Reference3MemberCode;
                selMember.Reference4MemberCode = nMember.Reference4MemberCode;
                selMember.OtherDetails1 = nMember.OtherDetails1;
                selMember.OtherDetails2 = nMember.OtherDetails2;
                se.SaveChanges();
            }

            return nMember;
        }

        public List<MembershipFeeTransaction> GetMembershipFeeTransactions(int memberId)
        {
            var data = from s in se.MembershipFeeTransactions
                       where s.MemberId == memberId && s.FeeType == 1
                       select s;
            return data.ToList();
        }


        //public List<MembershipFeeTransaction> GetSportsFeeTransactions(int memberId)
        //{
        //    var data = from s in se.MembershipFeeTransactions
        //               where s.MemberId == memberId && s.FeeType >1 && s.FeeType<8
        //        select s;
        //    return data.ToList();
        //}
        public List<MembershipFeeTransaction> GetSportsFeeTransactions(int memberId)
        {
            var excludedFeeTypes = new List<int?> { 1, 3, 8 };

            var data = se.MembershipFeeTransactions
                .Where(s => s.MemberId == memberId
                         && !excludedFeeTypes.Contains(s.FeeType))
                .ToList();

            return data;
        }
        public List<MembershipFeeTransaction> GetSportsFeeTransactions(int memberId, int feeTypeId)
        {
            var data = from s in se.MembershipFeeTransactions
                       where s.MemberId == memberId && s.FeeType == feeTypeId
                       select s;
            return data.ToList();
        }

        public List<MembershipFeeTransaction> GetLockerFeeTransactions(int memberId)
        {
            var data = from s in se.MembershipFeeTransactions
                       where s.MemberId == memberId && s.FeeType == 3
                       select s;
            return data.ToList();
        }

        public void SendPaymentNotification(PaymentNotificationRequest request)
        {
            if (IsEnabledWhatsAppNotification)
            {
                var whatsAppService = new WhatsAppService();
                if (request.FeeTransactionId > 0)
                {
                    MembershipFeeTransaction mFee = se.MembershipFeeTransactions.FirstOrDefault(r => r.MemershipFeeId == request.FeeTransactionId);
                    Member m = GetMember((int)mFee.MemberId);

                    WhatsAppPaymentContext ctx = new WhatsAppPaymentContext();
                    ctx.FeeType = GetFeeType((int)mFee.FeeType);
                    ctx.MemberCode = m.MemberCode;
                    ctx.MemberName = m.FirstName + " " + m.LastName;
                    ctx.PhoneNumber = FormatWithCountryCodeOnlyDigits(m.Phone, countryCode);
                    ctx.Amount = mFee.MembershipFee.ToString();
                    ctx.PaymentMode = mFee.PaymentMode;
                    ctx.ReceiptNo = mFee.PaymentDetails;
                    ctx.Language = GetMemberPreferredLanguage(m.OtherDetails2);
                    ctx.MembershipYear = GetMembershipYear( mFee);
                    ctx.TemplateKey = "1";
                    ctx.Period = GetPeriodType(mFee.FeeReceiptNo);
                   // whatsAppService.SendPaymentTemplateAsync(ctx);
                    whatsAppService.SendTextMessageAsync(ctx);
                }
                else
                {
                    Member m = GetMember((int)request.MemberId);
                    if (request.FeeType == 1)
                    {
                        MembeFeeStatus feeStatus = GetMemberFeeDetails(m);
                        WhatsAppPaymentContext ctx = new WhatsAppPaymentContext();
                        ctx.FeeType = GetFeeType((int)request.FeeType);
                        ctx.MemberCode = m.MemberCode;
                        ctx.MemberName = m.FirstName + " " + m.LastName;
                        ctx.PhoneNumber = FormatWithCountryCodeOnlyDigits(m.Phone, countryCode);
                        ctx.Amount = feeStatus.DueAmount.ToString();
                        //  ctx.PaymentMode = mFee.PaymentMode;
                        //ctx.ReceiptNo = mFee.PaymentDetails;
                        ctx.Language = GetMemberPreferredLanguage(m.OtherDetails2);
                        ctx.MembershipYear = feeStatus.DueFromYear + "-" + feeStatus.DueToYear;
                        ctx.TemplateKey = "2";
                        whatsAppService.SendTextMessageAsync(ctx);
                        whatsAppService.SendMessageWithQrAsync(ctx.PhoneNumber);

                    }

                }
            }

        }

        public void SendBirthdayNotification(MemberNotificationRequest request)
        {
            if (!IsEnabledWhatsAppNotification)
                return;

            var member = GetMember(request.MemberId);
            if (member == null)
                return;

            var whatsAppService = new WhatsAppService();
            WhatsAppPaymentContext ctx = new WhatsAppPaymentContext();
            ctx.MemberCode = member.MemberCode;
            ctx.MemberName = member.FirstName + " " + member.LastName;
            ctx.PhoneNumber = FormatWithCountryCodeOnlyDigits(member.Phone, countryCode);
            ctx.Language = GetMemberPreferredLanguage(member.OtherDetails2);
            ctx.ClubName = ConfigurationManager.AppSettings["WhatsApp_ClubName"] ?? "Cosmopolitan Club";
            ctx.TemplateKey = "3";
            whatsAppService.SendTextMessageAsync(ctx);
        }
        private string GetPeriodType(int? type)
        {
            switch (type)
            {
                case 2:
                    return "Monthly";
                default:
                    return "Annual";              
            }
        }
        public  string FormatWithCountryCodeOnlyDigits(string phoneNumber, string countryCode)
        {
            if (!string.IsNullOrWhiteSpace(testWhatsAppPhone))
            {
                return testWhatsAppPhone;
            }
            if (string.IsNullOrWhiteSpace(phoneNumber)) return phoneNumber;
            if (string.IsNullOrWhiteSpace(countryCode)) return phoneNumber;

            // 1. Clean inputs: Extract ONLY digits from both the phone number and country code
            string cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            string numericCountryCode = new string(countryCode.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(cleanPhone) || string.IsNullOrEmpty(numericCountryCode))
                return phoneNumber;

            // 2. If it already starts with the numeric country code, return it
            if (cleanPhone.StartsWith(numericCountryCode))
            {
                return cleanPhone;
            }

            // 3. Remove leading '0' (commonly used in local dialing, e.g., 09876543210 -> 9876543210)
            if (cleanPhone.StartsWith("0"))
            {
                cleanPhone = cleanPhone.Substring(1);
            }

            // 4. Check one more time in case the number was something like "0919876543210"
            if (cleanPhone.StartsWith(numericCountryCode))
            {
                return cleanPhone;
            }

            // 5. Prepend the numeric country code
            return numericCountryCode + cleanPhone;
        }


        public MembershipFeeTransaction SaveMemberFee(MembershipFeeTransaction mFee)
        {
            if (mFee.MemershipFeeId == 0)
            {
                try
                {

                    //if (mFee.FeeReceiptNo == 2)
                    //    mFee.MembershipYear = mFee.StartDate.ToString() + "-" + mFee.EndtDate.ToString();
                    //else
                        mFee.MembershipYear = Convert.ToDateTime(mFee.StartDate).Year.ToString() + "-" + Convert.ToDateTime(mFee.EndtDate).Year.ToString();
                    
                    se.MembershipFeeTransactions.AddObject(mFee);
                    se.SaveChanges();

                    if (IsEnabledWhatsAppNotification)
                    {
                        Member m = GetMember((int)mFee.MemberId);
                        var whatsAppService = new WhatsAppService();
                        WhatsAppPaymentContext ctx = new WhatsAppPaymentContext();
                        ctx.FeeType = GetFeeType((int)mFee.FeeType);
                        ctx.MemberCode = m.MemberCode;
                        ctx.MemberName = m.FirstName + " " + m.LastName;
                        ctx.PhoneNumber = FormatWithCountryCodeOnlyDigits(m.Phone, countryCode);
                        ctx.Amount = mFee.MembershipFee.ToString();
                        ctx.PaymentMode = mFee.PaymentMode;
                        ctx.ReceiptNo = mFee.PaymentDetails;
                        ctx.Language = GetMemberPreferredLanguage(m.OtherDetails2);
                        ctx.MembershipYear = GetMembershipYear(mFee);
                        ctx.TemplateKey = "1";
                        ctx.Period = GetPeriodType(mFee.FeeReceiptNo);
                        //  whatsAppService.SendPaymentTemplateAsync(ctx);
                        whatsAppService.SendTextMessageAsync(ctx);
                    }
                } catch (Exception ex)
                {

                }
                MembershipFeeTransaction selMember = se.MembershipFeeTransactions.FirstOrDefault(r => r.MemershipFeeId == mFee.MemershipFeeId);
                if (selMember != null)
                    mFee = selMember;
            }
            else
            {
                MembershipFeeTransaction selMember = se.MembershipFeeTransactions.FirstOrDefault(r => r.MemershipFeeId == mFee.MemershipFeeId);
                selMember.MemberId = mFee.MemberId;
                selMember.MembershipYear = Convert.ToDateTime(mFee.StartDate).Year.ToString() + "-" + Convert.ToDateTime(mFee.EndtDate).Year.ToString();
                selMember.MembershipFee = mFee.MembershipFee;
                selMember.PaidDate = mFee.PaidDate;
                selMember.PaymentMode = mFee.PaymentMode;
                selMember.PaymentDetails = mFee.PaymentDetails;
                selMember.StartDate = mFee.StartDate;
                selMember.EndtDate = mFee.EndtDate;
                selMember.Comments = mFee.Comments;
                selMember.FeeReceiptNo = mFee.FeeReceiptNo;
                selMember.FeeType = mFee.FeeType;
                selMember.FeePrefix = mFee.FeePrefix;

                se.SaveChanges();
            }

            return mFee;
        }


        private string GetMembershipYear(MembershipFeeTransaction member)
        {
            if (member.FeeReceiptNo == 2)
            {
                string startDate = Convert.ToDateTime(member.StartDate)
                                    .ToString("dd-MMM-yyyy");

                string endDate = Convert.ToDateTime(member.EndtDate)
                                  .ToString("dd-MMM-yyyy");

                return $"{startDate} to {endDate}";
            }

            return member.MembershipYear;
        }
        private string GetMemberPreferredLanguage(string language)
        {
            if (string.IsNullOrEmpty(language))
                return "en";
            else
                return language;
        }

        private string GetFeeType(int type)
        {
            switch (type)
            {
                case 1:
                    return "Membership";
                case 2:
                    return "Sports(All Games)";
                case 3:
                    return "Locker";
                case 4:
                    return "Gym";
                case 5:
                    return "Swimming Pool";
                case 6:
                    return "Shuttle";
                case 7:
                    return "Tennis";
                case 8:
                    return "Billiards";
                case 9:
                    return "Development";
                default:
                    return "Unknown";
            }
        }

        public string DeleteMemberFee(MembershipFeeTransaction fee)
                {
                    string message = "";
                    try
                    {
                        if (fee.MemershipFeeId != 0)
                        {
                            var item = se.MembershipFeeTransactions.Where(x => x.MemershipFeeId == fee.MemershipFeeId).FirstOrDefault();

                            se.MembershipFeeTransactions.DeleteObject(item);
                            se.SaveChanges();
                            message = "Success";
                        }

                    }
                    catch (Exception ex)
                    {
                        message = ex.Message;
                    }
                    return message;
                }

        public List<MembeFeeDueSummary> GetMembersFeeSummary()
        {
            List<MembeFeeDueSummary> memberFeeList = new List<MembeFeeDueSummary>();
            var memberList = GetActiveMembersList("");

            foreach (var member in memberList)
            {
                memberFeeList.Add(GetMemberFeeDetailsSummary(member));
            }

            return memberFeeList;
        }

        public MembeFeeDueSummary GetMemberFeeDetailsSummary(Member member)
        {
            MembeFeeDueSummary membeFeeStatus = new MembeFeeDueSummary();

            // Member member = GetMember(memberId);
            List<MembershipFeeTransaction> feeList = GetMembershipFeeTransactions(member.MemberId);

            int configStartYear = Convert.ToInt32(ConfigurationManager.AppSettings["FeeStartYear"]);

            float fee = Convert.ToInt32(ConfigurationManager.AppSettings["AnnualMaintenanceFee"]);


            if (member != null)
            {
                int feeStartYear = 0;
                int dojYear = 0;
                DateTime currenDate = DateTime.Now;
                int currentYear;

                if (currenDate.Month > 3)
                {
                    currentYear = currenDate.Year + 1;
                }
                else
                {
                    currentYear = currenDate.Year;
                }


                if (member.DOJ != null)
                {
                    DateTime tdate = Convert.ToDateTime(member.DOJ);

                    if (tdate.Month > 3)
                    {
                        dojYear = tdate.Year + 1;
                    }
                    else
                    {
                        dojYear = tdate.Year;
                    }
                }


                feeStartYear = configStartYear;
                if (dojYear > feeStartYear)
                {
                    feeStartYear = dojYear + 1;
                }

                if (feeList.Count > 0)
                {
                    DateTime paidEndDate = (DateTime)feeList.Max(x => x.EndtDate);
                    feeStartYear = paidEndDate.Year;
                }

                if (feeStartYear == currentYear)
                {
                    membeFeeStatus.FeeDueStatus = "Cleared";
                    membeFeeStatus.DueAmount = 0;
                }
                else
                {
                    membeFeeStatus.DueFromYear = feeStartYear;
                    membeFeeStatus.DueToYear = currentYear;
                    membeFeeStatus.DueAmount = (currentYear - feeStartYear) * fee;
                    membeFeeStatus.FeeDueStatus = "Pending";
                }

                membeFeeStatus.Member = member;
            }



            return membeFeeStatus;
        }


        public List<MembeFeeStatus> GetMembersFeeStatus()
        {
            List<MembeFeeStatus> memberFeeList = new List<MembeFeeStatus>();
            var memberList = GetActiveMembersList("");

            foreach (var member in memberList)
            {
                memberFeeList.Add(GetMemberFeeDetails(member));
            }

            return memberFeeList;
        }


        public void GenerateFeeAlerts()
        {
            List<MembeFeeStatus> memberFeeList = new List<MembeFeeStatus>();
            var memberList = GetActiveMembersList("");
            foreach (var member in memberList)
            {
                var mFee = GetMemberFeeDetails(member);

                if (mFee.FeeDueStatus == "Pending" && !mFee.IsSeniorCitizenEligible)
                {
                    SaveAlert(mFee);
                }
            }
        }
        private int GetAge(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue || dateOfBirth.Value == DateTime.MinValue)
                return 0;

            var today = DateTime.Today;

            if (dateOfBirth > today)
                throw new ArgumentException("Date of birth cannot be in the future.");

            int age = today.Year - dateOfBirth.Value.Year;

            // If birthday hasn't occurred yet this year, subtract 1
            if (dateOfBirth.Value.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        private string GetSeniorCitizenStatus(MembeFeeStatus feeStatus, int ageLimit, int dueCutoffYear)
        {
            if (feeStatus.Age <= ageLimit)
                return "Not Eligible";

            if (feeStatus.FeeDueStatus == "Pending" && feeStatus.DueFromYear <= dueCutoffYear)
                return "Not Eligible";

            return "Eligible";
        }

        private void ApplySeniorCitizenFeeOverride(MembeFeeStatus feeStatus, int ageLimit, int dueCutoffYear)
        {
            feeStatus.SeniorCitizenDueCutoffYear = dueCutoffYear;
            feeStatus.SeniorCitizenStatus = GetSeniorCitizenStatus(feeStatus, ageLimit, dueCutoffYear);
            feeStatus.IsSeniorCitizenEligible = feeStatus.SeniorCitizenStatus == "Eligible";

            if (!feeStatus.IsSeniorCitizenEligible)
                return;

            feeStatus.FeeDueStatus = "Cleared";
            feeStatus.DueFromYear = 0;
            feeStatus.DueToYear = 0;
            feeStatus.DueAmount = 0;
        }

        public MembeFeeStatus GetMemberFeeDetails(Member member)
        {
            MembeFeeStatus membeFeeStatus = new MembeFeeStatus();

            // Member member = GetMember(memberId);
            List<MembershipFeeTransaction> feeList = GetMembershipFeeTransactions(member.MemberId);

            int configStartYear = Convert.ToInt32(ConfigurationManager.AppSettings["FeeStartYear"]);
            float fee = Convert.ToInt32(ConfigurationManager.AppSettings["AnnualMaintenanceFee"]);
            int configNewStartYear = Convert.ToInt32(ConfigurationManager.AppSettings["NewFeeStartYear"]);
            float newFee = Convert.ToInt32(ConfigurationManager.AppSettings["AnnualMaintenanceFeeNew"]);
            int seniorCitizenAge = GetConfigInt("SeniorCitizenAge", 70);
            int seniorCitizenDueCutoffYear = GetConfigInt("SeniorCitizenDueCutoffYear", 2024);
            if (member != null)
            {
                int feeStartYear = 0;
                int dojYear = 0;
                DateTime startDate, endDate;
                DateTime currenDate = DateTime.Now;
                int currentYear;

                if (currenDate.Month > 3)
                {
                    currentYear = currenDate.Year + 1;
                }
                else
                {
                    currentYear = currenDate.Year;
                }
                try
                {
                    membeFeeStatus.Age = GetAge(Convert.ToDateTime(member.DOB));
                }
                catch (Exception ex)
                {
                }

                if (member.DOJ != null)
                {
                    DateTime tdate = Convert.ToDateTime(member.DOJ);
                    dojYear = tdate.Year;
                    //if (tdate.Month > 3)
                    //{
                    //    dojYear = tdate.Year;
                    //}
                    //else
                    //{
                    //    dojYear = tdate.Year - 1;
                    //}
                }


                var feeStatusList = new List<FeeStaus>();
                feeStartYear = configStartYear;
                if (dojYear > configStartYear)
                {
                    feeStartYear = dojYear;
                }


                for (int year = feeStartYear; year < currentYear; year++)
                {
                    startDate = new DateTime(year, 4, 1);
                    endDate = new DateTime(year + 1, 3, 31);
                    FeeStaus feeStatus = new FeeStaus();
                    feeStatus.Year = year.ToString() + " - " + (year + 1).ToString();
                    if (year < dojYear)
                    {
                        feeStatus.Status = "-";
                    }
                    else
                    {
                        feeStatus.Status = "-";
                    }

                    var cnt = feeList.Where(x => x.StartDate <= startDate && x.EndtDate >= endDate).Count();
                    if (cnt > 0)
                    {
                        feeStatus.Status = "Paid";
                    }
                    feeStatusList.Add(feeStatus);
                }

                if (dojYear > feeStartYear)
                {
                    feeStartYear = dojYear;
                }

                if (feeList.Count > 0)
                {
                    DateTime paidEndDate = (DateTime)feeList.Max(x => x.EndtDate);
                    feeStartYear = paidEndDate.Year;
                }

                if (feeStartYear >= currentYear)
                {
                    membeFeeStatus.FeeDueStatus = "Cleared";
                    membeFeeStatus.DueAmount = 0;
                }
                else
                {
                    membeFeeStatus.DueFromYear = feeStartYear;
                    membeFeeStatus.DueToYear = currentYear;
                    float oldFee = (configNewStartYear - feeStartYear) * fee;
                    if (oldFee < 0) {
                        oldFee = 0;
                    }

                    var newFeeDueStartYear = configNewStartYear > feeStartYear ? configNewStartYear : feeStartYear;
                    membeFeeStatus.DueAmount = oldFee + ((currentYear - newFeeDueStartYear) * newFee);
                    membeFeeStatus.FeeDueStatus = "Pending";

                }

                membeFeeStatus.CurrentYear = currentYear;
                membeFeeStatus.CurrentYearDue = newFee;
                ApplySeniorCitizenFeeOverride(membeFeeStatus, seniorCitizenAge, seniorCitizenDueCutoffYear);
                membeFeeStatus.Member = member;
                membeFeeStatus.FeeStatusList = feeStatusList;
            }

            return membeFeeStatus;
        }


        public List<MembeFeeStatus> GetSportsMembersFeeStatus(int feeTypeId)
        {
            List<MembeFeeStatus> memberFeeList = new List<MembeFeeStatus>();

            if (feeTypeId == 3)
            {
                var memberList1 = GetLockerMembers();
                var memberList2 = memberList1.OrderBy(x => Convert.ToInt32(x.Address3));
                foreach (var member in memberList2)
                {
                    var data = GetLockerDetails(member);
                    if (data != null)
                        memberFeeList.Add(data);
                }

                return memberFeeList;
            }

            var memberList = GetSportsMembers(feeTypeId);

            foreach (var member in memberList)
            {
                var data = GetSportstMemberFeeDetailsNew(member, feeTypeId);
                if (data != null)
                    memberFeeList.Add(data);
            }

            return memberFeeList;
        }


        public void SaveAlert(MembeFeeStatus mFee)
        {
            SMSAlert smsAlert = new SMSAlert();
            smsAlert.AlertType = "MFee";
            smsAlert.MemberId = mFee.Member.MemberId;
            smsAlert.AlertStatus = 1;
            smsAlert.MessageText = "Dear Member, please pay membership maitenenace fee Due Rs." + mFee.DueAmount + "/- for the period of " + mFee.DueFromYear + "-" + mFee.DueToYear;
            se.SMSAlerts.AddObject(smsAlert);
            se.SaveChanges();
        }

        public int DeleteSMSAlert(SMSAlert item)
        {
            var alert = se.SMSAlerts.Where(x => x.SMSAlertId == item.SMSAlertId).FirstOrDefault();
            if (alert.SMSAlertId > 0)
            {
                se.DeleteObject(alert);
                se.SaveChanges();
            }
            return 1;
        }
        public int SendSMSAlert(SMSAlert item)
        {
            SMSService sms = new SMSService();
            Member member = GetMember((int)item.MemberId);
            var res = sms.SendSMS(member.Phone, item.MessageText);

            if (item.SMSAlertId > 0)
            {
                var alert = se.SMSAlerts.Where(x => x.SMSAlertId == item.SMSAlertId).FirstOrDefault();
                if (res == 1)
                {
                    alert.AlertStatus = 3;
                    alert.AlertDate = (DateTime)DateTime.Now;
                }
                else
                {
                    alert.AlertStatus = 4;
                }
                se.SaveChanges();
            }
            else
            {
                if (res == 1)
                {
                    item.AlertStatus = 3;
                    item.AlertDate = (DateTime)DateTime.Now;
                }
                else
                {
                    item.AlertStatus = 4;
                }
                se.AddToSMSAlerts(item);
            }

            return res;
        }

        public List<SMSAlert> GetMemberSMSAlerts(string alertType, string alertStatus)
        {
            List<SMSAlert> memberSMSAlertList = new List<SMSAlert>();

            if (alertStatus == "Sent")
            {
                memberSMSAlertList = (from s in se.SMSAlerts
                                      where s.AlertType == alertType && s.AlertStatus == 3
                                      select s).ToList();
            }
            else
            {
                memberSMSAlertList = (from s in se.SMSAlerts
                                      where s.AlertType == alertType && (s.AlertStatus != 3)
                                      select s).ToList();
            }

            return memberSMSAlertList;
        }

        public string GetMemberSportsDetails(Member member)
        {
            DateTime currenDate = DateTime.Now;
            int currentYear;

            if (currenDate.Month > 2)
            {
                currentYear = currenDate.Year + 1;
            }
            else
            {
                currentYear = currenDate.Year;
            }

            int configStartYear = currentYear - 1;
            string feeYear = configStartYear.ToString() + "-" + currentYear.ToString();
            List<MembershipFeeTransaction> feeList = GetSportsFeeTransactions(member.MemberId);

            var item = feeList.Where(x => x.MembershipYear == feeYear).FirstOrDefault();
            if (item != null)
            {
                return "Sports(" + item.Comments + ") : Paid " + item.MembershipFee + "/- for " + feeYear;
            }
            else
            {
                return "";
            }
        }

        public MembershipFeeTransaction GetMemberSportsDetailsNew(Member member, int feeTypeId)
        {
            DateTime currenDate = DateTime.Now;
            int currentYear;

            if (currenDate.Month > 2)
            {
                currentYear = currenDate.Year + 1;
            }
            else
            {
                currentYear = currenDate.Year;
            }

            int configStartYear = currentYear - 1;
            string feeYear = configStartYear.ToString() + "-" + currentYear.ToString();
            List<MembershipFeeTransaction> feeList = GetSportsFeeTransactions(member.MemberId, feeTypeId);
        
            var item = feeList.Where(x => x.MembershipYear == feeYear).FirstOrDefault();
            if (item != null)
            {
                // return "Sports(" + item.PaymentDetails + ") : Paid " + item.MembershipFee + "/- for " + feeYear;
                return item;
            }
            else
            {
                return null;
            }
        }

        public MemberFeeDueDetailsDto GetMemberPaidDetails(
    Member member,
    int feeTypeId)
        {
            DateTime currentDate = DateTime.Now;

            List<MembershipFeeTransaction> feeList =
                GetSportsFeeTransactions(member.MemberId, feeTypeId)
                .OrderBy(x => x.StartDate)
                .ToList();

            var dto = new MemberFeeDueDetailsDto();

            if (feeList.Any())
            {
                // Paid period
                dto.PaidFromDate = feeList.First().StartDate;

                dto.PaidToDate = feeList.Last().EndtDate;

                // Current active transaction
                dto.Member = member;

               dto.FeeTypeId= feeTypeId;

                // Due period starts from last paid end date
                dto.DueFromDate = feeList.Last().EndtDate;

                // Due till current date/year
                dto.DueToDate = currentDate;
            }
            else
            {
                //dto.IsFeePaid = false;

                dto.DueFromDate = currentDate;

                dto.DueToDate = currentDate;
            }

            return dto;
        }

        public MembershipFeeTransaction GetMemberLockerDetails(Member member, string year)
        {
            //DateTime currenDate = DateTime.Now;
            //int currentYear;

            //if (currenDate.Month > 2)
            //{
            //    currentYear = currenDate.Year + 1;
            //}
            //else
            //{
            //    currentYear = currenDate.Year;
            //}

            //int configStartYear = currentYear - 1;
            //string feeYear = configStartYear.ToString() + "-" + currentYear.ToString();
            //string previousYear = (configStartYear-1).ToString() + "-" + (currentYear-1).ToString();
            List<MembershipFeeTransaction> feeList = GetLockerFeeTransactions(member.MemberId);
            // List<MembershipFeeTransaction> list = new List<MembershipFeeTransaction>();
            var item = feeList.Where(x => x.MembershipYear == year).FirstOrDefault();
            if (item != null)
            {
                // return "Sports(" + item.PaymentDetails + ") : Paid " + item.MembershipFee + "/- for " + feeYear;
                return item;
            }
            else
            {
                return null;
            }


        }

        public MembeFeeStatus GetSportstMemberFeeDetailsNew(Member member, int feeTypeId)
        {
            MembeFeeStatus memberFeeStatus = new MembeFeeStatus();

            var feeStatusList = new List<FeeStaus>();
            var feeDetails = GetMemberSportsDetailsNew(member, feeTypeId);
            if (feeDetails != null)
            {

                // return feeDetails;
                FeeStaus feeStatus = new FeeStaus();
                feeStatus.Year = feeDetails.MembershipYear;
                feeStatus.Status = " Paid " + feeDetails.MembershipFee + "/- (" + feeDetails.Comments + ")";

                feeStatusList.Add(feeStatus);
                memberFeeStatus.Member = member;
                memberFeeStatus.FeeType = feeTypeId;
                memberFeeStatus.FeeStatusList = feeStatusList;
                return memberFeeStatus;
            }
            return null;
        }

        public MembeFeeStatus GetLockerDetails(Member member)
        {
            MembeFeeStatus memberFeeStatus = new MembeFeeStatus();
            int configStartYear = Convert.ToInt32(ConfigurationManager.AppSettings["FeeStartYear"]);

            DateTime currenDate = DateTime.Now;
            int currentYear;

            if (currenDate.Month > 2)
            {
                currentYear = currenDate.Year + 1;
            }
            else
            {
                currentYear = currenDate.Year;
            }

            // int configStartYear = currentYear - 1;
            List<string> years = new List<string>(); //{ previousYear, feeYear };
            for (int year = configStartYear; year < currentYear; year++)
            {
                string feeYear = year.ToString() + "-" + (year + 1).ToString();
                years.Add(feeYear);
            }
            // string feeYear = configStartYear.ToString() + "-" + currentYear.ToString();
            //string previousYear = (configStartYear - 1).ToString() + "-" + (currentYear - 1).ToString();


            var feeStatusList = new List<FeeStaus>();
            foreach (var year in years)
            {
                var feeDetails = GetMemberLockerDetails(member, year);
                FeeStaus feeStatus = new FeeStaus();
                feeStatus.Year = year;
                if (feeDetails != null)
                {
                    feeStatus.Status = "Paid";
                }
                else
                {
                    feeStatus.Status = "";
                }
                feeStatusList.Add(feeStatus);
            }
            memberFeeStatus.Member = member;
            memberFeeStatus.FeeType = 3;
            memberFeeStatus.FeeStatusList = feeStatusList;
            return memberFeeStatus;
        }

        public MembeFeeStatus GeSportstMemberFeeDetails(Member member)
        {
            MembeFeeStatus membeFeeStatus = new MembeFeeStatus();

            // Member member = GetMember(memberId);
            List<MembershipFeeTransaction> feeList = GetSportsFeeTransactions(member.MemberId);

            if (member != null)
            {
                int feeStartYear = 0;
                int dojYear = 0;
                DateTime startDate, endDate;
                DateTime currenDate = DateTime.Now;
                int currentYear;

                if (currenDate.Month > 2)
                {
                    currentYear = currenDate.Year + 1;
                }
                else
                {
                    currentYear = currenDate.Year;
                }

                int configStartYear = currentYear - 1;

                if (member.DOJ != null)
                {
                    DateTime tdate = Convert.ToDateTime(member.DOJ);

                    if (tdate.Month > 2)
                    {
                        dojYear = tdate.Year + 1;
                    }
                    else
                    {
                        dojYear = tdate.Year;
                    }
                }


                var feeStatusList = new List<FeeStaus>();
                feeStartYear = configStartYear;
                int flag = 0;
                for (int year = feeStartYear; year < currentYear; year++)
                {
                    startDate = new DateTime(year, 4, 1);
                    endDate = new DateTime(year + 1, 3, 31);
                    FeeStaus feeStatus = new FeeStaus();
                    feeStatus.Year = year.ToString() + " - " + (year + 1).ToString();
                    if (year < dojYear)
                    {
                        feeStatus.Status = "-";
                    }
                    else
                    {
                        feeStatus.Status = "Not Paid";
                    }

                    var cnt = feeList.Where(x => x.StartDate <= startDate && x.EndtDate >= endDate).Count();
                    if (cnt > 0)
                    {
                        flag = 1;
                        feeStatus.Status = "Paid";
                    }
                    feeStatusList.Add(feeStatus);
                }


                membeFeeStatus.Member = member;
                membeFeeStatus.FeeStatusList = feeStatusList;
                if (flag == 0)
                    membeFeeStatus = null;
            }



            return membeFeeStatus;
        }



        public MemberDasboard GetDashboardSummary()
        {
            MemberDasboard md = new MemberDasboard();

            MembershipSummary ms = new MembershipSummary();
            ms.TotalMembers = GetMembersCount(0);
            ms.DonorsCount = GetMembersCount(1);
            ms.LifeCount = GetMembersCount(2);
            ms.SrCitizenCount = GetMembersCount(3);
            ms.CorporateCount = GetMembersCount(4);
            ms.HonorCount = GetMembersCount(5);
            md.MembershipSummary = ms;

            MembershipSummary ems = new MembershipSummary();
            ems.TotalMembers = GetMembersCount(0, 3);
            ems.DonorsCount = GetMembersCount(1, 3);
            ems.LifeCount = GetMembersCount(2, 3);
            ems.SrCitizenCount = GetMembersCount(3, 3);
            ems.CorporateCount = GetMembersCount(4, 3);
            ems.HonorCount = GetMembersCount(5, 3);
            md.ExpiredMembersSummary = ems;
            return md;
        }

        public int GetMembersCount(int membershipTypeId)
        {
            int cnt = 0;
            if (membershipTypeId > 0)
                cnt = se.Members.Where(x => x.MembershipTypeId == membershipTypeId).Count();
            else
                cnt = se.Members.Where(x => x.MembershipTypeId < 6).Count();
            return cnt;
        }

        public int GetMembersCount(int membershipTypeId, int status)
        {
            int cnt = 0;
            if (membershipTypeId > 0)
                cnt = se.Members.Where(x => x.MembershipTypeId == membershipTypeId && x.MembershipStatus == status).Count();
            else
                cnt = se.Members.Where(x => x.MembershipTypeId < 6 && x.MembershipStatus == status).Count();
            return cnt;
        }

        public List<Member> GetRoomDonors()
        {
            var donors = from m in se.Members
                         where m.IsRoomDonor == true
                         select m;


            return donors.ToList();
        }

        #endregion

        #region HotelManagement

        public List<RoomDonorsTransactions> GetDonorsData(DateTime startDate, DateTime endDate)
        {
            List<RoomDonorsTransactions> list = new List<RoomDonorsTransactions>();
            var donorList = GetRoomDonors();
            foreach (Member member in donorList)
            {
                RoomDonorsTransactions rdt = new RoomDonorsTransactions();
                rdt.Donor = member;
                var donorCoupons = (from d in se.DonorCoupons
                                    where d.MemberId == member.MemberId && d.CouponStartdate >= startDate
                                    && d.CouponEndDate <= endDate select d).FirstOrDefault();

                if (donorCoupons != null)
                {
                    rdt.StartCoupon = (int)donorCoupons.StartCoupon;
                    rdt.EndCoupon = (int)donorCoupons.EndCoupon;
                    rdt.TotalCoupons = (int)donorCoupons.CouponsCount;
                }

                var bklist = from p in se.BookingDetails
                             join b in se.Bookings
                             on p.BookingId equals b.BookingId
                             where b.MemberId == member.MemberId && b.CheckInTime >= startDate && b.CheckInTime <= endDate
                             && p.PlanCode == "Donor Plan"
                             select p;

                int cnt = 0;
                string details = "";
                foreach (var bd in bklist)
                {
                    if (bd.RoomDays != null)
                    {
                        cnt += (int)bd.RoomDays;
                        details += bd.CouponCode + ",";
                    }
                }
                rdt.Details = details.TrimEnd(',');
                rdt.UsedCoupons = cnt;
                list.Add(rdt);
            }

            return list.OrderBy(x => x.StartCoupon).ToList();
        }

        public List<Booking> GetBookingsList(DateTime fromDate, DateTime toDate, string status = "")
        {
            IQueryable<Booking> bookingsList;
            toDate = toDate.AddDays(1);

            if (status == null || status == "")
            {
                bookingsList = from c in se.Bookings
                                                   .Include("BookingDetails")
                                                     .Where(x => x.BookingDateTime >=fromDate && x.BookingDateTime <= toDate)
                               orderby c.BookingStatus
                               select c;
            }
            else
            {
                bookingsList = from c in se.Bookings
                                                   .Include("BookingDetails")
                                                   .Where(x => x.BookingDateTime >= fromDate && x.BookingDateTime <= toDate && x.BookingStatus == status)
                               orderby c.BookingStatus
                               select c;
            }

            return bookingsList.ToList(); ;
        }

        public List<Booking> GetBookingsList(string status = "")
        {
            IQueryable<Booking> bookingsList;

            if (status == null || status == "")
            {
                bookingsList = from c in se.Bookings
                                                   .Include("BookingDetails")
                               orderby c.BookingStatus
                               select c;
            }
            else
            {
                bookingsList = from c in se.Bookings
                                                   .Include("BookingDetails")
                                                   .Where(x=>x.BookingStatus==status)
                               orderby c.BookingStatus
                               select c;
            }

            return bookingsList.ToList(); ;
        }



        public List<CheckedInRoomDetails> GetCheckedInRoomsDetails()
        {
            var checkedInRooms = se.Bookings
                .Where(b => b.BookingStatus == "CheckIn")
                .Join(se.BookingDetails,
                      b => b.BookingId,
                      bd => bd.BookingId,
                      (b, bd) => new { Booking = b, BookingDetails = bd })
                .Join(se.Rooms,
                      x => x.BookingDetails.RoomId,
                      r => r.RoomId,
                      (x, r) => new CheckedInRoomDetails
                      {
                          BookingId = x.Booking.BookingId,
                          BookingDetailId = x.BookingDetails.BookingDetailId,
                          RoomId = r.RoomId,
                          RoomNo = r.RoomNo,
                          Guest = x.Booking.GuestName,
                          CheckInDate = (DateTime)x.Booking.CheckInTime
                      })
                .ToList();

            return checkedInRooms;
        }


        public Room SaveRoom(Room nRoom)
        {
            if (nRoom.RoomId == 0)
            {
                se.Rooms.AddObject(nRoom);
                se.SaveChanges();
                Room selRoom = se.Rooms.FirstOrDefault(r => r.RoomId == nRoom.RoomId);
                if(selRoom != null)
                    nRoom = selRoom;
            }
            else
            {
                Room selRoom = se.Rooms.FirstOrDefault(r => r.RoomId == nRoom.RoomId);
                selRoom.RoomNo = nRoom.RoomNo;
                selRoom.RoomStatus = nRoom.RoomStatus;
                selRoom.FloorNo = nRoom.FloorNo;
                selRoom.Details = nRoom.Details;
                selRoom.TariffId = nRoom.TariffId;
                selRoom.UpdatedBy = nRoom.UpdatedBy;
                selRoom.UpdatedOn = DateTime.Now;
                se.SaveChanges();
            }

            return nRoom;
        }
        public List<Room> GetRoomsList()
        {
            IQueryable<Room> roomList = from c in se.Rooms
                                                       select c;

            return roomList.ToList(); 
        }
        public Room GetRoom(int roomId)
        {
            Room room = (from c in se.Rooms
                            where c.RoomId == roomId
                                        select c).FirstOrDefault<Room>();

            return room;
        }

        public List<Room> GetRoomsList(string status)
        {
            IQueryable<Room> roomList;
            if (status != null && status.Trim() !="")

            {
                roomList = from c in se.Rooms
                           where c.RoomStatus.ToLower().Contains(status)
                           select c;
            }
            else
            {
                roomList = from c in se.Rooms
                                select c;
            }

            return roomList.ToList(); ;
        }

        private string GetRoomStatusName(string roomStatus)
        {
            string statusName = "";

            switch(roomStatus)
            {
                case "1":
                    statusName = "Available";
                    break;
                case "2":
                    statusName = "Dirty";
                    break;
                case "3":
                    statusName = "Dead Room";
                    break;
                default:
                    statusName = "Others";
                    break;
            }

            return statusName;

        }

        public List<RoomTypeSummary> GetRoomsAvailability()
        {
            List<RoomTypeSummary> list = new List<RoomTypeSummary>();
            var tariffs = GetRoomTariffs();
            foreach (var tariff in tariffs)
            {
                List<Room> roomList = (from r in se.Rooms
                                       where r.TariffId == tariff.TariffId                                    
                                       select r).ToList();
                RoomTypeSummary rts = new RoomTypeSummary();
                int totRooms = roomList.Count();
                int availableRooms = 0;
                rts.TarridId = tariff.TariffId.ToString();
                rts.RoomType = tariff.TariffName;
                foreach (var room in roomList)
                {
                    int status = GetRoomStatus(room, DateTime.Now);
                    if (status == 1)
                    {
                        availableRooms += 1;
                    }
                }
                rts.TotalRooms = totRooms;
                rts.AvailableRooms = availableRooms;

                list.Add(rts);
            }
            return list;

        }

        public RoomSummary GetRoomSummary(DateTime tdate)
        {
            RoomSummary roomSummary=new RoomSummary();
            List<Room> roomList = (from r in se.Rooms
                                   orderby r.FloorNo, r.RoomNo
                                   select r).ToList();

            Dictionary<string, List<RoomModel>> floorData = new Dictionary<string, List<RoomModel>>();
           
            foreach(var room in roomList)
            {
                List<RoomModel> rooms = new List<RoomModel>();
                RoomModel rm = new RoomModel();
                rm.RoomNo = room.RoomNo;
                rm.RoomType =(room.TariffId == null ? "" : GetRoomTariffDetails((int) room.TariffId));
                rm.RoomStatus = GetRoomStatus(room, tdate);
                if (floorData.ContainsKey(room.FloorNo))
                {
                    rooms = floorData[room.FloorNo];
                }
                rooms.Add(rm);

                floorData[room.FloorNo] = rooms;
            }

            List<FloorModel> floorList = new List<FloorModel>();
            foreach (var item in floorData)
            {
                FloorModel fm = new FloorModel();
                fm.FloorNo = item.Key;
                fm.Rooms = item.Value;
                floorList.Add(fm);
            }

            roomSummary.Floors = floorList;

            //get day summary;



            return roomSummary;
        }

        private string GetRoomTariffDetails(int tariffId)
        {
            string details;

            var tariff = se.Tariffs.Where(x => x.TariffId == tariffId).FirstOrDefault();

            details = tariff.TariffName; // + " - " + tariff.Price.ToString();
            return details;
        }

        private int GetRoomStatus(Room room, DateTime tdate)
        {
            int roomStatus = 1;

             switch(room.RoomStatus)
            {
                case "Dirty":
                    roomStatus = 4;
                    break;
                case "Dead Room":
                    roomStatus = 5;
                    break;
                case "Maintenance":
                    roomStatus = 6;
                    break;
            }

            if (roomStatus != 1) return roomStatus;

            var checkInItem = (from b in se.BookingDetails
                            where b.RoomId == room.RoomId 
                            && b.Status.Contains("CheckIn")
                            select b).FirstOrDefault();

            if (checkInItem != null &&  tdate>=checkInItem.RoomCheckIn )
            {
                roomStatus = 2;
                return roomStatus;
            }

            var blockedItem = (from b in se.BookingDetails
                               where b.RoomId == room.RoomId
                               && b.Status.Contains("Blocked")
                               select b).FirstOrDefault();

            if (blockedItem != null && tdate >= blockedItem.PlannedCheckIn && tdate <= blockedItem.PlannedCheckOut)
            {
                roomStatus = 3;           
            }

            return roomStatus;
        }

        public List<Room> GetAvailabelRoomsforBooking(DateTime strDate)
        {
            List<Room> itemList = new List<Room>();

            IQueryable<Room> roomList = from r in se.Rooms
                                        where !r.RoomStatus.Contains("Dirty")
                                        select r;

            foreach(var item in roomList)
            {
                int status = GetRoomStatus(item, strDate);
                if (status==1)
                {
                    itemList.Add(item);
                }                
            }

            return itemList;
        }

        public void UpdateBooking(Booking booking)
        {
            using (SAMSData.SAMSEntities se1 = new SAMSData.SAMSEntities())
            {
                List<int> roomsList = new List<int>();
                Booking item = (from p in se1.Bookings
                                .Include("BookingDetails")
                                where p.BookingId == booking.BookingId
                                select p).FirstOrDefault();

                item.BookingNo = booking.BookingNo;
                item.BookingDateTime = booking.BookingDateTime;
                item.BookingReferenceType = booking.BookingReferenceType;
                item.BookingType = booking.BookingType;
                item.CheckInTime = booking.CheckInTime;
                item.CheckoutTime = booking.CheckoutTime;
                item.GuestName = booking.GuestName;
                item.GuestPlace = booking.GuestPlace;
                item.GuestPhone = booking.GuestPhone;
                item.GuestAlternatePhone = booking.GuestAlternatePhone;
                item.GuestAddress = booking.GuestAddress;
                item.VisitPurpose = booking.VisitPurpose;
                item.IdProofType = booking.IdProofType;
                item.IdProofNo = booking.IdProofNo;
                item.Remarks = booking.Remarks;
                item.Age = booking.Age;
                item.AdvanceAmount = booking.AdvanceAmount;
                item.AdvancePaymentType = booking.AdvancePaymentType;
                item.AdvancePaymentDetails = booking.AdvancePaymentDetails;
                item.PlannedCheckIn = booking.PlannedCheckIn;
                item.PlannedCheckOut = booking.PlannedCheckOut;
                item.NoOfRooms = booking.NoOfRooms;
                item.BookingStatus = booking.BookingStatus;
                item.TotalServiceAmount = booking.TotalServiceAmount;
                item.TotalAmount = booking.TotalAmount;
                item.NetAmount = booking.NetAmount;
                item.CompanyName = booking.CompanyName;
                item.CompanyAddress = booking.CompanyAddress;
                item.GSTNo = booking.GSTNo;
                item.MemberId = booking.MemberId;
                se1.SaveChanges();

                var pdlist = (from p in item.BookingDetails
                              select p).ToList();

                foreach (BookingDetail p in pdlist)
                {
                    var attachedChild = (from c in booking.BookingDetails
                                         where c.BookingDetailId == p.BookingDetailId
                                         select c).FirstOrDefault();

                    if (attachedChild == null)
                    {
                        se1.BookingDetails.DeleteObject(p);
                        se1.SaveChanges();
                    }
                }

                foreach (BookingDetail p in booking.BookingDetails)
                {
                    var attachedChild = (from c in se1.BookingDetails
                                         where c.BookingDetailId == p.BookingDetailId
                                         select c).FirstOrDefault();

                    if (attachedChild != null)
                    {
                        // Existing child - apply new values                     
                        attachedChild.RoomId = p.RoomId;
                        attachedChild.RoomRent = p.RoomRent;
                        attachedChild.ExtraBedAmount = p.ExtraBedAmount;
                        attachedChild.NoOfAdults = p.NoOfAdults;
                        attachedChild.NoOfChildren = p.NoOfChildren;
                        attachedChild.GuestDetails = p.GuestDetails;
                        attachedChild.RoomCheckIn = p.RoomCheckIn;
                        attachedChild.RoomCheckOut = p.RoomCheckOut;
                        attachedChild.RoomDays = p.RoomDays;
                        attachedChild.TotalRoomRent = p.TotalRoomRent;
                        attachedChild.TotalExtraBedAmount = p.TotalExtraBedAmount;
                        attachedChild.TotalAmount = p.TotalAmount;
                        attachedChild.ServiceAmount = p.ServiceAmount;
                        attachedChild.FoodAmount = p.FoodAmount;
                        attachedChild.OtherAmount = p.OtherAmount;
                        attachedChild.OtherDetails = p.OtherDetails;
                        attachedChild.Discount = p.Discount;
                        attachedChild.NetAmount = p.NetAmount;
                        attachedChild.PaymentMode = p.PaymentMode;
                        attachedChild.PaymentDetails = p.PaymentDetails;
                        attachedChild.BookingType = p.BookingType;                    
                        attachedChild.PlannedCheckIn = p.PlannedCheckIn;
                        attachedChild.PlannedCheckOut = p.PlannedCheckOut;
                        attachedChild.PlanCode = p.PlanCode;
                        attachedChild.CouponCode = p.CouponCode;
                        if (p.Status =="CheckOut" && attachedChild.Status =="CheckIn")
                        {
                            roomsList.Add((int)attachedChild.RoomId);
                        }
                        attachedChild.Status = p.Status;
                        se1.SaveChanges();

                    }
                    else
                    {
                        // New child
                        // Don't insert original object. It will attach whole detached graph
                        BookingDetail pitem = new BookingDetail();
                        pitem.BookingId = booking.BookingId;
                        pitem.RoomId = p.RoomId;
                        pitem.RoomRent = p.RoomRent;
                        pitem.ExtraBedAmount = p.ExtraBedAmount;
                        pitem.NoOfAdults = p.NoOfAdults;
                        pitem.NoOfChildren = p.NoOfChildren;
                        pitem.GuestDetails = p.GuestDetails;
                        pitem.RoomCheckIn = p.RoomCheckIn;
                        pitem.RoomCheckOut = p.RoomCheckOut;
                        pitem.RoomDays = p.RoomDays;
                        pitem.TotalRoomRent = p.TotalRoomRent;
                        pitem.TotalExtraBedAmount = p.TotalExtraBedAmount;
                        pitem.TotalAmount = p.TotalAmount;
                        pitem.ServiceAmount = p.ServiceAmount;
                        pitem.FoodAmount = p.FoodAmount;
                        pitem.OtherAmount = p.OtherAmount;
                        pitem.OtherDetails = p.OtherDetails;
                        pitem.Discount = p.Discount;
                        pitem.NetAmount = p.NetAmount;
                        pitem.PaymentMode = p.PaymentMode;
                        pitem.PaymentDetails = p.PaymentDetails;
                        pitem.BookingType = p.BookingType;
                        pitem.Status = p.Status;
                        pitem.PlannedCheckIn = p.PlannedCheckIn;
                        pitem.PlannedCheckOut = p.PlannedCheckOut;
                        pitem.PlanCode = p.PlanCode;
                        pitem.CouponCode = p.CouponCode;
                        se1.BookingDetails.AddObject(pitem);
                        se1.SaveChanges();

                      
                    }

                }
                //change roomstatus to dirty on checkout
                foreach(var roomId in roomsList)
                {
                    var Room = se1.Rooms.Where(x => x.RoomId == roomId).FirstOrDefault();
                    if (Room!=null)
                    {
                        Room.RoomStatus = "Dirty";
                        SaveRoom(Room);
                    }
                }
            }
        }
        public IQueryable<Tariff> GetRoomTariffs()
        {
            IQueryable<Tariff> tariffList = from c in se.Tariffs
                                        select c;

            return tariffList;
        }



        public User ValidateUser(User user)
        {
            string userName = SAMSSecurity.EncryptString(user.UserName);
            string password  = SAMSSecurity.EncryptString(user.Password);

            var userDet = GetUser(userName,password,user.ApplicationName);

            User cUser = new User();
            cUser.UserID = userDet.UserID;
            cUser.UserName = user.UserName;
            cUser.Password = userDet.Password;
            cUser.UserLevel = SAMSSecurity.DecryptString(userDet.UserLevel);
            return cUser;
        }

        public void RoomCheckOut(BookingDetail bookingDetail)
        {
            Booking _booking = (from b in se.Bookings
                                .Include("BookingDetails")
                                where b.BookingId == bookingDetail.BookingId
                                select b).FirstOrDefault<Booking>();

            bool bookingStatusFlag = true;
            bool freeBookingFlag = false;

            foreach (BookingDetail bd in _booking.BookingDetails)
            {
                if (bd.BookingDetailId == bookingDetail.BookingDetailId)
                {
                    // bd.Room = GetRoom(bd.RoomId.Value);
                    bd.RoomCheckIn = bookingDetail.RoomCheckIn;
                    bd.RoomCheckOut = bookingDetail.RoomCheckOut;
                    bd.PaymentMode = bookingDetail.PaymentMode;
                    bd.PaymentDetails = bookingDetail.PaymentDetails;
                    bd.ServiceAmount = GetRoomServiceAmount((int)bookingDetail.BookingId, (int)bd.RoomId);
                    DateTime tdate = (bd.RoomCheckOut == null) ? DateTime.Now : (DateTime)bd.RoomCheckOut;
                    bd.RoomDays = GetRoomDays((DateTime)bd.RoomCheckIn, tdate);
                    bd.CouponCode = bookingDetail.CouponCode;

                    //// bd.RoomDays = (tdate.Date - ((DateTime)bd.RoomCheckIn).Date).Days;
                    // TimeSpan ts = tdate - (DateTime)bd.RoomCheckIn;               
                    // var hoursDiff = ts.TotalHours;
                    // var hours = hoursDiff;

                    // bd.RoomDays = (int)Math.Floor(hoursDiff / 24);
                    //     int extraHours = (int)Math.Ceiling(hoursDiff % 24) ;
                    // if (bd.RoomDays==0)
                    // {
                    //     bd.RoomDays = 1;
                    // }
                    // else
                    // if (extraHours>2)
                    // {
                    //     bd.RoomDays++;
                    // }
                    // if (bd.RoomDays <= 0)
                    // {
                    //     bd.RoomDays = 1;
                    // }

                    if (bd.CouponCode!=null && bd.CouponCode.ToLower() == "free")
                    {
                        bd.TotalRoomRent = 0;
                        bd.TaxAmount = 0;
                        bd.NetAmount = 0;
                        bd.Status = "CheckOut";
                        freeBookingFlag = true;
                    }
                    else
                    {
                        bd.TotalExtraBedAmount = bd.ExtraBedAmount == null ? 0 : bd.ExtraBedAmount;// * bd.RoomDays;
                        bd.TotalRoomRent = bd.RoomRent == null ? 0 : bd.RoomRent * bd.RoomDays;
                        bd.TotalAmount = bd.TotalRoomRent + bd.TotalExtraBedAmount;
                        if (bd.Tax > 0)
                        {
                            double taxAmount = (double)(bd.TotalAmount * bd.Tax / 100);
                            bd.TaxAmount = taxAmount;
                        }
                        else
                        {
                            bd.TaxAmount = 0;
                        }
                        bd.TotalAmount += bd.TaxAmount;
                        bd.NetAmount = bd.TotalAmount + bd.ServiceAmount;
                        bd.Status = "CheckOut";
                    }
                }

                if (bd.Status != "CheckOut")
                {
                    bookingStatusFlag = false;
                }
            }
            _booking.TotalServiceAmount = _booking.BookingDetails.Sum(x => x.ServiceAmount);
            _booking.TotalAmount = _booking.BookingDetails.Sum(x => x.TotalAmount);
            _booking.NetAmount = _booking.TotalServiceAmount + _booking.TotalAmount;

            if (bookingStatusFlag)
            {
                _booking.BookingStatus = "CheckOut";
                _booking.CheckoutTime = bookingDetail.RoomCheckOut;
                _booking.BookingNoPrefix = GetBookingNoPrefix(_booking.BookingType);
                if (freeBookingFlag)
                {
                    _booking.BookingNoPrefix += "F";
                }
                _booking.BookingNo = GetNextBookingNumber(_booking.BookingNoPrefix, _booking.BookingType);
            }

            UpdateBooking(_booking);

            foreach (BookingDetail bd in _booking.BookingDetails)
            {
                var receipts = GetRoomPayments((int) bd.BookingId);
                var advances = receipts.Where(x=>x.RoomId==bd.RoomId).Sum(x => x.Amount);
                if (advances != null)
                {
                    if (bd.NetAmount > advances)
                    {
                        //settlement
                        BookingPayment bp = new BookingPayment();
                        bp.BookingId = bd.BookingId;
                        bp.RoomId = bd.RoomId;
                        bp.Amount = bd.NetAmount - advances;
                        bp.PaymentType = "Settlement";
                        bp.ReceiptNo = "Balance";
                        bp.PaymentMode = "Cash";
                        bp.PaymentDate = DateTime.Now;
                        SaveReceipt(bp);
                    }
                    else if (advances > bd.NetAmount)
                    {
                        //refund
                        BookingPayment bp = new BookingPayment();
                        bp.BookingId = bd.BookingId;
                        bp.RoomId = bd.RoomId;
                        bp.Amount = bd.NetAmount  - advances;
                        bp.PaymentType = "Settlement";
                        bp.ReceiptNo = "Refund";
                        bp.PaymentMode = "Cash";
                        bp.PaymentDate = DateTime.Now;
                        SaveReceipt(bp);
                    }
                }
            }
        }
        
        public void SaveBooking(Booking booking)
        {
            try
            {

                if (booking.BookingId == 0)
                {
                    if(booking.BookingType == "Current")
                    {
                        booking.BookingStatus = "CheckIn";
                    }
                    else if(booking.BookingType == "Advance")
                    {
                        booking.BookingStatus = "Advance";
                    }
                    else if (booking.BookingType == "Block")
                    {
                        booking.BookingStatus = "Blocked";
                    }
                }

                bool existFlag = false;
                  foreach (BookingDetail bd in booking.BookingDetails)
                {
                    bd.Room = null;

                    if (bd.BookingDetailId ==0 )
                    {
                        var bkDet = (from b in se.BookingDetails
                                     where b.RoomId == bd.RoomId && b.Status == "CheckIn"
                                     select b).FirstOrDefault();
                        if (bkDet != null )
                        {
                            existFlag = true;
                        }

                        bd.BookingType = booking.BookingType;
                        bd.PlannedCheckIn = booking.PlannedCheckIn;
                        bd.PlannedCheckOut = booking.PlannedCheckOut;
                        bd.RoomCheckIn = booking.CheckInTime;
                        //bd.RoomCheckOut = booking.CheckoutTime;
                        bd.Status = booking.BookingStatus;
                    }

                }

                if (booking.BookingId == 0)
                {

                    //int lastBookigNo = se.Bookings.Max(b => b.BookingNo).Value;
                    //booking.BookingNo = lastBookigNo + 1;

                    // booking.BookingNoPrefix = GetBookingNoPrefix(booking.BookingType);
                    // booking.BookingNo = GetNextBookingNumber(booking.BookingNoPrefix);
                    if (!existFlag)
                    {
                        booking.CreatedOn = DateTime.Now;
                        se.Bookings.AddObject(booking);
                        se.SaveChanges();

                        foreach (var bd in booking.BookingDetails)
                        {
                            if (bd.Status == "CheckIn" && bd.RoomCheckIn != null)
                            {
                                // ── Inventory integration ────────────────────────────────────────
                                try
                                {
                                    new InventoryManager().AssignCheckinStock(
                                        bd.BookingDetailId, 0);
                                }
                                catch { /* Non-blocking: log and continue */ }

                                // ─────────────────────────────────────────────────────────────────
                            }
                        }
                    }
                    else
                    {

                    }
                }
                else
                {
                    UpdateBooking(booking);
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        public void DeleteBooking(Booking booking)
        {
            if (booking.BookingId > 0)
            {
                try
                {
                    var bd = se.BookingDetails.FirstOrDefault(x => x.BookingId == booking.BookingId);
                    se.BookingDetails.DeleteObject(bd);
                    var bk = se.Bookings.FirstOrDefault(x => x.BookingId == booking.BookingId);
                    se.Bookings.DeleteObject(bk);
                    se.SaveChanges();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private string GetBookingNoPrefix(string bookingType)
        {
            string statusName = "";

            switch (bookingType)
            {
                case "Current":
                      statusName = ConfigurationManager.AppSettings["CheckInPrefix"]; ;
                    break;
                case "Block":
                    statusName = "CB";
                    break;
                case "Advance":
                    statusName = ConfigurationManager.AppSettings["AdvancedPrefix"]; ;
                    break;
                default:
                    statusName = "CC";
                    break;
            }

            return statusName;

        }

        public int GetNextBookingNumber(string prefix, string bookingType)
        {
            var billNumber = 0;
            try
            {
                billNumber = (from s in se.Bookings
                              where s.BookingNoPrefix== prefix
                              select (int)s.BookingNo).Max();


                billNumber = billNumber + 1;
            }
            catch
            {
                billNumber = 0;
            }

          if ( billNumber==0)
            {
               
                switch (bookingType)
                {
                    case "Current":
                        billNumber = Convert.ToInt32( ConfigurationManager.AppSettings["CheckInStartNumebr"]);
                        break;              
                    case "Advance":
                        billNumber = Convert.ToInt32(ConfigurationManager.AppSettings["AdvacnedStartNumebr"]) ;
                        break;
                    default:
                        billNumber = 1;
                        break;
                }
            }
           

           

            return billNumber;
        }
        public Booking GetBooking(int id)
        {
            Booking _booking = (from b in se.Bookings
                                .Include("BookingDetails")
                                where b.BookingId == id
                                select b).FirstOrDefault<Booking>();
            foreach (BookingDetail bd in _booking.BookingDetails)
            {
                bd.Room = GetRoom(bd.RoomId.Value);
                //if (_booking.BookingStatus == "CheckIn")
                //{
                //    bd.ServiceAmount = GetRoomServiceAmount(id, (int)bd.RoomId);
                //    DateTime tdate = (bd.RoomCheckOut == null) ? DateTime.Now : (DateTime)bd.RoomCheckOut;
                //    bd.RoomCheckOut = Convert.ToDateTime(tdate.ToString("yyyy-MM-dd HH:mm"));
                //    // bd.RoomDays = (tdate.Date - ((DateTime)bd.RoomCheckIn).Date).Days;
                //    bd.RoomDays = GetRoomDays((DateTime)bd.RoomCheckIn, tdate);
                //    bd.TotalExtraBedAmount = bd.ExtraBedAmount == null ? 0 : bd.ExtraBedAmount * bd.RoomDays;
                //    bd.TotalRoomRent = bd.RoomRent == null ? 0 : bd.RoomRent * bd.RoomDays;
                //    bd.TotalAmount = bd.TotalRoomRent + bd.TotalExtraBedAmount;
                //    if (bd.Tax > 0)
                //    {
                //        double taxAmount = (double)(bd.TotalAmount * bd.Tax / 100);
                //        bd.TaxAmount = taxAmount;
                //    }
                //    else
                //    {
                //        bd.TaxAmount = 0;
                //    }

                //    bd.TotalAmount += bd.TaxAmount;

                //    bd.NetAmount = bd.TotalAmount + bd.ServiceAmount;
                //    //  bd.NetAmount = bd.TotalExtraBedAmount + bd.TotalRoomRent + bd.ServiceAmount;
                //}
            }
            //if (_booking.BookingStatus == "CheckIn")
            //{
            //    _booking.TotalServiceAmount = _booking.BookingDetails.Sum(x => x.ServiceAmount);
            //    _booking.TotalAmount = _booking.BookingDetails.Sum(x => x.TotalAmount);

            //    _booking.NetAmount = _booking.TotalServiceAmount + _booking.TotalAmount;
            //    //_booking.CheckoutTime = DateTime.Now;
            //}
            return _booking;

          
        }

        public Booking GetBookingWithCheckoutDetails(int id)
        {
            Booking _booking = (from b in se.Bookings
                                .Include("BookingDetails")
                                where b.BookingId == id
                                select b).FirstOrDefault<Booking>();
            foreach (BookingDetail bd in _booking.BookingDetails)
            {
                bd.Room = GetRoom(bd.RoomId.Value);
                if (_booking.BookingStatus == "CheckIn")
                {
                    bd.ServiceAmount = GetRoomServiceAmount(id, (int)bd.RoomId);
                    DateTime tdate = (bd.RoomCheckOut == null) ? DateTime.Now : (DateTime)bd.RoomCheckOut;
                    bd.RoomCheckOut = Convert.ToDateTime(tdate.ToString("yyyy-MM-dd HH:mm"));
                    // bd.RoomDays = (tdate.Date - ((DateTime)bd.RoomCheckIn).Date).Days;
                    bd.RoomDays = GetRoomDays((DateTime)bd.RoomCheckIn, tdate);
                    bd.TotalExtraBedAmount = bd.ExtraBedAmount == null ? 0 : bd.ExtraBedAmount;// * bd.RoomDays;
                    bd.TotalRoomRent = bd.RoomRent == null ? 0 : bd.RoomRent * bd.RoomDays;
                    bd.TotalAmount = bd.TotalRoomRent + bd.TotalExtraBedAmount;
                    if (bd.Tax > 0)
                    {
                        double taxAmount = (double)(bd.TotalAmount * bd.Tax / 100);
                        bd.TaxAmount = taxAmount;
                    }
                    else
                    {
                        bd.TaxAmount = 0;
                    }

                    bd.TotalAmount += bd.TaxAmount;

                    bd.NetAmount = bd.TotalAmount + bd.ServiceAmount;
                   // bd.CouponCode = coupons;
                    //  bd.NetAmount = bd.TotalExtraBedAmount + bd.TotalRoomRent + bd.ServiceAmount;
                }
            }
            if (_booking.BookingStatus == "CheckIn")
            {
                _booking.TotalServiceAmount = _booking.BookingDetails.Sum(x => x.ServiceAmount);
                _booking.TotalAmount = _booking.BookingDetails.Sum(x => x.TotalAmount);

                _booking.NetAmount = _booking.TotalServiceAmount + _booking.TotalAmount;
                //_booking.CheckoutTime = DateTime.Now;
            }
            return _booking;


        }

        int GetRoomDays(DateTime fdate, DateTime tdate)
        {
            TimeSpan ts = tdate - (DateTime)fdate;
            var hoursDiff = ts.TotalHours;
            var hours = hoursDiff;
            int roomDays = 0;
            roomDays = (int)Math.Floor(hoursDiff / 24);
            int extraHours = (int)Math.Ceiling(hoursDiff % 24);
            if (roomDays == 0)
            {
                roomDays = 1;
            }
            else
            if (extraHours > 2)
            {
                roomDays++;
            }
            if (roomDays <= 0)
            {
                roomDays = 1;
            }
            return roomDays;
        }
        public double GetRoomServiceAmount(int bookingId, int roomId)
        {
            double amount = 0;
            try
            {
                amount = Convert.ToDouble( se.RoomServices.Where(x => x.BookingId == bookingId && x.RoomId == roomId).Sum(x => x.ServiceAmount));
            }
            catch { 
            }
            return amount;
        }
        //public Room SaveRoom(Room room)
        //{
        //    se.Rooms.AddObject(room);
        //    se.SaveChanges();
        //    return room;
        //}


        public BookingPayment SaveReceipt(BookingPayment receipt)
        {
            try
            {
                if (receipt.BookingPaymentId == 0)
                {
                    receipt.CreatedOn = DateTime.Now;
                    se.BookingPayments.AddObject(receipt);
                    se.SaveChanges();
                    
                }
                else
                {
                    BookingPayment selReceipt = se.BookingPayments.FirstOrDefault(r => r.BookingPaymentId == receipt.BookingPaymentId);
                    selReceipt.RoomId = receipt.RoomId;
                    selReceipt.PaymentType = receipt.PaymentType;
                    selReceipt.PaymentMode = receipt.PaymentMode;
                    selReceipt.PaymentDate = receipt.PaymentDate;
                    selReceipt.Amount = receipt.Amount;
                    selReceipt.ReceiptNo = receipt.ReceiptNo;
                    selReceipt.PaymentDetails = receipt.ReceiptNo;
                  //  selReceipt.UpdatedOn = DateTime.Now;
                    se.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            return receipt;
        }
        public string DeleteReceipt(BookingPayment receipt)
        {
            string message = "";
            try
            {
                if (receipt.BookingPaymentId != 0)
                {
                    var item = se.BookingPayments.Where(x => x.BookingPaymentId == receipt.BookingPaymentId).FirstOrDefault();

                    se.BookingPayments.DeleteObject(item);
                    se.SaveChanges();
                    message = "Success";
                }

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return message;
        }

        public RoomService SaveService(RoomService rService)
        {
            try
            {
                if (rService.RoomServiceId == 0)
                {
                    rService.CreatedOn = DateTime.Now;
                    se.RoomServices.AddObject(rService);
                    se.SaveChanges();
                    RoomService selService = se.RoomServices.FirstOrDefault(r => r.RoomServiceId == rService.RoomServiceId);
                    if (selService != null)
                        rService = selService;
                }
                else
                {
                    RoomService selService = se.RoomServices.FirstOrDefault(r => r.RoomServiceId == rService.RoomServiceId);
                    selService.RoomId = rService.RoomId;
                    selService.ServiceType = rService.ServiceType;
                    selService.ServiceAmount = rService.ServiceAmount;
                    selService.ServiceDateTime = rService.ServiceDateTime;
                    selService.UpdatedOn = DateTime.Now;
                    se.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            return rService;
        }


        public string DeleteService(RoomService rService)
        {
            string message="";
            try
            {
                if (rService.RoomServiceId != 0)
                {
                    var item = se.RoomServices.Where(x => x.RoomServiceId == rService.RoomServiceId).FirstOrDefault();
                   
                    se.RoomServices.DeleteObject(item);
                    se.SaveChanges();
                    message = "Success";
                }
                
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return message;
        }

        public List<RoomService> GetRoomServices(int bookingId)
        {
            IQueryable<RoomService> _services = from s in se.RoomServices

                                                where s.BookingId == bookingId
                                                select s;

            return _services.ToList();
        }
        public List<BookingPayment> GetRoomPayments(int bookingId)
        {
            IQueryable<BookingPayment> _services = from s in se.BookingPayments

                                                where s.BookingId == bookingId
                                                select s;

            return _services.ToList();
        }
        public List<BookingDetail> GetBookingDetails(int bookingId)
        {
            IQueryable<BookingDetail> _details = from s in se.BookingDetails

                                                 where s.BookingId == bookingId
                                                 select s;

            return _details.ToList();
        }


        #region HMReports
        public List<RoomSummaryReport> GetRoomSummaryReportData(DateTime fromDate, DateTime toDate)
        {
            List<RoomSummaryReport> lstRoomSummary = new List<RoomSummaryReport>();
            List<Room> roomList = (from r in se.Rooms
                                   .Include("Tariff")
                                   orderby r.FloorNo, r.RoomNo
                                   select r).ToList();



            foreach (var room in roomList)
            {
                RoomSummaryReport rptRoom = new RoomSummaryReport();
                rptRoom.RoomId = room.RoomId;
                rptRoom.RoomNo = room.RoomNo;
                rptRoom.DaysBookedforDonor =0;
                rptRoom.DonorBookingAmount =0;
                rptRoom.DaysBookedforGeneral = 0;
                rptRoom.GeneralBookingAmount = 0;
                rptRoom.TotalAmount =0;
                rptRoom.DaysBooked = 0;
                var lstBookings = (from b in se.BookingDetails
                                   where b.RoomId == room.RoomId && ((b.RoomCheckIn >= fromDate && b.RoomCheckIn <= toDate) || (b.RoomCheckOut >= fromDate && b.RoomCheckOut <= toDate))
                                   select b).ToList();

                foreach(var item in lstBookings)
                {
                    DateTime startDate, endDate;
                    startDate = (DateTime)item.RoomCheckIn;
                    //endDate = item.RoomCheckOut==null? null:(DateTime)item.RoomCheckOut;
                    if (item.RoomCheckIn < fromDate)
                    {
                        startDate = fromDate;
                    }
                    if(item.RoomCheckOut == null || item.RoomCheckOut > toDate)
                    {
                        endDate = toDate;
                    }
                    else
                        endDate = (DateTime)item.RoomCheckOut;

                    int days = 0;
                    if (item.RoomCheckIn == startDate && item.RoomCheckOut == endDate)
                    {
                        days  = (int)item.RoomDays;
                    }
                    else
                    {
                        days= (endDate - fromDate).Days;
                    }

                    rptRoom.DaysBooked += days;
                    if (item.PlanCode == "Donor Plan")
                    {
                        rptRoom.DaysBookedforDonor += days;
                        rptRoom.DonorBookingAmount += days * item.RoomRent;
                        rptRoom.TotalAmount += days * item.RoomRent;
                    }
                    else
                    {
                        
                        rptRoom.DaysBookedforGeneral += days;
                        rptRoom.GeneralBookingAmount += days * item.RoomRent;
                        rptRoom.TotalAmount += days * item.RoomRent;
                    }
                }

                //rptRoom.DaysBooked = lstBookings.Count();
                //rptRoom.DaysVacancy = (int)((toDate - fromDate).TotalDays - rptRoom.DaysBooked);
                //rptRoom.TotalAmount = lstBookings.Sum(b => b.RoomRent);// + lstBookings.Sum(b => b.ExtraBedAmount);
                //rptRoom.TotalDays = rptRoom.DaysBooked + rptRoom.DaysVacancy;
                rptRoom.TariffId = room.Tariff.TariffId;
                rptRoom.TariffName = room.Tariff.TariffName;
                //rptRoom.DaysBookedforDonor = lstBookings.Where(x => x.PlanCode == "Donor Plan").Count();
                //rptRoom.DaysBookedforGeneral = lstBookings.Where(x => x.PlanCode != "Donor Plan").Count();

                //rptRoom.DonorBookingAmount = lstBookings.Where(x => x.PlanCode == "Donor Plan").Sum(x => x.RoomRent);
                //rptRoom.GeneralBookingAmount = lstBookings.Where(x => x.PlanCode != "Donor Plan").Sum(x => x.RoomRent);

                lstRoomSummary.Add(rptRoom);
            }

            return lstRoomSummary.OrderBy(x=>x.RoomNo).ToList();
        }



        public List<RoomCheckoutReport> GetRoomCheckoutData(DateTime fromDate, DateTime toDate)
        {
            List<RoomCheckoutReport> lstRoomSummary = new List<RoomCheckoutReport>();
            List<Room> roomList = (from r in se.Rooms
                                   .Include("Tariff")
                                   orderby r.FloorNo, r.RoomNo
                                   select r).ToList();
            var newToDate = toDate.AddDays(1);
            var lstBookings = (from b in se.BookingDetails
                               where (b.RoomCheckOut >= fromDate && b.RoomCheckOut <= newToDate)
                               select b).ToList();

            foreach (var bk in lstBookings)
            {
                RoomCheckoutReport rptRoom = new RoomCheckoutReport();
                var room = roomList.Where(x => x.RoomId == bk.RoomId).FirstOrDefault();
                var booking = se.Bookings.Where(x => x.BookingId == bk.BookingId).FirstOrDefault();

                rptRoom.BookingId = booking.BookingId;
                rptRoom.BookingNo = booking.BookingNo;
                rptRoom.RoomId = room.RoomId;
                rptRoom.RoomNo = room.RoomNo;
                rptRoom.Plan = bk.PlanCode;
                rptRoom.RoomRent = bk.RoomRent;
                rptRoom.CheckIn = (DateTime)bk.RoomCheckIn;
                rptRoom.CheckOut = (DateTime)bk.RoomCheckOut;
                rptRoom.TotalDays = (int)bk.RoomDays;
                rptRoom.TotalAmount = bk.TotalAmount;
                rptRoom.ExtraBedAmount = bk.ExtraBedAmount;
                rptRoom.MemberCode = booking.MemberCode;
                rptRoom.MemberName = booking.MemberName;
                rptRoom.Coupons = bk.CouponCode;
                rptRoom.TariffId = room.Tariff.TariffId;
                rptRoom.TariffName = room.Tariff.TariffName;
                rptRoom.CompanyName = booking.CompanyName;
                rptRoom.GSTNo = booking.GSTNo;
                if (bk.TaxAmount > 0)
                {
                    rptRoom.CGST = bk.TaxAmount / 2;
                    rptRoom.SGST = bk.TaxAmount / 2;
                }
                else
                {
                    rptRoom.CGST = 0;
                    rptRoom.SGST = 0;
                }
                try
                {
                    var advData = (from p in se.BookingPayments
                                  where p.BookingId == booking.BookingId && p.PaymentType== "Advance"
                                   select p).ToList();

                    if (advData != null)
                    {
                        rptRoom.AdvanceReceipts = string.Join(",", advData.Select(p => p.ReceiptNo));
                        rptRoom.AdvanceAmount = advData.Sum(x => x.Amount);
                    }
                }
                catch { }
                lstRoomSummary.Add(rptRoom);
            }
            return lstRoomSummary.OrderBy(x => x.RoomNo).ToList();
        }

       
        public PaymentSummary GetPaymentDetails(DateTime fromDate, DateTime toDate)
        {
            PaymentSummary ps = new PaymentSummary();
            toDate = toDate.AddDays(1);
            ps.FromDate = fromDate;
            ps.ToDate = toDate;
            ps.TotalAmount = 0;
            ps.ByCard = 0;
            ps.ByCash = 0;
            ps.ByOnline = 0;
            ps.ByDonorBookings = 0;
            ps.ByGeneralBookings = 0;
            var lstPayments = (from p in se.BookingPayments
                                   where p.CreatedOn >= fromDate && p.CreatedOn <= toDate
                                   orderby p.CreatedOn
                                   select p).ToList();

            int startValue = 0;
            string tdate=DateTime.Now.ToShortDateString();
            PaymentDetail pd=new PaymentDetail();
            List<PaymentDetail> details = new List<PaymentDetail>();
            foreach (var item in lstPayments)
            {
                if (startValue==0)
                {
                    tdate = ((DateTime) item.CreatedOn).ToString("dd-MMM-yyyy");
                    pd.PaymentDate = tdate;
                    startValue = 1;
                }

                if(tdate != ((DateTime) item.CreatedOn).ToString("dd-MMM-yyyy"))
                {
                    details.Add(pd);
                    pd = new PaymentDetail();

                    tdate = ((DateTime)item.CreatedOn).ToString("dd-MMM-yyyy");
                    pd.PaymentDate = tdate;
                }

                switch (item.PaymentMode)
                {
                    case "Cash":
                        ps.ByCash += (double) item.Amount;
                        pd.ByCash += (double)item.Amount;
                        break;
                    case "Card":
                        ps.ByCard += (double) item.Amount;
                        pd.ByCard += (double)item.Amount;
                        break;
                    default:
                        ps.ByOnline += (double)item.Amount;
                        pd.ByOnline += (double)item.Amount;
                        break;
                }

                var bd = (from b in se.BookingDetails where b.BookingId == item.BookingId select b).FirstOrDefault();
                if (bd.PlanCode == "Donor Plan")
                {
                    ps.ByDonorBookings += (double)item.Amount;
                    pd.ByDonorBookings += (double)item.Amount;
                }
                else
                {
                    ps.ByGeneralBookings += (double)item.Amount;
                    pd.ByGeneralBookings += (double)item.Amount;
                }

                pd.TotalAmount = pd.ByCash + pd.ByCard + pd.ByOnline;
            }
            details.Add(pd);
            ps.TotalAmount = ps.ByCash + ps.ByCard + ps.ByOnline;
            ps.Details = details;
            return ps;
        }


        public List<PaymentHistory> GetPaymentHistoryData(DateTime fromDate, DateTime toDate)
        {
            List<PaymentHistory> lstPaymentHistory = new List<PaymentHistory>();
            List<Room> roomList = (from r in se.Rooms
                                   .Include("Tariff")
                                   orderby r.FloorNo, r.RoomNo
                                   select r).ToList();


         //   DateTime fdate = (DateTime)fromDate;
            DateTime tdate = ((DateTime)toDate).AddDays(1);
            foreach (var room in roomList)
            {

                var lstPayments = (from p in se.BookingPayments
                                   join b in se.Bookings
                                   on p.BookingId equals b.BookingId
                                   where p.RoomId == room.RoomId && b.CheckoutTime  >= fromDate && b.CheckoutTime  <= tdate
                                   select new
                                   {
                                       BookingId = p.BookingId,
                                       BookingNo = b.BookingNo,
                                       ReceiptNo = p.ReceiptNo,
                                       PaymentType = p.PaymentType,
                                       PaymentMode = p.PaymentMode,
                                       Amount = p.Amount,
                                       Details = p.PaymentDetails,
                                       PaymentDate = p.PaymentDate,
                                       BookingNoPrefix = b.BookingNoPrefix,
                                       CheckInDate = b.CheckInTime,
                                       CheckOutDate = b.CheckoutTime
                                   }).ToList();
                List<PaymentDetails> lstDetails = new List<PaymentDetails>();
                foreach (var _payment in lstPayments)
                {
                    PaymentHistory _roomHistory = new PaymentHistory();
                    _roomHistory.RoomId = room.RoomId;
                    _roomHistory.RoomNo = room.RoomNo;
                    _roomHistory.TariffId = room.TariffId;
                    _roomHistory.TariffName = room.Tariff.TariffName;
                    _roomHistory.BookingId = _payment.BookingId;
                    _roomHistory.BookingNo = _payment.BookingNo;
                    _roomHistory.ReceiptNo = _payment.ReceiptNo;
                    _roomHistory.PaymentType = _payment.PaymentType;
                    _roomHistory.PaymentMode = _payment.PaymentMode;
                    _roomHistory.PaymentDate = _payment.PaymentDate.ToString(); ;
                    _roomHistory.Amount = _payment.Amount;
                    _roomHistory.Details = _payment.Details;
                    _roomHistory.CheckIn =(DateTime) _payment.CheckInDate;
                    _roomHistory.CheckOut = (DateTime)_payment.CheckOutDate;

                    lstPaymentHistory.Add(_roomHistory);
                }

            }

            return lstPaymentHistory;
        }


        #endregion
        #endregion



        #region userManagement

        public IQueryable<SAMSConfiguration> GetConfigurationData()
        {
            IQueryable<SAMSConfiguration> configList = from c in se.SAMSConfigurations
                                                       select c;

            return configList;
        }

        public SAMSUser GetUser(string loginID, string password,string appName="hotel")
        {            
            SAMSUser su = (from s in se.SAMSUsers
                           where s.UserName == loginID && s.Password == password && s.Status == true && s.ApplicationName==appName
                           select s).FirstOrDefault();

            return su;
        }

        public SAMSUser GetUser(int userid)
        {
            SAMSUser su = (from s in se.SAMSUsers
                           where s.UserID == userid
                           select s).FirstOrDefault();            
            return su;
        }

        public IQueryable<SAMSUser> GetUserList()
        {
            IQueryable<SAMSUser> su = (from s in se.SAMSUsers
                                       select s);
            return su;
        }

        public void SaveUser(SAMSUser su)
        {
            se.SAMSUsers.AddObject(su);
            se.SaveChanges();
        }
        public void UpdateUser(SAMSUser su)
        {
            //   SAMSData.SAMSEntities se1 = new    SAMSData.SAMSEntities();

            SAMSUser udata = (from u in se.SAMSUsers
                              where u.UserID == su.UserID
                              select u).FirstOrDefault();
            udata.UserName = su.UserName;
            udata.Password = su.Password;
            udata.UserLevel = su.UserLevel;
            udata.Status = su.Status;
            se.SaveChanges();
            
        }

        

        public Business GetBusiness()
        {
            Business bm = (from b in se.Businesses
                           where b.Status == true
                           select b).FirstOrDefault();

            return bm;
        }

        public void SaveBusiness(Business bd)
        {
            se.Businesses.AddObject(bd);
            se.SaveChanges();
        }

        public void UpdateBusiness(Business bd)
        {

              SAMSData.SAMSEntities se1 = new   SAMSData.SAMSEntities();

            Business bdata = (from b in se1.Businesses
                              where b.BusinessID == bd.BusinessID
                              select b).FirstOrDefault();

            bdata.BusinessName = bd.BusinessName;
            bdata.Description = bd.Description;
            bdata.ManagerName = bd.ManagerName;
            bdata.Address1 = bd.Address1;
            bdata.Address2 = bd.Address2;
            bdata.Place = bd.Place;
            bdata.Phone1 = bd.Phone1;
            bdata.Phone2 = bd.Phone2;
            bdata.Email = bd.Email;
            bdata.State = bd.State;
            bdata.TAN = bd.TAN;
            bdata.TIN = bd.TIN;
            bdata.PAN = bd.PAN;
            bdata.STAX = bd.STAX;
            bdata.Other2 = bd.Other2;
            bdata.Other1 = bd.Other1;
            se1.SaveChanges();
            se1 = null;
        }

        #endregion

        #region Products

        public IQueryable<Tax> GetTaxes()
        {
            IQueryable<Tax> taxList = from t in se.Taxes
                                      orderby t.Priority 
                                      select t;

            return taxList;
        }

        public IQueryable<Product> GetProducts()
        {

            IQueryable<Product> productList = from p in se.Products
                                              orderby p.ProductName
                                              select p;
          
            return productList;
        }

        public IQueryable<ProductView> GetAllProducts()
        {

            IQueryable<ProductView> productList = from p in se.Products
                                              orderby p.ProductName
                                              select new ProductView
                                              {
                                                  ProductID = p.ProductID,
                                                  ProductName = p.ProductName
                                              };

            return productList;
        }



        public IQueryable<Product> GetProducts(int sectionID)
        {

            IQueryable<Product> productList = from p in se.Products
                                              where p.Section.SectionID==sectionID 
                                              orderby p.ProductName
                                              select p;

            return productList;
        }

        public IQueryable<Product> GetGroupProducts( int groupID)
        {

            IQueryable<Product> productList = from p in se.Products
                                              where p.ProductGroupID == groupID 
                                              orderby p.ProductName
                                              select p;

            return productList;
        }

        public IQueryable<PurchaseDetail> GetProductData(string barcode)
        {

            IQueryable<PurchaseDetail> PurchaseDetail = from p in se.PurchaseDetails
                                                        .Include("Product")
                                              where p.BarCode ==barcode 
                                              orderby p.Product.ProductName 
                                              select p;

            return PurchaseDetail;
        }

     
        public Product GetProduct(int id)
        {

            Product item = (from p in se.Products
                            .Include("ProductGroup")
                            .Include("Measurement")
                            .Include("Section")
                            where p.ProductID == id
                            select p).FirstOrDefault();


            return item;
        }

        public Product GetProduct(int sectionID, string productName)
        {

            Product item = (from p in se.Products                            
                            where p.SectionID == sectionID && p.ProductName == productName 
                            select p).FirstOrDefault();


            return item;
        }


        public IQueryable<PurchaseDetail> GetProductDetails(int ProductID)
        {
            IQueryable<PurchaseDetail> itemList = from p in se.PurchaseDetails
                                                  where p.ProductID == ProductID
                                                  select p;


            return itemList;
        }


    


        public IQueryable GetRecentProducts()
        {
            
            IQueryable productList = from p in se.Products
                                     orderby p.ProductID descending
                                     select new
                                     {
                                         ProductID = p.ProductID,
                                         ProductName = p.ProductName,
                                         SectionName=p.Section.SectionName,
                                         GroupName=p.ProductGroup.GroupName ,
                                         Units=p.Measurement.MeasurementName ,
                                         ProductCode=p.ProductCode,
                                         Active=p.Status, 
                                         Description=p.Description,
                                         Accessorices=p.Accessories 
                                        };

            
            return productList;
        }

        public IQueryable GetRecentProducts(int sectionID)
        {
           // se.Refresh(System.Data.Objects.RefreshMode.StoreWins, se.Products);
            IQueryable productList = from p in se.Products
                                     where p.SectionID == sectionID
                                     orderby p.ProductName
                                     select new
                                     {
                                         ProductID = p.ProductID,
                                         ProductName = p.ProductName,
                                         SectionName = p.Section.SectionName,
                                         GroupName = p.ProductGroup.GroupName,
                                         Units = p.Measurement.MeasurementName,
                                         ProductCode = p.ProductCode,
                                         Active = p.Status,
                                         Description = p.Description,
                                         Accessorices = p.Accessories
                                     };


            return productList;
        }
        public IQueryable<ProductGroup> GetProductGroups(int sectionID)
        {
            IQueryable<ProductGroup> pgList = from p in se.ProductGroups
                                              where p.SectionID == sectionID 
                                              select p;
            return pgList;
        }

        public IQueryable<ProductGroup> GetProductGroups(int sectionID,string groupType)
        {
            IQueryable<ProductGroup> pgList = from p in se.ProductGroups
                                              where p.SectionID==sectionID && p.Description.Equals(groupType)
                                              select p;
            return pgList;
        }

        public IQueryable<ProductGroup> GetProductGroups( string groupType)
        {
            IQueryable<ProductGroup> pgList = from p in se.ProductGroups
                                              where  p.Description.Equals(groupType)
                                              orderby p.GroupName
                                              select p;
            return pgList;
        }

        public ProductGroup SaveProductGroup(ProductGroup pg)
        {
             SAMSData.SAMSEntities se = new SAMSData.SAMSEntities();
            se.ProductGroups.AddObject(pg);
            se.SaveChanges();
            return pg;
        }

        public ProductGroup UpdateProductGroup(ProductGroup pg)
        {
            SAMSData.SAMSEntities se = new SAMSData.SAMSEntities();
            ProductGroup pgItem = (from p in se.ProductGroups
                                   where p.ProductGroupID == pg.ProductGroupID
                                   select p).FirstOrDefault();

            pgItem.GroupName = pg.GroupName;

            se.SaveChanges();
            return pgItem;
        }


        public Product SaveProuct(Product item)
        {
            SAMSData.SAMSEntities se = new SAMSData.SAMSEntities();

            ProductGroup pg = (from p in se.ProductGroups
                               where p.ProductGroupID == item.ProductGroup.ProductGroupID
                               select p).FirstOrDefault();
            Section sc = (from s in se.Sections
                          where s.SectionID == item.Section.SectionID
                          select s).FirstOrDefault();

            Measurement mm = (from m in se.Measurements
                              where m.MeasurementID == item.Measurement.MeasurementID
                              select m).FirstOrDefault();

            Product item1 = new Product();
            item1.ProductName = item.ProductName;
            item1.Description = item.Description;
            item1.Accessories = item.Accessories;
            item1.ProductCode = item.ProductCode;
            item1.ProductGroup = pg;
            item1.Measurement = mm;
            item1.Section = sc;
            item1.Status = item.Status;
            item1.ProductTax = item.ProductTax;
            //item.ProductGroup = pg;
            //item.Measurement = mm;
            //item.Section = sc;
            se.Products.AddObject(item1);
            se.SaveChanges();
            se = null;
            return item1;
        }

        public Product UpdateProduct(Product item)
        {
               SAMSData.SAMSEntities se = new    SAMSData.SAMSEntities();
            //EntityKey key = new EntityKey("   SAMSData.SAMSEntities.Products", "ProductID", item.ProductID );

            //Product item1 = (Product)se.GetObjectByKey(key);
                        
            Product item1 = (from a in se.Products
                             where a.ProductID == item.ProductID
                             select a).FirstOrDefault();
            ProductGroup pg = (from p in se.ProductGroups 
                               where p.ProductGroupID == item.ProductGroup.ProductGroupID
                                   select p).FirstOrDefault();
            Section sc= (from s in se.Sections 
                         where s.SectionID== item.Section.SectionID
                         select s).FirstOrDefault();

           Measurement mm= (from m in se.Measurements 
                            where m.MeasurementID==item.Measurement.MeasurementID
                            select m).FirstOrDefault();

            item1.ProductName = item.ProductName;
            item1.Description = item.Description;
            item1.Accessories = item.Accessories;
            item1.ProductCode = item.ProductCode;
            item1.ProductGroup= pg;
            item1.Measurement = mm;
            item1.Section = sc;
            item1.Status = item.Status;
            item1.ProductTax = item.ProductTax;
            
            se.SaveChanges();
            se = null;
            return item1;
        }




        public IQueryable<Measurement> GetMeasurements()
        {            
            IQueryable<Measurement> itemList = (from m in se.Measurements
                                                select m);
            
            return itemList;
        }

        public Measurement GetMeasurement(int id)
        {

            Measurement item = (from m in se.Measurements
                                where m.MeasurementID == id
                                select m).FirstOrDefault();

            return item;
        }
        public void UpdateMeasurement(Measurement md)
        {
            //   SAMSData.SAMSEntities se1 = new    SAMSData.SAMSEntities();

            Measurement mData = (from m in se.Measurements
                                 where m.MeasurementID == md.MeasurementID
                                 select m).FirstOrDefault();
            mData.MeasurementName = md.MeasurementName;
            mData.MeasurementValue = md.MeasurementValue;
            mData.MeasurementMasterValue = md.MeasurementMasterValue;
            se.SaveChanges();
            
            
        }
        public void SaveMeasurement(Measurement md)
        {
            se.Measurements.AddObject(md);
            se.SaveChanges();
        }

        public ProductGroup  GetProductGroup(int id)
        {
           
            ProductGroup item = (from m in se.ProductGroups
                                where m.ProductGroupID  == id
                                select m).FirstOrDefault();
            
            return item;
        }

        public IQueryable<Section> GetAllSections()
        {
            
            IQueryable<Section> sectionList = from s in se.Sections
                                              select s;
            
            return sectionList;
        }

        public IQueryable<Section> GetSections(int businessID)
        {
            
            IQueryable<Section> sectionList = from s in se.Sections
                                              join b in se.BusinessSections on 
                                              s.SectionID equals b.Section.SectionID 
                                              select s;
            
            return sectionList;
        }


        public Section GetSection(int id)
        {   
            Section item = (from s in se.Sections
                            where s.SectionID==id
                            select s).FirstOrDefault();
            
            return item;

        }

      
        #endregion
        #region Sales
        public int GetNextBillNumber(string prefix, int businessID)
        {
            var billNumber = 0;
            try
            {
                billNumber = (from s in se.Sales
                              where s.BusinessID == businessID  
                              select s.BillNumber).Max();


                billNumber = billNumber + 1;
            }
            catch
            {
                billNumber = 1;
            }

            billNumber = (billNumber == null ? 1 : billNumber);

            return billNumber;
        }

        public int GetNextManualBillNumber(int businessId,int sectionId)
        {
            var billNumber = 0;
            try
            {

                var mList = (from s in se.Sales
                             where s.BusinessID == businessId && s.SectionID == sectionId && s.ManualBillNumber !=null && !s.ManualBillNumber.Contains("D") && s.ManualBillNumber.Trim() !="" 
                                                     select s.ManualBillNumber.Trim()).ToList();
                


                //billNumber = (from c in mList
                  //            select Convert.ToInt32(c.Bno)).Max();
                if (mList.Count() != 0)
                {
                    billNumber = mList.Select(int.Parse).ToList().Max();

                    billNumber = billNumber + 1;
                }
            }
            catch
            {
                billNumber = 1;
            }

            billNumber = (billNumber == 0 ? 1 : billNumber);

            return billNumber;
        }


        private bool IsNumeric(string bno)
        {
            try
            {
                
                var tmp = Convert.ToInt32(bno);
                if( tmp>0)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        public double? GetStockAvailability(int productID, string pmodel,string psize)
        {
            double? purQty = 0;
            try
            {
                purQty = (from p in se.PurchaseDetails
                          where p.ProductID == productID && p.BatchNumber == pmodel && p.Size == psize
                          select p.Quantity).Sum();
            }
            catch
            {
                purQty = 0;
            }

                double? saleQty = 0;
                try
                {
                    saleQty = (from s in se.SaleDetails
                               where s.ProductID == productID && s.BatchNumber == pmodel && s.Size == psize
                               select s.Quantity).Sum();
                }
            catch {
                saleQty = 0;
                }

            purQty = (purQty == null ? 0 : purQty);

            saleQty = (saleQty == null ? 0 : saleQty);

            
            return purQty - saleQty;
        }


        public Sale SaveSale(Sale saleData)
        {
            if (saleData.SalesID > 0)
            {
                return UpdateSale(saleData);
            }
            else
            {
                saleData.BillNumber = GetNextBillNumber("",1);
                saleData.BusinessID = 1;
                saleData.SectionID = 1;
                se.AddToSales(saleData);
                se.SaveChanges();
                return saleData;
            }
        }

        public Sale GetSale(int id)
        {
            Sale sale = (from p in se.Sales
                        where p.SalesID == id
                         select p).FirstOrDefault();

            return sale;
        }

        public List<Sale> GetSaleList()
        {
            var list = se.Sales.ToList();
            return list;
        }
        public Sale UpdateSale(Sale saleData)
        {

            using (SAMSEntities se1 = new SAMSEntities())
            {

                Sale pur = (from p in se1.Sales
                            where p.SalesID == saleData.SalesID
                            select p).FirstOrDefault();
                pur.BusinessID = 1;
                pur.SectionID = 1;

                pur.TransactionDate = saleData.TransactionDate;
                pur.CustomerName = saleData.CustomerName;
                pur.Place = saleData.Place;
                pur.Phone = saleData.Phone;
                pur.SaleCategory = saleData.SaleCategory;
                pur.BillNumber = saleData.BillNumber;

                pur.Company = saleData.Company;
                pur.Itemcount = saleData.Itemcount;
                pur.kgs = saleData.kgs;
                pur.Cost = saleData.Cost;
                pur.BillAmount = saleData.BillAmount;
                pur.Comments = saleData.Comments;
                pur.PaymentDays = saleData.PaymentDays;
                pur.PaymentDate = saleData.PaymentDate;

                se1.SaveChanges();
                return saleData;


            }
        }



            #endregion


            #region Accounts

            public IQueryable<Ledger> GetLedgersByPlace(string place, int businessID)
        {
            IQueryable<Ledger> ledgerList = from l in se.Ledgers
                                             .Include("LedgerGroup")
                                            where l.Place.Contains(place) && l.MasterBusinessID == businessID
                                            orderby l.LedgerName
                                            select l;

            return ledgerList;
        }

        public IQueryable GetLedgerPlaces()
        {
            IQueryable placeList = (from l in se.Ledgers
                                    orderby l.Place
                                    select l.Place).Distinct();

            return placeList;
        }

        public IQueryable GetLedgerDistricts()
        {
            IQueryable placeList = (from l in se.Ledgers
                                    orderby l.Other1 
                                    select l.Other1 ).Distinct();

            return placeList;
        }

        //public IQueryable<Ledger> GetLedgers(int ledgerGroupID)
        //{
        //    IQueryable<Ledger> ledgerList = from l in se.Ledgers
        //                                    where l.LedgerGroup.LedgerGroupID == ledgerGroupID
        //                                    orderby l.LedgerName 
        //                                    select l;

        //    return ledgerList;
        //}


        public IQueryable<Ledger> GetLedgers(string ledgerName, int businessID)
        {
            IQueryable<Ledger> ledgerList = from l in se.Ledgers
                                             .Include("LedgerGroup")
                                            where l.LedgerName.Contains(ledgerName) && l.MasterBusinessID == businessID
                                            orderby  l.LedgerName
                                            select l;

            return ledgerList;
        }


        //public List<Ledger> GetOustandingReport(int businessID, DateTime toDate, List<int> ledgerGroupIDs, int type=0)
        //{

        //    List<Ledger> ledgerList = new List<Ledger>();
        //    IQueryable<Ledger> legList;

        //    //var ledgerGroupList =  ledgerGroupIDs.Split(',').Select(Int32.Parse).ToList();

        //    if (ledgerGroupIDs.Count == 0)
        //    {
        //        legList = (from l in se.Ledgers
        //                   where l.MasterBusinessID == businessID && l.LedgerID != App.CashLedger
        //                   orderby l.Place,l.LedgerName
        //                   select l);
        //    }
        //    else
        //    {
        //        legList = (from l in se.Ledgers
        //                   where l.MasterBusinessID == businessID && ledgerGroupIDs.Contains(l.LedgerGroupID)
        //                   orderby l.Place, l.LedgerName
        //                   select l);
        //    }

        //  var  ledgerOutStandings = (from lj in se.LedgerJournals
        //                           where lj.TransactionDate<=toDate
        //                           group lj by new { lj.LedgerID}
        //                               into grp
        //                               select new   {
        //                                   LedgerID = grp.Key.LedgerID,                                           
        //                                   DrAmount =grp.Sum(x => x.DrAmount),
        //                                   CrAmount= grp.Sum(x => x.CrAmount)                                          
        //                               }).ToList();

        //  //var ledgerOutStandings = (from lj in se.LedgerJournals
        //  //                          join l in legList on lj.LedgerID equals l.LedgerID
        //  //                          where lj.TransactionDate <= toDate
        //  //                          group lj by new { l.LedgerID, l.LedgerName, l.Place, l.OpenningBalanceType, l.OpenningBalance }
        //  //                              into grp
        //  //                              select new
        //  //                              {
        //  //                                  LedgerID = grp.Key.LedgerID,
        //  //                                  LedgerName = grp.Key.LedgerName,
        //  //                                  Place = grp.Key.Place,
        //  //                                  OBType = grp.Key.OpenningBalanceType,
        //  //                                  OB = grp.Key.OpenningBalance,
        //  //                                  DrAmount = grp.Sum(x => x.DrAmount),
        //  //                                  CrAmount = grp.Sum(x => x.CrAmount)
        //  //                              }).ToList();
            

         

        //    foreach (var lg in legList)
        //    {
        //        double CrAmount = 0, DrAmount = 0; 
        //        //double CrAmount = Convert.ToDouble(lg.CrAmount), DrAmount = Convert.ToDouble(lg.DrAmount); 
        //          try
        //            {
        //                if (lg.OpenningBalanceType == 1)
        //                    CrAmount  += Convert.ToDouble(lg.OpenningBalance);
        //                else
        //                    DrAmount  += Convert.ToDouble( lg.OpenningBalance);
                        
        //               // CrAmount = Convert.ToDouble(lg.CrAmount); //+ Convert.ToDouble(lg.OB);
        //                //else
        //                 //   DrAmount = Convert.ToDouble(lg.DrAmount); //+ Convert.ToDouble( lg.OB);

        //                var lgDet = ledgerOutStandings.FirstOrDefault(x=>x.LedgerID == lg.LedgerID);
        //                if (lgDet != null)
        //                {
        //                    CrAmount += lgDet.CrAmount;
        //                    DrAmount += lgDet.DrAmount;
        //                }


        //            }
        //        catch {
        //        }

        //        double ledBalance = DrAmount - CrAmount;
        //        if (ledBalance != 0)
        //        {
        //            Ledger lg1 = new Ledger();
        //            lg1.LedgerID = lg.LedgerID;
        //            lg1.LedgerName = lg.LedgerName;
        //            lg1.Place = lg.Place;
        //            lg1.Address = lg.Address;
        //            lg1.Phone1 = lg.Phone1;
        //            lg1.LedgerGroupID = lg.LedgerGroupID;
        //            lg1.Other1 = lg.Other1;
        //         //   lg1.Notes=lg.Notes;

        //            if (ledBalance > 0)
        //            {
        //                lg1.OpenningBalance = ledBalance;
        //                lg1.OpenningBalanceType = 2;
        //            }
        //            else if (ledBalance < 0)
        //            {
        //                lg1.OpenningBalance = Math.Abs(ledBalance);
        //                lg1.OpenningBalanceType = 1;
        //            }

        //            if (type==0 || lg1.OpenningBalanceType==type)
        //                ledgerList.Add(lg1);
        //        }
        //    }
                
        //    return ledgerList;
        //}

      

        public double  GetLedgerOB(int businessID, int ledgerID, DateTime fromDate)
        {            
            IQueryable<LedgerJournal> ledjrList = (from l in se.LedgerJournals
                                                   where l.BusinessID == businessID && l.LedgerID == ledgerID
                                                   && l.TransactionDate < fromDate 
                                                   orderby l.TransactionDate
                                                   select l);
            double ob = 0,obCrAmt=0, obDrAmt=0;

            Ledger lg = (from a in se.Ledgers
                         where a.LedgerID == ledgerID
                         select a).FirstOrDefault();

            if (lg.OpenningBalanceType == 1)
            {
                try
                {
                    obCrAmt = Convert.ToDouble(lg.OpenningBalance);
                }
                catch
                {
                    obCrAmt = 0;
                }
            }
            else
            {
                try
                {
                    obDrAmt = Convert.ToDouble(lg.OpenningBalance);
                }
                catch
                {
                    obDrAmt = 0;
                }
            }


            if (ledjrList != null)
            {
                double DrAmount = 0;
                try
                {
                     DrAmount = ledjrList.Sum(m => m.DrAmount);
                }
                catch { }

                double CrAmount = 0;
                try
                {
                     CrAmount = ledjrList.Sum(m => m.CrAmount);
                }
                catch { }
               
                DrAmount += obDrAmt;
                CrAmount += obCrAmt;
                ob = DrAmount - CrAmount;
            }


            return ob;
        }

        public DateTime? GetCustomerLedgerStartDate()
        {
            DateTime? fdate = (from a in se.LedgerJournals
                               select a.TransactionDate).Min();
            return fdate;
        }

        public DateTime? GetCustomerLedgerStartDate(int ledgerID)
        {
            DateTime? fdate = null;
            try
            {
                fdate = (from a in se.LedgerJournals
                         where a.LedgerID == ledgerID
                         select a.TransactionDate).Min();

            }
            catch
            {
                fdate = null;
            }
            return fdate;
        }


        public IQueryable<Ledger> GetCustomerList(string place)
        {
            IQueryable<Ledger> custList;
            if (place == "")
            {
                custList = from a in se.Ledgers
                           orderby place, a.LedgerName
                           select a;
            }
            else
            {
                custList = from a in se.Ledgers
                           orderby place, a.LedgerName
                           where place.Contains(a.Place)
                           select a;
            }

            return custList;
        }


        public bool IsBalanceDueforDays(double ndays, int ledID)
        {
            bool outstandingFlag = false;

            try
            {
                DateTime lastDebitDate = (from a in se.LedgerJournals
                                          where a.DrAmount > 0 && a.LedgerID == ledID
                                          select a.TransactionDate).Max();

                double outDays = GetNumberOfDays(lastDebitDate, DateTime.Now);
                if (outDays >= ndays)
                    outstandingFlag = true;
            }
            catch { }

            return outstandingFlag;
        }

        private double GetNumberOfDays(DateTime fromDate, DateTime toDate)
        {
            double noDays = 0;

            TimeSpan ts = toDate - fromDate;
            noDays = ts.TotalDays;

            return noDays;
        }
        public double GetLedgerBalance(int ledID)
        {
            double amt = 0;

            IQueryable<LedgerJournal> ledList = from a in se.LedgerJournals
                                                where a.LedgerID == ledID
                                                select a;
            double crAmt = 0;
            try
            {
                crAmt = ledList.Select(x => x.CrAmount).Sum();
            }
            catch
            {
                crAmt = 0;
            }
            double drAmt = 0;
            try
            {
                drAmt = ledList.Select(x => x.DrAmount).Sum();
            }
            catch
            {
                drAmt = 0;
            }

            Ledger lgr = se.Ledgers.First(x => x.LedgerID == ledID);

            if (lgr.OpenningBalance != null && lgr.OpenningBalanceType!=null)
            {
                if (lgr.OpenningBalanceType == 1)
                    crAmt += Convert.ToDouble( lgr.OpenningBalance);
                else
                    drAmt += Convert.ToDouble( lgr.OpenningBalance);
            }

            amt = crAmt - drAmt;

            return amt;
        }


        public List<LedgerJournalView> GetLedgerJournalswithOB(int businessID, int ledgerID, DateTime fromDate, DateTime toDate)
        {

            IQueryable<LedgerJournal> ledjrListOB = (from l in se.LedgerJournals
                                                   where l.BusinessID == businessID && l.LedgerID == ledgerID
                                                   && l.TransactionDate < fromDate
                                                   orderby l.TransactionDate
                                                   select l);
            double ob = 0, obCrAmt = 0, obDrAmt = 0;

            Ledger lg = (from a in se.Ledgers
                         where a.LedgerID == ledgerID
                         select a).FirstOrDefault();

            if (lg.OpenningBalanceType == 1)
            {
                try
                {
                    obCrAmt = Convert.ToDouble(lg.OpenningBalance);
                }
                catch
                {
                    obCrAmt = 0;
                }
            }
            else
            {
                try
                {
                    obDrAmt = Convert.ToDouble(lg.OpenningBalance);
                }
                catch
                {
                    obDrAmt = 0;
                }
            }

                double DrAmount = 0, CrAmount = 0;
                try
                {

                    DrAmount = ledjrListOB.Sum(m => m.DrAmount);
                }
                catch
                {
                    DrAmount = 0;
                }
                try
                {
                    CrAmount = ledjrListOB.Sum(m => m.CrAmount);
                }
                catch
                {
                    CrAmount = 0;
                }
                DrAmount += obDrAmt;
                CrAmount += obCrAmt;
                ob = DrAmount - CrAmount;



                List<LedgerJournalView> ledjrList = (from l in se.LedgerJournals
                                             where l.BusinessID == businessID && l.LedgerID == ledgerID
                                             && l.TransactionDate >= fromDate && l.TransactionDate <= toDate
                                             orderby l.TransactionDate
                                             select new LedgerJournalView
                                             {
                                                 TransactionDate=l.TransactionDate,
                                                 LedgerID=l.LedgerID,
                                                 BusinessID=l.BusinessID,
                                                 ChequeNo=l.ChequeNo,
                                                 Narration=l.Narration,
                                                 EntryType=l.EntryType,
                                                 CrAmount=l.CrAmount,
                                                 DrAmount=l.DrAmount,
                                                 LedgerJournalID=l.LedgerJournalID,
                                                 ReferenceID=l.ReferenceID,
                                             //    interest=l.interest,
                                             //    EntryGroup = l.EntryGroup
                                                
                                             }
                                             ).ToList();
                LedgerJournalView lj = new LedgerJournalView();
            lj.Narration = "Openning Balance";
            lj.TransactionDate = Convert.ToDateTime(lg.CreatedDate);

            if (ob >= 0)
            {
                lj.DrAmount = ob;
               
            }
            else
            {
                lj.CrAmount = Math.Abs(ob);
            }

            ledjrList.Add(lj);

            return ledjrList.OrderByDescending(m=>m.TransactionDate).ToList();
        }

        
        public IQueryable<LedgerJournal> GetLedgerJournals(int businessID, DateTime fromDate, DateTime toDate)
        {
            IQueryable<LedgerJournal> ledjrList = (from l in se.LedgerJournals
                                                   .Include("Ledger")
                                                   where l.BusinessID == businessID 
                                                   && l.TransactionDate >= fromDate && l.TransactionDate <= toDate
                                                   orderby l.TransactionDate,l.LedgerJournalID
                                                   select l);
            return ledjrList;
        }



        //public double GetDaybookOB(int businessID, DateTime fromDate)
        //{
        //    try
        //    {
        //        var ledCashList = (from l1 in se.LedgerJournals
        //                           where l1.BusinessID == businessID
        //                           && l1.TransactionDate < fromDate && l1.LedgerID == App.CashLedger
        //                           orderby l1.TransactionDate
        //                           select new { l1.EntryType, l1.ReferenceID });

        //        IQueryable<LedgerJournal> ledjrList = (from l in se.LedgerJournals
        //                                               join p in ledCashList on new { EntryType = l.EntryType, ReferecenceId = l.ReferenceID }
        //                                               equals new { EntryType = p.EntryType, ReferecenceId = p.ReferenceID }
        //                                               where l.BusinessID == businessID
        //                                               && l.TransactionDate < fromDate && l.LedgerID != App.CashLedger
        //                                               orderby l.TransactionDate
             
        //                                               select l);

        //        Ledger led = se.Ledgers.Where(x => x.LedgerID == App.CashLedger).FirstOrDefault();

        //        double debitAmt = 0, creditAmt=0;

        //        try
        //        {
        //            if (led.OpenningBalanceType == 1)
        //                creditAmt = Convert.ToDouble( led.OpenningBalance);
        //            else
        //                debitAmt = Convert.ToDouble( led.OpenningBalance);
        //        }
        //        catch { }

        //        if (ledjrList.Count() > 0)
        //        {
        //            debitAmt += ledjrList.Sum(x => x.DrAmount);
        //            creditAmt += ledjrList.Sum(x => x.CrAmount);
        //        }


        //            return creditAmt - debitAmt;
                

        //    }
        //    catch {
        //        return 0;
        //    }

        //    //List<Daybook> daybookList = new List<Daybook>();
        //    //foreach (LedgerJournal lj in ledjrList)
        //    //{
        //    //    var cnt = (from p in ledCashList
        //    //               where p.EntryType == lj.EntryType && p.ReferenceID == lj.ReferenceID
        //    //               select p).Count();
        //    //    Daybook db = new Daybook();
        //    //    db.LedgerName = lj.Ledger.LedgerName;
        //    //    db.Place = lj.Ledger.Place;
        //    //    db.TransactionDate = lj.TransactionDate;
        //    //    db.EntryType = lj.EntryType;
        //    //    db.Narration = lj.Narration;
        //    //    db.ChequeNo = lj.ChequeNo;
        //    //    db.BusinessID = lj.BusinessID;
        //    //    db.CrAmount = lj.CrAmount;
        //    //    db.DrAmount = lj.DrAmount;
        //    //    db.LedgerID = lj.LedgerID;
        //    //    db.LedgerJournalID = lj.LedgerJournalID;
        //    //    if (cnt > 0)
        //    //    {
        //    //        db.CrAmount1 = db.CrAmount;
        //    //        db.DrAmount1 = db.DrAmount;
        //    //        db.CrAmount = 0;
        //    //        db.DrAmount = 0;
        //    //        db.Type = 2;
        //    //    }
        //    //    else
        //    //    {
        //    //        db.Type = 1;
        //    //    }

        //    //    daybookList.Add(db);
        //    //}
        //}

      

        public IQueryable<LedgerJournal> GetLedgerJournals(int businessID,int ledgerID, DateTime fromDate, DateTime toDate)
        {
           
            IQueryable<LedgerJournal> ledjrList = (from l in se.LedgerJournals
                                                   where l.BusinessID == businessID && l.LedgerID == ledgerID
                                                   && l.TransactionDate >= fromDate && l.TransactionDate <= toDate
                                                   orderby l.TransactionDate                                                   
                                                   select l);
            return ledjrList;
        }

        public IQueryable<LedgerView> GetLedgers(int businessID)
        {
            IQueryable<LedgerView> ledgerList = from l in se.Ledgers
                                            .Include("LedgerGroup")
                                            where l.MasterBusinessID == businessID
                                            orderby l.LedgerName
                                            select new LedgerView { 
                                                LedgerID=l.LedgerID,
                                                LedgerGroupID=l.LedgerGroupID,
                                                LedgerName=l.LedgerName,
                                                Address=l.Address,
                                                Place=l.Place,
                                                Phone1=l.Phone1,
                                                Phone2=l.Phone2,
                                                Email=l.Email,
                                                GroupName=l.LedgerGroup.GroupName,
                                                CreatedDate=l.CreatedDate,
                                                OpenningBalance=l.OpenningBalance,
                                                OpenningBalanceType=l.OpenningBalanceType,
                              //                  CreditLimit=l.CreditLimit,
                              //                  Notes=l.Notes,
                                                Other1=l.Other1
                                            };

            return ledgerList;
        }


        public IQueryable<LedgerView> GetLedgers(List<int> ledgerGroupID, int businessID)
        {

            IQueryable<LedgerView> ledgerList = from l in se.Ledgers
                                            .Include("LedgerGroup")
                                            where ledgerGroupID.Contains(l.LedgerGroup.LedgerGroupID) && l.MasterBusinessID == businessID 
                                            orderby l.LedgerName
                                            select new LedgerView
                                            {
                                                LedgerID = l.LedgerID,
                                                LedgerGroupID = l.LedgerGroupID,
                                                LedgerName = l.LedgerName,
                                                Address = l.Address,
                                                Place = l.Place,
                                                Phone1 = l.Phone1,
                                                Phone2 = l.Phone2,
                                                Email = l.Email,
                                                GroupName = l.LedgerGroup.GroupName,
                                                CreatedDate = l.CreatedDate,
                                                OpenningBalance = l.OpenningBalance,
                                                OpenningBalanceType = l.OpenningBalanceType,
                                                OBTypeName = l.OpenningBalanceType==1?"Cr":"Dr",
                                         //       CreditLimit = l.CreditLimit,
                                        //        Notes = l.Notes,
                                                Other1 = l.Other1
                                            };

            return ledgerList;
        }

        public IQueryable<LedgerView> GetLedgers(int businessID, int ledgerID)
        {

            IQueryable<LedgerView> ledgerList = from l in se.Ledgers
                                            .Include("LedgerGroup")
                                                where  l.MasterBusinessID == businessID && l.LedgerID== ledgerID
                                                orderby l.LedgerName
                                                select new LedgerView
                                                {
                                                    LedgerID = l.LedgerID,
                                                    LedgerGroupID = l.LedgerGroupID,
                                                    LedgerName = l.LedgerName,
                                                    Address = l.Address,
                                                    Place = l.Place,
                                                    Phone1 = l.Phone1,
                                                    Phone2 = l.Phone2,
                                                    Email = l.Email,
                                                    GroupName = l.LedgerGroup.GroupName,
                                                    CreatedDate = l.CreatedDate,
                                                    OpenningBalance = l.OpenningBalance,
                                                    OpenningBalanceType = l.OpenningBalanceType,
                                                    OBTypeName = l.OpenningBalanceType == 1 ? "Cr" : "Dr",
                                                  //  CreditLimit = l.CreditLimit,
                                                    Other1=l.Other1,
                                                    Other2 =l.Other2,
                                             //       Notes=l.Notes
                                                };

            return ledgerList;
        }

        public Ledger GetLedger(int ledgerID)
        {
            Ledger ledgerItem= (from l in se.Ledgers
                                .Include("LedgerGroup")
                                where l.LedgerID == ledgerID
                                select l).FirstOrDefault(); ;

            return ledgerItem;
        }
        
        public void SaveLedgerGroup(LedgerGroup lg)
        {
            se.LedgerGroups.AddObject(lg);
            se.SaveChanges();
        }

        public void UpdateLedgerGroup(LedgerGroup lg)
        {
            LedgerGroup ledgerGroupItem = (from l in se.LedgerGroups
                                    where l.LedgerGroupID == lg.LedgerGroupID
                                 select l).FirstOrDefault(); ;

            ledgerGroupItem.GroupName = lg.GroupName;
            ledgerGroupItem.GroupType = lg.GroupType;
            ledgerGroupItem.Status = lg.Status;
            se.SaveChanges();
        }


        public IQueryable<GroupDetail> GetLedgerGroups()
        {
            IQueryable<GroupDetail> grpList = from g in se.LedgerGroups
                                              orderby g.GroupName
                                              select new GroupDetail
                                              {
                                                  GroupName = g.GroupName,
                                                  LedgerGroupID = g.LedgerGroupID,
                                                  GroupType = g.GroupType,
                                                  GroupDescription = g.GroupName
                                              };
            return grpList;
        }


        public IQueryable<LedgerGroup> GetLedgerGroup(int id)
        {
            IQueryable<LedgerGroup> grpList = from g in se.LedgerGroups
                                              where g.LedgerGroupID==id
                                              orderby g.GroupName
                                              select g;
            return grpList;
        }



        public bool IsLedgerExists(int ledgerID, int businessID,string ledgerName,string place)
        {
            Ledger led = (from s in se.Ledgers
                          where s.LedgerID != ledgerID && s.LedgerName == ledgerName && s.Place==place && s.MasterBusinessID == businessID
                          select s).FirstOrDefault();

            if (led != null)
                return true;
            else
                return false;
        }

        public Ledger SaveLedger(Ledger item)
        {

            se.Ledgers.AddObject(item);// .AddObject("Ledgers", item);
            se.SaveChanges();

            return item;
        }

        public Ledger UpdateLedger(Ledger item)
        {
               SAMSData.SAMSEntities se1 = new    SAMSData.SAMSEntities() ;

            Ledger ledData = (from l in se1.Ledgers 
                            where l.LedgerID==item.LedgerID
                             select l).FirstOrDefault();


            ledData.LedgerGroupID = item.LedgerGroupID;
            ledData.LedgerName = item.LedgerName;
            ledData.Address = item.Address;
            ledData.Place = item.Place;
            ledData.State = item.State;
            ledData.Pin = item.Pin;
            ledData.Phone1 = item.Phone1;
            ledData.Email = item.Email;
            ledData.CreatedDate = item.CreatedDate;
            ledData.PAN = item.PAN;
            ledData.TIN = item.TIN;
            ledData.STax = item.STax;
            ledData.CST = item.CST;
      //      ledData.AGLNo = item.AGLNo;
      //      ledData.PLNo = item.PLNo;
            ledData.OpenningBalance = item.OpenningBalance;
            ledData.OpenningBalanceType = item.OpenningBalanceType;
            ledData.Status = item.Status;
            ledData.Other1 = item.Other1;
    //        ledData.CreditLimit = item.CreditLimit;
     //       ledData.Notes = item.Notes;
            //ledData.Other2 = item.Other2;
            
            se1.SaveChanges();

            return ledData ;
        }

        public IQueryable<PurchaseView> GetPurchaseList()
        {
            IQueryable<PurchaseView> pList;

            pList = from p in se.Purchases
                                         .Include("Ledger")
                                         .Include("Section")
                    orderby p.PurchaseID descending
                    select new PurchaseView {
                        PurchaseID = p.PurchaseID,
                        VendorID = p.VendorID,
                //        OtherId1 = p.OtherId1,
                        InvoiceNumber = p.InvoiceNumber,
                        InvoiceDate = p.InvoiceDate,
                        NetAmount = p.NetAmount,
                        FreightandHamali = p.FreightandHamali,
                        InvoiceAmount = p.InvoiceAmount,
                        Vendor = p.Ledger.LedgerName
                          };

            return pList;
        }

        public IQueryable<Purchase> GetPurchaseList(DateTime? fromDate, DateTime? toDate)
        {
            IQueryable<Purchase> pList;

            {
                pList = from p in se.Purchases
                       .Include("Ledger")
                       .Include("Section")
                        where p.TransactionDate >= fromDate && p.TransactionDate <= toDate && p.BillType != 3
                        orderby p.InvoiceDate, p.PurchaseID
                        select p;
            }

            return pList;
        }

        public PurchaseView GetPurchase(int purchaseID)
        {
            Purchase pur;

            pur = (from p in se.Purchases
                             .Include("Ledger")
                             .Include("Section")
                             .Include("PurchaseDetails")
                   where p.PurchaseID == purchaseID
                   select p).FirstOrDefault();

            PurchaseView purView = new PurchaseView
            {
                PurchaseID = pur.PurchaseID,
                VendorID = pur.VendorID,
               // OtherId1 = pur.OtherId1,
                InvoiceNumber = pur.InvoiceNumber,
                InvoiceDate = pur.InvoiceDate,
                NetAmount = pur.NetAmount,
                FreightandHamali = pur.FreightandHamali,
                InvoiceAmount = pur.InvoiceAmount,
                Vendor = pur.Ledger.LedgerName                                           
            };

            purView.PurchaseDetailsView = new List<PurchseDetailsView>();
            int sno = 1;
            foreach(var pd in pur.PurchaseDetails)
            {
                purView.PurchaseDetailsView.Add(new PurchseDetailsView
                {
                    PurchaseDetailsID = pd.PurchaseDetailsID,
                    PurchaseID = pd.PurchaseID,
                    ProductID = pd.ProductID,
                    Quantity = pd.Quantity,
                    PurchasePrice = pd.PurchasePrice,
                    Size = pd.Size,
                    Sno = sno,
                    Amount = pd.Quantity * pd.PurchasePrice
                });
                sno++;
            }

                        return purView;
        }

      

        public void SavePurchase(Purchase purchaseData)
        {
            se.Purchases.AddObject(purchaseData);
            se.SaveChanges();

            string referenceText = "";
            //try
            //{
            //   var branch= GetLedgerGroup(Convert.ToInt16( purchaseData.OtherId1)).FirstOrDefault();

            //    if (branch != null)
            //    {
            //        referenceText = branch.GroupName;
            //    }
            //}
            //catch {  }

            if (purchaseData.PurchaseID > 0 && purchaseData.OtherCharges2 != 1)
            {
                List<LedgerJournal> ledgerJournals = GetPurchaseLedgerJournals(purchaseData, referenceText);
                foreach (LedgerJournal lj in ledgerJournals)
                {
                    se.LedgerJournals.AddObject(lj);
                }

                if (ledgerJournals.Count > 0)
                {
                    se.SaveChanges();
                }
            }

        }


        private List<LedgerJournal> GetPurchaseLedgerJournals(Purchase purchaseData, string referenceText)
        {
            List<LedgerJournal> ledgerJournals = new List<LedgerJournal>();

            LedgerJournal lj1 = new LedgerJournal();
            lj1.ReferenceID = purchaseData.PurchaseID;
            lj1.BusinessID = Convert.ToInt16(purchaseData.BusinessID);
            lj1.TransactionDate = Convert.ToDateTime(purchaseData.TransactionDate);
            lj1.LedgerID = Convert.ToInt16(purchaseData.VendorID);
            lj1.Narration = "By Inv.No." + purchaseData.InvoiceNumber ;
            if (referenceText.Length >0)
            {
                lj1.Narration += " For " + referenceText;
            }
            lj1.ChequeNo = "";//purchaseData.Details1;
            lj1.EntryType = "Purchase";
            lj1.CrAmount = Convert.ToDouble(purchaseData.InvoiceAmount);
          //  lj1.EntryGroup = "Purchase";

            ledgerJournals.Add(lj1);
            //Hide Tax separate ledger
            // taxAmount = 0;
            
            //LedgerJournal lj2 = new LedgerJournal();
            //lj2.ReferenceID = purchaseData.PurchaseID;
            //lj2.BusinessID = Convert.ToInt16(purchaseData.BusinessID);
            //lj2.TransactionDate = Convert.ToDateTime(purchaseData.TransactionDate);
            //lj2.LedgerID = GetConfigLedger("PurchaseLedger");
            //lj2.Narration = "To Inv.No. " + purchaseData.InvoiceNumber;
            //if (referenceText.Length > 0)
            //{
            //    lj2.Narration += " For " + referenceText;
            //}
            //lj2.ChequeNo = purchaseData.Details1;
            //lj2.EntryType = "Purchase";
            //lj2.DrAmount = Convert.ToDouble(purchaseData.InvoiceAmount );
            //lj2.EntryGroup = "General";
            //ledgerJournals.Add(lj2);

            if (purchaseData.FreightandHamali > 0)
            {
                LedgerJournal lj3 = new LedgerJournal();
                lj3.ReferenceID = purchaseData.PurchaseID;
                lj3.BusinessID = Convert.ToInt16(purchaseData.BusinessID);
                lj3.TransactionDate = Convert.ToDateTime(purchaseData.TransactionDate);
                lj3.LedgerID = Convert.ToInt16(purchaseData.VendorID); 
                lj3.Narration = "By Inv.No. " + purchaseData.InvoiceNumber;
                if (referenceText.Length > 0)
                {
                    lj3.Narration += " For " + referenceText;
                }
                lj3.ChequeNo = ""; // purchaseData.Details1;
                lj3.EntryType = "Purchase";
              //  lj3.EntryGroup = "Hamali";
                lj3.DrAmount= Convert.ToDouble(purchaseData.FreightandHamali);
                ledgerJournals.Add(lj3);

                var groupLedgerId = 0;// GetLedgerGroupCashLedger(Convert.ToInt16( purchaseData.OtherId1));
                if (groupLedgerId >0)
                {
                    LedgerJournal lj4 = new LedgerJournal();
                    lj4.ReferenceID = purchaseData.PurchaseID;
                    lj4.BusinessID = Convert.ToInt16(purchaseData.BusinessID);
                    lj4.TransactionDate = Convert.ToDateTime(purchaseData.TransactionDate);
                    lj4.LedgerID = groupLedgerId;
                    lj4.Narration = "To Inv.No. " + purchaseData.InvoiceNumber;
                    if (referenceText.Length > 0)
                    {
                        lj4.Narration += " For " + referenceText;
                    }
                    lj4.ChequeNo = ""; // purchaseData.Details1;
                    lj4.EntryType = "Purchase";
                 //   lj4.EntryGroup = "Hamali";
                    lj4.DrAmount = Convert.ToDouble(purchaseData.FreightandHamali);
                    ledgerJournals.Add(lj4);
                }
            }
            return ledgerJournals;
        }

        private int GetLedgerGroupCashLedger(int ledgerGroupId)
        {
            var ledger = (from l in se.Ledgers
                          where l.LedgerGroupID == ledgerGroupId && l.Other1.Equals("Cash")
                          select l).FirstOrDefault();
            if (ledger != null)
                return ledger.LedgerID;
            else
                return 0;
        }

        private int GetConfigLedger(string key)
        {
            var config = se.SAMSConfigurations.Where(x => x.KeyName == key).FirstOrDefault();
            if (config != null)
            {
                return Convert.ToInt32(config.KeyValue);
            }
            else
                return 0;
        }

        public void UpdatePurchase(Purchase purchaseData)
        {
            string referenceText = "";
            //try
            //{
            //    var branch = GetLedgerGroup(Convert.ToInt16(purchaseData.OtherId1)).FirstOrDefault();

            //    if (branch != null)
            //    {
            //        referenceText = branch.GroupName;
            //    }
            //}
            //catch { }


            using (SAMSData.SAMSEntities se1 = new   SAMSData.SAMSEntities())
            {

                Purchase pur = (from p in se1.Purchases
                                .Include("PurchaseDetails")
                                where p.PurchaseID == purchaseData.PurchaseID
                                select p).FirstOrDefault();

                pur.BusinessID = purchaseData.BusinessID;
                pur.SectionID = purchaseData.SectionID;
                pur.VendorID = purchaseData.VendorID;
                pur.TransactionDate = purchaseData.TransactionDate;
                pur.InvoiceNumber = purchaseData.InvoiceNumber;
                pur.InvoiceDate = purchaseData.InvoiceDate;
                pur.OtherCharges2 = purchaseData.OtherCharges2;
                pur.DCNumber = purchaseData.DCNumber;
                pur.DCDate = purchaseData.DCDate;
                pur.PurchaseCategory = purchaseData.PurchaseCategory;
                pur.BillType = purchaseData.BillType;
                pur.NetAmount = purchaseData.NetAmount;
                pur.InvoiceAmount = purchaseData.InvoiceAmount;
                try
                {
                    pur.FreightandHamali = purchaseData.FreightandHamali;
                }
                catch
                {
                    pur.FreightandHamali = 0;
                }

                
                //try
                //{
                //    pur.OtherId1 = purchaseData.OtherId1;
                //}
                //catch { }

                se1.SaveChanges();

                //var pdList = new EntityCollection<PurchaseDetail>();

                var pdlist = (from p in pur.PurchaseDetails
                              select p).ToList();

                foreach (PurchaseDetail p in pdlist)
                {
                    var attachedChild = (from c in purchaseData.PurchaseDetails
                                         where c.PurchaseDetailsID == p.PurchaseDetailsID
                                         select c).FirstOrDefault();

                    if (attachedChild == null)
                    {
                        se1.PurchaseDetails.DeleteObject(p); 
                        se1.SaveChanges();
                    }
                }

                foreach (PurchaseDetail p in purchaseData.PurchaseDetails)
                {
                    var attachedChild = (from c in se1.PurchaseDetails
                                         where c.PurchaseDetailsID == p.PurchaseDetailsID
                                         select c).FirstOrDefault();

                    if (attachedChild != null)
                    {
                        // Existing child - apply new values
                        attachedChild.ProductID = p.ProductID;
                        attachedChild.BatchNumber = p.BatchNumber;
                        attachedChild.PurchasePrice = p.PurchasePrice;
                        attachedChild.Quantity = p.Quantity;
                        attachedChild.SalePrice = p.SalePrice;
                        attachedChild.Size = p.Size;
                        attachedChild.Tax = p.Tax;
                        attachedChild.TaxAmount = p.TaxAmount;
                        attachedChild.BarCode = p.BarCode;
                      //  attachedChild.MeasurementID = p.MeasurementID;
                        attachedChild.MfgDate = p.MfgDate;
                        attachedChild.ExpDate = p.ExpDate;
                        //attachedChild.PackingTypeID = p.PackingTypeID;
                        //attachedChild.PackingQty = p.PackingQty;
                        //attachedChild.CompanyId = p.CompanyId;
                        se1.SaveChanges();

                    }
                    else
                    {
                        // New child
                        // Don't insert original object. It will attach whole detached graph
                        PurchaseDetail pitem = new PurchaseDetail();
                        pitem.PurchaseID = pur.PurchaseID;
                        pitem.ProductID = p.ProductID;
                        pitem.BatchNumber = p.BatchNumber;
                        pitem.PurchasePrice = p.PurchasePrice;
                        pitem.Quantity = p.Quantity;
                        pitem.SalePrice = p.SalePrice;
                        pitem.Size = p.Size;
                        pitem.Tax = p.Tax;
                        pitem.TaxAmount = p.TaxAmount;
                        pitem.BarCode = p.BarCode;
                       // pitem.MeasurementID = p.MeasurementID;
                        pitem.MfgDate = p.MfgDate;
                        pitem.ExpDate = p.ExpDate;
                        //pitem.PackingTypeID = p.PackingTypeID;
                        //pitem.PackingQty = p.PackingQty;
                        //pitem.CompanyId = p.CompanyId;
                        se1.PurchaseDetails.AddObject(pitem);
                        se1.SaveChanges();
                    }

                }

                string etype = "Purchase";


                IQueryable<LedgerJournal> ljList = from l in se1.LedgerJournals
                                                   where l.ReferenceID == purchaseData.PurchaseID && l.EntryType == etype
                                                   select l;
                foreach (LedgerJournal l in ljList)
                {
                    se1.LedgerJournals.DeleteObject(l);
                }
                se1.SaveChanges();

                if (purchaseData.PurchaseID > 0 && purchaseData.OtherCharges2 != 1)
                {

                    List<LedgerJournal> ledgerJournals = GetPurchaseLedgerJournals(purchaseData, referenceText);
                    foreach (LedgerJournal lj in ledgerJournals)
                    {
                        se1.LedgerJournals.AddObject(lj);
                    }

                    if (ledgerJournals.Count > 0)
                    {
                        se1.SaveChanges();
                    }
                }
                
            }


        }

        public bool DeletePurchase(int purchaseId)
        {
            bool successFlag = false;
            using (SAMSData.SAMSEntities se1 = new   SAMSData.SAMSEntities())
            {

                Purchase pur = (from p in se1.Purchases
                                .Include("PurchaseDetails")
                                where p.PurchaseID == purchaseId
                                select p).FirstOrDefault();


                if (pur != null)
                {
                    string etype = "Purchase";


                    IQueryable<LedgerJournal> ljList = from l in se1.LedgerJournals
                                                       where l.ReferenceID == purchaseId && l.EntryType == etype
                                                       select l;
                    foreach (LedgerJournal l in ljList)
                    {
                        se1.LedgerJournals.DeleteObject(l);
                    }
                    se1.SaveChanges();

                    
                    var pdlist = (from p in pur.PurchaseDetails
                                  select p).ToList();

                    foreach (PurchaseDetail p in pdlist)
                    {

                        se1.PurchaseDetails.DeleteObject(p);
                        se1.SaveChanges();

                    }

                    se1.Purchases.DeleteObject(pur);
                    se1.SaveChanges();
                    successFlag = true;
                }

            }
            return successFlag;
        }



        public IQueryable<Voucher> GetVouchersList(int voucherType, int businessID)
        {
            IQueryable<Voucher> voucherList = from v in se.Vouchers
                                              .Include("Ledger")
                                              .Include("Ledger1")
                                           where v.VoucherType == voucherType && v.BusinessID == businessID
                                           orderby v.VoucherID descending 
                                           select v;

            return voucherList;
        }

        public IQueryable<Voucher> GetVouchersList(int voucherType, int businessID, int ledgerID)
        {
            IQueryable<Voucher> voucherList = from v in se.Vouchers
                                              .Include("Ledger")
                                              .Include("Ledger1")
                                              where v.VoucherType == voucherType && v.BusinessID == businessID && v.FromLedger == ledgerID
                                              orderby v.VoucherID descending
                                              select v;

            return voucherList;
        }


        public Voucher GetVoucher(int voucherID)
        {
            Voucher vItem = (from v in se.Vouchers
                                              .Include("Ledger")
                                              .Include("Ledger1")
                            where v.VoucherID == voucherID                            
                            select v).FirstOrDefault();

            return vItem;
        }

        public bool ValidateDuplicateVoucherEntry(Voucher Vdata)
        {
            var voucher = (from v in se.Vouchers
                        where
                            v.VoucherType == Vdata.VoucherType && v.FromLedger == Vdata.FromLedger && v.ToLedger == Vdata.ToLedger
                            && v.VoucherDate == Vdata.VoucherDate && v.ManualVoucherNo == Vdata.ManualVoucherNo && v.Amount==Vdata.Amount
                            && v.BusinessID==Vdata.BusinessID
                        select v).FirstOrDefault();

            if (voucher != null)
                return true;
            else
                return false;
        }


  
        
        public Voucher SaveVoucher(Voucher vData)
        {
            se.Vouchers.AddObject(vData);// .AddToVouchers(vData);
            se.SaveChanges();

            if (vData.VoucherID > 0)
            {
                LedgerJournal lj1 = GetVourcherFromLedgerJournal(vData);

                LedgerJournal lj2 = GetvoucherToLedgerJournal(vData);
                se.LedgerJournals.AddObject(lj1);// .AddToLedgerJournals(lj1);
                se.LedgerJournals.AddObject(lj2); // .AddToLedgerJournals(lj2);
                se.SaveChanges();
            }

            return vData;
        }




        private static LedgerJournal GetvoucherToLedgerJournal(Voucher vData)
        {
            LedgerJournal lj2 = new LedgerJournal();
            lj2.ReferenceID = vData.VoucherID;
            lj2.BusinessID = Convert.ToInt16(vData.BusinessID);
            lj2.TransactionDate = Convert.ToDateTime(vData.VoucherDate);
            lj2.LedgerID = Convert.ToInt16(vData.ToLedger);
            lj2.ChequeNo = vData.ChqNo;
            if (vData.VoucherType == 1)
            {
                lj2.EntryType = "PMT";
                lj2.CrAmount = Convert.ToDouble(vData.Amount);
                lj2.Narration = "By " + vData.Narration ;
            }
            else if (vData.VoucherType == 2)
            {
                lj2.EntryType = "RCP";
                lj2.DrAmount = Convert.ToDouble(vData.Amount);
                lj2.Narration = "To " + vData.Narration ;
            }

            if (vData.ManualVoucherNo.Trim() != "")
                lj2.Narration += " M.Entry No.:" + vData.ManualVoucherNo;

            return lj2;
        }

        private static LedgerJournal GetVourcherFromLedgerJournal(Voucher vData)
        {
            LedgerJournal lj1 = new LedgerJournal();
            lj1.ReferenceID = vData.VoucherID;
            lj1.BusinessID = Convert.ToInt16(vData.BusinessID);
            lj1.TransactionDate = Convert.ToDateTime(vData.VoucherDate);
            lj1.LedgerID = Convert.ToInt16(vData.FromLedger);
            if (vData.ManualVoucherNo.Trim() != "")
                lj1.Narration = vData.Narration + " " + vData.ManualVoucherNo;
            else
                lj1.Narration = vData.Narration;
            lj1.ChequeNo = vData.ChqNo;
            if (vData.VoucherType == 1)
            {
                lj1.EntryType = "PMT";
                lj1.DrAmount = Convert.ToDouble(vData.Amount);
                lj1.Narration = "To " + vData.Narration;
            }
            else if (vData.VoucherType == 2)
            {
                lj1.EntryType = "RCP";
                lj1.CrAmount = Convert.ToDouble(vData.Amount);
                lj1.Narration = "By " + vData.Narration;
            }

            if (vData.ManualVoucherNo.Trim() != "")
                lj1.Narration += " M.Entry No.:" + vData.ManualVoucherNo;

            return lj1;
        }

        public Voucher UpdateVoucher(Voucher vData)
        {
              SAMSData.SAMSEntities se1 = new   SAMSData.SAMSEntities();

            Voucher vItem = (from v in se1.Vouchers
                             where v.VoucherID == vData.VoucherID
                             select v).FirstOrDefault();

            if (vItem != null)
            {
                vItem.BusinessID = vData.BusinessID;
                vItem.VoucherNo = vData.VoucherNo;
                vItem.VoucherID = vData.VoucherID;
                vItem.VoucherType = vData.VoucherType;
                vItem.VoucherDate = vData.VoucherDate;
                vItem.Amount = vData.Amount;
                vItem.FromLedger = vData.FromLedger;
                vItem.ToLedger = vData.ToLedger;
                vItem.ChqNo = vData.ChqNo;
                vItem.Narration = vData.Narration;
                vItem.ManualVoucherNo = vData.ManualVoucherNo;
                
                se1.SaveChanges();

                if (vData.VoucherID > 0)
                {
                    string etype="";
                     if (vData.VoucherType == 1)
                    {
                        etype = "PMT";                        
                    }
                    else if (vData.VoucherType == 2)
                    {
                        etype = "RCP";                        
                    }

                    IQueryable<LedgerJournal> ljList = from l in se1.LedgerJournals 
                                         where l.ReferenceID==vData.VoucherID  && l.EntryType==etype
                                         select l;
                    foreach (LedgerJournal l in ljList)
                    {
                        se1.LedgerJournals.DeleteObject(l);//  .DeleteObject(l);                        
                    }
                    se1.SaveChanges();

                    LedgerJournal lj1 = GetVourcherFromLedgerJournal(vData);

                    LedgerJournal lj2 = GetvoucherToLedgerJournal(vData);

                    se1.LedgerJournals.AddObject(lj1);// .AddToLedgerJournals(lj1);
                    se1.LedgerJournals.AddObject(lj2); // .AddToLedgerJournals(lj2);
                    se1.SaveChanges();
                }
            }
            return vData;
        }


        public void DeleteVoucher(Voucher vData)
        {
               SAMSData.SAMSEntities se1 = new    SAMSData.SAMSEntities();

            Voucher vItem = (from v in se1.Vouchers
                             where v.VoucherID == vData.VoucherID
                             select v).FirstOrDefault();

            if (vItem != null)
            {

                if (vData.VoucherID > 0)
                {
                    string etype = "";
                    if (vData.VoucherType == 1)
                    {
                        etype = "PMT";
                    }
                    else if (vData.VoucherType == 2)
                    {
                        etype = "RCP";
                    }

                    IQueryable<LedgerJournal> ljList = from l in se1.LedgerJournals
                                                       where l.ReferenceID == vData.VoucherID && l.EntryType == etype
                                                       select l;
                    foreach (LedgerJournal l in ljList)
                    {
                        se1.LedgerJournals.AddObject(l); // .DeleteObject(l);
                    }

                }

                se1.Vouchers.DeleteObject(vItem); // .DeleteObject(vItem);

                se1.SaveChanges();
            }

        }

        

   
        public List<LedgerJournalView> SaveDayBookEntries(List<LedgerJournalView> ljList)
        {
            foreach (LedgerJournalView lj in ljList)
            {
                LedgerJournal lj1 = new LedgerJournal();
                lj1.LedgerID = lj.LedgerID;
                lj1.BusinessID = lj.BusinessID==0?1:lj.BusinessID;
                lj1.CrAmount = lj.CrAmount;
                lj1.DrAmount = lj.DrAmount;
                lj1.Narration = lj.Narration;
                lj1.DBEntryType = lj.DBEntryType;
                lj1.TransactionDate = lj.TransactionDate;
                lj1.EntryType = lj.EntryType;
                lj1.ChequeNo = lj.ChequeNo;
                //lj1.interest = lj.interest;
                //lj1.EntryGroup = lj.EntryGroup;

                se.LedgerJournals.AddObject(lj1); // .AddToLedgerJournals(lj1);
            }
            se.SaveChanges();

            return ljList;
        }

        public void UpdateDayBookEntries(LedgerJournalView lj)
        {
            LedgerJournal lj1 = (from l in se.LedgerJournals
                                 where l.LedgerJournalID == lj.LedgerJournalID
                                 select l).FirstOrDefault();

            // change modified user if any change is there
            if (lj1.CrAmount != lj.CrAmount || lj1.DrAmount != lj.DrAmount || lj1.EntryType != lj.EntryType || lj1.Narration != lj.Narration)
            {
                //Cheque no is using for entered by or modified by user
                lj1.ChequeNo = lj.ChequeNo;
            }
            
                lj1.LedgerID = lj.LedgerID;
                lj1.BusinessID = lj.BusinessID;
                lj1.CrAmount = lj.CrAmount;
                lj1.DrAmount = lj.DrAmount;
                lj1.Narration = lj.Narration;
                lj1.DBEntryType = lj.DBEntryType;
                lj1.TransactionDate = lj.TransactionDate;
                lj1.EntryType = lj.EntryType;
               // lj1.interest = lj.interest;
               //lj1.EntryGroup = lj.EntryGroup;


            //se.LedgerJournals.Add(lj1); // .AddToLedgerJournals(lj1);

            se.SaveChanges();
                        
        }

        public void DeleteLedgerJournal(int ledgerjournalId)
        {
            LedgerJournal lj = se.LedgerJournals.FirstOrDefault(x => x.LedgerJournalID == ledgerjournalId);
            if (lj != null)
            {
                se.LedgerJournals.DeleteObject(lj);// .DeleteObject(lj);
                se.SaveChanges();
            }
        }

      
        #endregion

      

    }

}
