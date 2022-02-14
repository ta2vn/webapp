/*
    MIT license
    Ping tatadoan@outlook.com for Info | Bug | More Feature
    I'd be happy to have a response
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading;

namespace Tlang.Web
{
    public static class G
    {
        public const string ServerName = "Tata Server";
        
        public const string MyUserAgent = "Mozilla/5.0 (Linux; Android 7.0; Pixel C Build/NRD90M; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.98 Safari/537.36";

        public const string ContentLength = "Content-Length";

        public const string ContentType = "Content-Type";

        public const string UserAgent = "User-Agent";

        public const string Host = "Host";

        public const string Accept = "Accept";

        public const string SecWebSocketKey = "Sec-WebSocket-Key";

        public const string ContentDisposition = "Content-Disposition";

        public const string TransferEncoding = "Transfer-Encoding";

        public const string Connection = "Connection";

        

        public static byte[] WebHeaderBreak = new byte[] { 0x0D, 0x0A, 0x0D, 0x0A };
        public static byte[] WebLineBreak = new byte[] { 0x0D, 0x0A };

        //websocket
        public static string SHA1Base64(string value)
        {
            SHA1Managed hashString = new SHA1Managed();
            byte[] b1 =  Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String( hashString.ComputeHash(b1));
        }

        
    }
}
