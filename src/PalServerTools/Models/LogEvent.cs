namespace PalServerTools.Models
{
    public static class LogEvent
    {
        public static EventId System = new EventId(0, "System");
        public static EventId Operation = new EventId(1, "Operation");
        public static EventId Back = new EventId(2, "Back");
    }
}
