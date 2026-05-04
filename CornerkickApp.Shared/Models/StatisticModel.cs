using System.Collections.Generic;

namespace CornerkickApp.Shared.Models
{
  public class StatisticModel : LayoutModel
  {
    public int iLand { get; set; }
    public int iDivision { get; set; }

    public class TableEntryTeams
    {
      public int iIx { get; set; }
      public int iTeamId { get; set; }
      public string sTeamName { get; set; } = "";
      public string sEmblem { get; set; } = "";
      public float fTeamAveSkill { get; set; }
      public float fTeamAveAge { get; set; }
      public int iTeamValueTotal { get; set; }
      public int nPlayer { get; set; }
      public float fTeamAveSkill11 { get; set; }
      public float fTeamAveAge11 { get; set; }
      public int iTeamValueTotal11 { get; set; }
      public float fAttrFactor { get; set; }
      public string sLeague { get; set; } = "";
    }

    public class TableEntryStadiums
    {
      public int iIx { get; set; }
      public string sName { get; set; } = "";
      public string sClubName { get; set; } = "";
      public int iTotal { get; set; }
      public int iTotalCtn { get; set; }
      public int iType0 { get; set; }
      public int iType1 { get; set; }
      public int iType2 { get; set; }
      public int iType0Ctn { get; set; }
      public int iType1Ctn { get; set; }
      public int iType2Ctn { get; set; }
      public bool bTopring { get; set; }
    }
  }
}