
using LuisBot.Model;
using Microsoft.Bot.Connector;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace LuisBot.Utilities
{

    public class SharepointAcessToken
    {
        public SharepointAcessToken()
        {
        }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public long ExpiresIn { get; set; }
    }

    class SharepointProfile
    {
        public SharepointProfile()
        {
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Helpers implementing Sharepoint API calls.
    /// </summary>
    public static class SharepointHelpers
    {

        public static readonly string siteURL = "https://outlook.office365.com/";

        
        public enum Tagged { All = 1, UnTagged = 2, AddTag = 3, SearchTag = 4, AskSharepointAccount = 5 };
        //TODO - code refactor
        public static string paccesstoken = string.Empty;
        public async static Task<string> ExchangeCodeForAccessToken(ConversationReference conversationReference, string code, string SharepointOauthCallback)
        {
            var redirectUri = SharepointOauthCallback;
            
            var authContext = new AuthenticationContext("https://login.microsoftonline.com/common");
            var authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(
                code,
                new Uri(redirectUri),
                new ClientCredential(
                   Constants.SharepointAppId,
                    Constants.SharepointAppSecret));

           
            return authResult.AccessToken;
            
        }
            

  

        public static string GetOAuthCallBack(ConversationReference conversationReference, string SharepointOauthCallback)
        {

            var uri = GetUri(SharepointOauthCallback,
                Tuple.Create("userId", TokenEncoder(conversationReference.User.Id)),
                Tuple.Create("botId", TokenEncoder(conversationReference.Bot.Id)),
                Tuple.Create("conversationId", TokenEncoder(conversationReference.Conversation.Id)),
                Tuple.Create("serviceUrl", TokenEncoder(conversationReference.ServiceUrl)),
                Tuple.Create("channelId", conversationReference.ChannelId)
                );
            return uri.ToString();
        }

        // because of a limitation on the characters in Sharepoint redirect_uri, we don't use the serialization of the cookie.
        // http://stackoverflow.com/questions/4386691/Sharepoint-error-error-validating-verification-code
        public static string TokenEncoder(string token)
        {
            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(token));
        }

        public static string TokenDecoder(string token)
        {
            return Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(token));
        }

        public static string GetSharepointLoginURL(ConversationReference conversationReference, string SharepointOauthCallback)
        {
            var redirectUri = GetOAuthCallBack(conversationReference, SharepointOauthCallback);

            Uri resourceUri = new Uri(siteURL);

            //var uri = GetUri("https://login.microsoftonline.com/common",
            var uri = GetUri("https://login.microsoftonline.com/common/oauth2/authorize",
                 Tuple.Create("resource", resourceUri.Scheme + Uri.SchemeDelimiter + resourceUri.Host),
                Tuple.Create("client_id", Constants.SharepointAppId),
                Tuple.Create("redirect_uri", SharepointOauthCallback),
                Tuple.Create("response_type", "code"),
                Tuple.Create("state", "lentest=abc&userId=" + TokenEncoder(conversationReference.User.Id)
                + "&botId=" + TokenEncoder(conversationReference.Bot.Id)
                  + "&conversationId=" + TokenEncoder(conversationReference.Conversation.Id) + "&serviceUrl=" + TokenEncoder(conversationReference.ServiceUrl)
                + "&channelId=" + TokenEncoder(conversationReference.ChannelId)
                ));

            return uri.ToString();

        }

        private static async Task<T> SharepointRequest<T>(Uri uri)
        {
            string json;
            using (HttpClient client = new HttpClient())
            {
                json = await client.GetStringAsync(uri).ConfigureAwait(false);
            }

            try
            {
                var result = JsonConvert.DeserializeObject<T>(json);
                return result;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Unable to deserialize the Sharepoint response.", ex);
            }
        }

        private static Uri GetUri(string endPoint, params Tuple<string, string>[] queryParams)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (var queryparam in queryParams)
            {
                queryString[queryparam.Item1] = queryparam.Item2;
            }

            var builder = new UriBuilder(endPoint);
            builder.Query = queryString.ToString();
            return builder.Uri;
        }       
    }
}