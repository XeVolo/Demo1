using BlazorClient;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorClient.Protos;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());
    var channel = GrpcChannel.ForAddress("https://localhost:7166", new GrpcChannelOptions { HttpHandler = httpHandler });
    return new Greeter.GreeterClient(channel);
});

await builder.Build().RunAsync();
