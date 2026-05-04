using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CornerkickApp.Shared.Models
{
  public class NewGameModel : LayoutModel
  {
    //public string ReturnUrl { get; set; }

    [Required]
    //[DataType(DataType.PostalCode)]
    [Display(Name = "Anzahl Spieler")]
    public int iNoOfUser { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Vorname")]
    public string sFirstName { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Nachname")]
    public string sName { get; set; }

    //public List<SelectListItem> sliCountries { get; } = new List<SelectListItem>();

    [Required]
    [Display(Name = "Land")]
    public int iLand { get; set; }

    [Display(Name = "Verein")]
    public int iClubId { get; set; }
    //public List<SelectListItem> sliClubs { get; } = new List<SelectListItem>();

    [Display(Name = "Vereinsausstattung")]
    public int iStartMode { get; set; }
    //public List<SelectListItem> sliStartmode { get; } = new List<SelectListItem>();

    [Display(Name = "Schwierigkeitsgrad")]
    public int iLevel { get; set; }
    //public List<SelectListItem> sliLevel { get; } = new List<SelectListItem>();

    [Display(Name = "Scouting")]
    public bool bScouting { get; set; }

    public NewGameModel()
    {
      iLand = 36;

#if _WebApp
      foreach (int iN in CkAppShared.iNations) {
        string sLand = "Land " + iN.ToString();
        if (CornerkickManager.Main.sLand != null && iN < CornerkickManager.Main.sLand.Length) sLand = CornerkickManager.Main.sLand[iN];

        //sliCountries.Add(new SelectListItem { Text = sLand, Value = iN.ToString(), Selected = iN == iLand });
      }
#else
      for (int iN = 0; iN < CornerkickManager.Main.sLand.Length; iN++) {
        CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, iN, 0);
        if (league == null) continue;

        string sLand = "Land " + iN.ToString();
        if (CornerkickManager.Main.sLand != null) sLand = CornerkickManager.Main.sLand[iN];

        //sliCountries.Add(new SelectListItem { Text = sLand, Value = iN.ToString(), Selected = iN == iLand });
      }
#endif

      // Start mode
      //sliStartmode.Add(new SelectListItem { Text = "Neu", Value = "0" });
      //sliStartmode.Add(new SelectListItem { Text = "Original", Value = "1" });

      // Difficulty level
      //sliLevel.Add(new SelectListItem { Text = "Fußball?", Value = "0" });
      //sliLevel.Add(new SelectListItem { Text = "Stammtischtrainer", Value = "1", Selected = true });
      //sliLevel.Add(new SelectListItem { Text = "Erfahrener Trainer", Value = "2" });
      //sliLevel.Add(new SelectListItem { Text = "Lichtgestalt", Value = "3" });
    }
  }
}
