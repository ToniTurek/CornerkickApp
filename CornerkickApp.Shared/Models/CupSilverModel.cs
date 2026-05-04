using System.Collections.Generic;

namespace CornerkickApp.Shared.Models
{
  public class CupSilverModel : LayoutModel
  {
    public bool bOk { get; set; }

    //public List<SelectListItem> ddlSeason { get; set; }
    public int iSeason { get; set; }

    //public List<SelectListItem> ddlGroup { get; set; }
    public int iGroup { get; set; }

    //public List<SelectListItem> ddlMatchday { get; set; }
    public int iMatchday { get; set; }

    public CupSilverModel()
    {
      //ddlGroup = new List<SelectListItem>();
      //for (byte iG = 0; iG < 8; iG++) ddlGroup.Add(new SelectListItem { Text = ((char)(65 + iG)).ToString(), Value = iG.ToString() });
    }
  }
}
