using CornerkickApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CornerkickApp.Controllers
{
  public class AddUser
  {
#if _WebApp
    public static bool addUser(string sFirstName, string sName, CornerkickManager.Club clb, int iLevel, bool bScouting, int iStartMode, bool bUserCarrierMode, string sUserId = "", string sUserPw = "")
#else
    public static bool addUser(string sFirstName, string sName, CornerkickManager.Club clb, int iLevel, bool bScouting, int iStartMode, bool bUserCarrierMode, int nUser, string sUserId = "", string sUserPw = "")
#endif
    {
      try {
        // Add user to game
        CornerkickManager.User? usr = addUserToCk(sFirstName, sName, sId: sUserId, clubExist: clb, bUserCarrierMode, iLevel: (byte)iLevel, bScouting: bScouting);
        if (usr == null) return false;

        // Add password
        usr.sPw = sUserPw;

        // Clear sponsors
        usr.club.sponsorMain = new CornerkickManager.Finance.Sponsor();
        usr.club.ltSponsorBoards.Clear();
        if (!bUserCarrierMode) usr.club.ltSponsorOffers.Add(App.createDefaultSponsor(iLevel, true, 1));

        // Clear staff
        /*
        byte iStaffLevel = (byte)Math.Max(4 - usr.club.iDivision, 0);
        usr.club.staff.iCoTrainer = iStaffLevel;
        usr.club.staff.iCondiTrainer = iStaffLevel;
        usr.club.staff.iMentalTrainer = iStaffLevel;
        usr.club.staff.iPhysio = iStaffLevel;
        usr.club.staff.iJouthTrainer = iStaffLevel;
        usr.club.staff.iJouthScouting = iStaffLevel;
        usr.club.staff.ltDoctor = null;
        usr.club.staff.iKibitzer = 0;
        */

        // Clear captain
        usr.club.iCaptainId = [-1, -1, -1];

        // Set addmission price to default
        usr.club.iAdmissionPrice = CornerkickManager.Stadium.iSpectatorPriceDefault;

        // Clear record games
        usr.club.ltGameRecord.Clear();

        // Clear position last season (for new sponsors)
        usr.club.iPosLastSeason = 0;

        // Clear successes
        usr.club.ltSuccess.Clear();

        // Set nb of grounds to 1
        usr.club.buildings.iGround = 1;

        // Reset club if start mode = 0
        if (iStartMode == 0) {
          // Reset stadium
          string sStadiumName = usr.club.stadium.sName;

          // Set default stadium
          //usr.club.stadium = Controllers.App.getDefaultStadium();
          usr.club.stadium = new CornerkickGame.Stadium();
          usr.club.stadium.sName = sStadiumName;

          // Buildings
          usr.club.buildings = new CornerkickManager.Club.Buildings();
          usr.club.buildings.bgTrainingCourts = new CornerkickManager.Club.Buildings.Building();
        }

        // Set number of grounds
        usr.club.buildings.iGround = (byte)CornerkickManager.Stadium.getRequiredGrounds(usr.club);

#if !_WebApp
        if (CkAppShared.iUserActive + 1 >= nUser) { // Start the game
          // Start new season
          CkAppShared.ckMng.setNewSeason();

          // Check player
          foreach (CornerkickManager.Player pl in CkAppShared.ckMng.ltPlayer) {
            if (pl.contract != null) {
              if (pl.plGame.getAge(CkAppShared.ckMng.dtDatum) < 16) {
                // Move young player in pro team to jouth team
                if (CornerkickManager.PlayerTool.ownPlayer(pl.contract.club, pl, 1)) {
                  pl.contract.club.ltPlayer.Remove(pl);
                  pl.contract.club.ltPlayerJouth.Add(pl);
                }
              } else {
                // Get league and nat. cup to calc nb. of games / season
                int iGamesPerSeason = 0;
                CornerkickManager.Cup? league = CkAppShared.ckMng.ltCups.Find(c => c.iId == CkAppShared.iCupIdLeague && c.iId2 == pl.contract.club.iLand && c.iId3 == pl.contract.club.iDivision);
                if (league != null) iGamesPerSeason += league.getMatchdaysTotal();
                CornerkickManager.Cup? cupNat = CkAppShared.ckMng.ltCups.Find(c => c.iId == CkAppShared.iCupIdNatCup && c.iId2 == pl.contract.club.iLand);
                if (cupNat != null) iGamesPerSeason += 1;

                // Reset player contracts
                pl.contract = CornerkickManager.PlayerTool.getContract(pl, pl.contract.iLength, pl.contract.club, CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd, iGamesPerSeason: iGamesPerSeason, bForceNewContract: true);
                pl.contract.iSalary = Tool.roundInt((int)(pl.contract.iSalary * (0.7 + CkAppShared.random.NextDouble() * 0.4)), 3);
              }
            }
          }

          // Set default game-speed
          foreach (CornerkickManager.User u in CkAppShared.ckMng.ltUser) {
            if (u.club.nextGame == null) u.club.nextGame = new CornerkickGame.Game.Data();
            u.club.nextGame.iGameSpeed = CkAppShared.iOptionGameSpeedFast;
          }

          // Book initial training camps
          foreach (CornerkickManager.User u in CkAppShared.ckMng.ltUser) {
            if (u.iResp < CkAppShared.iUserRespTrainingCamps) Controllers.Member.CalendarController.bookTrainingscamp(u, CkAppShared.ckMng.dtSeasonStart.AddDays(6), CkAppShared.ckMng.dtSeasonStart.AddDays(13), 1);
          }

          // Initialize tutorial and end-of-contract-info array
          Member.TutorialController.initialiteTutorial();
          Member.MemberController.bHideEocInfo = new bool[CkAppShared.ckMng.ltUser.Count];

          CkAppShared.iUserActive = 0;

          Tool.setCssStringClubColors(usr.club.cl1);
        } else { // Add another user
          CkAppShared.iUserActive++;

          return false;
        }
#endif
      } catch (Exception e) {
        string sErrorMsg = "ERROR starting new game. Message: " + e.Message + Environment.NewLine + e.StackTrace;
        CkAppShared.ckMng.tl.writeLog(sErrorMsg);
        //writeLog(Path.Combine(App.DocumentsDir, "ckapp.log"), sErrorMsg);

        return false;
      }

      return true;
    }

    // Cornerkick section
    public static CornerkickManager.User? addUserToCk(string sFirstName, string sSurname, string sId = "", CornerkickManager.Club? clubExist = null, bool bUserCarrierMode = true, byte iLevel = 1, bool bScouting = false)
    {
      try {
        //CornerkickManager.Club? clubExist = CkAppShared.ckMng.ltClubs.Find(c => c.iId == iClubIdExist);

        if (clubExist == null) {
          CkAppShared.ckMng.tl.writeLog(Path.Combine(App.getDocumentsDir, "ckapp.err"), "ERROR: Cannot find club");
          return null;
        }

        CornerkickManager.Cup? league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, clubExist.iLand, clubExist.iDivision);
        if (league == null) {
          CkAppShared.ckMng.tl.writeLog(Path.Combine(App.getDocumentsDir, "ckapp.err"), "ERROR: Cannot get league");
          return null;
        }

        CornerkickManager.User usr = new CornerkickManager.User();
        usr.sFirstname = sFirstName.Trim();
        usr.sSurname = sSurname.Trim();
        if (string.IsNullOrEmpty(sId)) usr.id = CkAppShared.ckMng.ltUser.Count.ToString();
        else                           usr.id = sId;

        usr.iResp = (byte)(bUserCarrierMode ? 0 : CornerkickManager.Names.sUserRespLvlNames.Length - 1);
        usr.iLevel = iLevel;

        usr.bScouting = bScouting;
        if (usr.bScouting) {
          usr.scout = new CornerkickManager.Main.Staff.Scout();
          usr.scout.iId = -9;
          usr.fSkillPointsFree = 3f;
        } else {
          usr.fSkillPointsFree = 2f;
        }

        // Set budget plan based on club division
        usr.budget.iInSpec = 10000000;
        usr.budget.iInBonusSponsor = 10000000;

        // Add defaults
        usr.lti = [.. UserOptionsModel.iUserOptionsDefaults];

        /*
        // Do not show tutorial if resp level > 0
        if (iRespLvlStart > 0) usr.lti[UserOptionsModel.iUserOptionsIxTutorialShow] = 0;
        */

#if DEBUG
        int nUser = 1;
        for (byte iU = 0; iU < nUser; iU++) {
#endif
          CornerkickManager.Club clb = clubExist;

          try {
            // Set club account to 10 mio.
            clb.iBalance = 10000000;
            clb.iBalanceSecret = 0;
          } catch (Exception e) {
            CkAppShared.ckMng.tl.writeLog(Path.Combine(Controllers.App.getDocumentsDir, "ckapp.err"), "ERROR: Cannot set club balance. Message: " + e.Message + Environment.NewLine + e.StackTrace);
          }

          usr.club = clb;
          try {
            usr.club.nextGame = CkAppShared.ckMng.tl.getNextGame(usr.club, CkAppShared.ckMng.dtDatum);

            if (usr.club.nextGame == null) CkAppShared.ckMng.tl.writeLog(Path.Combine(Controllers.App.getDocumentsDir, "ckapp.err"), "ERROR: nextGame = null!");
            else usr.club.nextGame.iGameSpeed = CkAppShared.iOptionGameSpeedFast;
          } catch (Exception e) {
            CkAppShared.ckMng.tl.writeLog(Path.Combine(Controllers.App.getDocumentsDir, "ckapp.err"), "ERROR: Cannot set nextGame. Message: " + e.Message + Environment.NewLine + e.StackTrace);
          }
          usr.dtStart = CkAppShared.ckMng.dtDatum;
          usr.dtClubStart = CkAppShared.ckMng.dtDatum;

#if DEBUG
          if (iU == 0) {
#endif
            clb.user = usr;

            // Add scouts
            if (usr.scout != null) clb.staff.ltScouts.Add(usr.scout);
            if (usr.bScouting) {
              Controllers.App.addFreelancerScouts(usr);
            }

            // Calc total tv bonus for budget plan
            long iTvBonusTotal = 0;
            foreach (CornerkickManager.Cup c in CkAppShared.ckMng.ltCups) {
              if (c.checkClubInCup(clb)) iTvBonusTotal += c.settings.iTvBonus * Math.Max(c.getMatchdaysGroup(), 1);
            }
            usr.budget.iInTvBonus = iTvBonusTotal;

            try {
              CkAppShared.ckMng.ltUser.Add(usr);
            } catch (Exception e) {
              CkAppShared.ckMng.tl.writeLog(Path.Combine(Controllers.App.getDocumentsDir, "ckapp.err"), "ERROR: Cannot add user to list. Message: " + e.Message + Environment.NewLine + e.StackTrace);
            }

            try {
              Controllers.Member.TutorialController.initialiteTutorial();
            } catch (Exception e) {
              CkAppShared.ckMng.tl.writeLog(Path.Combine(Controllers.App.getDocumentsDir, "ckapp.err"), "ERROR: Cannot initialite tutorial. Message: " + e.Message + Environment.NewLine + e.StackTrace);
            }
#if DEBUG
          }
#endif

#if DEBUG
          if (iU == 0) {
#endif
            string sWelcomeMsg = usr.sFirstname + " " + usr.sSurname + ", herzlich Willkommen bei Ihrem neuen Verein " + clb.sName + "!";
            CkAppShared.ckMng.sendNews(usr, sWelcomeMsg, 3, usr.club.iId);
            string sWelcomeMsg2 = "Schauen Sie sich die Anleitung um mehr über die Funktionsweise von Cornerkick zu erfahren.";
            CkAppShared.ckMng.sendNews(usr, sWelcomeMsg2, 3, usr.club.iId);

            // Create newspaper
            string sNewspaper = "Herzlich Willkommen!#<b>" + usr.sFirstname + " " + usr.sSurname + "</b> steigt als neuer Manager bei <b>" + clb.sName + "</b> ein. ";
            sNewspaper += "Aktuell befindet sich der Verein in der " + league.sName + ". ";
            sNewspaper += "In Fach&shy;kreisen werden dem Verein unter der neuen Leitung große Ambitionen nachgesagt...";
            CkAppShared.ckMng.sendNews(CkAppShared.ckMng.ltUser[0], sNewspaper, 203);
#if DEBUG
          }
        }
#endif

        /*
        if (!timerCkCalender.Enabled && iClubExist < 0) {
          CkAppShared.ckMng.calcMatchdays();
          CkAppShared.ckMng.drawCup(league);
        }
        */

        return usr;
      } catch (Exception e) {
        CkAppShared.ckMng.tl.writeLog(Path.Combine(Controllers.App.getDocumentsDir, "ckapp.log"), "ERROR in addUserToCk. Message: " + e.Message + Environment.NewLine + e.StackTrace);
      }

      return null;
    }
  }
}
