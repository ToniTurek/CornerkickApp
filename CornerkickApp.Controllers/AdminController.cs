using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CornerkickApp.Shared.Models;

namespace CornerkickApp.Controllers
{
  public class AdminController
  {
    public AdminViewModel Settings()
    {
      AdminViewModel modelAdmin = new AdminViewModel();
#if _WebApp
      if (CkAppShared.ckMng == null) {
        modelAdmin.bCk = false;
        return modelAdmin;
      }
      modelAdmin.bCk = true;

      // Timer
      if (CkAppShared.timerCkCalender != null) {
        modelAdmin.bTimer            = CkAppShared.timerCkCalender.Enabled;
        modelAdmin.fCalendarInterval = CkAppShared.timerCkCalender.Interval / 1000;
      }

      if (CkAppShared.timerSave != null) {
        modelAdmin.bTimerSave = CkAppShared.timerSave.Enabled;
      }

      // Settings
      modelAdmin.sStartHour = "";
      if (CkAppShared.settings.iStartHour >= 0) modelAdmin.sStartHour = CkAppShared.settings.iStartHour.ToString();
      modelAdmin.iGameSpeed          = App.getGameSpeedFromUsers();
      modelAdmin.bLoginPossible      = CkAppShared.settings.bLoginPossible;
      modelAdmin.bEmailCertification = CkAppShared.settings.bEmailCertification;
      modelAdmin.bRegisterDuringGame = CkAppShared.settings.bRegisterDuringGame;
      modelAdmin.bMaintenance        = CkAppShared.settings.bMaintenance;
      modelAdmin.sInfo               = CkAppShared.settings.sInfo;
      if (CkAppShared.settings.dtCounterStart > DateTime.Now) modelAdmin.dtCounterStart = CkAppShared.settings.dtCounterStart;
      else modelAdmin.dtCounterStart = null;
      modelAdmin.sHomeDir   = Path.Combine(App.getHomeDir(), "App_Data");
      modelAdmin.sHomeDirCk = CkAppShared.ckMng.settings.sHomeDir;

      // Statistics
      modelAdmin.nClubs  = CkAppShared.ckMng.ltClubs .Count;
      modelAdmin.nUser   = CkAppShared.ckMng.ltUser  .Count;
      modelAdmin.nPlayer = CkAppShared.ckMng.ltPlayer.Count;

      modelAdmin.dtCkCurrent = CkAppShared.ckMng.dtDatum;
      modelAdmin.dtCkApproach = App.getCkApproachDate();
      modelAdmin.fIntervalAveToApproachTarget = App.getIntervalAve();

      // Files
      modelAdmin.bLogExist = System.IO.File.Exists(Path.Combine(modelAdmin.sHomeDir, "log", "ck.log"));

      //DirectoryInfo d = new DirectoryInfo(sHomeDir + "save");
      //FileInfo[] ltCkxFiles = d.GetFiles("*.ckx");
      modelAdmin.bSaveDirExist  = Directory.Exists(Path.Combine(modelAdmin.sHomeDir, "save"));
      if (modelAdmin.bSaveDirExist) modelAdmin.bAutosaveExist = System.IO.File.Exists(Path.Combine(modelAdmin.sHomeDir, "save", ".autosave.ckx"));

      if (CkAppShared.clubAdmin != null) modelAdmin.iSelectedClubAdmin = CkAppShared.clubAdmin.iId;
#endif

      return modelAdmin;
    }

    public void StartCalendar(AdminViewModel modelAdmin)
    {
#if _WebApp
      /*
      if (CkAppShared.ckMng.iSaisonCount == 0) {
        App.ltLog.Clear();
        CkAppShared.ckMng.setNeueSaison();
      }
      */
      CkAppShared.settings.iStartHour = -1;
      if (!string.IsNullOrEmpty(modelAdmin.sStartHour)) {
        int.TryParse(modelAdmin.sStartHour, out CkAppShared.settings.iStartHour);
      }

      if (modelAdmin.fCalendarInterval < 1E-6) {
        CkAppShared.timerCkCalender.Enabled = false;
        return;
      }

      /*
      // If first step: Add CPU teams
      if (CkAppShared.ckMng.dtDatum.Date.Equals(CkAppShared.ckMng.dtSeasonStart.Date) && CkAppShared.ckMng.iSaisonCount == 0) {
      }
      */

      setGameSpeedToAllUsers(modelAdmin.iGameSpeed);

      // Do one step now
      //CkAppShared.ckMng.next(true);

      // Start the timer
      CkAppShared.timerCkCalender.Interval = modelAdmin.fCalendarInterval * 1000;
      CkAppShared.timerCkCalender.Enabled = true;

      // Save last state
      App.saveLaststate(CkAppShared.ckMng.settings.sHomeDir);
#endif
    }

    public static void setGameSpeedToAllUsers(int iGameSpeed)
    {
      foreach (CornerkickManager.User user in CkAppShared.ckMng.ltUser) {
        if (user.club  ?.nextGame != null) user.club  .nextGame.iGameSpeed = iGameSpeed;
        if (user.nation?.nextGame != null) user.nation.nextGame.iGameSpeed = iGameSpeed;
      }
    }

    public void StopCalendar(AdminModel modelAdmin)
    {
#if _WebApp
      CkAppShared.timerCkCalender.Enabled = false;
      CkAppShared.timerSave.Enabled = true;

      // Save last state
      App.saveLaststate(CkAppShared.ckMng.settings.sHomeDir);
#endif
    }

    public void OneStep()
    {
#if _WebApp
      // Do one step now
      App.performCalendarStep(bSave: false);
#endif
    }

    public void StepBack()
    {
      CkAppShared.ckMng.dtDatum = CkAppShared.ckMng.dtDatum.AddMinutes(-15);
    }

    public void RestartCk(AdminModel modelAdmin)
    {
#if _WebApp
      if (CkAppShared.timerCkCalender != null) CkAppShared.timerCkCalender.Enabled = false;

      App.newCk();
#endif
    }

    public void SaveAutosave()
    {
#if _WebApp
      App.save(CkAppShared.timerCkCalender, true);
#endif
    }

    public void DeleteAutosave()
    {
      string sFileAutosave = Path.Combine(App.getHomeDir(), "App_Data", "save", ".autosave.ckx");
      if (System.IO.File.Exists(sFileAutosave)) System.IO.File.Delete(sFileAutosave);
    }

    public void DeleteSaveFolder()
    {
      // Delete save directory
      string sDirSave = Path.Combine(App.getHomeDir(), "App_Data", "save");
      if (System.IO.Directory.Exists(sDirSave)) System.IO.Directory.Delete(sDirSave, true);

      // Delete laststate.txt file
      string sFileLaststate = Path.Combine(App.getHomeDir(), "App_Data", "laststate.txt");
      if (System.IO.File.Exists(sFileLaststate)) System.IO.File.Delete(sFileLaststate);
    }

    public void LoadAutosave(AdminViewModel modelAdmin)
    {
#if _WebApp
      string sFileAutosave = Path.Combine(App.getHomeDir(), "App_Data", "save", modelAdmin.sSelectedAutosaveFile);
      if (System.IO.File.Exists(sFileAutosave)) {
        CkAppShared.timerCkCalender.Enabled = false;

        App.newCk(bLoadGame: false);

        CkAppShared.ckMng.io.load(sFileAutosave);
      }
#endif
    }

    public string TransferMoney(int iClubTransferMoney, int iTransferMoney, string sTransferMoneySubject)
    {
      if (iTransferMoney == 0) return "";

#if _WebApp
      try {
        CornerkickManager.Club clbTransferMoney = CkAppShared.ckMng.ltClubs.Find(c => c.iId == iClubTransferMoney);
        if (clbTransferMoney == null) return "";

        CornerkickManager.Finance.doTransaction(clbTransferMoney, CkAppShared.ckMng.dtDatum, iTransferMoney, 0, sSubject: sTransferMoneySubject);
        return iTransferMoney.ToString("N0", System.Globalization.CultureInfo.CurrentCulture) + " € an " + clbTransferMoney.sName + " transferiert (Betreff: '" + sTransferMoneySubject + "')";
      } catch {
      }
#endif

      return "";
    }

    public string ShiftClubToLeague(int iClub, int iCup)
    {
      CornerkickManager.Club? clb = CkAppShared.ckMng.ltClubs.Find(c => c.iId == iClub);
      if (clb == null) return "";

      CornerkickManager.Cup cup = CkAppShared.ckMng.ltCups[iCup];
      CornerkickManager.Cup cupCurrent = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, iId2: cup.iId2, iId3: clb.iDivision);
      clb.iDivision = (byte)cup.iId3;
      cupCurrent.ltClubs[0].Remove(clb);
      cup.ltClubs[0].Insert(0, clb);

      CkAppShared.ckMng.calcMatchdays();
      CkAppShared.ckMng.drawCup(cup);

      return "Club " + clb.sName + " in Liga " + cup.sName + " verschoben";
    }

    public void setSettings(bool bEmailCertification, bool bRegisterDuringGame, bool bLoginPossible, bool bMaintenance, string sInfo, DateTime dtCounterStart)
    {
       CkAppShared.settings.bEmailCertification = bEmailCertification;
       CkAppShared.settings.bRegisterDuringGame = bRegisterDuringGame;
       CkAppShared.settings.bLoginPossible      = bLoginPossible;
       CkAppShared.settings.bMaintenance        = bMaintenance;
       CkAppShared.settings.sInfo               = sInfo;
       CkAppShared.settings.dtCounterStart      = dtCounterStart;
    }

    public AdminViewModel Log()
    {
      AdminViewModel modelAdmin = new AdminViewModel();

      modelAdmin.ltLog = new List<string>();
      modelAdmin.ltErr = new List<string>();

      /*
      if (CkAppShared.ckMng != null) {
        if (CkAppShared.ckMng.log != null) {
          if (CkAppShared.ckMng.log.Count > 0) {

            //ltLog = new List<string>(CkAppShared.ckMng.log);
            App.ltLog.AddRange(CkAppShared.ckMng.log);
          }
        }
      }

      foreach (string s in App.ltLog) modelAdmin.sLog += s + '\n';
      */

      // Log
      string sFileLog = Path.Combine(App.getHomeDir(), "App_Data", "log", CornerkickManager.Main.sLogFile);
      try {
        // Create an instance of StreamReader to read from a file.
        // The using statement also closes the StreamReader.
        using (StreamReader sr = new StreamReader(sFileLog)) {
          string sLine;
          // Read and display lines from the file until the end of 
          // the file is reached.
          while ((sLine = sr.ReadLine()) != null) {
            modelAdmin.ltLog.Add(sLine);
          }
        }
      } catch (Exception e) {
        // Let the user know what went wrong.
        Console.WriteLine("The file could not be read:");
        Console.WriteLine(e.Message);
      }

      // Error
      string sFileErr = Path.Combine(App.getHomeDir(), "App_Data", "log", CornerkickManager.Main.sErrorFile);
      try {
        // Create an instance of StreamReader to read from a file.
        // The using statement also closes the StreamReader.
        using (StreamReader sr = new StreamReader(sFileErr)) {
          string sLine;
          // Read and display lines from the file until the end of 
          // the file is reached.
          while ((sLine = sr.ReadLine()) != null) {
            modelAdmin.ltErr.Add(sLine);
          }
        }
      } catch (Exception e) {
        // Let the user know what went wrong.
        Console.WriteLine("The file could not be read:");
        Console.WriteLine(e.Message);
      }

      return modelAdmin;
    }

    public void DeleteLog()
    {
      var diLog = new DirectoryInfo(Path.Combine(App.getHomeDir(), "App_Data", "log"));
      foreach (var file in diLog.EnumerateFiles("*.log")) {
        file.Delete();
      }
      foreach (var file in diLog.EnumerateFiles("*.err")) {
        file.Delete();
      }

      string sFileLogZip = Path.Combine(App.getHomeDir(), "App_Data", "log.zip");
      if (System.IO.File.Exists(sFileLogZip)) System.IO.File.Delete(sFileLogZip);
    }

    public string getFilesInDirectory(string sDir = ".")
    {
      //if (string.IsNullOrEmpty(sDir) || sDir.Equals(".")) sDir = Server.MapPath("~");
      if (string.IsNullOrEmpty(sDir)) {
        sDir = ".";
      } else {
        sDir = Path.Combine(App.getHomeDir(), sDir);
      }

      DirectoryInfo d = new DirectoryInfo(sDir);
      if (!d.Exists) {
        //Response.StatusCode = 1;
        return "Directory does not exist!";
      }

      string sContent = ".." + '\n';

      // First get directories
      foreach (string sSubDir in Directory.GetDirectories(sDir)) {
        sContent += "<DIR> " + Path.GetFileName(sSubDir) + '\n';
      }

      // then get files
      //return Json(d.GetFiles("*").ToArray(), JsonRequestBehavior.AllowGet);
      foreach (FileInfo fi in d.GetFiles("*")) {
        sContent += fi.Name + " (" + fi.Length.ToString() + "b)\n";
      }

      return sContent;
    }

    public class UnfinishedGame
    {
      public int iCupId1 { get; set; }
      public int iCupId2 { get; set; }
      public int iCupId3 { get; set; }
      public string sCupName { get; set; } = "";
      public int iMd { get; set; }
      public string sDate { get; set; } = "";
      public int iClubIdH { get; set; }
      public int iClubIdA { get; set; }
      public string sClubNameH { get; set; } = "";
      public string sClubNameA { get; set; } = "";
    }
    public List<UnfinishedGame> GetUnfinishedGames()
    {
      List<UnfinishedGame> ltUg = new List<UnfinishedGame>();

      foreach (CornerkickManager.Cup cup in CkAppShared.ckMng.ltCups) {
        if (Math.Abs(cup.iId) == CkAppShared.iCupIdTestgame) continue; // No test-games

        for (int iMd = 0; iMd < cup.ltMatchdays.Count; iMd++) {
          if (cup.ltMatchdays[iMd].ltGameData == null) continue;

          for (int iGd = 0; iGd < cup.ltMatchdays[iMd].ltGameData.Count; iGd++) {
            CornerkickGame.Game.Data gd = cup.ltMatchdays[iMd].ltGameData[iGd];
            if (gd.dt.CompareTo(CkAppShared.ckMng.dtDatum) < 0) { // Game is in past
              if (gd.team[0].iGoals < 0 ||
                  gd.team[1].iGoals < 0) {
                ltUg.Add(
                  new UnfinishedGame() {
                    iCupId1 = cup.iId,
                    iCupId2 = cup.iId2,
                    iCupId3 = cup.iId3,
                    sCupName = cup.sName,
                    iMd = iMd,
                    sDate = gd.dt.ToString("d", new CultureInfo("de-DE")) + " - " + gd.dt.ToString("t", new CultureInfo("de-DE")),
                    iClubIdH = gd.team[0].iTeamId,
                    iClubIdA = gd.team[1].iTeamId,
                    sClubNameH = CornerkickManager.Tool.getClubFromId(gd.team[0].iTeamId, CkAppShared.ckMng.ltClubs).sName,
                    sClubNameA = CornerkickManager.Tool.getClubFromId(gd.team[1].iTeamId, CkAppShared.ckMng.ltClubs).sName
                  }
                );
              }
            }
          }
        }
      }

      return ltUg;
    }

    public object setUnfinishedGameResult(int iCupId1, int iCupId2, int iCupId3, int iMd, int iClubIdH, int iClubIdA, int iGoalsH, int iGoalsA, DateTime dtNewGameDate)
    {
      if ((iGoalsH < 0 || iGoalsA < 0) && dtNewGameDate.CompareTo(CkAppShared.ckMng.dtDatum) < 0) return new { result = false, message = "Goals or date not valid!" };

      CornerkickManager.Cup cup = CkAppShared.ckMng.tl.getCup(iCupId1, iCupId2, iCupId3);
      if (cup == null) return new { result = false, message = "Cup not found!" };

      if (cup.ltMatchdays == null || iMd >= cup.ltMatchdays.Count) return new { result = false, message = "Matchday not valid" };

      for (int iGd = 0; iGd < cup.ltMatchdays[iMd].ltGameData.Count; iGd++) {
        if (cup.ltMatchdays[iMd].ltGameData[iGd].team[0].iTeamId == iClubIdH &&
            cup.ltMatchdays[iMd].ltGameData[iGd].team[1].iTeamId == iClubIdA) {
          // Reset cup ids
          cup.ltMatchdays[iMd].ltGameData[iGd].iGameType  = cup.iId;
          cup.ltMatchdays[iMd].ltGameData[iGd].iGameType2 = cup.iId2;
          cup.ltMatchdays[iMd].ltGameData[iGd].iGameType3 = cup.iId3;

          // Set new result
          if (iGoalsH >= 0 && iGoalsA >= 0) {
            cup.ltMatchdays[iMd].ltGameData[iGd].team[0].iGoals = iGoalsH;
            cup.ltMatchdays[iMd].ltGameData[iGd].team[1].iGoals = iGoalsA;

            return new { result = true, message = "Game result set to " + iGoalsH.ToString() + " : " + iGoalsA.ToString() };
          } else if (dtNewGameDate.CompareTo(CkAppShared.ckMng.dtDatum) > 0) {
            cup.ltMatchdays[iMd].ltGameData[iGd].dt = dtNewGameDate;

            return new { result = true, message = "Game date set to " + dtNewGameDate.ToString("d", new CultureInfo("de-DE")) + " - " + dtNewGameDate.ToString("t", new CultureInfo("de-DE")) + " Uhr" };
          }
        }
      }

      return new { result = false, message = "Clubs not found!" };
    }

    public void SetAdminClub(int iClubIx)
    {
      CkAppShared.clubAdmin = null;

      if (iClubIx <                         0) return;
      if (iClubIx >= CkAppShared.ckMng.ltClubs.Count) return;

      CkAppShared.clubAdmin = CkAppShared.ckMng.ltClubs[iClubIx];
    }

#if _USE_BLOB
    public ActionResult CreateBlobContainer()
    {
      CloudBlobContainer container = App.GetCloudBlobContainer();
      ViewBag.Success = container.CreateIfNotExists();
      ViewBag.BlobContainerName = container.Name;

      return View();
    }

#endif

    internal static void removeUser(CornerkickManager.User usr)
    {
      if (usr.club != null) {
        // Set CPU name to club
        string sNameNew = "";
        int iC = 0;
        while (iC < 10000) {
          iC++;
          sNameNew = "Team_" + CornerkickManager.Main.sLand[usr.club.iLand] + "_" + iC.ToString();

          bool bFound = true;
          foreach (CornerkickManager.Club clbExist in CkAppShared.ckMng.ltClubs) {
            if (clbExist.sName.Equals(sNameNew)) {
              bFound = false;
              break;
            }
          }

          if (bFound) break;
        }
        if (!string.IsNullOrEmpty(sNameNew)) usr.club.sName = sNameNew;

        // Rename stadium
        usr.club.stadium.sName = usr.club.sName + " Stadion";

        // Clear user
        usr.club.user = null;

        // Clear further data
        usr.club.ltMerchandisingHistory = null;

        // Add player if too few
        while (usr.club.ltPlayer.Count < 22) CkAppShared.ckMng.plt.newPlayer(club: usr.club);

        // Delete emblem
        string sBaseDir = CkAppShared.sWwwRootDir;
        if (string.IsNullOrEmpty(sBaseDir)) sBaseDir = App.getHomeDir();
#if !DEBUG
        sBaseDir = System.IO.Directory.GetParent(sBaseDir).FullName;
#endif

        foreach (string sFileExt in new string[3] { ".png", ".jpg", ".gif" }) {
          string sFilenameLocal = Path.Combine(sBaseDir, "Content", "Uploads", "emblems", usr.club.iId.ToString() + sFileExt);
          try {
            System.IO.File.Delete(sFilenameLocal);
          } catch {
          }

#if !DEBUG
#if _USE_AMAZON_S3
          // Remove emblem from aws
          Task.Run(async () => await App.as3.deleteFileAsync(CkAppShared.sCkInstanceName + "/emblems/" + usr.club.iId + sFileExt));
#endif
#endif
        }

        // Do auto formation
        CkAppShared.ckMng.doFormation(usr.club);
      }

      if (usr.nation != null) usr.nation.user = null;
      CkAppShared.ckMng.ltUser.Remove(usr);
    }
  }
}
