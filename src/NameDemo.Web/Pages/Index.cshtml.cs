using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NameDemo.Web.Services;

namespace NameDemo.Web.Pages;

public class IndexModel(NameApiClient nameApiClient) : PageModel
{
    [BindProperty]
    public int? EditId { get; set; }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    public IReadOnlyList<GuestNameDto> Names { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public bool IsEditing => EditId is > 0;

    public async Task OnGetAsync(int? editId, CancellationToken cancellationToken)
    {
        Names = await nameApiClient.GetNamesAsync(cancellationToken);

        if (editId is not > 0)
        {
            return;
        }

        var guest = Names.FirstOrDefault(n => n.Id == editId.Value);
        if (guest is null)
        {
            ErrorMessage = "That name could not be found for editing.";
            return;
        }

        EditId = guest.Id;
        Name = guest.Name;
        LastName = guest.LastName;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!TryValidateNames(out var error))
        {
            ErrorMessage = error;
            Names = await nameApiClient.GetNamesAsync(cancellationToken);
            return Page();
        }

        if (EditId is > 0)
        {
            var updated = await nameApiClient.UpdateNameAsync(EditId.Value, Name, LastName, cancellationToken);
            if (!updated)
            {
                ErrorMessage = "Could not update the name. Is the API running?";
                Names = await nameApiClient.GetNamesAsync(cancellationToken);
                return Page();
            }
        }
        else
        {
            var saved = await nameApiClient.AddNameAsync(Name, LastName, cancellationToken);
            if (!saved)
            {
                ErrorMessage = "Could not save your name. Is the API running?";
                Names = await nameApiClient.GetNamesAsync(cancellationToken);
                return Page();
            }
        }

        return RedirectToPage();
    }

    private bool TryValidateNames(out string? error)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            error = "Please enter your first name.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            error = "Please enter your last name.";
            return false;
        }

        error = null;
        return true;
    }
}
