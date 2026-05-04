using CornerkickApp.Shared.Models;
using static CornerkickApp.Shared.Models.CkAppShared;

namespace CornerkickApp.Controllers.Member
{
  public class UserController
  {
    public static UserModel Get(CornerkickManager.User _usr)
    {
      UserModel model = new UserModel();

      // Tutorial
      if (ttUser != null) {
        int iUserIx = ckMng.ltUser.IndexOf(_usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) model.tutorial = ttUser[iUserIx];
      }

      return model;
    }

    public static UserOptionsModel UserOptions(CornerkickManager.User usr)
    {
      UserOptionsModel mdOptions = new UserOptionsModel();

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb?.nextGame != null) {
        mdOptions.iGameSpeed  = clb.nextGame.iGameSpeed;
        mdOptions.iGameSpeed2 = clb.nextGame.iGameSpeed2;
      }

      if (mdOptions.iGameSpeed  < 2) mdOptions.iGameSpeed  = 0;
      if (mdOptions.iGameSpeed2 < 2) mdOptions.iGameSpeed2 = 0;

      if (usr.lti != null) {
        // Balance today
        if (usr.lti.Count > UserOptionsModel.iUserOptionsIxShowBalance) mdOptions.bShowBalanceToday = usr.lti[UserOptionsModel.iUserOptionsIxShowBalance] > 0;

        // Set comment level
        if (usr.lti.Count > UserOptionsModel.iUserOptionsIxComment) mdOptions.iComments = usr.lti[UserOptionsModel.iUserOptionsIxComment];

        // Set show pitch
        if (usr.lti.Count > UserOptionsModel.iUserOptionsIxShowPitch) mdOptions.iShowPitch = usr.lti[UserOptionsModel.iUserOptionsIxShowPitch];

        // Set animations level
        if (usr.lti.Count > UserOptionsModel.iUserOptionsIxAnimations) mdOptions.iAnimations = usr.lti[UserOptionsModel.iUserOptionsIxAnimations];

        // Set font size
        if (usr.lti.Count > UserOptionsModel.iUserOptionsIxFontSize) mdOptions.iFontSize = usr.lti[UserOptionsModel.iUserOptionsIxFontSize];

        // Set calendar speed
        if (usr.lti.Count > UserOptionsModel.iUserOptionsIxCalSpeed) mdOptions.iCalSpeed = usr.lti[UserOptionsModel.iUserOptionsIxCalSpeed];

        // Scouting
        mdOptions.bScouting = usr.bScouting;
      }

      // Tutorial
      if (ttUser != null) {
        int iUserIx = ckMng.ltUser.IndexOf(usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) mdOptions.bShowTutorial = ttUser[iUserIx].bShow;
      }

      return mdOptions;
    }
  }
}
