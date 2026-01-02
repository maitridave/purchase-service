using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using CorrelationId;
using FastEndpoints;
using AI.PurchaseService.Shared;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var webAppBuilder = WebApplication.CreateBuilder(args);

webAppBuilder.Services.AddEndpointsApiExplorer();
webAppBuilder.Services.AddFastEndpoints(o => o.IncludeAbstractValidators = true);

webAppBuilder.Configuration.SetBasePath(webAppBuilder.Environment.ContentRootPath)
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{webAppBuilder.Environment.EnvironmentName}.json", optional: true);

webAppBuilder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

webAppBuilder.Logging.ClearProviders();
webAppBuilder.Host.UseSerilog((context, _, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    }
);


webAppBuilder.Services.AddControllers();

// Add OpenTelemetry for observability
webAppBuilder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(builder => builder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

webAppBuilder.Services.AddConfigurations(webAppBuilder.Configuration)
    .AddServiceExtension(webAppBuilder.Configuration)
    .AddAuthentication(webAppBuilder.Configuration);
var app = webAppBuilder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseCorrelationId();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(c =>
{
    c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    c.Serializer.Options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    c.Serializer.Options.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    c.Endpoints.Configurator = ep =>
    {
        ep.PostProcessors(Order.After, new GlobalErrorLogger());
    };
    c.Versioning.Prefix = "v";
    c.Versioning.DefaultVersion = 1;
    c.Versioning.PrependToRoute = true;
});
app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapHealthChecks("/purchase-service/api/v1/health/liveness");
app.MapHealthChecks("/purchase-service/api/v1/health/readiness");

app.Run();