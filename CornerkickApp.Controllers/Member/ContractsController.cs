using CornerkickApp.Shared.Models;
using Newtonsoft.Json;

namespace CornerkickApp.Controllers.Member
{
  public class ContractsController
  {
    public static ContractsModel Model(CornerkickManager.User _usr)
    {
      ContractsModel model = new ContractsModel();
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);

      return model;
    }

    public static List<ContractsModel.TableEntry> GetTable(CornerkickManager.User? _usr, bool bPro, bool bJouth, byte iPos, bool bNextSeason)
    {
      List<ContractsModel.TableEntry> ltTable = new List<ContractsModel.TableEntry>();

      if (_usr == null) return ltTable;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return ltTable;

      // Get staff for scouting
      CornerkickManager.Main.Staff? staff = null;
      if (_usr.bScouting) staff = clb.staff;

      // Contract
      int iGamesPerSeason = 0;
      CornerkickManager.Cup? league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, clb.iLand, clb.iDivision);
      if (league != null) iGamesPerSeason += league.getMatchdays(clb);
      CornerkickManager.Cup? cupNat = CkAppShared.ckMng.ltCups.Find(c => c.iId == CkAppShared.iCupIdNatCup && c.iId2 == clb.iLand);
      if (cupNat != null) iGamesPerSeason += 1;

      int iGameType = 0;
      if (clb.nextGame != null) iGameType = clb.nextGame.iGameType;

      List<List<CornerkickManager.Player>> ltPlayerProJouth = new List<List<CornerkickManager.Player>>();
      if (bPro) ltPlayerProJouth.Add(clb.ltPlayer);
      if (bJouth) ltPlayerProJouth.Add(clb.ltPlayerJouth);

      bool bJouth2 = !bPro;
      foreach (List<CornerkickManager.Player> ltPlayerTeam in ltPlayerProJouth) {
        // Update player numbers if nation
        if (clb.bNation) {
          for (byte iP = 0; iP < Math.Min(ltPlayerTeam.Count, byte.MaxValue); iP++) ltPlayerTeam[iP].plGame.iNrNat = (byte)(iP + 1);
        }

        List<string[]> ltLV = CkAppShared.ckMng.ui.listTeam(ltPlayerTeam, clb, false, iGameType, nPlStart: 0, iPosFilter: iPos, staff: staff, iMoneyTotal: 1000);

        foreach (CornerkickManager.Player plCon in ltPlayerTeam) {
          if (plCon.contract == null) continue;
          if (bNextSeason && plCon.contract.iLength == 1) continue;

          // Filter player by position
          if (iPos > 0 && !plCon.plGame.checkMainPos(iPos)) continue;

          int iContractLength = plCon.contract.iLength;
          if (bNextSeason) iContractLength--;

          float fHappyWithContract = bJouth2 ? 0f : CornerkickManager.PlayerTool.getHappyWithContractFactor(plCon, CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd, iGamesPerSeason: iGamesPerSeason, user: _usr);

          // Player negotiation mood
          CornerkickManager.Transfer.Offer offer = CkAppShared.ckMng.tr.getOffer(plCon, plCon.contract.club);
          float fNeg = 0f;
          if (!bJouth2 && !plCon.bRetire && (fHappyWithContract < 1.01f || plCon.contract.iLength < 2)) {
            fNeg = 1f;
            if (offer != null) fNeg = Math.Max((offer.contract.fMood - 0.1f) / 0.9f, 0f);
          }

          // User negotiation skill for fast negotiations
          float fNegUserSkill = -1;
          if (!bJouth2 && !bNextSeason && fNeg > 0f && iContractLength < CkAppShared.iContractLengthMax) {
            fNegUserSkill = 1f;
            if (_usr.iSkillNegotiation > 0) fNegUserSkill = 1f - ((0.05f * _usr.iSkillNegotiation) * (1f - (0.05f * (_usr.iSkillNegotiation - 1))));
          }

          float[]? fSkills = null;
          if (staff != null) fSkills = staff.getScoutedSkills(plCon.plGame);

          ltTable.Add(new ContractsModel.TableEntry {
            iId = plCon.plGame.iId,
            iNb = bJouth2 ? 0 : plCon.plGame.iNr,
            sName = plCon.plGame.sName,
            sPosition = CornerkickManager.PlayerTool.getStrPos(plCon),
            fSkill = CornerkickGame.Tool.getAveSkill(plCon.plGame, fSkills: fSkills),
            iValue = plCon.getValue(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd, iMoneyTotal: 1000) * 1000,
            iSalary = plCon.contract.iSalary,
            iLength = iContractLength,
            fHappy = fHappyWithContract,
            fNeg = fNeg,
            fNegUserSkill = fNegUserSkill,
            sNat = CornerkickManager.Main.sLandShort[plCon.iNat1],
            iAge = (int)plCon.plGame.getAge(CkAppShared.ckMng.dtDatum),
            fTalent = plCon.getTalentAve() + 1f,
            fSkillIdeal = CornerkickGame.Tool.getAveSkill(plCon.plGame, bIdeal: true, fSkills: fSkills),
            iBonusPlay = plCon.contract.iPlay,
            iBonusGoal = plCon.contract.iGoal,
            iFixTransferFee = plCon.contract.iFixTransferFee,
            bRetire = plCon.bRetire,
            bJouth = bJouth2
          });
        }

        bJouth2 = true;
      }

      if (bNextSeason) {
        //DateTime dtSeasonStartNext = App.ckMng.dtSeasonStart.AddYears(1);

        foreach (CornerkickManager.Player plNext in CkAppShared.ckMng.ltPlayer) {
          if (plNext.contractNext != null) {
            if (plNext.contractNext.club.iId == clb.iId) {
              // Filter player by position
              if (iPos > 0 && !plNext.plGame.checkMainPos(iPos)) continue;

              float[]? fSkills = _usr.bScouting ? clb.staff.getScoutedSkills(plNext.plGame) : null;

              ltTable.Add(new ContractsModel.TableEntry {
                iId = plNext.plGame.iId,
                iNb = 0,
                sName = plNext.plGame.sName + " *",
                sPosition = CornerkickManager.PlayerTool.getStrPos(plNext),
                fSkill = CornerkickGame.Tool.getAveSkill(plNext.plGame, fSkills: fSkills),
                iValue = plNext.getValue(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd) * 1000,
                iSalary = plNext.contractNext.iSalary,
                iLength = plNext.contractNext.iLength,
                sNat = CornerkickManager.Main.sLandShort[plNext.iNat1],
                iAge = (int)plNext.plGame.getAge(CkAppShared.ckMng.dtDatum),
                fTalent = plNext.getTalentAve() + 1f,
                fSkillIdeal = CornerkickGame.Tool.getAveSkill(plNext.plGame, bIdeal: true, fSkills: fSkills),
                iBonusPlay = plNext.contractNext.iPlay,
                iBonusGoal = plNext.contractNext.iGoal,
                iFixTransferFee = plNext.contractNext.iFixTransferFee,
                bJouth = bJouth
              });
            }
          }
        }
      }

      return ltTable;
    }

    public static bool extendPlayerContractFast(CornerkickManager.User? _usr, CornerkickManager.Player pl, int iYears, float fMoodTarget = 1f)
    {
      if (_usr == null) return false;
      if (pl == null) return false;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return false;
      if (iYears < 0) return false;
      if (iYears + pl.contract.iLength > CkAppShared.iContractLengthMax) return false;

      int iGamesPerSeason = 0;
      CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, clb.iLand, clb.iDivision);
      if (league != null) iGamesPerSeason = league.getMatchdays(clb);

      CornerkickManager.Player.Contract contractReq = CornerkickManager.PlayerTool.getContract(pl, (byte)iYears, clb, CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd, iFixedFeeOffer: 0, iGamesPerSeason: iGamesPerSeason); // Get requested contract from player

      contractReq.iLength = (byte)((pl.contract != null ? pl.contract.iLength : 0) + iYears);
      contractReq.iSalary = (int)(contractReq.iSalary * fMoodTarget);
      contractReq.iPlay   = (int)(contractReq.iPlay   * fMoodTarget);
      contractReq.iPoint  = (int)(contractReq.iPoint  * fMoodTarget);
      contractReq.iGoal   = (int)(contractReq.iGoal   * fMoodTarget);

      pl.contract = contractReq;

      // Remove hidden entry from transfer list
      CkAppShared.ckMng.tr.removePlayerFromTransferlist(pl);

      return true;
    }
  }
}
