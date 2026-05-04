using CornerkickApp.Shared.Models;
using static CornerkickApp.Shared.Models.CkAppShared;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  public class TeamController
  {
    public static TeamModel Team(CornerkickManager.User _usr)
    {
      TeamModel mdTeam = new TeamModel();
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);

      // System
      mdTeam.sliSystem = new List<SelectListItem>();

      // Formationen
      mdTeam.ltsFormations = new List<SelectListItem>();
      mdTeam.ltsFormationsOwn = new List<SelectListItem>();

      if (clb == null) return mdTeam;

      //mdTeam.club = clb;
      mdTeam.sliSystem = MemberController.getSliTacticSystem(clb);
      mdTeam.iSystem = clb.iTactic;
      mdTeam.iTcOrient = (int)(clb.ltTactic[clb.iTactic].fOrientation * 100f);

      mdTeam.sCl = new string[clb.cl1.Length];
      for (int i = 0; i < clb.cl1.Length; i++) mdTeam.sCl[i] = Tool.convertToRgb(clb.cl1[i]);

      // Tutorial
      if (ttUser != null) {
        int iUserIx = ckMng.ltUser.IndexOf(_usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) mdTeam.tutorial = ttUser[iUserIx];
      }

      //mdTeam.bAdmin = Models.AdminModel.checkUserIsAdmin(User);
      mdTeam.bGame = MemberController.checkUserGame2(_usr);

      //clb.nextGame = ckMng.tl.getNextGame(clb, ckMng.dtDatum);
      if (mdTeam.bGame) {
        mdTeam.nPlStart = _usr.game.data.nPlStart;
        mdTeam.nPlRes   = _usr.game.data.nPlRes;
      } else if (clb.nextGame != null) {
        mdTeam.nPlStart = clb.nextGame.nPlStart;
        mdTeam.nPlRes   = clb.nextGame.nPlRes;

        // Set default radius of action if not set
        for (int i = 0; i < clb.nextGame.nPlStart; i++) {
          if (i >= clb.ltPlayer.Count) break;
          if (clb.ltPlayer[i]?.plGame == null) continue;

          /*
          if (clb.ltPlayer[i].plGame.fRadOfAction[0] < 0.01f || clb.ltPlayer[i].plGame.fRadOfAction[1] < 0.01f) {
            clb.ltPlayer[i].plGame.fRadOfAction = CornerkickGame.Tool.getPlayerRadiusOfActionDefault(CornerkickGame.Tool.getPosRole(clb.ltTactic[clb.iTactic].formation.positions[i].pt, CornerkickGame.Field.ConvertPitch(clb.nextGame.fPitchSizeRel)));
          }
          */
          if (clb.ltTactic[clb.iTactic].formation.positions[i].fRadOfAction[0] < 0.01f || clb.ltTactic[clb.iTactic].formation.positions[i].fRadOfAction[1] < 0.01f) {
            clb.ltTactic[clb.iTactic].formation.positions[i].fRadOfAction = CornerkickGame.Tool.getPlayerRadiusOfActionDefault(CornerkickGame.Tool.getPosRole(clb.ltTactic[clb.iTactic].formation.positions[i].pt, CornerkickGame.Field.ConvertPitch(clb.nextGame.fPitchSizeRel)));
          }
        }
      }

      //mdTeam.ltPlayer = setModelLtPlayer(_usr);

      mdTeam.ltsFormations.Add(new SelectListItem { Text = "0 - Eigene", Value = "0" });
      foreach (CornerkickGame.Tactic.Formation frm in ckMng.ltFormationen) {
        string sFrmName = frm.iId.ToString() + " - " + frm.sName;
        int nFrmName = ckMng.ltFormationen.FindAll(f => f.sName.Contains(frm.sName)).Count;
        if (nFrmName > 1) {
          int mFrmName = mdTeam.ltsFormations.FindAll(f => f.Text.Contains(frm.sName)).Count;
          sFrmName += " " + Convert.ToChar(mFrmName + 65).ToString();
        }

        mdTeam.ltsFormations.Add(new SelectListItem {
          Text = sFrmName,
          Value = frm.iId.ToString()
        });
      }

      if (_usr != null && _usr.ltFormations != null) {
        for (int i = 0; i < _usr.ltFormations.Count; i++) {
          // Reset id of user formations
          CornerkickGame.Tactic.Formation frmUser = _usr.ltFormations[i];
          frmUser.iId = ckMng.ltFormationen.Count + i + 1;
          _usr.ltFormations[i] = frmUser;

          mdTeam.ltsFormations.Add(new SelectListItem {
            Text = (ckMng.ltFormationen.Count + i + 1).ToString() + " - " + frmUser.sName,
            Value = (ckMng.ltFormationen.Count + i + 1).ToString()
          });
        }
      }

      mdTeam.iFormation = clb.ltTactic[clb.iTactic].formation.iId;

      return mdTeam;
    }

    public static Task<TeamModel.TableTeam[]> getTableTeam(CornerkickManager.User usr, int iPlayerMax, int iSelectedPlayer = 0, int iTactic = 0)
    {
      /*
      List<CornerkickGame.Player> ltSpieler = new List<CornerkickGame.Player>();
      foreach (int iSp in AccountController.ckClub.ltPlayer) {
        ltSpieler.Add(ckMng.ltPlayer[iSp]);
      }
      */
      CornerkickManager.Club? club = MemberController.ckClub(usr);
      if (club?.ltPlayer == null) return Task.FromResult(new TeamModel.TableTeam[0]);

      int iGameType = 0;
      int nPlStart = 0;
      int nPlRes   = 0;
      System.Drawing.Point ptPitch = new System.Drawing.Point(0, 0);
      int iGameMin = 90;
      if (club.nextGame != null) {
        iGameType = club.nextGame.iGameType;
        nPlStart  = club.nextGame.nPlStart;
        nPlRes    = club.nextGame.nPlRes;
        ptPitch   = CornerkickGame.Field.ConvertPitch(club.nextGame.fPitchSizeRel);
        iGameMin  = club.nextGame.iGameMinutes;
      }

      // Create copy of player list
      List<CornerkickManager.Player?> ltPlayerTeam = [.. club.ltPlayer];

      // Update player numbers if nation
      if (club.bNation) {
        for (byte iP = 0; iP < Math.Min(ltPlayerTeam.Count, byte.MaxValue); iP++) {
          if (ltPlayerTeam[iP] != null) ltPlayerTeam[iP].plGame.iNrNat = (byte)(iP + 1);
        }
      }

      bool bGame = MemberController.checkUserGame2(usr);
      if (bGame) {
        iGameType = usr.game.data.iGameType;
        nPlStart  = usr.game.data.nPlStart;
        nPlRes    = usr.game.data.nPlRes;
        ptPitch   = usr.game.ptPitch;
        iGameMin  = usr.game.data.iGameMinutes + (usr.game.data.bOvertime ? usr.game.data.iGameMinutesOvertime : 0);

        if (usr.game.data.ltState.Count > 0) {
          byte iHA = 0;
          if (club.iId == usr.game.data.team[1].iTeamId) iHA = 1;

          ltPlayerTeam.Clear();
          foreach (CornerkickGame.Player plG in usr.game.player[iHA]) {
            if (plG == null) continue;
            ltPlayerTeam.Add(club.ltPlayer.Find(p => p.plGame.iId == plG.iId));
          }
        }
      }

      // Get staff for scouting
      CornerkickManager.Main.Staff? staff = usr.bScouting ? ClubController.getClubStaff(club) : null;

      List<string[]> ltLV = ckMng.ui.listTeam(ltPlayerTeam, club, bGame, iGameType, iPlayerMax: iPlayerMax, nPlStart: nPlStart, nPlRes: nPlRes, staff: staff, iMoneyTotal: 1000);

      //The table or entity I'm querying
      TeamModel.TableTeam[] tblTeam = new TeamModel.TableTeam[ltLV.Count];

      for (int i = 0; i < tblTeam.Length; i++) {
        if (ltPlayerTeam == null) continue;
        if (i >= ltPlayerTeam.Count) break;
        //if (ltPlayerTeam[i] == null) continue;

        CornerkickGame.Player? plGame = ltPlayerTeam[i]?.plGame;
        if (plGame == null) continue;

        int iSusp = 0;
        int.TryParse(ltLV[i][19], out iSusp);

        string sName = ltLV[i][2];
        int iId = -1;
        int.TryParse(ltLV[i][0], out iId);

        int iNr = -1;
        int.TryParse(ltLV[i][1], out iNr);

        if (i == ckMng.plt.getCaptainIx(club)) sName += " (C)";

        int iNat = int.Parse(ltLV[i][13]);
        string sNat = iNat < CornerkickManager.Main.sLandShort.Length ? CornerkickManager.Main.sLandShort[iNat] : "";

        int iVal = 0;
        int.TryParse(ltLV[i][10].Replace(".", ""), out iVal);
        int iSal = 0;
        int.TryParse(ltLV[i][11].Replace(".", ""), out iSal);

        /*
        float fGrade = -1f;
        float.TryParse(ltLV[i][22], out fGrade);
        */
        byte iPos = 0;
        if (i < nPlStart && club != null && club.ltTactic[club.iTactic].formation.positions.Length > i) iPos = CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(club.ltTactic[club.iTactic].formation.positions[i].pt, ptPitch));
        float fGrade = plGame.getGrade(iPos, iGameMinutes: iGameMin);

        // Correct skill if player is selected
        if (iSelectedPlayer > 0) {
          if (iSelectedPlayer < club.ltTactic[iTactic].formation.positions.Length + 1) {
            byte iPosPlSel = CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(club.ltTactic[iTactic].formation.positions[iSelectedPlayer - 1].pt, ckMng.game.ptPitch));
            float[]? fSkills = staff != null ? staff.getScoutedSkills(plGame) : null;
            ltLV[i][5] = CornerkickGame.Tool.getAveSkill(plGame, iPosPlSel, fSkills: fSkills).ToString("0.0");
          } else if (i < club.ltTactic[iTactic].formation.positions.Length && iSelectedPlayer < tblTeam.Length + 1) {
            byte iPosPlSel = CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(club.ltTactic[iTactic].formation.positions[i].pt, ckMng.game.ptPitch));
            float[]? fSkills = staff != null ? staff.getScoutedSkills(ltPlayerTeam[iSelectedPlayer - 1].plGame) : null;
            ltLV[i][5] = CornerkickGame.Tool.getAveSkill(ltPlayerTeam[iSelectedPlayer - 1].plGame, iPosPlSel, fSkills: fSkills).ToString("0.0");
          }
        }

        float[] fRadOfAction = new float[2];
        if (i < club.ltTactic[iTactic].formation.positions.Length) {
          //fRadOfAction = ltPlayerTeam[i].plGame.fRadOfAction;
          fRadOfAction = club.ltTactic[iTactic].formation.positions[i].fRadOfAction;
          if (fRadOfAction[0] < 0f || fRadOfAction[1] < 0f) fRadOfAction = CornerkickGame.Tool.getPlayerRadiusOfActionDefault(CornerkickGame.Tool.getPosRole(club.ltTactic[iTactic].formation.positions[i].pt, ckMng.game.ptPitch));
        }

        //Hard coded data here that I want to replace with query results
        tblTeam[i] = new TeamModel.TableTeam { iIndex = i + 1, iId = iId, iNr = iNr, sNull = "", sName = sName, sPosition = ltLV[i][3], sHp = ltLV[i][4], fSkill = float.Parse(ltLV[i][5]), fCondi = ltPlayerTeam[i].plGame.fCondition, fFresh = ltPlayerTeam[i].plGame.fFresh, fMoral = ltPlayerTeam[i].plGame.fMoral, fExperience = ltPlayerTeam[i].plGame.fExperience, iMarktwert = iVal, iGehalt = iSal, iLz = int.Parse(ltLV[i][12]), sNat = sNat, sForm = ltLV[i][14], fAge = float.Parse(ltLV[i][15]), fTalent = float.Parse(ltLV[i][16]), bSubstituted = ltLV[i][17] == "ausg", fLeader = ltPlayerTeam[i].plGame.character.fLeader, fSkillIdeal = float.Parse(ltLV[i][18]), iSuspended = iSusp, sCaptain = ltLV[i][21], fGrade = fGrade, bAtNationalTeam = ltLV[i][1].StartsWith("-"), fRadOfAction = fRadOfAction };
      }

      return Task.FromResult(tblTeam);
    }

    public static void UpdateRow(CornerkickManager.User usr, ref TeamModel model, int fromPosition, int toPosition)
    {
      if (fromPosition < 1 || toPosition < 1) return;

      CornerkickManager.Club? clb = MemberController.ckClub(usr);

      if (clb == null) return;

      CornerkickManager.Player pl = clb.ltPlayer[fromPosition - 1];
      if (!pl.plGame.bPlayed) {
        clb.ltPlayer.RemoveAt(fromPosition - 1);
        clb.ltPlayer.Insert(toPosition - 1, pl);

        //ltPlayer = setModelLtPlayer(usr, model);

        getTeamData(usr, model, clb.ltTactic[clb.iTactic].formation.iId);
      }
    }

    public static bool SwitchPlayerByIndex(CornerkickManager.User usr, TeamModel model, int iIndex1, int iIndex2, int nToBeSubstituted = 0)
    {
      if (iIndex1 < 0 || iIndex2 < 0) return false;

      CornerkickManager.Club? club = MemberController.ckClub(usr);
      if (club == null) return false;

      int jPosMin = Math.Min(iIndex1, iIndex2);
      int jPosMax = Math.Max(iIndex1, iIndex2);

      if (MemberController.checkUserGame2(usr)) {
        byte iHA = 0;
        if (club.iId == usr.game.data.team[1].iTeamId) iHA = 1;

        // If switch of player in starting 11 --> do it directly
        if (jPosMin < usr.game.data.nPlStart && jPosMax < usr.game.data.nPlStart) {
          usr.game.doSubstitution(iHA, (byte)jPosMin, (byte)jPosMax);
        } else {
          // Return if ...
          if (usr.game.player[iHA][jPosMax].bPlayed)                    return false; // ... player in has already played
          if (usr.game.iSubstitutionsLeft[iHA] - nToBeSubstituted <= 0) return false; // ... no subs left
          if (usr.game.data.iGameType > 0 &&
              usr.game.data.iGameType <= usr.game.player[iHA][jPosMin].iSuspension.Length &&
              usr.game.player[iHA][jPosMin].iSuspension[usr.game.data.iGameType - 1] > 0) return false; // ... player out is suspended

          CornerkickManager.Player pl = CornerkickManager.PlayerTool.getPlayerFromId(usr.game.player[iHA][jPosMax].iId, club.ltPlayer);
          if (!club.bNation && CornerkickManager.PlayerTool.atNationalTeam(pl, ckMng.ltClubs)) return false; // ... player in is at national team

          if (model.ltiSubstitution == null) {
            model.ltiSubstitution = new List<int[]>();
          }
          model.ltiSubstitution.Add([jPosMin, jPosMax, 0]);
        }
      }

      // Switch player in club list
      CornerkickManager.Player pl1 = club.ltPlayer[jPosMin];
      CornerkickManager.Player pl2 = club.ltPlayer[jPosMax];

      club.ltPlayer.Remove(pl1);
      club.ltPlayer.Remove(pl2);

      club.ltPlayer.Insert(jPosMin, pl2);
      club.ltPlayer.Insert(jPosMax, pl1);

      model.ltPlayer = setModelLtPlayer(usr, model);

      return true;
    }

    public static bool SwitchPlayerByID(CornerkickManager.User usr, TeamModel model, int iId1, int iId2, int nToBeSubstituted = 0)
    {
      if (iId1 < 0) return false;
      if (iId2 < 0) return false;

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return false;

      int iIndex1 = -1;
      int iIndex2 = -1;
      for (int iIx = 0; iIx < clb.ltPlayer.Count; iIx++) {
        if      (clb.ltPlayer[iIx].plGame.iId == iId1) iIndex1 = iIx;
        else if (clb.ltPlayer[iIx].plGame.iId == iId2) iIndex2 = iIx;

        if (iIndex1 >= 0 && iIndex2 >= 0) break;
      }

      return SwitchPlayerByIndex(usr, model, iIndex1, iIndex2, nToBeSubstituted: nToBeSubstituted);
    }

    public static bool SwitchPlayer(CornerkickManager.User usr, TeamModel model, CornerkickManager.Player pl1, CornerkickManager.Player pl2, int nToBeSubstituted = 0)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return false;

      int iIndex1 = clb.ltPlayer.IndexOf(pl1);
      int iIndex2 = clb.ltPlayer.IndexOf(pl2);

      return SwitchPlayerByIndex(usr, model, iIndex1, iIndex2, nToBeSubstituted: nToBeSubstituted);
    }

    public static void SetSubstitutions(CornerkickManager.User usr, TeamModel model)
    {
      // Check if game running
      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          if (model.ltiSubstitution != null) {
            CornerkickManager.Club? clb = MemberController.ckClub(usr);
            if (clb == null) return;

            byte iHA = 0;
            if (clb.iId == usr.game.data.team[1].iTeamId) iHA = 1;

            foreach (int[] iSub in model.ltiSubstitution) {
              usr.game.substitute(iHA == 0, (byte)iSub[0], (byte)iSub[1], 0);
            }
            model.ltiSubstitution.Clear();
            model.ltsSubstitution = GetSubstitutionList(usr, model);
          }
        }
      }
    }

    public static void UnsetSubstitutions(CornerkickManager.User usr, TeamModel model)
    {
      if (model.ltiSubstitution != null) model.ltiSubstitution.Clear();

      // Check if game running
      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          CornerkickManager.Club? clb = MemberController.ckClub(usr);
          if (clb == null) return;

          byte iHA = 0;
          if (clb.iId == usr.game.data.team[1].iTeamId) iHA = 1;

          usr.game.ltSubstitutions[iHA].Clear();
          model.ltsSubstitution = GetSubstitutionList(usr, model);
        }
      }
    }

    public static List<string[]>? GetSubstitutionList(CornerkickManager.User usr, TeamModel model)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return null;

      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          List<string[]> ltsSubstitution = new List<string[]>();

          byte iHA = 0;
          if (clb.iId == usr.game.data.team[1].iTeamId) iHA = 1;

          model.iSubRest = (byte)Math.Max(usr.game.iSubstitutionsLeft[iHA] - usr.game.ltSubstitutions[iHA].Count, 0);

          if (usr.game.ltSubstitutions[iHA] != null) {
            foreach (byte[] iSub in usr.game.ltSubstitutions[iHA]) {
              ltsSubstitution.Add([ usr.game.player[iHA][iSub[0]].sName, usr.game.player[iHA][iSub[1]].sName, iSub[2].ToString(), "0" ]);
            }
          }

          if (model.ltiSubstitution != null) {
            foreach (int[] iSub in model.ltiSubstitution) {
              ltsSubstitution.Add([ usr.game.player[iHA][iSub[0]].sName, usr.game.player[iHA][iSub[1]].sName, iSub[2].ToString(), "1" ]);
            }
          }

          return ltsSubstitution;
          //return Json(new { ltsSubstitution = ltsSubstitution, iSubsPerf = usr.game.data.nSubstitutions - usr.game.iSubstitutionsLeft[iHA] });
        }
      }

      return null;
    }

    public static bool doAutoFormation(CornerkickManager.User usr, int iType = 0)
    {
      // Check if game running
      if (usr.game != null) {
        if (!usr.game.data.bFinished) return false;
      }

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return false;

      ckMng.doFormation(clb, iType);

      return true;
    }

    private static void setFormation(CornerkickManager.User usr, int iF, int iTactic = 0, CornerkickManager.Club? clb = null)
    {
      if (iF < 0) return;

      if (clb == null) clb = MemberController.ckClub(usr);
      if (clb == null) return;

      // Store ind. orientation
      float[]   fIndOrient   = clb.ltTactic[iTactic].formation.positions.Select(f => f.fIndOrientation).ToArray();
      float[][] fRadOfAction = clb.ltTactic[iTactic].formation.positions.Select(f => f.fRadOfAction).ToArray();

      if (iF < ckMng.ltFormationen.Count) {
        clb.ltTactic[iTactic].formation = ckMng.ltFormationen[iF].Clone();
      } else if (iF < ckMng.ltFormationen.Count + usr.ltFormations.Count) {
        clb.ltTactic[iTactic].formation = usr.ltFormations[iF - ckMng.ltFormationen.Count].Clone();
      }

      // Apply ind. orientation
      for (int i = 0; i < clb.ltTactic[iTactic].formation.positions.Length; i++) {
        clb.ltTactic[iTactic].formation.positions[i].fIndOrientation = fIndOrient[i];
        clb.ltTactic[iTactic].formation.positions[i].fRadOfAction    = fRadOfAction[i];
      }

      updatePlayerOfGame(usr.game, clb);
    }

    public static TeamModel.TeamData? getTeamData(CornerkickManager.User usr, TeamModel model, int iF, int iSP = -1, bool bMobile = false, int iTactic = 0)
    {
      if (usr == null) return null;

      TeamModel.TeamData tD = new TeamModel.TeamData();

      CornerkickManager.Club? club = MemberController.ckClub(usr);
      if (club == null) return null;

      setFormation(usr, iF, iTactic: iTactic, clb: club);

      model.ltPlayer = setModelLtPlayer(usr, model);
      model.ltsSubstitution = GetSubstitutionList(usr, model);

      tD.ltPlayer2 = new List<TeamModel.Player>();

      // Get staff for scouting
      CornerkickManager.Main.Staff? staff = usr.bScouting ? ClubController.getClubStaff(club) : null;

      //tD.formation = club.ltTactic[iTactic].formation;
      CornerkickGame.Tactic.Formation frm = club.ltTactic[iTactic].formation;

      int iSuspIx = -1;
      if (usr.game != null && !usr.game.data.bFinished) {
        iSuspIx = usr.game.data.iGameType - 1;
        tD.ptPitch = usr.game.ptPitch;
      } else if (club.nextGame != null) {
        iSuspIx = club.nextGame.iGameType - 1;
        tD.ptPitch = new System.Drawing.Point((int)(ckMng.game.ptPitch.X * club.nextGame.fPitchSizeRel), (int)(ckMng.game.ptPitch.Y * club.nextGame.fPitchSizeRel));
      }

      int iP = 0;
      foreach (CornerkickGame.Player pl in model.ltPlayer) {
        if (pl == null) continue;

        TeamModel.Player pl2 = new TeamModel.Player();

        pl2.iId = pl.iId;
        pl2.sName = pl.sName;
        pl2.iNb = pl.iNr;

        CornerkickManager.Player? plMng = club.ltPlayer.Find(p => p.plGame.iId == pl.iId);
        if (plMng == null) continue;

        pl2.sNat = CornerkickManager.Main.sLandShort[plMng.iNat1];
        //pl2.sPortrait = PlayerController.getPlayerPortrait(plMng);
        pl2.sPortrait = PlayerController.getPlayerPortraitHtmlImg(plMng, sStyle: "height: 100%; width: 100%; object-fit: contain", bSmall: true);

        // Check player card
        pl2.iCard = pl.bYellowCard ? 1 : 0;
        if (iSuspIx >= 0 && iSuspIx < pl.iSuspension.Length && pl.iSuspension[iSuspIx] > 0) pl2.iCard = pl.iSuspension[iSuspIx] > 2 ? 3 : 2;

        if (iP < frm.positions.Length) {
          pl2.ptPos = new TeamModel.Point(frm.positions[iP].pt);
          pl2.iPos = CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(frm.positions[iP].pt, tD.ptPitch));

          float[]? fSkills = staff != null ? staff.getScoutedSkills(pl) : null;
          float fSkillAve = CornerkickGame.Tool.getAveSkill(pl, CornerkickGame.Tool.getPosRole(frm.positions[iP].pt, tD.ptPitch), fSkills: fSkills);
          pl2.sSkillAve = fSkillAve > 0f ? fSkillAve.ToString("0.0") : "?";
        }

        // Individuel player tactic
        pl2.iIxManMarking = pl.iIxManMarking;
        pl2.bOffStandards = pl.bOffStandards;
        if (iP < frm.positions.Length) pl2.iTcIndOrient = (int)((frm.positions[iP].fIndOrientation < -1f ? CornerkickGame.Tool.getPlayerIndividualOrientationDefault(pl2.iPos) : frm.positions[iP].fIndOrientation) * 100);
        //pl2.fTcIndOrientMinMax = TeamGetIndOrientationMinMax(iP, club, iTactic: iTactic);

        tD.ltPlayer2.Add(pl2);

        iP++;

        if (club.nextGame != null && iP >= club.nextGame.nPlStart) break;
      }

      if (iSP >= 0) {
        tD.plSelected = model.ltPlayer[iSP];

        /*
        if (iSP < tD.formation.fIndOrientation.Length) {
          tD.fIndOrientation = tD.formation.fIndOrientation[iSP];
          if (tD.fIndOrientation < -1f) tD.fIndOrientation = CornerkickGame.Tool.getPlayerIndividualOrientationDefault(tD.ltPlayer2[iSP].iPos);
        }

        tD.sDivRoa = TeamGetPlayerRadiusOfAction(iSP, club, iTactic: iTactic);
        tD.fIndOrientationMinMax = TeamGetIndOrientationMinMax(iSP, club, iTactic: iTactic);
        */
      }

      tD.iCaptainIx = ckMng.plt.getCaptainIx(club);

      // Team averages
      float[] fTeamAve11 = CornerkickManager.Tool.getTeamAve(club, ckMng.dtDatum, ckMng.dtSeasonEnd, ptPitch: ckMng.game.ptPitch, iPlStop: 11, iTactic: (byte)iTactic, bScouting: usr.bScouting);
      tD.sTeamAveSkill = fTeamAve11[3] > 0f ? fTeamAve11[3].ToString("0.00") : "?";
      tD.sTeamAveAge = fTeamAve11[4].ToString("0.0");

      // Team strength by position (defence/midfield/foreward)
      tD.fTeamStrengthPos = new float[3];
      for (int iPl = 0; iPl < 11; iPl++) {
        if (iPl >= club.ltPlayer.Count) break;
        if (club.ltTactic[iTactic].formation.positions == null) break;
        if (club.ltTactic[iTactic].formation.positions.Length <= iPl) break;

        CornerkickManager.Player pl = club.ltPlayer[iPl];
        if (pl == null) continue;
        if (pl.plGame.injury != null && pl.plGame.injury.fLength > 1f) continue;

        byte iPos = CornerkickGame.Tool.getPosRole(club.ltTactic[iTactic].formation.positions[iPl].pt, ckMng.game.ptPitch);
        if (iPos == 1) continue;

        float[]? fSkills = staff != null ? staff.getScoutedSkills(pl.plGame) : null;

        if (CornerkickGame.Tool.getBasisPos(iPos) < 5) {
          tD.fTeamStrengthPos[0] += CornerkickGame.Tool.getAveSkill(pl.plGame, iPos: iPos, fSkills: fSkills);
        } else if (CornerkickGame.Tool.getBasisPos(iPos) < 9) {
          tD.fTeamStrengthPos[1] += CornerkickGame.Tool.getAveSkill(pl.plGame, iPos: iPos, fSkills: fSkills);
        } else {
          tD.fTeamStrengthPos[2] += CornerkickGame.Tool.getAveSkill(pl.plGame, iPos: iPos, fSkills: fSkills);
        }
      }

      tD.sEmblem = ClubController.getClubEmblemImg(club, sStyle: "width: 100%");

      tD.bNation = club.bNation;

      //////////////////////////////////////////////////////////////////////////////////////////////////
      // Opponent team
      //////////////////////////////////////////////////////////////////////////////////////////////////
      // Get current or next game data
      CornerkickGame.Game.Data? gdOpp = null;
      if (usr.game != null && !usr.game.data.bFinished) gdOpp = usr.game.data;
      else if (club.nextGame != null) gdOpp = club.nextGame;

      if (gdOpp != null) {
        if (club.bNation) tD.iKibitzer = 3;
        else              tD.iKibitzer = club.staff.iKibitzer;

        int iClubOpp = gdOpp.team[1].iTeamId;
        if (gdOpp.team[1].iTeamId == club.iId) iClubOpp = gdOpp.team[0].iTeamId;

        tD.ltPlayerOpp2 = new List<TeamModel.Player>();

        tD.bOppTeam = iClubOpp >= 0;

        if (tD.bOppTeam && tD.iKibitzer > 0) {
          CornerkickManager.Club? clubOpp = ckMng.ltClubs.Find(c => c.iId == iClubOpp);

          if (clubOpp != null) {
            //tD.formationOpp = clubOpp.ltTactic[clubOpp.iTactic].formation;
            CornerkickGame.Tactic.Formation frmOpp = clubOpp.ltTactic[clubOpp.iTactic].formation;

            List<CornerkickManager.Player> ltPlOpp = new List<CornerkickManager.Player>();
            if (MemberController.checkUserGame2(usr)) {
              byte iHA = 0;
              if (iClubOpp == usr.game.data.team[1].iTeamId) iHA = 1;

              List<CornerkickGame.Player> ltPlGameOpp = usr.game.player[iHA].ToList();
              foreach (CornerkickGame.Player plGameOpp in ltPlGameOpp) {
                if (plGameOpp == null) continue;
                ltPlOpp.Add(clubOpp.ltPlayer.Find(p => p.plGame.iId == plGameOpp.iId));
              }
            } else {
              ltPlOpp = clubOpp.ltPlayer;
            }

            for (byte iPl = 0; iPl < gdOpp.nPlStart; iPl++) {
              if (iPl >= clubOpp.ltPlayer.Count) break;

              CornerkickManager.Player plOpp = ltPlOpp[iPl];
              if (plOpp == null) continue;

              TeamModel.Player plOpp2 = new TeamModel.Player();

              plOpp2.iId = plOpp.plGame.iId;
              plOpp2.sName = plOpp.plGame.sName;
              plOpp2.iNb = plOpp.plGame.iNr;
              plOpp2.sNat = plOpp.iNat1 < CornerkickManager.Main.sLandShort.Length ? CornerkickManager.Main.sLandShort[plOpp.iNat1] : "";
              plOpp2.iCard = plOpp.plGame.bYellowCard ? 1 : 0;
              if (iSuspIx >= 0 && iSuspIx < plOpp.plGame.iSuspension.Length && plOpp.plGame.iSuspension[iSuspIx] > 0) plOpp2.iCard = plOpp.plGame.iSuspension[iSuspIx] > 2 ? 3 : 2;
              //plOpp2.sPortrait = PlayerController.getPlayerPortrait(plOpp, bSmall: true);
              plOpp2.sPortrait = PlayerController.getPlayerPortraitHtmlImg(plOpp, sStyle: "height: 100%; width: 100%; object-fit: contain", bSmall: true);

              if (frmOpp.positions.Length > iPl) {
                plOpp2.ptPos = new TeamModel.Point(frmOpp.positions[iPl].pt);
                plOpp2.iPos = CornerkickGame.Tool.getBasisPos(CornerkickGame.Tool.getPosRole(frmOpp.positions[iPl].pt, ckMng.game.ptPitch));
              }

              float[]? fSkills = staff != null ? staff.getScoutedSkills(plOpp.plGame) : null;
              float fPlOppAveSkill = CornerkickGame.Tool.getAveSkill(plOpp.plGame, 99, fSkills: fSkills);
              if (tD.iKibitzer == 3) fPlOppAveSkill = (float)Math.Round(fPlOppAveSkill * 2f) / 2f;
              plOpp2.sSkillAve = fPlOppAveSkill > 0f ? fPlOppAveSkill.ToString("0.0") : "?";

              tD.ltPlayerOpp2.Add(plOpp2);
            }

            // Opp. team averages
            float[] fTeamOppAve11 = CornerkickManager.Tool.getTeamAve(clubOpp, ckMng.dtDatum, ckMng.dtSeasonEnd, ptPitch: ckMng.game.ptPitch, iPlStop: gdOpp.nPlStart, bScouting: usr.bScouting);
            tD.sTeamOppAveSkill = fTeamOppAve11[3] > 0f ? fTeamOppAve11[3].ToString("0.00") : "?";
            tD.sTeamOppAveAge = fTeamOppAve11[4].ToString("0.0");
            tD.sEmblemOpp = ClubController.getClubEmblemImg(clubOpp, sStyle: "width: 100%");
          }
        }
      }

      return tD;
    }

    internal static List<CornerkickGame.Player> setModelLtPlayer(CornerkickManager.User user, TeamModel model)
    {
      List<CornerkickGame.Player> ltPlayer = new List<CornerkickGame.Player>();

      CornerkickManager.Club? club = MemberController.ckClub(user);
      if (club == null) return ltPlayer;

      foreach (CornerkickManager.Player pl in club.ltPlayer) {
        ltPlayer.Add(pl.plGame);
      }

      if (MemberController.checkUserGame2(user)) {
        model.ltsSubstitution = new List<string[]>();

        byte iHA = 0;
        if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

        ltPlayer = user.game.player[iHA].ToList();

        model.iSubRest = (byte)Math.Max(user.game.iSubstitutionsLeft[iHA] - user.game.ltSubstitutions[iHA].Count, 0);

        if (model.ltiSubstitution != null) {
          foreach (int[] iSub in model.ltiSubstitution) {
            model.ltsSubstitution.Add(new string[] { user.game.player[iHA][iSub[0]].sName, user.game.player[iHA][iSub[1]].sName, iSub[2].ToString() });
          }
        }
      }

      return ltPlayer;
    }

    public static bool movePlayer(CornerkickManager.User? _usr, int iPlId, int iDirection, out System.Drawing.Point ptNew, int iTactic = 0)
    {
      ptNew = new System.Drawing.Point(0, 0);

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return false;

      CornerkickManager.Player? pl = clb.ltPlayer.Find(p => p.plGame.iId == iPlId);
      if (pl == null) return false;

      int iIndexPlayer = clb.ltPlayer.IndexOf(pl);

      if (iIndexPlayer < 0 || iIndexPlayer >= clb.ltTactic[iTactic].formation.positions.Length) return false;

      clb.ltTactic[iTactic].formation.iId = 0;

      if        (iDirection == 1) {
        if (clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.X < (ckMng.game.ptPitch.X / 2) - 1) clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.X += 2;
      } else if (iDirection == 2) {
        if (clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.Y < +ckMng.game.ptPitch.Y) {
          clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.Y += 1;
          if (clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.Y % 2 == 0) clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.X += 1;
          else                                                                       clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.X -= 1;
        }
      } else if (iDirection == 3) {
        if (clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.X > 1) clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.X -= 2;
      } else if (iDirection == 4) {
        if (clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.Y > -ckMng.game.ptPitch.Y) {
          clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.Y -= 1;
          if (clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.Y % 2 == 0) clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.X += 1;
          else                                                                       clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt.X -= 1;
        }
      }

      ptNew = clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt;

      updatePlayerOfGame(_usr.game, clb);

      return true;
    }

    public static bool movePlayerTo(CornerkickManager.User _usr, int iPlayerId, int iX, int iY, int iTactic = 0)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return false;

      CornerkickManager.Player? pl = clb.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl == null) return false;

      int iIndexPlayer = clb.ltPlayer.IndexOf(pl);
      if (iIndexPlayer < 0 || iIndexPlayer >= clb.ltTactic[iTactic].formation.positions.Length) return false;

      // Snap keeper
      if (Math.Abs(iY) < 4 && iX < 4) {
        iX = 1;
        iY = 0;
      }

      if (iX < 0) return false;
      if (iX > ckMng.game.ptPitch.X / 2) return false;
      if (iY < -ckMng.game.ptPitch.Y) return false;
      if (iY > +ckMng.game.ptPitch.Y) return false;

      System.Drawing.Point ptNew = new System.Drawing.Point(iX, iY);
      CornerkickGame.Tool.correctPos(ref ptNew);

      CornerkickGame.Tactic.Formation? f = null;
      int iFrmId = clb.ltTactic[iTactic].formation.iId;
      if (iFrmId > 0) {
        f = ckMng.ltFormationen.Find(f => f.iId == iFrmId).Clone();
        if (f != null) clb.ltTactic[iTactic].formation = f.Value.Clone();
      }
      clb.ltTactic[iTactic].formation.iId = 0;
      clb.ltTactic[iTactic].formation.positions[iIndexPlayer].pt = ptNew;

      updatePlayerOfGame(_usr.game, clb);

      return true;
    }

    public static void TeamSetOffenceFlag(CornerkickManager.User usr, List<CornerkickGame.Player> ltPlayer, int iPlayerIx, bool bSet)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return;
      if (iPlayerIx < 0) return;
      if (clb.ltPlayer == null) return;
      if (iPlayerIx >= clb.ltPlayer.Count) return;
      if (ltPlayer == null) return;

      clb.ltPlayer[iPlayerIx].plGame.bOffStandards = bSet;
      ltPlayer[iPlayerIx] = clb.ltPlayer[iPlayerIx].plGame;
      updatePlayerOfGame(usr.game, clb);
    }

    // Set positions to player of current game
    public static void updatePlayerOfGame(CornerkickGame.Game game, CornerkickManager.Club clb)
    {
      if (game == null) return;
      if (clb == null) return;
      if (game.data.bFinished) return;

      byte iHA = 0;
      if (game.data.team[1].iTeamId == clb.iId) iHA = 1;

      // Update player
      for (byte iPl = 0; iPl < game.data.nPlStart; iPl++) {
        foreach (CornerkickManager.Player pl in clb.ltPlayer) {
          if (pl.plGame.iId == game.player[iHA][iPl].iId) {
            game.player[iHA][iPl] = pl.plGame;
            break;
          }
        }
      }

      // Update formation
      game.data.team[iHA].ltTactic = clb.ltTactic;
    }

#if false
    public JsonResult saveFormation(string sName, int iTactic = 0)
    {
      if (string.IsNullOrEmpty(sName)) Json(-1);

      CornerkickManager.User user = _usr;
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false);

      club.ltTactic[iTactic].formation.iId = ckMng.ltFormationen.Count + user.ltFormations.Count + 1;

      CornerkickGame.Tactic.Formation frmUser = CornerkickGame.Tactic.newFormation(11);
      frmUser.iId = club.ltTactic[iTactic].formation.iId;
      frmUser.sName = sName;
      for (int iPt = 0; iPt < club.ltTactic[iTactic].formation.ptPos.Length; iPt++) frmUser.ptPos[iPt] = club.ltTactic[iTactic].formation.ptPos[iPt];

      user.ltFormations.Add(frmUser);

      return Json(ckMng.ltFormationen.Count + user.ltFormations.Count);
    }

    public JsonResult deleteFormation(int iFormation)
    {
      CornerkickManager.User user = _usr;

      if (iFormation >= ckMng.ltFormationen.Count + 1 && iFormation < ckMng.ltFormationen.Count + user.ltFormations.Count + 1) {
        CornerkickManager.Club club = ckClub();

        user.ltFormations.RemoveAt(iFormation - ckMng.ltFormationen.Count - 1);

        CkAufstellungFormation(0);
      }

      return Json(1);
    }
#endif

    public static void setIndOrientation(CornerkickManager.User _usr, int iIndexPlayer, int iIndOrientation, int iTactic=0)
    {
      if (iIndexPlayer < 0) return;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb?.ltTactic == null) return;
      if (clb.ltTactic.Count <= iTactic) return;

      clb.ltTactic[iTactic].formation.positions[iIndexPlayer].fIndOrientation = iIndOrientation * 0.01f;

      updatePlayerOfGame(_usr.game, clb);
    }

    public static float[]? GetPlayerRadiusOfActionById(CornerkickManager.User _usr, int iPlayerId, float[] fPlayerRoa, float fRoaFrac, int iTactic = 0)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      CornerkickManager.Player? pl = clb.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl == null) return null;

      return GetPlayerRadiusOfAction(_usr, clb.ltPlayer.IndexOf(pl), fPlayerRoa, fRoaFrac, iTactic);
    }
    public static float[]? GetPlayerRadiusOfAction(CornerkickManager.User _usr, int iPlayerIx, float[] fPlayerRoa, float fRoaFrac, int iTactic = 0)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      CornerkickGame.Game.Data? gd = _usr.game != null ? _usr.game.data : clb.nextGame != null ? clb.nextGame : null;
      if (gd == null) return null;

      if (iPlayerIx < 0 || iPlayerIx >= gd.nPlStart || iPlayerIx >= clb.ltPlayer.Count) return null;

      CornerkickGame.Tactic tc = clb.ltTactic[iTactic];

      System.Drawing.Point ptPitch = CornerkickGame.Field.ConvertPitch(gd.fPitchSizeRel);

      /*
      int[] ltOffsiteX = new int[gd.nPlStart];
      for (int i = 0; i < gd.nPlStart; i++) {
        ltOffsiteX[i] = CornerkickGame.Tool.getXPosOffence(tc, i, ckMng.game.ptPitch);
      }
      Array.Sort(ltOffsiteX);
      int iXMin = ltOffsiteX[1];
      */
      int[] ltIx = new int[tc.formation.positions.Length];
      for (int i = 0; i < ltIx.Length; i++) ltIx[i] = i;
      int iXMin = CornerkickGame.Tool.getOffsiteX(false, gd.nPlStart, ltIx.Select(i => CornerkickGame.Tool.getXPosOffence(tc, i, ckMng.game.ptPitch)).ToArray(), ptPitch);

      int iXOffence = CornerkickGame.Tool.getXPosOffence(tc, iPlayerIx, ckMng.game.ptPitch);
      System.Drawing.Point ptPosOffence = new System.Drawing.Point(iXOffence, tc.formation.positions[iPlayerIx].pt.Y);
      /*
      TeamModel.Point ptRoaTL = new TeamModel.Point(CornerkickGame.Tool.getOffencePos(ptPosOffence, fPlayerRoa, 0, ptPitch, iXMin, +fRoaFrac, -fRoaFrac));
      TeamModel.Point ptRoaBR = new TeamModel.Point(CornerkickGame.Tool.getOffencePos(ptPosOffence, fPlayerRoa, 0, ptPitch, iXMin, -fRoaFrac, +fRoaFrac));

      return [
        1f - (ptRoaTL.x / (float)ptPitch.X),
        (ptPitch.Y + ptRoaTL.y) / (float)(2 * ptPitch.Y),
        Math.Abs(ptRoaBR.x - ptRoaTL.x) / (float)ptPitch.X,
        Math.Abs(ptRoaBR.y - ptRoaTL.y) / (float)(2 * ptPitch.Y),
      ];
      */
      double[] dTL = CornerkickGame.Tool.getOffencePosFrac(ptPosOffence, fPlayerRoa, 0, ptPitch, iXMin, +fRoaFrac, -fRoaFrac);
      double[] dBR = CornerkickGame.Tool.getOffencePosFrac(ptPosOffence, fPlayerRoa, 0, ptPitch, iXMin, -fRoaFrac, +fRoaFrac);

      return [
        1f - (float)dTL[0],
        (float)dTL[1] + 0.5f,
        (float)Math.Abs(dBR[0] - dTL[0]),
        (float)Math.Abs(dBR[1] - dTL[1])
      ];
    }

    public static float SetPlayerRadiusOfAction(CornerkickManager.User _usr, int iIndexPlayer, int iXY, float fChange, int iTactic = 0)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return 0f;

      /*
      CornerkickManager.Player? pl = clb.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl == null) return 0f;

      pl.plGame.fRadOfAction[iXY] += fChange;

      pl.plGame.fRadOfAction[iXY] = Math.Max(pl.plGame.fRadOfAction[iXY], 0f);
      pl.plGame.fRadOfAction[iXY] = Math.Min(pl.plGame.fRadOfAction[iXY], 1f);
      */

      clb.ltTactic[iTactic].formation.positions[iIndexPlayer].fRadOfAction[iXY] += fChange;

      clb.ltTactic[iTactic].formation.positions[iIndexPlayer].fRadOfAction[iXY] = Math.Max(clb.ltTactic[iTactic].formation.positions[iIndexPlayer].fRadOfAction[iXY], 0f);
      clb.ltTactic[iTactic].formation.positions[iIndexPlayer].fRadOfAction[iXY] = Math.Min(clb.ltTactic[iTactic].formation.positions[iIndexPlayer].fRadOfAction[iXY], 1f);

      return clb.ltTactic[iTactic].formation.positions[iIndexPlayer].fRadOfAction[iXY];
    }

    public static float[] GetIndOrientationMinMax(int iPlayerIndex, CornerkickGame.Tactic tc, float fScale = 1f)
    {
      float[] fIndOrientationMinMax = new float[2];

      if (iPlayerIndex < 0 || iPlayerIndex >= ckMng.game.data.nPlStart) return fIndOrientationMinMax;

      //CornerkickGame.Player pl = club.ltPlayer[iPlayerIndex].plGame;

      fIndOrientationMinMax[0] = CornerkickGame.Tool.getXPosOffence(tc.formation.positions[iPlayerIndex].pt.X, tc.fOrientation, -1f * fScale, ckMng.game.ptPitch.X) / (float)ckMng.game.ptPitch.X;
      fIndOrientationMinMax[1] = CornerkickGame.Tool.getXPosOffence(tc.formation.positions[iPlayerIndex].pt.X, tc.fOrientation, +1f * fScale, ckMng.game.ptPitch.X) / (float)ckMng.game.ptPitch.X;

      return fIndOrientationMinMax;
    }

    public static bool SetManMarking(CornerkickManager.User _usr, int iIxPlayer, int iIxPlayerOpp)
    {
      if (iIxPlayer < 0) return false;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return false;

      CornerkickGame.Player pl = clb.ltPlayer[iIxPlayer].plGame;

      if (pl.iIxManMarking == iIxPlayerOpp) pl.iIxManMarking = -1;
      else                                  pl.iIxManMarking = (sbyte)iIxPlayerOpp;

      updatePlayerOfGame(_usr.game, clb);

      return true;
    }

  }
}
