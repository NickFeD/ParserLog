
namespace ParserLog;

public interface IFileService
{
    IEnumerator<Log> Read(FileInfo file);
    void Save(FileInfo file, IEnumerator<Log> logs);
}
