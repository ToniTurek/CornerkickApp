using CornerkickApp.Shared.Models;
using System.Runtime.InteropServices.JavaScript;
using static CornerkickApp.Shared.Models.CkAppShared;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  public class PlayerController
  {
    public static PlayerModel Model(string sUserId, int id)
    {
      PlayerModel model = new PlayerModel();

      CornerkickManager.User? usr = MemberController.ckUser(sUserId);
      if (usr == null) return model;
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      CornerkickManager.Player? plDetails;

      if (id < 0) {
        if (clb == null) return model;
        plDetails = clb.ltPlayer[0];
        id = plDetails.plGame.iId;
      } else {
        plDetails = ckMng.ltPlayer.Find(p => p.plGame.iId == id);
      }
      if (plDetails == null) return model;

      CornerkickManager.Main.Staff? staff = usr.bScouting ? ClubController.getClubStaff(clb) : null;
      float[]? fSkills = staff != null ? staff.getScoutedSkills(plDetails.plGame) : null;

      //model.ci = MemberController.getCi(_usr);

      model.bSound = true;
      if (usr.lti?.Count > UserOptionsModel.iUserOptionsIxSound) model.bSound = usr.lti[UserOptionsModel.iUserOptionsIxSound] > 0;

      model.bScouting = usr.bScouting;

      // Tutorial
      if (ttUser != null) {
        int iUserIx = ckMng.ltUser.IndexOf(usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) model.tutorial = ttUser[iUserIx];
      }

      //model.pldetails = plDetails;
      model.sNat1 = CornerkickManager.Main.sLandShort[plDetails.iNat1];
      model.sNat2 = CornerkickManager.Main.sLandShort[plDetails.iNat2];
      model.iAge = (int)plDetails.plGame.getAge(ckMng.dtDatum);
      model.sBirthday = plDetails.plGame.dtBirthday.ToString("d", MemberController.getCi(usr)) + " (" + model.iAge.ToString() + ")";
      model.sHeightWeight = plDetails.plGame.iHeight.ToString("0cm") + " / " + plDetails.plGame.iWeight.ToString("0kg");
      model.sPos = CornerkickManager.PlayerTool.getStrPos(plDetails.plGame);
      model.sFoot = "beidfüßig";
      if (plDetails.plGame.fFootL < 0.99 || plDetails.plGame.fFootR < 0.99) {
        model.sFoot = plDetails.plGame.fFootL.ToString("0%") + " / " + plDetails.plGame.fFootR.ToString("0%");
      }
      model.sInjury = "";
      if (plDetails.plGame.injury != null) {
        model.sInjury = plDetails.plGame.injury.sName + ": " + plDetails.plGame.injury.fLength.ToString("0") + "d (" + plDetails.plGame.getInjuryRel().ToString("0%") + ")";
      }
      model.sClub = "vereinslos";
      if (plDetails.contract?.club != null) {
        model.sClub = plDetails.contract.club.sName;
      }

      model.iPlayerIndTr = plDetails.iIndTraining;

      model.iSuspension = plDetails.plGame.iSuspension;
      if (plDetails.plGame.doping != null && plDetails.plGame.doping.fEffectMax > 0f) {
        model.sDopingName = plDetails.plGame.doping.sName;

        model.fDoping = new float[4];
        model.fDoping[0] = plDetails.plGame.doping.fEffect;
        model.fDoping[1] = plDetails.plGame.doping.fEffect / plDetails.plGame.doping.fEffectMax;
        model.fDoping[2] = plDetails.plGame.doping.fReductionRate;
        model.fDoping[3] = plDetails.plGame.doping.fDetectable * (plDetails.plGame.doping.fEffect / plDetails.plGame.doping.fEffectMax);
      }

      model.sCharacter = [
        plDetails.plGame.character.fLeader.ToString("0.0%"),
        plDetails.plGame.character.fShoot.ToString("0.0%"),
        plDetails.plGame.character.fPass.ToString("0.0%"),
        plDetails.plGame.character.fDribbling.ToString("0.0%"),
        plDetails.plGame.character.fFlexibel.ToString("0.0%"),
        plDetails.plGame.character.fRobustness.ToString("0.0%"),
        (2f - plDetails.plGame.character.fCholeric).ToString("0.0%"),
        plDetails.plGame.character.fMoney.ToString("0.0%")
      ];
      model.contract = plDetails.contract == null ? null : plDetails.contract.Clone();
      model.bRetire = plDetails.bRetire;
      model.sClubNextSeason = "";
      if (plDetails.contractNext?.club != null && plDetails.contractNext.iLength > 0) model.sClubNextSeason = plDetails.contractNext.club.sName;

      model.bOwnPlayer = CornerkickManager.PlayerTool.ownPlayer(clb, plDetails);
      //model.bAdmin = AdminModel.checkUserIsAdmin(_usr);
      model.bOnTransferlist = ckMng.plt.onTransferlist(plDetails);
      bool bAtNationalTeam = CornerkickManager.PlayerTool.atNationalTeam(plDetails, ckMng.ltClubs);

      CornerkickManager.Club? clbPlayer = null;
      if (bAtNationalTeam) clbPlayer = CornerkickManager.Tool.getNation(plDetails.iNat1, ckMng.ltClubs);
      else if (plDetails.contract != null) clbPlayer = plDetails.contract.club;

      if (clbPlayer != null) {
        model.bJouth = CornerkickManager.PlayerTool.ownPlayer(clbPlayer, plDetails, 2);
        model.bJouthBelow16 = plDetails.plGame.getAge(ckMng.dtDatum) < 16;
        model.bJouthWithContract = model.bJouth && plDetails.contract.iSalary != CornerkickManager.Finance.iPlayerJouthSalary;

        model.sColorJersey1 = Tool.convertToRgb(clbPlayer.cl1[0]);
        model.sColorJersey2 = Tool.convertToRgb(clbPlayer.cl1[1]);
        model.sColorJerseyNb = Tool.convertToRgb(clbPlayer.cl1[2]);
        if (Tool.checkColorsSimilar(clbPlayer.cl1[0], clbPlayer.cl1[2])) { // If number color is similar to main color, use second color
          if (Tool.checkColorsSimilar(clbPlayer.cl1[0], clbPlayer.cl1[1])) { // If second color is also similar to main color, use black or white
            if (clbPlayer.cl1[1].R + clbPlayer.cl1[1].G + clbPlayer.cl1[1].B / 3 > 128) model.sColorJerseyNb = "rgb(  0,   0,   0)"; // Black
            else model.sColorJerseyNb = "rgb(255, 255, 255)"; // White
          } else {
            model.sColorJerseyNb = model.sColorJersey2;
          }
        }
        //System.Drawing.Color clJerseyNo = getColorBW(clbPlayer);
        //model.sColorJerseyNo = "rgb(" + clJerseyNo.R.ToString() + "," + clJerseyNo.G.ToString() + "," + clJerseyNo.B.ToString() + ")";
        model.sColor2 = model.sColorJersey2;
        if (Tool.checkColorsSimilar(clbPlayer.cl1[0], clbPlayer.cl1[1])) model.sColor2 = model.sColorJerseyNb;

        model.bCpuPlayerNotOnTransferlist = !model.bOwnPlayer && !ckMng.plt.onTransferlist(plDetails) && !model.bJouth && plDetails.contractNext == null;
      }
      if (clb != null) model.bNation = clb.bNation;

      model.iContractYears = 1;

      model.sName = plDetails.plGame.sName;
      model.fTalentAve = plDetails.getTalentAve() + 1f;

      //model.sPortrait = getPlayerPortrait(plDetails);
      model.sPortrait = getPlayerPortraitHtmlImg(plDetails, sStyle: "height: 100%; width: 100%; object-fit: contain");
      //model.bytePortrait = getPlayerPortraitFile(plDetails);
      if (plDetails.contract == null) model.sEmblem = ClubController.getClubEmblemImg(null, "height: 100%; width: 100%; object-fit: contain");
      else model.sEmblem = ClubController.getClubEmblemImg(plDetails.contract.club, "height: 100%; width: 100%; object-fit: contain");

      List<int> ltNoExist = new List<int>();
      if (clbPlayer != null) {
        foreach (CornerkickManager.Player pl in clbPlayer.ltPlayer) {
          ltNoExist.Add(pl.plGame.iNr);
        }
      }

      model.ltNo = new List<int>();
      model.iNo = plDetails.plGame.iNr;

      if (plDetails.plGame.iNr == 0) {
        model.ltNo.Add(0);
      }

      for (int j = 1; j < 41; j++) {
        if (ltNoExist.IndexOf(j) >= 0 && j != plDetails.plGame.iNr) continue;
        model.ltNo.Add(j);
      }

      // Current position
      model.iPos = 0;
      if (clbPlayer != null) {
        for (int iPl = 0; iPl < Math.Min(11, clbPlayer.ltPlayer.Count); iPl++) {
          if (plDetails == clbPlayer.ltPlayer[iPl]) {
            plDetails.plGame.iIndex = (byte)iPl;
            model.iPos = CornerkickGame.Tool.getPosRole(plDetails.plGame, clbPlayer.ltTactic[clbPlayer.iTactic].formation, ckMng.game.ptPitch);
            break;
          }
        }
      }

      // Player value
      model.sValue = (plDetails.getValue(ckMng.dtDatum, ckMng.dtSeasonEnd) * 1000).ToString("N0", MemberController.getCi(usr)) + " €";

      // Position table
      int iPosCur = 0;
      if (clb != null) {
        for (int iPl = 0; iPl < Math.Min(11, clb.ltPlayer.Count); iPl++) {
          if (plDetails.plGame.iId == clb.ltPlayer[iPl].plGame.iId) {
            iPosCur = CornerkickGame.Tool.getPosRole(plDetails.plGame, clb.ltTactic[clb.iTactic].formation, ckMng.game.ptPitch);
            break;
          }
        }
      }

      List<DataPointDD> ltPlayerPosSkillData = new List<DataPointDD>();
      for (int i = 0; i < plDetails.plGame.fExperiencePos.Length; i++) {
        byte j = (byte)(i + 1);

        string sPosSkill = CornerkickManager.Main.sPosition[i + 1];
        if (iPosCur > 0 && iPosCur == i + 1) {
          sPosSkill += " *";
        }

        ltPlayerPosSkillData.Add(new DataPointDD(x: plDetails.plGame.fExperiencePos[i], y: CornerkickGame.Tool.getAveSkill(plDetails.plGame, j, fSkills: fSkills), z: sPosSkill));
      }
      model.ltPlayerPosSkillData = ltPlayerPosSkillData.ToArray();

      // Skill table
      if (clb != null) model.sSkillTable = getSkillTable(plDetails, model.iPos, clb, model.bAdmin ? false : usr.bScouting);

      // Captain
      if (clbPlayer != null) {
        model.bCaptain = id == clbPlayer.iCaptainId[0];
        model.bCaptain2 = id == clbPlayer.iCaptainId[1];
      }

      // Doping
      model.ddlDoping = new List<SelectListItem>();
      byte iDp = 0;
      foreach (CornerkickGame.Player.Doping dp in ckMng.ltDoping) {
        model.ddlDoping.Add(
          new SelectListItem {
            Text = dp.sName,
            Value = iDp++.ToString()
          }
        );
      }

      // Injury
      /*
      if (plDetails.plGame.injury != null) {
        Random rnd = new Random();
        if (plDetails.plGame.injury.iType >= CornerkickManager.Main.ltInjury.Length) plDetails.plGame.injury.iType = (byte)(CornerkickManager.Main.ltInjury.Length - 1);
        if (plDetails.plGame.injury.iType2 < 0 || plDetails.plGame.injury.iType2 >= CornerkickManager.Main.ltInjury[plDetails.plGame.injury.iType].Count) plDetails.plGame.injury.iType2 = (sbyte)rnd.Next(CornerkickManager.Main.ltInjury[plDetails.plGame.injury.iType].Count);
      }
      */

      // Contract
      int iGamesPerSeason = 0;
      if (clb != null) {
        CornerkickManager.Cup league = ckMng.tl.getCup(iCupIdLeague, clb.iLand, clb.iDivision);
        if (league != null) iGamesPerSeason = league.getMatchdays(clb);
      }

      //model.sContractHappyFactor = CornerkickManager.PlayerTool.getHappyWithContractFactor(plDetails, ckMng.dtDatum, ckMng.dtSeasonEnd, iGamesPerSeason: iGamesPerSeason).ToString("0.0%");
      model.fContractHappyFactor = CornerkickManager.PlayerTool.getHappyWithContractFactor(plDetails, ckMng.dtDatum, ckMng.dtSeasonEnd, iGamesPerSeason: iGamesPerSeason, user: usr);

      // Next / Prev. Player
      /*
      model.plPrev = null;
      model.plNext = null;
      */

      if (clb != null) {
        int iIndex = clb.ltPlayer.IndexOf(plDetails);

        if (iIndex >= 0) {
          if (iIndex > 0) {
            model.iPlPrevId = clb.ltPlayer[iIndex - 1].plGame.iId;
            model.sPlPrevName = clb.ltPlayer[iIndex - 1].plGame.sName;
          }
          if (iIndex < clb.ltPlayer.Count - 1) {
            model.iPlNextId = clb.ltPlayer[iIndex + 1].plGame.iId;
            model.sPlNextName = clb.ltPlayer[iIndex + 1].plGame.sName;
          }
        }

        // Scouts
        model.ltScouts = new List<PlayerModel.Scout>();

        // Add freelancer scouts if not already added
        addFreelancerScouts(usr);

        // Club scouts
        foreach (CornerkickManager.Main.Staff.Scout sc in clb.staff.ltScouts) {
          if (sc.iId >= 0) {
            int iMinutesActivate = 0;
            if (sc.bFreelancer) {
              iMinutesActivate = -1;
            } else if (!sc.bActive) {
              iMinutesActivate = (int)(ckMng.dtDatum.Date.Add(new TimeSpan(12, 00, 00)) - ckMng.dtDatum).TotalMinutes;
              if (iMinutesActivate < 0) iMinutesActivate += 24 * 60;
            }

            model.ltScouts.Add(new PlayerModel.Scout(sc.iId, sc.sName, sc.iSkill, sc.nDataPerScouting, sc.getSalary().ToString("C0", MemberController.getCi(clb)), iMinutesActivate));
          }
        }
      }

      // Player is editable
#if _WebApp
      model.bEditable = (ckMng.dtDatum - usr.dtClubStart).TotalHours < 24;
      model.bLiveGame = usr != null && usr.game != null && !usr.game.data.bFinished;
#endif
      model.bSeasonStart = ckMng.dtDatum.Date.Equals(ckMng.dtSeasonStart.Date);

      // Player Development data
      List<DataPointTD>[] ltDataPoints = new List<DataPointTD>[2];
      ltDataPoints[0] = new List<DataPointTD>(); // Skill
      ltDataPoints[1] = new List<DataPointTD>(); // Value

      foreach (CornerkickManager.Player.History hty in plDetails.ltHistory) {
        if (hty.fStrength > 0f) ltDataPoints[0].Add(new DataPointTD(hty.dt, hty.fStrength));
        if (hty.iValue > 0) ltDataPoints[1].Add(new DataPointTD(hty.dt, hty.iValue * 1000));
      }

      float fSkillAveNow = CornerkickGame.Tool.getAveSkill(plDetails.plGame, bIdeal: true, fSkills: fSkills);
      if (fSkillAveNow > 0f) ltDataPoints[0].Add(new DataPointTD(ckMng.dtDatum, fSkillAveNow));
      ltDataPoints[1].Add(new DataPointTD(ckMng.dtDatum, plDetails.getValue(ckMng.dtDatum, ckMng.dtSeasonEnd) * 1000));

      model.ltDevData = new DataPointTD[][] { ltDataPoints[0].ToArray(), ltDataPoints[1].ToArray() };

      return model;
    }

    internal static void addFreelancerScouts(CornerkickManager.User usr)
    {
      if (usr?.club?.staff == null) return;

      if (usr.club.staff.ltScouts == null) usr.club.staff.ltScouts = new List<CornerkickManager.Main.Staff.Scout>();
      foreach (CornerkickManager.Main.Staff.Scout scFl in CornerkickManager.Main.staff.ltScouts.FindAll(s => s.bFreelancer)) {
        if (!usr.club.staff.ltScouts.Any(s => s.iId == scFl.iId)) usr.club.staff.ltScouts.Add(scFl.Clone(bReduced: true));
      }
    }

    public static bool setName(int iPlayerId, string sName)
    {
      CornerkickManager.Player? pl = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl?.plGame == null) return false;

      pl.plGame.sName = sName;

      return true;
    }

    public static bool setPlayerNo(int iPlayerId, int iNo)
    {
      CornerkickManager.Player? pl = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl?.plGame == null) return false;

      pl.plGame.iNr = (byte)iNo;

      return true;
    }

    public static bool setIndTraining(int iPlayerId, int iIndTr)
    {
      return setIndTraining(ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId), iIndTr);
    }

    public static bool setIndTraining(CornerkickManager.Player? pl, int iIndTr)
    {
      if (pl == null) return false;

      pl.iIndTraining = (byte)iIndTr;

      return true;
    }

    public static float[] getAveSkill(CornerkickManager.User? usr, int iPlayerId)
    {
      if (iPlayerId < 0) return [0f, 0f];

      CornerkickManager.Player? plMng = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (plMng == null) return [0f, 0f];

      CornerkickGame.Player pl = plMng.plGame;

      float[] fSkills = [0f, 0f];
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (usr != null && usr.bScouting && clb != null) {
        if (usr.bScouting) {
          CornerkickManager.Main.Staff? staff = ClubController.getClubStaff(clb);

          if (staff != null) fSkills = staff.getScoutedSkills(pl);
        }
      }

      return [CornerkickGame.Tool.getAveSkill(pl, fSkills: fSkills), CornerkickGame.Tool.getAveSkill(pl, bIdeal: true, fSkills: fSkills)];
    }

    public static List<SelectListItem> getIndSkillsSelectList(int iPlayerId)
    {
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

      List<SelectListItem> ltIndSkills = new List<SelectListItem>();
      foreach (byte iS in ltSkills) {
        float fSkillScouted = 0f;
        byte iTalent = 0;
        CornerkickManager.Player? pl = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
        if (pl != null) {
          if (pl.contract?.club?.staff != null) fSkillScouted = pl.contract.club.staff.getScoutedSkill(pl.plGame, iS);
          iTalent = pl.getTalent(iS);
        }

        ltIndSkills.Add(new SelectListItem() { Text = CornerkickManager.Names.sSkills[iS] + " - " + (fSkillScouted > 0f ? fSkillScouted.ToString("0.0") : "?") + " (" + iTalent.ToString() + ")", Value = iS.ToString() });
      }

      return ltIndSkills;
    }

    public static string[][] getSkillTable(int iPlId, byte iPos, CornerkickManager.User? usr)
    {
      CornerkickManager.Player? pl = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlId);
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      return getSkillTable(pl, iPos, clb, usr == null ? false : usr.bScouting);
    }
    public static string[][] getSkillTable(CornerkickManager.Player? pl, byte iPos, CornerkickManager.Club? clb, bool bScouting)
    {
      //bool bOwnPlayer = CornerkickManager.PlayerTool.ownPlayer(clb, plDetails);

      // Define skill order
      byte[] iSkills = [
        101, // Endurance
        CornerkickGame.Game.iSkillIxSpeed,
        100, // Speed with ball
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
      ];

      // Define table
      string[][] sTable = new string[iSkills.Length][];
      for (int i = 0; i < sTable.Length; i++) sTable[i] = new string[12];

      if (pl == null) return sTable;
      if (clb == null) return sTable;

      CornerkickManager.Main.Staff? staff = ClubController.getClubStaff(clb);

      // Speed
      int iIx = 0;
      foreach (byte iS in iSkills) {
        // Skill index
        sTable[iIx][0] = iS.ToString();

        // Skill category
        if (iIx == 0) sTable[iIx][1] = CornerkickManager.Names.sSkillCategorys[CornerkickGame.Game.iSkillCategoryIxEndurance];
        else if (iIx == 1) sTable[iIx][1] = CornerkickManager.Names.sSkillCategorys[CornerkickGame.Game.iSkillCategoryIxAthletic];
        else if (iIx == 5) sTable[iIx][1] = CornerkickManager.Names.sSkillCategorys[CornerkickGame.Game.iSkillCategoryIxCoordination];
        else if (iIx == 7) sTable[iIx][1] = CornerkickManager.Names.sSkillCategorys[CornerkickGame.Game.iSkillCategoryIxDuel];
        else if (iIx == 8) sTable[iIx][1] = CornerkickManager.Names.sSkillCategorys[CornerkickGame.Game.iSkillCategoryIxPower];
        else if (iIx == 11) sTable[iIx][1] = CornerkickManager.Names.sSkillCategorys[CornerkickGame.Game.iSkillCategoryIxAccuracy];
        else if (iIx == 15) sTable[iIx][1] = CornerkickManager.Names.sSkillCategorys[CornerkickGame.Game.iSkillCategoryIxHeader];
        else if (iIx == 16) sTable[iIx][1] = CornerkickManager.Names.sSkillCategorys[CornerkickGame.Game.iSkillCategoryIxCognition];

        if (iS == 100) {
          sTable[iIx][3] = "Schnelligk. m. Ball";
          if (bScouting) {
            float fSkillScoutedSpeed = staff != null ? staff.getScoutedSkill(pl.plGame, CornerkickGame.Game.iSkillIxSpeed) : 0f;
            float fSkillScoutedTechnic = staff != null ? staff.getScoutedSkill(pl.plGame, CornerkickGame.Game.iSkillIxTechnic) : 0f;
            if (fSkillScoutedSpeed > 0f && fSkillScoutedTechnic > 0f) sTable[iIx][4] = (fSkillScoutedSpeed - fSkillScoutedSpeed / fSkillScoutedTechnic).ToString("0.0");
            else sTable[iIx][4] = "?";
          } else {
            // To-do: Add legacy code
          }
        } else if (iS == 101) {
          sTable[iIx][2] = (pl.iTalent[CornerkickGame.Game.iSkillCategoryIxEndurance] + 1).ToString();
          sTable[iIx][3] = "Kondition";
          sTable[iIx][4] = pl.plGame.fCondition.ToString("0.0%");
          sTable[iIx][8] = CornerkickGame.Game.iSkillCategoryIxEndurance.ToString();
        } else {
          byte iSkill = 0;

          sTable[iIx][2] = (pl.getTalent(iS) + 1).ToString();
          sTable[iIx][3] = CornerkickManager.Names.sSkills[iS];
          float fSkillScouted = 0f;
          if (bScouting) {
            fSkillScouted = staff != null ? staff.getScoutedSkill(pl.plGame, iS) : 0f;
            if (fSkillScouted > 0f) {
              iSkill = (byte)fSkillScouted;
              sTable[iIx][4] = fSkillScouted.ToString("0.0");
              sTable[iIx][9] = (fSkillScouted / 10f).ToString("0%"); // Progress-bar width
              sTable[iIx][11] = staff != null ? staff.getScoutingData(pl.plGame, iS).Count.ToString() : ""; // Indicator for scouting button
            } else {
              sTable[iIx][4] = "?";
              sTable[iIx][9] = "0%"; // Progress-bar width
            }
          } else {
            iSkill = pl.plGame.iSkill[iS];
            fSkillScouted = iSkill;
            sTable[iIx][4] = iSkill.ToString();
            sTable[iIx][5] = CornerkickGame.Tool.getSkillEff(pl.plGame, iS, iPos).ToString("0.0");

            // Progress-bar width
            sTable[iIx][9] = (iSkill / 10.0).ToString("0%");
          }
          sTable[iIx][6] = (pl.plGame.fSkillTraining[iS] + 1f).ToString("0.0%");
          sTable[iIx][7] = pl.plGame.fIndTraining[iS].ToString("0.0%");

          // Progress-bar color
          sTable[iIx][10] = getColorFromValue(-1, fSkillScouted);
        }

        // Skill category index
        if (iS < CornerkickGame.Game.iSkillCategoryConvTable.Length) sTable[iIx][8] = CornerkickGame.Game.iSkillCategoryConvTable[iS].ToString();

        iIx++;
      }

      return sTable;
    }

    public static string getPlayerPortrait(int iPlayerId, bool bSmall = false)
    {
      return getPlayerPortrait(ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId), bSmall: bSmall);
    }
    public static string getPlayerPortrait(CornerkickManager.Player? plPortrait, bool bSmall = false)
    {
      string sPortraitDefaultDir = Path.Combine(sContentDir, "Images", "portraits");
      if (plPortrait == null) return Path.Combine(sPortraitDefaultDir, "0.png").Replace("\\\\", "/");

      // First: try user uploaded portrait
#if _WebApp
      string sPortraitDir = Path.Combine(sContentDir, "Uploads", "portraits");
#else
      string sPortraitDir = string.IsNullOrEmpty(ckMng.sDatabaseName) ? "" : Path.Combine(App.getMediaDir(ckMng.sDatabaseName), "images", "portraits");
#endif
      if (Directory.Exists(sPortraitDir)) {
        string sPortraitFile = Path.Combine(sPortraitDir, plPortrait.plGame.iId.ToString()) + ".png";
        if (bSmall) sPortraitFile = Tool.resizeImage(sPortraitFile + ".png", 100, "s");
        if (File.Exists(sPortraitFile)) return sPortraitFile.Replace("\\\\", "/");
      }

      // Second: return default portrait
      return Path.Combine(sPortraitDefaultDir, getPlayerPortraitId(plPortrait).ToString()).Replace("\\\\", "/") + ".png";
    }

    /// <summary>
    /// Generates an HTML <img> tag for displaying a player's portrait image, applying optional styling and size
    /// preferences.
    /// </summary>
    /// <param name="iPlayerId">The unique identifier of the player whose portrait image is to be retrieved.</param>
    /// <param name="sStyle">An optional CSS style string that defines the appearance of the image. The default value is "height: 100%;
    /// width: 100%; object-fit: contain".</param>
    /// <param name="bSmall">A value indicating whether to retrieve a smaller version of the portrait image. The default value is <see
    /// langword="false"/>.</param>
    /// <returns>A string containing the URL of the player's portrait image. Returns an empty string if the player is not found.</returns>
    public static string getPlayerPortraitHtmlImg(int iPlayerId, string sStyle = "height: 100%; width: 100%; object-fit: contain", bool bSmall = false)
    {
      return getPlayerPortraitHtmlImg(ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId), sStyle: sStyle, bSmall: bSmall);
    }
    /// <summary>
    /// Generates an HTML <img> tag for displaying a player's portrait image, applying optional styling and size
    /// preferences.
    /// </summary>
    /// <remarks>If the player parameter is null, the method returns a default portrait image. The method
    /// supports both user-uploaded and default images, and includes a fallback image if the specified portrait cannot
    /// be loaded.</remarks>
    /// <param name="plPortrait">The player for whom the portrait image is generated. If null, a default portrait image is used.</param>
    /// <param name="sStyle">A CSS style string to apply to the image element. The default is "height: 100%; width: 100%; object-fit:
    /// contain".</param>
    /// <param name="bSmall">true to request a smaller version of the portrait if available; otherwise, false.</param>
    /// <returns>A string containing an HTML <img> tag for the player's portrait, including appropriate alt text, title, and
    /// styling.</returns>
    public static string getPlayerPortraitHtmlImg(CornerkickManager.Player? plPortrait, string sStyle = "height: 100%; width: 100%; object-fit: contain", bool bSmall = false)
    {
      string sPortrait = "<img src=\"" + sContentDir + "/Images/portraits/";

      if (!string.IsNullOrEmpty(sStyle)) sStyle = " style=\"" + sStyle + "\"";

      if (plPortrait == null) return sPortrait + "0.png\" alt=\"Portrait\" " + sStyle + " title=\"ohne\"/>";

      bool bUserPortrait;
      bool bSmallOk;
      byte[]? bPortrait = getPlayerPortraitFile(plPortrait, out bUserPortrait, out bSmallOk, bSmall);

      //      if (File.Exists(sPortraitFile)) {
      if (bUserPortrait && bPortrait != null) {
#if _WebApp
          sPortrait = "<img src=\"" + sContentDir + "/Uploads/portraits/" + plPortrait.plGame.iId.ToString();
          if (bSmall && bSmallOk) sPortrait += "s";
          sPortrait += ".png\"";
#else
        //sPortrait = "<img src=\"/Content/Uploads/media_" + ckMng.sDatabaseName + "/images/portraits/" + plPortrait.plGame.iId.ToString();
        sPortrait = "<img src=\"data:image/*;base64," + @Convert.ToBase64String(bPortrait) + "\"";
#endif
      } else {
        sPortrait += getPlayerPortraitId(plPortrait).ToString();
        //if (bSmall && bSmallOk) sPortrait += "s";
        sPortrait += ".png\"";
      }
      /*
      } else {
        sPortrait += "0.png\"";
      }
      */

      return sPortrait + " alt=\"Portrait\"" + sStyle + " title=\"" + plPortrait.plGame.sName + "\" onerror=\"this.src='" + sContentDir + "/Images/portraits/0.png'\" >";
    }

    class PlayerPortrait
    {
      public int iPlayerId;
      public byte[] bPortrait = [];
    }
    private static List<PlayerPortrait> ltPlayerPortraits = new List<PlayerPortrait>();
    private static byte[]? getPlayerPortraitFile(int iPlayerId, bool bSmall = false)
    {
      bool bUserPortrait;
      bool bSmallOk;

      return getPlayerPortraitFile(ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId), out bUserPortrait, out bSmallOk, bSmall);
    }
    private static byte[]? getPlayerPortraitFile(CornerkickManager.Player? plPortrait, bool bSmall = false)
    {
      bool bUserPortrait;
      bool bSmallOk;

      return getPlayerPortraitFile(plPortrait, out bUserPortrait, out bSmallOk, bSmall);
    }
    private static byte[]? getPlayerPortraitFile(CornerkickManager.Player? plPortrait, out bool bUserPortrait, out bool bSmallOk, bool bSmall = false)
    {
      bUserPortrait = true;
      bSmallOk = false;

      if (plPortrait == null) return null;

      // 1st: check cached portraits
      if (bSmall) {
        PlayerPortrait? pp = ltPlayerPortraits.Find(p => p.iPlayerId == plPortrait.plGame.iId);
        if (pp != null && pp.bPortrait.Length > 0) return pp.bPortrait;
      }

      // 2nd: try user uploaded portraits
#if _WebApp
      string sPortraitDir = Path.Combine(sContentDir, "Uploads", "portraits");
#else
      //string sPortraitDir = System.IO.Path.Combine(sContentDir, "Uploads", "media_" + ckMng.sDatabaseName, "images", "portraits");
      string sPortraitDir = string.IsNullOrEmpty(ckMng.sDatabaseName) ? "" : Path.Combine(App.getMediaDir(ckMng.sDatabaseName), "images", "portraits");
#endif

      if (!string.IsNullOrEmpty(sPortraitDir)) {
#if _WebApp
        for (byte i = 0; i < 2; i++) { // If bSmall --> try both
#endif
        string sPortraitFile = Path.Combine(sPortraitDir, plPortrait.plGame.iId.ToString());

#if _WebApp
          // Create small image
          if (bSmall) Tool.resizeImage(sPortraitFile + ".png", 100, "s");

          if (bSmall && i == 0) sPortraitFile += "s";
#endif
        sPortraitFile += ".png";

        try {
#if _WebApp
          if (bSmall && i == 0) bSmallOk = true;
#endif
          byte[] bPortrait = File.ReadAllBytes(sPortraitFile);
          if (bSmall) {
            bPortrait = Tool.resizeImage(bPortrait, 64);

            /* To-Do: Save small portrait to media for re-use (Similar to getClubEmblemImgSrc())
#if _WebApp
            string sTinyEmblemDir = sContentDir + "/Uploads/emblems/.tiny";
#else
            string sTinyEmblemDir = Path.Combine(App.getMediaDir(ckMng.sDatabaseName), "images", "emblems", ".tiny");
#endif
            if (!Directory.Exists(sTinyEmblemDir)) {
              Directory.CreateDirectory(sTinyEmblemDir);
            }
             */
            // Cache small portrait
            ltPlayerPortraits.Add(new PlayerPortrait() { iPlayerId = plPortrait.plGame.iId, bPortrait = bPortrait });
          }
          return bPortrait;
        } catch (Exception ex) {
          /*
          ckMng.tl.writeLog("Error loading player " + plPortrait.plGame.sName + " (" + plPortrait.plGame.iId.ToString() + ") portrait." +
            Environment.NewLine + ex.Message +
            Environment.NewLine + ex.StackTrace,
            sLogFile: CornerkickManager.Main.sErrorFile);
          */
        }
#if _WebApp
        }
#endif
      }

      // 3rd: try default portraits
      for (byte i = 0; i < 2; i++) { // If bSmall --> try both
        string sPortraitFile = Path.Combine(sHomeDir, "Images", "portraits", getPlayerPortraitId(plPortrait).ToString());

        // Create small image
        if (bSmall) Tool.resizeImage(sPortraitFile + ".png", 100, "s");

        if (bSmall && i == 0) sPortraitFile += "s";
        sPortraitFile += ".png";

        bUserPortrait = false;

        try {
          if (bSmall && i == 0) bSmallOk = true;
          return File.ReadAllBytes(sPortraitFile);
        } catch (Exception ex) {
          /*
          ckMng.tl.writeLog("Error loading player " + plPortrait.plGame.sName + " (" + plPortrait.plGame.iId.ToString() + ") portrait." +
            Environment.NewLine + ex.Message +
            Environment.NewLine + ex.StackTrace,
            sLogFile: CornerkickManager.Main.sErrorFile);
          */
        }
      }

      return null;
    }

    public static void setRandomPortrait(CornerkickManager.Player pl)
    {
      if (pl == null) return;

      try {
        if (pl.clSkin.B == 0) {
          string sDirPortrait = Path.Combine(sHomeDir, "Images", "portraits");
          int nPortraitFiles = byte.MaxValue;

          if (Directory.Exists(sDirPortrait)) {
            DirectoryInfo diPortrait = new DirectoryInfo(sDirPortrait);

            nPortraitFiles = diPortrait.GetFiles("*.png").Length;
          }

          pl.clSkin = getSkinFromId((ushort)random.Next(nPortraitFiles));
        }
      } catch {
      }
    }

    public static byte getPlayerPortraitId(CornerkickManager.Player plPortrait)
    {
      byte[] byteArrPortrait = [plPortrait.clSkin.R, plPortrait.clSkin.G];
      return (byte)BitConverter.ToUInt16(byteArrPortrait, 0);

      /*
      string sDirPortrait = System.IO.Path.Combine(App.getHomeDir(), "Content", "Images", "portraits");

      if (System.IO.Directory.Exists(sDirPortrait)) {
        System.IO.DirectoryInfo diPortrait = new System.IO.DirectoryInfo(sDirPortrait);

      }
      */
    }

    public static System.Drawing.Color getSkinFromId(ushort iPortraitId)
    {
      byte[] b = BitConverter.GetBytes(iPortraitId);
      return System.Drawing.Color.FromArgb(b[0], b[1], 1);
    }

    public static DataPointSD[]? getCFM(int iPlId)
    {
      CornerkickManager.Player? plMng = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlId);
      if (plMng == null) return null;

      CornerkickGame.Player pl = plMng.plGame;

      DataPointSD[] dataPoints = new DataPointSD[4];

      float fCondiMax = 1f;
      if (pl.doping != null) fCondiMax += pl.doping.fEffect;
      dataPoints[0] = new DataPointSD("Kondition", fCondiMax, fCondiMax.ToString("0.0%"));
      dataPoints[1] = new DataPointSD("Kondition", pl.fCondition, pl.fCondition.ToString("0.0%"));
      dataPoints[2] = new DataPointSD("Frische", pl.fFresh, pl.fFresh.ToString("0.0%"));
      dataPoints[3] = new DataPointSD("Moral", pl.fMoral, pl.fMoral.ToString("0.0%"));

      return dataPoints;
    }

    public static float[]? scoutPlayer(CornerkickManager.Club clb, int iPlayerId, int iScoutId)
    {
      if (clb == null) return null;

      CornerkickManager.Player? plMng = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (plMng == null) return null;

      CornerkickManager.Main.Staff.Scout? sc = clb.staff.ltScouts.Find(s => s.iId == iScoutId);
      if (sc == null) return null;

      List<CornerkickManager.Main.Staff.Scout.PlayerData.Details> ltPdd = sc.scoutPlayer(plMng.plGame, ckMng.dtDatum);

      if (sc.ltPlayerData.Count < 1) return null;

      List<float> ltSkills = new List<float>();
      foreach (CornerkickManager.Main.Staff.Scout.PlayerData.Details pdd in ltPdd) ltSkills.Add(getScoutedSkillAve(clb, iPlayerId, pdd.iSkillIx));

      int iMinutesActivate = -1;
      if (sc.bFreelancer) {
        CornerkickManager.Finance.doTransaction(clb, ckMng.dtDatum, -sc.getSalary(), CornerkickManager.Finance.iTransferralTypePayScouting);
      } else {
        iMinutesActivate = (int)(ckMng.dtDatum.Date.Add(new TimeSpan(12, 00, 00)) - ckMng.dtDatum).TotalMinutes;
        if (iMinutesActivate < 0) iMinutesActivate += 24 * 60;
      }

      return ltSkills.ToArray();
      //return new { iMinutesActivate = iMinutesActivate, skill_data = ltSkills.ToArray() };
    }

    public static PlayerModel.ScoutingDataPlus[] getScoutingData(CornerkickManager.Club? clb, int iPlayerId, int iSkillIx)
    {
      return getScoutingData(ClubController.getClubStaff(clb), ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId), iSkillIx);
    }
    public static PlayerModel.ScoutingDataPlus[] getScoutingData(CornerkickManager.Main.Staff? staff, CornerkickManager.Player? plMng, int iSkillIx)
    {
      PlayerModel.ScoutingDataPlus[] scp = Array.Empty<PlayerModel.ScoutingDataPlus>();
      if (staff == null) return scp;
      if (plMng == null) return scp;

      List<PlayerModel.ScoutingDataPlus> ltSdp = new List<PlayerModel.ScoutingDataPlus>();
      List<CornerkickManager.Main.Staff.ScoutingData> ltSd = staff.getScoutingData(plMng.plGame, iSkillIx);
      foreach (CornerkickManager.Main.Staff.ScoutingData sd in ltSd) {
        PlayerModel.ScoutingDataPlus sdp = new PlayerModel.ScoutingDataPlus(sd);
        if (sd.iScoutId == -9) sdp.sScoutName = "Ich (Lv: " + sd.iScoutSkill.ToString() + ")";
        else sdp.sScoutImg = ClubController.getScoutImg(sd.iScoutId, sd.sScoutName + " (Level: " + sd.iScoutSkill.ToString() + ")");
        ltSdp.Add(sdp);
      }

      return ltSdp.OrderBy(s => s.dt).ToArray();
    }

    public static float getScoutedSkillAve(CornerkickManager.Club? clb, int iPlayerId, int iSkillIx)
    {
      return getScoutedSkillAve(ClubController.getClubStaff(clb), ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId), iSkillIx);
    }
    public static float getScoutedSkillAve(CornerkickManager.Main.Staff? staff, CornerkickManager.Player? plMng, int iSkillIx)
    {
      if (plMng == null) return -1f;
      if (staff == null) return -1f;

      return staff.getScoutedSkill(plMng.plGame, iSkillIx);
      //return new { skill_ix = iSkillIx, skill_ave = fSkillAve, n_scout = staff.getScoutingData(plMng.plGame, iSkillIx).Count };
    }

    public static Task<PlayerModel.ScoutingDataPlus[]> removeScoutingData(CornerkickManager.Club? clb, int iScoutId, int iPlayerId, int iSkillIx, int iSkill, DateTime dt)
    {
      return removeScoutingData(ClubController.getClubStaff(clb), iScoutId, ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId), iSkillIx, iSkill, dt);
    }
    public static Task<PlayerModel.ScoutingDataPlus[]> removeScoutingData(CornerkickManager.Main.Staff? staff, int iScoutId, CornerkickManager.Player? plMng, int iSkillIx, int iSkill, DateTime dt)
    {
      PlayerModel.ScoutingDataPlus[] scp = Array.Empty<PlayerModel.ScoutingDataPlus>();
      if (staff == null) return Task.FromResult(scp);
      if (plMng == null) return Task.FromResult(scp);

      CornerkickManager.Main.Staff.Scout? sc = staff.getScouts(plMng.plGame).Find(s => s.iId == iScoutId);

      if (sc != null) {
        foreach (CornerkickManager.Main.Staff.Scout.PlayerData.Details pdd in sc.getScoutedPlayerData(plMng.plGame, iSkillIx)) {
          if (iSkill == pdd.iSkill && dt.Equals(pdd.dt)) {
            //sc.ltPlayerData.RemoveAt(i);
            pdd.iSkill = 0;
            return Task.FromResult(getScoutingData(staff, plMng, iSkillIx));
          }
        }
      }

      return Task.FromResult(scp);
    }

    public static string? getClubCaptain(CornerkickManager.Club clb, int iC)
    {
      if (clb == null) return null;

      CornerkickManager.Player pl = ckMng.ltPlayer.Find(p => p.plGame.iId == clb.iCaptainId[iC]);

      if (pl != null) return pl.plGame.sName;

      return null;
    }

    public static string? makeCaptain(CornerkickManager.Club clb, int iPlayerId, int iC)
    {
      if (iPlayerId < 0) return null;
      if (clb == null) return null;

      CornerkickManager.Player pl = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl == null) return null;

      if (clb.iCaptainId == null) clb.iCaptainId = new int[3];

      if (iC >= clb.iCaptainId.Length) return null;

      string sCaptain = iC == 0 ? "Kapitän" : "Vize-Kapitän";

      ckMng.plt.makePlayerCaptain(pl, (byte)iC, clb);

      return "Sie haben " + pl.plGame.sName + " zum " + sCaptain + " ernannt.";
    }

    // Type: -1 - skill, 0 - Condi, 1 - Fresh, 2 - Moral, 99 - delta
    public static string getColorFromValue(int iType, float f)
    {
      if (iType == 0) {
        if (f > 0.90) return "green";
        if (f > 0.70) return "YellowGreen";
        if (f > 0.50) return "orange";
        return "red";
      } else if (iType == 1) {
        if (f > 0.95) return "green";
        if (f > 0.90) return "YellowGreen";
        if (f > 0.80) return "orange";
        return "red";
      } else if (iType == 2) {
        if (f > 1.10) return "green";
        if (f > 0.95) return "YellowGreen";
        if (f > 0.90) return "orange";
        return "red";
      } else if (iType == 99) {
        if (f > 0.05) return "green";
        if (f > 0.00) return "YellowGreen";
        if (f > -0.00001) return "black";
        if (f > -0.05) return "orange";
        return "red";
      } else if (iType == -1) {
        if (f > 12.5) return "#ff00ff"; // magenta
        if (f > 10.5) return "#ffc0cb"; // pink
        if (f > 8.5) return "#2cba00"; // dark-green
        if (f > 7.5) return "#00ff00"; // green
        if (f > 6.5) return "#a3ff00"; // yellow-green
        if (f > 5.5) return "#fff400"; // yellow
        if (f > 4.5) return "#ffa700"; // orange
        if (f > 3.5) return "#ff0000"; // red
        return "#c80000"; // dark-red
      }

      return "black";
    }

    public static List<DataPointTD>[]? GetTrainingHistoryData(int iPlayerId)
    {
      CornerkickManager.Player? pl = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl == null) return null;

      CornerkickManager.Player.TrainingHistory trHistCurrent = new CornerkickManager.Player.TrainingHistory();
      trHistCurrent.dt = ckMng.dtDatum;
      trHistCurrent.fKFM = [pl.plGame.fCondition, pl.plGame.fFresh, pl.plGame.fMoral, 0f, 0f];

      List<DataPointTD>[] dataPoints = new List<DataPointTD>[3];

      for (byte j = 0; j < dataPoints.Length; j++) {
        dataPoints[j] = new List<DataPointTD>();

        for (int i = 0; i < pl.ltTrainingHistory.Count; i++) {
          CornerkickManager.Player.TrainingHistory trHist = pl.ltTrainingHistory[i];

          CornerkickManager.PlayerTool.Training tr = CornerkickManager.PlayerTool.getTraining(trHist.iType, ckMng.plt.ltTraining);
          //string sTrainingName = "";
          //if (tr.iId >= 0) sTrainingName = tr.sName;
          //dataPoints[j].Add(new Models.DataPointGeneral(iDate, trHist.fKFM[j], z: sTrainingName));
          dataPoints[j].Add(new DataPointTD(trHist.dt, trHist.fKFM[j], z: tr.sName));
        }

        dataPoints[j].Add(new DataPointTD(trHistCurrent.dt, trHistCurrent.fKFM[j], z: "aktuell"));

        //dataPoints[j] = dataPoints[j].OrderByDescending(d => d.x).ToList();
      }

      return dataPoints;
    }

    // iMode: 0 - last game only, 1 - season, 2 - total
    public static List<PlayerModel.Stat> GetStatistic(int iPlayerId, int iMode = 1)
    {
      const byte nStatLength = 4;

      int[] iGoalsTotal = new int[nStatLength];

      List<PlayerModel.Stat> plStat2 = new List<PlayerModel.Stat>();

      CornerkickManager.Player? player = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (player == null) return plStat2;

      CornerkickGame.Player.Statistic[] plStat = new CornerkickGame.Player.Statistic[nStatLength];

      if (iMode > 0) {
        // Create EP statistic
        CornerkickGame.Player.Statistic plStat3 = player.plGame.getStatistic(iCupIdInt, iMode == 1);
        CornerkickGame.Player.Statistic? plStatEP = null;
        if (plStat3 != null) {
          if (plStatEP == null) plStatEP = new CornerkickGame.Player.Statistic();
          for (int iS = 0; iS < plStat3.iStat.Length; iS++) plStatEP.iStat[iS] += plStat3.iStat[iS];
        }

        plStat = new CornerkickGame.Player.Statistic[nStatLength] { player.plGame.getStatistic(iCupIdLeague, iMode == 1), player.plGame.getStatistic(iCupIdNatCup, iMode == 1), plStatEP, player.plGame.getStatistic(iCupIdWc, iMode == 1) };

        iGoalsTotal = new int[nStatLength] { player.getGoalsTotal(iCupIdLeague, iMode == 1), player.getGoalsTotal(iCupIdNatCup, iMode == 1), player.getGoalsTotal(iCupIdInt, iMode == 1), player.getGoalsTotal(iCupIdWc, iMode == 1) };
      } else {
        //for (int i = 0; i < plStat.Length; i++) plStat[i] = new CornerkickGame.Player.Statistic();

        int iGtIx = -1;
        if (player.plGame.statGame.iGameType == iCupIdLeague) iGtIx = 0;
        else if (player.plGame.statGame.iGameType == iCupIdNatCup) iGtIx = 1;
        else if (player.plGame.statGame.iGameType == iCupIdInt) iGtIx = 2;
        else if (player.plGame.statGame.iGameType == iCupIdWc) iGtIx = 3;

        if (iGtIx >= 0) {
          plStat[iGtIx] = player.plGame.statGame;

          iGoalsTotal[iGtIx] = player.plGame.getGoalsTotal();
        }
      }

      plStat2.Add(new PlayerModel.Stat() { Name = "Spiele", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[0]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Minuten", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[28]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Min./Spiel", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[28]).ToArray(), iRef = plStat.Select(ps => ps == null ? -1 : ps.iStat[0]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Note", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[29]).ToArray(), iRef = plStat.Select(ps => ps == null ? -1 : ps.iStat[30] * 10).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Tore", iStat = plStat.Select(ps => ps == null ? -1 : iMode > 0 ? player.getGoalsTotal(ps.iGameType, iMode == 1) : player.plGame.getGoalsTotal()).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "mit rechts", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[1]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "mit links", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[2]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "per Kopf", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[3]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Tore pro Spiel", iStat = plStat.Select(ps => ps == null ? -1 : iMode > 0 ? player.getGoalsTotal(ps.iGameType, iMode == 1) : player.plGame.getGoalsTotal()).ToArray(), iRef = plStat.Select(ps => ps == null ? -1 : ps.iStat[0]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "11m +", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[4]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "11m -", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[5]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "11m", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[4]).ToArray(), iRef = plStat.Select(ps => ps == null ? -1 : ps.iStat[4] + ps.iStat[5]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Freistoß +", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[6]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Freistoß -", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[7]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Freistoß", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[6]).ToArray(), iRef = plStat.Select(ps => ps == null ? -1 : ps.iStat[6] + ps.iStat[7]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Torvorlagen", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[8]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Torschüsse", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[9]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Tors. pro Tor", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[9]).ToArray(), iRef = iGoalsTotal });
      plStat2.Add(new PlayerModel.Stat() { Name = "Schüsse aufs Tor", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[10]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Torschussvorl.", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[27]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Abspiel +", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[15]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Abspiel -", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[16]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Abspiel", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[15]).ToArray(), iRef = plStat.Select(ps => ps == null ? -1 : ps.iStat[15] + ps.iStat[16]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Pässe abgef.", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[26]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Zweikampf def. +", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[17]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Zweikampf def. -", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[18]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Zweikampf def.", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[17]).ToArray(), iRef = plStat.Select(ps => ps == null ? -1 : ps.iStat[17] + ps.iStat[18]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Schüsse geblockt", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[32]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Zweikampf off. +", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[19]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Zweikampf off. -", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[20]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Zweikampf off.", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[19]).ToArray(), iRef = plStat.Select(ps => ps == null ? -1 : ps.iStat[19] + ps.iStat[20]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Fouls", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[21]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Gelbe Karten", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[22]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Gelb-Rote Karten", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[23]).ToArray() });
      plStat2.Add(new PlayerModel.Stat() { Name = "Rote Karten", iStat = plStat.Select(ps => ps == null ? -1 : ps.iStat[24]).ToArray() });

      return plStat2;
    }

    // Returns
    //   0: contract extension
    //   1: new contract (for own jouth player)
    //   2: new contract for external player
    //   3: new contract for external player with ending contract
    public static int[]? GetContractTypeLength(CornerkickManager.User? _usr, CornerkickManager.Player? plContract)
    {
      if (plContract?.contract == null) return null;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      byte iType = 0;
      byte iContractLengthCurrent = 0;
      if (!CornerkickManager.PlayerTool.ownPlayer(clb, plContract)) {
        iType = 2;

        // If player has a club (and a contract) and not fixed transfer fee and is not on transfer list or contract is ending
        if (plContract.contract != null &&
            ((plContract.contract.iFixTransferFee < 1 && !ckMng.plt.onTransferlist(plContract)) ||
             CornerkickManager.PlayerTool.checkIfContractIsEnding(plContract, ckMng.dtDatum, ckMng.dtSeasonEnd) && plContract.contractNext == null)) iType++;
      } else if (checkIfNewContract(plContract, clb)) {
        iType = 1;
      }

      if (iType == 0) iContractLengthCurrent = plContract.contract.iLength;

      return [iType, iContractLengthCurrent];
    }

    // Returns
    //   true:  external player or jouth player with initial contract (not transfered)
    //   false: own player
    private static bool checkIfNewContract(CornerkickManager.Player? pl, CornerkickManager.Club clbUser)
    {
      if (pl == null) return false;
      if (pl.contract == null) return true;

      bool bForceNewContract = true;

      CornerkickManager.Club? clbPlayer = null;
      if (pl?.contract?.club != null) clbPlayer = pl.contract.club;
      if (clbPlayer == null) return true;

      if (clbUser != null) bForceNewContract = clbUser.iId != clbPlayer.iId;

      if (!bForceNewContract && CornerkickManager.PlayerTool.ownPlayer(clbPlayer, pl, 2)) {
        if (pl.contract.iSalary == CornerkickManager.Finance.iPlayerJouthSalary) bForceNewContract = true;
      }

      return bForceNewContract;
    }

    public static CornerkickManager.Player.Contract[]? GetContract(CornerkickManager.User? _usr, ContractsModel.PlayerSalary ps, out string sMsg)
    {
      return GetContract(_usr, ps.iPlayerId, (byte)ps.iYears, ps.iSalary, out sMsg, ps.iBonusPlay, ps.iBonusPoint, ps.iBonusGoal, ps.iFixedFee, ps.bNegotiateNextSeason);
    }
    public static CornerkickManager.Player.Contract[]? GetContract(CornerkickManager.User? _usr, int iPlayerId, byte iYears, int iSalary, out string sMsg, int iBonusPlay = 0, int iBonusGoal = 0, int iBonusPoint = 0, int iFixedFee = 0, bool bNegotiate = true)
    {
      sMsg = "";

      if (iPlayerId < 0) {
        sMsg = "Invalid player";
        return null;
      }

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      CornerkickManager.Player? plSalary = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (plSalary == null) {
        sMsg = "Invalid player";
        return null;
      }

      int iGamesPerSeason = 0;
      CornerkickManager.Cup league = ckMng.tl.getCup(iCupIdLeague, clb.iLand, clb.iDivision);
      if (league != null) iGamesPerSeason = league.getMatchdays(clb);

      bool bForceNewContract = checkIfNewContract(plSalary, clb);

      if (bForceNewContract && iYears < 1) {
        sMsg = "Minimum contract length: 1 year";
        return null;
      }

      CornerkickManager.Player.Contract contract = ckMng.plt.negotiatePlayerContract(plSalary, clb, iYears, iSalaryOffer: iSalary, iBonusPlayOffer: iBonusPlay, iBonusPointOffer: iBonusPoint, iBonusGoalOffer: iBonusGoal, ltCupBonusOffer: plSalary.contract != null ? plSalary.contract.ltCupBonus : null, iGamesPerSeason: iGamesPerSeason, iFixedFeeOffer: iFixedFee, bNegotiate: bNegotiate, bForceNewContract: bForceNewContract);

      // Create reduced contract to return
      CornerkickManager.Player.Contract cctReq = new CornerkickManager.Player.Contract();
      cctReq.iLength = iYears;            // Length of contract [years] (use relative years e.g. only additional years for extended contract)
      cctReq.iSalary = Tool.roundInt(contract.iSalary, 2);  // Salary [$/month]
      cctReq.iPlay = Tool.roundInt(contract.iPlay, 2);  // Bonus play
      cctReq.iPoint = Tool.roundInt(contract.iPoint, 2);  // Bonus point
      cctReq.iGoal = Tool.roundInt(contract.iGoal, 2);  // Bonus goal
      cctReq.iFixTransferFee = Tool.roundInt(contract.iFixTransferFee, 2);  // Fix transfer fee
      cctReq.bTransferCurrentSeason = contract.bTransferCurrentSeason; // Player was transferred in current season (no further transfer allowed)
      cctReq.fMood = contract.fMood; // Player mood while negotiating

      // Get not yet finish negotiated contract
      CornerkickManager.Transfer.Offer offer = ckMng.tr.getOffer(plSalary, clb);
      if (offer?.contract != null) cctReq.fMood = offer.contract.fMood; // Player mood while negotiating

      CornerkickManager.Player.Contract cctOff = new CornerkickManager.Player.Contract();
      if (offer?.contractOffered == null) {
        cctOff = cctReq.Clone();
      } else {
        cctOff.iLength = offer.contractOffered.iLength;  // Length of contract [years]
        cctOff.iSalary = offer.contractOffered.iSalary;  // Salary [$/month]
        cctOff.iPlay = offer.contractOffered.iPlay;    // Bonus play
        cctOff.iPoint = offer.contractOffered.iPoint;   // Bonus point
        cctOff.iGoal = offer.contractOffered.iGoal;    // Bonus goal
        cctOff.iFixTransferFee = offer.contractOffered.iFixTransferFee;  // Fix transfer fee
        cctOff.bTransferCurrentSeason = offer.contractOffered.bTransferCurrentSeason; // Player was transferred in current season (no further transfer allowed)

        cctOff.ltCupBonus = offer.contractOffered.ltCupBonus;
      }

      return [cctReq, cctOff];
    }

    public static float GetContractQuotientOfferedRequired(CornerkickManager.User? _usr, int iPlayerId, CornerkickManager.Player.Contract cctOff)
    {
      return GetContractQuotientOfferedRequired(_usr, iPlayerId, cctOff.iLength, cctOff.iSalary, cctOff.iPlay, cctOff.iPoint, cctOff.iGoal, cctOff.ltCupBonus, cctOff.iFixTransferFee);
    }
    public static float GetContractQuotientOfferedRequired(CornerkickManager.User? _usr, int iPlayerId, int iYears, int iSalaryOff, int iBonusPlayOff, int iBonusPointOff, int iBonusGoalOff, List<CornerkickManager.Player.Contract.CupBonus> ltCupBonus, int iFixTransferFee)
    {
      if (iPlayerId < 0) return -1f;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return -1f;

      int iGamesPerSeason = 0;
      CornerkickManager.Cup league = ckMng.tl.getCup(iCupIdLeague, clb.iLand, clb.iDivision);
      if (league != null) iGamesPerSeason = league.getMatchdays(clb);

      CornerkickManager.Player? pl = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl == null) return -1f;

      // Add current contract years
      byte iYearsReq = (byte)iYears;
      if (!checkIfNewContract(pl, clb)) {
        iYearsReq += pl.contract.iLength;
      }

      int iSalaryTotReq = CornerkickManager.PlayerTool.getSalaryTotalRequired(pl, iYearsReq, ckMng.dtDatum, ckMng.dtSeasonEnd, iFixedFee: iFixTransferFee, user: _usr);
      int iSalaryTotOff = CornerkickManager.PlayerTool.getSalaryTotal(iSalary: iSalaryOff, iBonusPlay: iBonusPlayOff, iBonusPoint: iBonusPointOff, iBonusGoal: iBonusGoalOff, ltCupBonus: ltCupBonus, clbPlayer: clb, iGamesPerSeason: iGamesPerSeason, fBonusGoalFactor: pl.getGoalBonusFactor());

      return iSalaryTotOff / (float)iSalaryTotReq;
    }

    // iMode: 0 - Extention, 1 - new contract
    //const byte iContractLengthMax = 5;
    public static string NegotiateContract(CornerkickManager.User? _usr, ContractsModel.PlayerSalary ps)
    {
      CornerkickManager.Club? clbUser = MemberController.ckClub(_usr);
      if (clbUser == null) return "Error";

      if (ps.iPlayerId < 0) return "Error";
      if (ps.iYears < 0) return "0";

      // Get player
      CornerkickManager.Player? plContract = ckMng.ltPlayer.Find(p => p.plGame.iId == ps.iPlayerId);
      if (plContract == null) return "Error";

      if (ps.iSalary < 0) return "Error";
      if (ps.iBonusPlay < 0) return "Error";
      if (ps.iBonusPoint < 0) return "Error";
      if (ps.iBonusGoal < 0) return "Error";
      if (ps.iFixedFee < 0) return "Error";

      string sReturn = "";
      if (CornerkickManager.PlayerTool.ownPlayer(clbUser, plContract)) { // Contract extention
        byte iContractLegth = ps.iYears;

        bool bForceNewContract = checkIfNewContract(plContract, clbUser);
        if (bForceNewContract && ps.iYears < 1) return "Fehler: Minimale Vertragslänge = 1 Jahr";
        if (!bForceNewContract) iContractLegth += plContract.contract.iLength;

        if (iContractLegth > iContractLengthMax) return "Fehler: Maximale Vertragslänge = " + iContractLengthMax.ToString() + " Jahre";

        plContract.contract.iLength = iContractLegth;
        plContract.contract.iSalary = ps.iSalary;
        plContract.contract.iPlay = ps.iBonusPlay;
        plContract.contract.iPoint = ps.iBonusPoint;
        plContract.contract.iGoal = ps.iBonusGoal;
        plContract.contract.ltCupBonus = ps.ltCupBonus;
        plContract.contract.iFixTransferFee = ps.iFixedFee;
        plContract.contract.fMood = ps.fPlayerMood;

        sReturn = "Der Vertrag mit " + plContract.plGame.sName + " wurde ";
        if (ps.iYears > 0) sReturn += "um " + ps.iYears.ToString() + " Jahre verlängert.";
        else sReturn += "geändert.";

        if (plContract.contract?.club != null) {
          CornerkickManager.Club clb = plContract.contract.club;

          for (int iPlJ = 0; iPlJ < clb.ltPlayerJouth.Count; iPlJ++) {
            CornerkickManager.Player plJ = clb.ltPlayerJouth[iPlJ];

            if (plJ.plGame.iId == plContract.plGame.iId) {
              /*
              if ((int)plJ.plGame.getAge(ckMng.dtDatum) < 16) return "Fehler: Spieler zu jung für Profivertrag";

              clb.ltPlayerJouth.RemoveAt(iPlJ);
              clb.ltPlayer.Add(plJ);

              // Reset jersey number
              plJ.plGame.iNr = 0;

              // Add club history to player
              plContract.ltClubHistory.Add(new CornerkickManager.Player.ClubHistory() {
                club = clb,
                dt = ckMng.dtDatum,
                iTransferFee = 0,
                bJouth = false
              });

              sReturn = "Der Jugendspieler " + plContract.plGame.sName + " hat ihr Angebot angenommen und gehört ab sofort dem Profikader an.";
              */

              if (moveJouthPlayerToProTeam(plJ, clb, out sReturn)) break;
              return sReturn;
            }
          }
        }

        // Remove hidden entry from transfer list
        ckMng.tr.removePlayerFromTransferlist(plContract);
      } else { // New contract
        if (ps.iYears < 1) return "0";
        if (ps.iYears > iContractLengthMax) return "Fehler: Maximale Vertragslänge = " + iContractLengthMax.ToString() + " Jahre";

        // Create new offer
        CornerkickManager.Transfer.Offer offer = new CornerkickManager.Transfer.Offer();
        CornerkickManager.Player.Contract contract = new CornerkickManager.Player.Contract();
        contract.iLength = (byte)ps.iYears;
        contract.iSalary = ps.iSalary;
        contract.iPlay = ps.iBonusPlay;
        contract.iPoint = ps.iBonusPoint;
        contract.iGoal = ps.iBonusGoal;
        if (plContract.contract != null) contract.ltCupBonus = ps.ltCupBonus;
        contract.iFixTransferFee = ps.iFixedFee;
        contract.fMood = ps.fPlayerMood;
        contract.club = clbUser;
        offer.contract = contract;
        offer.bNextSeason = ps.bNegotiateNextSeason;

        ckMng.tr.addChangeOffer(ps.iPlayerId, offer);
        sReturn = "Sie haben sich mit dem Spieler " + plContract.plGame.sName + " auf eine Zusammenarbeit über " + ps.iYears.ToString() + " Jahre geeinigt.";
      }

      return sReturn;
    }

    public static bool moveJouthPlayerToProTeam(CornerkickManager.Player plJ, CornerkickManager.Club clb, out string sMsg)
    {
      sMsg = "";

      if ((int)plJ.plGame.getAge(ckMng.dtDatum) < 16) {
        sMsg = "Fehler: Spieler zu jung für Profivertrag";
        return false;
      }

      clb.ltPlayerJouth.Remove(plJ);
      clb.ltPlayer.Add(plJ);

      // Reset jersey number
      plJ.plGame.iNr = 0;

      // Add club history to player
      plJ.ltClubHistory.Add(new CornerkickManager.Player.ClubHistory() {
        club = clb,
        dt = ckMng.dtDatum,
        iTransferFee = 0,
        bJouth = false
      });

      sMsg = "Der Jugendspieler " + plJ.plGame.sName + " hat ihr Angebot angenommen und gehört ab sofort dem Profikader an.";

      return true;
    }

    private static List<CornerkickManager.Player.Contract.CupBonus>? getCupBonus(int[] iaCupBonus, CornerkickManager.Club clb)
    {
      if (iaCupBonus == null) return null;
      if (clb == null) return null;

      List<CornerkickManager.Player.Contract.CupBonus>? ltCupBonus = null;
      for (int jCb = 0; jCb < iaCupBonus.Length; jCb++) {
        if (jCb % 3 == 0) {
          if (ltCupBonus == null) ltCupBonus = new List<CornerkickManager.Player.Contract.CupBonus>();

          CornerkickManager.Cup? cup = null;
          if (iaCupBonus[jCb] == 1) cup = ckMng.tl.getCup(iaCupBonus[jCb], clb.iLand, clb.iDivision);
          else if (iaCupBonus[jCb] == 2) cup = ckMng.tl.getCup(iaCupBonus[jCb], clb.iLand);
          else cup = ckMng.tl.getCup(iaCupBonus[jCb]);
          jCb++;

          ltCupBonus.Add(
            new CornerkickManager.Player.Contract.CupBonus() {
              cup = cup,
              iPlace = (byte)iaCupBonus[jCb++],
              iValue = iaCupBonus[jCb]
            }
          );
        }
      }

      return ltCupBonus;
    }

    public static PlayerModel.ScoutingResult? Scout(CornerkickManager.User? _usr, int iScoutId, int iPlayerId)
    {
      return Scout(_usr, iScoutId, ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId));
    }
    public static PlayerModel.ScoutingResult? Scout(CornerkickManager.User? _usr, int iScoutId, CornerkickManager.Player? plMng)
    {
      if (plMng == null) return null;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      CornerkickManager.Main.Staff.Scout? sc = clb.staff.ltScouts.Find(s => s.iId == iScoutId);
      if (sc == null) return null;

      List<CornerkickManager.Main.Staff.Scout.PlayerData.Details> ltPdd = sc.scoutPlayer(plMng.plGame, ckMng.dtDatum);

      if (sc.ltPlayerData.Count < 1) return null;

      PlayerModel.ScoutingResult sr = new PlayerModel.ScoutingResult();

      foreach (CornerkickManager.Main.Staff.Scout.PlayerData.Details pdd in ltPdd) sr.ltSkills.Add(GetScoutingSkillData(plMng, pdd.iSkillIx, clb));

      int iMinutesActivate = -1;
      if (sc.bFreelancer) {
        CornerkickManager.Finance.doTransaction(clb, ckMng.dtDatum, -sc.getSalary(), CornerkickManager.Finance.iTransferralTypePayScouting);
      } else {
        iMinutesActivate = (int)(ckMng.dtDatum.Date.Add(new TimeSpan(12, 00, 00)) - ckMng.dtDatum).TotalMinutes;
        if (iMinutesActivate < 0) iMinutesActivate += 24 * 60;
      }

      return sr;
    }

    private static PlayerModel.ScoutingResult.Skill? GetScoutingSkillData(CornerkickManager.Player? plMng, int iSkillIx, CornerkickManager.Club clb)
    {
      if (plMng == null) return null;

      CornerkickManager.Main.Staff? staff = ClubController.getClubStaff(clb);
      if (staff == null) return null;

      PlayerModel.ScoutingResult.Skill srs = new PlayerModel.ScoutingResult.Skill() {
        iSkillIx = iSkillIx,
        fSkillAve = staff.getScoutedSkill(plMng.plGame, iSkillIx),
        n_scout = staff.getScoutingData(plMng.plGame, iSkillIx).Count
      };

      return srs;
    }

    public static List<PlayerModel.ClubHistory>? GetClubHistoryTable(int iPlayerId)
    {
      CornerkickManager.Player? pl = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl == null) return null;

      //The table or entity I'm querying
      List<PlayerModel.ClubHistory> ltClubHistory = new List<PlayerModel.ClubHistory>();

      if (pl.ltClubHistory != null) {
        for (int iCh = 0; iCh < pl.ltClubHistory.Count; iCh++) {
          CornerkickManager.Player.ClubHistory ch = pl.ltClubHistory[iCh];

          // Get club name
          string sClubName = "vereinslos";
          if (ch.club != null) {
            sClubName = ch.club.sName;
          }
          if (ch.bJouth) sClubName += " (Jugend)";

          ltClubHistory.Add(new PlayerModel.ClubHistory {
            iIx = iCh,
            sClubTakeName = sClubName,
            dt = ch.dt,
            iValue = pl.getValueHistory(ch.dt) * 1000,
            iTransferFee = ch.iTransferFee
          });
        }
      }

      return ltClubHistory;
    }

    public static List<PlayerModel.InjuryHistory>? GetInjuryHistoryTable(int iPlayerId)
    {
      CornerkickManager.Player? plMng = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (plMng == null) return null;

      CornerkickGame.Player pl = plMng.plGame;

      //The table or entity I'm querying
      List<PlayerModel.InjuryHistory> ltInjuryHistory = new List<PlayerModel.InjuryHistory>();

      if (pl.ltInjuryHistory != null) {
        for (int iIh = 0; iIh < pl.ltInjuryHistory.Count; iIh++) {
          CornerkickGame.Player.InjuryHistory ih = pl.ltInjuryHistory[iIh];

          // Remove corrupt entry
          if (ih.injury == null) {
            pl.ltInjuryHistory.RemoveAt(iIh);
            iIh--;
            continue;
          }

          ltInjuryHistory.Add(new PlayerModel.InjuryHistory {
            iIx = iIh,
            dt = ih.dt,
            sInjuryName = ih.injury.sName,
            iInjuryLength = ih.injury.iLengthStart
          });
        }
      }

      return ltInjuryHistory;
    }

    public static float[] GetPassDetails(int iPlayerId)
    {
      return GetPassDetails(ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId));
    }
    public static float[] GetPassDetails(CornerkickManager.Player? pl)
    {
      if (pl?.plGame == null) return [];

      return [
          CornerkickGame.Tool.getMaxPassLength(pl.plGame, bHighPass: true),
          CornerkickGame.Tool.getMaxPassLength(pl.plGame, bHighPass: false),
          pl.plGame.fFootR,
          pl.plGame.fFootL,
          0.8f
        ];
    }

    public class ComparePlayer
    {
      public CornerkickManager.Player? player { get; set; }
      public CornerkickManager.User? user { get; set; }
    }
    public static List<ComparePlayer> ltComparePlayer = new List<ComparePlayer>();
    public static CornerkickManager.Player? MarkComparePlayer(CornerkickManager.User? _usr, int iPlayerId)
    {
      if (_usr == null) return null;

      CornerkickManager.Player? pl = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl == null) return null;

      ComparePlayer? cp = ltComparePlayer.Find(c => c.user == _usr);
      if (cp == null) {
        ltComparePlayer.Add(new ComparePlayer() {
          user = _usr,
          player = pl
        });
      } else {
        cp.player = pl;
      }

      return pl;
    }

    public static CornerkickManager.Player? GetComparePlayer(CornerkickManager.User? _usr)
    {
      if (_usr == null) return null;

      return ltComparePlayer.Find(c => c.user == _usr)?.player;
    }

    public static void DeleteComparePlayer(CornerkickManager.User? _usr)
    {
      if (_usr == null) return;

      ltComparePlayer.RemoveAll(c => c.user == _usr);
    }

  }
}
