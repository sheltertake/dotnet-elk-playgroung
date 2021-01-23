# dotnet-elk-playgroung

Playground repository to learn serilog + elk integration.

 - sink: https://github.com/FantasticFiasco/serilog-sinks-http
 - sample app: https://github.com/FantasticFiasco/serilog-sinks-http-sample-dotnet-core
 - elk compose: https://github.com/sheltertake/docker-elk
   - inherit from https://github.com/deviantony/docker-elk without paid features 

## 1 - console application + serilog + elk

```cmd
dotnet new console -n ConsoleAppSerilogElk
```

 - enrichers
 - levels
 - sink
 - formatter
 - temp durable json
 - serilog formatting output

```csharp
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
```

 - log produced

```json
{
  "_index": "logstash-2021.01.23-000001",
  "_type": "_doc",
  "_id": "A-gRL3cBciHruFKnlLA1",
  "_version": 1,
  "_score": null,
  "_source": {
    "RenderedMessage": "Logging objects Dto { Id: 72, Name: \"Heathcote\", Dob: 10/22/2020 11:58:13 }",
    "@version": "1",
    "Timestamp": "2021-01-23T12:46:51.7549680+01:00",
    "MessageTemplate": "Logging objects {@dto}",
    "host": "gateway",
    "port": 37958,
    "Level": "Verbose",
    "Properties": {
      "EnvironmentUserName": "********",
      "ProcessName": "ConsoleAppSerilogElk",
      "SourceContext": "ConsoleAppSerilogElk.Program",
      "ProcessId": 20704,
      "MachineName": "******",
      "ThreadId": 1,
      "MemoryUsage": 9585904,
      "dto": {
        "Id": 72,
        "_typeTag": "Dto",
        "Name": "Heathcote",
        "Dob": "2020-10-22T11:58:13.6140770+02:00"
      },
      "AssemblyName": "ConsoleAppSerilogElk",
      "AssemblyVersion": "1.0.0.0"
    },
    "@timestamp": "2021-01-23T11:46:57.098Z"
  },
  "fields": {
    "Properties.dto.Dob": [
      "2020-10-22T09:58:13.614Z"
    ],
    "@timestamp": [
      "2021-01-23T11:46:57.098Z"
    ],
    "Timestamp": [
      "2021-01-23T11:46:51.754Z"
    ]
  },
  "sort": [
    1611402417098
  ]
}
```
## nuget packages

 - Serilog.Sinks.Http
 - Serilog.Sinks.Console

## elk without paid features

```cmd
git clone https://github.com/sheltertake/docker-elk.git
cd docker-elk
docker-compose up
```

 - navigate http://localhost:5601/ 

```text
The stack is pre-configured with the following privileged bootstrap user:

user: elastic
password: changeme
```

 - create index pattern logstash*


## logstash.conf

 - adding codec => json seems work. the buffer of logs are splitted


```yaml
input {
	beats {
		port => 5044
	}

	tcp {
		port => 5000
		codec => json
	}
}


output {
	elasticsearch {
		hosts => "elasticsearch:9200"
		user => "elastic"
		password => "changeme"
		ecs_compatibility => disabled
	}
}

```
 - serilog sink must be configured with this DurableHttpUsingFileSizeRolledBuffers. Otherwise doesn't work correctly 

```csharp
WriteTo.DurableHttpUsingFileSizeRolledBuffers(
    requestUri: "http://localhost:5000",
    batchFormatter: new Serilog.Sinks.Http.BatchFormatters.ArrayBatchFormatter()
)
```

## enrichers + logs in elasticsearch

## issues
 - docker run

 - the property message contain this text
```json
{"events":[{"Timestamp":"2021-01-23T10:47:12.8307313+01:00","Level":"Information","MessageTemplate":"Log information","RenderedMessage":"Log information","Properties":{"SourceContext":"ConsoleAppSerilogElk.Program"}}
```
 - this json is not valid.
 - error and warning doesn't sent
 - other logs sent not by me

```csharp
WriteTo.Http(
    requestUri: "http://localhost:5000",
    queueLimit: 1
)
```

 - try override serilog (?) batchFormatter

```csharp
WriteTo.Http(
    requestUri: "http://localhost:5000",
    batchFormatter: new Serilog.Sinks.Http.BatchFormatters.ArrayBatchFormatter(),
    // queueLimit: 1
)
```

```json
{
  "_index": "logstash-2021.01.23-000001",
  "_type": "_doc",
  "_id": "9Z65LncB82wj-0D53aNI",
  "_version": 1,
  "_score": null,
  "_source": {
    "host": "gateway",
    "port": 37332,
    "message": "[{\"Timestamp\":\"2021-01-23T11:10:48.4208840+01:00\",\"Level\":\"Warning\",\"MessageTemplate\":\"Log warning\",\"RenderedMessage\":\"Log warning\",\"Properties\":{\"SourceContext\":\"ConsoleAppSerilogElk.Program\"}}\r",
    "@version": "1",
    "@timestamp": "2021-01-23T10:11:08.636Z"
  },
  "fields": {
    "@timestamp": [
      "2021-01-23T10:11:08.636Z"
    ]
  },
  "sort": [
    1611396668636
  ]
}
```

 - try using 

```csharp
WriteTo.DurableHttpUsingFileSizeRolledBuffers(
    requestUri: "http://localhost:5000",
    batchFormatter: new Serilog.Sinks.Http.BatchFormatters.ArrayBatchFormatter()
)
```

 - json message seems correct with 3 elements

```json
{
  "_index": "logstash-2021.01.23-000001",
  "_type": "_doc",
  "_id": "lZ6_LncB82wj-0D5-Key",
  "_version": 1,
  "_score": null,
  "_source": {
    "host": "gateway",
    "port": 37348,
    "message": "[{\"Timestamp\":\"2021-01-23T11:17:26.6543296+01:00\",\"Level\":\"Warning\",\"MessageTemplate\":\"Log warning\",\"RenderedMessage\":\"Log warning\",\"Properties\":{\"SourceContext\":\"ConsoleAppSerilogElk.Program\"}},{\"Timestamp\":\"2021-01-23T11:17:26.7578957+01:00\",\"Level\":\"Information\",\"MessageTemplate\":\"Log information\",\"RenderedMessage\":\"Log information\",\"Properties\":{\"SourceContext\":\"ConsoleAppSerilogElk.Program\"}},{\"Timestamp\":\"2021-01-23T11:17:26.7595224+01:00\",\"Level\":\"Error\",\"MessageTemplate\":\"Log error\",\"RenderedMessage\":\"Log error\",\"Properties\":{\"SourceContext\":\"ConsoleAppSerilogElk.Program\"}}]",
    "@version": "1",
    "@timestamp": "2021-01-23T10:17:48.869Z"
  },
  "fields": {
    "@timestamp": [
      "2021-01-23T10:17:48.869Z"
    ]
  },
  "sort": [
    1611397068869
  ]
}
```

 - unescaped 

```json
[{
	"Timestamp": "2021-01-23T11:17:26.6543296+01:00",
	"Level": "Warning",
	"MessageTemplate": "Log warning",
	"RenderedMessage": "Log warning",
	"Properties": {
		"SourceContext": "ConsoleAppSerilogElk.Program"
	}
}, {
	"Timestamp": "2021-01-23T11:17:26.7578957+01:00",
	"Level": "Information",
	"MessageTemplate": "Log information",
	"RenderedMessage": "Log information",
	"Properties": {
		"SourceContext": "ConsoleAppSerilogElk.Program"
	}
}, {
	"Timestamp": "2021-01-23T11:17:26.7595224+01:00",
	"Level": "Error",
	"MessageTemplate": "Log error",
	"RenderedMessage": "Log error",
	"Properties": {
		"SourceContext": "ConsoleAppSerilogElk.Program"
	}
}]
```