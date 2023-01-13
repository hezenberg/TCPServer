using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public abstract class Protocol
    {
        enum ErrorCode
        {
            BadRequest = 400
        };

        public static int size_data_package = 5, size_buffer_read = 256;

        public static void DisconnectClient(ref Client client)
        {
            client.status = false;
            client.stream.Close();
            Console.WriteLine("Client {0} disconnected", client.GetIpClient());
        }
    }
}
