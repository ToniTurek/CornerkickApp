using CornerkickApp.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CornerkickApp.Components.Pages.Member
{
  public partial class Formation
  {
    [Parameter]
    public TeamModel.TeamData TeamData { get; set; } = new TeamModel.TeamData();

    [Parameter]
    public float fScale { get; set; } = 1f;

    float fHeightTot = 122;
    readonly string[] sPos = ["", "TW", "IV", "LV", "RV", "DM", "LM", "RM", "OM", "LA", "RA", "ST", "LIB", "OLV", "ORV", "ZM", "", "", "", "", "", "HS"];

    int iSelectedPlayer = -1;
    double startX, startY, offsetX, offsetY;

    void OnDragStart(Microsoft.AspNetCore.Components.Web.DragEventArgs args, int iPl)
    {
      iSelectedPlayer = iPl;

      startX = args.ClientX;
      startY = args.ClientY;
    }

    void OnDrag(Microsoft.AspNetCore.Components.Web.DragEventArgs args)
    {
      offsetX += args.ClientX - startX;
      offsetY += args.ClientY - startY;
    }

    void OnDragOver(Microsoft.AspNetCore.Components.Web.DragEventArgs args)
    {
      offsetX += args.ClientX - startX;
      offsetY += args.ClientY - startY;
    }

    void OnDragEnd(Microsoft.AspNetCore.Components.Web.DragEventArgs args)
    {
      offsetX += args.ClientX - startX;
      offsetY += args.ClientY - startY;
    }
  }
}
