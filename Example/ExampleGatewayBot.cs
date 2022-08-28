using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sharpcord_bot_library;

namespace Example
{
    //you must specify this for the bot to be used
    [DiscordBot("bot name", DiscordBotAttribute.BotType.gateway)]
    public class ExampleGatewayBot : GatewayBot
    {
        //WARNING: the gateway connection is somehow janky, it dies after a while and can't reconnect, causing your bot to be offline. Use and external task scheduler to restart sharpcord every few hours to eliminate this issue, or fix the issue and create a pull request.

        public ExampleGatewayBot()
        {
            //use AuthType.Bot here.
            base.InitBot(new Authorization(Authorization.AuthType.Bot, "your bot token"));
        }
        public override void Interaction(JObject o)
        {
            //here you can handle interactions, and respond to them with base.http
            //refer to the Discord manual for the ways to respond and what is in the JObject.
            //make sure to repond in a few seconds so the user sees the reply.
            //alternatively, you can use a deferred response message, so the user sees a loading status and send a followup message.
        }
    }
}
