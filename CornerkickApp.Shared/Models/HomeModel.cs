using System;
using System.Runtime.Serialization;

namespace CornerkickApp.Shared.Models
{
  public class HomeModel : LayoutModel
  {
    public static int iFilesLoad { get; set; }
    public string sCkInstanceName { get; set; } = "";
  }

  //DataContract for Serializing Data - required to serve in JSON format
  [DataContract]
  public class DataPointDoubleXY
  {
    public DataPointDoubleXY(double x, double y, string z = "")
    {
      this.x = x;
      this.y = y;
      this.z = z;
    }

    // Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "x")]
    public Nullable<double> x = null;

    // Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "y")]
    public Nullable<double> y = null;

    // Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "z")]
    public string z;
  }
}
