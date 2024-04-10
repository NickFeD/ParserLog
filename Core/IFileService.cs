
namespace ParserLog;

public interface IFileService
{
    IAsyncEnumerable<Log> Read(FileInfo file);
    void Save(FileInfo file, IAsyncEnumerable<Log> logs);
}
