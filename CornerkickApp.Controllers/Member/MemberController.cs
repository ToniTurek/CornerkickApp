using CornerkickApp.Shared.Models;
using CornerkickApp.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using System.Net;
using System.Runtime.Serialization;
using WebsocketChat.Websocket;
using static CornerkickApp.Shared.Models.CkAppShared;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  [Authorize]
  public class MemberController
  {
    public readonly CornerkickManager.User _usr;

    public MemberController(CornerkickManager.User usr)
    {
      _usr = usr;
    }

    public readonly static string[] sCultureInfo = new string[82] {
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "en-GB",
      "",
      "",
      "",
      "fr-FR",
      "",
      "",
      "de-DE",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      ""
    };

    public static sbyte[] iShowClubNat = new sbyte[iNations.Length]; // Flag if club will be used if nation is possible (-1: show nat, 0: unset, +1: show club)
    public static bool[] bHideEocInfo = []; // Flag if end of contract info will be displayed

#if _WebApp
    public static CornerkickManager.User? ckUser(string sUserId)
    {
      if (string.IsNullOrEmpty(sUserId)) return null;
      if (ckMng?.ltUser == null) return null;

      foreach (CornerkickManager.User usr in ckMng.ltUser) {
        if (usr.id.Equals(sUserId)) return usr;
      }
#else
    public static CornerkickManager.User? ckUser(string? sUserId = null)
    {
      if (ckMng.ltUser == null) return null;

      if (ckMng.ltUser.Count > 0 && iUserActive < ckMng.ltUser.Count) {
        CornerkickManager.User usr = ckMng.ltUser[iUserActive];

        return usr;
      }
#endif

      return null;
    }

    // Get cornerkick club from user
    internal CornerkickManager.Club? ckClub()
    {
      return ckClub(_usr);
    }

    public static CornerkickManager.Club? ckClub(CornerkickManager.User? usr)
    {
      if (usr == null) return null;

      // National team
      if (usr.nation != null) {
        for (byte iN = 0; iN < iShowClubNat.Length; iN++) {
          if (usr.nation.iLand == iNations[iN]) {
            if (iShowClubNat[iN] < 0) return usr.nation;
            break;
          }
        }
      }

      // Club
      return usr.club;
    }

    internal CultureInfo getCi()
    {
      return getCi(ckClub());
    }

    public static CultureInfo getCi(CornerkickManager.User? usr)
    {
      if (usr != null) return getCi(ckClub(usr));

      return CultureInfo.CurrentCulture;
    }

    public static CultureInfo getCi(CornerkickManager.Club? clb)
    {
      if (clb != null) return getCi(clb.iLand);

      return CultureInfo.CurrentCulture;
    }
    public static CultureInfo getCi(int iLand)
    {
      if (iLand >= 0 && iLand < sCultureInfo.Length) return new CultureInfo(sCultureInfo[iLand]);

      return CultureInfo.CurrentCulture;
    }

#if !_WebApp
    public static string GetUserIdStandaloneApp(Shared.MyAuthenticationStateProvider _auth)
    {
      var uidentity = _auth.GetIdentity().Result;
      if (uidentity == null) return "";

      string? sUid = uidentity.Name;
      if (string.IsNullOrEmpty(sUid)) return "";

      return GetUserIdStandaloneApp(sUid);
    }

    public static string GetUserIdStandaloneApp(string sUid)
    {
      return sUid.Replace("@", "_at_");
    }
#if false
    public static async Task<string> uploadGameAsync(string sFileCk, Shared.MyAuthenticationStateProvider _auth, AmazonS3FileTransfer as3, IAmazonS3Service AmazonS3Service)
    {
      if (string.IsNullOrEmpty(sFileCk)) return "File empty";

      try {
        // Add extension if not already
        if (!Path.GetExtension(sFileCk).Equals(".ckx")) sFileCk += ".ckx";

        var uidentity = _auth.GetIdentity().Result;
        if (uidentity == null) return "Failed get identity";

        string? sUid = uidentity.Name;
        if (string.IsNullOrEmpty(sUid)) return "User id empty";

        AmazonS3Credentials? as3credentials = await AmazonS3Service.GetAmazonS3CredentialsAsync();
        as3 = new AmazonS3FileTransfer(as3credentials.sAwsKeyId, as3credentials.sAwsSecretKey);
        await as3.uploadFileAsync(sFileCk, "app_save/" + GetUserIdStandaloneApp(sUid) + "/" + Path.GetFileName(sFileCk), "application/zip");

        return "";
      } catch (Exception e) {
        string sErrorMsg = "ERROR saving game. Message: " + e.Message + Environment.NewLine + e.StackTrace;
        ckMng.tl.writeLog(sErrorMsg);

        return sErrorMsg;
      }
    }
#endif
#endif

    public sbyte[]? SwitchClubNation()
    {
      CornerkickManager.User usr = _usr;
      if (usr == null) return null;

      if (usr.nation != null) {
        for (byte iN = 0; iN < iShowClubNat.Length; iN++) {
          if (usr.nation.iLand == iNations[iN]) {
            iShowClubNat[iN] *= -1;
            if (iShowClubNat[iN] == 0) iShowClubNat[iN] = 1;
            break;
          }
        }
      }

      //return Json(iShowClubNat);
      return iShowClubNat;
    }

#if _CONSOLE
    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Console
    /// </summary>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult ConsoleNews()
    {
      if (AccountController.ckconsole == null) return View("Console", "");

      string s = ckMng.sWochentag[(int)ckMng.dtDatum.DayOfWeek] + ", " + ckMng.dtDatum.ToShortDateString() + ", " + ckMng.dtDatum.ToShortTimeString() + " Uhr\n\n";

      if (ModelState.IsValid) {
        ModelState.Clear();
      }

      for (int iN = 0; iN < AccountController.ckUser.ltNews.Count; iN++) {
        CornerkickManager.Main.News news = AccountController.ckUser.ltNews[iN];
        if (news.bUnread) {
          s += news.sNews + '\n';
          //news.bUnread = false;
          //AccountController.ckUser.ltNews[iN] = news;
        }
      }

      ViewData["sNews"] = s;

      //return RedirectToAction("Console", "Member");
      return View("Console", "");
    }

    [Authorize]
    public ActionResult Console()
    {
      if (AccountController.ckconsole == null) return View("Console", "");

      string s = "";

      if (ModelState.IsValid) {
        ModelState.Clear();
      }
      //foreach (string s1 in AccountController.ckconsole.ltPrint) s += s1 + '\n';

      ViewData["s"] = s;

      //return RedirectToAction("Console", "Member");
      return View("Console", "");
    }

    public ActionResult ConsoleInput(ConsoleModels input)
    {
      ViewData["s"] = "";

      if (ModelState.IsValid) {
        //TODO: SubscribeUser(model.Email);
      }

      if (string.IsNullOrEmpty(input.sIn)) AccountController.ckconsole.resetMenu();

      if (passInputToCk(input.sIn)) AccountController.ckconsole.game(AccountController.ckUser);
      Console();

      return View("Console", "");
    }

    private bool passInputToCk(string sIn)
    {
      //AccountController.ckconsole.sInput = sIn;
      return true;
    }
#endif

    internal static string[] getTeamDetails(CornerkickManager.Club clb, bool bScouting = false)
    {
      CornerkickManager.Main.Staff? staff = ClubController.getClubStaff(clb);

      float[] fTeamAve = CornerkickManager.Tool.getTeamAve(clb, ckMng.dtDatum, ckMng.dtSeasonEnd, bScouting: bScouting, staff: staff);
      float[] fTeamAve11 = CornerkickManager.Tool.getTeamAve(clb, ckMng.dtDatum, ckMng.dtSeasonEnd, ptPitch: ckMng.game.ptPitch, iPlStop: 11, bScouting: bScouting, staff: staff);
      string sCFM = "K: " + fTeamAve[0].ToString("0.0%") + ", F: " + fTeamAve[1].ToString("0.0%") + ", M: " + fTeamAve[2].ToString("0.0%");
      string sStrength = "Durchschnittsstärke (Startelf): " + (fTeamAve[3] > 0f ? fTeamAve[3].ToString("0.00") : "?") + (fTeamAve11[3] > 0f ? fTeamAve11[3].ToString(" (0.00)") : " (?)");

      return [sCFM, sStrength];
    }

    public static string getNextGameInfo(CornerkickManager.Club clb)
    {
      if (clb == null) return "";

      string sDtNextGame = "";

      if (clb.nextGame != null) {
        if (clb.nextGame.dt.Equals(ckMng.dtDatum)) {
          sDtNextGame = "Jetzt";
        } else if (clb.nextGame.dt.Date.Equals(ckMng.dtDatum.Date)) {
          sDtNextGame = "Heute, " + clb.nextGame.dt.ToString("t", getCi(clb)) + " Uhr";
        } else {
          sDtNextGame = clb.nextGame.dt.ToString("d", getCi(clb)) + " (" + (clb.nextGame.dt.Date - ckMng.dtDatum.Date).TotalDays.ToString("0") + "d)";
        }

        sDtNextGame += " - ";

        if (clb.nextGame.iGameType == 99) {
          sDtNextGame += "Saisonende";
        } else {
          if (clb.nextGame.team[0].iTeamId < 0 || clb.nextGame.team[1].iTeamId < 0) sDtNextGame += "freilos";
          else                                                                      sDtNextGame += clb.nextGame.team[0].sTeam + " vs. " + clb.nextGame.team[1].sTeam;

          CornerkickManager.Cup cupNext = ckMng.tl.getCup(clb.nextGame);
          if (cupNext != null) sDtNextGame += " (" + cupNext.sName + ")";
        }
      } else {
        if (ckMng.dtSeasonEnd.Equals(ckMng.dtDatum)) {
          sDtNextGame = "Jetzt";
        } else if (ckMng.dtSeasonEnd.Date.Equals(ckMng.dtDatum.Date)) {
          sDtNextGame = "Heute, " + ckMng.dtSeasonEnd.ToString("t", getCi(clb)) + " Uhr";
        } else {
          sDtNextGame = ckMng.dtSeasonEnd.ToString("d", getCi(clb)) + " (" + (ckMng.dtSeasonEnd.Date - ckMng.dtDatum.Date).TotalDays.ToString("0") + "d)";
        }
        sDtNextGame += " - Saisonende";
      }

      return sDtNextGame;
    }

    internal PreviewGameModel.GameInfo? getGameInfo(CornerkickGame.Game.Data gameData)
    {
      return getGameInfo(_usr, gameData);
    }
    public static PreviewGameModel.GameInfo? getGameInfo(CornerkickManager.User? usr)
    {
      if (usr == null) return new PreviewGameModel.GameInfo();

      CornerkickManager.Club? clb = ckClub(usr);
      if (clb?.nextGame == null) return new PreviewGameModel.GameInfo();

      return getGameInfo(usr, clb.nextGame);
    }
    public static PreviewGameModel.GameInfo? getGameInfo(CornerkickManager.User usr, CornerkickGame.Game.Data gameData)
    {
      if (gameData == null) return null;

      PreviewGameModel.GameInfo gi = new PreviewGameModel.GameInfo();

      CultureInfo ci = getCi(usr);

      gi.fHoursUntilGame = (gameData.dt - ckMng.dtDatum).TotalHours;

      gi.sGameDate = "Anstoß: " + gameData.dt.ToString("D", ci) + ", " + gameData.dt.ToString("t", ci) + " Uhr";

      CornerkickManager.Club? clbH = ckMng.ltClubs.Find(c => c.iId == gameData.team[0].iTeamId);
      CornerkickManager.Club? clbA = ckMng.ltClubs.Find(c => c.iId == gameData.team[1].iTeamId);
      if (clbH != null) {
        gi.sClubNameH = clbH.sName;
        gi.sClubEmblemH = ClubController.getClubEmblemImg(clbH, sStyle: "width: 100%");
      }
      if (clbA != null) {
        gi.sClubNameA = clbA.sName;
        gi.sClubEmblemA = ClubController.getClubEmblemImg(clbA, sStyle: "width: 100%");
      }

      gi.sStadium = getStadiumInfo(gameData.stadium);

      CornerkickManager.Cup cupNext = ckMng.tl.getCup(gameData);
      if (cupNext != null) {
        gi.sCupName = cupNext.sName;
        gi.sCupEmblem = CupController.getCupEmblemImg(cupNext, sStyle: "height: 100%; width: 100%; object-fit: contain");
        if (usr.lti.Count > UserOptionsModel.iUserOptionsIxSound && usr.lti[UserOptionsModel.iUserOptionsIxSound] > 0) {
          gi.sCupAnthem = cupNext.iId.ToString() + "_" + cupNext.iId2.ToString() + "_" + cupNext.iId3.ToString() + ".mp3";
        }

        gi.iMatchday = cupNext.getMatchdaysTotal() > 1 ? cupNext.getMatchday(ckMng.dtDatum) : -1;

        if (clbH != null) {
          int iPlaceH = cupNext.getPlace(clbH, ckMng.dtDatum);
          if (iPlaceH > 0) gi.sClubPlaceH = iPlaceH.ToString() + ". Platz";
        }
        if (clbA != null) {
          int iPlaceA = cupNext.getPlace(clbA, ckMng.dtDatum);
          if (iPlaceA > 0) gi.sClubPlaceA = iPlaceA.ToString() + ". Platz";
        }
      }

      gi.referee = null;
      if (gameData.referee != null && gameData.referee.fQuality > 0f && gameData.referee.fStrict > 0f) {
        gi.referee = gameData.referee;
        gi.fRefereeCorrupt = gameData.getRefereeCorrupt();
      }

      return gi;
    }

    public static string getStadiumInfo(CornerkickGame.Stadium stadium)
    {
      if (stadium != null && !string.IsNullOrEmpty(stadium.sName) && stadium.getSeats() > 0) {
        return stadium.sName + " (" + stadium.getSeats().ToString("N0") + ")";
      }

      return "";
    }

    public static TeamModel.Player[][]? getGamePreviewPlayer(CornerkickManager.User usr)
    {
      CornerkickManager.Club? clbUser = ckClub(usr);
      if (clbUser?.nextGame == null) return null;
      CornerkickManager.Main.Staff? staff = null;
      if (usr.bScouting) staff = ClubController.getClubStaff(clbUser);

      TeamModel.Player[][] ltPlayerClb = new TeamModel.Player[2][];
      int iTactic = 0;

      int nPl = clbUser.nextGame.nPlStart;
      CornerkickManager.Club? clbH = ckMng.ltClubs.Find(c => c.iId == clbUser.nextGame.team[0].iTeamId);
      CornerkickManager.Club? clbA = ckMng.ltClubs.Find(c => c.iId == clbUser.nextGame.team[1].iTeamId);

      int iC = 0;
      foreach (CornerkickManager.Club? clb in new CornerkickManager.Club?[] { clbH, clbA }) {
        if (clb == null) continue;

        ltPlayerClb[iC] = new TeamModel.Player[nPl];

        int iP = 0;
        foreach (CornerkickManager.Player pl in clb.ltPlayer) {
          if (pl == null) continue;
          if (iP >= nPl) break;

          float[]? fSkills = staff != null ? staff.getScoutedSkills(pl.plGame) : null;

          TeamModel.Player pl2 = new TeamModel.Player();

          pl2.sName = pl.plGame.sName;
          pl2.iNb = pl.plGame.iNr;

          pl2.sNat = pl.iNat1 >= 0 && pl.iNat1 < CornerkickManager.Main.sLandShort.Length ? CornerkickManager.Main.sLandShort[pl.iNat1] : "Unb";
          //pl2.sPortrait = PlayerController.getPlayerPortrait(pl);
          pl2.sPortrait = PlayerController.getPlayerPortraitHtmlImg(pl, sStyle: "height: 100%; width: 100%; object-fit: contain", bSmall: true);
          pl2.iPos = CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(clb.ltTactic[iTactic].formation.positions[iP].pt, ckMng.game.ptPitch));
          pl2.sSkillAve = CornerkickGame.Tool.getAveSkill(pl.plGame, CornerkickGame.Tool.getPosRole(clb.ltTactic[iTactic].formation.positions[iP].pt, ckMng.game.ptPitch), fSkills: fSkills).ToString("0.0");
          pl2.ptPos = new TeamModel.Point(clb.ltTactic[iTactic].formation.positions[iP].pt);
          pl2.sAge = ((int)pl.plGame.getAge(ckMng.dtDatum)).ToString();

          ltPlayerClb[iC][iP] = pl2;

          iP++;
        }

        iC++;
      }

      return ltPlayerClb;
    }

    /// <summary>
    /// Retrieves the team's development data for a specified user, optionally including expected training results and
    /// filtering by training camp or week.
    /// </summary>
    /// <remarks>The first dimension of the returned array contains actual and, if requested, expected
    /// training data. The method returns an empty result if the user is not linked to a club or if a week number
    /// greater than zero is specified.</remarks>
    /// <param name="_usr">The user for whom to retrieve team development data. Cannot be null.</param>
    /// <param name="bExpected">true to include expected training data in the results; otherwise, false. The default is false.</param>
    /// <param name="iTrainingsCamp">The identifier of the training camp to consider when retrieving expected training data. The default is 0, which
    /// means no specific camp is considered.</param>
    /// <param name="iWeek">The week number for which to retrieve training data. A value of 0 retrieves data for the current week; values
    /// greater than 0 are not supported and will result in an empty result.</param>
    /// <returns>A two-dimensional array of lists containing data points for the team's development. Returns null if the user is
    /// not associated with a valid club or if the week number is greater than 0.</returns>
    public static List<DataPointTD>[][]? GetTeamDevelopmentData(CornerkickManager.User? _usr, bool bExpected = false, int iTrainingsCamp = 0, int iWeek = 0)
    {
      List<DataPointTD>[][] dataPoints = [new List<DataPointTD>[3], new List<DataPointTD>[3]];

      if (_usr == null) return dataPoints;
      if (iWeek > 0) return dataPoints;

      CornerkickManager.Club? clb = ckClub(_usr);
      if (clb == null) return null;

      CornerkickManager.Player.TrainingHistory trHistCurrent = new CornerkickManager.Player.TrainingHistory();
      trHistCurrent.dt = ckMng.dtDatum;
      trHistCurrent.fKFM = CornerkickManager.Tool.getTeamAve(clb, ckMng.dtDatum, ckMng.dtSeasonEnd, bScouting: _usr.bScouting);

      DateTime dtWeekStart = ckMng.dtDatum.Date.AddDays(-1);
      while ((int)dtWeekStart.DayOfWeek != 6) dtWeekStart = dtWeekStart.AddDays(-1);
      dtWeekStart = dtWeekStart.AddDays(iWeek * 7);

      for (byte j = 0; j < dataPoints[0].Length; j++) {
        for (int i = clb.ltTrainingHist.Count - 1; i > 0; i--) {
          CornerkickManager.Player.TrainingHistory trHist = clb.ltTrainingHist[i];

          if (trHist.dt.CompareTo(dtWeekStart)            >  0 &&
              trHist.dt.CompareTo(dtWeekStart.AddDays(9)) <= 0 &&
              trHist.dt.CompareTo(ckMng.dtDatum) <= 0 &&
              trHist.fKFM[j] > 0f) {
            //int iDate = convertDeltaDateTimeToTimestamp(trHist.dt, ckMng.dtDatum);

            string sTrainingName = "";
            if (tsTraining.Contains(trHist.dt.TimeOfDay)) {
              CornerkickManager.PlayerTool.Training tr = CornerkickManager.PlayerTool.getTraining(trHist.iType, ckMng.plt.ltTraining);
              sTrainingName = tr.sName + " (Start)";
            } else if (tsTraining.Contains(trHist.dt.AddMinutes(-90).TimeOfDay) && i > 0) {
              CornerkickManager.PlayerTool.Training trLast = CornerkickManager.PlayerTool.getTraining(clb.ltTrainingHist[i - 1].iType, ckMng.plt.ltTraining);
              sTrainingName = trLast.sName + " (Ende)";
            }

            if (dataPoints[0][j] == null) dataPoints[0][j] = new List<DataPointTD>();
            dataPoints[0][j].Add(new DataPointTD(trHist.dt, trHist.fKFM[j], z: sTrainingName));
          }
        }

        if (iWeek == 0 && trHistCurrent.fKFM[j] > 0f) {
          //int iDateCurrent = convertDeltaDateTimeToTimestamp(trHistCurrent.dt, ckMng.dtDatum);
          if (dataPoints[0][j] == null || dataPoints[0][j].Count == 0 || !trHistCurrent.dt.Equals(dataPoints[0][j][0].X)) {
            if (dataPoints[0][j] == null) dataPoints[0][j] = new List<DataPointTD>();
            dataPoints[0][j].Insert(0, new DataPointTD(trHistCurrent.dt, trHistCurrent.fKFM[j], z: "aktuell"));
          }
        }
      }

      if (bExpected) {
        // Initialize dataPoints list
        for (byte j = 0; j < dataPoints[1].Length; j++) dataPoints[1][j] = new List<DataPointTD>();

        // Add current training history data to dataPoints
        for (byte j = 0; j < dataPoints[1].Length; j++) {
          if (dataPoints[0][j] != null) dataPoints[1][j].Add(dataPoints[0][j][0]);
        }

        // Get training camp
        CornerkickManager.TrainingCamp.Booking camp = new CornerkickManager.TrainingCamp.Booking();
        foreach (CornerkickManager.TrainingCamp.Camp cp in ckMng.tcp.ltCamps) {
          if (cp.iId == iTrainingsCamp) {
            camp.camp = cp;
            camp.dtDeparture = ckMng.dtDatum.AddDays(-1);
            camp.dtReturn    = ckMng.dtDatum.AddDays(+8);
            break;
          }
        }

        // Clone list of player in club
        List<CornerkickManager.Player> ltPlayerTrExp = new List<CornerkickManager.Player>();
        foreach (CornerkickManager.Player pl in clb.ltPlayer) ltPlayerTrExp.Add(pl.Clone());

        // Sort by training date
        List<CornerkickManager.Main.TrainingPlan.Unit> ltTrUnits = clb.training.ltUnit.OrderBy(tu => tu.dt).ToList();

        // Add training if none for the next 7 days
        for (int iD = 0; iD < 7; iD++) {
          List<CornerkickManager.Main.TrainingPlan.Unit> ltTrUnitsToday = CornerkickManager.Main.TrainingPlan.getTrainingUnitsToday(ltTrUnits, clb.ltTrainingHist, ckMng.dtDatum.AddDays(iD));

          if (ltTrUnitsToday == null) {
            ltTrUnitsToday = new List<CornerkickManager.Main.TrainingPlan.Unit>();

            ltTrUnits.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = ckMng.dtDatum.Date.AddDays(iD).Add(tsTraining[0]), iType = 0 });
            ltTrUnits.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = ckMng.dtDatum.Date.AddDays(iD).Add(tsTraining[1]), iType = 0 });
            ltTrUnits.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = ckMng.dtDatum.Date.AddDays(iD).Add(tsTraining[2]), iType = 0 });
          }

          for (int iToD = 0; iToD < 3; iToD++) {
            bool bTuFound = false;
            for (int iTu = 0; iTu < ltTrUnitsToday.Count; iTu++) {
              if (ltTrUnitsToday[iTu].dt.TimeOfDay.Equals(tsTraining[iToD])) {
                bTuFound = true;
                break;
              }
            }
            if (bTuFound) continue;

            CornerkickManager.Main.TrainingPlan.Unit tuTmp = new CornerkickManager.Main.TrainingPlan.Unit() { dt = ckMng.dtDatum.Date.AddDays(iD).Add(tsTraining[iToD]), iType = 0 };
            ltTrUnits.Add(tuTmp);
            ltTrUnitsToday.Add(tuTmp);
          }
        }

        // Get next saturday
        DateTime dtNextSaturday = ckMng.dtDatum.Date;
        while ((int)dtNextSaturday.DayOfWeek != 6) dtNextSaturday = dtNextSaturday.AddDays(1);

        // Until next saturday ...
        foreach (CornerkickManager.Main.TrainingPlan.Unit tu in ltTrUnits) {
          if (tu.iType < 0) continue;
          if (tu.dt.CompareTo(ckMng.dtDatum) < 0) continue; // If in past
          if (tu.dt.CompareTo(dtNextSaturday.Add(new TimeSpan(15, 30, 00))) > 0) break; // If too far in future

          //if      ((int)dtTmp.DayOfWeek == 0 && dtTmp.Hour > 10) break;
          //else if ((int)dtTmp.DayOfWeek == 1 && dtTmp.Hour < 10) break;

          // ... do training for each player
          for (int iP = 0; iP < ltPlayerTrExp.Count; iP++) {
            CornerkickManager.Player plTmp = ltPlayerTrExp[iP];
            CornerkickManager.PlayerTool.Training tr = CornerkickManager.PlayerTool.getTraining(tu.iType, ckMng.plt.ltTraining);
            CornerkickManager.PlayerTool.doTraining(ref plTmp,
                                                    tr,
                                                    ckMng.plt.ltTraining,
                                                    clb.staff.iCondiTrainer,
                                                    clb.staff.iPhysio,
                                                    clb.buildings.bgGym.iLevel,
                                                    clb.buildings.bgSpa.iLevel,
                                                    tu.dt,
                                                    _usr,
                                                    iTrainingPerDay: 3,
                                                    ltPlayerTeam: ltPlayerTrExp,
                                                    campBooking: camp,
                                                    bJouth: false,
                                                    bNoInjuries: true,
                                                    ltTrRule: clb.training.ltRule);
          }

          // ... get training history data
          CornerkickManager.Player.TrainingHistory trHistExp = new CornerkickManager.Player.TrainingHistory();
          trHistExp.dt   = tu.dt;
          trHistExp.fKFM = CornerkickManager.Tool.getTeamAve(ltPlayerTrExp, ckMng.dtDatum, ckMng.dtSeasonEnd);
          trHistExp.iType = tu.iType;

          // ... add training history data to dataPoints
          for (int j = 0; j < dataPoints[1].Length; j++) {
            //int iDate = convertDeltaDateTimeToTimestamp(trHistExp.dt, ckMng.dtDatum);
            CornerkickManager.PlayerTool.Training tr = CornerkickManager.PlayerTool.getTraining(trHistExp.iType, ckMng.plt.ltTraining);
            dataPoints[1][j].Insert(0, new DataPointTD(trHistExp.dt, trHistExp.fKFM[j], z: tr.sName));
          }
        }
      }

      /*
      // Normalize date
      int iDateMin = 0;
      for (int i = 0; i < dataPoints.Length; i++) {
        for (int j = 0; j < dataPoints[i].Length; j++) {
          if (dataPoints[i][j] == null) continue;

          for (int k = 0; k < dataPoints[i][j].Count; k++) {
            if (dataPoints[i][j][k].x.HasValue) iDateMin = Math.Min(iDateMin, dataPoints[i][j][k].x.Value);
          }
        }
      }
      for (int i = 0; i < dataPoints.Length; i++) {
        for (int j = 0; j < dataPoints[i].Length; j++) {
          if (dataPoints[i][j] == null) continue;

          for (int k = 0; k < dataPoints[i][j].Count; k++) {
            if (dataPoints[i][j][k].x.HasValue) dataPoints[i][j][k].x -= iDateMin;
          }
        }
      }
      */

#if _USE_NEWTONSOFTJSON
      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
#else
      return dataPoints;
#endif
    }

    internal static List<SelectListItem> getSliTacticSystem(CornerkickManager.Club clb)
    {
      List<SelectListItem> sliSystem = new List<SelectListItem>();
      for (int iS = 0; iS < clb.ltTactic.Count; iS++) {
        sliSystem.Add(new SelectListItem { Text = "System " + (iS + 1).ToString(), Value = iS.ToString() });
      }
      sliSystem.Add(new SelectListItem { Text = "Frei", Value = "-1" });

      return sliSystem;
    }
    readonly static TimeSpan[] tsTraining = new TimeSpan[] { new TimeSpan(9, 30, 00), new TimeSpan(12, 00, 00), new TimeSpan(16, 30, 00) };
    static CornerkickManager.Cup? cupGlobal = null;
    static int iSeasonGlobal = 0;

    public static CornerkickManager.Cup? getCup(int iSeason, int iType, int iLand = -1, int iDivision = -1)
    {
      CornerkickManager.Cup? cup = null;

      if (iSeason <= 0) iSeason = ckMng.iSeason;

      if (iSeason < ckMng.iSeason) { // Past seasons
#if _WebApp
        if (iSeason == iSeasonGlobal && cupGlobal != null) {
          if (cupGlobal.iId == iType && (iLand < 0 || cupGlobal.iId2 == iLand) && (iDivision < 0 || cupGlobal.iId3 == iDivision)) return cupGlobal;
        }

        string sFileLoad = Path.Combine(sAppDataDir, "archive");
        List<CornerkickManager.Cup> ltCupsTmp;
        try {
          ltCupsTmp = ckMng.io.readCups(sFileLoad, iSeason);
        } catch (Exception e) {
          ckMng.tl.writeLog("Error in loading archived cup. Message: " + e.Message + Environment.NewLine + e.StackTrace, CornerkickManager.Main.sErrorFile);
          return null;
        }
#else
        List<CornerkickManager.Cup> ltCupsTmp = new List<CornerkickManager.Cup>();
        foreach (CornerkickManager.CupArchive ca in ckMng.ltCupArchive) {
          if (ca.iSeason == iSeason) {
            ltCupsTmp = ca.ltCups;
            break;
          }
        }
#endif

        if (ltCupsTmp == null) return null;

        foreach (CornerkickManager.Cup cp in ltCupsTmp) {
          if (cp.iId == iType && (iLand < 0 || cp.iId2 == iLand) && (iDivision < 0 || cp.iId3 == iDivision)) {
            cup = cp;
            cupGlobal = cp;
            iSeasonGlobal = iSeason;
            break;
          }
        }
      } else { // Current season
        cup = ckMng.tl.getCup(iType, iLand, iDivision);
      }

      return cup;
    }

    public static List<LeagueModel.GameInfo> getGameInfos(CornerkickManager.Cup cup, int iMatchday, CornerkickManager.User? usr = null, byte iGroup = 0, bool bCompact = false)
    {
      List<LeagueModel.GameInfo> ltGameInfos = new List<LeagueModel.GameInfo>();

      if (cup == null) return ltGameInfos;
      if (cup.ltMatchdays == null) return ltGameInfos;
      if (cup.ltMatchdays.Count < 1) return ltGameInfos;

      if (iMatchday < 0) iMatchday = cup.getMatchday(ckMng.dtDatum);
      //if (iMatchday >= cup.ltMatchdays.Count) iMatchday = cup.ltMatchdays.Count - 1;
      if (iMatchday >= cup.ltMatchdays.Count) return ltGameInfos;  // Return empty list if unknown matchday (e.g. not drawed yet)

      if (cup.ltMatchdays[iMatchday]?.ltGameData == null) return ltGameInfos;

      CornerkickManager.Club? clbUser = ckClub(usr);

      for (int iGd = 0; iGd < cup.ltMatchdays[iMatchday].ltGameData.Count; iGd++) {
        CornerkickGame.Game.Data gd = cup.ltMatchdays[iMatchday].ltGameData[iGd];

        string sClubNameH = "-";
        string sClubNameA = "-";
        int iIdH = gd.team[0].iTeamId;
        int iIdA = gd.team[1].iTeamId;

        // Check group
        if (cup.settings.nGroups > 1 && cup.checkCupGroupPhase(iMatchday)) {
          if (cup.ltClubs[iGroup].Find(c => c.iId == iIdH) == null && cup.ltClubs[iGroup].Find(c => c.iId == iIdA) == null) continue;
        }

        CornerkickManager.Club? clbH = ckMng.ltClubs.Find(c => c.iId == iIdH);
        CornerkickManager.Club? clbA = ckMng.ltClubs.Find(c => c.iId == iIdA);
        if (clbH != null) sClubNameH = clbH.sName;
        if (clbA != null) sClubNameA = clbA.sName;

        LeagueModel.GameInfo gi = new LeagueModel.GameInfo();
        gi.sDt = gd.dt.ToString(bCompact ? "dd.MM." : "d", getCi(usr)) + " " + gd.dt.ToString("t", getCi(usr));

        gi.sNameH = sClubNameH;
        gi.sNameA = sClubNameA;

        gi.iIdH = iIdH;
        gi.iIdA = iIdA;

        gi.sResult = "-";
        string sLiveGameTd = getLiveGameTd(gd);
        if (!string.IsNullOrEmpty(sLiveGameTd)) {
          gi.sResult = sLiveGameTd;
        } else {
          if (gd.team[0].iGoals + gd.team[1].iGoals >= 0) {
            gi.sResult = "<a style=\"text-decoration:none\" href=\"/member/viewgame/";

            string sFilenameGame = CornerkickGame.Tool.getFilenameGame(gd);
            if (File.Exists(Path.Combine(sAppDataDir, "save", "games", sFilenameGame + ".ckgx"))) {
              gi.sResult += sFilenameGame;
            } else {
              gi.sResult += getGameDataId(cup, iMatchday, iGd);
            }
            gi.sResult += "\" target=\"\">" + CornerkickManager.UI.getResultString(gd) + "</a>";
            /*
            gi.sResult = "<a href=\"/member/viewgame/" + CornerkickManager.UI.getResultString(gd) + "</a>";
            */
      }
    }

        gi.bBold = clbUser != null && (iIdH == clbUser.iId || iIdA == clbUser.iId);

        ltGameInfos.Add(gi);
      }

      return ltGameInfos;
    }

    private static string getLiveGameTd(CornerkickGame.Game.Data gd)
    {
      foreach (CornerkickManager.User usr in ckMng.ltUser) {
        if (usr?.game != null) {
          if (!usr.game.data.bFinished && usr.game.data.dt.Date.Equals(gd.dt.Date)) {
            if (usr.game.data.team[0].iTeamId == gd.team[0].iTeamId ||
                usr.game.data.team[1].iTeamId == gd.team[1].iTeamId) {
              string s = "<a href=\"/member/viewgame/" + usr.id + "\" target=\"\" style=\"white-space: nowrap; color: #ff8c00; text-decoration:none\">";
              s += CornerkickManager.UI.getResultString(usr.game.data) + " - " + CornerkickManager.UI.getMinuteString(usr.game.tsMinute, false) + " Min.";
              s += "</a>";

              return s;
            }
          }
        }
      }

      return "";
    }

    private static string getGameDataId(CornerkickManager.Cup cup, int iMd, int iGd)
    {
      return "gdid_" + cup.iId.ToString() + "_" + cup.iId2.ToString() + "_" + cup.iId3.ToString() + "_" + iMd.ToString() + "_" + iGd.ToString();
    }

    public static List<SelectListItem> getDdlSeason()
    {
      List<SelectListItem> ddlSeason = new List<SelectListItem>();

      for (int iS = 1; iS <= ckMng.iSeason; iS++) {
        ddlSeason.Add(new SelectListItem { Text = Tool.getSeasonString(iS), Value = iS.ToString(), Selected = iS == ckMng.iSeason });
      }

      return ddlSeason;
    }

    public static List<SelectListItem> getDdlLand(int iCupId, int iLandSelected = -1)
    {
      List<SelectListItem> ddlLand = new List<SelectListItem>();

#if _WebApp
      foreach (int iLand in iNations) {
        ddlLand.Add(new SelectListItem { Text = CornerkickManager.Main.sLand[iLand], Value = iLand.ToString() });
      }
#else
      if (CornerkickManager.Main.sLand != null) {
        for (int iN = 0; iN < CornerkickManager.Main.sLand.Length; iN++) {
          CornerkickManager.Cup leagueFirst = ckMng.tl.getCup(iCupId, iN, 0);
          if (leagueFirst == null) continue;

          string sLand = "Land " + iN.ToString();
          if (!string.IsNullOrEmpty(CornerkickManager.Main.sLand[iN])) sLand = CornerkickManager.Main.sLand[iN];

          ddlLand.Add(new SelectListItem {
            Text = sLand,
            Value = iN.ToString(),
            Selected = iN == iLandSelected
          });
        }
      }
#endif
      return ddlLand;
    }

    public static List<LeagueModel.ScorerItem> getScorerTable(int iGameType, int iLand = -1, int iGameType3 = -1, CornerkickManager.Club? clb = null)
    {
      List<LeagueModel.ScorerItem> ltDtScorer = new List<LeagueModel.ScorerItem>();

#if DEBUG
      System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
      sw.Start();
      double fElapsedTime = 0.0;
#endif

      List<CornerkickManager.UI.Scorer> ltScorer = ckMng.ui.getScorer2((byte)iGameType, iLand: iLand, iGameType3: iGameType3);
#if DEBUG
      sw.Stop();
      fElapsedTime = sw.ElapsedMilliseconds / 1000.0;

      sw.Restart();
#endif

      int iIx = 0;
      foreach (CornerkickManager.UI.Scorer sc in ltScorer) {
        LeagueModel.ScorerItem dts = new LeagueModel.ScorerItem();
        dts.iIx = iIx + 1;
        dts.sClubName = sc.sTeam;
        dts.iGoals = sc.iGoals;
        dts.iAssists = sc.iAssists;
        dts.iScorer = sc.iGoals + sc.iAssists;

        //CornerkickManager.Player? plSc = ckMng.ltPlayer.Find(p => p.plGame.iId == sc.iId);
        if (sc.plScorer?.plGame != null) {
          dts.iId = sc.plScorer.plGame.iId;
          dts.sPlName = sc.plScorer.plGame.sName;

          if (CornerkickManager.PlayerTool.ownPlayer(clb, sc.plScorer, iType: 1)) dts.bBold = true;

          //if (sc.plScorer.contract?.club != null) dts.sClubEmblem = ClubController.getClubEmblemImg(sc.plScorer.contract.club, "height: 24px; width: 24px; object-fit: contain", bTiny: true);
          //if (sc.plScorer.contract?.club != null) dts.sClubEmblem = Path.Combine(App.getMediaDir(ckMng.sDatabaseName), "images", "emblems", ".tiny", sc.plScorer.contract.club.iId.ToString() + ".png");
          //if (sc.plScorer.contract?.club != null) dts.iClubId = sc.plScorer.contract.club.iId;
          //if (sc.plScorer.contract?.club != null) dts.bClubEmblem = ClubController.getClubEmblemFile(sc.plScorer.contract.club.iId, true);
          if (sc.plScorer.contract?.club != null) dts.sClubEmblem = sc.sTeam;
        }

        ltDtScorer.Add(dts);

        iIx++;
      }
#if DEBUG
      sw.Stop();
      fElapsedTime = sw.ElapsedMilliseconds / 1000.0;
#endif

      return ltDtScorer;
    }

    public static List<LeagueModel.KeeperItem> getKeeperTable(int iGameType, int iLand = -1, int iDivision = -1, CornerkickManager.Club? clb = null)
    {
      List<LeagueModel.KeeperItem> ltDtKeeper = new List<LeagueModel.KeeperItem>();

      List<CornerkickManager.UI.Keeper> ltKeeper = ckMng.ui.getKeeper2((byte)iGameType, iLand: iLand, iDivision: iDivision);

      int iIx = 0;
      foreach (CornerkickManager.UI.Keeper kp in ltKeeper) {
        LeagueModel.KeeperItem dtk = new LeagueModel.KeeperItem();
        dtk.iIx = iIx + 1;
        dtk.iId = kp.iId;
        dtk.sPlName = kp.sName;
        dtk.sClubName = kp.sTeam;
        dtk.fSaves = kp.fSaves;
        dtk.iGamesNoGoal = kp.iGamesNoGoal;
        dtk.iMinNoGoal = kp.iMinNoGoal;

        CornerkickManager.Player? plKp = ckMng.ltPlayer.Find(p => p.plGame.iId == kp.iId);
        if (plKp != null) {
          if (CornerkickManager.PlayerTool.ownPlayer(clb, plKp)) dtk.bBold = true;
          //if (plKp.contract?.club != null) dtk.sClubEmblem = ClubController.getClubEmblemImg(plKp.contract.club, "height: 24px; width: 24px; object-fit: contain", bTiny: true);
          if (plKp.contract?.club != null) dtk.sClubEmblem = kp.sTeam;
        }

        ltDtKeeper.Add(dtk);

        iIx++;
      }

      return ltDtKeeper;
    }


    private static DateTime convertTimestampToDateTime(double timestamp)
    {
      var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
      return origin.AddSeconds(timestamp);
    }

    private static long convertDateTimeToTimestamp(DateTime dt)
    {
      //var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
      //return (long)(dt - origin).TotalSeconds;
      return (dt.AddHours(-2).Ticks - 621355968000000000) / 10000;
    }

    private static int convertDeltaDateTimeToTimestamp(DateTime dt, DateTime dt_ref)
    {
      return (int)(dt - dt_ref).TotalMinutes;
      //return (dt.AddHours(-2).Ticks - 621355968000000000) / 10000;
    }

    internal bool checkUserGame2()
    {
      return checkUserGame2(_usr);
    }
    public static bool checkUserGame2(CornerkickManager.User? usr)
    {
      if (usr?.game == null) return false;

      CornerkickGame.Game.Data gdUser = usr.game.data;
      return (ckMng.dtDatum.CompareTo(gdUser.dt) > 0 || gdUser.tsMinute.Ticks > 0) && !gdUser.bFinished;
    }

#if !_WebApp
    public static bool CheckNextUser(bool bSetNextUser)
    {
      if (iUserActive < ckMng.ltUser.Count - 1) {
        if (bSetNextUser) iUserActive++;

        return false;
      }

      return true; // Next calendar step
    }

    /*
     [0] - true: during live game
     [1] - true: game to start
     */
    public static bool[] CheckUserGame(CornerkickManager.User? usr)
    {
      if (checkUserGame2(usr))                     return [ true,  false ];
      if (checkUserGameToStart(usr, ckMng.ltUser)) return [ false, true  ];

      return [ false, false ];
    }

    internal bool checkUserGameToStart()
    {
      return checkUserGameToStart(_usr, ckMng.ltUser.Skip(iUserActive + 1).ToList());
    }
    internal static bool checkUserGameToStart(CornerkickManager.User? usr, List<CornerkickManager.User> ltUser)
    {
      if (usr?.club?.nextGame == null) return false;

      if (usr.club.nextGame.dt.Equals(ckMng.dtDatum) && !usr.club.nextGame.bFinished) {
        if (ltUser != null) {
          // Check if game to start is against following user
          foreach (CornerkickManager.User usr2 in ltUser) {
            if (usr.id.Equals(usr2.id)) continue;
            if (usr2?.club?.nextGame == null) continue;

            // Check for same game if not last user
            if (ltUser.IndexOf(usr) < ltUser.Count - 1 && usr.club.nextGame.dt.Equals(usr2.club.nextGame.dt)) {
              if (usr.club.nextGame.team[0].iTeamId == usr2.club.nextGame.team[0].iTeamId &&
                  usr.club.nextGame.team[1].iTeamId == usr2.club.nextGame.team[1].iTeamId) return false;
            }
          }
        }

        // Check for valid teams (opponent team)
        if (usr.club.nextGame.team[0].iTeamId < 0) return false;
        if (usr.club.nextGame.team[1].iTeamId < 0) return false;

        return true;
      }

      return false;
    }

    public static List<CalendarReturn> SetCalenderNext(bool bForce, List<CornerkickManager.Main.IgnoreNextWarning>? ltWarnIgnore = null, IProgress<int[]>? progress = null, byte iGetDetails = 0)
    {
      List<CalendarReturn> ltCalRet = new List<CalendarReturn>();

      /*if (bForce) iUserActive = 0;*/
      //if (!checkUserGameToStart(usr, ckMng.ltUser)) iUserActive = 0;

      List<CornerkickManager.Main.NextReturn> ltCkRet = ckMng.next(bForce: bForce, ltWarnIgnore: ltWarnIgnore, progress: progress);

      bool bGetDetails = iGetDetails == 0 || (iGetDetails == 1 && ckMng.dtDatum.Hour == 0 && ckMng.dtDatum.Minute == 0) || (iGetDetails == 2 && ckMng.dtDatum.Hour % 2 == 0 && ckMng.dtDatum.Minute == 0);

      foreach (CornerkickManager.User usr in ckMng.ltUser) {
        if (usr == null) continue;

        CornerkickManager.Club? clb = ckClub(usr);
        if (clb == null) continue;

        CultureInfo ci = getCi(clb);

        bool bGame = false;
        CornerkickManager.Main.NextReturn nr = CornerkickManager.Main.getNextReturn(usr, ltCkRet);
        if (nr != null) {
          //if (nr.ltReturn.Count == 1 && nr.ltReturn[0] == 3) {
          if (nr.ltReturn.Contains(CornerkickManager.Main.CalendarReturn.UserGameToStart)) {
            bGame = true;
          } else if (nr.ltReturn.Count == 1 && nr.ltReturn[0] == CornerkickManager.Main.CalendarReturn.SeasonStart) {
            ltCkRet = ckMng.next(bForce: true);
            nr = CornerkickManager.Main.getNextReturn(usr, ltCkRet);
          }
        }

        string[] sTeamDetails = ["", ""];
        if (bGetDetails) sTeamDetails = getTeamDetails(clb, bScouting: usr.bScouting);

        ltCalRet.Add(
          new CalendarReturn() {
            user = usr,
            ltRet = nr == null ? new List<CornerkickManager.Main.CalendarReturn>() : nr.ltReturn,
            sDate = ckMng.dtDatum.ToString("ddd", ci) + ", " + ckMng.dtDatum.ToString("d", ci) + ", " + ckMng.dtDatum.ToString("t", ci),
            iBalance = clb.iBalance,
            iBalanceSecret = clb.iBalanceSecret,
            sCFM = sTeamDetails[0],
            sStrength = sTeamDetails[1],
            gameInfo = bGame && clb.nextGame != null ? getGameInfo(usr, clb.nextGame) : null,
            bWithDetails = bGetDetails
          }
        );
      }

      return ltCalRet;
    }
#endif

    public int compareDates(DateTime dt)
    {
      return compareDates(ckClub(), dt);
    }
    public static int compareDates(CornerkickManager.Club? clb, DateTime dt, TimeSpan tsLength = new TimeSpan())
    {
      const double fReqGapHours = 1.0; // Required gap in hours between dates

      // Check for night
      if (dt.Add(tsLength).TimeOfDay.CompareTo(CornerkickManager.Main.tsNightStart) > 0) return -3;
      if (dt.TimeOfDay.CompareTo(CornerkickManager.Main.tsNightEnd) < 0) return -3;

      if (clb == null) return -99;

      // Check for games at that day
      List<CornerkickGame.Game.Data> ltGd = ckMng.tl.getNextGames(clb, ckMng.dtDatum, false);

      foreach (CornerkickGame.Game.Data gd in ltGd) {
        /*
        double fTotH = (dt - gd.dt).TotalHours;
        if (fTotH > 0.0 && fTotH < +4) return -2;
        if (fTotH < 0.0 && fTotH > -4) return -2;
        */
        if (gd.dt.Date.Equals(dt.Date)) return -2;
        if (Math.Abs((dt - gd.dt.AddMinutes(gd.iGameMinutes)).TotalHours) < 8) return -2;
      }

      // Check for events
      foreach (CornerkickManager.Club.Event.Item evi in clb.ltEvent) {
        if ((evi.dt - dt.Add(tsLength)).TotalHours < fReqGapHours &&
            (dt - evi.dt.Add(evi.ev.tsLength)).TotalHours < fReqGapHours) return -4;
      }

      // Check for training camp travels
      if (checkForTrainingCamp(clb, dt, tsLength: tsLength, bTravelOnly: true)) return -5;

      return dt.CompareTo(ckMng.dtDatum) < 0 ? -1 : dt.CompareTo(ckMng.dtDatum);
    }
    public static bool checkForTrainingCamp(CornerkickManager.Club? clb, DateTime dt, TimeSpan tsLength = new TimeSpan(), bool bTravelOnly = false)
    {
      const double fReqGapHours = 1.0; // Required gap in hours between dates

      if (clb == null) return false;

      // Check for training camp travels
      foreach (CornerkickManager.TrainingCamp.Booking tcb in clb.ltCamp) {
        // Check departure travel
        if ((tcb.dtDeparture - dt.Add(tsLength)).TotalHours < fReqGapHours &&
            (dt - tcb.dtDeparture.Add(tcb.camp.tsTravel)).TotalHours < fReqGapHours) return true;

        // Check return travel
        if ((tcb.dtReturn.Add(-tcb.camp.tsTravel) - dt.Add(tsLength)).TotalHours < fReqGapHours &&
            (dt - tcb.dtReturn).TotalHours < fReqGapHours) return true;

        // Check presents time
        if (!bTravelOnly) {
          if (tcb.dtDeparture.CompareTo(dt) < 0 && tcb.dtReturn.CompareTo(dt) > 0) return true;
        }
      }

      return false;
    }
    public static bool checkForPlayerMeetings(CornerkickManager.User? usr, DateTime dt)
    {
      if (usr?.ltMeetings == null) return false;

      foreach (CornerkickManager.User.Meeting mtg in usr.ltMeetings) {
        if (Math.Abs((mtg.dt - dt).TotalMinutes) < 30) return true;
      }

      return false;
    }

  }
}
