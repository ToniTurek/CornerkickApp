using CornerkickApp.Shared.Models;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  public class SponsorController
  {
    public static SponsorModel Get(CornerkickManager.User _usr)
    {
      SponsorModel sponsorModel = new SponsorModel();

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return sponsorModel;

      // Sponsor badges
      // Create google sponsor banner
      string sSponsorGoogleQuad = "<script async src=\"https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js\"></script>";
      sSponsorGoogleQuad += "<ins class=\"adsbygoogle\" style=\"display:block\" data-ad-client=\"ca-pub-4643281447734684\" data-ad-slot=\"1299616919\" data-ad-format=\"auto\" data-full-width-responsive=\"true\"></ins>";
      sSponsorGoogleQuad += "<script>(adsbygoogle = window.adsbygoogle || []).push({});</script>";

      string sSponsorGoogleHori = "<script async src=\"https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js\"></script>";
      sSponsorGoogleHori += "<ins class=\"adsbygoogle\" style=\"display:inline-block;width:320px;height:50px\" data-ad-client=\"ca-pub-4643281447734684\" data-ad-slot=\"3762128727\"></ins>";
      sSponsorGoogleHori += "<script>(adsbygoogle = window.adsbygoogle || []).push({});</script>";

      // Add google/amazon sponsor banner to list
      // Main sponsors
      sponsorModel.ltSponsorBadgeMain = new List<string>();

      // Amazon banner 300x250
      /*
      sponsorModel.ltSponsorBadgeMain.Add("<iframe src=\"https://rcm-eu.amazon-adsystem.com/e/cm?o=3&p=12&l=ur1&category=amazongeneric&banner=1JA20XDVWF6Z9EDMAZ82&f=ifr&linkID=b6fe36fdca5d9eac0118d134db97f909&t=cornerkick08-21&tracking_id=cornerkick08-21\" width=\"300\" height=\"250\" scrolling=\"no\" border=\"0\" marginwidth=\"0\" style=\"border: none\"; frameborder=\"0\" sandbox=\"allow-scripts allow-same-origin allow-popups allow-top-navigation-by-user-activation\"></iframe>");
      sponsorModel.ltSponsorBadgeMain.Add("<iframe src=\"https://rcm-eu.amazon-adsystem.com/e/cm?o=3&p=12&l=ur1&category=pw&banner=1RWHE2Q7M2AQ18P32H02&f=ifr&linkID=84ef44af0d12e55ea8107cabae16dc90&t=cornerkick08-21&tracking_id=cornerkick08-21\" width=\"300\" height=\"250\" scrolling=\"no\" border=\"0\" marginwidth=\"0\" style=\"border:none;\" frameborder=\"0\" sandbox=\"allow-scripts allow-same-origin allow-popups allow-top-navigation-by-user-activation\"></iframe>");
      sponsorModel.ltSponsorBadgeMain.Add("<iframe src=\"https://rcm-eu.amazon-adsystem.com/e/cm?o=3&p=13&l=ur1&category=pw&banner=0XBKCDT8E2GQB8ZCFD82&f=ifr&linkID=065473609014ed78e759b8d785781f30&t=cornerkick08-21&tracking_id=cornerkick08-21\" width=\"468\" height= \"60\" scrolling=\"no\" border=\"0\" marginwidth=\"0\" style=\"border:none;\" frameborder=\"0\" sandbox=\"allow-scripts allow-same-origin allow-popups allow-top-navigation-by-user-activation\"></iframe>");
      sponsorModel.ltSponsorBadgeMain.Add("<iframe src=\"https://rcm-eu.amazon-adsystem.com/e/cm?o=3&p=13&l=ur1&category=pw&banner=1CCQRQGC35NQ06XQWGR2&f=ifr&linkID=b3ac6d81a525a78aa4dfb01813ab5e23&t=cornerkick08-21&tracking_id=cornerkick08-21\" width=\"468\" height= \"60\" scrolling=\"no\" border=\"0\" marginwidth=\"0\" style=\"border:none;\" frameborder=\"0\" sandbox=\"allow-scripts allow-same-origin allow-popups allow-top-navigation-by-user-activation\"></iframe>");
      sponsorModel.ltSponsorBadgeMain.Add("<iframe src=\"https://rcm-eu.amazon-adsystem.com/e/cm?o=3&p=13&l=ur1&category=pw&banner=0HQD8NPXF3GWMK4C8TG2&f=ifr&linkID=e3dd849aff04bb3f05ffea09f1dd6d1d&t=cornerkick08-21&tracking_id=cornerkick08-21\" width=\"468\" height= \"60\" scrolling=\"no\" border=\"0\" marginwidth=\"0\" style=\"border:none;\" frameborder=\"0\" sandbox=\"allow-scripts allow-same-origin allow-popups allow-top-navigation-by-user-activation\"></iframe>");
      sponsorModel.ltSponsorBadgeMain.Add("<iframe src=\"https://rcm-eu.amazon-adsystem.com/e/cm?o=3&p=13&l=ur1&category=pw&banner=06XW0KF9SZXP91NYRVR2&f=ifr&linkID=b00f3e7a9e90f543d3645f13424f4d0f&t=cornerkick08-21&tracking_id=cornerkick08-21\" width=\"468\" height= \"60\" scrolling=\"no\" border=\"0\" marginwidth=\"0\" style=\"border:none;\" frameborder=\"0\" sandbox=\"allow-scripts allow-same-origin allow-popups allow-top-navigation-by-user-activation\"></iframe>");
      sponsorModel.ltSponsorBadgeMain.Add(sSponsorGoogleQuad);
      */

      sponsorModel.ltSponsorBadgeMain.Add("<img src=\"" + CkAppShared.sContentDir + "/Images/CornerFlag.png\" style=\"position: relative; width: 100%; height: 100%; object-fit: contain\"/>");
      while (sponsorModel.ltSponsorBadgeMain.Count <= 12) {
        sponsorModel.ltSponsorBadgeMain.Add("<img src=\"" + CkAppShared.sContentDir + "/Images/sponsors/1a.png\" style=\"position: relative; width: 100%; height: 100%; object-fit: contain\"/>");
      }

      while (sponsorModel.ltSponsorBadgeBoard.Count <= 12) {
        sponsorModel.ltSponsorBadgeBoard.Add("<img src=\"" + CkAppShared.sContentDir + "/Images/sponsors/1.png\" style=\"position: relative; width: 100%; height: 100%; object-fit: contain\"/>");
      }

      // Sound
      sponsorModel.bSound = true;
      if (_usr.lti?.Count > UserOptionsModel.iUserOptionsIxSound) sponsorModel.bSound = _usr.lti[UserOptionsModel.iUserOptionsIxSound] > 0;

      Random rnd = new Random();

      if (clb.sponsorMain?.iId >= CkAppShared.ckMng.fz.ltSponsoren.Count) {
        clb.sponsorMain.iId = (byte)rnd.Next(1, CkAppShared.ckMng.fz.ltSponsoren.Count);
      }

      sponsorModel.sponsorMain = clb.sponsorMain == null ? new CornerkickManager.Finance.Sponsor() : clb.sponsorMain;
      sponsorModel.ltSponsorBoards = clb.ltSponsorBoards;
      sponsorModel.ltSponsorOffers = clb.ltSponsorOffers;

      // Collect sponsor names
      sponsorModel.ltSponsorNames = new List<string>();
      foreach (CornerkickManager.Finance.Spons spns in CkAppShared.ckMng.fz.ltSponsoren) {
        sponsorModel.ltSponsorNames.Add(spns.name);
      }

      sponsorModel.ltSponsorBoardIds = new List<int>();
      for (int iS = 0; iS < sponsorModel.ltSponsorBoards.Count; iS++) {
        CornerkickManager.Finance.Sponsor spon = sponsorModel.ltSponsorBoards[iS];

        if (spon.iId >= CkAppShared.ckMng.fz.ltSponsoren.Count) {
          spon.iId = (byte)rnd.Next(1, CkAppShared.ckMng.fz.ltSponsoren.Count);
        }

        for (int iB = 0; iB < spon.nBoards; iB++) sponsorModel.ltSponsorBoardIds.Add(spon.iId);
      }

      sponsorModel.sEmblem = ClubController.getClubEmblemImgSrc(clb);
      sponsorModel.sColorJersey = Tool.convertToRgb(clb.cl1[0]);

      // Tutorial
      if (CkAppShared.ttUser != null) {
        int iUserIx = CkAppShared.ckMng.ltUser.IndexOf(_usr);
        if (iUserIx >= 0 && iUserIx < CkAppShared.ttUser.Length) sponsorModel.tutorial = CkAppShared.ttUser[iUserIx];
      }

      return sponsorModel;
    }

    public static List<SponsorModel.TableItemSponsorMain>? GetTableMain(CornerkickManager.User? _usr)
    {
      if (_usr == null) return null;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      //The table or entity I'm querying
      List<SponsorModel.TableItemSponsorMain> query = new List<SponsorModel.TableItemSponsorMain>();

      if (clb.sponsorMain?.iId > 0) {
        query.Add(new SponsorModel.TableItemSponsorMain() {
          bOffer = false,
          iId = clb.sponsorMain.iId,
          iIndex = -1,
          sName = CkAppShared.ckMng.fz.ltSponsoren[clb.sponsorMain.iId].name,
          iMoneyYear = clb.sponsorMain.iGeldJahr,
          iMoneyVic = clb.sponsorMain.iMoneyVicHome,
          iMoneyCupWin = clb.sponsorMain.iGeldMeister,
          iYears = clb.sponsorMain.iYears,
          fMood = clb.sponsorMain.fMood == 0f ? 1f : clb.sponsorMain.fMood
        });
      }
      int iSpOffer = 0;
      for (int iS = 0; iS < clb.ltSponsorOffers.Count; iS++) {
        CornerkickManager.Finance.Sponsor spon = clb.ltSponsorOffers[iS];

        if (spon.iType != 0) continue;

        SponsorModel.TableItemSponsorMain deSponsorMain = new SponsorModel.TableItemSponsorMain();
        deSponsorMain.bOffer = true;
        deSponsorMain.iId = spon.iId;
        deSponsorMain.iIndex = iSpOffer++;
        spon.iId = (byte)Math.Min(spon.iId, CkAppShared.ckMng.fz.ltSponsoren.Count - 1);
        deSponsorMain.sName = CkAppShared.ckMng.fz.ltSponsoren[spon.iId].name;
        deSponsorMain.iMoneyYear = spon.iGeldJahr;
        deSponsorMain.iMoneyVic = spon.iMoneyVicHome;
        deSponsorMain.iMoneyCupWin = spon.iGeldMeister;
        deSponsorMain.iYears = spon.iYears;
        deSponsorMain.fMood = spon.fMood == 0f ? 1f : spon.fMood;

        query.Add(deSponsorMain);
      }

      return query;
    }

    public static List<SponsorModel.TableItemSponsorBoard>? GetTableBoard(CornerkickManager.User _usr)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      //The table or entity I'm querying
      List<SponsorModel.TableItemSponsorBoard> query = new List<SponsorModel.TableItemSponsorBoard>();

      int iSpOffer = 0;
      bool bOffer = false;
      foreach (List<CornerkickManager.Finance.Sponsor> ltSponsor in new List<CornerkickManager.Finance.Sponsor>[] { clb.ltSponsorBoards, clb.ltSponsorOffers }) {
        for (int iS = 0; iS < ltSponsor.Count; iS++) {
          CornerkickManager.Finance.Sponsor spon = ltSponsor[iS];

          if (spon.iType != 1) continue;

          SponsorModel.TableItemSponsorBoard deSponsorBoard = new SponsorModel.TableItemSponsorBoard();
          deSponsorBoard.bOffer = bOffer;
          deSponsorBoard.iId = spon.iId;
          if (bOffer) deSponsorBoard.iIndex = iSpOffer++;
          else        deSponsorBoard.iIndex = -1;
          spon.iId = (byte)Math.Min(spon.iId, CkAppShared.ckMng.fz.ltSponsoren.Count - 1);
          deSponsorBoard.sName = CkAppShared.ckMng.fz.ltSponsoren[spon.iId].name;
          deSponsorBoard.iMoneyVicHome = spon.iMoneyVicHome;
          deSponsorBoard.nBoards = spon.nBoards;
          deSponsorBoard.iYears = spon.iYears;
          deSponsorBoard.fMood = spon.fMood == 0f ? 1f : spon.fMood;

          query.Add(deSponsorBoard);
        }

        bOffer = true;
      }

      return query;
    }

    public static List<SponsorModel.TableItemSponsorSpecial>? GetTableSpecial(CornerkickManager.User _usr)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      //The table or entity I'm querying
      List<SponsorModel.TableItemSponsorSpecial> query = new List<SponsorModel.TableItemSponsorSpecial>();

      foreach (CornerkickManager.Finance.SponsorSpecial spsp in CkAppShared.ckMng.fz.ltSponsorSpecial) {
        SponsorModel.TableItemSponsorSpecial deSponsorSpecial = new SponsorModel.TableItemSponsorSpecial();
        deSponsorSpecial.bOffer = true;
        if (clb.ltSponsorSpecial != null) {
          foreach (CornerkickManager.Finance.SponsorSpecial.Contract spspc in clb.ltSponsorSpecial) {
            if (spspc.spsp.iId == spsp.iId) {
              deSponsorSpecial.bOffer = false;
              break;
            }
          }
        }
        deSponsorSpecial.iId = spsp.iId;
        deSponsorSpecial.sName = spsp.sName;
        deSponsorSpecial.iMoney = spsp.iMoney;
        if      (spsp.iType == 1) deSponsorSpecial.sCondition = "Kein Gegentor";
        else if (spsp.iType == 2) deSponsorSpecial.sCondition = "4 oder mehr eigene Tore";
        else if (spsp.iType == 3) deSponsorSpecial.sCondition = "Keine Karten";
        else if (spsp.iType == 4) deSponsorSpecial.sCondition = "3 oder mehr Gegentore";
        else if (spsp.iType == 5) deSponsorSpecial.sCondition = "Kein eigenes Tor";

        query.Add(deSponsorSpecial);
      }

      return query;
    }

    public static CornerkickManager.Finance.Sponsor? Negotiate(CornerkickManager.User _usr, bool bMain, int iSponsorIx, int iOffer1, int iOffer2, int iOffer3, int iReq1, int iReq2, int iReq3, out float fMoodStart)
    {
      fMoodStart = 0f;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      float fSensitivityFactor = 1f / _usr.getDefaultUserSkillFactor(_usr.iSkillNegotiation);

      int jS = 0;
      foreach (CornerkickManager.Finance.Sponsor spon in clb.ltSponsorOffers) {
        if (bMain && spon.iType != 0) continue;
        if (!bMain && spon.iType != 1) continue;

        if (jS++ == iSponsorIx) {
          fMoodStart = spon.fMood;

          double fMood = 0.0;
          double fMood1 = spon.fMood;
          double fMood2 = spon.fMood;
          double fMood3 = spon.fMood;

          if (iOffer1 > 0) {
            CornerkickManager.Tool.negotiate(ref fMood1, ref iOffer1, iReq1, fSensitivityFactor: fSensitivityFactor, bWantedIsHigherThanOffered: false);
          }

          if (iOffer2 > 0) {
            CornerkickManager.Tool.negotiate(ref fMood2, ref iOffer2, iReq2, fSensitivityFactor: fSensitivityFactor, bWantedIsHigherThanOffered: false);
          }

          if (iOffer3 > 0) {
            CornerkickManager.Tool.negotiate(ref fMood3, ref iOffer3, iReq3, fSensitivityFactor: fSensitivityFactor, bWantedIsHigherThanOffered: false);
          }

          if (bMain) {
            fMood = (fMood1 * 0.6) + (fMood2 * 0.2) + (fMood3 * 0.2);
          } else {
            fMood = fMood1;
          }

          if (fMood < 0.1) {
            fMood = -1.0; // Negotiation cancelled
            clb.ltSponsorOffers.Remove(spon);
          }

          if (bMain) {
            spon.iGeldJahr     = Tool.roundInt(iOffer1, 2);
            spon.iMoneyVicHome = Tool.roundInt(iOffer2, 2);
            spon.iGeldMeister  = Tool.roundInt(iOffer3, 2);
          } else {
            spon.iMoneyVicHome = Tool.roundInt(iOffer1, 2);
          }
          spon.fMood = (float)fMood;

          return spon;
        }
      }

      return null;
    }

    public static int TakeSponsor(CornerkickManager.User _usr, int iType, int iSponsorIndex)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return 0;

      int jS = 0;
      foreach (CornerkickManager.Finance.Sponsor spon in clb.ltSponsorOffers) {
        if (spon.iType != iType) continue;

        if (jS++ == iSponsorIndex) {
          return CkAppShared.ckMng.ui.setSponsor(ref clb, spon);
        }
      }

      return 0;
    }

    public int GetCancelCompensation(CornerkickManager.User _usr)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return 0;
      if (clb.sponsorMain == null) return 0;

      double fSeasonRel = 1.0 - (CkAppShared.ckMng.dtDatum - CkAppShared.ckMng.dtSeasonStart).TotalDays / (CkAppShared.ckMng.dtSeasonEnd - CkAppShared.ckMng.dtSeasonStart).TotalDays;

      int iCompensation = (int)(clb.sponsorMain.iGeldJahr * fSeasonRel);
      iCompensation += (int)(clb.sponsorMain.iGeldJahr * 0.1 * clb.sponsorMain.iYears); // Add 10% for each year of contract

      return (iCompensation / 1000) * 1000;
    }

    public static bool CancelMain(CornerkickManager.User _usr, int iSponsorIx, int iCost)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return false;

      if (iSponsorIx < 0) {
        clb.sponsorMain = null;
        CornerkickManager.Finance.doTransaction(clb, CkAppShared.ckMng.dtDatum, -iCost, CornerkickManager.Finance.iTransferralTypePaySponsorCompensation);
        return true;
      }

      int jS = 0;
      foreach (CornerkickManager.Finance.Sponsor spon in clb.ltSponsorOffers) {
        if (spon.iType != 0) continue;

        if (jS++ == iSponsorIx) {
          clb.ltSponsorOffers.Remove(spon);
          return true;
        }
      }

      return false;
    }

    public static bool TakeSponsorSpecial(CornerkickManager.User _usr, int iSponsorId)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return false;

      if (clb.ltSponsorSpecial == null) clb.ltSponsorSpecial = new List<CornerkickManager.Finance.SponsorSpecial.Contract>();

      // Check if already two special sponsors
      if (clb.ltSponsorSpecial.Count > 1) return false;

      clb.ltSponsorSpecial.Add(new CornerkickManager.Finance.SponsorSpecial.Contract() { spsp = CkAppShared.ckMng.fz.getSponsorSpecial((byte)iSponsorId) });

      return true;
    }

  }
}
