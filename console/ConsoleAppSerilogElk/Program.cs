using Bogus;
using Serilog;
using Serilog.Events;
using System;
using System.Threading;

namespace ConsoleAppSerilogElk
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithAssemblyName()
                .Enrich.WithAssemblyVersion()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .Enrich.WithMemoryUsage()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .WriteTo.DurableHttpUsingFileSizeRolledBuffers(
                    requestUri: "http://localhost:5000",
                    batchFormatter: new Serilog.Sinks.Http.BatchFormatters.ArrayBatchFormatter(),
                    bufferBaseFileName: $"C:\\Temp\\elk-serilog\\Buffer-{AppDomain.CurrentDomain.FriendlyName}"
                )
                .WriteTo.Console()
                .CreateLogger()
                .ForContext<Program>();



            logger.Warning("Logging warning");

            logger.Information("Logging information");

            logger.Error("Logging error");

            //https://github.com/serilog/serilog/wiki/Formatting-Output
            logger.Verbose("Logging objects {@dto}", new DtoGenerator().Generate());

            Log.CloseAndFlush();

            Thread.Sleep(3000);
        }
    }
    public class Dto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? Dob { get; set; }
    }
    public class DtoGenerator
    {
        private readonly Faker<Dto> faker;

        public DtoGenerator()
        {
            faker = new Faker<Dto>()
                .RuleFor(x => x.Id, faker => faker.Random.Number(1, 100))
                .RuleFor(x => x.Name, faker => faker.Name.LastName())
                .RuleFor(x => x.Dob, faker => faker.Date.Past());
        }

        public Dto Generate()
        {
            return faker.Generate();
        }
    }
}
