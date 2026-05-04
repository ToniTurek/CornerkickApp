using CornerkickApp.Shared.Models;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CornerkickApp.Controllers.Member
{
  public class StadiumController
  {
    public static StadiumModel Model(CornerkickManager.User? _usr, int iStadium = 0)
    {
      StadiumModel model = new StadiumModel();

      if (_usr == null) return model;

      CornerkickGame.Stadium? stadium = GetStadium(_usr, iStadium);
      if (stadium?.blocks == null) return model;

      if (iStadium == 0) {
        CornerkickManager.Club? clb = MemberController.ckClub(_usr);
        if (clb == null) return model;

        // Stadium name editable
        model.bEditable = (CkAppShared.ckMng.dtDatum - _usr.dtClubStart).TotalHours < 24;
        model.bEditable = model.bEditable || (stadium.sName.StartsWith("Team_") && stadium.sName.EndsWith(" Stadion"));
        model.bEditable = model.bEditable || stadium.sName.Equals(clb.sName + " Stadion");

        model.iSnackbarReq = (byte)CornerkickManager.UI.getRequiredFeature(clb, 0);
        model.iToiletsReq  = (byte)CornerkickManager.UI.getRequiredFeature(clb, 1);
      }


      model.bSound = true;
      if (_usr.lti.Count > UserOptionsModel.iUserOptionsIxSound) model.bSound = _usr.lti[UserOptionsModel.iUserOptionsIxSound] > 0;

      //model.stadium = clb.stadium;
      //model.sBlocksJSON = JsonConvert.SerializeObject(model.stadium.blocks);

      model.sName = stadium.sName;
      model.fPitchQuality = stadium.fPitchQuality;

      model.iSeats = new int[3];
      model.iSeatsConstr = new int[3];
      for (byte iBT = 0; iBT < 3; iBT++) {
        model.iSeats[iBT] = stadium.getSeats(iType: iBT, iModeConstruction: 0);
        model.iSeatsConstr[iBT] = stadium.getSeats(iType: iBT, iModeConstruction: 1);
      }

      // Topring
      model.bTopring = StadiumHasTopring(stadium);

      // Blocks
      model.blocks = new StadiumModel.Block[model.bTopring ? stadium.blocks.Length : 10];
      for (byte iB = 0; iB < model.blocks.Length; iB++) {
        model.blocks[iB] = new StadiumModel.Block() {
          sName                  = stadium.blocks[iB].sName,
          iSeats                 = stadium.blocks[iB].iSeats,
          iType                  = stadium.blocks[iB].iType,
          bRoof                  = stadium.blocks[iB].bRoof,
          iSeatsDaysConstruct    = stadium.blocks[iB].iSeatsDaysConstruct,
          iSeatsDaysConstructIni = stadium.blocks[iB].iSeatsDaysConstructIni
        };
      }

      // Set block position
      bool bMobile = false;
      const float fDivWidth = 0.07f;
      const float fDivHeight = 0.04f;
      model.blocks[ 0].X = 0.27f;                              model.blocks[ 0].Y = model.bTopring ? 0.77f : 0.73f;        // A
      model.blocks[ 1].X = 0.50f - (fDivWidth / 2f);           model.blocks[ 1].Y = model.blocks[ 0].Y; // B
      model.blocks[ 2].X = 1f - model.blocks[0].X - fDivWidth; model.blocks[ 2].Y = model.blocks[ 0].Y; // C
      model.blocks[ 3].X = bMobile ? 0.72f : 0.80f;            model.blocks[ 3].Y = 0.55f;        // D
      model.blocks[ 4].X = model.blocks[ 3].X;                 model.blocks[ 4].Y = 0.32f;        // E
      model.blocks[ 5].X = model.blocks[ 2].X;                 model.blocks[ 5].Y = model.bTopring ? 1f - model.blocks[ 0].Y - fDivHeight : 0.12f; // F
      model.blocks[ 6].X = model.blocks[ 1].X;                 model.blocks[ 6].Y = model.blocks[ 5].Y; // G
      model.blocks[ 7].X = model.blocks[ 0].X;                 model.blocks[ 7].Y = model.blocks[ 5].Y; // H
      model.blocks[ 8].X = bMobile && stadium.blocks[18].iSeats > 0 ? 1f - model.blocks[4].X - fDivWidth : 0.20f - fDivWidth; model.blocks[ 8].Y = model.blocks[4].Y; // I
      model.blocks[ 9].X = bMobile && stadium.blocks[19].iSeats > 0 ? 1f - model.blocks[3].X - fDivWidth : 0.20f - fDivWidth; model.blocks[ 9].Y = model.blocks[3].Y; // J

      if (model.bTopring) {
        model.blocks[10].X = model.blocks[ 0].X;                  model.blocks[10].Y = 0.9f;         // A1
        model.blocks[11].X = model.blocks[ 1].X;                  model.blocks[11].Y = model.blocks[10].Y; // B1
        model.blocks[12].X = model.blocks[ 2].X;                  model.blocks[12].Y = model.blocks[10].Y; // C1
        model.blocks[13].X = 0.88f;                               model.blocks[13].Y = model.blocks[ 3].Y; // D1
        model.blocks[14].X = model.blocks[13].X;                  model.blocks[14].Y = model.blocks[ 4].Y; // E1
        model.blocks[15].X = model.blocks[ 5].X;                  model.blocks[15].Y = bMobile ? 0.01f : 0.05f; // F1
        model.blocks[16].X = model.blocks[ 6].X;                  model.blocks[16].Y = model.blocks[15].Y; // G1
        model.blocks[17].X = model.blocks[ 7].X;                  model.blocks[17].Y = model.blocks[15].Y; // H1
        model.blocks[18].X = bMobile && stadium.blocks[18].iSeats > 0 ? 0.01f : 1 - model.blocks[14].X - fDivWidth; model.blocks[18].Y = model.blocks[ 8].Y; // I1
        model.blocks[19].X = bMobile && stadium.blocks[19].iSeats > 0 ? 0.01f : 1 - model.blocks[13].X - fDivWidth; model.blocks[19].Y = model.blocks[ 9].Y; // J1
        model.blocks[20].X = 0.08f;                               model.blocks[20].Y = 0.84f;        // K
        model.blocks[21].X = 1 - model.blocks[20].X - fDivWidth;  model.blocks[21].Y = model.blocks[20].Y; // L
        model.blocks[22].X = model.blocks[21].X;                  model.blocks[22].Y = 0.12f;        // M
        model.blocks[23].X = model.blocks[20].X;                  model.blocks[23].Y = model.blocks[22].Y; // N
      }

      // Blocks construction table
      /*
      model.sBlocksConstrName = new string[clb.stadium.blocks.Length];
      model.iBlocksConstrSeats = new int[clb.stadium.blocks.Length];
      model.sBlocksConstrType = new string[clb.stadium.blocks.Length];
      model.sBlocksConstrRoof = new bool[clb.stadium.blocks.Length];
      model.iBlocksConstrDays = new int[clb.stadium.blocks.Length];
      for (int i = 0; i < clb.stadium.blocks.Length; i++) {
        model.sBlocksConstrName[i] = clb.stadium.blocks[i].sName;
        model.iBlocksConstrSeats[i] = clb.stadium.blocks[i].iSeats;
        model.sBlocksConstrType[i] = CornerkickManager.Stadium.sBlocktype[clb.stadium.blocks[i].iType];
        model.sBlocksConstrRoof[i] = clb.stadium.blocks[i].bRoof;
        model.iBlocksConstrDays[i] = clb.stadium.blocks[i].iSeatsDaysConstruct;
      }
      */

      // Set facility
      model.facility = stadium.facility;

      if (stadium.facility.iVideoDaysConstruct == 0) stadium.facility.iVideoNew = stadium.facility.iVideo;
      model.iVideo = stadium.facility.iVideoNew;

      model.iSnackbar = stadium.facility.iSnackbar;
      model.iSnackbarNew = (byte)Math.Max(stadium.facility.iSnackbarNew - stadium.facility.iSnackbar, 0);

      model.iToilets = stadium.facility.iToilets;
      model.iToiletsNew  = (byte)Math.Max(stadium.facility.iToiletsNew - stadium.facility.iToilets, 0);

      if (stadium.facility.iSecurityDaysConstruct == 0) stadium.facility.iSecurityNew = stadium.facility.iSecurity;
      model.iSecurity = stadium.facility.iSecurityNew;

      model.iSecurityStaff = stadium.facility.iSecurityStaff;

      return model;
    }

    public static CornerkickGame.Stadium? GetStadium(CornerkickManager.User? usr, int iStadium)
    {
      if (usr == null) return null;

      return GetStadium(MemberController.ckClub(usr), iStadium: iStadium);
    }
    public static CornerkickGame.Stadium? GetStadium(CornerkickManager.Club? clb, int iStadium)
    {
      CornerkickGame.Stadium stadium = new CornerkickGame.Stadium();
      if (iStadium == 0) {
        if (clb == null) return null;

        stadium = clb.stadium;
      } else if (iStadium == -1) {
        stadium = CkAppShared.stadiumDefaultTrainingCourt;
      } else if (iStadium == 1) {
        stadium = CkAppShared.stadiumDefaultSmall;
      } else if (iStadium == 2) {
        stadium = CkAppShared.stadiumDefaultBig;
      }

      return stadium;
    }

    private static bool StadiumHasTopring(CornerkickGame.Stadium stdm)
    {
      return stdm.facility.bTopring && stdm.facility.iTopringDaysConstruct == 0;
    }

    public static bool setName(CornerkickManager.User? _usr, string sName)
    {
      if (_usr == null) return false;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return false;

      if (clb.stadium == null) return false;

      clb.stadium.sName = sName;

      return true;
    }

    public static float[] GetRenewPitchCost(CornerkickManager.User? _usr)
    {
      if (_usr == null) return new float[2];

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return new float[2];

      return [
        Math.Min(1f - clb.stadium.fPitchQuality, 0.1f),
        CkAppShared.ckMng.st.getCostStadiumRenewPitch(clb.stadium, 0.1f, user: _usr)
      ];
    }

    public static float RenewPitch(CornerkickManager.User? _usr, out string sMsg)
    {
      sMsg = "";
      if (_usr == null) return -1f;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return -1f;

      CkAppShared.ckMng.ui.renewStadiumPitch(ref clb, 0.1f);
      sMsg = "Der Stadionrasen wurde erneuert.";

      return clb.stadium.fPitchQuality;
    }

    public static int[]? GetBuildCost(CornerkickManager.User? _usr, int iBlock, int iSeats, int iType, bool bRoof)
    {
      if (_usr == null) return null;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      int iSeatsMax = CornerkickManager.Stadium.getMaxSeats(clb.stadium, (byte)iType);
      if (iSeatsMax > 0 && iSeats > iSeatsMax) return [iSeatsMax, -1, -1];

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.blocks[iBlock].iSeats = iSeats;
      stadiumNew.blocks[iBlock].iType = (byte)iType;
      stadiumNew.blocks[iBlock].bRoof = bRoof;

      int[] iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(stadiumNew, clb.stadium, user: _usr);

      int iDispoOk = 0;
      if (CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

      int[] iCostDaysDispo = [ iCostDays[0], iCostDays[1], iDispoOk ];

      return iCostDaysDispo;
    }

    public static int BuildBlock(CornerkickManager.User? _usr, int iBlock, int iSeats, int iType, bool bRoof, out string sMsg)
    {
      sMsg = "";

      if (_usr == null) return -1;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return -1;

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.blocks[iBlock].iSeats = iSeats;
      stadiumNew.blocks[iBlock].iType = (byte)iType;
      stadiumNew.blocks[iBlock].bRoof = bRoof;

      CkAppShared.ckMng.ui.buildStadium(ref clb, stadiumNew);

      sMsg = "Der Ausbau des Stadions wurde in Auftrag gegeben.";
      return clb.stadium.blocks[iBlock].iSeatsDaysConstruct;
    }

    public static int[][] GetExtras(CornerkickManager.User? _usr, int iStadium = 0)
    {
      CornerkickGame.Stadium? stadium = GetStadium(_usr, iStadium: iStadium);
      if (stadium == null) return new int[4][];

      int nVideoDaysConstract    = CornerkickManager.Stadium.facVideo   .iDaysConstruct[stadium.facility.iVideoNew];
      int nSecurityDaysConstract = CornerkickManager.Stadium.facSecurity.iDaysConstruct[stadium.facility.iSecurityNew];
      int nSnackbarDaysConstract = CornerkickManager.Stadium.getCostDaysContructSnackbar(stadium.facility.iSnackbarNew, stadium.facility.iSnackbar, _usr)[1];
      int nToiletsDaysConstract  = CornerkickManager.Stadium.getCostDaysContructToilets (stadium.facility.iToiletsNew,  stadium.facility.iToilets,  _usr)[1];

      return [
        [ stadium.facility.iVideo   , stadium.facility.iVideoNew,    stadium.facility.iVideoDaysConstruct,    nVideoDaysConstract    ],
        [ stadium.facility.iSecurity, stadium.facility.iSecurityNew, stadium.facility.iSecurityDaysConstruct, nSecurityDaysConstract ],
        [ stadium.facility.iSnackbar, stadium.facility.iSnackbarNew, stadium.facility.iSnackbarDaysConstruct, nSnackbarDaysConstract ],
        [ stadium.facility.iToilets,  stadium.facility.iToiletsNew,  stadium.facility.iToiletsDaysConstruct,  nToiletsDaysConstract  ]
      ];
    }

    // Video-wall
    public static int[] GetCostVideo(CornerkickManager.User? _usr, int iLevel)
    {
      int[] iCostDaysDispo = [0, 0, 0];

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return iCostDaysDispo;

      if (clb.stadium.facility.iVideoNew != iLevel) {
        int iDispoOk = 0;
        if (CkAppShared.ckMng.fz.checkDispoLimit(CornerkickManager.Stadium.facVideo.iCost[iLevel], clb)) iDispoOk = 1;

        iCostDaysDispo[0] = CornerkickManager.Stadium.facVideo.iCost[iLevel];
        iCostDaysDispo[1] = CornerkickManager.Stadium.facVideo.iDaysConstruct[iLevel];
        iCostDaysDispo[2] = iDispoOk;
      }

      return iCostDaysDispo;
    }

    public static float GetVideoInfo(CornerkickManager.User? _usr, int iLevel)
    {
      return CornerkickManager.Stadium.getVideoWallFactor(iLevel);
    }

    public static string BuildVideo(CornerkickManager.User? _usr, int iLevel)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return string.Empty;

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.facility.iVideo = (byte)iLevel;

      CkAppShared.ckMng.ui.buildStadium(ref clb, stadiumNew);

      return "Der Bau der Anzeigentafel wurde in Auftrag gegeben";
    }

    // Snackbars
    public static int[] GetCostSnackbar(CornerkickManager.User? _usr, int iBuildNew)
    {
      int[] iCostDaysDispo = [0, 0, 0];

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return iCostDaysDispo;

      if (iBuildNew != 0) {
        int iDispoOk = 0;
        int[] iCostDays = CornerkickManager.Stadium.getCostDaysContructSnackbar(clb.stadium.facility.iSnackbarNew + iBuildNew, clb.stadium.facility.iSnackbarNew, _usr);
        if (CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

        iCostDaysDispo[0] = iCostDays[0];
        iCostDaysDispo[1] = iCostDays[1];
        iCostDaysDispo[2] = iDispoOk;
      }

      return iCostDaysDispo;
    }

    public static string BuildSnackbar(CornerkickManager.User? _usr, int iBuildNew)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return string.Empty;

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.facility.iSnackbar = (byte)(stadiumNew.facility.iSnackbar + iBuildNew);

      CkAppShared.ckMng.ui.buildStadium(ref clb, stadiumNew);

      return "Der Ausbau der Imbissbuden wurde in Auftrag gegeben";
    }

    // Toilets
    public static int[] GetCostToilets(CornerkickManager.User? _usr, int iBuildNew)
    {
      int[] iCostDaysDispo = [0, 0, 0];

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return iCostDaysDispo;

      if (iBuildNew != 0) {
        int iDispoOk = 0;
        int[] iCostDays = CornerkickManager.Stadium.getCostDaysContructToilets(clb.stadium.facility.iToiletsNew + iBuildNew, clb.stadium.facility.iToiletsNew, _usr);
        if (CkAppShared.ckMng.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

        iCostDaysDispo[0] = iCostDays[0];
        iCostDaysDispo[1] = iCostDays[1];
        iCostDaysDispo[2] = iDispoOk;
      }

      return iCostDaysDispo;
    }

    public static string BuildToilets(CornerkickManager.User? _usr, int iBuildNew)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return string.Empty;

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.facility.iToilets = (byte)(clb.stadium.facility.iToilets + iBuildNew);

      CkAppShared.ckMng.ui.buildStadium(ref clb, stadiumNew);

      return "Der Ausbau der Toiletten wurde in Auftrag gegeben";
    }

    // Security
    public static int[] GetCostSecurity(CornerkickManager.User? _usr, int iLevel)
    {
      int[] iCostDaysDispo = [0, 0, 0];

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return iCostDaysDispo;

      if (iLevel >= CornerkickManager.Stadium.facSecurity.iCost.Length) return iCostDaysDispo;

      if (clb.stadium.facility.iSecurityNew != iLevel) {
        int iDispoOk = 0;
        if (CkAppShared.ckMng.fz.checkDispoLimit(CornerkickManager.Stadium.facSecurity.iCost[iLevel], clb)) iDispoOk = 1;

        iCostDaysDispo[0] = CornerkickManager.Stadium.facSecurity.iCost[iLevel];
        iCostDaysDispo[1] = CornerkickManager.Stadium.facSecurity.iDaysConstruct[iLevel];
        iCostDaysDispo[2] = iDispoOk;
      }

      return iCostDaysDispo;
    }

    public static float GetSecurityInfo(CornerkickManager.User? _usr, int iLevel, int iSecurityStaff = 0, int iStadium = 0)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return -1f;

      CornerkickGame.Stadium? stadium = GetStadium(_usr, iStadium);
      if (stadium == null) return -1f;

      int[] iSpec = [ stadium.getSeats(iType: 0), stadium.getSeats(iType: 1), stadium.getSeats(iType: 2) ];

      return (float)CornerkickManager.Main.getRiskSpecRiot(iSpec, iLevel, iSecurityStaff);
    }

    public static string BuildSecurity(CornerkickManager.User? _usr, int iLevel)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return string.Empty;

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.facility.iSecurity = (byte)iLevel;

      CkAppShared.ckMng.ui.buildStadium(ref clb, stadiumNew);

      return "Der Ausbau der Sicherheitsausstattung wurde in Auftrag gegeben";
    }

    public static double[] SetSecurityStaff(CornerkickManager.User? _usr, int nSecurityStaff, int iStadium = 0)
    {
      if (_usr == null) return new double[2];

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return new double[2];

      CornerkickGame.Stadium? stadium = GetStadium(_usr, iStadium);
      if (stadium == null) return new double[2];

      stadium.facility.iSecurityStaff = nSecurityStaff;

      int[] iSpec = [ stadium.getSeats(iType: 0), stadium.getSeats(iType: 1), stadium.getSeats(iType: 2) ];

      return [ CornerkickManager.Stadium.getCostSecurityStaff(nSecurityStaff, _usr), CornerkickManager.Main.getRiskSpecRiot(iSpec, stadium) ];
    }

    internal CornerkickGame.Stadium convertToStadion(int[] iSeats, int[] iSeatType, int[] iSeatsBuild)
    {
      CornerkickGame.Stadium stadium = new CornerkickGame.Stadium();
      if (iSeats != null) {
        for (int i = 0; i < iSeats.Length; i++) stadium.blocks[i].iSeats = iSeats[i];
      }
      if (iSeatType != null) {
        for (int i = 0; i < iSeatType.Length; i++) stadium.blocks[i].iType = (byte)iSeatType[i];
      }
      if (iSeatsBuild != null) {
        for (int i = 0; i < iSeatsBuild.Length; i++) stadium.blocks[i].iSeatsDaysConstruct = iSeatsBuild[i];
      }

      return stadium;
    }

  }
}
