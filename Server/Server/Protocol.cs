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
        public static int size_data_package = 10, buffer_size = 256;


        public static int UnpackingPackage(NetworkStream stream)
        {
            int expected_size = 0;
            byte[] data_pack = new byte[size_data_package];
            byte[] data_pack_c = new byte[size_data_package];

            stream.Read(data_pack, 0, size_data_package);
            expected_size = (Int32)(BitConverter.ToInt16(data_pack_c, 0));

            data_pack = null;
            data_pack_c = null;

            return expected_size;
        }

  
    }
}
