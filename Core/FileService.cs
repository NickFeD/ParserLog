using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ParserLog;

public class FileService(ILogger logger) : IFileService
{
    private readonly ILogger _logger = logger;

    public IEnumerator<Log> Read(FileInfo file)
    {

        const int bufferSize = 32 * 1024 * 1024; // 32Mb

        FileStream fs = new FileStream(file.FullName, FileMode.Open,
            FileAccess.Read, FileShare.Read, bufferSize, true);

        using StreamReader streamReader = new StreamReader(fs, Encoding.UTF8, true, bufferSize);

        string? stringLog;

        while ((stringLog = streamReader.ReadLine()) != null)
        { 
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

    public void Save(FileInfo file, IEnumerator<Log> logs)
    {
        Dictionary<string, int> keyValuePairs = new();
        HashSet<string> keys = new HashSet<string>();
        
        while (logs.MoveNext())
        {
            var ipAddress = logs.Current.IpAddress;

            if (!keyValuePairs.ContainsKey(ipAddress))
            {
                keyValuePairs.Add(ipAddress, 0);
                keys.Add(ipAddress);
            };
            keyValuePairs[ipAddress] = +1;
        }
        List<string> strings = new(); 
        for (int i = 0; i < keys.Count; i++)
        {
            strings.Add($"{keys.ElementAt(i)} {keyValuePairs[keys.ElementAt(i)]}");
        }
        File.WriteAllLines(file.FullName, strings);
    }
}
