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
    public class RouteRule
    {
        string _Rule = "";
        public string Rule
        {
            get
            {
                return _Rule;
            }
        }
        public string Method = "";
        public string StartWith = "";
        public string EndWidth = "";
        public string Text = "";
        public bool IsAll = false;
        public List<string> Parts;
        public WebHandle Handle;

        public void Set(string rule)
        {
            _Rule = rule;
            if (rule == "*")
            {
                IsAll = true;
            }
            else if (rule.IndexOf('{')>=0)
            {
                Parts = BreakeRoute(rule);
                StartWith = Parts[0];
            }
            else if (rule.EndsWith("*"))
            {
                StartWith = rule.Substring(0, rule.Length - 1);
            }
            else if (rule.StartsWith("*"))
            {
                EndWidth = rule.Substring(1);
            }
            else
            {
                Text = rule;
            }
        }

        public bool Check(string url)
        {
            if (IsAll) return true;
            if (StartWith.Length > 0)
            {
                return url.StartsWith(StartWith);
            }
            if (EndWidth.Length > 0)
            {
                return url.EndsWith(EndWidth);
            }
            return Text == url;
        }

        public void Parser(string url,Node node)
        {
            int k = 0;
            for (int i = 0; i < Parts.Count; i++)
            {
                string p = Parts[i];
                if (p.StartsWith("{"))
                {
                    string n = p.Substring(1, p.Length - 2);
                    if (i + 1 < Parts.Count)
                    {
                        int h = url.IndexOf(Parts[i + 1], k, StringComparison.InvariantCulture);
                        if (h > k)
                        {
                            string v = url.Substring(k, h - k);
                            node.Set(n, v);
                            k = h;
                        }
                        else
                        {
                            string v = url.Substring(k);
                            node.Set(n, v);
                        }
                    }
                    else
                    {
                        string v = url.Substring(k);
                        node.Set(n, v);
                    }
                }
                else
                {
                    k += p.Length;
                }
            }
        }

        static List<string> BreakeRoute(string content)
        {
            List<string> li = new List<string>();
            string s = "";
            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                if (c == '{')
                {
                    if (s.Length > 0)
                    {
                        li.Add(s);

                        s = "";
                    }
                    int j = content.IndexOf('}', i + 1);
                    if (j > i)
                    {
                        li.Add(content.Substring(i, j - i + 1));
                        i = j;
                    }
                }
                else
                {
                    s += c.ToString();
                }
            }
            if (s.Length > 0)
            {
                li.Add(s);
            }
            return li;
        }
    }
}
