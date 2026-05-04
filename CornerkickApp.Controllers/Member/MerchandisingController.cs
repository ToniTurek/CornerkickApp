using CornerkickApp.Shared.Models;

namespace CornerkickApp.Controllers.Member
{
  public class MerchandisingController
  {
    public static MerchandisingModel Model(CornerkickManager.User? _usr)
    {
      MerchandisingModel md = new MerchandisingModel();

      if (_usr == null) return md;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return md;

      md.marketer = clb.merchMarketer;
      if (clb.merchMarketer == null) md.iMarketerMoney = getMerchandisingMarketerOffer(clb);
      else                           md.iMarketerMoney = clb.merchMarketer.iMoney;

      md.bFanshopsAvailable = clb.buildings.bgFanshop.iLevel > 0;
      md.bMarketer = clb.merchMarketer != null;

      // Season of budget plan
      md.sctSeason = new List<LayoutModel.SelectListItem>() { new LayoutModel.SelectListItem { Text = CkAppShared.ckMng.dtSeasonStart.Year.ToString(), Value = "-1" } };
      for (int i = clb.ltMerchandisingHistory.Count - 1; i >= 0; i--) {
        CornerkickManager.Club.MerchandisingHistory mh = clb.ltMerchandisingHistory[i];
        md.sctSeason.Add(new LayoutModel.SelectListItem { Text = (CkAppShared.ckMng.dtSeasonStart.Year - (CkAppShared.ckMng.iSeason - mh.iSeason)).ToString(), Value = mh.iSeason.ToString() });
      }

      // Secret Balance
      md.fBalanceSecretFracMerchandisingIncome = clb.fBalanceSecretFracMerchandisingIncome * 100f;

      // Tutorial
      if (CkAppShared.ttUser != null) {
        int iUserIx = CkAppShared.ckMng.ltUser.IndexOf(_usr);
        if (iUserIx >= 0 && iUserIx < CkAppShared.ttUser.Length) md.tutorial = CkAppShared.ttUser[iUserIx];
      }

      return md;
    }

    private static int getMerchandisingMarketerOffer(CornerkickManager.Club clb)
    {
      if (clb == null) return 0;
      double fSeasonFrac = (CkAppShared.ckMng.dtSeasonEnd - CkAppShared.ckMng.dtDatum).TotalDays / (CkAppShared.ckMng.dtSeasonEnd - CkAppShared.ckMng.dtSeasonStart).TotalDays;

      int iUserLevel = clb.user != null ? clb.user.iLevel : 0;
      return (int)(Math.Max(clb.getAttractionFactor(CkAppShared.ckMng.iSeason, ltCups: CkAppShared.ckMng.ltCups, dtNow: CkAppShared.ckMng.dtDatum), 500) * (5000 + ((3 - iUserLevel) * 2000)) * fSeasonFrac);
    }

    public static bool TakeMarketer(CornerkickManager.User? _usr)
    {
      if (_usr == null) return false;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return false;

      clb.merchMarketer = new CornerkickManager.Club.MerchandisingMarketer();
      clb.merchMarketer.marketer = CkAppShared.ckMng.ltMerchandisingMarketer[0];
      clb.merchMarketer.iMoney = getMerchandisingMarketerOffer(clb);

      CornerkickManager.Finance.doTransaction(clb, CkAppShared.ckMng.dtDatum, clb.merchMarketer.iMoney, CornerkickManager.Finance.iTransferralTypeInMerchandisingMarketer);

      // Set budget
      _usr.budget.iInMerchandising = clb.merchMarketer.iMoney;

      return true;
    }
    
    public static List<MerchandisingModel.DatatableMerchandising> GetItems(CornerkickManager.User? _usr, int iSeason)
    {
      List<MerchandisingModel.DatatableMerchandising> ltItems = new List<MerchandisingModel.DatatableMerchandising>();

      if (_usr == null) return ltItems;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return ltItems;

      int iIx = 0;
      foreach (CornerkickManager.Merchandising.Item mi in CkAppShared.ckMng.ltMerchandising) {
        MerchandisingModel.DatatableMerchandising dtm = new MerchandisingModel.DatatableMerchandising();

        CornerkickManager.Club.MerchandisingItem cmi = clb.getMerchandisingItem(mi, iSeason);

        dtm.iIx = iIx++;
        dtm.iId = mi.iId;
        dtm.sName = mi.sName;
        dtm.sPriceBasic = mi.fPriceBuy.ToString("0.00") + " €";
        dtm.fPriceBuy = mi.fPriceBuy;
        dtm.fPriceSell = mi.fPriceBuy;
        if (cmi != null) {
          dtm.iPresent = cmi.iPresent;
          dtm.fPricePresentBuyAve = cmi.fPricePresentBuyAve;
          dtm.iValuePresent = (int)(cmi.fPricePresentBuyAve * cmi.iPresent);
          dtm.iSold = cmi.iSold;
          dtm.fPriceSell = cmi.fPrice;
          dtm.iItemIncome = cmi.iIncome;
          dtm.iWinLoose = cmi.iIncome - (int)((dtm.iPresent + dtm.iSold) * cmi.fPricePresentBuyAve);
          if (dtm.iSold > 0) dtm.sPriceSellAve = (cmi.iIncome / (float)dtm.iSold).ToString("0.00") + " €";
        }
        dtm.bPlayerJersey = mi.bPlayerJersey;

        ltItems.Add(dtm);
      }

      return ltItems;
    }

  }
}
