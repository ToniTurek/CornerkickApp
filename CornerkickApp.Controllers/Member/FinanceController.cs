using CornerkickApp.Shared.Models;
using System;

namespace CornerkickApp.Controllers.Member
{
  public class FinanceController
  {
    public static FinanceModel Model(CornerkickManager.User? _usr)
    {
      FinanceModel model = new FinanceModel();

      if (_usr == null) return model;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return model;

      model.iPrice1 = clb.iAdmissionPrice[0];
      model.iPrice2 = clb.iAdmissionPrice[1];
      model.iPrice3 = clb.iAdmissionPrice[2];

      model.iPriceSeason1 = clb.iAdmissionPriceSeasonal[0];
      model.iPriceSeason2 = clb.iAdmissionPriceSeasonal[1];
      model.iPriceSeason3 = clb.iAdmissionPriceSeasonal[2];
      model.fSeasonalTicketsMaxFrac = clb.fSeasonalTicketsMaxFrac * 100f;

      model.iSeasonalTickets = new int[clb.iSpectatorsSeasonal.Length];
      model.iSeasonalTickets = clb.iSpectatorsSeasonal;

      model.ltAccount = clb.ltAccount;
      model.bEditable = CkAppShared.ckMng.dtDatum.Date.Equals(CkAppShared.ckMng.dtSeasonStart.Date);

      // Year of budget plan
      model.sliYears = [ new LayoutModel.SelectListItem { Text = Tool.getSeasonString(CkAppShared.ckMng.iSeason), Value = "-1" }, ];
      for (int iY = 0; iY < _usr.ltBudget.Count; iY++) {
        model.sliYears.Add(new LayoutModel.SelectListItem { Text = Tool.getSeasonString(CkAppShared.ckMng.iSeason - iY - 1), Value = iY.ToString() });
      }

      // Secret Balance
      model.fBalanceSecretFracAdmissionPrice = clb.fBalanceSecretFracAdmissionPrice * 100f;
      model.sBalanceSecret = clb.iBalanceSecret.ToString("N0", MemberController.getCi(_usr)) + " €";

      return model;
    }

    public static long[][] GetBudgetPlan(CornerkickManager.User? usr, int iYear, bool bNetto)
    {
      long[][] iBudget = new long[2][];

      if (usr == null) return iBudget;

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return iBudget;

      CornerkickManager.Finance.Budget[] bd = new CornerkickManager.Finance.Budget[2];
      if (iYear < 0) {
        bd[0] = usr.budget;
        bd[1] = CkAppShared.ckMng.ui.getActualBudget(clb);

        if (bd[0].iPaySalary == 0) bd[0].iPaySalary = clb.getSalaryPlayer();
        if (bd[0].iPayStaff  == 0) bd[0].iPayStaff  = clb.getSalaryStuff ();
      } else  if (usr.ltBudget.Count - 1 - iYear < usr.ltBudget.Count) {
        bd[0] = usr.ltBudget[usr.ltBudget.Count - 1 - iYear][0];
        bd[1] = usr.ltBudget[usr.ltBudget.Count - 1 - iYear][1];
      }

      iBudget[0] = new long[20]; // Plan
      iBudget[1] = new long[20]; // Real

      for (byte i = 0; i < 2; i++) {
        int j = 0;

        long iTransferNetto      = bd[i].iInTransfer      - bd[i].iPayTransfer;
        long iMerchandisingNetto = bd[i].iInMerchandising - bd[i].iPayMerchandising;

        iBudget[i][j++] = bd[i].iInSpec;
        iBudget[i][j++] = bd[i].iInBonusCup;
        iBudget[i][j++] = bd[i].iInBonusSponsor;
        iBudget[i][j++] = bd[i].iInTvBonus;
        if (bNetto) iBudget[i][j++] = iMerchandisingNetto > 0 ? iMerchandisingNetto : 0;
        else        iBudget[i][j++] = bd[i].iInMerchandising;
        if (bNetto) iBudget[i][j++] = iTransferNetto > 0 ? iTransferNetto : 0;
        else        iBudget[i][j++] = bd[i].iInTransfer;
#if _WebApp
        iBudget[i][j++] = bd[i].iInGenericA;
#endif
        iBudget[i][j++] = bd[i].iInMisc;
        iBudget[i][j++] = bd[i].iPaySalary;
        iBudget[i][j++] = bd[i].iPayStaff + bd[i].iPayScouting;
        iBudget[i][j++] = bd[i].iPayStadium + bd[i].iPayStadiumSurr;
        iBudget[i][j++] = bd[i].iPayStadiumSurrMaintenance;
        if (bNetto) iBudget[i][j++] = iMerchandisingNetto < 0 ? -iMerchandisingNetto : 0;
        else        iBudget[i][j++] = bd[i].iPayMerchandising;
        if (bNetto) iBudget[i][j++] = iTransferNetto < 0 ? -iTransferNetto : 0;
        else        iBudget[i][j++] = bd[i].iPayTransfer;
        iBudget[i][j++] = bd[i].iPayTravel;
        iBudget[i][j++] = bd[i].iPayInterest;
        iBudget[i][j++] = bd[i].iPayMisc;

        long iInTotal  = CornerkickManager.Finance.getBudgetInTotal (bd[i], bNetto: bNetto);
        long iPayTotal = CornerkickManager.Finance.getBudgetPayTotal(bd[i], bNetto: bNetto);
        iBudget[i][j++] = iInTotal;
        iBudget[i][j++] = iPayTotal;

        iBudget[i][j] = iInTotal - iPayTotal;
      }

      return iBudget;
    }

    public static List<CkAppShared.DataPointTL>? GetBalanceHistory(CornerkickManager.User? usr, FinanceModel financeModel)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return null;

      List<CkAppShared.DataPointTL> dataPoints = new List<CkAppShared.DataPointTL>();

      foreach (CornerkickManager.Finance.Account kto in clb.ltAccount) {
        if (kto.dt.CompareTo(CkAppShared.ckMng.dtDatum.AddDays(-30)) > 0) {
          dataPoints.Add(new CkAppShared.DataPointTL(kto.dt, kto.iBalance));
        }
      }

      dataPoints.Add(new CkAppShared.DataPointTL(CkAppShared.ckMng.dtDatum, clb.iBalance));

      return dataPoints;
    }

    public static int SetAdmissionPrice(CornerkickManager.User? usr, int iType, int iValue, bool bSeasonalPrice = false)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return 0;

      if (iType >= 0) {
        if (bSeasonalPrice) clb.iAdmissionPriceSeasonal[iType] = iValue;
        else                clb.iAdmissionPrice        [iType] = iValue;
      }

      int iInSpec = 0;
      CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, clb.iLand, clb.iDivision);
      if (league != null) {
        CornerkickGame.Stadium stm = clb.stadium;

        int iSelectedStadium = 1;
        if (usr != null && usr.lti.Count > UserOptionsModel.iUserOptionsIxStadium) iSelectedStadium = usr.lti[UserOptionsModel.iUserOptionsIxStadium];

        if      (iSelectedStadium ==  1) stm = CkAppShared.stadiumDefaultSmall;
        else if (iSelectedStadium ==  2) stm = CkAppShared.stadiumDefaultBig;
        else if (iSelectedStadium == -1) stm = CkAppShared.stadiumDefaultTrainingCourt;
        iInSpec = (stm.getSeats(0) * clb.iAdmissionPrice[0]) +
                  (stm.getSeats(1) * clb.iAdmissionPrice[1]) +
                  (stm.getSeats(2) * clb.iAdmissionPrice[2]);
        int nGamesHome = league.getMatchdaysTotal();
        iInSpec *= nGamesHome;
      }

      return iInSpec;
    }

    public static List<FinanceModel.DataPointSpec>[]? GetSpecHistory(CornerkickManager.User? usr, FinanceModel financeModel)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return null;

      List<FinanceModel.DataPointSpec>[] dataPoints =
      [
        new List<FinanceModel.DataPointSpec>(), // Actual spec.
        new List<FinanceModel.DataPointSpec>(), // Stadium size
      ];

      List<CornerkickGame.Game.Data> ltGdPast = CkAppShared.ckMng.tl.getNextGames(clb, CkAppShared.ckMng.dtDatum, false, true);
      int i = 0;
      foreach (CornerkickGame.Game.Data gd in ltGdPast) {
        if (gd.team[0].iTeamId == clb.iId) {
          int iSpecTotal = gd.iSpectators[0] + gd.iSpectators[1] + gd.iSpectators[2];
          if (iSpecTotal > 0) {
            int[] iSpec = [gd.iSpectators[0], gd.iSpectators[1], gd.iSpectators[2]];

            CornerkickManager.Cup? c = CkAppShared.ckMng.ltCups.Find(c => c.iId == gd.iGameType);
            string sCupName = c != null ? c.sName : "";
            string sInfo = "<b>" + gd.dt.ToString("d", MemberController.getCi(usr)) + " - " + sCupName + "</b><br>" + gd.team[1].sTeam;

            dataPoints[0].Add(new FinanceModel.DataPointSpec(i, iSpecTotal, iSpec, z: sInfo));

            int iStadiumSeats = Math.Max(gd.stadium.getSeats(), iSpecTotal);
            if (iStadiumSeats > 0) {
              dataPoints[1].Add(new FinanceModel.DataPointSpec(i, gd.stadium.getSeats(), [gd.stadium.getSeats(0), gd.stadium.getSeats(1), gd.stadium.getSeats(2)], z: sInfo));
            }
            i--;
          }
        }
      }

      return dataPoints;
    }

    public static List<CkAppShared.DataPointIL>[] GetBudgetHistory(CornerkickManager.User? usr, FinanceModel financeModel)
    {
      List<CkAppShared.DataPointIL>[] dataPoints = new List<CkAppShared.DataPointIL>[2];

      if (usr?.ltBudget == null) return dataPoints;

      dataPoints[0] = new List<CkAppShared.DataPointIL>(); // Turnover
      dataPoints[1] = new List<CkAppShared.DataPointIL>(); // Profit

      for (int iY = 0; iY < usr.ltBudget.Count; iY++) {
        int iYear = CkAppShared.ckMng.iSeason - usr.ltBudget.Count + iY;

        long iInTotal  = CornerkickManager.Finance.getBudgetInTotal (usr.ltBudget[iY][1]);
        long iPayTotal = CornerkickManager.Finance.getBudgetPayTotal(usr.ltBudget[iY][1]);

        dataPoints[0].Add(new CkAppShared.DataPointIL(iYear, iInTotal, "<u>Saison: " + iYear.ToString() + "</u></br>Umsatz: " + iInTotal.ToString("N0", MemberController.getCi(usr)) + " €"));

        string sWinLoose;
        string sWinLooseColor;
        if (iInTotal - iPayTotal < 0) {
          sWinLoose = "Verlust";
          sWinLooseColor = "red";
        } else {
          sWinLoose = "Gewinn";
          sWinLooseColor = "green";
        }
        dataPoints[1].Add(new CkAppShared.DataPointIL(iYear, iInTotal - iPayTotal, "<u>Saison: " + iYear.ToString() + "</u></br>" + sWinLoose + ": <span style=\"color:" + sWinLooseColor + "\">" + (iInTotal - iPayTotal).ToString("N0", MemberController.getCi(usr)) + " €</span>"));
      }

      /*
      // Add current values
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Content("", "application/json");

      CornerkickManager.Finance.Budget bdCur = App.ckMng.ui.getActualBudget(clb);
      long iInTotalCur  = App.ckMng.fz.getBudgetInTotal (bdCur);
      long iPayTotalCur = App.ckMng.fz.getBudgetPayTotal(bdCur);
      string sInfoCur = "Saison: " + App.ckMng.iSeason.ToString() + "</br>Umsatz: " + iInTotalCur.ToString("N0", getCi()) + " €";
      dataPoints[0].Add(new Models.DataPointGeneral(App.ckMng.iSeason, iInTotalCur, sInfoCur));
      dataPoints[1].Add(new Models.DataPointGeneral(App.ckMng.iSeason, iInTotalCur - iPayTotalCur, "Gewinn: " + (iInTotalCur - iPayTotalCur).ToString("N0", getCi()) + " €"));
      */

      return dataPoints;
    }

  }
}
