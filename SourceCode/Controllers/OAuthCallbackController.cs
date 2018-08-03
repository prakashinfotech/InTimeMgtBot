using Autofac;
using LuisBot.Model;
using LuisBot.Utilities;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static LuisBot.Utilities.SharepointHelpers;

namespace LuisBot.Controllers
{
    public class OAuthCallbackController : ApiController
    {
        [HttpGet]
        [Route("api/OAuthCallback")]
        public async Task<HttpResponseMessage> OAuthCallback([FromUri] string code, [FromUri] string session_state, string state, CancellationToken token)
        {

            var dict = HttpUtility.ParseQueryString(state);
            string json = JsonConvert.SerializeObject(dict.Cast<string>().ToDictionary(k => k, v => dict[v]));
            Address encodedAddress = JsonConvert.DeserializeObject<Address>(json);
            Address address = new Address(
                botId: SharepointHelpers.TokenDecoder(encodedAddress.BotId),
                channelId: SharepointHelpers.TokenDecoder(encodedAddress.ChannelId),
                conversationId: SharepointHelpers.TokenDecoder(encodedAddress.ConversationId),
                serviceUrl: SharepointHelpers.TokenDecoder(encodedAddress.ServiceUrl),
                userId: SharepointHelpers.TokenDecoder(encodedAddress.UserId)
                );

            var conversationReference = address.ToConversationReference();

            // Exchange the Sharepoint Auth code with Access token
            var accessToken = await SharepointHelpers.ExchangeCodeForAccessToken(conversationReference, code, Constants.SharepointOauthCallback.ToString());

            // Create the message that is send to conversation to resume the login flow
            var msg = conversationReference.GetPostToBotMessage();
            msg.Text = $"token:{accessToken}";

            // Resume the conversation to AuthDialog

            await Conversation.ResumeAsync(conversationReference, msg);

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, msg))
            {
                var dataBag = scope.Resolve<IBotData>();
                await dataBag.LoadAsync(token);
                ConversationReference pending;
                if (dataBag.PrivateConversationData.TryGetValue("persistedCookie", out pending))
                {
                    // remove persisted cookie
                    dataBag.PrivateConversationData.RemoveValue("persistedCookie");
                    await dataBag.FlushAsync(token);
                    return Request.CreateResponse("You are now logged in! Continue talking to the bot.");
                }
                else
                {
                    // Callback is called with no pending message as a result the login flow cannot be resumed.
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new InvalidOperationException("Cannot resume!"));
                }
            }
        }

        

    }
}
