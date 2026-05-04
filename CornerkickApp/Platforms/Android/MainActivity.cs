using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui;

namespace CornerkickApp;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges =
        ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
  protected override void OnActivityResult(
      int requestCode, Result resultCode, Intent data)
  {
    base.OnActivityResult(requestCode, resultCode, data);

    var picker = IPlatformApplication.Current.Services
        .GetService<IFolderPickerService>() as FolderPickerService;

    picker?.OnActivityResult(requestCode, resultCode, data);
  }
}
