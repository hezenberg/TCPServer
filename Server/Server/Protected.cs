using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class Protected
    {
        public static double TimeLimitClient = 3000000; // 0.3 sec
        public static long BlockingTimeTicks = 50000000; // 5 sec
        public const string ReportBlocking = "00044You have been blocked for 3 seconds for spam";

        public static bool ClientIsBlocking(Client client)
        {

            if ((DateTime.Now.Ticks - client.ticks_start_blocking) < BlockingTimeTicks && client.ticks_start_blocking > 0)
            {
                return true;
            }                  
            else
            {
                client.ticks_start_blocking = 0;
                return false;
            }
        }

        public static bool ClientIsDDOS(Client client)
        {
            long now_ticks = DateTime.Now.Ticks;
            if (client.ticks_last_message != -1)
            {
                if ((now_ticks - client.ticks_last_message) < TimeLimitClient)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine("Client {0} has been blocking", client.GetIpClient()); Console.ResetColor();
                    client.ticks_start_blocking = now_ticks;
                    return true;
                }

                client.ticks_last_message = now_ticks;
                return false;
            }
            else
            {
                client.ticks_last_message = now_ticks;
                return false;
            }
        }

    }
}
