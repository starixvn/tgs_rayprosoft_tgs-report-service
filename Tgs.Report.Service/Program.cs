using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.MessageQueue;
using Tgs.Report.Core;
using Tgs.Report.ViewModel;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Services
    .AddHttpClient()
    .AddReportRepository((sp, optionsBuilder) =>
    {
        var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("Tgs");
        _ = optionsBuilder.UseNpgsql(connectionString);
    })
    .AddReportCore()
    .AddTgsLog()
    .AddGameProvidersPluggableServerClient((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        options.BaseUrl = config.GetValue<Uri>("Report:GameProvidersPluggableBaseUrl");
    });

builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddBackOfficeApplicationServices();

//builder.Logging
//    .AddOpenTelemetry(options =>
//    {
//        options.IncludeFormattedMessage = true;
//        options.ParseStateValues = true;
//        _ = options.AddFluentdForwardExporter(fluentOptions =>
//        {
//            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
//            fluentOptions.Host = nodeIp;
//            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
//            fluentOptions.UseMessagePack();
//        });
//    });

//builder.Services
//    .AddOptions<OtlpExporterOptions>()
//    .Configure<IConfiguration>((options, conifg) =>
//    {
//        var nodeIp = conifg.GetValue<string>("NODE_IP");

//        if (!string.IsNullOrWhiteSpace(nodeIp))
//            options.Endpoint = new Uri($"http://{nodeIp}:4317");

//        conifg.GetSection("Opentelemetry:Tracing").Bind(options);
//    });

builder.Services
    .AddOpenTelemetryTracing(builder => builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
        })
        .AddProfileViewExporter()
        .AddOtlpExporter())
    .Configure<AspNetCoreInstrumentationOptions>(options =>
    {
        options.Filter = httpContext =>
        {
            return !httpContext.Request.Path.HasValue
                || !httpContext.Request.Path.StartsWithSegments("/swagger")
                && !httpContext.Request.Path.StartsWithSegments("/healthz")
                && !httpContext.Request.Path.StartsWithSegments("/profiler/")
                && !new[]
                {
                    "html",
                    "ico",
                    "png",
                    "jpg"
                }.Any(
                    ext => httpContext.Request.Path.Value.EndsWith($".{ext}"));
        };
    });
builder.Services.AddProfileViewer();

builder.Services
    .AddMessageQueue()
    .AddNatsMessageQueue(natsConfig =>
    {
        natsConfig
            .ConfigQueueOptions((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                options.Url = config.GetValue<string>("NatsSetting:Urls");
            });
    })
    .ClearAllExchanges()
    .AddNatsGlobPatternExchange("*");

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionFeature?.Error != null)
        {
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ExceptionHandler");

            logger.LogError(exceptionFeature.Error, exceptionFeature.Error.Message);

            var messageSender = app.Services.GetRequiredService<IMessageSender>();

            await messageSender.PublishAsync("report.error",
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ReportErrorMessage(
                        $"{app.Environment}.report: {exceptionFeature.Error.Message}"))))
                .ConfigureAwait(false);
        }
    }
});

// Configure the HTTP request pipeline.
app.UseSwaggerUI();

app.MapControllers();
app.MapSwagger();
app.MapHealthChecks("/healthz");
app.MapProfileViewer();

app.Run();