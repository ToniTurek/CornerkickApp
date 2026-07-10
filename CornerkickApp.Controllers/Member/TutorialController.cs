using CornerkickApp.Shared.Models;
using Microsoft.AspNetCore.Components;
using static CornerkickApp.Shared.Models.CkAppShared;

namespace CornerkickApp.Controllers.Member
{
  public class TutorialController
  {
    public const byte iTtMainLevelStart = 0;
    public const byte iTtMainLevelTeam = 1;
    public const byte iTtMainLevelPlayer = 2;
    public const byte iTtMainLevelUser = 3;
    public const byte iTtMainLevelTraining = 4;
    public const byte iTtMainLevelCalendar = 5;
    public const byte iTtMainLevelTactic = 6;
    public const byte iTtMainLevelEnd = 7;
    public const byte iTtMainLevelPart2Start = 8;
    public const byte iTtMainLevelPart2End = 8;
    public const byte iTtMainLevelPart3Start = 10;
    public const byte iTtMainLevelContracts = 11;
    public const byte iTtMainLevelPart3End = 12;
    public const byte iTtMainLevelPart4Start = 13;
    public const byte iTtMainLevelStaff = 14;
    public const byte iTtMainLevelSpec = 15;
    public const byte iTtMainLevelSponsors = 16;
    public const byte iTtMainLevelMerch = 17;
    public const byte iTtMainLevelStSurr = 18;
    public const byte iTtMainLevelPart4End = 12;

    public static readonly byte[] iMainLevelsPart1 = [
      iTtMainLevelStart,
      iTtMainLevelTeam,
      iTtMainLevelPlayer,
      iTtMainLevelUser,
      iTtMainLevelTraining,
      iTtMainLevelCalendar,
      iTtMainLevelTactic,
      iTtMainLevelEnd
    ];

    public static readonly byte[] iMainLevelsPart2 = [
      iTtMainLevelPart2Start
    ];

    public static readonly byte[] iMainLevelsPart3 = [
      iTtMainLevelPart3Start,
      iTtMainLevelContracts
    ];

    public static readonly byte[] iMainLevelsPart4 = [
      iTtMainLevelPart4Start,
      iTtMainLevelStaff,
      iTtMainLevelSponsors,
      iTtMainLevelMerch,
      iTtMainLevelStSurr
    ];

    public class Tutorial
    {
      public bool bShow;
      public int iLevel;
    }
    //public static Tutorial[] ttUser; // User tutorial

    public static int GetNbOfSubLevels(int iMainLevel)
    {
      int nSubLevels = 0;
      while (get_level_text(new CornerkickManager.User(), (iMainLevel * 10) + nSubLevels, false) != null) {
        nSubLevels++;
      }
      return nSubLevels;
    }

    public static int GetNbOfLevels(int iLevel)
    {
      int nLevels = 0;

      byte[] iMainLevels = GetMainLevels(iLevel);

      foreach (int iMainLevel in iMainLevels) {
        nLevels += GetNbOfSubLevels(iMainLevel);
      }
      return nLevels;
    }

    public static byte[] GetMainLevels(int iLevel)
    {
      int iPart = GetPart(iLevel);

      byte[] iMainLevels = [];
      if      (iPart == 1) iMainLevels = iMainLevelsPart1;
      else if (iPart == 2) iMainLevels = iMainLevelsPart2;
      else if (iPart == 3) iMainLevels = iMainLevelsPart3;
      else if (iPart == 4) iMainLevels = iMainLevelsPart4;

      return iMainLevels;
    }
    
    public static int GetPart(int iLevel)
    {
      int iPart = 4;
      if      (iLevel <= iTtMainLevelEnd      * 10) iPart = 1;
      else if (iLevel <= iTtMainLevelPart2End * 10) iPart = 2;
      else if (iLevel <= iTtMainLevelPart3End * 10) iPart = 3;
      else if (iLevel <= iTtMainLevelPart4End * 10) iPart = 4;

      return iPart;
    }

    public static bool IsLastSubLevel(int iLevel, int iMainLevel)
    {
      return iLevel == (iMainLevel * 10) + GetNbOfSubLevels(iMainLevel) - 1;
    }

    public static void initialiteTutorial()
    {
      // Set length of tutorial class array
      ttUser = new CkAppShared.Tutorial[ckMng.ltUser.Count];
      for (int iU = 0; iU < ttUser.Length; iU++) {
        // Initialize tutorial class
        ttUser[iU] = new CkAppShared.Tutorial() { bShow = true, iLevel = 0 };

        // Get user info
        CornerkickManager.User usr = ckMng.ltUser[iU];

        if (usr.lti != null) {
          if (usr.lti.Count > UserOptionsModel.iUserOptionsIxTutorialShow) ttUser[iU].bShow = usr.lti[UserOptionsModel.iUserOptionsIxTutorialShow] > 0;
          if (usr.lti.Count > UserOptionsModel.iUserOptionsIxTutorialLevel) ttUser[iU].iLevel = usr.lti[UserOptionsModel.iUserOptionsIxTutorialLevel];
        }
      }
    }

    public class TutorialReturn
    {
      public string sText = "";
      public string sHeader = "";
      public string sButtonNextText { get; set; } = "weiter";
      public string sButtonBackText { get; set; } = "zurück";
      public string sButton3Text { get; set; } = "";
      public int iStep { get; set; }
      public int nSteps { get; set; }
    }

    public static TutorialReturn getTutorial(CornerkickManager.User usr, int iLevel, bool bScouting)
    {
      TutorialReturn ttr = new TutorialReturn();
      LevelReturn? ttlr = null;

      while ((ttlr = get_level_text(usr, iLevel, bScouting)) == null) {
        if (iLevel == iTtMainLevelEnd * 10) break;
        if (iLevel == iTtMainLevelPart2End * 10) break;
        if (iLevel == iTtMainLevelPart3End * 10) break;

        iLevel++;
      }

      /*
      if (ttRef) {
        ttRef.iLevel = iLevel;
      }
      */

      int jLevel = 0;
      byte[] iMainLevels = GetMainLevels(iLevel);
      foreach (int iMainLevel in iMainLevels) {
        if      (iLevel / 10 >  iMainLevel) jLevel += GetNbOfSubLevels(iMainLevel);
        else if (iLevel / 10 == iMainLevel) jLevel += iLevel - (iMainLevel * 10);
      }

      if (ttlr != null) {
        ttr.sText = ttlr.sText;
        ttr.sHeader = ttlr.sHeader;
        ttr.iStep = jLevel + 1;
        ttr.nSteps = GetNbOfLevels(iLevel);

        //ckDialog.actionOnOk = () => rmWarningAndContinueCalendar(iRet, cr);
        if (iLevel == 0) {
          ttr.sButtonBackText = "";
          ttr.sButton3Text = "nicht mehr anzeigen";
        } else {
          ttr.sButton3Text = "von Vorne";
          /*
          class: "btnDialog btn btn-outline-secondary",
            tabIndex: -1,
            icon: "ui-icon-arrowreturn-1-e",
            click: function () {
              setLevel(true, 0, function() { window.open('/Member/Desk', '_self', false); });
            }
          });
          */
          ttr.sButtonBackText = "zurück";
          /*
             class: "btnDialog btn btn-outline-secondary",
             tabIndex: -1,
             icon: "ui-icon-arrow-1-w",
             click: function () {
               const iMainLevelCur = parseInt(iLevel / 10);

               iLevel = iLevel - 1;
               while (!get_level_text(iLevel).text && iLevel > 0) {
                 iLevel = iLevel - 1;
               }

               if (parseInt(iLevel / 10) < iMainLevelCur) {
                 setLevel(true, iLevel, function () { navigateToSite(iLevel) } );
               } else {
                 $(this).dialog('destroy').remove();
                 setLevel(true, iLevel, function () { drawTutorial2(parent, iLevel, ttRef, elsHighlight, bWebApp, bScouting); });
               }
             }
           });
           */
        }

        if (ttlr.iNextButton > 0) {
          ttr.sButtonNextText = "überspringen";
          string sNextButtonIcon = "";
          if (ttlr.iNextButton == 1) {
            ttr.sButtonNextText = "weiter";
            sNextButtonIcon = "ui-icon-arrow-1-e";
          } else if (ttlr.iNextButton == 3) {
            ttr.sButtonNextText = "schließen";
            sNextButtonIcon = "ui-icon-arrow-1-e";
          }

          /*
          buttons.push({
            text: sNextButtonText,
            icon: sNextButtonIcon,
            class: "btnDialog btn btn-outline-primary",
            tabIndex: -1,
            id: "bnNext",
            click: function () {
              if (iNextButton < 3) {
                setLevel(
                  true,
                  iLevel + 1,
                  function () {
                    if (iLevel === (iTtMainLevelTeam * 10) + 0) {
                      let elem = document.getElementById("tablediv");
                      elem.scrollIntoView({ left: 0, block: 'start', behavior: 'smooth' });
                      //e.preventDefault();

                      setTimeout(function () { drawTutorial2(parent, iLevel + 1, ttRef, elsHighlight, bWebApp, bScouting); }, 1000);
                    } else {
                      drawTutorial2(parent, iLevel + 1, ttRef, elsHighlight, bWebApp, bScouting);
                    }
                  }
                );
              }
              $(this).dialog('destroy').remove();
            }
          });
          */
        } else {
          ttr.sButtonNextText = "";
        }
      }

      return ttr;
    }

    [Inject]
    public static NavigationManager MyNavigationManager { get; set; } = default!;
    public static bool setTutorialLvl(CornerkickManager.User usr, bool bShow, int iLevel, Action? action = null)
    {
      if (usr == null) return false;

      if (ttUser != null) {
        int iUserIx = ckMng.ltUser.IndexOf(usr);
        if (iUserIx >= 0 && iUserIx < ttUser.Length) {
          ttUser[iUserIx].bShow = bShow;
          ttUser[iUserIx].iLevel = iLevel;
        }
      }

      if (usr.lti == null) usr.lti = new List<int>();
      while (usr.lti.Count <= Math.Max(UserOptionsModel.iUserOptionsIxTutorialShow, UserOptionsModel.iUserOptionsIxTutorialLevel)) usr.lti.Add(0);

      usr.lti[UserOptionsModel.iUserOptionsIxTutorialShow] = bShow ? iLevel / 1000 + 1 : 0;
      usr.lti[UserOptionsModel.iUserOptionsIxTutorialLevel] = iLevel;

      if (action != null) action();
      //if (navmng != null) navmng.NavigateTo(navmng.Uri, forceLoad: true);

      return true;
    }

    private class LevelReturn
    {
      public string sText = "";
      public string sHeader = "";
      public byte iNextButton = 1; // Default: next
    }

    private static LevelReturn? get_level_text(CornerkickManager.User usr, int iLevel, bool bScouting)
    {
      LevelReturn lr = new LevelReturn();

      if (iLevel <= iTtMainLevelEnd * 10) {
        lr.sHeader += "Teil 1 - ";
      } else if (iLevel <= iTtMainLevelPart2End * 10) {
        lr.sHeader += "Teil 2 - ";
      } else if (iLevel <= iTtMainLevelPart3End * 10) {
        lr.sHeader += "Teil 3 - ";
      }

      // Header
      if ((iLevel / 10) == iTtMainLevelStart) {
        lr.sHeader += "Einleitung";
      } else if ((iLevel / 10) == iTtMainLevelTeam) {
        lr.sHeader += "Aufstellung";
      } else if ((iLevel / 10) == iTtMainLevelPlayer) {
        lr.sHeader += "Spieler";
      } else if ((iLevel / 10) == iTtMainLevelUser) {
        lr.sHeader += "Persönliches";
      } else if ((iLevel / 10) == iTtMainLevelTraining) {
        lr.sHeader += "Training";
      } else if ((iLevel / 10) == iTtMainLevelStaff) {
        lr.sHeader += "Personal";
      } else if ((iLevel / 10) == iTtMainLevelCalendar) {
        lr.sHeader += "Saisonvorbereitung";
      } else if ((iLevel / 10) == iTtMainLevelTactic) {
        lr.sHeader += "Taktik";
      } else if ((iLevel / 10) == iTtMainLevelSponsors) {
        lr.sHeader += "Sponsoren";
      } else if ((iLevel / 10) == iTtMainLevelMerch) {
        lr.sHeader += "Merchandising";
      } else if ((iLevel / 10) == iTtMainLevelStSurr) {
        lr.sHeader += "Vereinsgelände";
      } else if ((iLevel / 10) == iTtMainLevelPart2Start) {
        lr.sHeader += "Start";
      } else if ((iLevel / 10) == iTtMainLevelContracts) {
        lr.sHeader += "Spielerverträge";
      } else {
        lr.sHeader += "Ende";
      }

      int iSubLevel = 0;
      if (iLevel == (iTtMainLevelStart * 10) + iSubLevel++) {
        lr.sText  = "Willkommen zu unserem Treffen " + usr.sFirstname + " " + usr.sSurname + " und schön, dass du Zeit hast.";
        lr.sText += "<br><br>Aufgrund anhaltender Erfolglosigkeit mussten wir die Zusammenarbeit mit unserem bisherigen Cheftrainer leider beenden.";
        lr.sText += " Bis wir einen neuen Trainer gefunden haben würden wir uns freuen, wenn du die Mannschaft interimsweise für eine kurze Zeit übernimmst. Wir glauben das könnte eine große Chance für dich sein!";
        lr.sText += "<br>Wir machen jetzt einen kleinen Rundgang und ich zeige dir alles.";
        lr.sText += "<br><br>Zum Fortfahren, klicke auf den button \"weiter\"";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelStart * 10) + iSubLevel++) {
        lr.sText = "Diese Seite stellt das Hauptmenü von CORNERKICK dar. Über die Menüleiste oben kannst du auf die verschiedenen Seiten navigieren.<br><br>Mit einem Klick auf die Eckfahne <img src=\"" + sContentDir + "/Images/logo_64.png\" alt=\"logo\" style=\"width: 16px\"> oben links kommst du immer wieder hierhin zurück.";
        lr.iNextButton = 1;
        return lr;
#if _WebApp
      } else if (iLevel == (iTtMainLevelStart * 10) + iSubLevel++) {
        lr.sText = "Ist das online Spiel gestartet, wird alle 2 Minuten ein Zeitschritt durchgeführt, welcher 15 Minuten im Spiel vergehen lässt. Hierdurch vergeht eine Woche im Spiel genau innerhalb eines Tages.";
        lr.sText += "<br><br>Ligaspiele Samstags um 15:30 Uhr finden somit immer Abends um 20:30 Uhr statt.";
        lr.iNextButton = 1;
        return lr;
#endif
      } else if (iLevel == (iTtMainLevelStart * 10) + iSubLevel++) {
        lr.sText = "Gibt es dringende Aufgaben zu erledigen, erscheinen hierfür Warnmeldungen im oberen Bereich dieser Seite.<br><br>Im Laufe der Einführung sollten sie nach und nach weniger werden.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelStart * 10) + iSubLevel++) {
        lr.sText = "Als Erstes wollen wir uns mal deine Mannschaft ansehen. Klicke dazu im Menü oben auf \"<i class=\"fas fa-users\"></i> Mannschaft -> <img src=\"" + sContentDir + "/Icons/menu/pitch_black.png\" width=\"13\" height=\"18\"/> Aufstellung\" (oder auf den Link " + getScrollToElement("Durchschnittsstärke (Startelf)", "aDeskStrength") + " unter dem Abschnitt \"Aktuelle Lage\" auf dieser Seite).";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelTeam * 10) + 0) {
        lr.sText = "Sehr gut!";
        lr.sText += "<br><br>Auf der unteren Hälfe des Fussballfeldes befindet sich deine aktuell aufgestellte Mannschaft. Sobald der nächste Gegner feststeht (und du einen Spielbeobachter eingestellt hast), siehst du die Aufstellung deines Gegners in der oberen Hälfte des Spielfeldes.";
        lr.sText += "<br>Die Position deiner Spieler auf dem Spielfeld kannst du verändern, indem du sie einfach mit gedrückter Maustaste (mobil: lange drücken) verschiebst oder einen Spieler markierst und die orangenen Pfeile anklickst. Eine andere Standardformation kannst du auch über das Dropdown-Menü über der Spielerliste auswählen.";
        lr.sText += "<br><br>Klicke jetzt auf \"weiter\"";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelTeam * 10) + 1) {
        lr.sText = getScrollToElement("Unter bzw. neben", "team_table") + " dem Fussballfeld ist deine Mannschaft aufgelistet. Die Spieler der Startformation sind grün, die Einwechselspieler blau hinterlegt. Alle weiteren Spieler gehören nicht dem Kader für das nächste Spiel an.";
        lr.sText += "<br><br>Klicke auf \"weiter\"";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelTeam * 10) + 2) {
        lr.sText = "Unter \"Pos.\" ist die aktuelle Position auf dem Spielfeld und unter \"HP\" die tatsächliche Hauptposition des Spielers angegeben.";
        lr.sText += "<br>Die bunten Pfeile unter \"Form\" geben die aktuelle Tagesform des Spielers an. Diese ändert sich jeden Tag und lässt sich nicht beeinflussen.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelTeam * 10) + 3) {
        lr.sText = "Klicke nun auf einen Spieler in der Liste aber nicht auf den Namen. Dieser wird markiert. Wenn du nun einen anderen Spieler anklickst, werden sie getauscht.";
        lr.sText += "<br><br>Probiere das mal aus!";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelTeam * 10) + 4) {
        lr.sText = "Ausgezeichnet!";
        lr.sText += "<br><br>Wenn du auf den button " + getScrollToElement("Auto", "bnAutoFormation") + " oberhalb der Spielerliste klickst, wird immer die aktuell beste Mannschaft aufgestellt (sofern du weißt, welche Spieler die besten sind. Dazu später mehr).";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelTeam * 10) + 5) {
        lr.sText = "Als nächstes schauen wir uns mal einen deiner Spieler genauer an. Klicke hierzu auf den Namen eines Spielers in der Aufstellungsliste.";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelPlayer * 10) + 0) {
        string sTextInsert = usr.iResp < iUserRespPlayerStatistic ? "\"Überblick\" und \"Fähigkeiten\"" : "\"Überblick\", \"Fähigkeiten\" und \"Statistik\"";
        lr.sText = "Gut gemacht. Hier findest du alle Informationen über den Spieler in den Menüs " + sTextInsert + ".";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelPlayer * 10) + 1) {
        
        //lr.sText = "Im Menü \"Überblick\" hast du im Abschnitt <a href=\"" + getNextPage(iLevel, usr) + "/#header_options\">Optionen</a> verschiedene Aktionsmöglichkeiten. Zunächst kannst du hier deinen Spieler zu deinem Kapitän oder Vize-Kapitän ernennen. Später im Spiel kannst auch du mit dem Spieler Vertragsgespräche führen oder ihn auf die Transferliste setzen.";
        lr.sText = "Im Menü \"Überblick\" hast du im Abschnitt " + getScrollToElement("Optionen", "header_options") + " verschiedene Aktionsmöglichkeiten. Zunächst kannst du hier deinen Spieler zu deinem Kapitän oder Vize-Kapitän ernennen. Später im Spiel kannst auch du mit dem Spieler Vertragsgespräche führen oder ihn auf die Transferliste setzen.";
        lr.sText += "<br><br>Kapitäne sollten die Spieler mit der höchsten Führungspersönlichkeit (FP) sein.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelPlayer * 10) + 2) {
        lr.sText = "Klicke jetzt auf den Reiter " + getScrollToElement("Fähigkeiten", "div_top") + ".";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelPlayer * 10) + 3) {
        lr.sText = "Im ersten Abschnitt " + getScrollToElement("Positionen", "divPosSkillContainer") + " siehst du, welche relative und absolute Stärke dein Spieler auf allen Positionen hat (Spalte \"Wert\" bzw. \"Stärke\").";
        lr.sText += "<br><br>Setzt du einen Spieler auf einer Position ein, auf der er noch nicht einen Wert von 100% erreicht hat, erlernt er diese Position abhängig von seiner Charaktereigenschaft \"Flexibilität\" (s. Menü \"Überblick\").";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelPlayer * 10) + 4) {
        lr.sText = "Unter der Überschrift " + getScrollToElement("Individuelle Fähigkeiten", "divPlayerSkills") + " sind die einzelnen Fähigkeitswerte deines Spielers (Spalte \"Wert\") aufgelistet, welche jeweils einer Kategorie (\"Kat.\") zugeordnet sind. Aus ihnen sowie der Moral und der Frische deines Spielers setzt sich seine aktuelle Gesamtstärke zusammen.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelPlayer * 10) + 5) {
        lr.sText = "Da du deine Mannschaft gerade erst übernommen hast, hattest du noch keine Möglichkeit deine Spieler kennenzulernen. Daher sind dir ihre Fähigkeiten auch noch nicht bekannt und werden als \"?\" dargestellt.";
        lr.sText += "<br><br>Jeden Tag um 12:00 Uhr sowie bei Spielen deiner Mannschaft erhältst du neue Eindrücke von den Fähigkeiten deiner Spieler und die Fragezeichen werden nach und nach durch Zahlen ersetzt. Allerdings sind deine Beobachtungsfähigkeiten zu Beginn des Spiels noch nicht sehr gut, so dass die Daten nicht unbedingt korrekt sein müssen.";
        lr.iNextButton = 1;
        return lr;
      /*
      } else if (iLevel == (iTtMainLevelPlayer * 10) + 6) {
        lr.sText = "Die individuellen Fähigkeiten kannst du als Trainer auf zwei Arten beeinflussen:";
        lr.sText += "<br><ul><li>Zum Einen wird durch Mannschaftstraining wie z.B. \"Torschüsse\" der Trainingswert der Fähigkeit (in diesem Fall \"Schusskraft\" und \"Schussgenauigkeit\") verbessert.</li>";
        lr.sText += "<li>Zum Anderen kann ein Spieler auch eine Fähigkeit individuell trainieren. Welche das ist, kannst du über die Knöpfe unter \"Ind.\" festlegen. Wie wahrscheinlich sich dein Spieler in dieser Fähigkeit verbessert, hängt u.A. von seinem Talent (\"Tal.\") in der jeweiligen Kategorie ab, sowie von seiner Spielpraxis. Aber auch mit jedem Mannschaftstraining (sowie durch Aktionen im Spiel) steigt sein Bonus in dieser Fähigkeit an, wodurch seine Chancen ebenfalls erhöht werden, sich in dieser um ein Level zu verbessern.</li></ul>";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelPlayer * 10) + 7) {
        lr.sText = "Manche Fähigkeiten wie \"Schnelligkeit\" sind dabei schwerer zu erlernen als andere (wie z.B. \"Zweikampf\"). In der Graphik unter \"Entwicklungschancen\" sind die einzelnen Faktoren sowie die resultierende Wahrscheinlichkeit einer Verbesserung pro Tag dargestellt.";
        lr.iNextButton = 1;
        return lr;
      */
      } else if (iLevel == (iTtMainLevelPlayer * 10) + 6) {
        lr.sText = "Jetzt wollen wir uns mal deine ganz persönlichen Fähigkeiten genauer anschauen. Gehe ins Menü \"<i class=\"fas fa-desktop\"></i> Büro -> <i class=\"fas fa-circle-user\"></i> Persönliches\".";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelUser * 10) + 0) {
        lr.sText = "Im " + getScrollToElement("oberen Teil", "divUserRespContainer") + " findest du zunächst einmal deine aktuelle Stellung im Verein. ";
        lr.sText += "Wenn du dich gut schlägst, wirst du vielleicht irgendwann befördert und erhälst die Kontrolle über weitere Verantwortungsbereiche!";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelUser * 10) + 1) {
        lr.sText = getScrollToElement("Darunter", "divUserSkillsContainer") + " befindet sich eine Übersicht über deine persönlichen Fähigkeiten als Fussball-Manager.";
        lr.sText += "<br><br>Mit jedem Sieg - abhängig vom <a href=\"/usermanual/#h2Cups\" target=\"_blank\"><u>Attraktionsfaktor</u></a> des Wettbewerbs - gewinnst du Fähigkeitspunkte. Ist die Menge an Fähigkeitspunkten >= 1, kannst du sie einsetzen um deine persönlichen Fähigkeiten zu verbessern.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelUser * 10) + 2) {
        lr.sText = "Zu Beginn des Spiels hast du bereits Fähigkeitspunkte zur Verfügung. Besonders wichtig ist es, deine Spieler gut einschätzen zu können um z.B. die beste Mannschaft aufstellen zu können.";
        lr.sText += "<br><br>Erhöhe daher deine Fähigkeiten der \"Spielereinschätzung\" bis auf das Level 3, indem du in der entsprechenden Zeile zweimal auf das grüne Plus klickst.";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelUser * 10) + 3) {
        lr.sText = "Gut. Verteile jetzt noch den restlichen Fähigkeitspunkt auf \"Trainingsgestaltung\".";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelUser * 10) + 4) {
        lr.sText = "Als nächstes wollen wir mal das Training für die nächste Woche festlegen. Klicke dazu im Menü oben auf \"<i class=\"fas fa-users\"></i> Mannschaft -> <i class=\"fas fa-dumbbell\"></i> Mannschaftstraining\".";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelTraining * 10) + 0) {
        lr.sText = "Du kannst pro Tag maximal drei Trainingseinheiten festlegen, es sein denn, während einer Trainingszeit findet ein Spiel oder ein Ereignis statt.";
        lr.sText += "<br><br>Unter der Überschrift " + getScrollToElement("Trainingsplan", "header_training_plan") + " kannst du für jede Einheit individuell festlegen welche Trainingsart durchgeführt wird.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelTraining * 10) + 1) { // 17
        lr.sText = "Unter " + getScrollToElement("Vorlagen Trainingswoche", "training_templates") + " stehen auch vorgefertigte Trainingspläne für eine Woche zur Verfügung.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelTraining * 10) + 2) {
        lr.sText = "Zu Beginn der Saison solltest du erst einmal die Kondition deiner Spieler trainieren. Mit hoher Kondition verlieren deine Spieler weniger Frische im Spiel oder Training.";
        lr.sText += "<br><br>Klicke jetzt auf die Vorlage \"Kondition\".";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelTraining * 10) + 3) {
        lr.sText = "Gehe jetzt " + getScrollToElement("zur nächsten Woche", "aNextWeek") + ".";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelTraining * 10) + 4) {
        lr.sText = "Setze ebenfalls das Training auf \"Kondition\".";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelTraining * 10) + 5) {
        lr.sText = "Gehe dann noch eine Woche weiter.";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelTraining * 10) + 6) {
        lr.sText = "An Spieltagen solltest du nur Regeneration trainieren, damit die Frische deiner Spieler beim Spiel möglichst hoch ist. Diese wirkt sich nämlich direkt auf die Fähigkeiten der Spieler aus.";
        lr.sText += "<br><br>Setze das Training für diese Woche auf \"Ausgeglichen\".";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelTraining * 10) + 7) {
        lr.sText = "Klicke jetzt auf \"Aktuelle Trainingswoche bis Saisonende fortführen\".";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelTraining * 10) + 8) {
        /*
        if (usr.iResp < iUserRespStaff) {
        */
        lr.sText = "Als nächstes wollen wir zur Vorbereitung auf die Saison ein Testspiel durchführen. Dies, sowie weitere Ereignisse wie Gespräche mit deinen Spielern, die Buchung von Trainingslager, Weihnachtsfeiern usw. können über den Kalender geplant werden.";
        lr.sText += "<br><br>Diesen kannst du über \"<i class=\"fas fa-calendar-day\"></i> Saison -> <i class=\"fas fa-calendar-day\"></i> Kalender\" oder durch einen klick auf das aktuelle Datum oben links erreichen.";
        /*
        } else {
          lr.sText = "Um effektives Training durchführen zu können, brauchst du Personal wie z.B. einen Co-Trainer, einen Konditionstrainer und einen Masseur.";
          lr.sText += "<br><br>Um Personal einzustellen, gehe in das Menü \"Verein->Personal\".";
        }
        */
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelCalendar * 10) + 0) {
        if (usr.iResp < iUserRespTrainingCamps && usr.club?.ltCamp != null && usr.club.ltCamp.Count > 0) {
          lr.sText = "Wie du an dem orangenen Eintrag sehen kannst, wurde von deinem Vorgänger bereits ein Trainingslager vom " + usr.club.ltCamp[0].dtDeparture.ToShortDateString() + " bis zum " + usr.club.ltCamp[0].dtReturn.ToShortDateString() + " gebucht.<br><br>";
        }
        lr.sText += "Um ein Testspiel zu planen, klicke einfach in ein freies Feld im Kalender, welches sich in der Zukunft befindet. Wähle z.B. den nächsten Sonntag um 14:00 Uhr aus. Anschließend wählst du die Option \"Testspiel vereinbaren\" und suchst dir einen gewünschten Testspielgegner aus. Ist es ein Computerverein, bekommst du sofort eine Antwort, andernfalls bekommt der Manager des von dir ausgewählten Vereins eine Anfrage.";
        lr.sText += "<br><br>Testspielanfragen an dich findest du unter dem Kalender. Dort kannst du sie entweder bestätigen oder ablehnen.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelCalendar * 10) + 1 && usr.iResp >= iUserRespTrainingCamps) {
        lr.sText = "Trainingslager oder Ereignisse durchzuführen, kann sehr vorteilhaft sein. Für eine genaue Beschreibung klicke <a href=\"/usermanual/#h2Calendar\" target=\"_blank\"><u>hier</u></a>.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelCalendar * 10) + (usr.iResp < iUserRespTrainingCamps ? 1 : 2)) {
        lr.sText = "Ein Schlüssel zum Erfolg ist neben einer guten Vorbereitung auf das Spiel auch die Taktik. Um diese einzustellen, gehe jetzt zu \"<i class=\"fas fa-users\"></i> Mannschaft -> <i class=\"fas fa-sliders\"></i> Taktik\".";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelTactic * 10) + 0) {
        lr.sText = "Hier siehst du das Taktikboard. Lass dich von seiner Komplexität nicht abschrecken. Die wichtigsten Einstellungen sind die offensive Ausrichtung (orangener Schieber) und der Einsatz (magentafarbener Schieber) deiner Spieler.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelTactic * 10) + 1) {
        lr.sText = "Beginne ein Spiel nicht mit zu viel Einsatz, da deine Spieler ansonsten zu schnell zu viel Frische verlieren. Am besten, du lässt den Einsatz erstmal in der mittleren Position. Liegst du in der Schlussphase eines Spiels hinten, lohnt es sich mit mehr Einsatz zu spielen.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelTactic * 10) + 2) {
        lr.sText = getScrollToElement("Ganz unten", "divTcSubs") + " kannst du deinem Co-Trainer bereits vor dem Spiel mitteilen, welche Auswechslungen er für dich durchführen soll. Hast du also mal keine Zeit während eines Spiels dabei zu sein, kannst du trotzdem Frische Kräfte bringen. Dies kann unabhängig des Spielstandes sinnvoll sein.";
        lr.sText += "<br><br>Eine Beschreibung der weiteren Taktikeinstellungen findest du <a href=\"/usermanual/#h3Tactic\" target=\"_blank\"><u>hier</u></a>.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelTactic * 10) + 3) {
        //lr.sText = "Um deine Spieler sowie dein Personal, den Ausbau deines Stadions oder einen neuen Starstürmer bezahlen zu können, brauchst du Geld. Ein Weg, an frisches Kapital zu kommen sind Sponsoren.";
        //lr.sText += "<br><br>Gehe jetzt ins Menü \"Verein->Sponsoren\".";
        lr.sText = "Wenn du hier fertig bist, gehe zurück ins Hauptmenü <img src=\"" + sContentDir + "/Images/logo_64.png\" alt=\"logo\" style=\"width: 16px\">.";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelEnd * 10) + 0) { // End 1st part
        /*
        lr.sText = "Dies ist das Ende des 1. Teils des Tutorials. Abschließend solltest du noch folgendes tun:";
        lr.sText += "<ul>";
        lr.sText += "<li>Dich auf dem Transfermarkt nach interessanten Spielern umsehen (\"Büro->Transfermarkt\")</li>";
        lr.sText += "<li>Dein Stadion und dein Vereinsgelände ausbauen (\"Verein->Stadion\" und \"Verein->Vereinsgelände\")</li>";
        lr.sText += "<li>Dir einen Merchandising-Vermarkter zulegen (\"Verein->Merchandising\")</li>";
        lr.sText += "</ul>";
        lr.sText += "<br>Für alles weitere, schau dir die <a href=\"/usermanual\" target=\"_blank\">Anleitung</a> an oder stell deine Frage an <a href=\"mailto:mail@cornerkick-manager.de?subject=Cornerkick Frage\">mail@cornerkick-manager.de</a>. Viel Erfolg!";
        lr.sText += "<br><br>Du kannst dieses Tutorial später noch einmal starten, indem du oben rechts auf deine e-mail klickst und anschließend unter \"Optionen\" den Haken bei \"Tutorial zeigen\" setzt.";
        lr.iNextButton = 0;
        */
        lr.sText = "Dies ist das Ende des 1. Teils des Tutorials.";
#if !_WebApp
        lr.sText += " Starte nun den Zeitablauf in dem du oben rechts auf \"weiter <i class=\"fa fa-play\"></i>\" klickst.";
#endif
        lr.sText += "<br><br>Für alles weitere, schau dir die <a href=\"/usermanual\" target=\"_blank\">Anleitung</a> an oder stell deine Frage an <a href=\"mailto:mail@cornerkick-manager.de?subject=Cornerkick Frage\">mail@cornerkick-manager.de</a>.";
        lr.sText += "<br><br>Weitere Infos über das Spiel und zur Community findest du <a href=\"/info\">hier</a>.";
        //lr.sText += "<br><br>Du kannst dieses Tutorial später noch einmal starten, indem du oben rechts auf deine e-mail klickst und anschließend unter \"Optionen\" den Haken bei \"Tutorial zeigen\" setzt.";

        //setLevel(false, 999);

        lr.iNextButton = 3;
        return lr;
        /*
         * End 1st part, start second part with first game
         */
      } else if (iLevel == (iTtMainLevelPart2Start * 10) + 0) { // Start 2st part
        lr.sText = "Es ist soweit, dein erstes Spiel steht an! Du solltest jetzt noch einmal deine Aufstellung überprüfen (und eventuell mit \"Auto\" deine beste Mannschaft aufstellen) sowie deinen Kapitän und Vizekapitän bestimmen.";
        lr.sText += "<br><br>Wenn du soweit bist, klicke oben rechts auf \"zum Spiel <i class=\"fa fa-futbol\"></i>\". Viel Erfolg!";
        lr.iNextButton = 3;
        return lr;
        /*
         * End 2nd part, start 3rd part with player contracts
         */
      } else if (iLevel == (iTtMainLevelPart3Start * 10) + 0) {
        lr.sText = "Zu Beginn des Spiels wurden alle deine Spieler mit Verträgen unterschiedlicher Laufzeit ausgestattet. Möglicherweise haben manche deiner Spieler nur einen Vertrag bis zum Ende dieser Saison. Ab der Rückrunde können Spieler mit auslaufenden Verträgen ablösefrei für die nächste Saison verpflichtet werden. Es ist also wichtig, dass du die Verträge von Spielern, die du gerne halten würdest, rechtzeitig verlängerst.";
        lr.sText += "<br><br>Gehe jetzt ins Menü \"<i class=\"fas fa-users\"></i> Mannschaft -> <i class=\"fas fa-address-card\"></i> Verträge\".";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelContracts * 10) + 0) { // 3nd part: Player contracts
        lr.sText = "Auf dieser Seite siehst du eine Tabelle deiner Spieler mit Informationen über ihre Verträge. Unter \"Lz [a]\" steht deren Laufzeit in Jahren. Klickst du auf \"Lz [a]\", wird die Liste nach der Laufzeit sortiert.";
        lr.sText += "<br><br>Falls es Spieler gibt, deren Laufzeit nur noch 1 Jahr beträgt, bedeutet es, dass deren Verträge am Saisonende ausläuft. Verträge mit Spielern kannst du prinzipiell auf zwei verschiedene Arten verlängern.";
        lr.sText += " Bei deinem aktuellen Status als " + CornerkickManager.Names.sUserRespLvlNames[usr.iResp] + " erledigt zunächst die Personalabteilung die Vertragsdetails.";
        lr.sText += "<br><br>Klicke jetzt auf \"weiter\"";
        lr.iNextButton = 1;
        return lr;
        /*
      } else if (iLevel == (iTtMainLevelContracts * 10) + 1) {
        lr.sText = "Eine Möglichkeit ist, mit jedem Spieler einen neuen Vertrag auszuhandeln, in dem du auf der Spielerdetailseite (klick auf den Namen) unter \"Optionen\" auf die Schaltfläche \"Vertrag verlängern\" klickst. So kannst du jeden Vertrag individuell verhandeln.";
        lr.sText += "<br><br>--> \"weiter\"";
        lr.iNextButton = 1;
        return lr;
        */
      } else if (iLevel == (iTtMainLevelContracts * 10) + 1) {
        lr.sText = "Teile einfach der Personalabteilung mit, mit welchem Spieler du den Vertrag verlängern möchtest, in dem du auf dieser Seite in der entsprechenden Zeile auf die Schaltfläche unter \"Blitzverh.\" klickst. Hierdurch wird der Vertrag dieses Spielers automatisch zu neuen Konditionen um ein Jahr verlängert.";
        lr.sText += "<br><br>Dabei wird jeweils die Zufriedenheit des Spielers mit dem Vertrag erzielt, welche in der Schaltfläche steht. Je weiter du dein Verhandlungsgeschick erhöhst (\"Büro->Persönliches\"), desto niedriger wird dieser Wert und entsprechend günstiger der Vertrag.";
        lr.sText += "<br><br>Bei Verträgen, welche über die Blitzverhandlung verlängert wurden, sind keine speziellen Vertragdetails wie fixe Ablösen oder Platzierungsboni enthalten.";
        lr.iNextButton = 3;
        return lr;
        /*
      } else if (iLevel == (iTtMainLevelContracts * 10) + 2) {
        lr.sText = "Um deine Sündhaft teuren \"Superstars\" besser finanzieren zu können, könntest du deinen Fans tiefer in die Tasche greifen und deine Zuschauereinnahmen optimieren.";
        lr.sText += "<br><br>Gehe daher jetzt ins Menü \"Verein->Finanzen\" (oder klick auf dein Guthaben oben rechts).";
        lr.iNextButton = 0;
        return lr;
        */
        /*
         * End 3rd part, start 4th part with staff, sponsors and calendar
         */
#if _WebApp
      } else if (iLevel == (iTtMainLevelContracts * 10) + 2) {
        lr.sText = "Eine weitere Einnahmequelle für deinen Verein stellen Kooperationen mit anderen Managern dar. Über den Link im Abschnitt \"Kooperationen\" kannst du über eine vorgefertigte Email Freunde zu Cornerkick einladen. Ihnen wird dann automatisch dein Verein als Kooperationspartner vorgeschlagen.";
        lr.sText += "<br><br>Hast du bereits Kooperationen geschlossen, werden sie in der Tabelle aufgelistet. Hast du jemanden eingeladen, bist du der Mutterverein und partizipierst monatlich abhängig vom Erfolg deines Tochtervereins an diesem. Aber auch wenn du eingeladen wurdest und somit der Tochterverein bist, erhälst du durch Synergieeffekte monatlich eine bestimmte Summe abhängig vom Erfolg deines Muttervereins (siehe auch Infos in der <a href=\"/usermanual/#h4UserCoop\" target=\"_blank\"><u>Anleitung</u></a>).";
        lr.iNextButton = 1;
        return lr;
#endif
      } else if (iLevel == (iTtMainLevelPart4Start * 10) + 0) { // Start 3st part
        lr.sText = "Herzlich Willkommen zum 4. Teil des CORNERKICK Tutorials.";
        lr.sText += "<br><br>Du solltest jetzt schon einige Erfahrungen gesammelt haben. In diesem Teil wollen wir uns ein paar weitere Aspekte des Spiels anschauen.";
        lr.sText += "<br><br>Zum Fortfahren, klicke auf den button \"weiter\"";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelStaff * 10) + 0 && usr.iResp >= iUserRespStaff) {
        lr.sText = "Ein Co-Trainer hilft dir bei der Entwicklung der individuellen Fähigkeiten deiner Spieler. Konditionstrainer und Masseur erhöhen die Effektivität von Konditions- und Regenerationstraining.";
        lr.sText += "<br><br>Wähle jetzt dein Personal aus und klicke auf den button \"Personal einstellen\" (grünes Plus).<br>ACHTUNG: Um dein Personal anschließend zu ändern, musst du dem bisherigen Trainer eine Abfindung in Höhe des halben Jahresgehalts zahlen.";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelStaff * 10) + 1 && usr.iResp >= iUserRespStaff) {
        lr.sText = "Hervorragend!<br><br>Zum Beobachten von fremden Spielern musst du Scouts einstellen (Reiter: \"Scoutingabteilung\"). Je höher ihr Wert ist, desto geringer ist das Risiko, dass sie bei einer Beobachtung die Fähigkeit des Spielers falsch einschätzen.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelStaff * 10) + 2 && usr.iResp >= iUserRespStaff) {
        lr.sText = "Hast du verletzte Spieler, kannst du deren Heilung mit (mehr oder weniger begabten) Ärzten beschleunigen (Reiter: \"Medizinische Abteilung\"). Achte dabei auch auf die Art der Verletzung deines Spielers und die Fähigkeit des Arztes in diesem Bereich. Hast du einen Arzt eingestellt, kannst du ihm einen Patienten zuweisen.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelStaff * 10) + 3 && usr.iResp >= iUserRespStaff) {
        lr.sText = "Als nächstes wollen wir zur Vorbereitung auf die Saison ein Testspiel durchführen. Dies, sowie weitere Ereignisse wie Trainingslager, Weihnachtsfeiern usw. können über den Kalender geplant werden.";
        lr.sText += "<br><br>Diesen kannst du über \"Saison->Kalender\" oder durch einen klick auf das aktuelle Datum oben links erreichen.";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelSponsors * 10) + 0 && usr.iResp >= iUserRespFinance) {
        lr.sText = "Im Menü \"Hauptsponsor\" kannst du deinen Haupt- bzw. Trikotsponsor auswählen.";
        lr.sText += "<br><br>Zu Beginn des Spiels hast du bereits zwei Angebote. Wähle jetzt das zweite Angebot aus und klicke auf \"verhandeln\".";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelSponsors * 10) + 1 && usr.iResp >= iUserRespFinance) {
        lr.sText = "Klicke im Verhandlungsmenü einmal auf \"verhandeln\".";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelSponsors * 10) + 2 && usr.iResp >= iUserRespFinance) {
        lr.sText = "Klicke jetzt im Verhandlungsmenü auf \"zurück\" und nimm eines der beiden Angebote an in dem du es in der Liste markierst und auf \"Sponsor wählen\" klickst.";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelSponsors * 10) + 3 && usr.iResp >= iUserRespFinance) {
        lr.sText = "Im Laufe der Zeit kommen noch weitere Angebote dazu. Für die nächste Saison musst du dann einen neuen Sponsor wählen.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelSponsors * 10) + 4 && usr.iResp >= iUserRespFinance) {
        lr.sText = "Im Menü \"Banden\" hast du die Möglichkeit, insgesamt 12 Banden an Sponsoren zu vermieten. Im Laufe der Zeit bekommst du auch hier neue Angebote. Es lohnt sich also, auf dieser Seite hin und wieder mal vorbei zu schauen.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelSponsors * 10) + 5 && usr.iResp >= iUserRespFinance) {
        lr.sText = "Zu guter Letzt kannst du im Menü \"Spezialsponsoren\" noch für jeweils eine Saison zwei Spezialsponsoren wählen.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelSponsors * 10) + 6 && usr.iResp >= iUserRespFinance) {
        lr.sText = "Eine weitere Einnahmequelle ist der Verkauf von Merchandisingartikeln deines Vereins.";
        lr.sText += "<br><br>Gehe jetzt ins Menü \"Verein->Merchandising\".";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelMerch * 10) + 0 && usr.iResp >= iUserRespMerchandising) {
        lr.sText = "Prinzipiell hast du die Wahl, entweder einen Merchandisingvermarkter zu beauftragen oder deine Merchandisingverkäufe selbst zu vermarkten.";
        lr.sText += "<br><br>Die Selbstvermarktung deiner Merchandisingartikel bringt dir in der Regel mehr Geld ein als das, was der Vermarkter dir zu zahlen bereit ist. Allerdings ist es aufwendiger und du musst zuerst auf dem Vereinsgelände mindestens einen Fanshop gebaut haben.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelMerch * 10) + 1 && usr.iResp >= iUserRespMerchandising) {
        lr.sText = "Klicke jetzt auf die Schaltfläche \"annehmen\" um den Vermarkter zu engagieren.";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelMerch * 10) + 2 && usr.iResp >= iUserRespMerchandising) {
        lr.sText = "Nun hast du schon einigen finanziellen Spielraum. Schau doch mal auf dem Vereinsgelände nach, ob du das ein- oder andere Gebäude bauen bzw. erweitern möchtest.";
        lr.sText += "<br><br>Gehe jetzt ins Menü \"Verein->Vereinsgelände\".";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelStSurr * 10) + 0 && usr.iResp >= iUserRespStadium) {
        lr.sText = "Investitionen in Gebäude sind eine gute Gelegenheit, deinen Verein langfristig zum Erfolg zu führen, da sie dir - einmal gebaut - dauerhaft Vorteile bringen.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelStSurr * 10) + 1 && usr.iResp >= iUserRespStadium) {
        lr.sText = "Zu Beginn des Spiels hast du bereits einige Gebäude auf einer geringen Ausbaustufe. Für andere Gebäudetypen benötigst du jedoch erstmal ein zusätzliches Gelände um darauf dein Gebäude bauen zu können.";
        lr.sText += "<br><br>Klicke jetzt auf die Schaltfläche \"Grundstück kaufen\".";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelStSurr * 10) + 2 && usr.iResp >= iUserRespStadium) {
        lr.sText = "Wähle den Gebäudetyp \"Fitnessraum\" aus und klicke auf \"Ausbau zu: Fitnessräder\".";
        lr.iNextButton = 2;
        return lr;
      } else if (iLevel == (iTtMainLevelStSurr * 10) + 3 && usr.iResp >= iUserRespStadium) {
        lr.sText = "Gut gemacht. Überlege dir, ob du noch andere Gebäude bauen bzw. erweitern möchtest. Auf manchen Geländefeldern siehst du in der oberen rechten Ecke ein Warnsymbol. Wenn du mit der Maus darüber gehst, erscheint zusätzliche Information über die minimale Ausstattung, welche nicht zu Beeinträchtigungen z.B. beim Zuschaueraufkommen führt.";
        lr.sText += "<br><br>Für weitere Informationen, was genau die einzelnen Gebäude bewirken, schaue in die <a href=\"/usermanual/#h3Buildings\" target=\"_blank\"><u>Anleitung</u></a>.";
        lr.sText += "<br><br>Wenn du hier fertig bist, gehe zurück ins Hauptmenü.";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelSpec * 10) + 0) {
        lr.sText = "Auf dieser Seite erhälst du einen Überblick, wie es finanziell um deinen Verein gestellt ist.";
        lr.sText += "<br><br>Klicke jetzt auf den Reiter \"Zuschauer\", um die Eintrittspreise festzulegen.";
        lr.iNextButton = 0;
        return lr;
      } else if (iLevel == (iTtMainLevelSpec * 10) + 1) {
        lr.sText = "Dein Stadion bietet maximal drei Kategorien an: Steh- und Sitzplätze sowie Logen (V.I.P.).";
        lr.sText += "<br><br>Für jede dieser Kategorien gibt es ein Potential an Zuschauern, wobei bei ausverkaufter Kategorie ein geringer Anteil in eine andere Kategorie wechselt. Gibt es z.B. keine Stehplätze mehr, kaufen sich manche Zuschauer eine Sitzplatzkarte (allerdings eher keine Loge). Andersherum kauft sich ein kleiner Teil der Zuschauer, welcher einen Sitzplatz haben wollte eine Stehplatzkarte, ist kein Sitzplatz mehr verfügbar (und manche sogar eine Loge).";
        lr.sText += "<br><br>Am besten für dich ist es jedoch, wenn du jede Kategorie anbietest und möglichst genau so viel für eine Karte verlangst, wie Zuschauer bereit sind dafür zu zahlen.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelSpec * 10) + 2) {
        lr.sText = "In der Grafik unter \"Zuschauerhistorie\" siehst du als rote Linie deine Stadionkapazität sowie in grün die Zuschauer aller bisherigen Heimspiele.";
        lr.sText += "<br><br>Gehst du mit der Maus in die Grafik, erhälst du mehr Details zu jedem einzelnen Heimspiel, zum Beispiel die Anzahl der Zuschauer in den drei Kategorien.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelSpec * 10) + 3) {
        lr.sText = "Mit den Eingabefeldern neben der Zuschauergrafik kannst du die Eintrittspreise verändern.";
        lr.sText += "<br><br>Sind eine oder mehrere Kategorien immer ausverkauft, erhöhe die Preise. Bleiben viele Plätze in einer Kategorie frei, senke sie.";
        lr.iNextButton = 1;
        return lr;
      } else if (iLevel == (iTtMainLevelPart4End * 10) + 0) { // End 4th part
        lr.sText = "Dies ist das Ende des 4. Teils des Tutorials.";

        //setLevel(false, 1999);
        lr.iNextButton = 3;
        return lr;
      }

      return null;
    }

    private static string getScrollToElement(string elText, string anchor)
    {
      return "<span class=\"pseudo-link\" onclick=\"document.getElementById('" + anchor + "').scrollIntoView({ behavior: 'smooth'})\">" + elText + "</span>";
    }

    public static string getNextPage(int iLevel, CornerkickManager.User _usr)
    {
      if ((iLevel / 10) == iTtMainLevelStart) {
        return "/member/desk";
      } else if ((iLevel / 10) == iTtMainLevelTeam) {
        return "/member/team";
      } else if ((iLevel / 10) == iTtMainLevelPlayer) {
        CornerkickManager.Club? clb = MemberController.ckClub(_usr);
        if (clb?.ltPlayer == null) return "";
        if (clb.ltPlayer.Count < 1) return "";
        return "/member/playerdetails/" + clb.ltPlayer[0].plGame.iId.ToString();
      } else if ((iLevel / 10) == iTtMainLevelUser) {
        return "/member/user";
      } else if ((iLevel / 10) == iTtMainLevelTraining) {
        return "/member/training";
      } else if ((iLevel / 10) == iTtMainLevelStaff) {
        return "/member/staff";
      } else if ((iLevel / 10) == iTtMainLevelCalendar) {
        return "/member/calendar";
      } else if ((iLevel / 10) == iTtMainLevelTactic) {
        return "/member/tactic";
      } else if ((iLevel / 10) == iTtMainLevelSponsors) {
        return "/member/sponsor";
      } else if ((iLevel / 10) == iTtMainLevelMerch) {
        return "/member/merchandising";
      } else if ((iLevel / 10) == iTtMainLevelStSurr) {
        return "/member/buildings";
      } else if ((iLevel / 10) == iTtMainLevelContracts) {
        return "/member/contracts";
      } else if ((iLevel / 10) == iTtMainLevelSpec) {
        return "/member/finance";
      }

      return "";
    }
  }
}
