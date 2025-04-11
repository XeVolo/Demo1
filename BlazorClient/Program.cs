using BlazorClient;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorClient.Protos;
using Microsoft.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var handler = new GrpcWebHandler(new HttpClientHandler());

    var http = new HttpClient(handler);
    var channel = Grpc.Net.Client.GrpcChannel.ForAddress("https://localhost:7166", new GrpcChannelOptions { HttpHandler = handler });

    return new Greeter.GreeterClient(channel);
});
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var handler = new GrpcWebHandler(new HttpClientHandler());

    var http = new HttpClient(handler);
    var channel = Grpc.Net.Client.GrpcChannel.ForAddress("https://localhost:7166", new GrpcChannelOptions { HttpHandler = handler });

    return new AuthService.AuthServiceClient(channel);
});

await builder.Build().RunAsync();
