using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoLease.Web.Pages.Language;

public class SetModel : PageModel
{
    public IActionResult OnGet(string culture, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return Redirect(returnUrl ?? Url.Content("~/"));
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        return Redirect(returnUrl ?? Url.Content("~/"));
    }
}