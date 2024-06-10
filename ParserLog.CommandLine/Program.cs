using Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParserLog;
using ParserLog.CommandLine;
using ParserLog.CommandLine.Services;
using System.CommandLine;
using System.Net;

internal class Program
{
    private static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddTransient<ILogger, Logger>();
        builder.Services.AddSingleton<IFileService, FileService>();
        builder.Services.AddSingleton<ILogProcessor, LogProcessor>();

        using IHost host = builder.Build();

        host.Run();

        ILogProcessor logProcessor = host.Services.GetRequiredService<ILogProcessor>();

        var rootCommand = GetRootCommand(logProcessor);

        rootCommand.Invoke(args);
    }

    private static RootCommand GetRootCommand(ILogProcessor logProcessor)
    {
        var fileLogOption = new Option<FileInfo>(
            name: "--file-log",
            description: "путь к файлу с логами");

        var fileOutputOption = new Option<FileInfo>(
            name: "--file-output",
            getDefaultValue: () => new FileInfo("output.txt"),
            description: "путь к файлу с результатом");

        var timeStartOption = new Option<DateOnly?>(
            name: "--time-start",
            description: "нижняя граница временного интервала");
        var timeEndOption = new Option<DateOnly?>(
            name: "--time-end",
            description: "верхняя граница временного интервала.");
        var addressStartOption = new Option<IPAddress?>(
            name: "--address-start",
            description: "нижняя граница диапазона адресов, необязательный параметр, по умолчанию обрабатываются все адреса");

        var addressMaskOption = new Option<int?>(
            name: "--address-mask",
            description: "маска подсети, задающая верхнюю границу диапазона десятичное число. Необязательный параметр. В случае, если он не указан, обрабатываются все адреса, начиная с нижней границы диапазона. Параметр нельзя использовать, если не задан address-start\r\n");

        var rootCommand = new RootCommand("Sample app for System.CommandLine");

        rootCommand.AddOption(fileLogOption);
        rootCommand.AddOption(fileOutputOption);
        rootCommand.AddOption(timeStartOption);
        rootCommand.AddOption(timeEndOption);
        rootCommand.AddOption(addressStartOption);
        rootCommand.AddOption(addressMaskOption);

        rootCommand.SetHandler((fileLog, fileOutput, timeStart, timeEnd, addressStart, addressMask) =>
        {
            logProcessor.ProcessLogs(fileLog, fileOutput, timeStart, timeEnd, addressStart, addressMask);
        },
            fileLogOption, fileOutputOption, timeStartOption, timeEndOption, addressStartOption, addressMaskOption);
        return rootCommand;
    }

}