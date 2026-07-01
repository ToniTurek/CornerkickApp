using CornerkickApp.Components.Shared.Components;
using CornerkickApp.Components.Shared.Layout;
using CornerkickApp.Shared.Models;
using CornerkickApp.Shared.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using CornerkickApp.Controllers.Member;

namespace CornerkickApp.Services
{
  public class Emblem
  {
    public int iId { get; set; }
    public int iId2 { get; set; }
    public int iId3 { get; set; }
    public byte[]? bEmblem { get; set; }
  }

  public static class ImageSharpHelpers
  {
    public static IImageProcessingContext ResizeDownTo(this IImageProcessingContext context, int maxWidth, int maxHeight)
    {
      var currentSize = context.GetCurrentSize();
      if (currentSize.Width > maxWidth || currentSize.Height > maxHeight) {
        context.Resize(new ResizeOptions() {
          Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max,
          Size = new SixLabors.ImageSharp.Size(maxWidth, maxHeight),
        });
      }
      return context;
    }
  }

  internal class StartGameService
  {
    internal static Task<List<string[]>> getFilesToLoad(bool bDatabase = false)
    {
      List<string[]> ltFiles = new List<string[]>();

      DirectoryInfo d;
      if (bDatabase) d = new DirectoryInfo(Path.Combine(CkAppShared.sAppDataDir, "database"));
      else           d = new DirectoryInfo(Path.Combine(CkAppShared.sAppDataDir, "save"));

      if (d.Exists) {
        string[] ltFileExt;
        if (bDatabase) {
          ltFileExt = ["*.ckdbx"/*, "*.xml.zip"*/];
        } else {
          ltFileExt = ["*.ckx"];
        }

        foreach (string sFileExt in ltFileExt) {
          //FileInfo[] ltCkxFiles = d.GetFiles(sFileExt).OrderBy(f => f.CreationTime).ToArray();
          FileInfo[] ltCkxFiles = d.GetFiles(sFileExt).OrderBy(f => f.LastWriteTime).ToArray();

          foreach (FileInfo ckx in ltCkxFiles) {
            string sMedia = "";
            if (bDatabase) {
              if (Controllers.App.checkMediaExist(ckx.Name)) sMedia = Controllers.App.getMediaDir(ckx.Name);
            }

            ltFiles.Add([Path.GetFileNameWithoutExtension(ckx.Name), ckx.FullName, ckx.LastWriteTime.ToString("g"), sMedia, sFileExt]);
          }
        }

        // Add XML database
        if (bDatabase) {
          DirectoryInfo[] ltCkXmlFolder = d.GetDirectories().OrderBy(f => f.LastWriteTime).ToArray();
          foreach (DirectoryInfo dxml in ltCkXmlFolder) {
            if (!File.Exists(Path.Combine(dxml.FullName, "General.xml"))) continue;

            string sMedia = "";
            if (Controllers.App.checkMediaExist(dxml.Name)) sMedia = Controllers.App.getMediaDir(dxml.Name);

            ltFiles.Add([Path.GetFileNameWithoutExtension(dxml.Name), dxml.FullName, dxml.LastWriteTime.ToString("g"), sMedia, "XML"]);
          }
        }
      }

      return Task.FromResult(ltFiles);
    }

    internal const string sMediaImgDir = "images";
    internal const string sMediaSoundsDir = "sounds";
    internal readonly static string[] sMediaImgTypeDir = new string[] { "cup_emblems", "emblems", "portraits" };
    public static int[] GetMediaFileCounts(string sMediaDir)
    {
      int[] iFileCounts = new int[sMediaImgTypeDir.Length];
      for (int i = 0; i < iFileCounts.Length; i++) iFileCounts[i] = -1;

#if ANDROID
      var folderUri = Android.Net.Uri.Parse(sMediaDir);
      if (folderUri == null) return iFileCounts;

      var context = Android.App.Application.Context;

      var folder = AndroidX.DocumentFile.Provider.DocumentFile.FromTreeUri(context, folderUri);

      if (folder == null || !folder.IsDirectory) return iFileCounts;

      var dirPics = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
      var dirDocs = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
      //var dir = Android.OS.Environment.GetExternalStoragePublicDirectory(sMediaDir);
      //var files = Directory.GetFiles(sMediaDir, "*", SearchOption.AllDirectories);
      var files = folder.ListFiles();
      var filesPics = Directory.GetFiles(dirPics.AbsolutePath, "*", SearchOption.AllDirectories);
      var filesDocs = Directory.GetFiles(dirDocs.AbsolutePath, "*", SearchOption.AllDirectories);
      iFileCounts[0] = files.Length;
      iFileCounts[1] = filesPics.Length;
      iFileCounts[2] = filesDocs.Length;
#else
    // Import images
    string[] sImgDirs = Directory.GetDirectories(sMediaDir, sMediaImgDir, SearchOption.AllDirectories);
    if (sImgDirs != null && sImgDirs.Length > 0) {
      int iType = 0;
      foreach (string img_type_dir in sMediaImgTypeDir) {
        string sImgTypeDir = Path.Combine(sImgDirs[0], img_type_dir);
        if (!Directory.Exists(sImgTypeDir)) continue;

        int iCount = 0;
        foreach (string img_str in Directory.GetFiles(sImgTypeDir, "*.*")) {
          iCount++;
        }

        iFileCounts[iType++] = iCount;
      }
    }
#endif

      return iFileCounts;
    }

#if ANDROID
    void Traverse(AndroidX.DocumentFile.Provider.DocumentFile dir)
    {
      foreach (var file in dir.ListFiles()) {
        if (file.IsDirectory) Traverse(file);
        else                  Console.WriteLine(file.Name);
      }
    }
#endif

    public static async Task<int[]> copyMediaData(string sourcePath, string targetPath = "", IProgress<int[]>? progress = null, bool bClean = true, bool bCount = false, int iFilesTotal = -1)
    {
      CkAppShared.ckMng.tl.writeLog("Import media from " + sourcePath + " to " + targetPath);

      if (!bCount && iFilesTotal < 0) {
        int[] iFilesCount = await copyMediaData(sourcePath, progress: progress, bCount: true);
        foreach (int i in iFilesCount) iFilesTotal += i < 0 ? 0 : i;
        if (iFilesTotal == 0) return [];
      }

      if (!bCount) {
        if (Directory.Exists(targetPath) && bClean) Directory.Delete(targetPath, true);
        Directory.CreateDirectory(targetPath);
        Directory.CreateDirectory(Path.Combine(targetPath, sMediaImgDir));
      }

      int[] iFileCounts = new int[sMediaImgTypeDir.Length];
      int processCount = await Task.Run<int>(() => {
        int iCount = 0;
        int iType = 0;

        // Import images
        string[] sImgDirs = Directory.GetDirectories(sourcePath, sMediaImgDir, SearchOption.AllDirectories);
        if (sImgDirs != null && sImgDirs.Length > 0) {
          string sImgDir = sImgDirs[0];
          foreach (string img_type_dir in sMediaImgTypeDir) {
            if (!Directory.Exists(Path.Combine(sImgDir, img_type_dir))) {
              iType++;
              continue;
            }

            if (!bCount) Directory.CreateDirectory(Path.Combine(targetPath, sMediaImgDir, img_type_dir));

            int jCount = 0;
            foreach (string img_str in Directory.GetFiles(Path.Combine(sImgDir, img_type_dir), "*")) {
              if (progress != null) progress.Report([iFilesTotal, iCount++, iType]);
              jCount++;

              // Continue if only counting
              if (bCount) continue;

              SixLabors.ImageSharp.Image? six_img = null;
              try {
                six_img = SixLabors.ImageSharp.Image.Load(img_str);
              } catch (Exception ex) {
                CkAppShared.ckMng.tl.writeLog("Error loading media file: " + img_str, bError: true);
              }
              //six_img.Mutate(x => x.Resize(24, 24));

              if (six_img != null) {
                string sTargetFile = Path.Combine(targetPath, sMediaImgDir, img_type_dir, Path.GetFileNameWithoutExtension(img_str) + ".png");

                try {
                  six_img.Mutate(x => x.ResizeDownTo(0, 128));
                  six_img.SaveAsPng(sTargetFile);
                  //File.Copy(png, sTargetFile, true);
                } catch (Exception ex) {
                  CkAppShared.ckMng.tl.writeLog("Error importing media file: " + img_str + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, bError: true);
                }
              }
            }

            iFileCounts[iType] = jCount;
            iType++;
          }
        }

        return iCount;
      });

      return iFileCounts;
    }

    internal static async Task<List<string[]>> GetFilesInCloudAsync(Controllers.Shared.MyAuthenticationStateProvider _auth, AmazonS3Credentials? as3credentials)
    {
      if (as3credentials == null) return new List<string[]> { };
      if (string.IsNullOrEmpty(as3credentials.sAwsKeyId))     return new List<string[]> { };
      if (string.IsNullOrEmpty(as3credentials.sAwsSecretKey)) return new List<string[]> { };

      return await GetFilesInCloudAsync(_auth, new Controllers.AmazonS3FileTransfer(as3credentials.sAwsKeyId, as3credentials.sAwsSecretKey));
    }
    internal static async Task<List<string[]>> GetFilesInCloudAsync(Controllers.Shared.MyAuthenticationStateProvider _auth, Controllers.AmazonS3FileTransfer as3)
    {
      List<string[]> ltFiles = new List<string[]>();

      if (!_auth.IsAuthenticated()) return ltFiles;

      var uidentity = _auth.GetIdentity().Result;
      if (uidentity == null) return ltFiles;

      string? sUid = uidentity.Name;
      if (string.IsNullOrEmpty(sUid)) return ltFiles;

      IList<Amazon.S3.Model.S3Object> ltS3Obj = await as3.listFilesAsync("app_save/" + Controllers.Member.MemberController.GetUserIdStandaloneApp(sUid)/* + "/*.ckx"*/);

      ltFiles = new List<string[]>();
      foreach (Amazon.S3.Model.S3Object s3o in ltS3Obj) {
        ltFiles.Add([Path.GetFileNameWithoutExtension(s3o.Key), s3o.Key, s3o.LastModified.ToString("g"), s3o.Size.ToString(), s3o.BucketName]);
      }

      return ltFiles;
    }

#if false
    public static async Task<string> uploadGameAsync(string sFileCk, Controllers.Shared.MyAuthenticationStateProvider _auth, AmazonS3FileTransfer as3, IAmazonS3Service AmazonS3Service)
    {
      if (string.IsNullOrEmpty(sFileCk)) return "File empty";

      try {
        // Add extension if not already
        if (!Path.GetExtension(sFileCk).Equals(".ckx")) sFileCk += ".ckx";

        var uidentity = _auth.GetIdentity().Result;
        if (uidentity == null) return "Failed get identity";

        string? sUid = uidentity.Name;
        if (string.IsNullOrEmpty(sUid)) return "User id empty";

        AmazonS3Credentials? as3credentials = await AmazonS3Service.GetAmazonS3CredentialsAsync();
        as3 = new AmazonS3FileTransfer(as3credentials.sAwsKeyId, as3credentials.sAwsSecretKey);
        await as3.uploadFileAsync(sFileCk, "app_save/" + Controllers.Member.MemberController.GetUserIdStandaloneApp(sUid) + "/" + Path.GetFileName(sFileCk), "application/zip");

        return "";
      } catch (Exception e) {
        string sErrorMsg = "ERROR saving game. Message: " + e.Message + Environment.NewLine + e.StackTrace;
        CkAppShared.ckMng.tl.writeLog(sErrorMsg);

        return sErrorMsg;
      }
    }
#endif

    public static readonly byte[] iNationsDefaultDb = [
      36, // GER
      29, // ENG
      30, // ESP
      45, // ITA
      33, // FRA
      54, // NED
      13, // BRA
       3  // ARG
    ];

    internal static async Task<bool> loadDatabase(IProgress<int[]> progress, string? sFileCkSelected = null)
    {
      CornerkickManager.Main ckMng = Controllers.App.getCkMngDefault(bContinuingTime: false);
      ckMng.settings.sHomeDir = CkAppShared.ckMng.settings.sHomeDir;
      ckMng.settings.sLogDir  = CkAppShared.ckMng.settings.sLogDir;

#if ANDROID
      Platforms.Android.Initialize.initialize(ckMng);
#endif

      if (string.IsNullOrEmpty(sFileCkSelected) || sFileCkSelected.Equals("default")) {
        CkAppShared.iNations = iNationsDefaultDb;
        ckMng = await Controllers.App.setCkMngToDefault(ckMng, progress);
      } else {
        try {
          ckMng.io.load(sFileCkSelected, bDatabase: true/*, sMediaDir: Path.Combine(Startup.sWwwRootDir, "Content", "Uploads")*/);

          // DEBUG
          /*
          foreach (CornerkickManager.Cup cup in ckMng.ltCups.FindAll(c => c.iId == CkAppShared.iCupIdLeague)) {
            cup.settings.iStart = 1;
            cup.settings.iEnd = -4;
            cup.settings.nGroups = 1;
            cup.settings.iDayOfWeek = 6; // Saturday
            cup.settings.bGroupsTwoGames = true;
          }
          */

          foreach (CornerkickManager.Player pl in CkAppShared.ckMng.ltPlayer) {
            byte iPos = pl.plGame.getMainPosition();
            if (iPos == 2) {  // IV
              if (pl.plGame.fExperiencePos[3] < 0.3f && pl.plGame.fFootL > 0.9f) pl.plGame.fExperiencePos[3] += 0.3f;  // LV
              if (pl.plGame.fExperiencePos[4] < 0.3f && pl.plGame.fFootR > 0.9f) pl.plGame.fExperiencePos[4] += 0.3f;  // LV
              if (pl.plGame.fExperiencePos[5] < 0.2) pl.plGame.fExperiencePos[5] += 0.2f;  // DM
            } else if (iPos == 3) {  // LV
              if (pl.plGame.fExperiencePos[6] < 0.3f) pl.plGame.fExperiencePos[6] += 0.3f;  // LM
              if (pl.plGame.fExperiencePos[4] < 0.3f && pl.plGame.fFootR > 0.9f) pl.plGame.fExperiencePos[4] += 0.3f;  // RV
            } else if (iPos == 4) {  // RV
              if (pl.plGame.fExperiencePos[7] < 0.3f) pl.plGame.fExperiencePos[7] += 0.3f;  // RM
              if (pl.plGame.fExperiencePos[3] < 0.3f && pl.plGame.fFootL > 0.9f) pl.plGame.fExperiencePos[3] += 0.3f;  // RV
            } else if (iPos == 5) {  // DM
              if (pl.plGame.fExperiencePos[2] < 0.2f) pl.plGame.fExperiencePos[2] += 0.2f;  // RM
            } else if (iPos == 6) {  // LM
              if (pl.plGame.fExperiencePos[ 3] < 0.3f) pl.plGame.fExperiencePos[ 3] += 0.3f;  // LV
              if (pl.plGame.fExperiencePos[ 9] < 0.3f) pl.plGame.fExperiencePos[ 9] += 0.3f;  // LA
              if (pl.plGame.fExperiencePos[ 8] < 0.3f) pl.plGame.fExperiencePos[ 8] += 0.3f;  // OM
            } else if (iPos == 7) {  // RM
              if (pl.plGame.fExperiencePos[ 4] < 0.3f) pl.plGame.fExperiencePos[ 4] += 0.3f;  // RV
              if (pl.plGame.fExperiencePos[10] < 0.3f) pl.plGame.fExperiencePos[10] += 0.3f;  // RA
              if (pl.plGame.fExperiencePos[ 8] < 0.3f) pl.plGame.fExperiencePos[ 8] += 0.3f;  // OM
            } else if (iPos == 9) {  // LA
              if (pl.plGame.fExperiencePos[ 3] < 0.2f) pl.plGame.fExperiencePos[ 3] += 0.2f;  // LV
              if (pl.plGame.fExperiencePos[ 9] < 0.2f) pl.plGame.fExperiencePos[ 9] += 0.3f;  // LA
            } else if (iPos == 10) {  // RA
              if (pl.plGame.fExperiencePos[ 4] < 0.2f) pl.plGame.fExperiencePos[ 4] += 0.2f;  // RV
              if (pl.plGame.fExperiencePos[ 7] < 0.2f) pl.plGame.fExperiencePos[ 7] += 0.3f;  // RA
            } else if (iPos == 8) {  // OM
              if (pl.plGame.fExperiencePos[5] < 0.2f) pl.plGame.fExperiencePos[4] += 0.2f;  // RV
              if (pl.plGame.fExperiencePos[6] < 0.3f && pl.plGame.fFootL > 0.9f) pl.plGame.fExperiencePos[6] += 0.4f;  // LM
              if (pl.plGame.fExperiencePos[7] < 0.3f && pl.plGame.fFootR > 0.9f) pl.plGame.fExperiencePos[7] += 0.4f;  // RM
            }
          }

          // Order cups
          //ckMng.ltCups = ckMng.ltCups.OrderBy(c => c.iId2).ThenBy(c => c.iId3).ThenBy(c => c.iId).ToList();
        } catch (Exception e) {
          string sErrorMsg = "ERROR loading database. Message: " + e.Message + Environment.NewLine + e.StackTrace;
          ckMng.tl.writeLog(sErrorMsg);
          //writeLog(Path.Combine(Controllers.App.getDocumentsDir, "ckapp.log"), sErrorMsg);

          //return View("Log");
        }
      }

      // Set staff level based on club division
      foreach (CornerkickManager.Club clb in ckMng.ltClubs) {
        byte iStaffLevel = (byte)Math.Max(4 - clb.iDivision, 0);
        clb.staff.iCoTrainer     = iStaffLevel;
        clb.staff.iCondiTrainer  = iStaffLevel;
        clb.staff.iMentalTrainer = iStaffLevel;
        clb.staff.iPhysio        = iStaffLevel;
        clb.staff.iJouthTrainer  = iStaffLevel;
        clb.staff.iJouthScouting = iStaffLevel;
        clb.staff.iKibitzer = 0;

        if (clb.staff.ltDoctor == null) clb.staff.ltDoctor = new List<CornerkickManager.Main.Staff.Doctor>();
        clb.staff.ltDoctor.Clear();
        //for (int i = 0; i < iStaffLevel; i++) clb.staff.ltDoctor.Add(new CornerkickManager.Main.Staff.Doctor() { sName = "Dr. Müller-Wohllaib", iId = -iStaffLevel, iSkillMuscle = iStaffLevel, iSkillTendons = iStaffLevel, iSkillFracture = iStaffLevel, iSkillInternist = iStaffLevel });
        for (int i = 0; i < iStaffLevel - 1; i++) {
          if (i >= CornerkickManager.Main.staff.ltDoctor.Count) break;
          clb.staff.ltDoctor.Add(CornerkickManager.Main.staff.ltDoctor[i]);
        }

        /*
        clb.stadium = new CornerkickGame.Stadium();
        clb.stadium.sName = clb.sName + " Stadion";

        for (byte iB = 0; iB < 3; iB++) {
          clb.stadium.blocks[iB].iSeats = 2000;
          clb.stadium.blocks[iB].iType = 1;
        }
        for (byte iB = 3; iB < 8; iB++) clb.stadium.blocks[iB].iSeats = 1000;

        clb.stadium.facility.iTicketcounter = 1;
        clb.stadium.facility.iCarpark = 50;
        */
      }

      /*
      foreach (CornerkickManager.Cup c in ckMng.ltCups.FindAll(c => c.iId == CkAppShared.iCupIdLeague)) {
        c.settings.nGroups = 1;
        c.settings.bGroupsTwoGames = true;
        c.settings.iDayOfWeek = 6; // Saturday
        c.settings.iBonusCupWin = 20000000;
      }

      foreach (CornerkickManager.Cup c in ckMng.ltCups.FindAll(c => c.iId == CkAppShared.iCupIdNatCup)) {
        c.settings.bKo = true;
        c.settings.iDayOfWeek = 6; // Saturday
        c.settings.iBonusCupWin = 15000000;
        c.settings.bBonusReleaseCupWinInKo = true;
        c.settings.iTvBonus = 250000;
        c.settings.nYellowCardSuspension = 2;
      }
      */

      // Assign random portrait if none
      for (int iPl = 0; iPl < ckMng.ltPlayer.Count; iPl++) {
        Controllers.Member.PlayerController.setRandomPortrait(ckMng.ltPlayer[iPl]);
      }

      CkAppShared.ckMng = ckMng;
      Controllers.Tool.setNations();

      return true;
    }

    internal static async Task<bool> loadMedia(string dbfile, List<Emblem> ltCupEmblem, List<Emblem> ltClubEmblem, List<Emblem> ltPlayerPortrait, IProgress<int[]> progress)
    {
      if (!Controllers.App.checkMediaExist(dbfile)) return false;

      int processCount = await Task.Run<int>(() => {
        int iCount = 0;

        foreach (CornerkickManager.Cup cup in CkAppShared.ckMng.ltCups) {
          progress.Report([0, iCount]);

          try {
            using (var fileStream = new FileStream(Path.Combine(Controllers.App.getMediaDir(dbfile), "images", "cup_emblems", cup.iId.ToString() + "_" + cup.iId2.ToString() + "_" + cup.iId3.ToString() + ".png"), FileMode.Open)) {
              using (MemoryStream ms = new MemoryStream()) {
                fileStream.CopyTo(ms);
                ltCupEmblem.Add(new Emblem() { iId = cup.iId, iId2 = cup.iId2, iId3 = cup.iId3, bEmblem = ms.ToArray() });
              }
            }
          } catch (Exception ex) {
            Console.WriteLine(ex.Message);
          }

          iCount++;
        }
        foreach (CornerkickManager.Club clb in CkAppShared.ckMng.ltClubs) {
          progress.Report([1, iCount]);

          try {
            using (var fileStream = new FileStream(Path.Combine(Controllers.App.getMediaDir(dbfile), "images", "emblems", clb.iId.ToString() + ".png"), FileMode.Open)) {
              using (MemoryStream ms = new MemoryStream()) {
                fileStream.CopyTo(ms);
                ltClubEmblem.Add(new Emblem() { iId = clb.iId, bEmblem = ms.ToArray() });
              }
            }
          } catch (Exception ex) {
            Console.WriteLine(ex.Message);
          }

          iCount++;
        }
        foreach (CornerkickManager.Player pl in CkAppShared.ckMng.ltPlayer) {
          progress.Report([2, iCount]);

          try {
            using (var fileStream = new FileStream(Path.Combine(Controllers.App.getMediaDir(dbfile), "images", "portraits", pl.plGame.iId.ToString() + ".png"), FileMode.Open)) {
              using (MemoryStream ms = new MemoryStream()) {
                fileStream.CopyTo(ms);
                ltPlayerPortrait.Add(new Emblem() { iId = pl.plGame.iId, bEmblem = ms.ToArray() });
              }
            }
          } catch (Exception ex) {
            Console.WriteLine(ex.Message);
          }

          iCount++;
        }

        return iCount;
      });

      return true;
    }

    internal static async Task<byte[]?> loadMediaDataCup(string dbfile, CornerkickManager.Cup cup)
    {
      return await loadMediaDataCup(dbfile, cup.iId, cup.iId2, cup.iId3);
    }
    internal static async Task<byte[]?> loadMediaDataCup(string dbfile, int iId, int iId2, int iId3)
    {
      /*
      if (!Controllers.App.checkMediaExist(dbfile)) return null;

      string sMediaFile = Path.Combine(Controllers.App.getMediaDir(dbfile), "images", "cup_emblems", iId.ToString() + "_" + iId2.ToString() + "_" + iId3.ToString() + ".png");
      try {
        return File.ReadAllBytes(sMediaFile);
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }

      try {
        using (var fileStream = new FileStream(sMediaFile, FileMode.Open)) {
          using (MemoryStream ms = new MemoryStream()) {
            await fileStream.CopyToAsync(ms);
            return ms.ToArray();
          }
        }
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }

      return null;
      */
      return CupController.getCupEmblemB(iId, iId2, iId3, dbfile);
    }

    internal static async Task<byte[]?> loadMediaDataClub(string dbfile, int iId)
    {
      if (!Controllers.App.checkMediaExist(dbfile)) return null;

      string sMediaFile = Path.Combine(Controllers.App.getMediaDir(dbfile), "images", "emblems", iId.ToString() + ".png");
      try {
        return File.ReadAllBytes(sMediaFile);
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }

      try {
        using (var fileStream = new FileStream(sMediaFile, FileMode.Open)) {
          using (MemoryStream ms = new MemoryStream()) {
            await fileStream.CopyToAsync(ms);
            return ms.ToArray();
          }
        }
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }

      return null;
    }

    internal static async Task<byte[]?> loadMediaDataPlayer(string dbfile, int iId)
    {
      if (!Controllers.App.checkMediaExist(dbfile)) return null;

      string sMediaFile = Path.Combine(Controllers.App.getMediaDir(dbfile), "images", "portraits", iId.ToString() + ".png");
      try {
        return File.ReadAllBytes(sMediaFile);
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }

      try {
        using (var fileStream = new FileStream(sMediaFile, FileMode.Open)) {
          using (MemoryStream ms = new MemoryStream()) {
            await fileStream.CopyToAsync(ms);
            return ms.ToArray();
          }
        }
      } catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }

      return null;
    }
  }
}
