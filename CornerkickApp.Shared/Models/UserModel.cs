using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace CornerkickApp.Shared.Models
{
  public class UserModel : LayoutModel
  {
    public CornerkickManager.User usr { get; set; }
    public List<CornerkickManager.Main.News> ltUserMail { get; set; }
    public List<SelectListItem> ltDdlUser { get; set; }
    public string sMailTo { get; set; } = "";
    public string sInviteLink { get; set; } = "";

    public class DatatableCooperations
    {
      //public int iUserId { get; set; }
      public int iClubId { get; set; }
      public string sUser { get; set; } = "";
      public string sClub { get; set; } = "";
      public float fClubAttrFac { get; set; }
      public string sNat { get; set; } = "";
      public string sLeague { get; set; } = "";
      public string sStatus { get; set; } = "";
      public int iEarnings { get; set; }
    }

    public CkAppShared.Tutorial tutorial { get; set; }
  }

  public class UserOptionsModel : LayoutModel
  {
    public const byte iUserOptionsIxDeleteLog = 0;
    public const byte iUserOptionsIxSound = 1;
    public const byte iUserOptionsIxShowBalance = 2;
    public const byte iUserOptionsIxTutorialShow = 3;
    public const byte iUserOptionsIxTutorialLevel = 4;
    public const byte iUserOptionsIxComment = 5;
    public const byte iUserOptionsIxAnimations = 6;
    public const byte iUserOptionsIxShowPitch = 7;
    public const byte iUserOptionsIxFontSize = 8;
    public const byte iUserOptionsIxCalSpeed = 9;
    public const byte iUserOptionsIxStadium = 10;
    public static readonly byte[] iUserOptionsDefaults = [
        3, // Delete Log: After 1 Month
        1, // Sound: On
        1, // Show Balance: On
        1, // Show Tutorial: On
        0, // Tutorial Level: 0
        0, // All comments
        1, // Highlights
        1, // On
      100, // Font size
       10, // Calendar speed
        1  // Stadium (small default stadium)
    ];

    public const byte iUserOptionsStrIxInvitedById = 0;

    public bool bSound { get; set; }
    public bool bShowBalanceToday { get; set; }
    public bool bShowTutorial { get; set; }
    public int iCalSpeed { get; set; }
    public int iGameSpeed { get; set; }
    public int iGameSpeed2 { get; set; }
    public bool bScouting { get; set; }

    public int iComments { get; set; }
    public List<SelectListItem> ddlComments { get; set; }

    public int iShowPitch { get; set; }
    public List<SelectListItem> ddlShowPitch { get; set; }

    public int iAnimations { get; set; }
    public List<SelectListItem> ddlAnimations { get; set; }

    public int iFontSize { get; set; }
    public List<SelectListItem> ddlFontSize { get; set; }

    public UserOptionsModel()
    {
      // Comments select
      iComments = 0;

      ddlComments = new List<SelectListItem>() {
        new SelectListItem { Text = "alle",         Value = "0" },
        new SelectListItem { Text = "nur wichtige", Value = "1" }
      };

      // Show/hide pitch select
      iShowPitch = 1;
      ddlShowPitch = new List<SelectListItem>() {
        new SelectListItem { Text = "aus",   Value = "0" },
        new SelectListItem { Text = "Radar", Value = "2" },
        new SelectListItem { Text = "an",    Value = "1" }
      };

      // Animations select
      iAnimations = 0;
      ddlAnimations = new List<SelectListItem>();
      ddlAnimations.Add(new SelectListItem { Text = "aus", Value = "0" });
      ddlAnimations.Add(new SelectListItem { Text = "nur Highlights", Value = "1" });
      ddlAnimations.Add(new SelectListItem { Text = "alles", Value = "2" });

      // Font size select
      iFontSize = 100;
      ddlFontSize = new List<SelectListItem>();
      ddlFontSize.Add(new SelectListItem { Text =  "50%", Value =  "50" });
      ddlFontSize.Add(new SelectListItem { Text =  "75%", Value =  "75" });
      ddlFontSize.Add(new SelectListItem { Text =  "90%", Value =  "90" });
      ddlFontSize.Add(new SelectListItem { Text = "100%", Value = "100" });
      ddlFontSize.Add(new SelectListItem { Text = "110%", Value = "110" });
      ddlFontSize.Add(new SelectListItem { Text = "125%", Value = "125" });
      ddlFontSize.Add(new SelectListItem { Text = "150%", Value = "150" });
      ddlFontSize.Add(new SelectListItem { Text = "200%", Value = "200" });
    }
  }

  public class UserComponentModel
  {
    //public static CornerkickManager.Club clb { get; set; }
    public string sUserId { get; set; } = "";
    public int iHeadlineBalance { get; set; }
    public int iHeadlineBalanceSecret { get; set; }
    public string sHeadlineDate { get; set; } = "";
    public int iSeasonProgress { get; set; }
    public byte[] byteClubUserEmblem { get; set; }
    public string sClubEmblem { get; set; } = "";
  }

}
