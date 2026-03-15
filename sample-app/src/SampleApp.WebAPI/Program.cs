using NLog.Extensions.Logging;
using SampleApp.Application;
using SampleApp.Domain;
using SampleApp.Infrastructure;
using SampleApp.WebAPI;

var builder = WebApplication.CreateBuilder(args);

//Configure liogging provider
builder.Logging.ClearProviders();
builder.Logging.AddNLog(new NLogProviderOptions
{
    IncludeScopes = true,
    CaptureMessageProperties = true,
    CaptureMessageTemplates = true,
    RemoveLoggerFactoryFilter = true
});

NLogBootstrap.Configure(builder.Configuration, builder.Environment);

builder.Services.AddScoped<RequestLoggingMiddleware>();

builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

try
{
    app.Run();
}
finally
{
    NLog.LogManager.Shutdown();
}
