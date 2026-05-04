using CornerkickManager;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Security.Principal;

namespace CornerkickApp.Controllers.Shared
{
  public class MyAuthenticationStateProvider
  {
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public MyAuthenticationStateProvider(AuthenticationStateProvider authenticationStateProvider)
    {
      _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<ClaimsPrincipal> GetUser()
    {
      var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
      return authState.User;
    }

    public async Task<IIdentity?> GetIdentity()
    {
      ClaimsPrincipal user = await GetUser();
      return user.Identity;
    }

    public async Task<string> GetUserId()
    {
      ClaimsPrincipal user = await GetUser();
      if (user?.Identity == null) return "";
      if (!user.Identity.IsAuthenticated) return "";

      /*
      string sTest = user.Identity.Name;
      Claim? claim = user.FindFirst(ClaimTypes.NameIdentifier);
      return claim == null ? "" : claim.Value;
      string sTest = user.FindFirstValue("sub");
      sTest = user.FindFirstValue(ClaimTypes.Name);
      */

      string? s = user.FindFirstValue(ClaimTypes.NameIdentifier);
      return s == null ? "" : s;
    }

    public bool IsAuthenticated()
    {
      ClaimsPrincipal user = GetUser().Result;
      if (user?.Identity == null) return false;
      return user.Identity.IsAuthenticated;
    }
  }
}
