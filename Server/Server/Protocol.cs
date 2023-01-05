using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    static class Protocol
    {
        public static int size_data_package = 5, size_buffer_read = 256;


        public static string StrRead(ref Client client)
        {
            string Data = String.Empty;
            int expected_size = 0, total_size = 0;
            byte[] size_package = new byte[size_data_package];
            byte[] buffer_package = new byte[size_buffer_read];

            try
            {
                client.stream.Read(size_package, 0, size_data_package);
            }
            catch(IOException)
            {
                client.status = false;
                return "";
            }

            string str_size = System.Text.Encoding.UTF8.GetString(size_package);
            if (!Int32.TryParse(str_size, out expected_size))
            {
                Console.WriteLine("Bad request");
                StrWrite(ref client, "400");
            }

            expected_size++;

            while (total_size <= expected_size)
            {
                if ((expected_size - total_size) < size_buffer_read)
                {
                    client.stream.Read(buffer_package, 0, (expected_size - total_size));
                    Data += System.Text.Encoding.UTF8.GetString(buffer_package);
                    return Data;
                }

                int size = client.stream.Read(buffer_package, 0, size_buffer_read);
                total_size += size;
                Data += System.Text.Encoding.UTF8.GetString(buffer_package);
            }

            return Data;
        }

        public static void StrWrite(ref Client client, string data)
        {
            byte[] w_buffer = Encoding.ASCII.GetBytes(data);
            client.stream.Write(w_buffer, 0, w_buffer.Length);
        }


  
    }
}
