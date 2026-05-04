using System;
using System.Collections.Generic;

namespace CornerkickApp.Shared.Models
{
  public class CalendarModel : LayoutModel
  {
    public int iClubId { get; set; }

    //public IList<string> sCal { get; set; }
    public DateTime dtToday { get; set; }

    public string id { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string date { get; set; } = string.Empty;
    public string start { get; set; } = string.Empty;
    public string end { get; set; } = string.Empty;
    public string url { get; set; } = string.Empty;

    public bool allday { get; set; }

    public List<SelectListItem> sliTestgameClubs { get; set; }
    public string[] sTestgameClub { get; set; }

    public CornerkickManager.TrainingCamp.Camp camp { get; set; }

    public CkAppShared.Tutorial tutorial { get; set; }
    public string[][] ltPlayer { get; set; }

    public class Appointment
    {
      public int iID;

      public int iType { get; set; } // 0 - Testgame, 1 - Trainings camp, 2 - Event, 3 - Player meeting
      public DateTime dtStart { get; set; }
      public DateTime dtEnd { get; set; }

      public string sTitle { get; set; } = string.Empty;
      public string sDescription { get; set; } = string.Empty;
      public string sColor { get; set; } = string.Empty;
      public string sColor2 { get; set; } = string.Empty;
      public bool bEditable { get; set; }
      public bool bAllDay { get; set; }
      public string sClassName { get; set; } = string.Empty;

      public int iIdReturn;
      public int iIdReturn2;
    }

    public class TableTestGames
    {
      //public string sDateIso { get; set; }
      public DateTime dt { get; set; }
      public int iIdTeamOpp { get; set; }
      public string sTeamH { get; set; } = string.Empty;
      public string sTeamA { get; set; } = string.Empty;
    }
  }

  public class Testgame
  {
    public DateTime dt { get; set; }
    public int iTeamHome { get; set; }
    public int iTeamAway { get; set; }
  }

  public class DiaryEvent
  {
    public int iID { get; set; }
    public string sTitle { get; set; } = string.Empty;
    public string sDescription { get; set; } = string.Empty;
    public int SomeImportantKeyID { get; set; }
    public string sStartDate { get; set; } = string.Empty;
    public string sEndDate { get; set; } = string.Empty;
    public string StatusString { get; set; } = string.Empty;
    public string sColor { get; set; } = string.Empty;
    public string sColor2 { get; set; } = string.Empty;
    public bool bEditable { get; set; }
    public bool bAllDay { get; set; }
    public string sClassName { get; set; } = string.Empty;
  }

}
