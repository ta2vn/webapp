/*
    MIT license
    Ping tatadoan@outlook.com for Info | Bug | More Feature
    I'd be happy to have a response
 */
using System;
using System.Collections.Generic;
using System.Text;


namespace Tlang.Web
{
    public class Response
    {
        Connection _Con;

        public Response(Connection con)
        {
            _Con = con;
        }

        public string ResponseCode = "200";
        public string ContentType = "html";
        public string Charset = "";
        public bool Handled = false;
        public List<string> Headers = new List<string>();
        public byte[] Data;

        public bool ReadStream()
        {
            return _Con.ReadStream();
        }

        public void Send(byte[] data)
        {
            _Con.Send(data);
        }

        public void SendFile(string filename)
        {
            Handled = true;
            BinaryBuilder.SendFile(filename, _Con);
        }

        public void JSON(object obj)
        {
            Handled = true;
            ContentType = "text/json";
            Data = BinaryBuilder.Message(Reader.ToJSON(obj), "200", ContentType);
            _Con.Send(Data);
        }

        /// <summary>
        /// use for set cookie die in expires second
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="expires"></param>
        public void SetCookie(string name, string value, int expires)
        {
            string s = "Set-Cookie: " + name + "=" + System.Uri.EscapeDataString(value) + "; Max-Age:" + expires;
            Headers.Add(s);
        }

        public void SetCookie(string name, string value, int expires,string path)
        {
            string s = "Set-Cookie: " + name + "=" + System.Uri.EscapeDataString(value) + "; expires=" + T.GMT(System.DateTime.Now.AddMilliseconds(expires)) + "; path=" + path;
            Headers.Add(s);
        }

        public void ClearCookie(string name)
        {
            string s = "Set-Cookie: " + name + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT;";
            Headers.Add(s);
        }



        /// <summary>
        /// use for login and rediect to page inside
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="expires"></param>
        /// <param name="url"></param>
        public void Cookie(string name, string value, int expires, string url)
        {
            Handled = true;
            string s = "<script>document.cookie=\"" + name + "=" + System.Uri.EscapeDataString(value) + "; expires=" + T.GMT(System.DateTime.Now.AddMilliseconds(expires)) + "; path=/\"; document.location.href=\"" + T.EncodeString(url) + "\";</script>";
            Data = BinaryBuilder.Message(s, "200", ContentType);
            _Con.Send(Data);
        }

        public void Redirect(string url)
        {
            Handled = true;            
            Data = BinaryBuilder.MessageRedirectWeb(url, Headers.ToArray());
            _Con.Send(Data);
        }
        

        public void SendSocket(string text)
        {
            Handled = true;
            _Con.SendWebSocket(text);
        }

        public void SendSocket(string text, string username)
        {
            Handled = true;
            _Con.SendWebSocket(text, username);
        }

        public void End()
        {
            _Con.Sock.Disconnect(true);
        }

        public void Render(string text, object value)
        {
            Tem t = new Tem();
            t.Parser(text);
            Render(t, value);
        }

        public void Render(Tem tem, object value)
        {
            Handled = true;
            string html = tem.Render(value);
            Data = BinaryBuilder.Message(html, "200", ContentType);
            _Con.Send(Data);
        }
    }
}
