rem cd C:\Users\sjan\source\repos\test\MauiBlazorWeb_8\CornerkickApp

$dirs = @(CornerkickApp.Components, CornerkickApp.Controllers, CornerkickApp.Shared, ..\CornerkickGame, ..\CornerkickManager)
foreach ($d in $dirs) {
    powershell -Command Write-Output $number
    $obj = "$d\obj\Release"
    powershell -Command Remove-Item $obj -Recurse
}
cd CornerkickApp
dotnet publish -f net9.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore="C:\Users\sjan\AppData\Local\Xamarin\Mono for Android\Keystore\ckAndroidKeystore\ckAndroidKeystore.keystore" -p:AndroidSigningKeyAlias=ckandroidkeystore -p:AndroidSigningKeyPass=!Androidck1 -p:AndroidSigningStorePass=!Androidck1 -p:AndroidPackageFormats=aab -p:ApplicationId=com.cornerkick.maui

pause
