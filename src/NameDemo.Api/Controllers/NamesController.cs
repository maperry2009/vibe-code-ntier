using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NameDemo.Data;

namespace NameDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NamesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuestName>>> GetAll(CancellationToken cancellationToken)
    {
        var names = await db.GuestNames
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(names);
    }

    [HttpPost]
    public async Task<ActionResult<GuestName>> Create(
        [FromBody] CreateNameRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required." });
        }

        var guestName = new GuestName
        {
            Name = request.Name.Trim(),
            LastName = request.LastName?.Trim() ?? string.Empty
        };

        db.GuestNames.Add(guestName);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetAll), new { id = guestName.Id }, guestName);
    }
}

public record CreateNameRequest(string Name, string? LastName);
