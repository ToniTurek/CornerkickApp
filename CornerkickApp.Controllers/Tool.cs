using CornerkickApp.Shared.Models;
using Microsoft.AspNetCore.Components;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.Runtime.InteropServices.JavaScript;

namespace CornerkickApp.Controllers
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

  public class Tool
  {
    public static void setNations()
    {
      if (CkAppShared.ckMng.ltCups == null) return;

      List<byte> ltNations = new List<byte>();
      foreach (CornerkickManager.Cup cup in CkAppShared.ckMng.ltCups) {
        if (cup.iId == CkAppShared.iCupIdLeague && cup.iId2 >= 0) {
          if (!ltNations.Contains((byte)cup.iId2)) ltNations.Add((byte)cup.iId2);
        }
      }
      CkAppShared.iNations = ltNations.ToArray();
    }

    public static Task<List<LayoutModel.SelectListItem>> getCountries()
    {
      List<LayoutModel.SelectListItem> sliCountries = new List<LayoutModel.SelectListItem>();

      if (CornerkickManager.Main.sLand == null) return Task.FromResult(sliCountries);

      for (int iN = 0; iN < CornerkickManager.Main.sLand.Length; iN++) {
        CornerkickManager.Cup league = CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdLeague, iN, 0);
        if (league == null) continue;

        string sLand = "Land " + iN.ToString();
        if (CornerkickManager.Main.sLand != null) sLand = CornerkickManager.Main.sLand[iN];

        sliCountries.Add(new LayoutModel.SelectListItem { Text = sLand, Value = iN.ToString() });
      }

      return Task.FromResult(sliCountries);
    }

    public static Task<List<LayoutModel.SelectListItem>> getLeagues(int iLand)
    {
      List<LayoutModel.SelectListItem> sliLeagues = new List<LayoutModel.SelectListItem>();

      List<CornerkickManager.Cup> leagues = CkAppShared.ckMng.tl.getCups(CkAppShared.iCupIdLeague, iId2: iLand);
      if (leagues == null) return Task.FromResult(sliLeagues);

      foreach (CornerkickManager.Cup l in leagues) {
        sliLeagues.Add(new LayoutModel.SelectListItem() {
          Value = l.iId3.ToString(),
          Text = l.sName
        });
      }

      return Task.FromResult(sliLeagues);
    }

    public static Task<List<LayoutModel.SelectListItem>> getClubs(int iId, int iId2, int iId3 = -1)
    {
      return getClubs(CkAppShared.ckMng.tl.getCups(iId, iId2: iId2, iId3: iId3));
    }
    public static Task<List<LayoutModel.SelectListItem>> getClubs(CornerkickManager.Cup cup)
    {
      return getClubs(new List<CornerkickManager.Cup> { cup });
    }
    public static Task<List<LayoutModel.SelectListItem>> getClubs(List<CornerkickManager.Cup> cups)
    {
      List<LayoutModel.SelectListItem> sliClubs = new List<LayoutModel.SelectListItem>();

      if (cups == null) return Task.FromResult(sliClubs);

      foreach (CornerkickManager.Cup c in cups) {
        foreach (CornerkickManager.Club clb in c.ltClubs[0]) {
          sliClubs.Add(new LayoutModel.SelectListItem() {
            Value = clb.iId.ToString(),
            Text = clb.sName
          });
        }
      }

      return Task.FromResult(sliClubs);
    }

    public static RenderFragment createStars(float fStars, int iStarsMax = 10, int iStarSizePx = 0, bool bRound = true, int iWidthPx = 0, int iMarginBottomPx = 0) => builder => {
      int iStars = 0;
      if (fStars - Math.Floor(fStars) < 0.25) {
        iStars = (int)Math.Floor(fStars);
      } else {
        iStars = (int)Math.Ceiling(fStars);
      }

      if (bRound && iStarSizePx < 1) iStarSizePx = 18;

      for (int i = 0; i < Math.Max(iStars, iStarsMax); i++) {
        int iStarSizePx2 = iStarSizePx;
        if (bRound && i >= iStarsMax) iStarSizePx2 = 26;

        builder.OpenElement(1, "img");
        string sStar = "star.ico";
        if (i >= iStars) {
          sStar = "star_empty.png";
        } else if (i == iStars - 1 && fStars - Math.Floor(fStars) > 0.25 && fStars - Math.Floor(fStars) < 0.75) {
          sStar = "star_half-empty.png";
        }
        builder.AddAttribute(2, "src", CkAppShared.sContentDir + "/Icons/" + sStar);
        //builder.AddAttribute(3, "style", "position: absolute");

        if (bRound) {
          double fAngle = (i * Math.PI) / (5.0);
          int iStarsRadiusPx2 = (iWidthPx / 2) - (iStarSizePx2 / 2);
          double fTop = iStarsRadiusPx2 - (iStarsRadiusPx2 * (Math.Cos(fAngle) / 1.0));
          double fLft = iStarsRadiusPx2 + (iStarsRadiusPx2 * (Math.Sin(fAngle) / 1.0));
          builder.AddAttribute(3, "style", "position: absolute; top: " + fTop.ToString("0px") + "; left: " + fLft.ToString("0px") + "; width: " + iStarSizePx2.ToString("0px") + "; padding: 0px");
        } else {
          string sStyle = "position: relative; top: 0px; margin-right: 2px" + "; margin-bottom: " + iMarginBottomPx.ToString("0px") + "; padding: 0px";
          if (iStarSizePx2 > 0) sStyle += "; width: " + iStarSizePx2.ToString("0px");
          builder.AddAttribute(3, "style", sStyle);
        }
        builder.AddAttribute(4, "class", "star");
        builder.CloseElement();
      }
    };

    public static void setCssStringClubColors(System.Drawing.Color[] clClub)
    {
      CkAppShared.sCssStyleClubColors = "--clubcolor1: " + convertToRgb(clClub[0]) + "; --clubcolor2: " + convertToRgb(clClub[checkColorsSimilar(clClub[0], clClub[1]) ? 2 : 1]);
    }

    public static System.Drawing.Color convertToColor(string sColorRGB)
    {
      //This gives us an array of 3 strings each representing a number in text form.
      string[] splitString = sColorRGB.Split(',');
      if (splitString.Length < 3) return System.Drawing.Color.FromArgb(0, 0, 0);

      for (int i = 0; i < splitString.Length; i++) splitString[i] = new String(splitString[i].Where(Char.IsDigit).ToArray());

      //converts the array of 3 strings in to an array of 3 ints.
      int[] splitInts = splitString.Select(item => int.Parse(item)).ToArray();

      //takes each element of the array of 3 and passes it in to the correct slot
      return System.Drawing.Color.FromArgb(splitInts[0], splitInts[1], splitInts[2]);
    }

    public static string convertToHex(string sColorRGB)
    {
      System.Drawing.Color cl = convertToColor(sColorRGB);
      return convertToHex(cl);
    }

    public static string convertToHex(System.Drawing.Color cl)
    {
      return "#" + cl.R.ToString("X2") + cl.G.ToString("X2") + cl.B.ToString("X2");
    }

    public static string convertToRgb(System.Drawing.Color cl)
    {
      return "rgb(" + getColorRgbString(cl) + ")";
    }

    public static string getColorRgbString(System.Drawing.Color cl)
    {
      return cl.R.ToString() + "," + cl.G.ToString() + "," + cl.B.ToString();
    }

    public static System.Drawing.Color getColorComplementary(System.Drawing.Color cl)
    {
      return System.Drawing.Color.FromArgb(255 - cl.R, 255 - cl.G, 255 - cl.B);
    }

    public static System.Drawing.Color getColorBW(CornerkickManager.Club club)
    {
      return getColorBW(club.cl1[0]);
    }
    public static System.Drawing.Color getColorBW(System.Drawing.Color cl)
    {
      System.Drawing.Color clBW = System.Drawing.Color.Black;
      if (checkColorBW(cl)) clBW = System.Drawing.Color.White;

      return clBW;
    }
    public static bool checkColorBW(System.Drawing.Color cl)
    {
      return cl.R + cl.G + cl.B < 300;
    }

    public static string getColor0_1(float f)
    {
      return "rgb(" + Math.Min(2 * (1 - f) * 255, 255).ToString("0") + "," + Math.Min(2 * f * 255, 255).ToString("0") + ",0)";
    }

    public static bool checkColorsSimilar(System.Drawing.Color cl1, System.Drawing.Color cl2)
    {
      const int iDiff = 5;
      if (Math.Abs(cl1.R - cl2.R) < iDiff &&
          Math.Abs(cl1.G - cl2.G) < iDiff &&
          Math.Abs(cl1.B - cl2.B) < iDiff) return true;

      return false;
    }

    public static byte[]? ConvertToBytes(string sFilename)
    {
      if (!File.Exists(sFilename)) return null;

      // Load file meta data with FileInfo
      FileInfo fileInfo = new FileInfo(sFilename);

      if (fileInfo.Length < 1) return null;

      // The byte[] to save the data in
      byte[] data = new byte[fileInfo.Length];

      // Load a filestream and put its content into the byte[]
      using (FileStream fs = fileInfo.OpenRead()) {
        fs.Read(data, 0, data.Length);
      }

      // Delete the temporary file
      //fileInfo.Delete();

      return data;
    }

    public static byte[] ConvertImageToBytes(Image img)
    {
      using (MemoryStream ms = new MemoryStream()) {
        // Convert Image to byte[]
        img.SaveAsPng(ms);
        return ms.ToArray();
      }
    }

    public static string getNewsIcon(int iType)
    {
      if (iType == 1) return "<img src=\"" + CkAppShared.sContentDir + "/Icons/news/birthday.png\" title=\"birthday\" style=\"width: 16px\"/>";
      if (iType > 9 && iType < 14) return "<img src=\"" + CkAppShared.sContentDir + "/Icons/star.ico\" title=\"star\" style=\"width: 16px\"/>";
      if (iType == 16 || iType == 17) return "<img src=\"" + CkAppShared.sContentDir + "/Icons/news/doping.png\" title=\"doping\" style=\"width: 16px\"/>";
      if (iType == 18) return "<img src=\"" + CkAppShared.sContentDir + "/Icons/star.ico\" title=\"star\" style=\"width: 16px\"/>";
      if (iType == 20 || iType == 23) return "<img src=\"" + CkAppShared.sContentDir + "/Icons/ambulance2.png\" title=\"ambulance\" style=\"width: 16px\"/>";
      if (iType == 21 || iType == 22 || iType == 24 || iType == 25) return "<img src=\"" + CkAppShared.sContentDir + "/Icons/ambulance.png\" title=\"ambulance\" style=\"width: 16px\"/>";
      if (iType == 50) return "<img src=\"" + CkAppShared.sContentDir + "/Icons/news/stadium.png\" title=\"stadium\" style=\"width: 16px\"/>";
      if (iType == 51) return "<img src=\"" + CkAppShared.sContentDir + "/Icons/news/construction.png\" title=\"construction\" style=\"width: 16px\"/>";
      if (iType == 60) return "<img src=\"" + CkAppShared.sContentDir + "/Icons/news/coins.png\" title=\"cash\" style=\"width: 16px\"/>";

      return "<img src=\"" + CkAppShared.sContentDir + "/Icons/news/info.png\" title=\"info\" style=\"width: 16px\"/>";
    }

    public static string getNatIcon(int iNat, string sStyle = "")
    {
      if (CornerkickManager.Main.sLandShort == null || iNat < 0 || iNat >= CornerkickManager.Main.sLandShort.Length) return "";

      return getNatIcon(CornerkickManager.Main.sLandShort[iNat], sStyle: sStyle);
    }
    public static string getNatIcon(string sNat, string sStyle = "")
    {
      string sIcon = "<img src=\"" + CkAppShared.sContentDir + "/Icons/flags/";

      if (string.IsNullOrEmpty(sNat)) {
        sIcon += "0.png\" title=\"unknown";
      } else {
        sIcon += sNat + ".png\" title=\"" + sNat;
      }
      sIcon += "\" style=\"";

      if (string.IsNullOrEmpty(sStyle)) {
        sIcon += "width: 16px";
      } else {
        sIcon += sStyle;
      }

      sIcon += "\"/>";

      return sIcon;
    }

    public static string getFormIcon(string sForm)
    {
      if (string.IsNullOrEmpty(sForm)) return "o";

      sForm = sForm.Trim();

      string sIcon = "<img src=\"" + CkAppShared.sContentDir + "/Icons/";
      if (sForm.Equals("---")) {
        sIcon += "form0";
      } else if (sForm.Equals("-")) {
        sIcon += "form1";
      } else if (sForm.Equals("o")) {
        sIcon += "form2";
      } else if (sForm.Equals("+")) {
        sIcon += "form3";
      } else if (sForm.Equals("+++")) {
        sIcon += "form4";
      } else if (sForm.Equals("verl")) {
        sIcon += "ambulance";
      } else if (sForm.Equals("ang.")) {
        sIcon += "ambulance2";
      }

      sIcon += ".png\" title=\"" + sForm + "\" style=\"width: 16px\"/>";

      return sIcon;
    }

    public static string resizeImage(string sImageFileDatum, int iNewImageWidth, string sNewImagePath = "", string sNewImageAppendix = "", bool bShrinkOnly = false)
    {
      if (string.IsNullOrEmpty(sImageFileDatum)) return "";
      if (!File.Exists(sImageFileDatum)) return "";

      string? sNewImgDir = string.IsNullOrEmpty(sNewImagePath) ? Path.GetDirectoryName(sImageFileDatum) : sNewImagePath;
      if (string.IsNullOrEmpty(sNewImgDir)) sNewImgDir = ".";

      // Create target directory if it does not exist
      if (!Directory.Exists(sNewImgDir)) {
        try {
          Directory.CreateDirectory(sNewImgDir);
        } catch (Exception e) {
          CkAppShared.ckMng.tl.writeLog("Unable to create target directory '" + sNewImgDir + "'. Message: " + e.Message + Environment.NewLine + e.StackTrace, CornerkickManager.Main.sErrorFile);
        }
      }

      string sNewImageFile = Path.Combine(sNewImgDir, Path.GetFileNameWithoutExtension(sImageFileDatum)) + sNewImageAppendix + Path.GetExtension(sImageFileDatum);
      if (File.Exists(sNewImageFile)) return sNewImageFile; // Return if file already exist

      using (Image imgEmblem = Image.Load(sImageFileDatum)) {
        int iHeight = (int)(imgEmblem.Height * iNewImageWidth / (double)imgEmblem.Width);
        if (!(bShrinkOnly && imgEmblem.Width <= iNewImageWidth)) {
          imgEmblem.Mutate(x => x.Resize(iNewImageWidth, iHeight));
        }

        try {
          imgEmblem.Save(sNewImageFile);
        } catch (Exception e) {
          CkAppShared.ckMng.tl.writeLog("Error saving resized image '" + sNewImageFile + "'. Message: " + e.Message + Environment.NewLine + e.StackTrace, CornerkickManager.Main.sErrorFile);
        }
      }

      return sNewImageFile;
    }

    public static byte[] resizeImage(byte[]? bImgDatum, int iNewImageWidth, bool bShrinkOnly = false)
    {
      if (bImgDatum == null) return [];

      using (Image imgResized = Image.Load(bImgDatum)) {
        if (bShrinkOnly && imgResized.Width <= iNewImageWidth) return bImgDatum;

        int iHeight = (int)(imgResized.Height * iNewImageWidth / (double)imgResized.Width);
        imgResized.Mutate(x => x.Resize(iNewImageWidth, iHeight));
        return ConvertImageToBytes(imgResized);
      }
    }

    public static int roundIntBy(int i, int round_to)
    {
      if (i == 0) return 1;
      return Math.Max((int)Math.Pow(10.0, (int)Math.Log10(i) - round_to), 1);
    }
    public static int roundInt(int i, int round_to)
    {
      int iRound = roundIntBy(i, round_to);
      return (int)Math.Round(i / (float)iRound) * iRound;
    }
    public static int ceilInt(int i, int ceil_to)
    {
      int iRound = roundIntBy(i, ceil_to);
      return (int)Math.Ceiling(i / (float)iRound) * iRound;
    }

    public static string getSeasonString(int iSeason)
    {
      int iS = iSeason - CkAppShared.ckMng.iSeason;
      return CkAppShared.ckMng.dtSeasonStart.AddYears(iS).ToString("yyyy") + "/" + CkAppShared.ckMng.dtSeasonStart.AddYears(iS + 1).ToString("yy");
    }

    public static int getFirstAvailable(List<int> ltIds)
    {
      int? firstAvailable = Enumerable.Range(0, int.MaxValue)
                                .Except(ltIds)
                                .FirstOrDefault();
      return firstAvailable.HasValue ? firstAvailable.Value : 0;
    }
  }
}
