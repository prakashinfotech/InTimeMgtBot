using System;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Web;
using System.Net.Http;
using LuisBot.Model;
using RestSharp;
using Outlook = Microsoft.Office.Interop.Outlook;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Globalization;
using System.Collections.Specialized;
using LuisBot.Utilities;
using static LuisBot.Utilities.SharepointHelpers;
using System.IO;
using System.Net.Mail;

namespace InTimeManagement
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>, IDialog<string>
    {
        private static string UserID = string.Empty;
        public static readonly string AuthTokenKey = "AuthToken";
        public static string UserEmail = string.Empty;
        public static readonly string SharepointAccountKey = "SharePointAccount";
        static HttpClient client = new HttpClient();
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Gretting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Very nice to meet you...How can I help you?");
            context.Wait(MessageReceived);
        }

        /// <summary>
        /// When late coming or similar to type this method will be called.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("LateComings")]
        public async Task LateComingsIntent(IDialogContext context, LuisResult result)
        {
            string token;
            if (context.PrivateConversationData.TryGetValue(AuthTokenKey, out token))
            {
                //string ImageUrl = "http://images.clipartpanda.com/default-clipart-acspike_male_user_icon.png";//i.ProfileURL;//;
                Activity reply = ((Activity)context.Activity).CreateReply();
                reply.AttachmentLayout = AttachmentLayoutTypes.List;
                var client = new RestClient(Constants.ApiUrl);
                var response = client.Execute<List<PayrollResponse>>(new RestRequest());
                var data = response.Content;

                var model = JsonConvert.DeserializeObject<List<PayrollResponse>>(data);
                if (model.Count > 0)
                {
                    foreach (var item in model)
                    {

                        HeroCard card = new HeroCard
                        {
                            Title = item.userName,
                            Text = $"Date: " + DateTime.Parse(item.inTime).ToString("dd/MM/yyyy HH:mm") + " (" + item.counts + ")",
                            Subtitle = $"Last timings: " + item.pastINOutTime + " (" + TimeDiffrence(item.pastINOutTime) + ")",
                        };
                        //card.Images = new List<CardImage>
                        //{
                        //    new CardImage( url = ImageUrl)
                        //};
                        UserID = item.userId;
                        //string finaldate = item.inTime.Remove(-5);
                        card.Buttons = new List<CardAction>
                        {
                            new CardAction(ActionTypes.PostBack, "Send Intimation updated & Deduct EL", value: "O1 "+item.userId+" "+item.inTime+" "+item.counts+" "+item.emailAddress+" "+2),
                            new CardAction(ActionTypes.PostBack, "Send Intimation", value: "O2 "+item.userId+" "+item.inTime+" "+item.counts+" "+item.emailAddress+" "+3),
                            new CardAction(ActionTypes.PostBack, "Send Intimation and don’t Deduct EL",value:"O3 "+item.userId+" "+item.inTime+" "+item.counts+" "+item.emailAddress+" "+4),
                            new CardAction(ActionTypes.PostBack, "Ignore", value: "O4 "+item.userId+" "+item.inTime+" "+item.counts+" "+item.emailAddress+" "+5)
                        };
                        reply.Attachments.Add(card.ToAttachment());
                    }
                    await context.PostAsync(reply);
                }
                else
                {
                    await context.PostAsync("Data not available");
                }
            }
            else
            {
                await context.PostAsync("Please Login First!!");
            }

        }

        public string TimeDiffrence(string Time)
        {
            TimeSpan t1, t2, t3 = new TimeSpan();
            if (!string.IsNullOrWhiteSpace(Time))
            {
                if (Time != "0")
                {
                    var diff = Time.Split('-');
                    t1 = TimeSpan.Parse(diff[0].Trim().ToString());
                    t2 = TimeSpan.Parse(diff[1].Trim().ToString());
                    t3 = t2 - t1;
                }
                else
                {
                    t3 = TimeSpan.Zero;
                }
            }
            else
            {
                t3 = TimeSpan.Zero;
            }

            return t3.ToString(@"hh\:mm");
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("NoActionTaken")]
        public async Task NoActionTakenIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"This will be processed soon.");
            context.Wait(MessageReceived);
        }

        /// <summary>
        /// If action button send intimation with EL is clicked, this method will be called.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("SendIntimationWithEL")]
        public async Task SendIntimationWithELIntent(IDialogContext context, LuisResult result)
        {
            string token;
            if (context.PrivateConversationData.TryGetValue(AuthTokenKey, out token))
            {
                string userid = string.Empty, attendanceDate = string.Empty, Email = string.Empty, count = string.Empty, time = string.Empty, HRStatus = string.Empty;
                ElStatusResponse Apiresponse = new ElStatusResponse();
                var option = result.Query.Substring(0, 2);
                var splitdata = result.Query.Split(' ');
                var entityCount = splitdata.Length;
                var atte = DateTime.Now;
                if (entityCount > 0)
                {
                    userid = splitdata[1];
                    Email = splitdata[5];
                    count = splitdata[4];
                    time = splitdata[3];
                    attendanceDate = splitdata[2];
                    HRStatus = splitdata[6];

                }
                if (option == "O1")
                {
                    decimal el = 0.5m;
                    var user = Guid.Parse(userid);
                    atte = DateTime.Parse(attendanceDate);
                    var status = Int32.Parse(HRStatus);
                    Apiresponse = await UpdateStatus(user, atte, el, status);
                }
                else
                {
                    var user = Guid.Parse(userid);
                    atte = DateTime.Parse(attendanceDate);
                    var status = Int32.Parse(HRStatus);
                    Apiresponse = await UpdateStatus(user, atte, 0, status);
                }
                if (HRStatus != "5")
                {
                    if (Apiresponse.status.Contains("already"))
                    {
                        await context.PostAsync($"{Apiresponse.status}");
                    }
                    else
                    {
                        string member = "Member";
                        if (Apiresponse != null)
                        {
                            string[] data = Apiresponse.status.Split(':');
                            member = data[1].Split(' ')[1];
                            var status = await sendMail(Email, atte.ToString("dd/MM/yyyy"), time, option, member, Apiresponse.numofel, Apiresponse.totalel);
                            if (status == true)
                            {
                                await context.PostAsync(Apiresponse.status);
                            }
                            //else
                            //{
                            //    await context.PostAsync($"Mail sending is failed");
                            //}
                        }
                    }
                }
                else
                {
                    await context.PostAsync($"{Apiresponse.status}");
                }
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync($"Please Login First!!");
            }

        }

        /// <summary>
        /// Help
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"updated : You can search for late incomings by \n\n\n" +
                $"\n\n" +
                $"1. Process late incoming members");
            context.Wait(MessageReceived);
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("LogIn")]
        public async Task LoginIntent(IDialogContext context, LuisResult result)
        {
            await LogIn(context);
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("LogOut")]
        public async Task LogOutIntent(IDialogContext context, LuisResult result)
        {
            context.PrivateConversationData.RemoveValue(AuthTokenKey);
            await context.PostAsync("Your are logged out!");
            context.Wait(MessageReceived);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Didn't get what you are saying.");
            context.Wait(MessageReceived);
        }
        /// <summary>
        /// When Send mail button is clicked, this action will be called.
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="AttendanceDate"></param>
        /// <param name="time"></param>
        /// <param name="option"></param>
        /// <param name="member"></param>
        /// <param name="numofel"></param>
        /// <param name="totalel"></param>
        /// <returns></returns>
        //method to send email using smtp
        public async Task<bool> sendMail(string Email, string AttendanceDate, string time, string option, string member, string numofel, string totalel)
        {
            try
            {
                SmtpClient client = new SmtpClient("smtp-mail.outlook.com");

                client.Port = 587;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                System.Net.NetworkCredential credentials =
                    new System.Net.NetworkCredential(Constants.notificationEmail, Constants.notificationPassword);
                client.EnableSsl = true;
                client.Credentials = credentials;

                var mail = new MailMessage(Constants.notificationEmail.Trim(), Email.Trim());
                MailAddress copy = new MailAddress(Constants.CCsenderEmail.Trim());
                mail.CC.Add(copy);

                mail.Subject = "Late coming notification - " + member + " " + AttendanceDate + " " + time;
                mail.IsBodyHtml = true;
                mail.Body = PopulateBody(Email, AttendanceDate, time, option, member, numofel, totalel);
                client.Send(mail);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string PopulateBody(string Email, string date, string time, string option, string member, string numofel, string totalel)
        {
            string body = string.Empty;
            if (option == "o1" || option == "O1")
            {
                using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Templates/PrakashUninformedLateComing.html")))
                {
                    body = reader.ReadToEnd();
                }
            }
            else if (option == "o2" || option == "O2")
            {
                using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Templates/PrakashLatecomings.html")))
                {
                    body = reader.ReadToEnd();
                }
            }
            else if (option == "o3" || option == "O3")
            {
                using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Templates/PrakashInformedLateComings.html")))
                {
                    body = reader.ReadToEnd();
                }
            }
            if (totalel == "0.00")
            {
                totalel = "0";
            }

            body = body.Replace("{Email}", Email);
            body = body.Replace("{attendancedate}", date);
            body = body.Replace("{time}", time);
            body = body.Replace("{membername}", member);
            body = body.Replace("{numofel}", numofel);
            body = body.Replace("{totalel}", totalel);
            return body;
        }

        public async Task<ElStatusResponse> UpdateStatus(Guid userid, DateTime attendanceDate, decimal EL, int Hrstatus)
        {
            PayrollRequest reqModel = new PayrollRequest();
            ElStatusResponse res = new ElStatusResponse();
            if (userid != null && attendanceDate != null)
            {
                reqModel.Leave = EL.ToString();
                reqModel.Type = Convert.ToString(Hrstatus);
                reqModel.UserId = userid;
                var date = attendanceDate;
                reqModel.AttendanceDate = date.ToString("yyyy-MM-dd");
                //decimal a2 = TryParsedecimal("234.53453424233432423432", decimal.MinValue);

            }

            var client = new RestClient(Constants.ApiUrl);
            var request = new RestRequest(Constants.ApiUrl, Method.POST);

            request.RequestFormat = DataFormat.Json;
            request.AddBody(reqModel);
            request.AddHeader("email", UserEmail); 
            try
            {
                var response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    res = JsonConvert.DeserializeObject<ElStatusResponse>(response.Content);
                }
                return res;
            }
            catch (Exception error)
            {
                res.status = "Not updated successfully";
                res.totalel = "0";
                return res;
            }
        }

        public decimal TryParsedecimal(string input, decimal ifFail)
        {

            decimal output;
            if (decimal.TryParse(input, out output))
            {
                output = output;
            }
            else
            {
                output = ifFail;
            }
            return output;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await (argument);
            if (msg.Value != null)
            {
                dynamic value = msg.Value;
                string submittype = value.Type.ToString();
                switch (submittype)
                {
                    case "submitchoice":
                        var reply = context.MakeMessage();
                        reply.Text = "You have selected " + value.undefined.ToString();
                        await context.PostAsync(reply);
                        context.Done("Selected");
                        return;
                }
            }
            if (msg.Text.StartsWith("token:"))
            {
                RestClient client = new RestClient(Constants.ApiUrl);

                string status = string.Empty;
                var token = msg.Text.Remove(0, "token:".Length);
                context.PrivateConversationData.SetValue(AuthTokenKey, token);

                var Usermail = await UserInfo(token);

                if (!string.IsNullOrWhiteSpace(Usermail))
                {
                    UserEmail = Usermail;
                    var request = new RestRequest("authorization", Method.GET);
                    request.AddHeader("email", Usermail);

                    try
                    {
                        var response = client.Execute(request);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            status = response.Content;
                            if (status == "true")
                            {
                                await context.PostAsync("Your are logged in.");
                            }
                            else
                            {
                                await context.PostAsync("You are not authorized user. Logged out! Try with valid credentials");
                                context.PrivateConversationData.RemoveValue(AuthTokenKey);
                                //await context.PostAsync("Your are logged out! Try with valid credentials");
                                context.Wait(MessageReceived);
                            }
                        }
                    }
                    catch (Exception error)
                    {
                    }
                }
                context.Done(token);
            }
            else
            {
                await context.PostAsync("Please do login before move forward!");
                //await LogIn(context);
            }

        }

        #region Login

        public async Task<string> UserInfo(string Token)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Token);

            HttpResponseMessage response = await client.GetAsync(Constants.MicrosoftProfileUrl);

            string retResp = await response.Content.ReadAsStringAsync();
            AzureRespose data = JsonConvert.DeserializeObject<AzureRespose>(retResp);

            return data.EmailAddress;
        }

        private async Task LogIn(IDialogContext context)
        {
            string token;
            if (!context.PrivateConversationData.TryGetValue(AuthTokenKey, out token))
            {
                var conversationReference = context.Activity.ToConversationReference();

                context.PrivateConversationData.SetValue("persistedCookie", conversationReference);

                var reply = context.MakeMessage();
                reply.Type = "message";

                if (context.Activity.ChannelId == ChannelIds.Skype.ToString())
                {
                    Microsoft.Bot.Connector.Attachment plAttachment = GetSkypeSigninCard(conversationReference);
                    reply.Attachments.Add(plAttachment);
                }
                else
                {
                    Microsoft.Bot.Connector.Attachment plAttachment = GetSigninCard(conversationReference);
                    reply.Attachments.Add(plAttachment);
                }

                await context.PostAsync(reply);

                context.Wait(MessageReceivedAsync);
            }
            else
            {
                await context.PostAsync($"Your are already logged in.");
                context.Done(token);
            }
        }

        private static Microsoft.Bot.Connector.Attachment GetSkypeSigninCard(ConversationReference conversationReference)
        {
            var signinCard = new SigninCard
            {
                Text = "Please login to microsoft account",
                Buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Authentication Required", value: SharepointHelpers.GetSharepointLoginURL(conversationReference, Constants.SharepointOauthCallback.ToString())) }
            };

            return signinCard.ToAttachment();
        }

        private static Microsoft.Bot.Connector.Attachment GetSigninCard(ConversationReference conversationReference)
        {
            List<CardAction> cardButtons = new List<CardAction>();
            CardAction plButton = new CardAction()
            {
                Value = SharepointHelpers.GetSharepointLoginURL(conversationReference, Constants.SharepointOauthCallback.ToString()),
                Type = "openUrl",
                Title = "Authentication Required"
            };
            cardButtons.Add(plButton);

            SigninCard plCard = new SigninCard("Please login to microsoft account", new List<CardAction>() { plButton });
            return plCard.ToAttachment();
        }
        #endregion
    }
}