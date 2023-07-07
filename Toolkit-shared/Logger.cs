using System.Text;
using static Toolkit_Shared.ConsoleEscapeCodes;

namespace Toolkit_Shared
{
    public enum LogOption : int
    {
        NONE    = 0,
        INFO    = (1 << 0),
        WARNING = (1 << 1),
        ERROR   = (1 << 2)
    }

    public class ConsoleEscapeCodes
    {
        public static readonly string NEWLINE     = Environment.NewLine;
        public static readonly string NORMAL      = "\x1b[39m";
        public static readonly string RED         = "\x1b[91m";
        public static readonly string GREEN       = "\x1b[92m";
        public static readonly string YELLOW      = "\x1b[93m";
        public static readonly string BLUE        = "\x1b[94m";
        public static readonly string MAGENTA     = "\x1b[95m";
        public static readonly string CYAN        = "\x1b[96m";
        public static readonly string GREY        = "\x1b[97m";
        public static readonly string BOLD        = "\x1b[1m";
        public static readonly string NOBOLD      = "\x1b[22m";
        public static readonly string UNDERLINE   = "\x1b[4m";
        public static readonly string NOUNDERLINE = "\x1b[24m";
        public static readonly string REVERSE     = "\x1b[7m";
        public static readonly string NOREVERSE   = "\x1b[27m";
    }

    public class Logger
    {
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        
        public string logFilePath;
        public bool logToFile;
        public LogOption options;

        public Logger(string logFilePath, bool logToFile = true)
        {
            this.logFilePath = logFilePath;
            this.logToFile = logToFile;

            options |= LogOption.INFO;
            options |= LogOption.WARNING;
            options |= LogOption.ERROR;
        }

        public Logger(string logFilePath, bool logToFile, LogOption options)
        {
            this.logFilePath = logFilePath;
            this.logToFile = logToFile;

            this.options = options;
        }

        public bool HasLogOption(LogOption option)
        {
            return (options & option) == 0;
        }

        public static bool HasLogOption(LogOption options, LogOption option)
        {
            return (options & option) == 0;
        }

        public string GetCurrentLocalTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        public string GetCurrentUtcTime()
        {
            return DateTime.UtcNow.ToString("HH:mm:ss");
        }

        public void Write(string message)
        {
            Console.Write(message);

            if (logToFile)
                LogToFile(message);
        }

        public void WriteLine(string message)
        {
            Console.WriteLine(message);
            
            if (logToFile)
                LogToFile(message);
        }

        public void Info(string message)
        {
            if (HasLogOption(LogOption.INFO))
                return;

            string time = GetCurrentLocalTime();
            string _message = $"[{time}] [{Thread.CurrentThread.Name}/INFO]: {message}";
            Console.WriteLine($"{NORMAL}{_message}");

            if (logToFile)
                LogToFile(_message);
        }

        public void Warning(string message)
        {
            if (HasLogOption(LogOption.WARNING))
                return;

            string time = GetCurrentLocalTime();
            string _message = $"[{time}] [{Thread.CurrentThread.Name}/WARNING]: {message}";
            Console.WriteLine($"{YELLOW}{_message}");

            if (logToFile)
                LogToFile(_message);
        }

        public void Error(string message)
        {
            if (HasLogOption(LogOption.ERROR))
                return;

            string time = GetCurrentLocalTime();
            string _message = $"[{time}] [{Thread.CurrentThread.Name}/ERROR]: {message}";
            Console.WriteLine($"{RED}{_message}");

            if (logToFile)
                LogToFile(_message);
        }

        private void LogToFile(string message)
        {
            try
            {
                locker.EnterWriteLock();
                using (var file = File.OpenWrite(logFilePath)) 
                {
                    file.Seek(0, SeekOrigin.End);
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message + '\n');
                    file.Write(messageBytes, 0, messageBytes.Length);
                }
            }
            finally { locker.ExitWriteLock(); }
        }
    }
}
