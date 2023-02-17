using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Server
{	
	using Net = System.Net;
	using Sock = System.Net.Sockets;

    public class Server : TcpListener
    {
    
        // Список клиентов
        private List<Client> Clients;
       
        private int MaxConnection;
        private Thread MessageThread;
        private Thread HandleDisconnected;
        private Protocol Protocol;

        // Таймер через сколько сработает фильтр для удаления отключенных клиентов
        public int TimeSecForFilter = 5;
        public Server(string ip, int port, int max_connect)
            : base(Net.IPAddress.Parse(ip), port)
        {
            Protocol = new Protocol();
        
            MaxConnection = max_connect;
            Clients = new List<Client>();
        

            try
            {
                this.Start();

            }
            catch (SocketException e)
            {
                ShowError("Server startup failed!", e.ToString());
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Server socket create!");
            /* Основные потоки сервера */
            Console.WriteLine("Started threads...");
            MessageThread = new Thread(HandleMessages); MessageThread.Start();
            HandleDisconnected = new Thread(FileterClients); HandleDisconnected.Start();
            Console.WriteLine("Server started!");
            Console.ResetColor();

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
                    catch (SocketException)
                    {
                        continue;
                    }

                    client.SetBufferSize(Protocol.size_buffer_read);
                    Clients.Add(client);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Client: {0} connected, {1} clients on the server.", client.GetIpClient(), Clients.Count);
                    Console.ResetColor();
                }
                else
                {
                    ShowError("New client trying connect, limit connection!");
                }
            }
        }   

        /* Проверка клиентов на отключения через каждые TimeSecForFilter секунд */
        private void FileterClients()
        {
            while (true)
            {
                Thread.Sleep(TimeSecForFilter * 1000);
                byte[] testsend = { };
                for (int i = 0; i < Clients.Count; i++)
                {
                    if(Clients[i].status_connect)
                    {
                        try
                        {
                            Clients[i].client.Client.Send(testsend);
                        }
                        catch (SocketException)
                        {
                            DisconnectClient(i);
                        }
                    }
                    else
                    {
                        DisconnectClient(i);
                    }
               
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
                    string msg = Protocol.Read(client);
            
                    if (msg != null)
                    {
                        byte[] ready_data = Protocol.CreatePackage(msg.Length, msg);
                        Console.ForegroundColor = ConsoleColor.Blue; Console.WriteLine(msg); Console.ResetColor();
                        for (int j = 0; j < DataForIter.Count; j++)
                        {
                            if (j == i)
                                continue;
                            Protocol.Write(DataForIter[j], Protocol.CreatePackage(msg.Length, msg));
                        }
                    }
                    else
                        continue;

                }

                DataForIter = null;
            }

        }


        private void ShowError(string text, string error = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if(error != null)
                Console.WriteLine("{0}\n{1}", text, error);
            else
                Console.WriteLine(text);
            Console.ResetColor();
        }

        private void DisconnectClient(int id)
        {
            Console.ForegroundColor = ConsoleColor.Yellow; 
            Console.WriteLine("Client id:{0} disconnected", id); 
            Console.ResetColor();
            Clients[id].client.Close();
            Clients.RemoveAt(id);
        }
    }
}
