using System;
using System.IO;
using System.Text;
using Tlang;
using Tlang.JSON;
using Tlang.Web;


namespace App
{
    class Program
    {
        public static string MyPath="";
        public static WebApp app;
        static void Main(string[] args)
        {
            MyPath = Directory.GetCurrentDirectory();
            app = new WebApp();

            app.Get("", ResponseFileIndex);
            app.Post("getpass", GetPass);
            app.Get("*.csv", NoDownload);
            app.Get("*", ResponseFile);
            app.Start();
            
            

            Console.WriteLine("Web App running " + app.Url);
            F.OpenUrl(app.Url);
        }

        

        public static object ResponseFile(Request req, Response res)
        {
            //Console.WriteLine(req.Path);
            if (File.Exists(req.Path))
            {
                res.SendFile(req.Path);
            }
            if (Directory.Exists(Directory.GetCurrentDirectory() + req.Path))
            {
                return Files(req, res);
            }
            res.ResponseCode = "404";
            return "404";
        }

        public static object NoDownload(Request req, Response res)
        {
            
            res.ResponseCode = "404";
            return "Khong duoc tai file";
        }


        public static object ResponseFileIndex(Request req, Response res)
        {
            string path = "index.html";
            if (File.Exists(path))
            {
                res.SendFile(path);
            }
            res.ResponseCode = "404";
            return "404";
        }

        public static object Files(Request req, Response res)
        {
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory() + req.Path);
            NodeArray ar = new NodeArray();

            foreach (FileInfo fi in di.GetFiles())
            {
                Node n = new Node();
                n.Set("name", fi.Name);
                n.Set("size", fi.Length);
                n.Set("last", fi.LastWriteTime);
                ar.Add(n);
            }
            res.JSON(ar);
            return "OK";
        }

       

        public static object GetPass(Request req, Response res)
        {
            if (req.Post["username"] != null)
            {
                string username = req.Post["username"].ToString();

                if (File.Exists("data.csv"))
                {
                    NodeArray data = Reader.FromCSVFile("data.csv");
                    NodeArray check = data.Filter("username", username);
                    if (check.Count > 0)//tim thay user do
                    {
                        Node item = (Node)check[0];
                        return item.GetString("password");
                    }
                    else// khong tim thay
                    {
                       return "Khong tim thay";
                    }
                }
                else
                {
                    return "Khong tim thay file data.csv";
                }
            }
            else
            {
                return "Vui long nhap username";
            }
        }
    }
}
