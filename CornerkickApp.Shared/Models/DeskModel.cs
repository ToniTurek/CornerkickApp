using System.Runtime.Serialization;

namespace CornerkickApp.Shared.Models
{
  public class DeskModel : LayoutModel
  {
    //public CornerkickManager.User user { get; set; } = new CornerkickManager.User();
    //public CornerkickManager.Club club { get; set; } = new CornerkickManager.Club();
    public int iClubId { get; set; }
    public bool bNation { get; set; }

    public int[] iPlaceLeague { get; set; } = new int[3];
    public int    iLand { get; set; } = -1;
    public int    iDiv { get; set; } = -1;
    public string sNatCupRound { get; set; } = "";
    public string sNatCupEliminated { get; set; } = "";
    public int    iIntCupId2 { get; set; } = -1;
    public string sIntCupName { get; set; } = "";
    public string sIntCupRound { get; set; } = "";
    public string sSeries { get; set; } = "";

    //public CornerkickManager.Main.Staff staff { get; set; } = new CornerkickManager.Main.Staff();

    public bool bUserExist { get; set; }
    public byte iUserRespLv { get; set; }
    public bool bShowPreviewGame { get; set; }

    public static SelectListItem[] ddlDeleteLog { get; set; } = new SelectListItem[4];
    public int iDeleteLog { get; set; }
    public bool bShowBalanceToday { get; set; }

    public class DatatableNews
    {
      public int iId { get; set; }
      public int iType { get; set; }
      public string sDate { get; set; } = "";
      public string sText { get; set; } = "";
      public bool bOld { get; set; }
      public string sHeader { get; set; } = "";
      public string sImg { get; set; } = "";
    }
    public List<DatatableNews> ltNews = new List<DatatableNews>();

    public CkAppShared.Tutorial? tutorial { get; set; }

    public DeskModel()
    {
      ddlDeleteLog = new SelectListItem[] {
        new SelectListItem() { Text =  "7 Tagen",  Value = "1" },
        new SelectListItem() { Text = "14 Tagen",  Value = "2" },
        new SelectListItem() { Text =  "1 Monat",  Value = "3" },
        new SelectListItem() { Text =      "Nie",  Value = "0" }
      };
    }
  }

  public class DeskWarningsModel
  {
    public bool bAny { get; set; }
    public int iCaptainId { get; set; } = -1;
    public float fSkillPointsFree { get; set; }
    public bool bEmblemExist { get; set; } = true;
    public bool bNoTrainingWarning { get; set; }
    public bool bNoStaffWarning { get; set; }
    public bool bNoMerchandisingWarning { get; set; }
    public bool bInjuredPlayerNoDoctorWarning { get; set; }
    public int iSponsorId { get; set; }
    public bool bNation { get; set; }
  }

  public class DeskStatusModel
  {
    public string sNextGameInfo { get; set; } = "";
    public float[]? fCFM { get; set; }
    public float[]? fCFM_LastWeek { get; set; }
    public float[]? fStrength { get; set; }
    public byte iWeather { get; set; }
  }

  //DataContract for Serializing Data - required to serve in JSON format
  [DataContract]
  public class DataPointLastGames2
  {
    public DataPointLastGames2(int x, double y)
    {
      this.X = x;
      this.Y = y;
    }

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "x")]
    public Nullable<int> X = null;

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "y")]
    public Nullable<double> Y = null;
  }

  //DataContract for Serializing Data - required to serve in JSON format
  [DataContract]
  public class DataPointLastGames
  {
    public DataPointLastGames(string sX, int iY)
    {
      this.X = sX;
      this.Y = iY;
    }

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "Game")]
    public string X = "";

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "Result")]
    public Nullable<int> Y = null;
  }
}
