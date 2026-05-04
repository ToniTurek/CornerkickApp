using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CornerkickApp.Shared.Models
{
  public class CupModel : LayoutModel
  {
    public int iClubId { get; set; }

    public string sCupName { get; set; } = "Nat. Pokal";
    public string sCupEmblem { get; set; } = "";
    public byte[]? bCupEmblem { get; set; }

    [Display(Name = "Saison: ")]
    public int iSeason { get; set; }

    [Display(Name = "Spieltag: ")]
    public int iMatchday { get; set; }

    [Display(Name = "Gruppe: ")]
    public byte iGroup { get; set; }

    public int iMatchdayCurrent { get; set; }
    public int nMdsGroup { get; set; }

    public List<SelectListItem> ddlSeason { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ddlLand { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ddlDivision { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ddlMatchdays { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ddlGroups { get; set; } = new List<SelectListItem>();
  }
}