using Core;
using System.Net;

namespace Core
{
    public interface ILogProcessor
    {
        IEnumerable<Log> ProcessLogs(FileInfo fileLog, FileInfo fileOutput, DateOnly? timeStart, DateOnly? timeEnd, IPAddress? addressStart, int? addressMask);
    }
}