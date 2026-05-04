using CornerkickApp.Shared.Models;

namespace CornerkickApp.Controllers.Member
{
  public class BuildingsController
  {
    public static StadiumSurroundingsModel Model(CornerkickManager.User? _usr)
    {
      StadiumSurroundingsModel mdStadionSurr = new StadiumSurroundingsModel();

      if (_usr == null) return mdStadionSurr;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return mdStadionSurr;

      mdStadionSurr.bSound = true;
      if (_usr.lti.Count > UserOptionsModel.iUserOptionsIxSound) mdStadionSurr.bSound = _usr.lti[UserOptionsModel.iUserOptionsIxSound] > 0;

      mdStadionSurr.sMainanenceCostTotal = clb.getBuildingsMaintenanceCost().ToString("N0", MemberController.getCi(_usr)) + " €";

      mdStadionSurr.iTrainingsgel     = clb.buildings.bgTrainingCourts.iLevel;
      mdStadionSurr.iGym              = clb.buildings.bgGym.iLevel;
      mdStadionSurr.iSpa              = clb.buildings.bgSpa.iLevel;
      mdStadionSurr.iJouthInternat    = clb.buildings.bgJouthInternat.iLevel;
      mdStadionSurr.iClubHouse        = clb.buildings.bgClubHouse.iLevel;
      mdStadionSurr.iClubMuseum       = clb.buildings.bgClubMuseum.iLevel;
      mdStadionSurr.iCarpark          = Math.Max(clb.stadium.facility.iCarpark, clb.stadium.facility.iCarparkNew);
      mdStadionSurr.iCounter          = Math.Max(clb.stadium.facility.iTicketcounter, clb.stadium.facility.iTicketcounterNew);
      mdStadionSurr.iFanshop          = clb.buildings.bgFanshop.iLevel;
      if (clb.buildings.bgFanshop.ctn != null) mdStadionSurr.iFanshop = Math.Max(mdStadionSurr.iFanshop, clb.buildings.bgFanshop.ctn.iLevelNew);

      mdStadionSurr.sColor1 = "rgb(" + clb.cl1[0].R.ToString() + "," + clb.cl1[0].G.ToString() + "," + clb.cl1[0].B.ToString() + ")";

      // Tutorial
      if (CkAppShared.ttUser != null) {
        int iUserIx = CkAppShared.ckMng.ltUser.IndexOf(_usr);
        if (iUserIx >= 0 && iUserIx < CkAppShared.ttUser.Length) mdStadionSurr.tutorial = CkAppShared.ttUser[iUserIx];
      }

      return mdStadionSurr;
    }

    public static StadiumSurroundingsModel.Buildings? GetBuildings(CornerkickManager.User? _usr)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      StadiumSurroundingsModel.Building[] bdgsAll = new StadiumSurroundingsModel.Building[10];
      for (byte iB = 0; iB < bdgsAll.Length; iB++) bdgsAll[iB] = new StadiumSurroundingsModel.Building();

      StadiumSurroundingsModel.Buildings buildings = new StadiumSurroundingsModel.Buildings();
      buildings.ltBuildings     = new List<StadiumSurroundingsModel.Building>();
      buildings.ltBuildingsFree = new List<StadiumSurroundingsModel.Building>();

      int[] iCostDays = new int[2];

      // Training courts
      byte iType = 0;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgTrainingCourts.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgTrainingCourts.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgTrainingCourts.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgTrainingCourts.sLevelNames[clb.buildings.bgTrainingCourts.iLevel];
      bdgsAll[iType].iCostMaintenance = CornerkickManager.Stadium.bdgTrainingCourts.iCostMaintenance[clb.buildings.bgTrainingCourts.iLevel];
      if (clb.buildings.bgTrainingCourts.iLevel + 1 < CornerkickManager.Stadium.bdgTrainingCourts.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgTrainingCourts.sLevelNames[clb.buildings.bgTrainingCourts.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgTrainingCourts.iGround[Math.Max(clb.buildings.bgTrainingCourts.iLevel, clb.buildings.bgTrainingCourts.ctn != null ? clb.buildings.bgTrainingCourts.ctn.iLevelNew : (byte)0)];
      if (clb.buildings.bgTrainingCourts.ctn != null && clb.buildings.bgTrainingCourts.ctn.iLevelNew > clb.buildings.bgTrainingCourts.iLevel) {
        bdgsAll[iType].fDaysConstruct = clb.buildings.bgTrainingCourts.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildTrainingCourts(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildTrainingCourts(clb.buildings.bgTrainingCourts.iLevel + 1, clb.buildings.bgTrainingCourts.iLevel);
        bdgsAll[iType].iCostConstructNext = iCostDays[0];
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        if (clb.buildings.bgTrainingCourts.iLevel + 1 < CornerkickManager.Stadium.bdgTrainingCourts.iCostMaintenance.Length) bdgsAll[iType].iCostMaintenanceNext = CornerkickManager.Stadium.bdgTrainingCourts.iCostMaintenance[clb.buildings.bgTrainingCourts.iLevel + 1];
        bdgsAll[iType].bDispoOk = CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgTrainingCourts.iLevel >= 0 || (clb.buildings.bgTrainingCourts.ctn != null && clb.buildings.bgTrainingCourts.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                                                                                                buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Gym
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgGym.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgGym.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgGym.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgGym.sLevelNames[clb.buildings.bgGym.iLevel];
      bdgsAll[iType].iCostMaintenance = CornerkickManager.Stadium.bdgGym.iCostMaintenance[clb.buildings.bgGym.iLevel];
      if (clb.buildings.bgGym.iLevel + 1 < CornerkickManager.Stadium.bdgGym.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgGym.sLevelNames[clb.buildings.bgGym.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgGym.iGround[Math.Max(clb.buildings.bgGym.iLevel, clb.buildings.bgGym.ctn != null ? clb.buildings.bgGym.ctn.iLevelNew : (byte)0)];
      if (clb.buildings.bgGym.ctn != null && clb.buildings.bgGym.ctn.iLevelNew > clb.buildings.bgGym.iLevel) {
        bdgsAll[iType].fDaysConstruct = clb.buildings.bgGym.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildGym(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildGym(clb.buildings.bgGym.iLevel + 1, clb.buildings.bgGym.iLevel);
        bdgsAll[iType].iCostConstructNext = iCostDays[0];
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        if (clb.buildings.bgGym.iLevel + 1 < CornerkickManager.Stadium.bdgGym.iCostMaintenance.Length) bdgsAll[iType].iCostMaintenanceNext = CornerkickManager.Stadium.bdgGym.iCostMaintenance[clb.buildings.bgGym.iLevel + 1];
        bdgsAll[iType].bDispoOk = CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgGym.iLevel > 0 || (clb.buildings.bgGym.ctn != null && clb.buildings.bgGym.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                                                              buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Spa
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgSpa.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgSpa.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgSpa.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgSpa.sLevelNames[clb.buildings.bgSpa.iLevel];
      bdgsAll[iType].iCostMaintenance = CornerkickManager.Stadium.bdgSpa.iCostMaintenance[clb.buildings.bgSpa.iLevel];
      if (clb.buildings.bgSpa.iLevel + 1 < CornerkickManager.Stadium.bdgSpa.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgSpa.sLevelNames[clb.buildings.bgSpa.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgSpa.iGround[Math.Max(clb.buildings.bgSpa.iLevel, clb.buildings.bgSpa.ctn != null ? clb.buildings.bgSpa.ctn.iLevelNew : (byte)0)];
      if (clb.buildings.bgSpa.ctn != null && clb.buildings.bgSpa.ctn.iLevelNew > clb.buildings.bgSpa.iLevel) {
        bdgsAll[iType].fDaysConstruct = clb.buildings.bgSpa.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildSpa(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildSpa(clb.buildings.bgSpa.iLevel + 1, clb.buildings.bgSpa.iLevel);
        bdgsAll[iType].iCostConstructNext = iCostDays[0];
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        if (clb.buildings.bgSpa.iLevel + 1 < CornerkickManager.Stadium.bdgSpa.iCostMaintenance.Length) bdgsAll[iType].iCostMaintenanceNext = CornerkickManager.Stadium.bdgSpa.iCostMaintenance[clb.buildings.bgSpa.iLevel + 1];
        bdgsAll[iType].bDispoOk = CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgSpa.iLevel > 0 || (clb.buildings.bgSpa.ctn != null && clb.buildings.bgSpa.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                                                              buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Jouth internat
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgJouthInternat.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgJouthInternat.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgJouthInternat.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgJouthInternat.sLevelNames[clb.buildings.bgJouthInternat.iLevel];
      bdgsAll[iType].iCostMaintenance = CornerkickManager.Stadium.bdgJouthInternat.iCostMaintenance[clb.buildings.bgJouthInternat.iLevel];
      if (clb.buildings.bgJouthInternat.iLevel + 1 < CornerkickManager.Stadium.bdgJouthInternat.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgJouthInternat.sLevelNames[clb.buildings.bgJouthInternat.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgJouthInternat.iGround[Math.Max(clb.buildings.bgJouthInternat.iLevel, clb.buildings.bgJouthInternat.ctn != null ? clb.buildings.bgJouthInternat.ctn.iLevelNew : (byte)0)];
      if (clb.buildings.bgJouthInternat.ctn != null && clb.buildings.bgJouthInternat.ctn.iLevelNew > clb.buildings.bgJouthInternat.iLevel) {
        bdgsAll[iType].fDaysConstruct = clb.buildings.bgJouthInternat.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildJouthInternat(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildJouthInternat(clb.buildings.bgJouthInternat.iLevel + 1, clb.buildings.bgJouthInternat.iLevel);
        bdgsAll[iType].iCostConstructNext = iCostDays[0];
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        if (clb.buildings.bgJouthInternat.iLevel + 1 < CornerkickManager.Stadium.bdgJouthInternat.iCostMaintenance.Length) bdgsAll[iType].iCostMaintenanceNext = CornerkickManager.Stadium.bdgJouthInternat.iCostMaintenance[clb.buildings.bgJouthInternat.iLevel + 1];
        bdgsAll[iType].bDispoOk = CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgJouthInternat.iLevel > 0 || (clb.buildings.bgJouthInternat.ctn != null && clb.buildings.bgJouthInternat.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                                                                                            buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Club House
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgClubHouse.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgClubHouse.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgClubHouse.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgClubHouse.sLevelNames[clb.buildings.bgClubHouse.iLevel];
      bdgsAll[iType].iCostMaintenance = CornerkickManager.Stadium.bdgClubHouse.iCostMaintenance[clb.buildings.bgClubHouse.iLevel];
      if (clb.buildings.bgClubHouse.iLevel + 1 < CornerkickManager.Stadium.bdgClubHouse.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgClubHouse.sLevelNames[clb.buildings.bgClubHouse.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgClubHouse.iGround[Math.Max(clb.buildings.bgClubHouse.iLevel, clb.buildings.bgClubHouse.ctn != null ? clb.buildings.bgClubHouse.ctn.iLevelNew : (byte)0)];
      if (clb.buildings.bgClubHouse.ctn != null && clb.buildings.bgClubHouse.ctn.iLevelNew > clb.buildings.bgClubHouse.iLevel) {
        bdgsAll[iType].fDaysConstruct = clb.buildings.bgClubHouse.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildClubHouse(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildClubHouse(clb.buildings.bgClubHouse.iLevel + 1, clb.buildings.bgClubHouse.iLevel);
        bdgsAll[iType].iCostConstructNext = iCostDays[0];
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        if (clb.buildings.bgClubHouse.iLevel + 1 < CornerkickManager.Stadium.bdgClubHouse.iCostMaintenance.Length) bdgsAll[iType].iCostMaintenanceNext = CornerkickManager.Stadium.bdgClubHouse.iCostMaintenance[clb.buildings.bgClubHouse.iLevel + 1];
        bdgsAll[iType].bDispoOk = CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgClubHouse.iLevel > 0 || (clb.buildings.bgClubHouse.ctn != null && clb.buildings.bgClubHouse.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                                                                                buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Club Museum
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgClubMuseum.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgClubMuseum.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgClubMuseum.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgClubMuseum.sLevelNames[clb.buildings.bgClubMuseum.iLevel];
      bdgsAll[iType].iCostMaintenance = CornerkickManager.Stadium.bdgClubMuseum.iCostMaintenance[clb.buildings.bgClubMuseum.iLevel];
      if (clb.buildings.bgClubMuseum.iLevel + 1 < CornerkickManager.Stadium.bdgClubMuseum.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgClubMuseum.sLevelNames[clb.buildings.bgClubMuseum.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgClubMuseum.iGround[Math.Max(clb.buildings.bgClubMuseum.iLevel, clb.buildings.bgClubMuseum.ctn != null ? clb.buildings.bgClubMuseum.ctn.iLevelNew : (byte)0)];
      if (clb.buildings.bgClubMuseum.ctn != null && clb.buildings.bgClubMuseum.ctn.iLevelNew > clb.buildings.bgClubMuseum.iLevel) {
        bdgsAll[iType].fDaysConstruct = clb.buildings.bgClubMuseum.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildClubMuseum(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildClubMuseum(clb.buildings.bgClubMuseum.iLevel + 1, clb.buildings.bgClubMuseum.iLevel);
        bdgsAll[iType].iCostConstructNext = iCostDays[0];
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        if (clb.buildings.bgClubMuseum.iLevel + 1 < CornerkickManager.Stadium.bdgClubMuseum.iCostMaintenance.Length) bdgsAll[iType].iCostMaintenanceNext = CornerkickManager.Stadium.bdgClubMuseum.iCostMaintenance[clb.buildings.bgClubMuseum.iLevel + 1]  ;
        bdgsAll[iType].bDispoOk = CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgClubMuseum.iLevel > 0 || (clb.buildings.bgClubMuseum.ctn != null && clb.buildings.bgClubMuseum.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                                                                                   buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Carpark
      iType++;
      iCostDays = CornerkickManager.Stadium.getCostDaysContructCarpark(Math.Max(clb.stadium.facility.iCarpark + 1, clb.stadium.facility.iCarparkNew), clb.stadium.facility.iCarpark, _usr);
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sCarparkName;
      bdgsAll[iType].bTypeInt = true;
      bdgsAll[iType].iLevel = clb.stadium.facility.iCarpark;
      bdgsAll[iType].sName = clb.stadium.facility.iCarpark.ToString();
      bdgsAll[iType].sNameNext = Math.Max(clb.stadium.facility.iCarpark + 1, clb.stadium.facility.iCarparkNew).ToString();
      bdgsAll[iType].nRepeat = (int)Math.Ceiling(Math.Max(clb.stadium.facility.iCarpark, clb.stadium.facility.iCarparkNew) / (float)CornerkickManager.Stadium.iCarparkPerGround);
      bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
      bdgsAll[iType].iLevelReq = CornerkickManager.UI.getRequiredFeature(clb, 2);
      if (clb.stadium.facility.iCarparkNew > clb.stadium.facility.iCarpark) {
        bdgsAll[iType].fDaysConstruct = clb.stadium.facility.iCarparkDaysConstruct;
      } else {
        bdgsAll[iType].iCostConstructNext = iCostDays[0];
        bdgsAll[iType].bDispoOk = CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.stadium.facility.iCarpark > 0 || clb.stadium.facility.iCarparkNew > 0) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                           buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Ticketcounter
      iType++;
      iCostDays = CornerkickManager.Stadium.getCostDaysContructTicketcounter(Math.Max(clb.stadium.facility.iTicketcounter + 1, clb.stadium.facility.iTicketcounterNew), clb.stadium.facility.iTicketcounter, _usr);
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.sTicketcounterName;
      bdgsAll[iType].bTypeInt = true;
      bdgsAll[iType].iLevel = clb.stadium.facility.iTicketcounter;
      bdgsAll[iType].sName = clb.stadium.facility.iTicketcounter.ToString();
      bdgsAll[iType].sNameNext = Math.Max(clb.stadium.facility.iTicketcounter + 1, clb.stadium.facility.iTicketcounterNew).ToString();
      bdgsAll[iType].nRepeat = (int)Math.Ceiling(Math.Max(clb.stadium.facility.iTicketcounter, clb.stadium.facility.iTicketcounterNew) / (float)CornerkickManager.Stadium.iTicketcounterPerGround);
      bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
      bdgsAll[iType].iLevelReq = CornerkickManager.UI.getRequiredFeature(clb, 3);
      if (clb.stadium.facility.iTicketcounterNew > clb.stadium.facility.iTicketcounter) {
        bdgsAll[iType].fDaysConstruct = clb.stadium.facility.iTicketcounterDaysConstruct;
      } else {
        bdgsAll[iType].iCostConstructNext = iCostDays[0];
        bdgsAll[iType].bDispoOk = CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.stadium.facility.iTicketcounter > 0 || clb.stadium.facility.iTicketcounterNew > 0) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                                       buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Fanshops
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = clb.buildings.bgFanshop.sName;
      bdgsAll[iType].bTypeInt = true;
      bdgsAll[iType].iLevel = clb.buildings.bgFanshop.iLevel;
      bdgsAll[iType].sName = clb.buildings.bgFanshop.iLevel.ToString();
      bdgsAll[iType].nRepeat = (int)Math.Ceiling(Math.Max(clb.buildings.bgFanshop.iLevel, clb.buildings.bgFanshop.ctn != null ? clb.buildings.bgFanshop.ctn.iLevelNew : (byte)0) / (float)CornerkickManager.Stadium.iFanshopPerGround);
      bdgsAll[iType].iLevelReq = CornerkickManager.UI.getRequiredFeature(clb, 4, iCustomers: (int)clb.getAttractionFactor(CkAppShared.ckMng.iSeason, ltCups: CkAppShared.ckMng.ltCups, dtNow: CkAppShared.ckMng.dtDatum));
      if (clb.buildings.bgFanshop.ctn != null && clb.buildings.bgFanshop.ctn.iLevelNew > clb.buildings.bgFanshop.iLevel) {
        bdgsAll[iType].sNameNext = clb.buildings.bgFanshop.ctn.iLevelNew.ToString();
        bdgsAll[iType].fDaysConstruct = clb.buildings.bgFanshop.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildFanshop(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildFanshop(clb.buildings.bgFanshop.iLevel + 1, clb.buildings.bgFanshop.iLevel);
        bdgsAll[iType].sNameNext = (clb.buildings.bgFanshop.iLevel + 1).ToString();
        bdgsAll[iType].iCostConstructNext = iCostDays[0];
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        bdgsAll[iType].bDispoOk = CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgFanshop.iLevel > 0 || (clb.buildings.bgFanshop.ctn != null && clb.buildings.bgFanshop.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                                                                          buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      // Mass-transit
      iType++;
      bdgsAll[iType].iType = iType;
      bdgsAll[iType].sCategory = CornerkickManager.Stadium.bdgMassTransit.sTypeName;
      bdgsAll[iType].iLevel = clb.buildings.bgMassTransit.iLevel;
      bdgsAll[iType].iLevelMax = CornerkickManager.Stadium.bdgMassTransit.sLevelNames.Length - 1;
      bdgsAll[iType].sName = CornerkickManager.Stadium.bdgMassTransit.sLevelNames[clb.buildings.bgMassTransit.iLevel];
      bdgsAll[iType].iCostMaintenance = CornerkickManager.Stadium.bdgMassTransit.iCostMaintenance[clb.buildings.bgMassTransit.iLevel];
      if (clb.buildings.bgMassTransit.iLevel + 1 < CornerkickManager.Stadium.bdgMassTransit.sLevelNames.Length) bdgsAll[iType].sNameNext = CornerkickManager.Stadium.bdgMassTransit.sLevelNames[clb.buildings.bgMassTransit.iLevel + 1];
      bdgsAll[iType].nRepeat = CornerkickManager.Stadium.bdgMassTransit.iGround[Math.Max(clb.buildings.bgMassTransit.iLevel, clb.buildings.bgMassTransit.ctn != null ? clb.buildings.bgMassTransit.ctn.iLevelNew : (byte)0)];
      if (clb.buildings.bgMassTransit.ctn != null && clb.buildings.bgMassTransit.ctn.iLevelNew > clb.buildings.bgMassTransit.iLevel) {
        bdgsAll[iType].fDaysConstruct = clb.buildings.bgMassTransit.ctn.fDaysConstruct;
        bdgsAll[iType].nDaysConstructTotal = CornerkickManager.Stadium.getCostDaysBuildMassTransit(clb)[1];
      } else {
        iCostDays = CornerkickManager.Stadium.getCostDaysBuildMassTransit(clb.buildings.bgMassTransit.iLevel + 1, clb.buildings.bgMassTransit.iLevel);
        bdgsAll[iType].iCostConstructNext = iCostDays[0];
        bdgsAll[iType].nDaysConstructTotal = iCostDays[1];
        if (clb.buildings.bgMassTransit.iLevel + 1 < CornerkickManager.Stadium.bdgMassTransit.iCostMaintenance.Length) bdgsAll[iType].iCostMaintenanceNext = CornerkickManager.Stadium.bdgMassTransit.iCostMaintenance[clb.buildings.bgMassTransit.iLevel + 1];
        bdgsAll[iType].bDispoOk = CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb);
      }
      if (clb.buildings.bgMassTransit.iLevel > 0 || (clb.buildings.bgMassTransit.ctn != null && clb.buildings.bgMassTransit.ctn.iLevelNew > 0)) buildings.ltBuildings    .Add(bdgsAll[iType]);
      else                                                                                                                                      buildings.ltBuildingsFree.Add(bdgsAll[iType]);

      buildings.iGround = (byte)Math.Max(clb.buildings.iGround, buildings.ltBuildings.Count);
      buildings.sCostBuyGround = CornerkickManager.Stadium.getCostBuyGround(clb.buildings.iGround).ToString("N0", MemberController.getCi(_usr));

      return buildings;
    }

    public static StadiumSurroundingsModel.Building? GetTypeNumber(CornerkickManager.User? _usr, int iType, int iNew, int iCurrent)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      StadiumSurroundingsModel.Building bdg = new StadiumSurroundingsModel.Building();

      bdg.iType = (byte)iType;
      bdg.iLevel = (iCurrent + iNew) - 1;
      bdg.sNameNext = (iCurrent + iNew).ToString();

      int[] iCostDays = new int[2];
      if      (iType == 6) iCostDays = CornerkickManager.Stadium.getCostDaysContructCarpark(clb.stadium.facility.iCarpark + iNew, clb.stadium.facility.iCarpark, _usr);
      else if (iType == 7) iCostDays = CornerkickManager.Stadium.getCostDaysContructTicketcounter(clb.stadium.facility.iTicketcounter + iNew, clb.stadium.facility.iTicketcounter, _usr);
      else if (iType == 8) iCostDays = CornerkickManager.Stadium.getCostDaysBuildBuilding(clb, iType, clb.buildings.bgFanshop.iLevel + iNew);

      bdg.iCostConstructNext  = iCostDays[0];
      bdg.nDaysConstructTotal = iCostDays[1];

      return bdg;
    }

    public static bool BuildBuilding(CornerkickManager.User? _usr, int iType, int iLevel, out string sRet)
    {
      sRet = String.Empty;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return false;

      string[] sConstructionNames = new string[] {
        CornerkickManager.Stadium.bdgTrainingCourts.sTypeName,
        CornerkickManager.Stadium.bdgGym.sTypeName,
        CornerkickManager.Stadium.bdgSpa.sTypeName,
        CornerkickManager.Stadium.bdgJouthInternat.sTypeName,
        CornerkickManager.Stadium.bdgClubHouse.sTypeName,
        CornerkickManager.Stadium.bdgClubMuseum.sTypeName,
        CornerkickManager.Stadium.sCarparkName,
        CornerkickManager.Stadium.sTicketcounterName,
        CornerkickManager.Stadium.sFanshopName,
        CornerkickManager.Stadium.bdgMassTransit.sTypeName
      };

      if (iType == 6) { // Carpark
        if (clb.stadium.facility.iCarparkNew != iLevel) {
          // Check if enough grounds are available
          if (Math.Ceiling(iLevel / (float)CornerkickManager.Stadium.iCarparkPerGround) > Math.Ceiling(clb.stadium.facility.iCarpark / (float)CornerkickManager.Stadium.iCarparkPerGround)) {
            if (clb.buildings.iGround <= CornerkickManager.Stadium.getRequiredGrounds(clb)) {
              sRet = "Sie benötigen erst ein neues Grundstück";
              return false;
            }
          }

          clb.stadium.facility.iCarparkNew = iLevel;
          int[] iCostDaysCp = CornerkickManager.Stadium.getCostDaysContructCarpark(iLevel, clb.stadium.facility.iCarpark, _usr);
          clb.stadium.facility.iCarparkDaysConstruct = iCostDaysCp[1];

          CornerkickManager.Finance.doTransaction(clb, CkAppShared.ckMng.dtDatum, -iCostDaysCp[0], CornerkickManager.Finance.iTransferralTypePayStadiumSurr, "Bau " + sConstructionNames[iType]);
        }
      } else if (iType == 7) { // Ticketcounter
        if (clb.stadium.facility.iTicketcounterNew != iLevel) {
          // Check if enough grounds are available
          if (Math.Ceiling(iLevel / (float)CornerkickManager.Stadium.iTicketcounterPerGround) > Math.Ceiling(clb.stadium.facility.iTicketcounter / (float)CornerkickManager.Stadium.iTicketcounterPerGround)) {
            if (clb.buildings.iGround <= CornerkickManager.Stadium.getRequiredGrounds(clb)) {
              sRet = "Sie benötigen erst ein neues Grundstück";
              return false;
            }
          }

          clb.stadium.facility.iTicketcounterNew = (byte)iLevel;
          int[] iCostDaysTc = CornerkickManager.Stadium.getCostDaysContructTicketcounter(iLevel, clb.stadium.facility.iTicketcounter, _usr);
          clb.stadium.facility.iTicketcounterDaysConstruct = iCostDaysTc[1];

          CornerkickManager.Finance.doTransaction(clb, CkAppShared.ckMng.dtDatum, -iCostDaysTc[0], CornerkickManager.Finance.iTransferralTypePayStadiumSurr, "Bau " + sConstructionNames[iType]);
        }
      } else {
        if (!CornerkickManager.UI.doConstruction(clb, iType, (byte)iLevel, CkAppShared.ckMng.dtDatum, sConstructionNames[iType])) {
          sRet = "Sie benötigen erst ein neues Grundstück";
          return false;
        }
      }

      sRet = "Der Bau des <b>" + sConstructionNames[iType] + "s</b> wurde in Auftrag gegeben";
      return true;
    }

    public static int[]? GetCostDaysNumber(CornerkickManager.User _usr, int iType, int iCurrent, int iNew)
    {
      if (_usr == null) return null;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      int[] iCostDays = new int[2];
      if      (iType == 6) iCostDays = CornerkickManager.Stadium.getCostDaysContructCarpark(iCurrent + iNew, iCurrent, _usr);
      else if (iType == 7) iCostDays = CornerkickManager.Stadium.getCostDaysContructTicketcounter(iCurrent + iNew, iCurrent, _usr);
      else if (iType == 8) iCostDays = CornerkickManager.Stadium.getCostDaysBuildBuilding(clb, iType, iCurrent + iNew);

      return iCostDays;
    }

    public static bool BuyGround(CornerkickManager.User? _usr)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return false;

      int iCost = CornerkickManager.Stadium.getCostBuyGround(clb.buildings.iGround);

      // Check dispo
      if (!CkAppShared.ckMng.fz.checkDispoLimit(iCost, clb)) {
        return false;
      }

      // Add and pay ground
      clb.buildings.iGround++;
      CornerkickManager.Finance.doTransaction(clb, CkAppShared.ckMng.dtDatum, -iCost, CornerkickManager.Finance.iTransferralTypePayStadiumSurrGround);

      return true;
    }


  }
}
