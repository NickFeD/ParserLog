using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ParserLog;

internal class СommandHandler
{
    private readonly ILogger _log;
    private readonly Dictionary<string, Command> _nameToCommands = new();
    private readonly List<Command> _required = new();

    public СommandHandler(ILogger log)
    {
        _log = log;
        var file = new FileService(_log);

        List < Command > commands = 
            [
            new FileLogCommand(file,_log),
            new FileOutputCommand(file,_log), 
            new TimeStartCommand(_log),
            new TimeEndCommand(_log),
            new AddressStartCommand(_log),
            ];
        AddCommands(commands);
    }


    internal void Start(string[] args)
    {

        var list = new List<Command>();
        for (int i = 0; i < args.Length; i+=2)
        {
            var strCommand = args[i];
            if (!ValidateDetails(strCommand))
                return;
            var command = _nameToCommands[strCommand];
            command.Value = args[i+1];

            list.Add(command);
            if(command.IsRequired)
                _required.Remove(command);
        }
        list = list.OrderByDescending(c => c.Priority).ToList();

        IEnumerator<Log> enumerator = new List<Log>(0).GetEnumerator();
        foreach (var item in list)
        {
            enumerator = item.Move(enumerator);
        }
        while (enumerator.MoveNext())
        {
            _log.Info(enumerator.Current.IpAddress+" "+ enumerator.Current.DateTime);
        }


    }


    private bool ValidateDetails(string command)
    {
        if (!command.Substring(0, 2).Equals("--"))
        {
            _log.Error($"Сommands start with --");
            return false;
        }
        if (!_nameToCommands.ContainsKey(command))
        {
            _log.Error($"Сommand {command} not found");
            return false;
        } 
        return true;
    }


    private void AddCommands(List<Command> commands)
    {
        foreach (var command in commands)
        {
            _nameToCommands.Add(command.Name, command);
            if(command.IsRequired)
                _required.Add(command);
        }
    }
}
