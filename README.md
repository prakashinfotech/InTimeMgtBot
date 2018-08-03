# InTimeMgtBot
Azure BOT with Azure AD Sign in and LUIS capabilities that provides late coming member list and allow admin to auto deduct leave or send intimation via email as per action taken.


1.	At first step, when you connect to Time officer Bot in skype/team channel, 
BOT would represent SignIn Card. On clicking Sign in, BOT would redirect user to sign in in azure, 
once authenticated user can use Time officer BOT. It greets user and ask process late coming users in organization.

2. on typing "late coming"... or related to late coming users, BOT provides list of pending user list who are coming late than expected organization set arrival time. For we have taken payroll API (existing applciation)

3. Select user, and choose any action. BOT will do action as per chosen option.


# References to create Azure BOT Service, Developing, Testing, and publish on Azure.

1. Create a bot with Bot Service - https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart?view=azure-bot-service-3.0
2. Create a bot with the Bot Builder SDK for .NET - https://docs.microsoft.com/en-us/azure/bot-service/dotnet/bot-builder-dotnet-quickstart?view=azure-bot-service-3.0
3. Call a LUIS endpoint using C# - https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-get-started-cs-get-intent
4. Debug bots with the Bot Framework Emulator -  https://docs.microsoft.com/en-us/azure/bot-service/bot-service-debug-emulator?view=azure-bot-service-3.0
5. Deploy your bot to Azure - https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-3.0
6. Publish a bot to Bot Service - https://docs.microsoft.com/en-us/azure/bot-service/bot-service-continuous-deployment?view=azure-bot-service-3.0
 

  
# Note : After Creating Azure Bot Service, LUIS account,  update reference keys in web.config file.
  
  
  
  
  
  
  
  
  
  
 
 
 

 
