using System;
using System.Collections.Generic;
using System.IO;

namespace CornerkickApp.Shared.Models
{
  public class AdminModel
  {
    public static string sAdminEmail { get; set; } = "";
    public static CornerkickManager.Club? clubAdmin { get; set; }

    public static bool checkUserIsAdmin(string? sEmail, string sPw = "")
    {
#if DEBUG
      //return true;
#endif
      if (string.IsNullOrEmpty(sEmail)) return false;

      string sAdminEmail = AdminModel.sAdminEmail;

      if (string.IsNullOrEmpty(sAdminEmail)) return false;
      if (!sEmail.Equals(sAdminEmail)) return false;

      if (!string.IsNullOrEmpty(sPw)) {
        if (!sPw.Equals("!Cornerkick1")) return false;
      }

      return true;
    }
    public static bool checkUserIsAdmin(System.Security.Principal.IPrincipal user)
    {
      if (user.Identity == null) return false;
      return checkUserIsAdmin(user.Identity.Name);
    }
  }

  public class AdminViewModel : LayoutModel
  {
    public bool bCk { get; set; }
    public bool bTimer { get; set; }
    public bool bTimerSave { get; set; }
    public List<string> ltLog { get; set; }
    public List<string> ltErr { get; set; }
    public int nClubs { get; set; }
    public int nUser { get; set; }
    public int nPlayer { get; set; }
    public string sHomeDir { get; set; }
    public string sHomeDirCk { get; set; }

    public DateTime dtCkCurrent { get; set; }
    public DateTime dtCkApproach { get; set; }
    public double fIntervalAveToApproachTarget { get; set; }

    // Settings
    public double fCalendarInterval { get; set; }
    public string sStartHour { get; set; }
    public int iGameSpeed { get; set; }
    public bool bEmailCertification { get; set; }
    public bool bRegisterDuringGame { get; set; }
    public bool bLoginPossible { get; set; }
    public bool bMaintenance { get; set; }
    public string sInfo { get; set; }
    public DateTime? dtCounterStart;

    public bool bLogExist { get; set; }
    public bool bAutosaveExist { get; set; }
    public bool bSaveDirExist { get; set; }

    public string sSelectedAutosaveFile { get; set; }
    public List<SelectListItem> ddlAutosaveFiles { get; set; }

    // CPU Clubs to be selected by admin
    public List<SelectListItem> ddlClubsAdmin { get; set; }
    public int iSelectedClubAdmin { get; set; }

    // Transfer Money
    public List<SelectListItem> ddlClubsTransferMoney { get; set; }
    public int iClubTransferMoney { get; set; }
    public int iTransferMoney { get; set; }
    public string sTransferMoneySubject { get; set; }

    // CPU Clubs to be selected by admin
    public List<SelectListItem> ddlClubs { get; set; }
    public int iSelectedClub { get; set; }
    public List<SelectListItem> ddlCups { get; set; }
    public int iSelectedCup { get; set; }

    public AdminViewModel()
    {
      ddlAutosaveFiles = new List<SelectListItem>();
      ddlClubs = new List<SelectListItem>();
      ddlClubsAdmin = new List<SelectListItem>();
      ddlClubsTransferMoney = new List<SelectListItem>();
      for (int iC = 0; iC < CkAppShared.ckMng?.ltClubs?.Count; iC++) {
        CornerkickManager.Club clb = CkAppShared.ckMng.ltClubs[iC];

        SelectListItem sliClub = new SelectListItem {
          Text = clb.sName,
          Value = clb.iId.ToString()
        };
        ddlClubs.Add(sliClub);
        if (clb.user == null) {
          ddlClubsAdmin.Add(sliClub);
        } else if (!clb.bNation) {
          sliClub.Text += ": " + clb.iBalance.ToString("N0", System.Globalization.CultureInfo.CurrentCulture);
          ddlClubsTransferMoney.Add(sliClub);
        }
      }

      ddlCups = new List<SelectListItem>();
      for (int iC = 0; iC < CkAppShared.ckMng?.ltCups?.Count; iC++) {
        CornerkickManager.Cup cup = CkAppShared.ckMng.ltCups[iC];

        SelectListItem sliCup = new SelectListItem {
          Text = cup.sName,
          Value = iC.ToString()
        };
        ddlCups.Add(sliCup);
      }
    }
  }
}
