using ParserLog;
using System.Collections.Frozen;

internal class Program
{

    
    private static void Main(string[] args)
    {
        var logger = new Logger();
        var commandHandler = new СommandHandler(logger);

        commandHandler.Start(args);
        
    }
}