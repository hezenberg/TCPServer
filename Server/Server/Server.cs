using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;


// PROTICOL: 
// SIZE / TYPE_DATA / DATA

// Examples:
// 5 / text / Hello
// 500 / img / binary
// max size data - 1 gb

namespace Server
{	
	using Net = System.Net;
	using Sock = System.Net.Sockets;

    public class Server : TcpListener
    {
        // Хранит последнюю ошибку которая произошла в классе 
        private string LastError { get; set; } 
        // Список клиентов
        private List<Client> Clients;
        // Список каналов в которых находятся клиенты
        private List<List<Client>> Channels;
       
        private int MaxConnection;
        private Thread MessageThread;
      

        public Server(string ip, int port, int max_connect)
            : base(Net.IPAddress.Parse(ip), port)
        {
        
            this.MaxConnection = max_connect;
            this.LastError = String.Empty;
            this.Clients = new List<Client>();

            try
            {
                this.Start();
            }
            catch (SocketException e)
            {
                this.LastError = "Block INIT: " + e.ToString();
            }

            /* MAIN LOOP SERVER */

            this.MessageThread = new Thread(this.HandleMessages);
            this.MessageThread.Start();
            this.HandleNewConnection();

        }

     
        // Обработка новых подключений
        // Поключает клиента и закидывает в список Clients<Client>
        private void HandleNewConnection()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (this.MaxConnection >= this.Clients.Count)
                {
                    
                    Client client;
   
                    try
                    {
                        client = new Client(this.AcceptTcpClient());   
                    }
                    catch (SocketException e)
                    {
                        this.LastError = "Block CONNECT: " + e.ToString();
                        continue;
                    }

                    client.SetBufferSize(Protocol.size_buffer_read);
                    Clients.Add(client);

                    Console.WriteLine("Client: {0} connected", client.GetIpClient());
                }
                else
                {
                    Console.WriteLine("New client trying connect, limit connection!");
                }
            }
        }   

        private void HandleMessages()
        {
            while (true)
            {
                List<Client> DataForIter = new List<Client>(this.Clients);

                for (int i = 0; i < DataForIter.Count; i++)
                {
                    Client client = DataForIter[i];
                    var msg = Protocol.StrRead(ref client);
                    
                    if (client.status & msg != null)
                    {
                        Console.WriteLine(msg);
                    }
                    else
                    {
                        Clients.Remove(client);
                    }
                }

                DataForIter = null;
            }

        }


        private void Exit(int code, string text_error)
        {
            Console.WriteLine(text_error);
            Console.ReadKey();
            Environment.Exit(code);
        }
    }
}
