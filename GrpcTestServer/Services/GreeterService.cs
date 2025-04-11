using System;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Authenticator;
using BCrypt.Net;
using GrpcTestServer;
using GrpcTestServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

public class GreeterService : Greeter.GreeterBase
{
    public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        var userName = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

        return new HelloReply
        {
            Message = $"Hello {request.Name}, you are authenticated as {userName}!"
        };
    }
}
