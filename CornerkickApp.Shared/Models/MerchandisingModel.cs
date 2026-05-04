using System.Collections.Generic;

namespace CornerkickApp.Shared.Models
{
  public class MerchandisingModel : LayoutModel
  {
    public List<CornerkickManager.Club.MerchandisingItem> ltClubMerchandisingItems;

    public int iItem { get; set; }
    public List<SelectListItem> sliMerchandisingItems { get; set; }

    public CornerkickManager.Club.MerchandisingMarketer marketer { get; set; }
    public int iMarketerMoney { get; set; }

    public bool bFanshopsAvailable { get; set; }
    public bool bMarketer { get; set; }
    public float fBalanceSecretFracMerchandisingIncome { get; set; }

    public List<SelectListItem> sctSeason { get; set; }
    public int iSeason { get; set; }

    public CkAppShared.Tutorial tutorial { get; set; }

    public class DatatableMerchandising
    {
      public int iIx { get; set; }
      public int iId { get; set; }
      public string sName { get; set; }
      public int iPresent { get; set; }
      public int iValuePresent { get; set; }
      public int iAmountBuy { get; set; }
      public float fPricePresentBuyAve { get; set; }
      public int iSold { get; set; }
      public string sPriceBasic { get; set; }
      public float fPriceBuy { get; set; }
      public float fPriceBuyTotal => fPriceBuy * iAmountBuy;
      public float fPriceSell { get; set; }
      public int iItemIncome { get; set; }
      public int iWinLoose { get; set; }
      public string sPriceSellAve { get; set; }
      public bool bPlayerJersey { get; set; }
    }

    public MerchandisingModel()
    {
      sliMerchandisingItems = new List<SelectListItem>();

      foreach (CornerkickManager.Merchandising.Item mi in CkAppShared.ckMng.ltMerchandising) {
        sliMerchandisingItems.Add(new SelectListItem { Text = mi.sName, Value = mi.iId.ToString() });
      }
    }
  }
}
