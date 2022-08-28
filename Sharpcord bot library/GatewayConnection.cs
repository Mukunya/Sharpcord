using System;
using System.Collections.Generic;
using System.Linq;
using WebSocket4Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

//Nullreference exceptions shouldn't occurr. C# warnings ignorable.

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Sharpcord_bot_library
{
    public class GatewayConnection
    {
        private int heartbeat_interval;
        private int? heartbeat_seqN = null;
        private bool heartbeat_ack = true;
        private string token = "";
        private string session_id = "";
        private int reconnectcount = 0;
        private WebSocket Socket;
        public event EventHandler<string> LogInfo;
        public event EventHandler<string> LogWarn;
        public event EventHandler<Exception> LogError;

        public event EventHandler<JObject> INTERACTION_CREATE;
        private HTTP http;

        public GatewayConnection(Authorization auth)
        {
            this.token = auth.Token;
            http = new HTTP(auth);
        }
        ManualResetEvent connected = new ManualResetEvent(false);
        public void Connect()
        {
            while (true)
            {
                LogInfo?.Invoke(this, "Connecting.");
                string gatewayurl = http.Get(Discord.DC_API + "/gateway", new JObject(), false)["url"].ToString() + "/?v=9&encoding=json";
                LogInfo?.Invoke(this, $"Obtained gateway, address is {gatewayurl}");
                Socket = new WebSocket(gatewayurl);

                Socket.MessageReceived += Socket_MessageReceived;
                Socket.Closed += Socket_Closed;
                Socket.Open();
                Socket.Opened += (s, e) => connected.Set();
                connected.WaitOne();
                connected.Reset();
                if (Socket.State == WebSocketState.Open)
                {
                    LogInfo?.Invoke(this, "Connected to Discord gateway");
                    break;
                }
                
                
            }
            
        }
        public void Disconnect()
        {
            Socket.Close();
        }
        private void Socket_Closed(object sender, EventArgs e)
        {
            LogWarn?.Invoke(this, "Socket closed, attemting to reconnect.");
            Connect();
        }
    
        private void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                //Console.WriteLine(e.Message);
                JObject msg = JObject.Parse(e.Message);
                if (msg.ContainsKey("s"))
                {

                    int? i = msg["s"].ToObject<int?>();
                    if (i.HasValue)
                    {
                        heartbeat_seqN = i;
                    }
                }
                switch (msg["op"].ToObject<int>())
                {
                    case 10: //Gateway hello
                        heartbeat_interval = msg["d"]["heartbeat_interval"].ToObject<int>();
                        Gateway_Hearthbeat();
                        Socket.Send(JObject.FromObject(new
                        {
                            op = 2,
                            d = new
                            {
                                token = token,
                                intents = 1 << 4,
                                properties = JObject.Parse("{\"$os\": \""+Environment.OSVersion.VersionString+"\",\"$browser\": \".net 6.0\",\"$device\": \"Chinese server\"}")
                            }
                        }
                        ).ToString());

                        break;
                    case 11: //Heartbeat ACK
                        heartbeat_ack = true;
                        break;
                    case 1: //Heartbeat
                        heartbeat_ack = false;
                        Socket.Send(JObject.FromObject(new
                        {
                            op = 1,
                            d = heartbeat_seqN
                        }
                        ).ToString());
                        break;
                    case 0: //Gateway event
                        switch (msg["t"].ToString())
                        {
                            case "READY":
                                session_id = msg["d"]["session_id"].ToString();
                                reconnectcount = 0;
                                LogInfo?.Invoke(this, "Socket connection estabilished");
                                break;
                            case "INTERACTION_CREATE":
                                INTERACTION_CREATE?.Invoke(this, msg);
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError?.Invoke(this, ex);
            }
        }

        private void Gateway_Hearthbeat()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(heartbeat_interval);
                    if (!heartbeat_ack)
                    {
                        Socket.Close(1008, "Dead connection");
                        LogWarn?.Invoke(this, "Soket connection died, attempting to resume");
                        Resume();
                        return;
                    }
                    heartbeat_ack = false;
                    Socket.Send(JObject.FromObject(new
                    {
                        op = 1,
                        d = heartbeat_seqN
                    }
                    ).ToString());
                }
            });
        }

        private void Resume()
        {
            reconnectcount++;
            if (reconnectcount > 4)
            {
                LogWarn?.Invoke(this, "Reconnecting max attempts reached, connecting reguralrly.");
                heartbeat_seqN = null;
                Connect();
            }
            LogInfo?.Invoke(this, "Attempting to reconnect");
            Socket.Open();
            Socket.Send(JObject.FromObject(new
            {
                op = 6,
                d = new
                {
                    token = token,
                    session_id = session_id,
                    seq = heartbeat_seqN
                }
            }
            ).ToString());
        }
    }
}

