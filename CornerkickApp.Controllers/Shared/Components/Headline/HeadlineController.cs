using CornerkickApp.Shared.Models;
//using static CornerkickApp.Shared.Models.CkAppShared;
using System.Globalization;

namespace CornerkickApp.Controllers.Shared.Components.Headline
{
  public class HeadlineController
  {
    /*
    public Task<UserComponentModel> GetUserComponentModelAsync(
      DateTime dtNow
#if _WebApp
      , int iLoadState
      , System.Timers.Timer timerCkCalender
#endif
      )
    */
    public static Task<UserComponentModel> GetUserComponentModelAsync()
    {
      CultureInfo ci = CultureInfo.CurrentCulture;

      CornerkickManager.User usr = new CornerkickManager.User();
      CornerkickManager.Club? clb = Member.MemberController.ckClub(usr);

      UserComponentModel ucm = new UserComponentModel();

      //CornerkickManager.Main _ckMng = ICkAppShared.ckMng;

      if (clb != null) {
        // Get culture info
        //ci = Controllers.MemberController.getCiStatic(clb);

        // Headline cash
        ucm.iHeadlineBalance = clb.iBalance;
        ucm.iHeadlineBalanceSecret = clb.iBalanceSecret;
        /*
        string sStyle = clb.iBalance < 0 ? " style=\"color: red\"" : "";
        if (clb.iBalance != 0) ucm.sHeadlineCash = "<span" + sStyle + ">" + clb.iBalance.ToString("#,#", ci) + "</span>";
        if (clb.iBalanceSecret != 0) ucm.sHeadlineCash += " (" + clb.iBalanceSecret.ToString("#,#", ci) + ")";
        if (!string.IsNullOrEmpty(ucm.sHeadlineCash)) ucm.sHeadlineCash += " €";
        //sHeadlineCash = WebUtility.HtmlDecode(sHeadlineCash);
        */

        // Club/nation emblem
        //byteClubUserEmblem = App.ConvertToBytes(Controllers.MemberController.getClubEmblemFile(clb.iId));

        if (clb.bNation) {
          ucm.sClubEmblem = @"/Content/Icons/flags/" + CornerkickManager.Main.sLandShort[clb.iLand] + ".png";
        }
      }

      // Headline date
      //ucm.sHeadlineDate = dtNow.ToString("ddd", ci) + ", " + dtNow.ToString("d", ci) + ", " + dtNow.ToString("t", ci);

#if _WebApp
      if (CkAppShared.iLoadState == 1) {
        ucm.sHeadlineDate += " - Spielstand wird geladen ...";
      } else if (CkAppShared.iLoadState == 2) {
        ucm.sHeadlineDate += " - Zeitschritte werden durchgeführt ...";
      } else if (CkAppShared.timerCkCalender != null && !CkAppShared.timerCkCalender.Enabled) {
        ucm.sHeadlineDate += " (Pause)";
      }
#endif

      return Task.FromResult(ucm);
    }

    public static UserComponentModel Get(CornerkickManager.User usr)
    {
      CultureInfo ci = CultureInfo.CurrentCulture;

      CornerkickManager.Club? clb = Member.MemberController.ckClub(usr);

      UserComponentModel ucm = new UserComponentModel();

      if (clb != null) {
        // Get culture info
        //ci = Controllers.MemberController.getCiStatic(clb);

        // Headline cash
        if (usr.iResp >= CkAppShared.iUserRespFinance) {
          ucm.iHeadlineBalance = clb.iBalance;
          ucm.iHeadlineBalanceSecret = clb.iBalanceSecret;
          /*
          string sStyle = clb.iBalance < 0 ? " style=\"color: red\"" : "";
          if (clb.iBalance != 0) ucm.sHeadlineCash = "<span" + sStyle + ">" + clb.iBalance.ToString("#,#", ci);
          if (clb.iBalanceSecret != 0) ucm.sHeadlineCash += " (" + clb.iBalanceSecret.ToString("#,#", ci) + ")";
          if (!string.IsNullOrEmpty(ucm.sHeadlineCash)) ucm.sHeadlineCash += " €</span>";
          //sHeadlineCash = WebUtility.HtmlDecode(sHeadlineCash);
          */
        }

        // Club/nation emblem
        //byteClubUserEmblem = App.ConvertToBytes(Controllers.MemberController.getClubEmblemFile(clb.iId));

        if (clb.bNation) {
          ucm.sClubEmblem = @"/Content/Icons/flags/" + CornerkickManager.Main.sLandShort[clb.iLand] + ".png";
        }
      }

      // Headline date
      ucm.sHeadlineDate = CkAppShared.ckMng.dtDatum.ToString("ddd", ci) + ", " + CkAppShared.ckMng.dtDatum.ToString("d", ci) + ", " + CkAppShared.ckMng.dtDatum.ToString("t", ci);
      ucm.iSeasonProgress = (int)(100.0 * (CkAppShared.ckMng.dtDatum - CkAppShared.ckMng.dtSeasonStart).TotalDays / (CkAppShared.ckMng.dtSeasonEnd - CkAppShared.ckMng.dtSeasonStart).TotalDays);

#if _WebApp
      if (CkAppShared.iLoadState == 1) {
        ucm.sHeadlineDate += " - Spielstand wird geladen ...";
      } else if (CkAppShared.iLoadState == 2) {
        ucm.sHeadlineDate += " - Zeitschritte werden durchgeführt ...";
      } else if (CkAppShared.timerCkCalender != null && !CkAppShared.timerCkCalender.Enabled) {
        ucm.sHeadlineDate += " (Pause)";
      }
#endif

      return ucm;
    }
  }
}
