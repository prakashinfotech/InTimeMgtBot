# In Time Management BOT

Azure BOT with Azure AD Sign in and LUIS capabilities that provides late coming member list and allow admin to auto deduct leave or send intimation via email as per action taken.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

At first step, when you connect to Time officer Bot in skype/team channel, BOT would represent SignIn Card. On clicking Sign in, BOT would redirect user to sign in in azure, once authenticated user can use Time officer BOT. It greets user and ask process late coming users in organization.

on typing "late coming"... or related to late coming users, BOT provides list of pending user list who are coming late than expected organization set arrival time. For we have taken payroll API (existing applciation)

Select user, and choose any action. BOT will do action as per chosen option.

### References to create Azure BOT Service, Developing, Testing, and publish on Azure.

1. Create a bot with Bot Service - https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart?view=azure-bot-service-3.0
2. Create a bot with the Bot Builder SDK for .NET - https://docs.microsoft.com/en-us/azure/bot-service/dotnet/bot-builder-dotnet-quickstart?view=azure-bot-service-3.0
3. Call a LUIS endpoint using C# - https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-get-started-cs-get-intent


### Installing

A step by step series of examples that tell you how to get a development env running

1. Download / Clone code. 
2. Repleace web.config values as below. 

You will get MicrosoftAppId and MicrosoftAppPassword on creating azure bot app. You will get LUIS api key and App id on creating LUIS app. Copy those values and replace to below code.

```
    <add key="MicrosoftAppId" value="" />
    <add key="MicrosoftAppPassword" value="" />
    <add key="LuisAPIKey" value=""/>
    <add key="LuisAppId" value="" />
```

3. Create Azure Active Directory application https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-integrating-applications. Copy value of application id and application secret and replace values of SharepointAppId, and SharepointAppSecret respectively [ Constants.Cs class ]



## Debug Code

[Debug bots with the Bot Framework Emulator](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-debug-emulator?view=azure-bot-service-3.0)

## Deployment

1. [Deploy your bot to Azure](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-3.0)
2. [Publish a bot to Bot Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-continuous-deployment?view=azure-bot-service-3.0)


## Authors

* **Hiral Patel** (https://github.com/mehiralpatel)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

[Use Case](http://www.mehiralpatel.com/index.php/2018/02/time-officer-bot-virtual-assistant-of-hr/)
  
![Architecture Diagram](https://github.com/prakashinfotech/InTimeMgtBot/blob/master/Architecture%20Diagram%20-%201.jpg)
  
  
  
  
  
  
  
  
 
 
 

 
