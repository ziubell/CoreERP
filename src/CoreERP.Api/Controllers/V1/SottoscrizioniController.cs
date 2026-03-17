using System.Security.Claims;
using CoreERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreERP.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SottoscrizioniController : ControllerBase
{
    private readonly ISottoscrizioneNotificaRepository _repository;

    public SottoscrizioniController(ISottoscrizioneNotificaRepository repository)
    {
        _repository = repository;
    }

    private string GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException();

    [HttpGet("{entitaTipo}/{entitaId}")]
    public async Task<IActionResult> IsFollowing(string entitaTipo, int entitaId)
    {
        var following = await _repository.IsFollowingAsync(GetUserId(), entitaTipo, entitaId);
        return Ok(new { following });
    }

    [HttpPost("{entitaTipo}/{entitaId}")]
    public async Task<IActionResult> Follow(string entitaTipo, int entitaId)
    {
        await _repository.FollowAsync(GetUserId(), entitaTipo, entitaId);
        return NoContent();
    }

    [HttpDelete("{entitaTipo}/{entitaId}")]
    public async Task<IActionResult> Unfollow(string entitaTipo, int entitaId)
    {
        await _repository.UnfollowAsync(GetUserId(), entitaTipo, entitaId);
        return NoContent();
    }

    [HttpGet("{entitaTipo}/{entitaId}/followers")]
    public async Task<IActionResult> GetFollowers(string entitaTipo, int entitaId)
    {
        var followers = await _repository.GetFollowersAsync(entitaTipo, entitaId);
        return Ok(new { followers, count = followers.Count });
    }
}
