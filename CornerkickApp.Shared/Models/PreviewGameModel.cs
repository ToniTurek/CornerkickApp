using System.Collections.Generic;

namespace CornerkickApp.Shared.Models
{
  public class PreviewGameModel : LayoutModel
  {
    //public List<SelectListItem> ddlGames { get; set; }
    public string[] sGames { get; set; }

    public string sTeamH { get; set; }
    public string sTeamA { get; set; }
    public string sCupName { get; set; }
    public string sMd { get; set; }
    public string sStadium { get; set; }
    public string sReferee { get; set; }

    public PreviewGameModel()
    {
      //ddlGames = new List<SelectListItem>();
    }

    public class GameInfo
    {
      public string sGameDate { get; set; } = "";
      public string sCupName { get; set; } = "";
      public int iMatchday { get; set; }
      public string sStadium { get; set; } = "";
      public string sCupEmblem { get; set; } = "";
      public string sCupAnthem { get; set; } = "";
      public string sClubNameH { get; set; } = "";
      public string sClubNameA { get; set; } = "";
      public string sClubEmblemH { get; set; } = "";
      public string sClubEmblemA { get; set; } = "";
      public string sClubPlaceH { get; set; } = "";
      public string sClubPlaceA { get; set; } = "";
      public double fHoursUntilGame { get; set; }
      public CornerkickGame.Game.Referee? referee { get; set; }
      public float fRefereeCorrupt { get; set; }
    }

  }
}