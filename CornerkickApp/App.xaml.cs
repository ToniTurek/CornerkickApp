namespace CornerkickApp
{
  public partial class App : Application
  {
    public App()
    {
      InitializeComponent();
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
      Window window = new Window(new MainPage()) { Title = "Cornerkick-Manager" };

      // Keep screen on while app is running
      window.Created += (s, e) => {
        DeviceDisplay.KeepScreenOn = true;
      };

      return window;
    }
  }
}
