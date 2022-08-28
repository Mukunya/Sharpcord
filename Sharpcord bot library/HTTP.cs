#pragma warning disable SYSLIB0014 // Type or member is obsolete, WebRequest

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sharpcord_bot_library
{
    static public class HTTP
    {
        
        public static JObject Get(string url, JObject body, string BotToken, bool sendbody = true)
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            req.Method = "GET";
            req.Headers.Add(HttpRequestHeader.Authorization, "Bot " + BotToken);
            if (sendbody)
            {
                req.ContentType = "application/json";
                req.GetRequestStream().Write(Encoding.Default.GetBytes(body.ToString()));
            }
            WebResponse res = req.GetResponse();
            JObject o = JObject.Parse(new StreamReader(res.GetResponseStream()).ReadToEnd());
            res.Close();
            return o;
        }
        public static JObject Post(string url, JObject body, string BotToken)
        {
            try
            {
                HttpWebRequest req = WebRequest.CreateHttp(url);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Headers.Add(HttpRequestHeader.Authorization, "Bot " + BotToken);
                req.GetRequestStream().Write(Encoding.Default.GetBytes(body.ToString()));
                WebResponse res = req.GetResponse();
                JObject o = JObject.Parse(new StreamReader(res.GetResponseStream()).ReadToEnd());
                res.Close();
                return o;
            }
            catch (Exception)
            {
                return null;
            }
            
        }
        public static JObject Patch(string url, JObject body, string BotToken)
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            req.Method = "PATCH";
            req.ContentType = "application/json";
            req.Headers.Add(HttpRequestHeader.Authorization, "Bot " + BotToken);
            req.GetRequestStream().Write(Encoding.Default.GetBytes(body.ToString()));
            WebResponse res = req.GetResponse();
            JObject o = JObject.Parse(new StreamReader(res.GetResponseStream()).ReadToEnd());
            res.Close();
            return o;
        }
        public static JObject Put(string url, JObject body, string BotToken, bool sendbody = true)
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            req.Method = "PUT";
            req.Headers.Add(HttpRequestHeader.Authorization, "Bot " + BotToken);
            if (sendbody)
            {
                req.ContentType = "application/json";
                req.GetRequestStream().Write(Encoding.Default.GetBytes(body.ToString()));
            }
            WebResponse res = req.GetResponse();
            JObject o = JObject.Parse(new StreamReader(res.GetResponseStream()).ReadToEnd());
            res.Close();
            return o;
        }
        public static JObject Delete(string url, JObject body, string BotToken, bool sendbody = true)
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            req.Method = "DELETE";
            req.Headers.Add(HttpRequestHeader.Authorization, "Bot " + BotToken);
            if (sendbody)
            {
                req.ContentType = "application/json";
                req.GetRequestStream().Write(Encoding.Default.GetBytes(body.ToString()));
            }
            WebResponse res = req.GetResponse();
            JObject o = JObject.Parse(new StreamReader(res.GetResponseStream()).ReadToEnd());
            res.Close();
            return o;
        }
    }
}
