using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Infrastructure.Persistence;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db; // или IUserRepository
    public AdminUsersController(AppDbContext db) => _db = db;

    [HttpPost("{userId:guid}/make-admin")]
    public async Task<IActionResult> MakeAdmin(Guid userId)
    {
        var u = await _db.Users.FindAsync(userId);
        if (u is null) return NotFound();
        if (!u.IsAdmin) { u.IsAdmin = true; await _db.SaveChangesAsync(); }
        return NoContent();
    }

    [HttpPost("{userId:guid}/remove-admin")]
    public async Task<IActionResult> RemoveAdmin(Guid userId)
    {
        var u = await _db.Users.FindAsync(userId);
        if (u is null) return NotFound();
        if (u.IsAdmin) { u.IsAdmin = false; await _db.SaveChangesAsync(); }
        return NoContent();
    }
}
