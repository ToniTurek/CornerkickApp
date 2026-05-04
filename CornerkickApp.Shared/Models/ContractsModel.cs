using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CornerkickApp.Shared.Models
{
  public class ContractsModel : LayoutModel
  {
    public class TableEntry
    {
      public int iId { get; set; }
      public int iNb { get; set; }
      public string sName { get; set; } = "";
      public float fSkill { get; set; }
      public float fSkillIdeal { get; set; }
      public string sPosition { get; set; } = "";
      public int iAge { get; set; }
      public float fTalent { get; set; }
      public int iValue { get; set; }
      public int iSalary { get; set; }
      public int iBonusPlay { get; set; }
      public int iBonusGoal { get; set; }
      public int iFixTransferFee { get; set; }
      public int iLength { get; set; }
      public bool bRetire { get; set; }
      public float fHappy { get; set; }
      public float fNeg { get; set; }
      public float fNegUserSkill { get; set; }
      public string sNat { get; set; } = "";
      public bool bJouth { get; set; }
    }

    // Filter
    public List<SelectListItem> ltDdlFilterPos { get; set; }
    [Display(Name = "Pos.: ")]
    public string sFilterPos { get; set; }

    public CkAppShared.Tutorial tutorial { get; set; }

    public ContractsModel()
    {
      ltDdlFilterPos = new List<SelectListItem>();
      ltDdlFilterPos.Add(new SelectListItem { Text = "alle", Value = "0" });

      // Positionen zu Dropdown Menü hinzufügen
      for (int iPos = 1; iPos < 12; iPos++) {
        ltDdlFilterPos.Add(new SelectListItem { Text = CornerkickManager.Main.sPosition[iPos], Value = iPos.ToString() });
      }
    }

    public class PlayerSalary
    {
      public int iPlayerId { get; set; }
      public byte iYears { get; set; }
      public int iSalary { get; set; }
      public int iBonusPlay { get; set; }
      public int iBonusPoint { get; set; }
      public int iBonusGoal { get; set; }
      public List<CornerkickManager.Player.Contract.CupBonus>? ltCupBonus { get; set; }
      public int iFixedFee { get; set; }
      public bool bNegotiateNextSeason { get; set; }
      public float fPlayerMood { get; set; }
    }
  }
}
