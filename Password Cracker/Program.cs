using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Password_Cracker;
using Password_Cracker.Models;

namespace PasswordCrackerCentralized
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Cracking server");
            TcpListener listener = new TcpListener(IPAddress.Any, 4570);
            listener.Start();

            while (true)
            {
                TcpClient socket = listener.AcceptTcpClient();
                Task.Run(() => HandleClient(socket));
            }
        }
        public static void HandleClient(TcpClient socket)
        {
            Cracking cracker = new Cracking();
            Console.WriteLine(socket.Client.RemoteEndPoint.ToString());

            NetworkStream ns = socket.GetStream();
            StreamReader reader = new StreamReader(ns);
            StreamWriter writer = new StreamWriter(ns);
            
            while (true)
            {
                writer.WriteLine("request new chunk");
                writer.Flush();
                var message = "";
                try
                {
                    message = reader.ReadLine();
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e);
                    Console.WriteLine("Client terminated the connection");
                    return; //throw doesn't work
                }
                
                if (message != null)
                {
                    var data = JsonSerializer.Deserialize<List<String>>(message);

                    Console.WriteLine($"Server received: {data.Count} lines");

                    List<UserInfoClearText> listUserInfoClearTexts = cracker.RunCracking(data);

                    //writer.WriteLine(JsonSerializer.Serialize(listUserInfoClearTexts));
                    //writer.Flush();

                    foreach (var i in listUserInfoClearTexts)
                    {
                        writer.WriteLine(i.ToString());
                        writer.Flush();
                        Console.WriteLine($"Cracker sent: {i.ToString()}");
                    }
                }
            }
        }
    }
}