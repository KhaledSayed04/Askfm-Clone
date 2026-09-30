using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askfm_Clone.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected bool IsAuthorizedFor(int targetUserId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var currentUserId))
            {
                return currentUserId == targetUserId || User.IsInRole("Admin");
            }
            return false;
        }
    }
}