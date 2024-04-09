
namespace ParserLog;

public interface IFileService
{
    IEnumerator<Log> Read(string? path);
    IEnumerator<Log> Save(string? path, IEnumerator<Log> logs);
}
