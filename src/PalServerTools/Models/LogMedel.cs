using AntDesign;
using System;

namespace PalServerTools.Models
{
    public class LogMedel
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public LogLevel LogLevel { get; set; }

        public int? EventId { get; set; }

        public string? EventName { get; set; }

        public string? Exception { get; set; }

        public LogMedel()
        {

        }

        public LogMedel(string? message, LogLevel level, EventId? eventId, Exception? exception)
        {
            Message = message;
            LogLevel = level;
            EventId = eventId?.Id;
            EventName = eventId?.Name;
            Exception = exception?.ToString();
        }

        public override string ToString()
        {
            return $"{Timestamp.ToString("yyyy/MM/dd HH:mm:ss")} [{LogLevel}] [{EventId}:{EventName}]: {Message} \r\n {Exception}";
        }
    }
}
