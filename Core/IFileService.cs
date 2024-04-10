
namespace Core;

public interface IFileService
{
    IEnumerable<Log> Read(FileInfo file);
    void Save(FileInfo file, IEnumerable<Log> logs);
}
