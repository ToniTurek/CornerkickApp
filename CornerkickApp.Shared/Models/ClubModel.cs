using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CornerkickApp.Shared.Models
{
  public class ClubModel : LayoutModel
  {
    public int iClubId { get; set; } = -1;

    //public CornerkickManager.Club club { get; set; }
    public string sClubName { get; set; } = "";
    public bool bNation { get; set; }

    public string sUserName { get; set; } = "";

    // Emblem
    public string sEmblem { get; set; } = "";
    public byte[] byteClubDetailsEmblem { get; set; }
    public bool bEmblemEditable { get; set; }

    public string sLand { get; set; } = "";
    public string sDivision { get; set; } = "-";
    public string sPlace { get; set; } = "-";

    public List<Player> ltPlayer { get; set; } = new List<Player>();
    public int iPlayerJouth { get; set; }
    public float fAveStrength { get; set; }
    public float fAveAge { get; set; }

    public bool bScouting { get; set; }

    public float fAttrFc { get; set; }
    public string sStadium { get; set; } = "-";
    public string sStadiumSeats { get; set; } = "";

    public List<Success> ltSuccess { get; set; } = new List<Success>();
    public string[] sRecordGames { get; set; } = new string[4];

    public class Player
    {
      public int iId { get; set; }
      public string sName { get; set; } = "";
      public int iNo { get; set; }
      public int iPos { get; set; }
      public string sPos { get; set; } = "";
      public string sClub { get; set; } = "";
    }
    public class Success
    {
      public string sCupName { get; set; } = "";
      // Counter
      public int iWin { get; set; }
      public int iDraw { get; set; }
      public int iDefeat { get; set; }

      public int iCupWin { get; set; }

      // Cup place history
      public List<int[]> ltCupPlace { get; set; } = new List<int[]>(); // Final cup places (List of int[Place, Season])

      public string[] sRecordGames { get; set; } = new string[4];
    }
  }
}
