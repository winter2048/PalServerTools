using LiteDB;
using PalServerTools.Models;

namespace PalServerTools.Data
{
    public class LogService
    {
        private LiteDatabase _db;
        private ILiteCollection<LogMedel> _logsCollection;

        public LogService(LiteDatabase db)
        {
            _db = db;
            _logsCollection = _db.GetCollection<LogMedel>("logs");
        }

        public void AddLog(LogMedel log)
        {
            _logsCollection.Insert(log);
        }

        public IEnumerable<LogMedel> GetAllLogs(LogLevel logLevel = LogLevel.None, string? keywords = null, EventId? eventId = null)
        {
            var logs = _logsCollection.FindAll();
            if (logLevel != LogLevel.None)
            {
                logs = logs.Where(p => p.LogLevel == logLevel);
            }
            if (eventId != null)
            {
                logs = logs.Where(p => p.EventId == eventId.Value.Id);
            }
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                logs = logs.Where(p => !string.IsNullOrWhiteSpace(p.Message) && p.Message.Contains(keywords));
            }
            return logs.OrderByDescending(p => p.Timestamp);
        }

        public LogMedel GetLogById(string id)
        {
            return _logsCollection.FindById(id);
        }

        public IEnumerable<LogMedel> GetLogsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _logsCollection.Find(x => x.Timestamp >= startDate && x.Timestamp <= endDate);
        }

        public void DeleteLog(string id)
        {
            _logsCollection.Delete(id);
        }

        public void DeleteAllLogs()
        {
            _logsCollection.DeleteAll();
        }

        public long GetTotalLogCount()
        {
            return _logsCollection.Count();
        }

        public IEnumerable<LogMedel> SearchLogs(string keyword)
        {
            return _logsCollection.Find(x => x.Message.Contains(keyword));
        }
    }
}
