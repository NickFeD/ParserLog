
using System.Globalization;

namespace ParserLog;

public class TimeStartCommand : Command
{
    private readonly ILogger _logger;

    public TimeStartCommand( ILogger logger)
    {
        _logger = logger;
        Name = "--time-start";
        IsRequired = false;
        Priority = 1;
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
                yield return enumerator.Current;
            } 
            
            
        }
    }

}

