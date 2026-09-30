using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Askfm_Clone.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        protected bool IsAuthorizedFor(int targetUserId)
        {
            var currentUserId = GetCurrentUserId();
            return currentUserId.HasValue && (currentUserId.Value == targetUserId || User.IsInRole("Admin"));
        }
    }
}