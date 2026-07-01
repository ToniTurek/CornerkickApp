using CornerkickApp.Shared.Models;
using System;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  public class StaffController
  {
    public static StaffModel Model(CornerkickManager.User? _usr)
    {
      StaffModel model = new StaffModel();

      if (_usr == null) return model;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return model;

      model.bScouting = _usr.bScouting;

      model.staff = clb.staff;

      model.ltDdlPersonalCoachCo = new List<SelectListItem>();
      model.ltDdlPersonalCoachCondi = new List<SelectListItem>();
      model.ltDdlPersonalMasseur = new List<SelectListItem>();
      model.ltDdlPersonalMental = new List<SelectListItem>();
      model.ltDdlPersonalMed = new List<SelectListItem>();
      model.ltDdlPersonalJouthCoach = new List<SelectListItem>();
      model.ltDdlPersonalJouthScouting = new List<SelectListItem>();
      model.ltDdlPersonalKibitzer = new List<SelectListItem>();

      byte iStaffLevelMax = 7;
      if (clb.iDivision > 0) iStaffLevelMax = 4;

      for (byte i = iStaffLevelMax; i > 0; i--) model.ltDdlPersonalCoachCo.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostCoachCo[i].ToString("N0", MemberController.getCi(clb)) + " €/Monat", Value = i.ToString() });
      model.ltDdlPersonalCoachCo.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = iStaffLevelMax; i > 0; i--) model.ltDdlPersonalCoachCondi.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostCoachCondi[i].ToString("N0", MemberController.getCi(clb)) + " €/Monat", Value = i.ToString() });
      model.ltDdlPersonalCoachCondi.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = iStaffLevelMax; i > 0; i--) model.ltDdlPersonalMasseur.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostMasseur[i].ToString("N0", MemberController.getCi(clb)) + " €/Monat", Value = i.ToString() });
      model.ltDdlPersonalMasseur.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = iStaffLevelMax; i > 0; i--) model.ltDdlPersonalMental.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostMental[i].ToString("N0", MemberController.getCi(clb)) + " €/Monat", Value = i.ToString() });
      model.ltDdlPersonalMental.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = iStaffLevelMax; i > 0; i--) model.ltDdlPersonalMed.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostMed[i].ToString("N0", MemberController.getCi(clb)) + " €/Monat", Value = i.ToString() });
      model.ltDdlPersonalMed.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = iStaffLevelMax; i > 0; i--) model.ltDdlPersonalJouthCoach.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostJouthCoach[i].ToString("N0", MemberController.getCi(clb)) + " €/Monat", Value = i.ToString() });
      model.ltDdlPersonalJouthCoach.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = iStaffLevelMax; i > 0; i--) model.ltDdlPersonalJouthScouting.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostJouthScouting[i].ToString("N0", MemberController.getCi(clb)) + " €/Monat", Value = i.ToString() });
      model.ltDdlPersonalJouthScouting.Add(new SelectListItem { Text = "-", Value = "0" });

      if (clb.iDivision < 1) {
        model.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 4 - " + CornerkickManager.Finance.iCostKibitzer[4].ToString("N0", MemberController.getCi(clb)) + " €/Monat", Value = "4" });
        model.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 3 - " + CornerkickManager.Finance.iCostKibitzer[3].ToString("N0", MemberController.getCi(clb)) + " €/Monat", Value = "3" });
      }
      model.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 2 - " + CornerkickManager.Finance.iCostKibitzer[2].ToString("N0", MemberController.getCi(clb)) + " €/Monat", Value = "2" });
      model.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 1 - " + CornerkickManager.Finance.iCostKibitzer[1].ToString("N0", MemberController.getCi(clb)) + " €/Monat", Value = "1" });
      model.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "-", Value = "0" });

      // Tutorial
      if (CkAppShared.ttUser != null) {
        int iUserIx = CkAppShared.ckMng.ltUser.IndexOf(_usr);
        if (iUserIx >= 0 && iUserIx < CkAppShared.ttUser.Length) model.tutorial = CkAppShared.ttUser[iUserIx];
      }

      return model;
    }

    public static List<CornerkickManager.Main.Staff.Doctor> GetDrClub(CornerkickManager.User usr)
    {
      List<CornerkickManager.Main.Staff.Doctor> ltDr = new List<CornerkickManager.Main.Staff.Doctor>();

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return ltDr;
      if (clb.staff.ltDoctor == null) return ltDr;

      foreach (CornerkickManager.Main.Staff.Doctor dr in clb.staff.ltDoctor) {
        if (dr.iId >= 0) ltDr.Add(dr);
      }

      return ltDr;
    }

    public static List<CornerkickManager.Main.Staff.Doctor> GetDrFree(CornerkickManager.User usr)
    {
      List<CornerkickManager.Main.Staff.Doctor> ltDr = new List<CornerkickManager.Main.Staff.Doctor>();

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return ltDr;

      foreach (CornerkickManager.Main.Staff.Doctor dr in CornerkickManager.Main.staff.ltDoctor) {
        if (clb.staff.ltDoctor != null) {
          bool bCont = false;
          foreach (CornerkickManager.Main.Staff.Doctor drClub in clb.staff.ltDoctor) {
            if (dr.iId == drClub.iId) {
              bCont = true;
              break;
            }
          }
          if (bCont) continue;
        }

        ltDr.Add(dr);
      }

      return ltDr;
    }

    private string getDrImg(CornerkickManager.Main.Staff.Doctor dr)
    {
      return getDrImg(dr.iId, dr.sName);
    }
    private string getDrImg(int iDrId, string sDrName)
    {
      string sPortraitFile = System.IO.Path.Combine(CkAppShared.sHomeDir, "Content", "Images", "portraits", "doctors", iDrId.ToString() + ".png");
      if (System.IO.File.Exists(sPortraitFile)) {
        return "<img src=\"/Content/Images/portraits/doctors/" + iDrId.ToString() + ".png\" alt=\"Portrait\" style=\"width:100%\" title=\"" + sDrName + "\" >";
      }

      return "";
    }
    private object getDrObj(CornerkickManager.Main.Staff.Doctor dr)
    {
      return new { iId = dr.iId, sName = dr.sName, iSkillMuscle = dr.iSkillMuscle, iSkillTendons = dr.iSkillTendons, iSkillFracture = dr.iSkillFracture, iSkillInternist = dr.iSkillInternist, iCost = dr.getSalary(), iPayOff = dr.getPayOff(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd), sImg = getDrImg(dr) };
    }

    public static CornerkickManager.Main.Staff.Doctor? HireDr(CornerkickManager.User usr, int iDrId)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return null;

      CornerkickManager.Main.Staff.Doctor? dr = CornerkickManager.Main.staff.ltDoctor.Find(d => d.iId == iDrId);
      if (dr != null) {
        if (clb.staff.ltDoctor == null) clb.staff.ltDoctor = new List<CornerkickManager.Main.Staff.Doctor>();

        CornerkickManager.Main.Staff.Doctor drNew = dr.Clone();
        clb.staff.ltDoctor.Add(drNew);
        return drNew;
      }

      return null;
    }

    public static bool FireDr(CornerkickManager.User? usr, int iDrId)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return false;

      foreach (CornerkickManager.Main.Staff.Doctor dr in clb.staff.ltDoctor) {
        if (dr.iId == iDrId) {
          clb.staff.ltDoctor.Remove(dr);
          CornerkickManager.Finance.doTransaction(clb, CkAppShared.ckMng.dtDatum, -dr.getPayOff(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd), CornerkickManager.Finance.iTransferralTypePaySalaryStaff, "Abfindungen");
          return true;
        }
      }

      return false;
    }

    public static List<CornerkickManager.Main.Staff.Scout> GetScoutsClub(CornerkickManager.User usr)
    {
      List<CornerkickManager.Main.Staff.Scout> ltScouts = new List<CornerkickManager.Main.Staff.Scout>();

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return ltScouts;

      foreach (CornerkickManager.Main.Staff.Scout sc in clb.staff.ltScouts) {
        if (sc.iId >= 0 && !sc.bFreelancer) ltScouts.Add(sc);
      }

      return ltScouts;
    }

    public static List<CornerkickManager.Main.Staff.Scout> GetScoutsFree(CornerkickManager.User usr)
    {
      List<CornerkickManager.Main.Staff.Scout> ltScouts = new List<CornerkickManager.Main.Staff.Scout>();

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return ltScouts;

      foreach (CornerkickManager.Main.Staff.Scout sc in CornerkickManager.Main.staff.ltScouts) {
        bool bCont = false;
        foreach (CornerkickManager.Main.Staff.Scout scClub in clb.staff.ltScouts) {
          if (sc.iId == scClub.iId) {
            bCont = true;
            break;
          }
        }
        if (bCont) continue;

        ltScouts.Add(sc);
      }

      return ltScouts;
    }

    private string getScoutImg(CornerkickManager.Main.Staff.Scout sc)
    {
      return getScoutImg(sc.iId, sc.sName);
    }
    private string getScoutImg(int iScoutId, string sScoutName)
    {
      string sPortraitFile = System.IO.Path.Combine(CkAppShared.sHomeDir, "Content", "Images", "portraits", "scouts", iScoutId.ToString() + ".png");
      if (System.IO.File.Exists(sPortraitFile)) {
        return "<img src=\"/Content/Images/portraits/scouts/" + iScoutId.ToString() + ".png\" alt=\"Portrait\" style=\"width:100%\" title=\"" + sScoutName + "\" >";
      }

      return "";
    }
    private object getScoutObj(CornerkickManager.Main.Staff.Scout sc)
    {
      List<CkAppShared.DataPointDD> dataPoints = new List<CkAppShared.DataPointDD>();
      Meta.Numerics.Statistics.Distributions.ContinuousDistribution normal = new Meta.Numerics.Statistics.Distributions.NormalDistribution(mu: 0.0, sigma: sc.getSigma());

      for (double x = -2.0; x < 2.01; x += 0.1) {
        dataPoints.Add(new CkAppShared.DataPointDD(x, normal.ProbabilityDensity(x), z: Math.Pow(sc.getSigma(), 2.0).ToString("0.00")));
      }

      int iMinutesActivate = 0;
      if (sc.bFreelancer) {
        iMinutesActivate = -1;
      } else if (!sc.bActive) {
        iMinutesActivate = (int)(CkAppShared.ckMng.dtDatum.Date.Add(new TimeSpan(12, 00, 00)) - CkAppShared.ckMng.dtDatum).TotalMinutes;
        if (iMinutesActivate < 0) iMinutesActivate += 24 * 60;
      }

      return new { iId = sc.iId, sName = sc.sName, iSkill = sc.iSkill, nData = sc.nDataPerScouting, iCost = sc.getSalary(), iPayOff = sc.getPayOff(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd), normal_dist = dataPoints, sImg = getScoutImg(sc), iMinutesActivate = iMinutesActivate, nPlData = sc.ltPlayerData != null ? sc.ltPlayerData.Count : 0 };
    }

    public static bool HireScout(CornerkickManager.User usr, int iScId)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return false;

      CornerkickManager.Main.Staff.Scout? scNew = CornerkickManager.Main.staff.ltScouts.Find(s => s.iId == iScId);
      if (scNew != null) {
        clb.staff.ltScouts.Add(scNew.Clone(bReduced: true));
        return true;
      }

      return false;
    }

    public static bool FireScout(CornerkickManager.User usr, int iScId)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return false;

      foreach (CornerkickManager.Main.Staff.Scout sc in clb.staff.ltScouts) {
        if (sc.iId == iScId) {
          clb.staff.ltScouts.Remove(sc);
          CornerkickManager.Finance.doTransaction(clb, CkAppShared.ckMng.dtDatum, -sc.getPayOff(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd), CornerkickManager.Finance.iTransferralTypePaySalaryStaff, "Abfindungen");
          return true;
        }
      }

      return false;
    }

    public List<object>? ScoutGetPlayerList(CornerkickManager.User usr, int iScoutId)
    {
      List<object> ltScoutedPlayer = new List<object>();

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return null;

      CornerkickManager.Main.Staff? staff = ClubController.getClubStaff(clb);
      if (staff == null) return null;

      CornerkickManager.Main.Staff.Scout? sc = staff.ltScouts.Find(s => s.iId == iScoutId);
      if (sc == null) return null;

      List<CornerkickManager.Main.Staff.Scout.PlayerData> ltScPlData = new List<CornerkickManager.Main.Staff.Scout.PlayerData>(sc.ltPlayerData);

      foreach (CornerkickManager.Main.Staff.Scout.PlayerData spd in ltScPlData) {
        CornerkickManager.Player? plMng = CkAppShared.ckMng.ltPlayer.Find(p => p.plGame.iId == spd.pl.iId);
        float[] fSkills = staff.getScoutedSkills(spd.pl, sc);
        float fSkillAve = CornerkickGame.Tool.getAveSkill(spd.pl, fSkills: fSkills);
        ltScoutedPlayer.Add(new { player_id = spd.pl.iId, player_name = spd.pl.sName, date = spd.ltDetails.Max(d => d.dt), player_skill = fSkillAve, club_name = plMng.contract.club != null ? plMng.contract.club.sName : "vereinslos" });
      }

      return ltScoutedPlayer;
    }

    public static bool SetDr(CornerkickManager.User usr, int iPlId, int iDrId)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return false;
      if (clb.staff.ltDoctor == null) return false;

      CornerkickGame.Player? plInj = clb.ltPlayer.Find(p => p.plGame.iId == iPlId)?.plGame;
      if (plInj == null) plInj = clb.ltPlayerJouth.Find(p => p.plGame.iId == iPlId)?.plGame;
      if (plInj == null) return false;

      CornerkickManager.Main.Staff.Doctor? drCur = clb.staff.ltDoctor.Find(d => d.plPatient?.iId == iPlId);
      if (drCur != null) drCur.plPatient = null;

      CornerkickManager.Main.Staff.Doctor? drNew = clb.staff.ltDoctor.Find(d => d.iId == iDrId);
      if (drNew == null) return false;

      drNew.plPatient = plInj;

      return true;
    }

    public static int[] GetCost(CornerkickManager.User? usr, byte[] iLevel)
    {
      if (usr == null) return [0, 0];

      CornerkickManager.Club clb = new CornerkickManager.Club();

      clb.staff.iCoTrainer     = iLevel[0];
      clb.staff.iCondiTrainer  = iLevel[1];
      clb.staff.iPhysio        = iLevel[2];
      clb.staff.iMentalTrainer = iLevel[3];
      clb.staff.iJouthTrainer  = iLevel[4];
      clb.staff.iJouthScouting = iLevel[5];
      clb.staff.iKibitzer      = iLevel[6];

      // Add cost for doctors
      CornerkickManager.Club? clbUser = MemberController.ckClub(usr);
      if (clbUser != null) {
        if (clbUser.staff.ltDoctor != null) {
          clb.staff.ltDoctor = clbUser.staff.ltDoctor;
        }
      }

      return [(int)(clb.getSalaryStuff() / 12f), getPayOff(usr, iLevel)];
    }

    private static int getPayOff(CornerkickManager.User usr, byte[] iLevelNew)
    {
      int iPayOff = 0;
      for (byte iP = 0; iP < iLevelNew.Length; iP++) iPayOff += getPayOff(usr, iP, iLevelNew[iP]);

      return iPayOff;
    }

    private static int getPayOff(CornerkickManager.User usr, int iPersonal, byte iLevelNew)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return 0;
      if (CkAppShared.ckMng.dtDatum.Date.Equals(CkAppShared.ckMng.dtSeasonStart.Date)) return 0; // If start of season --> change staff for free

      int iMoney = 0;
      //int iMonthDiff = (CkAppShared.ckMng.dtSeasonEnd.Month - CkAppShared.ckMng.dtDatum.Month) + (12 * (CkAppShared.ckMng.dtSeasonEnd.Year - CkAppShared.ckMng.dtDatum.Year));
      int iMonthDiff = (int)Math.Ceiling((CkAppShared.ckMng.dtSeasonEnd - CkAppShared.ckMng.dtDatum).TotalDays / 30.0);
      if      (iPersonal == 0 && iLevelNew != clb.staff.iCoTrainer) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostCoachCo[clb.staff.iCoTrainer] / 2);
      else if (iPersonal == 1 && iLevelNew != clb.staff.iCondiTrainer) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostCoachCondi[clb.staff.iCondiTrainer] / 2);
      else if (iPersonal == 2 && iLevelNew != clb.staff.iPhysio) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostMasseur[clb.staff.iPhysio] / 2);
      else if (iPersonal == 3 && iLevelNew != clb.staff.iMentalTrainer) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostMental[clb.staff.iMentalTrainer] / 2);
      else if (iPersonal == 4 && iLevelNew != clb.staff.iJouthTrainer) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostJouthCoach[clb.staff.iJouthTrainer] / 2);
      else if (iPersonal == 5 && iLevelNew != clb.staff.iJouthScouting) iMoney = clb.staff.iJouthScouting < CornerkickManager.Finance.iCostJouthScouting.Length ? iMonthDiff * (CornerkickManager.Finance.iCostJouthScouting[clb.staff.iJouthScouting] / 2) : 0;
      else if (iPersonal == 6 && iLevelNew != clb.staff.iKibitzer) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostKibitzer[clb.staff.iKibitzer] / 2);

      return iMoney;
    }

    public static bool Hire(CornerkickManager.User? usr, int iType, int iLevel, out string sMsg)
    {
      sMsg = "";

      if (usr == null) return false;

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return false;

      // First: Pay personal pay-off costs
      int iPayOff = getPayOff(usr, iType, (byte)iLevel);
      CornerkickManager.Finance.doTransaction(clb, CkAppShared.ckMng.dtDatum, -iPayOff, CornerkickManager.Finance.iTransferralTypePaySalaryStaff, "Abfindungen");

      // Then, hire new personal
      if (iType == 0) {
        if (clb.staff.iCoTrainer == iLevel) {
          sMsg = "Keine Änderung";
          return false;
        }
        clb.staff.iCoTrainer = (byte)iLevel;
        sMsg = "Neuen Co-Trainer eingestellt.";
        if (iPayOff > 0) sMsg += "<br>Abfindung für bisherigen Co-Trainer: " + iPayOff.ToString("N0", MemberController.getCi(clb)) + " €";
        return true;
      } else if (iType == 1) {
        if (clb.staff.iCondiTrainer == iLevel) {
          sMsg = "Keine Änderung";
          return false;
        }
        clb.staff.iCondiTrainer = (byte)iLevel;
        sMsg = "Neuen Konditionstrainer eingestellt.";
        if (iPayOff > 0) sMsg += " Abfindung für bisherigen Konditionstrainer: " + iPayOff.ToString("N0", MemberController.getCi(clb)) + " €";
        return true;
      } else if (iType == 2) {
        if (clb.staff.iPhysio == iLevel) {
          sMsg = "Keine Änderung";
          return false;
        }
        clb.staff.iPhysio = (byte)iLevel;
        sMsg = "Neuen Masseur eingestellt.";
        if (iPayOff > 0) sMsg += " Abfindung für bisherigen Masseur: " + iPayOff.ToString("N0", MemberController.getCi(clb)) + " €";
        return true;
      } else if (iType == 3) {
        if (clb.staff.iMentalTrainer == iLevel) {
          sMsg = "Keine Änderung";
          return false;
        }
        clb.staff.iMentalTrainer = (byte)iLevel;
        sMsg = "Neuen Mentaltrainer eingestellt.";
        if (iPayOff > 0) sMsg += " Abfindung für bisherigen Mentaltrainer: " + iPayOff.ToString("N0", MemberController.getCi(clb)) + " €";
        return true;
      } else if (iType == 4) {
        if (clb.staff.iJouthTrainer == iLevel) {
          sMsg = "Keine Änderung";
          return false;
        }
        clb.staff.iJouthTrainer = (byte)iLevel;
        sMsg = "Neuen Jugendtrainer eingestellt.";
        if (iPayOff > 0) sMsg += " Abfindung für bisherigen Jugendtrainer: " + iPayOff.ToString("N0", MemberController.getCi(clb)) + " €";
        return true;
      } else if (iType == 5) {
        if (clb.staff.iJouthScouting == iLevel) {
          sMsg = "Keine Änderung";
          return false;
        }
        clb.staff.iJouthScouting = (byte)iLevel;
        sMsg = "Neuen Jugendscout eingestellt.";
        if (iPayOff > 0) sMsg += " Abfindung für bisherigen Jugendscout: " + iPayOff.ToString("N0", MemberController.getCi(clb)) + " €";
        return true;
      } else if (iType == 6) {
        if (clb.staff.iKibitzer == iLevel) {
          sMsg = "Keine Änderung";
          return false;
        }
        clb.staff.iKibitzer = (byte)iLevel;
        sMsg = "Neuen Spielbeobachter eingestellt.";
        if (iPayOff > 0) sMsg += " Abfindung für bisherigen Spielbeobachter: " + iPayOff.ToString("N0", MemberController.getCi(clb)) + " €";
        return true;
      } else if (iType == 7) {
        //if (clb.staff.iCoTrainer == iLevel) return Json(new { ok = false, message = "Keine Änderung" });
        CornerkickManager.Main.Staff.Doctor? drNew = HireDr(usr, iLevel);
        if (drNew != null) sMsg = "Neuen Arzt " + drNew.sName + " eingestellt.";
        if (iPayOff > 0) sMsg += " Abfindung für bisherigen Arzt: " + iPayOff.ToString("N0", MemberController.getCi(clb)) + " €";
        return true;
      } else if (iType == 8) {
        HireScout(usr, iLevel);
        sMsg = "Neuen Scout eingestellt.";
        return true;
      }

      return false;
    }

    public static List<StaffModel.TableItemInjuredPlayer> GetInjuredPlayer(CornerkickManager.User usr)
    {
      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return new List<StaffModel.TableItemInjuredPlayer>();

      List<StaffModel.TableItemInjuredPlayer.DrFree> ltDtInjPlDrFree = new List<StaffModel.TableItemInjuredPlayer.DrFree>();

      if (clb.staff.ltDoctor != null) {
        //int iIx = CornerkickManager.Main.staff.ltDoctor.Count;
        foreach (CornerkickManager.Main.Staff.Doctor dr in clb.staff.ltDoctor) {
          ltDtInjPlDrFree.Add(new StaffModel.TableItemInjuredPlayer.DrFree() { sName = dr.sName, iId = dr.iId });
        }
      }

      List<StaffModel.TableItemInjuredPlayer> ltDtInjPl = new List<StaffModel.TableItemInjuredPlayer>();
      foreach (CornerkickManager.Player pl in clb.ltPlayer) {
        if (pl.plGame.injury != null) {
          int iDrId = -9;
          float fDrInjRedFac = 0f;
          if (clb.staff.ltDoctor != null) {
            foreach (CornerkickManager.Main.Staff.Doctor dr in clb.staff.ltDoctor) {
              if (dr.plPatient?.iId == pl.plGame.iId) {
                iDrId = dr.iId;
                fDrInjRedFac = dr.getInjuryReductionFactor(pl.plGame);
                break;
              }
            }
          }
          ltDtInjPl.Add(
            new StaffModel.TableItemInjuredPlayer() {
              sPlName = pl.plGame.sName,
              iPlId = pl.plGame.iId,
              sInjuryName = pl.plGame.injury.sName,
              fInjuryRest = pl.plGame.injury.fLength * (1f - fDrInjRedFac),
              fInjuryProgress = (pl.plGame.injury.iLengthStart - pl.plGame.injury.fLength) / pl.plGame.injury.iLengthStart,
              drFree = ltDtInjPlDrFree.ToArray(),
              iDrId = iDrId,
              fDrInjRedFac = fDrInjRedFac
            });
        }
      }

      return ltDtInjPl;
    }

    public static CornerkickGame.Player.Doping? GetDoping(int iDp)
    {
      if (iDp >= CkAppShared.ckMng.ltDoping.Count) return null;

      return CkAppShared.ckMng.ltDoping[iDp];
    }

    public static bool DoDoping(CornerkickManager.User usr, int iPlayerIx, int iDp, out string sMsg)
    {
      sMsg = "";

      if (iDp < 0) return false;
      if (iDp >= CkAppShared.ckMng.ltDoping.Count) return false;

      CornerkickManager.Club? clb = MemberController.ckClub(usr);
      if (clb == null) return false;

      if (iPlayerIx < 0) return false;
      if (iPlayerIx >= clb.ltPlayer.Count) return false;
      CornerkickManager.Player pl = clb.ltPlayer[iPlayerIx];
      if (pl == null) return false;

      if (usr != null && usr.game != null && !usr.game.data.bFinished) {
        sMsg = "Kein Doping während eines Spiels möglich!";
        return false;
      }

      int iRet = CkAppShared.ckMng.plt.doDoping(pl, CkAppShared.ckMng.ltDoping[iDp]);
      if      (iRet == 1) sMsg = "Der Spieler hat keinen Vertrag bei Ihnen.";
      else if (iRet == 2) sMsg = "Der Inhalt Ihrer Schwarzen Kasse reicht leider nicht aus...";
      else if (iRet == 3) sMsg = "Sie können nicht während eines Spiels dopen.";

      return iRet == 0;
    }

  }
}
