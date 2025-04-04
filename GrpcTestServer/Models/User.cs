using System.ComponentModel.DataAnnotations;

namespace GrpcTestServer.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? Secret2FA { get; set; }
        public bool Is2FAEnabled { get; set; }
    }
}
