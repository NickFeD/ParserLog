using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ParserLog;

public class FileService(ILogger logger) : IFileService
{
    private readonly ILogger _logger = logger;

    public IEnumerator<Log> Read(string? path)
    {
        if (!File.Exists(path))
        {
            _logger.Error($"the file on the {path} was not found");
            yield break;
        }

        var enumerator = File.ReadLines(path).GetEnumerator();

        while (enumerator.MoveNext())
        { 
            var stringLog = enumerator.Current;

            var strings = stringLog.Split(':', 2);
            string datetime = strings[1];
            string ipAddress = strings[0];
             DateTime date;
            try
            {
                date = DateTime.ParseExact(datetime, "yyyy-MM-dd HH:mm:ss",
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None);
                
            }
            catch (Exception)
            {
                _logger.Error($"{stringLog} the datetime could not be processed, the datetime should look like yyyy-MM-dd HH:mm:ss");
                continue;
            }

            yield return new Log()
            {
                DateTime = date,
                IpAddress = ipAddress,
            };
        }
    }

    public IEnumerator<Log> Save(string? path, IEnumerator<Log> logs)
    {
        if (!Path.Exists(path))
        {
            _logger.Error($"{path} could not be determined as a file path");
            yield break;
        }

        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(path);
        }
        File.WriteAllLines(path, new LogEnum(logs).Select(l => $"{l.IpAddress}:{l.DateTime.ToString("yyyy-MM-dd HH:mm:ss")}"));
        
        while (logs.MoveNext())
        {
            yield return logs.Current;
        }
    }
}
