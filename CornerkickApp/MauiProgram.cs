using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using CornerkickApp.Services;
using CornerkickApp.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Radzen;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.ComponentModel;

#if WINDOWS
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace CornerkickApp
{
  public static class MauiProgram
  {
    public static MauiApp CreateMauiApp()
    {
      var builder = MauiApp.CreateBuilder();
      builder
          .UseMauiApp<App>()
          .UseMauiCommunityToolkit()
          .ConfigureFonts(fonts => {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
          })/*
          .ConfigureEssentials(essentials =>
          {
            essentials.UseVersionTracking();
          })*/;

      builder.Services.AddMauiBlazorWebView();

#if DEBUG
      builder.Services.AddBlazorWebViewDeveloperTools();
      builder.Logging.AddDebug();
#endif

      builder.Services.AddScoped<Controllers.Shared.MyAuthenticationStateProvider>();
      builder.Services.AddScoped<Controllers.AmazonS3FileTransfer>();
#if ANDROID
      builder.Services.AddSingleton<IFolderPickerService, FolderPickerService>();
#else
      builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);
      builder.Services.AddSingleton<IFolderPickerService, FolderPickerAdapter>();
#endif
      //builder.Services.AddSingleton<WeatherForecastService>();
      builder.Services.AddScoped<Controllers.App.TriggerService>();
      builder.Services.AddSingleton<Controllers.Shared.Components.Headline.HeadlineController>();
      builder.Services.AddSingleton<Shared.Models.CkAppShared>();
      builder.Services.AddSingleton<AppInfoService>();
      builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://0.0.0.0.com") });

      builder.Services.AddRadzenComponents();
      //builder.Services.AddHttpClient();
      builder.Services.AddTransient<Components.Pages.Database>();

      builder.Services
          .AddBlazorise(options => {
            options.Immediate = true;
          })
          .AddBootstrap5Providers()
          .AddFontAwesomeIcons();

#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(wndLifeCycleBuilder =>
            {
                wndLifeCycleBuilder.OnWindowCreated(window =>
                {
                    IntPtr nativeWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    WindowId win32WindowsId = Win32Interop.GetWindowIdFromWindow(nativeWindowHandle);
                    AppWindow winuiAppWindow = AppWindow.GetFromWindowId(win32WindowsId);
                    if(winuiAppWindow.Presenter is OverlappedPresenter p)
                        p.Maximize();
                    else
                    {
                        const int width = 1200;
                        const int height = 800;
                        winuiAppWindow.MoveAndResize(new RectInt32(1920 / 2 - width / 2, 1080 / 2 - height / 2, width, height));
                    }
                });
            });
        });
#endif

      // Clean-up temporary files in app data directory (e.g. from old versions):
      DirectoryInfo d_save = new DirectoryInfo(Path.Combine(Controllers.App.getDocumentsDir, "save"));
      if (d_save.Exists) {
        string[] tmp_dirs_to_delete = Directory.GetDirectories(d_save.FullName, ".*", SearchOption.TopDirectoryOnly);
        foreach (var d_to_delete in tmp_dirs_to_delete) {
          Directory.Delete(d_to_delete, true);
        }
      }

      setMauiHomeDir();
      //Controllers.App appCk = new Controllers.App(builder.Configuration, sHomeDir: Controllers.App.getDocumentsDir);
      Controllers.App appCk = new Controllers.App(builder.Configuration, sHomeDir: AppDomain.CurrentDomain.BaseDirectory);
      //Controllers.App appCk = new Controllers.App(builder.Configuration, sHomeDir: FileSystem.Current.AppDataDirectory);
      appCk.start();

      // Set Version
      //Shared.Models.CkAppShared.sVersion = AppInfo.Current.VersionString;
      Shared.Models.CkAppShared.sVersion = Controllers.App.Version;

#if ANDROID
      // Keep screen on on Android while app is running
      DeviceDisplay.Current.KeepScreenOn = true;

      Platforms.Android.Initialize.initialize(Shared.Models.CkAppShared.ckMng);

#if DEBUG
      //CreateDemoMedia();

      string[] dirs = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.TopDirectoryOnly);
      foreach (var d in dirs) {
        //System.IO.FileInfo info = new System.IO.FileInfo(file);
        // Do something with the Folder or just add them to a list via nameoflist.add();
        Debug.Write("d> " + d);
      }
      string[] fils = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.TopDirectoryOnly);
      foreach (var f in fils) {
        //System.IO.FileInfo info = new System.IO.FileInfo(file);
        // Do something with the Folder or just add them to a list via nameoflist.add();
        Debug.Write("f> " + f);
      }
      /*
      using (WebClient client = new WebClient()) {
        client.DownloadFile(new Uri("https://www.pngitem.com/pimgs/m/185-1850014_free-sample-hd-png-download.png"), Path.Combine(Controllers.App.getDocumentsDir, "demo.png"));
      }

			File.Copy(Path.Combine(Controllers.App.getDocumentsDir, "demo.png"), Path.Combine(Controllers.App.getDocumentsDir, "demo.ckdbx"));
			*/
#endif
#endif

      //HomeModel.iFilesLoad = await Maui.Data.StartGameService.getFilesToLoad(bDatabase: false).Count;
      Task<List<string[]>> tkFilesToLoad = Task.Run(() => StartGameService.getFilesToLoad(bDatabase: false));
      tkFilesToLoad.Wait();
      Shared.Models.HomeModel.iFilesLoad = tkFilesToLoad.Result.Count;

      //Register needed elements for authentication:
      TokenStorage.RemoveToken();
      // This is the core functionality
      builder.Services.AddAuthorizationCore();
      // This is our custom provider
      builder.Services.AddScoped<MauiAuthenticationStateProvider, MauiAuthenticationStateProvider>();
      // Use our custom provider when the app needs an AuthenticationStateProvider
      builder.Services.AddScoped<AuthenticationStateProvider>(s
          => (MauiAuthenticationStateProvider)s.GetRequiredService<MauiAuthenticationStateProvider>());

      // Add device-specific services used by the CornerkickApp.Shared project
      builder.Services.AddSingleton<IFormFactor, FormFactor>();
      builder.Services.AddScoped<IAmazonS3Service, AmazonS3Service>();

      return builder.Build();
    }

    private static void setMauiHomeDir()
    {
#if ANDROID
      var path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments);
      if (path != null) Controllers.App.sMauiHomeDir = path.AbsolutePath;
      //Controllers.App.sMauiHomeDir = Android.OS.Environment.DirectoryDocuments;
      //Controllers.App.sMauiHomeDir = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
#endif
    }

#if DEBUG
    public static void CreateDemoMedia()
    {
      string sDemoEmbDir = Path.Combine(Controllers.App.sMauiHomeDir, "media_demo", "images", "emblems");
      Directory.CreateDirectory(sDemoEmbDir);

      string sDemoPrtDir = Path.Combine(Controllers.App.sMauiHomeDir, "media_demo", "images", "portraits");
      Directory.CreateDirectory(sDemoPrtDir);

      SixLabors.ImageSharp.Image img_demo = new Image<Rgba32>(100, 100);
      SixLabors.ImageSharp.Drawing.Star star = new SixLabors.ImageSharp.Drawing.Star(new SixLabors.ImageSharp.PointF(50, 50), 40, 20, 5);
      SixLabors.ImageSharp.PointF[] points = star.Points.ToArray();
      SixLabors.ImageSharp.Color[] colors =
      {
        SixLabors.ImageSharp.Color.Red, SixLabors.ImageSharp.Color.Yellow, SixLabors.ImageSharp.Color.Green, SixLabors.ImageSharp.Color.Blue, SixLabors.ImageSharp.Color.Purple,
        SixLabors.ImageSharp.Color.Red, SixLabors.ImageSharp.Color.Yellow, SixLabors.ImageSharp.Color.Green, SixLabors.ImageSharp.Color.Blue, SixLabors.ImageSharp.Color.Purple
      };

      PathGradientBrush brush = new(points, colors, SixLabors.ImageSharp.Color.White);

      img_demo.Mutate(x => x.Fill(brush));
      for (int i = 0; i < 100; i++) img_demo.Save(Path.Combine(sDemoEmbDir, i.ToString() + ".png"));
      for (int i = 0; i < 100; i++) img_demo.Save(Path.Combine(sDemoPrtDir, i.ToString() + ".png"));
    }
#endif
  }
}