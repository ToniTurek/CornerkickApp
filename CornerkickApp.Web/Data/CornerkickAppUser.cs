using Microsoft.AspNetCore.Identity;

namespace CornerkickApp.Web.Data
{
  // Add profile data for application users by adding properties to the CornerkickAppUser class
  public class CornerkickAppUser : IdentityUser
  {
    public CornerkickAppUser()
    {
      iLand = -1;
    }

    [PersonalData]
    public string Vorname { get; set; } = "";

    [PersonalData]
    public string Nachname { get; set; } = "";

    [PersonalData]
    public string Vereinsname { get; set; } = "";

    public int iLand { get; set; }

    // Home colors as hex string (#RRGGBB)
    public string sClH1 { get; set; } = "";
    public string sClH2 { get; set; } = "";
    public string sClH3 { get; set; } = "";

    // Away colors as hex string (#RRGGBB)
    public string sClA1 { get; set; } = "";
    public string sClA2 { get; set; } = "";
    public string sClA3 { get; set; } = "";

    public string? InvitedById { get; set; } = "";

    public bool bOffline { get; set; }
  }
}
