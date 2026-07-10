using CornerkickApp.Shared.Models;
using System;
using System.Globalization;
using static CornerkickApp.Shared.Models.CkAppShared;

namespace CornerkickApp.Controllers.Member
{
  public class DeskController
  {
    public readonly CornerkickManager.User _usr;
    public readonly MemberController _mc;

    public DeskController(CornerkickManager.User usr)
    {
      _usr = usr;
      _mc = new MemberController(usr);
    }

    public static DeskModel Desk(CornerkickManager.User? _usr)
    {
      DeskModel mdDesk = new DeskModel() {
        bUserExist = false
      };

      if (_usr == null) return mdDesk;

      MemberController _mc = new MemberController(_usr);

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return mdDesk;

      CultureInfo ci = MemberController.getCi(clb);

      mdDesk.bUserExist = true;

#if _WebApp
      if (iLoadState != 0) {
        return mdDesk;
      }
#endif

      //mdDesk.iUserRespLv = _usr.getResposibleLevel(ckMng.dtDatum);
      mdDesk.iUserRespLv = _usr.iResp;

      //if (_usr.ltNews == null) return mdDesk;
      //mdDesk.user = usr;

      if (_usr.lti != null) {
        if (_usr.lti.Count > UserOptionsModel.iUserOptionsIxDeleteLog) mdDesk.iDeleteLog = _usr.lti[UserOptionsModel.iUserOptionsIxDeleteLog];
      }

      // Show todays balance?
      mdDesk.bShowBalanceToday = true;
      if (_usr.lti != null) {
        if (_usr.lti.Count > UserOptionsModel.iUserOptionsIxShowBalance) mdDesk.bShowBalanceToday = _usr.lti[UserOptionsModel.iUserOptionsIxShowBalance] > 0;
      }

      // Assign tutorial
      if (ttUser != null) {
        int iUserIx = ckMng.ltUser.IndexOf(_usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) {
          mdDesk.tutorial = ttUser[iUserIx];

          if (mdDesk.tutorial.iLevel == 999 && ckMng.dtDatum.CompareTo(new DateTime(_usr.dtClubStart.Year, 11, 1)) > 0/* && ckMng.dtDatum.Year > usr.dtClubStart.Year*/) {
            mdDesk.tutorial.iLevel = 1000;
            mdDesk.tutorial.bShow = true;
            TutorialController.setTutorialLvl(_usr, true, mdDesk.tutorial.iLevel, null);
            //_mc.SetTutorialLevel(true, 1000);
          }
        }
      }

      mdDesk.bNation = clb.bNation;
      mdDesk.iClubId = clb.iId;

      // Set default view club/nation if unset
      if (_usr.nation != null) {
        for (byte iN = 0; iN < MemberController.iShowClubNat.Length; iN++) {
          if (_usr.nation.iLand == iNations[iN]) {
            if (MemberController.iShowClubNat[iN] == 0 && (_usr.nation.nextGame != null || _usr.club.nextGame != null)) { // Unset
              if (_usr.nation.nextGame == null) MemberController.iShowClubNat[iN] = +1;
              else if (_usr.club.nextGame == null) MemberController.iShowClubNat[iN] = -1;
              else if (_usr.nation.nextGame.dt.CompareTo(_usr.club.nextGame.dt) < 0) MemberController.iShowClubNat[iN] = -1;
              else MemberController.iShowClubNat[iN] = +1;
            }
            break;
          }
        }
      }

      // Get Table
      if (clb.bNation) {
        CornerkickManager.Cup cupWc = ckMng.tl.getCup(iCupIdWc);

        if (cupWc != null && cupWc.checkClubInCup(clb)) {
          int iMd = cupWc.getMatchday(ckMng.dtDatum);
          string sText = "";

          mdDesk.sIntCupName = cupWc.sName;

          if (iMd < 3) {
            sText = "Gruppenphase, " + cupWc.getPlace(clb, iMd, bGroupPhaseOnly: true).ToString() + ". Platz";
          } else {
            int iPlace = cupWc.getPlace(clb, iMd);

            if (iPlace > 9) {
              sText = "ausgeschieden (Gruppenphase, " + cupWc.getPlace(clb, iMd, bGroupPhaseOnly: true).ToString() + ". Platz)";
            } else {
              if (iPlace > 1) {
                byte iKoRound = cupWc.getKoRound(iPlace);
                int iMdClub = Math.Max(cupWc.getMatchdays(clb), 0);

                if (iMdClub < iMd) sText = "ausgeschieden (" + CornerkickManager.Main.sCupRound[iKoRound - 1] + ")";
                else sText = CornerkickManager.Main.sCupRound[iKoRound - 1];
              } else {
                sText = "gewonnen";
              }
            }
          }

          mdDesk.sIntCupRound = sText;
        }
      } else {
        CornerkickManager.Cup? league = LeagueController.getClubDivision(clb);

        if (league != null) {
          mdDesk.iPlaceLeague = CupController.getCupPlace(league, clb, ckMng.dtDatum);
          /*
          mdDesk.iPlaceLeague[0] = iPlaceLeague;
          mdDesk.iPlaceLeague[1] = iGms;
          mdDesk.iPlaceLeague[2] = (ltTbl.Count - 1) * 2;
          */
        }

        // Nat. cup round
        mdDesk.sNatCupRound = "-";
        mdDesk.sNatCupEliminated = "";
        mdDesk.iLand = clb.iLand;
        mdDesk.iDiv = clb.iDivision;

        CornerkickManager.Cup cup = ckMng.tl.getCup(iCupIdNatCup, clb.iLand);
        if (cup != null) {

          if (cup.ltMatchdays != null) {
            if (cup.ltMatchdays.Count > 0) {
              if (cup.ltMatchdays[0].ltGameData != null) {
                int nPartFirstRound = cup.ltClubs[0].Count;

                if (nPartFirstRound > 0) {
                  int nRound = cup.getKoRound(nPartFirstRound);
                  int iMdClub = Math.Max(cup.getMatchdays(clb), 0);
                  int iMdCurr = cup.getMatchday(ckMng.dtDatum); // Current matchday

                  if (nRound - iMdClub >= 0) {
                    string sCupRound = (nRound - iMdClub).ToString() + ". Runde";
                    if (CornerkickManager.Main.sCupRound != null && nRound - iMdClub < CornerkickManager.Main.sCupRound.Length) sCupRound = CornerkickManager.Main.sCupRound[nRound - iMdClub];

                    mdDesk.sNatCupRound = sCupRound;
                    if (iMdClub < iMdCurr) mdDesk.sNatCupEliminated = "ausgeschieden";
                  }
                }
              }
            }
          }
        }

        // Intern. cup round
        foreach (CornerkickManager.Cup cupInternat in ckMng.ltCups.FindAll(c => c.iId == iCupIdInt)) {
          if (cupInternat != null && cupInternat.checkClubInCup(clb)) {
            int iMd = cupInternat.getMatchday(ckMng.dtDatum);
            string sText = "";

            mdDesk.iIntCupId2 = cupInternat.iId2;
            mdDesk.sIntCupName = cupInternat.sName;

            //int iClubsTotal = cupInternat.getClubsTotal();
            if (iMd < 6) {
              sText = "Gruppenphase, " + cupInternat.getPlace(clb, iMd, bGroupPhaseOnly: true).ToString() + ". Platz";
            } else {
              int iPlace = cupInternat.getPlace(clb, iMd);

              if (iPlace > 9) {
                sText = "ausgeschieden (Gruppenphase, " + cupInternat.getPlace(clb, iMd, bGroupPhaseOnly: true).ToString() + ". Platz)";
              } else {
                if (iPlace > 1) {
                  byte iKoRound = cupInternat.getKoRound(iPlace);
                  int iMdClub = Math.Max(cupInternat.getMatchdays(clb), 0);

                  if (iMdClub < iMd) sText = "ausgeschieden (" + CornerkickManager.Main.sCupRound[iKoRound - 1] + ")";
                  else sText = CornerkickManager.Main.sCupRound[iKoRound - 1];
                } else {
                  sText = "gewonnen";
                }
              }
            }

            mdDesk.sIntCupRound = sText;

            break; // Only one intern. cup possible
          }
        }
      }

      // Get series
      int[] iSeries = CornerkickManager.UI.getClubSeries(ckMng.tl.getNextGames(clb, ckMng.dtDatum, bPre: true, iGameType: -iCupIdTestgame), clb.iId, iGameType: -iCupIdTestgame);
      if (iSeries[1] > 2) mdDesk.sSeries = "Letzten " + iSeries[1].ToString() + " Spiele " + (iSeries[0] == 0 ? "gewonnen" : "verloren");
      if (iSeries[2] > iSeries[1]) mdDesk.sSeries += (string.IsNullOrEmpty(mdDesk.sSeries) ? "S" : " und s") + "eit " + iSeries[2].ToString() + " Spielen " + (iSeries[0] == 0 ? "ungeschlagen" : "sieglos");
      if (!string.IsNullOrEmpty(mdDesk.sSeries) && iSeries[0] == 0) mdDesk.sSeries += "!";
      /*
      desk.sVDL = "";
      List<CornerkickGame.Game.GameSummary> ltGameSummary = ckMng.tl.getNextGames(club, false, true);
      foreach (CornerkickGame.Game.GameSummary gs in ltGameSummary) {
        if        (gs.iGoalsH == gs.team[1].iGoals) {
          if      (gs.team[0].iTeamId == club.iId) desk.sVDL += "U, ";
          else if (gs.team[1].iTeamId == club.iId) desk.sVDL += "u, ";
        } else if (gs.iGoalsH >  gs.team[1].iGoals) {
          if      (gs.team[0].iTeamId == club.iId) desk.sVDL += "S, ";
          else if (gs.team[1].iTeamId == club.iId) desk.sVDL += "n, ";
        } else if (gs.iGoalsH <  gs.team[1].iGoals) {
          if      (gs.team[0].iTeamId == club.iId) desk.sVDL += "N, ";
          else if (gs.team[1].iTeamId == club.iId) desk.sVDL += "s, ";
        }
      }
      desk.sVDL = desk.sVDL.Trim();
      if (string.IsNullOrEmpty(desk.sVDL)) desk.sVDL = "-";
      */

      // Check if emblem exist
      //mdDesk.bEmblemExist = !getClubEmblem(club).StartsWith("<img src=\"/Content/Uploads/emblems/0.png");

#if _WebApp
      if (clb.nextGame != null) mdDesk.bShowPreviewGame = (clb.nextGame.dt - ckMng.dtDatum).TotalHours > -0.01 && (clb.nextGame.dt - ckMng.dtDatum).TotalHours < 2.01;
#endif

#if _USE_NEWTONSOFTJSON
      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
#endif

      return mdDesk;
      //return System.Text.Json.JsonSerializer.Serialize(mdDesk);
    }

    public static DeskWarningsModel GetWarnings(CornerkickManager.User? _usr)
    {
      DeskWarningsModel mdDeskWarnings = new DeskWarningsModel();
      mdDeskWarnings.bAny = false;

      if (_usr == null) return mdDeskWarnings;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return mdDeskWarnings;

      mdDeskWarnings.bNation = clb.bNation;

      mdDeskWarnings.fSkillPointsFree = _usr.fSkillPointsFree;
      mdDeskWarnings.bAny = mdDeskWarnings.bAny || mdDeskWarnings.fSkillPointsFree >= 1;

      mdDeskWarnings.iSponsorId = clb.sponsorMain.iId;
      if (clb.iCaptainId != null) {
        mdDeskWarnings.iCaptainId = clb.iCaptainId[0];
        mdDeskWarnings.bAny = mdDeskWarnings.bAny || mdDeskWarnings.iCaptainId < 0;
      }

      // No training warning
      /*
      bool bAdmin = AdminModel.checkUserIsAdmin(_usr);
      if (bAdmin) {
        return mdDesk;
      }
      */

      DateTime dtNextSunday = ckMng.dtDatum.Date.AddDays(2);
      while ((int)dtNextSunday.DayOfWeek != 0) dtNextSunday = dtNextSunday.AddDays(1);
      mdDeskWarnings.bNoTrainingWarning = dtNextSunday.CompareTo(ckMng.dtSeasonEnd) < 0;
      if (mdDeskWarnings.bNoTrainingWarning && clb.training.ltUnit != null) {
        foreach (CornerkickManager.Main.TrainingPlan.Unit tu in clb.training.ltUnit) {
          if (tu.iType > 0 && tu.dt.CompareTo(ckMng.dtDatum) > 0 && tu.dt.CompareTo(dtNextSunday) < 0) {
            mdDeskWarnings.bNoTrainingWarning = false;
            break;
          }
        }
      }
      mdDeskWarnings.bAny = mdDeskWarnings.bAny || mdDeskWarnings.bNoTrainingWarning;

      if (_usr.iResp >= iUserRespStaff) {
        // No staff warning
        mdDeskWarnings.bNoStaffWarning =
          clb.staff.iCoTrainer     == 0 &&
          clb.staff.iCondiTrainer  == 0 &&
          clb.staff.iPhysio        == 0 &&
          clb.staff.iMentalTrainer == 0 &&
          clb.staff.iJouthTrainer  == 0 &&
          clb.staff.iJouthScouting == 0 &&
          clb.staff.iKibitzer      == 0 &&
          !clb.bNation;
        mdDeskWarnings.bAny = mdDeskWarnings.bAny || mdDeskWarnings.bNoStaffWarning;

        // Injured player no doctor warning
        mdDeskWarnings.bInjuredPlayerNoDoctorWarning = false;
        foreach (CornerkickManager.Player pl in clb.ltPlayer) {
          if (pl.plGame.injury != null) {
            bool bPlayerHasDoctor = false;
            if (clb.staff.ltDoctor != null) {
              foreach (CornerkickManager.Main.Staff.Doctor dr in clb.staff.ltDoctor) {
                if (dr.plPatient?.iId == pl.plGame.iId) {
                  bPlayerHasDoctor = true;
                  break;
                }
              }

              mdDeskWarnings.bInjuredPlayerNoDoctorWarning = mdDeskWarnings.bInjuredPlayerNoDoctorWarning || !bPlayerHasDoctor;
            }
          }
        }
      }
      mdDeskWarnings.bAny = mdDeskWarnings.bAny || mdDeskWarnings.bInjuredPlayerNoDoctorWarning;

      // No merchandising warning
      mdDeskWarnings.bNoMerchandisingWarning = clb.merchMarketer == null && !clb.bNation && _usr.iResp >= iUserRespMerchandising;
      if (mdDeskWarnings.bNoMerchandisingWarning) {
        foreach (CornerkickManager.Club.MerchandisingItem mi in clb.ltMerchandisingItem) {
          if (mi.iPresent > 0) {
            mdDeskWarnings.bNoMerchandisingWarning = false;
            break;
          }
        }
      }
      mdDeskWarnings.bAny = mdDeskWarnings.bAny || mdDeskWarnings.bNoMerchandisingWarning;

      return mdDeskWarnings;
    }

    public static DeskStatusModel GetDeskStatus(CornerkickManager.User? _usr)
    {
      DeskStatusModel mdDeskStatus = new DeskStatusModel();

      if (_usr == null) return mdDeskStatus;
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return mdDeskStatus;

      // Date next game
      mdDeskStatus.sNextGameInfo = MemberController.getNextGameInfo(clb);

      // Weather next game
      if (clb.nextGame != null) mdDeskStatus.iWeather = clb.nextGame.iWeather;

      // Kondi/Frische/Moral
      float[] fTeamAve = CornerkickManager.Tool.getTeamAve(clb, ckMng.dtDatum, ckMng.dtSeasonEnd, bScouting: _usr.bScouting);
      float[] fTeamAve11 = CornerkickManager.Tool.getTeamAve(clb, ckMng.dtDatum, ckMng.dtSeasonEnd, ptPitch: ckMng.game.ptPitch, iPlStop: 11, bScouting: _usr.bScouting);
      mdDeskStatus.fCFM = [ fTeamAve[0], fTeamAve[1], fTeamAve[2] ];
      mdDeskStatus.fStrength = [ fTeamAve[3], fTeamAve11[3] ];

      return mdDeskStatus;
    }

    static readonly byte[] iNewspaperTypes = [CornerkickManager.Main.iNewsTypeNewYear, CornerkickManager.Main.iNewsTypeCupWin];
    static readonly byte[] iNewsNations    = [CornerkickManager.Main.iNewsTypeGeneral, CornerkickManager.Main.iNewsTypeCupDraw];
    public static DeskModel.DatatableNews[] GetNews(CornerkickManager.User _usr, bool bShortDateFormat)
    {
      List<DeskModel.DatatableNews> ltNews = new List<DeskModel.DatatableNews>();

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      CultureInfo ci = MemberController.getCi(clb);

      if (_usr == null) return ltNews.ToArray();
      if (_usr.ltNews == null) return ltNews.ToArray();

      int iDeleteLog = 0;
      if (_usr.lti != null) {
        if (_usr.lti.Count > UserOptionsModel.iUserOptionsIxDeleteLog) iDeleteLog = _usr.lti[UserOptionsModel.iUserOptionsIxDeleteLog];
      }

      // Remove identical news (same text and date)
      for (int iN = 1; iN < _usr.ltNews.Count; iN++) {
        if (_usr.ltNews[iN].dt.Equals(_usr.ltNews[iN - 1].dt) && _usr.ltNews[iN].sText.Equals(_usr.ltNews[iN - 1].sText)) {
          _usr.ltNews.RemoveAt(iN--);
        }

        // Remove news dependent on user responsible level
        if (_usr.ltNews[iN].iType >= 10 && _usr.ltNews[iN].iType < 15 && _usr.iResp < iUserRespJouth)             _usr.ltNews.RemoveAt(iN--); // (Jouth) Player development news
        if (_usr.ltNews[iN].iType == 15                               && _usr.iResp < iUserRespJouth)             _usr.ltNews.RemoveAt(iN--); // New jouth player news
        if (_usr.ltNews[iN].iType >= 18 && _usr.ltNews[iN].iType < 20 && _usr.iResp < iUserRespPlayerIndTraining) _usr.ltNews.RemoveAt(iN--); // Player development (scouting) news
        if (_usr.ltNews[iN].iType >= 23 && _usr.ltNews[iN].iType < 26 && _usr.iResp < iUserRespJouth)             _usr.ltNews.RemoveAt(iN--); // Jouth player injured news
        if (_usr.ltNews[iN].iType >= 30 && _usr.ltNews[iN].iType < 40 && _usr.iResp < iUserRespTransfers)         _usr.ltNews.RemoveAt(iN--); // Transfer news
        if (_usr.ltNews[iN].iType == 60                               && _usr.iResp < iUserRespSponsors)          _usr.ltNews.RemoveAt(iN--); // Sponsor news
      }

      for (int iN = _usr.ltNews.Count - 1; iN >= 0; iN--) {
        CornerkickManager.Main.News news = _usr.ltNews[iN];

        if (iNewspaperTypes.Contains(news.iType)) continue; // No newspaper news
        if ((!iNewsNations.Contains(news.iType) || news.sText.Contains("Transferangebot")) && clb != null && clb.bNation) continue; // No club news if nation

        if (news.iType < 99/* && news.bUnread*/) {
          if (news.bRead && news.bRead2) {
            if (iDeleteLog == 1 && (ckMng.dtDatum - news.dt).TotalDays > 7) {
              _usr.ltNews.Remove(news);
              iN--;
              continue;
            } else if (iDeleteLog == 2 && (ckMng.dtDatum - news.dt).TotalDays > 14) {
              _usr.ltNews.Remove(news);
              iN--;
              continue;
            } else if (iDeleteLog == 3 && (ckMng.dtDatum - news.dt).TotalDays > 30) {
              _usr.ltNews.Remove(news);
              iN--;
              continue;
            }
          }

          string sN = news.sText;

          sN = sN.Replace("Anleitung", "<a style=\"text-decoration:none\" href=\"/usermanual\">Anleitung</a>");
          sN = sN.Replace("Stadions", "<a style=\"text-decoration:none\" href=\"/member/stadium\">Stadions</a>");
          sN = sN.Replace("Stadionumgebung", "<a style=\"text-decoration:none\" href=\"/member/buildings\">Stadionumgebung</a>");
          sN = sN.Replace("Jugendmannschaft", "<a style=\"text-decoration:none\" href=\"/member/jouth\">Jugendmannschaft</a>");
          sN = sN.Replace("Jugendspieler", "<a style=\"text-decoration:none\" href=\"/member/jouth\">Jugendspieler</a>");
          sN = sN.Replace("Hauptsponsor", "<a style=\"text-decoration:none\" href=\"/member/sponsors\">Hauptsponsor</a>");
          sN = sN.Replace("Transferangebot", "<a style=\"text-decoration:none\" href=\"/member/transfer\">Transferangebot</a>");
          sN = sN.Replace("wählen Sie", "<a style=\"text-decoration:none\" href=\"/member/transfer\">wählen Sie</a>");

          // Replace cup names with link
          foreach (CornerkickManager.Cup cup in ckMng.ltCups) {
            if (sN.Contains(cup.sName)) {
              if      (cup.iId == iCupIdLeague)   sN = sN.Replace(cup.sName, "<a style=\"text-decoration:none\" href=\"/member/league\">" + cup.sName + "</a>");
              else if (cup.iId == iCupIdWc)       sN = sN.Replace(cup.sName, "<a style=\"text-decoration:none\" href=\"/Member/CupWc\">" + cup.sName + "</a>");
              else if (cup.iId == iCupIdTestgame) sN = sN.Replace(cup.sName, "<a style=\"text-decoration:none\" href=\"/Member/Calendar\">" + cup.sName + "</a>");
              else                                sN = sN.Replace(cup.sName, "<a style=\"text-decoration:none\" href=\"/member/cup/" + cup.iId.ToString() + "/" + cup.iId2.ToString() + "/" + cup.iId3.ToString() + "\">" + cup.sName + "</a>");
            }
          }

          // Replace player names with link
          foreach (CornerkickManager.Player pl in clb.ltPlayer) {
            if (!string.IsNullOrEmpty(pl.plGame.sName) && sN.Contains(pl.plGame.sName)) {
              sN = sN.Replace(pl.plGame.sName, "<a style=\"text-decoration:none\" href=\"/member/playerdetails/" + pl.plGame.iId.ToString() + "\" target = \"\">" + pl.plGame.sName + "</a>");
              break;
            }
          }

          foreach (CornerkickManager.Player pl in clb.ltPlayerJouth) {
            if (!string.IsNullOrEmpty(pl.plGame.sName) && sN.Contains(pl.plGame.sName)) {
              sN = sN.Replace(pl.plGame.sName, "<a style=\"text-decoration:none\" href=\"/member/playerdetails/" + pl.plGame.iId.ToString() + "\" target = \"\">" + pl.plGame.sName + "</a>");
              break;
            }
          }

          DeskModel.DatatableNews dtn = new DeskModel.DatatableNews();
          dtn.iId = iN;
          if (bShortDateFormat) dtn.sDate = news.dt.ToString("dd.MM.");
          else                  dtn.sDate = news.dt.ToString("d", ci);
          dtn.sDate += " " + news.dt.ToString("t", ci);
          dtn.sText = sN;
          dtn.iType = news.iType;
          dtn.bOld = news.bRead;

          ltNews.Add(dtn);

          //if (bMarkRead) {
#if _WebApp
          news.bRead = true;
          news.bRead2 = true;
#endif
          _usr.ltNews[iN] = news;
          //}
        }
      }

      return ltNews.ToArray();
    }

    public void SetDeleteLog(int iDeleteAfter)
    {
      if (_usr == null) return;

      if (_usr.lti == null) _usr.lti = new List<int>();
      if (_usr.lti.Count <= UserOptionsModel.iUserOptionsIxDeleteLog) _usr.lti.Add(0);

      _usr.lti[UserOptionsModel.iUserOptionsIxDeleteLog] = iDeleteAfter;
    }

    public static DeskModel.DatatableNews[] GetNewspaper(CornerkickManager.User? _usr)
    {
      List<DeskModel.DatatableNews> ltNews = new List<DeskModel.DatatableNews>();

      if (_usr == null) return ltNews.ToArray();
      if (_usr.ltNews == null) return ltNews.ToArray();

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      CultureInfo ci = MemberController.getCi(clb);


      List<CornerkickManager.Main.News> ltCkNews = new List<CornerkickManager.Main.News>(_usr.ltNews);
#if _WebApp
      foreach (CornerkickManager.Main.News news0 in ckMng.ltUser[0].ltNews) {
        if (news0.iType >= 200) ltCkNews.Add(news0);
      }
#endif

      try {
        for (int iN = ltCkNews.Count - 1; iN >= 0; iN--) {
          CornerkickManager.Main.News news = ltCkNews[iN];

          if (iNewspaperTypes.Contains(news.iType) || news.iType >= 200) {
            if ((ckMng.dtDatum - news.dt).TotalDays > 7) continue;

            string sN = news.sText;

            if (news.iType == CornerkickManager.Main.iNewsTypeNewYear) {
              sN = ckMng.dtDatum.Year.ToString() + "!#" + sN;
              news.iId = -1;
            } else if (news.iType == CornerkickManager.Main.iNewsTypeCupWin) {
              sN = "Was für ein Erfolg!#" + sN;

              // Print club bold
              CornerkickManager.Club? clbBold = ckMng.ltClubs.Find(c => c.iId == news.iId);
              if (clbBold != null) sN = sN.Replace(clbBold.sName, "<b>" + clbBold.sName + "</b>");
            }

            // Add player link
            CornerkickGame.Player? pl = null;
            if (news.iType == 200 && news.iId >= 0 && news.iId < ckMng.ltPlayer.Count) {
              pl = ckMng.ltPlayer.Find(p => p.plGame.iId == news.iId)?.plGame;
              if (pl != null) sN = sN.Replace(pl.sName, "<a style=\"text-decoration:none\" href=\"/Member/PlayerDetails?i=" + pl.iId.ToString() + "\" target = \"\">" + pl.sName + "</a>");
            }

            // Check for identical news already added
            bool bSameNews = false;
            foreach (DeskModel.DatatableNews dtnTmp in ltNews) {
              if (dtnTmp.sText.Equals(sN)) {
                bSameNews = true;
                break;
              }
            }
            if (bSameNews) continue;

            DeskModel.DatatableNews dtn = new DeskModel.DatatableNews();
            dtn.iId = news.iId;
            dtn.sDate = news.dt.ToString("d", ci) + " " + news.dt.ToString("t", ci);
            dtn.sText = sN;
            dtn.iType = news.iType;
            dtn.sHeader = news.sFromId;

            if (news.iId >= 0) {
              if (news.iType == 71) {
                dtn.sImg = ClubController.getClubEmblemImg(ckMng.ltClubs.Find(c => c.iId == news.iId), sStyle: "width: 100%", bTiny: true);
              } else {
                CornerkickManager.Player? plMng = ckMng.ltPlayer.Find(p => p.plGame.iId == news.iId);
                dtn.sImg = PlayerController.getPlayerPortraitHtmlImg(plMng, bSmall: true);
              }
            }

            ltNews.Add(dtn);
          }
        }
      } catch (Exception e) {
        Console.WriteLine(e.Message);
      }

#if _USE_NEWTONSOFTJSON
//      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
#endif

      return ltNews.ToArray();
      //return Json(ltNews.ToArray());
      //return Content(JsonConvert.SerializeObject(ltNews.ToArray(), _jsonSetting), "application/json");
      //return Json(new { aaData = ltNews.ToArray() });
    }

    public static string GetEndingContractsInfo(CornerkickManager.User _usr)
    {
      if (_usr == null) return "";

      CornerkickManager.Club clb = MemberController.ckClub(_usr);

      //if (AdminModel.checkUserIsAdmin(User)) return "";
      if (clb == null) return "";
      if (clb.bNation) return "";

      // Check hide info flag
      int iUserIx = ckMng.ltUser.IndexOf(_usr);

      if (MemberController.bHideEocInfo != null) {
        if (iUserIx >= 0 && iUserIx < MemberController.bHideEocInfo?.Length) {
          if (MemberController.bHideEocInfo[iUserIx]) return "";
        }
      }

      // Return if season end date is before current date (not set)
      if (ckMng.dtSeasonEnd.CompareTo(ckMng.dtDatum) < 0) return "";

      // Return if before december
      if (ckMng.dtDatum.Year < ckMng.dtSeasonEnd.Year && ckMng.dtDatum.Month < 12) return "";

      string sInfo = "";
      foreach (CornerkickManager.Player pl in clb.ltPlayer) {
        if (CornerkickManager.PlayerTool.checkIfContractIsEnding(pl, ckMng.dtSeasonEnd, ckMng.dtSeasonEnd, bIgnoreRetireringPlayer: true)) sInfo += pl.plGame.sName + ", ";
      }
      foreach (CornerkickManager.Player plJ in clb.ltPlayerJouth) {
        if (CornerkickManager.PlayerTool.checkIfContractIsEnding(plJ, ckMng.dtSeasonEnd, ckMng.dtSeasonEnd, bIgnoreRetireringPlayer: true)) sInfo += plJ.plGame.sName + ", ";
      }

      if (!string.IsNullOrEmpty(sInfo)) {
        if (sInfo.EndsWith(", ")) sInfo = sInfo.Remove(sInfo.Length - 2, 2);

        string sWhen = "sofort";
        if (ckMng.dtDatum.Year < ckMng.dtSeasonEnd.Year) sWhen = "nächstem Jahr";
        sInfo = "ACHTUNG! Folgende Spieler besitzen einen auslaufenden Vertrag und können ab " + sWhen + " von einem anderen Verein abgeworben werden:</br>" + sInfo;

        // Set flag to true to not show info again
        if (iUserIx >= 0 && iUserIx < MemberController.bHideEocInfo?.Length) MemberController.bHideEocInfo[iUserIx] = true;
      }

      //return Json(sInfo);
      return sInfo;
    }

    //public ContentResult GetLastGames()
    public static List<DataPointII>[] GetLastGames(CornerkickManager.User _usr)
    {
      List<DataPointII>[] dataPoints = new List<DataPointII>[4];

      dataPoints[0] = new List<DataPointII>(); // League
      dataPoints[1] = new List<DataPointII>(); // Nat. cup
      dataPoints[2] = new List<DataPointII>(); // Gold/Silver/Bronze Cup
      //dataPoints[3] = new List<DataPointII>(); // Testgame
      dataPoints[3] = new List<DataPointII>(); // National Team

      if (_usr == null) return dataPoints;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      CultureInfo ci = MemberController.getCi(clb);

      if (clb == null) return dataPoints;

      List<CornerkickGame.Game.Data> ltGameData = ckMng.tl.getNextGames(clb, ckMng.dtDatum, false, true);
      int iLg = 0;
      foreach (CornerkickGame.Game.Data gs in ltGameData) {
        if (gs.iGameType < 1 || gs.iGameType == iCupIdTestgame) continue; // Testgame
        if (gs.team[0].iGoals < 0 || gs.team[1].iGoals < 0) continue;
        if (gs.team[0].iTeamId == 0 && gs.team[1].iTeamId == 0) continue;

        CornerkickManager.Club? clbH = ckMng.ltClubs.Find(c => c.iId == gs.team[0].iTeamId);
        CornerkickManager.Club? clbA = ckMng.ltClubs.Find(c => c.iId == gs.team[1].iTeamId);

        string sDesc = "<p style=\"margin:0\">" + gs.dt.ToString("d", ci) + "</p>";
        if (clbH != null && clbA != null) sDesc += "<p style=\"margin:0\"><b>" + clbH.sName + " - " + clbA.sName + "</b></p>";
        sDesc += "<p style=\"margin:0\">" + CornerkickManager.UI.getResultString(gs) + "</p>";

        int iGameType = 0;
        if      (gs.iGameType == iCupIdNatCup) iGameType = 1;
        else if (gs.iGameType == iCupIdInt)    iGameType = 2;
        else if (gs.iGameType == iCupIdTestgame) continue;
        else if (gs.iGameType == iCupIdWc)     iGameType = 3;

        if (gs.team[0].iGoals == gs.team[1].iGoals) {
          dataPoints[iGameType].Add(new DataPointII(iLg--, 0, sDesc));
        } else if ((gs.team[0].iGoals > gs.team[1].iGoals && gs.team[0].iTeamId == clb.iId) ||
                   (gs.team[0].iGoals < gs.team[1].iGoals && gs.team[1].iTeamId == clb.iId)) {
          dataPoints[iGameType].Add(new DataPointII(iLg--, +1, sDesc));
        } else if ((gs.team[0].iGoals < gs.team[1].iGoals && gs.team[0].iTeamId == clb.iId) ||
                   (gs.team[0].iGoals > gs.team[1].iGoals && gs.team[1].iTeamId == clb.iId)) {
          dataPoints[iGameType].Add(new DataPointII(iLg--, -1, sDesc));
        }
      }

      //return dataPoints;
#if _USE_NEWTONSOFTJSON
      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return JsonConvert.SerializeObject(dataPoints, _jsonSetting);
#else
      return dataPoints;
#endif
    }

    public static CornerkickManager.Finance.Account[] GetBalanceToday(CornerkickManager.User _usr)
    {
      List<CornerkickManager.Finance.Account> ltBalanceToday = new List<CornerkickManager.Finance.Account>();

      CornerkickManager.Club clb = MemberController.ckClub(_usr);

      if (clb == null) return new CornerkickManager.Finance.Account[0];

      foreach (CornerkickManager.Finance.Account acc in clb.ltAccount) {
        if (acc.dt.Date.Equals(ckMng.dtDatum.Date)) {
          ltBalanceToday.Add(acc);
        }
      }

      return ltBalanceToday.ToArray();
    }

    public bool SetBalanceTodayDialog(bool bOn)
    {
      if (_usr == null) return false;

      if (_usr.lti == null) _usr.lti = new List<int>();
      while (_usr.lti.Count <= UserOptionsModel.iUserOptionsIxShowBalance) _usr.lti.Add(0);

      _usr.lti[UserOptionsModel.iUserOptionsIxShowBalance] = bOn ? 1 : 0;

      return true;
    }

    public PreviewGameModel.GameInfo GetPreviewGameInfo()
    {
      CornerkickManager.Club clb = MemberController.ckClub(_usr);
      CultureInfo ci = MemberController.getCi(clb);

      if (clb == null) return null;

      return MemberController.getGameInfo(_usr, ckMng.tl.getNextGame(clb, ckMng.dtDatum));
    }

  }
}
