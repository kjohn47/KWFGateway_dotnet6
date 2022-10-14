using KWFGateway.Extensions;
using KWFGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<FaviconMiddleware>();
app.UseMiddleware<GatewayMiddleware>();

app.UseHttpsRedirection();

app.Run();