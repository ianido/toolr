
using toolr.common;

namespace dbmigration.Process
{
    public interface IReportGenerator
    {
        void Report(string message, EventType logType = EventType.None);

        void Init();

        void End();
    }
}