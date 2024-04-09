
using System.Globalization;

namespace ParserLog;

public class TimeEndCommand : Command
{
    private readonly ILogger _logger;

    public TimeEndCommand(ILogger logger)
    {
        _logger = logger;
        Name = "--time-end";
        IsRequired = false;
        Priority = 2;
    }

    public override IEnumerator<Log> Move(IEnumerator<Log> enumerator)
    {
        while (enumerator.MoveNext())
        {
            if (!DateTime.TryParseExact(Value, "dd.MM.yyyy",
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out var date))
            {
                _logger.Error($"{Value} the datetime could not be processed, the datetime should look like dd.MM.yyyy");
                continue;
            }

            if (enumerator.Current.DateTime > date)
            {
                _logger.Info($"the contents of the file from the date {enumerator.Current.DateTime} is not being viewed");
                yield break;
            }

            yield return enumerator.Current;
        }
    }

}

