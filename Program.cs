using System;
using System.Text;
using Tlang;
using Tlang.JSON;
using Tlang.Web;


namespace App
{
    class Program
    {
        static void Main(string[] args)
        {
            WebApp app = new WebApp();
            app.Get("*", Hello);
            app.Start();

            Console.WriteLine("Web App running " + app.Url);
            F.OpenUrl(app.Url);
        }

        public static object Hello(Request req, Response res)
        {
            return "Hello world";
        }
    }
}
