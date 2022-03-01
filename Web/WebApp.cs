/*
    MIT license
    Ping tatadoan@outlook.com for Info | Bug | More Feature
    I'd be happy to have a response
*/
using System;
using System.Configuration;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.NetworkInformation;
using System.Text;
using Tlang.JSON;


namespace Tlang.Web
{

    public delegate object WebHandle(Request req, Response res);
    

    public class WebApp
    {
        public int Port = 0;
        public bool IsRun = false;
        public int Status = 0;        
        public bool IsTcp = true;
        Socket Listener;
        public IPEndPoint LocalPoint;
        Thread ListenThread;
        Thread CloserThread;

        public X509Certificate2 Certificate;


        public string Url
        {
            get
            {
                if (Certificate != null)
                {
                    if (Port == 443)
                    {
                        return "https://" + LocalPoint.Address.ToString();
                    }
                    return "https://" + LocalPoint;
                }
                else
                {
                    if (Port == 80)
                    {
                        return "http://" + LocalPoint.Address.ToString();
                    }
                    return "http://" + LocalPoint;
                }
            }
        }

        //public Dictionary<string, Connection> Connections = new Dictionary<string, Connection>();
        //public Dictionary<string, Connection> ConnectionWebSockets = new Dictionary<string, Connection>();

        public List<Connection> Connections = new List<Connection>();
        public List<Connection> ConnectionWebSockets = new List<Connection>();
        
        public List<RouteRule> Routes = new List<RouteRule>();

        public WebHandle WebSocketConnected;
        public WebHandle WebSocketData;

        public WebApp()
        {
            
        }

        

        public WebApp(object endPoint)
        {
            LocalPoint = GetEndPoint(endPoint);
            Port = LocalPoint.Port;
            //Register();
        }

        public static int NextOpenTcpPort(int startPort, int endPort)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();
            bool isClose = true;
            for (int p = startPort; p <= endPort; p++)
            {
                isClose = true;
                foreach (IPEndPoint ep in tcpEndPoints)
                {
                    if (ep.Port == p)
                    {
                        isClose = false;
                        break;
                    };
                }
                if (isClose) return p;
            }
            return 0;
        }

        public static IPEndPoint GetEndPoint(object endPoint)
        {
            if (endPoint is string)
            {
                string address = (string)endPoint;
                int j = address.IndexOf(':');
                if (j >= 0)
                {
                    string ip = address.Substring(0, j);
                    string port = address.Substring(j + 1);
                    int nport = int.Parse(port);
                    if (ip.Length == 0)
                    {
                        string enp = GetLocalIP().Split(',')[0];
                        //Console.WriteLine("Local EndPoint:" + enp);
                        return new IPEndPoint(IPAddress.Parse(enp), nport);
                    }
                    else
                    {
                        return new IPEndPoint(IPAddress.Parse(ip), nport);
                    }
                }
            }
            else if (endPoint is int)
            {
                string enp = GetLocalIP().Split(',')[0];
                //Console.WriteLine("Local EndPoint:" + enp);
                return new IPEndPoint(IPAddress.Parse(enp), (int)endPoint);
            }
            return new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
        }

        public static string GetLocalIP()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            string localIp = "";
            foreach (System.Net.NetworkInformation.NetworkInterface adapter in nics)
            {
                if (adapter.OperationalStatus != OperationalStatus.Up) continue;
                foreach (UnicastIPAddressInformation x in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (x.Address.AddressFamily == AddressFamily.InterNetwork && x.Address.ToString() != "127.0.0.1" && x.IsDnsEligible)
                    {
                        localIp += x.Address.ToString() + ",";
                    }
                }
            }
            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.OperationalStatus != OperationalStatus.Up) continue;
                foreach (UnicastIPAddressInformation x in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (x.Address.AddressFamily == AddressFamily.InterNetwork && x.Address.ToString() != "127.0.0.1" && !x.IsDnsEligible)
                    {
                        localIp += x.Address.ToString() + ",";
                    }
                }
            }
            string host = localIp.TrimEnd(',');
            if (host.Length == 0)
            {
                host = "127.0.0.1";
            }
            return host;
        }

        public static string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                Console.WriteLine(
                    "Found MAC Address: " + nic.GetPhysicalAddress() +
                    " Type: " + nic.NetworkInterfaceType);

                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    Console.WriteLine("New Max Speed = " + nic.Speed + ", MAC: " + tempMac);
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            return macAddress;
        }

        public static string GetMacAddresses()
        {
            List<string> l1 = new List<string>();
            string s = "";
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string addr = nic.GetPhysicalAddress().ToString();
                if (addr.Length > 0 && !l1.Contains(addr))
                {
                    l1.Add(addr);
                    s += "MAC: " + addr + " Type: " + nic.NetworkInterfaceType + "/" + nic.Speed + "\r\n";
                }
            }
            return s.TrimEnd('\r', '\n');
        }

        public void SetupCertificate(string fileName, string password)
        {
            Certificate = new X509Certificate2(fileName, password);
        }

        public void SetupCertificate()
        {
            Certificate = new X509Certificate2(App.Properties.Resources.certificate, "thangdoan");
        }

        public void Get(string url, WebHandle handle)
        {
            RouteRule r = new RouteRule();
            r.Method = "GET";
            r.Set(url);
            r.Handle = handle;
            Routes.Add(r);
        }

        public void Post(string url, WebHandle handle)
        {
            RouteRule r = new RouteRule();
            r.Method = "POST";
            r.Set(url);
            r.Handle = handle;
            Routes.Add(r);
        }

        public void Map(string url, WebHandle handle)
        {
            RouteRule r = new RouteRule();
            r.Set(url);
            r.Handle = handle;
            Routes.Add(r);
        }

        public void Map(string method, string url, WebHandle handle)
        {
            RouteRule r = new RouteRule();
            r.Set(url);
            r.Method = method;
            r.Handle = handle;
            Routes.Add(r);
        }

        public void Hand(Request req, Response res)
        {
            try
            {
                WebHandle han = Find(req.Path, req.Method, req);
                if (han != null)
                {
                    object kq = han(req, res);
                    if (!res.Handled)
                    {
                        if (req.IsDone)
                        {
                            if (res.Data == null)
                            {
                                if (res.Content.Length > 0)
                                {
                                    res.Data = BinaryBuilder.Html(res.Content, res.ResponseCode, res.ContentType, res.Charset, res.Headers.ToArray());
                                }
                                else
                                {
                                    res.Data = BinaryBuilder.Html(kq.ToString(), res.ResponseCode, res.ContentType, res.Charset, res.Headers.ToArray());
                                }
                            }
                            res.Send(res.Data);
                        }
                        else
                        {
                            res.Content += kq.ToString();
                        }
                    }
                }
                else
                {
                    byte[] ht404 = BinaryBuilder.Message("404", "404", "error");
                    res.Send(ht404);
                }
            }
            catch (Exception ex)
            {

                byte[] ht404 = BinaryBuilder.Message(ex.Message, "404", "error");
                res.Send(ht404);
            }
        }

       

        

        private WebHandle Find(string url, string method, Request req)
        {
            WebHandle han = null;
            for (int i = 0; i < Routes.Count; i++)
            {
                RouteRule r = Routes[i];
                if (r.Method.Length > 0)
                {
                    if (r.Method == method && r.Check(url))
                    {
                        if (r.Parts != null)
                        {
                            r.Parser(url, req.Get);
                        }
                        han = r.Handle;
                        break;
                    }
                }
                else
                {
                    if (r.Check(url))
                    {
                        if (r.Parts != null)
                        {
                            r.Parser(url, req.Get);
                        }
                        han = r.Handle;
                        break;
                    }
                }
            }
            return han;
        }

        public void Start()
        {
            if (Port == 0)
            {
                if (Certificate == null)
                {
                    LocalPoint = GetEndPoint(NextOpenTcpPort(80, 100));
                    Port = LocalPoint.Port;
                }
                else
                {
                    LocalPoint = GetEndPoint(NextOpenTcpPort(443, 500));
                    Port = LocalPoint.Port;
                }
            }
            if (IsTcp)
            {
                ListenThread = new Thread(new ThreadStart(Listenning));
                IsRun = true;
                ListenThread.Start();
                Status = 1;
            }
            CloserThread = new Thread(new ThreadStart(ClearDisConnected));
            CloserThread.Start();
        }

        public void Stop()
        {
            IsRun = false;
            Status = 0;
            if (ListenThread != null)
            {
                try
                {
                    ListenThread.Abort();
                }
                catch
                {

                }
            }

            List<Connection> list = new List<Connection>();
            try
            {
                for (int i = 0; i < Connections.Count; i++)
                {
                    list.Add(Connections[i]);
                }
                for (int i = 0; i < ConnectionWebSockets.Count; i++)
                {
                    list.Add(ConnectionWebSockets[i]);
                }
            }
            catch { }
            foreach (Connection t in list)
            {
                try
                {
                    t.Thread.Abort();
                    t.Sock.Shutdown(SocketShutdown.Both);
                    t.Sock.Close();
                }
                catch
                {

                }
            }
            Connections.Clear();
            ConnectionWebSockets.Clear();
            if (Listener != null)
            {
                try
                {
                    Listener.Close();
                    //if (OnStop != null)
                    //{
                    //    OnStop(this, "");
                    //}
                }
                catch
                {

                }
            }

        }

        private void Listenning()
        {
            try
            {
                Listener = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                Listener.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                Listener.Bind(new IPEndPoint(IPAddress.IPv6Any, LocalPoint.Port));                
                Listener.Listen(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            while (IsRun)
            {
                try
                {


                    Socket handler = Listener.Accept();
                    IPEndPoint RemotePoint = (IPEndPoint)handler.RemoteEndPoint;
                    string key = RemotePoint.ToString();
                    
                    if (Certificate != null)
                    {
                        ConnectionSSL ht = new ConnectionSSL(handler, this);
                        ht.certificate = Certificate;
                        ht.Key = key;
                        Connections.Add(ht);
                        ht.Start();
                    }
                    else
                    {
                        Connection ht = new Connection(handler, this);
                        ht.Key = key;
                        Connections.Add(ht);
                        ht.Start();
                    }

                }
                catch
                {

                }
            }
        }

        private void ClearDisConnected()
        {
            while (IsRun)
            {
                if (!ListenThread.IsAlive)
                {
                    Connections.Clear();
                    ConnectionWebSockets.Clear();
                    if (IsTcp)
                    {
                        ListenThread = new Thread(new ThreadStart(Listenning));
                        ListenThread.Start();
                    }
                }
                try
                {
                    List<Connection> all = new List<Connection>();
                    List<Connection> remove = new List<Connection>();

                    for (int i = 0; i < Connections.Count; i++)
                    {
                        all.Add(Connections[i]);
                    }
                    for (int i = 0; i < ConnectionWebSockets.Count; i++)
                    {
                        all.Add(ConnectionWebSockets[i]);
                    }
                    foreach (Connection rt in all)
                    {
                        if (!rt.IsKeepAlive)
                        {
                            if (!rt.IsConnected())
                            {
                                if (rt.IsWebSocket)
                                {
                                    Console.WriteLine("IsWebSocket close");
                                }
                                remove.Add(rt);
                            }
                        }
                    }
                    foreach (Connection rt in remove)
                    {
                        if (Connections.Contains(rt))
                        {
                            rt.Sock.Close();
                            Connections.Remove(rt);
                        }
                    }
                }
                catch { }
                Thread.Sleep(3000);
            }
        }

        public List<string> GetAllUserInRoom(string room)
        {
            List<string> users = new List<string>();
            for (int i = 0; i < ConnectionWebSockets.Count; i++)
            {
                Connection con = ConnectionWebSockets[i];
                if (con.req != null)
                {
                    string user = con.req.UserName;
                    if (!users.Contains(user))
                    {
                        users.Add(user);
                    }
                }
            }
            return users;
        }

        public List<string> GetAllRoom()
        {
            List<string> rooms = new List<string>();
            for (int i = 0; i < ConnectionWebSockets.Count; i++)
            {
                Connection con = ConnectionWebSockets[i];
                if (con.req != null)
                {
                    string room = con.req.Room;
                    if (!rooms.Contains(room))
                    {
                        rooms.Add(room);
                    }
                }
            }
            return rooms;
        }
    }
}
