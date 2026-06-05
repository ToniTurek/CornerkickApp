using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Components;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;

namespace CornerkickApp.Shared.Models
{
  public class ApiService
  {
    public ApiService(HttpClient httpClient, NavigationManager navigationManager)
    {
      HttpClient = httpClient;
      NavigationManager = navigationManager;
      HttpClient.BaseAddress = new Uri(NavigationManager.BaseUri);
    }

    public HttpClient HttpClient { get; }
    public NavigationManager NavigationManager { get; }
  }

  /*
  public interface ICkAppShared
  {
    public static CornerkickManager.Main ckMng { get; set; }
  }
  */

  public class CkAppShared
  {
    public static string sWwwRootDir = "";
    public const string sContentDir = "./_content/CornerkickApp.Components/Content";
    public static string sContentDirAbs;

    public static string? sFileCk = null;

    // Ck Colors
    public static readonly System.Drawing.Color clCk  = System.Drawing.Color.FromArgb( 31, 158,  69); // First color: green
    public static readonly System.Drawing.Color clCk2 = System.Drawing.Color.FromArgb(255,   0,   0); // Second color: red
    public static readonly System.Drawing.Color clCk3 = System.Drawing.Color.FromArgb(255, 242,   0); // Third color: yellow
    public static readonly System.Drawing.Color clCk4 = System.Drawing.Color.FromArgb(255, 116,  63); // Forth color: orange

    public static readonly string sClCkRgb  = clCk .R.ToString() + "," + clCk .G.ToString() + "," + clCk .B.ToString();
    public static readonly string sClCkRgb2 = clCk2.R.ToString() + "," + clCk2.G.ToString() + "," + clCk2.B.ToString();
    public static readonly string sClCkRgb3 = clCk3.R.ToString() + "," + clCk3.G.ToString() + "," + clCk3.B.ToString();
    public static readonly string sClCkRgb4 = clCk4.R.ToString() + "," + clCk4.G.ToString() + "," + clCk4.B.ToString();

    public static CornerkickManager.Main ckMng { get; set; } = default!;
    //[Inject] public static CornerkickManager.Main ckMng2 { get; set; } = default!;

    public static System.Timers.Timer timerCkCalender = null;
    public static System.Timers.Timer timerSave = null;
    public static List<string> ltLog = new List<string>();
    public static Random random = new Random();
    public static Settings settings = new Settings();
    public static string sVersion = "8.1.0 beta";
#if _WebApp
    public static int iLoadState = 1; // 1: Initial value, 2: starting calendar steps, 0: ready for login, 3: error
#else
    public static int iLoadState = 0; // 1: Initial value, 2: starting calendar steps, 0: ready for login, 3: error
#endif

    public static bool bUserIsAuthenticated { get; set; } = false;
#if _WebApp
    //private const double fStartDelay = 500.0; // [ms]
    public const double fLoadDelay = 1000.0; // [ms]

    public static bool bWebApp = true;
    public static bool bStandaloneLoggedIn { get; set; } = false;
#else
    public static bool bWebApp = false;
    public static bool bStandaloneLoggedIn { get; set; } = !bWebApp && ckMng?.ltUser?.Count > 0;
#endif

#if DEBUG
    public static bool debug = true;
#else
    public static bool debug = false;
#endif

    public static string sCkInstanceName = "";
    public static int iUserActive; // Current active user index

    public const byte iContractLengthMax = 5;

    public static int iFontSize = -1;
    public static string sCssStyleClubColors { get; set; } = "";

    public static CornerkickGame.Stadium stadiumDefaultTrainingCourt { get; set; } = new CornerkickGame.Stadium();
    public static CornerkickGame.Stadium stadiumDefaultSmall         { get; set; } = new CornerkickGame.Stadium();
    public static CornerkickGame.Stadium stadiumDefaultBig           { get; set; } = new CornerkickGame.Stadium();
    public const int iCostRentStadiumDefaultSmall =  25000; // € per home game
    public const int iCostRentStadiumDefaultBig   = 250000; // € per home game

    public const int iCupIdLeague = 1;   // National leagues
    public const int iCupIdNatCup = 2;   // National cups
    public const int iCupIdInt = 3;      // International cups
    public const int iCupIdWc = 6;       // World cup
    public const int iCupIdTestgame = 7; // Test games
    public readonly static string[] sCupColors = ["", "green", "blue", "rgb(255, 200, 14)", "rgb(192, 192, 192)", "", "", ""];

    public const int nPlayerNatMax = 32;

    // User Responibility
    public const byte iUserRespPlayerIndTraining = 1;
    public const byte iUserRespPlayerContractsFast = 1;
    public const byte iUserRespJouth = 1;
    public const byte iUserRespPlayerStatistic = 1;
    public const byte iUserRespPlayerContractsExpert = 2;
    public const byte iUserRespStaff = 2;
    public const byte iUserRespFinance = 2;
    public const byte iUserRespStatistic = 2;
    public const byte iUserRespStadiumShow = 2;
    public const byte iUserRespTrainingCamps = 2;
    public const byte iUserRespSponsors = 3;
    public const byte iUserRespStaffScouts = 3;
    public const byte iUserRespTransfers = 3;
    public const byte iUserRespFinanceAccountDetails = 3;
    public const byte iUserRespSpecPrice = 3;
    public const byte iUserRespBuildingsShow = 3;
    public const byte iUserRespStadium = 4;
    public const byte iUserRespBuildings = 4;
    public const byte iUserRespMerchandising = 4;
    public const byte iUserRespSponsorsSpecial = 4;
    public const byte iUserRespSecretBalance = 4;

    // Options
    public const int iOptionGameSpeedSlow     = 300;
    public const int iOptionGameSpeedNormal   = 200;
    public const int iOptionGameSpeedFast     = 100;
    public const int iOptionGameSpeedVeryFast =  50;

    public class Settings
    {
      public int iStartHour;
      public bool bEmailCertification;
      public bool bRegisterDuringGame;
      public bool bLoginPossible;
      public bool bMaintenance;
      public string sInfo = "";
      public DateTime dtCounterStart;

      public Settings()
      {
        iStartHour = -1;
        bLoginPossible = true;
        bEmailCertification = true;
        bRegisterDuringGame = true;
#if DEBUG
        bEmailCertification = false;
#endif
      }
    }

#if _WebApp
    //const string sSaveZip = "ckSave.zip";
    public const string sFilenameSave = ".autosave.ckx";
    public const string sFilenameSettings = "laststate.txt";
#endif

    public static byte[] iNations = [
#if _WebApp
      36, // GER
      29, // ENG
      30, // ESP
      45, // ITA
      33, // FRA
      54, // NED
      13, // BRA
       3  // ARG
#endif
    ]; // [CK Nat.] = sLand Nat.

    public static CornerkickManager.Club? clubAdmin;

    public class Tutorial
    {
      public bool bShow;
      public int iLevel;
    }
    public static Tutorial[] ttUser; // User tutorial

    public class Mail
    {
      public string sIdFrom { get; set; } = "";
      public string sIdTo { get; set; } = "";
      public bool bNew { get; set; }
      public DateTime dt { get; set; }
      public string sText { get; set; } = "";
    }
    public static List<Mail> ltMail;

    public static CornerkickGame.Stadium[] stadiums = new CornerkickGame.Stadium[] {
      new CornerkickGame.Stadium(),
      new CornerkickGame.Stadium()
    };

    //DataContract for Serializing Data - required to serve in JSON format
    [DataContract]
    public class DataPointLD
    {
      public DataPointLD(long x, double y, string z = "")
      {
        this.x = x;
        this.y = y;
        this.z = z;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public long? x { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public double? y { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "z")]
      public string z { get; set; }
    }

    [DataContract]
    public class DataPointII
    {
      public DataPointII(int x, int y, string z = "")
      {
        this.X = x;
        this.Y = y;
        this.Z = z;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public int? X { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public int? Y { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "z")]
      public string Z { get; set; }
    }

    [DataContract]
    public class DataPointID
    {
      public DataPointID(int x, double y, string z = "", string sColor = "")
      {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.sColor = sColor;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public int? X { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public double? Y { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "z")]
      public string Z { get; set; }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "sColor")]
      public string sColor { get; set; }
    }

    [DataContract]
    public class DataPointSD
    {
      public DataPointSD(string x, double y, string z = "", string sColor = "")
      {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.sColor = sColor;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public string X { get; set; }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public double? Y { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "z")]
      public string Z { get; set; }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "sColor")]
      public string sColor { get; set; }
    }

    [DataContract]
    public class DataPointTD
    {
      public DataPointTD(DateTime? x = null, double y = 0.0, string z = "")
      {
        this.X = x;
        this.Y = y;
        this.Z = z;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public DateTime? X { get; set; }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public double? Y { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "z")]
      public string Z { get; set; }
    }

    [DataContract]
    public class DataPointTL
    {
      public DataPointTL(DateTime x, long y, string z = "")
      {
        this.X = x;
        this.Y = y;
        this.Z = z;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public DateTime? X { get; set; }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public long? Y { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "z")]
      public string Z { get; set; }
    }

    [DataContract]
    public class DataPointTsD
    {
      public DataPointTsD(TimeSpan x, double y, string z = "")
      {
        this.X = x;
        this.Y = y;
        this.Z = z;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public TimeSpan? X { get; set; }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public double? Y { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "z")]
      public string Z { get; set; }
    }

    [DataContract]
    public class DataPointDD
    {
      public DataPointDD(double x, double y, string z = "")
      {
        this.X = x;
        this.Y = y;
        this.Z = z;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public double? X { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public double? Y { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "z")]
      public string Z { get; set; }
    }

    [DataContract]
    public class DataPointIL
    {
      public DataPointIL(int x, long y, string z = "")
      {
        this.X = x;
        this.Y = y;
        this.Z = z;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public int? X { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public long? Y { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "z")]
      public string Z { get; set; }
    }

    [DataContract]
    public class DataPointLL
    {
      public DataPointLL(long x, long y, string z = "")
      {
        this.X = x;
        this.Y = y;
        this.Z = z;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public long? X { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public long? Y { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "z")]
      public string Z { get; set; }
    }

    [DataContract]
    public class DataPoint2D_dec
    {
      public DataPoint2D_dec(double x, decimal y, string desc = "")
      {
        this.x = x;
        this.y = y;
        this.desc = desc;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public double? x = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public decimal? y = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "desc")]
      public string desc;
    }

#if _WebApp
    public static DateTime dtLoadCk = new DateTime(); // The ck DateTime when game was (re-)started

    public class TimerLoad : System.Timers.Timer
    {
      public string sAppDataDir;
    }
    public static TimerLoad timerLoad;
#else

    public class CalendarReturn
    {
      public CornerkickManager.User user { get; set; }
      public List<CornerkickManager.Main.CalendarReturn> ltRet { get; set; } = new List<CornerkickManager.Main.CalendarReturn>();
      public string sDate { get; set; } = "";
      public int iBalance { get; set; }
      public int iBalanceSecret { get; set; }
      public string sCFM { get; set; } = "";
      public string sStrength { get; set; } = "";
      public PreviewGameModel.GameInfo? gameInfo { get; set; }
      public bool bWithDetails { get; set; } = false;
    }
#endif

  }
}
