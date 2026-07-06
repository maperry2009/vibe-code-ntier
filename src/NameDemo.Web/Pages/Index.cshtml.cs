using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NameDemo.Web.Services;

namespace NameDemo.Web.Pages;

public class IndexModel(NameApiClient nameApiClient) : PageModel
{
    [BindProperty]
    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<GuestNameDto> Names { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Names = await nameApiClient.GetNamesAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Please enter your name.";
            Names = await nameApiClient.GetNamesAsync(cancellationToken);
            return Page();
        }

        var saved = await nameApiClient.AddNameAsync(Name, cancellationToken);
        if (!saved)
        {
            ErrorMessage = "Could not save your name. Is the API running?";
            Names = await nameApiClient.GetNamesAsync(cancellationToken);
            return Page();
        }

        return RedirectToPage();
    }
}
