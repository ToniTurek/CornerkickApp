using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CornerkickApp.Shared.Models
{
  public class SponsorModel : LayoutModel
  {
    public bool bSound { get; set; }

    public List<string> ltSponsorBadgeMain { get; set; } = new List<string>();
    public List<string> ltSponsorBadgeBoard { get; set; } = new List<string>();

    public List<CornerkickManager.Finance.Sponsor> ltSponsorOffers { get; set; }
    public List<CornerkickManager.Finance.Sponsor> ltSponsorBoards { get; set; }
    public      CornerkickManager.Finance.Sponsor  sponsorMain { get; set; }
    public List<int> ltSponsorBoardIds { get; set; }
    public List<string> ltSponsorNames { get; set; }

    public string sEmblem { get; set; } = "";
    public string sColorJersey { get; set; } = "";

    public CkAppShared.Tutorial tutorial { get; set; }

    public class TableItemSponsorMain
    {
      public bool bOffer { get; set; }
      public int iIndex { get; set; }
      public byte iId { get; set; }
      public string sName { get; set; } = "";
      public int iMoneyYear { get; set; }
      public int iMoneyVic { get; set; }
      public int iMoneyCupWin { get; set; }
      public byte iYears { get; set; }
      public float fMood { get; set; }
    }

    public class TableItemSponsorBoard
    {
      public bool   bOffer { get; set; }
      public int    iIndex { get; set; }
      public byte   iId { get; set; }
      public string sName { get; set; } = "";
      public int    iMoneyVicHome { get; set; }
      public byte   nBoards { get; set; }
      public byte   iYears { get; set; }
      public float fMood { get; set; }
    }

    public class TableItemSponsorSpecial
    {
      public bool bOffer { get; set; }
      public int iIndex { get; set; }
      public byte iId { get; set; }
      public string sName { get; set; } = "";
      public int iMoney { get; set; }
      public string sCondition { get; set; } = "";
    }
  }
}
