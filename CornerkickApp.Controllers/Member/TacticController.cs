using CornerkickApp.Shared.Models;
using static CornerkickApp.Shared.Models.CkAppShared;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  public class TacticController
  {
    public static TacticModel Model(CornerkickManager.User _usr)
    {
      TacticModel mdTc = new TacticModel();
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return mdTc;

      if (clb.iTactic >= clb.ltTactic.Count) clb.iTactic = 0;
      mdTc.tactic = clb.ltTactic[clb.iTactic];

      // Set system dropdown
      mdTc.sliSystem = MemberController.getSliTacticSystem(clb);
      mdTc.iSystem = clb.iTactic;

      mdTc.iTactics = [
         (int)( mdTc.tactic.fOrientation     * 100f),
         (int)( mdTc.tactic.fPower           * 100f),
         (int)( mdTc.tactic.fShootFreq       * 100f),
         (int)( mdTc.tactic.fAggressive      * 100f),
         (int)( mdTc.tactic.fPassRisk        * 100f),
         (int)( mdTc.tactic.fPassLength      * 100f),
         (int)( mdTc.tactic.fPassFreq        * 100f),
         (int)((mdTc.tactic.fPassLeft  + 1f) * 100f),
         (int)((mdTc.tactic.fPassRight + 1f) * 100f),
                mdTc.tactic.iGapOffsite,
         (int)((1f - (mdTc.tactic.fPassLeft + mdTc.tactic.fPassRight)) * 100f)
      ];

      // Set standards dropdown
      string[] sStandards = [ "11m", "Freistoß", "Ecke R.", "Ecke L." ];

      mdTc.ltDdlStandards = new List<SelectListItem>[4];
      mdTc.iStandards = new int[4];
      for (byte iS = 0; iS < 4; iS++) {
        mdTc.ltDdlStandards[iS] = new List<SelectListItem>();
        mdTc.ltDdlStandards[iS].Add(new SelectListItem { Text = "auto (" + sStandards[iS] + ")", Value = "-1" });
        for (byte iPl = 0; iPl < ckMng.game.data.nPlStart; iPl++) {
          if (clb.ltPlayer.Count <= iPl) break;

          CornerkickManager.Player pl = clb.ltPlayer[iPl];
          mdTc.ltDdlStandards[iS].Add(new SelectListItem { Text = pl.plGame.sName, Value = iPl.ToString(), Selected = iPl == clb.iStandards[iS] });
        }

        mdTc.iStandards[iS] = clb.iStandards[iS];
      }

      int nSubs = clb.staff.iCoTrainer > 2 ? clb.staff.iCoTrainer - 2 : 0;
      mdTc.ddlAutoSubsOut = new List<SelectListItem>[nSubs];
      mdTc.ddlAutoSubsIn  = new List<SelectListItem>[nSubs];

      mdTc.iAutoSubsOut = new int[nSubs];
      mdTc.iAutoSubsIn  = new int[nSubs];
      mdTc.iAutoSubsMin = new int[nSubs];

      for (int iAS = 0; iAS < nSubs; iAS++) CreateAutoSubs(mdTc, clb, iAS);

      if (clb.nextGame != null) {
        byte iHA = 0;
        if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;
        for (byte iAS = 0; iAS < nSubs; iAS++) {
          mdTc.iAutoSubsMin[iAS] = 60;
          if (iAS < clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count) mdTc.iAutoSubsMin[iAS] = clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][2];
        }
      }

      mdTc.bGame = MemberController.checkUserGame2(_usr);

      // Tutorial
      if (ttUser != null) {
        int iUserIx = ckMng.ltUser.IndexOf(_usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) mdTc.tutorial = ttUser[iUserIx];
      }

      return mdTc;
    }

    public static float update(CornerkickManager.User usr, int iType, int iValue, int iTactic)
    {
      float fRet = 0f;

      CornerkickManager.Club clb = MemberController.ckClub(usr);
      if (clb == null) return fRet;

      if      (iType == 0) clb.ltTactic[iTactic].fOrientation = iValue / 100f;
      else if (iType == 1) clb.ltTactic[iTactic].fPower       = iValue / 100f;
      else if (iType == 2) clb.ltTactic[iTactic].fShootFreq   = iValue / 100f;
      else if (iType == 3) clb.ltTactic[iTactic].fAggressive  = iValue / 100f;
      else if (iType == 4) clb.ltTactic[iTactic].fPassRisk    = iValue / 100f;
      else if (iType == 5) clb.ltTactic[iTactic].fPassLength  = iValue / 100f;
      else if (iType == 6) clb.ltTactic[iTactic].fPassFreq    = iValue / 100f;
      else if (iType == 7) {
        clb.ltTactic[iTactic].fPassLeft  = (iValue / 100f) - 1f;
        if (clb.ltTactic[iTactic].fPassLeft + clb.ltTactic[iTactic].fPassRight > 1f) clb.ltTactic[iTactic].fPassRight = (float)Math.Round(1f - clb.ltTactic[iTactic].fPassLeft, 2);
        fRet = clb.ltTactic[iTactic].fPassRight;
      } else if (iType == 8) {
        clb.ltTactic[iTactic].fPassRight = (iValue / 100f) - 1f;
        if (clb.ltTactic[iTactic].fPassLeft + clb.ltTactic[iTactic].fPassRight > 1f) clb.ltTactic[iTactic].fPassLeft  = (float)Math.Round(1f - clb.ltTactic[iTactic].fPassRight, 2);
        fRet = clb.ltTactic[iTactic].fPassLeft;
      } else if (iType == 9) {
        clb.ltTactic[iTactic].iGapOffsite = iValue;
      }

      // Set tactic of current game
      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          if      (usr.game.data.team[0].iTeamId == clb.iId) usr.game.data.team[0].ltTactic = clb.ltTactic;
          else if (usr.game.data.team[1].iTeamId == clb.iId) usr.game.data.team[1].ltTactic = clb.ltTactic;
        }
      }

      return fRet;
    }

    public static void setOffsite(CornerkickManager.User usr, bool bOffsite, int iTactic)
    {
      CornerkickManager.Club clb = MemberController.ckClub(usr);
      if (clb == null) return;

      clb.ltTactic[iTactic].bOffsite = bOffsite;

      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          if      (usr.game.data.team[0].iTeamId == clb.iId) usr.game.data.team[0].ltTactic[iTactic] = clb.ltTactic[iTactic];
          else if (usr.game.data.team[1].iTeamId == clb.iId) usr.game.data.team[1].ltTactic[iTactic] = clb.ltTactic[iTactic];
        }
      }
    }

    public static void setStandards(CornerkickManager.User usr, int iType, int iIndexPlayer)
    {
      if (iType < 0) return;

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return;

      clb.iStandards[iType] = iIndexPlayer;

      // Set standards of current game
      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          if      (usr.game.data.team[0].iTeamId == clb.iId) usr.game.data.team[0].iStandards = clb.iStandards;
          else if (usr.game.data.team[1].iTeamId == clb.iId) usr.game.data.team[1].iStandards = clb.iStandards;
        }
      }
    }

    private static void CreateAutoSubs(TacticModel mdTc, CornerkickManager.Club clb, int iAS)
    {
      if (mdTc.ddlAutoSubsOut == null) return;
      if (mdTc.ddlAutoSubsIn  == null) return;
      if (mdTc.iAutoSubsOut == null) return;
      if (mdTc.iAutoSubsIn  == null) return;

      mdTc.ddlAutoSubsOut[iAS] = new List<SelectListItem>();
      mdTc.ddlAutoSubsIn [iAS] = new List<SelectListItem>();

      mdTc.ddlAutoSubsOut[iAS].Add(new SelectListItem() { Text = "aus", Value = "-1" });
      mdTc.ddlAutoSubsIn [iAS].Add(new SelectListItem() { Text = "aus", Value = "-1" });

      mdTc.iAutoSubsOut[iAS] = -1;
      mdTc.iAutoSubsIn [iAS] = -1;

      byte iHA = 0;
      if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;

      // foreach player
      for (byte iPl = 0; iPl < clb.nextGame.nPlStart + clb.nextGame.nPlRes; iPl++) {
        if (iPl >= clb.ltPlayer.Count) break;

        CornerkickManager.Player pl = clb.ltPlayer[iPl];

        bool bOut = iPl < clb.nextGame.nPlStart;

        bool bContinue = false;
        byte jAS = 0;
        while (jAS < iAS) {
          if (jAS >= clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count) break;

          if ( bOut && iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][0]) bContinue = true;
          if (!bOut && iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][1]) bContinue = true;
          jAS++;
        }
        if (bContinue) continue;

        if (iAS < clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count) {
          if (clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][1] > clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][0]) {
            if (iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][0]) mdTc.iAutoSubsOut[iAS] = iPl;
            if (iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][1]) mdTc.iAutoSubsIn [iAS] = iPl;
          }
        }

        string sPos = "";
        string sStrength = "";
        pl.plGame.iIndex = iPl;

        CornerkickManager.Main.Staff? staff = ClubController.getClubStaff(clb);
        float[]? fSkills = staff != null ? staff.getScoutedSkills(pl.plGame) : null;
        if (iPl < clb.nextGame.nPlStart) {
          byte iPosRole = CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(pl.plGame, clb.ltTactic[clb.iTactic].formation, CornerkickGame.Field.ConvertPitch(clb.nextGame.fPitchSizeRel)));
          sPos = CornerkickManager.Main.sPosition[iPosRole];
          sStrength = CornerkickGame.Tool.getAveSkill(pl.plGame, iPos: iPosRole, bIdeal: false, fSkills: fSkills).ToString(" (0.0)");
        } else {
          sPos = CornerkickManager.PlayerTool.getStrPos(pl);
          sStrength = CornerkickGame.Tool.getAveSkill(pl.plGame, bIdeal: false, fSkills: fSkills).ToString(" (0.0)");
        }

        if (bOut) mdTc.ddlAutoSubsOut[iAS].Add(new SelectListItem() { Text = pl.plGame.sName + " - " + sPos + sStrength, Value = iPl.ToString() });
        else      mdTc.ddlAutoSubsIn [iAS].Add(new SelectListItem() { Text = pl.plGame.sName + " - " + sPos + sStrength, Value = iPl.ToString() });
      }
    }

    public static bool setAutoSubs(CornerkickManager.User usr, int iAS, int iIndexPlayerOut, int iIndexPlayerIn, int iMin)
    {
      if (iIndexPlayerOut < 0 || iIndexPlayerIn < 0 || iIndexPlayerOut == iIndexPlayerIn) return false;

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return false;
      if (clb.nextGame == null) return false;

      byte iHA = 0;
      if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;

      while (clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count <= iAS) clb.nextGame.team[iHA].ltSubstitutionsPlanned.Add(new byte[3] { 0, 0, 0 });

      clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][0] = (byte)iIndexPlayerOut;
      clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][1] = (byte)iIndexPlayerIn;
      clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][2] = (byte)iMin;

      bool bValid = iIndexPlayerOut >= 0 && iIndexPlayerIn >= 0 && iIndexPlayerOut != iIndexPlayerIn && iMin >= 0;

      if (!bValid) {
        int jAS = iAS + 1;
        while (jAS < clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count) {
          clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][0] = 0;
          clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][1] = 0;

          jAS++;
        }
      }

      return bValid;
    }


  }
}
