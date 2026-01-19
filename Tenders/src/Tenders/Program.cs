using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Tenders.Application;
using Tenders.Infrastucture;
using Tenders.Web.Filters;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "[HH:mm:ss.fff] ";
    options.UseUtcTimestamp = false;
});

builder.Services
    .AddControllers(o =>
    {
        o.Filters.Add<DefaultExceptionFilterAttribute>();
    })
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddMemoryCache();

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);
    

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
