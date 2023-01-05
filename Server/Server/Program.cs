using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server("127.0.0.1", 8080, 100);
        }
    }
}
