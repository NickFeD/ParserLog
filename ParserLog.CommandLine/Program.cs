using ParserLog;
using System.CommandLine;
using System.Globalization;
using System.Net;

internal class Program
{
    private static async Task Main(string[] args)
    {
        ILogger logger = new Logger();
        IFileService fileService = new FileService(logger);

        var fileLogOption = new Option<FileInfo>(
            name: "--file-log",
            description: "The file to read log.");

        var fileOutputOption = new Option<FileInfo?>(
            name: "--file-output",
            description: "The file to read log.");

        var timeStartOption = new Option<DateOnly?>(
            name: "--time-start",
            description: "The file to read log.");
        var timeEndOption = new Option<DateOnly?>(
            name: "--time-end",
            description: "The file to read log.");
        var addressStartOption = new Option<IPAddress?>(
            name: "--address-start",
            description: "The file to read log.");
        var addressMaskOption = new Option<int?>(
            name: "--address-mask",
            description: "The file to read log.");

        var rootCommand = new RootCommand("Sample app for System.CommandLine");

        rootCommand.AddOption(fileLogOption);
        rootCommand.AddOption(fileOutputOption);
        rootCommand.AddOption(timeStartOption);
        rootCommand.AddOption(timeEndOption);
        rootCommand.AddOption(addressStartOption);
        rootCommand.AddOption(addressMaskOption);

        rootCommand.SetHandler((fileLog, fileOutput, timeStart, timeEnd, addressStart, addressMask) =>
        {
            var enumerator = FileLog(fileLog!, logger,fileService);
            enumerator = TimeStart(timeStart.Value, enumerator);
            enumerator = TimeEnd(timeEnd.Value, enumerator, logger);
            enumerator = AddressStart(addressStart!, enumerator,logger);
            enumerator = AddressMask(addressStart!, addressMask.Value, enumerator);
            FileOutput(fileOutput, enumerator, logger,fileService);
        },
            fileLogOption, fileOutputOption, timeStartOption, timeEndOption, addressStartOption, addressMaskOption);

        await rootCommand.InvokeAsync(args);
    }
    public static async IAsyncEnumerable<Log> FileLog(FileInfo? file,ILogger logger,IFileService fileService)
    {
        logger.Info("File check");
        if (file is null || !Path.Exists(file.FullName))
        {
            logger.Error($"{file.FullName} could not be determined as a file path");
            yield break;
        }

        var enumerator = fileService.Read(file);
        await foreach (var item in enumerator)
        {
            yield return item;
        }
    }

    public static void FileOutput(FileInfo file, IAsyncEnumerable<Log> logs,ILogger logger, IFileService fileService)
    {
        logger.Info("File loading check");
        fileService.Save(file, logs);
    }

    public static async IAsyncEnumerable<Log> TimeStart(DateOnly dateStart, IAsyncEnumerable<Log> logs)
    {
        await foreach (Log log in logs)
        {
            if (log.DateTime > dateStart.ToDateTime(new TimeOnly()))
            {
                yield return log;
            }
        }
    }

    public static async IAsyncEnumerable<Log> TimeEnd(DateOnly dateEnd, IAsyncEnumerable<Log> logs,ILogger logger)
    {
        await foreach (Log log in logs)
        {
            if (log.DateTime > dateEnd.ToDateTime(new TimeOnly()))
            {
                logger.Info($"the contents of the file from the date {log.DateTime} is not being viewed");
                yield break;
            }

            yield return log;
        }
    }

    public static async IAsyncEnumerable<Log> AddressStart(IPAddress addressStart, IAsyncEnumerable<Log> logs,ILogger logger)
    {
        await foreach (Log log in logs)
        {
            if (!IPAddress.TryParse(log.IpAddress, out var enumeratorIPAddress))
            {
                logger.Error($"{log.IpAddress} no IP address");
                yield break;
            }

            if (!Validate(addressStart, enumeratorIPAddress))
            {
                continue;
            }

            yield return log;
        }
    }

    public static async IAsyncEnumerable<Log> AddressMask(IPAddress iPAddressStart, int addressMask, IAsyncEnumerable<Log> logs)
    {
        string binaryAddressStart = ToBinary(iPAddressStart);
        await foreach (Log log in logs)
        {
            string binaryIpAddress = ToBinary(IPAddress.Parse(log.IpAddress));
            if (binaryIpAddress.Substring(0, addressMask) == binaryAddressStart.Substring(0, addressMask))
            {
                yield return log;
            }
        }
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

    static string ToBinary(IPAddress ipAddress)
    {
        byte[] bytes = ipAddress.GetAddressBytes();
        string binaryString = string.Empty;
        foreach (byte b in bytes)
        {
            binaryString += Convert.ToString(b, 2).PadLeft(8, '0');
        }
        return binaryString;
    }
}