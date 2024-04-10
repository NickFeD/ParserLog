using Core;
using Moq;
using ParserLog.CommandLine;
using System.Net;

namespace ProgramTests;

public class LogProcessorTests
{

    [Fact]
    public void TestFileLog()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockFileService = new Mock<IFileService>();
        var testFile = new FileInfo("test.txt");
        var testLogs = new List<Log>
    {
        new Log
        {
            DateTime = DateTime.Now,
            IpAddress = IPAddress.Parse("127.0.0.1")
        }
    };
        mockFileService.Setup(fs => fs.Read(testFile)).Returns(testLogs);

        var logProcessor = new LogProcessor(mockLogger.Object, mockFileService.Object);

        // Act
        var result = logProcessor.FileLog(testFile);

        // Assert
        mockLogger.Verify(logger => logger.Info("File check"), Times.Once);
        mockFileService.Verify(fs => fs.Read(testFile), Times.Once);
        Assert.Equal(testLogs, result);
    }

    [Fact]
    public void TestFileOutput()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockFileService = new Mock<IFileService>();
        var testFile = new FileInfo("test.txt");
        var testLogs = new List<Log>
    {
        new Log
        {
            DateTime = DateTime.Now,
            IpAddress = IPAddress.Parse("127.0.0.1")
        }
    };
        var logProcessor = new LogProcessor(mockLogger.Object, mockFileService.Object);

        // Act
        logProcessor.FileOutput(testFile, testLogs);

        // Assert
        mockFileService.Verify(fs => fs.Save(testFile, testLogs), Times.Once);
    }

    [Fact]
    public void TestTimeStart()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockFileService = new Mock<IFileService>();
        var testDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var testLogs = new List<Log>
    {
        new Log
        {
            DateTime = DateTime.Now,
            IpAddress = IPAddress.Parse("127.0.0.1")
        },
        new Log
        {
            DateTime = DateTime.Now.AddDays(-2),
            IpAddress = IPAddress.Parse("127.0.0.1")
        }
    };
        var logProcessor = new LogProcessor(mockLogger.Object, mockFileService.Object);

        // Act
        var result = logProcessor.TimeStart(testDate, testLogs);

        // Assert
        Assert.Single(result);
        Assert.Equal(DateTime.Now.Date, result.First().DateTime.Date);
    }

    [Fact]
    public void TestTimeEnd()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockFileService = new Mock<IFileService>();
        var testDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var testLogs = new List<Log>
{
    new Log
    {
        DateTime = DateTime.Now,
        IpAddress = IPAddress.Parse("127.0.0.1")
    },
    new Log
    {
        DateTime = DateTime.Now.AddDays(2),
        IpAddress = IPAddress.Parse("127.0.0.1")
    }
};
        var logProcessor = new LogProcessor(mockLogger.Object, mockFileService.Object);

        // Act
        var result = logProcessor.TimeEnd(testDate, testLogs);

        // Assert
        Assert.Single(result);
        Assert.Equal(DateTime.Now.Date, result.First().DateTime.Date);
    }

    [Fact]
    public void TestAddressFilter_WithMask()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockFileService = new Mock<IFileService>();
        var testAddressStart = IPAddress.Parse("192.168.1.1");
        var testAddressMask = 24;
        var testLogs = new List<Log>
    {
        new Log
        {
            DateTime = DateTime.Now,
            IpAddress = IPAddress.Parse("192.168.1.2")
        },
        new Log
        {
            DateTime = DateTime.Now.AddDays(2),
            IpAddress = IPAddress.Parse("192.168.2.1")
        }
    };
        var logProcessor = new LogProcessor(mockLogger.Object, mockFileService.Object);

        // Act
        var result = logProcessor.AddressFilter(testAddressStart, testAddressMask, testLogs);

        // Assert
        Assert.Single(result);
        Assert.Equal("192.168.1.2", result.First().IpAddress.ToString());
    }

    [Fact]
    public void TestAddressFilter_WithoutMask()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockFileService = new Mock<IFileService>();
        var testAddressStart = IPAddress.Parse("192.168.1.1");
        var testLogs = new List<Log>
    {
        new Log
        {
            DateTime = DateTime.Now,
            IpAddress = IPAddress.Parse("192.168.1.2")
        },
        new Log
        {
            DateTime = DateTime.Now.AddDays(2),
            IpAddress = IPAddress.Parse("192.168.2.1")
        }
    };
        var logProcessor = new LogProcessor(mockLogger.Object, mockFileService.Object);

        // Act
        var result = logProcessor.AddressFilter(testAddressStart, null, testLogs);

        // Assert
        Assert.Equal(2, result.Count());
    }
}