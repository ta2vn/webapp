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
using System.Text;
using System.Reflection;
namespace Tlang.Web
{
    public class ConnectionSSL : Connection
    {
        SslStream sslStream;
        public X509Certificate2 certificate;

        public ConnectionSSL(Socket soc, WebApp parent)
            : base(soc, parent)
        {

        }


        public override void Handling()
        {
            tryCount = 0;
            try
            {
                //sslStream = new SslStream(new NetworkStream(Handler), false, VerifyClientCertificate);
                sslStream = new SslStream(new NetworkStream(Sock), false);
                //sslStream.BeginAuthenticateAsServer(certificate, AuthenticateCallback, null);

                //Console.WriteLine("remote:"+sslStream.BeginAuthenticateAsServer();
                sslStream.AuthenticateAsServer(certificate, false, (SslProtocols)0 | (SslProtocols)12 | (SslProtocols)48 | (SslProtocols)192 | (SslProtocols)768 | (SslProtocols)3072, false);
                //sslStream.AuthenticateAsServer(certificate);

                //readbyte = sslStream.Read(buffer, 0, buffer.Length);
                // Display the properties and settings for the authenticated stream.
                //NetData.DisplaySecurityLevel(sslStream);
                //NetData.DisplaySecurityServices(sslStream);
                //NetData.DisplayCertificateInformation(sslStream);
                //NetData.DisplayStreamProperties(sslStream);

                // Set timeouts for the read and write to 5 seconds.
                sslStream.ReadTimeout = 5000;
                sslStream.WriteTimeout = 5000;
                //Console.WriteLine("new:"+Key);
                //Thread.Sleep(100);
                while (Parent.IsRun && Sock.Connected && CountOut > 0)
                {
                    if (!IsKeepAlive) CountOut--;
                    if (Sock.Available == 0)
                    {
                        Thread.Sleep(2);
                        tryCount++;
                        if (tryCount > 500)
                        {
                            IsRun = false;
                            IsTimeOut = true;
                        }
                        continue;
                    }
                    if (Sock.Connected)
                    {
                        readbyte = Read(buffer);
                        //Console.WriteLine(sslStream.CipherAlgorithm);
                    }

                    if (Sock.Connected && readbyte > 0)
                    {
                        //byte[] receiveBytes = new byte[readbyte];
                        //System.Buffer.BlockCopy(buffer, 0, receiveBytes, 0, readbyte);

                        mem.Write(buffer, 0, readbyte);
                        //if (ContentLength > 10000 && !IsWebSocket && !IsChunked)
                        //{

                        //    string bou = GetBoundary(Header);
                        //    if (bou.Length > 0)
                        //    {
                        //        byte[] boubyte = Encoding.ASCII.GetBytes("--" + bou);
                        //        System.IO.FileStream fs = null;
                        //        //fs.Write(mem.ToArray(), 0, (int)mem.Length);
                        //        int koi = 0;
                        //        long tt = mem.Length;
                        //        while (ContentIndex + ContentLength > tt && Parent.IsRun && Handler.Connected)
                        //        {
                        //            readbyte = sslStream.Read(buffer, 0, buffer.Length);
                        //            koi = Bytes.IndexOf(buffer, boubyte, 0);
                        //            if (koi >= 0)
                        //            {
                        //                //thay roi do.
                        //                Console.WriteLine("koi " + koi);
                        //                int kii = Bytes.IndexOf(buffer, NetData.WebHeaderBreak, koi + boubyte.Length);
                        //                string ch = Encoding.UTF8.GetString(Bytes.SubBytes(buffer, koi + boubyte.Length + 2, kii - koi - boubyte.Length - 2));
                        //                string fname = GetFileName(ch.Split('\n'));
                        //                if (fname.Length > 0)
                        //                {
                        //                    if (fs != null)
                        //                    {
                        //                        fs.Close();
                        //                    }
                        //                    fs = System.IO.File.Create(fname);
                        //                }
                        //                fs.Write(buffer, kii + 4, readbyte - kii - 4);
                        //            }
                        //            else
                        //            {
                        //                //mem.Write(buffer, 0, readbyte);
                        //                fs.Write(buffer, 0, readbyte);
                        //            }
                        //        }

                        //        fs.Close();
                        //    }
                        //    else
                        //    {
                        //        while (ContentIndex + ContentLength > mem.Length && Parent.IsRun && Handler.Connected)
                        //        {
                        //            readbyte = sslStream.Read(buffer, 0, buffer.Length);
                        //            mem.Write(buffer, 0, readbyte);
                        //        }
                        //    }


                        //}
                        //string cn = Encoding.ASCII.GetString(receiveBytes);
                        ////Console.WriteLine("--" + cn + "--");

                        CheckPool();
                    }
                }
            }
            catch { }
            //catch (AuthenticationException ea)
            //{
            //    //Console.WriteLine("AuthenticationException:" + ea.Message + "\r\n" + ea.InnerException.Message);
            //    //if (Handler.Connected)
            //    //{
            //    //    int readbyte = Handler.Receive(buffer);
            //    //}
            //    //IsDone = true;
            //    //IsRun = false;
            //    IsError = true;

            //    //sslStream.Close();
            //    //Handler.Close();
            //    //Console.WriteLine(Parent.Connection.Count+" close:" + Key);
            //    //if (Parent.Connection.Count > 0)
            //    //{
            //    //    Parent.Connection.Remove(Key);
            //    //}

            //    //return;
            //}
            //catch (Exception ex)
            //{
            //    //if (ex.Message.Contains("same"))
            //    //{
            //    //    Console.WriteLine("nnn");
            //    //}
            //    //if (ex.InnerException.Message != "An unknown error occurred while processing the certificate")
            //    //    Console.WriteLine("AAASSL:" + ex.Message);
            //}
            finally
            {
                IsDone = true;
                IsRun = false;
                sslStream.Close();
                Sock.Close();

                if (Parent.Connections.Contains(this))
                {
                    Parent.Connections.Remove(this);
                }
                if (Parent.ConnectionWebSockets.Contains(this))
                {
                    Parent.ConnectionWebSockets.Remove(this);
                }
            }
        }

        public override int Read(byte[] buffer)
        {
            return sslStream.Read(buffer, 0, buffer.Length);
        }
        public override void Send(byte[] data)
        {
            if (certificate != null && sslStream != null && sslStream.CanWrite)
            {
                try
                {
                    //if(!SocketIsDisposed( Handler))
                    sslStream.Write(data);
                }
                catch (IOException ioe)
                {
                    Console.WriteLine(ioe.Message);
                    sslStream.Close();
                }
            }
        }
    }
}
