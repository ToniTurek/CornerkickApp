using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CornerkickApp.Platforms.Android
{
  internal class Initialize
  {
    internal static void initialize(CornerkickManager.Main ckMng)
    {
      string sRsrc = "";

      sRsrc = Task.Run(() => getResource("user_responsibility_level_names.txt")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) CornerkickManager.Names.sUserRespLvlNames = sRsrc.ReplaceLineEndings().Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s)).ToArray();

      sRsrc = Task.Run(() => getResource("position_names.txt")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Main.sPosition = sRsrc.ReplaceLineEndings().Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s)).ToArray();
        CornerkickManager.Names.sPosition = CornerkickManager.Main.sPosition;
      }

      sRsrc = Task.Run(() => getResource("skill_names.txt")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.PlayerTool.sSkills = sRsrc.ReplaceLineEndings().Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s)).ToArray();
        CornerkickManager.Names.sSkills = CornerkickManager.PlayerTool.sSkills;
      }

      sRsrc = Task.Run(() => getResource("skill_category_names.txt")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) CornerkickManager.Names.sSkillCategorys = sRsrc.ReplaceLineEndings().Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s)).ToArray();

      // Initialize formations from resource (using json file format)
      ckMng.ltFormationen = new List<CornerkickGame.Tactic.Formation>();
      sRsrc = Task.Run(() => getResource("formations.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Initialize.FormationsJson? frmJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.FormationsJson>(sRsrc);
        if (frmJson != null) {
          foreach (CornerkickManager.Initialize.FormationsJson.Formation2Json frm2Json in frmJson.formations) {
            CornerkickGame.Tactic.Formation frm = new CornerkickGame.Tactic.Formation(frm2Json.positions.Length);
            frm.iId = frm2Json.id;
            frm.sName = frm2Json.name ?? frm2Json.name;
            frm.positions = frm2Json.positions.Select(
              pos => new CornerkickGame.Tactic.Formation.Position {
                pt = new System.Drawing.Point(
                  (int)Math.Round(pos[0] * CornerkickGame.Game.ptPitch100.X),
                  (int)Math.Round(pos[1] * CornerkickGame.Game.ptPitch100.Y)
                )
              }).ToArray();
            ckMng.ltFormationen.Add(frm);
          }
        }
      }

      // Initialize trainings from resource (using json file format)
      ckMng.plt.ltTraining = new List<CornerkickManager.PlayerTool.Training>();
      sRsrc = Task.Run(() => getResource("training.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Initialize.TrainingJson? trJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.TrainingJson>(sRsrc);
        if (trJson != null) {
          foreach (CornerkickManager.Initialize.TrainingJson.Training2Json tr2jsn in trJson.training) {
            CornerkickManager.PlayerTool.Training training = new CornerkickManager.PlayerTool.Training();
            training.iId = tr2jsn.id;
            training.sName = tr2jsn.name;
            training.fCondi = tr2jsn.condi;
            training.fFresh = tr2jsn.fresh;
            training.fMoral = tr2jsn.moral;
            training.fInjuryRisk = tr2jsn.injuryrisk;

            training.ltSkillBonus = new List<CornerkickManager.PlayerTool.Training.SkillBonus>();
            foreach (CornerkickManager.Initialize.TrainingJson.Training2Json.SkillBonus sbjsn in tr2jsn.skillbonus) {
              CornerkickManager.PlayerTool.Training.SkillBonus sb = new CornerkickManager.PlayerTool.Training.SkillBonus();
              sb.iSkillId = sbjsn.skill;
              sb.fBonus = sbjsn.bonus;
              sb.iExcludeType = sbjsn.exclude;
              training.ltSkillBonus.Add(sb);
            }

            ckMng.plt.ltTraining.Add(training);
          }
        }
      }

      // Initialize injuries from resource (using json file format)
      CornerkickGame.Game.ltInjury = new List<CornerkickGame.Player.Injury>();
      sRsrc = Task.Run(() => getResource("injuries.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Initialize.InjuriesJson? injJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.InjuriesJson>(sRsrc);
        if (injJson != null) {
          foreach (CornerkickManager.Initialize.InjuriesJson.Injuries2Json inj2jsn in injJson.injuries) {
            CornerkickGame.Player.Injury inj = new CornerkickGame.Player.Injury();
            inj.sName = inj2jsn.name;
            inj.iType = inj2jsn.category;
            inj.iLengthMin = inj2jsn.days_min;
            inj.iLengthMax = inj2jsn.days_max;
            CornerkickGame.Game.ltInjury.Add(inj);
          }
        }
      }
      //Names.ltInjury = Main.ltInjury;

      // Initialize staff from resource (using json file format)
      CornerkickManager.Main.staff = new CornerkickManager.Main.Staff();
      sRsrc = Task.Run(() => getResource("staff.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Initialize.StaffJson? staffJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.StaffJson>(sRsrc);
        if (staffJson != null) {
          foreach (CornerkickManager.Initialize.StaffJson.Staff2Json staff2jsn in staffJson.staff) {
            if (staff2jsn.name.Equals("Med")) {
              int iIx = 0;
              foreach (CornerkickManager.Initialize.StaffJson.Staff2Json.Med med in staff2jsn.doctors) {
                CornerkickManager.Main.Staff.Doctor dr = new CornerkickManager.Main.Staff.Doctor();
                dr.iId = iIx++;
                dr.sName = med.name;
                dr.iSkillMuscle = (byte)med.skill_muscle;
                dr.iSkillTendons = (byte)med.skill_tendons;
                dr.iSkillFracture = (byte)med.skill_fracture;
                dr.iSkillInternist = (byte)med.skill_internist;
                dr.bDoping = med.doping;
                CornerkickManager.Main.staff.ltDoctor.Add(dr);
              }
            } else if (staff2jsn.name.Equals("Scout")) {
              int iIx = 0;
              foreach (CornerkickManager.Initialize.StaffJson.Staff2Json.Scout scout in staff2jsn.scouts) {
                CornerkickManager.Main.Staff.Scout sc = new CornerkickManager.Main.Staff.Scout();
                sc.iId = iIx++;
                sc.sName = scout.name;
                sc.iSkill = (byte)scout.skill;
                sc.nDataPerScouting = (byte)scout.skill_data_per_scouting;
                sc.bFreelancer = scout.freelancer;
                CornerkickManager.Main.staff.ltScouts.Add(sc);
              }
            }
          }
        }
      }

      // Initialize doping from resource (using json file format)
      ckMng.ltDoping = new List<CornerkickGame.Player.Doping>();
      sRsrc = Task.Run(() => getResource("doping.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Initialize.DopingJson? dopJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.DopingJson>(sRsrc);
        if (dopJson != null) {
          foreach (CornerkickManager.Initialize.DopingJson.Doping2Json dp2jsn in dopJson.doping) {
            CornerkickGame.Player.Doping dop = new CornerkickGame.Player.Doping();
            dop.sName = dp2jsn.name;
            dop.fReductionRate = dp2jsn.reduction;
            dop.fEffectMax = dp2jsn.condi;
            dop.fEffect = dop.fEffectMax;
            dop.fFreshGain = dp2jsn.fresh;
            dop.fDetectable = dp2jsn.detectable;
            dop.iCost = dp2jsn.cost;
            ckMng.ltDoping.Add(dop);
          }
        }
      }

      // Initialize cup round names from resource
      sRsrc = Task.Run(() => getResource("cup_round_names.txt")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) CornerkickManager.Main.sCupRound = sRsrc.ReplaceLineEndings().Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s)).ToArray();
      CornerkickManager.Names.sCupRound = CornerkickManager.Main.sCupRound;

      // Initialize news from resource (using json file format)
      sRsrc = Task.Run(() => getResource("news.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Initialize.News2Json? ltNews2 = JsonConvert.DeserializeObject<CornerkickManager.Initialize.News2Json>(sRsrc);
        if (ltNews2 != null) CornerkickManager.Main.ltNews = ltNews2.news;
      }

      // Initialize nation names from resource
      sRsrc = Task.Run(() => getResource("nations.txt")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        List<string> ltNationNames = sRsrc.ReplaceLineEndings().Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s)).ToList();

        CornerkickManager.Main.sLand = new string[ltNationNames.Count];
        CornerkickManager.Main.sLandShort = new string[ltNationNames.Count];
        CornerkickManager.Main.clNat1 = new System.Drawing.Color[ltNationNames.Count][];
        CornerkickManager.Main.clNat2 = new System.Drawing.Color[ltNationNames.Count][];
        int iN = 0;
        foreach (string sNationName in ltNationNames) {
          string[] sNnSplit = sNationName.Split(',');
          if (sNnSplit.Length > 0) CornerkickManager.Main.sLand[iN] = sNnSplit[0].Trim();
          if (sNnSplit.Length > 1) CornerkickManager.Main.sLandShort[iN] = sNnSplit[1].Trim();
          if (sNnSplit.Length > 4) {
            CornerkickManager.Main.clNat1[iN] = new System.Drawing.Color[3];
            CornerkickManager.Main.clNat1[iN][0] = System.Drawing.ColorTranslator.FromHtml(sNnSplit[2].Trim());
            CornerkickManager.Main.clNat1[iN][1] = System.Drawing.ColorTranslator.FromHtml(sNnSplit[3].Trim());
            CornerkickManager.Main.clNat1[iN][2] = System.Drawing.ColorTranslator.FromHtml(sNnSplit[4].Trim());
          }
          if (sNnSplit.Length > 7) {
            CornerkickManager.Main.clNat2[iN] = new System.Drawing.Color[3];
            CornerkickManager.Main.clNat2[iN][0] = System.Drawing.ColorTranslator.FromHtml(sNnSplit[5].Trim());
            CornerkickManager.Main.clNat2[iN][1] = System.Drawing.ColorTranslator.FromHtml(sNnSplit[6].Trim());
            CornerkickManager.Main.clNat2[iN][2] = System.Drawing.ColorTranslator.FromHtml(sNnSplit[7].Trim());
          }
          iN++;
        }
      }

      // Initialize stadium data from resource (using json file format)
      sRsrc = Task.Run(() => getResource("stadium.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Initialize.StadiumJson? stadJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.StadiumJson>(sRsrc);

        if (stadJson != null) {
          CornerkickManager.Stadium.sBlocktype = stadJson.block_type_names?.ToArray();

          if (stadJson.facility.video != null) {
            CornerkickManager.Stadium.facVideo = new CornerkickManager.Stadium.FacilityTemplate();
            CornerkickManager.Stadium.facVideo.sName = stadJson.facility.video.name;
            CornerkickManager.Stadium.facVideo.sLevelNames = stadJson.facility.video.level_names?.ToArray();
            CornerkickManager.Stadium.facVideo.iCost = stadJson.facility.video.costs?.ToArray();
            CornerkickManager.Stadium.facVideo.iDaysConstruct = stadJson.facility.video.construct?.ToArray();
          }

          if (stadJson.facility.security != null) {
            CornerkickManager.Stadium.facSecurity = new CornerkickManager.Stadium.FacilityTemplate();
            CornerkickManager.Stadium.facSecurity.sName = stadJson.facility.security.name;
            CornerkickManager.Stadium.facSecurity.sLevelNames = stadJson.facility.security.level_names?.ToArray();
            CornerkickManager.Stadium.facSecurity.iCost = stadJson.facility.security.costs?.ToArray();
            CornerkickManager.Stadium.facSecurity.iDaysConstruct = stadJson.facility.security.construct?.ToArray();
          }
        }
      }

      // Initialize building names from resource (using json file format)
      sRsrc = Task.Run(() => getResource("buildings.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Initialize.BuildingNamesJson? bdgJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.BuildingNamesJson>(sRsrc);

        if (bdgJson != null) {
          // Training Court
          CornerkickManager.Stadium.bdgTrainingCourts = new CornerkickManager.Stadium.BuildingTemplate();
          CornerkickManager.Stadium.bdgTrainingCourts.sTypeName = bdgJson.training_court.building_type_name;
          CornerkickManager.Stadium.bdgTrainingCourts.sLevelNames = bdgJson.training_court.names?.ToArray();
          CornerkickManager.Stadium.bdgTrainingCourts.iCost = bdgJson.training_court.costs?.ToArray();
          CornerkickManager.Stadium.bdgTrainingCourts.iDaysConstruct = bdgJson.training_court.construct?.ToArray();
          CornerkickManager.Stadium.bdgTrainingCourts.iGround = bdgJson.training_court.grounds?.ToArray();
          CornerkickManager.Stadium.bdgTrainingCourts.iCostMaintenance = bdgJson.training_court.maintenance?.ToArray();

          // Gym
          CornerkickManager.Stadium.bdgGym = new CornerkickManager.Stadium.BuildingTemplate();
          CornerkickManager.Stadium.bdgGym.sTypeName = bdgJson.gym.building_type_name;
          CornerkickManager.Stadium.bdgGym.sLevelNames = bdgJson.gym.names?.ToArray();
          CornerkickManager.Stadium.bdgGym.iCost = bdgJson.gym.costs?.ToArray();
          CornerkickManager.Stadium.bdgGym.iDaysConstruct = bdgJson.gym.construct?.ToArray();
          CornerkickManager.Stadium.bdgGym.iGround = bdgJson.gym.grounds?.ToArray();
          CornerkickManager.Stadium.bdgGym.iCostMaintenance = bdgJson.gym.maintenance?.ToArray();

          // Spa
          CornerkickManager.Stadium.bdgSpa = new CornerkickManager.Stadium.BuildingTemplate();
          CornerkickManager.Stadium.bdgSpa.sTypeName = bdgJson.spa.building_type_name;
          CornerkickManager.Stadium.bdgSpa.sLevelNames = bdgJson.spa.names?.ToArray();
          CornerkickManager.Stadium.bdgSpa.iCost = bdgJson.spa.costs?.ToArray();
          CornerkickManager.Stadium.bdgSpa.iDaysConstruct = bdgJson.spa.construct?.ToArray();
          CornerkickManager.Stadium.bdgSpa.iGround = bdgJson.spa.grounds?.ToArray();
          CornerkickManager.Stadium.bdgSpa.iCostMaintenance = bdgJson.spa.maintenance?.ToArray();

          // Jouth internat
          CornerkickManager.Stadium.bdgJouthInternat = new CornerkickManager.Stadium.BuildingTemplate();
          CornerkickManager.Stadium.bdgJouthInternat.sTypeName = bdgJson.jouth_internat.building_type_name;
          CornerkickManager.Stadium.bdgJouthInternat.sLevelNames = bdgJson.jouth_internat.names?.ToArray();
          CornerkickManager.Stadium.bdgJouthInternat.iCost = bdgJson.jouth_internat.costs?.ToArray();
          CornerkickManager.Stadium.bdgJouthInternat.iDaysConstruct = bdgJson.jouth_internat.construct?.ToArray();
          CornerkickManager.Stadium.bdgJouthInternat.iGround = bdgJson.jouth_internat.grounds?.ToArray();
          CornerkickManager.Stadium.bdgJouthInternat.iCostMaintenance = bdgJson.jouth_internat.maintenance?.ToArray();

          // Club house
          CornerkickManager.Stadium.bdgClubHouse = new CornerkickManager.Stadium.BuildingTemplate();
          CornerkickManager.Stadium.bdgClubHouse.sTypeName = bdgJson.club_house.building_type_name;
          CornerkickManager.Stadium.bdgClubHouse.sLevelNames = bdgJson.club_house.names?.ToArray();
          CornerkickManager.Stadium.bdgClubHouse.iCost = bdgJson.club_house.costs?.ToArray();
          CornerkickManager.Stadium.bdgClubHouse.iDaysConstruct = bdgJson.club_house.construct?.ToArray();
          CornerkickManager.Stadium.bdgClubHouse.iGround = bdgJson.club_house.grounds?.ToArray();
          CornerkickManager.Stadium.bdgClubHouse.iCostMaintenance = bdgJson.club_house.maintenance?.ToArray();

          // Club museum
          CornerkickManager.Stadium.bdgClubMuseum = new CornerkickManager.Stadium.BuildingTemplate();
          CornerkickManager.Stadium.bdgClubMuseum.sTypeName = bdgJson.club_museum.building_type_name;
          CornerkickManager.Stadium.bdgClubMuseum.sLevelNames = bdgJson.club_museum.names?.ToArray();
          CornerkickManager.Stadium.bdgClubMuseum.iCost = bdgJson.club_museum.costs?.ToArray();
          CornerkickManager.Stadium.bdgClubMuseum.iDaysConstruct = bdgJson.club_museum.construct?.ToArray();
          CornerkickManager.Stadium.bdgClubMuseum.iGround = bdgJson.club_museum.grounds?.ToArray();
          CornerkickManager.Stadium.bdgClubMuseum.iCostMaintenance = bdgJson.club_museum.maintenance?.ToArray();

          // Mass-transit
          CornerkickManager.Stadium.bdgMassTransit = new CornerkickManager.Stadium.BuildingTemplate();
          CornerkickManager.Stadium.bdgMassTransit.sTypeName = bdgJson.mass_transit.building_type_name;
          CornerkickManager.Stadium.bdgMassTransit.sLevelNames = bdgJson.mass_transit.names?.ToArray();
          CornerkickManager.Stadium.bdgMassTransit.iCost = bdgJson.mass_transit.costs?.ToArray();
          CornerkickManager.Stadium.bdgMassTransit.iDaysConstruct = bdgJson.mass_transit.construct?.ToArray();
          CornerkickManager.Stadium.bdgMassTransit.iGround = bdgJson.mass_transit.grounds?.ToArray();
          CornerkickManager.Stadium.bdgMassTransit.iCostMaintenance = bdgJson.mass_transit.maintenance?.ToArray();

          // Carpark
          CornerkickManager.Stadium.sCarparkName = bdgJson.carpark.building_type_name;
          CornerkickManager.Stadium.iCarparkPerGround = bdgJson.carpark.capacity_per_ground;

          // Ticketcounter
          CornerkickManager.Stadium.sTicketcounterName = bdgJson.ticket_counter.building_type_name;
          CornerkickManager.Stadium.iTicketcounterPerGround = bdgJson.ticket_counter.capacity_per_ground;

          // Fanshops
          CornerkickManager.Stadium.sFanshopName = bdgJson.fanshop.building_type_name;
          CornerkickManager.Stadium.iFanshopPerGround = bdgJson.fanshop.capacity_per_ground;
        }
      }

      // Initialize trainigcamps from resource (using json file format)
      sRsrc = Task.Run(() => getResource("trainingcamps.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Initialize.TrainingCampJson? trcJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.TrainingCampJson>(sRsrc);
        if (trcJson != null) {
          foreach (CornerkickManager.Initialize.TrainingCampJson.TrainingCamp2Json trc2jsn in trcJson.trainingcamps) {
            CornerkickManager.TrainingCamp.Camp trc = new CornerkickManager.TrainingCamp.Camp();
            trc.iId = trc2jsn.id;
            trc.sName = trc2jsn.name;
            trc.fBonusCondi = trc2jsn.condi;
            trc.fBonusFresh = trc2jsn.fresh;
            trc.fBonusMoral = trc2jsn.moral;
            trc.fBonusF = trc2jsn.skill;
            trc.fMalusFreshTravel = trc2jsn.travel_malus;
            trc.iCost = trc2jsn.cost_stay;
            trc.iCostTravel = trc2jsn.cost_travel;
            trc.tsTravel = TimeSpan.FromHours(trc2jsn.travel_duration);

            ckMng.tcp.ltCamps.Add(trc);
          }
        }
      }

      // Initialize account type names from resource (using json file format)
      sRsrc = Task.Run(() => getResource("account_names.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Finance.ltAccountTypeNames = new List<CornerkickManager.Finance.AccountTypeName>();

        CornerkickManager.Initialize.AccountTypeNamesJson? atnJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.AccountTypeNamesJson>(sRsrc);
        if (atnJson != null) {
          foreach (CornerkickManager.Initialize.AccountTypeNamesJson.AccountTypeNames2Json atn2jsn in atnJson.accounttypes) {
            CornerkickManager.Finance.AccountTypeName atn = new CornerkickManager.Finance.AccountTypeName();
            atn.iType = atn2jsn.type;
            atn.sName = atn2jsn.name;

            CornerkickManager.Finance.ltAccountTypeNames.Add(atn);
          }
        }
      }

      // Initialize sponsors from resource (using json file format)
      sRsrc = Task.Run(() => getResource("sponsor.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Initialize.SponsorJson? spnJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.SponsorJson>(sRsrc);
        if (spnJson != null) {
          ckMng.fz.ltSponsoren = new List<CornerkickManager.Finance.Spons>();
          ckMng.fz.ltSponsoren.Add(new CornerkickManager.Finance.Spons()); // Add empty sponsor
          ckMng.fz.ltSponsoren.AddRange(spnJson.sponsor);
        }
      }

      // Initialize special sponsors
      sRsrc = Task.Run(() => getResource("sponsor_special.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        ckMng.fz.ltSponsorSpecial = new List<CornerkickManager.Finance.SponsorSpecial>();

        byte iId = 1;
        CornerkickManager.Initialize.SponsorSpecialJson? spspJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.SponsorSpecialJson>(sRsrc);
        if (spspJson != null) {
          foreach (CornerkickManager.Initialize.SponsorSpecialJson.SponsorSpecial2Json spsp2jsn in spspJson.sponsorspecial) {
            CornerkickManager.Finance.SponsorSpecial ss = new CornerkickManager.Finance.SponsorSpecial();
            ss.iId = iId++;
            ss.iType = spsp2jsn.type;
            ss.iMoney = spsp2jsn.money;
            ss.sName = spsp2jsn.name;

            ckMng.fz.ltSponsorSpecial.Add(ss);
          }
        }
      }

      // Initialize merchandising from resource (using json file format)
      ckMng.ltMerchandising = new List<CornerkickManager.Merchandising.Item>();
      sRsrc = Task.Run(() => getResource("merchandising.json")).Result;
      if (!string.IsNullOrEmpty(sRsrc)) {
        CornerkickManager.Initialize.MerchandisingJson? merJson = JsonConvert.DeserializeObject<CornerkickManager.Initialize.MerchandisingJson>(sRsrc);
        if (merJson != null) {
          foreach (CornerkickManager.Initialize.MerchandisingJson.Item merItemJson in merJson.merchandising) {
            CornerkickManager.Merchandising.Item mer = new CornerkickManager.Merchandising.Item();
            mer.iId = merItemJson.id;
            mer.sName = merItemJson.name;
            mer.fPriceBuy = merItemJson.price;
            mer.fSellFactor = merItemJson.sellfactor;
            mer.fQuantityFactor = merItemJson.quantityfactor;
            mer.bPlayerJersey = merItemJson.player_jersey;

            ckMng.ltMerchandising.Add(mer);
          }
        }
      }

      sRsrc = Task.Run(() => getResource(Path.Combine("Names", "player_names.txt"))).Result;
      if (!string.IsNullOrEmpty(sRsrc)) CornerkickManager.PlayerTool.ltNames = sRsrc.ReplaceLineEndings().Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s)).ToList();

      sRsrc = Task.Run(() => getResource(Path.Combine("Names", "player_surnames.txt"))).Result;
      if (!string.IsNullOrEmpty(sRsrc)) CornerkickManager.PlayerTool.ltSurnames = sRsrc.ReplaceLineEndings().Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s)).ToList();

      if (CornerkickManager.Main.sLandShort != null) {
        CornerkickManager.PlayerTool.ltNamesNat = new List<string>[CornerkickManager.Main.sLandShort.Length];
        CornerkickManager.PlayerTool.ltSurnamesNat = new List<string>[CornerkickManager.Main.sLandShort.Length];

        for (int iL = 0; iL < CornerkickManager.Main.sLandShort.Length; iL++) {
          sRsrc = Task.Run(() => getResource(Path.Combine("Names", "player_names_" + CornerkickManager.Main.sLandShort[iL] + ".txt"))).Result;
          if (!string.IsNullOrEmpty(sRsrc)) CornerkickManager.PlayerTool.ltNamesNat[iL] = sRsrc.ReplaceLineEndings().Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s)).ToList();

          sRsrc = Task.Run(() => getResource(Path.Combine("Names", "player_surnames_" + CornerkickManager.Main.sLandShort[iL] + ".txt"))).Result;
          if (!string.IsNullOrEmpty(sRsrc)) CornerkickManager.PlayerTool.ltSurnamesNat[iL] = sRsrc.ReplaceLineEndings().Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s)).ToList();
        }
      }
    }

    public static async Task<string> getResource(string sFilename)
    {
      string dataResourceText = null;
      try {
        using var stream = await FileSystem.OpenAppPackageFileAsync(sFilename);
        using var reader = new StreamReader(stream);

        string s = await reader.ReadToEndAsync();
        dataResourceText = s;
        //Console.Write(dataResourceText);
      } catch (FileNotFoundException ex) {
        //dataResourceText = "Data file not found.";
        //Logger.LogError(ex, "'Resource/Raw/Data.txt' not found.");
      }

      return dataResourceText;
    }
  }
}
