/*
    MIT license
    Ping tatadoan@outlook.com for Info | Bug | More Feature
    I'd be happy to have a response
*/
using System;
using System.Configuration;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Reflection;
using Tlang.JSON;

namespace Tlang.Web
{
    public class Connection
    {
        public Socket Sock;
        public Thread Thread;
        protected WebApp Parent;
        public string Key;
        public DateTime Last = DateTime.Now;
        public byte Mode = 0;
        public byte[] buffer = new byte[4096];
        public int CountOut = 1000;

        public MemoryStream mem = new MemoryStream();
        public MemoryStream memchun;
        public bool IsEndWhenDone = false;//http 1.0 end when done
        public bool IsRun = false;
        public bool IsDone = false;
        public bool IsError = false;
        public bool IsTimeOut = false;
        public bool IsChunked = false;
        public bool IsWebSocket = false;
        public bool IsStream = false;
        public bool IsKeepAlive = false;
        byte[] boundaryKey = null;
        public int readbyte = 0;
        public int tryCount = 0;
        public string[] Headers;
        public int HeadLength = -1;
        public int ContentIndex = -1;// la vi tri bat dau content, tuc la sau 0x0D, 0x0A, 0x0D, 0x0A
        public long ContentLength = -1;
        public long TotalLength = 0;
        public string ContentType = "";
        bool fs = false;
        byte[] receiveData;
        public string Paras = "";
        public Request req;
        public Response res;

        

        

        public string LocalIp
        {
            get
            {
                if (Sock != null)
                {
                    return ((IPEndPoint)Sock.LocalEndPoint).Address.ToString();
                }
                return "";
            }
        }

        public string RemoteIp
        {
            get
            {
                if (Sock != null)
                {
                    return ((IPEndPoint)Sock.RemoteEndPoint).Address.ToString();
                }
                return "";
            }
        }

        public bool IsConnected()
        {
            if (SocketIsDisposed(Sock)) return false;
            return !((Sock.Poll(1000, SelectMode.SelectRead) && (Sock.Available == 0)) || !Sock.Connected);
        }

        public static bool SocketIsDisposed(Socket s)
        {
            BindingFlags bfIsDisposed = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty;
            // Retrieve a FieldInfo instance corresponding to the field
            PropertyInfo field = s.GetType().GetProperty("CleanedUp", bfIsDisposed);
            // Retrieve the value of the field, and cast as necessary
            return (bool)field.GetValue(s, null);
        }



        public Connection(Socket soc, WebApp parent)
        {
            Sock = soc;
            Parent = parent;
            Key = soc.RemoteEndPoint.ToString();
        }

        public void Start()
        {
            Thread = new Thread(Handling);
            Thread.Start();
        }





        byte[] WebSocketReader(byte[] data)
        {
            bool fin = (data[0] & 0x80) != 0,
                    mask = (data[1] & 0x80) != 0; // must be true, "All messages from the client to the server have this bit set"

            int opcode = data[0] & 0x0f, // expecting 1 - text message
                msglen = data[1] - 128, // & 0111 1111
                offset = 2;

            Mode = (byte)opcode;

            if (msglen == 126)
            {
                // was ToUInt16(bytes, offset) but the result is incorrect
                msglen = BitConverter.ToUInt16(new byte[] { data[3], data[2] }, 0);

                offset = 4;
            }
            else if (msglen == 127)
            {
                Console.WriteLine("TODO: msglen == 127, needs qword to store msglen");
                // i don't really know the byte order, please edit this
                // msglen = BitConverter.ToUInt64(new byte[] { bytes[5], bytes[4], bytes[3], bytes[2], bytes[9], bytes[8], bytes[7], bytes[6] }, 0);
                // offset = 10;
            }

            if (msglen == 0)
                Console.WriteLine("msglen == 0");
            else if (mask)
            {
                if (data.Length < msglen && Mode != 8)
                {
                    //if (sslStream != null)
                    //{
                    //    int bytesre = 0;
                    //    do
                    //    {
                    //        bytesre = sslStream.Read(buffer, 0, buffer.Length);
                    //        mem.Write(buffer, 0, bytesre);
                    //        if (mem.Length > msglen) break;
                    //    } while (bytesre != 0);
                    //    data = mem.ToArray();
                    //}
                    //else
                    //{
                    //    return new byte[0];
                    //}
                }
                byte[] decoded = new byte[msglen];
                byte[] masks = new byte[4] { data[offset], data[offset + 1], data[offset + 2], data[offset + 3] };
                offset += 4;

                for (int i = 0; i < msglen; ++i)
                    decoded[i] = (byte)(data[offset + i] ^ masks[i % 4]);

                mem.Position = 0;
                mem.SetLength(0);
                return decoded;
                //Console.WriteLine("{0}", text);
                //return text;
            }
            else
            {
                Console.WriteLine("mask bit not set" + data.Length + "--" + Encoding.ASCII.GetString(data));
            }
            return new byte[0];
        }

        //string WebSocketText(byte[] data)
        //{
        //    bool fin = (data[0] & 0x80) != 0,
        //            mask = (data[1] & 0x80) != 0; // must be true, "All messages from the client to the server have this bit set"

        //    int opcode = data[0] & 0x0f, // expecting 1 - text message
        //        msglen = data[1] - 128, // & 0111 1111
        //        offset = 2;

        //    if (msglen == 126)
        //    {
        //        // was ToUInt16(bytes, offset) but the result is incorrect
        //        msglen = BitConverter.ToUInt16(new byte[] { data[3], data[2] }, 0);
        //        offset = 4;
        //    }
        //    else if (msglen == 127)
        //    {
        //        Console.WriteLine("TODO: msglen == 127, needs qword to store msglen");
        //        // i don't really know the byte order, please edit this
        //        // msglen = BitConverter.ToUInt64(new byte[] { bytes[5], bytes[4], bytes[3], bytes[2], bytes[9], bytes[8], bytes[7], bytes[6] }, 0);
        //        // offset = 10;
        //    }

        //    if (msglen == 0)
        //        Console.WriteLine("msglen == 0");
        //    else if (mask)
        //    {
        //        byte[] decoded = new byte[msglen];
        //        byte[] masks = new byte[4] { data[offset], data[offset + 1], data[offset + 2], data[offset + 3] };
        //        offset += 4;

        //        for (int i = 0; i < msglen; ++i)
        //            decoded[i] = (byte)(data[offset + i] ^ masks[i % 4]);

        //        string text = Encoding.UTF8.GetString(decoded);
        //        Console.WriteLine("{0}", text);
        //        return text;
        //    }
        //    else
        //    {
        //        Console.WriteLine("mask bit not set" + data.Length + "--" + Encoding.ASCII.GetString(data));
        //    }
        //    return "error";
        //}



        public virtual void Handling()
        {
            tryCount = 0;
            try
            {
                while (Parent.IsRun && Sock.Connected && CountOut > 0)
                {
                    if (!IsKeepAlive) CountOut--;
                    if (Sock.Available == 0)
                    {
                        Thread.Sleep(2);
                        if (IsStream || IsWebSocket || IsChunked)
                        {

                        }
                        else
                        {
                            tryCount++;
                            if (tryCount > 500)
                            {
                                IsRun = false;
                                IsTimeOut = true;
                                //Console.WriteLine("TimeOut: con Count " + Parent.Connections.Count);
                                break;
                            }
                        }
                        continue;
                    }
                    readbyte = Sock.Receive(buffer);
                    if (Sock.Connected && readbyte > 0)
                    {
                        tryCount = 0;
                        mem.Write(buffer, 0, readbyte);
                        CheckPool();
                    }
                    if (IsEndWhenDone)
                    {
                        if (Parent.Connections.Count > 0)
                        {
                            if (Parent.Connections.Contains(this))
                            {
                                Parent.Connections.Remove(this);
                            }
                        }
                        Sock.Close();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection Error:" + ex.Message);
                IsError = true;
            }
            finally
            {
                IsDone = true;
                IsRun = false;
                
                //Console.WriteLine("Out: " + Parent.Connection[Key].Handler.RemoteEndPoint);
                //Console.WriteLine(Encoding.ASCII.GetString(buffer));
                if (req != null)
                {
                    if (req.FileName.Length > 0)
                    {
                        IO.GetFile(req.FileName).Close();
                    }
                }
                if (IsWebSocket)
                {
                    if (Parent.ConnectionWebSockets.Count > 0)
                    {
                        if (Parent.ConnectionWebSockets.Contains(this))
                        {
                            Sock.Close();
                            Parent.ConnectionWebSockets.Remove(this);
                        }
                    }
                }
                else
                {
                    if (Parent.Connections.Count > 0)
                    {
                        if (Parent.Connections.Contains(this))
                        {
                            Sock.Close();
                            Parent.Connections.Remove(this);
                        }
                    }
                }
                
            }
        }

        protected void CheckPool()
        {
            if (Headers == null)
            {
                if (!CheckHeader()) return;
            }

            if (IsChunked)
            {
                CheckChunked();
            }
            else
            {
                if (IsWebSocket)
                {
                    CheckWebSocket();
                }
                else
                {

                    if (ContentLength > 0 && boundaryKey != null)
                    {
                        TotalLength = ContentIndex + ContentLength;
                        if (boundaryKey != null)
                        {
                            mem.Position = 0;
                            mem.SetLength(0);
                            mem.Write(buffer, ContentIndex, readbyte - ContentIndex);
                            res = new Response(this);
                            ReadBoundary();
                        }
                        else
                        {
                            while (TotalLength > mem.Length && Parent.IsRun && Sock.Connected)
                            {
                                readbyte = Read(buffer);
                                mem.Write(buffer, 0, readbyte);
                            }
                        }
                    }
                    if (ContentIndex + ContentLength == mem.Length || req.Method == "GET")
                    {
                        receiveData = F.SubBytes(mem.ToArray(), ContentIndex);
                        Done();
                        RecieveData(receiveData);
                    }
                }
            }

        }

        void Done()
        {
            IsDone = true;
            IsRun = false;
            boundaryKey = null;
            Paras = "";
            mem.Position = 0;
            mem.SetLength(0);
            Headers = null;
            ContentLength = -1;
        }

        void RecieveData(byte[] data)
        {

            if (req.Method == "POST" || req.Method == "PUT" || req.Method == "INSERT")
            {

                if (Paras.Length == 0)
                {
                    Paras = Encoding.ASCII.GetString(data);
                }
                req.UpdatePost(Paras);
            }
            req.Data = data;
            if (res == null)
            {
                res = new Response(this);
            }            
            Parent.Hand(req, res);
        }



        void ReadBoundary()
        {
            while (Parent.IsRun && Sock.Connected)
            {
                if (boundaryKey == null) return;
                while (mem.Length < boundaryKey.Length && readbyte > 0)// mem ngan qua kho kiem tra dc, doc them cho dai hon
                {
                    readbyte = Read(buffer);
                    mem.Write(buffer, 0, readbyte);
                }
                if (mem.Length == 0)
                {
                    Done();
                    return;
                }
                byte[] mm = mem.ToArray();


                int begin = F.IndexOf(mm, boundaryKey, 0);//check begin
                if (begin >= 0)//ok
                {
                    if (mm.Length == boundaryKey.Length + 4)//xong fim
                    {

                        
                        IsDone = true;
                        Parent.Hand(req, res);
                        Done();
                        return;
                    }

                    int beginhead = begin + boundaryKey.Length + 2;// for \r\n;
                    int endhead = F.IndexOf(mm, G.WebHeaderBreak, beginhead);
                    if (endhead > beginhead)
                    {
                        string[] head1 = Encoding.ASCII.GetString(F.SubBytes(mm, beginhead, endhead - beginhead)).Split('\n');
                        //Console.WriteLine(head1);
                        string fname = IO.GetFileName(head1);
                        string name = GetName(head1);
                        int beginbody = endhead + 4;
                        if (fname.Length == 0)
                        {
                            int endbody = F.IndexOf(mm, boundaryKey, beginbody);// endbody is begin another bou
                            byte[] aah = F.SubBytes(mm, beginbody, endbody - beginbody - 2);// luon du \r\n
                            Paras += name + "=" + Encoding.UTF8.GetString(aah) + "&";
                            CutStream(mem, endbody);
                        }
                        else
                        {
                            
                            Console.WriteLine("upload file " + fname);
                            req.UpdatePost(Paras);
                            CutStream(mem, beginbody);
                            req.FileName = fname;
                            //Response res = new Response(this);
                            BoundaryDone = false;
                            Parent.Hand(req, res);

                        }
                    }
                }
                else
                {
                    // sai o dau roi
                    Done();
                    return;
                }
            }
        }

        bool BoundaryDone = false;

        public bool ReadStream()
        {
            tryCount = 0;
            if (BoundaryDone)
            {
                return false;
            }
            while (mem.Length < boundaryKey.Length && readbyte > 0 && Parent.IsRun && Sock.Connected)
            {
                readbyte = Read(buffer);
                mem.Write(buffer, 0, readbyte);
            }
            if (mem.Length < 5)
            {
                Done();
                req.FileName = "";
                return false;
            }
            byte[] mm = mem.ToArray();
            //if (mm.Length == 44)
            //{
            //    string contt = Encoding.ASCII.GetString(mm);
            //}
            int begin = F.IndexOf(mm, boundaryKey, 0);//check begin
            if (begin >= 0)//ok
            {
                req.Data = F.SubBytes(mm, 0, begin - 2);
                CutStream(mem, begin);
                req.FileName = "";
                BoundaryDone = true;
                return true;
            }
            else
            {
                req.Data = mm;
                mem.Position = 0;
                mem.SetLength(0);
                return true;
            }
            return false;
        }

        bool CheckHeader()
        {
            byte[] hbuffer = F.SubRightBytes(mem.ToArray(), G.WebHeaderBreak);

            if (hbuffer == null || hbuffer.Length == 0) return false;
            string headerString = Encoding.ASCII.GetString(hbuffer);
            if (headerString.IndexOf("filename=") > 0)
            {
                headerString = Encoding.UTF8.GetString(hbuffer);//tai sao utf 7
            }
            Headers = headerString.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            //Console.WriteLine("\r\n");
            //Console.WriteLine(headerString);

            HeadLength = hbuffer.Length;

            IsDone = false;


            req = new Request(this);
            req.Headers = Headers;

            string h0 = Headers[0];
            if (h0.IndexOf("HTTP/1.0")>0)// http 1.0
            {
                IsEndWhenDone = true;
            }
            string method = h0.Substring(0, h0.IndexOf(' '));
            int j = h0.IndexOf('/');
            string url = "";
            if (j > 0)
            {
                url = h0.Substring(j + 1, h0.IndexOf(' ', j) - j - 1);
            }
            req.Method = method;
            if (url.Contains("%"))
            {
                url = System.Uri.UnescapeDataString(url);
            }
            req.Path = url;
            j = url.IndexOf('?');
            if (j >= 0)
            {
                string UrlParamRaw = url.Substring(j + 1);
                req.Path = url.Substring(0, j);
                req.UpdateGet(UrlParamRaw);
            }
            else
            {
                req.Get = new Node();
            }
            
            req.UpdateHeader();
            req.UpdateCookies();

            



            ContentLength = (req.Head.ContainsKey(G.ContentLength)) ? long.Parse(req.Head[G.ContentLength].ToString()) : 0;


            if (req.Head.ContainsKey(G.ContentType))
            {
                ContentType = req.Head[G.ContentType].ToString();
                j = ContentType.IndexOf("boundary=");
                if (j > 0)
                {
                    string bou = ContentType.Substring(j + 9);
                    boundaryKey = Encoding.ASCII.GetBytes("--" + bou);
                }
                else
                {
                    boundaryKey = null;
                }

            }

            if (req.Head.ContainsKey(G.Connection))
            {
                string conn = req.Head[G.Connection].ToString();
                if (conn.ToLower().IndexOf("keep-alive") >= 0)
                {
                    IsKeepAlive = true;
                }

            }


            if (ContentLength == 0)
            {
                IsChunked = req.Head.ContainsKey(G.TransferEncoding) && req.Head[G.TransferEncoding].ToString().IndexOf("chunked") >= 0;
                if (IsChunked)
                {
                    IsStream = CheckIsStream(Headers);
                    memchun = new MemoryStream();
                    if (mem.Length > HeadLength + 4)//\r\n\r\n
                    {
                        CutStream(mem, HeadLength + 4);
                    }
                    else
                    {
                        mem.Position = 0;
                        mem.SetLength(0);
                        return false;
                    }
                }
            }
            IsWebSocket = req.Head.ContainsKey(G.SecWebSocketKey);
            if (IsWebSocket)
            {
                string swk = req.Head[G.SecWebSocketKey].ToString().Trim();
                string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                string swkaSha1Base64 = G.SHA1Base64(swka);

                byte[] response = Encoding.UTF8.GetBytes(
                    "HTTP/1.1 101 Switching Protocols\r\n" +
                    "Connection: Upgrade\r\n" +
                    "Upgrade: websocket\r\n" +
                    "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");
                //Console.WriteLine(Encoding.ASCII.GetString(data));
                Send(response);
                mem.Position = 0;
                mem.SetLength(0);


                //Handler.SendTimeout = 0;
                IsKeepAlive = true;

                Parent.Connections.Remove(this);
                Parent.ConnectionWebSockets.Add(this);
                //BuildRequest();
                req.Status = 1;
                if (Parent.WebSocketConnected != null)
                {
                    Response res = new Response(this);
                    Parent.WebSocketConnected(req, res);
                }
                Console.WriteLine("Websocket Client connected");
            }
            else
            {
                if (res != null)
                {
                    res.Handled = false;
                    res.Data = null;
                    res.Content = "";
                }
            }

            ContentIndex = HeadLength + 4;//0x0D, 0x0A, 0x0D, 0x0A
            return true;
        }

        bool CheckChunked()
        {
            if (mem.Length == 0) return false;
            int chunsize = 0;
            byte[] cbuffer = mem.ToArray();
            int ii = F.IndexOf(cbuffer, G.WebLineBreak, 0);
            byte[] cc = F.SubBytes(cbuffer, 0, ii);

            chunsize = int.Parse(Encoding.ASCII.GetString(cc), System.Globalization.NumberStyles.HexNumber);
            // byte[] size = Encoding.ASCII.GetBytes(String.Format("{0:x2}", chunsize));
            if (chunsize > 0)
            {
                if (cc.Length + 2 + chunsize <= cbuffer.Length)
                {
                    if (IsStream)
                    {
                        receiveData = F.SubBytes(cbuffer, cc.Length + 2, chunsize);
                        RecieveData(receiveData);
                    }
                    else
                    {
                        memchun.Write(cbuffer, cc.Length + 2, chunsize);
                    }
                    CutStream(mem, cc.Length + 4 + chunsize);
                    if (mem.Length > 0 && mem.Length < 10)
                    {
                        ii = F.IndexOf(mem.ToArray(), G.WebLineBreak, 0);
                        if (ii > 0)
                        {
                            cc = F.SubBytes(mem.ToArray(), 0, ii);
                            chunsize = int.Parse(Encoding.ASCII.GetString(cc), System.Globalization.NumberStyles.HexNumber);
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            if (chunsize == 0)
            {
                if (!IsStream)
                {
                    receiveData = memchun.ToArray();
                    RecieveData(receiveData);
                }
                IsDone = true;
                IsRun = false;
                mem.Position = 0;
                mem.SetLength(0);
                Headers = null;
                //if (ReceiveData != null)
                //{
                //    ReceiveData(this, receiveData);
                //}
            }
            return true;
        }

        bool CheckWebSocket()
        {
            if (mem.Length > 0)
            {
                receiveData = mem.ToArray();
                //mem.Position = 0;
                //mem.SetLength(0);



                receiveData = WebSocketReader(receiveData);
                //Console.WriteLine(Encoding.UTF8.GetString(receiveData));
                if (receiveData.Length > 0)
                {
                    if (Mode == 8)
                    {
                        //close websocket
                    }
                    else if (Mode == 9)//ping
                    {
                        Console.WriteLine("Websocket ping");
                        Send(BinaryBuilder.WebSocket(10, receiveData, true));//pong

                    }
                    else if (Mode == 10)//ping
                    {
                        //Send(NetData.WebSocket(10, receiveData, true));//pong
                        Console.WriteLine("Websocket pong");
                    }
                    else
                    {
                        if (receiveData.Length > 3 && receiveData[0] == 0x23)// bat dau bang #
                        {
                            string text = Encoding.UTF8.GetString(receiveData).Substring(1);
                            int j = text.IndexOf('=');
                            if (j > 0)
                            {
                                string name = text.Substring(0, j);
                                string value = text.Substring(j + 1);


                            }
                        }
                        else
                        {
                            
                            //RecieveSocket(receiveData);
                            req.Data = receiveData;
                            req.Status = 2;
                            if (Parent.WebSocketData != null)
                            {
                                Response res = new Response(this);
                                Parent.WebSocketData(req, res);
                            }

                            Console.WriteLine(Encoding.UTF8.GetString(receiveData));
                        }
                    }
                }
                //Send(NetData.WebSocket(Encoding.UTF8.GetBytes(webtext), true));
            }
            return true;
        }

        void ParserHeaders(string[] headers)
        {
            if (headers != null)
            {
                foreach (string h in headers)
                {
                    if (h.IndexOf("Content-Length") >= 0)
                    {
                        ContentLength = long.Parse(h.Substring(h.IndexOf(':') + 1).Trim());
                    }
                    else if (h.IndexOf("chunked") >= 0)
                    {
                        IsChunked = true;
                    }
                    else if (h.IndexOf("Sec-WebSocket-Key") >= 0)
                    {
                        IsWebSocket = true;
                    }
                    else
                    {

                    }
                }
            }
        }

        void CutStream(MemoryStream m, int start)
        {
            if (start < m.Length)
            {
                byte[] b = F.SubBytes(m.ToArray(), start);
                m.Position = 0;
                m.SetLength(0);
                m.Write(b, 0, b.Length);
            }
        }

        string GetName(string[] headers)
        {
            if (headers != null)
            {
                foreach (string h in headers)
                {
                    if (h.IndexOf(" name=") > 0)
                    {
                        string n = h.Substring(h.IndexOf(" name=") + 6).Trim();
                        n = n.Substring(1, n.Length - 2);
                        if (n.IndexOf('&') >= 0)
                        {
                            n = IO.HtmlDecode(n).Replace('?', ' ');
                        }
                        return n;
                    }
                }
            }
            return "";
        }

        bool CheckIsStream(string[] headers)
        {
            if (headers != null)
            {
                foreach (string h in headers)
                {
                    if (h.IndexOf("User-Agent: Lavf") >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool VerifyClientCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate != null)
            {
                Console.WriteLine("");
                //certificate.
            }

            //Console.WriteLine("");
            return true;
        }

        public virtual int Read(byte[] buffer)
        {
            return Sock.Receive(buffer);
        }

        public virtual void Send(byte[] data)
        {

            if (!SocketIsDisposed(Sock))
                Sock.Send(data);

        }

        public void SendWebSocket(string text)
        {
            try
            {
                if (IsWebSocket)
                    Send(BinaryBuilder.WebSocket(Encoding.UTF8.GetBytes(text), true));
            }
            catch
            {

            }
        }

        public void SendWebSocket(string text, string userName)
        {
            byte[] data = BinaryBuilder.WebSocket(Encoding.UTF8.GetBytes(text), true);
            for (int i = 0; i < Parent.ConnectionWebSockets.Count; i++)
            {
                Connection con = Parent.ConnectionWebSockets[i];
                if (con.req != null && con.IsWebSocket&& con.req.UserName == userName)
                {
                    try
                    {
                        con.Send(data);
                    }
                    catch { }
                }
            }
        }

        public void SendWebSocketRoom(string text, string room)
        {
            byte[] data = BinaryBuilder.WebSocket(Encoding.UTF8.GetBytes(text), true);
            for (int i = 0; i < Parent.ConnectionWebSockets.Count; i++)
            {
                Connection con = Parent.ConnectionWebSockets[i];
                if (con.req != null && con.IsWebSocket && con.req.Room == room)
                {
                    try
                    {
                        con.Send(data);
                    }
                    catch { }
                }
            }
        }
    }
}
