using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.MessageQueue;
using Tgs.Report.ViewModel;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry();

builder.Configuration.AddJsonFile("appsettings.json",
        optional: true,
        reloadOnChange: true);

builder.WebHost.UseUrls($"http://0.0.0.0:{builder.Configuration["HostingPort"]}");

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

        options.BaseUrl = config.GetValue<Uri>("Report:GameProvidersPluggableBaseUrl")!;
    })
    .AddPlayerServiceClient((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        options.BaseUrl = config.GetValue<Uri>("Report:PlayerServiceBaseUrl")!;
    })
    .AddBrandServiceClient((options, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        options.BrandCertificateBaseUrl = config.GetValue<Uri>("Report:BrandCertificateUrl")!;
    })
    .AddMemoryCache();

builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("Otel:NodeIp");
            fluentOptions.Host = nodeIp!;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("Otel:ServiceName")!;
            fluentOptions.UseMessagePack();
        });
    });

builder.Services
    .AddOptions<OtlpExporterOptions>()
    .Configure<IConfiguration>((options, conifg) =>
    {
        var nodeIp = conifg.GetValue<string>("NODE_IP");

        if (!string.IsNullOrWhiteSpace(nodeIp))
        {
            options.Endpoint = new($"http://{nodeIp}:4317");
        }

        conifg.GetSection("Opentelemetry:Tracing").Bind(options);
    });

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(_ => _.AddService(builder.Configuration.GetValue<string>("Otel:ServiceName")))
    .WithTracing(tracingBuilder => tracingBuilder
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddAspNetCoreInstrumentation(options =>
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
        })
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
        })
        .AddProfileViewExporter()
        .AddOtlpExporter(options => options.Endpoint = builder.Configuration.GetValue<Uri>("Otel:Collector")));

builder.Services.AddProfileViewer();

builder.Services
    .AddMessageQueue()
    .AddNatsMessageQueue(natsConfig =>
    {
        natsConfig
            .ConfigQueueOptions((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                options.Url = config.GetConnectionString("Nats")!;
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

app.UseRequestLocalization(option =>
{
    var supportedCultures = new[]
    {
        "en-US",
        "zh-CN"
    };

    _ = option
        .SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    option.ApplyCurrentCultureToResponseHeaders = true;
});

SentrySdk.CaptureMessage($"Started result ReportService at {DateTime.Now.ToString()}");

app.Run();