using KWFGateway.Extensions;
using KWFGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors(opt =>
{
    //TODO:
    opt.AllowAnyOrigin();
    opt.AllowAnyHeader();
    opt.AllowAnyMethod();
});
app.UseMiddleware<GatewayMiddleware>();
app.UseMiddleware<FaviconMiddleware>();

app.Run();