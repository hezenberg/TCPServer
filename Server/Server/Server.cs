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
       
        private int MaxConnection;
        private Thread MessageThread;
        private Thread HandleDisconnected;
        private MessageProtocol MsgProtocol;

        // Таймер через сколько сработает фильтр для удаления отключенных клиентов
        public int TimeSecForFilter = 5;
        public Server(string ip, int port, int max_connect)
            : base(Net.IPAddress.Parse(ip), port)
        {
            MsgProtocol = new MessageProtocol();
        
            MaxConnection = max_connect;
            LastError = String.Empty;
            Clients = new List<Client>();
        

            try
            {
                this.Start();

            }
            catch (SocketException e)
            {
                this.LastError = "Block INIT: " + e.ToString();
            }

            Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("Server socket create!"); Console.ResetColor();

            /* Основные потоки сервера */
            Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("Started threads..."); Console.ResetColor();
            MessageThread = new Thread(HandleMessages); MessageThread.Start();
            HandleDisconnected = new Thread(FileterClients); HandleDisconnected.Start();
            Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("Server started!"); Console.ResetColor();

            /* Главный поток */
            HandleNewConnection(); 
            
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

                    client.SetBufferSize(MessageProtocol.size_buffer_read);
                    Clients.Add(client);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Client: {0} connected, {1} clients on the server.", client.GetIpClient(), Clients.Count);
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("New client trying connect, limit connection!"); Console.ResetColor();
                }
            }
        }   

        /* Проверка клиентов на отключения через каждые TimeSecForFilter секунд */
        private void FileterClients()
        {   while(true)
            {
                Thread.Sleep(TimeSecForFilter * 1000);
                Protocol.DeadClientFilter(this.Clients);
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
                    string msg = MessageProtocol.Read(client);

                    if (msg != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue; Console.WriteLine(msg); Console.ResetColor();
                        for (int j = 0; j < DataForIter.Count; j++)
                        {
                            if (j == i)
                                continue;
                            client = DataForIter[j];
                            MessageProtocol.StrWrite(client, msg);
                        }
                    }
                    else
                        continue;

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
