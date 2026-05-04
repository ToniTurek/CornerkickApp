using CornerkickApp.Shared.Models;

namespace CornerkickApp.Controllers.Member
{
  public class JouthController
  {
    public static JouthModel Model(CornerkickManager.User _usr)
    {
      JouthModel model = new JouthModel();
      CornerkickManager.Club clb = MemberController.ckClub(_usr);

      return model;
    }

    public static Task<List<TeamModel.TableTeam>>? GetTableJouth(CornerkickManager.User usr)
    {
      var clb = MemberController.ckClub(usr);
      if (clb == null) return null;

      List<TeamModel.TableTeam> tblJouth = new List<TeamModel.TableTeam>();

      foreach (CornerkickManager.Player plJ in clb.ltPlayerJouth) {
        TeamModel.TableTeam dtJouth = new TeamModel.TableTeam();

        float[] fSkills = usr.bScouting ? clb.staff.getScoutedSkills(plJ.plGame) : null;

        dtJouth.iId = plJ.plGame.iId;
        dtJouth.sName = plJ.plGame.sName;
        dtJouth.fAge = plJ.plGame.getAge(CkAppShared.ckMng.dtDatum);
        dtJouth.sHp = CornerkickManager.PlayerTool.getStrPos(plJ);
        dtJouth.fSkill = CornerkickGame.Tool.getAveSkill(plJ.plGame, fSkills: fSkills, bIdeal: true);
        dtJouth.fTalent = plJ.getTalentAve() + 1f;
        dtJouth.sNat = CornerkickManager.Main.sLandShort[plJ.iNat1];

        tblJouth.Add(dtJouth);
      }

      return Task.FromResult(tblJouth);
    }
  }
}
