using System.Net;

namespace Core;

public class Log
{
    public required DateTime DateTime { get; set; }

    public required IPAddress IpAddress { get; set; }
}
