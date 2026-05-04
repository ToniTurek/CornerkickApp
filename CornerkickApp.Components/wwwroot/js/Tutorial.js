const iTtMainLevelStart    =  0;
const iTtMainLevelTeam     =  1;
const iTtMainLevelPlayer   =  2;
const iTtMainLevelUser     =  3;
const iTtMainLevelTraining =  4;
const iTtMainLevelStaff    =  5;
const iTtMainLevelCalendar =  6;
const iTtMainLevelTactic   =  7;
const iTtMainLevelSponsors =  8;
const iTtMainLevelMerch    =  9;
const iTtMainLevelStSurr   = 10;
const iTtMainLevelEnd      = 11;
const iTtMainLevelPart2Start = 100;
const iTtMainLevelContracts  = 101;
const iTtMainLevelSpec       = 102;
const iTtMainLevelPart2End   = 103;

const tt_highlight_className = "tt_highlight";
const tt_highlight_el_className = "tt_highlight_el";
function Tt_highlight(level, el, force_class=false) {
  this.level = level;
  this.el = el;
  this.force_class = force_class;
}

function drawTutorial(parent, iLevel, ttRef, elsHighlight, bWebApp, bScouting) {
  setTimeout(function () { drawTutorial2(parent, iLevel, ttRef, elsHighlight, bWebApp, bScouting); }, 500);
}

function drawTutorial2(parent, iLevel, ttRef, elsHighlight, bWebApp, bScouting) {
  while (!get_level_text(iLevel, bWebApp, bScouting).text) {
    if (iLevel == iTtMainLevelEnd * 10) { break; }
    if (iLevel == iTtMainLevelPart2End * 10) { break; }

    iLevel = iLevel + 1;
  }

  if (ttRef) {
    ttRef.iLevel = iLevel;
  }

  add_tt_highlight_class(elsHighlight, iLevel);

  // Text
  const txt_obj = get_level_text(iLevel, bWebApp, bScouting);
  const sText = txt_obj.text;
  const sHeader = txt_obj.header;
  const iNextButton = txt_obj.next_button;

  /*
  if (iLevel > 999) {
    setLevel(false, 0);
  }
  */

  drawTutorialDialog(parent, iLevel, sText, sHeader, iNextButton, ttRef, elsHighlight, bWebApp, bScouting);
}

function get_level_text(iLevel, bWebApp, bScouting) {
  var sText;
  var sHeader = "";
  var iNextButton = 1; // Default: next

  if (parseInt(iLevel) <= iTtMainLevelEnd * 10) {
    sHeader += "Teil 1 - ";
  } else if (parseInt(iLevel) <= iTtMainLevelPart2End * 10) {
    sHeader += "Teil 2 - ";
  }

  // Header
  if (parseInt(iLevel / 10) === iTtMainLevelStart) {
    sHeader += "Einleitung";
  } else if (parseInt(iLevel / 10) === iTtMainLevelTeam) {
    sHeader += "Aufstellung";
  } else if (parseInt(iLevel / 10) === iTtMainLevelPlayer) {
    sHeader += "Spieler";
  } else if (parseInt(iLevel / 10) === iTtMainLevelUser) {
    sHeader += "Persönliches";
  } else if (parseInt(iLevel / 10) === iTtMainLevelTraining) {
    sHeader += "Training";
  } else if (parseInt(iLevel / 10) === iTtMainLevelStaff) {
    sHeader += "Personal";
  } else if (parseInt(iLevel / 10) === iTtMainLevelCalendar) {
    sHeader += "Saisonvorbereitung";
  } else if (parseInt(iLevel / 10) === iTtMainLevelTactic) {
    sHeader += "Taktik";
  } else if (parseInt(iLevel / 10) === iTtMainLevelSponsors) {
    sHeader += "Sponsoren";
  } else if (parseInt(iLevel / 10) === iTtMainLevelMerch) {
    sHeader += "Merchandising";
  } else if (parseInt(iLevel / 10) === iTtMainLevelStSurr) {
    sHeader += "Vereinsgelände";
  } else if (parseInt(iLevel / 10) === iTtMainLevelPart2Start) {
    sHeader += "Start";
  } else if (parseInt(iLevel / 10) === iTtMainLevelContracts) {
    sHeader += "Spielerverträge";
  } else {
    sHeader += "Ende";
  }

  if (iLevel === (iTtMainLevelStart * 10) + 0) {
    sText = "Hallo und willkommen beim CORNERKICK Fußball-Manager!</br></br>Dies ist ein Tutorial, welches dir die grundlegenden Kenntnisse von CORNERKICK vermitteln soll.</br>Für detailliertere Informationen besuche bitte die <a href=\"/Home/UserManual\">Anleitung</a>.";
    sText += "</br></br>Zum Fortfahren, klicke auf den button \"weiter\"";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelStart * 10) + 1) {
    sText = "Diese Seite stellt das Hauptmenü von CORNERKICK dar. Über die Menüleiste oben kannst du auf die verschiedenen Seiten navigieren.</br></br>Mit einem Klick auf die Eckfahne oben links kommst du immer wieder hierhin zurück.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelStart * 10) + 2) {
    sText = "Ist das Spiel gestartet, wird alle 2 Minuten ein Zeitschritt durchgeführt, welcher 15 Minuten im Spiel vergehen lässt. Hierdurch vergeht eine Woche im Spiel genau innerhalb eines Tages.";
    sText += "</br></br>Ligaspiele der ersten Ligen Samstags um 15:30 Uhr finden somit immer Abends um 20:30 Uhr statt.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelStart * 10) + 3) {
    sText = "Gibt es dringende Aufgaben zu erledigen, erscheinen hierfür Warnmeldungen im oberen Bereich dieser Seite.</br></br>Im Laufe des Tutorials sollten sie nach und nach weniger werden.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelStart * 10) + 4) {
    sText = "Als Erstes wollen wir uns mal deine Mannschaft ansehen. Klicke dazu im Menü oben auf \"Mannschaft->Aufstellung\" (oder auf den Link \"Durchschnittsstärke (Startelf)\" unter dem Abschnitt \"Aktuelle Lage\" auf dieser Seite).";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelTeam * 10) + 0) {
    sText = "Sehr gut!";
    sText += "</br></br>Auf der unteren Hälfe des Fussballfeldes befindet sich deine aktuell aufgestellte Mannschaft. Sobald der nächste Gegner feststeht (und du einen Spielbeobachter eingestellt hast), siehst du die Aufstellung deines Gegners in der oberen Hälfte des Spielfeldes.";
    sText += "</br></br>Die Position deiner Spieler auf dem Spielfeld kannst du verändern, indem du sie mit der Maus verschiebst oder einen Spieler markierst und die orangenen Pfeile anklickst. Eine andere Standardformation kannst du auch über das Dropdown-Menü über der Spielerliste auswählen.";
    sText += "</br></br>Klicke jetzt auf \"weiter\"";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelTeam * 10) + 1) {
    sText = "Unter bzw. neben dem Fussballfeld ist deine Mannschaft aufgelistet. Die ersten elf Spieler sind in der Startformation (grün hinterlegt). Die nächsten sieben stehen als Einwechselspieler beim nächsten Spiel zur Verfügung (blau hinterlegt). Alle weiteren Spieler gehören nicht dem Kader für das nächste Spiel an.";
    sText += "</br></br>Klicke auf \"weiter\"";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelTeam * 10) + 2) {
    sText = "Klicke auf einen der ersten 11 Spieler in der Liste aber nicht auf den Namen. Dieser wird markiert. Wenn du nun einen anderen Spieler anklickst, werden sie getauscht.";
    sText += "</br></br>Probiere das mal aus!";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelTeam * 10) + 3) {
    sText = "Ausgezeichnet!";
    sText += "</br></br>Wenn du auf den button \"Auto\" oberhalb der Spielerliste klickst, wird immer die aktuell beste Mannschaft aufgestellt (sofern du weißt, welche Spieler die besten sind).";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelTeam * 10) + 4) {
    sText = "Als nächstes schauen wir uns mal einen deiner Spieler genauer an. Klicke hierzu auf den Namen eines Spielers in der Aufstellungsliste.";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelPlayer * 10) + 0) {
    sText = "Gut gemacht. Hier findest du alle Informationen über den Spieler in den drei Menüs \"Überblick\", \"Fähigkeiten\" und \"Statistik\".";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelPlayer * 10) + 1) {
    sText = "Im Menü \"Überblick\" hast du im Abschnitt \"Optionen\" verschiedene Aktionsmöglichkeiten. Zum Beispiel kannst du den Spieler auf die Transferliste setzen, oder zu deinem Kapitän oder Vize-Kapitän ernennen.";
    sText += "</br></br>Kapitäne sollten die Spieler mit der höchsten Führungspersönlichkeit (FP) sein.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelPlayer * 10) + 2) {
    sText = "Klicke jetzt auf den Reiter \"Fähigkeiten\".";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelPlayer * 10) + 3) {
    sText = "Im ersten Abschnitt \"Positionen\" siehst du, welche relative und absolute Stärke dein Spieler auf allen Positionen hat (Spalte \"Wert\" bzw. \"Stärke\").";
    sText += "</br></br>Setzt du einen Spieler auf einer Position ein, auf der er noch nicht einen Wert von 100% erreicht hat, erlernt er diese Position abhängig von seiner Charaktereigenschaft \"Flexibilität\" (s. Menü \"Überblick\").";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelPlayer * 10) + 4) {
    sText = "Unter der Überschrift \"Individuelle Fähigkeiten\" sind die einzelnen Fähigkeitswerte deines Spielers (Spalte \"Wert\") aufgelistet, welche jeweils einer Kategorie (\"Kat.\") zugeordnet sind. Aus ihnen sowie der Moral und der Frische deines Spielers setzt sich seine aktuelle Gesamtstärke zusammen.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelPlayer * 10) + 5) {
    sText = "Da du deine Mannschaft gerade erst übernommen hast, hattest du noch keine Möglichkeit deine Spieler kennenzulernen. Daher sind dir ihre Fähigkeiten auch noch nicht bekannt und werden als \"?\" dargestellt.";
    sText += "</br></br>Jeden Tag um 12:00 Uhr erhältst du neue Eindrücke von den Fähigkeiten deiner Spieler und die Fragezeichen werden nach und nach durch Zahlen ersetzt. Allerdings sind deine Beobachtungsfähigkeiten zu Beginn des Spiels noch nicht sehr gut, so dass die Daten nicht unbedingt korrekt sein müssen.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelPlayer * 10) + 6) {
    sText = "Die individuellen Fähigkeiten kannst du als Trainer auf zwei Arten beeinflussen:";
    sText += "</br><ul><li>Zum Einen wird durch Mannschaftstraining wie z.B. \"Torschüsse\" der Trainingswert der Fähigkeit (in diesem Fall \"Schusskraft\" und \"Schussgenauigkeit\") verbessert.</li>";
    sText += "<li>Zum Anderen kann ein Spieler auch eine Fähigkeit individuell trainieren. Welche das ist, kannst du über die Knöpfe unter \"Ind.\" festlegen. Wie wahrscheinlich sich dein Spieler in dieser Fähigkeit verbessert, hängt u.A. von seinem Talent (\"Tal.\") in der jeweiligen Kategorie ab, sowie von seiner Spielpraxis. Aber auch mit jedem Mannschaftstraining (sowie durch Aktionen im Spiel) steigt sein Bonus in dieser Fähigkeit an, wodurch seine Chancen ebenfalls erhöht werden, sich in dieser um ein Level zu verbessern.</li></ul>";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelPlayer * 10) + 7) {
    sText = "Manche Fähigkeiten wie \"Schnelligkeit\" sind dabei schwerer zu erlernen als andere (wie z.B. \"Zweikampf\"). In der Graphik unter \"Entwicklungschancen\" sind die einzelnen Faktoren sowie die resultierende Wahrscheinlichkeit einer Verbesserung pro Tag dargestellt.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelPlayer * 10) + 8) {
    sText = "Jetzt wollen wir uns mal deine ganz persönlichen Fähigkeiten genauer anschauen. Gehe ins Menü \"Büro->Persönliches\".";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelUser * 10) + 0) {
    sText = "Im oberen Teil findest du zunächst einmal eine Übersicht über deine persönlichen Fähigkeiten als Fussball-Manager.";
    sText += "</br></br>Mit jedem Sieg - abhängig vom <a href=\"/Home/UserManual/#h2Cups\" target=\"_blank\"><u>Attraktionsfaktor</u></a> des Wettbewerbs - gewinnst du Fähigkeitspunkte. Ist die Menge an Fähigkeitspunkten >= 1, kannst du sie einsetzen um deine persönlichen Fähigkeiten zu verbessern.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelUser * 10) + 1) {
    sText = "Zu Beginn des Spiels hast du bereits Fähigkeitspunkte zur Verfügung. Besonders wichtig ist es, deine Spieler gut einschätzen zu können um z.B. die beste Mannschaft aufstellen zu können.";
    sText += "</br></br>Erhöhe daher deine Fähigkeiten der \"Spielereinschätzung\" bis auf das Level 3, indem du in der entsprechenden Zeile zweimal auf das grüne Plus klickst.";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelUser * 10) + 2) {
    sText = "Gut. Verteile jetzt noch die restlichen Fähigkeitspunkte auf \"Trainingsgestaltung\" und \"Verhandlungsgeschick\".";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelUser * 10) + 3 && bWebApp) {
    sText = "Eine weitere Einnahmequelle für deinen Verein stellen Kooperationen mit anderen Managern dar. Über den Link im Abschnitt \"Kooperationen\" kannst du über eine vorgefertigte Email Freunde zu Cornerkick einladen. Ihnen wird dann automatisch dein Verein als Kooperationspartner vorgeschlagen.";
    sText += "</br></br>Hast du bereits Kooperationen geschlossen, werden sie in der Tabelle aufgelistet. Hast du jemanden eingeladen, bist du der Mutterverein und partizipierst monatlich abhängig vom Erfolg deines Tochtervereins an diesem. Aber auch wenn du eingeladen wurdest und somit der Tochterverein bist, erhälst du durch Synergieeffekte monatlich eine bestimmte Summe abhängig vom Erfolg deines Muttervereins (siehe auch Infos in der <a href=\"/Home/UserManual/#h4UserCoop\" target=\"_blank\"><u>Anleitung</u></a>).";
    iNextButton = 1;
  } else if ((iLevel === (iTtMainLevelUser * 10) + 3 && !bWebApp) || (iLevel === (iTtMainLevelUser * 10) + 4 && bWebApp)) {
    sText = "Als nächstes wollen wir mal das Training für die nächste Woche festlegen. Klicke dazu im Menü oben auf \"Mannschaft->Mannschaftstraining\".";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelTraining * 10) + 0) {
    sText = "Du kannst pro Tag maximal drei Trainingseinheiten festlegen, es sein denn, während einer Trainingszeit findet ein Spiel oder ein Ereignis statt.";
    sText += "</br></br>Unter der Überschrift \"Trainingsplan\" kannst du für jede Einheit individuell festlegen welche Trainingsart durchgeführt wird.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelTraining * 10) + 1) { // 17
    sText = "Unter \"Vorlagen Trainingswoche\" stehen auch vorgefertigte Trainingspläne für eine Woche zur Verfügung.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelTraining * 10) + 2) {
    sText = "Zu Beginn der Saison solltest du erst einmal die Kondition deiner Spieler trainieren. Mit hoher Kondition verlieren deine Spieler weniger Frische im Spiel oder Training.";
    sText += "</br></br>Klicke jetzt auf die Vorlage \"Kondition\".";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelTraining * 10) + 3) {
    sText = "Gehe jetzt zur nächsten Woche.";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelTraining * 10) + 4) {
    sText = "Setze ebenfalls das Training auf \"Kondition\".";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelTraining * 10) + 5) {
    sText = "Gehe dann noch eine Woche weiter.";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelTraining * 10) + 6) {
    sText = "An Spieltagen solltest du nur Regeneration trainieren, damit die Frische deiner Spieler beim Spiel möglichst hoch ist. Diese wirkt sich nämlich direkt auf die Fähigkeiten der Spieler aus.";
    sText += "</br></br>Setze das Training für diese Woche auf \"Ausgeglichen\".";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelTraining * 10) + 7) {
    sText = "Klicke jetzt auf \"Aktuelle Trainingswoche bis Saisonende fortführen\" und dann auf \"Bestätigen\".";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelTraining * 10) + 8) {
    sText = "Um effektives Training durchführen zu können, brauchst du Personal wie z.B. einen Co-Trainer, einen Konditionstrainer und einen Masseur.";
    sText += "</br></br>Um Personal einzustellen, gehe in das Menü \"Verein->Personal\".";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelStaff * 10) + 0) {
    sText = "Ein Co-Trainer hilft dir bei der Entwicklung der individuellen Fähigkeiten deiner Spieler. Konditionstrainer und Masseur erhöhen die Effektivität von Konditions- und Regenerationstraining.";
    sText += "</br></br>Wähle jetzt dein Personal aus und klicke auf den button \"Personal einstellen\" (grünes Plus).</br>ACHTUNG: Um dein Personal anschließend zu ändern, musst du dem bisherigen Trainer eine Abfindung in Höhe des halben Jahresgehalts zahlen.";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelStaff * 10) + 1) {
    sText = "Hervorragend!</br></br>Zum Beobachten von fremden Spielern musst du Scouts einstellen (Reiter: \"Scoutingabteilung\"). Je höher ihr Wert ist, desto geringer ist das Risiko, dass sie bei einer Beobachtung die Fähigkeit des Spielers falsch einschätzen.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelStaff * 10) + 2) {
    sText = "Hast du verletzte Spieler, kannst du deren Heilung mit (mehr oder weniger begabten) Ärzten beschleunigen (Reiter: \"Medizinische Abteilung\"). Achte dabei auch auf die Art der Verletzung deines Spielers und die Fähigkeit des Arztes in diesem Bereich. Hast du einen Arzt eingestellt, kannst du ihm einen Patienten zuweisen.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelStaff * 10) + 3) {
    sText = "Als nächstes wollen wir zur Vorbereitung auf die Saison ein Testspiel durchführen. Dies, sowie weitere Ereignisse wie Trainingslager, Weihnachtsfeiern usw. können über den Kalender geplant werden.";
    sText += "</br></br>Diesen kannst du über \"Saison->Kalender\" oder durch einen klick auf das aktuelle Datum oben links erreichen.";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelCalendar * 10) + 0) {
    sText = "Um ein Testspiel zu planen, klicke einfach in ein freies Feld im Kalender, welches sich in der Zukunft befindet. Wähle z.B. den nächsten Samstag um 14:00 Uhr. Anschließend wählst du die Option \"Testspiel vereinbaren\" und suchst dir einen gewünschten Testspielgegner aus. Ist es ein Computerverein, bekommst du sofort eine Antwort, andernfalls bekommt der Manager des von dir ausgewählten Vereins eine Anfrage.";
    sText += "</br></br>Testspielanfragen an dich findest du unter dem Kalender. Dort kannst du sie entweder bestätigen oder ablehnen.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelCalendar * 10) + 1) {
    sText = "Trainingslager oder Ereignisse durchzuführen, kann sehr vorteilhaft sein. Für eine genaue Beschreibung klicke <a href=\"/Home/UserManual/#h2Calendar\" target=\"_blank\"><u>hier</u></a>.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelCalendar * 10) + 2) {
    sText = "Ein Schlüssel zum Erfolg ist neben einer guten Vorbereitung auf das Spiel auch die Taktik. Um diese einzustellen, gehe jetzt zu \"Mannschaft->Taktik\".";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelTactic * 10) + 0) {
    sText = "Hier siehst du das Taktikboard. Lass dich von seiner Komplexität nicht abschrecken. Die wichtigsten Einstellungen sind die offensive Ausrichtung (orangener Schieber) und der Einsatz (magentafarbener Schieber) deiner Spieler.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelTactic * 10) + 1) {
    sText = "Beginne ein Spiel nicht mit zu viel Einsatz, da deine Spieler ansonsten zu schnell zu viel Frische verlieren. Am besten, du lässt den Einsatz erstmal in der mittleren Position. Liegst du in der Schlussphase eines Spiels hinten, lohnt es sich mit mehr Einsatz zu spielen.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelTactic * 10) + 2) {
    sText = "Ganz unten kannst du deinem Co-Trainer bereits vor dem Spiel mitteilen, welche Auswechslungen er für dich durchführen soll. Hast du also mal keine Zeit während eines Spiels dabei zu sein, kannst du trotzdem Frische Kräfte bringen. Dies kann unabhängig des Spielstandes sinnvoll sein.";
    sText += "</br></br>Eine Beschreibung der weiteren Taktikeinstellungen findest du <a href=\"/Home/UserManual/#h3Tactic\" target=\"_blank\"><u>hier</u></a>.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelTactic * 10) + 3) {
    sText = "Um deine Spieler sowie dein Personal, den Ausbau deines Stadions oder einen neuen Starstürmer bezahlen zu können, brauchst du Geld. Ein Weg, an frisches Kapital zu kommen sind Sponsoren.";
    sText += "</br></br>Gehe jetzt ins Menü \"Verein->Sponsoren\".";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelSponsors * 10) + 0) {
    sText = "Im Menü \"Hauptsponsor\" kannst du deinen Haupt- bzw. Trikotsponsor auswählen.";
    sText += "</br></br>Zu Beginn des Spiels hast du bereits zwei Angebote. Wähle jetzt das zweite Angebot aus und klicke auf \"verhandeln\".";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelSponsors * 10) + 1) {
    sText = "Klicke im Verhandlungsmenü einmal auf \"verhandeln\".";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelSponsors * 10) + 2) {
    sText = "Klicke jetzt im Verhandlungsmenü auf \"zurück\" und nimm eines der beiden Angebote an in dem du es in der Liste markierst und auf \"Sponsor wählen\" klickst.";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelSponsors * 10) + 3) {
    sText = "Im Laufe der Zeit kommen noch weitere Angebote dazu. Für die nächste Saison musst du dann einen neuen Sponsor wählen.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelSponsors * 10) + 4) {
    sText = "Im Menü \"Banden\" hast du die Möglichkeit, insgesamt 12 Banden an Sponsoren zu vermieten. Im Laufe der Zeit bekommst du auch hier neue Angebote. Es lohnt sich also, auf dieser Seite hin und wieder mal vorbei zu schauen.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelSponsors * 10) + 5) {
    sText = "Zu guter Letzt kannst du im Menü \"Spezialsponsoren\" noch für jeweils eine Saison zwei Spezialsponsoren wählen.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelSponsors * 10) + 6) {
    sText = "Eine weitere Einnahmequelle ist der Verkauf von Merchandisingartikeln deines Vereins.";
    sText += "</br></br>Gehe jetzt ins Menü \"Verein->Merchandising\".";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelMerch * 10) + 0) {
    sText = "Prinzipiell hast du die Wahl, entweder einen Merchandisingvermarkter zu beauftragen oder deine Merchandisingverkäufe selbst zu vermarkten.";
    sText += "</br></br>Die Selbstvermarktung deiner Merchandisingartikel bringt dir in der Regel mehr Geld ein als das, was der Vermarkter dir zu zahlen bereit ist. Allerdings ist es aufwendiger und du musst zuerst auf dem Vereinsgelände mindestens einen Fanshop gebaut haben.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelMerch * 10) + 1) {
    sText = "Klicke jetzt auf die Schaltfläche \"annehmen\" um den Vermarkter zu engagieren.";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelMerch * 10) + 2) {
    sText = "Nun hast du schon einigen finanziellen Spielraum. Schau doch mal auf dem Vereinsgelände nach, ob du das ein- oder andere Gebäude bauen bzw. erweitern möchtest.";
    sText += "</br></br>Gehe jetzt ins Menü \"Verein->Vereinsgelände\".";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelStSurr * 10) + 0) {
    sText = "Investitionen in Gebäude sind eine gute Gelegenheit, deinen Verein langfristig zum Erfolg zu führen, da sie dir - einmal gebaut - dauerhaft Vorteile bringen.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelStSurr * 10) + 1) {
    sText = "Zu Beginn des Spiels hast du bereits einige Gebäude auf einer geringen Ausbaustufe. Für andere Gebäudetypen benötigst du jedoch erstmal ein zusätzliches Gelände um darauf dein Gebäude bauen zu können.";
    sText += "</br></br>Klicke jetzt auf die Schaltfläche \"Grundstück kaufen\".";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelStSurr * 10) + 2) {
    sText = "Wähle den Gebäudetyp \"Fitnessraum\" aus und klicke auf \"Ausbau zu: Fitnessräder\".";
    iNextButton = 2;
  } else if (iLevel === (iTtMainLevelStSurr * 10) + 3) {
    sText = "Gut gemacht. Überlege dir, ob du noch andere Gebäude bauen bzw. erweitern möchtest. Auf manchen Geländefeldern siehst du in der oberen rechten Ecke ein Warnsymbol. Wenn du mit der Maus darüber gehst, erscheint zusätzliche Information über die minimale Ausstattung, welche nicht zu Beeinträchtigungen z.B. beim Zuschaueraufkommen führt.";
    sText += "</br></br>Für weitere Informationen, was genau die einzelnen Gebäude bewirken, schaue in die <a href=\"/Home/UserManual/#h3Buildings\" target=\"_blank\"><u>Anleitung</u></a>.";
    sText += "</br></br>Wenn du hier fertig bist, gehe zurück ins Hauptmenü.";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelEnd * 10) + 0) { // End 1st part
    /*
    sText = "Dies ist das Ende des 1. Teils des Tutorials. Abschließend solltest du noch folgendes tun:";
    sText += "<ul>";
    sText += "<li>Dich auf dem Transfermarkt nach interessanten Spielern umsehen (\"Büro->Transfermarkt\")</li>";
    sText += "<li>Dein Stadion und dein Vereinsgelände ausbauen (\"Verein->Stadion\" und \"Verein->Vereinsgelände\")</li>";
    sText += "<li>Dir einen Merchandising-Vermarkter zulegen (\"Verein->Merchandising\")</li>";
    sText += "</ul>";
    sText += "</br>Für alles weitere, schau dir die <a href=\"/Home/UserManual\" target=\"_blank\">Anleitung</a> an oder stell deine Frage an <a href=\"mailto:mail@cornerkick-manager.de?subject=Cornerkick Frage\">mail@cornerkick-manager.de</a>. Viel Erfolg!";
    sText += "</br></br>Du kannst dieses Tutorial später noch einmal starten, indem du oben rechts auf deine e-mail klickst und anschließend unter \"Optionen\" den Haken bei \"Tutorial zeigen\" setzt.";
    iNextButton = 0;
    */
    sText = "Dies ist das Ende des 1. Teils des Tutorials.";
    sText += "</br></br>Für alles weitere, schau dir die <a href=\"/Home/UserManual\" target=\"_blank\">Anleitung</a> an oder stell deine Frage an <a href=\"mailto:mail@cornerkick-manager.de?subject=Cornerkick Frage\">mail@cornerkick-manager.de</a>. Viel Erfolg!";
    sText += "</br></br>Du kannst dieses Tutorial später noch einmal starten, indem du oben rechts auf deine e-mail klickst und anschließend unter \"Optionen\" den Haken bei \"Tutorial zeigen\" setzt.";

    setLevel(false, 999);

    iNextButton = 3;
  } else if (iLevel === (iTtMainLevelPart2Start * 10) + 0) { // Start 2st part
    sText = "Herzlich Willkommen zum 2. Teil des CORNERKICK Tutorials.";
    sText += "</br></br>Du solltest jetzt schon einige Erfahrungen gesammelt haben. In diesem Teil wollen wir uns ein paar weitere Aspekte des Spiels anschauen.";
    sText += "</br></br>Zum Fortfahren, klicke auf den button \"weiter\"";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelPart2Start * 10) + 1) {
    sText = "Zu Beginn des Spiels wurden alle deine Spieler mit Verträgen unterschiedlicher Laufzeit ausgestattet. Möglicherweise haben manche deiner Spieler nur einen Vertrag bis zum Ende dieser Saison. Ab der Rückrunde können Spieler mit auslaufenden Verträgen ablösefrei für die nächste Saison verpflichtet werden. Es ist also wichtig, dass du die Verträge von Spielern, die du gerne halten würdest, rechtzeitig verlängerst.";
    sText += "</br></br>Gehe jetzt ins Menü \"Mannschaft->Verträge\".";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelContracts * 10) + 0) { // 2nd part: Player contracts
    sText = "Auf dieser Seite siehst du eine Tabelle deiner Spieler mit Informationen über ihre Verträge. Unter \"Lz [a]\" steht deren Laufzeit in Jahren. Klickst du auf \"Lz [a]\", wird die Liste nach der Laufzeit sortiert.";
    sText += "</br></br>Falls es Spieler gibt, deren Laufzeit nur noch 1 Jahr beträgt, bedeutet es, dass deren Verträge am Saisonende ausläuft. Verträge mit Spielern kannst du auf zwei verschiedene Arten verlängern.";
    sText += "</br></br>Klicke jetzt auf \"weiter\"";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelContracts * 10) + 1) {
    sText = "Eine Möglichkeit ist, mit jedem Spieler einen neuen Vertrag auszuhandeln, in dem du auf der Spielerdetailseite (klick auf den Namen) unter \"Optionen\" auf die Schaltfläche \"Vertrag verlängern\" klickst. So kannst du jeden Vertrag individuell verhandeln.";
    sText += "</br></br>--> \"weiter\"";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelContracts * 10) + 2) {
    sText = "Die andere, einfachere Möglichkeit ist, auf dieser Seite in der entsprechenden Zeile die Schaltfläche unter \"Blitzverh.\" zu klicken. Hierdurch wird der Vertrag dieses Spielers automatisch zu neuen Konditionen um ein Jahr verlängert.";
    sText += "</br></br>Dabei wird jeweils die Zufriedenheit des Spielers mit dem Vertrag erzielt, welche in der Schaltfläche steht. Je weiter du dein Verhandlungsgeschick erhöhst (\"Büro->Persönliches\"), desto niedriger wird dieser Wert und entsprechend günstiger der Vertrag.";
    sText += "</br></br>Bei Verträgen, welche über die Blitzverhandlung verlängert wurden, sind keine speziellen Vertragdetails wie fixe Ablösen oder Platzierungsboni enthalten.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelContracts * 10) + 3) {
    sText = "Um deine Sündhaft teuren \"Superstars\" besser finanzieren zu können, könntest du deinen Fans tiefer in die Tasche greifen und deine Zuschauereinnahmen optimieren.";
    sText += "</br></br>Gehe daher jetzt ins Menü \"Verein->Finanzen\" (oder klick auf dein Guthaben oben rechts).";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelSpec * 10) + 0) {
    sText = "Auf dieser Seite erhälst du einen Überblick, wie es finanziell um deinen Verein gestellt ist.";
    sText += "</br></br>Klicke jetzt auf den Reiter \"Zuschauer\", um die Eintrittspreise festzulegen.";
    iNextButton = 0;
  } else if (iLevel === (iTtMainLevelSpec * 10) + 1) {
    sText = "Dein Stadion bietet maximal drei Kategorien an: Steh- und Sitzplätze sowie Logen (V.I.P.).";
    sText += "</br></br>Für jede dieser Kategorien gibt es ein Potential an Zuschauern, wobei bei ausverkaufter Kategorie ein geringer Anteil in eine andere Kategorie wechselt. Gibt es z.B. keine Stehplätze mehr, kaufen sich manche Zuschauer eine Sitzplatzkarte (allerdings eher keine Loge). Andersherum kauft sich ein kleiner Teil der Zuschauer, welcher einen Sitzplatz haben wollte eine Stehplatzkarte, ist kein Sitzplatz mehr verfügbar (und manche sogar eine Loge).";
    sText += "</br></br>Am besten für dich ist es jedoch, wenn du jede Kategorie anbietest und möglichst genau so viel für eine Karte verlangst, wie Zuschauer bereit sind dafür zu zahlen.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelSpec * 10) + 2) {
    sText = "In der Grafik unter \"Zuschauerhistorie\" siehst du als rote Linie deine Stadionkapazität sowie in grün die Zuschauer aller bisherigen Heimspiele.";
    sText += "</br></br>Gehst du mit der Maus in die Grafik, erhälst du mehr Details zu jedem einzelnen Heimspiel, zum Beispiel die Anzahl der Zuschauer in den drei Kategorien.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelSpec * 10) + 3) {
    sText = "Mit den Eingabefeldern neben der Zuschauergrafik kannst du die Eintrittspreise verändern.";
    sText += "</br></br>Sind eine oder mehrere Kategorien immer ausverkauft, erhöhe die Preise. Bleiben viele Plätze in einer Kategorie frei, senke sie.";
    iNextButton = 1;
  } else if (iLevel === (iTtMainLevelPart2End * 10) + 0) { // End 2nd part
    sText = "Dies ist das Ende des 2. Teils des Tutorials.";

    setLevel(false, 1999);

    iNextButton = 3;
  }

  return { text: sText, header: sHeader, next_button: iNextButton };
}

function add_tt_highlight_class(tt_highlight_els, level) {
  // Remove tt class from all elements
  var els_tt_rem = document.getElementsByClassName(tt_highlight_className);
  while (els_tt_rem.length) {
    els_tt_rem[0].classList.remove(tt_highlight_className);
  }
  var els_tt_el_rem = document.getElementsByClassName(tt_highlight_el_className);
  for (var i = els_tt_el_rem.length - 1; i >= 0; i--) {
    if (els_tt_el_rem[i] && els_tt_el_rem[i].parentElement) {
      els_tt_el_rem[i].parentElement.removeChild(els_tt_el_rem[i]);
      //console.log(els_tt_rem.length + ", " + i);
    }
  }

  if (tt_highlight_els) {
    if (Array.isArray(tt_highlight_els)) {
      var el_to_highlight;
      for (var i = 0; i < tt_highlight_els.length; i++) {
        if (tt_highlight_els[i].level == level) {
          //tt_highlight_els[i].el.classList.add(tt_highlight_className);
          el_to_highlight = tt_highlight_els[i].el;
          add_tt_highlight_class_blink(el_to_highlight, tt_highlight_els[i].force_class);
        /*
        } else {
          if (el_to_highlight && el_to_highlight == tt_highlight_els[i].el) {
            continue;
          }

          tt_highlight_els[i].el.classList.remove(tt_highlight_className);
        */
        }
      }
    } else {
      if (tt_highlight_els.level == level) {
        add_tt_highlight_class_blink(tt_highlight_els.el, tt_highlight_els.force_class);
      /*
      } else {
        tt_highlight_els.el.classList.remove(tt_highlight_className);
      */
      }
    }
  }
}

function add_tt_highlight_class_blink(tt_highlight_el, force_class = false, n = 0) {
  const tt_margin = 1;

  if (tt_highlight_el == null) { return; }

  var els_tt_rem = tt_highlight_el.getElementsByClassName(tt_highlight_el_className);

  if (els_tt_rem.length > 0 || tt_highlight_el.classList.contains(tt_highlight_className)) {
    // Remove tt element
    for (var i = els_tt_rem.length - 1; i >= 0; i--) {
      if (els_tt_rem[i] && els_tt_rem[i].parentElement) {
        els_tt_rem[i].parentElement.removeChild(els_tt_rem[i]);
      }
    }
    tt_highlight_el.classList.remove(tt_highlight_className);
  } else {
    var div_tt_highlight;
    if (tt_highlight_el.offsetWidth > 0 && tt_highlight_el.offsetHeight > 0 && !force_class) {
      // Add tt element
      div_tt_highlight = document.createElement("div");
      div_tt_highlight.className = tt_highlight_el_className;
      div_tt_highlight.style.pointerEvents = "none";
      div_tt_highlight.style.border = "2px solid red";
      div_tt_highlight.style.backgroundColor = "rgba(255, 0, 0, 0.1)";
      tt_highlight_el.appendChild(div_tt_highlight);
      div_tt_highlight.style.position = "absolute";
      if (tt_highlight_el.nodeName == "TR") {
        div_tt_highlight.style.left = (tt_highlight_el.offsetLeft + tt_margin).toString() + "px";
        div_tt_highlight.style.top = (tt_highlight_el.offsetTop + tt_margin).toString() + "px";
      } else {
        if (getComputedStyle(tt_highlight_el).textAlign == "right") {
          div_tt_highlight.style.right = -tt_margin.toString() + "px";
        } else {
          div_tt_highlight.style.left = -tt_margin.toString() + "px";
        }
        div_tt_highlight.style.top = -tt_margin.toString() + "px";
      }
      div_tt_highlight.style.width = (tt_highlight_el.offsetWidth + tt_margin).toString() + "px";
      div_tt_highlight.style.height = (tt_highlight_el.offsetHeight + tt_margin).toString() + "px";
    } else {
      tt_highlight_el.classList.add(tt_highlight_className);
    }

    n = n + 1;
    if (n > 2) {
      if (div_tt_highlight) {
        div_tt_highlight.style.backgroundColor = "";
      }

      return;
    }
  }
  setTimeout(function () { add_tt_highlight_class_blink(tt_highlight_el, force_class, n); }, 100);
}

function clear_tt_highlight_class(els) {
  if (els) {
    for (var i; i < els.length; i++) {
      els[i].classList.remove(tt_highlight_className);
    }
  }
}

function drawTutorialDialog(parent, iLevel, sText, sHeader, iNextButton, ttRef, elsHighlight, bWebApp, bScouting) {
  // Remove existing tutorial dialogs
  var dgTt = document.getElementsByClassName("ui-dialog");
  for (var i = dgTt.length - 1; i >= 0; i--) {
    if (dgTt[i] && dgTt[i].parentElement && dgTt[i].contains(document.getElementById("divTutorial"))) {
      dgTt[i].parentElement.removeChild(dgTt[i]);
    }
  }

  // Calculate incremental level
  var jLevel = 0;
  var jLevelMax = 0;
  var iTtStart = 0;
  var iTtEnd = iTtMainLevelEnd;
  if (iLevel >= iTtMainLevelPart2Start * 10) {
    iTtStart = iTtMainLevelPart2Start;
    iTtEnd = iTtMainLevelPart2End;
  }

  for (var j = iTtStart * 10; j < iTtEnd * 10; j++) {
    if (get_level_text(j).text) {
      if (j < iLevel) { jLevel = jLevel + 1; }

      jLevelMax = jLevelMax + 1;
    }
  }

  var div0 = document.createElement("div");
  div0.id = "divTutorial";
  div0.title = "Cornerkick Tutorial " + sHeader + " (" + (jLevel + 1).toString() + " / " + (jLevelMax + 1).toString() + ")";

  // Text section
  var div1 = document.createElement("div");
  div1.style.position = "relative";
  div1.style.width = "90%";
  div1.style.height = "auto";
  div1.innerHTML = sText;
  div0.appendChild(div1);

  parent.appendChild(div0);

  var iWidth = Math.max(320, Math.trunc(parent.offsetWidth * 0.5));

  // Create buttons
  var buttons = [];

  if (iLevel === 0) {
    buttons.push({
      text: "nicht mehr anzeigen",
      class: "btnDialog btn btn-outline-secondary",
      icon: "ui-icon-closethick",
      tabIndex: -1,
      click: function () {
        setLevel(false, iLevel);

        $(this).dialog('destroy').remove();
      }
    });
  } else {
    buttons.push({
      text: "von Vorne",
      class: "btnDialog btn btn-outline-secondary",
      tabIndex: -1,
      icon: "ui-icon-arrowreturn-1-e",
      click: function () {
        setLevel(true, 0, function() { window.open('/Member/Desk', '_self', false); });
      }
    });

    buttons.push({
      text: "zurück",
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
  }

  if (iNextButton > 0) {
    var sNextButtonText = "überspringen";
    var sNextButtonIcon = "";
    if (iNextButton === 1) {
      sNextButtonText = "weiter";
      sNextButtonIcon = "ui-icon-arrow-1-e";
    } else if (iNextButton === 3) {
      sNextButtonText = "schließen";
      sNextButtonIcon = "ui-icon-arrow-1-e";
    }

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
                /*
                $('html, body').animate({
                  scrollTop: $("#tablediv").offset().top
                }, 1000, function () {
                  drawTutorial(parent, iLevel + 1);
                });
                */

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
  }

  $(div0).dialog({
    autoOpen: true,
    width: iWidth,
    buttons: buttons
  });
}

function setLevel(bShow, iLevel, _callbackFnc) {
  $.ajax({
    type: 'GET',
    url: '/Member/SetTutorialLevel',
    dataType: "json",
    data: { bShow: bShow, iLevel: iLevel },
    success: function (bOk) {
      if (bOk) {
        if (_callbackFnc) {
          _callbackFnc();
        }
      }
    }
  });
}

function navigateToSite(iLevel) {
  if (parseInt(iLevel / 10) === iTtMainLevelStart) {
    window.open('/Member/Desk', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelTeam) {
    window.open('/Member/Team', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelPlayer) {
    window.open('/Member/PlayerDetails', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelUser) {
    window.open('/Member/UserView', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelTraining) {
    window.open('/Member/Training', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelStaff) {
    window.open('/Member/Personal', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelCalendar) {
    window.open('/Member/Calendar', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelTactic) {
    window.open('/Member/Tactic', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelSponsors) {
    window.open('/Member/Sponsor', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelMerch) {
    window.open('/Member/Merchandising', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelStSurr) {
    window.open('/Member/StadiumSurroundings', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelContracts) {
    window.open('/Member/Contracts', '_self', false);
  } else if (parseInt(iLevel / 10) === iTtMainLevelSpec) {
    window.open('/Member/Finance', '_self', false);
  }
}
