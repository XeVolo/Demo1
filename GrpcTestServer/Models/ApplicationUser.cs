using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GrpcTestServer.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Guid Id { get; set; }
    }
}
