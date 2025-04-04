using GrpcTestServer.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.WithOrigins("http://localhost:5199") // Adres Blazor WebAssembly
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddSingleton<JwtService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
app.UseCors("AllowAll");

app.UseRouting();
app.UseGrpcWeb();
// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>().EnableGrpcWeb();
app.MapGrpcReflectionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
