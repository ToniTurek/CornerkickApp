using Android.App;
using Android.Content;
using Microsoft.Maui.ApplicationModel;

namespace CornerkickApp;

public class FolderPickerService : IFolderPickerService
{
  const int RequestCode = 1234;
  TaskCompletionSource<string>? _tcs;

  public Task<string> PickFolderAsync()
  {
    _tcs = new TaskCompletionSource<string>();

    var intent = new Intent(Intent.ActionOpenDocumentTree);
    intent.AddFlags(
        ActivityFlags.GrantReadUriPermission |
        ActivityFlags.GrantWriteUriPermission |
        ActivityFlags.GrantPersistableUriPermission);

    Platform.CurrentActivity.StartActivityForResult(intent, RequestCode);
    return _tcs.Task;
  }

  public void OnActivityResult(int requestCode, Result resultCode, Intent data)
  {
    if (requestCode != RequestCode || resultCode != Result.Ok)
      return;

    var uri = data.Data;

    var flags = data.Flags & (ActivityFlags.GrantReadUriPermission |
                              ActivityFlags.GrantWriteUriPermission);

    Android.App.Application.Context.ContentResolver
        .TakePersistableUriPermission(uri, flags);

    _tcs?.TrySetResult(uri.ToString());
  }
}
