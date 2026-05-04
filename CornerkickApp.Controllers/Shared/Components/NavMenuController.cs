using CornerkickApp.Shared.Models;

namespace CornerkickApp.Controllers.Shared.Components
{
  public class NavMenuController
  {
    public static byte Get(CornerkickManager.User usr)
    {
      return usr.iResp;
    }
  }
}
