using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;


namespace Server
{
    class Client
    {
        private TcpClient client;
        public NetworkStream stream;

        public Client(TcpClient client)
        {
            this.client = client;
            stream = new NetworkStream(client.Client);
        }
    }
}
