using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sharpcord_bot_library;

namespace Example
{
    //You must add this attribute
    [DiscordBot("Name of your bot", DiscordBotAttribute.BotType.webhook)]
    public class ExampleWebhookbot : WebhookBot
    {
        public ExampleWebhookbot()
        {
            //endpoint: 
            // the full url you have to add to interaction webbhook url will be your hostname + endpoint.
            // Note: sharpcord hosts endpoints on a http server on the port specified in config.txt file of the host. You must use a reverse https proxy with a trusted ssl certificate, or use cloudflare.
            base.InitBot("/example", "public key found on discord.com/developers/applications", new Authorization(Authorization.AuthType.Bot, "your bot token"));
        }
        public override void Interaction(JObject o, Action<JObject> Reply)
        {
            //handle the interction you receive as JObject o. Refer to the Discord docs for the contents of that JObject.
            //Usually, you send back a deferred response message, this will be the direct response to the http request to the webhook.
            //Invoke Reply with the response json to respond to the interaction, there is a 1 second timeout.
        }

        public override void Interaction_followup(JObject o)
        {
            //this calls once the http response has been sent, use this for commands that take a bit of time.
            //you can send followup messages or update the original message using base.http
        }
    }
}
