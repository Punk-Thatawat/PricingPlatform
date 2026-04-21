using Extensions.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.AddPlatformObservability("ApiGateway");
builder.Services.AddOpenApi();
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ApiGateway v1");
        options.SwaggerEndpoint("/openapi/pricing/v1.json", "Pricing v1");
        options.SwaggerEndpoint("/openapi/rules/v1.json", "Rule v1");
        options.RoutePrefix = "swagger";
    });
}

app.MapGet("/", () => Results.Ok(new
{
    service = "ApiGateway",
    routes = new[]
    {
        "/api/pricing/*",
        "/api/rules/*"
    }
}));

app.MapPlatformObservability();
app.MapReverseProxy();

app.Run();
