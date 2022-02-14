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
    public class Node : Dictionary<string, object>
    {
        public Node(): base(StringComparer.InvariantCultureIgnoreCase)
        {

        }

        public void Set(string name, object value)
        {
            if (ContainsKey(name))
            {
                this[name] = value;
            }
            else
            {
                Add(name, value);
            }
        }


        public object Get(string name)
        {
            if (ContainsKey(name))
            {
                return this[name];
            }
            return null;
        }

        public string GetString(string name)
        {
            if (ContainsKey(name))
            {
                return this[name].ToString();
            }
            return "";
        }

        public int GetInt(string name)
        {
            if (ContainsKey(name))
            {
                object value = this[name];
                if (value != null)
                {
                    if (value is int)
                    {
                        return (int)value;
                    }
                    int n = 0;
                    int.TryParse(value.ToString(), out n);
                    return n;
                }
            }
            return 0;
        }

        public long GetInt64(string name)
        {
            if (ContainsKey(name))
            {
                object value = this[name];
                if (value != null)
                {
                    if (value is long)
                    {
                        return (long)value;
                    }
                    long n = 0;
                    long.TryParse(value.ToString(), out n);
                    return n;

                }
            }
            return 0;
        }

        public decimal GetDecimal(string name)
        {
            if (ContainsKey(name))
            {
                object value = this[name];
                if (value != null)
                {
                    if (value is decimal)
                    {
                        return (decimal)value;
                    }
                    decimal n = 0;
                    decimal.TryParse(value.ToString(), out n);
                    return n;
                }
            }
            return 0;
        }

        public DateTime GetDateTime(string name)
        {
            if (ContainsKey(name))
            {
                object value = this[name];
                if (value != null)
                {
                    if (value is DateTime)
                    {
                        return (DateTime)value;
                    }
                }
            }
            return DateTime.MinValue;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            foreach (KeyValuePair<string, object> o in this)
            {
                sb.Append(T.NameValue(o.Key, o.Value) + " , ");
            }
            return sb.ToString().TrimEnd(',', ' ') + "}";
        }
    }
}
