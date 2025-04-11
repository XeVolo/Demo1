using GrpcTestServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using GrpcTestServer.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()  
            .AllowAnyHeader()  
            .AllowAnyMethod()
            .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    });
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7166, listenOptions =>
    {
        listenOptions.UseHttps(); 
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2; 
    });
});

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<ApplicationUser>()
    .AddInMemoryClients(new[] {
        new Client
        {
            ClientId = "blazor-client",
            AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedScopes = { "api1" },
            AllowOfflineAccess = true
        }
    })
    .AddInMemoryApiScopes(new[] {
        new ApiScope("api1", "My API")
    })
    .AddDeveloperSigningCredential();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:7166";
        options.TokenValidationParameters.ValidateAudience = false;
    });
builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddCors();

builder.Services.AddHttpClient();
var app = builder.Build();

app.UseCors("AllowAll");

app.UseRouting();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GreeterService>().EnableGrpcWeb();
app.MapGrpcService<AuthService>().EnableGrpcWeb();

app.Run();
