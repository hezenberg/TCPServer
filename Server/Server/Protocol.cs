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

            /* Получение данных о размере будещего пакета.
             * Если данных нет то функция вернет null и пойдет проверять следующего.
             * Если будет вызванна ошибка ввода вывода клиент будет удален. */
            try
            {
                if (client.IsDataExist())
                    client.stream.Read(size_package, 0, size_data_package);
                else
                    return null;
            }
            catch(IOException)
            {
                DisconnectClient(ref client);
                return null;
            }

            /* Распаковка данных, если попытка преобразовать первые 5 байтов в int завершится провалом
             * сервер вернет ошибку запроса, и переключится на другого клиента */
            string str_size = System.Text.Encoding.UTF8.GetString(size_package);
            if (!Int32.TryParse(str_size, out expected_size))
            {
                Console.WriteLine("Bad request");
                return null;
            }

            expected_size++; 

            /* Считывание данных с сокета, считанно данных будет ровно столько сколько 
             * было указанно в размере пакета. */
            while (total_size <= expected_size)
            {
            
                try
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
                catch(IOException e)
                {
                    DisconnectClient(ref client);
                    return null;
                }
            }

            return Data;
                
        }

        public static void StrWrite(ref Client client, string data)
        {
            byte[] w_buffer = Encoding.ASCII.GetBytes(data);
            client.stream.Write(w_buffer, 0, w_buffer.Length);
        }

        // Закрытие соединения с клиентов и установка статуса клиента на удаление
        public static void DisconnectClient(ref Client client)
        {
            client.status = false;
            client.stream.Close();
            Console.WriteLine("Client {0} disconnected", client.GetIpClient());
        }
  
    }
}
