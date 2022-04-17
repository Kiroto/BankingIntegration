using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BankingIntegration
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            IntegrationServer server = new IntegrationServer();
            server.Start();
            Console.WriteLine($"Listening at port {server.port}");
            server.NewLog += HandleServerLog;
            Console.ReadKey();
            server.Stop();
        }

        static void HandleServerLog(Log log)
        {
            switch (log.Severity)
            {
                case Log.LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case Log.LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case Log.LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    break;
            }
            Console.WriteLine(log);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
