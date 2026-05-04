using System.Collections.Generic;

namespace CornerkickApp.Shared.Models
{
  public class StadiumModel : LayoutModel
  {
    public string sName { get; set; } = "";

    public int[] iSeats { get; set; }
    public int[] iSeatsConstr { get; set; }

    /*
    public string[] sBlocksConstrName { get; set; }
    public int[] iBlocksConstrSeats { get; set; }
    public string[] sBlocksConstrType { get; set; }
    public bool[] sBlocksConstrRoof { get; set; }
    public int[] iBlocksConstrDays { get; set; }
    */

    public bool bTopring { get; set; }

    public bool bEditable { get; set; } // Stadium name editable

    //public CornerkickGame.Stadium stadium { get; set; }
    //public string sBlocksJSON { get; set; }
    public class Block
    {
      public string sName { get; set; } = "";
      public int iSeats { get; set; }
      public int iSeatsDaysConstruct { get; set; }
      public int iSeatsDaysConstructIni { get; set; }
      public byte iType { get; set; }
      public bool bRoof { get; set; }
      public float X { get; set; }
      public float Y { get; set; }
    }
    public Block[] blocks = new Block[24];

    public float fPitchQuality { get; set; }

    public CornerkickGame.Stadium.Facility facility { get; set; }

    public class Extra
    {
      public int iLevel { get; set; }
      public int iLevelNew { get; set; }
    }
    public Extra[] extras = new Extra[4];

    public List<SelectListItem> ddlVideo { get; set; }
    public byte iVideo { get; set; }
    //public List<SelectListItem> ddlSecurity { get; set; }
    public byte iSnackbar { get; set; }
    public byte iSnackbarNew { get; set; }
    public byte iToilets { get; set; }
    public byte iToiletsNew { get; set; }
    public byte iSecurity { get; set; }
    public byte iSecurityNew { get; set; }
    public byte iSnackbarReq { get; set; }
    public byte iToiletsReq { get; set; }
    public int iSecurityStaff { get; set; }

    public bool bSound { get; set; }

    public StadiumModel()
    {
      iSeats       = new int[24];
      iSeatsConstr = new int[24];

      facility = new CornerkickGame.Stadium.Facility();

      ddlVideo = new List<SelectListItem>();
      for (byte iV = 0; iV < CornerkickManager.Stadium.facVideo.sLevelNames.Length; iV++) ddlVideo.Add(new SelectListItem { Text = CornerkickManager.Stadium.facVideo.sLevelNames[iV], Value = iV.ToString() });

      /*
      ddlSecurity = new List<SelectListItem>();
      for (byte iV = 0; iV < CornerkickManager.Stadium.facSecurity.sLevelNames.Length; iV++) ddlSecurity.Add(new SelectListItem { Text = CornerkickManager.Stadium.facSecurity.sLevelNames[iV], Value = iV.ToString() });
      */
    }
  }
}
