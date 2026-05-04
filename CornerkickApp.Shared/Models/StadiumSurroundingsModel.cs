using System.Collections.Generic;

namespace CornerkickApp.Shared.Models
{
  public class StadiumSurroundingsModel : LayoutModel
  {
    public bool bSound { get; set; }
    public string sColor1 { get; set; } = "";
    public string sMainanenceCostTotal { get; set; } = "";

    public List<SelectListItem> ddlTrainingsgel { get; set; } = new List<SelectListItem>();
    public int iTrainingsgel { get; set; }
    public int iTrainingNew  { get; set; }

    public List<SelectListItem> ddlGym { get; set; } = new List<SelectListItem>();
    public int iGym { get; set; }
    public int iGymNew { get; set; }

    public List<SelectListItem> ddlSpa { get; set; } = new List<SelectListItem>();
    public int iSpa { get; set; }
    public int iSpaNew { get; set; }

    public List<SelectListItem> ddlJouthInternat { get; set; } = new List<SelectListItem>();
    public int iJouthInternat    { get; set; }
    public int iJouthInternatNew { get; set; }

    public List<SelectListItem> ddlClubHouse { get; set; } = new List<SelectListItem>();
    public int iClubHouse    { get; set; }
    public int iClubHouseNew { get; set; }

    public List<SelectListItem> ddlClubMuseum { get; set; } = new List<SelectListItem>();
    public int iClubMuseum    { get; set; }
    public int iClubMuseumNew { get; set; }

    public int iCarpark { get; set; }
    public int iCarparkNew { get; set; }
    public int iCarparkReq { get; set; } // Required level

    public int iCounter { get; set; }
    public int iCounterNew { get; set; }
    public int iCounterReq { get; set; } // Required level

    public int iFanshop { get; set; }
    public int iFanshopNew { get; set; }
    public int iFanshopReq { get; set; } // Required level

    public CkAppShared.Tutorial tutorial { get; set; }

    public class Buildings
    {
      public byte iGround { get; set; }
      public string sCostBuyGround { get; set; } = "";
      public List<Building> ltBuildings { get; set; } = new List<Building>();
      public List<Building> ltBuildingsFree { get; set; } = new List<Building>();
    }

    public class Building
    {
      public string sCategory { get; set; } = "";
      public int iLevel { get; set; }
      public int iLevelMax { get; set; } // Maximum level
      public int iLevelReq { get; set; } // Required level
      public byte iType { get; set; }
      public bool bTypeInt { get; set; } // true: Integer type, false: level type
      public string sName { get; set; } = "";
      public string sNameNext { get; set; } = "";
      public float fDaysConstruct { get; set; }
      public int nDaysConstructTotal { get; set; }
      public int iCostConstructNext { get; set; }
      public int iCostMaintenance { get; set; }
      public int iCostMaintenanceNext { get; set; }
      public bool bDispoOk { get; set; }
      public int nRepeat { get; set; } // Number of repeated buildings
    }
  }
}
