using Askfm_Clone.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/users/{userId}/blocks")]
[Authorize]
[ApiController]
public class BlockController : BaseController
{
    private readonly IBlockService _blockService;

    public BlockController(IBlockService blockService)
    {
        _blockService = blockService;
    }

 
    [HttpPost("{targetUserId}")]
    public async Task<IActionResult> BlockUser(int userId, int targetUserId, [FromQuery] bool isAnonymous = false)
    {
        if (!IsAuthorizedFor(userId)) return Forbid();
        if (userId == targetUserId) return BadRequest("You cannot block yourself.");

        var result = await _blockService.BlockAsync(userId, targetUserId, isAnonymous);
        if (!result) return BadRequest("Failed to block the user.");

        // Return 201 Created with Location pointing to IsBlocked endpoint
        return CreatedAtAction(nameof(IsBlocked),
                               new { userId, targetUserId },
                               new { blockedId = targetUserId, isAnonymous });
    }

   
    [HttpDelete("{targetUserId}")]
    public async Task<IActionResult> UnblockUser(int userId, int targetUserId)
    {
        if (!IsAuthorizedFor(userId)) return Forbid();

        var result = await _blockService.UnblockAsync(userId, targetUserId);
        return result ? NoContent() : NotFound("No block relationship found to remove.");
    }

   
    [HttpGet("{targetUserId}")]
    public async Task<IActionResult> IsBlocked(int userId, int targetUserId)
    {
        if (!IsAuthorizedFor(userId)) return Forbid();

        var isBlocked = await _blockService.IsBlockedAsync(userId, targetUserId);
        return Ok(new { userId, targetUserId, isBlocked });
    }

    
    [HttpGet]
    public async Task<IActionResult> GetBlockedUsers(int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (!IsAuthorizedFor(userId)) return Forbid();

        var result = await _blockService.GetBlockedUsersAsync(userId, pageNumber, pageSize);
        return Ok(result);
    }
}
