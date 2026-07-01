#if _WebApp
using System.Globalization;
using System.Timers;
#endif
using CornerkickApp.Shared.Models;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using System.Numerics;
using System.Security.Claims;

namespace CornerkickApp.Controllers
{
  public class App
  {
    //////////////////////////////////////////////////////////////////////////
    // Version tags
    //////////////////////////////////////////////////////////////////////////
    public const string VersionMajor = ThisAssembly.Git.BaseVersion.Major;
    public const string VersionMinor = ThisAssembly.Git.BaseVersion.Minor;
    public const string VersionPatch = ThisAssembly.Git.BaseVersion.Patch;
    public const string Commit = ThisAssembly.Git.Commit;
    public static string Version {
      get {
        return VersionMajor + "." + VersionMinor + "." + VersionPatch;
      }
    }

    //public static CornerkickManager.Main ckMng;
    public static AmazonS3FileTransfer? as3;

    public App(IConfiguration config, string sHomeDir, string sAppDataDir)
    {
      CkAppShared.sHomeDir    = sHomeDir;
      CkAppShared.sAppDataDir = sAppDataDir;

      as3 = new AmazonS3FileTransfer(config);

#if _WebApp
      // Get cornerkick instance name
#if _DEPLOY_ON_HOST // Use environment variable
      App.sCkInstanceName = Environment.GetEnvironmentVariable("ckInstanceName");
#else
      CkAppShared.sCkInstanceName = config.GetSection("ckInstanceName").Value;
#endif
#endif

      // Get admin email
      AdminModel.sAdminEmail = Environment.GetEnvironmentVariable("ckAdminEmail"); // First, get admin email from environment
      if (string.IsNullOrEmpty(AdminModel.sAdminEmail)) AdminModel.sAdminEmail = config.GetSection("ckAdminEmail")?.Value; // If empty, get it from appsettings.json

      //start();
    }

    //Timer timerStart;
    public void start()
    {
#if _WebApp
      // Create stopwatch
      System.Diagnostics.Stopwatch swStart = new System.Diagnostics.Stopwatch();

      // Start stopwatch
      swStart.Start();

      try {
        newCk();

        // Stop stopwatch
        swStart.Stop();
        TimeSpan tsStart = swStart.Elapsed;

        // Write elapsed time to log
        if (CkAppShared.ckMng != null) CkAppShared.ckMng.tl.writeLog("Elapsed time during start: " + tsStart.TotalSeconds.ToString("0.000") + "s");
      } catch (Exception e) {
        System.Diagnostics.Debug.WriteLine(e.Message);
        System.Diagnostics.Debug.WriteLine(e.StackTrace);
      }

      /*
      timerStart = new Timer(fStartDelay);
      timerStart.Elapsed += new ElapsedEventHandler(timerStart_Tick);
      timerStart.Start();
      */
#else
      //ckMng = new CornerkickManager.Main(sHomeDir: Path.Combine(getHomeDir(), "App_Data"), sLogDir: DocumentsDir, bArchiveCups: false);
      // Create new cornerkick manager instance
      CkAppShared.ckMng = GetIniCkMng();
#endif

      // Apply default stadiums
      CkAppShared.stadiumDefaultTrainingCourt = getDefaultStadium(bTiny: true);
      CkAppShared.stadiumDefaultSmall         = getDefaultStadium();
      CkAppShared.stadiumDefaultBig           = getDefaultStadium(bBig: true);

#if _USE_AMAZON_S3
      if (as3 != null && CkAppShared.ckMng != null) CkAppShared.ckMng.tl.writeLog("Ck instance name was set to '" + as3.sCkInstanceName + "'");
#endif
    }

    public static CornerkickManager.Main GetIniCkMng()
    {
      //CornerkickGame.Models.Microsoft_ML.DecisionMaking.LoadModel(Path.Combine(getDocumentsDir, "mlmodels", "actionModel.zip"));

      return new CornerkickManager.Main(sHomeDir: CkAppShared.sHomeDir,
                                        sLogDir: CkAppShared.sAppDataDir,
                                        bContinuingTime: false,
                                        iTrainingsPerDay: 3,
                                        iTrainingsPerDayMax: 3,
                                        bPlayerTransferOnlyOncePerSeason: true,
                                        iWriteGamesToDisk: 0,
                                        bArchiveCups: false,
                                        fMoralMin: 0.4f
                                      );
    }

    /*
    private void timerStart_Tick(object sender, EventArgs e)
    {
      timerStart.Stop();

      // Create stopwatch
      System.Diagnostics.Stopwatch swStart = new System.Diagnostics.Stopwatch();

      // Start stopwatch
      swStart.Start();

      try {
        newCk();

        // Stop stopwatch
        swStart.Stop();
        TimeSpan tsStart = swStart.Elapsed;

        // Write elapsed time to log
        if (CkAppShared.ckMng != null) CkAppShared.ckMng.tl.writeLog("Elapsed time during start: " + tsStart.TotalSeconds.ToString("0.000") + "s");
      } catch {
      }
    }
    */

#if _WebApp
    internal static void newCk(bool bLoadGame = true)
    {
      // Create new cornerkick manager instance
      CkAppShared.ckMng = new CornerkickManager.Main(sHomeDir: CkAppShared.sHomeDir,
                                                     sLogDir: CkAppShared.sAppDataDir,
                                                     bContinuingTime: true,
                                                     iTrainingsPerDay: 3,
                                                     iTrainingsPerDayMax: 3,
                                                     bPlayerTransferOnlyOncePerSeason: true,
                                                     iWriteGamesToDisk: 0,
                                                     bArchiveCups: true,
                                                     fMoralMin: 0.4f
                                         );

      CkAppShared.ckMng.tl.writeLog("WebMvc START");

      CkAppShared.ckMng.dtDatum = new DateTime(DateTime.Now.Year, CkAppShared.ckMng.dtDatum.Month, CkAppShared.ckMng.dtDatum.Day);

#if _SCOUTING_BETA_MODE
      CornerkickManager.Main.staff.ltScouts.Add(
        new CornerkickManager.Main.Staff.Scout() {
          iId = 100,
          sName = "Frei 1",
          iSkill = 3,
          nDataPerScouting = 7
        }
      );
      CornerkickManager.Main.staff.ltScouts.Add(
        new CornerkickManager.Main.Staff.Scout() {
          iId = 101,
          sName = "Frei 2",
          iSkill = 6,
          nDataPerScouting = 3
        }
      );
#endif

      if (CkAppShared.timerCkCalender == null) {
        CkAppShared.timerCkCalender = new System.Timers.Timer(120000);
        CkAppShared.timerCkCalender.Elapsed += new ElapsedEventHandler(timerCkCalender_Elapsed);
      }
      CkAppShared.timerCkCalender.Enabled = false;

      if (CkAppShared.timerSave == null) {
        CkAppShared.timerSave = new System.Timers.Timer(15 * 60 * 1000); // 15 min.
        CkAppShared.timerSave.Elapsed += new ElapsedEventHandler(timerSave_Elapsed);
      }
      CkAppShared.timerSave.Enabled = false;

      // Create big default stadium
      for (int iB = 0; iB < CkAppShared.stadiums[0].blocks.Length; iB++) {
        if (iB < 8) {
          CkAppShared.stadiums[0].blocks[iB].iSeats = 4000;
          CkAppShared.stadiums[0].blocks[iB].iType = 1;
        } else if (iB < 10) {
          CkAppShared.stadiums[0].blocks[iB].iSeats = 6000;
        } else {
          CkAppShared.stadiums[0].blocks[iB].iSeats = 2000;
          CkAppShared.stadiums[0].blocks[iB].iType = 1;
        }
      }
      CkAppShared.stadiums[0].blocks[1].iSeats = 1000;
      CkAppShared.stadiums[0].blocks[1].iType = 2;

      // Create small default stadium
      for (int iB = 0; iB < 10; iB++) {
        if (iB < 8) {
          CkAppShared.stadiums[1].blocks[iB].iSeats = 2000;
          CkAppShared.stadiums[1].blocks[iB].iType = 1;
        } else {
          CkAppShared.stadiums[1].blocks[iB].iSeats = 3000;
        }
      }

      // Load ck game
      if (bLoadGame) {
        CkAppShared.iLoadState = 1;
        loadAsync(CkAppShared.sAppDataDir, iDelay: (int)CkAppShared.fLoadDelay);
        /*
        CkAppShared.timerLoad = new CkAppShared.TimerLoad {
          Interval = CkAppShared.fLoadDelay,
          sAppDataDir = sAppDataDir
        };

        CkAppShared.timerLoad.Elapsed += new ElapsedEventHandler(timerLoad_Tick);
        CkAppShared.timerLoad.Start();
        */
      }

#if !DEBUG
#if _VS_2019
      // Login of admin to start database
      string sAdminEmail = ConfigurationManager.AppSettings["ckAdminEmail"];
      if (!string.IsNullOrEmpty(sAdminEmail)) {
        Controllers.AccountController accountController = new Controllers.AccountController();
        Task<SignInStatus> tkLoginAdmin = Task.Run(async () => await accountController.SignInManager.PasswordSignInAsync(sAdminEmail, "test", isPersistent: false, shouldLockout: false));
      }
#endif
#endif
    }

    private static async Task loadAsync(string sAppDataDir, int iDelay = 0)
    {
      if (iDelay > 0) await Task.Delay(iDelay);
      load(sAppDataDir);
    }
    /*
    private static void timerLoad_Tick(object sender, EventArgs e)
    {
      try {
        if (CkAppShared.timerLoad != null) CkAppShared.timerLoad.Stop();
      } catch {
      }

      string sAppDataDir = ((CkAppShared.TimerLoad)sender).sAppDataDir;

      load(sAppDataDir);
    }
    */
#endif

    private static void fillLeaguesWithCpuClubs(CornerkickManager.Main ckMngTmp, CornerkickManager.Cup league, CornerkickManager.Cup cupAdd, byte nLeagueSize = 16)
    {
      fillLeaguesWithCpuClubs(ckMngTmp, league, new List<CornerkickManager.Cup>() { cupAdd }, nLeagueSize: nLeagueSize);
    }
    private static void fillLeaguesWithCpuClubs(CornerkickManager.Main ckMngTmp, CornerkickManager.Cup league, List<CornerkickManager.Cup> ltCupsAdd, byte nLeagueSize = 16)
    {
      int iC = 0;
      while (league.ltClubs[0].Count < nLeagueSize) {
        string sTeamNameLand = "Land_" + league.iId2.ToString();
        if (CornerkickManager.Main.sLandShort != null && CornerkickManager.Main.sLandShort.Length > league.iId2) sTeamNameLand = CornerkickManager.Main.sLandShort[league.iId2] + "_";
        CornerkickManager.Club? clb = createClub(ckMngTmp, "Team_" + sTeamNameLand + (iC + (league.iId3 * nLeagueSize) + 1).ToString(), (byte)league.iId2, (byte)league.iId3);
        if (clb == null) break;

        int iSkillChange = 0;
#if _WebApp
        iSkillChange = -league.iId3;
#endif
        addPlayerToClub(ckMngTmp, ref clb, iSkillChange: iSkillChange, bInitial: true);

        ckMngTmp.ltClubs.Add(clb);

        foreach (CornerkickManager.Cup c in ltCupsAdd) c.ltClubs[0].Add(clb);
        league.ltClubs[0].Add(clb);

        iC++;
      }
    }

    private static void createCupGold(CornerkickManager.Main ckMngTmp)
    {
      CornerkickManager.Cup cupGold = ckMngTmp.tl.getCup(CkAppShared.iCupIdInt, iId2: 0);

      // Create Gold Cup
      if (cupGold == null) {
        cupGold = new CornerkickManager.Cup(bKo: true, bKoTwoGames: true, nGroups: 8, bGroupsTwoGames: true, nQualifierKo: 2);
        ckMngTmp.ltCups.Add(cupGold);
      }

      cupGold.iId = CkAppShared.iCupIdInt;
      cupGold.iId2 = 0;
      cupGold.sName = "Gold Cup";
      cupGold.settings.iNeutral = 2;
      cupGold.settings.iBonusStart = 10000000; // 10 mio.
      cupGold.settings.iBonusCupWin = 16000000; // 16 mio.
      cupGold.settings.iBonusVicGroup = 5000000; //  5 mio.
      cupGold.settings.bBonusReleaseCupWinInKo = true;
      cupGold.settings.iDayOfWeek = 3;
      cupGold.settings.tsTimeOfDay = new TimeSpan(20, 45, 00);
      cupGold.settings.fAttraction = 1.25f;
      cupGold.settings.iTvBonus = 300000;

      // Add qualifications
      cupGold.ltQualification = new List<CornerkickManager.Cup.Qualification>();
      foreach (byte iN in CkAppShared.iNations) {
        CornerkickManager.Cup league = ckMngTmp.tl.getCup(CkAppShared.iCupIdLeague, iId2: iN, iId3: 0);
        if (league != null) cupGold.ltQualification.Add(new CornerkickManager.Cup.Qualification() { iPlaceFirst = 1, iPlaceLast = 4, cup = league });
      }
    }

    private static void createCupSilver(CornerkickManager.Main ckMngTmp)
    {
      CornerkickManager.Cup cupSilver = ckMngTmp.tl.getCup(CkAppShared.iCupIdInt, iId2: 1);

      // Create Silver Cup
      if (cupSilver == null) {
        cupSilver = new CornerkickManager.Cup(bKo: true, bKoTwoGames: true, nGroups: 8, bGroupsTwoGames: true, nQualifierKo: 2);
        ckMngTmp.ltCups.Add(cupSilver);
      }

      cupSilver.iId = CkAppShared.iCupIdInt;
      cupSilver.iId2 = 1;
      cupSilver.sName = "Silver Cup";
      cupSilver.settings.iNeutral = 2;
      cupSilver.settings.iBonusStart = 7500000; //  7.5 mio.
      cupSilver.settings.iBonusCupWin = 12000000; // 12 mio.
      cupSilver.settings.iBonusVicGroup = 3500000; //  3.5 mio.
      cupSilver.settings.bBonusReleaseCupWinInKo = true;
      cupSilver.settings.iDayOfWeek = 4;
      cupSilver.settings.tsTimeOfDay = new TimeSpan(20, 45, 00);
      cupSilver.settings.fAttraction = 1.00f;
      cupSilver.settings.iTvBonus = 200000;

      // Add qualifications
      cupSilver.ltQualification = new List<CornerkickManager.Cup.Qualification>();
      foreach (byte iN in CkAppShared.iNations) {
        CornerkickManager.Cup league = ckMngTmp.tl.getCup(CkAppShared.iCupIdLeague, iId2: iN, iId3: 0);
        if (league != null) cupSilver.ltQualification.Add(new CornerkickManager.Cup.Qualification() { iPlaceFirst = 5, iPlaceLast = 8, cup = league });
      }
    }

    private static void createCupBronze(CornerkickManager.Main ckMngTmp)
    {
      CornerkickManager.Cup cupBronze = ckMngTmp.tl.getCup(CkAppShared.iCupIdInt, iId2: 2);

      // Create Bronze Cup
      if (cupBronze == null) {
        cupBronze = new CornerkickManager.Cup(bKo: true, bKoTwoGames: true, nGroups: 8, bGroupsTwoGames: true, nQualifierKo: 2);
        ckMngTmp.ltCups.Add(cupBronze);
      }

      cupBronze.iId = CkAppShared.iCupIdInt;
      cupBronze.iId2 = 2;
      cupBronze.sName = "Bronze Cup";
      cupBronze.settings.iNeutral = 2;
      cupBronze.settings.iBonusStart = 5000000; //  5 mio.
      cupBronze.settings.iBonusCupWin = 8000000; // 8 mio.
      cupBronze.settings.iBonusVicGroup = 2500000; //  2.5 mio.
      cupBronze.settings.bBonusReleaseCupWinInKo = true;
      cupBronze.settings.iDayOfWeek = 3;
      cupBronze.settings.tsTimeOfDay = new TimeSpan(21, 00, 00);
      cupBronze.settings.fAttraction = 0.80f;
      cupBronze.settings.iTvBonus = 100000;

      // Add qualifications
      cupBronze.ltQualification = new List<CornerkickManager.Cup.Qualification>();
      foreach (byte iN in CkAppShared.iNations) {
        CornerkickManager.Cup league = ckMngTmp.tl.getCup(CkAppShared.iCupIdLeague, iId2: iN, iId3: 0);
        if (league != null) cupBronze.ltQualification.Add(new CornerkickManager.Cup.Qualification() { iPlaceFirst = 9, iPlaceLast = 12, cup = league });
      }
    }

    private static void createCupWc(CornerkickManager.Main ckMngTmp, DateTime dtLeagueEnd)
    {
      CornerkickManager.Cup cupWc = ckMngTmp.tl.getCup(CkAppShared.iCupIdWc);

      if (cupWc == null) {
        cupWc = new CornerkickManager.Cup(bKo: true, bKoTwoGames: false, nGroups: 2, bGroupsTwoGames: false, nQualifierKo: 2);
        cupWc.iId = CkAppShared.iCupIdWc;
        cupWc.sName = "Weltmeisterschaft";
        cupWc.settings.iNeutral = 1;
        /*
        cupWc.settings.dtStart = dtLeagueEnd.Date + new TimeSpan(20, 30, 00);
        cupWc.settings.dtEnd = ckMngTmp.dtSeasonEnd.AddDays(-1).Date + new TimeSpan(20, 00, 00);
        */
        //cupWc.settings.iStart = (sbyte)-((ckMngTmp.dtSeasonEnd.Date - dtLeagueEnd.Date).TotalDays * 7);
        cupWc.settings.iStart = 0;
        cupWc.settings.iEnd = 0;
        cupWc.settings.nYears = 2;
        cupWc.settings.tsTimeOfDay = new TimeSpan(20, 00, 00);
        cupWc.settings.fAttraction = 1.50f;
        ckMngTmp.ltCups.Add(cupWc);

        int iGroup = 0;
        foreach (byte iN in CkAppShared.iNations) {
          CornerkickManager.Club clbNat = createNation(ckMngTmp, iN);

          ckMngTmp.ltClubs.Add(clbNat);

          cupWc.ltClubs[iGroup / 4].Add(clbNat);
          iGroup++;
        }
      }
    }

    public static CornerkickManager.Club createNation(CornerkickManager.Main ckMngTmp, int iNat)
    {
      CornerkickManager.Club clbNat = new CornerkickManager.Club();
      clbNat.bNation = true;
      //clbNat.iId = ckMngTmp.ltClubs.Count;
      clbNat.iId = Tool.getFirstAvailable(ckMngTmp.ltClubs.Select(c => c.iId).ToList());
      clbNat.sName = CornerkickManager.Main.sLand != null && CornerkickManager.Main.sLand.Length > iNat ? CornerkickManager.Main.sLand[iNat] : "Land_" + iNat.ToString();
      clbNat.iLand = iNat;

      //clbNat.ltTactic[0].formation = ckMngTmp.ltFormationen[8];
      clbNat.ltTactic[0].formation = ckMngTmp.ltFormationen[CkAppShared.random.Next(ckMngTmp.ltFormationen.Count)].Clone();

      // Nat. staff
      clbNat.staff.iCondiTrainer = 6;
      clbNat.staff.iPhysio = 6;
      clbNat.staff.iMentalTrainer = 6;
      clbNat.staff.ltDoctor.Add(new CornerkickManager.Main.Staff.Doctor() { sName = "Dr. Müller-Wohllaib", iId = -6, iSkillMuscle = 6, iSkillTendons = 6, iSkillFracture = 6, iSkillInternist = 6 });
      clbNat.staff.ltDoctor.Add(new CornerkickManager.Main.Staff.Doctor() { sName = "Dr. Müller-Wohllaib", iId = -6, iSkillMuscle = 6, iSkillTendons = 6, iSkillFracture = 6, iSkillInternist = 6 });
      clbNat.staff.ltDoctor.Add(new CornerkickManager.Main.Staff.Doctor() { sName = "Dr. Müller-Wohllaib", iId = -6, iSkillMuscle = 6, iSkillTendons = 6, iSkillFracture = 6, iSkillInternist = 6 });
      clbNat.staff.iKibitzer = 3;

      // Nat. buildings
      clbNat.buildings.bgTrainingCourts.iLevel = 5;
      clbNat.buildings.bgGym.iLevel = 5;
      clbNat.buildings.bgSpa.iLevel = 5;

      ckMngTmp.doFormation(clbNat);

      setNationColors(clbNat);

      return clbNat;
    }

    private static void setNationColors(CornerkickManager.Club clbNat)
    {
      if (CornerkickManager.Main.clNat1[clbNat.iLand] != null && CornerkickManager.Main.clNat1[clbNat.iLand].Length > 2) {
        clbNat.cl1 = CornerkickManager.Main.clNat1[clbNat.iLand];
      }
      if (CornerkickManager.Main.clNat2[clbNat.iLand] != null && CornerkickManager.Main.clNat2[clbNat.iLand].Length > 2) {
        clbNat.cl2 = CornerkickManager.Main.clNat2[clbNat.iLand];
      }
    }
#if false
    private static void setNationColors(CornerkickManager.Club clbNat)
    {
      /*
      36, // GER
      29, // ENG
      30, // ESP
      45, // ITA
      33, // FRA
      54, // NED
      13, // BRA
       3  // ARG
       */
      if (clbNat.iLand == 36) { // GER
        clbNat.cl1[0] = System.Drawing.Color.White;
        clbNat.cl1[1] = System.Drawing.Color.Black;
        clbNat.cl1[2] = System.Drawing.Color.Black;

        clbNat.cl2[0] = System.Drawing.Color.Red;
        clbNat.cl2[1] = System.Drawing.Color.White;
        clbNat.cl2[2] = System.Drawing.Color.White;
      } else if (clbNat.iLand == 29) { // ENG
        clbNat.cl1[0] = System.Drawing.Color.White;
        clbNat.cl1[1] = System.Drawing.Color.FromArgb(15, 28, 115); // Blue
        clbNat.cl1[2] = System.Drawing.Color.FromArgb(15, 28, 115); // Blue

        clbNat.cl2[0] = System.Drawing.Color.FromArgb(255, 0, 0); // Red
        clbNat.cl2[1] = System.Drawing.Color.White;
        clbNat.cl2[2] = System.Drawing.Color.White;
      } else if (clbNat.iLand == 30) { // ESP
        clbNat.cl1[0] = System.Drawing.Color.FromArgb(255, 0, 0); // Red
        clbNat.cl1[1] = System.Drawing.Color.FromArgb(22, 70, 151); // Blue
        clbNat.cl1[2] = System.Drawing.Color.FromArgb(22, 70, 151); // Blue

        clbNat.cl2[0] = System.Drawing.Color.White;
        clbNat.cl2[1] = System.Drawing.Color.White;
        clbNat.cl2[2] = System.Drawing.Color.FromArgb(255, 0, 0); // Red
      } else if (clbNat.iLand == 45) { // ITA
        clbNat.cl1[0] = System.Drawing.Color.FromArgb(17, 62, 215); // Azure-blue
        clbNat.cl1[1] = System.Drawing.Color.White;
        clbNat.cl1[2] = System.Drawing.Color.White;

        clbNat.cl2[0] = System.Drawing.Color.White;
        clbNat.cl2[1] = System.Drawing.Color.FromArgb(17, 62, 215); // Azure-blue
        clbNat.cl2[2] = System.Drawing.Color.FromArgb(17, 62, 215); // Azure-blue
      } else if (clbNat.iLand == 33) { // FRA
        clbNat.cl1[0] = System.Drawing.Color.FromArgb(17, 40, 85); // Blue
        clbNat.cl1[1] = System.Drawing.Color.White;
        clbNat.cl1[2] = System.Drawing.Color.White;

        clbNat.cl2[0] = System.Drawing.Color.White;
        clbNat.cl2[1] = System.Drawing.Color.FromArgb(17, 40, 85); // Blue
        clbNat.cl2[2] = System.Drawing.Color.FromArgb(17, 40, 85); // Blue
      } else if (clbNat.iLand == 54) { // NED
        clbNat.cl1[0] = System.Drawing.Color.FromArgb(255, 79, 0); // Orange
        clbNat.cl1[1] = System.Drawing.Color.FromArgb(255, 79, 0);
        clbNat.cl1[2] = System.Drawing.Color.White;

        clbNat.cl2[0] = System.Drawing.Color.White;
        clbNat.cl2[1] = System.Drawing.Color.FromArgb(255, 79, 0);
        clbNat.cl2[2] = System.Drawing.Color.FromArgb(255, 79, 0);
      } else if (clbNat.iLand == 10) { // BEL
        clbNat.cl1[0] = System.Drawing.Color.FromArgb(221, 27, 31); // Red
        clbNat.cl1[1] = System.Drawing.Color.Black;
        clbNat.cl1[2] = System.Drawing.Color.FromArgb(253, 213, 53); // Gold

        clbNat.cl2[0] = System.Drawing.Color.FromArgb(152, 236, 253); // Light-blue
        clbNat.cl2[1] = System.Drawing.Color.White;
        clbNat.cl2[2] = System.Drawing.Color.White;
      } else if (clbNat.iLand == 13) { // BRA
        clbNat.cl1[0] = System.Drawing.Color.FromArgb(255, 229, 0); // Yellow
        clbNat.cl1[1] = System.Drawing.Color.FromArgb(0, 60, 255); // Blue
        clbNat.cl1[2] = System.Drawing.Color.FromArgb(0, 60, 255); // Blue

        clbNat.cl2[0] = System.Drawing.Color.FromArgb(0, 60, 255); // Blue
        clbNat.cl2[1] = System.Drawing.Color.White;
        clbNat.cl2[2] = System.Drawing.Color.White;
      } else if (clbNat.iLand == 3) { // ARG
        clbNat.cl1[0] = System.Drawing.Color.FromArgb(214, 237, 255); // Light-blue
        clbNat.cl1[1] = System.Drawing.Color.White;
        clbNat.cl1[2] = System.Drawing.Color.White;

        clbNat.cl2[0] = System.Drawing.Color.FromArgb(0, 53, 94);
        clbNat.cl2[1] = System.Drawing.Color.FromArgb(214, 237, 255); // Light-blue
        clbNat.cl2[2] = System.Drawing.Color.White;
      }
    }
#endif

    internal static DateTime getWcNominationDeadline(CornerkickManager.Cup cupWc)
    {
      if (cupWc == null) return new DateTime();
      if (cupWc.ltMatchdays == null) return new DateTime();
      if (cupWc.ltMatchdays.Count == 0) return new DateTime();

      //return cupWc.ltMatchdays[0].dt.Date;
      return cupWc.ltMatchdays[0].dt.Date.AddDays(-6);
    }

#if _WebApp
    internal static int getStepsFromTargetToApproach()
    {
      return (int)((getCkTargetDate() - getCkApproachDate()).TotalMinutes / 15.0);
    }

    internal static DateTime getCkTargetDate()
    {
      DateTime dtCkTarget = CkAppShared.ckMng.dtDatum.Date.Add(new TimeSpan(15, 30, 0));

      // If saturday and after target time --> add one day
      if ((int)CkAppShared.ckMng.dtDatum.DayOfWeek == 6 && CkAppShared.ckMng.dtDatum.TimeOfDay.CompareTo(new TimeSpan(15, 30, 0)) > 0) dtCkTarget = dtCkTarget.AddDays(1);

      while ((int)dtCkTarget.DayOfWeek != 6) dtCkTarget = dtCkTarget.AddDays(1);

      return dtCkTarget;
    }

    static TimeSpan tsTarget = new TimeSpan(20, 30, 0); // Target system time
    internal static DateTime getCkApproachDate()
    {
      double fDayRel = getDayRelBetweenNowAndTarget();

      // Get target ck date
      DateTime dtCkTarget = getCkTargetDate();

      return dtCkTarget.AddDays(-fDayRel * 7);
    }

    internal static double getDayRelBetweenNowAndTarget()
    {
      // German (and France) time:
      var euTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
      DateTime euTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, euTimeZone);

      double fDayRel = (tsTarget - euTime.TimeOfDay).TotalDays;
      if (fDayRel < 0) fDayRel += 1.0;

      return fDayRel;
    }

    internal static int getDeltaStepsBetweenNowAndApproach()
    {
      DateTime dtCkApproach = getCkApproachDate();
      return (int)((dtCkApproach - CkAppShared.ckMng.dtDatum).TotalMinutes / 15.0);
    }

    internal static int getDeltaStepsBetweenNowAndTarget()
    {
      DateTime dtCkTarget = getCkTargetDate();
      return (int)((dtCkTarget - CkAppShared.ckMng.dtDatum).TotalMinutes / 15.0);
    }

    internal static double getIntervalForOneWeek()
    {
      // Capital letters = Real-time
      //     MIN * S  * H   /  qu  * h  * d
      return (60 * 60 * 24) / (4.0 * 24 * 7);
    }

    internal static double getIntervalAve()
    {
      double fDayRel = getDayRelBetweenNowAndTarget();
      int iStepsDelta = getDeltaStepsBetweenNowAndTarget();
      return (fDayRel * 24 * 60 * 60) / iStepsDelta;
    }

    internal static TimeSpan getApproachTime()
    {
      return tsTarget.Add(TimeSpan.FromSeconds(getStepsFromTargetToApproach() * getIntervalForOneWeek()));
    }

    private static void timerCkCalender_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      // Disable save timer (not needed if calendar timer is on)
      CkAppShared.timerSave.Enabled = false;

      if (CkAppShared.timerCkCalender.Interval < 1000) CkAppShared.timerCkCalender.Enabled = false;

      CkAppShared.timerCkCalender.Enabled = performCalendarStep();
    }

    private static void timerSave_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      save(bForce: true);
    }

    public static bool performCalendarStep(bool bSave = true)
    {
      CornerkickManager.Cup cupGold   = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdInt, iId2: 0);
      CornerkickManager.Cup cupSilver = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdInt, iId2: 1);
      CornerkickManager.Cup cupBronze = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdInt, iId2: 2);

      CornerkickManager.Club? clb0 = CkAppShared.ckMng.ltClubs.Find(c => c.iId == 0);

      // Reset home dir
      string sHomeDir = Path.Combine(CkAppShared.sHomeDir);
      if (sHomeDir != null && !sHomeDir.Equals(CkAppShared.ckMng.settings.sHomeDir)) {
        CkAppShared.ckMng.tl.writeLog("Reset ck home dir from " + CkAppShared.ckMng.settings.sHomeDir + " to " + sHomeDir);
        CkAppShared.ckMng.settings.sHomeDir = sHomeDir;
      }

      if (CkAppShared.ckMng.ltUser.Count == 0) return true;

      if (CkAppShared.settings.iStartHour >= 0 && CkAppShared.settings.iStartHour <= 24) {
        if (DateTime.Now.Hour != CkAppShared.settings.iStartHour && DateTime.Now.Hour > 13) {
          if (CkAppShared.ckMng.dtDatum.Equals(CkAppShared.ckMng.dtSeasonStart) ||
             ((int)CkAppShared.ckMng.dtDatum.DayOfWeek == 1 && CkAppShared.ckMng.dtDatum.Hour == 0 && CkAppShared.ckMng.dtDatum.Minute == 0)) {
            return true;
          }
        }
      }

      if (CkAppShared.ckMng.dtDatum.Hour == 0 && CkAppShared.ckMng.dtDatum.Minute == 0 && CkAppShared.ckMng.dtDatum.Second == 0) {
        // Put player from cpu club on transferlist if too many
        const int iClubCpuPlayerMax = 25;
        const int iClubCpuPlayerMin = 16;
        for (int iC = 1; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
          CornerkickManager.Club clbCpu = CkAppShared.ckMng.ltClubs[iC];
          if (clbCpu.user != null) continue;
          if (clbCpu.bNation) continue;
          if (clbCpu.ltPlayer.Count <= iClubCpuPlayerMax) continue;

          CkAppShared.ckMng.doFormation(clbCpu);

          for (int iP = iClubCpuPlayerMax; iP < clbCpu.ltPlayer.Count; iP++) {
            CornerkickManager.Club? clbCpuTake = null;

            // Find cpu club with to few players
            for (int jC = 1; jC < CkAppShared.ckMng.ltClubs.Count; jC++) {
              if (iC == jC) continue;

              CornerkickManager.Club clbCpuTakeTmp = CkAppShared.ckMng.ltClubs[jC];
              if (clbCpuTakeTmp.user != null) continue;
              if (clbCpuTakeTmp.bNation) continue;

              if (clbCpuTakeTmp.ltPlayer.Count < iClubCpuPlayerMin) {
                clbCpuTake = clbCpuTakeTmp;
                break;
              }
            }

            CkAppShared.ckMng.tr.putPlayerOnTransferlist(clbCpu.ltPlayer[iP], 0);

            if (clbCpuTake != null) {
              CkAppShared.ckMng.tr.transferPlayer(clbCpu.ltPlayer[iP], clbCpuTake, bForce: true);
            }
            /*
            int jP = iP;
            while (CkAppShared.ckMng.tr.putPlayerOnTransferlist(clbCpu.ltPlayer[jP], 0) != 1 && jP > 0) jP--;

            if (clbCpuTake != null) {
              CkAppShared.ckMng.tr.transferPlayer(clbCpu, clbCpu.ltPlayer[jP], clbCpuTake);
            }
            */
          }
        }

        // Check if new jouth player and put on transferlist
        CornerkickManager.Player plNew = CkAppShared.ckMng.plt.newPlayer(clb0, iNat: CkAppShared.iNations[CkAppShared.random.Next(CkAppShared.iNations.Length)]);
        CkAppShared.ckMng.tr.putPlayerOnTransferlist(plNew, 0);

        // Player jouth
        foreach (CornerkickManager.Player plJ in clb0.ltPlayerJouth) {
          CkAppShared.ckMng.tr.putPlayerOnTransferlist(plJ, 0);
        }

        /*
        if (countCpuPlayerOnTransferlist() > 200) {
          for (int iT = 0; iT < CkAppShared.ckMng.ltTransfer.Count; iT++) {
            CornerkickManager.Transfer.Item transfer = CkAppShared.ckMng.ltTransfer[iT];
            if (club0.ltPlayer.IndexOf(transfer.player) >= 0) {
              CkAppShared.ckMng.tr.removePlayerFromTransferlist(transfer.player);
              break;
            }
          }
        }
        */

        // retire cpu player
        if (clb0.ltPlayer.Count > 1500) {
          CkAppShared.ckMng.plt.retirePlayer(clb0.ltPlayer[0], clb0);
        }

        //checkCpuJouth();
      } // If midnight

      // Save .autosave
      if (bSave && CkAppShared.ckMng.dtDatum.Minute == 0 && CkAppShared.ckMng.dtDatum.Hour % 2 == 0) {
        save(CkAppShared.timerCkCalender.Interval);
      }

      if ((CkAppShared.ckMng.dtDatum.Equals(CkAppShared.ckMng.dtSeasonStart) || CkAppShared.ckMng.dtDatum.Year < 1900) &&
          CkAppShared.ckMng.iSeason == 0) {
        CkAppShared.ltLog.Clear();
        CkAppShared.ckMng.setNewSeason();
      }

      // Do next step
      List<CornerkickManager.Main.NextReturn> ltRetCk = new List<CornerkickManager.Main.NextReturn>();
      try {
#if _WebApp
        ltRetCk = CkAppShared.ckMng.next(bForce: true);
#else
        ltRetCk = CkAppShared.ckMng.next();
#endif
      } catch (Exception e) {
        CkAppShared.ckMng.tl.writeLog("performCalendarStep(): Error in ck next()" + Environment.NewLine + e.Message + e.StackTrace, CornerkickManager.Main.sErrorFile);
      }

      // Reset CPU player
      if (CkAppShared.ckMng.dtDatum.TimeOfDay.Equals(new TimeSpan(15, 0, 0))) {
        for (int iC = 0; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
          CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];
          if (clb.user != null) continue;

          float fContiCPU = 0.9f;
          float fFreshCPU = 1.0f;
          if (clb.iDivision > 0) {
            fContiCPU = 0.65f;
            fFreshCPU = 0.80f;
          }
          for (int iP = 0; iP < clb.ltPlayer.Count; iP++) {
            clb.ltPlayer[iP].plGame.fCondition = fContiCPU;
            clb.ltPlayer[iP].plGame.fFresh = fFreshCPU;
            clb.ltPlayer[iP].plGame.fMoral = Math.Max(clb.ltPlayer[iP].plGame.fMoral, 0.95f);
          }
        }
      }

      // Reset player moral if ...
      for (int iPl = 0; iPl < CkAppShared.ckMng.ltPlayer.Count; iPl++) {
        CornerkickManager.Player pl = CkAppShared.ckMng.ltPlayer[iPl];

        try {
          string sClubName = pl.contract?.club != null ? pl.contract.club.sName : "vereinslos";

          // ... NaN
          if (float.IsNaN(pl.plGame.fMoral)) {
            pl.plGame.fMoral = 1f;
            CkAppShared.ckMng.tl.writeLog("Reset moral (NaN) of player " + pl.plGame.sName + ", id: " + iPl.ToString() + ", club: " + sClubName, CornerkickManager.Main.sErrorFile);
          }

          // ... below minimum
          if (pl.plGame.fMoral < CkAppShared.ckMng.settings.fMoralMin) {
            CkAppShared.ckMng.tl.writeLog("Reset moral (" + pl.plGame.fMoral.ToString("0.0%") + ") of player " + pl.plGame.sName + ", id: " + iPl.ToString() + ", club: " + sClubName, CornerkickManager.Main.sErrorFile);
            pl.plGame.fMoral = CkAppShared.ckMng.settings.fMoralMin;
          }
        } catch (Exception e) {
          CkAppShared.ckMng.tl.writeLog(pl.plGame.sName + ", id: " + iPl.ToString() + ": " + e.Message + Environment.NewLine + e.StackTrace, CornerkickManager.Main.sErrorFile);
        }
      }

      // CHEAT-SECTION
      // Jan no injury
      try {
        CornerkickManager.Player plJan = CkAppShared.ckMng.ltPlayer.Find(p => p.plGame.sName.Equals("Jan Suleyman"));
        if (plJan != null) plJan.plGame.injury = null;
      } catch {
      }
#if false
      CornerkickManager.Player plAaronSchulz = CkAppShared.ckMng.ltPlayer[3029];
      plAaronSchulz.plGame.injury = null;

      // Janos/David money
      if (CkAppShared.ckMng.dtDatum.Day == 1 && CkAppShared.ckMng.dtDatum.Hour == 20 && CkAppShared.ckMng.dtDatum.Minute == 0 && CkAppShared.ckMng.dtDatum.Second == 0) {
        CornerkickManager.Club clbJanos = CkAppShared.ckMng.ltClubs[5];
        CornerkickManager.Club clbDavid = CkAppShared.ckMng.ltClubs[143];

        int iBalanceMin = 10000000;
        foreach (CornerkickManager.Club clbBalance in new CornerkickManager.Club[] { clbJanos, clbDavid }) {
          if (clbBalance.iBalance < iBalanceMin) {
            int iDeltaBalance = iBalanceMin - clbBalance.iBalance;
            CornerkickManager.Finance.doTransaction(clbBalance, CkAppShared.ckMng.dtDatum, iDeltaBalance, 0, "Ausgleich");
          }
        }
      }
      // END OF CHEAT-SECTION
#endif

      // Assign random portrait if none
      for (int iPl = 0; iPl < CkAppShared.ckMng.ltPlayer.Count; iPl++) {
        setPlayerRandomPortrait(CkAppShared.ckMng.ltPlayer[iPl]);
      }

      // Set training template if user has set no training
      if (CkAppShared.ckMng.dtDatum.DayOfWeek == DayOfWeek.Monday && CkAppShared.ckMng.dtDatum.Hour == 0 && CkAppShared.ckMng.dtDatum.Minute == 0) {
        foreach (CornerkickManager.Club clb in CkAppShared.ckMng.ltClubs) {
          if (clb.user == null) continue;

          bool bTraining = false;

          CornerkickManager.Main.TrainingPlan.Unit[][] tpWeek = Member.TrainingController.getTrainingPlan(clb.user, 0).Result;
          foreach (CornerkickManager.Main.TrainingPlan.Unit[] tpDay in tpWeek) {
            foreach (CornerkickManager.Main.TrainingPlan.Unit tu in tpDay) {
              if (tu.iType > 0 && tu.iType < 100) {
                bTraining = true;
                break;
              }
            }
            if (bTraining) break;
          }

          if (!bTraining) {
            Member.TrainingController.setTrainingWeekTemplate(clb.user, 0, 2);
          }
        }
      }

      /*
      // Beginn of new season
      if (CornerkickManager.Main.checkNextReturn(4, ltRetCk)) {
        // Draw leagues
        foreach (int iN in iNations) {
          for (int iDiv = 0; iDiv < 2; iDiv++) {
            CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(1, iN, iDiv);
            if (league == null) continue;
            league.draw(CkAppShared.ckMng.dtDatum);
          }
        }

        // Draw gold/silver cup
        cupGold.draw(CkAppShared.ckMng.dtDatum);
        cupSilver.draw(CkAppShared.ckMng.dtDatum);
        cupBronze.draw(CkAppShared.ckMng.dtDatum);

        // Set club next game
        foreach (CornerkickManager.Club clb in CkAppShared.ckMng.ltClubs) {
          clb.nextGame = CkAppShared.ckMng.tl.getNextGame(clb, CkAppShared.ckMng.dtDatum);
        }
      }
      */

      // End of season
      if (CornerkickManager.Main.checkNextReturn(99, ltRetCk)) {
        // Clear national coaches
        clearNations();

        /*
        //////////////////////////////////////////////////
        // Nominate clubs to international cups
        //////////////////////////////////////////////////
        // Clear groups
        for (byte iG = 0; iG < cupGold  .ltClubs.Length; iG++) cupGold  .ltClubs[iG].Clear();
        for (byte iG = 0; iG < cupSilver.ltClubs.Length; iG++) cupSilver.ltClubs[iG].Clear();
        for (byte iG = 0; iG < cupBronze.ltClubs.Length; iG++) cupBronze.ltClubs[iG].Clear();

        // Add clubs ...
        int iGroupGold = 0;
        int iGroupSilver = 0;
        int iGroupBronze = 0;
        foreach (int iN in iNations) {
          // ... of league iN ...
          CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(1, iN, 0);
          if (league == null) continue;

          List<CornerkickManager.Cup.TableItem> ltTbl = league.getTable();

          // ... to Gold Cup
          for (byte jL = 0; jL < 4; jL++) {
            if (iGroupGold >= cupGold.ltClubs.Length) iGroupGold = 0;
            cupGold.ltClubs[iGroupGold].Add(ltTbl[jL].club);
            iGroupGold++;
          }

          // ... to Silver Cup
          for (byte jL = 4; jL < 8; jL++) {
            if (iGroupSilver >= cupSilver.ltClubs.Length) iGroupSilver = 0;
            cupSilver.ltClubs[iGroupSilver].Add(ltTbl[jL].club);
            iGroupSilver++;
          }

          // ... to Bronze Cup
          for (byte jL = 8; jL < 12; jL++) {
            if (iGroupBronze >= cupBronze.ltClubs.Length) iGroupBronze = 0;
            cupBronze.ltClubs[iGroupBronze].Add(ltTbl[jL].club);
            iGroupBronze++;
          }
        }

        cupGold.ltMatchdays.Clear();
        cupSilver.ltMatchdays.Clear();
        cupBronze.ltMatchdays.Clear();

        CkAppShared.ckMng.calcMatchdays();
        */

        return false;
      }

      // Remove testgame requests if in past
      List<CornerkickManager.Cup> ltCupsTmp = new List<CornerkickManager.Cup>(CkAppShared.ckMng.ltCups);
      foreach (CornerkickManager.Cup cup in ltCupsTmp) {
        if (cup == null) continue;

        if (cup.iId == -CkAppShared.iCupIdTestgame) {
          if (cup.ltMatchdays.Count < 1) continue;

          if (cup.ltMatchdays[0].dt.CompareTo(CkAppShared.ckMng.dtDatum) <= 0) { // if request in past or now ...
            CkAppShared.ckMng.ltCups.Remove(cup); // ... remove cup
          }
        }
      }

      // Inform user if transfer offer too low
      if (CkAppShared.ckMng.dtDatum.TimeOfDay.Equals(new TimeSpan(12, 00, 00))) {
        // For each transfer
        foreach (CornerkickManager.Transfer.Item transfer in CkAppShared.ckMng.ltTransfer) {
          if (transfer.player == null) continue;
          if (transfer.player.contract == null) continue;

          CornerkickManager.Club clbCpu = transfer.player.contract.club;
          if (CkAppShared.ckMng.ltUser.IndexOf(clbCpu.user) == 0) continue; // If main CPU user

          // If no offer yet --> make cpu offer
          if (transfer.ltOffers.Count == 0) {
            if (CkAppShared.random.Next(4) == 0) { // Each 4th day
              int iFeeCpu0 = (int)(1000 * transfer.player.getValue(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd) * (0.4 + (0.5 * CkAppShared.random.NextDouble())));
              CornerkickManager.Transfer.makeTransferOffer(transfer, iFeeCpu0, CornerkickManager.PlayerTool.getContract(transfer.player, (byte)CkAppShared.random.Next(1, 5), clb0, CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd), CkAppShared.ckMng.dtDatum);

              continue;
            }
          }

          if (clbCpu.user != null) continue; // If human user

          // Get max offer
          int iOfferMax = 0;
          foreach (CornerkickManager.Transfer.Offer offer in transfer.ltOffers) {
            iOfferMax = Math.Max(iOfferMax, offer.iFee);
          }

          // Inform users
          if (iOfferMax > 0) {
            foreach (CornerkickManager.Transfer.Offer offer in transfer.ltOffers) {
              if (offer.iFee > 0 && offer.iFee < iOfferMax) {
                CkAppShared.ckMng.sendNews(offer.contract.club.user, "Ihr Transferangebot für den Spieler " + transfer.player.plGame.sName + " ist leider nicht (mehr) hoch genug.", CornerkickManager.Main.iNewsTypePlayerTransferOfferOutbid);
              }
            }
          }
        }
      }

      // Nominate user for WC
      DateTime dtWcStart = new DateTime();
      bool bReturn = true;

      foreach (int iN in CkAppShared.iNations) {
        CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, iN, 0);
        if (league == null) continue;
        if (league.ltMatchdays == null) continue;
        if (league.ltMatchdays.Count == 0) continue;

        dtWcStart = new DateTime(Math.Max(league.ltMatchdays[league.ltMatchdays.Count - 1].dt.Ticks, dtWcStart.Ticks));
      }

      if (cupGold != null) {
        if (cupGold.ltMatchdays != null && cupGold.ltMatchdays.Count > 0) {
          if (cupGold.ltMatchdays[cupGold.ltMatchdays.Count - 1].ltGameData != null) {
            if (cupGold.ltMatchdays[cupGold.ltMatchdays.Count - 1].ltGameData.Count == 1) {
              dtWcStart = new DateTime(Math.Max(cupGold.ltMatchdays[cupGold.ltMatchdays.Count - 1].dt.Ticks, dtWcStart.Ticks));
            }
          }
        }
      }

      // Pay cooperation money
      if (CkAppShared.ckMng.dtDatum.Day == 1 && CkAppShared.ckMng.dtDatum.TimeOfDay.Equals(new TimeSpan(10, 00, 00))) {
        foreach (CornerkickManager.User uCoop1 in CkAppShared.ckMng.ltUser) {
          if (uCoop1 == null) continue;
          if (uCoop1.club == null) continue;
          if (uCoop1.club.bNation) continue;

          foreach (CornerkickManager.User uCoop2 in CkAppShared.ckMng.ltUser) {
            if (uCoop2 == null) continue;
            if (uCoop2.club == null) continue;
            if (uCoop2.club.bNation) continue;
            if (uCoop1.id.Equals(uCoop2.id)) continue;

            // If usr was invited by u or u was invited by usr
            if (uCoop2.lts != null && uCoop2.lts.Count > UserOptionsModel.iUserOptionsStrIxInvitedById && uCoop2.lts[UserOptionsModel.iUserOptionsStrIxInvitedById].Equals(uCoop1.id)) {
              int iEarnings1 = getCooperationIncome(uCoop2.club.getAttractionFactor(CkAppShared.ckMng.iSeason, ltCups: CkAppShared.ckMng.ltCups, dtNow: CkAppShared.ckMng.dtDatum), true);
              int iEarnings2 = getCooperationIncome(uCoop1.club.getAttractionFactor(CkAppShared.ckMng.iSeason, ltCups: CkAppShared.ckMng.ltCups, dtNow: CkAppShared.ckMng.dtDatum), false);

              CornerkickManager.Finance.doTransaction(uCoop1.club, CkAppShared.ckMng.dtDatum, iEarnings1, CornerkickManager.Finance.iTransferralTypeInGenericA, "Erträge Kooperation " + uCoop2.club.sName);
              CornerkickManager.Finance.doTransaction(uCoop2.club, CkAppShared.ckMng.dtDatum, iEarnings2, CornerkickManager.Finance.iTransferralTypeInGenericA, "Erträge Kooperation " + uCoop1.club.sName);
            }
          }
        }
      }

      //while (dtWcDraw.DayOfWeek != DayOfWeek.Sunday) dtWcDraw = dtWcDraw.AddDays(1);
      dtWcStart = dtWcStart.Date.AddDays(1).AddHours(12);

      CornerkickManager.Cup cupWc = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdWc);

      const int nPlayerNat = 23;
      DateTime dtWcSelectPlayerFinish = new DateTime();
      if (cupWc != null) {
        if (CkAppShared.ckMng.dtDatum.Equals(CkAppShared.ckMng.dtSeasonStart) || CkAppShared.ckMng.dtDatum.Equals(CkAppShared.ckMng.dtSeasonStart.AddMinutes(15))) {
          /*
          cupWc.settings.dtStart = dtWcStart.AddDays(13).Date + new TimeSpan(20, 30, 00);
          cupWc.settings.dtEnd = CkAppShared.ckMng.dtSeasonEnd.AddDays(-1).Date + new TimeSpan(20, 00, 00);
          */
          CkAppShared.ckMng.calcMatchdays(cupWc, CkAppShared.ckMng.dtSeasonStart, CkAppShared.ckMng.dtSeasonEnd);
          cupWc.draw(CkAppShared.ckMng.dtDatum);
        }

        // Reduce player for WC / Remove national coaches
        if (cupWc.ltMatchdays != null) {
          if (cupWc.ltMatchdays.Count > 0) {
            // Reduce player for WC
            dtWcSelectPlayerFinish = getWcNominationDeadline(cupWc);

            if (CkAppShared.ckMng.dtDatum.Equals(dtWcSelectPlayerFinish)) {
              // For each national team
              foreach (CornerkickManager.Club nat in CkAppShared.ckMng.ltClubs) {
                if (!nat.bNation) continue;

                if (nat.user != null && nat.ltPlayer.Count < 11) {
                  CkAppShared.ckMng.sendNews(nat.user, "Der Verband von " + nat.sName + " entscheidet sich dann doch für einen anderen Trainer.");
                  nat.user.nation = null;
                  nat.user = null;

                  // Nominate player
                  nat.ltPlayer.Clear();
                  nat.ltPlayer = CkAppShared.ckMng.getBestPlayer(nat.iLand, iPlCount: nPlayerNat);
                }

                while (nat.ltPlayer.Count > nPlayerNat) nat.ltPlayer.RemoveAt(nPlayerNat);

                // Set player no.
                for (int iP = 0; iP < nat.ltPlayer.Count; iP++) nat.ltPlayer[iP].plGame.iNrNat = (byte)(iP + 1);
              }
            }

            // Remove national coaches
            CornerkickManager.Cup.Matchday mdWcFinal = cupWc.ltMatchdays[cupWc.ltMatchdays.Count - 1];
            if (mdWcFinal.ltGameData != null && mdWcFinal.ltGameData.Count == 1 && CkAppShared.ckMng.dtDatum.Equals(mdWcFinal.dt.AddDays(1))) { // Final game
              clearNations();
            }
          }
        }

        // Nominate nation coaches
        if (CkAppShared.ckMng.dtDatum.Equals(CkAppShared.ckMng.dtSeasonStart) || CkAppShared.ckMng.dtDatum.Equals(CkAppShared.ckMng.dtSeasonStart.AddMinutes(15))) {
          /*
          ///////////////////////////////////////////////////////////////////////////////////////////
          // Option A: Select national coaches from winner of leagues
          ///////////////////////////////////////////////////////////////////////////////////////////
          foreach (CornerkickManager.Cup league in CkAppShared.ckMng.ltCups) { // For each 1st division league
            if (league.iId != 1) continue;
            if (league.iId3 > 0) continue;
            if (league.ltMatchdays == null) continue;
            if (league.ltMatchdays.Count < 2) continue;

            List<CornerkickManager.Cup.TableItem> tbl = league.getTable();
            foreach (CornerkickManager.Cup.TableItem item in tbl) {
              if (item.club.user != null) {
                if (CkAppShared.ckMng.ltUser.IndexOf(item.club.user) == 0) continue; // If main CPU user

                CornerkickManager.Club nat = CornerkickManager.Tool.getNation(league.iId2, CkAppShared.ckMng.ltClubs);
                if (nat == null) continue;

                nat.user = item.club.user;
                item.club.user.nation = nat;

                // Inform user
                if (dtWcSelectPlayerFinish.CompareTo(CkAppShared.ckMng.dtDatum) > 0) CkAppShared.ckMng.sendNews(item.club.user, "Bitte wählen Sie noch bis zum " + dtWcSelectPlayerFinish.ToString("d", Controllers.MemberController.getCiStatic(league.iId2)) + " Ihre " + nPlayerNat.ToString() + " Spieler für die Endrunde aus.");
                CkAppShared.ckMng.sendNews(item.club.user, "Welche Ehre! Der Verband von " + nat.sName + " stellt Sie als Nationaltrainer für die kommende WM ein.");

                bReturn = false;
                break;
              }
            }
          }
          */

          ///////////////////////////////////////////////////////////////////////////////////////////
          // Option B: Select national coaches from club attraction factor
          ///////////////////////////////////////////////////////////////////////////////////////////
          // Collect list of nations based on their skill
          List<Nation2> ltNat2 = new List<Nation2>();
          foreach (int iNat in CkAppShared.iNations) {
            CornerkickManager.Club nat = CornerkickManager.Tool.getNation(iNat, CkAppShared.ckMng.ltClubs);
            if (nat == null) continue;

            Nation2 nat2 = new Nation2();
            nat2.nation = nat;

            List<CornerkickManager.Player> ltPlayerNatTmp = CkAppShared.ckMng.getBestPlayer(iNat, iPlCount: nPlayerNat);
            foreach (CornerkickManager.Player plNatTmp in ltPlayerNatTmp) {
              nat2.fSkillTotal += CornerkickGame.Tool.getAveSkill(plNatTmp.plGame, bIdeal: true);
            }

            ltNat2.Add(nat2);
          }
          ltNat2 = ltNat2.OrderByDescending(o => o.fSkillTotal).ToList();

          // Collect user based on their clubs attraction factor
          List<User4Nat> ltUser4Nat = new List<User4Nat>();
          foreach (CornerkickManager.User usr in CkAppShared.ckMng.ltUser) {
            if (usr.club == null) continue;

            ltUser4Nat.Add(new User4Nat { usr = usr, fAttrFactor = usr.club.getAttractionFactor(CkAppShared.ckMng.iSeason, ltCups: CkAppShared.ckMng.ltCups, dtNow: CkAppShared.ckMng.dtDatum) });
          }
          ltUser4Nat = ltUser4Nat.OrderByDescending(o => o.fAttrFactor).ToList();

          // Link nations to user
          for (int iN2 = 0; iN2 < ltNat2.Count; iN2++) {
            if (iN2 >= ltUser4Nat.Count) break;
            if (ltUser4Nat[iN2].fAttrFactor < 100f) break;

            CornerkickManager.Club nat = ltNat2[iN2].nation;
            CornerkickManager.User usr = ltUser4Nat[iN2].usr;

            // Assign user to nat and vise versa
            nat.user = usr;
            usr.nation = nat;

            // Inform user
            if (dtWcSelectPlayerFinish.CompareTo(CkAppShared.ckMng.dtDatum) > 0) CkAppShared.ckMng.sendNews(usr, "Bitte wählen Sie bis zum " + dtWcSelectPlayerFinish.AddDays(-1).ToString("d", Member.MemberController.getCi(usr.club != null ? usr.club.iLand : nat.iLand)) + " 23:59 Uhr Ihre " + nPlayerNat.ToString() + " Spieler für die Endrunde aus.");
            else CkAppShared.ckMng.sendNews(usr, "Bitte wählen Sie Ihre " + nPlayerNat.ToString() + " Spieler für die Endrunde aus.");
            CkAppShared.ckMng.sendNews(usr, "Welche Ehre! Der Verband von " + nat.sName + " stellt Sie als Nationaltrainer für die kommende WM ein.");
          }

          foreach (int iNat in CkAppShared.iNations) {
            CornerkickManager.Club nat = CornerkickManager.Tool.getNation(iNat, CkAppShared.ckMng.ltClubs);
            if (nat == null) continue;
            nat.ltPlayer.Clear();

            // Add all player of that nation
            if (nat.user == null) nat.ltPlayer = CkAppShared.ckMng.getBestPlayer(iNat, iPlCount: nPlayerNat);
          }
        }
      }

      return bReturn;
    }

    private class Nation2
    {
      public CornerkickManager.Club nation { get; set; }
      public float fSkillTotal { get; set; }
    }

    private class User4Nat
    {
      public CornerkickManager.User usr { get; set; }
      public float fAttrFactor { get; set; }
    }

    private static void clearNations()
    {
      foreach (CornerkickManager.User usrNat in CkAppShared.ckMng.ltUser) usrNat.nation = null;
      foreach (CornerkickManager.Club nat in CkAppShared.ckMng.ltClubs) {
        if (nat.bNation) {
          nat.user = null;
          nat.ltPlayer.Clear();
        }
      }
    }

    private static int countCpuPlayerOnTransferlist()
    {
      int nPl = 0;
      foreach (CornerkickManager.Transfer.Item transfer in CkAppShared.ckMng.ltTransfer) {
        if (CkAppShared.ckMng.ltClubs[0].ltPlayer.IndexOf(transfer.player) >= 0) nPl++;
      }

      return nPl;
    }

    /*
    private static void checkCpuJouth()
    {
      while (CkAppShared.ckMng.ltClubs[0].ltJugendspielerID.Count > 0) {
        int iPlId = CkAppShared.ckMng.ltClubs[0].ltJugendspielerID[0];
        CkAppShared.ckMng.ltClubs[0].ltJugendspielerID.RemoveAt(0);
        CkAppShared.ckMng.ltClubs[0].ltPlayerId.Add(iPlId);
        CkAppShared.ckMng.ui.putPlayerOnTransferlist(iPlId, 0);
      }
    }
    */

    internal bool saveAsync(double timerCalenderInterval = 10000.0, bool bForce = false)
    {
      Task<bool> tkSave = Task.Run(() => save(timerCalenderInterval, bForce: bForce));
      //tkSave.Wait();

      return tkSave.Result;
    }
    public static bool save(double timerCalenderInterval = 10000.0, bool bForce = false)
    {
      // Don't save if calendar to fast
      if (timerCalenderInterval < 10000.0 && !bForce) return false;

      string sAppDataDir = CkAppShared.sAppDataDir;

      try {
#if _DEPLOY_ON_AZURE
        try {
          sHomeDir = HttpContext.Current.Server.MapPath("~");
        } catch {
          CkAppShared.ckMng.tl.writeLog("save: unable to create sHomeDir from Server.MapPath", CornerkickManager.Main.sErrorFile);
          sHomeDir = "D:\\home\\site\\wwwroot";
#endif
        if (sAppDataDir.EndsWith("\\")) sAppDataDir = sAppDataDir.Remove(sAppDataDir.Length - 1);
      } catch (Exception e) {
        CkAppShared.ckMng.tl.writeLog("save: HttpException: " + e.Message);
#if _DEPLOY_ON_AZURE
        sHomeDir = "D:\\home\\site\\wwwroot";
#endif
      }

      // Write last ck state to file
      saveLaststate(sAppDataDir);

      // Clear CPU user news before saving
      for (int iN = 0; iN < CkAppShared.ckMng.ltUser[0].ltNews.Count; iN++) {
        if (CkAppShared.ckMng.ltUser[0].ltNews[iN].iType < 200) {
          CkAppShared.ckMng.ltUser[0].ltNews.RemoveAt(iN);
          iN--;
          continue;
        }

        if ((CkAppShared.ckMng.dtDatum - CkAppShared.ckMng.ltUser[0].ltNews[iN].dt).TotalDays > 7) {
          CkAppShared.ckMng.ltUser[0].ltNews.RemoveAt(iN);
          iN--;
          continue;
        }
      }

      // Clear CPU clubs before saving
      foreach (CornerkickManager.Club clb in CkAppShared.ckMng.ltClubs) {
        if (clb.user == null || clb.iId == 0) {
          clb.ltAccount.Clear();
          clb.ltTrainingHist.Clear();
          clb.ltSponsorOffers.Clear();
          clb.iBalance = 0;
          clb.stadium = null;
        }
      }

      // Compose filename
      string sFilenameSave2 = ".autosave_" + CkAppShared.ckMng.dtDatum.ToString("yyyy-MM-dd_HH-mm") + ".ckx";
      string sFileSave2 = Path.Combine(sAppDataDir, "save", sFilenameSave2);
      CkAppShared.ckMng.tl.writeLog("save file: " + sFileSave2);

      // Save
      CornerkickManager.IO.Return io = new CornerkickManager.IO.Return();
      try {
        io = CkAppShared.ckMng.io.save(sFileSave2);
      } catch (Exception e) {
        CkAppShared.ckMng.tl.writeLog("ERROR: could not save to file " + sFileSave2 + Environment.NewLine + e.Message + e.StackTrace, CornerkickManager.Main.sErrorFile);
      }

      // Upload save
      if (io.bOk) {
#if _USE_AMAZON_S3
        Task.Run(() => as3.uploadFileAsync(sFileSave2, as3.sCkInstanceName + "save/" + sFilenameSave2, "application/zip"));
#endif

        // Copy autosave file with datum to basic one (could use file link)
        string sFileSave = sAppDataDir + "/save/" + CkAppShared.sFilenameSave;
        if (File.Exists(sFileSave)) {
          try {
            File.Delete(sFileSave);
          } catch {
          }
        }

        File.Copy(sFileSave2, sFileSave);
#if _USE_AMAZON_S3
        Task.Run(() => as3.uploadFileAsync(sFileSave, as3.sCkInstanceName + "save/" + CkAppShared.sFilenameSave, "application/zip"));
#endif
      } else {
        CkAppShared.ckMng.tl.writeLog("ERROR: Save error messages: " + io.sError, CornerkickManager.Main.sErrorFile);
      }

      //saveMerchHistory(sHomeDir);

#if _USE_AMAZON_S3
      // Upload games
      DirectoryInfo diGames = new DirectoryInfo(Path.Combine(sAppDataDir, "save", "games"));
      CkAppShared.ckMng.tl.writeLog("Directory info games: '" + diGames.FullName + "'. Exist: " + diGames.Exists.ToString());

      if (diGames.Exists) {
        FileInfo[] ltCkgFiles = diGames.GetFiles("*.ckgx");
        CkAppShared.ckMng.tl.writeLog("File info games length: " + ltCkgFiles.Length.ToString());

        foreach (FileInfo ckg in ltCkgFiles) {
#if _VS2019
          DateTime dtGame;
          int iTeamIdH;
          int iTeamIdA;
          int iCupId;

          Controllers.ViewGameController.getFilenameInfo(ckg, out dtGame, out iTeamIdH, out iTeamIdA, out iCupId);

          if (dtGame.CompareTo(dtLoadCk) < 0) continue; // If game was already present when ck was started
          if (iCupId == iCupIdTestgame) continue; // If game is test-game
#endif

          string sFileGameSave = Path.Combine(sAppDataDir, "save", "games", ckg.Name);
          Task.Run(() => as3.uploadFileAsync(sFileGameSave, as3.sCkInstanceName + "save/games/" + ckg.Name, "application/zip"));
        }
      }
#endif

#if _USE_AMAZON_S3
      saveMails();
#else
      saveMails();
#endif

#if _USE_AMAZON_S3
      // Upload wishlist
      Task.Run(() => as3.uploadFileAsync(Path.Combine(sAppDataDir, "wishlist.json"), "wishlist.json"));
#endif

      // Save logs
      Task.Run(() => SaveLogsAsync(sAppDataDir));

      return true;
    }

    public static async Task SaveLogsAsync(string sHomeDir)
    {
      foreach (string sLogFileUpload in new string[] { CornerkickManager.Main.sLogFile, CornerkickManager.Main.sErrorFile }) {
        string sLogFileUploadFull = Path.Combine(sHomeDir, "log", sLogFileUpload);
        if (File.Exists(sLogFileUploadFull)) {
          await WaitUntilFileIsNotLocked(sLogFileUploadFull);

#if !DEBUG
          try {
#if _USE_BLOB
            CornerkickWebMvc.Controllers.BlobsController bcontr = new Controllers.BlobsController();
            bcontr.uploadBlob("blobLog", sFileZipLog);
#endif
#if _USE_AMAZON_S3
            await Task.Run(() => as3.uploadFileAsync(sLogFileUploadFull, sKey: as3.sCkInstanceName + "log/" + sLogFileUpload + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), nRetry: 3));
#endif
          } catch (Exception e) {
#if _USE_BLOB
            CkAppShared.ckMng.tl.writeLog("ERROR: could not upload log file to blob", CkAppShared.ckMng.sErrorFile);
#endif
#if _USE_AMAZON_S3
            CkAppShared.ckMng.tl.writeLog("ERROR: could not upload log file '" + sLogFileUploadFull + "' to amazon s3" + Environment.NewLine + e.Message, CornerkickManager.Main.sErrorFile);
#endif
          }
#endif
        }
      }
    }

    public static async Task WaitUntilFileIsNotLocked(string fileLocked, int sleep = 50)
    {
      while (IsFileLocked(fileLocked)) {
        await Task.Delay(sleep);
      }
    }
    protected static bool IsFileLocked(string fileLocked)
    {
      try {
        using (FileStream stream = File.Open(fileLocked, FileMode.Open, FileAccess.Read, FileShare.None)) {
          stream.Close();
        }
      } catch (IOException) {
        //the file is unavailable because it is:
        //still being written to
        //or being processed by another thread
        //or does not exist (has already been processed)
        return true;
      }

      //file is not locked
      return false;
    }

    public static int getGameSpeedFromUsers()
    {
      foreach (CornerkickManager.User usr in CkAppShared.ckMng.ltUser) {
        if (usr.club?.nextGame != null) return usr.club.nextGame.iGameSpeed;
        else if (usr.nation?.nextGame != null) return usr.nation.nextGame.iGameSpeed;
      }

      return 0;
    }

    internal static void saveLaststate(string sTargetDir)
    {
      string sFileSettings = Path.Combine(sTargetDir, CkAppShared.sFilenameSettings);

      using (StreamWriter fileSettings = new StreamWriter(sFileSettings)) {
        fileSettings.WriteLine((CkAppShared.timerCkCalender.Interval / 1000.0).ToString("g", CultureInfo.InvariantCulture));
        fileSettings.WriteLine(CkAppShared.timerCkCalender.Enabled.ToString());
        fileSettings.WriteLine(DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
        fileSettings.WriteLine(getGameSpeedFromUsers().ToString());
        fileSettings.WriteLine(CkAppShared.settings.bEmailCertification.ToString());
        fileSettings.WriteLine(CkAppShared.settings.bRegisterDuringGame.ToString());

        fileSettings.WriteLine(CkAppShared.settings.bMaintenance.ToString());
        fileSettings.WriteLine(CkAppShared.settings.sInfo);
        fileSettings.WriteLine(CkAppShared.settings.dtCounterStart.ToString("s", CultureInfo.InvariantCulture));

        fileSettings.Close();
      }

#if _USE_AMAZON_S3
      Task.Run(() => as3.uploadFileAsync(sFileSettings, as3.sCkInstanceName + CkAppShared.sFilenameSettings));
#endif
    }

    private static void saveMails()
    {
      string sDirMail = Path.Combine(CkAppShared.ckMng.settings.sHomeDir, "mail");
      if (!System.IO.Directory.Exists(sDirMail)) System.IO.Directory.CreateDirectory(sDirMail);

      if (CkAppShared.ltMail == null) return;

      foreach (CkAppShared.Mail mail in CkAppShared.ltMail) {
        string sDateTime = mail.dt.ToString("yyyyMMddHHmmss");
        string sFilenameMail = mail.sIdTo + "_" + sDateTime + ".txt";
        string sFilenameMail2 = Path.Combine(sDirMail, sFilenameMail);

        using (StreamWriter fileMail = new StreamWriter(sFilenameMail2)) {
          string sText = mail.sIdTo + " " + mail.sIdFrom + " " + sDateTime + " " + mail.bNew.ToString() + Environment.NewLine + mail.sText;
          fileMail.Write(sText);
          fileMail.Close();

#if _USE_AMAZON_S3
          Task.Run(() => as3.uploadFileAsync(sFilenameMail2, as3.sCkInstanceName + "mail/" + sFilenameMail));
#endif
        }
      }
    }

    private static void saveMerchHistory(string sTargetDir)
    {
      string sFileMH = Path.Combine(sTargetDir, "merchandisingHistory", "club_");

      foreach (CornerkickManager.User usr in CkAppShared.ckMng.ltUser) {
        if (usr.club == null) continue;

        /*
          saveValue(mh.iSeason, ref byteArr, ref iK);

          if (mh.marketer != null) {
            byteArr[iK++] = (byte)(mh.marketer.marketer.iId + 1);
            saveValue(mh.marketer.iMoney, ref byteArr, ref iK);
          } else {
            byteArr[iK++] = 0; // No marketer

            byteArr[iK++] = (byte)club.ltMerchandisingItem.Count;
            foreach (Club.MerchandisingItem cmi in mh.ltMerchandisingItem) {
              byteArr[iK++] = cmi.item.iId;
              saveValue(cmi.iPresent, ref byteArr, ref iK);
              saveValue(cmi.fPricePresentBuyAve, ref byteArr, ref iK);
              saveValue(cmi.iSold, ref byteArr, ref iK);
              saveValue(cmi.iIncome, ref byteArr, ref iK);
              saveValue(cmi.fPrice, ref byteArr, ref iK);
            }
          }
            fileMHClub.WriteLine((timerCkCalender.Interval / 1000.0).ToString("g", CultureInfo.InvariantCulture));
         */
        using (StreamWriter fileMHClub = new StreamWriter(sFileMH + usr.club.iId + ".txt")) {
          fileMHClub.WriteLine(CkAppShared.ckMng.iSeason.ToString());

          if (usr.club.merchMarketer != null) {
            fileMHClub.WriteLine((usr.club.merchMarketer.marketer.iId + 1).ToString() + " " + usr.club.merchMarketer.iMoney.ToString());
          } else {
            fileMHClub.WriteLine("0 0");

            fileMHClub.WriteLine(usr.club.ltMerchandisingItem.Count.ToString());
            foreach (CornerkickManager.Club.MerchandisingItem cmi in usr.club.ltMerchandisingItem) {
              fileMHClub.Write(cmi.item.iId.ToString() + " ");
              fileMHClub.Write(cmi.iPresent.ToString() + " ");
              fileMHClub.Write(cmi.fPricePresentBuyAve.ToString("g", CultureInfo.InvariantCulture) + " ");
              fileMHClub.Write(cmi.iSold.ToString() + " ");
              fileMHClub.Write(cmi.iIncome.ToString() + " ");
              fileMHClub.Write(cmi.fPrice.ToString("g", CultureInfo.InvariantCulture));
              fileMHClub.WriteLine("");
            }
          }

          fileMHClub.Close();
        }
      }
    }

    private static void loadMerchHistory(string sTargetDir)
    {
      NumberStyles style = NumberStyles.Number | NumberStyles.AllowDecimalPoint;

      foreach (CornerkickManager.User usr in CkAppShared.ckMng.ltUser) {
        if (usr.club == null) continue;

        string sFileMH = Path.Combine(sTargetDir, "merchandisingHistory", "club_" + usr.club.iId.ToString() + ".txt");

        if (File.Exists(sFileMH)) {
          CornerkickManager.Club.MerchandisingHistory mh = new CornerkickManager.Club.MerchandisingHistory();

          System.IO.StreamReader fileMHClub = new System.IO.StreamReader(@sFileMH);

          string sLine;
          int iLine = 0;
          while ((sLine = fileMHClub.ReadLine()) != null) {
            if (iLine == 0) {
              int.TryParse(sLine, out mh.iSeason);
            } else if (iLine == 1) {
              string[] sLineSplit = sLine.Split();

              int iMMId = 0;
              int.TryParse(sLineSplit[0], out iMMId);

              if (iMMId > 0) {
                mh.marketer = new CornerkickManager.Club.MerchandisingMarketer();
                mh.marketer.marketer = CornerkickManager.Club.MerchandisingMarketer.getMarketer(iMMId - 1, CkAppShared.ckMng.ltMerchandisingMarketer);
                int.TryParse(sLineSplit[1], out mh.marketer.iMoney);
              }
            } else if (iLine == 2) {
              int iItems = 0;
              int.TryParse(sLine, out iItems);
            } else {
              CornerkickManager.Club.MerchandisingItem cmi = new CornerkickManager.Club.MerchandisingItem();

              string[] sLineSplit = sLine.Split();

              int iMiId = int.Parse(sLineSplit[0]);
              cmi.item = CornerkickManager.Merchandising.getItem(iMiId, CkAppShared.ckMng.ltMerchandising);

              cmi.iPresent = int.Parse(sLineSplit[1]);
              cmi.fPricePresentBuyAve = float.Parse(sLineSplit[2], style, CultureInfo.InvariantCulture);
              cmi.iSold = int.Parse(sLineSplit[3]);
              cmi.iIncome = int.Parse(sLineSplit[4]);
              cmi.fPrice = float.Parse(sLineSplit[5], style, CultureInfo.InvariantCulture);

              if (mh.ltMerchandisingItem == null) mh.ltMerchandisingItem = new List<CornerkickManager.Club.MerchandisingItem>();
              mh.ltMerchandisingItem.Add(cmi);
            }

            iLine++;
          }

          fileMHClub.Close();

          usr.club.ltMerchandisingHistory.Add(mh);
        }
      }
    }

    internal static bool load(string sAppDataDir)
    {
      // Create stopwatch
      System.Diagnostics.Stopwatch swLoad = new System.Diagnostics.Stopwatch();

      // Start stopwatch
      swLoad.Start();

      if (string.IsNullOrEmpty(sAppDataDir)) return false;

      string sFileLoad = Path.Combine(sAppDataDir, "save", CkAppShared.sFilenameSave);

#if !DEBUG
#if _USE_BLOB
      string sFileZipLog = sAppDataDir + "log.zip";

      CornerkickWebMvc.Controllers.BlobsController bcontr = new Controllers.BlobsController();
      if (!System.IO.File.Exists(sFileLoad)) bcontr.downloadBlob("blobSave", sFileLoad);
      bcontr.downloadBlob("blobLog", sFileZipLog);
#endif
#if _USE_AMAZON_S3
      //if (!System.IO.File.Exists(sFileLoad)) {
      try {
        as3.downloadFile(as3.sCkInstanceName + "save/" + CkAppShared.sFilenameSave, sFileLoad);
      } catch {
        CkAppShared.ckMng.tl.writeLog("ERROR: Unable to download file " + as3.sCkInstanceName + CkAppShared.sFilenameSave + " to: " + sFileLoad, CornerkickManager.Main.sErrorFile);
      }
      /*
      if (Directory.Exists(sAppDataDir + "save")) {
        try {
          Directory.Delete(sAppDataDir + "save", true);
        } catch {
          CkAppShared.ckMng.tl.writeLog("ERROR: unable to delete existing temp. load directory: " + sAppDataDir + "save", CkAppShared.ckMng.sErrorFile);
        }
      }

      Directory.CreateDirectory(sAppDataDir + "save");
      if (System.IO.File.Exists(sAppDataDir + sSaveZip)) ZipFile.ExtractToDirectory(sAppDataDir + sSaveZip, sAppDataDir + "save");
      */
      //}

      // Download log async
      //Task<bool> tkDownloadLog = Task.Run(async () => await downloadFileAsync(as3, "ckLog", sAppDataDir + "/log.zip"));

      // Download Google ads.txt async
      as3.downloadFile("ads.txt", Path.Combine(CkAppShared.sWwwRootDir, "ads.txt"));
#endif
#endif

      if (!File.Exists(sFileLoad)) {
        CkAppShared.ckMng.tl.writeLog("Start new default game.");

        CornerkickManager.Main _ckMng = CkAppShared.ckMng;
        Progress<int[]> progress = new Progress<int[]>(ReportProgress);

        _ckMng = setCkMngToDefault(_ckMng, progress).Result;
        CkAppShared.ckMng = _ckMng;
        CkAppShared.ckMng.iSeason = 1;

        // Enable save timer to save every 15 min.
        CkAppShared.timerSave.Enabled = true;

        CkAppShared.iLoadState = 3; // New game (pause)

        return false;
      }

      // Load ck state
      if (string.IsNullOrEmpty(CkAppShared.ckMng.io.load(sFileLoad))) {
        CkAppShared.ckMng.tl.writeLog("File " + sFileLoad + " loaded.");
        //loadMerchHistory(CkAppShared.ckMng.settings.sAppDataDir);

        // Admin club
        CornerkickManager.Club? clb0 = CkAppShared.ckMng.ltClubs.Find(c => c.iId == 0);

        // Set admin user to CPU
        //if (CkAppShared.ckMng.ltClubs.Count > 0) CkAppShared.ckMng.ltClubs[0].user = null;
        if (clb0 != null) clb0.user = null;

        // Set length of EocInfo flag
        Member.MemberController.bHideEocInfo = new bool[CkAppShared.ckMng.ltUser.Count];

        // Set length of tutorial class array
        Member.TutorialController.initialiteTutorial();

        // Delete CPU club stadiums
        List<CornerkickManager.Club> ltClubsCPU = CkAppShared.ckMng.ltClubs.FindAll(c => c.user == null);
        foreach (CornerkickManager.Club clb in ltClubsCPU) {
          clb.stadium = CkAppShared.stadiums[clb.iDivision > 0 ? 1 : 0];
        }

        // Set retired players name to none
        List<CornerkickManager.Player> ltPlayerRet = CornerkickManager.PlayerTool.getRetiredPlayer(CkAppShared.ckMng.ltPlayer);
        for (int iPl = 0; iPl < ltPlayerRet.Count; iPl++) {
          ltPlayerRet[iPl].plGame.sName = "";
        }

        // Transfer player from club0 to cpu-club if too few
        foreach (CornerkickManager.Club clb in CkAppShared.ckMng.ltClubs) {
          if (clb == null) continue;
          if (clb.bNation) continue;
          if (clb.user != null) continue;
          if (clb.iId == 0) continue;

          int iPlTransferIx = 0;
          while (clb.ltPlayer.Count < 16) {
            if (!CkAppShared.ckMng.tr.transferPlayer(clb0.ltPlayer[iPlTransferIx], clb, bForce: true)) iPlTransferIx++;
            if (iPlTransferIx >= clb0.ltPlayer.Count) break;
          }
        }

        // Retire club player if too many
        int iCountPlRet = 0;
        const int iClubCpuPlayerMax = 26;
        foreach (CornerkickManager.Club clb in CkAppShared.ckMng.ltClubs) {
          if (clb == null) continue;
          if (clb.bNation) continue;
          if (clb.user != null) continue;
          if (clb.iId == 0) continue;
          if (clb.ltPlayer.Count <= iClubCpuPlayerMax) continue;

          CkAppShared.ckMng.doFormation(clb);
          while (clb.ltPlayer.Count > iClubCpuPlayerMax) {
            CornerkickManager.Player plRet = clb.ltPlayer[clb.ltPlayer.Count - 1];
            CkAppShared.ckMng.plt.retirePlayer(plRet, clb);
            iCountPlRet++;
          }
        }

        // Delete past trainings (and trainings after season end) from club
        foreach (CornerkickManager.Club clb in CkAppShared.ckMng.ltClubs) {
          for (int iTU = 0; iTU < clb.training.ltUnit.Count; iTU++) {
            if (clb.training.ltUnit[iTU].dt.CompareTo(CkAppShared.ckMng.dtDatum) < 0 || clb.training.ltUnit[iTU].dt.CompareTo(CkAppShared.ckMng.dtSeasonEnd) > 0) {
              clb.training.ltUnit.RemoveAt(iTU--);
            }
          }
        }

        // Add freelancer-scouts if missing
        foreach (CornerkickManager.User usr in CkAppShared.ckMng.ltUser) addFreelancerScouts(usr);

#if false
        // Extent jouth player contract
        for (int iC = 0; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
          CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];
          if (clb == null) continue;
          if (clb.bNation) continue;
          //if (clb.user != null) continue;

          for (int iP = 0; iP < clb.ltPlayerJouth.Count; iP++) {
            CornerkickManager.Player plJ = clb.ltPlayerJouth[iP];

            byte iContractLengthMin = (byte)(18 - (int)plJ.plGame.getAge(CkAppShared.ckMng.dtDatum));
            if (plJ.contract.iLength < iContractLengthMin) {
              plJ.contract.iLength = iContractLengthMin;
            }
          }
        }

        // Check if club-id of player correct
        for (int iPl = 0; iPl < CkAppShared.ckMng.ltPlayer.Count; iPl++) {
          CornerkickManager.Player pl = CkAppShared.ckMng.ltPlayer[iPl];

          CornerkickManager.Club clb = null;
          if (pl.contract != null) clb = pl.contract.club;

          if (clb != null) {
            if (!CornerkickManager.PlayerTool.ownPlayer(clb, pl)) {
              CkAppShared.ckMng.tl.writeLog("Add player to team 0: " + pl.plGame.sName + ", current club: " + clb.sName, CornerkickManager.Main.sErrorFile);

              pl.contract.club = CkAppShared.ckMng.ltClubs[0];
              if (pl.plGame.getAge(CkAppShared.ckMng.dtDatum) < 18) CkAppShared.ckMng.ltClubs[0].ltPlayerJouth.Add(pl);
              else                                       CkAppShared.ckMng.ltClubs[0].ltPlayer     .Add(pl);
            }
          }
        }

        // Check if club-id of player correct #2
        for (int iC = 0; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
          CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];
          if (clb.bNation) continue;

          for (int iP = 0; iP < clb.ltPlayer.Count; iP++) {
            CornerkickManager.Player pl = clb.ltPlayer[iP];

            if (pl.contract == null) {
              CkAppShared.ckMng.tl.writeLog("Create contract for player: " + pl.plGame.sName + " of club: " + clb.sName, CornerkickManager.Main.sErrorFile);
              pl.contract = CornerkickManager.PlayerTool.getContract(pl, 1, clb, CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd);
            }

            if (pl.contract.club == null) {
              CkAppShared.ckMng.tl.writeLog("Correct club of player: " + pl.plGame.sName + ", club: " + clb.sName, CornerkickManager.Main.sErrorFile);
              pl.contract.club = clb;
            }
          }
        }

        // Check if player in prof and jouth team
        for (int iC = 0; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
          CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];
          if (clb.bNation) continue;

          for (int iP = 0; iP < clb.ltPlayer.Count; iP++) {
            for (int iPJ = 0; iPJ < clb.ltPlayerJouth.Count; iPJ++) {
              if (clb.ltPlayer[iP] == clb.ltPlayerJouth[iPJ]) {
                CkAppShared.ckMng.tl.writeLog("Remove double player from jouth team: " + clb.ltPlayer[iP].plGame.sName + ", club: " + clb.sName, CornerkickManager.Main.sErrorFile);

                clb.ltPlayerJouth.RemoveAt(iPJ);
                iPJ--;
              }
            }
          }
        }

        // Check if young player are in jouth team
        for (int iC = 0; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
          CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];
          if (clb.user != null && iC > 0) continue;
          if (clb.bNation) continue;

          for (int iP = 0; iP < clb.ltPlayer.Count; iP++) {
            CornerkickManager.Player pl = clb.ltPlayer[iP];

            if (pl.plGame.getAge(CkAppShared.ckMng.dtDatum) < 16) {
              CkAppShared.ckMng.tl.writeLog("Move player to jouth team: " + pl.plGame.sName + ", club: " + clb.sName, CornerkickManager.Main.sErrorFile);

              clb.ltPlayerJouth.Add   (pl);
              clb.ltPlayer     .Remove(pl);

              if (iC == 0) {
                CkAppShared.ckMng.tr.putPlayerOnTransferlist(pl, 0);
              }
              iP--;
            }
          }
        }

        // Check if club history of player correct
        for (int iPl = 0; iPl < CkAppShared.ckMng.ltPlayer.Count; iPl++) {
          CornerkickManager.Player pl = CkAppShared.ckMng.ltPlayer[iPl];

          for (int iCH = 1; iCH < pl.ltClubHistory.Count; iCH++) {
            if (pl.ltClubHistory[iCH].club.iId == pl.ltClubHistory[iCH - 1].club.iId &&
                pl.ltClubHistory[iCH].bJouth  == pl.ltClubHistory[iCH - 1].bJouth) {
              pl.ltClubHistory.RemoveAt(iCH);
              iCH--;
            }
          }
        }

        // Check if no contract --> no club
        for (int iPl = 0; iPl < CkAppShared.ckMng.ltPlayer.Count; iPl++) {
          CornerkickManager.Player pl = CkAppShared.ckMng.ltPlayer[iPl];

          if (pl.contract != null && pl.contract.iLength == 0 && !pl.bRetire) {
            CornerkickManager.Club clb = null;
            string sClubName = "vereinslos";

            if (pl.contract.club != null) {
              clb = pl.contract.club;
              sClubName = clb.sName;
            }

            if (pl.contract.iSalary > 0) {
              CkAppShared.ckMng.tl.writeLog("Extend player contract with length = 0: " + pl.plGame.sName + ", club: " + sClubName, CornerkickManager.Main.sErrorFile);
              pl.contract.iLength++;
            } else {
              pl.contract = CornerkickManager.PlayerTool.getContract(pl, 1, clb, CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd);
              //ckMng.tl.writeLog("Retire player with contract length = 0: " + pl.sName + ", club: " + sClubName, CornerkickManager.Main.sErrorFile);
              //ckMng.plr.retirePlayer(pl, clb);
            }

            continue;
          }
        }
#endif

        // Check for nation in club history and reset player to club if nation has become club
        foreach (CornerkickManager.Player pl in CkAppShared.ckMng.ltPlayer) {
          try {
            if (pl == null) continue;
            if (pl.ltClubHistory == null) continue;

            // Check club history
            for (int iCH = pl.ltClubHistory.Count - 1; iCH >= 0; iCH--) {
              //if (pl.ltClubHistory[iCH].club == null || pl.ltClubHistory[iCH].club.bNation) {
              if (pl.ltClubHistory[iCH].club != null && pl.ltClubHistory[iCH].club.bNation) {
                pl.ltClubHistory.RemoveAt(iCH);
              }
            }

            // Check contract
            if (pl.contract != null) {
              if (pl.contract.club != null && pl.contract.club.bNation) {
                if (pl.ltClubHistory.Count > 0) pl.contract.club = pl.ltClubHistory[pl.ltClubHistory.Count - 1].club;
                else pl.contract.club = CkAppShared.ckMng.ltClubs[0];

                CkAppShared.ckMng.tr.removePlayerFromTransferlist(pl);
              }
            }
          } catch (Exception e) {
            Console.WriteLine(e.Message);
          }
        }

        List<CornerkickManager.Club> ltNat = CkAppShared.ckMng.ltClubs.FindAll(n => n.bNation);
        foreach (CornerkickManager.Club nat in ltNat) {
          nat.sponsorMain = new CornerkickManager.Finance.Sponsor();
          nat.ltSponsorOffers.Clear();
          nat.ltSponsorBoards.Clear();
        }

        // Check if past games are not finished
        Task<bool> tkPerformPastGames = Task.Run(() => performPastGamesAsync(CkAppShared.ckMng.dtDatum));

#if DEBUG
        // DEBUG
        /*
        foreach (CornerkickManager.Cup cup in CkAppShared.ckMng.ltCups.FindAll(c => c.iId == iCupIdLeague)) {
          foreach (CornerkickGame.Game.Data gd in cup.ltMatchdays[cup.ltMatchdays.Count - 1].ltGameData) {
            gd.dt = gd.dt.AddDays(-7);
            cup.ltMatchdays[cup.ltMatchdays.Count - 1].dt = gd.dt;
          }
        }
        foreach (CornerkickManager.Cup cup in CkAppShared.ckMng.ltCups.FindAll(c => c.iId == iCupIdNatCup)) {
          foreach (CornerkickGame.Game.Data gd in cup.ltMatchdays[cup.ltMatchdays.Count - 1].ltGameData) {
            gd.dt = gd.dt.AddDays(-10);
            gd.dt = gd.dt.Date.Add(new TimeSpan(20, 00, 00));
            cup.ltMatchdays[cup.ltMatchdays.Count - 1].dt = gd.dt;
          }
        }
        foreach (CornerkickManager.Cup cup in CkAppShared.ckMng.ltCups.FindAll(c => c.iId == iCupIdGold || c.iId == iCupIdSilver || c.iId == iCupIdBronze)) {
          foreach (CornerkickGame.Game.Data gd in cup.ltMatchdays[cup.ltMatchdays.Count - 1].ltGameData) {
            gd.dt = gd.dt.Date.Add(new TimeSpan(20, 00, 00));
            cup.ltMatchdays[cup.ltMatchdays.Count - 1].dt = gd.dt;
          }
        }
         foreach (CornerkickManager.Cup cup in CkAppShared.ckMng.ltCups) {
           foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
             foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
               if (gd.dt.CompareTo(CkAppShared.ckMng.dtDatum) > 0) gd.referee = new CornerkickGame.Game.Referee();
             }
           }
         }
         // Set league division 2 game times
         List<CornerkickManager.Cup> ltCupsDiv2 = CkAppShared.ckMng.ltCups.FindAll(c => c.iId == iCupIdLeague && c.iId3 > 0);
         foreach (CornerkickManager.Cup cup in ltCupsDiv2) {
           foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
             md.dt = md.dt.Date.Add(new TimeSpan(13, 30, 00));
             foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
               gd.dt = md.dt;
             }
           }
         }

         // Move player back to club if contract has ended
         foreach (CornerkickManager.Player pl in CkAppShared.ckMng.ltPlayer) {
           try {
             if (pl == null) continue;
             if (pl.ltClubHistory == null) continue;
             if (pl.ltClubHistory.Count == 0) continue;

             CornerkickManager.Club clbLast = pl.ltClubHistory[pl.ltClubHistory.Count - 1].club;
             if (clbLast == null) continue;

             // Check contract
             if (pl.contract == null && clbLast.user != null) {
               Controllers.MemberController.extendPlayerContractFast(pl, clbLast, 1, 1f);
               clbLast.ltPlayer.Add(pl);
             }
           } catch (Exception e) {
             Console.WriteLine(e.Message);
           }
         }

         CornerkickManager.Club clbMirko = CkAppShared.ckMng.ltClubs[3];
         CkAppShared.ckMng.ltPlayer[4126].changeClub(clbMirko, CkAppShared.ckMng.dtSeasonStart, CkAppShared.ckMng.dtSeasonEnd);
         CkAppShared.ckMng.ltPlayer[4126].contract = CkAppShared.ckMng.ltPlayer[4126].contractNext;
         CkAppShared.ckMng.ltPlayer[4126].contractNext = null;

         CornerkickManager.Club clbDan   = CkAppShared.ckMng.ltClubs[4];
         CornerkickManager.Club clbDavid = CkAppShared.ckMng.ltClubs[143];
         CornerkickManager.Club clbJanos = CkAppShared.ckMng.ltClubs[5];
         CornerkickManager.Club clbLeif  = CkAppShared.ckMng.ltClubs[139];
         CornerkickManager.Club clbJonas = CkAppShared.ckMng.ltClubs[7];
         clbDan  .iBalance = 100000000;
         clbDavid.iBalance = 100000000;
         clbJanos.iBalance = 100000000;
         clbLeif .iBalance = 100000000;
         clbJonas.iBalance = 100000000;
         CornerkickManager.Club clbToni  = CkAppShared.ckMng.ltClubs[1];
         CornerkickManager.Club clbPapa  = CkAppShared.ckMng.ltClubs[2];
         CornerkickManager.Club clbMirko = CkAppShared.ckMng.ltClubs[3];
         CornerkickManager.Club clbDan   = CkAppShared.ckMng.ltClubs[4];
         CornerkickManager.Club clbJanos = CkAppShared.ckMng.ltClubs[5];
         CornerkickManager.Club clbAxel  = CkAppShared.ckMng.ltClubs[6];
         CornerkickManager.Club clbJonas = CkAppShared.ckMng.ltClubs[7];
         CornerkickManager.Club clbAaron = CkAppShared.ckMng.ltClubs[9];
         CornerkickManager.Club clbFelix = CkAppShared.ckMng.ltClubs[137];
         CornerkickManager.Club clbDavid = CkAppShared.ckMng.ltClubs[143];
         CornerkickManager.Club clbLeif  = CkAppShared.ckMng.ltClubs[139];

         Controllers.AdminController.removeUser(CkAppShared.ckMng.ltUser[13]);
         Controllers.AdminController.removeUser(CkAppShared.ckMng.ltUser[12]);
         Controllers.AdminController.removeUser(CkAppShared.ckMng.ltUser[9]);
         Controllers.AdminController.removeUser(CkAppShared.ckMng.ltUser[8]);
         Controllers.AdminController.removeUser(CkAppShared.ckMng.ltUser[3]);
         CornerkickManager.Club clbPatrick = CkAppShared.ckMng.ltClubs[140];
         clbPatrick.iBalance = 30000000;
         CornerkickManager.Club clbJonas = CkAppShared.ckMng.ltClubs[7];
         CornerkickManager.Finance.doTransaction(clbJonas, CkAppShared.ckMng.dtDatum, 40000000, 0, "Schmiergeld");
         CkAppShared.ckMng.dtSeasonEnd = CkAppShared.ckMng.dtDatum;

         CornerkickManager.Cup cupG = CkAppShared.ckMng.tl.getCup(iCupIdGold);
         CornerkickManager.Cup cupS = CkAppShared.ckMng.tl.getCup(iCupIdSilver);
         CornerkickManager.Cup cupB = CkAppShared.ckMng.tl.getCup(iCupIdBronze);

         cupG.ltQualification = new List<CornerkickManager.Cup.Qualification>();
         cupS.ltQualification = new List<CornerkickManager.Cup.Qualification>();
         cupB.ltQualification = new List<CornerkickManager.Cup.Qualification>();
         foreach (int iN in iNations) {
           CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(iCupIdLeague, iN, 0);
           if (league == null) continue;

           // ... to Gold Cup
           cupG.ltQualification.Add(new CornerkickManager.Cup.Qualification() { cup = league, iPlaceFirst = 1, iPlaceLast =  4 });
           cupS.ltQualification.Add(new CornerkickManager.Cup.Qualification() { cup = league, iPlaceFirst = 5, iPlaceLast =  8 });
           cupB.ltQualification.Add(new CornerkickManager.Cup.Qualification() { cup = league, iPlaceFirst = 9, iPlaceLast = 12 });
         }

         for (int iC = 0; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
           CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];

           for (int iP = 0; iP < clb.ltPlayer.Count; iP++) {
             CornerkickManager.Player pl = clb.ltPlayer[iP];

             if (pl.contract != null) pl.contract.iLength = Math.Min(pl.contract.iLength, (byte)5);
           }
         }

         CornerkickManager.Club clbPapa = CkAppShared.ckMng.ltClubs[2];
         clbPapa.cl2[2] = System.Drawing.Color.Blue;

         CornerkickManager.Club clbJonas = CkAppShared.ckMng.ltClubs[7];
         clbJonas.cl1[2] = clbJonas.cl2[0];
         clbJonas.cl2[2] = clbJonas.cl1[0];

         CornerkickManager.Club clbDan = CkAppShared.ckMng.ltClubs[4];
         clbDan.cl2[2] = clbDan.cl1[2];

         CornerkickManager.Club clbFelix = CkAppShared.ckMng.ltClubs[137];
         clbFelix.iBalance = 20000000;

         CornerkickManager.Cup cupS = CkAppShared.ckMng.tl.getCup(4);
         cupS.ltMatchdays[0].ltGameData[0].team[0].iGoals = 0;
         cupS.ltMatchdays[0].ltGameData[0].team[1].iGoals = 3;
         cupS.ltMatchdays[0].ltGameData[6].team[0].iGoals = 0;
         cupS.ltMatchdays[0].ltGameData[6].team[1].iGoals = 3;

         CkAppShared.ckMng.fz.fGlobalCreditInterest = 0.025f;

         CornerkickManager.Club clbPatrick = CkAppShared.ckMng.ltClubs[140];
         clbPatrick.iBalance = 20000000;
         CornerkickManager.Club clbLouis = CkAppShared.ckMng.ltClubs[141];
         clbLouis.iBalance = 20000000;

         CornerkickManager.Club clbDavid = CkAppShared.ckMng.ltClubs[143];
         CornerkickManager.Tool.switchClubs(clbDavid, CkAppShared.ckMng.ltClubs[13], CkAppShared.ckMng.tl.getCup(1, 36, 0), CkAppShared.ckMng.tl.getCup(1, 36, 1));

         createCupSilver();
         createCupBronze();

         CkAppShared.ckMng.dtDatum = CkAppShared.ckMng.dtDatum.AddMinutes(-15);

         // Modify talent array
         int[][] iPlayerTalentMain = new int[12][];
         iPlayerTalentMain[1] = new int[] { CornerkickGame.Game.iSkillCategoryIxAthletic, CornerkickGame.Game.iSkillCategoryIxCognition }; // Keeper
         iPlayerTalentMain[2] = new int[] { CornerkickGame.Game.iSkillCategoryIxAthletic, CornerkickGame.Game.iSkillCategoryIxDuel }; // CD
         iPlayerTalentMain[3] = new int[] { CornerkickGame.Game.iSkillCategoryIxAthletic, CornerkickGame.Game.iSkillCategoryIxDuel }; // LD
         iPlayerTalentMain[4] = iPlayerTalentMain[3]; // RD
         iPlayerTalentMain[5] = new int[] { CornerkickGame.Game.iSkillCategoryIxAthletic, CornerkickGame.Game.iSkillCategoryIxDuel }; // DM
         iPlayerTalentMain[6] = new int[] { CornerkickGame.Game.iSkillCategoryIxAthletic, CornerkickGame.Game.iSkillCategoryIxCoordination, CornerkickGame.Game.iSkillCategoryIxAccuracy }; // LM
         iPlayerTalentMain[7] = iPlayerTalentMain[6]; // RM
         iPlayerTalentMain[8] = new int[] { CornerkickGame.Game.iSkillCategoryIxAthletic, CornerkickGame.Game.iSkillCategoryIxCoordination, CornerkickGame.Game.iSkillCategoryIxAccuracy }; // OM
         iPlayerTalentMain[9] = new int[] { CornerkickGame.Game.iSkillCategoryIxAthletic, CornerkickGame.Game.iSkillCategoryIxCoordination }; // LF
         iPlayerTalentMain[10] = iPlayerTalentMain[9]; // RF
         iPlayerTalentMain[11] = new int[] { CornerkickGame.Game.iSkillCategoryIxAthletic, CornerkickGame.Game.iSkillCategoryIxAccuracy, CornerkickGame.Game.iSkillCategoryIxPower }; // FW

         foreach (CornerkickManager.Player pl in CkAppShared.ckMng.ltPlayer) {
           // Endurance
           pl.iTalent[CornerkickGame.Game.iSkillCategoryIxEndurance] = (byte)random.Next(3, 7);

           List<int> ltPosMain = new List<int>();
           for (byte iP = 0; iP < pl.plGame.fExperiencePos.Length; iP++) {
             if (pl.plGame.fExperiencePos[iP] > 0.99) ltPosMain.Add(iP + 1);
           }

           for (byte iSC = 1; iSC < CornerkickGame.Game.iSkillCategoryLength; iSC++) {
             bool bMC = false;

             foreach (byte iPM in ltPosMain) {
               foreach (byte iMC in iPlayerTalentMain[iPM]) {
                 if (iSC == iMC) {
                   bMC = true;
                   break;
                 }
               }

               if (bMC) break;
             }

             if (bMC) continue;

             pl.iTalent[iSC] = (byte)Math.Max(random.Next(10), pl.iTalent[CornerkickGame.Game.iSkillCategoryIxAthletic] - 2);
           }
         }

         setNationColors();

         // TMP section
         // Check for double club successes
         for (int iC = 0; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
           CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];

           for (int iS = 0; iS < clb.ltSuccess.Count; iS++) {
             CornerkickManager.Main.Success suc = clb.ltSuccess[iS];

             for (int iCP = 0; iCP < suc.ltCupPlace.Count; iCP++) {
               for (int jCP = iCP + 1; jCP < suc.ltCupPlace.Count; jCP++) {
                 if (suc.ltCupPlace[iCP][1] == suc.ltCupPlace[jCP][1]) {
                   suc.ltCupPlace.RemoveAt(jCP);
                   break;
                 }
               }
             }
           }
         }

         CornerkickManager.Club clbToni = CkAppShared.ckMng.ltClubs[1];
         clbToni.iBalance += 100000000;

         CornerkickManager.Cup cupGold = CkAppShared.ckMng.tl.getCup(3);
         cupGold.ltMatchdays[8].ltGameData[3].team[1].iTeamId = 67;
         cupGold.ltMatchdays[9].ltGameData[3].team[0].iTeamId = 67;

         // Increase skill
         for (int iC = 1; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
           CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];

           if (clb.user == null) continue;
           if (iC == 1) continue; // Toni
           if (iC == 3) continue; // Mirko

           for (int iP = 0; iP < clb.ltPlayer.Count; iP++) {
             CornerkickManager.Player pl = clb.ltPlayer[iP];

             for (byte iPos = 1; iPos <= 11; iPos++) {
               if (pl.plGame.fExperiencePos[iPos - 1] > 0.999f) {
                 if      (iPos == 1) pl.plGame.iSkill[14]++; // 14 - Jump power
                 else if (iPos == 2 || iPos == 3 || iPos == 4 || iPos == 5) pl.plGame.iSkill[3]++; // Duel defence
                 else if (iPos == 6 || iPos == 7) pl.plGame.iSkill[7]++; // 7 - High pass accuracy
                 else if (iPos == 8) pl.plGame.iSkill[5]++; // 5 - Low pass accuracy
                 else if (iPos == 9 || iPos == 10) pl.plGame.iSkill[2]++; // 2 - Duel offense
                 else if (iPos == 11) pl.plGame.iSkill[9]++; // 9 - Shoot accuracy

                 break;
               }
             }

             // Reduce contract money
             pl.contract.iSalary = (int)(pl.contract.iSalary * 0.9);
             pl.contract.iPlay   = (int)(pl.contract.iPlay   * 0.9);
             pl.contract.iGoal   = (int)(pl.contract.iGoal   * 0.9);
           }
         }

         foreach (CornerkickManager.Player pl in CkAppShared.ckMng.ltPlayer) {
           if (pl.contract != null) {
             if (pl.contract.club.ltPlayer.IndexOf(pl) < 0 && pl.contract.club.ltPlayerJouth.IndexOf(pl) < 0) {
               pl.contract.club.ltPlayer.Add(pl);
             }
           }
         }

         for (int iU = 1; iU < CkAppShared.ckMng.ltUser.Count; iU++) {
           for (int iN = 0; iN < CkAppShared.ckMng.ltUser[iU].ltNews.Count; iN++) {
             CornerkickManager.Main.News nws = CkAppShared.ckMng.ltUser[iU].ltNews[iN];

             if (nws.iType == 203) {
               CkAppShared.ckMng.ltUser[iU].ltNews.RemoveAt(iN);
               iN--;

               string sNewspaper = "Herzlich Willkommen!#" + CkAppShared.ckMng.ltUser[iU].sFirstname + " " + CkAppShared.ckMng.ltUser[iU].sSurname + " steigt als neuer Manager bei " + CkAppShared.ckMng.ltUser[iU].club.sName + " ein. ";
               sNewspaper += "Aktuell befindet sich der Verein in der 2. Liga Deutschland. ";
               sNewspaper += "In Fach&shy;kreisen werden dem Verein unter der neuen Leitung große Ambitionen nachgesagt...";
               CkAppShared.ckMng.sendNews(CkAppShared.ckMng.ltUser[0], sNewspaper, 203);
             }
           }
         }

         foreach (CornerkickGame.Player pl in CkAppShared.ckMng.ltPlayer) {
           pl.fMoral = 1f;
         }
         CkAppShared.ckMng.ltUser.RemoveAt(12);
         CkAppShared.ckMng.ltClubs[141].user = null;
         CkAppShared.ckMng.ltClubs[141].sName = "Team_Deutschland_21";
         CkAppShared.ckMng.ltUser.RemoveAt(10);
         CkAppShared.ckMng.ltClubs[140].user = null;
         CkAppShared.ckMng.ltClubs[140].sName = "Team_Deutschland_20";

         CornerkickManager.Club clbFelix = CkAppShared.ckMng.ltClubs[137];

         clbAxel.iDivision = 1;
         clbFelix.iDivision = 0;
         CkAppShared.ckMng.ltCups[1].ltClubs[0].Remove(clbAxel);
         CkAppShared.ckMng.ltCups[2].ltClubs[0].Insert(0, clbAxel);
         CkAppShared.ckMng.ltCups[1].ltClubs[0].Add(clbFelix);
         CkAppShared.ckMng.ltCups[2].ltClubs[0].Remove(clbFelix);

         CkAppShared.ckMng.ltCups[1].ltMatchdays.Clear();
         CkAppShared.ckMng.ltCups[2].ltMatchdays.Clear();

         CkAppShared.ckMng.calcMatchdays();
         CkAppShared.ckMng.ltCups[1].draw(CkAppShared.ckMng.dtDatum);
         CkAppShared.ckMng.ltCups[2].draw(CkAppShared.ckMng.dtDatum);

         CornerkickManager.Cup cupGold = CkAppShared.ckMng.tl.getCup(3);
         cupGold.settings.tsTimeOfDay = new TimeSpan(20, 45, 0);

         CornerkickManager.Cup cupSilver = CkAppShared.ckMng.tl.getCup(4);
         cupSilver.settings.tsTimeOfDay = new TimeSpan(21, 00, 0);

         CkAppShared.ckMng.ltPlayer[225].iSkill[0]++;
         CornerkickManager.Club clbDan = CkAppShared.ckMng.ltClubs[4];
         clbDan.user.ltNews.RemoveAt(15);
         clbDan.user.ltNews.RemoveAt(13);

         CornerkickManager.Club clbFelix = CkAppShared.ckMng.ltClubs[137];
         clbFelix.iBalance += 50000000;

         int iReduction = 0;
         for (int iA = 0; iA < clbMirko.ltAccount.Count; iA++) {
           CornerkickManager.Finance.Account ac = clbMirko.ltAccount[iA];

           if (ac.iType == 0 && ac.sSubject.Equals("Stadionmiete Finale Testspiel")) {
             iReduction -= (int)ac.iValue;
             clbMirko.ltAccount.RemoveAt(iA);
             iA--;
           }
         }
         clbMirko.iBalance += iReduction;

         // Add cup place to successes if missing
         for (int iC = 1; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
           CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];

           for (int iCp = 0; iCp < CkAppShared.ckMng.ltCups.Count; iCp++) {
             CornerkickManager.Cup cp = CkAppShared.ckMng.ltCups[iCp];

             CornerkickManager.Main.Success suc = CornerkickManager.Tool.getSuccess(clb, cp);
             bool bFound = false;
             if (suc != null) {
               for (int iS = 0; iS < suc.ltCupPlace.Count; iS++) {
                 if (suc.ltCupPlace[iS][1] == CkAppShared.ckMng.iSeason) {
                   bFound = true;
                   break;
                 }
               }
             }

             if (!bFound) {
               int iCupPlace = cp.getPlace(clb, CkAppShared.ckMng.dtDatum);
               if (iCupPlace > 0) suc.ltCupPlace.Add(new int[] { iCupPlace, CkAppShared.ckMng.iSeason });
             }
           }
         }

         // Reset CPU player
         if (CkAppShared.ckMng.dtDatum.TimeOfDay.Equals(new TimeSpan(15, 0, 0))) {
           for (int iC = 0; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
             CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];
             if (clb.user != null) continue;

             for (int iP = 0; iP < clb.ltPlayer.Count; iP++) {
               clb.ltPlayer[iP].fCondition = 0.9f;
               clb.ltPlayer[iP].fFresh = 1.0f;
               clb.ltPlayer[iP].fMoral = Math.Max(clb.ltPlayer[iP].fMoral, 0.95f);
             }
           }
         }

         CkAppShared.ckMng.dtDatum = CkAppShared.ckMng.dtDatum.AddMinutes(-30);

         for (int iC = 0; iC < CkAppShared.ckMng.ltCups.Count; iC++) {
           CornerkickManager.Cup cp = CkAppShared.ckMng.ltCups[iC];

           if (cp.iId == 2) {
             cp.settings.iNeutral = 1;
           }
         }

         CkAppShared.ckMng.ltPlayer[3138].ltClubHistory[ckMng.ltPlayer[3138].ltClubHistory.Count - 1].iTransferFee -= 140000000;

         CornerkickGame.Stadium.Construction ctn = new CornerkickGame.Stadium.Construction();
         int[] iCostDays = CornerkickManager.Stadium.getCostDaysBuildBuilding(clbMirko, 9, 1);
         ctn.iLevelNew = 1;
         ctn.fDaysConstruct = iCostDays[1];
         ctn.fDaysConstructIni = iCostDays[1];
         clbMirko.buildings.bgMassTransit.ctn = ctn;

         //ckMng.dtDatum = CkAppShared.ckMng.dtDatum.AddMinutes(-60);

         CkAppShared.ckMng.ltClubs[ckMng.ltPlayer[4906].iClubId].ltPlayer.Remove(CkAppShared.ckMng.ltPlayer[4906]);
         CkAppShared.ckMng.ltClubs[0].ltPlayer.Add(CkAppShared.ckMng.ltPlayer[4906]);
         CkAppShared.ckMng.ltPlayer[4906].iClubId = 0;
         CkAppShared.ckMng.ltPlayer[4906].ltClubHistory.RemoveAt(CkAppShared.ckMng.ltPlayer[4906].ltClubHistory.Count - 1);

         CkAppShared.ckMng.ltClubs[ckMng.ltPlayer[4322].iClubId].ltPlayer.Remove(CkAppShared.ckMng.ltPlayer[4322]);
         CkAppShared.ckMng.ltClubs[0].ltPlayer.Add(CkAppShared.ckMng.ltPlayer[4322]);
         CkAppShared.ckMng.ltPlayer[4322].iClubId = 0;
         CkAppShared.ckMng.ltPlayer[4322].ltClubHistory.RemoveAt(CkAppShared.ckMng.ltPlayer[4322].ltClubHistory.Count - 1);

         for (int iPl = 0; iPl < CkAppShared.ckMng.ltPlayer.Count; iPl++) {
           CkAppShared.ckMng.ltPlayer[iPl].fMoral = 1f;
         }

         CkAppShared.ckMng.ltCups[1].draw(CkAppShared.ckMng.dtDatum);
         CkAppShared.ckMng.ltCups[2].draw(CkAppShared.ckMng.dtDatum);

         CornerkickGame.Player plSpeed = CkAppShared.ckMng.ltPlayer[142];
         string sDev = plSpeed.iSkill[0].ToString() + " -> ";
         plSpeed.iSkill[0]++;
         sDev += plSpeed.iSkill[0].ToString();
         CkAppShared.ckMng.sendNews(CkAppShared.ckMng.ltClubs[plSpeed.iClubId].user, CornerkickManager.Main.iNewsTypePlayerDevRnd, iId: plSpeed.iId, sReplace: plSpeed.sName, sReplace2: CornerkickManager.Player.sSkills[0], sReplace3: sDev);

         List<int> ltPlayerRetIx = CornerkickManager.Player.getRetiredPlayerIx(CkAppShared.ckMng.ltPlayer);
         foreach (int iPlRet in ltPlayerRetIx) {
           CkAppShared.ckMng.ltPlayer[iPlRet].sName = "";
         }

         for (int iC = 0; iC < CkAppShared.ckMng.ltCups.Count; iC++) {
           CornerkickManager.Cup cp = CkAppShared.ckMng.ltCups[iC];

           if (Math.Abs(cp.iId) == 3 || Math.Abs(cp.iId) == 4) {
             for (int iMd = 0; iMd < cp.ltMatchdays.Count; iMd++) {
               cp.ltMatchdays[iMd].dt = cp.ltMatchdays[iMd].dt.AddMinutes(315);

               for (int iGd = 0; iGd < cp.ltMatchdays[iMd].ltGameData.Count; iGd++) {
                 cp.ltMatchdays[iMd].ltGameData[iGd].dt = cp.ltMatchdays[iMd].dt;
               }
             }
           }
         }

         foreach (CornerkickGame.Player pl in new CornerkickGame.Player[] { CkAppShared.ckMng.ltPlayer[186], CkAppShared.ckMng.ltPlayer[3589], CkAppShared.ckMng.ltPlayer[206] }) {
           string sDev = pl.iSkill[0].ToString() + " -> ";
           pl.iSkill[0]++;
           sDev += pl.iSkill[0].ToString();
           CkAppShared.ckMng.sendNews(CkAppShared.ckMng.ltClubs[pl.iClubId].user, CornerkickManager.Main.iNewsTypePlayerDevRnd, iId: pl.iId, sReplace: pl.sName, sReplace2: CornerkickManager.Player.sSkills[0], sReplace3: sDev);
         }

         // Create new player
         CornerkickManager.Club club0 = CkAppShared.ckMng.ltClubs[0];
         while (CkAppShared.ckMng.ltPlayer.Count < 5000) {
           CornerkickGame.Player plNew = CkAppShared.ckMng.plr.newPlayer(club0, iNat: iNations[random.Next(iNations.Length)], bForceNew: true);
           CkAppShared.ckMng.tr.putPlayerOnTransferlist(plNew, 0);
         }
         CkAppShared.ckMng.dtSeasonEnd = CkAppShared.ckMng.dtSeasonEnd.AddDays(-5);

         CkAppShared.ckMng.ltPlayer[2930].fFootR = CkAppShared.ckMng.ltPlayer[2930].fFootL;
         CkAppShared.ckMng.ltPlayer[2930].fFootL = 1f;

         foreach (int iNat in iNations) {
           CornerkickManager.Club nat = CornerkickManager.Tool.getNation(iNat, CkAppShared.ckMng.ltClubs);
           if (nat == null) continue;
           if (nat.user != null) continue;

           // Add all player of that nation
           nat.ltPlayer = CkAppShared.ckMng.getBestPlayer(iNat, iPlCount: 22);
         }

         CornerkickManager.Club clbJanos = CkAppShared.ckMng.ltClubs[5];
         CornerkickManager.Finance.doTransaction(ref clbJanos, CkAppShared.ckMng.dtDatum, 20000000, "Zuschuss", 0);

         for (int iC = 0; iC < CkAppShared.ckMng.ltCups.Count; iC++) {
           CornerkickManager.Cup cp = CkAppShared.ckMng.ltCups[iC];

           if (Math.Abs(cp.iId) == 1) {
             cp.ltMatchdays[29].dt = cp.ltMatchdays[29].dt.AddDays(-7);

             for (int iGd = 0; iGd < cp.ltMatchdays[29].ltGameData.Count; iGd++) {
               cp.ltMatchdays[29].ltGameData[iGd].dt = cp.ltMatchdays[29].dt;
             }
           }
         }

         CornerkickManager.Club clbBW = CkAppShared.ckMng.ltClubs[2];
         clbBW.iBalance += 2422378;
         for (int iA = 0; iA < clbBW.ltAccount.Count; iA++) {
           CornerkickManager.Finance.Account ac = clbBW.ltAccount[iA];

           if (ac.iType == CornerkickManager.Finance.iTransferralTypePayStadium && ac.iValue == -3200000 && ac.dt.Day == 16) {
             ac.iValue += 2400000;
             clbBW.ltAccount[iA] = ac;
             break;
           }
         }
         clbBW.ltAccount.RemoveAt(clbBW.ltAccount.Count - 1);
         clbBW.ltAccount.RemoveAt(clbBW.ltAccount.Count - 1);

         bool bSet = false;
         for (int iA = 0; iA < clbMirko.ltAccount.Count; iA++) {
           CornerkickManager.Finance.Account ac = clbMirko.ltAccount[iA];

           if (ac.iType == CornerkickManager.Finance.iTransferralTypePayStadium && ac.iValue == -4000000 && ac.dt.Day == 18) {
             clbMirko.iBalance += 3000000;
             ac.iValue += 3000000;
             ac.iBalance += 3000000;
             clbMirko.ltAccount[iA] = ac;
             bSet = true;
           } else if (bSet) {
             ac.iBalance += 3000000;
             clbMirko.ltAccount[iA] = ac;
           }
         }

         long iUpdateBalance = 0;
         for (int iA = 0; iA < clbToni.ltAccount.Count; iA++) {
           CornerkickManager.Finance.Account ac = clbToni.ltAccount[iA];

           if (ac.iType == CornerkickManager.Finance.iTransferralTypePayMerchandising && (ac.iValue == -1456999 || ac.iValue == -2147999)) {
             clbToni.iBalance -= (int)ac.iValue;
             clbToni.ltAccount.RemoveAt(iA);
             iA--;
             iUpdateBalance += -ac.iValue;
           } else if (iUpdateBalance > 0) {
             ac.iBalance += iUpdateBalance;
             clbToni.ltAccount[iA] = ac;
           }
         }

         CkAppShared.ckMng.ltPlayer[101].iSkill[0] = 6;

         CornerkickManager.Club clbMirko = CkAppShared.ckMng.ltClubs[3];
         clbMirko.iBalance -= 6805000;
         clbMirko.ltAccount.RemoveAt(clbMirko.ltAccount.Count - 1);

         for (int iC = 0; iC < CkAppShared.ckMng.ltCups.Count; iC++) {
           CornerkickManager.Cup cp = CkAppShared.ckMng.ltCups[iC];

           if (Math.Abs(cp.iId) == 2) {
             cp.ltMatchdays[2].dt = cp.ltMatchdays[2].dt.AddDays(14);

             for (int iGd = 0; iGd < cp.ltMatchdays[2].ltGameData.Count; iGd++) {
               cp.ltMatchdays[2].ltGameData[iGd].dt = cp.ltMatchdays[2].dt;
             }
           }
         }

         CornerkickManager.Cup cupGold = CkAppShared.ckMng.tl.getCup(3);
         int iCp = cupGold.getPlace(CkAppShared.ckMng.ltClubs[1], CkAppShared.ckMng.dtDatum);
         Console.WriteLine(iCp);

         for (int iC = 0; iC < CkAppShared.ckMng.ltCups.Count; iC++) {
           CornerkickManager.Cup cupTg = CkAppShared.ckMng.ltCups[iC];
           if (Math.Abs(cupTg.iId) == iCupIdTestgame) cupTg.sName = "Testspiel";
         }

         for (int iC = 0; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
           CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];

           for (int iPl = 0; iPl < clb.ltPlayerJouth.Count; iPl++) {
             clb.ltPlayerJouth[iPl].fCondition = 0.5f;
             clb.ltPlayerJouth[iPl].fFresh = 1f;
             clb.ltPlayerJouth[iPl].fMoral = 1f;
             clb.ltPlayerJouth[iPl].fExperience = 0.5f;
           }
         }

         CornerkickManager.Club clbToni = CkAppShared.ckMng.ltClubs[1];

         for (int iC = 0; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
           CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];

           if (clb.bNation) {
             clb.ltPlayer.Clear();
             continue;
           }

           for (int iS = 0; iS < clb.ltSuccess.Count; iS++) {
             if (clb.ltSuccess[iS].cup.iId == 1) {
               clb.ltSuccess[iS].ltCupPlace.Clear();
               for (int iSn = 1; iSn < CkAppShared.ckMng.iSeason; iSn++) {
                 clb.ltSuccess[iS].ltCupPlace.Add(CornerkickManager.Tool.getLeaguePlace(Controllers.MemberController.getCup(iSn, 1, clb.iLand, clb.iDivision), clb));
               }
             }
           }
         }

         CornerkickManager.Cup cupWc = CkAppShared.ckMng.tl.getCup(7);
         cupWc.ltMatchdays[cupWc.ltMatchdays.Count - 1].dt = cupWc.ltMatchdays[cupWc.ltMatchdays.Count - 1].dt.AddDays(5);
         cupWc.ltMatchdays[cupWc.ltMatchdays.Count - 1].ltGameData[0].dt = cupWc.ltMatchdays[cupWc.ltMatchdays.Count - 1].dt;

         CornerkickManager.Club clbMirko = CkAppShared.ckMng.ltClubs[3];
         CornerkickManager.Finance.doTransaction(ref clbMirko, CkAppShared.ckMng.dtDatum, -15250000, "Bau Trainingsgelände", CornerkickManager.Finance.iTransferralTypePayStadiumSurr);

         for (int iC = 0; iC < CkAppShared.ckMng.ltClubs.Count; iC++) {
           CornerkickManager.Club clbNat = CkAppShared.ckMng.ltClubs[iC];
           if (clbNat.bNation) clbNat.ltPlayer.Clear();
         }
         */
        // END TMP section
#endif

#if _WebApp
        CkAppShared.dtLoadCk = CkAppShared.ckMng.dtDatum;
#endif

        string sFileLastState = Path.Combine(sAppDataDir, CkAppShared.sFilenameSettings);
#if !DEBUG
#if _USE_AMAZON_S3
        //if (!File.Exists(sFileLastState)) as3.downloadFile("laststate", sFileLastState);
        as3.downloadFile(as3.sCkInstanceName + CkAppShared.sFilenameSettings, sFileLastState);
#endif

        if (File.Exists(sFileLastState)) {
          CkAppShared.ckMng.tl.writeLog("Reading laststate from file: " + sFileLastState);

          string[] sStateFileContent = File.ReadAllLines(sFileLastState);

          DateTime dtLast = new DateTime();
          if (sStateFileContent.Length > 3) {
            //double fInterval = getIntervalAve(); // Calendar interval [s]
            double fInterval = 0.0; // Calendar interval [s]

            NumberStyles style = NumberStyles.Number | NumberStyles.AllowDecimalPoint;
            fInterval = double.Parse(sStateFileContent[0], style, CultureInfo.InvariantCulture);
            CkAppShared.ckMng.tl.writeLog("Set calendar interval to " + fInterval.ToString("0.000") + "s");

            bool bCalendarRunning = false;
            bool.TryParse(sStateFileContent[1], out bCalendarRunning);

            if (fInterval > 10.0 && bCalendarRunning && DateTime.TryParse(sStateFileContent[2], out dtLast)) {
              //double fTotalMin = (DateTime.Now - dtLast).TotalMinutes;
              //int nSteps = (int)(fTotalMin / (fInterval / 60f));
              int nSteps = getDeltaStepsBetweenNowAndApproach();

              if (nSteps > 0) {
                CkAppShared.ckMng.tl.writeLog("Last step was at " + dtLast.ToString("s", CultureInfo.InvariantCulture) + " (ck: " + CkAppShared.ckMng.dtDatum.ToString("s", CultureInfo.InvariantCulture) + ") - now: " + DateTime.Now.ToString("s", CultureInfo.InvariantCulture) + " (ck approach: " + getCkApproachDate().ToString("s", CultureInfo.InvariantCulture) + ")");

                int iGameSpeed = 0; // Calendar interval [s]
                int.TryParse(sStateFileContent[3], out iGameSpeed);

                // Perform calendar steps in background
                Task<bool> tkPerformCalendarSteps = Task.Run(() => performCalendarSteps(fInterval, iGameSpeed, bCalendarRunning));
              } else {
                CkAppShared.timerCkCalender.Interval = fInterval * 1000.0; // Convert [s] to [ms]
                CkAppShared.timerCkCalender.Enabled = bCalendarRunning;
                CkAppShared.ckMng.tl.writeLog("Calendar Interval set to " + CkAppShared.timerCkCalender.Interval.ToString() + " ms");
              }
            }

            if (sStateFileContent.Length > 5) {
              bool.TryParse(sStateFileContent[4], out CkAppShared.settings.bEmailCertification);
              bool.TryParse(sStateFileContent[5], out CkAppShared.settings.bRegisterDuringGame);
            }

            if (sStateFileContent.Length > 6) bool.TryParse(sStateFileContent[6], out CkAppShared.settings.bMaintenance);
            if (sStateFileContent.Length > 7) CkAppShared.settings.sInfo = sStateFileContent[7];
            if (sStateFileContent.Length > 8) DateTime.TryParse(sStateFileContent[8], out CkAppShared.settings.dtCounterStart);
          }
        } else {
          CkAppShared.ckMng.tl.writeLog("laststate file '" + sFileLastState + "' does not exist");
        }

#if _USE_AMAZON_S3
        // Download emblems
        Task<bool> tkDownloadEmblems = Task.Run(async () => await downloadFilesAsync(as3.sCkInstanceName + "emblems/", sAppDataDir, ".png"));

        // Download portraits
        Task<bool> tkDownloadPortraits = Task.Run(async () => await downloadFilesAsync(as3.sCkInstanceName + "portraits/", sAppDataDir, ".png"));

        // Download mails
        Task<bool> tkDownloadMail = Task.Run(async () => await downloadMailsAsync(sAppDataDir));

        // Download wishlist
        as3.downloadFile("wishlist.json", Path.Combine(sAppDataDir, "wishlist.json"));

        // Download archive cups
        if (!Directory.Exists(Path.Combine(sAppDataDir, "archive"))) Directory.CreateDirectory(Path.Combine(sAppDataDir, "archive"));

        for (int iS = 1; iS < CkAppShared.ckMng.iSeason; iS++) {
          string sCupDir = Path.Combine(sAppDataDir, "archive", iS.ToString());
          string sCupKey = "archive/" + iS.ToString() + "/Cup";

          if (!Directory.Exists(sCupDir)) Directory.CreateDirectory(sCupDir);

          try {
            as3.downloadFile(as3.sCkInstanceName + sCupKey, Path.Combine(sCupDir, "Cup"));
          } catch {
            CkAppShared.ckMng.tl.writeLog("ERROR: Unable to download cups", CornerkickManager.Main.sErrorFile);
          }
        }

        // Download archive games
        /*
        try {
          Task<bool> tkDownloadGames = Task.Run(async () => await downloadFilesAsync(as3.sCkInstanceName + "save/games/", sAppDataDir, ".ckgx"));
        } catch {
          CkAppShared.ckMng.tl.writeLog("ERROR: Unable to download games", CornerkickManager.Main.sErrorFile);
        }
        */
#endif
#else
        readMails();
#endif

        // Stop stopwatch
        swLoad.Stop();
        TimeSpan tsLoad = swLoad.Elapsed;

        // Write elapsed time to log
        CkAppShared.ckMng.tl.writeLog("Elapsed time during load: " + tsLoad.TotalSeconds.ToString("0.000") + "s");

        // If no calendar timer --> enable save timer to save every 15 min.
        CkAppShared.timerSave.Enabled = !CkAppShared.timerCkCalender.Enabled;

        CkAppShared.iLoadState = 0; // Success

        return true;
      }

      // If error while loading ...
      CkAppShared.timerSave.Enabled = false; // ... do not overwrite the file
      CkAppShared.settings.bLoginPossible = false; // ... disable user login

      CkAppShared.iLoadState = 3; // Error or new game

      return false;
    }

#if _USE_AMAZON_S3
    private static async Task<bool> downloadMailsAsync(string sHomeDir)
    {
      await as3.downloadAllFilesAsync(as3.sCkInstanceName + "mail/", sHomeDir, null, ".txt");

      readMails();

      return true;
    }
#endif

    private static void readMails()
    {
      string sDirMail = Path.Combine(CkAppShared.ckMng.settings.sHomeDir, "mail");
      if (System.IO.Directory.Exists(sDirMail)) {
        CkAppShared.ltMail = new List<CkAppShared.Mail>();

        DirectoryInfo diMail = new DirectoryInfo(sDirMail);

        foreach (var fileMail in diMail.GetFiles("*.txt")) {
          string sContent = System.IO.File.ReadAllText(fileMail.FullName);
          string[] sContentSplit = sContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

          string[] sHeader = sContentSplit[0].Split();
          if (sHeader.Length < 3) continue;

          string sToId = sHeader[0];
          string sFromId = sHeader[1];
          string sDate = sHeader[2];
          bool bNew = true;
          if (sHeader.Length > 3) bNew = sHeader[3].Equals("true");

          DateTime dtMail = DateTime.ParseExact(sDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

          CkAppShared.Mail mail = new CkAppShared.Mail();
          mail.sIdTo = sToId;
          mail.sIdFrom = sFromId;
          mail.bNew = bNew;
          mail.dt = dtMail;
          mail.sText = "";
          for (int iL = 1; iL < sContentSplit.Length; iL++) { // For each line of text
            mail.sText += sContentSplit[iL] + Environment.NewLine;
          }

          CkAppShared.ltMail.Add(mail);

          // Delete mail file after import
          try {
            fileMail.Delete();
          } catch {
            CkAppShared.ckMng.tl.writeLog("Unable to delete mail: " + fileMail.FullName, CornerkickManager.Main.sErrorFile);
          }
        }
      }
    }

    private static bool performCalendarSteps(double fInterval, int iGameSpeed, bool bCalendarRunning)
    {
      // Create stopwatch
      System.Diagnostics.Stopwatch swCalSteps = new System.Diagnostics.Stopwatch();

      // Start stopwatch
      swCalSteps.Start();

      // Temp. set game speed to 0 for fast calculation
      Controllers.AdminController.setGameSpeedToAllUsers(0);

      int nSteps = getDeltaStepsBetweenNowAndApproach();

      if (nSteps > 0) {
        CkAppShared.iLoadState = 2;

        CkAppShared.ckMng.tl.writeLog("Performing approx. " + nSteps.ToString() + " calendar steps");

        int iS = 0;
        while (getCkApproachDate().CompareTo(CkAppShared.ckMng.dtDatum) > 0) {
          try {
            bool bBreak = !performCalendarStep(false);
            if (bBreak) {
              bCalendarRunning = false;
              break;
            }
          } catch (Exception e) {
            CkAppShared.ckMng.tl.writeLog("performCalendarStepsAsync(): Error in performCalendarStep() at step: " + iS.ToString() + Environment.NewLine + e.Message + e.StackTrace, CornerkickManager.Main.sErrorFile);
          }

          iS++;
        }
      }

      CkAppShared.timerCkCalender.Interval = fInterval * 1000.0; // Convert [s] to [ms]
      CkAppShared.timerCkCalender.Enabled = bCalendarRunning;

      CkAppShared.ckMng.tl.writeLog("Calendar Interval set to " + CkAppShared.timerCkCalender.Interval.ToString() + " ms");

      CkAppShared.ckMng.tl.writeLog("Game speed set to " + iGameSpeed.ToString() + "ms");
      Controllers.AdminController.setGameSpeedToAllUsers(iGameSpeed);

      // Stop stopwatch
      swCalSteps.Stop();

      // Write elapsed time to log
      CkAppShared.ckMng.tl.writeLog("Elapsed time while performing calendar steps: " + swCalSteps.Elapsed.TotalSeconds.ToString("0.000") + "s");

      CkAppShared.iLoadState = 0;

      return true;
    }

    private static bool performPastGamesAsync(DateTime dtNow)
    {
      List<Task<bool>> ltTkGames = new List<Task<bool>>();

      foreach (CornerkickManager.Cup cp in CkAppShared.ckMng.ltCups) {
        if (Math.Abs(cp.iId) == CkAppShared.iCupIdTestgame) continue; // No test-games

        for (int iMd = 0; iMd < cp.ltMatchdays.Count; iMd++) {
          if (cp.ltMatchdays[iMd].dt.CompareTo(dtNow) < 0) { // Game is in past
            foreach (CornerkickGame.Game.Data gd in cp.ltMatchdays[iMd].ltGameData) {
              if (gd.team[0].iGoals < 0 ||
                  gd.team[1].iGoals < 0) {
                CornerkickGame.Game.Data gd2 = CkAppShared.ckMng.tl.setGameData(gd);
                ltTkGames.Add(Task.Run(() => CkAppShared.ckMng.doGame(gd2, bRunGameInBackground: true, cup: cp)));
              }
            }
          }
        }
      }

      Task<bool[]> tkGames = Task.Run(async () => await Task.WhenAll(ltTkGames));
      tkGames.Wait();

      return true;
    }
#endif

#if _USE_AMAZON_S3
    private static async Task<bool> downloadFilesAsync(string sS3SubDir, string sTargetPath, string sFiles)
    {
      await as3.downloadAllFilesAsync(sS3SubDir, sTargetPath, null, sFiles);

      return true;
    }
#endif

    public static CornerkickManager.Main getCkMngDefault(bool bContinuingTime = false)
    {
      CornerkickManager.Main ckMngDefault = new CornerkickManager.Main(sHomeDir: CkAppShared.sHomeDir,
                                                                       sLogDir: CkAppShared.sAppDataDir,
                                                                       bContinuingTime: bContinuingTime,
                                                                       iTrainingsPerDay: 3,
                                                                       iTrainingsPerDayMax: 3,
                                                                       bPlayerTransferOnlyOncePerSeason: true,
                                                                       iWriteGamesToDisk: 0,
                                                                       fMoralMin: 0.4f);

      return ckMngDefault;
    }
    public static async Task<CornerkickManager.Main> setCkMngToDefault(CornerkickManager.Main ckMngDefault, IProgress<int[]> progress)
    {
      ckMngDefault.dtDatum = new DateTime(DateTime.Now.Year, ckMngDefault.dtDatum.Month, ckMngDefault.dtDatum.Day);

      // New game
      DateTime dtLeagueStart;
      DateTime dtLeagueEnd;
      ckMngDefault.setSeasonStartEndDates(out dtLeagueStart, out dtLeagueEnd);

      const byte nDivisions = 1;

      /////////////////////////////////////////////////////////////////////
      // Create nat. Cups and Leagues
      int processCount = await Task.Run<int>(() => {
        int iCount = 0;

        foreach (int iLand in CkAppShared.iNations) {
          // Create nat. cup
          CornerkickManager.Cup cup = new CornerkickManager.Cup(bKo: true);
          cup.iId = CkAppShared.iCupIdNatCup;
          cup.iId2 = iLand;
          cup.iId3 = 0;
          cup.sName = "Pokal";
          if (CornerkickManager.Main.sLand != null && CornerkickManager.Main.sLand.Length > iLand) cup.sName += " " + CornerkickManager.Main.sLand[iLand];
          cup.settings.fAttraction = 1.0f;
          cup.settings.iNeutral = 1;
          cup.settings.iBonusCupWin = 8000000; // 8 mio.
          cup.settings.bBonusReleaseCupWinInKo = true;
          cup.ltQualification = new List<CornerkickManager.Cup.Qualification>();
          ckMngDefault.ltCups.Add(cup);

          // Create nat. indoor cup
          CornerkickManager.Cup cupIndoor = new CornerkickManager.Cup(bKo: true);
          cupIndoor.iId = CkAppShared.iCupIdNatCup;
          cupIndoor.iId2 = iLand;
          cupIndoor.iId3 = 1;
          cupIndoor.sName = "Hallenpokal";
          if (CornerkickManager.Main.sLand != null && CornerkickManager.Main.sLand.Length > iLand) cupIndoor.sName += " " + CornerkickManager.Main.sLand[iLand];
          cupIndoor.settings.fAttraction = 0.5f;
          cupIndoor.settings.iNeutral = 1;
          cupIndoor.settings.nGroups = 4 * nDivisions;
          cupIndoor.settings.nQualifierKo = 2;

          cupIndoor.settings.nGameMin = 12;
          cupIndoor.settings.nPlStart = 6;
          cupIndoor.settings.nPlRes = 99;
          cupIndoor.settings.nSubstitutions = 99;
          cupIndoor.settings.bOffsite = false;
          cupIndoor.settings.fPitchSizeRel = 0.25f;

          cupIndoor.settings.iStart = 20;
          cupIndoor.settings.iEnd   = 24;

          cupIndoor.settings.iBonusCupWin = 2000000; // 2 mio.
          cupIndoor.settings.bBonusReleaseCupWinInKo = true;
          cupIndoor.ltQualification = new List<CornerkickManager.Cup.Qualification>();
          ckMngDefault.ltCups.Add(cupIndoor);

          // Create leagues
          for (byte iD = 0; iD < nDivisions; iD++) {
            progress.Report([iLand, iD, iCount]);

            CornerkickManager.Cup league = new CornerkickManager.Cup(nGroups: 1, bGroupsTwoGames: true);
            league.iId = CkAppShared.iCupIdLeague;
            league.iId2 = iLand;
            league.iId3 = iD;
            league.sName = (iD + 1).ToString() + ". Liga";
            if (CornerkickManager.Main.sLand != null && CornerkickManager.Main.sLand.Length > iLand) league.sName += " " + CornerkickManager.Main.sLand[iLand];
            league.settings.fAttraction = 1.0f - (iD * 0.25f);
            ckMngDefault.ltCups.Add(league);

            fillLeaguesWithCpuClubs(ckMngDefault, league, new List<CornerkickManager.Cup>() { cup, cupIndoor });

            /*
            Task tskWait = Task.Delay(2000);
            tskWait.Wait();
            */

            cup.ltQualification.Add(
              new CornerkickManager.Cup.Qualification() {
                cup = league,
                iPlaceFirst = 1,
                iPlaceLast = 0
              });
            cupIndoor.ltQualification.Add(
              new CornerkickManager.Cup.Qualification() {
                cup = league,
                iPlaceFirst = 1,
                iPlaceLast = 0
              });

            iCount++;
          }
        }

        /////////////////////////////////////////////////////////////////////
        // Create Internat. cups
        createCupGold(ckMngDefault);
        createCupSilver(ckMngDefault);
        createCupBronze(ckMngDefault);
        createCupWc(ckMngDefault, dtLeagueEnd);

        ckMngDefault.calcMatchdays();

        foreach (CornerkickManager.Cup cup in ckMngDefault.ltCups) cup.draw(ckMngDefault.dtDatum);

        // Set clubs next game
        for (int iC = 0; iC < ckMngDefault.ltClubs.Count; iC++) ckMngDefault.ltClubs[iC].nextGame = ckMngDefault.tl.getNextGame(ckMngDefault.ltClubs[iC], ckMngDefault.dtDatum);

        ckMngDefault.dtDatum = ckMngDefault.dtSeasonStart;

        return iCount;
      });

      return ckMngDefault;
    }

    public static int getCooperationIncome(float fClubAttrFac, bool bDaughterClub)
    {
      if (bDaughterClub) return (int)(fClubAttrFac * 500);

      return (int)(fClubAttrFac * 200);
    }

    public static CornerkickManager.Club? createClub(CornerkickManager.Main ckMngTmp, string sTeamname, byte iLand, byte iLiga, CornerkickManager.Club? clbReplace = null)
    {
      if (ckMngTmp == null) {
        return null;
      }

      CornerkickManager.Club clb = createClub(sTeamname, iLand, iLiga, 0, clbReplace: clbReplace);
      if (string.IsNullOrEmpty(clb.sName)) clb.sName = "Team";

#if _WebApp
      if (clbReplace == null) clb.iId = ckMngTmp.ltClubs.Count + 1; // If web app: keep id 0 free for admin club
#else
      if (clbReplace == null) clb.iId = ckMngTmp.ltClubs.Count;
#endif

      return clb;
    }

    public static CornerkickManager.Club createClub(string sTeamname, byte iLand, byte iLiga, int iUserLevel, CornerkickManager.Club? clbReplace = null)
    {
      CornerkickManager.Club? clb = clbReplace;
      if (clb == null) clb = new CornerkickManager.Club();

      clb.sName = sTeamname.Trim();
      if (string.IsNullOrEmpty(clb.sName)) clb.sName = "Team";

      if (clbReplace == null) {
        clb.iId = Tool.getFirstAvailable(CkAppShared.ckMng.ltClubs.Select(c => c.iId).ToList());
      }

      clb.iLand = iLand;
      clb.iDivision = iLiga;

      // Set random jersey colors
      if (clbReplace == null) {
        for (byte iC = 0; iC < 3; iC++) {
          byte[] bRbg = new byte[3];
          CkAppShared.random.NextBytes(bRbg);
          clb.cl1[iC] = System.Drawing.Color.FromArgb(bRbg[0], bRbg[1], bRbg[2]);
          CkAppShared.random.NextBytes(bRbg);
          clb.cl2[iC] = System.Drawing.Color.FromArgb(bRbg[0], bRbg[1], bRbg[2]);
        }
      }

      // Reset club tactic
      clb.ltTactic.Clear();
      clb.ltTactic.Add(new CornerkickGame.Tactic());
      if (CkAppShared.ckMng.ltFormationen?.Count > 0) clb.ltTactic[0].formation = CkAppShared.ckMng.ltFormationen[CkAppShared.random.Next(CkAppShared.ckMng.ltFormationen.Count)].Clone();

#if DEBUG
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = CkAppShared.ckMng.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 2 });
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = CkAppShared.ckMng.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 3 });
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = CkAppShared.ckMng.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 4 });
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = CkAppShared.ckMng.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 6 });
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = CkAppShared.ckMng.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 9 });
      clb.training.ltUnit.Add(new CornerkickManager.Main.TrainingPlan.Unit() { dt = CkAppShared.ckMng.dtDatum.Date.AddDays(1).Add(new TimeSpan(9, 30, 0)), iType = 1 });
#endif

      // Set default stadium
      clb.stadium = getDefaultStadium();

      // Clear sponsors
      clb.sponsorMain = new CornerkickManager.Finance.Sponsor();
      clb.ltSponsorBoards.Clear();
      clb.ltSponsorOffers.Add(createDefaultSponsor(iUserLevel, true,  1));
      clb.ltSponsorOffers.Add(createDefaultSponsor(iUserLevel, false, 1));
      clb.ltSponsorOffers.Add(createDefaultSponsor(iUserLevel, false, 2));
      clb.ltSponsorOffers.Add(createDefaultSponsor(iUserLevel, false, 3));
      clb.ltSponsorOffers.Add(createDefaultSponsor(iUserLevel, false, 4));

      // Clear account
      clb.ltAccount.Clear();

      // Remove staff
      clb.staff = new CornerkickManager.Main.Staff();

      // Clear training
      clb.training = new CornerkickManager.Main.TrainingPlan();

      // Clear captain
      clb.iCaptainId = new int[3] { -1, -1, -1 };

      // Clear record games
      clb.ltGameRecord.Clear();

      // Stadium
      clb.stadium.sName = clb.sName + " Stadion";
      clb.stadium.facility.iTicketcounter = 1;
      clb.stadium.facility.iCarpark = 5;
      clb.buildings.iGround = (byte)CornerkickManager.Stadium.getRequiredGrounds(clb);

      clb.iAdmissionPrice[0] = 10;
      clb.iAdmissionPrice[1] = 30;
      clb.iAdmissionPrice[2] = 100;

      clb.iAdmissionPriceSeasonal[0] = 200;
      clb.iAdmissionPriceSeasonal[1] = 600;
      clb.iAdmissionPriceSeasonal[2] = 2000;
      clb.fSeasonalTicketsMaxFrac = 0f;

      // Clear position last season (for new sponsors)
      clb.iPosLastSeason = 0;

      // Clear successes
      clb.ltSuccess.Clear();

      return clb;
    }

    public static CornerkickGame.Stadium getDefaultStadium(bool bBig = false, bool bTiny = false)
    {
      CornerkickGame.Stadium stadium = new CornerkickGame.Stadium();

      if (bBig) {
        stadium.sName = "Großes Stadion";

        stadium.blocks[0].iSeats = 4000;
        stadium.blocks[0].iType = 1;
        stadium.blocks[1].iSeats = 500;
        stadium.blocks[1].iType = 2;
        stadium.blocks[2].iSeats = 4000;
        stadium.blocks[2].iType = 1;
        for (byte iB = 5; iB < 8; iB++) {
          stadium.blocks[iB].iSeats = 4000;
          stadium.blocks[iB].iType = 1;
        }
        for (byte iB = 3; iB <= 4; iB++) stadium.blocks[iB].iSeats = 6000;
        for (byte iB = 8; iB <= 9; iB++) stadium.blocks[iB].iSeats = 6000;
        for (byte iB = 10; iB < 24; iB++) {
          stadium.blocks[iB].iSeats = 1500;
          stadium.blocks[iB].iType = 1;
        }

        stadium.facility.bTopring = true;
        stadium.facility.iTicketcounter = 5;
        stadium.facility.iVideo = 3;
        stadium.facility.iSnackbar = 10;
        stadium.facility.iToilets = 15;
        stadium.facility.iSecurity = 3;
        stadium.facility.iCarpark = 5000;
      } else if (bTiny) {
        stadium.sName = "Trainingsplatz";

        for (byte iB = 0; iB < 10; iB++) stadium.blocks[iB].iSeats = 100;

        stadium.facility.iSnackbar = 1;
      } else {
        stadium.sName = "Kleines Stadion";

        for (byte iB = 0; iB < 3; iB++) {
          stadium.blocks[iB].iSeats = 2000;
          stadium.blocks[iB].iType = 1;
        }
        for (byte iB = 5; iB < 8; iB++) {
          stadium.blocks[iB].iSeats = 2000;
          stadium.blocks[iB].iType = 1;
        }
        for (byte iB = 3; iB <= 4; iB++) stadium.blocks[iB].iSeats = 3000;
        for (byte iB = 8; iB <= 9; iB++) stadium.blocks[iB].iSeats = 3000;

        stadium.facility.iTicketcounter = 1;
        stadium.facility.iVideo = 1;
        stadium.facility.iSnackbar = 2;
        stadium.facility.iToilets = 4;
        stadium.facility.iSecurity = 1;
        stadium.facility.iCarpark = 50;
      }

      return stadium;
    }

    public static CornerkickManager.Finance.Sponsor createDefaultSponsor(int iLevel, bool bMainSponsor, byte iId)
    {
      CornerkickManager.Finance.Sponsor sponUser = new CornerkickManager.Finance.Sponsor();

      if (iLevel > 3) return sponUser;

      sponUser.iType = (byte)(bMainSponsor ? 0 : 1);
      sponUser.iId = iId;
      sponUser.iYears = 1;
      if (bMainSponsor) {
        sponUser.iGeldJahr = 20000000 - (iLevel * 5000000); // 15 mio.
        sponUser.iGeldMeister = 1000000; //  1 mio.
        sponUser.iMoneyVicHome = 50000;
      } else {
        sponUser.nBoards = 1;
        sponUser.iMoneyVicHome = 10000;
      }
      sponUser.fMood = 1f;

      return sponUser;
    }

    public static void addFreelancerScouts(CornerkickManager.User usr)
    {
      if (usr?.club?.staff == null) return;

      if (usr.club.staff.ltScouts == null) usr.club.staff.ltScouts = new List<CornerkickManager.Main.Staff.Scout>();
      foreach (CornerkickManager.Main.Staff.Scout scFl in CornerkickManager.Main.staff.ltScouts.FindAll(s => s.bFreelancer)) {
        if (!usr.club.staff.ltScouts.Any(s => s.iId == scFl.iId)) usr.club.staff.ltScouts.Add(scFl.Clone(bReduced: true));
      }
    }

    public static void addPlayerToClub(CornerkickManager.Main ckMngTmp, ref CornerkickManager.Club club, float fSkillAve = 0f, int iSkillChange = 0, bool bScouting = false, bool bInitial = false)
    {
      int iSpeed = 0;
      float fTalent = 0;
      float fAlter = 0f;

      byte[] nPl = [
        2,
        3,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2,
        2
      ];
      for (byte iPos = 1; iPos < nPl.Length + 1; iPos++) {
        for (byte iPl = 0; iPl < nPl[iPos - 1]; iPl++) {
          CornerkickManager.Player pl = ckMngTmp.plt.newPlayer(fSkillAve, club: club, iPos: iPos, bForceNew: true, bScouting: bScouting, bInitial: bInitial);
#if DEBUG
          pl.plGame.fFresh = 1f;
#endif

          // Change player skill
          if (iSkillChange != 0) {
            for (int iS = 1; iS < pl.plGame.iSkill.Length; iS++) {
              pl.plGame.iSkill[iS] = (byte)Math.Max(pl.plGame.iSkill[iS] + iSkillChange, 2);
            }
          }

          // Set jersey number
          pl.plGame.iNr = (byte)(iPos + (11 * iPl));

          // Set random portrait
          setPlayerRandomPortrait(pl);

          // Count speed
          iSpeed += pl.plGame.iSkill[0];

          // Count talent
          fTalent += pl.getTalentAve();

          // Count age
          float fAge = pl.plGame.getAge(ckMngTmp.dtDatum);
          fAlter += fAge;
        }
      }

      // Equalize player speed
      byte iCount7 = 0;
      byte iCount5 = 0;
      for (int iPl = 0; iPl < club.ltPlayer.Count; iPl++) {
        CornerkickManager.Player pl = club.ltPlayer[iPl];
        pl.plGame.iSkill[CornerkickGame.Game.iSkillIxSpeed] = 6;
      }

      while (iCount7 < 2 || iCount5 < 1) {
        CornerkickManager.Player pl = club.ltPlayer[CkAppShared.random.Next(club.ltPlayer.Count)];

        if (!pl.plGame.checkMainPos(1)) { // if not keeper
          if (iCount7 < 2) {
            pl.plGame.iSkill[CornerkickGame.Game.iSkillIxSpeed] = 7;
            iCount7++;
          } else if (iCount5 < 1) {
            pl.plGame.iSkill[CornerkickGame.Game.iSkillIxSpeed] = 5;
            iCount5++;
          }
        }
      }
      /*
      while (iSpeed < 6 * club.ltPlayer.Count) {
        for (byte iPl = 0; iPl < club.ltPlayer.Count; iPl++) {
          CornerkickGame.Player pl = ckMngTmp.ltPlayer[club.ltPlayer[iPl]];
          if (pl.iF[0] < 6) {
            pl.iF[0]++;
            ckMngTmp.ltPlayer[club.ltPlayer[iPl]] = pl;

            iSpeed++;
            break;
          }
        }
      }

      while (iSpeed > 6 * club.ltPlayer.Count) {
        for (byte iPl = 0; iPl < club.ltPlayer.Count; iPl++) {
          CornerkickGame.Player pl = ckMngTmp.ltPlayer[club.ltPlayer[iPl]];
          if (pl.iF[0] > 6) {
            pl.iF[0]--;
            ckMngTmp.ltPlayer[club.ltPlayer[iPl]] = pl;

            iSpeed++;
            break;
          }
        }
      }
      */

      // Equalize player talent
      while (fTalent > 4.51 * club.ltPlayer.Count) {
        CornerkickManager.Player pl = club.ltPlayer[CkAppShared.random.Next(club.ltPlayer.Count)];
        if (pl.getTalentAve() > 2f) {
          for (byte iT = 0; iT < pl.iTalent.Length; iT++) {
            if (pl.iTalent[iT] > 0) pl.iTalent[iT]--;
          }

          fTalent -= 1f;
        }
      }

      while (fTalent < 4.49 * club.ltPlayer.Count) {
        CornerkickManager.Player pl = club.ltPlayer[CkAppShared.random.Next(club.ltPlayer.Count)];
        if (pl.getTalentAve() < 7f) {
          for (byte iT = 0; iT < pl.iTalent.Length; iT++) {
            if (pl.iTalent[iT] < 9) pl.iTalent[iT]++;
          }

          fTalent += 1f;
        }
      }

      // Equalize player age
      while (fAlter > 24.0f * club.ltPlayer.Count) {
        CornerkickManager.Player pl = club.ltPlayer[CkAppShared.random.Next(club.ltPlayer.Count)];

        if (pl.plGame.getAge(ckMngTmp.dtDatum) > 22) {
          try {
            pl.plGame.dtBirthday = new DateTime(pl.plGame.dtBirthday.Year + 1, pl.plGame.dtBirthday.Month, pl.plGame.dtBirthday.Day); // make younger
          } catch {
            continue;
          }

          fAlter -= 1f;
        }
      }

      while (fAlter < 24.0f * club.ltPlayer.Count) {
        CornerkickManager.Player pl = club.ltPlayer[CkAppShared.random.Next(club.ltPlayer.Count)];

        if (pl.plGame.getAge(ckMngTmp.dtDatum) < 30) {
          try {
            pl.plGame.dtBirthday = new DateTime(pl.plGame.dtBirthday.Year - 1, pl.plGame.dtBirthday.Month, pl.plGame.dtBirthday.Day); // make older
          } catch {
            continue;
          }

          fAlter += 1f;
        }
      }

      // Equalize factor of player (age - 10) * talent 
      // --> (24.0 - 10) * 4.5 = 63.0
      int iBreak = 0;
      float fAgeTalent = getAgeTalent(ckMngTmp, club);
      while (fAgeTalent > 64.0 || fAgeTalent < 62.0) {
        CornerkickManager.Player pl1 = club.ltPlayer[CkAppShared.random.Next(club.ltPlayer.Count)];
        CornerkickManager.Player pl2 = club.ltPlayer[CkAppShared.random.Next(club.ltPlayer.Count)];

        if (pl1.getTalentAve() > 2f && pl1.getTalentAve() < 7f &&
            pl2.getTalentAve() > 2f && pl2.getTalentAve() < 7f) {
          int iDeltaTalent = +1;
          if ((fAgeTalent > 70.75 && pl1.plGame.dtBirthday.Year > pl2.plGame.dtBirthday.Year) ||
              (fAgeTalent < 68.75 && pl1.plGame.dtBirthday.Year < pl2.plGame.dtBirthday.Year)) iDeltaTalent = -1;

          for (byte iT = 0; iT < pl1.iTalent.Length; iT++) {
            if (pl1.iTalent[iT] < 1 && iDeltaTalent > 0) continue;
            if (pl1.iTalent[iT] > 9 && iDeltaTalent < 0) continue;

            pl1.iTalent[iT] = (byte)(pl1.iTalent[iT] - iDeltaTalent);
          }

          for (byte iT = 0; iT < pl2.iTalent.Length; iT++) {
            if (pl2.iTalent[iT] < 1 && iDeltaTalent < 0) continue;
            if (pl2.iTalent[iT] > 9 && iDeltaTalent > 0) continue;

            pl2.iTalent[iT] = (byte)(pl2.iTalent[iT] + iDeltaTalent);
          }

          fAgeTalent = getAgeTalent(ckMngTmp, club);
        } else if (pl1.plGame.getAge(ckMngTmp.dtDatum) > 22 &&
                   pl2.plGame.getAge(ckMngTmp.dtDatum) < 30) {
          try {
            pl1.plGame.dtBirthday = new DateTime(pl1.plGame.dtBirthday.Year + 1, pl1.plGame.dtBirthday.Month, pl1.plGame.dtBirthday.Day); // make younger
          } catch {
            ckMngTmp.tl.writeLog("ERROR: cannot make player " + pl1.plGame.sName + " younger. Player birthday: " + pl1.plGame.dtBirthday.ToShortDateString(), CornerkickManager.Main.sErrorFile);
          }

          try {
            pl2.plGame.dtBirthday = new DateTime(pl2.plGame.dtBirthday.Year - 1, pl2.plGame.dtBirthday.Month, pl2.plGame.dtBirthday.Day); // make older
          } catch {
            ckMngTmp.tl.writeLog("ERROR: cannot make player " + pl2.plGame.sName + " older. Player birthday: " + pl2.plGame.dtBirthday.ToShortDateString(), CornerkickManager.Main.sErrorFile);
          }

          fAgeTalent = getAgeTalent(ckMngTmp, club);
        }

        iBreak++;
        if (iBreak > 1000) {
          break;
        }
      }
    }

    private static float getAgeTalent(CornerkickManager.Main ckMngTmp, CornerkickManager.Club club)
    {
      float fAgeTalent = 0f;

      // Count age
      foreach (CornerkickManager.Player pl in club.ltPlayer) {
        fAgeTalent += (pl.plGame.getAge(ckMngTmp.dtDatum) - 10f) * pl.getTalentAve();
      }

      return fAgeTalent / club.ltPlayer.Count;
    }

    private static void setPlayerRandomPortrait(CornerkickManager.Player pl)
    {
      if (pl == null) return;

      try {
        if (pl.clSkin.B == 0) {
#if _WebApp
          int nPortraitFiles = 240;
          ushort iPortraitId = (ushort)CkAppShared.random.Next(nPortraitFiles);

          byte[] b = BitConverter.GetBytes(iPortraitId);

          pl.clSkin = System.Drawing.Color.FromArgb(b[0], b[1], 1);
#else

          string sDirPortrait = Path.Combine(CkAppShared.sHomeDir, "Content", "Images", "portraits");

          if (Directory.Exists(sDirPortrait)) {
            DirectoryInfo diPortrait = new DirectoryInfo(sDirPortrait);

            int nPortraitFiles = diPortrait.GetFiles("*.png").Length;
            ushort iPortraitId = (ushort)CkAppShared.random.Next(nPortraitFiles);

            byte[] b = BitConverter.GetBytes(iPortraitId);

            pl.clSkin = System.Drawing.Color.FromArgb(b[0], b[1], 1);
          }
#endif
        }
      } catch {
      }
    }

    public class TriggerService
    {
      // Use Func<Task> to allow for async methods
      public Func<Task>? OnTriggerAsync { get; set; }

      // Trigger the async method
      public async Task TriggerMethodAsync()
      {
        if (OnTriggerAsync != null) {
          await OnTriggerAsync.Invoke();
        }
      }
    }

#if !_WebApp
    public static string getMediaDir(string sDbFilename)
    {
      if (string.IsNullOrEmpty(sDbFilename)) return "";

      for (byte i = 0; i < 2; i++) {
        foreach (string dirMedia in Directory.GetDirectories(Path.Combine(CkAppShared.sAppDataDir, "database"), "media_*", SearchOption.TopDirectoryOnly)) {
          string? lastFolderName = Path.GetFileName(dirMedia);
          if (!string.IsNullOrEmpty(lastFolderName)) {
            if (i == 0 && sDbFilename.Equals  (lastFolderName.Replace("media_", "")) ||
                i == 1 && sDbFilename.Contains(lastFolderName.Replace("media_", ""))) return dirMedia;
          }
        }
      }

      return "";
    }

    public static bool checkMediaExist(string sDbFilename)
    {
      string sMediaDir = getMediaDir(sDbFilename);

      if (string.IsNullOrEmpty(sMediaDir)) return false;
      if (!Directory.Exists(sMediaDir)) return false;

      DirectoryInfo d_media = new DirectoryInfo(sMediaDir);
      return d_media.Exists;
    }
#endif

    public static void writeLog(string sLogFile, string sLogText)
    {
      try {
        StreamWriter swLog = new StreamWriter(sLogFile, true);
        swLog.WriteLine(sLogText);
        swLog.Close();
      } catch {
      }
    }

    public static bool uploadFile(Stream fileStream, string sFolder, int iFileId)
    {
#if DEBUG
      //return false;
#endif

      try {
        if (fileStream != null && fileStream.Length > 0) {
          // Create directory if not existing
          if (!Directory.Exists(sFolder)) Directory.CreateDirectory(sFolder);

          // Compose temporary filename
          string sFilePng = Path.Combine(sFolder, iFileId.ToString() + ".png");
          CkAppShared.ckMng.tl.writeLog("Save file to '" + sFilePng + "'");

          // Save to disk
          using (Image imgEmblem = Image.Load(fileStream)) {
            imgEmblem.SaveAsPng(sFilePng);
          }

#if _WebApp
          // Upload to as3
          Task.Run(() => as3.uploadFileAsync(sFilePng, as3.sCkInstanceName + sFolder + "/" + iFileId.ToString() + ".png", "image/custom"));
#endif
        }

        return true;
      } catch (Exception ex) {
        CkAppShared.ckMng.tl.writeLog("Error writing/uploading image." + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, bError: true);
        return false;
      }
    }

    public static bool uploadFile(byte[] b, string sFolder, int iId, int? iId2 = null, int? iId3 = null)
    {
#if DEBUG
      //return false;
#endif

      try {
        if (b != null && b.Length > 0) {
          // Create directory if not existing
          if (!Directory.Exists(sFolder)) Directory.CreateDirectory(sFolder);

          // Compose filename
          string sFile = iId.ToString();
          if (iId2 != null) sFile += "_" + iId2.ToString();
          if (iId3 != null) sFile += "_" + iId3.ToString();
          string sFilePng = Path.Combine(sFolder, sFile + ".png");
          CkAppShared.ckMng.tl.writeLog("Save file to '" + sFilePng + "'");

          // Save to disk
          using (Image imgEmblem = Image.Load(b)) {
            imgEmblem.SaveAsPng(sFilePng);
          }

#if _WebApp
          // Upload to as3
          Task.Run(() => as3.uploadFileAsync(sFilePng, as3.sCkInstanceName + sFolder + "/" + sFile + ".png", "image/custom"));
#endif
        }

        return true;
      } catch (Exception ex) {
        CkAppShared.ckMng.tl.writeLog("Error writing/uploading image." + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, bError: true);
        return false;
      }
    }

    private static int iProgressLandCreated { get; set; } = 0;
    private static string sLandCreated { get; set; } = "";
    public static void ReportProgress(int[] value)
    {
      if (CornerkickManager.Main.sLand != null && value[0] < CornerkickManager.Main.sLand.Length) {
        sLandCreated = "Erstelle Liga: " + CornerkickManager.Main.sLand[value[0]] + ", " + (value[1] + 1).ToString() + " Liga ...";
        iProgressLandCreated = ((100 * (value[2] + 1)) / (2 * CkAppShared.iNations.Length));
      }
    }

  }
}
