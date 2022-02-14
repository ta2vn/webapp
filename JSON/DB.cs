/*
    MIT license
    Ping tatadoan@outlook.com for Info | Bug | More Feature
    I'd be happy to have a response
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Tlang.JSON
{
    public class DB
    {
        public string Path = "";
        public NodeArray Data;
        FileStream fs;
        Dictionary<string, Node> IDs = new Dictionary<string, Node>();

        public DB(string path)
        {
            Path = path;
            Open();
        }

        public void Open()
        {
            object ok = Reader.Parser(Text());
            if (ok is NodeArray)
            {
                Data = (NodeArray)ok;
                IDs = Data.GetDic("id");
            }
            else
            {
                Data = new NodeArray();
                IDs.Clear();
            }
        }

        public Node FindByID(object id)
        {
            if (id != null)
            {
                if (IDs.ContainsKey(id.ToString()))
                {
                    return IDs[id.ToString()];
                }
            }
            return null;
        }


        /// <summary>
        /// Find new first
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Node FindBy(string name, object value)
        {
            for (int i = Data.Count - 1; i >= 0; i--)
            {
                object obj = Data[i];
                if (obj is Node)
                {
                    Node n = (Node)obj;
                    if (n.ContainsKey(name) && n[name].Equals(value))
                    {
                        return n;
                    }
                }
            }
            return null;
        }


        

        public Node New(Node node)
        {
            string id = node.GetString("id");
            if (id == "")
            {
                id = DateTime.Now.Ticks.ToString();
                node.Set("id", id);
            }
            IDs.Add(id, node);
            Data.Add(node);
            return node;
        }

        public bool Update(object id, Node nodenew)
        {

            Node nodeold = FindByID(id);
            if (nodeold == null) return false;
            foreach (KeyValuePair<string, object> k in nodenew)
            {
                nodeold.Set(k.Key, k.Value);
            }

            return true;
        }

        public bool Delete(object id)
        {
            if (id != null)
            {
                string ids = id.ToString();
                if (IDs.ContainsKey(ids))
                {
                    Node node = IDs[ids];
                    if (Data.Contains(node))
                    {
                        Data.Remove(node);
                    }
                    IDs.Remove(ids);
                    return true;
                }
            }
            return false;
        }

        public bool Delete(Node node)
        {
            string id = node.GetString("id");
            if (IDs.ContainsKey(id))
            {
                Data.Remove(node);
                IDs.Remove(id);
                return true;
            }
            return Data.Remove(node);
        }

        byte[] ReadBinary()
        {
            fs = IO.GetFile(Path);
            byte[] data = new byte[(int)fs.Length];
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(data, 0, data.Length);
            fs.Close();
            return data;
        }

        

        void SaveBinary(byte[] data)
        {
            fs = IO.GetFile(Path, FileMode.OpenOrCreate);
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(data, 0, data.Length);
            fs.SetLength((long)data.Length);
            fs.Close();
        }
        
        public string Text()
        {
            byte[] buffer = ReadBinary();
            if (buffer == null) return "";
            if (buffer.Length > 2)
            {
                if (buffer[0] == 0xFF)
                {
                    if (buffer[1] == 0xFE)
                    {
                        if (buffer[2] == 0x00)
                        {
                            //HeaderByte = IO.SubBytes(buffer, 0, 4);
                            return Encoding.UTF32.GetString(F.SubBytes(buffer, 4));
                        }
                        //HeaderByte = IO.SubBytes(buffer, 0, 2);
                        return Encoding.Unicode.GetString(F.SubBytes(buffer, 2));
                    }
                }
                if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                {
                    //HeaderByte = IO.SubBytes(buffer, 0, 3);
                    return Encoding.UTF8.GetString(F.SubBytes(buffer, 3));
                }
            }
            return Encoding.UTF8.GetString(buffer);
        }

        public void Save()
        {
            string text = Data.ToString();
            byte[] data = Encoding.ASCII.GetBytes(text);
            SaveBinary(data);
            fs.Close();
        }
    }
}
