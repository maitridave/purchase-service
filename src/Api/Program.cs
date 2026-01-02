using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using FastEndpoints;
using FastEndpoints.Swagger;
using AI.PurchaseService.Domain.Data;
using AI.PurchaseService.Domain.Mappings;
using AI.PurchaseService.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add FastEndpoints and Swagger
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

// Add Entity Framework
builder.Services.AddDbContext<PurchaseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(PurchaseMappingProfile));

// Register custom services including MassTransit
builder.Services.AddServiceExtension(builder.Configuration);

var app = builder.Build();
app.Environment.EnvironmentName = "Development";
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseFastEndpoints(c =>
{
    c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    c.Serializer.Options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    c.Serializer.Options.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
});

app.Run();