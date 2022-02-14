using System;
using System.Collections.Generic;
using System.Text;

namespace Tlang
{
    /// <summary>
    /// Template Engine
    /// </summary>
    public static class T
    {


        static System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");

        static string DateZ = "yyyy-MM-ddTH:mm:ss.fffZ";

        static string[] _Input2 = new string[] { "div", "span", "label", "button", "ul", "li", "option" };

        public static bool Contains(string[] list, string value)
        {
            foreach (string s in list)
            {
                if (s == value)
                {
                    return true;
                }
            }
            return false;
        }

        public static string JSONDate(DateTime date)
        {
            return date.ToString(DateZ, ci);
        }

        public static string GMT(DateTime date)
        {
            return date.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'", ci);
        }
        /// <summary>
        /// Use for Node JSON
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string NameValue(string name, object value)
        {
            if (value is string)
            {
                return "\"" + EncodeString(name) + "\" : \"" + EncodeString((string)value) + "\"";
            }
            else if (value is decimal || value is float)
            {
                return "\"" + EncodeString(name) + "\" : " + value.ToString().Replace(',', '.');
            }
            else if (value is DateTime)
            {
                DateTime dt = (DateTime)value;
                return "\"" + EncodeString(name) + "\" : \"" + JSONDate(dt) + "\"";
            }
            else
            {
                return "\"" + EncodeString(name) + "\" : " + value;
            }
        }

        public static string NameEqualValue(string name, object value)
        {
            if (value is string)
            {
                return "\"" + EncodeString(name) + "\" = \"" + EncodeString((string)value) + "\"";
            }
            else if (value is decimal || value is float)
            {
                return "\"" + EncodeString(name) + "\" = " + value.ToString().Replace(',', '.');
            }
            else if (value is DateTime)
            {
                DateTime dt = (DateTime)value;
                return "\"" + EncodeString(name) + "\" = \"" + JSONDate(dt) + "\"";
            }
            else
            {
                return "\"" + EncodeString(name) + "\" = " + value;
            }
        }

        public static string EncodeString(string content)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < content.Length; i++)
            {

                switch (content[i])
                {
                    case '\\':
                        sb.Append("\\\\");
                        continue;
                    case '\r':
                        sb.Append("\\r");
                        continue;
                    case '\n':
                        sb.Append("\\n");
                        continue;
                    case '"':
                        sb.Append("\\\"");
                        continue;
                }
                if (content[i] > 128 || content[i] < 14)
                {
                    sb.Append("\\u" + ((int)content[i]).ToString("X").PadLeft(4, '0'));
                }
                else
                {
                    sb.Append(content[i]);
                }


            }
            return sb.ToString();
        }

        public static string DecodeString(string content)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '\\')
                {
                    if (i < content.Length - 1)
                    {
                        switch (content[i + 1])
                        {
                            case 'u':
                                if (i < content.Length - 4)
                                {
                                    string hex = content.Substring(i + 2, 4);
                                    sb.Append((char)int.Parse(hex, System.Globalization.NumberStyles.HexNumber));
                                    i += 5;
                                }
                                break;
                            case 'r':
                                sb.Append('\r');
                                i++;
                                break;
                            case 'n':
                                sb.Append('\n');
                                i++;
                                break;
                            case '\\':
                                sb.Append('\\');
                                i++;
                                break;
                            case '"':
                                //Console.WriteLine(content);
                                sb.Append('"');
                                i++;
                                break;
                        }
                    }

                }
                else
                {
                    sb.Append(content[i]);
                }
            }
            return sb.ToString();
        }


        public static string Input(string typeClassId, object value)
        {
            string s = "";
            StringBuilder sb = new StringBuilder();
            sb.Append("<input");
            for (int i = 0; i < typeClassId.Length; i++)
            {
                char c = typeClassId[i];
                switch (c)
                {
                    case '#':
                    case '.':
                        if (s.Length > 0)
                        {
                            if (s[0] == '.')
                            {
                                sb.Append(" class=\"" + s.Substring(1) + "\"");
                            }
                            else if (s[0] == '#')
                            {
                                sb.Append(" id=\"" + s.Substring(1) + "\"");
                            }
                            else
                            {
                                if (Contains(_Input2, s))
                                {
                                    return Input2(s, typeClassId, value);
                                }
                                sb.Append(" type=\"" + s + "\"");
                            }
                        }
                        s = c.ToString();
                        break;
                    default:
                        s += c.ToString();
                        break;
                }
            }
            if (s.Length > 0)
            {
                if (s[0] == '.')
                {
                    sb.Append(" class=\"" + s.Substring(1) + "\"");
                }
                else if (s[0] == '#')
                {
                    sb.Append(" id=\"" + s.Substring(1) + "\"");
                }
                else
                {
                    sb.Append(" type=\"" + s + "\"");
                }
            }
            if (value != null)
            {
                sb.Append(" value=\"" + value.ToString() + "\"");
            }
            return sb.ToString() + ">";
        }

        public static string Input2(string tag, string ClassId, object value)
        {
            string s = "";
            StringBuilder sb = new StringBuilder();
            sb.Append("<" + tag);
            for (int i = 0; i < ClassId.Length; i++)
            {
                char c = ClassId[i];
                switch (c)
                {
                    case '#':
                    case '.':
                        if (s.Length > 0)
                        {
                            if (s[0] == '.')
                            {
                                sb.Append(" class=\"" + s.Substring(1) + "\"");
                            }
                            else if (s[0] == '#')
                            {
                                sb.Append(" id=\"" + s.Substring(1) + "\"");
                            }
                            else
                            {
                                //sb.Append(" type=\"" + s + "\"");
                            }
                        }
                        s = c.ToString();
                        break;
                    default:
                        s += c.ToString();
                        break;
                }
            }
            if (s.Length > 0)
            {
                if (s[0] == '.')
                {
                    sb.Append(" class=\"" + s.Substring(1) + "\"");
                }
                else if (s[0] == '#')
                {
                    sb.Append(" id=\"" + s.Substring(1) + "\"");
                }
                else
                {
                    //sb.Append(" type=\"" + s + "\"");
                }
            }
            sb.Append(">");
            if (value != null)
            {
                sb.Append(value.ToString());
            }
            return sb.ToString() + "</" + tag + ">";
        }

        public static List<string> BreakK(string content, string partern)
        {
            string s = "";
            List<string> li = new List<string>();
            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                if (partern.IndexOf(c) >= 0)
                {
                    if (s.Length > 0)
                    {
                        li.Add(s);
                    }
                    s = c.ToString();
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
