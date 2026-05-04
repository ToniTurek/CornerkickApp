using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace CornerkickApp.Shared.Models
{
  public class TeamModel : LayoutModel
  {
    public TeamModel()
    {
      /*
      string sAutoFormType = "0";
      if (!string.IsNullOrEmpty(sAutoFormationType)) sAutoFormType = sAutoFormationType;
      */
      ltDdlAutoFormationType = new List<SelectListItem> {
        new SelectListItem { Text = "max. Stärke",    Value =  "0" },
        new SelectListItem { Text = "max. Kondition", Value = "+1" },
        new SelectListItem { Text = "min. Kondition", Value = "-1" },
        new SelectListItem { Text = "max. Frische",   Value = "+2" },
        new SelectListItem { Text = "min. Frische",   Value = "-2" },
        new SelectListItem { Text = "max. Erfahrung", Value = "+3" },
        new SelectListItem { Text = "min. Erfahrung", Value = "-3" },
        new SelectListItem { Text = "max. Alter",     Value = "+4" },
        new SelectListItem { Text = "min. Alter",     Value = "-4" }
      };
    }

    public class Point
    {
      public Point(System.Drawing.Point? pt = null, double Z = 0.0)
      {
        if (pt.HasValue) {
          this.x = pt.Value.X;
          this.y = pt.Value.Y;
        }

        this.z = Z;
      }

      public int x { get; set; }
      public int y { get; set; }
      public double z { get; set; }

      public System.Drawing.Point toPoint()
      {
        return new System.Drawing.Point(x, y);
      }
    }

    //public CornerkickManager.Club club { get; set; }

    public byte nPlStart { get; set; }
    public byte nPlRes { get; set; }

    public bool bAdmin { get; set; }
    public CkAppShared.Tutorial tutorial { get; set; }

    public List<CornerkickGame.Player> ltPlayer { get; set; } = new List<CornerkickGame.Player>();
    public List<string[]>? ltsSubstitution { get; set; }
    public List<int   []>? ltiSubstitution { get; set; }
    public byte iSubRest { get; set; }
    public bool bGame { get; set; }

    public List<SelectListItem>? sliSystem { get; set; }
    public int iSystem { get; set; }
    public int iTcOrient { get; set; }

    public string[] sCl { get; set; }

    [DataType(DataType.Text)]
    [Display(Name = "Formation")]
    public int iFormation { get; set; }
    public List<SelectListItem>? ltsFormations { get; set; }
    public List<SelectListItem>? ltsFormationsOwn { get; set; }

    [DataType(DataType.Text)]
    [Display(Name = "Typ")]
    public static List<SelectListItem>? ltDdlAutoFormationType { get; set; }

    //public TeamData tD;

    public class Player
    {
      public int iId { get; set; }
      public byte iPos { get; set; }
      public Point ptPos { get; set; }
      public string sName { get; set; } = "";
      public string sSkillAve { get; set; } = "";
      public string sTeamname { get; set; } = "";
      public string sAge { get; set; } = "";
      public string sNat { get; set; } = "";
      public string sPortrait { get; set; } = "";
      public int iCard { get; set; }
      public sbyte iIxManMarking { get; set; }
      public bool bOffStandards { get; set; }
      public int iTcIndOrient { get; set; }
      //public float[] fTcIndOrientMinMax { get; set; }
      public byte iNb { get; set; }
    }

    public class TeamData
    {
      // Player details
      public List<Player> ltPlayer2 { get; set; } = new List<Player>();
      //public CornerkickGame.Tactic.Formation formation { get; set; }
      //public Point[]? ptPos { get; set; }
      public string sTeamAveSkill { get; set; } = "";
      public string sTeamAveAge { get; set; } = "";
      public float[] fTeamStrengthPos { get; set; }
      public string sEmblem { get; set; } = "";

      public System.Drawing.Point ptPitch { get; set; }

      // Opponent player details
      public byte iKibitzer { get; set; }
      public bool bOppTeam { get; set; } // Opponent team exist
      public List<Player> ltPlayerOpp2 { get; set; } = new List<Player>();
      //public CornerkickGame.Tactic.Formation formationOpp { get; set; }
      //public Point[]? ptPosOpp { get; set; }
      public string sTeamOppAveSkill { get; set; } = "";
      public string sTeamOppAveAge { get; set; } = "";
      public string sEmblemOpp { get; set; } = "";

      public CornerkickGame.Player? plSelected { get; set; }
      public byte iCaptainIx { get; set; }
      public string sDivRoa { get; set; } = "";

      public float fTeamAveStrength { get; set; }
      public float fTeamAveAge { get; set; }

      public bool bNation { get; set; }
    }

    public class TableTeam
    {
      public int iIndex { get; set; }
      public int iId { get; set; }
      public int iNr { get; set; }
      public string sNull { get; set; } = "";
      public string sName { get; set; } = "";
      public string sPosition { get; set; } = "";
      public string sHp { get; set; }
      public float fSkill { get; set; }
      public float fCondi { get; set; }
      public float fFresh { get; set; }
      public float fMoral { get; set; }
      public float fSkillIdeal { get; set; }
      public float fExperience { get; set; }
      public string sForm { get; set; } = "";
      public float fAge { get; set; }
      public float fTalent { get; set; }
      public bool bSubstituted { get; set; }
      public float fLeader { get; set; }
      public int iMarktwert { get; set; }
      public int iGehalt { get; set; }
      public int iLz { get; set; }
      public string sNat { get; set; } = "";
      public int iSuspended { get; set; }
      public string sCaptain { get; set; } = "";
      public float fGrade { get; set; }
      public bool bAtNationalTeam { get; set; }
      public float[] fRadOfAction { get; set; } = new float[2];
    }
  }
}
