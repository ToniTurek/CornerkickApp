using System.Threading;
using System.Threading.Tasks;
using System.IO;
using CommunityToolkit.Maui.Storage;

namespace CornerkickApp.Services
{
  public class FolderPickerAdapter : IFolderPickerService
  {
    private readonly IFolderPicker _picker;

    public FolderPickerAdapter(IFolderPicker picker)
    {
      _picker = picker;
    }

    public async Task<string> PickFolderAsync()
    {
      using var cts = new CancellationTokenSource();
      var result = await _picker.PickAsync(cts.Token);
      if (!result.IsSuccessful) return string.Empty;

      // Normalisiere Ergebnis als Pfad (kann platformabhängig sein)
      return result.Folder?.Path ?? string.Empty;
    }
  }
}