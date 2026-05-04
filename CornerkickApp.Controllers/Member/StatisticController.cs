using CornerkickApp.Shared.Models;
using System.Linq;
using System.Numerics;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  public class StatisticController
  {
    public static StatisticModel Get(CornerkickManager.User _usr)
    {
      StatisticModel model = new StatisticModel();

      model.iLand = 0;
      model.iDivision = 0;

      CornerkickManager.Club? clbUser = MemberController.ckClub(_usr);
      if (clbUser != null) {
        model.iLand     = clbUser.iLand;
        model.iDivision = clbUser.iDivision;
      }

      return model;
    }

    public static List<StatisticModel.TableEntryTeams> GetTableTeams(int iLand, int iDivision)
    {
      //The table or entity I'm querying
      List<StatisticModel.TableEntryTeams> ltDeTeams = new List<StatisticModel.TableEntryTeams>();

      int iC = 0;
      foreach (CornerkickManager.Club clb in CkAppShared.ckMng.ltClubs) {
        if (clb.iLand < 0) continue;
        if (clb.bNation) continue;
        if (iLand >= 0 && iLand != clb.iLand) continue;
        if (iDivision >= 0 && iDivision != clb.iDivision) continue;

        float[] fAve = CornerkickManager.Tool.getTeamAve(clb, CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd, bTeamValue: true);
        int iVal = (int)fAve[5];

        float[] fAve11 = CornerkickManager.Tool.getTeamAve(clb, CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd, ptPitch: CkAppShared.ckMng.game.ptPitch, iPlStop: 11, bTeamValue: true);
        int iVal11 = (int)fAve11[5];

        CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, clb.iLand, clb.iDivision);
        string sLeagueName = "-";
        if (league != null) sLeagueName = league.sName;

        ltDeTeams.Add(new StatisticModel.TableEntryTeams {
          iIx = iC,
          iTeamId = clb.iId,
          sTeamName = clb.sName,
          sEmblem = ClubController.getClubEmblemImg(clb, "height: 20px; width: 20px; object-fit: contain", bTiny: true),
          fTeamAveSkill = fAve[3],
          fTeamAveAge = fAve[4],
          iTeamValueTotal = iVal,
          nPlayer = clb.ltPlayer.Count,
          fTeamAveSkill11 = fAve11[3],
          fTeamAveAge11 = fAve11[4],
          iTeamValueTotal11 = iVal11,
          fAttrFactor = clb.getAttractionFactor(CkAppShared.ckMng.iSeason, ltCups: CkAppShared.ckMng.ltCups, dtNow: CkAppShared.ckMng.dtDatum),
          sLeague = sLeagueName
        });

        iC++;
      }

      return ltDeTeams;
    }

    public static List<StatisticModel.TableEntryStadiums> GetStadiumsTable()
    {
      //The table or entity I'm querying
      List<StatisticModel.TableEntryStadiums> ltDeStadiums = new List<StatisticModel.TableEntryStadiums>();

      foreach (CornerkickManager.Club clb in CkAppShared.ckMng.ltClubs) {
        if (clb == null) continue;
        if (clb.stadium == null) continue;

        StatisticModel.TableEntryStadiums dteStadium = new StatisticModel.TableEntryStadiums();

        dteStadium.sName = string.IsNullOrEmpty(clb.stadium.sName) ? clb.sName + " Stadion" : clb.stadium.sName;
        dteStadium.sClubName = clb.sName;
        dteStadium.iType0 = clb.stadium.getSeats(0);
        dteStadium.iType1 = clb.stadium.getSeats(1);
        dteStadium.iType2 = clb.stadium.getSeats(2);

        foreach (CornerkickGame.Stadium.Block blk in clb.stadium.blocks) {
          if (blk.iSeatsDaysConstruct > 0) {
            if      (blk.iType == 0) dteStadium.iType0Ctn += blk.iSeats;
            else if (blk.iType == 1) dteStadium.iType1Ctn += blk.iSeats;
            else if (blk.iType == 2) dteStadium.iType2Ctn += blk.iSeats;
          }
        }

        dteStadium.iTotal = dteStadium.iType0 + dteStadium.iType1 + dteStadium.iType2;
        dteStadium.iTotalCtn = dteStadium.iType0Ctn + dteStadium.iType1Ctn + dteStadium.iType2Ctn;

        dteStadium.bTopring = clb.stadium.facility != null && clb.stadium.facility.bTopring && clb.stadium.facility.iTopringDaysConstruct == 0;

        ltDeStadiums.Add(dteStadium);
      }

      ltDeStadiums = ltDeStadiums.OrderByDescending(o => o.iTotal).ToList().GetRange(0, 20);
      for (int i = 0; i < ltDeStadiums.Count; i++) {
        ltDeStadiums[i].iIx = i + 1;
      }

      return ltDeStadiums;
    }

    public static StatisticPlayerModel GetStatisticPlayerModel(CornerkickManager.Club? clb = null)
    {
      StatisticPlayerModel mdStatPlayer = new StatisticPlayerModel();

      mdStatPlayer.iNation = -1;
      mdStatPlayer.ddlNations = new List<SelectListItem>();
      mdStatPlayer.ddlNations.Add(new SelectListItem {
        Text = "Weltauswahl",
        Value = mdStatPlayer.iNation.ToString()
      });
      for (int iN = 0; iN < CkAppShared.iNations.Length; iN++) {
        mdStatPlayer.ddlNations.Add(new SelectListItem {
          Text = CornerkickManager.Main.sLand[CkAppShared.iNations[iN]],
          Value = CkAppShared.iNations[iN].ToString()
        });
      }

      mdStatPlayer.iFilterLand   = clb != null ? clb.iLand     : -1; // All nations
      mdStatPlayer.iFilterLeague = clb != null ? clb.iDivision : -1; // All leagues
      mdStatPlayer.iFilterClub   = -1; // All clubs

      mdStatPlayer.ddlFilterLand =
      [
        new SelectListItem {
          Text = "Alle Länder",
          Value = "-1"
        },
        .. Tool.getCountries().Result,
      ];

      if (mdStatPlayer.iFilterLand >= 0) {
        mdStatPlayer.ddlFilterLeague =
        [
          new SelectListItem {
            Text = "Alle Ligen",
            Value = "-1"
          },
          .. Tool.getLeagues(mdStatPlayer.iFilterLand).Result,
        ];
      }

      mdStatPlayer.iFormation = 19;
      mdStatPlayer.ltsFormations = new List<SelectListItem>();
      for (int i = 0; i < CkAppShared.ckMng.ltFormationen.Count; i++) {
        mdStatPlayer.ltsFormations.Add(new SelectListItem {
          Text = (i + 1).ToString() + " - " + CkAppShared.ckMng.ltFormationen[i].sName,
          Value = i.ToString()
        });
      }

      /*
      CornerkickManager.Club clbUser = ckClub();

      int iLand = 0;
      int iDivision = 0;
      if (clbUser != null) {
        iLand = clbUser.iLand;
        iDivision = clbUser.iDivision;
      }
      */

      mdStatPlayer.sPlayerSkillBest = new string[CornerkickManager.PlayerTool.sSkills.Length][];
      for (byte iS = 0; iS < CornerkickManager.PlayerTool.sSkills.Length; iS++) {
        if (iS == 17) continue; // Game intelligence skill skill
        if (iS == CornerkickGame.Player.iIndTrainingIxFoot) continue; // Both foot skill

        mdStatPlayer.sPlayerSkillBest[iS] = new string[5]; // Skill name, player name, skill value, club, player value

        CornerkickManager.Player? plSkillBest = null;
        float fSkillBest = 0f;

        foreach (CornerkickManager.Player pl in CkAppShared.ckMng.ltPlayer) {
          if (pl.checkRetired()) continue;

          // Get position role
          byte iPos = 0;
          for (byte jPos = 1; jPos <= pl.plGame.fExperiencePos.Length; jPos++) {
            if (pl.plGame.checkMainPos(jPos)) {
              iPos = jPos;
              break;
            }
          }

          float fSkillTmp = CornerkickGame.Tool.getSkillEff(pl.plGame, iS, iPos);
          if (fSkillTmp > fSkillBest) {
            plSkillBest = pl;
            fSkillBest = fSkillTmp;
          }
        }

        mdStatPlayer.sPlayerSkillBest[iS][0] = CornerkickManager.PlayerTool.sSkills[iS];
        if (plSkillBest != null) {
          mdStatPlayer.sPlayerSkillBest[iS][1] = "<a style=\"text-decoration: none\" href=\"/member/playerdetails/" + plSkillBest.plGame.iId.ToString() + "\">" + plSkillBest.plGame.sName + "</a>";
          mdStatPlayer.sPlayerSkillBest[iS][2] = fSkillBest.ToString("0.000");
          string sClubName = "vereinslos";
          if (plSkillBest.contract?.club != null) sClubName = plSkillBest.contract.club.sName;
          mdStatPlayer.sPlayerSkillBest[iS][3] = sClubName;
          mdStatPlayer.sPlayerSkillBest[iS][4] = (plSkillBest.getValue(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd) * 1000).ToString("N0");
        }
      }

      mdStatPlayer.ddlFilterCup = new List<SelectListItem>();

      /*
      foreach (int iN in CkAppShared.iNations) {
        for (byte iD = 0; iD < 2; iD++) {
          CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(1, iN, iD);
          if (league != null) ddlFilterCup.Add(new SelectListItem { Text = league.sName, Value = "1," + iN.ToString() + "," + iD.ToString() });
        }

        CornerkickManager.Cup cupNat = CkAppShared.ckMng.tl.getCup(2, iN, -1);
        ddlFilterCup.Add(new SelectListItem { Text = cupNat.sName, Value = "2," + iN.ToString() + ",-1" });
      }
      */

      CornerkickManager.Cup cupL = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague);
      if (cupL != null) mdStatPlayer.ddlFilterCup.Add(new SelectListItem { Text = "Liga", Value = CkAppShared.iCupIdLeague.ToString() });

      CornerkickManager.Cup cupN = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdNatCup);
      if (cupN != null) mdStatPlayer.ddlFilterCup.Add(new SelectListItem { Text = "Nat. Pokal", Value = CkAppShared.iCupIdNatCup.ToString() });

      CornerkickManager.Cup cupG = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdInt);
      if (cupG != null) mdStatPlayer.ddlFilterCup.Add(new SelectListItem { Text = "Int. Pokal", Value = CkAppShared.iCupIdInt.ToString() });

      CornerkickManager.Cup cupW = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdWc);
      if (cupW != null) mdStatPlayer.ddlFilterCup.Add(new SelectListItem { Text = cupW.sName, Value = CkAppShared.iCupIdWc.ToString() });

      mdStatPlayer.iFilterCup = CkAppShared.iCupIdLeague;

      return mdStatPlayer;
    }

    public static TeamModel.TeamData GetBest11(CornerkickManager.User _usr, int iNat, int iF, bool bJouth = false)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      CornerkickManager.Main.Staff? staff = ClubController.getClubStaff(clb);

      TeamModel.TeamData tD = new TeamModel.TeamData();
      tD.ltPlayer2 = new List<TeamModel.Player>();

      CornerkickGame.Tactic.Formation frm = CkAppShared.ckMng.ltFormationen[iF];

      List<CornerkickManager.Player> ltPlayerBest = new List<CornerkickManager.Player>();

      for (byte iP = 0; iP < 11; iP++) {
        float fStrength = 0f;
        tD.ltPlayer2.Add(null);
        ltPlayerBest.Add(null);

        byte iPosExact = CornerkickGame.Tool.getPosRole(frm.positions[iP].pt, CkAppShared.ckMng.game.ptPitch);
        byte iPos = CornerkickGame.Tool.getBasisPos(iPosExact);

        try {
        foreach (CornerkickManager.Player pl in CkAppShared.ckMng.ltPlayer) {
          if (pl.bRetire) continue;
          if (bJouth && pl.plGame.getAge(CkAppShared.ckMng.dtDatum) > 18f) continue;
          if (iNat >= 0 && pl.iNat1 != iNat) continue;

          // Check if club is nation
          if (pl.contract?.club != null) {
            if (pl.contract.club.bNation) continue;
          }

          // Check if same player already in same role
          if (iPos > 0) {
            bool bSame = false;
            foreach (CornerkickManager.Player plSame in ltPlayerBest) {
              if (plSame != null && plSame.plGame.iId == pl.plGame.iId && plSame.plGame.fExperiencePos[iPos - 1] > 0.999) {
                bSame = true;
                break;
              }
            }
            if (bSame) continue;
          }

          float fStrengthTmp = CornerkickGame.Tool.getAveSkill(pl.plGame, iPos);
          if (fStrengthTmp > fStrength) {
            if (tD.ltPlayer2[iP] == null) tD.ltPlayer2[iP] = new TeamModel.Player();

            tD.ltPlayer2[iP].iId = pl.plGame.iId;
            tD.ltPlayer2[iP].sName = pl.plGame.sName;
            tD.ltPlayer2[iP].iNb = (byte)(iP + 1);
            if (pl.iNat1 >= 0 && pl.iNat1 < CornerkickManager.Main.sLandShort.Length) tD.ltPlayer2[iP].sNat = CornerkickManager.Main.sLandShort[pl.iNat1];
            //tD.ltPlayer2[iP].sPortrait = PlayerController.getPlayerPortrait(pl, bSmall: true);
            tD.ltPlayer2[iP].sPortrait = PlayerController.getPlayerPortraitHtmlImg(pl, sStyle: "height: 100%; width: 100%; object-fit: contain", bSmall: true);
            tD.ltPlayer2[iP].iPos = iPos;

            if (frm.positions.Length > iP) {
              tD.ltPlayer2[iP].ptPos = new TeamModel.Point(frm.positions[iP].pt);

              float[]? fSkills = staff != null ? staff.getScoutedSkills(pl.plGame) : null;
              float fSkillAveScouted = CornerkickGame.Tool.getAveSkill(pl.plGame, iPos, fSkills: fSkills);
              tD.ltPlayer2[iP].sSkillAve = fSkillAveScouted > 0f ? fSkillAveScouted.ToString("0.0") : "?";
            }

            fStrength = fStrengthTmp;

            if (pl.contract?.club != null) tD.ltPlayer2[iP].sTeamname = pl.contract.club.sName;
            tD.ltPlayer2[iP].sAge = pl.plGame.getAge(CkAppShared.ckMng.dtDatum).ToString("0.0");

            ltPlayerBest[iP] = pl;
          }
        }
        } catch (Exception ex) {
          CkAppShared.ckMng.tl.writeLog(ex.Message, "GetBest11 - Player: " + iP.ToString() + ", iNat: " + iNat.ToString() + ", bJouth: " + bJouth.ToString(), bError: true);
        }
      }

      float[] fTeamAve11 = CornerkickManager.Tool.getTeamAve(ltPlayerBest, CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd, iPlStop: 11);
      tD.fTeamAveStrength = fTeamAve11[3];
      tD.fTeamAveAge      = fTeamAve11[4];

      return tD;
    }

    public static List<StatisticPlayerModel.TableEntryPlayer> GetPlayer(bool bSeason, CornerkickManager.Cup? cup)
    {
      if (cup == null) return new List<StatisticPlayerModel.TableEntryPlayer>();

      return GetPlayer(bSeason, cup.iId, iCupId2: cup.iId2, iCupId3: cup.iId3);
    }
    public static List<StatisticPlayerModel.TableEntryPlayer> GetPlayer(bool bSeason, int iCupId, int iCupId2 = -1, int iCupId3 = -1)
    {
      //The table or entity I'm querying
      List<StatisticPlayerModel.TableEntryPlayer> ltDePlayer = new List<StatisticPlayerModel.TableEntryPlayer>();

      List<CornerkickManager.Cup>? ltCupFilter = CkAppShared.ckMng.ltCups.FindAll(c => c.iId == iCupId && (iCupId2 < 0 || c.iId2 == iCupId2) && (iCupId3 < 0 || c.iId3 == iCupId3));

      foreach (CornerkickManager.Player pl in CkAppShared.ckMng.ltPlayer) {
        if (pl == null) continue;
        if (pl.bRetire) continue;
        if (string.IsNullOrEmpty(pl.plGame.sName)) continue;
        if (pl.contract == null) continue;

        if (ltCupFilter != null) {
          bool bFound = false;
          foreach (CornerkickManager.Cup cupFilter in ltCupFilter) {
            if (cupFilter.getParticipants().Contains(pl.contract.club)) {
              bFound = true;
              break;
            }
          }

          if (!bFound) continue; // Player not in cup participants
        }
        //if (iCupId2 >= 0 && pl.contract.club.iLand != iCupId2) continue;
        //if (iCupId3 >= 0 && pl.contract.club.iDivision != iCupId3) continue;

        CornerkickGame.Player.Statistic stat = pl.plGame.getStatistic(iCupId, bSeason: bSeason/*, iGameType2: iCupId2*/);
        if (stat == null) continue;

        int[] iStat = stat.iStat;

        if (iStat[0] < 1) continue;

        int iGoalsTotal = pl.getGoalsTotal(iCupId, bSeason/*, iGameType2: iLand*/);

        try {
          ltDePlayer.Add(
            new StatisticPlayerModel.TableEntryPlayer() {
              iPlayerIx = ltDePlayer.Count,
              iPlayerId = pl.plGame.iId,
              sName = pl.plGame.sName.Trim(),
              sNat = pl.iNat1 >= 0 && pl.iNat1 < CornerkickManager.Main.sLandShort.Length ? CornerkickManager.Main.sLandShort[pl.iNat1] : "UBK",
              fAge = pl.plGame.getAge(CkAppShared.ckMng.dtDatum),
              sClubName = pl.contract == null ? "vereinslos" : pl.contract.club.sName,
              iGames = iStat[0],
              iMinutes = iStat[28],
              iGoals = iGoalsTotal,
              iGoalsRight = iStat[1],
              iGoalsLeft = iStat[2],
              iGoalsHeader = iStat[3],
              fGoalsPerGame = iStat[0] > 0 ? iGoalsTotal / (float)iStat[0] : -1,
              iPenaltyP = iStat[4],
              iPenaltyM = iStat[5],
              fPenalty = iStat[4] + iStat[5] > 0 ? iStat[4] / (float)(iStat[4] + iStat[5]) : -1,
              iFreekickP = iStat[6],
              iFreekickM = iStat[7],
              fFreekick = iStat[6] + iStat[7] > 0 ? iStat[6] / (float)(iStat[6] + iStat[7]) : -1,
              iAssists = iStat[8],
              iShoots = iStat[9],
              iShootsOG = iStat[10],
              iAssistShoots = iStat[27],
              iPassP = iStat[15],
              iPassM = iStat[16],
              fPass = iStat[15] + iStat[16] > 0 ? iStat[15] / (float)(iStat[15] + iStat[16]) : -1,
              iDuelDefP = iStat[17],
              iDuelDefM = iStat[18],
              fDuelDef = iStat[17] + iStat[18] > 0 ? iStat[17] / (float)(iStat[17] + iStat[18]) : -1,
              iDuelOffP = iStat[19],
              iDuelOffM = iStat[20],
              fDuelOff = iStat[19] + iStat[20] > 0 ? iStat[19] / (float)(iStat[19] + iStat[20]) : -1,
              iFouls = iStat[21],
              iCardY = iStat[22],
              iCardYR = iStat[23],
              iCardR = iStat[24],
              iBallContacts = iStat[25],
              iPassStolen = iStat[26],
              fGrade = iStat[30] > 0 ? (iStat[29] * 0.1f) / iStat[30] : 0f
            }
          );
        } catch (Exception e) {
          Console.WriteLine(e.Message);
        }
      }

      return ltDePlayer;
    }

    public static List<PlayerModel.ClubHistory> StatisticGetTransferTable()
    {
      //The table or entity I'm querying
      List<PlayerModel.ClubHistory> ltDeClubHistory = new List<PlayerModel.ClubHistory>();

      int iT = 1;
      foreach (CornerkickManager.Player pl in CkAppShared.ckMng.ltPlayer) {
        for (int iCh = 0; iCh < pl.ltClubHistory.Count; iCh++) {
          CornerkickManager.Player.ClubHistory ch = pl.ltClubHistory[iCh];

          if (ch.iTransferFee > 0) {
            if (ch.club == null) {
              ch.iTransferFee = 0;
              continue;
            }

            // Get name of new club
            string sClubTakeName = ch.club.sName;

            // Get name of old club
            if (iCh > 0) {
              CornerkickManager.Player.ClubHistory chLast = pl.ltClubHistory[iCh - 1];

              // If no last club --> no transfer --> continue
              if (chLast.club == null) {
                ch.iTransferFee = 0;
                continue;
              }

              string sClubGiveName = chLast.club.sName;

              ltDeClubHistory.Add(new PlayerModel.ClubHistory {
                iIx = iT++,
                iId = pl.plGame.iId,
                sPlayerName = pl.plGame.sName,
                sClubTakeName = sClubTakeName,
                sClubGiveName = sClubGiveName,
                dt = ch.dt,
                iValue = pl.getValueHistory(ch.dt) * 1000,
                iTransferFee = ch.iTransferFee
              });
            }
          }
        }
      }

      if (ltDeClubHistory.Count >= 20) ltDeClubHistory = ltDeClubHistory.OrderByDescending(o => o.iTransferFee).ToList().GetRange(0, 20);

      for (int i = 0; i < ltDeClubHistory.Count; i++) {
        ltDeClubHistory[i].iIx = i + 1;
      }

      return ltDeClubHistory;
    }

  }
}
