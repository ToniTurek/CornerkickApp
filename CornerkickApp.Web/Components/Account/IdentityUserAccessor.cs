using CornerkickApp.Web.Data;
using Microsoft.AspNetCore.Identity;

namespace CornerkickApp.Web.Components.Account
{
  internal sealed class IdentityUserAccessor(UserManager<CornerkickAppUser> userManager, IdentityRedirectManager redirectManager)
  {
    public async Task<CornerkickAppUser?> GetRequiredUserAsync(HttpContext context)
    {
      if (context == null) return null;

      var user = await userManager.GetUserAsync(context.User);

      if (user is null) {
        redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
      }

      return user;
    }
  }
}
