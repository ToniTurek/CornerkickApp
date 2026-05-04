using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CornerkickApp.Shared.Models
{
  public class TransferModel : LayoutModel
  {
    public bool bTransferlistOpen { get; set; }

    [Display(Name = "Laufzeit [a]:")]
    public int iContractYears { get; set; }
    [Display(Name = "Gebotenes Gehalt:")]
    public int iContractSalaryOffer { get; set; }

    [Display(Name = "Ablöse [mio. €]:")]
    [DisplayFormat(DataFormatString = "{0:0,0}")]
    public int iTransferFee { get; set; }
    public int iTransferFeeSecretBalance { get; set; }

    public int iOfferClubId { get; set; }

    // Filter
    public List<SelectListItem> sliFilterPos { get; set; }
    [Display(Name = "Pos.: ")]
    public int iFilterPos { get; set; }

    public List<SelectListItem> sliFilterFType { get; set; }
    public int iFilterFType { get; set; }
    public List<SelectListItem> sliFilterF { get; set; }
    public int iFilterF { get; set; }

    public List<SelectListItem> sliFilterNations { get; set; } = new List<SelectListItem>();
    public int iFilterNation { get; set; }

    public List<SelectListItem> sliFilterClubs { get; set; } = new List<SelectListItem>();
    public int iFilterClub { get; set; }

    public bool bNation { get; set; }
    public bool bNominationPossible { get; set; }

    public bool bSound { get; set; }

    public TransferModel()
    {
      iFilterPos = 0;
      // Positionen zu Dropdown Menü hinzufügen
      sliFilterPos = new List<SelectListItem>() { new SelectListItem { Text = "alle", Value = iFilterPos.ToString() } };
      for (int iPos = 1; iPos < 12; iPos++) {
        sliFilterPos.Add(new SelectListItem { Text = CornerkickManager.Main.sPosition[iPos], Value = iPos.ToString() });
      }

      iFilterFType = -1;
      // Positionen zu Dropdown Menü hinzufügen
      sliFilterFType = new List<SelectListItem>() { new SelectListItem { Text = "-", Value = iFilterFType.ToString() } };
      for (int iF = 0; iF < CornerkickManager.PlayerTool.sSkills.Length - 1; iF++) {
        sliFilterFType.Add(new SelectListItem { Text = CornerkickManager.PlayerTool.sSkills[iF], Value = iF.ToString() });
      }

      sliFilterF = new List<SelectListItem>();
      // Positionen zu Dropdown Menü hinzufügen
      for (int iF = 1; iF < 11; iF++) {
        sliFilterF.Add(new SelectListItem { Text = iF.ToString(), Value = iF.ToString() });
      }
    }

    public class TransferItem
    {
      public List<Offer>? ltOffers { get; set; }
      public int iPlayerId { get; set; }
      public int iOffer { get; set; }  // -2: not on transfer list, -1: negotiation cancelled, +1: already offered, +2: own player with offers, +3: own player of nation
      public int iIx { get; set; }
      public DateTime dt { get; set; }
      public string sName { get; set; } = "";
      public string sPos { get; set; } = "";
      public float fStrength { get; set; }
      public float fStrengthIdeal { get; set; }
      public float fTalentAve { get; set; }
      public string sClubName { get; set; } = "";
      public int iValue { get; set; }
      public int iFixtransferfee { get; set; }
      public string sNat { get; set; } = "";
      public float fAge { get; set; }
      public bool bEndingContract { get; set; }
      public bool bOwnPlayer { get; set; }

      public class Offer
      {
        public int iIx { get; set; }
        public DateTime dt { get; set; } // Date offer was made
        public int iFee { get; set; }
        public int iFeeSecret { get; set; }
        public bool bNextSeason { get; set; } // Contract starts next season
        public int iClubId { get; set; }
        public string sClubName { get; set; } = "";
      }
    }

    public class DatatableEntryTransferDetails
    {
      public int i { get; set; }
      public int iPlayerId { get; set; }
      public int iClubId { get; set; }
      public string club { get; set; } = "";
      public string fee { get; set; } = "";
    }
  }
}
