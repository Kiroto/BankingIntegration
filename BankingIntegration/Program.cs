using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BankingIntegration
{
    class Program
    {
        private static readonly log4net.ILog logger
               = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            IntegrationServer server = new IntegrationServer();
            logger.Info("HELLO THERE");
            server.NewLog += HandleServerLog;
            server.Start();
            Console.WriteLine($"Listening at port {server.port}");
            Console.ReadKey();
            server.Stop();
        }

        static void HandleServerLog(Log log)
        {
            string logtext = log.ToString();
            switch (log.Severity)
            {
                case Log.LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    logger.Info(logtext);
                    break;
                case Log.LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    logger.Warn(logtext);
                    break;
                case Log.LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.Error(logtext);
                    break;
                default:
                    break;
            }
            Console.WriteLine(log);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
