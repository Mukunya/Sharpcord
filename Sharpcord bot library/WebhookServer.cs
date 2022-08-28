using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using NSec.Cryptography;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Sharpcord_bot_library
{
    public class WebhookServer
    {
        static HttpListener Listener;
        static IAsyncResult listenerAsync;
        public static bool enabled = false;
        public static Dictionary<string,WebhookServer> Webhooks = new Dictionary<string, WebhookServer>();

        public static event EventHandler<string> LogDebug;
        public static event EventHandler<string> LogInfo;
        public static event EventHandler<string> LogWarn;
        public static event EventHandler<Exception> LogError;
        public string endpoint = "/default";

        PublicKey BotKey;
        public Action<InteractionObject> Interaction_create;
        public Action<string> Interaction_followup;

        public static void Setup()
        {
            string port = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/config.txt");
            LogInfo?.Invoke(null, "Webhook initializing. Port is " + port);
            Listener = new HttpListener();
            Listener.Prefixes.Add("http://*:"+port + "/");
            Listener.Start();
            LogInfo?.Invoke(null, "Webhook open and listening.");
            listenerAsync = Listener.BeginGetContext((_) => { Request(); }, null);
            //Request();
        }
        private static void Request()
        {
            LogDebug?.Invoke(null, "================================================================");
            LogDebug?.Invoke(null, "");
            LogDebug?.Invoke(null, "================================================================");
            HttpListenerContext context = Listener.EndGetContext(listenerAsync);
            listenerAsync = Listener.BeginGetContext((_) => { Request(); }, null);
            LogInfo?.Invoke(null, "Request received. RawUrl: " + context.Request.RawUrl);
            if (context.Request.Headers["CF-RAY"] == null)
            {
                context.Response.StatusCode = 403;
                context.Response.OutputStream.Write(Encoding.UTF8.GetBytes("Forbidden. Don't bypass cloudflare."));
                context.Response.Close();
                return;
            }
            if (Webhooks.ContainsKey(context.Request.RawUrl) && context.Request.Headers["User-Agent"] == "Discord-Interactions/1.0 (+https://discord.com)")
            {
                Webhooks[context.Request.RawUrl].Interaction_request(context);
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.OutputStream.Write(Encoding.UTF8.GetBytes("Not found."));
                context.Response.Close();
            }
        }
        private void Interaction_request(HttpListenerContext c)
        {
            try
            {
                LogDebug?.Invoke(null, "Request received.");
                string data = new StreamReader(c.Request.InputStream, Encoding.UTF8).ReadToEnd();
                LogDebug?.Invoke(null, data);
                LogDebug?.Invoke(null, c.Request.Headers.ToString());
                bool b = SignatureAlgorithm.Ed25519.Verify(BotKey, Encoding.UTF8.GetBytes(c.Request.Headers.Get("X-Signature-Timestamp") + data), StringToByteArrayFastest(c.Request.Headers.Get("X-Signature-Ed25519")?? "00"));
                LogDebug?.Invoke(null, "Signature verification: "+ b.ToString());
                if (!b)
                {
                    LogDebug?.Invoke(null, "Response:  401; Wrong signature");
                    c.Response.StatusCode = 401;
                    c.Response.ContentType = "application/json";
                    c.Response.OutputStream.Write(Encoding.UTF8.GetBytes("{\"error\":\"Bad network signature\"}"));
                    c.Response.Close();
                }
                else
                {
                    LogDebug?.Invoke(null, "Response:  200");
                    c.Response.StatusCode = 200;

                    if (JObject.Parse(data)["type"].ToString() == "1")
                    {
                        LogDebug?.Invoke(null, "Returned: {\"type\":1}");
                        c.Response.ContentType = "application/json";
                        c.Response.OutputStream.Write(Encoding.UTF8.GetBytes("{\"type\":1}"));
                        c.Response.Close();
                    }
                    else
                    {
                        try
                        {
                            LogDebug?.Invoke(null, "Waiting for app to return reply");
                            InteractionObject interaction = new InteractionObject() { Message = data, Callback = new TimeoutCallbackEvent() };
                            Interaction_create?.Invoke(interaction);
                            if (interaction.Callback.Wait(1000))
                            {
                                LogDebug?.Invoke(null, "Response got, " + interaction.Response);
                                c.Response.ContentType = "application/json";
                                c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(interaction.Response));
                                c.Response.Close();
                                Interaction_followup?.Invoke(data);
                            }
                            else
                            {
                                LogWarn?.Invoke(null, "Response wait timed out, droppping interaction");
                                throw new TimeoutException("Command timed out");
                            }
                        }
                        catch (Exception e)
                        {
                            LogError?.Invoke(null, e);
                            try
                            {
                                c.Response.ContentType = "application/json";
                                c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(JObject.FromObject(new
                                {
                                    type = 4,
                                    embeds = new object[]
                                    {
                                       new
                                       {
                                          title= "An error occured",
                                          description= "Executing this command resulted in an exception.",
                                          color= 0xff0000,
                                          fields=new object[]
                                          {

                                            new {
                                              name= "Details for developers",
                                              value= "`" + e.ToString() + "`"
                                            }
                                          },
                                          footer= new
                                          {
                                              text= "This is an automatic message generated by Sharpcord"
                                          }
                                       }
                                    },
                                    flags = 1<<6
                                }).ToString()));
                                c.Response.Close();
                            }
                            catch
                            {

                            }
                            
                        }


                    }
                }
            }
            catch (Exception e)
            {
                LogError?.Invoke(null, e);
            }
            
        }
        //stolen code from stackoverflow, no idea how it works
        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
        public WebhookServer(string endpoint, PublicKey key)
        {
            if (!enabled)
            {
                enabled = true;
                Setup();
            }
            this.endpoint = endpoint;
            BotKey = key;
            Webhooks.Add(endpoint, this);
        }

    }
    public class InteractionObject
    {
        public string Message { get; set; }
        public string Response { get; set; }
        public TimeoutCallbackEvent Callback;
    }
}
