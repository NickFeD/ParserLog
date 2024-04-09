using ParserLog;
using System.CommandLine;
using System.Globalization;
using System.Net;

internal class Program
{
    private static void Main(string[] args)
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

        rootCommand.Invoke(args);
    }
    public static IEnumerator<Log> FileLog(FileInfo? file,ILogger logger,IFileService fileService)
    {
        logger.Info("File check");
        if (file is null || !Path.Exists(file.FullName))
        {
            logger.Error($"{file.FullName} could not be determined as a file path");
            yield break;
        }

        var enumerator = fileService.Read(file);
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
    }

    public static void FileOutput(FileInfo file, IEnumerator<Log> enumerator,ILogger logger, IFileService fileService)
    {
        logger.Info("File loading check");
        fileService.Save(file, enumerator);
    }

    public static IEnumerator<Log> TimeStart(DateOnly dateStart,IEnumerator<Log> enumerator)
    {
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.DateTime > dateStart.ToDateTime(new TimeOnly()))
            {
                yield return enumerator.Current;
            }
        }
    }

    public static IEnumerator<Log> TimeEnd(DateOnly dateEnd, IEnumerator<Log> enumerator,ILogger logger)
    {
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.DateTime > dateEnd.ToDateTime(new TimeOnly()))
            {
                logger.Info($"the contents of the file from the date {enumerator.Current.DateTime} is not being viewed");
                yield break;
            }

            yield return enumerator.Current;
        }
    }

    public static IEnumerator<Log> AddressStart(IPAddress addressStart, IEnumerator<Log> enumerator,ILogger logger)
    {
        while (enumerator.MoveNext())
        {
            if (!IPAddress.TryParse(enumerator.Current.IpAddress, out var enumeratorIPAddress))
            {
                logger.Error($"{enumerator.Current.IpAddress} no IP address");
                yield break;
            }

            if (!Validate(addressStart, enumeratorIPAddress))
            {
                continue;
            }

            yield return enumerator.Current;
        }
    }

    public static IEnumerator<Log> AddressMask(IPAddress iPAddressStart, int addressMask, IEnumerator<Log> enumerator)
    {
        string binaryAddressStart = ToBinary(iPAddressStart);
        while (enumerator.MoveNext())
        {
            string binaryIpAddress = ToBinary(IPAddress.Parse(enumerator.Current.IpAddress));
            if (binaryIpAddress.Substring(0, addressMask) == binaryAddressStart.Substring(0, addressMask))
            {
                yield return enumerator.Current;
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