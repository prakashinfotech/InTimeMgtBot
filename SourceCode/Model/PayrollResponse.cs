using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Model
{
    public class PayrollResponse
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string emailAddress { get; set; }
        public string inTime { get; set; }
        public string pastINOutTime { get; set; }
        public int hrStatus { get; set; }
        public int counts { get; set; }
    }
    public class PayrollRequest {
        public Guid UserId { get; set; }
        public string Leave { get; set; }
        public String AttendanceDate { get; set; }
        public string Type { get; set; }
    }
    public static class Constants
    {
        public static readonly Uri SharepointOauthCallback = new Uri("https://<<bot app serice>>.azurewebsites.net/api/OAuthCallback");
        
        public static readonly Uri ApiUrl = new Uri("<<api url>>");
        
        public static readonly Uri MicrosoftProfileUrl = new Uri("https://outlook.office.com/api/v2.0/me");

        public static readonly string SharepointAppId = "<<azure AD app-id>>";
        public static readonly string SharepointAppSecret = "<<Azure AD app-secret>>";

        
        public static readonly string senderEmail = "<<sender-email>>";
        public static readonly string notificationEmail = "<<notification-email>>";
        public static readonly string CCsenderEmail = "<<cc-email>>";
        public static readonly string notificationPassword = "<<password>>";
    }
    public class ElStatusResponse
    {
        public string status { get; set; }
        public string totalel { get; set; }
        public string numofel { get; set; }
    }

    public enum HRStatus {
        Initial = 1,
        SendIntimationWithEL = 2,
        SendIntimationOnly = 3,
        SendIntimationWithoutEL = 4,
        Ignore = 5
    }
    public class UpdateDetails
    {
        public Guid UserId { get; set; }
        public decimal Leave { get; set; }
        public DateTime AttendanceDate { get; set; }
    }
    public class AzureRespose
    {
      
        public string Id { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public string Alias { get; set; }
        public string MailboxGuid { get; set; }
    }
}