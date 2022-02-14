/*
    MIT license
    Ping tatadoan@outlook.com for Info | Bug | More Feature
    I'd be happy to have a response
 */
using System;
using System.Collections.Generic;
using System.Text;
using Tlang.JSON;

namespace Tlang.Web
{
    public class Request
    {
        public Node Head;
        public Node Get;
        public Node Post;
        public Node Cookies;
        Connection _Con;

        public string Method = "";
        public string Path = "";
        public string ContentType = "";
        public string Charset = "";
        public string UserAgent = "";
        public long ContentLength = 0;
        public string Accept = "";
        public string UserName = "";
        public string FileName = "";
        public string[] Headers;
        public byte[] Data;

        public Request(Connection con)
        {
            _Con = con;
        }

        public string Text()
        {
            if (Data == null || Data.Length == 0) return "";
            if (Charset == "")
            {
                return Encoding.ASCII.GetString(Data);
            }
            return Encoding.GetEncoding(Charset).GetString(Data);
        }

        public int Status = 0;

        public bool HasRange()
        {
            foreach (string h in Headers)
            {
                if (h.StartsWith("Range"))
                {
                    Console.WriteLine(h);
                    int j = h.IndexOf('=');
                    if (j > 0)
                    {
                        string[] u = h.Substring(j + 1).Trim().Split('-');
                        if (u.Length == 2)
                        {
                            long s = 0;
                            long.TryParse(u[0], out s);
                            long l = -1;
                            if (u[1].Trim().Length > 0)
                            {
                                long.TryParse(u[1], out l);
                            }
                            RangeStart = s;
                            RangeLength = l;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public long RangeStart = 0;
        public long RangeLength = 0;

        public void UpdateGet(string content)
        {
            Get = ParserParam(content);
        }

        public void UpdatePost(string content)
        {
            Post = ParserParam(content);
        }

        public void UpdateCookies()
        {
            Cookies = ParserCookies(GetCookie(Headers));
        }

        public void UpdateHeader()
        {
            Head = new Node();
            int j = 0;
            foreach (string h in Headers)
            {
                j = h.IndexOf(':');
                if (j > 0)
                {
                    Head.Set(h.Substring(0, j), h.Substring(j + 1).Trim());
                }
            }
            if (Head.ContainsKey(G.UserAgent))
            {
                UserAgent = Head[G.UserAgent].ToString();
            }

            if (Head.ContainsKey(G.Accept))
            {
                Accept = Head[G.Accept].ToString();
            }
            if (Head.ContainsKey(G.ContentType))
            {
                ContentType = Head[G.ContentType].ToString();
                j = ContentType.IndexOf("charset=");
                if (j > 0)
                {
                    Charset = ContentType.Substring(j + 8).Trim();
                }
            }
        }

        private string GetCookie(string[] headers)
        {
            if (headers != null)
            {
                foreach (string h in headers)
                {
                    if (h.IndexOf("Cookie") >= 0)
                    {
                        return h.Substring(h.IndexOf(':') + 1).Trim();
                    }
                }
            }
            return "";
        }

        private Node ParserParam(string content)
        {
            string[] ps = content.Split('&');
            Node dic = new Node();
            foreach (string p in ps)
            {
                int j = p.IndexOf('=');
                if (j >= 0)
                {
                    string name = p.Substring(0, j);
                    string value = p.Substring(j + 1);
                    try
                    {
                        if (dic.ContainsKey(name))
                        {
                            dic.Remove(name);
                        }
                        dic.Add(name, System.Uri.UnescapeDataString(value.Replace('+', ' ')));
                    }
                    catch { }
                }
            }
            return dic;
        }

        private Node ParserCookies(string content)
        {
            string[] ps = content.Split(';');
            Node dic = new Node();
            foreach (string p in ps)
            {
                int j = p.IndexOf('=');
                if (j >= 0)
                {
                    string name = p.Substring(0, j).Trim();
                    string value = p.Substring(j + 1).Trim();
                    dic.Add(name, System.Uri.UnescapeDataString(value.Replace('+', ' ')));
                }
            }
            return dic;
        }
    }
}
