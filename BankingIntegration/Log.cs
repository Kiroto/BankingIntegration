using System;
using System.Collections.Generic;
using System.Text;

namespace BankingIntegration
{
    class Log
    {
        public enum LogSeverity
        {
            Info,
            Warning,
            Error
        }

        public enum LogSource
        {
            Core,
            InternetBanking,
            CashierApp,
            Self
        }

        public LogSeverity Severity = LogSeverity.Info;
        public LogSource Source = LogSource.Self;
        public String SourceName;
        public String Message;
        public DateTime time = DateTime.UtcNow;

        public Log(String message)
        {
            Message = message;
        }

        public Log(String message, LogSeverity severity)
        {
            Message = message;
            Severity = severity;
        }

        public Log(String message, LogSource source)
        {
            Message = message;
            Source = source;
        }

        public Log(String message, LogSource source, LogSeverity severity)
        {
            Message = message;
            Source = source;
            Severity = severity;
        }

        override public string ToString()
        {
            return $"[{Source}] {Message}";
        }
    }
}
