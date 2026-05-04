using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CornerkickApp.Shared.Models
{
  public class UserManualModel : LayoutModel
  {
    // Cup attraction factors
    public string sAfLeague { get; set; }
    public string sAfCup { get; set; }
    public string sCupGoldAf { get; set; }
    public string sCupGoldBonus { get; set; }
    public string sCupGoldBonusStart { get; set; }
    public string sCupSilverAf { get; set; }
    public string sCupSilverBonus { get; set; }
    public string sCupSilverBonusStart { get; set; }
    public string sCupBronzeAf { get; set; }
    public string sCupBronzeBonus { get; set; }
    public string sCupBronzeBonusStart { get; set; }
    public string sAfWc { get; set; }
    public string sAfTg { get; set; }

    // Chart training CFM
    public string sTraining { get; set; }
    //public List<SelectListItem> ddlTraining { get; set; }

    public string sPlayerTrainingCoachCondi { get; set; }
    //public List<SelectListItem> ddlPlayerTrainingCoachCondi { get; set; }

    public string sPlayerTrainingCoachPhysio { get; set; }
    //public List<SelectListItem> ddlPlayerTrainingCoachPhysio { get; set; }

    public string sPlayerTrainingCamp { get; set; }
    //public List<SelectListItem> ddlPlayerTrainingCamp { get; set; }

    public string sPlayerTrainingDoping { get; set; }
    //public List<SelectListItem> ddlPlayerTrainingDoping { get; set; }

    // Chart player steps fresh loss
    public string sStepsSpeed { get; set; }
    //public List<SelectListItem> ddlStepsSpeed { get; set; }

    public string sStepsAcceleration { get; set; }
    //public List<SelectListItem> ddlStepsAcceleration { get; set; }

    public string sStepsLastSteps { get; set; }
    //public List<SelectListItem> ddlStepsLastSteps { get; set; }

    // Chart player duel
    public string sDuelMode { get; set; }
    //public List<SelectListItem> sliDuelMode { get; set; }
    public string sDuelDef { get; set; }
    //public List<SelectListItem> sliDuelDef { get; set; }
    public string sDuelOff { get; set; }
    //public List<SelectListItem> sliDuelOff { get; set; }
    public string sDuelPos { get; set; }
    //public List<SelectListItem> sliDuelPos { get; set; }

    public UserManualModel()
    {
      // Chart training CFM
      //ddlTraining = new List<SelectListItem>();
      foreach (CornerkickManager.PlayerTool.Training tr in CkAppShared.ckMng.plt.ltTraining) {
        if (tr.iId < 0) continue;

        /*
        ddlTraining.Add(
          new SelectListItem {
            Text = tr.sName,
            Value = tr.iId.ToString()
          }
        );
        */
      }

      /*
      ddlPlayerTrainingCoachCondi = new List<SelectListItem>();
      for (byte i = 7; i > 0; i--) ddlPlayerTrainingCoachCondi.Add(new SelectListItem { Text = "Level: " + i.ToString(), Value = i.ToString() });
      ddlPlayerTrainingCoachCondi.Add(new SelectListItem { Text = "-", Value = "0" });

      ddlPlayerTrainingCoachPhysio = new List<SelectListItem>();
      for (byte i = 7; i > 0; i--) ddlPlayerTrainingCoachPhysio.Add(new SelectListItem { Text = "Level: " + i.ToString(), Value = i.ToString() });
      ddlPlayerTrainingCoachPhysio.Add(new SelectListItem { Text = "-", Value = "0" });

      // Trainings camp
      ddlPlayerTrainingCamp = new List<SelectListItem>();
      ddlPlayerTrainingCamp.Add(new SelectListItem { Text = "-", Value = "-1" });
      for (byte i = 0; i < App.ckMng.tcp.ltCamps.Count; i++) ddlPlayerTrainingCamp.Add(new SelectListItem { Text = App.ckMng.tcp.ltCamps[i].sName, Value = i.ToString() });

      // Doping
      ddlPlayerTrainingDoping = new List<SelectListItem>();
      ddlPlayerTrainingDoping.Add(new SelectListItem { Text = "-", Value = "-1" });
      for (byte i = 0; i < App.ckMng.ltDoping.Count; i++) ddlPlayerTrainingDoping.Add(new SelectListItem { Text = App.ckMng.ltDoping[i].sName, Value = i.ToString() });

      // Chart player steps fresh loss
      ddlStepsSpeed = new List<SelectListItem>();
      for (byte i = 4; i < 11; i++) ddlStepsSpeed.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });

      ddlStepsAcceleration = new List<SelectListItem>();
      for (byte i = 4; i < 11; i++) ddlStepsAcceleration.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });

      ddlStepsLastSteps = new List<SelectListItem>();
      for (byte i = 0; i < 9; i++) ddlStepsLastSteps.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });

      // Chart player duel
      sliDuelMode = new List<SelectListItem>();
      sliDuelMode.Add(new SelectListItem { Text = "Entfernung vom Tor", Value = "0" });
      sliDuelMode.Add(new SelectListItem { Text = "Taktik Aggressivität", Value = "1" });

      sliDuelDef = new List<SelectListItem>();
      sliDuelOff = new List<SelectListItem>();
      for (byte i = 4; i < 11; i++) {
        sliDuelDef.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
        sliDuelOff.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
      }

      sliDuelPos = new List<SelectListItem>();
      sliDuelPos.Add(new SelectListItem { Text = "Vorne", Value = "0" });
      sliDuelPos.Add(new SelectListItem { Text = "seitl. Vorne", Value = "1" });
      sliDuelPos.Add(new SelectListItem { Text = "seitl. Hinten", Value = "2" });
      sliDuelPos.Add(new SelectListItem { Text = "Hinten", Value = "3" });
      */
    }

  }
}