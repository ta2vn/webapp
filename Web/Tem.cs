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
    public class Tem
    {
        List<Block> Blocks = new List<Block>();

        /// <summary>
        /// Type ${code}
        /// </summary>
        /// <param name="content"></param>
        
        public void Parser(string content)
        {
            Blocks.Clear();
            StringBuilder sb = new StringBuilder();
            int j = 0;
            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                switch (c)
                {
                    case '$':
                        if (i + 2 < content.Length && content[i + 1] == '{')// check start with ${
                        {
                            j = Reader.FindEndBraceIndex(content, i + 2);
                            if (j > i)
                            {
                                if (sb.Length > 0)
                                {
                                    Block b = new Block();
                                    b.Content = sb.ToString();
                                    sb = new StringBuilder();
                                    Blocks.Add(b);
                                }
                                string code = content.Substring(i + 2, j - i - 2).Trim();
                                Block bc = new Block();
                                bc.Type = BlockType.Code;
                                bc.Content = code;
                                Blocks.Add(bc);
                                i = j;
                            }
                            else
                            {
                                return;
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
            if (sb.Length > 0)
            {
                Block b = new Block();
                b.Content = sb.ToString();
                sb = new StringBuilder();
                Blocks.Add(b);
            }
        }

        public string Render(object node)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Blocks.Count; i++)
            {
                Block b = Blocks[i];
                if (b.Type == BlockType.Text)
                {
                    sb.Append(b.Content);
                }
                else
                {
                    sb.Append(Code(node, b));
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// Calcualte code to text
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public string Code(object obj, Block code)
        {
            object so = Reader.Get(obj, code.Content);
            if (so != null)
            {
               return so.ToString();
            }
            return "";
        }

        public string Render(Dictionary<string, Tem> tems, object node)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Blocks.Count; i++)
            {
                Block b = Blocks[i];
                if (b.Type == BlockType.Text)
                {
                    sb.Append(b.Content);
                }
                else
                {
                    sb.Append(Code(node,tems, b));
                }
            }
            return sb.ToString();
        }

        public string Code(object obj, Dictionary<string, Tem> tems, Block code)
        {
            switch (code.Type)
            {
                case BlockType.Code: 
                    object so = Reader.Get(obj, code.Content);
                    if (so != null)
                    {
                        return so.ToString();
                    }
                    return "";
                case BlockType.Include:
                    if (tems.ContainsKey(code.Content))
                    {
                        Tem t = tems[code.Content];
                        return t.Render(obj);
                    }
                    return "";
                case BlockType.Item:
                    if (tems.ContainsKey(code.Content))
                    {
                        Tem t = tems[code.Content];
                        if (obj is NodeArray)
                        {
                            StringBuilder sb = new StringBuilder();
                            NodeArray ar = (NodeArray)obj;
                            for (int i = 0; i < ar.Count; i++)
                            {
                                sb.Append(t.Render(ar[i]));
                            }
                            return sb.ToString();
                        }
                        return t.Render(obj);
                    }
                    return "";

            }
            return "";
        }
    }
}
