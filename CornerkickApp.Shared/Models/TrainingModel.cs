using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CornerkickApp.Shared.Models
{
  public class TrainingRulesInput : LayoutModel
  {
    public List<SelectListItem> sliTrainingRulesCFM { get; set; }
    public List<SelectListItem> sliTrainingRulesSmGr { get; set; }
    public List<SelectListItem> sliTrainingRulesType { get; set; }

    public int iTrainingRulesCFM { get; set; }
    public int iTrainingRulesSmGr { get; set; }
    public float fTrainingRulesValue { get; set; }
    public int iTrainingRulesType { get; set; }

    public TrainingRulesInput()
    {
      sliTrainingRulesCFM  = new List<SelectListItem>();
      sliTrainingRulesSmGr = new List<SelectListItem>();
      sliTrainingRulesType = new List<SelectListItem>();

      iTrainingRulesSmGr = -1;
    }
  }
  public class TrainingModel : LayoutModel
  {
    public int iWeekIni { get; set; }

    public CornerkickManager.Main.TrainingPlan.Unit[][] ltTu { get; set; }
    public int   [] iTrainingCount    { get; set; }
    public string[] sTrainingCountRel { get; set; }
    public CkAppShared.Tutorial tutorial { get; set; }

    public TrainingRulesInput[] tripts { get; set; }

    public class TableItem
    {
      public int iId { get; set; }
      public int iNb { get; set; }
      public string sName { get; set; } = string.Empty;
      public float fSkill { get; set; }
      public float fSkillIdeal { get; set; }
      public float fCondi { get; set; }
      public float fFresh { get; set; }
      public float fMoral { get; set; }
      public string sPosition { get; set; } = string.Empty;
      public int iAge { get; set; }
      public float fTalent { get; set; }
      public string sNat { get; set; } = string.Empty;
      public string sTrLast { get; set; } = string.Empty;
      public int iIndTr { get; set; }
      public float fSkillIndTr { get; set; }
      public byte iTalentIndTr { get; set; }
    }

    public List<object> lt_ind_skills { get; set; }
  }

  [DataContract]
  public class DataPointTeamFAve2
  {
    public DataPointTeamFAve2(string s, double fFAve)
    {
      this.s = s;
      this.f = fFAve;
    }

    public string s = null;

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "f")]
    public Nullable<double> f = null;
  }
}
