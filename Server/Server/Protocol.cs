using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;


namespace Server
{
    public class Protocol
    {
        public static int size_data_package = 5, size_buffer_read = 256;

        public static string Read(Client client)
        {   
            string Data = String.Empty;
            int expected_size = 0, total_size = 0, size = 0;
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
            catch(Exception)
            {
                return null;
            }

            /* Распаковка данных, если попытка преобразовать первые 5 байтов в int завершится провалом
             * сервер вернет ошибку запроса, и переключится на другого клиента */
            string str_size = System.Text.Encoding.UTF8.GetString(size_package);
            if (!Int32.TryParse(str_size, out expected_size))
            {
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Bad request"); Console.ResetColor();
                return null;
            }

            expected_size++; 

            /* Считывание данных с сокета, считанно данных будет ровно столько сколько 
             * было указанно в размере пакета. */
            while (total_size <= expected_size)
            {
                try
                {
                    if ((expected_size - total_size) < size_buffer_read){
                    
                        size = client.stream.Read(buffer_package, 0, (expected_size - total_size));
                        Data += System.Text.Encoding.UTF8.GetString(buffer_package, 0, size);
                        return Data;
                    }

                    size = client.stream.Read(buffer_package, 0, size_buffer_read);
                    total_size += size;
                    Data += System.Text.Encoding.UTF8.GetString(buffer_package, 0, size);
                }
                catch(IOException)
                {
                    return null;
                }
            }

            return Data;
                
        }


        /*  Отправка данных клиенту, принимает уже готовые к отправке данные (CreatePackage) */
        public static void Write(Client client, byte[] data)
        {
            try
            {
                client.stream.Write(data, 0, data.Length);
            }
            catch(IOException)
            {
                client.status_connect = false;
            }
           
        }

     
        
        /* Создает пакет вида SIZE+MESSAGE для отправки клиенту 
         * size - размер данных
         * message - данные */
        public static byte[] CreatePackage(int size, string message)
        {
            string len = message.Length.ToString();
            string ready_message = String.Empty;

            char[] len_c = { '0', '0', '0', '0', '0' };
            for (int i = 0; i < len.Length; i++)
            {
                len_c[len_c.Length - (i+1)] = len[i];
            }

            string len_s = new string(len_c);
            ready_message = len_s + message;
            return Encoding.ASCII.GetBytes(ready_message);
        }

        
  
    }
}
