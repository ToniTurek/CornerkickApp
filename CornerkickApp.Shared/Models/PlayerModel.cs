using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CornerkickApp.Shared.Models
{
  public class PlayerModel : LayoutModel
  {
    //public CornerkickManager.User usr { get; set; }

    public bool bScouting { get; set; }
    public bool bSound { get; set; }
    //public CultureInfo ci { get; set; }

    public CkAppShared.Tutorial? tutorial { get; set; }

    //public CornerkickManager.Player pldetails { get; set; }
    public string sNat1 { get; set; } = "";
    public string sNat2 { get; set; } = "";
    public string sBirthday { get; set; } = "";
    public int iAge { get; set; }
    public string sHeightWeight { get; set; } = "";
    public string sPos { get; set; } = "";
    public string sFoot { get; set; } = "";
    public string sInjury { get; set; } = "";
    public string sClub { get; set; } = "";

    public int iPlayerIndTr { get; set; }

    public byte[]? iSuspension { get; set; }
    public string sDopingName { get; set; } = "";
    public float[]? fDoping { get; set; }

    public string[]? sCharacter { get; set; }
    public CornerkickManager.Player.Contract? contract { get; set; }
    public bool bRetire { get; set; }
    public string sClubNextSeason { get; set; } = "";

    //public CornerkickGame.Player plPrev { get; set; }
    //public CornerkickGame.Player plNext { get; set; }
    public int    iPlPrevId { get; set; } = -1;
    public string sPlPrevName { get; set; } = "";
    public int    iPlNextId { get; set; } = -1;
    public string sPlNextName { get; set; } = "";

    public bool bOwnPlayer { get; set; }
    public bool bJouth { get; set; }
    public bool bJouthBelow16 { get; set; }
    public bool bJouthWithContract { get; set; }
    public bool bNation { get; set; }
    public bool bOnTransferlist { get; set; }
    public bool bCpuPlayerNotOnTransferlist { get; set; }
    public bool bAdmin { get; set; }

    public byte iPos { get; set; }
    public string[][]? sSkillTable { get; set; }
    public CkAppShared.DataPointDD[]? ltPlayerPosSkillData { get; set; }

    // Emblem
    public string sPortrait { get; set; } = "";
    //public byte[]? bytePortrait { get; set; }
    public string sEmblem { get; set; } = "";
    public byte[]? byteClubEmblem { get; set; }

    public string sColorJersey1 { get; set; } = "";
    public string sColorJersey2 { get; set; } = "";
    public string sColorJerseyNb { get; set; } = "";
    public string sColor2 { get; set; } = ""; // For tab border/text color

    // Contract
    [Display(Name = "zusätzl. Laufzeit [a]:")]
    public int iContractYears { get; set; }
    [Display(Name = "Gebotenes Gehalt:")]
    public int iContractSalaryOffer { get; set; }

    public string sName { get; set; } = "";
    public float fTalentAve { get; set; }

    [Display(Name = "Neue Rückennr.:")]
    public int iNo { get; set; }
    public List<int>? ltNo { get; set; }

    public string sValue { get; set; } = "";

    public bool bCaptain  { get; set; }
    public bool bCaptain2 { get; set; }

    public int iDp { get; set; }
    public List<SelectListItem>? ddlDoping { get; set; }

    public float fContractHappyFactor { get; set; }

    public bool bEditable { get; set; }
    public bool bSeasonStart { get; set; }
    public bool bLiveGame { get; set; }

    public CkAppShared.DataPointTD[][]? ltDevData { get; set; }

    public class Stat
    {
      public string Name { get; set; } = string.Empty;
      public int[] iStat { get; set; }
      public int[] iRef { get; set; }
    }

    public class Scout
    {
      public Scout(int _iId, string _sName, int _iSkill, int _nDataPerScouting, string _sCost, int _iMinActive)
      {
        iId = _iId;
        sName = _sName;
        iSkill = _iSkill;
        nDataPerScouting = _nDataPerScouting;
        sCost = _sCost;
        iMinActive = _iMinActive;
      }
      public int iId { get; set; }
      public string sName { get; set; }
      public int iSkill { get; set; }
      public int nDataPerScouting { get; set; }
      public string sCost { get; set; }
      public int iMinActive { get; set; }
    }
    public List<Scout>? ltScouts { get; set; }

    public class ScoutingResult
    {
      public class Skill
      {
        public int iSkillIx { get; set; }
        public float fSkillAve { get; set; }
        public int n_scout { get; set; }
      }
      public int iMinActivate { get; set; }
      public List<Skill> ltSkills { get; set; } = new List<Skill>();
    }

    public class ClubHistory
    {
      public int    iIx { get; set; }
      public int    iId { get; set; }
      public string sPlayerName { get; set; } = "";
      public string sClubTakeName { get; set; } = "";
      public string sClubGiveName { get; set; } = "";
      public DateTime dt { get; set; }
      public int iValue { get; set; }
      public int iTransferFee { get; set; }
    }

    public class InjuryHistory
    {
      public int iIx { get; set; }
      public DateTime dt { get; set; }
      public string sInjuryName { get; set; } = "";
      public int iInjuryLength { get; set; }
    }

    public class Contract : CornerkickManager.Player.Contract
    {
      public List<CupBonus>? ltCupBonus { get; set; }
    }

    public class ScoutingDataPlus
    {
      public DateTime dt { get; set; }
      public int iSkill { get; set; }
      public int iSkillIx { get; set; }
      public int iScoutId { get; set; }
      public string sScoutImg { get; set; }
      public string sScoutName { get; set; }

      public ScoutingDataPlus(CornerkickManager.Main.Staff.ScoutingData sd = null)
      {
        sScoutImg = "";
        sScoutName = "";

        if (sd != null) {
          this.dt = sd.dt;
          this.iSkill = sd.iSkill;
          this.iSkillIx = sd.iSkillIx;
          this.iScoutId = sd.iScoutId;
        }
      }
    }

  }
}
