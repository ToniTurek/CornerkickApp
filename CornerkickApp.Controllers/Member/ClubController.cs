using CornerkickApp.Shared.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using static CornerkickApp.Shared.Models.CkAppShared;

namespace CornerkickApp.Controllers.Member
{
  public class ClubController
  {
    public static ClubModel Model(CornerkickManager.User _usr, int id)
    {
      ClubModel model = new ClubModel();

      CornerkickManager.Club? clb = null;
      if (id < 0) clb = MemberController.ckClub(_usr);
      else        clb = ckMng.ltClubs.Find(c => c.iId == id);

      model.sClubName = "Verein unbekannt";
      if (clb == null) return model;

      model.iClubId = clb.iId;
      model.sClubName = clb.sName;
      model.bNation = clb.bNation;
      model.sEmblem = getClubEmblemImg(clb, sStyle: "width: 100%");

      model.sUserName = "Computer";
      if (clb.user != null) model.sUserName = clb.user.sFirstname + " " + clb.user.sSurname;

      model.sLand = clb.iLand >= 0 ? CornerkickManager.Main.sLand[clb.iLand] : "";
      if (!clb.bNation) {
        //model.sDivision = (CupController.getClubDivision(clb) + 1).ToString() + ". Liga";
        CornerkickManager.Cup? league = LeagueController.getClubDivision(clb);
        if (league != null) {
          model.sDivision = league.sName;
          model.sPlace = league.getPlace(clb, ckMng.dtDatum).ToString();
        }
      }

      foreach (CornerkickManager.Player pl in clb.ltPlayer) {
        model.ltPlayer.Add(new ClubModel.Player() {
          iId = pl.plGame.iId,
          sName = pl.plGame.sName,
          iNo = model.bNation ? pl.plGame.iNrNat : pl.plGame.iNr,
          iPos = pl.plGame.getMainPosition(),
          sPos = CornerkickManager.PlayerTool.getStrPos(pl.plGame),
          sClub = model.bNation ? (pl.contract?.club != null ? pl.contract.club.sName : "vereinslos") : ""
        });
      }
      model.iPlayerJouth = clb.ltPlayerJouth.Count;

      float[] fTeamAve11 = CornerkickManager.Tool.getTeamAve(clb, ckMng.dtDatum, ckMng.dtSeasonEnd, ptPitch: ckMng.game.ptPitch, iPlStop: 11, iTactic: (byte)clb.iTactic, bScouting: clb.user == null ? false : clb.user.bScouting);
      model.fAveStrength = fTeamAve11[3];
      model.fAveAge = fTeamAve11[4];

      model.fAttrFc = clb.getAttractionFactor(ckMng.iSeason, ltCups: ckMng.ltCups, dtNow: ckMng.dtDatum);
      if (!model.bNation) {
        model.sStadium = clb.stadium.sName;
        int iSeats = clb.stadium.getSeats();
        if (iSeats > 0) model.sStadiumSeats = "(" + iSeats.ToString("#,#", MemberController.getCi(clb)) + ")";
        else            model.sStadiumSeats = "(0)";
      }

      //model.ltSuccess = clb.ltSuccess;
      foreach (CornerkickManager.Main.Success suc in clb.ltSuccess.OrderBy(s => s.cup.iId).ThenBy(s => s.cup.iId3)) {
        if (suc.cup == null) continue;
        if (suc.cup.iId == iCupIdTestgame) continue;

        ClubModel.Success suc2 = new ClubModel.Success();
        suc2.sCupName = suc.cup.sName;

        suc2.iWin = suc.iWin;
        suc2.iDraw = suc.iDraw;
        suc2.iDefeat = suc.iDefeat;

        //suc2.iCupWin = suc.getCupWin();

        suc2.ltCupPlace = suc.ltCupPlace;

        suc2.sRecordGames = [
          getStringRecordGame(clb, suc.cup.iId, +1, 0, iGameType2: suc.cup.iId2, iGameType3: suc.cup.iId3),
          getStringRecordGame(clb, suc.cup.iId, +1, 1, iGameType2: suc.cup.iId2, iGameType3: suc.cup.iId3),
          getStringRecordGame(clb, suc.cup.iId, -1, 0, iGameType2: suc.cup.iId2, iGameType3: suc.cup.iId3),
          getStringRecordGame(clb, suc.cup.iId, -1, 1, iGameType2: suc.cup.iId2, iGameType3: suc.cup.iId3)
        ];
        model.ltSuccess.Add(suc2);
      }

      return model;
    }

    // Returns always clubs staff, not nation
    public static CornerkickManager.Main.Staff? getClubStaff(CornerkickManager.Club? clb)
    {
      if (clb == null) return null;

      if (clb.bNation) {
        if (clb.user?.club != null) return clb.user.club.staff;
        return null;
      }

      return clb.staff;
    }

    public static string getClubEmblemImg(CornerkickManager.Club? clb, string sStyle = "", bool bTiny = false)
    {
      if (!string.IsNullOrEmpty(sStyle)) sStyle = " style=\"" + sStyle + "\"";

      return "<img src=\"" + getClubEmblemImgSrc(clb, bTiny: bTiny) + "\"" + sStyle + " title=\"" + (clb != null ? clb.sName : "") + "\" alt=\"Wappen\" />";
    }

    public static string getClubEmblemImgSrc(CornerkickManager.Club? clb, bool bTiny = false)
    {
#if _WebApp
      string sEmblem = sContentDir + "/Uploads/emblems/";
      if (clb == null) return sEmblem + "0.png\" alt=\"Wappen\" ";
#else
      string sEmblemDefault = sContentDir + "/Uploads/emblems/0.png";
      if (clb == null) return sEmblemDefault;

      string sEmblem;
#endif

      byte[]? bEmblem = getClubEmblemFile(clb, bTiny: bTiny);

      if (bTiny && bEmblem == null) {
        bEmblem = getClubEmblemFile(clb, bTiny: false);

        if (bEmblem != null) {
#if _WebApp
          string sTinyEmblemDir = sContentDir + "/Uploads/emblems/.tiny";
#else
          string sTinyEmblemDir = Path.Combine(App.getMediaDir(ckMng.sDatabaseName), "images", "emblems", ".tiny");
#endif
          if (!Directory.Exists(sTinyEmblemDir)) {
            Directory.CreateDirectory(sTinyEmblemDir);
          }

          try {
            bEmblem = Tool.resizeImage(bEmblem, 24);
            Image imageEmbTiny = Image.Load(bEmblem);
            imageEmbTiny.SaveAsPng(Path.Combine(sTinyEmblemDir, clb.iId.ToString() + ".png"));
            /*
            Image imageEmbTiny = Image.Load(bEmblem);
            imageEmbTiny.Mutate(x => x.Resize(24, 24));
            imageEmbTiny.SaveAsPng(Path.Combine(sTinyEmblemDir, clb.iId.ToString() + ".png"));
            */
          } catch (Exception e) {
            ckMng.tl.writeLog("Error creating tiny emblem for club " + clb.iId.ToString() + ": " + e.Message, bError: true);
          }
        }
      }


      if (clb.bNation) {
        sEmblem = sContentDir + "/Icons/flags/";
        if (bEmblem != null) sEmblem += CornerkickManager.Main.sLandShort[clb.iLand];
        else                 sEmblem += "0";
        bTiny = false;
      } else {
        if (bEmblem != null) {
#if _WebApp
          sEmblem += clb.iId.ToString();
#else
          // style=\"height: 100%; width: 100%; object-fit: contain\"
          sEmblem = "data:image/*;base64," + @Convert.ToBase64String(bEmblem);
#endif
        } else {
          bTiny = false;

#if _WebApp
          sEmblem += "0";
#else
          return sEmblemDefault;
#endif
        }
      }

#if _WebApp
      if (bTiny) sEmblem += "_tiny";

      sEmblem += ".png";
#endif

      return sEmblem;
    }
    public static byte[]? getClubEmblemFile(CornerkickManager.Club clb, bool bTiny = false)
    {
      return getClubEmblemFile(clb.iId, bNation: clb.bNation, bTiny: bTiny);
    }
    public static byte[]? getClubEmblemFile(int iClubId, bool bNation = false, bool bTiny = false)
    {
      string sEmblemFile = "";

      if (bNation) {
        sEmblemFile = Path.Combine(sContentDir, "Icons", "flags", CornerkickManager.Main.sLandShort[iClubId] + ".png");
      } else {
#if _WebApp
        sEmblemFile = Path.Combine(sContentDir, "Uploads", "emblems", ".tiny", iClubId.ToString() + ".png");
#else
        if (string.IsNullOrEmpty(ckMng.sDatabaseName)) return null;

        //sEmblemFile = System.IO.Path.Combine(App.getHomeDir(), "App_Data", "database", "media_" + ckMng.sDatabaseName, "images", "emblems", iClubId.ToString() + sTiny + ".png");
        //sEmblemFile = System.IO.Path.Combine(App.getHomeDir(), "Content", "Uploads", "media_" + ckMng.sDatabaseName, "images", "emblems", iClubId.ToString() + sTiny + ".png");

        if (bTiny) {
          //string sTinyEmblemDir = Path.Combine(App.getHomeDir(), "Resources", "Raw", "Images");
          string sTinyEmblemDir = Path.Combine(App.getMediaDir(ckMng.sDatabaseName), "images", "emblems", ".tiny");
          if (Directory.Exists(sTinyEmblemDir)) {
            sEmblemFile = Path.Combine(sTinyEmblemDir, iClubId.ToString() + ".png");
          }
        } else {
          sEmblemFile = Path.Combine(App.getMediaDir(ckMng.sDatabaseName), "images", "emblems", iClubId.ToString() + ".png");
        }
#endif
      }

      if (string.IsNullOrEmpty(sEmblemFile)) return null;

      try {
        return File.ReadAllBytes(sEmblemFile);
      } catch (Exception ex) {
        Console.WriteLine("Error loading club " + iClubId.ToString() + " emblem." + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
        //ckMng.tl.writeLog("Error loading club " + iClubId.ToString() + " emblem." + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, sLogFile: CornerkickManager.Main.sErrorFile);
      }

      return null;
    }

    public static string getStringRecordGame(CornerkickManager.Club clb, int iGameType, sbyte iWDD, byte iHA, int iGameType2 = -1, int iGameType3 = -1)
    {
      CornerkickGame.Game.Data gdRecord = CornerkickManager.UI.getRecordGame(clb, iGameType, iWDD, iHA, iGameType2: iGameType2, iGameType3: iGameType3);

      if (gdRecord != null) {
        string sTeamH = gdRecord.team[0].sTeam;
        string sTeamA = gdRecord.team[1].sTeam;

        CornerkickManager.Club? clbH = ckMng.ltClubs.Find(c => c.iId == gdRecord.team[0].iTeamId);
        CornerkickManager.Club? clbA = ckMng.ltClubs.Find(c => c.iId == gdRecord.team[1].iTeamId);
        if (string.IsNullOrEmpty(sTeamH) && clbH != null) sTeamH = clbH.sName;
        if (string.IsNullOrEmpty(sTeamA) && clbA != null) sTeamA = clbA.sName;

        string sTeamOpp = sTeamA;
        if (clb.iId == gdRecord.team[1].iTeamId) sTeamOpp = sTeamH;

        return gdRecord.team[0].iGoals.ToString() + ":" + gdRecord.team[1].iGoals.ToString() + " vs. " + sTeamOpp + ", " + gdRecord.dt.ToString("d", MemberController.getCi(clb));
      }

      return "-";
    }

    public static List<DataPointSD>[] getAttrFtr(int iClubId)
    {
      return getAttrFtr(ckMng.ltClubs.Find(c => c.iId == iClubId));
    }
    public static List<DataPointSD>[] getAttrFtr(CornerkickManager.Club? clb)
    {
      int[] iLtCupIds = [ iCupIdLeague, iCupIdNatCup, iCupIdInt ];
      List<DataPointSD>[] ltDataPoints = new List<DataPointSD>[ckMng.ltCups.Count];

      if (clb == null) return ltDataPoints;

      //for (int iC = 0; iC < iLtCupIds.Length; iC++) {
        //ltDataPoints[iC] = new List<DataPointSD>();

        /*
        int iDivMax = 1;
        if (iLtCupIds[iC] == iCupIdLeague) iDivMax = 2;
        */

        //foreach (CornerkickManager.Cup cup in ckMng.ltCups.FindAll(c => c.iId == iLtCupIds[iC])) {
        List<CornerkickManager.Cup> ltCupsOrdered = ckMng.ltCups.OrderBy(c => c.iId).ToList();
        for (int iC = 0; iC < ltCupsOrdered.Count; iC++) {
        /*
        for (int iD = 0; iD < iDivMax; iD++) {
          CornerkickManager.Cup? cup = null;
          if      (iLtCupIds[iC] == iCupIdLeague) cup = ckMng.tl.getCup(iLtCupIds[iC], iId2: clb.iLand, iId3: iD);
          else if (iLtCupIds[iC] == iCupIdNatCup) cup = ckMng.tl.getCup(iLtCupIds[iC], iId2: clb.iLand);
          else                                    cup = ckMng.tl.getCup(iLtCupIds[iC]);
          */
          CornerkickManager.Cup cup = ltCupsOrdered[iC];
          if (cup == null) continue;
          //if (!cup.checkClubInCup(clb)) continue;

          ltDataPoints[iC] = new List<DataPointSD>();

          CornerkickManager.Main.Success suc = CornerkickManager.Tool.getSuccess(clb, cup);

          if (suc != null) {
            for (int iS = 1; iS <= ckMng.iSeason; iS++) {
              float fAttrF = clb.getAttractionFactor(suc, ckMng.iSeason, iSeasonSelected: iS, cup: cup, dtNow: ckMng.dtDatum);

              //if (fAttrF > 0f) {
                string sCupPlace = "<table><tr><td colspan=\"2\"><b><u>" + cup.sName + "</u></b></td></tr>";
                for (int iCP = 0; iCP < suc.ltCupPlace.Count; iCP++) {
                  if (suc.ltCupPlace[iCP][1] == iS) {
                    sCupPlace += "<tr><td>Platz:</td><td>" + suc.ltCupPlace[iCP][0].ToString() + "</td></tr>";
                    sCupPlace += "<tr><td>Ber. Attraktionf.:</td><td>" + CornerkickManager.Club.getAttractionFactor(suc.ltCupPlace[iCP][0], cup.settings.fAttraction, 0).ToString("0.0") + "</td></tr>";
                    sCupPlace += "<tr><td>Faktor Saison:</td><td>" + CornerkickManager.Club.getAttractionFactorYearsAgoFactor(ckMng.iSeason - suc.ltCupPlace[iCP][1], fYearsAgoBonus: clb.fGetYearsAgoBonus()).ToString("0.000") + "</td></tr>";
                    sCupPlace += "<tr><td><b>Attraktionfaktor: </b></td><td><b>" + fAttrF.ToString("0.0") + "</b></td></tr></table>";
                    break;
                  }
                }

                ltDataPoints[iC].Add(new DataPointSD(Tool.getSeasonString(iS), fAttrF, z: sCupPlace, sColor: sCupColors[cup.iId]));
              //}
            }
          }
        }
      //}

      return ltDataPoints;
    }

    /// <summary>
    /// Scouts section
    /// </summary>
    /// <param name="clb"></param>
    /// <returns></returns>
    // Scouts
    public static List<object>? StaffGetScouts(int iClubId)
    {
      return StaffGetScouts(ckMng.ltClubs.Find(c => c.iId == iClubId));
    }
    public static List<object>? StaffGetScouts(CornerkickManager.Club? clb)
    {
      if (clb == null) return null;

      List<object> ltScouts = new List<object>();

      foreach (CornerkickManager.Main.Staff.Scout sc in clb.staff.ltScouts) {
        if (sc.iId >= 0) ltScouts.Add(getScoutObj(sc));
      }

      return ltScouts;
    }

    public static object? StaffGetScoutsFree(CornerkickManager.Club clb)
    {
      if (clb == null) return null;

      List<object> ltScouts = new List<object>();

      foreach (CornerkickManager.Main.Staff.Scout sc in CornerkickManager.Main.staff.ltScouts) {
        bool bCont = false;
        foreach (CornerkickManager.Main.Staff.Scout scClub in clb.staff.ltScouts) {
          if (sc.iId == scClub.iId) {
            bCont = true;
            break;
          }
        }
        if (bCont) continue;

        ltScouts.Add(getScoutObj(sc));
      }

      return ltScouts;
    }

    public static string getScoutImg(CornerkickManager.Main.Staff.Scout sc)
    {
      return getScoutImg(sc.iId, sc.sName);
    }
    public static string getScoutImg(int iScoutId, string sScoutName)
    {
      string sBaseDir = App.getHomeDir();
      string sPortraitFile = System.IO.Path.Combine(sBaseDir, "Content", "Images", "portraits", "scouts", iScoutId.ToString() + ".png");
      if (System.IO.File.Exists(sPortraitFile)) {
        return "<img src=\"/Content/Images/portraits/scouts/" + iScoutId.ToString() + ".png\" alt=\"Portrait\" style=\"width:100%\" title=\"" + sScoutName + "\" >";
      }

      return "";
    }
    private static object getScoutObj(CornerkickManager.Main.Staff.Scout sc)
    {
      List<DataPointDD> dataPoints = new List<DataPointDD>();
      Meta.Numerics.Statistics.Distributions.ContinuousDistribution normal = new Meta.Numerics.Statistics.Distributions.NormalDistribution(mu: 0.0, sigma: sc.getSigma());

      for (double x = -2.0; x < 2.01; x += 0.1) {
        dataPoints.Add(new DataPointDD(x, normal.ProbabilityDensity(x), z: Math.Pow(sc.getSigma(), 2.0).ToString("0.00")));
      }

      int iMinutesActivate = 0;
      if (sc.bFreelancer) {
        iMinutesActivate = -1;
      } else if (!sc.bActive) {
        iMinutesActivate = (int)(ckMng.dtDatum.Date.Add(new TimeSpan(12, 00, 00)) - ckMng.dtDatum).TotalMinutes;
        if (iMinutesActivate < 0) iMinutesActivate += 24 * 60;
      }

      return new { iId = sc.iId, sName = sc.sName, iSkill = sc.iSkill, nData = sc.nDataPerScouting, iCost = sc.getSalary(), iPayOff = sc.getPayOff(ckMng.dtDatum, ckMng.dtSeasonEnd), normal_dist = dataPoints, sImg = getScoutImg(sc), iMinutesActivate = iMinutesActivate, nPlData = sc.ltPlayerData != null ? sc.ltPlayerData.Count : 0 };
    }

    public static object StaffHireScout(CornerkickManager.Club clb, int iScId)
    {
      if (clb == null) return null;

      CornerkickManager.Main.Staff.Scout? scNew = CornerkickManager.Main.staff.ltScouts.Find(s => s.iId == iScId);
      if (scNew != null) {
        clb.staff.ltScouts.Add(scNew.Clone(bReduced: true));
        return new { ok = true };
      }

      return new { ok = false, Message = "Scout not found!" };
    }

    public static object StaffFireScout(CornerkickManager.Club clb, int iScId)
    {
      if (clb == null) return null;

      foreach (CornerkickManager.Main.Staff.Scout sc in clb.staff.ltScouts) {
        if (sc.iId == iScId) {
          clb.staff.ltScouts.Remove(sc);
          CornerkickManager.Finance.doTransaction(clb, ckMng.dtDatum, -sc.getPayOff(ckMng.dtDatum, ckMng.dtSeasonEnd), CornerkickManager.Finance.iTransferralTypePaySalaryStaff, "Abfindungen");
          return new { ok = true };
        }
      }

      return new { ok = false, Message = "Scout not found!" };
    }

    public static object StaffScoutGetPlayerList(CornerkickManager.Club clb, int iScoutId)
    {
      if (clb == null) return null;

      List<object> ltScoutedPlayer = new List<object>();

      CornerkickManager.Main.Staff? staff = getClubStaff(clb);
      if (staff == null) return null;

      CornerkickManager.Main.Staff.Scout? sc = staff.ltScouts.Find(s => s.iId == iScoutId);
      if (sc == null) return null;

      List<CornerkickManager.Main.Staff.Scout.PlayerData> ltScPlData = new List<CornerkickManager.Main.Staff.Scout.PlayerData>(sc.ltPlayerData);

      foreach (CornerkickManager.Main.Staff.Scout.PlayerData spd in ltScPlData) {
        CornerkickManager.Player? plMng = ckMng.ltPlayer.Find(p => p.plGame.iId == spd.pl.iId);
        if (plMng != null) {
          float[] fSkills = staff.getScoutedSkills(spd.pl, sc);
          float fSkillAve = CornerkickGame.Tool.getAveSkill(spd.pl, fSkills: fSkills);
          ltScoutedPlayer.Add(new { player_id = spd.pl.iId, player_name = spd.pl.sName, date = spd.ltDetails.Max(d => d.dt), player_skill = fSkillAve, club_name = plMng.contract.club != null ? plMng.contract.club.sName : "vereinslos" });
        }
      }

      return new { aaData = ltScoutedPlayer };
    }

    public static long GetSalaryTotal(CornerkickManager.User usr, bool bInclJouth = false)
    {
      if (usr.club == null) return 0;

      return GetSalaryTotal(usr.club, bInclJouth: bInclJouth);
    }
    public static long GetSalaryTotal(CornerkickManager.Club clb, bool bInclJouth = false)
    {
      if (clb == null) return 0;

      long lSalaryTotal = 0;

      foreach (CornerkickManager.Player pl in clb.ltPlayer) lSalaryTotal += pl.contract.iSalary;

      if (bInclJouth) {
        foreach (CornerkickManager.Player pl in clb.ltPlayerJouth) lSalaryTotal += pl.contract.iSalary;
      }

      return lSalaryTotal;
    }
  }
}
