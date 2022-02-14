/*
    MIT license
    Ping tatadoan@outlook.com for Info | Bug | More Feature
    I'd be happy to have a response
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace Tlang.JSON
{
    public class NodeArray : List<object>
    {

        public Dictionary<string, Node> GetDic(string name)
        {
            Dictionary<string, Node> dic = new Dictionary<string, Node>();
            foreach (object o in this)
            {
                if (o is Node)
                {
                    Node n = (Node)o;
                    string key = n.GetString(name);
                    if (key.Length > 0)
                    {
                        if (dic.ContainsKey(key))
                        {
                            dic[key] = n;
                        }
                        else
                        {
                            dic.Add(key, n);
                        }
                    }
                }
            }
            return dic;
        }

        public NodeArray Filter(string name, object value)
        {
            NodeArray list = new NodeArray();
            for (int i = Count - 1; i >= 0; i--)
            {
                object obj = this[i];
                if (obj is Node)
                {
                    Node n = (Node)obj;
                    if (n.ContainsKey(name) && n[name].Equals(value))
                    {
                        list.Add(n);
                    }
                }
            }
            return list;
        }


        public NodeArray Between(string name, object from, object to)
        {
            NodeArray list = new NodeArray();
            if (from is DateTime && to is DateTime)
            {
                DateTime dtfrom = (DateTime)from;
                DateTime dtto = (DateTime)to;
                for (int i = 0; i < Count; i++)
                {
                    object obj = this[i];
                    if (obj is Node)
                    {
                        Node n = (Node)obj;
                        if (n.ContainsKey(name))
                        {
                            DateTime dt = n.GetDateTime(name);
                            if (!dt.Equals(DateTime.MinValue))
                            {
                                if (dt >= dtfrom && dt <= dtto)
                                {
                                    list.Add(n);
                                }
                            }
                        }
                    }
                }
                return list;
            }

            if (from is decimal && to is decimal)
            {
                decimal dtfrom = (decimal)from;
                decimal dtto = (decimal)to;
                for (int i = 0; i < Count; i++)
                {
                    object obj = this[i];
                    if (obj is Node)
                    {
                        Node n = (Node)obj;
                        if (n.ContainsKey(name))
                        {
                            decimal dt = n.GetDecimal(name);
                            if (dt >= dtfrom && dt <= dtto)
                            {
                                list.Add(n);
                            }
                        }
                    }
                }
                return list;
            }

            if (from is int && to is int)
            {
                int dtfrom = (int)from;
                int dtto = (int)to;
                for (int i = 0; i < Count; i++)
                {
                    object obj = this[i];
                    if (obj is Node)
                    {
                        Node n = (Node)obj;
                        if (n.ContainsKey(name))
                        {
                            decimal dt = n.GetInt(name);
                            if (dt >= dtfrom && dt <= dtto)
                            {
                                list.Add(n);
                            }
                        }
                    }
                }
                return list;
            }
            return list;
        }

        /// <summary>
        /// New first from 1 to data.count/pagesize
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public NodeArray Page(int page, int pageSize)
        {
            if (Count == 0) return this;

            NodeArray list = new NodeArray();
            if (page < 1) return list;
            int p = page - 1;
            int start = Count - 1 - p * pageSize;
            int end = Math.Max(-1, start - pageSize);

            for (int i = start; i > end; i--)
            {
                list.Add(this[i]);
            }
            return list;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            foreach (object o in this)
            {
                if (o is string)
                {
                    sb.Append("\"" + Reader.EncodeString(o.ToString()) + "\" , ");
                }
                else if (o is decimal || o is float)
                {
                    sb.Append(o.ToString().Replace(',', '.') + " , ");
                }
                else
                {
                    sb.Append(o.ToString() + " , ");
                }
            }
            return sb.ToString().TrimEnd(' ', ',') + "]";
        }
    }
}
