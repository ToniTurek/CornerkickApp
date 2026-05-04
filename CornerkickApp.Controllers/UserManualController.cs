using CornerkickApp.Shared.Models;
using static CornerkickApp.Shared.Models.CkAppShared;
using System.Globalization;

namespace CornerkickApp.Controllers
{
  public class UserManualController
  {
    public static UserManualModel Get()
    {
      UserManualModel mdUm = new UserManualModel();

#if _WebApp
      byte iNationDefault = iNations[0];
#else
      byte iNationDefault = 36;
#endif

      CornerkickManager.Cup league = ckMng.tl.getCup(iCupIdLeague, iNationDefault, 0);
      if (league != null) mdUm.sAfLeague = league.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup cup = ckMng.tl.getCup(iCupIdNatCup, iNationDefault);
      if (cup != null) mdUm.sAfCup = cup.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup cupGold = ckMng.tl.getCup(iCupIdInt, iId2: 0);
      if (cupGold != null) {
        mdUm.sCupGoldAf = cupGold.settings.fAttraction.ToString("0.00");
        mdUm.sCupGoldBonus = (cupGold.settings.iBonusCupWin / 1000000.0).ToString("0");
        mdUm.sCupGoldBonusStart = (cupGold.settings.iBonusStart / 1000000.0).ToString("0.0");
      }

      CornerkickManager.Cup cupSilver = ckMng.tl.getCup(iCupIdInt, iId2: 1);
      if (cupSilver != null) {
        mdUm.sCupSilverAf = cupSilver.settings.fAttraction.ToString("0.00");
        mdUm.sCupSilverBonus = (cupSilver.settings.iBonusCupWin / 1000000.0).ToString("0");
        mdUm.sCupSilverBonusStart = (cupSilver.settings.iBonusStart / 1000000.0).ToString("0.0");
      }

      CornerkickManager.Cup cupBronze = ckMng.tl.getCup(iCupIdInt, iId2: 2);
      if (cupBronze != null) {
        mdUm.sCupBronzeAf = cupBronze.settings.fAttraction.ToString("0.00");
        mdUm.sCupBronzeBonus = (cupBronze.settings.iBonusCupWin / 1000000.0).ToString("0");
        mdUm.sCupBronzeBonusStart = (cupBronze.settings.iBonusStart / 1000000.0).ToString("0.0");
      }

      CornerkickManager.Cup cupWc = ckMng.tl.getCup(iCupIdWc);
      if (cupWc != null) mdUm.sAfWc = cupWc.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup tg = ckMng.tl.getCup(iCupIdTestgame);
      if (tg != null) mdUm.sAfTg = tg.settings.fAttraction.ToString("0.00");

      return mdUm;
    }

    public static List<DataPointII>[] UmGetStadiumCost()
    {
      CornerkickManager.User user = new CornerkickManager.User();
      user.sSurname = "manual";
      user.iLevel = 1;

      List<DataPointII>[] dataPoints = new List<DataPointII>[3];
      dataPoints[0] = new List<DataPointII>();
      dataPoints[1] = new List<DataPointII>();
      dataPoints[2] = new List<DataPointII>();

      int[] iCostDays;

      for (int iDp = 0; iDp < 3; iDp++) {
        CornerkickGame.Stadium stDatum = new CornerkickGame.Stadium();
        stDatum.blocks[0].iSeats = iDp * 1000;

        CornerkickGame.Stadium stNew = new CornerkickGame.Stadium();

        stNew.blocks[0].iSeats = stDatum.blocks[0].iSeats + 500;
        iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(stNew, stDatum, user);
        dataPoints[iDp].Add(new DataPointII(500, iCostDays[0]));

        stNew.blocks[0].iSeats = stDatum.blocks[0].iSeats + 1000;
        iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(stNew, stDatum, user);
        dataPoints[iDp].Add(new DataPointII(1000, iCostDays[0]));

        stNew.blocks[0].iSeats = stDatum.blocks[0].iSeats + 2000;
        iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(stNew, stDatum, user);
        dataPoints[iDp].Add(new DataPointII(2000, iCostDays[0]));
      }

      return dataPoints;
      //return Content(dataPoints, "application/json");
    }

    public List<DataPointLD>[] UmGetPlayerTraining(int iType, int iTrainerCondi, int iTrainerPhysio, int iCamp, int iDoping, int iAge)
    {
      List<DataPointLD>[] dataPoints = new List<DataPointLD>[3];

      // Initialize dataPoints list
      for (byte j = 0; j < dataPoints.Length; j++) dataPoints[j] = new List<DataPointLD>();

      // Create temp. ck manager instance
      CornerkickManager.Main mnUm = new CornerkickManager.Main();
      mnUm.dtDatum = mnUm.dtDatum.AddDays(1);

      // Create user
      CornerkickManager.User usr = new CornerkickManager.User();
      mnUm.ltUser.Add(usr);

      // Create club
      CornerkickManager.Club clb = new CornerkickManager.Club();
      mnUm.ltClubs.Add(clb);

      // Create player
      CornerkickManager.Player pl = new CornerkickManager.Player();
      pl.plGame.fCondition = 0.6f;
      pl.plGame.fFresh = 0.8f;
      pl.plGame.fMoral = 1.0f;
      pl.contract = CornerkickManager.PlayerTool.getContract(pl, 1, clb, mnUm.dtDatum, mnUm.dtSeasonEnd);
      pl.plGame.dtBirthday = mnUm.dtDatum.AddYears(-iAge);

      // Trainings camp
      CornerkickManager.TrainingCamp.Booking camp = new CornerkickManager.TrainingCamp.Booking();
      if (iCamp >= 0 && iCamp < ckMng.tcp.ltCamps.Count) {
        camp.camp = ckMng.tcp.ltCamps[iCamp];
        camp.dtDeparture = mnUm.dtDatum.AddDays(-1);
        camp.dtReturn = mnUm.dtDatum.AddDays(+8);
      }

      // Doping
      if (iDoping >= 0 && iDoping < ckMng.ltDoping.Count) pl.plGame.doDoping(ckMng.ltDoping[iDoping]);

      // For the next 7 days ...
      for (byte iD = 0; iD < 7; iD++) {
        DateTime dtTmp = mnUm.dtDatum.AddDays(iD);

        if (iD > 0) {
          //if ((int)dtTmp.DayOfWeek == 0) break;

          // Reduce doping effect
          if (pl.plGame.doping != null && pl.plGame.doping.fEffect > 0.001f) pl.plGame.doping.fEffect -= (pl.plGame.doping.fEffect * pl.plGame.doping.fReductionRate);

          // ... do training
          CornerkickManager.PlayerTool.Training training = CornerkickManager.PlayerTool.getTraining(iType, ckMng.plt.ltTraining);
          CornerkickManager.PlayerTool.doTraining(ref pl, training, ckMng.plt.ltTraining, iTrainerCondi, iTrainerPhysio, 2, 2, dtTmp, usr, iTrainingPerDay: 1, ltPlayerTeam: null, campBooking: camp, bJouth: false, bNoInjuries: true);
        }

        // ... add training data to dataPoints
        dataPoints[0].Add(new DataPointLD(iD + 1, pl.plGame.fCondition));
        dataPoints[1].Add(new DataPointLD(iD + 1, pl.plGame.fFresh));
        dataPoints[2].Add(new DataPointLD(iD + 1, pl.plGame.fMoral));
      }

      return dataPoints;
    }

    public List<DataPointLD>[] UmGetPlayerStepsFreshLoss(int iSpeed, int iAcceleration, int iStepsLast, string sFreshStart, string sCondition, string sForm, string sTcPower)
    {
      List<DataPointLD>[] dataPoints = new List<DataPointLD>[3];

      float fTcPower = 0.0f;
      if (!float.TryParse(sTcPower, NumberStyles.Float, CultureInfo.InvariantCulture, out fTcPower)) return null;

      // Create player
      CornerkickGame.Player pl = new CornerkickGame.Player();
      if (!float.TryParse(sCondition, NumberStyles.Float, CultureInfo.InvariantCulture, out pl.fCondition)) return null;
      if (!float.TryParse(sForm, NumberStyles.Float, CultureInfo.InvariantCulture, out pl.fForm)) return null;
      if (!float.TryParse(sFreshStart, NumberStyles.Float, CultureInfo.InvariantCulture, out pl.fFresh)) return null;
      pl.iSkill[0] = (byte)iSpeed;
      pl.iSkill[16] = (byte)iAcceleration;
      pl.fMoral = 1.0f;
      pl.fExperiencePos[10] = 1f;

      CornerkickGame.Tool.setPlayerSteps(pl, 11, 0);
      pl.iStepsCurr = 0;
      pl.iStepsLast = (byte)iStepsLast;

      // Create player with no acceleration effect
      CornerkickGame.Player plNoAcc = pl.Clone();
      plNoAcc.iStepsLast = (byte)iSpeed;

      // Initialize dataPoints list
      for (byte j = 0; j < dataPoints.Length; j++) dataPoints[j] = new List<DataPointLD>();

      // For all steps ...
      byte iS = 0;
      while (pl.fSteps > 0) {
        // ... add fresh
        dataPoints[0].Add(new DataPointLD(iS, pl.fFresh));
        dataPoints[1].Add(new DataPointLD(iS, plNoAcc.fFresh));
        if (CornerkickGame.Rules.getFreshPlayerMoveLimit(pl.fSteps, pl.fFresh, fTcPower) < 1f) dataPoints[2].Add(new DataPointLD(iS, 1f));
        else                                                                                   dataPoints[2].Add(new DataPointLD(iS, 0f));

        pl.fSteps -= 1f;
        plNoAcc.fSteps -= 1f;

        pl.iStepsCurr++;
        plNoAcc.iStepsCurr++;

        // Reduce fresh
        float fAcceleration = CornerkickGame.Tool.getSkillEff(pl, 16, 11);
        pl.fFresh -= CornerkickGame.Rules.getFreshLoss(pl, 15, fAcceleration);
        plNoAcc.fFresh -= CornerkickGame.Rules.getFreshLoss(plNoAcc, 15, fAcceleration);

        iS++;
      }

      return dataPoints;
    }

    public List<DataPointDoubleXY>[] UmGetChartPlayerDuel(int iMode, int iDuelDef, int iDuelOff, byte iDuelPos, string sVar, bool bInOwnBox, bool bLastPlayer, bool bAlreadyYellowCard)
    {
      List<DataPointDoubleXY>[] dataPoints = new List<DataPointDoubleXY>[8];

      float fVar = float.Parse(sVar, NumberStyles.Float, CultureInfo.InvariantCulture);

      // Initialize dataPoints list
      for (byte j = 0; j < dataPoints.Length; j++) dataPoints[j] = new List<DataPointDoubleXY>();

      float fLoop = -1f;
      if (iMode == 0) fLoop = 0f;

      float fDuelPosPitchRel;
      float fTcAgg;

      // For all steps ...
      while (fLoop < 1.001f) {
        fLoop = (float)Math.Round(fLoop, 3);

        if (iMode == 0) {
          fDuelPosPitchRel = fLoop;
          fTcAgg = fVar;
        } else {
          fDuelPosPitchRel = fVar;
          fTcAgg = fLoop;
        }

        double[] fDuelChances = CornerkickGame.Rules.getChancesDuel(iDuelDef, iDuelOff, iDuelPos, fTcAgg, 1f - fDuelPosPitchRel, bInOwnBox: bInOwnBox, bLastPlayer: bLastPlayer, bAlreadyYellowCard: bAlreadyYellowCard);

        for (int i = 0; i < fDuelChances.Length; i++) dataPoints[i].Add(new DataPointDoubleXY(fLoop, fDuelChances[i]));

        dataPoints[5].Add(new DataPointDoubleXY(fLoop, CornerkickGame.Rules.getDuelStepReduction(CornerkickGame.Rules.getChanceDuelWin(iDuelDef, iDuelOff, fTcAgg, iTackleSector: iDuelPos), 0.5)));
        dataPoints[6].Add(new DataPointDoubleXY(fLoop, CornerkickGame.Rules.getDuelStepReduction(CornerkickGame.Rules.getChanceDuelWin(iDuelDef, iDuelOff, fTcAgg, iTackleSector: iDuelPos), 1.0)));

        fLoop += 0.01f;
      }

      return dataPoints;
    }

    public static List<DataPointDD> GetErf()
    {
      List<DataPointDD> dataPoints = new List<DataPointDD>();

      for (double x = -2.5; x < 2.501; x += 0.01) {
        dataPoints.Add(new DataPointDD(x, CornerkickGame.Tool.erf(x), z: "erf"));
      }

      return dataPoints;
    }
    public static List<DataPointDD> GetInvErf()
    {
      List<DataPointDD> dataPoints = new List<DataPointDD>();

      for (double y = 0.01; y < 1.0; y += 0.01) {
        dataPoints.Add(new DataPointDD(y, CornerkickGame.Tool.inv_erf(y), z: "inverse erf"));
      }

      return dataPoints;
    }
    public static List<DataPointDD>[] GetProbabilityDensity()
    {
      List<DataPointDD>[] dataPoints = new List<DataPointDD>[3];
      dataPoints[0] = new List<DataPointDD>();
      dataPoints[1] = new List<DataPointDD>();
      dataPoints[2] = new List<DataPointDD>();

      Meta.Numerics.Statistics.Distributions.ContinuousDistribution normal = new Meta.Numerics.Statistics.Distributions.NormalDistribution();
      Meta.Numerics.Statistics.Distributions.ContinuousDistribution gumbel = new Meta.Numerics.Statistics.Distributions.GumbelDistribution();
      Meta.Numerics.Statistics.Distributions.ContinuousDistribution pareto = new Meta.Numerics.Statistics.Distributions.ParetoDistribution(0.5, 0.5);
      for (double x = -5.0; x < 5.001; x += 0.1) {
        dataPoints[0].Add(new DataPointDD(x, normal.ProbabilityDensity(x), z: "normal prob. density"));
        dataPoints[1].Add(new DataPointDD(x, gumbel.ProbabilityDensity(x), z: "gumbel prob. density"));
        dataPoints[2].Add(new DataPointDD(x, pareto.ProbabilityDensity(x), z: "pareto prob. density"));
      }

      return dataPoints;
    }
    public static List<DataPointDD>[] GetUmScoutProbabilityDensity(int iSkill)
    {
      List<DataPointDD>[] dataPoints = new List<DataPointDD>[5];
      for (int i = 0; i < dataPoints.Length; i++) dataPoints[i] = new List<DataPointDD>();

      CornerkickManager.Main.Staff.Scout scout = new CornerkickManager.Main.Staff.Scout(iSkill: (byte)iSkill);
      double sig_2 = Math.Pow(scout.getSigma(), 2.0);

      Meta.Numerics.Statistics.Distributions.ContinuousDistribution normal = new Meta.Numerics.Statistics.Distributions.NormalDistribution(mu: 0.0, sigma: Math.Sqrt(sig_2));

      for (double x = -3.0; x < 3.001; x += 0.1) {
        x = Math.Round(x, 4);
        //dataPoints[Math.Max((int)Math.Ceiling(Math.Round(x, 4)) + 1, 0)].Add(new DataPointDD(x, normal.ProbabilityDensity(x), desc: "sig^2=" + sig_2.ToString("0.0")));
        if (x <= -2.0)              dataPoints[0].Add(new DataPointDD(x, normal.ProbabilityDensity(x), z: "sig^2=" + sig_2.ToString("0.0")));
        if (x >= -2.0 && x <= -1.0) dataPoints[1].Add(new DataPointDD(x, normal.ProbabilityDensity(x)));
        if (x >= -1.0 && x <= +1.0) dataPoints[2].Add(new DataPointDD(x, normal.ProbabilityDensity(x)));
        if (x >= +1.0 && x <= +2.0) dataPoints[3].Add(new DataPointDD(x, normal.ProbabilityDensity(x)));
        if (x >= +2.0)              dataPoints[4].Add(new DataPointDD(x, normal.ProbabilityDensity(x)));
      }

      return dataPoints;
    }
    public static List<DataPointDD>[] GetNormalDistribution()
    {
      List<DataPointDD>[] dataPoints = new List<DataPointDD>[4];
      dataPoints[0] = new List<DataPointDD>();
      dataPoints[1] = new List<DataPointDD>();
      dataPoints[2] = new List<DataPointDD>();
      dataPoints[3] = new List<DataPointDD>();

      int k = 0;
      foreach (double sig_2 in new double[] { 0.2, 0.5, 1.0, 5.0 }) {
        Meta.Numerics.Statistics.Distributions.ContinuousDistribution normal = new Meta.Numerics.Statistics.Distributions.NormalDistribution(mu: 0.0, sigma: Math.Sqrt(sig_2));

        for (double x = -4.0; x < 4.001; x += 0.01) {
          dataPoints[k].Add(new DataPointDD(x, normal.ProbabilityDensity(x), z: "sig^2=" + sig_2.ToString("0.0")));
        }

        k++;
      }

      return dataPoints;
    }
    public static List<DataPointDD>[] GetInverseNormalDistribution()
    {
      List<DataPointDD>[] dataPoints = new List<DataPointDD>[4];
      dataPoints[0] = new List<DataPointDD>();
      dataPoints[1] = new List<DataPointDD>();
      dataPoints[2] = new List<DataPointDD>();
      dataPoints[3] = new List<DataPointDD>();

      int k = 0;
      foreach (double sig_2 in new double[] { 0.2, 0.5, 1.0, 5.0 }) {
        Meta.Numerics.Statistics.Distributions.ContinuousDistribution normal = new Meta.Numerics.Statistics.Distributions.NormalDistribution(mu: 0.0, sigma: Math.Sqrt(sig_2));

        for (double y = 0.0; y < 1.0; y += 0.001) {
          dataPoints[k].Add(new DataPointDD(y, normal.InverseLeftProbability(y), z: "sig^2=" + sig_2.ToString("0.0")));
        }

        k++;
      }

      return dataPoints;
    }
    public static List<DataPointDD>[] GetDistFct()
    {
      List<DataPointDD>[] dataPoints = new List<DataPointDD>[4];
      dataPoints[0] = new List<DataPointDD>();
      dataPoints[1] = new List<DataPointDD>();
      dataPoints[2] = new List<DataPointDD>();
      dataPoints[3] = new List<DataPointDD>();

      for (double x = -4.0; x < 4.001; x += 0.01) {
        int k = 0;
        foreach (double sig_2 in new double[] { 0.2, 0.5, 1.0, 5.0 }) {
          dataPoints[k++].Add(new DataPointDD(x, CornerkickGame.Tool.distribution_fct(x, Math.Sqrt(sig_2)), z: "sigma^2=" + sig_2.ToString("0.0")));
        }
      }

      return dataPoints;
    }
    public static List<DataPointDD> GetUmScoutDistFct(int iSkill)
    {
      List<DataPointDD> dataPoints = new List<DataPointDD>();

      CornerkickManager.Main.Staff.Scout scout = new CornerkickManager.Main.Staff.Scout(iSkill: (byte)iSkill);
      double sig_2 = Math.Pow(scout.getSigma(), 2.0);

      for (double x = -3.0; x < 3.01; x += 0.1) {
        dataPoints.Add(new DataPointDD(x, CornerkickGame.Tool.distribution_fct(x, Math.Sqrt(sig_2)), z: "sig^2=" + sig_2.ToString("0.0")));
      }

      return dataPoints;
    }
  }
}
