#define _AI2
//#define _PLOT_MORAL

using System.Globalization;
using static CornerkickApp.Shared.Models.LayoutModel;
using static CornerkickApp.Shared.Models.CkAppShared;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;


#if _WebApp
using System.Security.Claims;
#endif
using CornerkickApp.Shared.Models;

namespace CornerkickApp.Controllers.Member
{
  public class ViewGameController
  {
    private static List<ViewGameModel.gameData2> ltGd;

    public readonly MemberController _mc;

    public ViewGameController(CornerkickManager.User usr)
    {
      _mc = new MemberController(usr);
    }

    /// <summary>
    /// Draw game
    /// </summary>
    /// <param name="usr"></param>
    /// <param name="sGameId">Game id of live game / Game data id of finished game</param>
    /// <returns></returns>
    public static ViewGameModel Model(CornerkickManager.User? usr, string sGameId = "")
    {
      ViewGameModel view = new ViewGameModel();

      view.iStateGlobal = 0;
      ltPlayerDataGame = null;


#if _WebApp
      bViewGameLocations_executed = false;
#endif
      bGetDataStatisticObject_executed = false;

      // Get user
      if (usr == null) return view;

      // Get users club
      CornerkickManager.Club? clb = MemberController.ckClub(usr);

#if !_WebApp
      if (clb?.nextGame != null && string.IsNullOrEmpty(sGameId)) {
        //if (clb.nextGame.iGameSpeed < 1) clb.nextGame.iGameSpeed = 200;
        if (clb.nextGame.iGameSpeed < 1) clb.nextGame.iGameSpeed = 1;

        // If user game has not started yet (or is in the past) --> start/continue game
        if (clb.nextGame.dt.Equals(ckMng.dtDatum) && (usr.game == null || usr.game.data.dt.CompareTo(clb.nextGame.dt) < 0)) {
          ckMng.doGame(clb.nextGame, bAlwaysWriteToDisk: true);

          // If last (or only) user ...
          if (iUserActive + 1 == ckMng.ltUser.Count) {
            // ... add at least one minute to mng time (if game is now)
            if (ckMng.dtDatum.Equals(usr?.game?.data.dt)) {
              if (usr.game.data.bFinished) ckMng.dtDatum = ckMng.dtDatum.AddMinutes(usr.game.data.tsMinute.TotalMinutes + 15);
              else                         ckMng.dtDatum = ckMng.dtDatum.AddMinutes(1);
            }
          }
        }
      }
#endif
      /*
#if !_WebApp
      if (user.game == null) CkAppShared.ckMng.doGame(user.club?.nextGame, bAlwaysWriteToDisk: true);
#endif
      */

#if DEBUG
      view.bAdmin = true;
#else
#if _WebApp
      //view.bAdmin = AdminModel.checkUserIsAdmin(user.id);
#endif
#endif

      CornerkickGame.Game? game = usr.game;

      // Initialize own game flag
      view.bOwnLiveGame = (game != null && !game.data.bFinished) || view.bAdmin;

      if (view.bOwnLiveGame) {
        // Set system dropdown
        if (clb != null) {
          //view.sliSystem = MemberController.getSliTacticSystem(clb);
          //if (view.sliSystem.Count > 1) view.sliSystem.RemoveAt(view.sliSystem.Count - 1);
          view.iSystem = clb.iTactic;
        }
      }

      /*
      if (view.bAdmin && game == null) {
        game = ckMng.game.tl.getDefaultGame();
        game.data.iGameSpeed = 300;
      }
      */

      // Set user options
      view.bSound = true;
      if (usr.lti != null) {
        if (usr.lti.Count > UserOptionsModel.iUserOptionsIxSound)      view.bSound      = usr.lti[UserOptionsModel.iUserOptionsIxSound] > 0;
        if (usr.lti.Count > UserOptionsModel.iUserOptionsIxComment)    view.iComments   = usr.lti[UserOptionsModel.iUserOptionsIxComment];
        if (usr.lti.Count > UserOptionsModel.iUserOptionsIxShowPitch)  view.iShowPitch  = usr.lti[UserOptionsModel.iUserOptionsIxShowPitch];
        if (usr.lti.Count > UserOptionsModel.iUserOptionsIxAnimations) view.iAnimations = usr.lti[UserOptionsModel.iUserOptionsIxAnimations];
      }

      // Create game selector menu
      //view.ddlGames = new List<SelectListItem>();

      if (!string.IsNullOrEmpty(sGameId)) {
        if (sGameId.StartsWith("gdid_")) {
          string[] sDataSplit = sGameId.Split('_');
          if (sDataSplit.Length > 5) {
            int iCupId1;
            int iCupId2;
            int iCupId3;
            int iMd;
            int iGd;
            if (int.TryParse(sDataSplit[1], out iCupId1)) {
              if (int.TryParse(sDataSplit[2], out iCupId2)) {
                if (int.TryParse(sDataSplit[3], out iCupId3)) {
                  CornerkickManager.Cup? cup = ckMng.ltCups.Find(c => c.iId == iCupId1 && c.iId2 == iCupId2 && c.iId3 == iCupId3);
                  if (cup != null && int.TryParse(sDataSplit[4], out iMd)) {
                    if (iMd >= 0 && iMd < cup.ltMatchdays.Count && int.TryParse(sDataSplit[5], out iGd)) {
                      if (iGd >= 0 && iGd < cup.ltMatchdays[iMd].ltGameData.Count) {
                        CornerkickGame.Game.Data gd = cup.ltMatchdays[iMd].ltGameData[iGd];

                        game = new CornerkickGame.Game();
                        game.data = gd;
                        view.bOwnLiveGame = gd.team[0].iTeamId == usr.club.iId || gd.team[1].iTeamId == usr.club.iId; // Set own live-game flag
                      }
                    }
                  }
                }
              }
            }
          }
        } else {
          // First, look for live games in user list
          bool bUserGame = false;
          foreach (CornerkickManager.User usrGame in ckMng.ltUser) {
            if (usrGame.id.Equals(sGameId)) {
              game = usrGame.game;
              bUserGame = true;
              view.bOwnLiveGame = usrGame.id.Equals(usr.id); // Set own live-game flag
              break;
            }
          }

          // If not live game, check for stored games to load
          if (!bUserGame) game = ckMng.io.loadGame(Path.Combine(App.getHomeDir(), "App_Data", "save", "games", sGameId + ".ckgx"));
        }
      } else if (game == null || game.data.bFinished/* || view.bAdmin*/) {
        view.bOwnLiveGame = false; // Set own live-game flag

        List<FileInfo> fiGames = getFileInfoGames(usr.club);

        // Insert past games into dropdownmenu
        foreach (FileInfo ckg in fiGames) {
          DateTime dtGame;
          int iTeamIdH;
          int iTeamIdA;
          int iCupId;
          string? sFilenameInfo = getFilenameInfo(ckg, out dtGame, out iTeamIdH, out iTeamIdA, out iCupId);
          if (string.IsNullOrEmpty(sFilenameInfo)) continue;

          /*
          view.ddlGames.Insert(0,
            new SelectListItem {
              Text = sFilenameInfo,
              Value = ckg.Name
            }
          );
          */
        }

        // Admin livegame
        /*
        if (AdminModel.checkUserIsAdmin(User)) {
          view.ddlGames.Insert(0, new SelectListItem { Text = "Livespiel", Value = "" });
        }
        */

        //if (view.ddlGames.Count > 0) view.ddlGames[0].Selected = true;

        if (game == null && fiGames.Count > 0) {
          string sFilenameGame = Path.Combine(App.getHomeDir(), "App_Data", "save", "games", fiGames[fiGames.Count - 1].Name);
          try {
            game = ckMng.io.loadGame(sFilenameGame);
          } catch (Exception e) {
            ckMng.tl.writeLog("Unable to load game: '" + sFilenameGame + "'" + Environment.NewLine + e.Message + e.StackTrace, CornerkickManager.Main.sErrorFile);
          }
        }

        // Insert next games
        /*
        List<CornerkickGame.Game.Data> ltGdNextGames = CkAppShared.ckMng.tl.getNextGames(clubUser, CkAppShared.ckMng.dtDatum);
        for (byte j = 0; j < ltGdNextGames.Count; j++) {
          CornerkickGame.Game.Data gd = ltGdNextGames[j];
          view.ddlGames.Insert(0, new SelectListItem {
            Text = gd.dt.ToString("d", Controllers.MemberController.getCiStatic(User)) + " " + gd.dt.ToString("t", Controllers.MemberController.getCiStatic(User)) + " *: " + gd.team[0].sTeam + " - " + gd.team[1].sTeam,
            Value = (-j - 1).ToString()
          });
        }
        */
      }

      iniGameData(usr, view, game);

      view.ddlShoots = new List<SelectListItem>(view.ddlHeatmap);
      view.ddlDuels  = new List<SelectListItem>(view.ddlHeatmap);
      view.ddlPasses = new List<SelectListItem>(view.ddlHeatmap);

      return view;
    }

    private static ViewGameModel.gameData2? getViewGameData(string sUserId)
    {
      if (ltGd == null) return null;

      for (int iGd = 0; iGd < ltGd.Count; iGd++) {
        if (ltGd[iGd].sUserId.Equals(sUserId)) return ltGd[iGd];
      }

      return null;
    }
    private static void setViewGameDataList(ViewGameModel.gameData2 gd)
    {
      for (int iGd = 0; iGd < ltGd.Count; iGd++) {
        if (ltGd[iGd].sUserId == null) continue;

        if (ltGd[iGd].sUserId.Equals(gd.sUserId)) {
          ltGd[iGd] = gd;
          return;
        };
      }

      ltGd.Add(gd);
    }

    private static List<FileInfo> getFileInfoGames(CornerkickManager.Club clubUser)
    {
      List<FileInfo> fiGames = new List<FileInfo>();

      DirectoryInfo d = new DirectoryInfo(Path.Combine(App.getHomeDir(), "App_Data", "save", "games"));

      if (d.Exists) {
        FileInfo[] ltCkgFiles = d.GetFiles("*.ckgx");

        foreach (FileInfo ckg in ltCkgFiles) {
          string[] sFilenameData = Path.GetFileNameWithoutExtension(ckg.Name).Split('x');
          if (sFilenameData.Length < 3) continue;

          DateTime dtGame;
          int iTeamIdH;
          int iTeamIdA;
          int iCupId;
          if (string.IsNullOrEmpty(getFilenameInfo(ckg, out dtGame, out iTeamIdH, out iTeamIdA, out iCupId))) continue;

          if (iTeamIdH == clubUser.iId || iTeamIdA == clubUser.iId/* || AdminModel.checkUserIsAdmin(User)*/) fiGames.Add(ckg);
        }
      }

      return fiGames;
    }

    private string? getFilenameInfo(FileInfo fiGame, out DateTime dtGame, out int iTeamIdH, out int iTeamIdA, out int iCupId)
    {
      return getFilenameInfo(fiGame, out dtGame, out iTeamIdH, out iTeamIdA, out iCupId, _mc.getCi());
    }
    public static string? getFilenameInfo(FileInfo fiGame, out DateTime dtGame, out int iTeamIdH, out int iTeamIdA, out int iCupId, CultureInfo ciUser = null)
    {
      dtGame = new DateTime();
      iCupId = -1;
      iTeamIdH = -1;
      iTeamIdA = -1;

      string[] sFilenameData = Path.GetFileNameWithoutExtension(fiGame.Name).Split('x');
      if (sFilenameData.Length < 3) return null;

      // Date/Time
      if (!DateTime.TryParseExact(sFilenameData[0], "yyyyMMdd_HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtGame)) return null;

      // Cup ID
      string[] sFilenameDataCupId = sFilenameData[1].Split('_');
      if (!int.TryParse(sFilenameDataCupId[0], out iCupId)) return null;

      // Team names
      string[] sFilenameDataTeamIds = sFilenameData[2].Split('_');
      if (!int.TryParse(sFilenameDataTeamIds[0], out iTeamIdH)) return null;
      if (!int.TryParse(sFilenameDataTeamIds[1], out iTeamIdA)) return null;

      string sRet = dtGame.ToString("d", ciUser) + " " + dtGame.ToString("t", ciUser);

      int iTeamIdHTmp = iTeamIdH;
      int iTeamIdATmp = iTeamIdA;
      CornerkickManager.Club? clbH = ckMng.ltClubs.Find(c => c.iId == iTeamIdHTmp);
      CornerkickManager.Club? clbA = ckMng.ltClubs.Find(c => c.iId == iTeamIdATmp);
      if (clbH != null && clbA != null) sRet += ": " + clbH.sName + " - " + clbA.sName;

      return sRet;
    }

    public static bool loadGame(CornerkickManager.User usr, ViewGameModel view, string sFilename)
    {
      if (sFilename == null) return false;

      // Get user
      if (usr == null) return false;
      ViewGameModel.gameData2? gd2 = getViewGameData(usr.id);
      if (gd2 == null) gd2 = new ViewGameModel.gameData2();

      string sFilenameGame = Path.Combine(App.getHomeDir(), "App_Data", "save", "games", sFilename);

      try {
        gd2.game = ckMng.io.loadGame(sFilenameGame);

        if (view.bAdmin) usr.game = gd2.game;

        iniGameData(usr, view, gd2.game);
      } catch {
        ckMng.tl.writeLog("Unable to load game: '" + sFilenameGame + "'", CornerkickManager.Main.sErrorFile);
      }

      return true;
    }

    public static void iniGameData(CornerkickManager.User usr, ViewGameModel view, CornerkickGame.Game? game)
    {
      CornerkickManager.Club? clbUser = MemberController.ckClub(usr);
      if (clbUser == null) return;

      if (game == null) {
        view.gD = new ViewGameModel.gameData();
        view.gD.iGoalsH = -1;
        view.gD.iGoalsA = -1;

        return;
      }

      if (ltGd == null) ltGd = new List<ViewGameModel.gameData2>();
      ViewGameModel.gameData2 gd2 = new ViewGameModel.gameData2();
      ViewGameModel.gameData gD = new ViewGameModel.gameData();
      gd2.sUserId = usr.id;
      gd2.viewGd = gD;
      gd2.game = game;

      gD.iTeamId = clbUser.iId;
      gD.bOwnLiveGame = view.bOwnLiveGame;
      gD.nPlStart = game.data.nPlStart;
      gD.iGameMinutes = game.data.iGameMinutes;
      gD.ptPitch = game.ptPitch;

      gD.sJerseyColors[0] = [ "white", "blue", "blue", "blue", "white", "white" ];
      gD.sJerseyColors[1] = [ "white", "red",  "red",  "red",  "white", "white" ];

      CornerkickManager.Club? clbH = ckMng.ltClubs.Find(c => c.iId == game.data.team[0].iTeamId);
      CornerkickManager.Club? clbA = ckMng.ltClubs.Find(c => c.iId == game.data.team[1].iTeamId);

      gD.sStadium = game.data.stadium.sName;

      System.Drawing.Color[] clH1 = new System.Drawing.Color[3];
      if (clbH != null) {
        // Set team name
        gD.sTeamH = clbH.sName;

        gD.sEmblemH = ClubController.getClubEmblemImgSrc(clbH);

        clH1 = clbH.cl1;
        if (Tool.checkColorsSimilar(clH1[0], clH1[2])) clH1[2] = Tool.getColorComplementary(clH1[0]);
        System.Drawing.Color[] clH2 = clbH.cl2;
        if (Tool.checkColorsSimilar(clH2[0], clH2[2])) clH2[2] = Tool.getColorComplementary(clH2[0]);
        for (int iC = 0; iC < clbH.cl1.Length; iC++) gD.sJerseyColors[0][iC                  ] = Tool.convertToRgb(clH1[iC]);
        for (int iC = 0; iC < clbH.cl2.Length; iC++) gD.sJerseyColors[0][iC + clbH.cl1.Length] = Tool.convertToRgb(clH2[iC]);
        if (string.IsNullOrEmpty(gD.sStadium) && clbH.stadium != null) gD.sStadium = clbH.stadium.sName;
      }

      if (clbA != null) {
        // Set team name
        gD.sTeamA = clbA.sName;

        gD.sEmblemA = ClubController.getClubEmblemImgSrc(clbA);

        System.Drawing.Color[] clA1 = clbA.cl1;
        if (Tool.checkColorsSimilar(clA1[0], clA1[2])) clA1[2] = Tool.getColorComplementary(clA1[0]);
        System.Drawing.Color[] clA2 = clbA.cl2;
        if (Tool.checkColorsSimilar(clA2[0], clA2[2])) clA2[2] = Tool.getColorComplementary(clA2[0]);

        // If main color similar to main home color --> swap colors (use second color set)
        if (Tool.checkColorsSimilar(clH1[0], clA1[0])) {
          System.Drawing.Color[] cltmp = clA1;
          clA1 = clA2;
          clA2 = cltmp;
        }

        for (int iC = 0; iC < clbA.cl1.Length; iC++) gD.sJerseyColors[1][iC                  ] = Tool.convertToRgb(clA1[iC]);
        for (int iC = 0; iC < clbA.cl2.Length; iC++) gD.sJerseyColors[1][iC + clbA.cl1.Length] = Tool.convertToRgb(clA2[iC]);
      }

      /*
      if (!System.IO.File.Exists(Path.Combine(sEmblemDir, sEmblemH))) sEmblemH = "0.png";
      if (!System.IO.File.Exists(Path.Combine(sEmblemDir, sEmblemA))) sEmblemA = "0.png";
      */

      // Stadium seats
      gD.sStadium += " (" + (game.data.iSpectators[0] + game.data.iSpectators[1] + game.data.iSpectators[2]).ToString("N0", MemberController.getCi(clbUser)) + " / " + game.data.stadium.getSeats().ToString("N0", MemberController.getCi(clbUser)) + ")";

      // Heatmap
      if (game.player != null) {
        string[] sHA = [ "H", "A" ];
        // Add player to heatmap
        for (byte iHA = 0; iHA < 2; iHA++) {
          if (game.player[iHA] == null) continue;

          for (byte iPl = 0; iPl < game.data.nPlStart; iPl++) {
            if (game.player[iHA][iPl] == null) continue;
            view.ddlHeatmap.Add(new SelectListItem { Text = "(" + sHA[iHA] + ") " + game.player[iHA][iPl].sName + " - " + game.player[iHA][iPl].iNr, Value = (2 + (iHA * game.data.nPlStart) + iPl).ToString() });
          }
        }
      }

      view.iGameSpeed = game.data.iGameSpeed;

      view.game = game;

      //gD = getAllGameData(view);

      setGameData(usr, ref gD, gd2.game, gd2.sUserId);

      GetScoutedPlayer(usr, gd2.game.data.dt);

      view.gD = gD;
    }

    /*
    iState
      -4: admin
      -3: initial call
      -2: game finished
      -1: running game
      >= 0: specific state
    bExecuteGame
      false(default): game data from game running in background are fetched
      true:           game is executed at each step
    */
#if _WebApp
    private static bool bViewGameLocations_executed = false;
#endif
    private static float[][] fMoralChange  = new float[2][];
    private static float[][] fLeaderChange = new float[2][];
    public static Task<ViewGameModel.State> getState(CornerkickManager.User usr, int iState = -1/*, int iSleep = 0*/, bool bAverage = false, bool bExecuteGame = false, bool bImediately = false, bool bOnlyMainComments = false, bool bPlayerAtBallDetails = true, sbyte iPlNextAction = -1, sbyte iPlNextActionResult = -1)
    {
      ViewGameModel.State state = new ViewGameModel.State();

#if _WebApp
      if (bViewGameLocations_executed) return Task.FromCanceled<ViewGameModel.State>(CancellationToken.None);
      bViewGameLocations_executed = true;
#endif

      ViewGameModel.ltLoc = new List<float[]>();

      CornerkickManager.Club? clb = MemberController.ckClub(usr);

      ViewGameModel.gameData2? gd2 = getViewGameData(usr.id);
      if (gd2?.game == null) {
#if _WebApp
        bViewGameLocations_executed = false;
#endif
        return Task.FromCanceled<ViewGameModel.State>(CancellationToken.None);
      }

      // Store previous player moral/leader values
      float[][] fMoralPrev  = new float[2][];
      float[][] fLeaderPrev = new float[2][];
      if (gd2.game.player != null) {
        for (byte iHA = 0; iHA < 2; iHA++) {
          if (gd2.game.player[iHA] == null) continue;

          fMoralPrev [iHA] = new float[gd2.game.data.nPlStart];
          fLeaderPrev[iHA] = new float[gd2.game.data.nPlStart];

          for (int iP = 0; iP < gd2.game.data.nPlStart; iP++) {
            fMoralPrev [iHA][iP] = gd2.game.player[iHA][iP].fMoral;
            fLeaderPrev[iHA][iP] = gd2.game.player[iHA][iP].character.fLeader;
          }
        }
      }

      // Set used game speed
      state.iGameSpeedUsed = gd2.game.iGameSpeedUsed;

      int iRet = 1;
#if !_WebApp
      if (bExecuteGame && usr.game != null && !usr.game.data.bFinished) {
        bool bResultsSet = false;

        if (bImediately) usr.game.iStepsWait = 0;

        iRet = usr.game.next(iPlayerNextAction: iPlNextAction, iPlNextActionResult: iPlNextActionResult);

        /*
        if (iRet == 9) {
          bViewGameLocations_executed = false;
          return Task.FromResult(state);
        }
        */

        // Store game results if game is finished
        if (iRet == 0 || usr.game.data.bFinished) {
          bResultsSet = ckMng.setGameResult(ref usr.game.data);
        }

        // If last (or only) user ...
        if (iUserActive + 1 == ckMng.ltUser.Count) {
          // ... set mng time (if time after game is in future)
          TimeSpan tsLiveGame = usr.game.data.tsMinute;
          if (!usr.game.data.bFirstHalf) tsLiveGame = tsLiveGame.Add(new TimeSpan(0, 15, 0)); // Add half-time
          DateTime dtAfterGame = usr.game.data.dt.Add(tsLiveGame);
          if (ckMng.dtDatum.CompareTo(dtAfterGame) < 0) ckMng.dtDatum = dtAfterGame;
        }

        //if (usr.game.data.bFinished) usr.game = null;
      }
#endif

      bool bAdmin = false;
      /*
      bool bAdmin = AdminModel.checkUserIsAdmin(User);

      if (bAdmin) {
        if (iState > 0) usr.game = gd2.game;
      }
      */

      CornerkickGame.Game.State gameState = gd2.game.newState();
      if (gd2.game.data.ltState.Count > 0) gameState = gd2.game.data.ltState[gd2.game.data.ltState.Count - 1];

      if (iState >= 0 && iState < gd2.game.data.ltState.Count) gameState = gd2.game.data.ltState[iState];
      //if (fTime >= 0f) gameState = CkAppShared.ckMng.game.tl.getState(_usr.game.data, fTime);
      //CornerkickGame.Game.State gameState = CkAppShared.ckMng.game.tl.getState(_usr.game.data, fTime);

      // Set gameState indicator
      state.iState = gameState.i;

      /*
      float fFinished = 0f;
      if (fTime >= 0f) fFinished = gameState.i;
      else if (_usr.game.data.bFinished) fFinished = 1f;
      */
      state.bFinished = gd2.game.data.bFinished;

      // Set break fraction e.g. for half-time
      state.fBreak = -1f;
      if (clb != null && (gd2.game.data.team[0].iTeamId == clb.iId || gd2.game.data.team[1].iTeamId == clb.iId)) {
        state.fBreak = Math.Abs(gd2.game.iStandard) == 9 ? (gd2.game.nStepsWait - gd2.game.iStepsWait) / (float)gd2.game.nStepsWait : -1f;

        if (state.fBreak > -0.0001f) {
          float fUserBonus = usr.iSkillMotivation * 0.001f;
          state.ltSpeachOptions = new List<ViewGameModel.State.SpeachOption>() {
            new ViewGameModel.State.SpeachOption() { name = "Spieler motivieren", moral_boost = (0.01f + fUserBonus) * gd2.game.data.fGamePriority, chance_moral_boost = 0.80f, moral_drop = -0.01f * gd2.game.data.fGamePriority, chance_moral_drop = 0.1f },
            new ViewGameModel.State.SpeachOption() { name = "Spieler antreiben",  moral_boost = (0.02f + fUserBonus) * gd2.game.data.fGamePriority, chance_moral_boost = 0.67f, moral_drop = -0.01f * gd2.game.data.fGamePriority, chance_moral_drop = 0.2f },
            new ViewGameModel.State.SpeachOption() { name = "Spieler anschreien", moral_boost = (0.05f + fUserBonus) * gd2.game.data.fGamePriority, chance_moral_boost = 0.40f, moral_drop = -0.02f * gd2.game.data.fGamePriority, chance_moral_drop = 0.3f }/*,
            new ViewGameModel.State.SpeachOption() { name = "Option 4", moral_boost = 0.050f, chance_moral_boost = 0.30f, moral_drop = -0.01f, chance_moral_drop = 0.30f }*/
          };
        }
      }

      // Scout player on actions
      if (iRet == 1) ScoutPlayerOnAction(usr, clb, gameState, gd2.game.data.dt + gd2.game.tsMinute, gd2.game.data.dt, iStandard: (byte)Math.Abs(gd2.game.iStandard));

      //if (gd2.game.data.iGameSpeed > 1 || state.bFinished) {
      if (state.iGameSpeedUsed > 1 || iRet == 2 || iRet == 3 || iRet < 0 || state.bFinished) {
        // Set flag for updating statistic
        //state.bUpdateStatistic = gameState.bNewRound || iState == -3;
        state.bUpdate          = iRet < 9 || state.bFinished;
        state.bUpdateStatistic = (iRet < 9 && (gameState.bNewRound || (gameState.shoot != null && gameState.shoot.bFinished))) || iRet == 2 || iRet == 3 || state.bFinished;

        // Events
        state.evt = null;
        if (iState != -4) {
          // Shoots
          if (gameState.shoot?.plShoot != null && gameState.shoot.bFinished) {
            if (gameState.shoot.bFinished) {
              if      (gameState.shoot.result == CornerkickGame.Game.Shoot.Result.Goal)                                                                   state.evt = (ViewGameModel.State.Event)(1 + gameState.shoot.plShoot.iHA);
              else if (gameState.shoot.result == CornerkickGame.Game.Shoot.Result.Bar || gameState.shoot.result == CornerkickGame.Game.Shoot.Result.Post) state.evt = ViewGameModel.State.Event.PostBar;
              else                                                                                                                                        state.evt = ViewGameModel.State.Event.NoGoal;
            } else {
              state.evt = ViewGameModel.State.Event.ShootInProgress;
            }
          }

          // Foul
          if (gameState.duel.plDef != null && gameState.duel.iResult > 1) {
            state.evt = gameState.duel.iResult > 3 ? ViewGameModel.State.Event.RedCard : ViewGameModel.State.Event.Foul;
          }

          // Whistle
          if      (iRet == 2) state.evt = ViewGameModel.State.Event.HalfTime;
          else if (iRet == 3) state.evt = ViewGameModel.State.Event.FullTime;
          else if (iRet == 4) state.evt = ViewGameModel.State.Event.PenaltyShootout;
          else if (iRet == 6) state.evt = ViewGameModel.State.Event.Offsite;
          else if (iRet == 9) state.evt = ViewGameModel.State.Event.Wait;
        }

        /*
        if (state.evt == ViewGameModel.State.Event.HalfTime || state.evt == ViewGameModel.State.Event.FullTime) {
          Console.WriteLine();
        }
        */

        // Ball
        CornerkickGame.Game.Ball ball;
        if (iState < 0) ball = gd2.game.ball;
        else            ball = gameState.ball;

        state.gBall = new ViewGameModel.Ball();

        state.gBall.pos = new ViewGameModel.Position(ball.Pos.X, ball.Pos.Y, ball.Pos.Z);
        if (bAverage) {
          TeamModel.Point ptAve = new TeamModel.Point(CornerkickManager.UI.getAveragePos(gd2.game, -1, -1, iState));
          state.gBall.pos.x = ptAve.x;
          state.gBall.pos.x = ptAve.y;
          state.gBall.pos.z = 0f;
        }
        state.gBall.pos.x =  state.gBall.pos.x / (gd2.game.ptPitch.X * 1f);
        state.gBall.pos.y = (state.gBall.pos.y / (gd2.game.ptPitch.Y * 2f)) + 0.5f;

        // Player
        state.ltPlayer = new List<ViewGameModel.Player>();

        if (gd2.game.player != null) {
          for (byte iHA = 0; iHA < 2; iHA++) {
            if (fMoralChange [iHA] == null) fMoralChange [iHA] = new float[gd2.game.data.nPlStart];
            if (fLeaderChange[iHA] == null) fLeaderChange[iHA] = new float[gd2.game.data.nPlStart];

            for (int iP = 0; iP < gd2.game.data.nPlStart; iP++) {
              ViewGameModel.Player gPlayer = new ViewGameModel.Player();

              CornerkickGame.Player pl;
              if (iState < 0) pl = gd2.game.player[iHA][iP];
              else            pl = gameState.player[iHA][iP];

              if (pl == null) continue;
              if (string.IsNullOrEmpty(pl.sName)) continue;

              gPlayer.bHome = iHA == 0;
              gPlayer.bKeeper = CornerkickGame.Tool.checkPlayerIsKeeper(pl, gd2.game.tc[iHA].formation, gd2.game.ptPitch);
              gPlayer.sName = pl.sName;

              if (gameState.duel.plDef != null && gameState.duel.iResult > 2) {
                gPlayer.bShowCard = gameState.duel.plDef.iId == pl.iId;
                if (gPlayer.bShowCard) {
                  if      (pl.iSuspension[gd2.game.iSuspensionIx] > 0) gPlayer.iCard = 3;  // Red card
                  else if (pl.bYellowCard)                             gPlayer.iCard = 1;  // Yellow card

                  if (gPlayer.iCard == 3 && gameState.duel.iResult == 4) gPlayer.iCard = 2;  // Change to yellow/red card

                  state.bUpdateStatistic = true;
                }
              }

              gPlayer.pos = new ViewGameModel.Position(pl.ptPos, gd2.game.ptPitch);
              gPlayer.posLast = new ViewGameModel.Position(pl.ptPosLast, gd2.game.ptPitch);
              //gPlayer.ptPosLast = new Models.TeamModels.Point(pl.getLastPosition());
              if (bAverage) {
                gPlayer.pos = new ViewGameModel.Position(CornerkickManager.UI.getAveragePos(gd2.game, iHA, iP, iState), gd2.game.ptPitch);
              }

              gPlayer.posTarget = new ViewGameModel.Position(pl.ptPosTarget, gd2.game.ptPitch);
              gPlayer.iLookAt = pl.iLookAt;
              gPlayer.iNo = pl.iNr;

              // Moral/Leader change
              gPlayer.fMoralChange = 0f;
              gPlayer.fLeaderChange = 0f;
              if (iState < 0 &&
                  (state.evt == ViewGameModel.State.Event.GoalHome ||
                   state.evt == ViewGameModel.State.Event.GoalAway)) {
                if (iRet < 9) {
                  fMoralChange [iHA][iP] = pl.fMoral            - fMoralPrev [iHA][iP];
                  fLeaderChange[iHA][iP] = pl.character.fLeader - fLeaderPrev[iHA][iP];
                }

                gPlayer.fMoralChange  = fMoralChange[iHA][iP];
                gPlayer.fLeaderChange = fLeaderChange[iHA][iP];
              } else {
                fMoralChange [iHA][iP] = 0f;
                fLeaderChange[iHA][iP] = 0f;
              }

              state.ltPlayer.Add(gPlayer);
              //ViewGameModel.ltLoc.Add(new float[5] { iPosX, iPosY, pl.iLookAt, pl.iNr, fCard });

              if (bAdmin && iState >= 0) {
                gd2.game.player[iHA][iP].iHA = pl.iHA;
                gd2.game.player[iHA][iP].iIndex = pl.iIndex;
                gd2.game.player[iHA][iP].ptPos.X = pl.ptPos.X;
                gd2.game.player[iHA][iP].ptPos.Y = pl.ptPos.Y;
                gd2.game.player[iHA][iP].iLookAt = pl.iLookAt;
                gd2.game.player[iHA][iP].fSteps = 6f;
              }
            }
          }
        }

        // If admin: set finished to true to avoid recall of drawGame()
        //if (bAdmin) state.bFinished = true;

        if (bAdmin && iState >= 0) {
          gd2.game.ball = ball;
          gd2.game.tsMinute = gameState.tsMinute;

          // Find player at ball
          gd2.game.ball.plAtBall = null;
          if (gd2.game.ball.iStep == 0) {
            for (byte iHA = 0; iHA < 2; iHA++) {
              for (int iP = 0; iP < ckMng.game.data.nPlStart; iP++) {
                if (iP < gd2.game.player[iHA].Length) {
                  CornerkickGame.Player plAtBall = gd2.game.player[iHA][iP];
                  if (plAtBall == null) continue;

                  if (ball.ptPos == plAtBall.ptPos) {
                    gd2.game.ball.plAtBall = plAtBall;
                    break;
                  }
                }
              }

              if (gd2.game.ball.plAtBall != null) break;
            }
          }
        }

        CornerkickGame.Player? plActive = null;
        if (gameState.ball != null) {
          plActive = gameState.ball.plAtBall;
          if (plActive == null) plActive = gameState.ball.plAtBallLast;
        }

        // Active player portrait
        state.sPlActivePortraitImg = "";
        state.sPlActiveName = "";
        state.iPlActiveHA = -1;
        if (!bAverage && plActive != null && bPlayerAtBallDetails) {
          string[] sPlayerActiveDetails = GetPlayerActiveDetails(plActive);
          state.sPlActiveName = sPlayerActiveDetails[0];
          state.sPlActivePortraitImg = sPlayerActiveDetails[1];
          state.iPlActiveHA = plActive.iHA;
        }

        // Player chances
        if (gameState.ball?.plAtBall == null) {
          state.fPlAction = null;
          state.fPlActionRnd = -1f;
        } else {
          state.fPlAction = gameState.fPlAction;
          state.fPlActionRnd = gameState.fPlActionRandom;
        }

        // Pass targets
        state.ltPassTargets = new List<ViewGameModel.PassTarget>();
        if (plActive != null && gameState.fPlAction != null && gameState.fPlAction[1] > 0.001f) {
          // Player pass targets
          float[] fPlAction;
          double fPlActionRnd;
          System.Drawing.Point? ptPassTarget;
          bool bLowPass = false;
          sbyte iAction = gd2.game.ai.getPlayerAction(plActive, out fPlAction, out fPlActionRnd, out ptPassTarget, out bLowPass);

          // Get all pass targets
          List<CornerkickGame.AI.Receiver> ltReceiver = CornerkickGame.AI.getReceiverList(gd2.game, plActive, 0);
          if (ltReceiver != null) {
            foreach (CornerkickGame.AI.Receiver rec in ltReceiver) {
              // Check if current pass target is players choice
              bool bPlayerChoice = false;
              if (iAction == 1 && ptPassTarget.HasValue) bPlayerChoice = ptPassTarget.Value.Equals(rec.ptTarget);

              state.ltPassTargets.Add(new ViewGameModel.PassTarget(rec.ptTarget, Z: rec.fPassChance, bPlayerChoice: bPlayerChoice));
            }
          }
        }

        // Ball positions
        state.gBall.ptPos = new TeamModel.Point(ball.ptPos);
        state.gBall.ptPosLast = new TeamModel.Point(ball.ptPosLast);

        // Ball target
        if (ball.iStep > 0 && ball.nSteps > 0) {
          state.gBall.ptTarget = new ViewGameModel.Ball.PointTarget(ball: ball);
        }

        // Shoot flag
        state.iShootRes = 99;
        if (gameState.shoot?.plShoot != null) {
          state.iShootHA = (byte)(gameState.shoot.iHA + 1);

          // Reset ball target
          if (ball.nSteps > 0) {
            state.gBall.ptTarget = new ViewGameModel.Ball.PointTarget(ball: ball, pt: gameState.shoot.ptPosBallFinal);
            state.gBall.ptPosLast = new TeamModel.Point(ball.ptPos);
          }

          if (gd2.game.iStepsWait == 1 && !gameState.shoot.bFinished) {
            state.iShootRes = gameState.shoot.plShootBlocked == null ? (byte)gameState.shoot.result : (byte)7;
            state.fShootRnd = gameState.shoot.fRnd;
            if (ball.nSteps == 0) {
              if (ball.plAtBall != null && gd2.game.iStandard == 0) state.gBall.ptPosLast = new TeamModel.Point(ball.plAtBall.getLastPosition());
              else                                                  state.gBall.ptPosLast = new TeamModel.Point(ball.ptPos);
              state.gBall.ptTarget = null;
            }
          }
        }

        // Comments
        state.ltComments = new List<string[]>();
        if (gameState.ltComment != null) {
          for (int iC = 0; iC < gameState.ltComment.Count; iC++) {
            CornerkickGame.Game.Comment c = gameState.ltComment[iC];

            if (string.IsNullOrEmpty(c.sText)) continue;
            if (bOnlyMainComments && !c.bAlways) continue;

            // Compose style
            string sFontWeight = "normal";
            string sTextDeco = "none";
            if ((gameState.shoot != null && gameState.shoot.fChanceOnGoal > 0) || c.sText.Contains("Rote Karte für")) {
              sFontWeight = "bold";
            }
            if (gameState.shoot?.plShoot != null && gameState.shoot.result == CornerkickGame.Game.Shoot.Result.Goal && gameState.shoot.bFinished) {
              sFontWeight = "bold";
              //sTextDeco = "blink";
              sTextDeco = "underline";
            }

            string[] sCommentNew = [
              gameState.i.ToString(),
              CornerkickManager.UI.getMinuteString(c.tsMinute, true) + ": ",
              c.sText,
              sFontWeight,
              sTextDeco
            ];

            state.ltComments.Add(sCommentNew);
          }
        }
      }

      /*
      // Stadium block sizes
      state.ltStadiumBlockSizes = new int[gd2.game.data.stadium.blocks.Length][];
      //int iSpec = (gd2.game.data.iSpectators[0] + gd2.game.data.iSpectators[1] + gd2.game.data.iSpectators[2]);

      // Count number of blocks dep. on type
      int[] nBlocks = new int[3];
      int iB = 0;
      foreach (CornerkickGame.Stadium.Block block in gd2.game.data.stadium.blocks) {
        if ((gd2.game.data.stadium.facility == null || !gd2.game.data.stadium.facility.bTopring) && iB > 9) break;

        nBlocks[block.iType]++;
        iB++;
      }

      iB = 0;
      foreach (CornerkickGame.Stadium.Block block in gd2.game.data.stadium.blocks) {
        state.ltStadiumBlockSizes[iB] = new int[] { block.iSeats / 200, block.iType, 0, 0 };
        if (nBlocks[block.iType] > 0) {
          state.ltStadiumBlockSizes[iB][2] = Math.Min(state.ltStadiumBlockSizes[iB][0], gd2.game.data.iSpectators[block.iType] / (nBlocks[block.iType] * 200));
          state.ltStadiumBlockSizes[iB][3] = block.bRoof ? 1 : 0;
        }

        iB++;
      }
      */

#if _WebApp
      bViewGameLocations_executed = false;
#endif

      return Task.FromResult(state);
    }

    public static string[] GetPlayerActiveDetails(CornerkickGame.Player plActive)
    {
      string[] sDetails = [ "", "" ];

      sDetails[0] = plActive.sName + " - " + plActive.iNr.ToString();
      CornerkickManager.Player? plMng = ckMng.ltPlayer.Find(p => p.plGame.iId == plActive.iId);
      sDetails[1] = PlayerController.getPlayerPortraitHtmlImg(plMng, sStyle: "height: 100%; object-fit: contain");

      return sDetails;
    }

    //
    // Scout player after actions
    //
    private static void ScoutPlayerOnAction(CornerkickManager.User? usr, CornerkickManager.Club? clb, CornerkickGame.Game.State gameState, DateTime dtNow, DateTime dtGameStart, byte iStandard = 0)
    {
      if (usr == null) return;
      if (!usr.bScouting) return;
      if (usr.scout == null) return;
      if (clb == null) return;

      if (random.NextDouble() < (0.3 / usr.scout.nDataPerScouting) + 0.6) return;

      // Scout defence player in duel
      if (gameState.duel.plDef != null && CornerkickManager.PlayerTool.ownPlayer(clb, gameState.duel.plDef)) {
        AddToScoutedPlayerList(usr, gameState.duel.plDef, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxDuelDef);
      }

      // Scout offence player in duel
      if (gameState.duel.plOff != null && CornerkickManager.PlayerTool.ownPlayer(clb, gameState.duel.plOff)) {
        AddToScoutedPlayerList(usr, gameState.duel.plOff, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxDuelOff);
      }

      // Scout player shooting
      if (gameState.shoot != null && gameState.shoot.plShoot != null && gameState.shoot.bFinished) {
        if (CornerkickManager.PlayerTool.ownPlayer(clb, gameState.shoot.plShoot)) {
          // Scout shoot player skills
          if (gameState.shoot.type == CornerkickGame.Game.Shoot.Type.Header) {
            AddToScoutedPlayerList(usr, gameState.shoot.plShoot, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxHeader);
          } else if (gameState.shoot.type == CornerkickGame.Game.Shoot.Type.Freekick) {
            AddToScoutedPlayerList(usr, gameState.shoot.plShoot, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxShootPower);
            AddToScoutedPlayerList(usr, gameState.shoot.plShoot, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxFreekick);
          } else if (gameState.shoot.type == CornerkickGame.Game.Shoot.Type.Penalty) {
            AddToScoutedPlayerList(usr, gameState.shoot.plShoot, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxShootPower);
            AddToScoutedPlayerList(usr, gameState.shoot.plShoot, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxPenalty);
          } else {
            AddToScoutedPlayerList(usr, gameState.shoot.plShoot, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxShootPower);
            AddToScoutedPlayerList(usr, gameState.shoot.plShoot, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxShootAcc);
          }
        } else if (gameState.shoot.plShootBlocked != null && CornerkickManager.PlayerTool.ownPlayer(clb, gameState.shoot.plShootBlocked)) {
          // Scout player blocking shoot
          AddToScoutedPlayerList(usr, gameState.shoot.plShootBlocked, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxDuelDef);
        } else if (gameState.shoot.plKeeper != null && CornerkickManager.PlayerTool.ownPlayer(clb, gameState.shoot.plKeeper)) {
          // Scout keeper skills
          AddToScoutedPlayerList(usr, gameState.shoot.plKeeper, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxReaction);
          AddToScoutedPlayerList(usr, gameState.shoot.plKeeper, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxJump);
          AddToScoutedPlayerList(usr, gameState.shoot.plKeeper, dtNow, dtGameStart, CornerkickGame.Game.iSkillIxCatch);
        }
      }

      // Scout player passing
      if (gameState.pass.plPasser != null && CornerkickManager.PlayerTool.ownPlayer(clb, gameState.pass.plPasser)) {
        AddToScoutedPlayerList(usr, gameState.pass.plPasser, dtNow, dtGameStart, gameState.ball.bLow ? CornerkickGame.Game.iSkillIxLowPassPower : CornerkickGame.Game.iSkillIxHighPassPower);
        AddToScoutedPlayerList(usr, gameState.pass.plPasser, dtNow, dtGameStart, gameState.ball.bLow ? CornerkickGame.Game.iSkillIxLowPassAcc   : CornerkickGame.Game.iSkillIxHighPassAcc);
      }
    }

    private static List<CornerkickManager.Main.Staff.Scout.PlayerData>? ltPlayerDataGame = null;
    private static void AddToScoutedPlayerList(CornerkickManager.User usr, CornerkickGame.Player pl, DateTime dtNow, DateTime dtGameStart, byte iSkillIx)
    {
      if (ltPlayerDataGame == null) ltPlayerDataGame = new List<CornerkickManager.Main.Staff.Scout.PlayerData>();

      CornerkickManager.Main.Staff.Scout.PlayerData? pd = ltPlayerDataGame.Find(p => p.pl.iId == pl.iId);
      if (pd != null) {
        //List<CornerkickManager.Main.Staff.Scout.PlayerData.Details> ltPddExist = pd.ltDetails.FindAll(d => d.iSkillIx == iSkillIx);
        //if (ltPddExist != null && ltPddExist.Count > usr.scout.nDataPerScouting) return;

        if (pd.ltDetails.Count >= usr.scout.nDataPerScouting) return;

        List<CornerkickManager.Main.Staff.Scout.PlayerData.Details> ltPdd = scoutPlayer(usr, pl, dtNow, dtGameStart, iSkillIx);
        pd.ltDetails.AddRange(ltPdd);
      } else {
        List <CornerkickManager.Main.Staff.Scout.PlayerData.Details> ltPdd = scoutPlayer(usr, pl, dtNow, dtGameStart, iSkillIx);

        ltPlayerDataGame.Add(new CornerkickManager.Main.Staff.Scout.PlayerData() {
          pl = pl,
          ltDetails = ltPdd
        });
      }
    }

    private static List<CornerkickManager.Main.Staff.Scout.PlayerData.Details> scoutPlayer(CornerkickManager.User usr, CornerkickGame.Player pl, DateTime dtNow, DateTime dtGameStart, byte iSkillIx)
    {
      List<CornerkickManager.Main.Staff.Scout.PlayerData.Details> ltPdd = usr.scout.scoutPlayer(pl, dtNow, iSkillIx, nDataPerScouting: 1);

      foreach (CornerkickManager.Main.Staff.Scout.PlayerData.Details pdd in ltPdd) {
        pdd.dt = dtGameStart.Date.Add(pdd.dt - dtGameStart);
      }

      return ltPdd;
    }

    public static List<CornerkickManager.Main.Staff.Scout.PlayerData>? GetScoutedPlayer(CornerkickManager.User usr, DateTime dtGameStart)
    {
      if (usr == null) return null;
      if (usr.scout == null) return null;
      if (usr.scout.ltPlayerData == null) return null;

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return null;

      if (ltPlayerDataGame != null) return ltPlayerDataGame.OrderBy(pd => pd.pl.iIndex).ToList();

      ltPlayerDataGame = new List<CornerkickManager.Main.Staff.Scout.PlayerData>();
      foreach (CornerkickManager.Main.Staff.Scout.PlayerData pd in usr.scout.ltPlayerData) {
        List<CornerkickManager.Main.Staff.Scout.PlayerData.Details> ltDetails = pd.ltDetails.FindAll(d => d.dt >= dtGameStart);

        ltPlayerDataGame.Add(new CornerkickManager.Main.Staff.Scout.PlayerData() {
          pl = pd.pl,
          ltDetails = ltDetails
        });
      }

      return ltPlayerDataGame.OrderBy(pd => pd.pl.iIndex).ToList();
    }

    public static bool SetMoralSpeach(CornerkickManager.User usr, float fMoralChange)
    {
      if (usr?.game == null) return false;

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return false;

      byte iHA = 0;
      if (usr.game.data.team[1].iTeamId == clb.iId) iHA = 1;

      foreach (CornerkickGame.Player pl in usr.game.player[iHA]) pl.fMoral += fMoralChange;

      return true;
    }

    private static bool bGetDataStatisticObject_executed = false;
    public static Task<ViewGameModel.gameData>? getStatistics(CornerkickManager.User? usr, int iState = -1, int iHeatmap = -1, int iAllShoots = -1, int iAllDuels = -1, int iAllPasses = -1, bool bAddFMList = true)
    {
      if (usr == null) return null;

      if (bGetDataStatisticObject_executed) return null;
      bGetDataStatisticObject_executed = true;

      ViewGameModel.gameData2? gd2 = getViewGameData(usr.id); // view.gD;
      if (gd2 == null) return null;

      ViewGameModel.gameData gD = gd2.viewGd; // view.gD;
      sbyte iStandard = 0;

      if (gd2.game == null) {
        setGameData(usr, ref gD, gd2.game, gd2.sUserId, out iStandard, iState, iHeatmap, iAllShoots, iAllDuels, iAllPasses);
        return Task.FromResult(gD);
      }

      CornerkickGame.Game.Data gameData = gd2.game.data;

      // Clear chart arrays
      //gD.ltF = new List<DataPointTsD>[gameData.nPlStart];
      //for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) gD.ltF[iPl] = new List<DataPointTsD>();

#if _PLOT_MORAL
      gD.ltM = new List<Models.DataPointGeneral>[_usr.game.data.nPlStart];
      for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) gD.ltM[iPl] = new List<Models.DataPointGeneral>();
#endif

      /*if (gD.nStates == 0) {
        gD = getAllGameData(gameData);
      } else */
      if (iState >= 0) {
        gD = getAllGameData(usr, gameData, gD, iState: iState, bAddFMList: bAddFMList);
      } else if (iState < 0) {
        if (iState == -3) {
          gD.iLastStatePerformed = 0;
          gD.sTimelineIcons = "";
        }

        /*
        // Return null if not new round
        if (gameData.ltState.Count > 0 && iState != -3) {
          if (!gameData.ltState[gameData.ltState.Count - 1].bNewRound) return Json(null);
        }
        */

        // Add game data to struct
        for (int i = gD.iLastStatePerformed + 1; i < gameData.ltState.Count; i++) {
          try {
            addGameData(ref gD, gameData, i, bAddFMList: bAddFMList);
          } catch (Exception e) {
            Console.WriteLine(e.Message);
          }
        }
      }

      iStandard = gd2.game.iStandard;
      setGameData(usr, ref gD, gd2.game, gd2.sUserId, out iStandard, iState, iHeatmap, iAllShoots, iAllDuels, iAllPasses);

      bGetDataStatisticObject_executed = false;

      return Task.FromResult(gD);
    }

    public static object? GetPlayerChances(CornerkickManager.User usr)
    {
      ViewGameModel.gameData2? gd2 = getViewGameData(usr.id); // view.gD;
      if (gd2 == null) return null;
      if (gd2.game == null) return null;

      CornerkickGame.Game.Data gameData = gd2.game.data;
      CornerkickGame.Game.State state = gameData.ltState[gameData.ltState.Count - 1];

      return new { fPlAction = state.fPlAction, fPlActionRnd = state.fPlActionRandom };
    }

    // Adds comment, shoots/cards and chart data of current state to gD
    private static void addGameData(ref ViewGameModel.gameData gD, CornerkickGame.Game.Data gameData, int iState = -1, bool bAddFMList = true)
    {
      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      if (gameData?.ltState == null) return;
      if (gameData.ltState.Count == 0) return;

      CornerkickGame.Game.State state = gameData.ltState[gameData.ltState.Count - 1];
      if (iState >= 0 && iState < gameData.ltState.Count) state = gameData.ltState[iState];

      float fLeft = ((state.tsMinute.Hours * 60f) + state.tsMinute.Minutes + (state.tsMinute.Seconds / 60f)) / 0.9f;
      if (gameData.bFinished) fLeft = (100.0f * state.i) / gameData.ltState.Count;

      for (byte jHA = 0; jHA < 2; jHA++) {
        string sIconPos = jHA == 0 ? "top: 2px" : "bottom: 2px";

        string sTeam = gameData.team[0].sTeam;
        if (jHA > 0) sTeam = gameData.team[1].sTeam;

        string sOnClick = "";
        string sCursor = "";
        if (gameData.bFinished) {
          int iStateMinute = (state.tsMinute.Hours * 60) + state.tsMinute.Minutes + (state.tsMinute.Seconds / 60);
          sOnClick = "class=\"timelineIcon\" data-state=\"" + state.i.ToString() + "\" data-minute=\"" + iStateMinute.ToString() + "\" ";
          sCursor = "; cursor: pointer";
        }

        // Shoots
        CornerkickGame.Game.Shoot shoot = state.shoot;
        if (shoot?.plShoot != null && shoot.bFinished && shoot.iHA == jHA) {
          string sShootDesc = "<b>" + CornerkickManager.UI.getMinuteString(shoot.tsMinute, false) + " Min.:</b> " +
                              shoot.iGoalsH.ToString() + ":" + shoot.iGoalsA.ToString();
          sShootDesc += " - " + shoot.plShoot.sName;

          // If goal --> add number of goals in current season
          if (shoot.result == CornerkickGame.Game.Shoot.Result.Goal && gameData.iGameType > 0) {
            CornerkickManager.Player? plMngShoot = ckMng.ltPlayer.Find(p => p.plGame.iId == shoot.plShoot.iId);

            if (plMngShoot != null) {
              int iGoals = plMngShoot.getGoalsTotal(gameData.iGameType);

              if (iGoals > 0) {
                // Reduce nb of goals if further goals in future
                List<CornerkickGame.Game.Shoot> ltShootsTotal = CornerkickManager.UI.getShoots(gameData.ltState, jHA);
                foreach (CornerkickGame.Game.Shoot sht in ltShootsTotal) {
                  if (sht.result == CornerkickGame.Game.Shoot.Result.Goal && sht.plShoot != null && sht.plShoot.iId == shoot.plShoot.iId && sht.tsMinute.CompareTo(shoot.tsMinute) > 0) iGoals--;
                }

                sShootDesc += " [" + iGoals.ToString() + "]";
              }
            }
          }

          if (shoot.type == CornerkickGame.Game.Shoot.Type.Penalty) {
            sShootDesc += ", FE";
          } else if (shoot.plShoot.ptPos.X >= 0) {
            float fDist = CornerkickGame.Tool.getDistanceToGoal(shoot.plShoot, ckMng.game.ptPitch.X, ckMng.game.fConvertDist2Meter)[0];

            sShootDesc += ", Entf.:" + fDist.ToString("0.0").PadLeft(5) + "m";
          }

          if (shoot.plAssist != null) {
            sShootDesc += " (" + shoot.plAssist.sName;

            // Add number of assists in current season
            if (shoot.result == CornerkickGame.Game.Shoot.Result.Goal && gameData.iGameType > 0) {
              CornerkickManager.Player? plMngAssist = ckMng.ltPlayer.Find(p => p.plGame.iId == shoot.plAssist.iId);

              if (plMngAssist != null) {
                int iAssists = plMngAssist.plGame.getStatistic(gameData.iGameType).iStat[8];

                if (iAssists > 0) {
                  // Reduce nb of assists if further assists in future
                  List<CornerkickGame.Game.Shoot> ltShootsTotal = CornerkickManager.UI.getShoots(gameData.ltState, jHA);
                  foreach (CornerkickGame.Game.Shoot sht in ltShootsTotal) {
                    if (sht.result == CornerkickGame.Game.Shoot.Result.Goal && sht.plAssist != null && sht.plAssist.iId == shoot.plAssist.iId && sht.tsMinute.CompareTo(shoot.tsMinute) > 0) iAssists--;
                  }

                  sShootDesc += " [" + iAssists.ToString() + "]";
                }
              }
            }

            sShootDesc += ")";
          }

          string sImg = "yellow";
          if (shoot.result == CornerkickGame.Game.Shoot.Result.Goal) {
            sImg = "white";

            if (string.IsNullOrEmpty(gD.sStatGoals)) gD.sStatGoals = "<b><u>Tore:</u></b>";
            gD.sStatGoals += "<br/>" + sShootDesc;
          } else if (shoot.result == CornerkickGame.Game.Shoot.Result.Missed || shoot.result == CornerkickGame.Game.Shoot.Result.Blocked) {
            sImg = "cyan";
          }

          if (shoot.type == CornerkickGame.Game.Shoot.Type.Penalty) sImg += "_penalty";

          gD.sTimelineIcons += "<img " + sOnClick + "src=\"" + sContentDir + "/Icons/ball_" + sImg + ".png\" alt=\"Torschuss\" style=\"position: absolute; " + sIconPos + "; width: 12px; left: " + (fLeft - 0.5).ToString(nfi) + "%" + sCursor + "\" title=\"" + sShootDesc + "\"/>";

          // Count shoots
          gD.iShoots[jHA]++;
          if (shoot.result != CornerkickGame.Game.Shoot.Result.Missed && shoot.result != CornerkickGame.Game.Shoot.Result.Blocked) gD.iShootsOnGoal[jHA]++;

          // Sum expected goals
          gD.fGoalsX[jHA] += CornerkickGame.Rules.getChanceShootGoal(state.shoot.fRefChanceOnGoal, state.shoot.fRefChanceKeeperSave);
        } // shoot

        // Passes
        CornerkickGame.Game.Pass pass = state.pass;
        if (pass.plPasser != null && pass.plPasser.iHA == jHA) {
          if (pass.plReceiver == null) gD.iPassesBad[jHA]++;
          else if (pass.plReceiver.iHA != pass.plPasser.iHA) gD.iPassesBad[jHA]++;
          else if (pass.plReceiver != pass.plPasser) gD.iPassesGood[jHA]++;
        }

        // Duels
        CornerkickGame.Game.Duel duel = state.duel;
        if (duel.plDef != null && duel.plDef.iHA == jHA) {
          if (duel.iResult > 2) {
            string sCardDesc = "<b>" + CornerkickManager.UI.getMinuteString(duel.tsMinute, false) + " Min.:</b> " +
                                sTeam +
                                " - " +
                                duel.plDef.sName;

            string sImg = "y";
            byte iStatCard = 22;
            if (duel.iResult == 4) {
              sImg = "yr";
              iStatCard = 23;
            } else if (duel.iResult == 5) {
              sImg = "r";
              iStatCard = 24;
            }

            // Add number of cards
            CornerkickManager.Player? plMngDuelDef = ckMng.ltPlayer.Find(p => p.plGame.iId == duel.plDef.iId);
            if (plMngDuelDef != null) {
              int nCards = plMngDuelDef.plGame.getStatistic(gameData.iGameType).iStat[iStatCard];
              if (nCards > 0) sCardDesc += " [" + nCards.ToString() + "]";
            }

            gD.sTimelineIcons += "<img " + sOnClick + "src=\"" + sContentDir + "/Icons/" + sImg + "Card.png\" alt=\"Karte\" style=\"position: absolute; " + sIconPos + "; width: 12px; left: " + (fLeft - 0.5).ToString(nfi) + "%" + sCursor + "\" title=\"" + sCardDesc + "\"/>";

            if (string.IsNullOrEmpty(gD.sStatCards)) gD.sStatCards = "<b><u>Karten:</u></b>";
            gD.sStatCards += "<br/><img " + sOnClick + "style=\"position: relative" + sCursor + "\" src =\"" + sContentDir + "/Icons/" + sImg + "Card.png\"/>" + sCardDesc;
          }

          // Count duels (fouls)
          gD.iDuels[jHA]++;
          if (duel.iResult > 1) gD.iFouls[jHA]++;
        }
      } // iHA

      // Chart
      if (gD.bOwnLiveGame && gameData.iGameSpeed > 1 && !gameData.bFinished) {
        byte iHA = 2;
        if      (gD.iTeamId == gameData.team[0].iTeamId) iHA = 0;
        else if (gD.iTeamId == gameData.team[1].iTeamId) iHA = 1;
        if (bAddFMList && iHA < 2) {
          if (gD.ltF == null || iState == -1) {
            gD.ltF = new List<DataPointTD>[gameData.nPlStart];
            for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) gD.ltF[iPl] = new List<DataPointTD>();
          }

#if _PLOT_MORAL
          if (gD.ltM == null || iState == -1) {
            gD.ltM = new List<Models.DataPointGeneral>[_usr.game.data.nPlStart];
            for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) gD.ltM[iPl] = new List<Models.DataPointGeneral>();
          }
#endif

          for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) {
            if (state.player?[iHA]?[iPl] == null) continue;

            gD.ltF[iPl].Add(new DataPointTD(new DateTime() + state.tsMinute, state.player[iHA][iPl].fFresh, z: state.player[iHA][iPl].sName));
          }
#if _PLOT_MORAL
          for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) {
            gD.ltM[iPl].Add(new Models.DataPointGeneral(state.i, state.player[jHA][iPl].fMoral));
          }
#endif
        }
      }

      // Set counter of performed state
      gD.iLastStatePerformed = iState;
    }

    private static void setGameData(CornerkickManager.User usr, ref ViewGameModel.gameData gd, CornerkickGame.Game game, string sUserId, int iState = -1, int iHeatmap = -1, int iAllShoots = -1, int iAllDuels = -1, int iAllPasses = -1)
    {
      sbyte iStandard;
      setGameData(usr, ref gd, game, sUserId, out iStandard, iState: iState, iHeatmap: iHeatmap, iAllShoots: iAllShoots, iAllDuels: iAllDuels, iAllPasses: iAllPasses);
    }
    private static void setGameData(CornerkickManager.User usr, ref ViewGameModel.gameData gd, CornerkickGame.Game game, string sUserId, out sbyte iStandard, int iState = -1, int iHeatmap = -1, int iAllShoots = -1, int iAllDuels = -1, int iAllPasses = -1)
    {
      iStandard = 0;

      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      if (game?.data == null) {
        gd.iGoalsH = -1;
        gd.iGoalsA = -1;

        return;
      }

      if (game.data.ltState?.Count > 0) {
        CornerkickGame.Game.State state = game.data.ltState[game.data.ltState.Count - 1];
        if (iState >= 0 && iState < game.data.ltState.Count) state = game.data.ltState[iState];

        gd.nStates = game.data.ltState.Count;

        //gd.tsMinute = state.tsMinute;
        gd.fMinute = (state.tsMinute.Hours * 60) + state.tsMinute.Minutes + (state.tsMinute.Seconds / 60);

        iStandard = state.iStandard;

        gd.ltDrawLineShoot = new List<ViewGameModel.drawLine>();
        gd.ltDrawLinePass = new List<ViewGameModel.drawLine>();
        gd.sCard = "";
        gd.sStatSubs = "";

        // Draw shoot on pitch
        if ((iState >= 0 && iState < game.data.ltState.Count) || iAllShoots >= 0) {
          if (iAllShoots >= 0) {
            byte iHA = 0;
            int iPlIx = -1;
            getStatisticHAPlayerIx(iAllShoots, game.data.nPlStart, out iHA, out iPlIx);

            for (int iSt = 0; iSt < game.data.ltState.Count; iSt++) {
              CornerkickGame.Game.State stateTmp = game.data.ltState[iSt];
              gd.ltDrawLineShoot.AddRange(getShootLine(stateTmp, game, iHA, iPlIx));

              if (iState >= 0 && iSt >= iState) break; // If review --> stop at selected state
            }
          } else {
            gd.ltDrawLineShoot = getShootLine(state, game);
          }
        }

        // Draw pass on pitch
        if ((iState >= 0 && iState < game.data.ltState.Count) || iAllPasses >= 0) {
          if (iAllPasses >= 0) {
            byte iHA = 0;
            int iPlIx = -1;
            getStatisticHAPlayerIx(iAllPasses, game.data.nPlStart, out iHA, out iPlIx);

            for (int iSt = 0; iSt < game.data.ltState.Count; iSt++) {
              CornerkickGame.Game.State stateTmp = game.data.ltState[iSt];
              gd.ltDrawLinePass.Add(getPassLine(stateTmp, game, iHA, iPlIx));

              if (iState >= 0 && iSt >= iState) break; // If review --> stop at selected state
            }
          } else {
            gd.ltDrawLinePass.Clear();
            gd.ltDrawLinePass.Add(getPassLine(state, game));
          }
        }

        // Draw duel on pitch
        if ((iState >= 0 && iState < game.data.ltState.Count) || iAllDuels >= 0) {
          // Show duel on pitch
          if (iAllDuels >= 0) {
            byte iHA = 0;
            int iPlIx = -1;
            getStatisticHAPlayerIx(iAllDuels, game.data.nPlStart, out iHA, out iPlIx);

            for (int iSt = 0; iSt < game.data.ltState.Count; iSt++) {
              CornerkickGame.Game.State stateTmp = game.data.ltState[iSt];
              gd.sCard += getDuelIcon(stateTmp, game, iHA, iPlIx);

              if (iState >= 0 && iSt >= iState) break; // If review --> stop at selected state
            }
          } else {
            gd.sCard = getDuelIcon(state, game);
          }
        }

        for (byte iHA = 0; iHA < 2; iHA++) {
          string sTeam = game.data.team[0].sTeam;
          if (iHA > 0) sTeam = game.data.team[1].sTeam;

          //int iStTmp = 0;
          // loop over states
          //foreach (CornerkickGame.Game.State state in gameData.ltState) {

          if (iHA == 0) {
            gd.iGoalsH = state.iGoalsH;
            gd.iPossession[0] = state.iPossessionH;
            gd.iCornerkick[0] = state.iCornerkickH;
            gd.iOffsite[0] = state.iOffsiteH;
          } else {
            gd.iGoalsA = state.iGoalsA;
            gd.iPossession[1] = state.iPossessionA;
            gd.iCornerkick[1] = state.iCornerkickA;
            gd.iOffsite[1] = state.iOffsiteA;
          }
          if (gd.iPassesGood[iHA] + gd.iPassesBad[iHA] > 0) gd.fPassGood[iHA] = (100 * gd.iPassesGood[iHA]) / (float)(gd.iPassesGood[iHA] + gd.iPassesBad[iHA]);
        }
      } else { // if ltState.Count == 0
        gd.iGoalsH = game.data.team[0].iGoals;
        gd.iGoalsA = game.data.team[1].iGoals;
      }

      for (byte iHA = 0; iHA < 2; iHA++) {
        string sTeam = game.data.team[0].sTeam;
        if (iHA > 0) sTeam = game.data.team[1].sTeam;

        // Substitutions
        if (game.data.team[iHA].ltSubstitutions != null) {
          for (int iS = 0; iS < game.data.team[iHA].ltSubstitutions.Count; iS++) {
            int[] iSub = game.data.team[iHA].ltSubstitutions[iS];
            float fMin = iSub[2];
            float fFullTime = 0.9f;
            if (game.data.bOvertime || game.data.bShootout) fFullTime = 1.2f;

            CornerkickGame.Player? plIn  = ckMng.ltPlayer.Find(p => p.plGame.iId == iSub[1])?.plGame;
            CornerkickGame.Player? plOut = ckMng.ltPlayer.Find(p => p.plGame.iId == iSub[0])?.plGame;

            if (plIn != null && plOut != null) {
              string sSubDesc = "<b>" + (iSub[2] + 1).ToString() + ". Min.:</b> " + sTeam + " - " + plIn.sName + " für " + plOut.sName;

              if (string.IsNullOrEmpty(gd.sStatSubs)) gd.sStatSubs = "<b><u>Spielerwechsel:</u></b>";
              gd.sStatSubs += "<br/>" + sSubDesc;

              string sIconPos = iHA == 0 ? "top: 2px" : "bottom: 2px";
              gd.sTimelineIcons += "<img src=\"" + sContentDir + "/Icons/sub.png\" alt=\"Spielerwechsel\" style=\"position: absolute; " + sIconPos + "; width: 12px; left: " + ((fMin - 0.5) / fFullTime).ToString(nfi) + "%\" title=\"" + sSubDesc + "\"/>";
            }
          }
        }
      } // iHA

      // Referee quality
      if (game.data.referee != null) {
        gd.sRefereeQuality = game.data.referee.fQuality.ToString("0.0%") + " / " + game.data.referee.fStrict.ToString("0.0%");
        gd.sRefereeDecisions = "-";
        if (game.data.referee.iDecisions[0] > 0) gd.sRefereeDecisions = (game.data.referee.iDecisions[1] / (float)game.data.referee.iDecisions[0]).ToString("0.0%");
      }

      // Bar statistics
      /*
      float fPossessionH = 50f;
      if (gd.iPossession[0] + gd.iPossession[1] > 0) fPossessionH = 100f * gd.iPossession[0] / (float)(gd.iPossession[0] + gd.iPossession[1]);

      if (AdminModel.checkUserIsAdmin(User)) {
        if (game.ball.plAtBall != null) {
#if _AI2
          float fShootOnGoal = game.ai.getChanceShootOnGoal(game.ball.plAtBall, 0);
#else
          float fShootOnGoal = game.ai.getChanceShootOnGoal    (game.ball.plAtBall);
#endif
          float fKeeperSave = game.ai.getChanceShootKeeperSave(game.ball.plAtBall, 0);

          gd.sAdminChanceShootOnGoal = "<u>Change Schuss aufs Tor:</u> " + fShootOnGoal.ToString("0.0%") + "<br/>";
          gd.sAdminChanceGoal = "<u>Change Tor:</u> " + (fShootOnGoal * (1f - fKeeperSave)).ToString("0.0%");
        }
      }
      */

      // Heatmap
      gd.sDivHeatmap = "";
      /*
      if (iHeatmap >= 0) {
        byte iHA = 0;
        int iPlIx = -1;
        getStatisticHAPlayerIx(iHeatmap, game.data.nPlStart, out iHA, out iPlIx);
        gd.sDivHeatmap = getDivHeatmap(usr, iHA, iState, iPlIx);
      }
      */

      ViewGameModel.gameData2 gd2 = new ViewGameModel.gameData2() {
        sUserId = sUserId,
        viewGd = gd,
        game = game
      };
      setViewGameDataList(gd2);
    }

    public static void getStatisticHAPlayerIx(int i, byte nPlStart, out byte iHA, out int iPl)
    {
      iHA = 0;
      iPl = -1;
      if (i < 2) {
        iHA = (byte)i;
      } else {
        iHA = (byte)((i - 2) / nPlStart);
        iPl = (i - 2) - (iHA * nPlStart);
      }
    }

    public static List<ViewGameModel.drawLine> getShootLine(CornerkickGame.Game.State state, CornerkickGame.Game game, int iHA = -1, int iPlIx = -1)
    {
      List<ViewGameModel.drawLine> ltDrawLine = new List<ViewGameModel.drawLine>();

      CornerkickGame.Player plShoot = state.shoot.plShoot;

      if (plShoot == null) return ltDrawLine;
      if (iHA >= 0 && plShoot.iHA != iHA) return ltDrawLine;
      if (iPlIx >= 0 && plShoot.iIndex != iPlIx) return ltDrawLine;

      string sTitle = "";
      if (state.shoot.fChanceOnGoal > 0f && state.shoot.bFinished) {
        sTitle += "<div align=\"right\">";

        if (state.shoot.result == CornerkickGame.Game.Shoot.Result.SaveCatch || state.shoot.result == CornerkickGame.Game.Shoot.Result.SaveBounce || state.shoot.result == CornerkickGame.Game.Shoot.Result.SaveCornerkick) sTitle += "<strong>";
        sTitle += "Gehalten: " + 0.ToString("0.0%") + " .. " + state.shoot.fChanceKeeperSave.ToString("0.0%");
        if (state.shoot.result == CornerkickGame.Game.Shoot.Result.SaveCatch || state.shoot.result == CornerkickGame.Game.Shoot.Result.SaveBounce || state.shoot.result == CornerkickGame.Game.Shoot.Result.SaveCornerkick) sTitle += "</strong>";
        sTitle += "<br/>";

        if (state.shoot.result == CornerkickGame.Game.Shoot.Result.Goal) sTitle += "<strong>";
        sTitle += "Tor: " + state.shoot.fChanceKeeperSave.ToString("0.0%") + " .. " + state.shoot.fChanceOnGoal.ToString("0.0%");
        if (state.shoot.result == CornerkickGame.Game.Shoot.Result.Goal) sTitle += "</strong>";
        sTitle += "<br/>";

        if (state.shoot.result == CornerkickGame.Game.Shoot.Result.Bar || state.shoot.result == CornerkickGame.Game.Shoot.Result.Post) sTitle += "<strong>";
        sTitle += "Alu: " + state.shoot.fChanceOnGoal.ToString("0.0%") + " .. " + (state.shoot.fChanceOnGoal + state.shoot.fChancePostBar).ToString("0.0%");
        if (state.shoot.result == CornerkickGame.Game.Shoot.Result.Bar || state.shoot.result == CornerkickGame.Game.Shoot.Result.Post) sTitle += "</strong>";
        sTitle += "<br/>";

        if (state.shoot.result == CornerkickGame.Game.Shoot.Result.Missed) sTitle += "<strong>";
        sTitle += "Daneben: " + (state.shoot.fChanceOnGoal + state.shoot.fChancePostBar).ToString("0.0%") + " .. " + 1.ToString("0.0%");
        if (state.shoot.result == CornerkickGame.Game.Shoot.Result.Missed) sTitle += "</strong>";
        sTitle += "<br/>";

        sTitle += "<strong>Ergebnis: " + state.shoot.fRnd.ToString("0.0%") + "</strong>";

        sTitle += "</div>";

        ViewGameModel.drawLine drawLine = new ViewGameModel.drawLine();

        drawLine.x0 = plShoot.ptPos.X;
        drawLine.y0 = plShoot.ptPos.Y;
        if (state.shoot.result == CornerkickGame.Game.Shoot.Result.SaveBounce) { // keeper
          CornerkickGame.Player plKeeper = CornerkickGame.Tool.getKeeper(game.player[1 - plShoot.iHA], game.iSuspensionIx, game.tc[plShoot.iHA].formation, game.ptPitch, game.data.nPlStart);

          drawLine.x1 = plKeeper.ptPos.X;
          drawLine.y1 = plKeeper.ptPos.Y;
          drawLine.sColor = "yellow";
          drawLine.sTitle = sTitle;

          drawLine.x0 /= game.ptPitch.X;
          drawLine.x1 /= game.ptPitch.X;
          drawLine.y0 += game.ptPitch.Y;
          drawLine.y1 += game.ptPitch.Y;
          drawLine.y0 /= (game.ptPitch.Y * 2);
          drawLine.y1 /= (game.ptPitch.Y * 2);

          ltDrawLine.Add(drawLine);

          drawLine = new ViewGameModel.drawLine();
          drawLine.x0 = ltDrawLine[0].x1;
          drawLine.y0 = ltDrawLine[0].y1;
        } else if (state.shoot.result == CornerkickGame.Game.Shoot.Result.Post) { // post
          drawLine.x1 = (1 - state.shoot.iHA) * game.ptPitch.X;
          drawLine.y1 = -2;
          drawLine.sColor = "yellow";
          drawLine.sTitle = sTitle;

          drawLine.x0 /= game.ptPitch.X;
          drawLine.x1 /= game.ptPitch.X;
          drawLine.y0 += game.ptPitch.Y;
          drawLine.y1 += game.ptPitch.Y;
          drawLine.y0 /= (game.ptPitch.Y * 2);
          drawLine.y1 /= (game.ptPitch.Y * 2);

          ltDrawLine.Add(drawLine);

          drawLine = new ViewGameModel.drawLine();
          drawLine.x0 = ltDrawLine[0].x1;
          drawLine.y0 = ltDrawLine[0].y1;
        } else if (state.shoot.result == CornerkickGame.Game.Shoot.Result.Bar) { // bar
          drawLine.x1 = (1 - state.shoot.iHA) * game.ptPitch.X;
          drawLine.y1 = 0;
          drawLine.sColor = "yellow";
          drawLine.sTitle = sTitle;

          drawLine.x0 /= game.ptPitch.X;
          drawLine.x1 /= game.ptPitch.X;
          drawLine.y0 += game.ptPitch.Y;
          drawLine.y1 += game.ptPitch.Y;
          drawLine.y0 /= (game.ptPitch.Y * 2);
          drawLine.y1 /= (game.ptPitch.Y * 2);

          ltDrawLine.Add(drawLine);

          drawLine = new ViewGameModel.drawLine();
          drawLine.x0 = ltDrawLine[0].x1;
          drawLine.y0 = ltDrawLine[0].y1;
        }
        drawLine.x1 = state.ball.ptPos.X;
        drawLine.y1 = state.ball.ptPos.Y;
        if      (state.shoot.result == CornerkickGame.Game.Shoot.Result.Goal)                                                                     drawLine.sColor = "red";
        else if (state.shoot.result == CornerkickGame.Game.Shoot.Result.Missed || state.shoot.result == CornerkickGame.Game.Shoot.Result.Blocked) drawLine.sColor = "cyan";
        else                                                                                                                                      drawLine.sColor = "yellow";

        drawLine.sTitle = sTitle;

        drawLine.x0 /= game.ptPitch.X;
        drawLine.x1 /= game.ptPitch.X;
        drawLine.y0 += game.ptPitch.Y;
        drawLine.y1 += game.ptPitch.Y;
        drawLine.y0 /= (game.ptPitch.Y * 2);
        drawLine.y1 /= (game.ptPitch.Y * 2);
        /*
        */

        ltDrawLine.Add(drawLine);
      }

      return ltDrawLine;
    }

    private static ViewGameModel.drawLine getPassLine(CornerkickGame.Game.State state, CornerkickGame.Game game, int iHA = -1, int iPlIx = -1)
    {
      ViewGameModel.drawLine drawLine = new ViewGameModel.drawLine();

      CornerkickGame.Player plPass = state.pass.plPasser;

      if (plPass == null) return drawLine;
      if (iHA >= 0 && plPass.iHA != iHA) return drawLine;
      if (iPlIx >= 0 && plPass.iIndex != iPlIx) return drawLine;

      drawLine.x0 = plPass.ptPos.X;
      drawLine.y0 = plPass.ptPos.Y;
      if (state.pass.plReceiver != null) {
        drawLine.x1 = state.pass.plReceiver.ptPos.X;
        drawLine.y1 = state.pass.plReceiver.ptPos.Y;
      } else {
        drawLine.x1 = state.ball.ptPos.X;
        drawLine.y1 = state.ball.ptPos.Y;
      }

      drawLine.sColor = "lime";
      if (state.pass.plReceiver == null) drawLine.sColor = "red";
      else if (state.pass.plReceiver.iHA != plPass.iHA) drawLine.sColor = "red";

      return drawLine;
    }

    private static string getDuelIcon(CornerkickGame.Game.State state, CornerkickGame.Game game, int iHA = -1, int iPlIx = -1)
    {
      CornerkickGame.Player plDef = state.duel.plDef;
      if (plDef == null) return "";

      CornerkickGame.Player plOff = state.duel.plOff;
      if (plOff == null) return "";

      if (iPlIx >= 0 && plDef.iIndex != iPlIx && plOff.iIndex != iPlIx) return "";

      string sDuelDesc = CornerkickManager.UI.getMinuteString(state.duel.tsMinute, false) + " Min.: " +
                         state.duel.plDef.sName + " vs. " + state.duel.plOff.sName;
      sDuelDesc += "<br/>";

      if (state.duel.iResult > 1) sDuelDesc += "<strong>";
      sDuelDesc += "Foul: " + 0.ToString("0.0%") + " .. " + state.duel.fChanceFoul.ToString("0.0%");
      if (state.duel.iResult > 1) sDuelDesc += "</strong>";
      sDuelDesc += "<br/>";

      if (state.duel.iResult == 1) sDuelDesc += "<strong>";
      sDuelDesc += "Def.: " + state.duel.fChanceFoul.ToString("0.0%") + " .. " + (state.duel.fChanceFoul + state.duel.fChanceWinDef).ToString("0.0%");
      if (state.duel.iResult == 1) sDuelDesc += "</strong>";
      sDuelDesc += "<br/>";

      if (state.duel.iResult == 0) sDuelDesc += "<strong>";
      sDuelDesc += "Off.: " + (state.duel.fChanceFoul + state.duel.fChanceWinDef).ToString("0.0%").PadLeft(5) + " .. 100.0%";
      if (state.duel.iResult == 0) sDuelDesc += "</strong>";
      sDuelDesc += "<br/>";

      sDuelDesc += "<strong>Ergebnis: " + state.duel.fRnd.ToString("0.0%") + "</strong>";

      string sDefOff = "off";
      if (iHA >= 0 && iHA != state.duel.plOff.iHA) sDefOff = "def";

      string sImg = "duel_" + sDefOff + "_1"; // win off. / loose def.
      if (state.duel.iResult == 0) sImg = "duel_" + sDefOff + "_0"; // win def. / loose off.

      // win off. / fould (and card) def.
      if (iHA < 0 || iHA == state.duel.plDef.iHA) {
        if (state.duel.iResult == 2) sImg = "whistle";
        else if (state.duel.iResult == 3) sImg = "yCard";
        else if (state.duel.iResult == 4) sImg = "yrCard";
        else if (state.duel.iResult == 5) sImg = "rCard";
      }

      string sDuelIcon = "<div style=\"position: absolute; top: " + ((plDef.ptPos.Y + game.ptPitch.Y) / (float)(2 * game.ptPitch.Y)).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; left: " + (plDef.ptPos.X / (float)game.ptPitch.X).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; z-index: 99\">";
      sDuelIcon += "<img class=\"tooltipDuel\" src=\"" + sContentDir + "/Icons/" + sImg + ".png\" alt=\"Karte\" style=\"position: relative; width: 12px\" title=\"" + sDuelDesc + "\"/>";
      sDuelIcon += "</div>";

      return sDuelIcon;
    }

    public static ViewGameModel.gameData getAllGameData(CornerkickManager.User usr, CornerkickGame.Game.Data gd, ViewGameModel.gameData gD_old, int iState = -1, bool bAddFMList = true)
    {
      CornerkickManager.Club clbUser = MemberController.ckClub(usr);

      ViewGameModel.gameData gD = new ViewGameModel.gameData();
      gD.iTeamId = clbUser.iId;

      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      if (gd == null) return gD;

      // Initialize chart values
      gD.ltF = new List<DataPointTD>[gd.nPlStart];
      for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) gD.ltF[iPl] = new List<DataPointTD>();
#if _PLOT_MORAL
      gD.ltM = new List<DataPointID>[_usr.game.data.nPlStart];
      for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) gD.ltM[iPl] = new List<DataPointID>();
#endif

      //iniGameData(usr, view, gd2.game);
      gD.sUserId = gD_old.sUserId;
      gD.sTeamH = gD_old.sTeamH;
      gD.sTeamA = gD_old.sTeamA;
      gD.sEmblemH = gD_old.sEmblemH;
      gD.sEmblemA = gD_old.sEmblemA;
      gD.sJerseyColors = gD_old.sJerseyColors;

      for (int iSt = 0; iSt < gd.ltState.Count; iSt++) {
        CornerkickGame.Game.State state = gd.ltState[iSt];
        addGameData(ref gD, gd, iSt, bAddFMList: state.tsMinute.Seconds == 0 && state.bNewRound && bAddFMList);

        if (iState == iSt) break;
      }

      return gD;
    }

    public static List<ViewGameModel.HeatmapPoint> getDivHeatmap(CornerkickManager.User? usr, byte iHA = 0, int iStateMax = 0, int iPlayer = -1)
    {
      List<ViewGameModel.HeatmapPoint> ltHp = new List<ViewGameModel.HeatmapPoint>();

      if (usr == null) return ltHp;

      ViewGameModel.gameData2? gd2 = getViewGameData(usr.id);

      if (gd2?.game == null) return ltHp;

      float fHeatmapMax = 0f;
      float[][] fHeatmap = ckMng.ui.getHeatmap(gd2.game, iHA, ref fHeatmapMax, iStateMax, iPlayer);

      string sDiv = "";

      for (int iX = 0; iX < fHeatmap.Length; iX++) {
        float fXper = iX / (float)(fHeatmap.Length - 1);
        fXper -= 0.01f; // - 1% (half width)

        for (int iY = 0; iY < fHeatmap[iX].Length; iY++) {
          string sColor = "white";
          float fYper = iY / (float)(fHeatmap[iX].Length - 1);
          fYper -= 0.015f; // - 1.5% (half height)

          float fHeat = fHeatmap[iX][iY];

#if DEBUG
          /*
          // Debug - Keeper in goal
          if (CornerkickGame.Tool.checkIfAt(new Point(iX, iY - gd2.game.ptPitch.Y), 1, 0, gd2.game.ptPitch, gd2.game.ptBox, new Point(), new Point()) ||
              CornerkickGame.Tool.checkIfAt(new Point(iX, iY - gd2.game.ptPitch.Y), 1, 1, gd2.game.ptPitch, gd2.game.ptBox, new Point(), new Point())) {
            fHeat = 0.45f;
          } else {
            fHeat = 0.0f;
          }
          */
#endif

          if (fHeat == 0f) continue;

          int iZindex = 8;

          if (fHeat < 0.01f) {
            sColor = "DarkBlue";
            iZindex = 1;
          } else if (fHeat < 0.05f) {
            sColor = "LightSkyBlue";
            iZindex = 2;
            //} else if (fHeat < 0.2f) { sColor = "DarkGreen;
            //} else if (fHeat < 0.3f) { sColor = "ForestGreen;
          } else if (fHeat < 0.10f) {
            sColor = "Lime";
            iZindex = 3;
          } else if (fHeat < 0.20f) {
            sColor = "Yellow";
            iZindex = 4;
          } else if (fHeat < 0.30f) {
            sColor = "Orange";
            iZindex = 5;
          } else if (fHeat < 0.40f) {
            sColor = "Red";
            iZindex = 6;
          } else if (fHeat < 0.50f) {
            sColor = "Magenta";
            iZindex = 7;
          }
          //iZindex = (int)(fHeat * 10) + 1;

          ltHp.Add(
            new ViewGameModel.HeatmapPoint() { value = fHeat, x = fXper, y = fYper, level = iZindex, color = sColor }
          );

          /*
          sDiv += "<div style=\"position: absolute; width: 2%; height: 3%; top: " + fYper.ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; left: " + fXper.ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; background-color: " + sColor + "; -webkit-border-radius: 50%; -moz-border-radius: 50%; opacity: 0.5; z-index:" + iZindex.ToString() + "\">" +
                  //"<h2 style=\"position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 100%; color: black; z-index:2\">" + fHeat.ToString("0%") + "</h2>" +
                  "</div>";
          */
        }
      }

      return ltHp;
    }

    public static void AdminPlay(CornerkickManager.User usr)
    {
      if (usr.game == null) {
        usr.game = ckMng.game.tl.getDefaultGame();
      }

      usr.game.data.iGameSpeed = 300;

      usr.game.start();
    }

    public static void AdminStop(CornerkickManager.User usr)
    {
      if (usr.game == null) return;

      usr.game.stop();
    }

    public static void AdminNext(CornerkickManager.User usr, sbyte iNextAction, int iX = -1, int iY = -1)
    {
      if (usr.game == null) {
        usr.game = ckMng.game.tl.getDefaultGame();
      }

      usr.game.data.bFinished = false;
      usr.game.data.iGameSpeed = 1;

      usr.game.next(iPlayerNextAction: iNextAction, iPassNextActionX: iX, iPassNextActionY: iY);
    }

    public static void AdminNew(CornerkickManager.User usr)
    {
      usr.game = ckMng.game.tl.getDefaultGame();

      usr.game.data.iGameSpeed = 300;
    }

    public static void AdminSetPos(CornerkickManager.User usr, int iHA, int iPl, int iX, int iY)
    {
      if (usr.game == null) {
        usr.game = ckMng.game.tl.getDefaultGame();
      }

      if (iHA < 0) {
        usr.game.ball.ptPos = new System.Drawing.Point(iX, iY);
        usr.game.ball.iStep = 0;
        usr.game.ball.nSteps = 0;
        usr.game.ball.setPos();
        usr.game.ball.plAtBall = null;
      } else {
        usr.game.player[iHA][iPl].ptPos = new System.Drawing.Point(iX, iY);
      }

      for (int jHA = 0; jHA < 2; jHA++) {
        foreach (CornerkickGame.Player pl in usr.game.player[jHA]) {
          if (pl.ptPos == usr.game.ball.ptPos) {
            usr.game.ball.plAtBall = pl;
            break;
          }
        }
      }
    }

    public static void AdminSetReferee(CornerkickManager.User usr, float fReferee)
    {
      if (usr.game == null) {
        usr.game = CkAppShared.ckMng.game.tl.getDefaultGame();
      }

      usr.game.data.referee.fStrict = fReferee;
    }

    public static float AdminSetTaktik(CornerkickManager.User usr, int iTaktik, float fTaktik)
    {
      float fRet = 0f;
      byte iTactic = 0;

      CornerkickGame.Tactic tc = usr.game.data.team[0].ltTactic[iTactic];
      if (iTaktik == 0) tc.fOrientation = fTaktik;
      else if (iTaktik == 1) tc.fPower = fTaktik;
      else if (iTaktik == 2) tc.fShootFreq = fTaktik;
      else if (iTaktik == 3) tc.fAggressive = fTaktik;
      else if (iTaktik == 4) tc.fPassRisk = fTaktik;
      else if (iTaktik == 5) tc.fPassLength = fTaktik;
      else if (iTaktik == 6) tc.fPassFreq = fTaktik;
      else if (iTaktik == 7) {
        tc.fPassLeft = fTaktik - 1f;
        if (tc.fPassLeft + tc.fPassRight > 1f) tc.fPassRight = (float)Math.Round(1f - tc.fPassLeft, 2);
        fRet = tc.fPassRight;
      } else if (iTaktik == 8) {
        tc.fPassRight = fTaktik - 1f;
        if (tc.fPassLeft + tc.fPassRight > 1f) tc.fPassLeft = (float)Math.Round(1f - tc.fPassRight, 2);
        fRet = tc.fPassLeft;
      } else if (iTaktik == 9) tc.iGapOffsite = (int)Math.Round(fTaktik);

      // Set tactic of current game
      if (usr.game != null) {
        usr.game.data.team[0].ltTactic[iTactic] = tc;
        usr.game.data.team[1].ltTactic[iTactic] = tc;
        usr.game.tc[0] = tc;
        usr.game.tc[1] = tc;
      }

      return fRet;
    }

    public static object AdminGetPlayerChances(CornerkickManager.User usr)
    {
      if (usr.game == null) return null;

      CornerkickGame.Player plChance = usr.game.ball.plAtBall;
      if (plChance == null && usr.game.ball.plAtBallLast != null) plChance = usr.game.ball.plAtBallLast;
      if (plChance == null) return null;

      float[] fPlAction;
      double fPlActionRnd;
      System.Drawing.Point? ptPassTarget;
      bool bLowPass = false;
      sbyte iAction = usr.game.ai.getPlayerAction(plChance, out fPlAction, out fPlActionRnd, out ptPassTarget, out bLowPass);

      // Get all pass targets
      List<ViewGameModel.PassTarget> ltPtBallTarget = new List<ViewGameModel.PassTarget>();
      List<CornerkickGame.AI.Receiver> ltReceiver = CornerkickGame.AI.getReceiverList(usr.game, plChance, 0);
      if (ltReceiver != null) {
        foreach (CornerkickGame.AI.Receiver rec in ltReceiver) {
          // Check if current pass target is players choice
          bool bPlayerChoice = false;
          if (ptPassTarget.HasValue) bPlayerChoice = ptPassTarget.Value.Equals(rec.ptTarget);

          ltPtBallTarget.Add(new ViewGameModel.PassTarget(rec.ptTarget, Z: rec.fPassChance, bPlayerChoice: bPlayerChoice));
        }
      }

      return new { fPlAction = fPlAction, plPos = new TeamModel.Point(plChance.ptPos), ltPassTargets = ltPtBallTarget.ToArray() };
    }

    public static bool AdminGetPlayerTargetPos(CornerkickManager.User usr)
    {
      if (usr.game == null) return false;

      usr.game.ai.setTargetPositions();

      return true;
    }

  }
}
