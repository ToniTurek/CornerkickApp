using CornerkickApp.Shared.Models;
using System.Text.Json.Serialization;
using static CornerkickApp.Shared.Models.CkAppShared;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  [JsonSourceGenerationOptions(WriteIndented = true)]
  [JsonSerializable(typeof(List<CornerkickManager.Main.TrainingPlan.Unit[]>))]
  //[JsonSerializable(typeof(CornerkickManager.Main.TrainingPlan.Unit))]
  public partial class SourceGenerationContext : JsonSerializerContext
  {
  }

  public class TrainingController
  {
    readonly static TimeSpan[] tsTraining = new TimeSpan[] { new TimeSpan(9, 30, 00), new TimeSpan(12, 00, 00), new TimeSpan(16, 30, 00) };
    public static TrainingModel Model(CornerkickManager.User _usr)
    {
      TrainingModel model = new TrainingModel();

      model.iWeekIni = 0;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return model;

      if (ckMng.dtDatum.DayOfWeek == DayOfWeek.Saturday && ckMng.dtDatum.TimeOfDay > new TimeSpan(15, 30, 0)) model.iWeekIni = 1;

      model.iTrainingCount = new int[ckMng.plt.ltTraining.Count];

      int nTrTotal = 0;
      foreach (CornerkickManager.Player.TrainingHistory th in clb.ltTrainingHist) {
        bool bFound = false;
        foreach (TimeSpan tsTr in tsTraining) {
          if (tsTr.Equals(th.dt.TimeOfDay)) {
            bFound = true;
            break;
          }
        }
        if (!bFound) continue;

        if (th.iType > 0 && th.iType < model.iTrainingCount.Length) {
          model.iTrainingCount[th.iType]++;
          nTrTotal++;
        }
      }

      model.sTrainingCountRel = new string[model.iTrainingCount.Length];
      for (int iT = 0; iT < model.iTrainingCount.Length; iT++) {
        if (nTrTotal > 0) {
          model.sTrainingCountRel[iT] = (model.iTrainingCount[iT] / (float)nTrTotal).ToString("0.0%");
        } else {
          model.sTrainingCountRel[iT] = "-";
        }
      }

      // Training rules
      const int iTrRuleInpts = 3;
      string[][] sTrTypes = { new string[] { "Aufbau", "-2" }, new string[] { "Kondition", "2" }, new string[] { "Regeneration", "1" } };
      model.tripts = new TrainingRulesInput[iTrRuleInpts];
      for (int iTri = 0; iTri < iTrRuleInpts; iTri++) {
        model.tripts[iTri] = new TrainingRulesInput();

        model.tripts[iTri].sliTrainingRulesCFM.Add(new SelectListItem { Text = "Kondition", Value = "0" });
        model.tripts[iTri].sliTrainingRulesCFM.Add(new SelectListItem { Text = "Frische",   Value = "1" });

        for (int iTrTypes = 0; iTrTypes < sTrTypes.Length; iTrTypes++) {
          model.tripts[iTri].sliTrainingRulesType.Add(new SelectListItem { Text = sTrTypes[iTrTypes][0], Value = sTrTypes[iTrTypes][1] });
        }

        model.tripts[iTri].sliTrainingRulesSmGr.Add(new SelectListItem { Text = "kleiner", Value = "-1" });
        model.tripts[iTri].sliTrainingRulesSmGr.Add(new SelectListItem { Text = "größer",  Value =  "1" });
      }

      // Set club training rules
      if (clb.training.ltRule != null) {
        for (int iTrR = 0; iTrR < clb.training.ltRule.Count; iTrR++) {
          if (iTrR >= model.tripts.Length) break;
          if (clb.training.ltRule[iTrR].fValue < 0f) continue;
          if (clb.training.ltRule[iTrR].fValue > 1f) continue;

          model.tripts[iTrR].iTrainingRulesCFM = clb.training.ltRule[iTrR].iCFM;
          model.tripts[iTrR].iTrainingRulesSmGr = clb.training.ltRule[iTrR].bSmaller ? -1 : +1;
          model.tripts[iTrR].fTrainingRulesValue = clb.training.ltRule[iTrR].fValue * 100f;
          model.tripts[iTrR].iTrainingRulesType = clb.training.ltRule[iTrR].iType;
        }
      }

      byte[] ltSkills = new byte[] {
        CornerkickGame.Game.iSkillIxSpeed,
        CornerkickGame.Game.iSkillIxAcceleration,
        CornerkickGame.Game.iSkillIxJump,
        CornerkickGame.Game.iSkillIxTechnic,
        CornerkickGame.Game.iSkillIxDuelOff,
        CornerkickGame.Game.iSkillIxDuelDef,
        CornerkickGame.Game.iSkillIxLowPassPower,
        CornerkickGame.Game.iSkillIxHighPassPower,
        CornerkickGame.Game.iSkillIxShootPower,
        CornerkickGame.Game.iSkillIxLowPassAcc,
        CornerkickGame.Game.iSkillIxHighPassAcc,
        CornerkickGame.Game.iSkillIxShootAcc,
        CornerkickGame.Game.iSkillIxFreekick,
        CornerkickGame.Game.iSkillIxHeader,
        CornerkickGame.Game.iSkillIxPenalty,
        CornerkickGame.Game.iSkillIxReaction,
        CornerkickGame.Game.iSkillIxCatch
      };
      model.lt_ind_skills = new List<object>();
      foreach (byte iS in ltSkills) model.lt_ind_skills.Add(new { name = CornerkickManager.Names.sSkills[iS], value = iS });

      // Tutorial
      if (ttUser != null) {
        CornerkickManager.User usr = _usr;
        int iUserIx = ckMng.ltUser.IndexOf(usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) model.tutorial = ttUser[iUserIx];
      }

      return model;
    }

    public static Task<CornerkickManager.Main.TrainingPlan.Unit[][]>? getTrainingPlan(CornerkickManager.User? _usr, int iWeek)
    {
      if (_usr == null) return null;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      // Get last Sunday
      DateTime dtSunday = ckMng.dtDatum.Date.AddDays(iWeek * 7);
      while (dtSunday.DayOfWeek != DayOfWeek.Sunday) dtSunday = dtSunday.AddDays(-1);

      CornerkickManager.Main.TrainingPlan.Unit[][] ltTu = new CornerkickManager.Main.TrainingPlan.Unit[7][]; // For each day of week
      for (int iD = 0; iD < ltTu.Length; iD++) { // Loop until Saturday
        ltTu[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        // Add past trainings
        foreach (CornerkickManager.Player.TrainingHistory th in clb.ltTrainingHist) {
          if (th.dt.Date.Equals(dtSunday.AddDays(iD))) {
            int iIxTimeOfDay = 0; // 1st training
            if      (th.dt.Hour >= tsTraining[2].Hours) iIxTimeOfDay = 2; // 3rd training
            else if (th.dt.Hour >= tsTraining[1].Hours) iIxTimeOfDay = 1; // 2nd training

            if (ltTu[iD][iIxTimeOfDay] != null && ltTu[iD][iIxTimeOfDay].iType < 0) continue; // Already set

            CornerkickManager.Main.TrainingPlan.Unit tuPast = new CornerkickManager.Main.TrainingPlan.Unit();
            tuPast.dt = th.dt;
            tuPast.iType = (sbyte)-(th.iType + 1);

            ltTu[iD][iIxTimeOfDay] = tuPast;
          }
        }

        // Add future trainings
        foreach (CornerkickManager.Main.TrainingPlan.Unit tu in clb.training.ltUnit) {
          if (tu.dt.Date.Equals(dtSunday.AddDays(iD))) {
            int iIxTimeOfDay = 0; // 1st training
            if (tu.dt.Hour >= tsTraining[2].Hours) iIxTimeOfDay = 2; // 3rd training
            else if (tu.dt.Hour >= tsTraining[1].Hours) iIxTimeOfDay = 1; // 2nd training

            //if (tu.dt.CompareTo(ckMng.dtDatum) <= 0) tu.iType *= -1;

            ltTu[iD][iIxTimeOfDay] = tu;
          }
        }
      }

      List<CornerkickGame.Game.Data> ltGames = ckMng.tl.getNextGames(clb, dtSunday);

      for (int iD = 0; iD < ltTu.Length; iD++) { // Loop until Saturday
        for (int iT = 0; iT < ltTu[iD].Length; iT++) {
          // Set training type 'free' if null
          if (ltTu[iD][iT] == null) {
            DateTime dtTraining = dtSunday.AddDays(iD).Add(tsTraining[iT]);

            sbyte iType = 0;
            if (dtTraining.CompareTo(ckMng.dtDatum) <= 0) iType = -1; // Past training

            ltTu[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit() { dt = dtTraining, iType = iType };
          }

          // Set game/travel/event
          int iEvent = CornerkickManager.Tool.checkIfGameTravelEventIsClose(clb, ltTu[iD][iT].dt, ltGames: ltGames);
          if (iEvent == 0) iEvent = CornerkickManager.Tool.checkIfGameTravelEventIsClose(clb, ltTu[iD][iT].dt.Add(ckMng.settings.tsTrainingLength), ltGames: ltGames);

          if (iEvent > 0) {
            ltTu[iD][iT].iType = (sbyte)(100 + iEvent);
          }
        }
      }

      return Task.FromResult(ltTu);
    }

#if false
    [HttpPost]
    public JsonResult TrainingGetPlan(int iWeek)
    {
      return Json(JsonConvert.SerializeObject(getTrainingPlan(ckClub(), iWeek)));
    }
#endif

    public static int setTraining(CornerkickManager.User _usr, int iTrainingType, int iDay, int iIxTimeOfDay)
    {
      CornerkickManager.Club clb = MemberController.ckClub(_usr);
      if (clb == null) return -1;

      // Get last Sunday
      DateTime dtSunday = ckMng.dtDatum.Date;
      while ((int)(dtSunday.DayOfWeek) != 0) dtSunday = dtSunday.AddDays(-1);

      DateTime dtTraining = dtSunday.AddDays(iDay).Add(tsTraining[iIxTimeOfDay]);

      if (dtTraining.CompareTo(ckMng.dtDatum) < 0) return -1; // Return, if in past

      CornerkickManager.Main.TrainingPlan.Unit tu = clb.training.getTrainingUnit(dtTraining);
      if (tu == null) {
        tu = new CornerkickManager.Main.TrainingPlan.Unit();
        tu.dt = dtTraining;
        clb.training.ltUnit.Add(tu);
      }

      tu.iType = (sbyte)iTrainingType;

      return iTrainingType;
    }

    public static void copyTrainingPlan(CornerkickManager.User? _usr, int iWeek)
    {
      if (_usr == null) return;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return;

      Task<CornerkickManager.Main.TrainingPlan.Unit[][]>? tkTp = getTrainingPlan(_usr, iWeek);
      if (tkTp == null) return;

      CornerkickManager.Main.TrainingPlan.Unit[][]? tuPlan = tkTp.Result;

      // Get next Sunday
      DateTime dtStartCopy = ckMng.dtDatum.AddDays(iWeek * 7).Date;
      while (dtStartCopy.DayOfWeek != DayOfWeek.Sunday) dtStartCopy = dtStartCopy.AddDays(+1);

      // First delete all trainings starting from next week ...
      DateTime dtTmp = dtStartCopy;
      while (dtTmp.CompareTo(ckMng.dtSeasonEnd) < 0) {
        for (int iT = 0; iT < clb.training.ltUnit.Count; iT++) {
          if (clb.training.ltUnit[iT].dt.Date.Equals(dtTmp)) {
            clb.training.ltUnit.RemoveAt(iT--);
          }
        }

        dtTmp = dtTmp.AddDays(+1);
      }

      // ... then copy trainings plan of current week
      dtTmp = dtStartCopy;
      while (dtTmp.CompareTo(ckMng.dtSeasonEnd) < 0) {
        for (byte iD = 0; iD < tuPlan.Length; iD++) {
          for (byte iT = 0; iT < tuPlan[iD].Length; iT++) {
            if (tuPlan[iD][iT].iType != 0 && tuPlan[iD][iT].iType < 100) { // // Not free and not game
              CornerkickManager.Main.TrainingPlan.Unit tuCopy = tuPlan[iD][iT].Clone();
              tuCopy.dt = dtTmp.Add(tuPlan[iD][iT].dt.TimeOfDay);

              if (tuCopy.iType < 0) tuCopy.iType = (sbyte)-(tuCopy.iType + 1);

              clb.training.ltUnit.Add(tuCopy);
            }
          }

          dtTmp = dtTmp.AddDays(+1);
        }
      }
    }

    public class TrainingWeekTemplate
    {
      public string sName { get; set; } = "";
      public CornerkickManager.Main.TrainingPlan.Unit[][] tuPlan { get; set; }
    }
    public static List<TrainingWeekTemplate> ltTrainingWeekTemplate;

#if false
    [HttpPost]
    public JsonResult TrainingSetTemplate(int iWeek, int iType)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false);

      setTrainingWeekTemplate(clb, iWeek, iType);

      return Json(true);
    }
#endif

    public static void setTrainingWeekTemplate(CornerkickManager.User? _usr, int iWeek, int iType)
    {
      if (iType < 0) return;
      if (_usr == null) return;
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return;

      // Define training week templates
      ltTrainingWeekTemplate = new List<TrainingWeekTemplate>();

      // Condition
      TrainingWeekTemplate twt1 = new TrainingWeekTemplate();
      twt1.sName = "Kondition";
      twt1.tuPlan = new CornerkickManager.Main.TrainingPlan.Unit[7][];
      for (int iD = 0; iD < 7; iD++) {
        twt1.tuPlan[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        for (int iT = 0; iT < twt1.tuPlan[iD].Length; iT++) {
          twt1.tuPlan[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit();
        }
      }
      twt1.tuPlan[1][0].iType = 2;
      twt1.tuPlan[1][1].iType = 2;
      twt1.tuPlan[1][2].iType = 1;
      twt1.tuPlan[2][0].iType = 2;
      twt1.tuPlan[2][1].iType = 3;
      twt1.tuPlan[2][2].iType = 9;
      twt1.tuPlan[3][0].iType = 2;
      twt1.tuPlan[3][1].iType = 4;
      twt1.tuPlan[3][2].iType = 1;
      twt1.tuPlan[4][0].iType = 3;
      twt1.tuPlan[4][1].iType = 5;
      twt1.tuPlan[4][2].iType = 6;
      twt1.tuPlan[5][0].iType = 2;
      twt1.tuPlan[5][1].iType = 3;
      twt1.tuPlan[6][1].iType = 1;
      ltTrainingWeekTemplate.Add(twt1);

      // Regeneration
      TrainingWeekTemplate twt2 = new TrainingWeekTemplate();
      twt2.sName = "Regeneration";
      twt2.tuPlan = new CornerkickManager.Main.TrainingPlan.Unit[7][];
      for (int iD = 0; iD < 7; iD++) {
        twt2.tuPlan[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        for (int iT = 0; iT < twt2.tuPlan[iD].Length; iT++) {
          twt2.tuPlan[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit();
        }
      }
      twt2.tuPlan[1][0].iType = 1;
      twt2.tuPlan[1][1].iType = 10;
      twt2.tuPlan[2][0].iType = 7;
      twt2.tuPlan[2][1].iType = 1;
      twt2.tuPlan[2][2].iType = 12;
      twt2.tuPlan[3][0].iType = 6;
      twt2.tuPlan[3][1].iType = 1;
      twt2.tuPlan[4][0].iType = 3;
      twt2.tuPlan[4][1].iType = 4;
      twt2.tuPlan[4][2].iType = 1;
      twt2.tuPlan[5][0].iType = 8;
      twt2.tuPlan[5][1].iType = 1;
      twt2.tuPlan[6][1].iType = 1;
      ltTrainingWeekTemplate.Add(twt2);

      // Mixed
      TrainingWeekTemplate twt3 = new TrainingWeekTemplate();
      twt3.sName = "Ausgeglichen";
      twt3.tuPlan = new CornerkickManager.Main.TrainingPlan.Unit[7][];
      for (int iD = 0; iD < 7; iD++) {
        twt3.tuPlan[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        for (int iT = 0; iT < twt3.tuPlan[iD].Length; iT++) {
          twt3.tuPlan[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit();
        }
      }
      twt3.tuPlan[1][0].iType = 2;
      twt3.tuPlan[1][1].iType = 3;
      twt3.tuPlan[2][0].iType = 4;
      twt3.tuPlan[2][1].iType = 8;
      twt3.tuPlan[2][2].iType = 12;
      twt3.tuPlan[3][0].iType = 5;
      twt3.tuPlan[3][1].iType = 9;
      twt3.tuPlan[4][0].iType = 3;
      twt3.tuPlan[4][1].iType = 10;
      twt3.tuPlan[4][2].iType = 1;
      twt3.tuPlan[5][0].iType = 7;
      twt3.tuPlan[5][1].iType = 6;
      twt3.tuPlan[6][0].iType = 1;
      twt3.tuPlan[6][1].iType = 1;
      ltTrainingWeekTemplate.Add(twt3);

      // Mental
      TrainingWeekTemplate twt4 = new TrainingWeekTemplate();
      twt4.sName = "Ausgeglichen";
      twt4.tuPlan = new CornerkickManager.Main.TrainingPlan.Unit[7][];
      for (int iD = 0; iD < 7; iD++) {
        twt4.tuPlan[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        for (int iT = 0; iT < twt4.tuPlan[iD].Length; iT++) {
          twt4.tuPlan[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit();
        }
      }
      twt4.tuPlan[1][0].iType = 2;
      twt4.tuPlan[1][1].iType = 3;
      twt4.tuPlan[2][0].iType = 4;
      twt4.tuPlan[2][1].iType = 8;
      twt4.tuPlan[2][2].iType = 12;
      twt4.tuPlan[3][0].iType = 5;
      twt4.tuPlan[3][1].iType = 9;
      twt4.tuPlan[4][0].iType = 3;
      twt4.tuPlan[4][1].iType = 10;
      twt4.tuPlan[4][2].iType = 1;
      twt4.tuPlan[5][0].iType = 7;
      twt4.tuPlan[5][1].iType = 6;
      twt4.tuPlan[6][0].iType = 1;
      twt4.tuPlan[6][1].iType = 1;
      ltTrainingWeekTemplate.Add(twt4);

      // Duel
      TrainingWeekTemplate twt5 = new TrainingWeekTemplate();
      twt5.sName = "Zweikampf";
      twt5.tuPlan = new CornerkickManager.Main.TrainingPlan.Unit[7][];
      for (int iD = 0; iD < 7; iD++) {
        twt5.tuPlan[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        for (int iT = 0; iT < twt5.tuPlan[iD].Length; iT++) {
          twt5.tuPlan[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit();
        }
      }
      twt5.tuPlan[1][0].iType = 2;
      twt5.tuPlan[1][1].iType = 4;
      twt5.tuPlan[2][0].iType = 5;
      twt5.tuPlan[2][1].iType = 4;
      twt5.tuPlan[2][2].iType = 12;
      twt5.tuPlan[3][0].iType = 3;
      twt5.tuPlan[3][1].iType = 11;
      twt5.tuPlan[3][2].iType = 9;
      twt5.tuPlan[4][0].iType = 4;
      twt5.tuPlan[4][1].iType = 9;
      twt5.tuPlan[4][2].iType = 1;
      twt5.tuPlan[5][0].iType = 5;
      twt5.tuPlan[5][1].iType = 1;
      twt5.tuPlan[6][0].iType = 1;
      twt5.tuPlan[6][1].iType = 1;
      ltTrainingWeekTemplate.Add(twt5);

      // Shoot
      TrainingWeekTemplate twt6 = new TrainingWeekTemplate();
      twt6.sName = "Abschluss";
      twt6.tuPlan = new CornerkickManager.Main.TrainingPlan.Unit[7][];
      for (int iD = 0; iD < 7; iD++) {
        twt6.tuPlan[iD] = new CornerkickManager.Main.TrainingPlan.Unit[3];

        for (int iT = 0; iT < twt6.tuPlan[iD].Length; iT++) {
          twt6.tuPlan[iD][iT] = new CornerkickManager.Main.TrainingPlan.Unit();
        }
      }
      twt6.tuPlan[1][0].iType = 7;
      twt6.tuPlan[1][1].iType = 6;
      twt6.tuPlan[2][0].iType = 11;
      twt6.tuPlan[2][1].iType = 8;
      twt6.tuPlan[2][2].iType = 9;
      twt6.tuPlan[3][0].iType = 3;
      twt6.tuPlan[3][1].iType = 13;
      twt6.tuPlan[4][0].iType = 5;
      twt6.tuPlan[4][1].iType = 7;
      twt6.tuPlan[4][2].iType = 1;
      twt6.tuPlan[5][0].iType = 9;
      twt6.tuPlan[5][1].iType = 6;
      twt6.tuPlan[6][0].iType = 1;
      twt6.tuPlan[6][1].iType = 1;
      ltTrainingWeekTemplate.Add(twt6);

      if (iType >= ltTrainingWeekTemplate.Count) return;

      // Get last Sunday
      DateTime dtStartCopy = ckMng.dtDatum.AddDays(iWeek * 7).Date;
      while (dtStartCopy.DayOfWeek != DayOfWeek.Sunday) dtStartCopy = dtStartCopy.AddDays(-1);

      // First delete all trainings in this week ...
      /*
      CornerkickManager.Main.TrainingPlan.Unit[][] tuPlan = getTrainingPlan(clb, iWeek);
      tuPlan = null;
      */
      DateTime dtTmp = dtStartCopy;
      while (dtTmp.CompareTo(dtStartCopy.AddDays(7)) < 0) {
        for (int iT = 0; iT < clb.training.ltUnit.Count; iT++) {
          if (clb.training.ltUnit[iT].dt.Date.Equals(dtTmp)) {
            clb.training.ltUnit.RemoveAt(iT--);
          }
        }

        dtTmp = dtTmp.AddDays(+1);
      }

      TrainingWeekTemplate twt = ltTrainingWeekTemplate[iType];
      for (int iD = 0; iD < 7; iD++) {
        for (int iT = 0; iT < twt.tuPlan[iD].Length; iT++) {
          DateTime dtCopy = dtStartCopy.AddDays(iD).Add(tsTraining[iT]);
          if (dtCopy.CompareTo(ckMng.dtDatum) < 0) continue;

          CornerkickManager.Main.TrainingPlan.Unit tuCopy = twt.tuPlan[iD][iT].Clone();
          tuCopy.dt = dtCopy;
          clb.training.ltUnit.Add(tuCopy);
        }
      }
    }

    public static object TrainingSetTrainingRule(CornerkickManager.User? _usr, int iRule, int iCFM, int iSmGr, float fValue, int iType)
    {
      if (_usr == null) return new { ok = false, message = "User nicht gefunden!" };

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return new { ok = false, message = "Kein Verein gefunden!" };

      if (fValue <   0) return new { ok = false, message = "Wert muss größer als 0 sein!" };
      if (fValue > 100) return new { ok = false, message = "Wert muss kleiner als 100 sein!" };

      try {
        if (clb.training.ltRule == null) clb.training.ltRule = new List<CornerkickManager.Main.TrainingPlan.Rule>();
        while (clb.training.ltRule.Count <= iRule) clb.training.ltRule.Add(new CornerkickManager.Main.TrainingPlan.Rule());

        CornerkickManager.Main.TrainingPlan.Rule rule = clb.training.ltRule[iRule];
        rule.iCFM = (byte)iCFM;
        rule.bSmaller = iSmGr < 0;
        rule.fValue = (float)Math.Round(fValue * 0.01f, 3);
        rule.iType = iType;
        clb.training.ltRule[iRule] = rule;
      } catch (Exception e) {
        return new { ok = false, message = e.Message };
      }

      return new { ok = true };
    }

    public static List<TrainingModel.TableItem>? getTableTeam(CornerkickManager.User? _usr)
    {
      if (_usr == null) return null;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      // Get staff for scouting
      CornerkickManager.Main.Staff? staff = null;
      if (_usr.bScouting) staff = clb.staff;

      //The table or entity I'm querying
      List<TrainingModel.TableItem> query = new List<TrainingModel.TableItem>();

      foreach (CornerkickManager.Player plCon in clb.ltPlayer) {
        if (plCon.contract == null) continue;

        float[]? fSkills = null;
        if (staff != null) fSkills = staff.getScoutedSkills(plCon.plGame);

        string sTrLast = "keins";
        if (plCon.ltTrainingHistory != null && plCon.ltTrainingHistory.Count > 0) {
          for (int i = plCon.ltTrainingHistory.Count - 1; i > 0; i--) {
            if (plCon.ltTrainingHistory[i].iType != 0 && plCon.ltTrainingHistory[i].iType != -1) {
              CornerkickManager.PlayerTool.Training tr = CornerkickManager.PlayerTool.getTraining(plCon.ltTrainingHistory[i].iType, ckMng.plt.ltTraining);
              sTrLast = tr.sName;
              break;
            }
          }
        }

        query.Add(new TrainingModel.TableItem {
          iId = plCon.plGame.iId,
          iNb = plCon.plGame.iNr,
          sName = plCon.plGame.sName,
          sPosition = CornerkickManager.PlayerTool.getStrPos(plCon),
          fSkill = CornerkickGame.Tool.getAveSkill(plCon.plGame, fSkills: fSkills),
          fCondi = plCon.plGame.fCondition,
          fFresh = plCon.plGame.fFresh,
          fMoral = plCon.plGame.fMoral,
          sNat = CornerkickManager.Main.sLandShort[plCon.iNat1],
          iAge = (int)plCon.plGame.getAge(ckMng.dtDatum),
          fTalent = plCon.getTalentAve() + 1f,
          fSkillIdeal = CornerkickGame.Tool.getAveSkill(plCon.plGame, bIdeal: true, fSkills: fSkills),
          sTrLast = sTrLast,
          iIndTr = plCon.iIndTraining,
          fSkillIndTr = staff != null ? staff.getScoutedSkill(plCon.plGame, plCon.iIndTraining) : 0f,
          iTalentIndTr = (byte)(plCon.getTalent(plCon.iIndTraining) + 1)
        });
      }

      return query;
    }
  }
}
