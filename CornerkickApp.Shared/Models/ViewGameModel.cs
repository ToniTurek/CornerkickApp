using System;
using System.Collections.Generic;

namespace CornerkickApp.Shared.Models
{
  public class Point3D
  {
    /*
    public Point3D(CornerkickGame.Game.PointBall? ptBall = null)
    {
      if (ptBall.HasValue) {
        this.X = ptBall.Value.X;
        this.Y = ptBall.Value.Y;
        this.Z = ptBall.Value.Z;
      }
    }
    */

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
  }

  public class ViewGameModel : LayoutModel
  {
    public bool bAdmin { get; set; }
    public bool bOwnLiveGame { get; set; }
    public bool bSound { get; set; }

    public int iStateGlobal { get; set; }

    public CornerkickGame.Game? game { get; set; }

    public List<SelectListItem> ddlGames { get; set; }
    public string sSelectedGame { get; set; }

    public List<SelectListItem> sliSystem { get; set; }
    public int iSystem { get; set; }

    public static List<float[]> ltLoc { get; set; }
    //public string sKommentar { get; set; }

    public int iShowPitch { get; set; }
    public List<SelectListItem> ddlShowPitch { get; set; }
    public int iPositions { get; set; }
    public List<SelectListItem> ddlPositions { get; set; }
    public int iHeatmap   { get; set; }
    public List<SelectListItem> ddlHeatmap   { get; set; }
    public int iShoots    { get; set; }
    public List<SelectListItem> ddlShoots    { get; set; }
    public int iDuels     { get; set; }
    public List<SelectListItem> ddlDuels     { get; set; }
    public int iPasses    { get; set; }
    public List<SelectListItem> ddlPasses    { get; set; }

    public int iComments { get; set; }
    public List<SelectListItem> ddlComments { get; set; }

    public int iAnimations { get; set; }
    public List<SelectListItem> ddlAnimations { get; set; }

    public List<CornerkickGame.Game.Shoot>[] ltShoots { get; set; }

    public int iGameSpeed { get; set; }

    public static readonly string[] sBorderColorsKeeper = [ "rgb(57,255,20)", "rgb(243,243,21)" ];

    public class State
    {
      public enum Event : byte
      {
        NotStarted = 0,
        GoalHome = 1,
        GoalAway = 2,
        PostBar = 3,
        NoGoal = 4,
        Foul = 5,
        HalfTime = 6,
        FullTime = 7,
        PenaltyShootout = 8,
        ShootInProgress = 9,
        Offsite = 10,
        RedCard = 11,
        Wait = 99
      }

      public int iState { get; set; }
      public bool bFinished { get; set; }
      public float fBreak { get; set; }  // Fraction of break time e.g. during half-time
      public int iGameSpeedUsed { get; set; }
      public List<Player> ltPlayer { get; set; }
      public Ball gBall { get; set; }
      public string sPlActiveName { get; set; } = "";
      public string sPlActivePortraitImg { get; set; } = "";
      public int iPlActiveHA { get; set; }
      public float[]? fPlAction { get; set; }
      public float fPlActionRnd { get; set; }
      public List<PassTarget> ltPassTargets { get; set; }

      public List<string[]> ltComments { get; set; } = new List<string[]>();
      public bool bUpdate { get; set; }
      public bool bUpdateStatistic { get; set; }

      public Event? evt { get; set; } // 1: home goal, 2: away goal, 3: post/bar, 4: shoot (no goal), 5: referee whistle
      public int iEventCounter { get; set; }

      public byte iShootHA { get; set; } // Home/Away shoot
      public byte iShootRes { get; set; } // Shoot result
      public float fShootRnd { get; set; } // Shoot random

      public int[][] ltStadiumBlockSizes { get; set; } = new int[4][];

      public class SpeachOption
      {
        public string name { get; set; } = "";
        public float moral_boost { get; set; }
        public float moral_drop { get; set; }
        public float chance_moral_boost { get; set; }
        public float chance_moral_drop { get; set; }
      }
      public List<SpeachOption> ltSpeachOptions { get; set; }
    }

    public class Position
    {
      public Position(float x = 0f, float y = 0f, float z = 0f)
      {
        this.x = x;
        this.y = y;
        this.z = z;
      }
      public Position(TeamModel.Point? tmp, System.Drawing.Point ptPitch)
      {
        if (tmp == null) return;

        Position? p = convertCkToRel(new System.Drawing.Point(tmp.x, tmp.y), ptPitch);
        if (p != null) {
          this.x = p.x;
          this.y = p.y;
          this.z = (float)tmp.z;
        }
      }
      public Position(System.Drawing.Point? pt, System.Drawing.Point ptPitch)
      {
        Position? p = convertCkToRel(pt, ptPitch);
        if (p != null) {
          this.x = p.x;
          this.y = p.y;
          this.z = p.z;
        }
      }

      public static Position? convertCkToRel(System.Drawing.Point? pt, System.Drawing.Point ptPitch)
      {
        if (pt == null) return null;

        Position p = new Position();
        p.x =  pt.Value.X / (ptPitch.X * 1f);
        p.y = (pt.Value.Y / (ptPitch.Y * 2f)) + 0.5f;
        p.z = 0f;

        return p;
      }

      public float x { get; set; }
      public float y { get; set; }
      public float z { get; set; }
    }

    public class Player
    {
      public Position pos       { get; set; }
      public Position? posLast   { get; set; }
      public Position? posTarget { get; set; }
      public bool bHome { get; set; }
      public bool bKeeper { get; set; }
      public string sName { get; set; } = "";
      public byte iNo     { get; set; }
      public byte iLookAt { get; set; }
      public byte iCard   { get; set; }
      public bool bShowCard { get; set; }
      public float fLeaderChange { get; set; }
      public float fMoralChange { get; set; }
    }

    public class Ball
    {
      public class PointTarget : TeamModel.Point
      {
        public PointTarget(CornerkickGame.Game.Ball? ball = null, System.Drawing.Point? pt = null)
        {
          if (ball != null) {
            this.bLow = ball.bLow;
            this.iStep  = ball.iStep;
            this.nSteps = ball.nSteps;
            this.x = ball.ptPos.X;
            this.y = ball.ptPos.Y;
          }

          if (pt.HasValue) {
            this.x = pt.Value.X;
            this.y = pt.Value.Y;
          }
        }

        public bool bLow { get; set; }

        public int iStep { get; set; }
        public int nSteps { get; set; }
      }

      public Position pos { get; set; }
      public TeamModel.Point? ptPosLast   { get; set; }
      public TeamModel.Point? ptPos       { get; set; }
      public PointTarget? ptTarget { get; set; }
    }

    public class gameData
    {
      public string sUserId { get; set; }
      public bool bOwnLiveGame { get; set; }

      public int iLastStatePerformed { get; set; }

      public float fMinute { get; set; }

      public int iTeamId { get; set; }
      public string sTeamH { get; set; }
      public string sTeamA { get; set; }

      public string sEmblemH { get; set; }
      public string sEmblemA { get; set; }

      public string[][] sJerseyColors { get; set; }

      public string sStadium { get; set; }

      public byte nPlStart { get; set; }
      public int iGameMinutes { get; set; }
      public System.Drawing.Point ptPitch { get; set; }

      // Statistics
      public int iGoalsH { get; set; }
      public int iGoalsA { get; set; }
      public int[] iShoots { get; set; }
      public int[] iShootsOnGoal { get; set; }
      public float[] fGoalsX { get; set; }  // Expected goals
      public int[] iPassesGood { get; set; }
      public int[] iPassesBad  { get; set; }
      public int[] iDuels { get; set; }
      public int[] iFouls { get; set; }
      public int[] iPossession = new int[2];
      public int[] iCornerkick = new int[2];
      public int[] iOffsite = new int[2];
      public float[] fPassGood = new float[2];

      public int iState  { get; set; }
      public int nStates { get; set; }
      public CornerkickGame.Game.Shoot[][] ltShoots { get; set; }
      public CornerkickGame.Game.Duel [][] ltDuels  { get; set; }

      public List<drawLine> ltDrawLineShoot { get; set; }
      public List<drawLine> ltDrawLinePass { get; set; }
      public string sCard { get; set; }

      public string sTimelineIcons { get; set; }
      public string sStatGoals { get; set; }
      public string sStatCards { get; set; }
      public string sStatSubs  { get; set; }

      public string sRefereeQuality { get; set; }
      public string sRefereeDecisions { get; set; }

      public string sDivHeatmap { get; set; }

      // Chances
      public float[] fPlAction    { get; set; }
      public float   fPlActionRnd { get; set; }

      public string sAdminChanceShootOnGoal { get; set; }
      public string sAdminChanceGoal { get; set; }

      public List<CkAppShared.DataPointTD>[] ltF { get; set; }
      //public List<Models.DataPointGeneral>[] ltM { get; set; }

      public gameData()
      {
        iShoots       = new int  [2];
        iShootsOnGoal = new int  [2];
        fGoalsX       = new float[2];
        iPassesGood   = new int  [2];
        iPassesBad    = new int  [2];
        iDuels        = new int  [2];
        iFouls        = new int  [2];

        sJerseyColors = new string[2][];
      }
    }
    public gameData? gD;

    public class gameData2
    {
      public string sUserId { get; set; }

      public gameData viewGd { get; set; }
      public CornerkickGame.Game game { get; set; }
    }

    public struct drawLine
    {
      public float x0 { get; set; }
      public float y0 { get; set; }
      public float x1 { get; set; }
      public float y1 { get; set; }
      public string sColor { get; set; }
      public string sTitle { get; set; }
    }

    public struct HeatmapPoint
    {
      public float x { get; set; }
      public float y { get; set; }
      public string color { get; set; }
      public int level { get; set; }
    }

    public class PassTarget : TeamModel.Point
    {
      public PassTarget(System.Drawing.Point? pt = null, double Z = 0f, bool bPlayerChoice = false)
      {
        if (pt.HasValue) {
          this.x = pt.Value.X;
          this.y = pt.Value.Y;
        }

        this.z = Z;
        this.bPlayerChoice = bPlayerChoice;
      }

      public bool bPlayerChoice { get; set; }
    }

    public ViewGameModel()
    {
      UserOptionsModel mdUserOptions = new UserOptionsModel();

      ddlGames = new List<SelectListItem>();
      sSelectedGame = "";

      sliSystem = new List<SelectListItem>();

      ddlShoots = new List<SelectListItem>();
      ddlDuels  = new List<SelectListItem>();
      ddlPasses = new List<SelectListItem>();

      ltShoots = new List<CornerkickGame.Game.Shoot>[1];

      ddlShowPitch  = mdUserOptions.ddlShowPitch;
      ddlComments   = mdUserOptions.ddlComments;
      ddlAnimations = mdUserOptions.ddlAnimations;

      ddlHeatmap = new List<SelectListItem>();
      ddlHeatmap.Add(new SelectListItem { Text = "aus",      Value = "-1" });
      ddlHeatmap.Add(new SelectListItem { Text = "Heim",     Value =  "0" });
      ddlHeatmap.Add(new SelectListItem { Text = "Auswärts", Value =  "1" });

      iHeatmap = -1;
      iShoots  = -1;
      iDuels   = -1;
      iPasses  = -1;

      // Positions select
      ddlPositions = new List<SelectListItem>();
      ddlPositions.Add(new SelectListItem { Text = "aus",         Value = "-1" });
      ddlPositions.Add(new SelectListItem { Text = "tatsächlich", Value =  "0" });
      ddlPositions.Add(new SelectListItem { Text = "gemittelt",   Value =  "1" });

      iPositions = 0;
    }
  }
}
