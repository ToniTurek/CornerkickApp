using System.Collections.Generic;

namespace CornerkickApp.Shared.Models
{
  public class StaffModel : LayoutModel
  {
    public bool bScouting { get; set; }

    public CornerkickManager.Main.Staff staff { get; set; } = new CornerkickManager.Main.Staff();

    public List<SelectListItem> ltDdlPersonalCoachCo { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ltDdlPersonalCoachCondi { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ltDdlPersonalMasseur { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ltDdlPersonalMental { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ltDdlPersonalMed { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ltDdlPersonalJouthCoach { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ltDdlPersonalJouthScouting { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ltDdlPersonalKibitzer { get; set; } = new List<SelectListItem>();

    public CkAppShared.Tutorial tutorial { get; set; }

    public class DatatableStaffMed
    {
      public int iIx { get; set; }
      public CornerkickManager.Main.Staff.Doctor[] dr { get; set; }
      public string sName { get; set; } = "";
      public int iLevel { get; set; }
      public int[] iCost { get; set; }
      public int[] iPayOff { get; set; }
      public string sPatientName { get; set; } = "";
    }

    public class TableItemInjuredPlayer
    {
      public int iIx { get; set; }
      public string sPlName { get; set; } = "";
      public int iPlId { get; set; }
      public string sInjuryName { get; set; } = "";
      public float fInjuryRest { get; set; }
      public float fInjuryProgress { get; set; }
      public int iDrId { get; set; }
      public float fDrInjRedFac { get; set; }

      public class DrFree
      {
        public string sName { get; set; } = "";
        public int iId { get; set; }
      }
      public DrFree[] drFree { get; set; }
    }

  }
}
