using Core;
using System.Globalization;
using System.Net;
using System.Text;

namespace ParserLog.CommandLine.Services;

public class FileService(ILogger logger) : IFileService
{
    private readonly ILogger _logger = logger;

    public IEnumerable<Log> Read(FileInfo file)
    {

        const int bufferSize = 16 * 1024 * 1024; // 32Mb

        FileStream fs = new FileStream(file.FullName, FileMode.Open,
            FileAccess.Read, FileShare.Read, bufferSize, true);

        using StreamReader streamReader = new StreamReader(fs, Encoding.UTF8, true, bufferSize);

        string? stringLog;

        while ((stringLog = streamReader.ReadLine()) != null)
        {
            var strings = stringLog.Split(':', 2);
            string datetime = strings[1];
            string strIpAddress = strings[0];
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
            if (!IPAddress.TryParse(strIpAddress, out var ipAddress))
            {
                _logger.Error($" failed to convert {strIpAddress} to an IP address");
                continue;
            }

            yield return new Log()
            {
                DateTime = date,
                IpAddress = ipAddress,
            };
        }
    }

    public void Save(FileInfo file, IEnumerable<Log> logs)
    {
        Dictionary<IPAddress, int> keyValuePairs = [];
        HashSet<IPAddress> keys = [];

        foreach (var log in logs)
        {
            var ipAddress = log.IpAddress;

            if (!keyValuePairs.ContainsKey(ipAddress))
            {
                keyValuePairs.Add(ipAddress, 0);
                keys.Add(ipAddress);
            };
            keyValuePairs[ipAddress] += 1;
        }
        _logger.Info(keys.Count.ToString() + " lines save file");

        IEnumerable<string> SaveData()
        {
            StringBuilder stringBuilder = new StringBuilder();
            int tick = 0;
            for (int i = 0; i < keys.Count; i++)
            {
                if (1024 * 64 > tick)
                {
                    stringBuilder.AppendLine(keys.ElementAt(i) + " " + keyValuePairs[keys.ElementAt(i)]);
                    keyValuePairs.Remove(keys.ElementAt(i));
                    tick += 1;
                    continue;
                }

                yield return stringBuilder.ToString();
                _logger.Info(i + " lines save file");
                tick = 0;
                stringBuilder.Clear();

            }
            yield return stringBuilder.ToString();
        }

        File.WriteAllLines(file.FullName, SaveData());
    }
}
