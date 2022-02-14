/*
    MIT license
    Ping tatadoan@outlook.com for Info | Bug | More Feature
    I'd be happy to have a response
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Tlang.JSON;

namespace Tlang
{
    public static class IO
    {
        static Dictionary<string, FileStream> Pools = new Dictionary<string, FileStream>();

        static Dictionary<string, JSON.DB> DBs = new Dictionary<string, JSON.DB>();


        public static FileStream GetFile(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            fileName = fi.FullName;
            FileStream fs = Pools.ContainsKey(fileName) ? Pools[fileName] : null;
            if (fs == null || !fs.CanRead)
            {
                fs = new FileStream(fileName, FileMode.Open);
                if (Pools.ContainsKey(fileName))
                {
                    Pools.Remove(fileName);
                }
                Pools.Add(fileName, fs);
            }
            //if (fi.Length != fs.Length)// mo lai
            //{
            //    fs.Close();
            //    fs = new FileStream(fileName, FileMode.Open);
            //    if (Pools.ContainsKey(fileName))
            //    {
            //        Pools.Remove(fileName);
            //    }
            //    Pools.Add(fileName, fs);
            //}
            return fs;
        }

        public static FileStream GetFile(string fileName, FileMode mode)
        {
            FileInfo fi = new FileInfo(fileName);
            fileName = fi.FullName;
            FileStream fs = Pools.ContainsKey(fileName) ? Pools[fileName] : null;
            if (fs == null)
            {

                fs = new FileStream(fileName, mode, FileAccess.ReadWrite, FileShare.ReadWrite);
                if (Pools.ContainsKey(fileName))
                {
                    Pools.Remove(fileName);
                }
                Pools.Add(fileName, fs);
                return fs;
            }
            else
            {
                if (!fs.CanWrite)
                {
                    fs.Close();
                    fs = new FileStream(fileName, mode, FileAccess.ReadWrite, FileShare.ReadWrite); 
                    if (Pools.ContainsKey(fileName))
                    {
                        Pools.Remove(fileName);
                    }
                    Pools.Add(fileName, fs);
                }


            }
            return fs;
        }

        public static DB GetDB(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            fileName = fi.FullName;
            if (DBs.ContainsKey(fileName))
            {
                return DBs[fileName];
            }
            DB db = new DB(fileName);
            DBs.Add(fileName, db);
            return db;
        }

        public static void CloseAll()
        {
            foreach (KeyValuePair<string, FileStream> io in Pools)
            {
                if (io.Value != null)
                {
                    io.Value.Close();
                }
            }
            Pools.Clear();
        }


        

        public static string GetFileName(string[] headers)
        {
            if (headers != null)
            {
                foreach (string h in headers)
                {
                    if (h.IndexOf(" filename=") > 0)
                    {
                        string n = h.Substring(h.IndexOf(" filename=") + 10).Trim();
                        if (n.StartsWith("\"") && n.EndsWith("\""))
                            n = n.Substring(1, n.Length - 2);
                        if (n.IndexOf('&') >= 0)
                        {
                            n = HtmlDecode(n).Replace('?', ' ');
                        }
                        return n;
                    }
                }
            }
            return "";
        }

        public static string HtmlDecode(string content)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                switch (c)
                {
                    case '&':
                        if (i + 2 < content.Length)
                        {
                            if (content[i + 1] == '#')
                            {
                                int j = content.IndexOf(';', i);
                                if (j > 0)
                                {
                                    string num = content.Substring(i + 2, j - i - 2);
                                    sb.Append((char)int.Parse(num));
                                    i = j;
                                }
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        public static string HtmlEncode(string content)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                if (c > 128 || c < 14)
                {
                    sb.Append("&#" + (int)content[i] + ";");
                }
                else
                {
                    sb.Append(content[i]);
                }
            }
            return sb.ToString();
        }

        
    }
}
