
using PalServerTools.Data;
using PalServerTools.Models;

namespace PalServerTools.Utils
{
    public class LiteDBLogger : ILogger
    {
        private readonly LogService _logService;
        public LiteDBLogger(LogService logService)
        {
            _logService = logService;
        }


        public bool IsEnabled(LogLevel logLevel)
        {
            return true; // Implement the required behavior here
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // Implement the required behavior here
            return null;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var logMessage = formatter(state, exception);
            var log = new LogMedel(logMessage, logLevel, eventId.Id == 0 ? LogEvent.System : eventId, exception);
            Console.WriteLine(log.ToString());
            _logService.AddLog(log);
        }
    }
}
