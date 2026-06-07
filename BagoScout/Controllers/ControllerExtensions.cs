using BagoScout.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BagoScout.Controllers
{
    public static class ControllerExtensions
    {
        public static async Task<int?> GetAuthorizedUserIdAsync(this ControllerBase controller, ApplicationDbContext context)
        {
            var subClaim = controller.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(subClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
