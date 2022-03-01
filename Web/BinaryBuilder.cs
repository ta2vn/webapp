/*
    MIT license
    Ping tatadoan@outlook.com for Info | Bug | More Feature
    I'd be happy to have a response
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Tlang;

namespace Tlang.Web
{
    public static class BinaryBuilder
    {

        public static string AccessControlAllowOrigin = "Access-Control-Allow-Origin: *\r\n";
        public static string AccessControlAllowMethods ="Access-Control-Allow-Methods: *\r\n";
        public static string AccessControlAllowHeaders = "Access-Control-Allow-Headers: *\r\n";

        public static string GetContentType(string filename)
        {
            int j = filename.LastIndexOf('.');
            if (j > 0)
            {
                string ext = filename.Substring(j + 1);
                switch (ext)
                {
                    case "css":
                        return "text/css";
                    case "js":
                        return "text/javascript";
                    case "mht":
                        return "message/rfc822";
                    case "htm":
                    case "html":
                        return "text/html";
                    case "txt":
                    case "log":
                    case "tata":
                    case "csv":
                    case "json":
                    case "xml":
                        return "text/plain";
                    case "jpeg":
                    case "jpg":
                        return "image/jpeg";
                    case "png":
                        return "image/png";
                    case "svg":
                        return "image/svg+xml";
                    case "doc":
                    case "dot":
                        return "application/msword";
                    case "xls":
                    case "xlt":
                    case "xla":
                        return "application/vnd.ms-excel";
                    case "ppt":
                    case "pot":
                    case "pps":
                        return "application/vnd.ms-powerpoint";
                    case "docx":
                        return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    case "xlsx":
                        return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    case "pptx":
                        return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    case "webm":
                        return "video/webm";
                    case "avi":
                        return "video/x-msvideo";
                    case "mkv":
                        return "video/x-matroska";
                    default:
                        return "application";


                }

            }
            return "application";
        }

        public static bool SendFile(string fileName, Connection socket)// header and content buffer
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(fileName);
            try
            {
                if (fi.Exists)
                {
                    long flength = fi.Length;
                    int BufferSize = Math.Min((int)flength, 80000);
                    byte[] header = System.Text.Encoding.ASCII.GetBytes(
                                        "HTTP/1.1 200\r\n"
                                      + "Server: " + G.ServerName + "\r\n"
                                      + "Content-Length: " + flength + "\r\n"
                                      + AccessControlAllowOrigin
                                      + AccessControlAllowMethods
                                      + AccessControlAllowHeaders
                                      + "Connection: keep-alive\r\n"
                                      + "Content-Type: " + GetContentType(fileName) + "\r\n\r\n");



                    byte[] buffer = new byte[BufferSize];
                    int readedCount = 0;
                    int tocount = (int)(flength / BufferSize * 4);// number of try read file

                    FileStream fs = IO.GetFile(fileName);
                    socket.Send(header);

                    long sendedCount = 0;
                    fs.Seek(0, SeekOrigin.Begin);
                    readedCount = fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                    while (readedCount > 0 && socket.Sock.Connected && tocount > 0)
                    {
                        if (readedCount == buffer.Length)
                        {
                            socket.Send(buffer);
                        }
                        else
                        {
                            byte[] bufferlast = new byte[readedCount];
                            System.Buffer.BlockCopy(buffer, 0, bufferlast, 0, readedCount);
                            buffer = bufferlast;
                            socket.Send(bufferlast);
                        }
                        tocount--;
                        sendedCount += readedCount;


                        fs = IO.GetFile(fileName);

                        fs.Seek(sendedCount, SeekOrigin.Begin);
                        readedCount = fs.Read(buffer, 0, buffer.Length);

                        if (readedCount == 0)
                        {
                            readedCount = (int)(flength - sendedCount);
                            byte[] bufferlast = new byte[readedCount];
                            System.Buffer.BlockCopy(buffer, 0, bufferlast, 0, readedCount);
                            buffer = bufferlast;
                            socket.Send(bufferlast);

                            break;
                        }
                        //Console.WriteLine("DownFile :" + total + " / " + flength);
                    }

                    fs.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendFileOnWeb " + fileName + "===>" + ex.Message);
                socket.Send(Html("File not found", "404", "error"));
            }
            return false;
        }

        public static bool SendFileWithRange(string fileName, Connection socket, long start, long end)// header and content buffer
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(fileName);
            //int bufferSize = 10000 * 150;

            if (fi.Exists)
            {

                long flength = fi.Length;
                int BufferSize = Math.Min((int)flength, 80000);
                if (end == -1)
                {
                    end = flength - 1;
                }
                long leng = end - start + 1;
                if (start > flength)
                {
                    //error

                    //MessageOnWeb("ERROR", "200", "text/plain");
                    socket.Send(Html("File Over Start", "200", "error"));
                    return false;
                }
                if (end >= flength)
                {
                    leng = flength - start + 1;
                    end = flength - 1;
                }
                //Console.WriteLine("start:" + start + " end:" + end + " leng:" + leng);
                string headerString =
                                    "HTTP/1.1 206 Partial Content\r\n"
                                  + "Server: " + G.ServerName + "\r\n"
                                  + "Content-Length: " + leng + "\r\n"
                                  + "Connection: keep-alive\r\n"
                                  + "Accept-Ranges: bytes\r\n"
                                  + "Content-Range: bytes " + start + "-" + end + "/" + flength + "\r\n"
                                  + "Content-Type: " + GetContentType(fileName) + "\r\n\r\n";
                //Console.WriteLine(headerString);
                byte[] header = System.Text.Encoding.ASCII.GetBytes(headerString);
                socket.Send(header);
                if (start == end) return true;

                byte[] buffer = new byte[BufferSize];
                int count;

                FileStream fs = IO.GetFile(fileName);
                fs.Seek(start, SeekOrigin.Begin);

                long total = start;

                count = fs.Read(buffer, 0, buffer.Length);

                while (socket.Sock.Connected && count > 0)
                {

                    if (count != buffer.Length)// neu load ko dung se fix lai size
                    {
                        byte[] bufferlast = new byte[count];
                        System.Buffer.BlockCopy(buffer, 0, bufferlast, 0, count);
                        buffer = bufferlast;
                    }
                    if (total + count >= end)//last
                    {
                        int last = (int)end - (int)total + 1;
                        byte[] bufferlast = new byte[last];
                        System.Buffer.BlockCopy(buffer, 0, bufferlast, 0, last);
                        buffer = bufferlast;
                        socket.Send(buffer);
                        fs.Close();
                        return true;
                    }
                    socket.Send(buffer);
                    total += count;
                    fs.Seek(total, SeekOrigin.Begin);
                    count = fs.Read(buffer, 0, buffer.Length);
                }
                fs.Close();
                return true;
            }
            else
            {
                socket.Send(Html("File not found", "404", "error"));
            }
            return false;
        }

        #region Data
        public static byte[] Html(string message, string responseCode, string contentType)
        {
            return Html(message, responseCode, contentType, "", new string[] { });
        }

        public static byte[] Html(string message, string responseCode, string contentType, string charset,string[] header)// header and content
        {
            System.Text.Encoding ee = (charset.Length > 0) ? System.Text.Encoding.GetEncoding(charset) : System.Text.Encoding.ASCII;
            byte[] data = ee.GetBytes(message);
            string chh = (charset.Length > 0) ? ";charset=" + charset : "";
            string head = "HTTP/1.1 " + responseCode + "\r\n"
                              + "Server: " + G.ServerName + "\r\n"
                              + "Content-Length: " + data.Length.ToString() + "\r\n"
                              + AccessControlAllowOrigin
                              + AccessControlAllowMethods
                              + AccessControlAllowHeaders
                              + "Cache-Control: no-cache, no-store, must-revalidate\r\n"
                              + "Pragma: no-cache\r\n"
                              + "Expires: 0\r\n"
                              + "Connection: keep-alive\r\n"
                              + "Content-Type: " + contentType + chh + "\r\n";

            foreach (string h in header)
            {
                head += h + "\r\n";
            }
            head += "\r\n";
            byte[] bhead = System.Text.Encoding.ASCII.GetBytes(head);
            byte[] buffer = new byte[bhead.Length + data.Length];
            System.Buffer.BlockCopy(bhead, 0, buffer, 0, bhead.Length);
            System.Buffer.BlockCopy(data, 0, buffer, bhead.Length, data.Length);
            return buffer;

        }

        public static byte[] Message(string message, string responseCode, string contentType)// header and content
        {

            byte[] data = Encoding.ASCII.GetBytes(message);
            string head = "HTTP/1.1 " + responseCode + "\r\n"
                              + "Server: " + G.ServerName + "\r\n"
                              + "Content-Length: " + data.Length.ToString() + "\r\n"
                              + "Content-Type: " + contentType + "\r\n\r\n";
            byte[] bhead = System.Text.Encoding.ASCII.GetBytes(head);
            byte[] buffer = new byte[bhead.Length + data.Length];
            System.Buffer.BlockCopy(bhead, 0, buffer, 0, bhead.Length);
            System.Buffer.BlockCopy(data, 0, buffer, bhead.Length, data.Length);
            return buffer;
        }


        public static byte[] MessageRedirectWeb(string location, string[] header)// header and content
        {
            string head = "HTTP/1.1 303 See Other\r\n"
                             + "Content-Length: 0\r\n"
                             + AccessControlAllowOrigin
                             + AccessControlAllowMethods
                             + AccessControlAllowHeaders
                             + "Location: " + location + "\r\n";
            foreach (string h in header)
            {
                head += h + "\r\n";
            }
            head += "\r\n";
            return System.Text.Encoding.ASCII.GetBytes(head);
        }

        public static byte[] Unauthorized(string content, string[] header)
        {
            string head = "HTTP/1.1 401\r\n"
                        + "Content-Length: 0\r\n"
                        + "WWW-Authenticate: Basic realm=\"" + content + "\"\r\n";
                                        
            foreach (string h in header)
            {
                head += h + "\r\n";
            }
            head += "\r\n";
            return System.Text.Encoding.ASCII.GetBytes(head);
        }
       

        public static byte[] WebSocket(byte[] payload, bool isLastFrame)
        {
            return WebSocket(0x01, payload, isLastFrame);
        }

        public static byte[] WebSocket(byte opCode, byte[] payload, bool isLastFrame)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {

                byte finBitSetAsByte = isLastFrame ? (byte)0x80 : (byte)0x00;
                byte byte1 = (byte)(finBitSetAsByte | (byte)opCode);
                memoryStream.WriteByte(byte1);

                // NB, set the mask flag if we are constructing a client frame
                //byte maskBitSetAsByte = _isClient ? (byte)0x80 : (byte)0x00;
                byte maskBitSetAsByte = (byte)0x00;

                // depending on the size of the length we want to write it as a byte, ushort or ulong
                if (payload.Length < 126)
                {
                    byte byte2 = (byte)(maskBitSetAsByte | (byte)payload.Length);
                    memoryStream.WriteByte(byte2);
                }
                else if (payload.Length <= ushort.MaxValue)
                {
                    byte byte2 = (byte)(maskBitSetAsByte | 126);
                    memoryStream.WriteByte(byte2);
                    byte[] bb = BitConverter.GetBytes((ushort)payload.Length);
                    memoryStream.WriteByte(bb[1]);
                    memoryStream.WriteByte(bb[0]);
                }
                else
                {
                    byte byte2 = (byte)(maskBitSetAsByte | 127);
                    memoryStream.WriteByte(byte2);
                    byte[] bb = BitConverter.GetBytes((ulong)payload.Length);
                    memoryStream.WriteByte(bb[3]);
                    memoryStream.WriteByte(bb[2]);
                    memoryStream.WriteByte(bb[1]);
                    memoryStream.WriteByte(bb[0]);

                }

                //// if we are creating a client frame then we MUST mack the payload as per the spec
                //if (_isClient)
                //{
                //    byte[] maskKey = new byte[WebSocketFrameCommon.MaskKeyLength];
                //    _random.NextBytes(maskKey);
                //    memoryStream.Write(maskKey, 0, maskKey.Length);

                //    // mask the payload
                //    WebSocketFrameCommon.ToggleMask(maskKey, payload);
                //}

                memoryStream.Write(payload, 0, payload.Length);
                byte[] buffer = memoryStream.ToArray();
                return buffer;
            }
        }

        

        #endregion

        #region WebDAV

        public static byte[] HandleOPTIONS()
        {
            string head = "HTTP/1.1 200 OK\r\n"
            + "Date: " + System.DateTime.Now.ToString("R") + "\r\n"
            + "Server: Tata\r\n"
            + "Connection: Keep-Alive\r\n"
            + "Allow: *r\n"
            //+ "Allow: GET, OPTIONS, HEAD, PROPFIND, PROPPATCH, PUT, MKCOL, COPY, LOCK, UNLOCK\r\n"
                //+ "MS-Author-Via: DAV\r\n"
                //+ "DAV: 1,2\r\n"
                //+ "Accept-Ranges: bytes\r\n"
                //+ "Set-Cookie: session=webdav; Expires=" + DateTime.Now.AddDays(1).ToString("R") + "; Path=/\r\n"
            + "Content-Length: 0\r\n\r\n";
            return Encoding.ASCII.GetBytes(head);
        }

        public static byte[] HandlePROPFIND(string url, string deep)
        {
            
            string content = "";
            string header = "HTTP/1.1 207 Multi-Status\r\n"
            + "Server: Tata\r\n"
            + "Content-Type: application/xml; charset=\"utf-8\"\r\n"
            + "Content-Length: {}\r\n"
            + "Set-Cookie: session=webdav; Expires=" + DateTime.Now.AddDays(1).ToString("R") + "; Path=/\r\n"
            + "Date: " + System.DateTime.Now.ToString("R") + "\r\n\r\n";

            string hh = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?><d:multistatus xmlns:D=\"DAV:\">";
            string kk = "</d:multistatus>";

            string path = System.IO.Path.Combine(App.Program.MyPath, url);
            if (Directory.Exists(path))
            {
                if (deep == "1")
                {
                    DirectoryInfo di = new DirectoryInfo(path);

                    string mm = ResponseInfoFolder(url, di.LastWriteTime, di.CreationTime, di.Name); ;
                    string d = "/";
                    if (url.Length > 0)
                    {
                        d = "/" + url + "/";
                    }
                    DirectoryInfo[] dis = di.GetDirectories();
                    foreach (DirectoryInfo d1 in dis)
                    {
                        mm += ResponseInfoFolder(url + "/" + d1.Name, d1.LastWriteTime, d1.CreationTime, d1.Name) + "\r\n";
                        //Console.WriteLine(mm);
                    }

                    FileInfo[] fs = di.GetFiles();
                    foreach (FileInfo f in fs)
                    {
                        mm += ResponseInfoFile(d + f.Name, f.LastWriteTime, (int)f.Length, f.CreationTime, f.Name) + "\r\n";
                        //Console.WriteLine(mm);
                    }

                    content = hh + mm + kk;
                }
                if (deep == "0")
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    string d = "/";
                    if (url.Length > 0)
                    {
                        d = "/" + url + "/";
                    }
                    string mm = ResponseInfoFolder(d, di.LastWriteTime, di.CreationTime, di.Name);
                    content = hh + mm + kk;
                }


            }
            else
            {
                if (File.Exists(path))
                {
                    FileInfo f = new FileInfo(path);
                    string d = "/";
                    if (url.Length > 0)
                    {
                        d = "/" + url + "/";
                    }
                    string mm = ResponseInfoFile(d + f.Name, f.LastWriteTime, (int)f.Length, f.CreationTime, f.Name) + "\r\n";
                    
                    content = hh + mm + kk;

                }
                else
                {
                    FileInfo f = new FileInfo(path);
                    string d = "/";
                    if (url.Length > 0)
                    {
                        d = "/" + url + "/";
                    }
                    if (path.EndsWith(".ini"))
                    {
                        string mm = ResponseInfoFile404(d + f.Name) + "\r\n";

                        content = hh + mm + kk;
                        header = header.Replace("207 Multi-Status", "404 File Found");
                    }
                    else
                    {
                        string mm = ResponseInfoFile405(d + f.Name) + "\r\n";
                        content = hh + mm + kk;
                        header = header.Replace("207 Multi-Status", "405 Method Not Allowed");
                    }
                }
            }


            //Console.WriteLine(System.DateTime.Now.ToString("R"));
            //Console.WriteLine(content);
            
            byte[] bodybytes = Encoding.UTF8.GetBytes(content);
            byte[] headbytes = Encoding.ASCII.GetBytes(header.Replace("{}", bodybytes.Length.ToString()));
            return F.AddBytes(headbytes, bodybytes);
        }

        static string ResponseInfoFolder(string url, DateTime last, DateTime create, string name)
        {
            return "<d:response>"
            + "<d:href>/" + url + "</d:href>"
            + "<d:propstat><d:status>HTTP/1.1 200 OK</d:status><d:prop><d:resourcetype><d:collection/></d:resourcetype>"
            + "<d:getlastmodified>" + last.ToString("R") + "</d:getlastmodified>"
            + "<d:getcontentlength>0</d:getcontentlength>"
            + "<d:displayname>" + name + "</d:displayname>"
            + "<d:creationdate>" + create.ToString("R") + "</d:creationdate>"
            + "</d:prop></d:propstat></d:response>";

        }
        static string ResponseInfoFile(string url, DateTime last, int length, DateTime create, string name)
        {
            return "<d:response>"
            + "<d:href>" + url + "</d:href>"
            + "<d:propstat><d:status>HTTP/1.1 200 OK</d:status><d:prop><d:resourcetype/>"
            + "<d:getlastmodified>" + last.ToString("R") + "</d:getlastmodified>"
            + "<d:getcontentlength>" + length + "</d:getcontentlength>"
            + "<d:getcontenttype>" + GetContentType(name) + "</d:getcontenttype>"
            + "<d:displayname>" + name + "</d:displayname>"
            + "<d:creationdate>" + create.ToString("R") + "</d:creationdate>"
            + "</d:prop></d:propstat></d:response>";

        }

        static string ResponseInfoFile404(string url)
        {
            return "<d:response>"
            + "<d:href>" + url + "</d:href>"
            + "<d:propstat><d:status>HTTP/1.1 404 File Found</d:status></d:propstat></d:response>";

        }

        static string ResponseInfoFile405(string url)
        {
            return "<d:response>"
            + "<d:href>" + url + "</d:href>"
            + "<d:propstat><d:status>HTTP/1.1 405 Method Not Allowed</d:status></d:propstat></d:response>";

        }

        #endregion
    }
}
