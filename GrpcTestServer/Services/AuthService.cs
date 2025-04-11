using Grpc.Core;
using GrpcTestServer.Models;
using IdentityModel.Client;
using GrpcTestServer;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;

namespace GrpcTestServer.Services
{
    public class AuthService : GrpcTestServer.AuthService.AuthServiceBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpClientFactory = httpClientFactory;
        }

        public override async Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            if (request.Password != request.ConfirmPassword)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Password"));

            var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);

            return new RegisterReply{ Message="Ok"};
            
        }

        public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid username or password"));

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid username or password"));

            var client = _httpClientFactory.CreateClient();
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:7166");
            if (disco.IsError) throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid username or password"));

            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "blazor-client",
                ClientSecret = "secret",
                UserName = request.Email,
                Password = request.Password,
                Scope = "api1 offline_access"
            });

            LoginReply result1 = new LoginReply { Token = tokenResponse.AccessToken };

            return tokenResponse.IsError ? null : result1;
        }
    }
}
