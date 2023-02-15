using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

namespace Server
{
    public abstract class Protocol
    {
     

        public static int size_data_package = 5, size_buffer_read = 256;

        public static void DeadClientFilter(List<Client> clients)
        {
            byte[] testsend = {};
            for (int i = 0; i < clients.Count; i++)
            {
                try
                {
                    clients[i].client.Client.Send(testsend);
                }
                catch(SocketException)
                {
                    Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Client id:{0} disconnected", i); Console.ResetColor();
                    clients[i].client.Close();
                    clients.RemoveAt(i);
                }
                
            }
        }
    }
}
