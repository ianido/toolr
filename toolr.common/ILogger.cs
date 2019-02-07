namespace toolr.common
{
    public interface ILogger
    {
        void Log(string message, EventType type = EventType.None);
    }

    public enum EventType
    {
        Error, Info, Warning, None
    }
}