using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CornerkickApp.Shared.Models
{
  public class LeagueModel : LayoutModel
  {
    public int iClubId { get; set; }

    public string sCupName { get; set; } = "Liga";
    public string sCupEmblem { get; set; } = "";

    [Display(Name = "Saison: ")]
    public int iSeason { get; set; }

    [Display(Name = "Land: ")]
    public int iLand { get; set; }

    [Display(Name = "Spielklasse: ")]
    public int iDivision { get; set; }

    [Display(Name = "Spieltag: ")]
    public int iMatchday { get; set; }

    public int iMatchdayCurrent { get; set; }

    // List<CornerkickManager.Core.Tabellenplatz> ltTbpl = cr.getTabelleLiga(iSaison, iLand, iSpielklasse, iSpieltag, 0);
    //public List<CornerkickManager.Cup.TableItem> ltTblLast { get; set; } // Table last matchday
    public int iLeagueSize { get; set; }

    public List<SelectListItem> ddlLand { get; set; }
    public List<SelectListItem> ddlSeason { get; set; }
    public List<SelectListItem> ddlDivision { get; set; }
    public List<SelectListItem> ddlMatchdays { get; set; }

    public class GameInfo
    {
      public int iIx { get; set; }
      public string sDt { get; set; } = "";
      public int iIdH { get; set; }
      public int iIdA { get; set; }
      public string sNameH { get; set; } = "";
      public string sNameA { get; set; } = "";
      public string sResult { get; set; } = "";
      public bool bBold { get; set; }
    }

    public class TableItem
    {
      public int iIx { get; set; }
      public int iIxLast { get; set; }
      public int iId { get; set; }
      public string sName { get; set; } = "";
      public string sEmblem { get; set; } = "";
      public int iW { get; set; }
      public int iD { get; set; }
      public int iL { get; set; }
      public int iPoints { get; set; }
      public string sGoals { get; set; } = "";
      public int iGoalsDiff { get; set; }

      public string sBgColor { get; set; } = "white";
    }

    public class ScorerItem
    {
      public int iIx { get; set; }
      public int iId { get; set; }
      public string sPlName { get; set; } = "";
      public string sClubName { get; set; } = "";
      public int iClubId { get; set; }
      public string sClubEmblem { get; set; } = "";
      public byte[]? bClubEmblem { get; set; }
      public int iGoals { get; set; }
      public int iAssists { get; set; }
      public int iScorer { get; set; }
      public bool bBold { get; set; }
    }

    public class KeeperItem
    {
      public int iIx { get; set; }
      public int iId { get; set; }
      public string sPlName { get; set; } = "";
      public string sClubName { get; set; } = "";
      public string sClubEmblem { get; set; } = "";
      public float fSaves { get; set; }
      public int iGamesNoGoal { get; set; }
      public int iMinNoGoal { get; set; }
      public bool bBold { get; set; }
    }

    public LeagueModel()
    {
      //ddlLand = new List<SelectListItem>();
      //ddlDivision = new List<SelectListItem>();
      if (CkAppShared.iNations.Length > 0) iLand = CkAppShared.iNations[0];
    }

  }
}