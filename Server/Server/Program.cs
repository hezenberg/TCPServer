using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Server
{   
    class Program
    {
        static string cfg_path = "setting.cfg";
        static string ip;
        static int port, max_conn;
        static void ReadCfgFileServer()
        {
            try
            {
                StreamReader sr = new StreamReader(cfg_path);
                while (!sr.EndOfStream)
                {
                    string[] splt_line = sr.ReadLine().Split(':');
                    switch (splt_line[0].Trim())
                    {
                        case "ip":
                            ip = splt_line[1].Trim();
                            break;
                        case "port":
                            port = Int32.Parse(splt_line[1].Trim());
                            break;
                        case "max_connection":
                            max_conn = Int32.Parse(splt_line[1].Trim());
                            break;
                    }

                }

                sr.Close();
            }
            catch(FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Config file not found, server used defolt setting."); Console.ResetColor();
                ip = "127.0.0.1"; port = 80; max_conn = 100;
            }
       
          
        }


        static void Main(string[] args)
        {
            ReadCfgFileServer();
            var server = new Server(ip, port, max_conn);
            
        }
    }
}
