using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

namespace ParserLog;

public class AddressStartCommand : Command
{
    private readonly ILogger _logger;

    public AddressStartCommand(ILogger logger)
    {
        _logger = logger;
        Name = "--address-start";
        IsRequired = false;
        Priority = 0;
    }

    public override IEnumerator<Log> Move(IEnumerator<Log> enumerator)
    {
        if (!IPAddress.TryParse(Value, out var iPAddress ))
        {
            _logger.Error($"{Value} no IP address");
            yield break;
        }

        while (enumerator.MoveNext())
        {
            if (!IPAddress.TryParse(enumerator.Current.IpAddress, out var enumeratorIPAddress))
            {
                _logger.Error($"{Value} no IP address");
                yield break;
            }

            if (!Validate(iPAddress, enumeratorIPAddress))
            {
                continue;
            } 

            yield return enumerator.Current;
        }
    }
    private bool Validate(IPAddress iPAddressStart, IPAddress enumeratorIPAddress)
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

}

