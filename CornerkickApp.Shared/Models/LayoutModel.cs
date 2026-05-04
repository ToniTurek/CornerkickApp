using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;

namespace CornerkickApp.Shared.Models
{
  public abstract class LayoutModel : ComponentBase
  {
    public LayoutModel()
    {
      bStandaloneLoggedIn = !CkAppShared.bWebApp && CkAppShared.ckMng?.ltUser?.Count > 0;
      /*
      sClubEmblem = @"/Content/Uploads/emblems/0.png";

      if (App.ckMng != null) {
        usr = Controllers.MemberController.ckUserStatic(User, Controllers.MemberController._httpContextAccessor);
        if (usr != null) clb = usr.club;

        ci = Controllers.MemberController.getCiStatic(clb);

        if ((App.bWebApp && User != null && User.Identity.IsAuthenticated) || bStandaloneLoggedIn) {
          if (clb != null) {
            if (bStandaloneLoggedIn) {
              //sEmblem = @"/Content/Uploads/media_" + CornerkickApp.App.ckMng.sDatabaseName + "/images/emblems/";
              //sEmblem = "data:image/*;base64," + @Convert.ToBase64String(App.ConvertToBytes(Controllers.MemberController.getClubEmblemFile(clb.iId)));
              byteClubUserEmblem = App.ConvertToBytes(Controllers.MemberController.getClubEmblemFile(clb.iId));
            } else {
              sClubEmblem = @"/Content/Uploads/emblems/";
            }

            if (clb.bNation) {
              sClubEmblem = @"/Content/Icons/flags/" + CornerkickManager.Main.sLandShort[clb.iLand] + ".png";
            } else {
              sClubEmblem += clb.iId.ToString() + ".png";
            }
          }
        }
      }
      */
    }

    public class SelectListItem
    {
      public string Text { get; set; } = "";
      public string Value { get; set; } = "";
      public bool Selected { get; set; } = false;
    }

    public bool bStandaloneLoggedIn { get; set; }
  }
}
