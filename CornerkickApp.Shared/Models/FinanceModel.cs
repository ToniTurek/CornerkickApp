using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace CornerkickApp.Shared.Models
{
  public class FinanceModel : LayoutModel
  {
    public List<CornerkickManager.Finance.Account> ltAccount { get; set; }

    public string sKonto { get; set; } = string.Empty;

    [DataType(DataType.Currency)]
    [Display(Name = "Stehplätze:")]
    public int iPrice1 { get; set; }
    public int iPriceSeason1 { get; set; }

    [DataType(DataType.Currency)]
    [Display(Name = "Sitzplätze:")]
    public int iPrice2 { get; set; }
    public int iPriceSeason2 { get; set; }

    [DataType(DataType.Currency)]
    [Display(Name = "V.I.P.:")]
    public int iPrice3 { get; set; }
    public int iPriceSeason3 { get; set; }
    public float fSeasonalTicketsMaxFrac { get; set; }
    public int[] iSeasonalTickets { get; set; }

    public bool bEditable { get; set; }

    public List<SelectListItem> sliYears { get; set; } = new List<SelectListItem>();
    [Display(Name = "Jahr: ")]

    public bool bNetto { get; set; }

    public float  fBalanceSecretFracAdmissionPrice { get; set; }
    public string sBalanceSecret { get; set; } = string.Empty;

    public CkAppShared.Tutorial tutorial { get; set; }

    [DataContract]
    public class DataPointSpec
    {
      public DataPointSpec(int x, int y, int[] spec, string z = "")
      {
        this.X = x;
        this.Y = y;
        this.spec = spec;
        this.Z = z;
      }

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "x")]
      public int? X { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "y")]
      public int? Y { get; set; } = null;

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "spec")]
      public int[] spec = new int[3];

      // Explicitly setting the name to be used while serializing to JSON.
      [DataMember(Name = "z")]
      public string Z { get; set; } = "";
    }
  }

  public class DiaryFinanceEvent
  {
    public int iID;
    public string sTitle;
    public string sDescription;
    public string sStartDate;
    public string sEndDate;
    public string StatusString;
    public string sColor;
    public string sBackgroundColor;
    public string sBorderColor;
    public string sTextColor;
    public bool bEditable;
    public string ClassName;
  }

  //DataContract for Serializing Data - required to serve in JSON format
  [DataContract]
  public class DataPointKonto
  {
    public DataPointKonto(long x, long y)
    {
      this.X = x;
      this.Y = y;
    }

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "x")]
    public Nullable<long> X = null;
    //[DataMember(Name = "x")]
    //public Nullable<DateTime> X = null;

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "y")]
    public Nullable<double> Y = null;
  }
}
