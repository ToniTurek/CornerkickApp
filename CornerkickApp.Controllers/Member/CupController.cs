using CornerkickApp.Shared.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  public class CupController
  {
    public static CupModel Get(int iId1, int iId2, int iId3, CornerkickManager.User? _usr = null)
    {
      CupModel model = new CupModel();
      
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb != null) {
        model.iClubId = clb.iId;
      }

      CornerkickManager.Cup cup = CkAppShared.ckMng.tl.getCup(iId1, iId2, iId3);
      if (cup == null) return model;
      if (cup.ltMatchdays == null) return model;
      if (cup.ltMatchdays.Count < 1) return model;

      model.sCupName = cup.sName;
#if _WebApp
      model.sCupEmblem = getCupEmblem(cup);
#else
      model.bCupEmblem = getCupEmblemB(cup);
#endif
      model.iSeason = CkAppShared.ckMng.iSeason;
      model.iMatchday = cup.getMatchday(CkAppShared.ckMng.dtDatum);
      if (clb != null) model.iMatchday = getCupMatchday(model.iSeason, iId1, iId2, iId3, clb: clb) - 1;
      model.iMatchday = Math.Min(model.iMatchday, cup.getMatchdaysTotal() - 1);
      model.iMatchdayCurrent = model.iMatchday;
      model.nMdsGroup = cup.getMatchdaysGroup();

      //CkAppShared.iSeasonGlobal = CkAppShared.ckMng.iSeason;

      model.ddlSeason = MemberController.getDdlSeason();
      if (iId2 >= 0) model.ddlLand = MemberController.getDdlLand(iId1, iLandSelected: iId2);
      model.ddlMatchdays = getDdlMatchdays(model.iSeason, iId1, iId2, iId3);

      for (byte iG = 0; iG < cup.settings.nGroups; iG++) {
        if (clb != null && cup.ltClubs[iG].Find(c => c.iId == clb.iId) != null) model.iGroup = iG;

        model.ddlGroups.Add(new SelectListItem() { Text = Convert.ToChar(iG + 65).ToString(), Value = iG.ToString(), Selected = model.iGroup == iG });
      }

      return model;
    }

    public static List<LeagueModel.GameInfo> getGameInfos(int iSeason, int iId1, int iLand, int iId3, int iMd = -1, CornerkickManager.User? usr = null, byte iGroup = 0, bool bCompact = false)
    {
      List<LeagueModel.GameInfo> ltGameInfos = new List<LeagueModel.GameInfo>();

      //CornerkickManager.Cup cup = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdNatCup, iLand);
      CornerkickManager.Cup? cup = MemberController.getCup(iSeason, iId1, iLand, iId3);
      if (cup == null) return ltGameInfos;

      return MemberController.getGameInfos(cup, iMd, usr: usr, iGroup: iGroup, bCompact: bCompact);
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

    public static List<SelectListItem> getDdlMatchdays(int iSeason, int iId1, int iLand, int iId3, int iMdSelected = -1)
    {
      //CornerkickManager.Cup cup = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdNatCup, iLand);
      CornerkickManager.Cup? cup = MemberController.getCup(iSeason, iId1, iLand, iId3);
      if (cup == null) return new List<SelectListItem>();

      List<SelectListItem> ltMd = new List<SelectListItem>();
      // Spieltage zu Dropdown Menü hinzufügen
      int nMdsTotal = cup.getMatchdaysTotal();
      for (int iMd = 0; iMd < nMdsTotal; iMd++) {
        //ltMd[iMd] += ";" + CornerkickManager.Main.sCupRound[nRound - iMd - 1];

        string sTxt = (iMd + 1).ToString();
        int nMdsGroup = 0;
        int iRoundKo = 0;
        int k = 1;
        if (cup.settings.bKo && cup.settings.bKoTwoGames) k = 2;

        if (cup.settings.nGroups > 0) {
          nMdsGroup = cup.getMatchdaysGroup();
          iRoundKo = cup.getKoRound(cup.settings.nGroups * cup.settings.nQualifierKo) - ((iMd - nMdsGroup) / k);
        } else {
          iRoundKo = cup.getKoRound(cup.getClubsTotal()) - (iMd / k);
        }

        // If K.O. phase, show round name instead of matchday number
        if (iMd >= nMdsGroup && iRoundKo > 0 && iRoundKo <= CornerkickManager.Main.sCupRound.Length) {
          sTxt = CornerkickManager.Main.sCupRound[iRoundKo - 1];

          // If two games per round, show first/second leg (check also if final one game)
          if (k > 1 && (iMd < nMdsTotal - 1 || (nMdsTotal - nMdsGroup) % 2 == 0)) {
            sTxt += (iMd - nMdsGroup) % 2 == 0 ? " (Hin.)" : " (Rück.)";
          }
        }

        ltMd.Add(new SelectListItem {
          Text = sTxt,
          Value = iMd.ToString(),
          Selected = iMd == Math.Min(iMdSelected, nMdsTotal - 1)
        });
      }

      return ltMd;
    }

    public static TeamModel.TeamData? GetMatchdayTeam(int iCupId, int iLand, int iDiv)
    {
      TeamModel.TeamData tD = new TeamModel.TeamData();
      tD.ltPlayer2 = new List<TeamModel.Player>();

      CornerkickManager.Cup cup = CkAppShared.ckMng.tl.getCup(iCupId, iLand, iDiv);
      if (cup == null) return null;

      int iMd = cup.getMatchday(CkAppShared.ckMng.dtDatum);
      if (iMd < 1) return null;

      CornerkickManager.Cup.Matchday md = cup.ltMatchdays[iMd - 1];

      int iForm = 19;
      CornerkickGame.Tactic.Formation frm = CkAppShared.ckMng.ltFormationen[iForm];

      List<CornerkickManager.Player> ltPlayerMd = new List<CornerkickManager.Player>(); // List of all potential players
      List<CornerkickManager.Player> ltPlayerBest = new List<CornerkickManager.Player>();

      foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
        for (byte iHA = 0; iHA < 2; iHA++) {
          CornerkickManager.Club clb = CornerkickManager.Tool.getClubFromId(gd.team[iHA].iTeamId, CkAppShared.ckMng.ltClubs);
          if (clb == null) continue;
          if (clb.ltPlayer == null) continue;

          foreach (CornerkickManager.Player pl in clb.ltPlayer) {
            if (pl?.plGame?.statGame != null && pl.plGame.statGame.iGameType == iCupId && pl.plGame.statGame.iGameType2 == iLand && pl.plGame.statGame.iMatchday == iMd - 1/* && pl.plGame.bPlayed*/) ltPlayerMd.Add(pl);
          }
        }
      }

      for (byte iP = 0; iP < 11; iP++) {
        float fGrade = 7f;
        tD.ltPlayer2.Add(null);
        ltPlayerBest.Add(null);

        byte iPosExact = CornerkickGame.Tool.getPosRole(CkAppShared.ckMng.ltFormationen[iForm].positions[iP].pt, CkAppShared.ckMng.game.ptPitch);
        byte iPos = CornerkickGame.Tool.getBasisPos(iPosExact);

        foreach (CornerkickManager.Player pl in ltPlayerMd) {
          if (pl.plGame.fExperiencePos[iPos - 1] < 0.999) continue; // Main position

          // Check if already in best graded players list
          bool bSame = false;
          foreach (CornerkickManager.Player plSame in ltPlayerBest) {
            if (plSame != null && plSame.plGame.iId == pl.plGame.iId) {
              bSame = true;
              break;
            }
          }
          if (bSame) continue;

          float fGradeTmp = pl.plGame.getGrade(iPos, 90);

          if (fGradeTmp > 0f && fGradeTmp < fGrade) {
            if (tD.ltPlayer2[iP] == null) tD.ltPlayer2[iP] = new TeamModel.Player();

            tD.ltPlayer2[iP].iId = pl.plGame.iId;
            tD.ltPlayer2[iP].sName = pl.plGame.sName;
            tD.ltPlayer2[iP].iNb = (byte)(iP + 1);
            tD.ltPlayer2[iP].sNat = CornerkickManager.Main.sLandShort[pl.iNat1];
            tD.ltPlayer2[iP].sPortrait = PlayerController.getPlayerPortraitHtmlImg(pl, bSmall: true);
            tD.ltPlayer2[iP].iPos = iPos;

            if (frm.positions.Length > iP) {
              tD.ltPlayer2[iP].ptPos = new TeamModel.Point(frm.positions[iP].pt);

              tD.ltPlayer2[iP].sSkillAve = fGradeTmp.ToString("0.0");
            }

            fGradeTmp = pl.plGame.getGrade(iPos, 90);
            fGrade = fGradeTmp;

            if (pl.contract?.club != null) tD.ltPlayer2[iP].sTeamname = pl.contract.club.sName;
            tD.ltPlayer2[iP].sAge = pl.plGame.getAge(CkAppShared.ckMng.dtDatum).ToString("0.0");

            ltPlayerBest[iP] = pl;
          }
        }
      }

      /*
      float[] fTeamAve11 = CornerkickManager.Tool.getTeamAve(ltPlayerBest, App.ckMng.dtDatum, App.ckMng.dtSeasonEnd, iPlStop: 11);
      tD.fTeamAveStrength = fTeamAve11[3];
      tD.fTeamAveAge = fTeamAve11[4];
      */

      return tD;
    }

    public static int[] getCupPlace(CornerkickManager.Cup? cup, CornerkickManager.Club clb, DateTime dtNow)
    {
      if (cup?.ltMatchdays == null) return [0, 0, 0];
      if (cup.ltMatchdays.Count < 1) return [0, 0, 0];
      int nPartFirstRound = cup.getParticipants().Count;
      if (nPartFirstRound < 1) return [0, 0, 0];

      int iPlace = 1;
      int iGms = 0;

      if (cup.settings.bKo) {
        if (cup.ltMatchdays[0].ltGameData != null) {
            int nRound = cup.getKoRound(nPartFirstRound);
            int iMdClub = Math.Max(cup.getMatchdays(clb), 0);
            //int iMdCurr = cup.getMatchday(dtNow); // Current matchday

            if (nRound - iMdClub >= 0) {
              iPlace = nRound - iMdClub;
            }
        }
      } else {
        List<CornerkickManager.Cup.TableItem> ltTbl = cup.getTable();
        foreach (CornerkickManager.Cup.TableItem tbl in ltTbl) {
          if (tbl.iId == clb.iId) {
            iGms = tbl.iWDL[0] + tbl.iWDL[1] + tbl.iWDL[2];
            break;
          }
          iPlace++;
        }
      }

      return [iPlace, iGms, cup.getMatchdaysTotal()];
    }

    public static System.Drawing.Color getCupColor(CornerkickManager.Cup? cup)
    {
      if (cup == null) return System.Drawing.Color.White;

      if (cup.iId ==  CkAppShared.iCupIdLeague)   return System.Drawing.Color.FromArgb(  0, 175, 100); // Nat. league
      if (cup.iId ==  CkAppShared.iCupIdNatCup)   return System.Drawing.Color.FromArgb(100, 100, 255); // Nat. Cup
      if (cup.iId ==  CkAppShared.iCupIdInt)      return System.Drawing.Color.FromArgb(255, 200,  14); // Int. games
      if (cup.iId == -CkAppShared.iCupIdTestgame) return System.Drawing.Color.FromArgb(200, 200, 200); // Testgame requests
      if (cup.iId ==  CkAppShared.iCupIdWc)       return System.Drawing.Color.FromArgb( 91, 146, 229); // World cup

      return System.Drawing.Color.White;
    }

    public static string getCupEmblem(CornerkickManager.Cup cup)
    {
      return getCupEmblem(cup.iId, cup.iId2, cup.iId3);
    }
    public static string getCupEmblem(int iCupId, int iCupId2, int iCupId3, int iImgWidth = 0)
    {
      string sCupEmblem = "";

      string sCupEmblemIdString = iCupId.ToString() + "_" + iCupId2.ToString() + "_" + iCupId3.ToString();
#if !_WebApp
      // Try to get user added emblems first
      string sCupEmblemFileUser = Path.Combine(App.getMediaDir(CkAppShared.ckMng.sDatabaseName), "images", "cup_emblems", sCupEmblemIdString + ".png");
      byte[]? b = null;
      try {
        b = File.ReadAllBytes(sCupEmblemFileUser);
        return "data:image/*;base64," + @Convert.ToBase64String(b);
      } catch (Exception ex) {
        //ckMng.tl.writeLog("Error loading cup emblem." + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, sLogFile: CornerkickManager.Main.sErrorFile);
      }
#endif
      sCupEmblem = CkAppShared.sContentDir + "/Images/cup_emblems/" + sCupEmblemIdString + ".png";

      if ((iCupId == CkAppShared.iCupIdLeague || iCupId == CkAppShared.iCupIdNatCup) && iCupId2 >= 0) {
        // Draw National icon on trophy
        string sNatFile = Path.Combine(CkAppShared.sContentDir, "Icons", "flags", CornerkickManager.Main.sLandShort[iCupId2] + ".png");
        //if (File.Exists(sNatFile)) {
        Image? imgCup = null;
        /*
        for (int i = 0; i < 2; i++) {
          try {
            imgCup = Image.Load(sCupEmblemFile);
          } catch {
            sCupEmblemFile = Path.Combine(sContentDir, "Images", "cup_emblems", iCupId.ToString() + "_-1_-1.png");
            continue;
          }
        }
        */

        if (imgCup != null) {
          if (iImgWidth > 0) {
            int iImgHeight = (int)(iImgWidth * (imgCup.Height / (float)imgCup.Width));
            imgCup.Mutate(x => x.Resize(iImgWidth, iImgHeight));
          }
          Image imgNat = Image.Load(sNatFile);

          int iNatWidth = imgCup.Width;
          if      (iCupId == CkAppShared.iCupIdLeague) iNatWidth = (int)(imgCup.Width * 0.18);
          else if (iCupId == CkAppShared.iCupIdNatCup) iNatWidth = (int)(imgCup.Width * 0.32);
          int iNatHeight = (int)(iNatWidth * (imgNat.Height / (float)imgNat.Width));

          float fNatPosY = 0f;
          if (iCupId == CkAppShared.iCupIdLeague) fNatPosY = imgCup.Height * 0.34f;
          else if (iCupId == CkAppShared.iCupIdNatCup) fNatPosY = imgCup.Height * 0.865f - iNatHeight / 2f;

          imgNat.Mutate(x => x.Resize(iNatWidth, iNatHeight));
          imgCup.Mutate(o => o.DrawImage(imgNat, new Point((imgCup.Width - iNatWidth) / 2, (int)fNatPosY), 1f));

          sCupEmblem = "data:image/*;base64," + @Convert.ToBase64String(Tool.ConvertImageToBytes(imgCup));
        }
        //}
      }

      return sCupEmblem;
    }
    public static string getCupEmblemImg(CornerkickManager.Cup cup, string sStyle = "")
    {
      return getCupEmblemImg(cup.iId, cup.iId2, cup.iId3, sStyle: sStyle);
    }
    public static string getCupEmblemImg(int iCupId, int iCupId2, int iCupId3, string sStyle = "")
    {
      if (!string.IsNullOrEmpty(sStyle)) sStyle = " style=\"" + sStyle + "\"";

      string sCupEmblemImg = "<img src=\"" + getCupEmblem(iCupId, iCupId2, iCupId3, iImgWidth: 128) + "\"" + sStyle + " onerror=\"this.onerror=null;this.src='" + getCupEmblem(iCupId, -1, -1, iImgWidth: 128) + "'\" >";

      return sCupEmblemImg;
    }

#if !_WebApp
    public static byte[]? getCupEmblemB(CornerkickManager.Cup cup, string? sDbName = null)
    {
      return getCupEmblemB(cup.iId, cup.iId2, cup.iId3, sDbName: sDbName);
    }
    public static byte[]? getCupEmblemB(int iId, int iId2, int iId3, string? sDbName = null)
    {
      string _sDbName = sDbName ?? CkAppShared.ckMng.sDatabaseName;
      if (!App.checkMediaExist(_sDbName)) return null;

      //string sMediaFile = Path.Combine(ckMng.sDatabaseName, "images", "cup_emblems", iId.ToString() + "_" + iId2.ToString() + "_" + iId3.ToString() + ".png");
      //string sMediaFile = App.getDocumentsDir + "/database/media_" + CkAppShared.ckMng.sDatabaseName + "/images/cup_emblems/" + iId.ToString() + "_" + iId2.ToString() + "_" + iId3.ToString() + ".png";
      string sMediaFile = Path.Combine(App.getMediaDir(_sDbName), "images", "cup_emblems", iId.ToString() + "_" + iId2.ToString() + "_" + iId3.ToString() + ".png");
      try {
        return File.ReadAllBytes(sMediaFile);
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }

      try {
        using (var fileStream = new FileStream(sMediaFile, FileMode.Open)) {
          using (MemoryStream ms = new MemoryStream()) {
            fileStream.CopyToAsync(ms);
            return ms.ToArray();
          }
        }
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }

      return null;
    }
#endif

    public static int getCupMatchday(int iSeason, int iCupId, int iLand = -1, int iDivision = -1, CornerkickManager.Club? clb = null)
    {
      int iMd = 0;
      bool bLeagueUser = false;
      if (clb != null) bLeagueUser = clb.iLand == iLand && clb.iDivision == iDivision;

      if (iSeason < CkAppShared.ckMng.iSeason) { // Past seasons
        CornerkickManager.Cup cup = CkAppShared.ckMng.tl.getCup(iCupId, iId2: iLand, iId3: iDivision);
        if (cup != null) iMd = cup.getMatchdaysTotal();
      } else { // Current seasons
        // Get current matchday
        iMd = CkAppShared.ckMng.tl.getMatchday(iLand, iDivision, CkAppShared.ckMng.dtDatum, iCupId);

        // Increment matchday if match is today or tomorrow
        if (clb != null) {
          CornerkickGame.Game.Data gdNext = CkAppShared.ckMng.tl.getNextGame(clb, CkAppShared.ckMng.dtDatum, iGameType: iCupId);
          if (gdNext != null && (gdNext.dt.Date - CkAppShared.ckMng.dtDatum.Date).Days < 2) iMd++;
        }

        // Limit to 1
        iMd = Math.Max(iMd, 1);
      }

      return iMd;
    }

    const string sTableColorFst = "#ffffcc";
    const string sTableColorUp = "#ccffcc";
    const string sTableColorDown = "#ffcccc";
    const string sTableColorCupGold = "#ccffcc";
    const string sTableColorCupSilver = "#cce5ff";
    const string sTableColorCupBronze = "#ffc0cb";
    public static List<LeagueModel.TableItem> getTable(CornerkickManager.Cup? cup, int iMd = -1, byte iGroup = 0, CornerkickManager.User? usr = null)
    {
      List<LeagueModel.TableItem> ltClubs = new List<LeagueModel.TableItem>();

      if (cup == null) return ltClubs;

      if (iMd < 0) iMd = cup.getMatchday(CkAppShared.ckMng.dtDatum);
      iMd = Math.Min(iMd, cup.getMatchdaysGroup(iGroup) - 1); // Limit to max matchday of groups

      // Set highlight colors based on cup qualifications
      int iColorFst = 0;
      int iColorUp = 0;
      int iColorDown = 0;
      int iColorCupGold = 0;
      int iColorCupSilver = 0;
      int iColorCupBronze = 0;

      // Search for internat. cups
      foreach (CornerkickManager.Cup cupGSB in CkAppShared.ckMng.ltCups.FindAll(c => c.iId == CkAppShared.iCupIdInt)) {
        if (cupGSB != null && cupGSB.ltQualification != null) {
          foreach (CornerkickManager.Cup.Qualification qGSB in cupGSB.ltQualification) {
            if (qGSB.cup.iId == cup.iId && qGSB.cup.iId2 == cup.iId2 && qGSB.cup.iId3 == cup.iId3) {
              if      (cupGSB.iId2 == 0) iColorCupGold   = qGSB.iPlaceLast;
              else if (cupGSB.iId2 == 1) iColorCupSilver = qGSB.iPlaceLast;
              else if (cupGSB.iId2 == 2) iColorCupBronze = qGSB.iPlaceLast;
              break;
            }
          }
        }
      }

      // Search for league phase qualifications
      if (cup.settings.bKo && cup.settings.nQualifierKo > 0) {
        iColorUp = cup.settings.nQualifierKo; // Number of teams qualifying for knockout phase
      } else if (cup.ltQualification != null) {
        // Set first place color for league only
        iColorFst = 1;

        // Search for league up/downs
        foreach (CornerkickManager.Cup.Qualification q in cup.ltQualification) {
          if (q.cup.iId == CkAppShared.iCupIdLeague && q.cup.iId3 != cup.iId3) { // If league and different division
            if (q.cup.ltQualification != null) {
              // Loop over all qualifications to find linked cup
              foreach (CornerkickManager.Cup.Qualification qUpDown in q.cup.ltQualification) {
                if (qUpDown.cup.iId3 == cup.iId3) { // Find linked cup by division id
                  if      (qUpDown.iPlaceFirst < 2 && qUpDown.iPlaceLast > 0) iColorUp = qUpDown.iPlaceLast;
                  else if (qUpDown.iPlaceLast == 0)                           iColorDown = -(qUpDown.cup.getClubsTotal() - qUpDown.iPlaceFirst + 1);
                  break;
                }
              }
            }
          }
        }
      }

      List<CornerkickManager.Cup.TableItem> ltTbl     = cup.getTable(iMatchday: iMd + 1, iGroup: (sbyte)iGroup);
      List<CornerkickManager.Cup.TableItem> ltTblLast = cup.getTable(iMatchday: iMd,     iGroup: (sbyte)iGroup);

#if _WebApp
      int iUp = 0;
      int iDown = 0;
#endif
      int iIx = 0;
      foreach (CornerkickManager.Cup.TableItem ti in ltTbl) {
        string sBgColor = "white";
        if      (iColorFst       > 0 && iIx < iColorFst)       sBgColor = sTableColorFst;
#if _WebApp
        else if (usr != null && iUp  ++ < iColorUp  ) sBgColor = sTableColorUp;
        else if (usr == null && iDown++ > ltTbl.Count - iColorDown) sBgColor = sTableColorDown;
#else
        else if (iColorUp        > 0 && iIx < iColorUp)        sBgColor = sTableColorUp;
        else if (iColorDown      > 0 && iIx < iColorDown)      sBgColor = sTableColorDown;
#endif
        else if (iColorCupGold   > 0 && iIx < iColorCupGold)   sBgColor = sTableColorCupGold;
        else if (iColorCupSilver > 0 && iIx < iColorCupSilver) sBgColor = sTableColorCupSilver;
        else if (iColorCupBronze > 0 && iIx < iColorCupBronze) sBgColor = sTableColorCupBronze;
        else if (iColorFst       < 0 && iIx >= ltTbl.Count + iColorFst)       sBgColor = sTableColorFst;
        else if (iColorUp        < 0 && iIx >= ltTbl.Count + iColorUp)        sBgColor = sTableColorUp;
        else if (iColorDown      < 0 && iIx >= ltTbl.Count + iColorDown)      sBgColor = sTableColorDown;
        else if (iColorCupGold   < 0 && iIx >= ltTbl.Count + iColorCupGold)   sBgColor = sTableColorCupGold;
        else if (iColorCupSilver < 0 && iIx >= ltTbl.Count + iColorCupSilver) sBgColor = sTableColorCupSilver;
        else if (iColorCupBronze < 0 && iIx >= ltTbl.Count + iColorCupBronze) sBgColor = sTableColorCupBronze;

        // Last place
        int iPlLast = iIx + 1;
        for (int iLast = 0; iLast < ltTblLast.Count; iLast++) {
          if (ltTblLast[iLast].iId == ti.iId) {
            if (iIx != iLast) {
              iPlLast = iLast + 1;
            }
            break;
          }
        }

        CornerkickManager.Club? clb = cup.getParticipants().Find(c => c.iId == ti.iId);
        ltClubs.Add(
          new LeagueModel.TableItem() {
            iIx = ++iIx,
            iIxLast = iPlLast,
            iId = ti.iId,
            sName = ti.sName,
            sEmblem = ClubController.getClubEmblemImg(clb, "height: 24px; object-fit: contain", bTiny: true),
            iW = ti.iWDL[0],
            iD = ti.iWDL[1],
            iL = ti.iWDL[2],
            iPoints = ti.iPoints,
            sGoals = ti.iGoals.ToString() + ":" + ti.iGoalsOpp.ToString(),
            iGoalsDiff = ti.iGoals - ti.iGoalsOpp,
            sBgColor = sBgColor
          }
        );
      }

      return ltClubs;
    }

  }
}
