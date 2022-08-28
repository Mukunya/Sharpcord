using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpcord_bot_library
{
    public class Discord
    {
        public const string DC_API = "https://discord.com/api";
    }
    public abstract class Bot
    {
        public event EventHandler<string> LogInfo;
        public event EventHandler<string> LogWarn;
        public event EventHandler<Exception> LogError;

        internal void logInfo(object? sender, string e)
        {
            LogInfo?.Invoke(sender, e);
        }
        internal void logWarn(object? sender, string e)
        {
            LogWarn?.Invoke(sender, e);
        }
        internal void logError(object? sender, Exception e)
        {
            LogError?.Invoke(sender, e);
        }
        
    }
    public abstract class GatewayBot : Bot
    {
        GatewayConnection gateway;
        
        protected void InitBot(string token)
        {
            gateway = new GatewayConnection(token);
            gateway.LogWarn += (object? sender, string e) => { base.logWarn(sender, e); };
            gateway.LogError  += (object? sender, Exception e) => { base.logError(sender, e); };
            gateway.LogInfo  += (object? sender, string e) => { base.logInfo(sender, e); };
            gateway.INTERACTION_CREATE += INTERACTION_CREATE;
            base.logInfo(this, "Gateway bot initialization complete.");
            gateway.Connect();
        }
        internal void INTERACTION_CREATE(object? sender, JObject e)
        {
            Interaction(e);
        }
        abstract public void Interaction(JObject o);
    }

    public abstract class WebhookBot : Bot
    {
        internal static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        public WebhookServer server;
        protected void InitBot(string endpoint,string publickey)
        {
            server = new WebhookServer(
                endpoint,
                NSec.Cryptography.PublicKey.Import(
                    NSec.Cryptography.SignatureAlgorithm.Ed25519,
                    StringToByteArray(publickey),
                    NSec.Cryptography.KeyBlobFormat.RawPublicKey));

            server.Interaction_create = c =>
            {
                Interaction(JObject.Parse(c.Message), o =>
                {
                    c.Response = o.ToString();
                    c.Callback.Set();
                });
            };
            server.Interaction_followup = c =>
            {
                Interaction_followup(JObject.Parse(c));
            };
        }
        abstract public void Interaction(JObject o, Action<JObject> Reply);
        abstract public void Interaction_followup(JObject o);
        
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DiscordBotAttribute : Attribute
    {
        public enum BotType { gateway = 0, webhook = 1};
        public string DisplayName { get; set; }
        public BotType Type { get; set; }
        public DiscordBotAttribute(string displayName, BotType type)
        {
            DisplayName = displayName;
            Type=type;
        }

    }
}
