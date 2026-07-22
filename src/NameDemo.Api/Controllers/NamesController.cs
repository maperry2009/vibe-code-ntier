using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NameDemo.Api.Services;
using NameDemo.Data;

namespace NameDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NamesController(AppDbContext db, IWebhookNotifier webhookNotifier) : ControllerBase
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
        [FromBody] NameRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryValidateNameRequest(request, out var errorResult))
        {
            return errorResult!;
        }

        var guestName = new GuestName
        {
            Name = request.Name.Trim(),
            LastName = request.LastName.Trim()
        };

        db.GuestNames.Add(guestName);
        await db.SaveChangesAsync(cancellationToken);

        await webhookNotifier.TryNotifyGuestNameSavedAsync(guestName, cancellationToken);

        return CreatedAtAction(nameof(GetAll), new { id = guestName.Id }, guestName);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GuestName>> Update(
        int id,
        [FromBody] NameRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryValidateNameRequest(request, out var errorResult))
        {
            return errorResult!;
        }

        var guestName = await db.GuestNames.FindAsync([id], cancellationToken);
        if (guestName is null)
        {
            return NotFound(new { message = $"Name with id {id} was not found." });
        }

        guestName.Name = request.Name.Trim();
        guestName.LastName = request.LastName.Trim();
        await db.SaveChangesAsync(cancellationToken);

        return Ok(guestName);
    }

    private static bool TryValidateNameRequest(NameRequest request, out ActionResult? errorResult)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errorResult = new BadRequestObjectResult(new { message = "Name is required." });
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            errorResult = new BadRequestObjectResult(new { message = "Last name is required." });
            return false;
        }

        errorResult = null;
        return true;
    }
}

public record NameRequest(string Name, string LastName);
