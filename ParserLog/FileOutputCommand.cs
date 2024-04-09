using System;

namespace ParserLog;

public class FileOutputCommand : Command
{
    private readonly IFileService _fileService;
    private readonly ILogger _logger;

    public FileOutputCommand(IFileService fileService, ILogger logger)
    {
        _fileService = fileService;
        _logger = logger;
        Name = "--file-output";
        IsRequired = false;
        Priority = -1;
    }

    public override IEnumerator<Log> Move(IEnumerator<Log> enumerator)
    {
        _logger.Info("File check");

        enumerator = _fileService.Save(Value,enumerator);
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
    }

}
