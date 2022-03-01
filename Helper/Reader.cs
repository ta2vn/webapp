/*
    MIT license
    Ping tatadoan@outlook.com for Info | Bug | More Feature
    I'd be happy to have a response
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Tlang.JSON;

namespace Tlang
{
    public static class Reader
    {
        static NumberStyles NumberStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign;
        //static string RegDate = @"^\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d\.\d{3}Z$";
        /// <summary>
        /// Tim vi tri close cua String, khi gap \ skip 1 ky tu
        /// </summary>
        /// <param name="content"></param>
        /// <param name="index">vi tri sau vi tri bat dau String</param>
        /// <param name="c">la ky tu bat dau String ' hay "</param>
        /// <returns></returns>
        static int FindEndStringIndex(string content, int index, char c)
        {
            while (index < content.Length)
            {
                if (content[index] != c)
                {
                    if (content[index] == '\\')
                    {
                        index++;
                    }
                    index++;
                }
                else
                {
                    return index;
                }
            }
            return -1;
        }

        /// <summary>
        /// Tim vi tri close cua [], co check []{}() va String
        /// </summary>
        /// <param name="content"></param>
        /// <param name="index">vi tri sau vi tri bat dau cua []</param>
        /// <returns></returns>
        public static int FindEndBracketIndex(string content, int index)
        {
            int level = 0;
            for (; index < content.Length; index++)
            {
                char cc = content[index];
                switch (cc)
                {
                    case '[':
                        level++;
                        break;
                    case ']':
                        if (level == 0) return index;
                        level--;
                        break;
                    case '\"':
                    case '\'':
                        int end = FindEndStringIndex(content, index + 1, cc);
                        if (end == -1) return -1;
                        index = end;
                        break;
                    case '{':
                        int end1 = FindEndBraceIndex(content, index + 1);
                        if (end1 == -1) return -1;
                        index = end1;
                        break;
                    case '(':
                        int end2 = FindEndParenthesisIndex(content, index + 1);
                        if (end2 == -1) return -1;
                        index = end2;
                        break;
                }
            }
            return -1;
        }

        /// <summary>
        /// Tim vi tri close cua {}, co check []{}() va String
        /// </summary>
        /// <param name="content"></param>
        /// <param name="index">vi tri sau vi tri bat dau cua {}</param>
        /// <returns></returns>
        public static int FindEndBraceIndex(string content, int index)
        {
            int level = 0;
            for (; index < content.Length; index++)
            {
                char cc = content[index];
                switch (cc)
                {
                    case '{':
                        level++;
                        break;
                    case '}':
                        if (level == 0) return index;
                        level--;
                        break;
                    case '\"':
                    case '\'':
                        int end = FindEndStringIndex(content, index + 1, cc);
                        if (end == -1) return -1;
                        index = end;
                        break;
                    case '[':
                        int end1 = FindEndBracketIndex(content, index + 1);
                        if (end1 == -1) return -1;
                        index = end1;
                        break;
                    case '(':
                        int end2 = FindEndParenthesisIndex(content, index + 1);
                        if (end2 == -1) return -1;
                        index = end2;
                        break;
                }
            }
            return -1;
        }

        /// <summary>
        /// Tim vi tri close cua (), co check []{}() va String
        /// </summary>
        /// <param name="content"></param>
        /// <param name="index">vi tri sau vi tri bat dau cua ()</param>
        /// <returns></returns>
        public static int FindEndParenthesisIndex(string content, int index)
        {
            int level = 0;
            for (; index < content.Length; index++)
            {
                char cc = content[index];
                switch (cc)
                {
                    case '(':
                        level++;
                        break;
                    case ')':
                        if (level == 0) return index;
                        level--;
                        break;
                    case '\"':
                    case '\'':
                        int end = FindEndStringIndex(content, index + 1, cc);
                        if (end == -1) return -1;
                        index = end;
                        break;
                    case '[':
                        int end1 = FindEndBracketIndex(content, index + 1);
                        if (end1 == -1) return -1;
                        index = end1;
                        break;
                    case '{':
                        int end2 = FindEndBraceIndex(content, index + 1);
                        if (end2 == -1) return -1;
                        index = end2;
                        break;
                }
            }
            return -1;
        }

        static int ReadSpace(string content, int index)
        {
            while (index < content.Length)
            {
                char c = content[index];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                {
                    index++;
                }
                else
                {
                    return index;
                }
            }
            return index;
        }

        static int ReadValue(string content, int index)
        {
            while (index < content.Length)
            {
                char c = content[index];
                if (c != ',' && c != '}')
                {
                    index++;
                }
                else
                {
                    return index;
                }
            }
            return index;
        }

        static int ReadName(string content, int index)
        {
            while (index < content.Length)
            {
                char c = content[index];
                if (c != ',' && c != '}' && c != ':')
                {
                    index++;
                }
                else
                {
                    return index;
                }
            }
            return index;
        }

        static int ReadItem(string content, int index)
        {
            while (index < content.Length)
            {
                char c = content[index];
                if (c != ',' && c != ']')
                {
                    index++;
                }
                else
                {
                    return index;
                }
            }
            return index;
        }

        public static object Parser(string content)
        {
            try
            {
                int i = ReadSpace(content, 0);
                char c = content[i];
                //if (c == '{')
                //{
                //    Node node = new Node();
                //    int j = ReadNode(node, content, i + 1);
                //    if (j == i) return "error";
                //    return node;
                //}
                //else if (c == '[')
                //{
                //    NodeArray list = new NodeArray();
                //    int j = ReadArray(list, content, i + 1);
                //    if (j == i) return "error";
                //    return list;
                //}

                if (c == '{')
                {
                    Node node = new Node();
                    int j = ReadNodeJ(node, content, i + 1);
                    if (j == i) return "error";
                    return node;
                }
                else if (c == '[')
                {
                    NodeArray list = new NodeArray();
                    int j = ReadArrayJ(list, content, i + 1);
                    if (j == i) return "error";
                    return list;
                }
                else if (c == '"' || c == '\'')
                {
                    int j = FindEndStringIndex(content, i + 1, c);
                    if (j > i)
                    {
                        string txt = content.Substring(i + 1, j - i - 1);
                        return DecodeString(txt);
                    }
                    return "";
                }
                else
                {
                    return Cast(content.Trim());
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "error";
        }

        public static int ReadNode(Node node, string content, int index)
        {
            int i = ReadSpace(content, index);
            char c = content[i];
            
            while (true)
            {
                if (c == '\"')
                {
                    int j = FindEndStringIndex(content, i + 1, c);
                    if (j > 0)
                    {
                        string name = DecodeString(content.Substring(i + 1, j - 1 - i));
                        i = ReadSpace(content, j + 1);
                        c = content[i];
                        if (c == ':')
                        {
                            i++;
                            i = ReadSpace(content, i);
                            c = content[i];
                            if (c == '{')
                            {
                                Node node1 = new Node();
                                node.Set(name, node1);
                                j = ReadNode(node1, content, i + 1);
                                if (j > i)
                                {
                                    i = j + 1;
                                }
                                else
                                {
                                    throw new Exception("Node error : " + i + " - not end");
                                }
                            }
                            else if (c == '[')
                            {
                                NodeArray list1 = new NodeArray();
                                node.Set(name, list1);
                                j = ReadArray(list1, content, i + 1);
                                if (j > i)
                                {
                                    i = j + 1;
                                }
                                else
                                {
                                    throw new Exception("Node array error : " + i + " - not end");
                                }
                            }
                            else if (c == '"')
                            {
                                j = FindEndStringIndex(content, i + 1, c);
                                if (j > i)
                                {
                                    string value = DecodeString(content.Substring(i + 1, j - 1 - i));
                                    node.Set(name, value);
                                    i = j + 1;
                                }
                                else
                                {
                                    throw new Exception("String error : " + i + " - not end");
                                }
                            }
                            else
                            {
                                j = ReadValue(content, i);
                                string value = content.Substring(i, j - i).Trim();
                                if (value.Length > 0)
                                {
                                    node.Set(name, Cast(value));
                                }
                                i = j;
                            }


                            i = ReadSpace(content, i);

                            // neu khong dong } thi bi loi o day
                            if (i >= content.Length)
                            {
                                throw new Exception("Node error : " + (index - 1) + " - not end");
                            }
                            c = content[i];
                            if (c == '}')
                            {
                                return i;
                            }
                            else
                            {
                                if (c == ',')
                                {
                                    i++;
                                    i = ReadSpace(content, i);
                                    c = content[i];
                                }
                                else
                                {
                                    throw new Exception("At " + i + " not ,");
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("At " + i + " not :");
                        }
                    }
                    else
                    {
                        return index;
                    }
                }
                else if (c == '}')
                {
                    return i;
                }
                else
                {
                    throw new Exception("At " + i + " not \"");
                }
            }
            return index;
        }
        /// <summary>
        /// list include {},[],"",number,bool
        /// </summary>
        /// <param name="list"></param>
        /// <param name="content"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int ReadArray(NodeArray arr, string content, int index)//index -1 is [
        {
            int i = ReadSpace(content, index);
            char c = content[i];
            while (true)
            {
                if (c == '{')
                {
                    Node node = new Node();
                    arr.Add(node);
                    int j = ReadNode(node, content, i + 1);
                    if (j > i)
                    {
                        i = j + 1;
                    }
                    else
                    {
                        throw new Exception("Node error : " + i + " - not end");
                    }
                }
                else if (c == '[')
                {
                    NodeArray arr1 = new NodeArray();
                    arr.Add(arr1);
                    int j = ReadArray(arr1, content, i + 1);
                    if (j > i)
                    {
                        i = j + 1;
                    }
                    else
                    {
                        throw new Exception("Node array error : " + i + " - not end");
                    }
                }
                else if (c == '"')
                {
                    int j = FindEndStringIndex(content, i + 1, c);
                    if (j > i)
                    {
                        string value = DecodeString( content.Substring(i + 1, j - 1 - i));
                        arr.Add(value);
                        i = j + 1;
                    }
                    else
                    {
                        throw new Exception("String error : " + i + " - not end");
                    }
                }
                else
                {
                    int j = ReadItem(content, i);
                    string value = content.Substring(i, j - i).Trim();
                    if (value.Length > 0)
                    {
                        arr.Add(Cast(value));
                    }
                    i = j;
                }

                i = ReadSpace(content, i);

                // neu khong dong ] thi bi loi o day
                if (i >= content.Length)
                {
                    throw new Exception("Node array error : " + (index - 1) + " - not end");
                }
                c = content[i];
                if (c == ']')
                {
                    return i;
                }
                else
                {
                    if (c == ',')
                    {
                        i++;
                        i = ReadSpace(content, i);
                        c = content[i];
                    }
                    else
                    {
                        throw new Exception("At " + i + " not ,");
                    }
                }
            }
            return index;
        }


        public static int ReadNodeJ(Node node, string content, int index)
        {
            int i = ReadSpace(content, index);
            char c = content[i];

            while (true)
            {
                if (c == '\"' || c == '\'')
                {
                    int j = FindEndStringIndex(content, i + 1, c);
                    if (j > 0)
                    {
                        string name = DecodeString(content.Substring(i + 1, j - 1 - i));
                        i = ReadSpace(content, j + 1);
                        c = content[i];
                        if (c == ':')
                        {
                            i++;
                            i = ReadSpace(content, i);
                            c = content[i];
                            if (c == '{')
                            {
                                Node node1 = new Node();
                                node.Set(name, node1);
                                j = ReadNodeJ(node1, content, i + 1);
                                if (j > i)
                                {
                                    i = j + 1;
                                }
                                else
                                {
                                    throw new Exception("Node error : " + i + " - not end");
                                }
                            }
                            else if (c == '[')
                            {
                                NodeArray list1 = new NodeArray();
                                node.Set(name, list1);
                                j = ReadArrayJ(list1, content, i + 1);
                                if (j > i)
                                {
                                    i = j + 1;
                                }
                                else
                                {
                                    throw new Exception("Node array error : " + i + " - not end");
                                }
                            }
                            else if (c == '"' || c == '\'')
                            {
                                j = FindEndStringIndex(content, i + 1, c);
                                if (j > i)
                                {
                                    string value = DecodeString(content.Substring(i + 1, j - 1 - i));
                                    node.Set(name, value);
                                    i = j + 1;
                                }
                                else
                                {
                                    throw new Exception("String error : " + i + " - not end");
                                }
                            }
                            else
                            {
                                j = ReadValue(content, i);
                                string value = content.Substring(i, j - i).Trim();
                                if (value.Length > 0)
                                {
                                    node.Set(name, Cast(value));
                                }
                                i = j;
                            }


                            i = ReadSpace(content, i);

                            // neu khong dong } thi bi loi o day
                            if (i >= content.Length)
                            {
                                throw new Exception("Node error : " + (index - 1) + " - not end");
                            }
                            c = content[i];
                            if (c == '}')
                            {
                                return i;
                            }
                            else
                            {
                                if (c == ',')
                                {
                                    i++;
                                    i = ReadSpace(content, i);
                                    c = content[i];
                                }
                                else
                                {
                                    throw new Exception("At " + i + " not ,");
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("At " + i + " not :");
                        }
                    }
                    else
                    {
                        return index;
                    }
                }
                else if (c == '}')
                {
                    return i;
                }
                else
                {
                     int j = ReadName(content, i);
                     if (j > i)
                     {
                         string name = content.Substring(i, j  - i);
                         i = ReadSpace(content, j);
                         c = content[i];
                         if (c == ':')
                         {
                             i++;
                             i = ReadSpace(content, i);
                             c = content[i];
                             if (c == '{')
                             {
                                 Node node1 = new Node();
                                 node.Set(name, node1);
                                 j = ReadNodeJ(node1, content, i + 1);
                                 if (j > i)
                                 {
                                     i = j + 1;
                                 }
                                 else
                                 {
                                     throw new Exception("Node error : " + i + " - not end");
                                 }
                             }
                             else if (c == '[')
                             {
                                 NodeArray list1 = new NodeArray();
                                 node.Set(name, list1);
                                 j = ReadArrayJ(list1, content, i + 1);
                                 if (j > i)
                                 {
                                     i = j + 1;
                                 }
                                 else
                                 {
                                     throw new Exception("Node array error : " + i + " - not end");
                                 }
                             }
                             else if (c == '"' || c == '\'')
                             {
                                 j = FindEndStringIndex(content, i + 1, c);
                                 if (j > i)
                                 {
                                     string value = DecodeString(content.Substring(i + 1, j - 1 - i));
                                     node.Set(name, value);
                                     i = j + 1;
                                 }
                                 else
                                 {
                                     throw new Exception("String error : " + i + " - not end");
                                 }
                             }
                             else
                             {
                                 j = ReadValue(content, i);
                                 string value = content.Substring(i, j - i).Trim();
                                 if (value.Length > 0)
                                 {
                                     node.Set(name, Cast(value));
                                 }
                                 i = j;
                             }


                             i = ReadSpace(content, i);

                             // neu khong dong } thi bi loi o day
                             if (i >= content.Length)
                             {
                                 throw new Exception("Node error : " + (index - 1) + " - not end");
                             }
                             c = content[i];
                             if (c == '}')
                             {
                                 return i;
                             }
                             else
                             {
                                 if (c == ',')
                                 {
                                     i++;
                                     i = ReadSpace(content, i);
                                     c = content[i];
                                 }
                                 else
                                 {
                                     throw new Exception("At " + i + " not ,");
                                 }
                             }
                         }
                         else
                         {
                             throw new Exception("At " + i + " not :");
                         }
                     }
                     else
                     {
                         throw new Exception("At " + i + " not \"");
                     }
                }
            }
            return index;
        }
        /// <summary>
        /// list include {},[],"",number,bool
        /// </summary>
        /// <param name="list"></param>
        /// <param name="content"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int ReadArrayJ(NodeArray arr, string content, int index)//index -1 is [
        {
            int i = ReadSpace(content, index);
            char c = content[i];
            while (true)
            {
                if (c == '{')
                {
                    Node node = new Node();
                    arr.Add(node);
                    int j = ReadNodeJ(node, content, i + 1);
                    if (j > i)
                    {
                        i = j + 1;
                    }
                    else
                    {
                        throw new Exception("Node error : " + i + " - not end");
                    }
                }
                else if (c == '[')
                {
                    NodeArray arr1 = new NodeArray();
                    arr.Add(arr1);
                    int j = ReadArrayJ(arr1, content, i + 1);
                    if (j > i)
                    {
                        i = j + 1;
                    }
                    else
                    {
                        throw new Exception("Node array error : " + i + " - not end");
                    }
                }
                else if (c == '"')
                {
                    int j = FindEndStringIndex(content, i + 1, c);
                    if (j > i)
                    {
                        string value = content.Substring(i + 1, j - 1 - i);
                        arr.Add(DecodeString(value));
                        i = j + 1;
                    }
                    else
                    {
                        throw new Exception("String error : " + i + " - not end");
                    }
                }
                else
                {
                    int j = ReadItem(content, i);
                    string value = content.Substring(i, j - i).Trim();
                    if (value.Length > 0)
                    {
                        arr.Add(Cast(value));
                    }
                    i = j;
                }

                i = ReadSpace(content, i);

                // neu khong dong ] thi bi loi o day
                if (i >= content.Length)
                {
                    throw new Exception("Node array error : " + (index - 1) + " - not end");
                }
                c = content[i];
                if (c == ']')
                {
                    return i;
                }
                else
                {
                    if (c == ',')
                    {
                        i++;
                        i = ReadSpace(content, i);
                        c = content[i];
                    }
                    else
                    {
                        throw new Exception("At " + i + " not ,");
                    }
                }
            }
            return index;
        }

        public static object Cast(string content)
        {
            switch (content)
            {
                case "True":
                case "true":
                    return true;
                case "False":
                case "false":
                    return false;
                case "Null":
                case "null":
                    return null;
            }
            if (content.IndexOf('.') >= 0)
            {
                if (content.Length == 24 && content[23] == 'Z')//may be date
                {
                    string[] p = content.Split('-', ':', 'T', 'Z', '.');
                    if (p.Length == 8)
                    {
                        try
                        {
                            DateTime dt = new DateTime(int.Parse(p[0]), int.Parse(p[1]), int.Parse(p[2]), int.Parse(p[3]), int.Parse(p[4]), int.Parse(p[5]), int.Parse(p[6]));
                            return dt;
                        }
                        catch { }
                    }
                }

                decimal d = 0;
                decimal.TryParse(content,NumberStyle,CultureInfo.InvariantCulture, out d);
                return d;
            }
            else
            {
                long l = 0;
                if (long.TryParse(content, out l))
                {
                    if (l < int.MaxValue && l > int.MinValue)
                    {
                        return (int)l;
                    }
                    return l;
                }
                return 0;
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

        static List<NPath> ToPath(string content)
        {
            List<NPath> ps = new List<NPath>();
            string name = "";
            int j = 0;
            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                switch (c)
                {
                    case '[':
                        j = ReadItem(content, i + 1);
                        if (j > i)
                        {
                            string value = content.Substring(i + 1, j - i - 1);
                            NPath p = new NPath();
                            p.Type = NPathType.Array;
                            if (name.Length > 0)
                            {
                                p.Name = name;
                                name = "";
                            }
                            p.Index = int.Parse(value);
                            ps.Add(p);
                            i = j;
                        }
                        break;
                    case '.':
                        if (name.Length > 0)
                        {
                            NPath p = new NPath();
                            p.Name = name;
                            name = "";
                            ps.Add(p);
                        }
                        break;
                    default:
                        name += c.ToString();
                        break;
                }
            }
            if (name.Length > 0)
            {
                NPath p = new NPath();
                p.Name = name;
                name = "";
                ps.Add(p);
            }
            return ps;
        }

        public static string ToJSON(object obj)
        {
            if (!(obj is Node || obj is NodeArray))
            {
                if (obj is string)
                {
                    string s = (string)obj;
                    if (s.StartsWith("{") && s.EndsWith("}"))
                    {
                        return s;
                    }
                    if (s.StartsWith("[") && s.EndsWith("]"))
                    {
                        return s;
                    }
                    return "{}";
                }
                else if (obj is System.Collections.IEnumerable)
                {
                    System.Collections.IEnumerator em = ((System.Collections.IEnumerable)obj).GetEnumerator();
                    NodeArray ar = new NodeArray();
                    while (em.MoveNext())
                    {
                        ar.Add(em.Current);
                    }
                    obj = ar;
                }
                else
                {

                    PropertyInfo[] pis = obj.GetType().GetProperties();
                    Node n = new Node();
                    foreach (PropertyInfo pi in pis)
                    {
                        if (pi.CanRead)
                        {
                            object v = pi.GetValue(obj, new object[] { });
                            n.Set(pi.Name, v);
                        }
                    }
                    obj = n;

                }
            }

            if (obj is Node || obj is NodeArray)
            {
                return obj.ToString();
            }
            else
            {
                return "{}";
            }
        }

        static object Get1(List<NPath> ps, object obj, int k)
        {
            
            if (ps.Count -k  == 0) return obj;
            for (int i = 0; i < ps.Count - k; i++)
            {
                NPath p = ps[i];
                if (p.Type == NPathType.Name)
                {
                    if (obj is Node)
                    {
                        Node n = (Node)obj;
                        if (n.ContainsKey(p.Name))
                        {
                            obj = n[p.Name];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        obj = GetPropertyValue(obj, p.Name);
                        if (obj == null) return null;
                    }
                }
                else if (p.Type == NPathType.Array)
                {
                    if (obj is Node)
                    {
                        Node n = (Node)obj;
                        if (n.ContainsKey(p.Name))
                        {
                            obj = n[p.Name];
                            if (obj is NodeArray)
                            {
                                NodeArray ar = (NodeArray)obj;
                                if (p.Index >= 0)
                                {
                                    if (p.Index < ar.Count)
                                    {
                                        obj = ar[p.Index];
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                else
                                {
                                    if (p.Index + ar.Count < ar.Count)
                                    {
                                        obj = ar[p.Index + ar.Count];
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (obj is NodeArray)
                    {
                        NodeArray ar = (NodeArray)obj;
                        if (p.Index >= 0)
                        {
                            if (p.Index < ar.Count)
                            {
                                obj = ar[p.Index];
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            if (p.Index + ar.Count < ar.Count)
                            {
                                obj = ar[p.Index + ar.Count];
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    else
                    {
                        obj = GetArrayValue(obj, p.Index);
                        if (obj == null) return null;
                    }
                }
            }
            return obj;
        }

        public static object Get(object obj, string path)
        {
            List<NPath> ps = ToPath(path);
            return Get1(ps, obj, 0);
        }

        public static void Set(object obj, string path, object value)
        {
            List<NPath> ps = ToPath(path);
            object target = Get1(ps, obj, 1);
            NPath ls = ps[ps.Count - 1];
            if (target is Node)
            {
                Node n = (Node)target;
                if (ls.Type == NPathType.Array)
                {
                    if (n.ContainsKey(ls.Name))
                    {
                        target = n[ls.Name];
                    }
                }
                else
                {
                    n.Set(ls.Name, value);
                    return;
                }
            }
            if (target is NodeArray)
            {
                NodeArray ar = (NodeArray)target;
                if (ls.Index >= 0 && ls.Index < ar.Count)
                {
                    ar[ls.Index] = value;
                }
            }
        }

        static object GetPropertyValue(object obj, string name)
        {
            if (obj == null)
            {
                return null;
            }
            if (obj is Type)
            {
                Type ty = (Type)obj;
                PropertyInfo pi = ty.GetProperty(name.Replace("[]", ""));
                if (pi == null)
                {
                    return null;
                }
                try
                {
                    return pi.GetValue(ty, null);
                }
                catch
                {
                    return null;
                }
            }
            PropertyInfo pi1 = obj.GetType().GetProperty(name.Replace("[]", ""));
            if (pi1 == null)
            {
                MethodInfo mi = obj.GetType().GetMethod("GetValue");
                if (mi != null)
                {
                    return mi.Invoke(obj, new object[] { name });
                }
                return null;
            }
            return pi1.GetValue(obj, null);
        }

        static object GetArrayValue(object obj, int index)
        {
            if (obj is System.Collections.IEnumerable)
            {
                System.Collections.IEnumerator em = ((System.Collections.IEnumerable)obj).GetEnumerator();
                if (!em.MoveNext()) return null;
                for (int j = 0; j < index; j++)
                {
                    if (!em.MoveNext()) return null;
                }
                return em.Current;
            }
            return null;
        }

        public static NodeArray FromCSVString(string content,char sep)
        {
            string[] lines = content.Split('\n');
            string[] heads = lines[0].Split(sep);

            for (int i = 0; i < heads.Length; i++)
            {
                heads[i] = heads[i].Trim();
            }

            NodeArray list = new NodeArray();
            for (int r = 1; r < lines.Length; r++)
            {
                Node node = new Node();
                string line = lines[r].TrimEnd('\r');
                string[] values = line.Split(sep);
                int l = Math.Min(values.Length,heads.Length);
                for (int h = 0; h < l; h++)
                {
                    node.Set(heads[h], values[h]);
                }
                list.Add(node);
            }
            return list;
        }


        public static NodeArray FromCSVString(string content)
        {
            string line0 = "";
            int j = content.IndexOf('\n');
            if (j > 0)
            {
                line0 = content.Substring(0, j);
            }
            else
            {
                line0 = content;
            }
            char sep = ';';
            j = line0.IndexOf(',');
            if (j > 0)
            {
                sep = ',';
            }
            else
            {
                j = line0.IndexOf('\t');
                if (j > 0)
                {
                    sep = '\t';
                }
            }
            return FromCSVString(content, sep);
        }

        public static NodeArray FromCSVFile(string fileName)
        {
            string content = IO.OpenText(fileName);
            return FromCSVString(content);
        }

    }
}
