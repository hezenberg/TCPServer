using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;


namespace Server
{
    public class Client
    {
        public TcpClient client;
        public NetworkStream stream;
        public bool status_connect = true;

        /* Хранит время последнего сообщения */
        public long ticks_last_message = -1;
        public long ticks_start_blocking = 0;
        public Client(TcpClient client)
        {
            this.client = client;
            stream = new NetworkStream(client.Client);
            
        }

        public void SetBufferSize(int size)
        {
            client.SendBufferSize = size;
        }

        /* Возвращает IP клиента в строковом формате */
        public string GetIpClient()
        {
            return ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString(); ;
        }

        public bool IsDataExist()
        {
            try
            {
                return stream.DataAvailable;
            }
            catch
            {
                return true;
            }

        }
    }
}
