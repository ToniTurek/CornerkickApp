using System.Collections.Generic;

namespace CornerkickApp.Shared.Models
{
  public class CupWcModel : LayoutModel
  {
    //public List<SelectListItem> ddlSeason { get; set; }
    public int iSeason { get; set; }

    //public List<SelectListItem> ddlGroup { get; set; }
    public int iGroup { get; set; }

    //public List<SelectListItem> ddlMatchday { get; set; }
    public int iMatchday { get; set; }

    public CupWcModel()
    {
      /*
      ddlGroup = new List<SelectListItem>();
      ddlGroup.Add(new SelectListItem { Text = "A", Value = "0" });
      ddlGroup.Add(new SelectListItem { Text = "B", Value = "1" });

      ddlMatchday = new List<SelectListItem>();
      for (int iMd = 0; iMd < 3; iMd++) {
        ddlMatchday.Add(new SelectListItem { Text = (iMd + 1).ToString(), Value = (iMd + 1).ToString() });
      }
      ddlMatchday.Add(new SelectListItem { Text = "Halbfinale", Value = "4" });
      ddlMatchday.Add(new SelectListItem { Text = "Finale", Value = "5" });
      */
    }
  }
}