namespace ParserLog;

public class AddressMaskCommand : Command
{
    private readonly ILogger _logger;

    public AddressMaskCommand(ILogger logger)
    {
        _logger = logger;
        Name = "--address-mask";
        IsRequired = false;
        Priority = 0;
    }

    public override IEnumerator<Log> Move(IEnumerator<Log> enumerator)
    {
        while (enumerator.MoveNext())
        {
            

            yield return enumerator.Current;
        }
    }
}

