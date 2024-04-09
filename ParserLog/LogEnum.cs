using System.Collections;

namespace ParserLog;

public class LogEnum(IEnumerator<Log> logs) : IEnumerable<Log>
{
    private readonly IEnumerator<Log> _logs = logs;

    public IEnumerator<Log> GetEnumerator()
    {
        return (IEnumerator<Log>)_logs;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}