using CornerkickApp.Shared.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndroidApp = Android.App;
using AndroidNet = Android.Net;
using Android.Content;
using Android.Content.PM;

namespace CornerkickApp.Platforms.Android
{
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

  internal class DatabaseTools
  {
    public static async Task<int[]> copyMediaDataAndroid(string sourcePath, string targetPath = "", IProgress<int[]>? progress = null, bool bClean = true, bool bCount = false, int iFilesTotal = -1, string sMediaImgDir = "images", string[]? sMediaImgTypeDir = null)
    {
      if (sMediaImgTypeDir == null || sMediaImgTypeDir.Length == 0) return [];

      CkAppShared.ckMng.tl.writeLog("Import media from " + sourcePath + " to " + targetPath);

      if (!bCount && iFilesTotal < 0) {
        int[] iFilesCount = await copyMediaDataAndroid(sourcePath, progress: progress, bCount: true);
        foreach (int i in iFilesCount) iFilesTotal += i < 0 ? 0 : i;

        if (iFilesTotal == 0) return [];
      }

      var context = AndroidApp.Application.Context;

      if (!bCount) {
        if (Directory.Exists(targetPath) && bClean) Directory.Delete(targetPath, true);

        Directory.CreateDirectory(targetPath);
        Directory.CreateDirectory(Path.Combine(targetPath, sMediaImgDir));
      }

      int[] iFileCounts = new int[sMediaImgTypeDir.Length];

      int processCount = await Task.Run<int>(() => {
        int iCount = 0;
        int iType = 0;

        var folderUri = AndroidNet.Uri.Parse(sourcePath);
        if (folderUri == null) return iCount;

        var folder = AndroidX.DocumentFile.Provider.DocumentFile.FromTreeUri(context, folderUri);
        if (folder == null) return iCount;

        var dirs = folder.ListFiles();
        foreach (var dir in dirs) {
          if (dir.IsDirectory && !string.IsNullOrEmpty(dir.Name) && dir.Name.Equals(sMediaImgDir)) {
            foreach (string img_type_dir in sMediaImgTypeDir) {
              int jCount = 0;

              var dirs_img_type = dir.ListFiles();
              foreach (var dir_img_type in dirs_img_type) {
                if (dir_img_type.IsDirectory && !string.IsNullOrEmpty(dir_img_type.Name) && dir_img_type.Name.Equals(img_type_dir)) {
                  if (!bCount) Directory.CreateDirectory(Path.Combine(targetPath, sMediaImgDir, img_type_dir));

                  var files_img = dir_img_type.ListFiles();

                  foreach (var file_img in files_img) {
                    if (file_img.IsFile && !string.IsNullOrEmpty(file_img.Name)) {
                      if (progress != null) progress.Report([bCount ? 100 : iFilesTotal, iCount++, iType]);
                      jCount++;

                      // Continue if only counting
                      if (bCount) continue;

                      using var input = context.ContentResolver.OpenInputStream(file_img.Uri);

                      try {
                        if (input != null) {
                          string targetPath_img = Path.Combine(targetPath, sMediaImgDir, img_type_dir, Path.GetFileNameWithoutExtension(file_img.Name) + ".png");
                          SixLabors.ImageSharp.Image imageEmbTiny = SixLabors.ImageSharp.Image.Load(input);
                          imageEmbTiny.Mutate(x => x.ResizeDownTo(0, 128));
                          imageEmbTiny.SaveAsPng(targetPath_img);

                          /*
                          string targetPath_img = Path.Combine(targetPath, sMediaImgDir, img_type_dir, file_img.Name);
                          using var output = File.Create(targetPath_img);
                          if (input != null) input.CopyTo(output);
                          */
                        }
                      } catch (Exception ex) {
                        CkAppShared.ckMng.tl.writeLog("Error importing media file: " + file_img.Name + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, bError: true);
                      }
                    }
                  }
                }
              }

              iFileCounts[iType] = jCount;
              iType++;
            }
          }
        }

        return iCount;
      });

      return iFileCounts;
    }

    public static int getAndroidFileCount(string uri)
    {
      var folderUri = AndroidNet.Uri.Parse(uri);
      var context = AndroidApp.Application.Context;
      var folder = AndroidX.DocumentFile.Provider.DocumentFile.FromTreeUri(context, folderUri);
      var files = folder.ListFiles();
      return files.Length;

      /*
      foreach (var file in files) {
        if (file.IsFile) {
          string name = file.Name;
          long size = file.Length();
          string mime = file.Type;

          Console.WriteLine($"{name} ({size} bytes) [{mime}]");
        }
      }
      */
      return 0;
    }
  }
}
