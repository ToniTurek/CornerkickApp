using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CornerkickApp.Shared.Models
{
  public class TacticModel : LayoutModel
  {
    public CornerkickGame.Tactic? tactic { get; set; }

    public List<SelectListItem>? sliSystem { get; set; }
    public int iSystem { get; set; }

    public int[]? iTactics { get; set; }

    public bool bGame { get; set; }

    public List<SelectListItem>[]? ltDdlStandards { get; set; }
    public int[]? iStandards { get; set; }

    // Auto substitutions
    public List<SelectListItem>[]? ddlAutoSubsOut { get; set; } = Array.Empty<List<SelectListItem>>();
    public int[]? iAutoSubsOut { get; set; }

    public List<SelectListItem>[]? ddlAutoSubsIn { get; set; } = Array.Empty<List<SelectListItem>>();
    public int[]? iAutoSubsIn { get; set; }

    public int[]? iAutoSubsMin { get; set; }

    public CkAppShared.Tutorial? tutorial { get; set; }
  }
}
