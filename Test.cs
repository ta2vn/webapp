using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Tlang;
using Tlang.JSON;
using Tlang.Web;

namespace App
{
    public static class Test
    {
        

        public static void TestApp()
        {


            WebApp app = new WebApp();
            
            app.SetupCertificate();

            app.Get("*", Hello);

            app.Get("a/{id}/{name}", TestParam);
            app.Get("a-{id}-{name}", TestParam);
            app.Get("json", TestJSON);

            app.Get("user", User);

            app.Get("cookie", SetCookie);
            app.Get("cookie1", ClearCookie);
            app.Get("redirect", Redirect);
            app.Post("upload", Upload);

            app.Map("GET", "post/{id}", ById);
            app.Map("GET", "posts/{page}", Page);
            app.Map("GET", "posts", All);
            app.Map("POST", "posts", New);
            app.Map("PUT", "post/{id}", Update);
            app.Map("DELETE", "post/{id}", Delete);

            app.Get("*", ResponseFile);

            

           
            app.Start();


            Console.WriteLine("Web App running");
            Console.WriteLine(app.Url);
            F.OpenUrl(app.Url);
        }

        public static object Hello(Request req, Response res)
        {
            return "Hello world";
        }

        public static object TestParam(Request req, Response res)
        {
            res.Render("This is id=${id} and this is name=${name}", req.Get);
            return "OK";
        }

        public static object TestJSON(Request req, Response res)
        {
            res.JSON(new string[] { "a", "b", "c", "d", "e", "f" });
            return "OK";
        }

        public static object User(Request req, Response res)
        {
            return Reader.Parser("{name:'tata doan',age:30, email:'ok@gmail.com'}");
        }

        public static object SetCookie(Request req, Response res)
        {
            res.SetCookie("tata", F.Random().ToString(), 10240, "/");
            res.SetCookie("mary", F.Random().ToString(), 10240, "/");
            res.SetCookie("ruby", F.Random().ToString(), 10240, "/");
            return "OK";
        }

        public static object ClearCookie(Request req, Response res)
        {
            res.ClearCookie("tata");
            res.ClearCookie("mary");
            res.ClearCookie("ruby");
            res.ClearCookie("tom");
            return "OK";
        }

        public static object Redirect(Request req, Response res)
        {

            res.SetCookie("tata", F.Random().ToString(), 10240, "/");
            res.SetCookie("mary", F.Random().ToString(), 10240, "/");
            res.SetCookie("ruby", F.Random().ToString(), 10240, "/");
            res.Redirect("user");
            return "OK";
        }

        public static object ById(Request req, Response res)
        {
            DB db = IO.GetDB("db.json");
            Node item = db.FindByID(req.Get["id"]);
            if (item == null)
            {
                res.ResponseCode = "404";
                return "404";
            }
            return item;
        }

        public static object All(Request req, Response res)
        {
            DB db = IO.GetDB("db.json");
            return db.Data;
        }

        public static object Page(Request req, Response res)
        {
            DB db = IO.GetDB("db.json");
            return db.Data.Page(req.Get.GetInt("page"), 3);
        }

        public static object New(Request req, Response res)
        {
            object obj = Reader.Parser(req.Text());
            if (obj is Node)
            {
                Node item = (Node)obj;
                DB db = IO.GetDB("db.json");
                item.Set("last", DateTime.Now);
                item = db.New(item);
                db.Save();
                return item;
            }
            return "Error";
        }

        public static object Update(Request req, Response res)
        {

            object obj = Reader.Parser(req.Text());
            if (obj is Node)
            {
                Node itemnew = (Node)obj;
                DB db = IO.GetDB("db.json");
                db.Update(req.Get["id"], itemnew);
                db.Save();
                return db.FindByID(req.Get["id"]);
            }
            return "Error";
        }

        public static object Delete(Request req, Response res)
        {
            DB db = IO.GetDB("db.json");
            Node olditem = db.FindByID(req.Get["id"]);
            if (db.Delete(olditem))
            {
                db.Save();
                return "OK";
            }
            else
            {
                res.ResponseCode = "404";
                return "404";
            }
        }

        public static object ResponseFile(Request req, Response res)
        {
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

        public static object Upload(Request req, Response res)
        {
            //res.Handled = true;
            string filename = req.FileName;
            if (filename.Length > 0)
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                FileStream fs = IO.GetFile(filename, FileMode.OpenOrCreate);
                while (res.ReadStream())
                {
                    if (req.Data == null) break;
                    fs.Write(req.Data, 0, req.Data.Length);
                }
                fs.Close();
            }
            return filename + " OK";
        }

        public static object WebSocketConneted(Request req, Response res)
        {
            req.Room = req.Get.GetString("room");
            return "OK";
        }

        public static object WebSocketRoom(Request req, Response res)
        {
            string text = Encoding.UTF8.GetString(req.Data);
            res.SendSocketRoom(text, req.Room);
            return "OK";
        }
    }
}
