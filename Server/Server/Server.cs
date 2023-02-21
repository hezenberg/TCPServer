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

    public class Server
    {
        private List<Client> Clients; // Список подключенных клиентов
       
        private Thread MessageThread;

        private Protocol Protocol;   
        private TcpListener _Server; // Сам сервер

        // Адресс сервера
        private string ip = "127.0.0.1";
        private int port = 8080;

        // Конфигурационный файл
        static string cfg_path = "setting.cfg";

        // Максимальное количество подключенний к серверу
        int max_conn = 100;

        /* 1 Секунда это 10000000 Ticks.
         * По стандарту очистка клиентов происходит раз в минуту.*/
        int min_cleaning = 0, timer_cleaning = 1;  

        public Server()
        {
            AppSettingServer(); // Парсинг cfg файла
            _Server = new TcpListener(Net.IPAddress.Parse(ip), port);
            Protocol = new Protocol();
            Clients = new List<Client>();

            try
            {
                _Server.Start();

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
            Console.WriteLine("Server started!");
            Console.ResetColor();

            /* Главный поток */
            HandleNewConnection(); 
        }

      
        /* Чтение настроек сервера из cfg файла:
         * ip, port, max_conn */
        private void AppSettingServer()
        {
            try
            {
                StreamReader sr = new StreamReader(cfg_path);
                while (!sr.EndOfStream)
                {
                    string[] splt_line = sr.ReadLine().Split(':');
                    switch (splt_line[0].Trim())
                    {
                        case "ip":
                            ip = splt_line[1].Trim();
                            break;
                        case "port":
                            port = Int32.Parse(splt_line[1].Trim());
                            break;
                        case "max_connection":
                            max_conn = Int32.Parse(splt_line[1].Trim());
                            break;
                    }
                }

                sr.Close();
            }
            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Config file not found, server used default setting."); Console.ResetColor();
                ip = "127.0.0.1"; port = 80; max_conn = 100;
            }
        }
     
        
        private void HandleNewConnection()
        {
            while (true)
            {
                if (this.max_conn >= this.Clients.Count)
                {
                    Client client;
   
                    try
                    {
                        client = new Client(_Server.AcceptTcpClient());
                    }
                    catch (SocketException)
                    {
                        continue;
                    }

                    client.SetBufferSize(Protocol.size_buffer_read);
                    Clients.Add(client);

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("Client: {0} connected, {1} clients on the server.", client.GetIpClient(), Clients.Count);
                    Console.ResetColor();
                   
                }
                else
                {
                    ShowError("New client trying connect, limit connection!");
                }
            }
        }   

        /* Проверка подклюенных клиентов на активность
         * В случае если клиент не способен принять данные 
         * он будет отключен от сервера и удален из списка 
         */
        private void FilterClients()
        {
            ShowMessage("Cleaning the server from dead clients.", ConsoleColor.Yellow);
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

        /* Обработка запросов от клиентов и последующаяя рассылка сообщений 
         *всем подключенным клиентам кроме самого отправителя */
        private void HandleMessages()
        {
            while (true)
            {
                /* Текущее время сравнивается с 
                 * текущем временем(min) + добавленное(time_cleaning) */
                int min = DateTime.Now.Minute;
                if (min > min_cleaning)
                {
                    FilterClients();
                    min_cleaning = min + timer_cleaning;
                }

                List<Client> DataForIter = new List<Client>(this.Clients);

                for (int i = 0; i < DataForIter.Count; i++)
                {
                    Client client = DataForIter[i];
                    string msg = Protocol.Read(client);
                    if (msg != null)
                    {
                        byte[] ready_data = Protocol.CreatePackage(msg.Length, msg);
                        Console.WriteLine("{0}: {1}", client.GetIpClient(), msg);
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

        public static void ShowMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
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
