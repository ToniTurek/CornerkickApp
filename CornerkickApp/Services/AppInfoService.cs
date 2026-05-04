using Microsoft.Maui.ApplicationModel;

namespace CornerkickApp.Services
{
  public class AppInfoService
  {
    public string Version => AppInfo.Current.VersionString;
    public string Build => AppInfo.Current.BuildString;
  }
}
