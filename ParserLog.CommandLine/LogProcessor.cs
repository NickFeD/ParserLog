using Core;
using System.Net;

namespace ParserLog.CommandLine;

public class LogProcessor : ILogProcessor
{
    private ILogger _logger;
    private IFileService _fileService;

    public LogProcessor(ILogger logger, IFileService fileService)
    {
        _logger = logger;
        _fileService = fileService;
    }

    public IEnumerable<Log> ProcessLogs(FileInfo fileLog, FileInfo fileOutput, DateOnly? timeStart, DateOnly? timeEnd, IPAddress? addressStart, int? addressMask)
    {
        var enumerator = FileLog(fileLog);
        if (timeStart is not null)
            enumerator = TimeStart(timeStart.Value, enumerator);

        if (timeEnd is not null)
            enumerator = TimeEnd(timeEnd.Value, enumerator);
        if (addressStart is not null)
            enumerator = AddressFilter(addressStart, addressMask, enumerator);

        FileOutput(fileOutput, enumerator);
        return enumerator;
    }

    public IEnumerable<Log> FileLog(FileInfo file)
    {
        _logger.Info("File check");

        return _fileService.Read(file);
    }

    public void FileOutput(FileInfo file, IEnumerable<Log> enumerator)
    {
        _logger.Info("File loading check");
        _fileService.Save(file, enumerator);
    }

    public IEnumerable<Log> TimeStart(DateOnly dateStart, IEnumerable<Log> logs)
        => logs.Where(l => l.DateTime.Date >= dateStart.ToDateTime(new TimeOnly()));

    public IEnumerable<Log> TimeEnd(DateOnly dateEnd, IEnumerable<Log> logs)
        => logs.Where(l => l.DateTime < dateEnd.ToDateTime(new TimeOnly()));

    public IEnumerable<Log> AddressFilter(IPAddress addressStart, int? addressMask, IEnumerable<Log> logs)
    {

        if (addressMask is not null)
        {
            string binaryAddressStart = ToBinary(addressStart, addressMask.Value);
            return logs.Where(l => ToBinary(l.IpAddress, addressMask.Value) == binaryAddressStart);
        }

        return logs.Where(l => Validate(addressStart, l.IpAddress));
    }

    private static bool Validate(IPAddress iPAddressStart, IPAddress enumeratorIPAddress)
    {
        var bytesIPAddress = iPAddressStart.GetAddressBytes();
        var bytesEnumeratorIPAddress = enumeratorIPAddress.GetAddressBytes();
        for (int i = 0; i < bytesIPAddress.Length; i++)
        {
            if (bytesIPAddress[i] == bytesEnumeratorIPAddress[i])
            {
                continue;
            }
            else if (bytesIPAddress[i] < bytesEnumeratorIPAddress[i])
            {
                return true;
            }

            return false;
        }
        return true;
    }

    private static string ToBinary(IPAddress ipAddress, int addressMask)
    {
        byte[] bytes = ipAddress.GetAddressBytes();
        string binaryString = string.Empty;
        foreach (byte b in bytes)
        {
            binaryString += Convert.ToString(b, 2).PadLeft(8, '0');
        }
        return binaryString.Substring(0, addressMask);
    }
}
