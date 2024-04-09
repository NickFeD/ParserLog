
namespace ParserLog;

public class FileLogCommand : Command
{
    private readonly IFileService _fileService;
    private readonly ILogger _logger;

    public FileLogCommand(IFileService fileService, ILogger logger)
    {
        _fileService = fileService;
        _logger = logger;
        Name = "--file-log";
        IsRequired = true;
        Priority = int.MaxValue;
    }

    public override IEnumerator<Log> Move(IEnumerator<Log> enumerator)
    {
        _logger.Info("File check");
        if (!Path.Exists(Value))
        {
            _logger.Error($"{Value} could not be determined as a file path");
            yield break;
        }

        enumerator = _fileService.Read(Value);
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
    }

}
