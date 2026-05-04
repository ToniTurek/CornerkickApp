using CornerkickApp.Shared.Models;
using System.Net;
using static CornerkickApp.Shared.Models.CalendarModel;
using static CornerkickApp.Shared.Models.CkAppShared;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  public class CalendarController
  {
    public static CalendarModel Model(CornerkickManager.User _usr)
    {
      CalendarModel cal = new CalendarModel();
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);

      if (clb != null) {
        cal.iClubId = clb.iId;

        CornerkickGame.Game.Data gdLastGame = ckMng.tl.getNextGame(clb, ckMng.dtDatum, bPre: true);
        cal.ltPlayer = new string[clb.ltPlayer.Count][];
        for (int iP = 0; iP < clb.ltPlayer.Count; iP++) {
          CornerkickManager.Player pl = clb.ltPlayer[iP];
          //CornerkickGame.Player.Statistic plStat = pl.plGame.getStatistic(iCupIdLastGame);
          string sGrade = "keine";
          if (gdLastGame != null && gdLastGame.iGameType == pl.plGame.statGame.iGameType && (gdLastGame.iMatchday == 0 || gdLastGame.iMatchday == pl.plGame.statGame.iMatchday) && pl.plGame.statGame.iStat[29] > 0) {
            sGrade = (pl.plGame.statGame.iStat[29] * 0.1).ToString("0.0");
          }
          cal.ltPlayer[iP] = [pl.plGame.sName, pl.plGame.iId.ToString(), sGrade];
        }

        cal.sliTestgameClubs = new List<SelectListItem>();
        foreach (CornerkickManager.Club clbTg in ckMng.ltClubs) {
          if (clbTg.iId == cal.iClubId) continue;
          if (clbTg.iLand < 0) continue;
          if (clb.bNation != clbTg.bNation) continue;

          cal.sliTestgameClubs.Add(new SelectListItem { Text = clbTg.sName, Value = clbTg.iId.ToString() });
        }
      }

      // Tutorial
      if (ttUser != null) {
        int iUserIx = ckMng.ltUser.IndexOf(_usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) cal.tutorial = ttUser[iUserIx];
      }

      return cal;
    }

    public static Task<Appointment[]> getCalendarEvents(CornerkickManager.User? usr = null, int iDayMonthYear = 0, bool bMobile = false)
    {
      IList<Appointment> ltEvents = new List<Appointment>();

      CornerkickManager.Club? clb = null;
      if (usr != null) {
        clb = MemberController.ckClub(usr);
        if (clb == null) return Task.FromResult(ltEvents.ToArray());
      }

      DateTime dtStartWeek = ckMng.dtDatum;
      while ((int)(dtStartWeek.DayOfWeek) != 0) dtStartWeek = dtStartWeek.AddDays(-1);

      int iDay = 0;
      //DateTime dt = new DateTime(ckMng.dtDatum.Year, ckMng.dtDatum.Month, ckMng.dtDatum.Day);
      DateTime dt = ckMng.dtSeasonStart.Date;
      while (dt.CompareTo(ckMng.dtSeasonEnd) < 0) {
        // Night
        if (iDayMonthYear == 0 && !dt.Date.Equals(ckMng.dtSeasonEnd.Date)) { // Always but not on day of season end
          ltEvents.Add(new Appointment {
            iID = ltEvents.Count,
            sTitle = "Nachtruhe",
            sDescription = "Nachtruhe",
            dtStart = dt.Add(CornerkickManager.Main.tsNightStart),
            dtEnd = dt.AddDays(1).Add(CornerkickManager.Main.tsNightEnd),
            sColor = "rgb(0, 0, 140)",
            sColor2 = "white",
            bEditable = false,
            bAllDay = false,
            sClassName = "eventHideInMonthView eventHideInYearView"
          });
        }

        // New Year
        if (dt.Day == 1 && dt.Month == 1) {
          ltEvents.Add(new Appointment {
            iID = ltEvents.Count,
            sTitle = "Neujahr",
            sDescription = "Neujahr",
            dtStart = dt,
            sColor = "rgb(200, 200, 200)",
            bEditable = false,
            bAllDay = true
          });
        }

        dt = dt.AddDays(1);
        iDay++;
      }

      // Cups
      foreach (CornerkickManager.Cup cup in ckMng.ltCups) {
        int iMd = 0;
        foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
          if (md.ltGameData == null || md.ltGameData.Count == 0) { // If no data
            if (clb == null || cup.iId2 == clb.iLand) {
              ltEvents.Add(new Appointment {
                iID = ltEvents.Count,
                sTitle = (iDayMonthYear == 2 ? " " + md.dt.ToShortTimeString() + " - " : " ") + cup.sName + ", " + (iMd + 1).ToString().PadLeft(2) + ". Spieltag",
                dtStart = md.dt,
                dtEnd = md.dt.AddMinutes(105),
                sColor = "rgb(100, 100, 255)",
                bEditable = false,
                bAllDay = false
              });
            }

            iMd++;
            continue;
          }

          // Cup draw
          if (cup.iId == iCupIdNatCup && !cup.checkCupGroupPhase(iMd) && (clb == null || cup.iId2 == clb.iLand) && md.ltGameData.Count > 1) {
            ltEvents.Add(new Appointment {
              iID = ltEvents.Count,
              sTitle = (iDayMonthYear == 2 ? " 12:00 - " : " ") + cup.sName + ", Auslosung " + CornerkickManager.Main.sCupRound[cup.getKoRound(md.ltGameData.Count) - 1],
              dtStart = new DateTime(md.dt.Year, md.dt.Month, md.dt.AddDays(1).Day, 12, 0, 0),
              dtEnd = new DateTime(md.dt.Year, md.dt.Month, md.dt.AddDays(1).Day, 12, 30, 0),
              sColor = "rgb(100, 200, 255)",
              bEditable = false,
              bAllDay = false
            });
          }

          foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
            bool bAdmin = /*AdminModel.checkUserIsAdmin(_usr);*/false;
            int iIdH = gd.team[0].iTeamId;
            int iIdA = gd.team[1].iTeamId;
            if (clb  == null    || 
                iIdH == clb.iId ||
                iIdA == clb.iId ||
                //(cup.iId == iCupIdNatCup && cup.iId2 == clb.iLand && iMd == 0) ||
                 cup.iId == iCupIdWc ||
                bAdmin) {
              CornerkickManager.Club? clbH = ckMng.ltClubs.Find(c => c.iId == iIdH);
              if (clbH == null) continue;
              CornerkickManager.Club? clbA = ckMng.ltClubs.Find(c => c.iId == iIdA);
              if (clbA == null) continue;

              string sRes = CornerkickManager.UI.getResultString(gd);
              string sColor = Tool.convertToRgb(CupController.getCupColor(cup));

              string sTitle = (iDayMonthYear == 2 ? " " + gd.dt.ToShortTimeString() + " - " : " ") + cup.sName;
              if (cup.settings.nGroups > 0) { // Nat. league
                sTitle += " " + (iMd + 1).ToString().PadLeft(2) + ". Spieltag";
              } else if (cup.iId == iCupIdNatCup) { // Nat. Cup
                sTitle += ", " + CornerkickManager.Main.sCupRound[cup.getKoRound(md.ltGameData.Count)];
              }
              if (usr != null) {
                sTitle += ": " + clbH.sName + " vs. " + clbA.sName;
                if (!string.IsNullOrEmpty(sRes)) sTitle += ", " + sRes;
              }

              ltEvents.Add(new Appointment {
                iID = ltEvents.Count,
                sTitle = sTitle,
                dtStart = gd.dt,
                dtEnd = gd.dt.AddMinutes(105),
                sColor = sColor,
                bEditable = false,
                bAllDay = false
              });

              if (!bAdmin) break;
            }
          }
          /*
          } else if (cup.iId == iCupIdNatCup && cup.iId2 == club.iLand && iMd == 0 && dt.Date.Equals(ckMng.dtSeasonStart.AddDays(6).Date)) {
            ltEvents.Add(new Appointment {
              iID = ltEvents.Count,
              sTitle = " " + cup.sName + ", Auslosung 1. Runde",
              dtStart = new DateTime(dt.Year, dt.Month, dt.Day, 12,  0, 0),
              dtEnd   = new DateTime(dt.Year, dt.Month, dt.Day, 12, 30, 0),
              sColor = "rgb(100, 200, 255)",
              bEditable = false,
              bAllDay = false
            });
          */

          iMd++;
        }
      }

      if (clb != null) {
        if ((bMobile && iDayMonthYear < 1) || (!bMobile && iDayMonthYear < 2)) {
          // Future training
          foreach (CornerkickManager.Main.TrainingPlan.Unit tu in clb.training.ltUnit) {
            if (tu.iType > 0 && tu.iType < 100) {
              string sTrainingName = CornerkickManager.PlayerTool.getTraining(tu.iType, ckMng.plt.ltTraining).sName;

              ltEvents.Add(new Appointment {
                iID = ltEvents.Count,
                sTitle = " Training (" + sTrainingName + ")",
                sDescription = sTrainingName,
                dtStart = tu.dt,
                dtEnd = tu.dt.AddMinutes(90),
                sColor = "rgb(255, 255, 0)",
                sColor2 = "rgb(100, 100, 100)",
                bEditable = false,
                bAllDay = false,
                sClassName = "eventHideInYearView"
              });
            }
          }

          // Past trainings
          foreach (CornerkickManager.Player.TrainingHistory th in clb.ltTrainingHist) {
            if (th.iType > 1 && th.iType < 100) {
              string sTrainingName = CornerkickManager.PlayerTool.getTraining(th.iType, ckMng.plt.ltTraining).sName;

              ltEvents.Add(new Appointment {
                iID = ltEvents.Count,
                sTitle = " Training (" + sTrainingName + ")",
                sDescription = sTrainingName,
                dtStart = th.dt,
                dtEnd = th.dt.AddMinutes(90),
                sColor = "rgb(255, 255, 180)",
                sColor2 = "rgb(100, 100, 100)",
                bEditable = false,
                bAllDay = false,
                sClassName = "eventHideInYearView"
              });
            }
          }
        }

        // Trainingscamp
        if (clb.ltCamp != null) {
          foreach (CornerkickManager.TrainingCamp.Booking booking in clb.ltCamp) {
            if (booking != null) {
              ltEvents.Add(new Appointment {
                iID = ltEvents.Count,
                sTitle = "Abreise Trainingslager",
                sDescription = booking.camp.sName,
                dtStart = booking.dtDeparture,
                dtEnd = booking.dtDeparture.Add(booking.camp.tsTravel),
                sColor = "rgb(255, 100, 0)",
                bEditable = false,
                bAllDay = false
              });

              ltEvents.Add(new Appointment {
                iID = ltEvents.Count,
                sTitle = "Rückreise Trainingslager",
                sDescription = booking.camp.sName,
                dtStart = booking.dtReturn.Subtract(booking.camp.tsTravel),
                dtEnd = booking.dtReturn,
                sColor = "rgb(255, 100, 0)",
                bEditable = false,
                bAllDay = false
              });

              DateTime dtTc = booking.dtDeparture.Date.AddDays(1);
              while (dtTc.CompareTo(booking.dtReturn.Date) < 0) {
                ltEvents.Add(new Appointment {
                  iID = ltEvents.Count,
                  sTitle = "Trainingslager",
                  sDescription = booking.camp.sName,
                  dtStart = dtTc,
                  sColor = "rgb(255, 150, 0)",
                  bEditable = false,
                  bAllDay = true
                });

                dtTc = dtTc.AddDays(1);
              }
            }
          }
        }

        // Events
        if (clb.ltEvent != null) {
          foreach (CornerkickManager.Club.Event.Item evi in clb.ltEvent) {
            ltEvents.Add(new Appointment {
              iID = ltEvents.Count,
              sTitle = evi.ev.sName,
              sDescription = evi.ev.sName,
              dtStart = evi.dt,
              dtEnd = evi.dt.Add(evi.ev.tsLength),
              sColor = "rgb(255, 200, 255)",
              sColor2 = "rgb(0, 0, 0)",
              bEditable = false,
              bAllDay = false
            });
          }
        }
      }

      // Meetings
      if (usr != null && usr.ltMeetings != null) {
        foreach (CornerkickManager.User.Meeting mtg in usr.ltMeetings) {
          int iMtgLength = 30;
          string sMtgName = "Einzelgespräch " + mtg.player.plGame.sName;
          if (mtg.iType == CornerkickManager.User.iMeetingTypeHospitalVisit) {
            iMtgLength = 60;
            sMtgName = "KH-Besuch " + mtg.player.plGame.sName;
          }
          ltEvents.Add(new Appointment {
            iID = ltEvents.Count,
            sTitle = sMtgName,
            sDescription = sMtgName,
            dtStart = mtg.dt,
            dtEnd = mtg.dt.AddMinutes(iMtgLength),
            sColor = "rgb(255, 175, 200)",
            sColor2 = "rgb(0, 0, 0)",
            bEditable = false,
            bAllDay = false
          });
        }
      }

      // End of season
      ltEvents.Add(new Appointment {
        iID = ltEvents.Count,
        sTitle = "Saisonende",
        sDescription = "Saisonende",
        dtStart = ckMng.dtSeasonEnd,
        dtEnd = ckMng.dtSeasonEnd.Date.AddDays(1),
        sColor = "rgb(0, 0, 0)",
        bEditable = false,
        bAllDay = false
      });

      return Task.FromResult(ltEvents.ToArray());
    }

    public static bool addTestGame(CornerkickManager.User _usr, DateTime dtStart, int iClbRequestId, out string sRet)
    {
      return addTestGame(_usr, dtStart, ckMng.ltClubs.Find(c => c.iId == iClbRequestId), out sRet);
    }
    public static bool addTestGame(CornerkickManager.User _usr, DateTime dtStart, CornerkickManager.Club? clbRequest, out string sRet)
    {
      sRet = "";

      CornerkickManager.Club? clbUser = MemberController.ckClub(_usr);
      if (clbUser == null) return false;

      if (clbRequest == null) return false;

      if (clbRequest.iId == clbUser.iId) return false;

      CornerkickManager.Cup.Matchday md = new CornerkickManager.Cup.Matchday();
      md.dt = dtStart;

      // Check distance to club games for nation test games
      if (clbRequest.bNation) {
        const int iNatDaysDist = 5;
        foreach (CornerkickManager.Cup cup in ckMng.ltCups) {
          if (cup.iId == iCupIdTestgame) continue;

          foreach (CornerkickManager.Cup.Matchday md2 in cup.ltMatchdays) {
            foreach (CornerkickGame.Game.Data gd2 in md2.ltGameData) {
              if (Math.Abs((gd2.dt.Date - md.dt.Date).TotalDays) < iNatDaysDist) {
                sRet = "Testspiele der Nationalmannschaft müssen mindestens " + iNatDaysDist.ToString() + " Tage Abstand zu Vereinsspieltagen haben (z.B. " + cup.sName + ")";
                return false;
              }
            }
          }
        }
      } else {
        int iCompareDates = MemberController.compareDates(clbUser, md.dt, new TimeSpan(4, 0, 0));
        if (iCompareDates < 0) {
          if      (iCompareDates == -1) sRet = "Das ausgewählte Datum liegt in der Vergangenheit.";
          else if (iCompareDates == -3) sRet = "Keine Aktion während der Nachtruhe möglich.";
          else                          sRet = "Sie haben bereits einen anderen Termin an diesem Datum.";
          return false;
        }

        iCompareDates = MemberController.compareDates(clbRequest, md.dt, new TimeSpan(4, 0, 0));
        if (iCompareDates < 0) {
          if      (iCompareDates == -1) sRet = "Das ausgewählte Datum liegt in der Vergangenheit.";
          else if (iCompareDates == -3) sRet = "Keine Aktion während der Nachtruhe möglich.";
          else                          sRet = clbRequest.sName + " hat bereits einen anderen Termin an diesem Datum.";
          return false;
        }
      }

      md.ltGameData = new List<CornerkickGame.Game.Data>();
      CornerkickGame.Game.Data gd = new CornerkickGame.Game.Data();
      gd.team[0].iTeamId = clbUser.iId;
      gd.team[1].iTeamId = clbRequest.iId;
      gd.dt = md.dt;

      md.ltGameData.Add(gd);

      // Inform user
      if (clbRequest.user == null) {
#if !DEBUG
        CornerkickGame.Game.Data gdNext = ckMng.tl.getNextGame(clbRequest, md.dt, bPre: false);
        if (gdNext != null && (gdNext.dt - md.dt).TotalDays < 4) {
          sRet = "Anfrage für Testspiel abgelehnt. Begründung: Zu nah am nächsten Spiel";
          return false;
        }

        CornerkickGame.Game.Data gdPrev = ckMng.tl.getNextGame(clbRequest, md.dt, bPre: true);
        if (gdPrev != null && (md.dt - gdPrev.dt).TotalDays < 4) {
          sRet = "Anfrage für Testspiel abgelehnt. Begründung: Zu kurz nach letztem Spiel";
          return false;
        }
#endif
        createTestgame(md);

        sRet = "Testspiel am " + md.dt.ToString("d", MemberController.getCi(clbUser)) + " " + md.dt.ToString("t", MemberController.getCi(clbUser)) + " gegen " + clbRequest.sName + " vereinbart";

        clbUser.nextGame = ckMng.tl.getNextGame(clbUser, ckMng.dtDatum);

        // Clean club training plan for test game
        clbUser.cleanTraining(ckMng.settings.tsTrainingLength, clbUser.nextGame);
      } else {
        CornerkickManager.Cup cup = new CornerkickManager.Cup();
        cup.iId = -iCupIdTestgame;
        cup.sName = "Anfrage Testspiel";
        cup.ltMatchdays.Add(md);
        ckMng.ltCups.Add(cup);

        sRet = "Anfrage für Testspiel am " + md.dt.ToString("d", MemberController.getCi(clbUser)) + " " + md.dt.ToString("t", MemberController.getCi(clbUser)) + " gegen " + clbRequest.sName + " gesendet";

        ckMng.sendNews(clbRequest.user, "Sie haben eine neue Anfrage für ein Testspiel erhalten.", iType: 0, iId: clbRequest.iId);
      }

      return true;
    }

    private static void createTestgame(CornerkickManager.Cup.Matchday md)
    {
      if (md.ltGameData.Count < 1) return;

      CornerkickManager.Club? clbH = ckMng.ltClubs.Find(c => c.iId == md.ltGameData[0].team[0].iTeamId);
      CornerkickManager.Club? clbA = ckMng.ltClubs.Find(c => c.iId == md.ltGameData[0].team[1].iTeamId);
      if (clbH == null) return;
      if (clbA == null) return;

      CornerkickManager.Cup cupTestGames = ckMng.tl.getCup(iCupIdTestgame);

      if (cupTestGames == null) {
        cupTestGames = new CornerkickManager.Cup();
        cupTestGames.iId = iCupIdTestgame;
        cupTestGames.settings.iNeutral = 1;
        cupTestGames.settings.nPlRes = byte.MaxValue;
        cupTestGames.settings.nSubstitutions = byte.MaxValue;
        cupTestGames.settings.fAttraction = 0.25f;
        cupTestGames.sName = "Testspiel";
        ckMng.ltCups.Add(cupTestGames);
      }

      cupTestGames.ltMatchdays.Add(md);

      if (cupTestGames.ltClubs[0].Find(c => c.iId == clbH.iId) == null) cupTestGames.ltClubs[0].Add(clbH);
      if (cupTestGames.ltClubs[0].Find(c => c.iId == clbA.iId) == null) cupTestGames.ltClubs[0].Add(clbA);

      clbH.nextGame = ckMng.tl.getNextGame(clbH, ckMng.dtDatum);
      clbA.nextGame = ckMng.tl.getNextGame(clbA, ckMng.dtDatum);
    }

    public static int getTrainingscampDays(CornerkickManager.User _usr, DateTime dtStart, int iIgnoreGameType = 0)
    {
      return getTrainingscampDays(MemberController.ckClub(_usr), dtStart, iIgnoreGameType: iIgnoreGameType);
    }

    public static int getTrainingscampDays(CornerkickManager.Club? clb, DateTime dtStart, int iIgnoreGameType = 0)
    {
      if (clb == null) return 0;

      int nDays = getDaysUntilNextGame(clb, dtStart, iIgnoreGameType);

      // Get date until departure to next trainingscamp
      if (clb.ltCamp != null) {
        foreach (CornerkickManager.TrainingCamp.Booking b in clb.ltCamp) {
          if (b.dtDeparture.CompareTo(dtStart) > 0) nDays = Math.Min(nDays, (int)(b.dtDeparture.Date - dtStart.Date).TotalDays);
        }
      }

      return nDays;
    }

    public static bool bookTrainingscamp(CornerkickManager.User _usr, DateTime dtStart, DateTime dtEnd, int iCampId)
    {
      string sMsg;
      return bookTrainingscamp(MemberController.ckClub(_usr), dtStart, dtEnd, iCampId, out sMsg);
    }
    public static bool bookTrainingscamp(CornerkickManager.Club? clb, DateTime dtStart, DateTime dtEnd, int iCampId)
    {
      string sMsg;
      return bookTrainingscamp(clb, dtStart, dtEnd, iCampId, out sMsg);
    }
    public static bool bookTrainingscamp(CornerkickManager.User _usr, DateTime dtStart, DateTime dtEnd, int iCampId, out string sMsg)
    {
      return bookTrainingscamp(MemberController.ckClub(_usr), dtStart, dtEnd, iCampId, out sMsg);
    }
    public static bool bookTrainingscamp(CornerkickManager.Club? clb, DateTime dtStart, DateTime dtEnd, int iCampId, out string sMsg)
    {
      sMsg = "";

      if (clb == null) return false;

      dtStart = dtStart.Date.AddHours(9);

      if ((int)(dtStart.Date - ckMng.dtDatum.Date).TotalDays < 2) {
        sMsg = "Trainingslager müssen min. 2 Tage im vorraus gebucht werden!";
        return false;
      }

      dtEnd = dtEnd.Date.AddHours(18);

      /*
      int iCompareDates = MemberController.compareDates(clb, dtStart, dtEnd - dtStart);
      if (iCompareDates < 0) {
        if      (iCompareDates == -1) sMsg = "Das ausgewählte Datum liegt in der Vergangenheit.";
        else if (iCompareDates == -3) sMsg = "Keine Aktion während der Nachtruhe möglich.";
        else                          sMsg = clb.sName + " hat bereits einen anderen Termin an diesem Datum.";
        return false;
      }
      */
      if (MemberController.checkForTrainingCamp(clb, dtStart, dtEnd - dtStart)) {
        sMsg = "Sie haben bereits ein Trainingslager in diesem Zeitraum geplant.";
        return false;
      }

      foreach (CornerkickGame.Game.Data data in ckMng.tl.getNextGames(clb, ckMng.dtDatum, false)) {
        if (Math.Abs(data.iGameType) == iCupIdTestgame) continue;

        if (dtStart.Date.CompareTo(data.dt.Date) == 0) {
          sMsg = "Abreise am Spieltag nicht erlaubt!";
          return false;
        }
        if (dtEnd.Date.CompareTo(data.dt.Date) == 0) {
          sMsg = "Rückreise am Spieltag nicht erlaubt!";
          return false;
        }

        if (dtStart.Date.CompareTo(data.dt.Date) < 0 /* Start date before game date */ &&
            dtEnd  .Date.CompareTo(data.dt.Date) > 0 /* End date after game date */) {
          sMsg = "Trainingslager über Spieltag nicht erlaubt!";
          return false;
        }
      }

      CornerkickManager.TrainingCamp.Camp camp = ckMng.tcp.ltCamps.Find(c => c.iId == iCampId);

      CornerkickManager.TrainingCamp.bookCamp(ref clb, camp, dtStart, dtEnd, ckMng.dtDatum, ckMng.settings.tsTrainingLength);

      sMsg = "Trainingslager " + camp.sName + " für " + (dtEnd - dtStart).TotalDays.ToString("0") + " Nächte gebucht!";
      return true;
    }

#if false

    public JsonResult setSelectedCamp(CalendarModels mdCalendar, int iIx)
    {
      mdCalendar.camp = ckMng.tcp.ltCamps[iIx];

      return Json(JsonConvert.SerializeObject(mdCalendar.camp));
    }
#endif

    public static int getDaysUntilNextGame(CornerkickManager.Club clb, DateTime dtStart, int iIgnoreGameType = 0)
    {
      int nDays = 999;

      if (clb == null) return nDays;

      List<CornerkickGame.Game.Data> ltGdNext = ckMng.tl.getNextGames(clb, dtStart, false);
      foreach (CornerkickGame.Game.Data data in ltGdNext) {
        if (data.iGameType < 0 || data.iGameType == iIgnoreGameType) continue;

        nDays = Math.Min(nDays, (int)(data.dt - dtStart).TotalDays);
      }

      nDays = Math.Min(nDays, (int)(ckMng.dtSeasonEnd - dtStart).TotalDays);

      return nDays;
    }

    public static int getDaysUntilNextGame(CornerkickManager.Club clb, DateTime dtStart, List<int> ltIgnoreGameTypes)
    {
      int nDays = 999;

      foreach (int iIgnoreGameType in ltIgnoreGameTypes) {
        nDays = Math.Min(nDays, getDaysUntilNextGame(clb, dtStart, iIgnoreGameType));
      }

      return nDays;
    }

    public static Task<TableTestGames[]> GetTestgames(CornerkickManager.User _usr)
    {
      List<TableTestGames> ltDtTestgames = new List<TableTestGames>();

      if (_usr == null) return Task.FromResult(ltDtTestgames.ToArray());

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return Task.FromResult(ltDtTestgames.ToArray());

      List<Testgame> ltTestgames = new List<Testgame>();
      foreach (CornerkickManager.Cup cup in ckMng.ltCups) {
        if (cup.iId == -iCupIdTestgame) {
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
              if (gd.team[1].iTeamId == clb.iId) {
                Testgame tg = new Testgame();
                tg.dt = md.dt;
                tg.iTeamHome = gd.team[0].iTeamId;
                tg.iTeamAway = gd.team[1].iTeamId;
                ltTestgames.Add(tg);
              }
            }
          }
        }
      }

      foreach (Testgame tg in ltTestgames) {
        TableTestGames dtTestgames = new TableTestGames();

        dtTestgames.dt = tg.dt;
        //dtTestgames.sDateIso = tg.dt.ToString("yyyy-MM-ddTHH:mm");

        CornerkickManager.Club? clbH = ckMng.ltClubs.Find(c => c.iId == tg.iTeamHome);
        CornerkickManager.Club? clbA = ckMng.ltClubs.Find(c => c.iId == tg.iTeamAway);
        if (clbH != null) dtTestgames.sTeamH = clbH.sName;
        if (clbA != null) dtTestgames.sTeamA = clbA.sName;

        ltDtTestgames.Add(dtTestgames);
      }

      return Task.FromResult(ltDtTestgames.ToArray());
    }

    public static float[]? GetGradesLastGame(CornerkickManager.Club clb)
    {
      if (clb?.ltPlayer == null) return null;

      float[] fGradesLastGame = new float[clb.ltPlayer.Count];

      CornerkickGame.Game.Data gdLastGame = ckMng.tl.getNextGame(clb, ckMng.dtDatum, bPre: true);
      for (int iP = 0; iP < clb.ltPlayer.Count; iP++) {
        CornerkickManager.Player pl = clb.ltPlayer[iP];
        if (gdLastGame != null && gdLastGame.iGameType == pl.plGame.statGame.iGameType && (gdLastGame.iMatchday == 0 || gdLastGame.iMatchday == pl.plGame.statGame.iMatchday) && pl.plGame.statGame.iStat[29] > 0) {
          fGradesLastGame[iP] = pl.plGame.statGame.iStat[29] * 0.1f;
        }
      }

      return fGradesLastGame;
    }

    public static bool addMeeting(CornerkickManager.User usr, int iPlayerId, int iMtgType, DateTime dtStart, out string sReturn)
    {
      sReturn = "Error";

      if (usr == null) return false;

      CornerkickManager.Player? plMtg = ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (plMtg == null) {
        sReturn = "Player not found!";
        return false;
      }

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb != null) {
        int iCompareDates = MemberController.compareDates(clb, dtStart, new TimeSpan(0, 30, 0));
        if (iCompareDates < 0) {
          if      (iCompareDates == -1) sReturn = "Das ausgewählte Datum liegt in der Vergangenheit.";
          else if (iCompareDates == -2) sReturn = "Keine Gespräche während des Spieltages möglich.";
          else if (iCompareDates == -3) sReturn = "Keine Aktion während der Nachtruhe möglich.";
          else                          sReturn = "Sie haben bereits einen anderen Termin an diesem Datum.";
          return false;
        }
      }

      if (MemberController.checkForPlayerMeetings(usr, dtStart)) {
        sReturn = "Sie haben bereits ein Gespräch an diesem Termin!";
        return false;
      }

      CornerkickManager.User.Meeting mtg = new CornerkickManager.User.Meeting() {
        dt = dtStart,
        player = plMtg,
        iType = iMtgType
      };
      if (usr.ltMeetings == null) usr.ltMeetings = new List<CornerkickManager.User.Meeting>();
      usr.ltMeetings.Add(mtg);

      sReturn = "Einzelgespräch mit " + plMtg.plGame.sName + " vereinbart.";

      return true;
    }

  }
}
