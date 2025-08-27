using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Interfaces.Services;

namespace OnlineStore.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public AdminUsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpPost("{id:guid}/make-admin")]
    public async Task<IActionResult> MakeAdmin([FromRoute] Guid id, CancellationToken ct)
    {
        await _usersService.MakeAdminAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/remove-admin")]
    public async Task<IActionResult> RemoveAdmin([FromRoute] Guid id, CancellationToken ct)
    {
        await _usersService.RemoveAdminAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/soft-delete")]
    public async Task<IActionResult> SoftDelete([FromRoute] Guid id, CancellationToken ct)
    {
        await _usersService.SoftDeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore([FromRoute] Guid id, CancellationToken ct)
    {
        await _usersService.RestoreAsync(id, ct);
        return NoContent();
    }
}
