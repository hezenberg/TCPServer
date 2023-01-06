using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;


namespace Server
{
    class Client
    {
       public TcpClient client;
        public NetworkStream stream;
        public bool status = true;
        public Client(TcpClient client)
        {
            this.client = client;
            stream = new NetworkStream(client.Client);
            
        }

        public void SetBufferSize(int size)
        {
            client.SendBufferSize = size;
        }

        public string GetIpClient()
        {
            return ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString(); ;
        }

        public int Available()
        {
            return client.Available;
        }
    }
}
