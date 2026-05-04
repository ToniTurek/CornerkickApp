using CornerkickApp.Shared.Models;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  public class LeagueController
  {
    public static LeagueModel Get(CornerkickManager.User _usr)
    {
      LeagueModel model = new LeagueModel();

      model.iSeason = CkAppShared.ckMng.iSeason;
      model.ddlSeason = MemberController.getDdlSeason();

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb != null) {
        model.iClubId = clb.iId;
        model.iLand = clb.iLand;
        model.iDivision = getClubDivisionId(clb);

        model.iMatchday = CupController.getCupMatchday(model.iSeason, CkAppShared.iCupIdLeague, model.iLand, model.iDivision, clb: clb) - 1;
        model.iMatchdayCurrent = model.iMatchday;
      }

      CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, model.iLand, model.iDivision);
      if (league == null) return model;

      model.sCupName = league.sName;
      model.sCupEmblem = CupController.getCupEmblem(league);
      model.iLeagueSize = league.getParticipants(model.iMatchday, CkAppShared.ckMng.dtDatum).Count;

      //CkAppShared.iSeasonGlobal = CkAppShared.ckMng.iSeason;

      model.ddlLand = MemberController.getDdlLand(CkAppShared.iCupIdLeague, iLandSelected: model.iLand);
      model.ddlDivision = getDdlDivisions(model.iLand, iDivSelected: model.iDivision);
      model.ddlMatchdays = getDdlMatchdays(model.iSeason, model.iLand, model.iDivision, iMdSelected: model.iMatchday);

      return model;
    }

    public static List<SelectListItem> getDdlDivisions(int iLand, int iDivSelected = -1)
    {
      List<SelectListItem> ltDiv = new List<SelectListItem>();

      byte iDiv = 0;
      CornerkickManager.Cup cupDiv;
      while ((cupDiv = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, iLand, iDiv)) != null) {
        ltDiv.Add(new SelectListItem { Text = cupDiv.sName, Value = iDiv.ToString(), Selected = iDiv == iDivSelected });
        iDiv++;
      }

      return ltDiv;
    }

    public static List<SelectListItem> getDdlMatchdays(int iSeason, int iLand, int iDivision, int iMdSelected = -1)
    {
      //CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, iLand, iDivision);
      CornerkickManager.Cup? league = MemberController.getCup(iSeason, CkAppShared.iCupIdLeague, iLand, iDivision);
      if (league == null) return new List<SelectListItem>();

      List<SelectListItem> ltMd = new List<SelectListItem>();
      // Spieltage zu Dropdown Menü hinzufügen
      for (int iMd = 0; iMd < league.getMatchdaysTotal(); iMd++) {
        ltMd.Add(new SelectListItem {
          Text = (iMd + 1).ToString(),
          Value = iMd.ToString(),
          Selected = iMd == iMdSelected
        });
      }

      return ltMd;
    }

    public static List<LeagueModel.GameInfo> getGameInfos(int iSeason, int iLand, int iDivision, int iMd = -1, CornerkickManager.User? usr = null, bool bCompact = false)
    {
      List<LeagueModel.GameInfo> ltGameInfos = new List<LeagueModel.GameInfo>();

      //CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, iLand, iDivision);
      CornerkickManager.Cup? league = MemberController.getCup(iSeason, CkAppShared.iCupIdLeague, iLand, iDivision);
      if (league == null) return ltGameInfos;

      return MemberController.getGameInfos(league, iMd, usr: usr, bCompact: bCompact);
    }

    public static List<LeagueModel.TableItem> getTable(int iSeason, int iCupId, int iLand, int iDivision = -1, int iMd = -1, byte iGroup = 0)
    {
      List<LeagueModel.TableItem> ltClubs = new List<LeagueModel.TableItem>();

      //CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, iLand, iDivision);
      CornerkickManager.Cup? league = MemberController.getCup(iSeason, iCupId, iLand, iDivision);
      if (league == null) return ltClubs;

      return CupController.getTable(league, iMd: iMd, iGroup: iGroup);
    }

    public static CornerkickManager.Cup? getClubDivision(CornerkickManager.Club? clb)
    {
      if (clb == null) return null;

      List<CornerkickManager.Cup>? ltLeagues = CkAppShared.ckMng.ltCups.Where(c => c.iId == CkAppShared.iCupIdLeague && c.iId2 == clb.iLand).ToList();
      if (ltLeagues == null) return null;

      foreach (CornerkickManager.Cup l in ltLeagues) {
        if (l.ltClubs == null) continue;
        if (l.ltClubs.Length < 1) continue;
        foreach (List<CornerkickManager.Club> ltClb in l.ltClubs) {
          if (ltClb.Find(c => c.iId == clb.iId) != null) return l;
        }
      }
      return null;
    }

    public static int getClubDivisionId(CornerkickManager.Club? clb)
    {
      if (clb == null) return -1;

      List<CornerkickManager.Cup>? ltLeagues = CkAppShared.ckMng.ltCups.Where(c => c.iId == CkAppShared.iCupIdLeague && c.iId2 == clb.iLand).ToList();
      if (ltLeagues == null) return -1;

      foreach (CornerkickManager.Cup l in ltLeagues) {
        if (l.ltClubs == null) continue;
        if (l.ltClubs.Length < 1) continue;
        foreach (List<CornerkickManager.Club> ltClb in l.ltClubs) {
          if (ltClb.Find(c => c.iId == clb.iId) != null) return l.iId3;
        }
      }
      return -1;
    }

  }
}
