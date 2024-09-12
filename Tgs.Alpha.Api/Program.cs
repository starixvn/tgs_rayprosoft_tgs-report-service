using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry();

// Add services to the container.
builder.Services
    .AddHttpClient()
    .AddReportRepository((sp, optionsBuilder) =>
    {
        var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("Tgs");
        _ = optionsBuilder.UseNpgsql(connectionString);
    })
    .AddReportCore()
    //.AddTgsLog()
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");
app.MapHealthChecks("/");

SentrySdk.CaptureMessage($"Started result Alpha.Api Service at {DateTime.Now.ToString()}");


app.Run();
